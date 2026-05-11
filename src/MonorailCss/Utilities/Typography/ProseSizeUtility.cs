using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the size of prose content.
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

        var componentRule = GenerateProseSizeComponent(size, theme);
        results = ImmutableList.Create<AstNode>(componentRule);
        return true;
    }

    private ComponentRule GenerateProseSizeComponent(string size, Theme.Theme theme)
    {
        var baseDeclarations = new List<Declaration>();

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

        var childRules = ProseChildRulesBuilder.Build(size, theme, isRoot: false);

        if (theme.ProseCustomization != null)
        {
            childRules = ApplyCustomization(childRules, size, theme);
        }

        return new ComponentRule(
            baseDeclarations.ToImmutableList(),
            childRules.ToImmutableList());
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
    /// Returns examples of prose size utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("prose-sm", "Apply small prose typography scaling"),
            new Documentation.UtilityExample("prose-base", "Apply base prose typography scaling"),
            new Documentation.UtilityExample("prose-lg", "Apply large prose typography scaling"),
            new Documentation.UtilityExample("prose-xl", "Apply extra large prose typography scaling"),
            new Documentation.UtilityExample("prose-2xl", "Apply 2xl prose typography scaling"),
        ];
    }
}
