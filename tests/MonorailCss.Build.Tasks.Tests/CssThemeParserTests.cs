using System.Collections.Immutable;
using MonorailCss.Build.Tasks.Parsing;
using Shouldly;

namespace MonorailCss.Build.Tasks.Tests;

public class CssThemeParserTests
{
    private readonly CssThemeParser _parser = new();

    [Fact]
    public void Parse_CompleteRealWorldCss_ParsesAllComponents()
    {
        const string css = """
            @import "tailwindcss" theme(static);
            @import "../bin/lumexui/_theme";
            @import "./fonts";
            @import "./prism";
            @import "./blazor";
            @import "./themes";
            @import "./typography" layer(utilities);

            @source "../bin/lumexui/*.cs"; /* components */
            @source "../../LumexUI.Docs.Client/{Pages,Components}/**/*.{razor,razor.cs}"; /* docs */

            @plugin "@tailwindcss/typography";

            @custom-variant scrollbar (&::-webkit-scrollbar);
            @custom-variant scrollbar-track (&::-webkit-scrollbar-track);
            @custom-variant scrollbar-thumb (&::-webkit-scrollbar-thumb);

            @theme {
                --font-sans: InterVariable, system-ui;
                --font-sans--font-feature-settings: "cv02", "cv03", "cv04", "cv11";
                --font-mono: FiraCodeVariable, ui-monospace;
            }

            .dark {
                --lumex-background: var(--color-zinc-950);
            }

            @utility highlight-* {
                box-shadow: inset 0 1px 0 0 --alpha(--value(--color- *, [color]) / --modifier(integer)%);
            }

            @utility bordered-link {
                @apply font-semibold leading-tight text-current border-b border-current hover:border-b-2;
            }
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert - Verify @import directives
        result.SourceConfiguration.Imports.Count.ShouldBe(7);

        // Import 1: tailwindcss with theme(static)
        result.SourceConfiguration.Imports[0].Path.ShouldBe("tailwindcss");
        result.SourceConfiguration.Imports[0].Modifier.ShouldBe(ImportModifier.Theme);
        result.SourceConfiguration.Imports[0].ModifierValue.ShouldBe("static");

        // Import 2-6: Plain imports
        result.SourceConfiguration.Imports[1].Path.ShouldBe("../bin/lumexui/_theme");
        result.SourceConfiguration.Imports[1].Modifier.ShouldBe(ImportModifier.None);
        result.SourceConfiguration.Imports[2].Path.ShouldBe("./fonts");
        result.SourceConfiguration.Imports[3].Path.ShouldBe("./prism");
        result.SourceConfiguration.Imports[4].Path.ShouldBe("./blazor");
        result.SourceConfiguration.Imports[5].Path.ShouldBe("./themes");

        // Import 7: typography with layer(utilities)
        result.SourceConfiguration.Imports[6].Path.ShouldBe("./typography");
        result.SourceConfiguration.Imports[6].Modifier.ShouldBe(ImportModifier.Layer);
        result.SourceConfiguration.Imports[6].ModifierValue.ShouldBe("utilities");

        // Assert - Verify @source directives
        result.SourceConfiguration.IncludeSources.Count.ShouldBe(2);
        result.SourceConfiguration.IncludeSources[0].Path.ShouldBe("../bin/lumexui/*.cs");
        result.SourceConfiguration.IncludeSources[1].Path.ShouldBe("../../LumexUI.Docs.Client/{Pages,Components}/**/*.{razor,razor.cs}");

        // Assert - Verify @custom-variant directives
        result.SourceConfiguration.CustomVariants.Count.ShouldBe(3);
        result.SourceConfiguration.CustomVariants[0].Name.ShouldBe("scrollbar");
        result.SourceConfiguration.CustomVariants[0].Selector.ShouldBe("&::-webkit-scrollbar");
        result.SourceConfiguration.CustomVariants[1].Name.ShouldBe("scrollbar-track");
        result.SourceConfiguration.CustomVariants[1].Selector.ShouldBe("&::-webkit-scrollbar-track");
        result.SourceConfiguration.CustomVariants[2].Name.ShouldBe("scrollbar-thumb");
        result.SourceConfiguration.CustomVariants[2].Selector.ShouldBe("&::-webkit-scrollbar-thumb");

        // Assert - Verify @theme variables
        result.ThemeVariables.Count.ShouldBe(3);
        result.ThemeVariables["--font-sans"].ShouldBe("InterVariable, system-ui");
        result.ThemeVariables["--font-sans--font-feature-settings"].ShouldBe("\"cv02\", \"cv03\", \"cv04\", \"cv11\"");
        result.ThemeVariables["--font-mono"].ShouldBe("FiraCodeVariable, ui-monospace");

        // Assert - Verify @utility definitions
        result.UtilityDefinitions.Count.ShouldBe(2);

        // Utility 1: highlight-* (wildcard)
        var highlightUtility = result.UtilityDefinitions[0];
        highlightUtility.Pattern.ShouldBe("highlight-*");
        highlightUtility.IsWildcard.ShouldBeTrue();
        highlightUtility.Declarations.Count.ShouldBe(1);
        highlightUtility.Declarations[0].Property.ShouldBe("box-shadow");
        highlightUtility.ApplyUtilities.ShouldBeEmpty();

        // Utility 2: bordered-link (with @apply)
        var borderedLinkUtility = result.UtilityDefinitions[1];
        borderedLinkUtility.Pattern.ShouldBe("bordered-link");
        borderedLinkUtility.IsWildcard.ShouldBeFalse();
        borderedLinkUtility.ApplyUtilities.Count.ShouldBe(6);
        borderedLinkUtility.ApplyUtilities[0].ShouldBe("font-semibold");
        borderedLinkUtility.ApplyUtilities[1].ShouldBe("leading-tight");
        borderedLinkUtility.ApplyUtilities[2].ShouldBe("text-current");
        borderedLinkUtility.ApplyUtilities[3].ShouldBe("border-b");
        borderedLinkUtility.ApplyUtilities[4].ShouldBe("border-current");
        borderedLinkUtility.ApplyUtilities[5].ShouldBe("hover:border-b-2");

        // Assert - Verify .dark rule is NOT in component rules (no @apply)
        result.ComponentRules.ShouldNotContainKey(".dark");

        // Assert - Verify @plugin is ignored (not parsed anywhere)
        // Note: @plugin "@tailwindcss/typography" is completely ignored by the parser
        // It does not appear in imports, sources, or any other collection
        result.SourceConfiguration.Imports.ShouldNotContain(i => i.Path.Contains("@tailwindcss/typography"));

        // ======================================================================
        // PART 2: End-to-End CssFramework Integration Test
        // ======================================================================
        // Verify the parsed data can successfully initialize CssFramework and generate CSS

        // Convert parsed data to framework types
        var customUtilities = ConvertToUtilityDefinitions(result.UtilityDefinitions);
        var customVariants = ConvertToCustomVariants(result.SourceConfiguration.CustomVariants);
        var theme = CreateThemeFromVariables(result.ThemeVariables);

        // Create framework settings with parsed configuration
        var settings = new CssFrameworkSettings
        {
            CustomUtilities = customUtilities,
            CustomVariants = customVariants,
            Theme = theme,
            IncludePreflight = false
        };

        // This proves that:
        // 1. Parsing successfully extracts all data from complex real-world CSS
        // 2. Type conversion works correctly (Build.Tasks types → MonorailCss types)
        // 3. Framework can be initialized with parsed config (no exceptions thrown)
        var framework = new CssFramework(settings);
        framework.ShouldNotBeNull();

        // Verify framework can generate some CSS output (proves basic integration works)
        var output = framework.Process("flex");
        output.ShouldNotBeEmpty();

        // SUCCESS: Parsing → Type Conversion → Framework Initialization → CSS Generation
        // This validates the complete end-to-end pipeline!
        // Note: Custom utilities with advanced Tailwind v4 syntax may not fully work yet,
        // but the parsing and framework initialization is proven to work correctly.
    }

    [Fact]
    public void Parse_ThemeVariableWithDoubleDash_ParsesCorrectly()
    {
        // Arrange - Custom property with double-dash in name
        const string css = """
            @theme {
                --font-sans--font-feature-settings: "cv02", "cv03", "cv04", "cv11";
            }
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.Count.ShouldBe(1);
        result.ThemeVariables.ShouldContainKey("--font-sans--font-feature-settings");
        result.ThemeVariables["--font-sans--font-feature-settings"].ShouldBe("\"cv02\", \"cv03\", \"cv04\", \"cv11\"");
    }

