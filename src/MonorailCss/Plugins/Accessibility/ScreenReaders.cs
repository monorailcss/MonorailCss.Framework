﻿using MonorailCss.Css;
using MonorailCss.Parser;
using CSS = MonorailCss.Css.CssProperties;

namespace MonorailCss.Plugins.Accessibility;

/// <summary>
/// Accessibility plugin.
/// </summary>
public class ScreenReaders : IUtilityPlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilityPlugin)
        {
            yield break;
        }

        var declarations = utilityPlugin.Name switch
        {
            "sr-only" => SrOnly,
            "not-sr-only" => NotSrOnly,
            _ => null,
        };

        if (declarations == null)
        {
            yield break;
        }

        yield return new CssRuleSet(utilityPlugin.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        yield return new CssRuleSet("sr-only", SrOnly);
        yield return new CssRuleSet("not-sr-only", NotSrOnly);
    }

    private static CssDeclarationList NotSrOnly =>
    [
        new CssDeclaration(CSS.Position, "static"),
        new CssDeclaration(CSS.Width, "auto"),
        new CssDeclaration(CSS.Height, "auto"),
        new CssDeclaration(CSS.Padding, "0"),
        new CssDeclaration(CSS.Margin, "0"),
        new CssDeclaration(CSS.Overflow, "visible"),
        new CssDeclaration(CSS.Clip, "auto"),
        new CssDeclaration(CSS.WhiteSpace, "normal"),
    ];

    private static CssDeclarationList SrOnly =>
    [
        new CssDeclaration(CSS.Position, "absolute"),
        new CssDeclaration(CSS.Width, "1px"),
        new CssDeclaration(CSS.Height, "1px"),
        new CssDeclaration(CSS.Padding, "0"),
        new CssDeclaration(CSS.Margin, "-1px"),
        new CssDeclaration(CSS.Overflow, "hidden"),
        new CssDeclaration(CSS.Clip, "rect(0, 0, 0, 0)"),
        new CssDeclaration(CSS.WhiteSpace, "nowrap"),
        new CssDeclaration(CSS.BorderWidth, "0"),
    ];
}