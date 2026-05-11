using MonorailCss.Build.Tasks.Scanning;
using Shouldly;
using System.IO.Abstractions.TestingHelpers;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

namespace MonorailCss.Build.Tasks.Tests;

public class GlobScannerTests
{
    [Fact]
    public void IsGlobPattern_WithWildcard_ReturnsTrue()
    {
        // Act & Assert
        GlobScanner.IsGlobPattern("*.cs").ShouldBeTrue();
        GlobScanner.IsGlobPattern("**/*.razor").ShouldBeTrue();
        GlobScanner.IsGlobPattern("src/**/*.html").ShouldBeTrue();
    }

    [Fact]
    public void IsGlobPattern_WithBraceExpansion_ReturnsTrue()
    {
        // Act & Assert
        GlobScanner.IsGlobPattern("{Pages,Components}/**/*.razor").ShouldBeTrue();
        GlobScanner.IsGlobPattern("**/*.{razor,cs}").ShouldBeTrue();
    }

    [Fact]
    public void IsGlobPattern_WithQuestionMark_ReturnsTrue()
    {
        // Act & Assert
        GlobScanner.IsGlobPattern("file?.txt").ShouldBeTrue();
    }

    [Fact]
    public void IsGlobPattern_WithLiteralPath_ReturnsFalse()
    {
        // Act & Assert
        GlobScanner.IsGlobPattern("src/pages/index.html").ShouldBeFalse();
        GlobScanner.IsGlobPattern("C:\\Users\\file.txt").ShouldBeFalse();
    }

    [Fact]
    public void ExpandGlob_SimpleWildcard_MatchesFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");

        fileSystem.AddFile(XFS.Path(@"C:\project\file1.cs"), new MockFileData("class Foo {}"));
        fileSystem.AddFile(XFS.Path(@"C:\project\file2.cs"), new MockFileData("class Bar {}"));
        fileSystem.AddFile(XFS.Path(@"C:\project\readme.txt"), new MockFileData("readme"));

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "*.cs", excludeDirs).ToList();

        // Assert
        matches.Count.ShouldBe(2);
        matches.ShouldContain(XFS.Path(@"C:\project\file1.cs"));
        matches.ShouldContain(XFS.Path(@"C:\project\file2.cs"));
    }

    [Fact]
    public void ExpandGlob_RecursiveWildcard_MatchesNestedFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");

        fileSystem.AddFile(XFS.Path(@"C:\project\Pages\Index.razor"), new MockFileData("<h1>Index</h1>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Pages\About.razor"), new MockFileData("<h1>About</h1>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Components\Button.razor"), new MockFileData("<button></button>"));

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "**/*.razor", excludeDirs).ToList();

        // Assert
        matches.Count.ShouldBe(3);
        matches.ShouldContain(XFS.Path(@"C:\project\Pages\Index.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Pages\About.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Components\Button.razor"));
    }

    [Fact]
    public void ExpandGlob_BraceExpansion_MatchesMultipleDirectories()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");

        fileSystem.AddFile(XFS.Path(@"C:\project\Pages\Index.razor"), new MockFileData("<h1>Index</h1>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Components\Button.razor"), new MockFileData("<button></button>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Layouts\Main.razor"), new MockFileData("<main></main>"));

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "{Pages,Components}/**/*.razor", excludeDirs).ToList();

        // Assert
        matches.Count.ShouldBe(2);
        matches.ShouldContain(XFS.Path(@"C:\project\Pages\Index.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Components\Button.razor"));
        matches.ShouldNotContain(XFS.Path(@"C:\project\Layouts\Main.razor"));
    }

    [Fact]
    public void ExpandGlob_MultipleExtensions_MatchesBothExtensions()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");

        fileSystem.AddFile(XFS.Path(@"C:\project\Page.razor"), new MockFileData("<h1>Page</h1>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Page.razor.cs"), new MockFileData("public class Page {}"));
        fileSystem.AddFile(XFS.Path(@"C:\project\readme.txt"), new MockFileData("readme"));

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "*.{razor,cs}", excludeDirs).ToList();

        // Assert
        matches.Count.ShouldBe(2);
        matches.ShouldContain(XFS.Path(@"C:\project\Page.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Page.razor.cs"));
        matches.ShouldNotContain(XFS.Path(@"C:\project\readme.txt"));
    }

    [Fact]
    public void ExpandGlob_ExcludesSpecifiedDirectories()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");

        fileSystem.AddFile(XFS.Path(@"C:\project\src\app.cs"), new MockFileData("class App {}"));
        fileSystem.AddFile(XFS.Path(@"C:\project\bin\Debug\app.cs"), new MockFileData("compiled"));
        fileSystem.AddFile(XFS.Path(@"C:\project\obj\Debug\app.cs"), new MockFileData("temp"));

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "**/*.cs", excludeDirs).ToList();

        // Assert
        matches.Count.ShouldBe(1);
        matches.ShouldContain(XFS.Path(@"C:\project\src\app.cs"));
        matches.ShouldNotContain(XFS.Path(@"C:\project\bin\Debug\app.cs"));
        matches.ShouldNotContain(XFS.Path(@"C:\project\obj\Debug\app.cs"));
    }

    [Fact]
    public void ExpandGlob_ComplexPattern_WithBracesAndMultipleExtensions()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");

        // Pages directory
        fileSystem.AddFile(XFS.Path(@"C:\project\Pages\Index.razor"), new MockFileData("<h1>Index</h1>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Pages\Index.razor.cs"), new MockFileData("public class Index {}"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Pages\About.razor"), new MockFileData("<h1>About</h1>"));

        // Components directory
        fileSystem.AddFile(XFS.Path(@"C:\project\Components\Button.razor"), new MockFileData("<button></button>"));
        fileSystem.AddFile(XFS.Path(@"C:\project\Components\Button.razor.cs"), new MockFileData("public class Button {}"));

        // Layouts directory (should not match)
        fileSystem.AddFile(XFS.Path(@"C:\project\Layouts\Main.razor"), new MockFileData("<main></main>"));

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "{Pages,Components}/**/*.{razor,razor.cs}", excludeDirs).ToList();

        // Assert
        matches.Count.ShouldBe(5);
        matches.ShouldContain(XFS.Path(@"C:\project\Pages\Index.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Pages\Index.razor.cs"));
        matches.ShouldContain(XFS.Path(@"C:\project\Pages\About.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Components\Button.razor"));
        matches.ShouldContain(XFS.Path(@"C:\project\Components\Button.razor.cs"));
        matches.ShouldNotContain(XFS.Path(@"C:\project\Layouts\Main.razor"));
    }

    [Fact]
    public void ExpandGlob_NonExistentDirectory_ReturnsEmpty()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(XFS.Path(@"C:\nonexistent"), "**/*.cs", excludeDirs).ToList();

        // Assert
        matches.ShouldBeEmpty();
    }

    [Fact]
    public void ExpandGlob_EmptyDirectory_ReturnsEmpty()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var baseDir = XFS.Path(@"C:\project");
        fileSystem.AddDirectory(baseDir);

        var scanner = new GlobScanner(fileSystem);
        var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

        // Act
        var matches = scanner.ExpandGlob(baseDir, "**/*.cs", excludeDirs).ToList();

        // Assert
        matches.ShouldBeEmpty();
    }
}
