using MonorailCss.Build.Tasks.Parsing;
using Shouldly;

namespace MonorailCss.Build.Tasks.Tests;

public class PathPlaceholderResolverTests
{
    [Fact]
    public void ResolvePlaceholders_WithAllPlaceholders_ReplacesAll()
    {
        // Arrange
        const string path = "../bin/{Configuration}/{TargetFramework}/{RuntimeIdentifier}/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: "win-x64");

        // Assert
        result.ShouldBe("../bin/Debug/net9.0/win-x64/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithConfigurationOnly_ReplacesConfiguration()
    {
        // Arrange
        const string path = "../bin/{Configuration}/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Release",
            targetFramework: null,
            runtimeIdentifier: null);

        // Assert
        result.ShouldBe("../bin/Release/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithTargetFrameworkOnly_ReplacesTargetFramework()
    {
        // Arrange
        const string path = "../bin/{TargetFramework}/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: null,
            targetFramework: "net8.0",
            runtimeIdentifier: null);

        // Assert
        result.ShouldBe("../bin/net8.0/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithRuntimeIdentifierOnly_ReplacesRuntimeIdentifier()
    {
        // Arrange
        const string path = "../bin/{RuntimeIdentifier}/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: null,
            targetFramework: null,
            runtimeIdentifier: "linux-x64");

        // Assert
        result.ShouldBe("../bin/linux-x64/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_CaseInsensitive_ReplacesPlaceholders()
    {
        // Arrange - Test various casing combinations
        const string path1 = "../bin/{configuration}/MyLib.dll";
        const string path2 = "../bin/{CONFIGURATION}/MyLib.dll";
        const string path3 = "../bin/{Configuration}/MyLib.dll";

        // Act
        var result1 = PathPlaceholderResolver.ResolvePlaceholders(path1, "Debug", null, null);
        var result2 = PathPlaceholderResolver.ResolvePlaceholders(path2, "Debug", null, null);
        var result3 = PathPlaceholderResolver.ResolvePlaceholders(path3, "Debug", null, null);

        // Assert - All should resolve the same way
        result1.ShouldBe("../bin/Debug/MyLib.dll");
        result2.ShouldBe("../bin/Debug/MyLib.dll");
        result3.ShouldBe("../bin/Debug/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithNullValues_LeavesPlaceholdersUnchanged()
    {
        // Arrange
        const string path = "../bin/{Configuration}/{TargetFramework}/MyLib.dll";

        // Act - Pass null for all values
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: null,
            targetFramework: null,
            runtimeIdentifier: null);

        // Assert - Placeholders should remain unchanged
        result.ShouldBe("../bin/{Configuration}/{TargetFramework}/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithEmptyStrings_LeavesPlaceholdersUnchanged()
    {
        // Arrange
        const string path = "../bin/{Configuration}/{TargetFramework}/MyLib.dll";

        // Act - Pass empty strings
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "",
            targetFramework: "",
            runtimeIdentifier: "");

        // Assert - Placeholders should remain unchanged
        result.ShouldBe("../bin/{Configuration}/{TargetFramework}/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithPartialResolve_ReplacesOnlyAvailablePlaceholders()
    {
        // Arrange
        const string path = "../bin/{Configuration}/{TargetFramework}/{RuntimeIdentifier}/MyLib.dll";

        // Act - Only provide Configuration
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: null,
            runtimeIdentifier: null);

        // Assert - Only Configuration should be replaced
        result.ShouldBe("../bin/Debug/{TargetFramework}/{RuntimeIdentifier}/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_RealWorldExample_LumexUIScenario()
    {
        // Arrange - The original use case that prompted this feature
        const string path = "../bin/{Configuration}/{TargetFramework}/LumexUI.dll";

        // Act - Debug build for net9.0
        var debugResult = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);

        // Act - Release build for net9.0
        var releaseResult = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Release",
            targetFramework: "net9.0",
            runtimeIdentifier: null);

        // Assert
        debugResult.ShouldBe("../bin/Debug/net9.0/LumexUI.dll");
        releaseResult.ShouldBe("../bin/Release/net9.0/LumexUI.dll");
    }

    [Fact]
    public void ResolvePlaceholders_WithNullOrWhitespacePath_ReturnsOriginal()
    {
        // Act & Assert - Null path
        var nullResult = PathPlaceholderResolver.ResolvePlaceholders(
            null!,
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);
        nullResult.ShouldBeNull();

        // Act & Assert - Empty path
        var emptyResult = PathPlaceholderResolver.ResolvePlaceholders(
            "",
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);
        emptyResult.ShouldBe("");

        // Act & Assert - Whitespace path
        var whitespaceResult = PathPlaceholderResolver.ResolvePlaceholders(
            "   ",
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);
        whitespaceResult.ShouldBe("   ");
    }

    [Fact]
    public void ResolvePlaceholders_WithMultipleSamePlaceholders_ReplacesAll()
    {
        // Arrange - Multiple occurrences of the same placeholder
        const string path = "../{Configuration}/bin/{Configuration}/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: null,
            runtimeIdentifier: null);

        // Assert - All occurrences should be replaced
        result.ShouldBe("../Debug/bin/Debug/MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_PathWithoutPlaceholders_ReturnsUnchanged()
    {
        // Arrange - Regular path without placeholders
        const string path = "../bin/Debug/net9.0/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Release",
            targetFramework: "net8.0",
            runtimeIdentifier: "win-x64");

        // Assert - Path should remain unchanged
        result.ShouldBe("../bin/Debug/net9.0/MyLib.dll");
    }

    [Fact]
    public void ContainsPlaceholders_WithPlaceholders_ReturnsTrue()
    {
        // Arrange & Act & Assert
        PathPlaceholderResolver.ContainsPlaceholders("../bin/{Configuration}/MyLib.dll").ShouldBeTrue();
        PathPlaceholderResolver.ContainsPlaceholders("../bin/{TargetFramework}/MyLib.dll").ShouldBeTrue();
        PathPlaceholderResolver.ContainsPlaceholders("../bin/{RuntimeIdentifier}/MyLib.dll").ShouldBeTrue();
        PathPlaceholderResolver.ContainsPlaceholders("../{Configuration}/{TargetFramework}/MyLib.dll").ShouldBeTrue();
    }

    [Fact]
    public void ContainsPlaceholders_WithoutPlaceholders_ReturnsFalse()
    {
        // Arrange & Act & Assert
        PathPlaceholderResolver.ContainsPlaceholders("../bin/Debug/net9.0/MyLib.dll").ShouldBeFalse();
        PathPlaceholderResolver.ContainsPlaceholders("../bin/Release/MyLib.dll").ShouldBeFalse();
        PathPlaceholderResolver.ContainsPlaceholders("simple-path").ShouldBeFalse();
    }

    [Fact]
    public void ContainsPlaceholders_WithUnknownPlaceholder_ReturnsTrue()
    {
        // Arrange - Unknown placeholder that we don't support
        const string path = "../bin/{UnknownPlaceholder}/MyLib.dll";

        // Act & Assert - Should still detect it as a placeholder
        PathPlaceholderResolver.ContainsPlaceholders(path).ShouldBeTrue();
    }

    [Fact]
    public void ContainsPlaceholders_WithNullOrWhitespace_ReturnsFalse()
    {
        // Act & Assert
        PathPlaceholderResolver.ContainsPlaceholders(null!).ShouldBeFalse();
        PathPlaceholderResolver.ContainsPlaceholders("").ShouldBeFalse();
        PathPlaceholderResolver.ContainsPlaceholders("   ").ShouldBeFalse();
    }

    [Fact]
    public void ResolvePlaceholders_WithBraceNotPlaceholder_LeavesUnchanged()
    {
        // Arrange - Brace pattern that's not a placeholder (glob pattern)
        const string path = "../src/{Pages,Components}/**/*.razor";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);

        // Assert - Glob braces should be untouched (they contain commas, not valid placeholder names)
        result.ShouldBe("../src/{Pages,Components}/**/*.razor");
    }

    [Fact]
    public void ContainsPlaceholders_WithGlobBraces_ReturnsTrue()
    {
        // Arrange - Glob pattern with braces (technically contains braces, so should return true)
        const string path = "../src/{Pages,Components}/**/*.razor";

        // Act & Assert - This is detected as containing placeholders because it has {content}
        // This is OK because we warn about unresolved placeholders in the task
        PathPlaceholderResolver.ContainsPlaceholders(path).ShouldBeTrue();
    }

    [Fact]
    public void ResolvePlaceholders_WindowsPath_ResolvesCorrectly()
    {
        // Arrange - Windows-style path with backslashes
        const string path = @"..\bin\{Configuration}\{TargetFramework}\MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);

        // Assert
        result.ShouldBe(@"..\bin\Debug\net9.0\MyLib.dll");
    }

    [Fact]
    public void ResolvePlaceholders_UnixPath_ResolvesCorrectly()
    {
        // Arrange - Unix-style path with forward slashes
        const string path = "../bin/{Configuration}/{TargetFramework}/MyLib.dll";

        // Act
        var result = PathPlaceholderResolver.ResolvePlaceholders(
            path,
            configuration: "Debug",
            targetFramework: "net9.0",
            runtimeIdentifier: null);

        // Assert
        result.ShouldBe("../bin/Debug/net9.0/MyLib.dll");
    }
}