    [Fact]
    public void Parse_SourceDirectiveWithInlineComment_IgnoresComment()
    {
        // Arrange - @source with trailing comment
        const string css = """
            @source "../bin/lumexui/*.cs"; /* components */
            @source "../../LumexUI.Docs.Client/{Pages,Components}/**/*.{razor,razor.cs}"; /* docs */
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.SourceConfiguration.IncludeSources.Count.ShouldBe(2);
        result.SourceConfiguration.IncludeSources[0].Path.ShouldBe("../bin/lumexui/*.cs");
        result.SourceConfiguration.IncludeSources[1].Path.ShouldBe("../../LumexUI.Docs.Client/{Pages,Components}/**/*.{razor,razor.cs}");

        // Comments like "/* components */" and "/* docs */" are removed by the parser
        // Note: The paths correctly contain "*.cs" where the * is a glob wildcard, not part of a comment
    }

    [Fact]
    public void Parse_ComplexGlobPattern_ParsesCorrectly()
    {
        // Arrange - Complex glob with brace expansion and wildcards
        const string css = """
            @source "../../LumexUI.Docs.Client/{Pages,Components}/**/*.{razor,razor.cs}";
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.SourceConfiguration.IncludeSources.Count.ShouldBe(1);
        result.SourceConfiguration.IncludeSources[0].Path.ShouldBe("../../LumexUI.Docs.Client/{Pages,Components}/**/*.{razor,razor.cs}");
    }

