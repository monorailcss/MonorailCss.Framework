using MonorailCss.Css;

namespace MonorailCss.Plugins.Prose;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public partial class Prose
{
    private CssSettings GetBaseCssSettings()
    {
        return new CssSettings
        {
            Css =
            [
                (CssProperties.FontSize, ModifierSettings.Rem(16)),
                (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 16m)),
            ],
            ChildRules =
            [
                new CssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                new CssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(20, 20)),
                ]),
                new CssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(36, 16)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 36)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 36m)),
                ]),
                new CssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(24, 16)),
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 24m)),
                ]),
                new CssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 16)),
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(12, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                ]),
                new CssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 16m)),
                ]),
                new CssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                new CssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                new CssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                new CssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(3, 16)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(6, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(3, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 16)),
                ]),
                new CssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                ]),
                new CssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(21, 24)),
                ]),
                new CssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                ]),
                new CssRuleSet("pre", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 14m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(6)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(12, 14)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(16, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(12, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(16, 14)),
                ]),
                new CssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(26, 16)),
                ]),
                new CssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(26, 16)),
                ]),
                new CssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(8, 16)),
                ]),
                new CssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 16)),
                ]),
                new CssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 16)),
                ]),
                new CssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 16)),
                ]),
                new CssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 16)),
                ]),
                new CssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                ]),
                new CssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 16)), (CssProperties.PaddingLeft, ModifierSettings.Em(26, 16)),
                ]),
                new CssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(48, 16)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)), (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 14m)),
                ]),
                new CssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 14)),
                ]),
                new CssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 14)),
                ]),
                new CssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                new CssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(20 / 14m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 14)),
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