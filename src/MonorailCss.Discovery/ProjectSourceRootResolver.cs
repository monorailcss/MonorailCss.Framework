using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace MonorailCss.Discovery;

/// <summary>
/// Discovers the on-disk source-project directories that the currently-loaded, locally-built
/// assemblies were compiled from, by reading each assembly's portable-PDB document table.
/// <para>
/// This closes the <c>dotnet watch</c> cross-project gap. The discovery service auto-watches the
/// running app's <c>ContentRootPath</c>, but under <c>dotnet watch</c> a developer routinely edits
/// <c>.razor</c>/<c>.cs</c> files in <em>referenced</em> projects whose source lives elsewhere on
/// disk (e.g. running <c>docs\Pennington.Docs</c> while editing <c>src\Pennington.DocSite</c>).
/// Hot-reload deltas don't rewrite the on-disk PE, so the IL scan can't observe those edits;
/// watching the referenced project's source directory is the only reliable bridge.
/// </para>
/// <para>
/// A Debug build's portable PDB lists every source document (<c>.cs</c>, and <c>.razor</c> via the
/// Razor source generator's <c>#line</c>/checksum mappings) by its real on-disk path. We read those
/// documents, keep the ones that actually exist and aren't generated/intermediate output, and walk
/// up from each to the nearest directory containing a project file — that directory is the watch
/// root. Assemblies with no local source (Release/published builds, NuGet-cached RCLs whose PDBs
/// were stamped with CI paths, single-file/AOT images with no PDB) contribute nothing, so the
/// feature scopes itself to locally-editable projects without any configuration.
/// </para>
/// </summary>
internal static class ProjectSourceRootResolver
{
    // A source file nested more than this many directories below its project root is
    // pathological; capping the walk-up keeps a stray document path (or a symlink cycle on a
    // misconfigured tree) from spinning. Real project trees bottom out well under this.
    private const int MaxWalkUpDepth = 24;

    private static readonly string[] ProjectFilePatterns = { "*.csproj", "*.fsproj", "*.vbproj" };

