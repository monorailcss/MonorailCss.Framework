using System.Collections.Immutable;
using MonorailCss.Theme;
using Shouldly;

namespace MonorailCss.Tests.Theme;

public class CssThemeBuilderTests
{
    private readonly CssThemeBuilder _builder = new();

    [Fact]
    public void MergeWithCssSources_NullBaseTheme_ThrowsException()
    {
        // Arrange
        MonorailCss.Theme.Theme? baseTheme = null;
        var cssSources = new[] { "@theme { --color-red-500: #ef4444; }" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _builder.MergeWithCssSources(baseTheme!, cssSources));
    }

    [Fact]
    public void MergeWithCssSources_EmptyCssSources_ReturnsBaseTheme()
    {
        // Arrange
        var baseTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-blue-500", "#3b82f6");
        var cssSources = Array.Empty<string>();

        // Act
        var result = _builder.MergeWithCssSources(baseTheme, cssSources);

        // Assert
        result.ShouldBe(baseTheme);
    }

    [Fact]
    public void MergeWithCssSources_SingleCssSource_MergesThemeVariables()
    {
        // Arrange
        var baseTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-blue-500", "#3b82f6");
        var cssSources = new[] {
            @"
            @theme {
                --color-red-500: #ef4444;
                --color-green-500: #10b981;
            }
            "
        };

        // Act
        var result = _builder.MergeWithCssSources(baseTheme, cssSources);

        // Assert
        result.ResolveValue("--color-blue-500", []).ShouldBe("#3b82f6");
        result.ResolveValue("--color-red-500", []).ShouldBe("#ef4444");
        result.ResolveValue("--color-green-500", []).ShouldBe("#10b981");
    }

    [Fact]
    public void MergeWithCssSources_OverrideExistingVariable_UsesLastValue()
    {
        // Arrange
        var baseTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-orange-500", "#f97316");
        var cssSources = new[] {
            "@theme { --color-orange-500: purple; }"
        };

        // Act
        var result = _builder.MergeWithCssSources(baseTheme, cssSources);

        // Assert
        result.ResolveValue("--color-orange-500", []).ShouldBe("purple");
    }

    [Fact]
    public void MergeWithCssSources_MultipleCssSources_ProcessesInOrder()
    {
        // Arrange
        var baseTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444");
        var cssSources = new[] {
            "@theme { --color-red-500: crimson; --color-blue-500: blue; }",
            "@theme { --color-red-500: darkred; }" // This should win
        };

        // Act
        var result = _builder.MergeWithCssSources(baseTheme, cssSources);

        // Assert
        result.ResolveValue("--color-red-500", []).ShouldBe("darkred");
        result.ResolveValue("--color-blue-500", []).ShouldBe("blue");
    }

    [Fact]
    public void ExtractApplies_EmptyCssSources_ReturnsEmpty()
    {
        // Arrange
        var cssSources = Array.Empty<string>();

        // Act
        var result = _builder.ExtractApplies(cssSources);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ExtractApplies_SingleComponent_ExtractsCorrectly()
    {
        // Arrange
        var cssSources = new[] {
            ".btn { @apply bg-blue-500 text-white; }"
        };

        // Act
        var result = _builder.ExtractApplies(cssSources);

        // Assert
        result.Count.ShouldBe(1);
        result[".btn"].ShouldBe("bg-blue-500 text-white");
    }

    [Fact]
    public void ExtractApplies_MultipleComponents_ExtractsAll()
    {
        // Arrange
        var cssSources = new[] {
            @"
            .btn { @apply bg-blue-500 text-white; }
            .card { @apply shadow-lg p-4; }
            "
        };

        // Act
        var result = _builder.ExtractApplies(cssSources);

        // Assert
        result.Count.ShouldBe(2);
        result[".btn"].ShouldBe("bg-blue-500 text-white");
        result[".card"].ShouldBe("shadow-lg p-4");
    }

    [Fact]
    public void ExtractApplies_DuplicateSelectors_LastOneWins()
    {
        // Arrange
        var cssSources = new[] {
            ".btn { @apply bg-red-500; }",
            ".btn { @apply bg-blue-500 text-white; }" // This should win
        };

        // Act
        var result = _builder.ExtractApplies(cssSources);

        // Assert
        result.Count.ShouldBe(1);
        result[".btn"].ShouldBe("bg-blue-500 text-white");
    }

    [Fact]
    public void ProcessCssSources_CompleteExample_ProcessesBothThemeAndApplies()
    {
        // Arrange
        var baseTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-blue-500", "#3b82f6");
        var baseApplies = ImmutableDictionary<string, string>.Empty
            .Add(".card", "shadow-md");
        var cssSources = new[] {
            @"
            @import ""tailwindcss"";

            @theme {
                --color-orange-500: purple;
                --color-green-500: #10b981;
            }

            .btn {
                @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
            }

            .card {
                @apply shadow-lg rounded-lg;
            }
            "
        };

        // Act
        var (resultTheme, resultApplies) = _builder.ProcessCssSources(baseTheme, baseApplies, cssSources);

        // Assert
        // Check theme
        resultTheme.ResolveValue("--color-blue-500", []).ShouldBe("#3b82f6");
        resultTheme.ResolveValue("--color-orange-500", []).ShouldBe("purple");
        resultTheme.ResolveValue("--color-green-500", []).ShouldBe("#10b981");

        // Check applies
        resultApplies.Count.ShouldBe(2);
        resultApplies[".btn"].ShouldBe("bg-red-400 dark:bg-green-500 hover:bg-orange-500");
        resultApplies[".card"].ShouldBe("shadow-lg rounded-lg"); // Override from CSS
    }

    [Fact]
    public void ProcessCssSources_ChainMultipleSources_MaintainsOrder()
    {
        // Arrange
        var baseTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--spacing-4", "1rem");
        var baseApplies = ImmutableDictionary<string, string>.Empty;

        var cssSources = new[] {
            // First CSS source
            @"
            @theme {
                --color-primary: blue;
                --spacing-4: 1.5rem;
            }
            .btn { @apply bg-blue-500; }
            ",
            // Second CSS source (overrides)
            @"
            @theme {
                --color-primary: green;
            }
            .btn { @apply bg-green-500 text-white; }
            .link { @apply text-blue-500 underline; }
            "
        };

        // Act
        var (resultTheme, resultApplies) = _builder.ProcessCssSources(baseTheme, baseApplies, cssSources);

        // Assert
        // Theme: last values win
        resultTheme.ResolveValue("--color-primary", []).ShouldBe("green");
        resultTheme.ResolveValue("--spacing-4", []).ShouldBe("1.5rem");

        // Applies: last values win for duplicates
        resultApplies.Count.ShouldBe(2);
        resultApplies[".btn"].ShouldBe("bg-green-500 text-white");
        resultApplies[".link"].ShouldBe("text-blue-500 underline");
    }
}