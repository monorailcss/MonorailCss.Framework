using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for applying beautiful typographic defaults to HTML.
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

        var componentRule = GenerateProseComponent(theme);
        results = ImmutableList.Create<AstNode>(componentRule);
        return true;
    }

    private ComponentRule GenerateProseComponent(Theme.Theme theme)
    {
        var baseDeclarations = new List<Declaration>
        {
            new("max-width", "65ch"),
        };

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

        baseDeclarations.AddRange(MapColorVariables("gray", theme));
        baseDeclarations.Add(new Declaration("color", "var(--tw-prose-body)"));

        var childRules = ProseChildRulesBuilder.Build("base", theme, isRoot: true);

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
        if (value.StartsWith("var(--") && value.EndsWith(")"))
        {
            var varName = value.Substring(4, value.Length - 5);
            var resolvedValue = theme.ResolveValue(varName, []);
            if (resolvedValue != null)
            {
                return ResolveColorValue(resolvedValue, theme);
            }
        }

        return value;
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

        var rulesBySelector = defaultRules.ToDictionary(r => r.ChildSelector);

        foreach (var customRule in elementRules.Rules)
        {
            var declarations = customRule.Declarations
                .Select(d => new Declaration(d.Property, d.Value, d.Important))
                .ToImmutableList();

            if (rulesBySelector.TryGetValue(customRule.Selector, out var existingRule))
            {
                var mergedDeclarations = MergeDeclarations(existingRule.Declarations, declarations);
                rulesBySelector[customRule.Selector] = new ChildRule(
                    customRule.Selector,
                    mergedDeclarations,
                    customRule.UseWhereWrapper,
                    customRule.ExcludeClass);
            }
            else
            {
                rulesBySelector[customRule.Selector] = new ChildRule(
                    customRule.Selector,
                    declarations,
                    customRule.UseWhereWrapper,
                    customRule.ExcludeClass);
            }
        }

        return rulesBySelector.Values.ToList();
    }

    private ImmutableList<Declaration> MergeDeclarations(
        ImmutableList<Declaration> existing,
        ImmutableList<Declaration> custom)
    {
        var merged = new Dictionary<string, Declaration>();

        foreach (var decl in existing)
        {
            merged[decl.Property] = decl;
        }

        foreach (var decl in custom)
        {
            merged[decl.Property] = decl;
        }

        return merged.Values.ToImmutableList();
    }

    /// <summary>
    /// Returns examples of prose base utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("prose", "Apply prose typography styles with 65ch max-width"),
        ];
    }
}
