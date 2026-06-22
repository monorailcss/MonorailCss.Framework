using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Tests.Discovery;

/// <summary>
/// Locks the shared path policy used by both the recursive source walk and the referenced-project
/// root resolver, so the two consumers can't drift on what counts as ignored / scannable.
/// </summary>
public class DiscoveryPathsTests
{
    [Theory]
    [InlineData("Foo.razor", true)]
    [InlineData("Foo.cshtml", true)]
    [InlineData("Foo.cs", true)]
    [InlineData("Foo.html", true)]
    [InlineData("Foo.RAZOR", true)]
    [InlineData("Foo.md", false)]
    [InlineData("Foo.js", false)]
    [InlineData("Foo", false)]
    public void HasSupportedExtension_matches_scanned_source_types(string path, bool expected)
    {
        DiscoveryPaths.HasSupportedExtension(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData(@"C:\proj\obj\Debug\Foo.g.cs", true)]
    [InlineData(@"C:\proj\bin\Release\Foo.cs", true)]
    [InlineData(@"C:\proj\node_modules\pkg\Foo.cs", true)]
    [InlineData(@"C:\proj\.git\Foo.cs", true)]
    [InlineData(@"C:\proj\Components\Foo.razor", false)]
    [InlineData(@"C:\proj\src\Foo.cs", false)]
    public void IsInIgnoredDirectory_detects_ignored_segments(string path, bool expected)
    {
        DiscoveryPaths.IsInIgnoredDirectory(path).ShouldBe(expected);
    }

    [Fact]
    public void IsInIgnoredDirectory_does_not_false_match_on_substring()
    {
        // "obj" must match as a whole segment, not as a substring of "object-models".
        DiscoveryPaths.IsInIgnoredDirectory(@"C:\proj\object-models\Foo.cs").ShouldBeFalse();
        DiscoveryPaths.IsInIgnoredDirectory(@"C:\proj\binders\Foo.cs").ShouldBeFalse();
    }
}
