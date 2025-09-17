using System.Collections.Immutable;
using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class ThemeToCssConverterTests
{
    private readonly ThemeToCssConverter _converter = new();
    private readonly CssThemeParser _parser = new();

    [Fact]
    public void ConvertTheme_EmptyTheme_ReturnsEmptyString()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var result = _converter.ConvertTheme(theme);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConvertTheme_WithVariables_GeneratesThemeBlock()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-4", "1rem");

        // Act
        var result = _converter.ConvertTheme(theme);

        // Assert
        result.ShouldContain("@theme {");
        result.ShouldContain("--color-blue-500: #3b82f6;");
        result.ShouldContain("--color-red-500: #ef4444;");
        result.ShouldContain("--spacing-4: 1rem;");
        result.ShouldContain("}");
    }

    [Fact]
    public void ConvertTheme_WithImport_IncludesImportStatement()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444");

        // Act
        var result = _converter.ConvertTheme(theme, includeImport: true);

        // Assert
        result.ShouldStartWith("@import \"tailwindcss\";");
        result.ShouldContain("@theme {");
    }

    [Fact]
    public void ConvertApplies_EmptyApplies_ReturnsEmptyString()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty;

        // Act
        var result = _converter.ConvertApplies(applies);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ConvertApplies_WithComponents_GeneratesRules()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-blue-500 text-white p-4")
            .Add(".card", "shadow-lg rounded-lg");

        // Act
        var result = _converter.ConvertApplies(applies);

        // Assert
        result.ShouldContain(".btn {");
        result.ShouldContain("@apply bg-blue-500 text-white p-4;");
        result.ShouldContain("}");
        result.ShouldContain(".card {");
        result.ShouldContain("@apply shadow-lg rounded-lg;");
    }

    [Fact]
    public void ConvertApplies_WithVariants_PreservesVariants()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-red-400 dark:bg-green-500 hover:bg-orange-500");

        // Act
        var result = _converter.ConvertApplies(applies);

        // Assert
        result.ShouldContain("@apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;");
    }

    [Fact]
    public void Convert_CompleteExample_GeneratesFullCss()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-orange-500", "purple")
            .Add("--color-blue-500", "#3b82f6");

        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-red-400 dark:bg-green-500 hover:bg-orange-500")
            .Add(".card", "shadow-lg rounded-lg p-6");

        // Act
        var result = _converter.Convert(theme, applies, includeImport: true);

        // Assert
        result.ShouldStartWith("@import \"tailwindcss\";");
        result.ShouldContain("@theme {");
        result.ShouldContain("--color-orange-500: purple;");
        result.ShouldContain("--color-blue-500: #3b82f6;");
        result.ShouldContain("}");
        result.ShouldContain(".btn {");
        result.ShouldContain("@apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;");
        result.ShouldContain(".card {");
        result.ShouldContain("@apply shadow-lg rounded-lg p-6;");
    }

    [Fact]
    public void Convert_OnlyTheme_NoApplies()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444");

        // Act
        var result = _converter.Convert(theme, includeImport: false);

        // Assert
        result.ShouldNotContain("@import");
        result.ShouldContain("@theme {");
        result.ShouldContain("--color-red-500: #ef4444;");
        result.ShouldNotContain("@apply");
    }

    [Fact]
    public void ConvertSettings_ConvertsFullSettings()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            Theme = MonorailCss.Theme.Theme.CreateEmpty()
                .Add("--color-custom", "#123456"),
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "bg-blue-500 text-white")
        };

        // Act
        var result = _converter.ConvertSettings(settings);

        // Assert
        result.ShouldContain("@import \"tailwindcss\";");
        result.ShouldContain("--color-custom: #123456;");
        result.ShouldContain(".btn {");
        result.ShouldContain("@apply bg-blue-500 text-white;");
    }

    [Fact]
    public void RoundTrip_ThemeToCSsToTheme_PreservesData()
    {
        // Arrange
        var originalTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-lg", "2rem");

        // Act - Convert to CSS
        var css = _converter.ConvertTheme(originalTheme);

        // Parse the CSS back
        var parseResult = _parser.Parse(css);

        // Assert - Theme variables should match
        parseResult.ThemeVariables.Count.ShouldBe(3);
        parseResult.ThemeVariables["--color-red-500"].ShouldBe("#ef4444");
        parseResult.ThemeVariables["--color-blue-500"].ShouldBe("#3b82f6");
        parseResult.ThemeVariables["--spacing-lg"].ShouldBe("2rem");
    }

    [Fact]
    public void RoundTrip_AppliesToCssToApplies_PreservesData()
    {
        // Arrange
        var originalApplies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-blue-500 text-white p-4")
            .Add(".card", "shadow-lg rounded-lg")
            .Add(".link", "text-blue-500 underline hover:text-blue-700");

        // Act - Convert to CSS
        var css = _converter.ConvertApplies(originalApplies);

        // Parse the CSS back
        var parseResult = _parser.Parse(css);

        // Assert - Component rules should match
        parseResult.ComponentRules.Count.ShouldBe(3);
        parseResult.ComponentRules[".btn"].ShouldBe("bg-blue-500 text-white p-4");
        parseResult.ComponentRules[".card"].ShouldBe("shadow-lg rounded-lg");
        parseResult.ComponentRules[".link"].ShouldBe("text-blue-500 underline hover:text-blue-700");
    }

    [Fact]
    public void RoundTrip_CompleteSettings_PreservesEverything()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-orange-500", "purple")
            .Add("--font-display", "'Inter', sans-serif");

        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-red-400 dark:bg-green-500")
            .Add(".card", "p-6 shadow-lg");

        // Act - Convert to CSS
        var css = _converter.Convert(theme, applies, includeImport: true);

        // Parse the CSS back
        var parseResult = _parser.Parse(css);

        // Assert
        parseResult.HasImport.ShouldBeTrue();

        // Theme variables preserved
        parseResult.ThemeVariables.Count.ShouldBe(2);
        parseResult.ThemeVariables["--color-orange-500"].ShouldBe("purple");
        parseResult.ThemeVariables["--font-display"].ShouldBe("'Inter', sans-serif");

        // Component rules preserved
        parseResult.ComponentRules.Count.ShouldBe(2);
        parseResult.ComponentRules[".btn"].ShouldBe("bg-red-400 dark:bg-green-500");
        parseResult.ComponentRules[".card"].ShouldBe("p-6 shadow-lg");
    }

    [Fact]
    public void ConvertTheme_NullTheme_ThrowsException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _converter.ConvertTheme(null!));
    }

    [Fact]
    public void Convert_NullTheme_ThrowsException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _converter.Convert(null!));
    }

    [Fact]
    public void ConvertSettings_NullSettings_ThrowsException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _converter.ConvertSettings(null!));
    }

    [Fact]
    public void ConvertTheme_OrdersVariablesAlphabetically()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--z-index-10", "10")
            .Add("--color-red-500", "#ef4444")
            .Add("--animation-spin", "spin 1s linear infinite");

        // Act
        var result = _converter.ConvertTheme(theme);
        var lines = result.Split('\n').Where(l => l.Contains(':')).ToList();

        // Assert - Should be in alphabetical order
        lines[0].ShouldContain("--animation-spin");
        lines[1].ShouldContain("--color-red-500");
        lines[2].ShouldContain("--z-index-10");
    }

    [Fact]
    public void ConvertTheme_ExcludeDefaultsTrue_FiltersOutDefaultValues()
    {
        // Arrange - Create theme with both default and custom values
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "oklch(63.7% 0.237 25.331)") // This is the default value from ThemeDefaults
            .Add("--color-red-600", "#dc2626") // Custom value
            .Add("--font-sans", "ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'") // Default
            .Add("--color-custom", "#123456"); // Custom value

        // Act - With excludeDefaults=true (default)
        var resultWithDefaults = _converter.ConvertTheme(theme, includeImport: false, excludeDefaults: true);

        // Assert - Should only include non-default values
        resultWithDefaults.ShouldNotContain("--color-red-500"); // Default value should be excluded
        resultWithDefaults.ShouldNotContain("--font-sans"); // Default value should be excluded
        resultWithDefaults.ShouldContain("--color-red-600: #dc2626;"); // Custom value should be included
        resultWithDefaults.ShouldContain("--color-custom: #123456;"); // Custom value should be included
    }

    [Fact]
    public void ConvertTheme_ExcludeDefaultsFalse_IncludesAllValues()
    {
        // Arrange - Create theme with both default and custom values
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "oklch(63.7% 0.237 25.331)") // This is the default value from ThemeDefaults
            .Add("--color-red-600", "#dc2626") // Custom value
            .Add("--font-sans", "ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'") // Default
            .Add("--color-custom", "#123456"); // Custom value

        // Act - With excludeDefaults=false
        var resultWithoutFiltering = _converter.ConvertTheme(theme, includeImport: false, excludeDefaults: false);

        // Assert - Should include all values, both default and custom
        resultWithoutFiltering.ShouldContain("--color-red-500: oklch(63.7% 0.237 25.331);"); // Default value included
        resultWithoutFiltering.ShouldContain("--font-sans: ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji';"); // Default included
        resultWithoutFiltering.ShouldContain("--color-red-600: #dc2626;"); // Custom value included
        resultWithoutFiltering.ShouldContain("--color-custom: #123456;"); // Custom value included
    }

    [Fact]
    public void ConvertTheme_DefaultValueOverridden_IncludesCustomValue()
    {
        // Arrange - Override a default value with a custom one
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ff0000") // Override default red-500 with pure red
            .Add("--font-mono", "'Fira Code', monospace"); // Override default font-mono

        // Act - With excludeDefaults=true
        var result = _converter.ConvertTheme(theme, includeImport: false, excludeDefaults: true);

        // Assert - Should include overridden values since they differ from defaults
        result.ShouldContain("--color-red-500: #ff0000;"); // Custom override included
        result.ShouldContain("--font-mono: 'Fira Code', monospace;"); // Custom override included
    }

    [Fact]
    public void Convert_ExcludeDefaults_FiltersThemeButKeepsApplies()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "oklch(63.7% 0.237 25.331)") // Default value
            .Add("--color-custom", "#123456"); // Custom value

        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-red-500 text-white");

        // Act - With excludeDefaults=true
        var result = _converter.Convert(theme, applies, includeImport: false, excludeDefaults: true);

        // Assert
        result.ShouldNotContain("--color-red-500"); // Default theme value excluded
        result.ShouldContain("--color-custom: #123456;"); // Custom theme value included
        result.ShouldContain(".btn {"); // Applies always included
        result.ShouldContain("@apply bg-red-500 text-white;"); // Applies content preserved
    }
}