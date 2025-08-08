using MonorailCss.Css;

namespace MonorailCss.Plugins.Prose;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public partial class Prose
{
    private CssSettings GetSmCssSettings()
    {
        return new CssSettings
        {
            Css =
            [
                (CssProperties.FontSize, ModifierSettings.Rem(14)),
                (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 14m)),
            ],
            ChildRules =
            [
                new CssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 18)),
                ]),
                new CssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(20, 18)),
                ]),
                new CssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 14)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 30)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 30m)),
                ]),
                new CssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 14)),
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 20m)),
                ]),
                new CssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 14)),
                    (CssProperties.MarginTop, ModifierSettings.Em(28, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                ]),
                new CssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(20 / 14m)),
                ]),
                new CssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                new CssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                new CssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                new CssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(2, 14)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(5, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(2, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(5, 14)),
                ]),
                new CssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                ]),
                new CssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                ]),
                new CssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                ]),
                new CssRuleSet("pre", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(20 / 12m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 12)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 12)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(4)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 12)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 12)),
                ]),
                new CssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(22, 14)),
                ]),
                new CssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(22, 14)),
                ]),
                new CssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(4, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(4, 14)),
                ]),
                new CssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 14)),
                ]),
                new CssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 14)),
                ]),
                new CssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(8, 14)),
                ]),
                new CssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(8, 14)),
                ]),
                new CssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(4, 14)), (CssProperties.PaddingLeft, ModifierSettings.Em(22, 14)),
                ]),
                new CssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 14)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)), (CssProperties.LineHeight, ModifierSettings.Rounds(18 / 12m)),
                ]),
                new CssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 12)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 12)),
                ]),
                new CssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 12)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 12)),
                ]),
                new CssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                new CssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(16 / 12m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 12)),
                ]),
                new CssRuleSet(".prose > :first-child", [
                    (CssProperties.MarginTop, "0"),
                ], 100),
                new CssRuleSet(".prose > :last-child", [
                    (CssProperties.MarginBottom, "0"),
                ], 100),
            ],
        };
    }
}