using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-shadow plugin.
/// </summary>
public class TextShadow : BaseLookupPlugin
{
    /// <inheritdoc />
    protected override ImmutableDictionary<string, CssDeclarationList> GetLookups()
    {
        return new Dictionary<string, CssDeclarationList>
        {
            { "text-shadow-sm", [(CssProperties.TextShadow, "0 1px 2px rgb(0 0 0 / 0.05)")] },
            { "text-shadow", [(CssProperties.TextShadow, "0 1px 3px rgb(0 0 0 / 0.1), 0 1px 2px rgb(0 0 0 / 0.1)")] },
            { "text-shadow-md", [(CssProperties.TextShadow, "0 4px 6px rgb(0 0 0 / 0.1), 0 2px 4px rgb(0 0 0 / 0.1)")] },
            { "text-shadow-lg", [(CssProperties.TextShadow, "0 10px 15px rgb(0 0 0 / 0.1), 0 4px 6px rgb(0 0 0 / 0.1)")] },
            { "text-shadow-xl", [(CssProperties.TextShadow, "0 20px 25px rgb(0 0 0 / 0.1), 0 8px 10px rgb(0 0 0 / 0.1)")] },
            { "text-shadow-2xl", [(CssProperties.TextShadow, "0 25px 50px rgb(0 0 0 / 0.25)")] },
            { "text-shadow-none", [(CssProperties.TextShadow, "none")] },
        }.ToImmutableDictionary();
    }
}