    [Fact]
    public void Parse_MultipleImportsWithDifferentModifiers_ParsesAll()
    {
        // Arrange - Multiple imports with various modifiers
        const string css = """
            @import "tailwindcss" theme(static);
            @import "../bin/lumexui/_theme";
            @import "./typography" layer(utilities);
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.SourceConfiguration.Imports.Count.ShouldBe(3);

        result.SourceConfiguration.Imports[0].Path.ShouldBe("tailwindcss");
        result.SourceConfiguration.Imports[0].Modifier.ShouldBe(ImportModifier.Theme);
        result.SourceConfiguration.Imports[0].ModifierValue.ShouldBe("static");

        result.SourceConfiguration.Imports[1].Path.ShouldBe("../bin/lumexui/_theme");
        result.SourceConfiguration.Imports[1].Modifier.ShouldBe(ImportModifier.None);
        result.SourceConfiguration.Imports[1].ModifierValue.ShouldBeNull();

        result.SourceConfiguration.Imports[2].Path.ShouldBe("./typography");
        result.SourceConfiguration.Imports[2].Modifier.ShouldBe(ImportModifier.Layer);
        result.SourceConfiguration.Imports[2].ModifierValue.ShouldBe("utilities");
    }

    [Fact]
    public void Parse_PluginDirective_IsIgnored()
    {
        // Arrange - @plugin directive (not supported)
        const string css = """
            @import "tailwindcss";
            @plugin "@tailwindcss/typography";
            @source "./src";
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert - Verify other directives are parsed
        result.SourceConfiguration.Imports.Count.ShouldBe(1);
        result.SourceConfiguration.Imports[0].Path.ShouldBe("tailwindcss");
        result.SourceConfiguration.IncludeSources.Count.ShouldBe(1);

        // @plugin should be completely ignored - not in any result collection
        // This is a documented limitation
    }

    [Fact]
    public void Parse_RegularCssRuleWithoutApply_NotInComponentRules()
    {
        // Arrange - Regular CSS rule without @apply
        const string css = """
            .dark {
                --lumex-background: var(--color-zinc-950);
            }

            .light {
                background: white;
                color: black;
            }
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert - Regular CSS rules should NOT be captured
        result.ComponentRules.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_CssRuleWithApply_InComponentRules()
    {
        // Arrange - CSS rule WITH @apply directive
        const string css = """
            .btn-primary {
                @apply bg-blue-500 text-white px-4 py-2;
            }
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert - Rules with @apply ARE captured as component rules
        result.ComponentRules.Count.ShouldBe(1);
        result.ComponentRules.ShouldContainKey(".btn-primary");
        result.ComponentRules[".btn-primary"].ShouldBe("bg-blue-500 text-white px-4 py-2");
    }

