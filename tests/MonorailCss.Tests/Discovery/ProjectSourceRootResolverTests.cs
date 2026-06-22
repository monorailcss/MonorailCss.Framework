using System.Reflection;
using System.Runtime.CompilerServices;
using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

/// <summary>
/// Exercises the portable-PDB-based project-root discovery against this very test assembly: its
/// own PDB lists these test source files by their real on-disk paths, so the resolver must walk
/// up to the directory holding <c>MonorailCss.Tests.csproj</c>. These assertions hold whenever the
/// assembly is built and run on the same machine (the normal <c>dotnet test</c> / CI flow), because
/// both the PDB document paths and <see cref="CallerFilePathAttribute"/> are stamped at compile
/// time from the same source tree.
/// </summary>
public class ProjectSourceRootResolverTests
{
    [Fact]
    public void Resolves_this_test_projects_root_from_its_own_pdb()
    {
        var roots = ProjectSourceRootResolver.ResolveWatchRoots(
            new[] { typeof(ProjectSourceRootResolverTests).Assembly },
            Array.Empty<string>());

        roots.ShouldNotBeEmpty();

        var thisFileDir = Normalize(Path.GetDirectoryName(ThisFilePath())!);
        var match = roots.Select(Normalize).FirstOrDefault(root => IsSameOrUnder(thisFileDir, root));

        match.ShouldNotBeNull("expected a discovered root to be an ancestor of this test source file");
        Directory.Exists(match).ShouldBeTrue();
        Directory.EnumerateFiles(match!, "*.csproj").ShouldNotBeEmpty();
    }

    [Fact]
    public void Discovered_roots_are_real_directories_outside_bin_and_obj()
    {
        var roots = ProjectSourceRootResolver.ResolveWatchRoots(
            new[] { typeof(ProjectSourceRootResolverTests).Assembly },
            Array.Empty<string>());

        foreach (var root in roots)
        {
            Directory.Exists(root).ShouldBeTrue();
            DiscoveryPaths.IsInIgnoredDirectory(root).ShouldBeFalse($"root should not sit inside bin/obj/etc.: {root}");
        }
    }

    [Fact]
    public void Skips_excluded_assemblies()
    {
        var name = typeof(ProjectSourceRootResolverTests).Assembly.GetName().Name!;

        var roots = ProjectSourceRootResolver.ResolveWatchRoots(
            new[] { typeof(ProjectSourceRootResolverTests).Assembly },
            new[] { name });

        roots.ShouldBeEmpty();
    }

    [Fact]
    public void Skips_framework_assemblies()
    {
        // System.Private.CoreLib is a BCL assembly: filtered by the framework-name check, and it
        // has no PDB pointing at local source anyway.
        var roots = ProjectSourceRootResolver.ResolveWatchRoots(
            new[] { typeof(object).Assembly },
            Array.Empty<string>());

        roots.ShouldBeEmpty();
    }

    [Fact]
    public void Skips_assemblies_marked_MonorailCssNoScan()
    {
        // MonorailCss core ships [assembly: MonorailCssNoScan]. Even though it's a locally-built
        // project with a PDB whose documents point at existing source, NoScan must suppress source
        // watching too — otherwise watching it would be a slower form of the scan it declined.
        var roots = ProjectSourceRootResolver.ResolveWatchRoots(
            new[] { typeof(CssFramework).Assembly },
            Array.Empty<string>());

        roots.ShouldBeEmpty();
    }

    private static string ThisFilePath([CallerFilePath] string path = "") => path;

    private static string Normalize(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    private static bool IsSameOrUnder(string child, string parent) =>
        string.Equals(child, parent, StringComparison.OrdinalIgnoreCase)
        || child.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