    /// <summary>
    /// Resolves the distinct set of project-root directories backing <paramref name="assemblies"/>.
    /// Framework/BCL assemblies and anything in <paramref name="excludeAssemblies"/> are skipped,
    /// matching the IL scanner's filter so we don't pay to open hundreds of PDB-less BCL images.
    /// </summary>
    /// <param name="assemblies">Loaded assemblies to inspect — typically
    /// <c>AppDomain.CurrentDomain.GetAssemblies()</c>.</param>
    /// <param name="excludeAssemblies">Simple assembly names to skip (matched against
    /// <c>Assembly.GetName().Name</c>).</param>
    /// <returns>Absolute, existing project-root directories. Empty when no local source is found.</returns>
    [UnconditionalSuppressMessage(
        "SingleFile",
        "IL3000:Avoid accessing Assembly file path when publishing as a single file",
        Justification = "An empty Location (single-file/Native-AOT) is handled by skipping that assembly; the feature is development-only and degrades to watching nothing when no on-disk PE/PDB exists.")]
    public static IReadOnlyList<string> ResolveWatchRoots(
        IEnumerable<Assembly> assemblies,
        IReadOnlyCollection<string> excludeAssemblies)
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Document directories share a project root, so cache the (often-repeated) walk-up per
        // directory. null means "walked, found no project file" — still worth caching.
        var projectRootCache = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assemblies)
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            var name = assembly.GetName().Name;
            if (string.IsNullOrEmpty(name)
                || IlMetadataScanner.IsKnownFrameworkAssembly(name)
                || excludeAssemblies.Contains(name))
            {
                continue;
            }

            string location;
            try
            {
                location = assembly.Location;
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrEmpty(location) || !File.Exists(location))
            {
                continue;
            }

            CollectRootsFromAssemblyFile(location, roots, projectRootCache);
        }

        return roots.ToList();
    }

    /// <summary>
    /// Reads one DLL's associated portable PDB and folds every project root its source documents
    /// resolve to into <paramref name="roots"/>. Exposed internally so a test can exercise the PDB
    /// path against a known on-disk assembly without standing up the hosted service.
    /// </summary>
    /// <param name="assemblyPath">Absolute path to the DLL.</param>
    /// <param name="roots">Accumulator for discovered project-root directories.</param>
    /// <param name="projectRootCache">Per-directory walk-up cache, shared across assemblies.</param>
    internal static void CollectRootsFromAssemblyFile(
        string assemblyPath,
        HashSet<string> roots,
        Dictionary<string, string?> projectRootCache)
    {
        try
        {
            using var stream = File.OpenRead(assemblyPath);
            using var peReader = new PEReader(stream);

            if (!peReader.HasMetadata)
            {
                return;
            }

            // Honor the same skip contract as the IL scanner: a reference-only image carries no
            // real source, and an assembly that opted out of string scanning via
            // [assembly: MonorailCssNoScan] (the MonorailCss framework assemblies do) must not have
            // its source watched either — watching it is just a slower form of the scan it declined.
            var ilReader = peReader.GetMetadataReader();
            if (!ilReader.IsAssembly
                || IlMetadataScanner.HasReferenceAssemblyAttribute(ilReader)
                || IlMetadataScanner.HasMonorailCssNoScanAttribute(ilReader))
            {
                return;
            }

            // Handles both an embedded portable PDB and a sibling .pdb (the provider callback is
            // only invoked for the sibling case). Returns false for full/Windows PDBs, missing
            // PDBs, and reference-only images — all correctly treated as "no local source".
            if (!peReader.TryOpenAssociatedPortablePdb(assemblyPath, OpenPdbStream, out var provider, out _)
                || provider is null)
            {
                return;
            }

            using (provider)
            {
                var reader = provider.GetMetadataReader();
                foreach (var handle in reader.Documents)
                {
                    string docPath;
                    try
                    {
                        docPath = reader.GetString(reader.GetDocument(handle).Name);
                    }
                    catch
                    {
                        continue;
                    }

                    // Require an absolute path before the existence check: a relative document
                    // path (atypical PathMap / custom build) would otherwise resolve File.Exists
                    // against the process working directory and could spuriously match a same-named
                    // file in the running app, yielding a wrong or non-canonical watch root.
                    if (string.IsNullOrEmpty(docPath)
                        || !Path.IsPathRooted(docPath)
                        || !DiscoveryPaths.HasSupportedExtension(docPath)
                        || DiscoveryPaths.IsInIgnoredDirectory(docPath)
                        || !File.Exists(docPath))
                    {
                        continue;
                    }

                    var dir = Path.GetDirectoryName(docPath);
                    if (string.IsNullOrEmpty(dir))
                    {
                        continue;
                    }

                    var root = FindProjectRoot(dir, projectRootCache);
                    if (root is not null)
                    {
                        roots.Add(root);
                    }
                }
            }
        }
        catch
        {
            // Best-effort: a malformed PE/PDB, a file-locking race with the build, or an
            // unreadable image must not abort discovery. We simply learn about fewer roots.
        }
    }

    private static Stream? OpenPdbStream(string path) => File.Exists(path) ? File.OpenRead(path) : null;

    /// <summary>
    /// Walks up from <paramref name="startDir"/> to the nearest ancestor directory containing a
    /// project file, mirroring how <c>dotnet watch</c> keys its file watch on project directories.
    /// Returns <see langword="null"/> when no project file is found within
    /// <see cref="MaxWalkUpDepth"/> levels.
    /// </summary>
    private static string? FindProjectRoot(string startDir, Dictionary<string, string?> cache)
    {
        if (cache.TryGetValue(startDir, out var cached))
        {
            return cached;
        }

        string? result = null;
        var dir = startDir;
        for (var depth = 0; depth < MaxWalkUpDepth && !string.IsNullOrEmpty(dir); depth++)
        {
            if (DirectoryContainsProjectFile(dir))
            {
                result = Path.TrimEndingDirectorySeparator(dir);
                break;
            }

            var parent = Path.GetDirectoryName(dir);
            if (string.IsNullOrEmpty(parent) || string.Equals(parent, dir, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            dir = parent;
        }

        cache[startDir] = result;
        return result;
    }

    private static bool DirectoryContainsProjectFile(string dir)
    {
        try
        {
            foreach (var pattern in ProjectFilePatterns)
            {
                using var e = Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly).GetEnumerator();
                if (e.MoveNext())
                {
                    return true;
                }
            }
        }
        catch
        {
            // Directory vanished or is unreadable — treat as "no project file here".
        }

        return false;
    }
}
