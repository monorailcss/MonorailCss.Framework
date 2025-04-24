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
                GetProseCssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(20, 20)),
                ]),
                GetProseCssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(36, 16)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 36)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 36m)),
                ]),
                GetProseCssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(24, 16)),
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 24m)),
                ]),
                GetProseCssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 16)),
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(12, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                ]),
                GetProseCssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 16m)),
                ]),
                GetProseCssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                GetProseCssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                GetProseCssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                GetProseCssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(3, 16)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(6, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(3, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 16)),
                ]),
                GetProseCssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                ]),
                GetProseCssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(21, 24)),
                ]),
                GetProseCssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                ]),
                GetProseCssRuleSet("pre", [
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
                GetProseCssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(26, 16)),
                ]),
                GetProseCssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(26, 16)),
                ]),
                GetProseCssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(8, 16)),
                ]),
                GetProseCssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 16)),
                ]),
                GetProseCssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(6, 16)),
                ]),
                GetProseCssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 16)),
                ]),
                GetProseCssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 16)),
                ]),
                GetProseCssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 16)),
                ]),
                GetProseCssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(8, 16)), (CssProperties.PaddingLeft, ModifierSettings.Em(26, 16)),
                ]),
                GetProseCssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(48, 16)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)), (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 14m)),
                ]),
                GetProseCssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 14)),
                ]),
                GetProseCssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(8, 14)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 14)),
                ]),
                GetProseCssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                ]),
                GetProseCssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(14, 16)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(20 / 14m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 14)),
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