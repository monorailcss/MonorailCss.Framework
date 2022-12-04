using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Transitions;

/// <summary>
/// The animation plugin.
/// </summary>
public class Animation : IUtilityPlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilityPlugin)
        {
            yield break;
        }

        // this could be simpler, but we need to figure out how to do animate-spin which involves a
        // keyframe animation which we don't support yet.
        if (utilityPlugin.Name == "animate-none")
        {
            yield return new CssRuleSet(utilityPlugin.OriginalSyntax, AnimateNone);
        }
        else if (utilityPlugin.Name == "animate-ping")
        {
            yield return new CssRuleSet(utilityPlugin.OriginalSyntax, AnimatePing);
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        yield return new CssRuleSet("animate-none", AnimateNone);
        yield return new CssRuleSet("animate-ping", AnimatePing);
    }

    private static CssDeclarationList AnimateNone => new()
    {
        new("animation", "none"),
    };

    private static CssDeclarationList AnimatePing => new()
    {
        new("animation", "ping 1s cubic-bezier(0, 0, 0.2, 1) infinite"),
    };
}