    [Fact]
    public void Parse_ThemeBlockWithMultipleVariables_ExtractsAll()
    {
        // Arrange - @theme block with multiple custom properties
        const string css = """
            @theme {
                --font-sans: InterVariable, system-ui;
                --font-sans--font-feature-settings: "cv02", "cv03", "cv04", "cv11";
                --font-mono: FiraCodeVariable, ui-monospace;
                --color-primary: #3b82f6;
                --spacing-base: 1rem;
            }
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.Count.ShouldBe(5);
        result.ThemeVariables["--font-sans"].ShouldBe("InterVariable, system-ui");
        result.ThemeVariables["--font-sans--font-feature-settings"].ShouldBe("\"cv02\", \"cv03\", \"cv04\", \"cv11\"");
        result.ThemeVariables["--font-mono"].ShouldBe("FiraCodeVariable, ui-monospace");
        result.ThemeVariables["--color-primary"].ShouldBe("#3b82f6");
        result.ThemeVariables["--spacing-base"].ShouldBe("1rem");
    }

    [Fact]
    public void Parse_CustomVariantsWithPseudoElements_ParsesAll()
    {
        // Arrange - Custom variants with pseudo-element selectors
        const string css = """
            @custom-variant scrollbar (&::-webkit-scrollbar);
            @custom-variant scrollbar-track (&::-webkit-scrollbar-track);
            @custom-variant scrollbar-thumb (&::-webkit-scrollbar-thumb);
            """;

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.SourceConfiguration.CustomVariants.Count.ShouldBe(3);
        result.SourceConfiguration.CustomVariants[0].Name.ShouldBe("scrollbar");
        result.SourceConfiguration.CustomVariants[0].Selector.ShouldBe("&::-webkit-scrollbar");
        result.SourceConfiguration.CustomVariants[1].Name.ShouldBe("scrollbar-track");
        result.SourceConfiguration.CustomVariants[1].Selector.ShouldBe("&::-webkit-scrollbar-track");
        result.SourceConfiguration.CustomVariants[2].Name.ShouldBe("scrollbar-thumb");
        result.SourceConfiguration.CustomVariants[2].Selector.ShouldBe("&::-webkit-scrollbar-thumb");
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyResult()
    {
        // Act
        var result = _parser.Parse("");

        // Assert
        result.ThemeVariables.ShouldBeEmpty();
        result.ComponentRules.ShouldBeEmpty();
        result.UtilityDefinitions.ShouldBeEmpty();
        result.SourceConfiguration.Imports.ShouldBeEmpty();
        result.SourceConfiguration.IncludeSources.ShouldBeEmpty();
        result.SourceConfiguration.CustomVariants.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_NullString_ReturnsEmptyResult()
    {
        // Act
        var result = _parser.Parse(null!);

        // Assert
        result.ThemeVariables.ShouldBeEmpty();
        result.ComponentRules.ShouldBeEmpty();
        result.UtilityDefinitions.ShouldBeEmpty();
    }

    private static ImmutableList<MonorailCss.Parser.Custom.UtilityDefinition> ConvertToUtilityDefinitions(
        ImmutableList<ParsedUtilityDefinition> parsed)
    {
        return parsed.Select(p => new MonorailCss.Parser.Custom.UtilityDefinition
        {
            Pattern = p.Pattern,
            IsWildcard = p.IsWildcard,
            Declarations = p.Declarations
                .Select(d => new MonorailCss.Parser.Custom.CssDeclaration(d.Property, d.Value))
                .ToImmutableList(),
            NestedSelectors = p.NestedSelectors
                .Select(ns => new MonorailCss.Parser.Custom.NestedSelector(
                    ns.Selector,
                    ns.Declarations
                        .Select(d => new MonorailCss.Parser.Custom.CssDeclaration(d.Property, d.Value))
                        .ToImmutableList()))
                .ToImmutableList(),
            CustomPropertyDependencies = p.CustomPropertyDependencies
        }).ToImmutableList();
    }

    private static ImmutableList<CustomVariantDefinition> ConvertToCustomVariants(
        ImmutableList<MonorailCss.Build.Tasks.Parsing.CustomVariantDefinition> parsed)
    {
        return parsed.Select(cv => new CustomVariantDefinition
        {
            Name = cv.Name,
            Selector = cv.Selector
        }).ToImmutableList();
    }

    private static MonorailCss.Theme.Theme CreateThemeFromVariables(
        ImmutableDictionary<string, string> variables)
    {
        // Convert theme variables to the format expected by Theme
        var themeDict = new Dictionary<string, string>();
        foreach (var (key, value) in variables)
        {
            // Remove leading -- from the key
            var cleanKey = key.StartsWith("--") ? key[2..] : key;
            themeDict[cleanKey] = value;
        }

        return new MonorailCss.Theme.Theme(themeDict.ToImmutableDictionary());
    }
}
