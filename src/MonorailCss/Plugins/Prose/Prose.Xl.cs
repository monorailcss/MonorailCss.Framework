using MonorailCss.Css;

namespace MonorailCss.Plugins.Prose;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public partial class Prose
{
    private CssSettings GetXlCssSettings()
    {
        return new CssSettings
        {
            Css =
            [
                (CssProperties.FontSize, ModifierSettings.Rem(20)),
                (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 20m)),
            ],
            ChildRules =
            [
                new CssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(24, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 24m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 24)),
                ]),
                new CssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(48, 30)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 30)),
                ]),
                new CssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(56, 20)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(48, 56)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(56 / 56m)),
                ]),
                new CssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(36, 20)),
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 36)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 36)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 36m)),
                ]),
                new CssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 20)),
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 30)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 30m)),
                ]),
                new CssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(36, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(12, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                ]),
                new CssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                new CssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                new CssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                new CssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(5, 20)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 20)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(5, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 20)),
                ]),
                new CssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                ]),
                new CssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(31, 36)),
                ]),
                new CssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(27, 30)),
                ]),
                new CssRuleSet("pre", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 18m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(36, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(36, 18)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(8)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(20, 18)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(20, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                new CssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                new CssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 20)),
                ]),
                new CssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 20)),
                ]),
                new CssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 20)),
                ]),
                new CssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 20)),
                ]),
                new CssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 20)),
                ]),
                new CssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 20)), (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                new CssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(56, 20)),
                ]),
                new CssRuleSet("hr + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                new CssRuleSet("h2 + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                new CssRuleSet("h3 + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                new CssRuleSet("h4 + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                new CssRuleSet("table", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)), (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                ]),
                new CssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 18)),
                ]),
                new CssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(16, 18)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 18)),
                ]),
                new CssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                new CssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(18, 18)),
                ]),
                new CssRuleSet(".prose > :first-child", [
                    (CssProperties.MarginTop, "0"),
                ]),
                new CssRuleSet(".prose > :last-child", [
                    (CssProperties.MarginBottom, "0"),
                ]),
            ],
        };
    }
}