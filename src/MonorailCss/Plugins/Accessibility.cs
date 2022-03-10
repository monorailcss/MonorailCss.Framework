using MonorailCss.Css;
using CSS = MonorailCss.Css.CssProperties;

namespace MonorailCss.Plugins;

/// <summary>
/// Accessibility plugin.
/// </summary>
public class Accessibility : IUtilityPlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilityPlugin)
        {
            yield break;
        }

        var declarations = utilityPlugin.Name.ToLowerInvariant() switch
        {
            ".sr-only" => SrOnly,
            ".not-sr-only" => NotSrOnly,
            _ => default,
        };

        if (declarations == default)
        {
            yield break;
        }

        yield return new CssRuleSet(utilityPlugin.OriginalSyntax, declarations);
    }

    private static CssDeclarationList NotSrOnly => new CssDeclarationList
        {
            new(CSS.Position, "static"),
            new(CSS.Width, "auto"),
            new(CSS.Height, "auto"),
            new(CSS.Padding, "0"),
            new(CSS.Margin, "0"),
            new(CSS.Overflow, "visible"),
            new(CSS.Clip, "auto"),
            new(CSS.WhiteSpace, "normal"),
        };

    private static CssDeclarationList SrOnly => new CssDeclarationList
        {
            new(CSS.Position, "absolute"),
            new(CSS.Width, "1px"),
            new(CSS.Height, "1px"),
            new(CSS.Padding, "0"),
            new(CSS.Margin, "-1px"),
            new(CSS.Overflow, "hidden"),
            new(CSS.Clip, "rect(0, 0, 0, 0)"),
            new(CSS.WhiteSpace, "nowrap"),
            new(CSS.BorderWidth, "0"),
        };
}