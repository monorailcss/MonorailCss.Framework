using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

internal record CssSettings
{
    public CssDeclarationList Css { get; init; } = new();

    public ImmutableList<CssRuleSet> ChildRules { get; init; } = ImmutableList<CssRuleSet>.Empty;

    public static CssSettings operator +(CssSettings settings1, CssSettings settings2)
    {
        return settings1 with
        {
            ChildRules = settings1.ChildRules.AddRange(settings2.ChildRules),
            Css = settings1.Css + settings2.Css,
        };
    }
}