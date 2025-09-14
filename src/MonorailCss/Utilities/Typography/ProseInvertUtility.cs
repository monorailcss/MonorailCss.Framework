using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles prose-invert utility.
/// Only remaps CSS variables for dark mode/inverted colors.
/// </summary>
internal class ProseInvertUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public UtilityLayer Layer => UtilityLayer.Component;

    public string[] GetNamespaces() => ["--typography-color"];

    public string[] GetFunctionalRoots() => ["prose"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functional || functional.Root != "prose")
        {
            return false;
        }

        if (functional.Value?.Value != "invert")
        {
            return false;
        }

        // Generate CSS variable remapping for invert mode
        var declarations = GenerateInvertStyles(theme).ToList();

        // Check if we have custom invert rules
        if (theme.ProseCustomization != null)
        {
            var customRules = theme.ProseCustomization.GetRules(theme);
            if (customRules.TryGetValue("invert", out var elementRules) && elementRules.Rules.Count > 0)
            {
                // Create a ComponentRule with the custom invert rules
                var childRules = elementRules.Rules
                    .Select(customRule => new ChildRule(
                        customRule.Selector,
                        customRule.Declarations
                            .Select(d => new Declaration(d.Property, d.Value, d.Important))
                            .ToImmutableList(),
                        customRule.UseWhereWrapper,
                        customRule.ExcludeClass))
                    .ToList();

                var componentRule = new ComponentRule(
                    declarations.ToImmutableList(),
                    childRules.ToImmutableList());

                results = ImmutableList.Create<AstNode>(componentRule);
                return true;
            }
        }

        if (declarations.Count == 0)
        {
            return false;
        }

        results = declarations.Cast<AstNode>().ToImmutableList();
        return true;
    }

    private IEnumerable<Declaration> GenerateInvertStyles(Theme.Theme theme)
    {
        var declarations = new List<Declaration>();

        // Generate CSS variable remapping for invert mode
        var properties = new[]
        {
            "body", "headings", "lead", "links", "bold", "counters",
            "bullets", "hr", "quotes", "quote-borders", "captions",
            "kbd", "kbd-shadows", "code", "pre-code", "pre-bg",
            "th-borders", "td-borders",
        };

        foreach (var prop in properties)
        {
            declarations.Add(new Declaration($"--tw-prose-{prop}", $"var(--tw-prose-invert-{prop})"));
        }

        return declarations;
    }
}