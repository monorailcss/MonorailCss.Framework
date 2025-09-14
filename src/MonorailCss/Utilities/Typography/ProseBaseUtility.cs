using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles the base prose typography utility.
/// Only handles: prose (with max-width: 65ch).
/// </summary>
internal class ProseBaseUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public UtilityLayer Layer => UtilityLayer.Component;

    public string[] GetNamespaces() =>
    [
        "--typography-base", "--typography-color",
    ];

    public string[] GetFunctionalRoots() => ["prose"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility { Root: "prose", Value: null })
        {
            return false;
        }

        // Generate full prose styles with max-width using ComponentRule
        var componentRule = GenerateProseComponent(theme);
        results = ImmutableList.Create<AstNode>(componentRule);
        return true;
    }

    private ComponentRule GenerateProseComponent(Theme.Theme theme)
    {
        // Create declarations for the base .prose class
        var baseDeclarations = new List<Declaration>();

        // Add max-width for prose content - ONLY for base prose class
        baseDeclarations.Add(new Declaration("max-width", "65ch"));

        // Root font size and line height for base size
        var fontSize = theme.ResolveValue("--typography-base-font-size", ["--typography"]);
        var lineHeight = theme.ResolveValue("--typography-base-line-height", ["--typography"]);

        if (fontSize != null)
        {
            baseDeclarations.Add(new Declaration("font-size", fontSize));
        }

        if (lineHeight != null)
        {
            baseDeclarations.Add(new Declaration("line-height", lineHeight));
        }

        // Set default gray color CSS variables
        baseDeclarations.AddRange(MapColorVariables("gray", theme));

        // Base color - use CSS variables
        baseDeclarations.Add(new Declaration("color", "var(--tw-prose-body)"));

        // Generate child rules for all element styles
        var childRules = GenerateChildRules("base", theme);

        // Apply customization if provided
        if (theme.ProseCustomization != null)
        {
            childRules = ApplyCustomization(childRules, "DEFAULT", theme);
            childRules = ApplyCustomization(childRules, "base", theme);
        }

        return new ComponentRule(
            baseDeclarations.ToImmutableList(),
            childRules.ToImmutableList());
    }

    private IEnumerable<Declaration> MapColorVariables(string colorTheme, Theme.Theme theme)
    {
        var declarations = new List<Declaration>();

        // Map the color theme to CSS variables
        var properties = new[]
        {
            "body", "headings", "lead", "links", "bold", "counters", "bullets", "hr", "quotes", "quote-borders", "captions", "kbd", "kbd-shadows", "code", "pre-code", "pre-bg", "th-borders", "td-borders",
        };

        foreach (var prop in properties)
        {
            var value = theme.ResolveValue(
                colorTheme == "gray" ? $"--typography-color-{prop}" : $"--typography-color-{colorTheme}-{prop}",
                ["--typography-color"]);
            if (value != null)
            {
                var resolvedValue = ResolveColorValue(value, theme);
                declarations.Add(new Declaration($"--tw-prose-{prop}", resolvedValue));
            }

            // Also add the invert variables
            var invertValue = theme.ResolveValue(
                colorTheme == "gray" ? $"--typography-color-invert-{prop}" : $"--typography-color-{colorTheme}-invert-{prop}",
                ["--typography-color"]);
            if (invertValue != null)
            {
                var resolvedInvertValue = ResolveColorValue(invertValue, theme);
                declarations.Add(new Declaration($"--tw-prose-invert-{prop}", resolvedInvertValue));
            }
        }

        return declarations;
    }

    private string ResolveColorValue(string value, Theme.Theme theme)
    {
        // If the value contains var() references, recursively resolve them
        if (value.StartsWith("var(--") && value.EndsWith(")"))
        {
            // Extract the variable name
            var varName = value.Substring(4, value.Length - 5); // Remove "var(" and ")"

            // Try to resolve this variable from the theme
            var resolvedValue = theme.ResolveValue(varName, []);
            if (resolvedValue != null)
            {
                // Recursively resolve if this also contains var()
                return ResolveColorValue(resolvedValue, theme);
            }
        }

        return value;
    }

    private List<ChildRule> GenerateChildRules(string size, Theme.Theme theme)
    {
        var rules = new List<ChildRule>();

        // Paragraphs
        rules.Add(CreateChildRule("p", size, theme, [
            ("margin-top", $"--typography-{size}-p-margin-top"),
            ("margin-bottom", $"--typography-{size}-p-margin-bottom"),
        ]));

        // Lead paragraphs
        rules.Add(CreateChildRule("[class~=\"lead\"]", size, theme, [
            ("font-size", $"--typography-{size}-lead-font-size"),
            ("line-height", $"--typography-{size}-lead-line-height"),
            ("margin-top", $"--typography-{size}-lead-margin-top"),
            ("margin-bottom", $"--typography-{size}-lead-margin-bottom"),
            ("color", "var(--tw-prose-lead)"),
        ]));

        // Headings
        for (var i = 1; i <= 6; i++)
        {
            var h = $"h{i}";
            if (i <= 3)
            {
                // h1, h2, h3 have specific styles
                rules.Add(CreateChildRule(h, size, theme, [
                    ("font-size", $"--typography-{size}-{h}-font-size"),
                    ("margin-top", $"--typography-{size}-{h}-margin-top"),
                    ("margin-bottom", $"--typography-{size}-{h}-margin-bottom"),
                    ("line-height", $"--typography-{size}-{h}-line-height"),
                    ("font-weight", "800"),
                    ("color", "var(--tw-prose-headings)"),
                ]));
            }
            else
            {
                // h4, h5, h6
                rules.Add(CreateChildRule(h, size, theme, [
                    ("margin-top", $"--typography-{size}-h4-margin-top"),
                    ("margin-bottom", $"--typography-{size}-h4-margin-bottom"),
                    ("line-height", $"--typography-{size}-h4-line-height"),
                    ("font-weight", "700"),
                    ("color", "var(--tw-prose-headings)"),
                ]));
            }
        }

        // Blockquotes
        rules.Add(CreateChildRule("blockquote", size, theme, [
            ("margin-top", $"--typography-{size}-blockquote-margin-top"),
            ("margin-bottom", $"--typography-{size}-blockquote-margin-bottom"),
            ("padding-inline-start", $"--typography-{size}-blockquote-padding-inline-start"),
            ("border-inline-start-width", "0.25rem"),
            ("border-inline-start-color", "var(--tw-prose-quote-borders)"),
            ("quotes", "\"\\201C\"\"\\201D\"\"\\2018\"\"\\2019\""),
            ("font-style", "italic"),
            ("font-weight", "500"),
            ("color", "var(--tw-prose-quotes)"),
        ]));

        // Links
        rules.Add(CreateChildRule("a", size, theme, [
            ("color", "var(--tw-prose-links)"),
            ("text-decoration", "underline"),
            ("font-weight", "500"),
        ]));

        // Strong
        rules.Add(CreateChildRule("strong", size, theme, [
            ("color", "var(--tw-prose-bold)"),
            ("font-weight", "600"),
        ]));

        // Code
        rules.Add(CreateChildRule("code", size, theme, [
            ("font-size", $"--typography-{size}-code-font-size"),
            ("color", "var(--tw-prose-code)"),
            ("font-weight", "600"),
        ]));

        rules.Add(CreateChildRule("code::before", size, theme, [
            ("content", "\"`\""),
        ]));

        rules.Add(CreateChildRule("code::after", size, theme, [
            ("content", "\"`\""),
        ]));

        // Pre
        rules.Add(CreateChildRule("pre", size, theme, [
            ("font-size", $"--typography-{size}-pre-font-size"),
            ("line-height", $"--typography-{size}-pre-line-height"),
            ("margin-top", $"--typography-{size}-pre-margin-top"),
            ("margin-bottom", $"--typography-{size}-pre-margin-bottom"),
            ("border-radius", $"--typography-{size}-pre-border-radius"),
            ("padding-top", $"--typography-{size}-pre-padding-top"),
            ("padding-inline-end", $"--typography-{size}-pre-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-pre-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-pre-padding-inline-start"),
            ("background-color", "var(--tw-prose-pre-bg)"),
            ("overflow-x", "auto"),
            ("color", "var(--tw-prose-pre-code)"),
        ]));

        // Pre code
        rules.Add(CreateChildRule("pre code", size, theme, [
            ("background-color", "transparent"),
            ("border-width", "0"),
            ("border-radius", "0"),
            ("padding", "0"),
            ("font-weight", "inherit"),
            ("color", "inherit"),
            ("font-size", "inherit"),
            ("font-family", "inherit"),
            ("line-height", "inherit"),
        ]));

        rules.Add(CreateChildRule("pre code::before", size, theme, [
            ("content", "none"),
        ]));

        rules.Add(CreateChildRule("pre code::after", size, theme, [
            ("content", "none"),
        ]));

        // Keyboard
        rules.Add(CreateChildRule("kbd", size, theme, [
            ("font-size", $"--typography-{size}-kbd-font-size"),
            ("border-radius", $"--typography-{size}-kbd-border-radius"),
            ("padding-top", $"--typography-{size}-kbd-padding-top"),
            ("padding-inline-end", $"--typography-{size}-kbd-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-kbd-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-kbd-padding-inline-start"),
            ("font-family", "inherit"),
            ("color", "var(--tw-prose-kbd)"),
            ("font-weight", "500"),
            ("box-shadow", "0 0 0 1px rgb(var(--tw-prose-kbd-shadows) / 10%), 0 3px 0 rgb(var(--tw-prose-kbd-shadows) / 10%)"),
        ]));

        // Tables
        rules.Add(CreateChildRule("table", size, theme, [
            ("width", "100%"),
            ("table-layout", "auto"),
            ("margin-top", Em(32, GetBaseFontSize(size))),
            ("margin-bottom", Em(32, GetBaseFontSize(size))),
            ("font-size", $"--typography-{size}-table-font-size"),
            ("line-height", $"--typography-{size}-table-line-height"),
        ]));

        rules.Add(CreateChildRule("thead", size, theme, [
            ("border-bottom-width", "1px"),
            ("border-bottom-color", "var(--tw-prose-th-borders)"),
        ]));

        rules.Add(CreateChildRule("thead th", size, theme, [
            ("color", "var(--tw-prose-headings)"),
            ("font-weight", "600"),
            ("vertical-align", "bottom"),
            ("padding-inline-end", $"--typography-{size}-thead-th-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-thead-th-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-thead-th-padding-inline-start"),
        ]));

        rules.Add(CreateChildRule("tbody tr", size, theme, [
            ("border-bottom-width", "1px"),
            ("border-bottom-color", "var(--tw-prose-td-borders)"),
        ]));

        rules.Add(CreateChildRule("tbody tr:last-child", size, theme, [
            ("border-bottom-width", "0"),
        ]));

        rules.Add(CreateChildRule("tbody td", size, theme, [
            ("vertical-align", "baseline"),
            ("padding-top", $"--typography-{size}-tbody-td-padding-top"),
            ("padding-inline-end", $"--typography-{size}-tbody-td-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-tbody-td-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-tbody-td-padding-inline-start"),
        ]));

        rules.Add(CreateChildRule("tfoot", size, theme, [
            ("border-top-width", "1px"),
            ("border-top-color", "var(--tw-prose-th-borders)"),
        ]));

        rules.Add(CreateChildRule("tfoot td", size, theme, [
            ("vertical-align", "top"),
        ]));

        // Lists
        rules.Add(CreateChildRule("ul", size, theme, [
            ("margin-top", $"--typography-{size}-ul-margin-top"),
            ("margin-bottom", $"--typography-{size}-ul-margin-bottom"),
            ("padding-inline-start", $"--typography-{size}-ul-padding-inline-start"),
            ("list-style-type", "disc"),
        ]));

        rules.Add(CreateChildRule("ol", size, theme, [
            ("margin-top", $"--typography-{size}-ol-margin-top"),
            ("margin-bottom", $"--typography-{size}-ol-margin-bottom"),
            ("padding-inline-start", $"--typography-{size}-ol-padding-inline-start"),
            ("list-style-type", "decimal"),
        ]));

        rules.Add(CreateChildRule("li", size, theme, [
            ("margin-top", $"--typography-{size}-li-margin-top"),
            ("margin-bottom", $"--typography-{size}-li-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("ol > li", size, theme, [
            ("padding-inline-start", $"--typography-{size}-ol-li-padding-inline-start"),
        ]));

        rules.Add(CreateChildRule("ul > li", size, theme, [
            ("padding-inline-start", $"--typography-{size}-ul-li-padding-inline-start"),
        ]));

        rules.Add(CreateChildRule("ul > li::marker", size, theme, [
            ("color", "var(--tw-prose-bullets)"),
        ]));

        rules.Add(CreateChildRule("ol > li::marker", size, theme, [
            ("font-weight", "400"),
            ("color", "var(--tw-prose-counters)"),
        ]));

        // Nested lists
        rules.Add(CreateChildRule("ul ul, ul ol, ol ul, ol ol", size, theme, [
            ("margin-top", $"--typography-{size}-ul-ul-margin-top"),
            ("margin-bottom", $"--typography-{size}-ul-ul-margin-bottom"),
        ]));

        // Definition lists
        rules.Add(CreateChildRule("dl", size, theme, [
            ("margin-top", $"--typography-{size}-dl-margin-top"),
            ("margin-bottom", $"--typography-{size}-dl-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("dt", size, theme, [
            ("margin-top", $"--typography-{size}-dt-margin-top"),
            ("color", "var(--tw-prose-headings)"),
            ("font-weight", "600"),
        ]));

        rules.Add(CreateChildRule("dd", size, theme, [
            ("margin-top", $"--typography-{size}-dd-margin-top"),
            ("padding-inline-start", $"--typography-{size}-dd-padding-inline-start"),
        ]));

        // Horizontal rule
        rules.Add(CreateChildRule("hr", size, theme, [
            ("margin-top", $"--typography-{size}-hr-margin-top"),
            ("margin-bottom", $"--typography-{size}-hr-margin-bottom"),
            ("border-color", "var(--tw-prose-hr)"),
            ("border-top-width", "1px"),
        ]));

        // Images and media
        rules.Add(CreateChildRule("img", size, theme, [
            ("margin-top", $"--typography-{size}-img-margin-top"),
            ("margin-bottom", $"--typography-{size}-img-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("video", size, theme, [
            ("margin-top", $"--typography-{size}-video-margin-top"),
            ("margin-bottom", $"--typography-{size}-video-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("figure", size, theme, [
            ("margin-top", $"--typography-{size}-figure-margin-top"),
            ("margin-bottom", $"--typography-{size}-figure-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("figure > *", size, theme, [
            ("margin-top", "0"),
            ("margin-bottom", "0"),
        ]));

        rules.Add(CreateChildRule("figcaption", size, theme, [
            ("font-size", $"--typography-{size}-figcaption-font-size"),
            ("line-height", $"--typography-{size}-figcaption-line-height"),
            ("margin-top", $"--typography-{size}-figcaption-margin-top"),
            ("color", "var(--tw-prose-captions)"),
        ]));

        // Code inside headings
        rules.Add(CreateChildRule("h2 code", size, theme, [
            ("font-size", $"--typography-{size}-h2-code-font-size"),
        ]));

        rules.Add(CreateChildRule("h3 code", size, theme, [
            ("font-size", $"--typography-{size}-h3-code-font-size"),
        ]));

        // Prose child selectors
        rules.Add(CreateChildRule("> ul > li p", size, theme, [
            ("margin-top", $"--typography-{size}-li-margin-top"),
            ("margin-bottom", $"--typography-{size}-li-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("> ul > li > p:first-child", size, theme, [
            ("margin-top", $"--typography-{size}-li-margin-top"),
        ]));

        rules.Add(CreateChildRule("> ul > li > p:last-child", size, theme, [
            ("margin-bottom", $"--typography-{size}-li-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("> ol > li > p:first-child", size, theme, [
            ("margin-top", $"--typography-{size}-li-margin-top"),
        ]));

        rules.Add(CreateChildRule("> ol > li > p:last-child", size, theme, [
            ("margin-bottom", $"--typography-{size}-li-margin-bottom"),
        ]));

        rules.Add(CreateChildRule("> :first-child", size, theme, [
            ("margin-top", "0"),
        ]));

        rules.Add(CreateChildRule("> :last-child", size, theme, [
            ("margin-bottom", "0"),
        ]));

        return rules;
    }

    private ChildRule CreateChildRule(string element, string size, Theme.Theme theme, (string Property, string ThemeKeyOrValue)[] styles)
    {
        var declarations = new List<Declaration>();

        foreach (var (property, themeKeyOrValue) in styles)
        {
            // Check if it's a theme key or a direct value
            string? value;
            if (themeKeyOrValue.StartsWith("--typography-"))
            {
                value = theme.ResolveValue(themeKeyOrValue, ["--typography"]);
            }
            else if (themeKeyOrValue.StartsWith("var(") ||
                     char.IsDigit(themeKeyOrValue[0]) ||
                     themeKeyOrValue == "none" ||
                     themeKeyOrValue == "transparent" ||
                     themeKeyOrValue == "inherit" ||
                     themeKeyOrValue == "auto" ||
                     themeKeyOrValue == "baseline" ||
                     themeKeyOrValue == "top" ||
                     themeKeyOrValue == "italic" ||
                     themeKeyOrValue == "underline" ||
                     themeKeyOrValue == "disc" ||
                     themeKeyOrValue == "decimal" ||
                     themeKeyOrValue.Contains('"'))
            {
                // Direct value
                value = themeKeyOrValue;
            }
            else
            {
                // Try to compute Em or use as-is
                value = themeKeyOrValue;
            }

            if (value != null)
            {
                declarations.Add(new Declaration(property, value));
            }
        }

        // Return a ChildRule with proper configuration
        // The selector building is now handled by PostProcessor
        return new ChildRule(element, declarations.ToImmutableList());
    }

    private static double GetBaseFontSize(string size)
    {
        return size switch
        {
            "sm" => 14,
            "lg" => 18,
            "xl" => 20,
            "2xl" => 24,
            _ => 16, // base
        };
    }

    private static string Em(double px, double baseSize)
    {
        var value = Math.Round(px / baseSize, 7);
        return $"{value}em".Replace(".0em", "em");
    }

    private List<ChildRule> ApplyCustomization(List<ChildRule> defaultRules, string modifier, Theme.Theme theme)
    {
        if (theme.ProseCustomization == null)
        {
            return defaultRules;
        }

        var customRules = theme.ProseCustomization.GetRules(theme);

        if (!customRules.TryGetValue(modifier, out var elementRules))
        {
            return defaultRules;
        }

        // Create a dictionary of existing rules by selector for easier lookup
        var rulesBySelector = defaultRules.ToDictionary(r => r.ChildSelector);

        // Apply customizations
        foreach (var customRule in elementRules.Rules)
        {
            var declarations = customRule.Declarations
                .Select(d => new Declaration(d.Property, d.Value, d.Important))
                .ToImmutableList();

            if (rulesBySelector.TryGetValue(customRule.Selector, out var existingRule))
            {
                // Override existing rule - merge declarations
                var mergedDeclarations = MergeDeclarations(existingRule.Declarations, declarations);
                rulesBySelector[customRule.Selector] = new ChildRule(
                    customRule.Selector,
                    mergedDeclarations,
                    customRule.UseWhereWrapper,
                    customRule.ExcludeClass);
            }
            else
            {
                // Add new rule
                var newRule = new ChildRule(
                    customRule.Selector,
                    declarations,
                    customRule.UseWhereWrapper,
                    customRule.ExcludeClass);
                rulesBySelector[customRule.Selector] = newRule;
            }
        }

        return rulesBySelector.Values.ToList();
    }

    private ImmutableList<Declaration> MergeDeclarations(
        ImmutableList<Declaration> existing,
        ImmutableList<Declaration> custom)
    {
        var merged = new Dictionary<string, Declaration>();

        // Add existing declarations
        foreach (var decl in existing)
        {
            merged[decl.Property] = decl;
        }

        // Override with custom declarations
        foreach (var decl in custom)
        {
            merged[decl.Property] = decl;
        }

        return merged.Values.ToImmutableList();
    }
}