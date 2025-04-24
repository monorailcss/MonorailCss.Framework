using MonorailCss.Css;

namespace MonorailCss.Plugins.Prose;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public partial class Prose
{
    private CssSettings GetLgCssSettings()
    {
        return new CssSettings
        {
            Css =
            [
                (CssProperties.FontSize, ModifierSettings.Rem(18)),
                (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 18m)),
            ],
            ChildRules =
            [
                GetProseCssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(22, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 22m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 22)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 22)),
                ]),
                GetProseCssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(40, 24)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(24, 24)),
                ]),
                GetProseCssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(48, 18)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(40, 48)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(48 / 48m)),
                ]),
                GetProseCssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 18)),
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 30)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 30m)),
                ]),
                GetProseCssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 24m)),
                ]),
                GetProseCssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                ]),
                GetProseCssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                GetProseCssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                GetProseCssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                GetProseCssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(4, 18)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(4, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 18)),
                ]),
                GetProseCssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                ]),
                GetProseCssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(26, 30)),
                ]),
                GetProseCssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(21, 24)),
                ]),
                GetProseCssRuleSet("pre", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 16m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 16)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 16)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(6)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(16, 16)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(24, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(24, 16)),
                ]),
                GetProseCssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(28, 18)),
                ]),
                GetProseCssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(28, 18)),
                ]),
                GetProseCssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 18)),
                ]),
                GetProseCssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 18)),
                ]),
                GetProseCssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 18)),
                ]),
                GetProseCssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 18)),
                ]),
                GetProseCssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 18)),
                ]),
                GetProseCssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                ]),
                GetProseCssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 18)), (CssProperties.PaddingLeft, ModifierSettings.Em(28, 18)),
                ]),
                GetProseCssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(56, 18)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)), (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 16m)),
                ]),
                GetProseCssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 16)),
                ]),
                GetProseCssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 16)),
                ]),
                GetProseCssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                GetProseCssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 16m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 16)),
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