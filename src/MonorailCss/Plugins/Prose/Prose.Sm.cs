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
                GetProseCssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 18)),
                ]),
                GetProseCssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(20, 18)),
                ]),
                GetProseCssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 14)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 30)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 30m)),
                ]),
                GetProseCssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 14)),
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 20m)),
                ]),
                GetProseCssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 14)),
                    (CssProperties.MarginTop, ModifierSettings.Em(28, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                ]),
                GetProseCssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(20 / 14m)),
                ]),
                GetProseCssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                GetProseCssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                GetProseCssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                GetProseCssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(2, 14)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(5, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(2, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(5, 14)),
                ]),
                GetProseCssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                ]),
                GetProseCssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                ]),
                GetProseCssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                ]),
                GetProseCssRuleSet("pre", [
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
                GetProseCssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(22, 14)),
                ]),
                GetProseCssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(22, 14)),
                ]),
                GetProseCssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(4, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(4, 14)),
                ]),
                GetProseCssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 14)),
                ]),
                GetProseCssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 14)),
                ]),
                GetProseCssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(8, 14)),
                ]),
                GetProseCssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(8, 14)),
                ]),
                GetProseCssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 14)),
                ]),
                GetProseCssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(4, 14)), (CssProperties.PaddingLeft, ModifierSettings.Em(22, 14)),
                ]),
                GetProseCssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 14)),
                ]),
                GetProseCssRuleSet("hr + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                GetProseCssRuleSet("h2 + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                GetProseCssRuleSet("h3 + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                GetProseCssRuleSet("h4 + *", [
                    (CssProperties.MarginTop, "0"),
                ]),
                GetProseCssRuleSet("table", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)), (CssProperties.LineHeight, ModifierSettings.Rounds(18 / 12m)),
                ]),
                GetProseCssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 12)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 12)),
                ]),
                GetProseCssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 12)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 12)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 12)),
                ]),
                GetProseCssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 14)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 14)),
                ]),
                GetProseCssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(12, 14)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(16 / 12m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 12)),
                ]),
                GetProseCssRuleSet("> :first-child", [
                    (CssProperties.MarginTop, "0"),
                ]),
                GetProseCssRuleSet("> :last-child", [
                    (CssProperties.MarginBottom, "0"),
                ]),
            ],
        };
    }
}