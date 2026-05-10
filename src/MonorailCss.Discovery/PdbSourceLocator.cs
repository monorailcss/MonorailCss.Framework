using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace MonorailCss.Discovery;

/// <summary>
/// Resolves an assembly's local source-code root by reading <c>Document</c> entries from its
/// portable PDB (sibling file or embedded). Returns only when the PDB's document paths exist
/// on the current machine — assemblies consumed from NuGet, whose PDBs encode CI build paths
/// (<c>D:\a\_work\...</c>, <c>/_/src/...</c>), are filtered out by the on-disk existence check.
/// </summary>
internal static class PdbSourceLocator
{
    /// <summary>
    /// Tries to determine the source root for <paramref name="assembly"/>.
    /// Returns true and writes a normalized full path to <paramref name="sourceRoot"/> when at
    /// least one <c>Document</c> in the assembly's PDB resolves to an existing file on disk and
    /// their common ancestor is a non-trivial directory.
    /// </summary>
    public static bool TryGetSourceRoot(Assembly assembly, out string sourceRoot)
    {
        sourceRoot = string.Empty;

        if (assembly.IsDynamic)
        {
            return false;
        }

        string location;
        try
        {
            location = assembly.Location;
        }
        catch
        {
            return false;
        }

        if (string.IsNullOrEmpty(location) || !File.Exists(location))
        {
            return false;
        }

        try
        {
            var documents = ReadDocuments(location);
            if (documents.Count == 0)
            {
                return false;
            }

            var existing = new List<string>(documents.Count);
            foreach (var raw in documents)
            {
                if (string.IsNullOrEmpty(raw))
                {
                    continue;
                }

                string full;
                try
                {
                    full = Path.GetFullPath(raw);
                }
                catch
                {
                    continue;
                }

                if (File.Exists(full))
                {
                    existing.Add(full);
                }
            }

            if (existing.Count == 0)
            {
                return false;
            }

            var root = DeepestMajorityAncestor(existing);
            if (string.IsNullOrEmpty(root))
            {
                return false;
            }

            // Reject drive roots — a PDB whose documents all sit at the filesystem root is
            // never a useful watch target.
            var trimmed = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrEmpty(trimmed) || trimmed.Length <= 3)
            {
                return false;
            }

            sourceRoot = root;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> ReadDocuments(string assemblyPath)
    {
        using var peStream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(peStream);

        // Embedded PDB wins when present — sibling .pdb files are sometimes leftover stale.
        foreach (var entry in peReader.ReadDebugDirectory())
        {
            if (entry.Type != DebugDirectoryEntryType.EmbeddedPortablePdb)
            {
                continue;
            }

            using var provider = peReader.ReadEmbeddedPortablePdbDebugDirectoryData(entry);
            return ReadDocumentNames(provider.GetMetadataReader());
        }

        var siblingPdb = Path.ChangeExtension(assemblyPath, ".pdb");
        if (!File.Exists(siblingPdb))
        {
            return [];
        }

        try
        {
            using var pdbStream = File.OpenRead(siblingPdb);
            using var provider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
            return ReadDocumentNames(provider.GetMetadataReader());
        }
        catch
        {
            // Sibling pdb might be a Windows-style PDB (not portable) or otherwise unreadable.
            return [];
        }
    }

    private static List<string> ReadDocumentNames(MetadataReader reader)
    {
        var result = new List<string>(reader.Documents.Count);
        foreach (var handle in reader.Documents)
        {
            var doc = reader.GetDocument(handle);
            var name = reader.GetString(doc.Name);
            if (!string.IsNullOrEmpty(name))
            {
                result.Add(name);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the deepest directory that is an ancestor (or equal-to) of the largest number
    /// of <paramref name="paths"/>. Tolerates outliers — e.g. a single source-distributed file
    /// from a NuGet package sitting in <c>~/.nuget/packages</c> while the rest of the documents
    /// are in the user's project tree — by picking the directory that covers the most files
    /// rather than the strict common prefix of all of them.
    /// </summary>
    private static string DeepestMajorityAncestor(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
        {
            return string.Empty;
        }

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in paths)
        {
            var dir = Path.GetDirectoryName(p);
            while (!string.IsNullOrEmpty(dir))
            {
                counts.TryGetValue(dir, out var n);
                counts[dir] = n + 1;

                var parent = Path.GetDirectoryName(dir);
                if (string.IsNullOrEmpty(parent) || string.Equals(parent, dir, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                dir = parent;
            }
        }

        if (counts.Count == 0)
        {
            return string.Empty;
        }

        var maxCount = counts.Values.Max();
        return counts
            .Where(kv => kv.Value == maxCount)
            .OrderByDescending(kv => kv.Key.Length)
            .First()
            .Key;
    }
}
