using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace MonorailCss.Discovery;

/// <summary>
/// Shared IL-metadata scanning primitives used by both the runtime
/// <c>AssemblyClassScanner</c> in <c>MonorailCss.Discovery</c> and the build-time
/// <c>DllScanner</c> in <c>MonorailCss.Build.Tasks</c>. Each consumer is responsible for
/// obtaining a <see cref="MetadataReader"/> the way that fits its execution model
/// (in-memory <c>Assembly.TryGetRawMetadata</c> at runtime, on-disk
/// <c>PEReader.GetMetadataReader</c> at build time); the actual heap walk and
/// <c>[ReferenceAssembly]</c> filter are identical and live here.
/// </summary>
public static class IlMetadataScanner
{
    /// <summary>
    /// Walks the <c>#US</c> (User Strings) heap of <paramref name="reader"/> and feeds every
    /// non-empty entry to <paramref name="validationCache"/>'s
    /// <see cref="ValidationCache.CollectValid"/>. Mirrors the body that previously lived inline
    /// in both scanners — ECMA-335 II.24.2.4 prefix decode, no IL decoding necessary.
    /// </summary>
    /// <param name="reader">The metadata reader to walk.</param>
    /// <param name="validationCache">Validator that tokenizes and filters each raw heap entry.</param>
    /// <param name="output">Destination for tokens that pass validation.</param>
    public static void ScanUserStringHeap(MetadataReader reader, ValidationCache validationCache, ICollection<string> output)
    {
        var heapSize = reader.GetHeapSize(HeapIndex.UserString);

        // ECMA-335 II.24.2.4: each #US entry is a compressed-length prefix followed by
        // (UTF-16 chars * 2) bytes plus a 1-byte trailer. Offset 0 is reserved (empty/null).
        var offset = 1;
        while (offset < heapSize)
        {
            var handle = MetadataTokens.UserStringHandle(offset);
            var raw = reader.GetUserString(handle);

            if (raw.Length > 0)
            {
                validationCache.CollectValid(raw, output);
            }

            var dataBytes = (raw.Length * 2) + 1;
            var prefixBytes = dataBytes < 0x80 ? 1 : dataBytes < 0x4000 ? 2 : 4;
            offset += prefixBytes + dataBytes;
        }
    }

    /// <summary>
    /// True when <paramref name="name"/> looks like a BCL / runtime assembly. Reads as
    /// "skip the noise": the BCL is full of strings the candidate parser would have to chew
    /// through, and the false-positive rate on what comes out is essentially zero. Anything
    /// else (third-party packages, the user's own assemblies, RCLs, icon packs) gets scanned.
    /// </summary>
    /// <param name="name">The simple assembly name (no <c>.dll</c>, no version), as returned by
    /// <c>Assembly.GetName().Name</c> or <c>Path.GetFileNameWithoutExtension(path)</c>.</param>
    /// <returns>True when the name matches a known BCL / runtime prefix.</returns>
    public static bool IsKnownFrameworkAssembly(string name)
    {
        return name.StartsWith("System.", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.", StringComparison.Ordinal)
            || name.StartsWith("netstandard", StringComparison.Ordinal)
            || name.Equals("mscorlib", StringComparison.Ordinal)
            || name.Equals("WindowsBase", StringComparison.Ordinal);
    }

    /// <summary>
    /// True when the assembly's metadata carries
    /// <see cref="System.Runtime.CompilerServices.ReferenceAssemblyAttribute"/>. Reference
    /// assemblies (the API-only DLLs the BCL ships in
    /// <c>packs\Microsoft.NETCore.App.Ref</c>, etc.) contain only metadata stubs and never
    /// real string literals worth scanning. Callers should usually skip them.
    /// </summary>
    /// <param name="reader">The metadata reader to inspect.</param>
    /// <returns>True when the assembly is marked as reference-only.</returns>
    public static bool HasReferenceAssemblyAttribute(MetadataReader reader)
    {
        var assemblyDef = reader.GetAssemblyDefinition();
        foreach (var attrHandle in assemblyDef.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (IsAttributeOfType(reader, attr, "System.Runtime.CompilerServices", "ReferenceAssemblyAttribute"))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAttributeOfType(MetadataReader reader, CustomAttribute attr, string ns, string typeName)
    {
        switch (attr.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                {
                    var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                    if (memberRef.Parent.Kind != HandleKind.TypeReference)
                    {
                        return false;
                    }

                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    return reader.StringComparer.Equals(typeRef.Namespace, ns)
                           && reader.StringComparer.Equals(typeRef.Name, typeName);
                }

            case HandleKind.MethodDefinition:
                {
                    var methodDef = reader.GetMethodDefinition((MethodDefinitionHandle)attr.Constructor);
                    var typeDef = reader.GetTypeDefinition(methodDef.GetDeclaringType());
                    return reader.StringComparer.Equals(typeDef.Namespace, ns)
                           && reader.StringComparer.Equals(typeDef.Name, typeName);
                }

            default:
                return false;
        }
    }
}
