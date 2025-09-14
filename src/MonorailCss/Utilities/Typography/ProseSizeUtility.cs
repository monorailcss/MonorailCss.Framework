using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles prose size variant utilities.
/// Supports: prose-sm, prose-base, prose-lg, prose-xl, prose-2xl
/// Does NOT set max-width, only typography scaling.
/// </summary>
internal class ProseSizeUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public UtilityLayer Layer => UtilityLayer.Component;

    private static readonly HashSet<string> _sizeModifiers = ["base", "sm", "lg", "xl", "2xl"];

    public string[] GetNamespaces() =>
    [
        "--typography-base", "--typography-sm", "--typography-lg",
        "--typography-xl", "--typography-2xl",
    ];

    public string[] GetFunctionalRoots() => ["prose"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functional || functional.Root != "prose" || functional.Value == null)
        {
            return false;
        }

        var size = functional.Value.Value;
        if (!_sizeModifiers.Contains(size))
        {
            return false;
        }

        // Generate prose size styles WITHOUT max-width using ComponentRule
        var componentRule = GenerateProseSizeComponent(size, theme);
        results = ImmutableList.Create<AstNode>(componentRule);
        return true;
    }

    private ComponentRule GenerateProseSizeComponent(string size, Theme.Theme theme)
    {
        // Create declarations for the prose size variant
        var baseDeclarations = new List<Declaration>();

        // NO max-width for size variants!
        // Only font size and line height
        var fontSize = theme.ResolveValue($"--typography-{size}-font-size", ["--typography"]);
        var lineHeight = theme.ResolveValue($"--typography-{size}-line-height", ["--typography"]);

        if (fontSize != null)
        {
            baseDeclarations.Add(new Declaration("font-size", fontSize));
        }

        if (lineHeight != null)
        {
            baseDeclarations.Add(new Declaration("line-height", lineHeight));
        }

        // Generate child rules for all element styles with appropriate size scaling
        var childRules = GenerateChildRules(size, theme);

        // Apply customization if provided
        if (theme.ProseCustomization != null)
        {
            childRules = ApplyCustomization(childRules, size, theme);
        }

        return new ComponentRule(
            baseDeclarations.ToImmutableList(),
            childRules.ToImmutableList());
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
                ]));
            }
            else
            {
                // h4, h5, h6
                rules.Add(CreateChildRule(h, size, theme, [
                    ("margin-top", $"--typography-{size}-h4-margin-top"),
                    ("margin-bottom", $"--typography-{size}-h4-margin-bottom"),
                    ("line-height", $"--typography-{size}-h4-line-height"),
                ]));
            }
        }

        // Blockquotes
        rules.Add(CreateChildRule("blockquote", size, theme, [
            ("margin-top", $"--typography-{size}-blockquote-margin-top"),
            ("margin-bottom", $"--typography-{size}-blockquote-margin-bottom"),
            ("padding-inline-start", $"--typography-{size}-blockquote-padding-inline-start"),
        ]));

        // Code
        rules.Add(CreateChildRule("code", size, theme, [
            ("font-size", $"--typography-{size}-code-font-size"),
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
        ]));

        // Keyboard
        rules.Add(CreateChildRule("kbd", size, theme, [
            ("font-size", $"--typography-{size}-kbd-font-size"),
            ("border-radius", $"--typography-{size}-kbd-border-radius"),
            ("padding-top", $"--typography-{size}-kbd-padding-top"),
            ("padding-inline-end", $"--typography-{size}-kbd-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-kbd-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-kbd-padding-inline-start"),
        ]));

        // Tables
        rules.Add(CreateChildRule("table", size, theme, [
            ("margin-top", Em(32, GetBaseFontSize(size))),
            ("margin-bottom", Em(32, GetBaseFontSize(size))),
            ("font-size", $"--typography-{size}-table-font-size"),
            ("line-height", $"--typography-{size}-table-line-height"),
        ]));

        rules.Add(CreateChildRule("thead th", size, theme, [
            ("padding-inline-end", $"--typography-{size}-thead-th-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-thead-th-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-thead-th-padding-inline-start"),
        ]));

        rules.Add(CreateChildRule("tbody td", size, theme, [
            ("padding-top", $"--typography-{size}-tbody-td-padding-top"),
            ("padding-inline-end", $"--typography-{size}-tbody-td-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-tbody-td-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-tbody-td-padding-inline-start"),
        ]));

        // Lists
        rules.Add(CreateChildRule("ul", size, theme, [
            ("margin-top", $"--typography-{size}-ul-margin-top"),
            ("margin-bottom", $"--typography-{size}-ul-margin-bottom"),
            ("padding-inline-start", $"--typography-{size}-ul-padding-inline-start"),
        ]));

        rules.Add(CreateChildRule("ol", size, theme, [
            ("margin-top", $"--typography-{size}-ol-margin-top"),
            ("margin-bottom", $"--typography-{size}-ol-margin-bottom"),
            ("padding-inline-start", $"--typography-{size}-ol-padding-inline-start"),
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
        ]));

        rules.Add(CreateChildRule("dd", size, theme, [
            ("margin-top", $"--typography-{size}-dd-margin-top"),
            ("padding-inline-start", $"--typography-{size}-dd-padding-inline-start"),
        ]));

        // Horizontal rule
        rules.Add(CreateChildRule("hr", size, theme, [
            ("margin-top", $"--typography-{size}-hr-margin-top"),
            ("margin-bottom", $"--typography-{size}-hr-margin-bottom"),
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

        rules.Add(CreateChildRule("figcaption", size, theme, [
            ("font-size", $"--typography-{size}-figcaption-font-size"),
            ("line-height", $"--typography-{size}-figcaption-line-height"),
            ("margin-top", $"--typography-{size}-figcaption-margin-top"),
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