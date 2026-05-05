using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace MonorailCss.Discovery;

/// <summary>
/// Scans the <c>#US</c> (User Strings) heap of a loaded assembly's IL metadata, extracting every
/// string literal and validating each one against the framework's candidate parser.
/// Reads in-memory metadata via <c>System.Reflection.Metadata.AssemblyExtensions.TryGetRawMetadata</c>,
/// which exposes the live image including hot-reload deltas.
/// </summary>
internal sealed class AssemblyClassScanner
{
    // MVID is stable for the lifetime of a loaded module — even hot-reload deltas leave the base
    // PE image (and its MVID) unchanged. Caching by MVID lets a hot-reload event short-circuit
    // 16 of 17 assemblies on a typical docs site, since their #US heaps are byte-identical to
    // the previous scan. Hit count is exposed via <see cref="MvidCacheHits"/>.
    private readonly ConcurrentDictionary<Guid, ImmutableSortedSet<string>> _mvidCache = new();
    private readonly PreFilter _preFilter;
    private readonly ValidationCache _validationCache;
    private long _mvidCacheHits;
    private long _mvidCacheMisses;

    public AssemblyClassScanner(ValidationCache validationCache, PreFilter preFilter)
    {
        _validationCache = validationCache;
        _preFilter = preFilter;
    }

    public long MvidCacheHits => Interlocked.Read(ref _mvidCacheHits);

    public long MvidCacheMisses => Interlocked.Read(ref _mvidCacheMisses);

    /// <summary>
    /// Scans <paramref name="assembly"/> and adds every valid candidate string to <paramref name="output"/>.
    /// Returns false if the assembly was skipped (no metadata or reference-only).
    /// </summary>
    public bool Scan(Assembly assembly, ICollection<string> output)
    {
        return ScanCore(assembly, output);
    }

    private unsafe bool ScanCore(Assembly assembly, ICollection<string> output)
    {
        if (!assembly.TryGetRawMetadata(out var blob, out var length))
        {
            return false;
        }

        var md = new MetadataReader(blob, length);

        if (!md.IsAssembly)
        {
            return false;
        }

        if (HasReferenceAssemblyAttribute(md))
        {
            return false;
        }

        var mvid = md.GetGuid(md.GetModuleDefinition().Mvid);

        // Hot-reload events ask us to rescan every loaded assembly even though usually only
        // one delta-modified assembly's content is novel. MVID is stable across the lifetime
        // of the module (deltas don't change it), so a cache keyed on MVID returns the
        // previously-validated set instantly for unchanged assemblies.
        if (_mvidCache.TryGetValue(mvid, out var cached))
        {
            Interlocked.Increment(ref _mvidCacheHits);
            foreach (var c in cached)
            {
                output.Add(c);
            }

            return true;
        }

        Interlocked.Increment(ref _mvidCacheMisses);

        // Buffer into a local set so we can both populate the caller's collection and cache
        // the dedup'd, sorted result for the next rescan.
        var local = new HashSet<string>(StringComparer.Ordinal);
        var heapSize = md.GetHeapSize(HeapIndex.UserString);

        // ECMA-335 II.24.2.4: each #US entry is a compressed-length prefix followed by
        // (UTF-16 chars * 2) bytes plus a 1-byte trailer. Offset 0 is reserved (empty/null).
        var offset = 1;
        while (offset < heapSize)
        {
            var handle = MetadataTokens.UserStringHandle(offset);
            var raw = md.GetUserString(handle);
            if (raw.Length == 0)
            {
                offset += 1;
                continue;
            }

            ScanString(raw, local);

            var dataBytes = (raw.Length * 2) + 1;
            var prefixBytes = dataBytes < 0x80 ? 1 : dataBytes < 0x4000 ? 2 : 4;
            offset += prefixBytes + dataBytes;
        }

        var snapshot = local.ToImmutableSortedSet(StringComparer.Ordinal);
        _mvidCache[mvid] = snapshot;
        foreach (var c in snapshot)
        {
            output.Add(c);
        }

        return true;
    }

    private void ScanString(string raw, ICollection<string> output)
    {
        // Razor compiles adjacent static markup into a single AddMarkupContent("<div class=\"foo bar\">...")
        // string literal. A whitespace-only split would leak HTML fragments like `class="foo` and
        // `bar">content</div>` into the candidate stream. Instead we scan for class-shaped runs:
        // start at any candidate-leading char, advance through the safe character set OUTSIDE
        // brackets and through anything BALANCED inside [...], and stop at the first HTML/code
        // boundary.
        var i = 0;
        while (i < raw.Length)
        {
            var startChar = raw[i];
            if (!IsCandidateStart(startChar))
            {
                i++;
                continue;
            }

            var start = i;
            var depth = 0;
            while (i < raw.Length)
            {
                var c = raw[i];

                if (depth > 0)
                {
                    if (c == '[')
                    {
                        depth++;
                    }
                    else if (c == ']')
                    {
                        depth--;
                    }

                    i++;
                    continue;
                }

                if (c == '[')
                {
                    depth = 1;
                    i++;
                    continue;
                }

                if (!IsCandidateChar(c))
                {
                    break;
                }

                i++;
            }

            if (depth != 0 || i == start)
            {
                // Unmatched bracket or zero-width run — skip past the start char and continue
                if (i == start)
                {
                    i++;
                }

                continue;
            }

            var token = raw[start..i];
            if (!_preFilter.IsPlausible(token))
            {
                continue;
            }

            if (!_validationCache.TryValidate(token))
            {
                continue;
            }

            output.Add(token);
        }
    }

    private static bool IsCandidateStart(char c)
    {
        return (c is >= 'a' and <= 'z') || c is '-' or '!' or '@' or '[' or '*';
    }

    private static bool IsCandidateChar(char c)
    {
        // Class-name chars OUTSIDE brackets. Inside [...] the lexer accepts anything balanced,
        // so this set excludes characters that only appear in arbitrary values (`>`, `=`, `&`,
        // `<`, quotes, parens, commas, semicolons) and never in a bare utility token.
        return (c is >= 'a' and <= 'z')
            || (c is >= 'A' and <= 'Z')
            || (c is >= '0' and <= '9')
            || c is '-' or '_' or ':' or '.' or '/' or '!' or '%' or '*' or '#' or '@' or '~' or '$';
    }

    private static bool HasReferenceAssemblyAttribute(MetadataReader md)
    {
        var assemblyDef = md.GetAssemblyDefinition();
        foreach (var attrHandle in assemblyDef.GetCustomAttributes())
        {
            var attr = md.GetCustomAttribute(attrHandle);
            if (IsAttributeOfType(md, attr, "System.Runtime.CompilerServices", "ReferenceAssemblyAttribute"))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttributeOfType(MetadataReader md, CustomAttribute attr, string ns, string typeName)
    {
        switch (attr.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                {
                    var memberRef = md.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                    if (memberRef.Parent.Kind != HandleKind.TypeReference)
                    {
                        return false;
                    }

                    var typeRef = md.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    return md.StringComparer.Equals(typeRef.Namespace, ns)
                           && md.StringComparer.Equals(typeRef.Name, typeName);
                }

            case HandleKind.MethodDefinition:
                {
                    var methodDef = md.GetMethodDefinition((MethodDefinitionHandle)attr.Constructor);
                    var typeDef = md.GetTypeDefinition(methodDef.GetDeclaringType());
                    return md.StringComparer.Equals(typeDef.Namespace, ns)
                           && md.StringComparer.Equals(typeDef.Name, typeName);
                }

            default:
                return false;
        }
    }
}
