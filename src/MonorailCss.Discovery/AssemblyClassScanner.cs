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
    // Cache keyed on module MVID. The cache is sound only for assemblies whose IL has not
    // changed; callers who know an assembly's contents have changed (e.g. a hot-reload delta
    // applied to its in-memory metadata image) must call <see cref="Invalidate"/> first so the
    // next scan re-walks the heap. Hit count is exposed via <see cref="MvidCacheHits"/>.
    private readonly ConcurrentDictionary<Guid, ImmutableSortedSet<string>> _mvidCache = new();
    private readonly ValidationCache _validationCache;
    private long _mvidCacheHits;
    private long _mvidCacheMisses;

    public AssemblyClassScanner(ValidationCache validationCache)
    {
        _validationCache = validationCache;
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

    /// <summary>
    /// Drops any cached scan result for <paramref name="mvid"/>. Call this before rescanning an
    /// assembly whose contents have changed since the previous scan — e.g. when forwarding a
    /// hot-reload <c>MetadataUpdateHandler</c> notification.
    /// </summary>
    public void Invalidate(Guid mvid)
    {
        _mvidCache.TryRemove(mvid, out _);
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

        // Cache hit serves the previously-validated set without re-walking the heap. Callers
        // that know the assembly has changed (hot-reload) must <see cref="Invalidate"/> first.
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

            _validationCache.CollectValid(raw, local);

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
