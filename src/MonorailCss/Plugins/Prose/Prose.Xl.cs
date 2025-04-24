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
                GetProseCssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(24, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 24m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 24)),
                ]),
                GetProseCssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(48, 30)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 30)),
                ]),
                GetProseCssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(56, 20)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(48, 56)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(56 / 56m)),
                ]),
                GetProseCssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(36, 20)),
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 36)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 36)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 36m)),
                ]),
                GetProseCssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 20)),
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(20, 30)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 30m)),
                ]),
                GetProseCssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(36, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(12, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                ]),
                GetProseCssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                GetProseCssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                GetProseCssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                GetProseCssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(5, 20)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 20)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(5, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 20)),
                ]),
                GetProseCssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                ]),
                GetProseCssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(31, 36)),
                ]),
                GetProseCssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(27, 30)),
                ]),
                GetProseCssRuleSet("pre", [
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
                GetProseCssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                GetProseCssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                GetProseCssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 20)),
                ]),
                GetProseCssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 20)),
                ]),
                GetProseCssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 20)),
                ]),
                GetProseCssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 20)),
                ]),
                GetProseCssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 20)),
                ]),
                GetProseCssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 20)),
                ]),
                GetProseCssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 20)), (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                GetProseCssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(56, 20)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)), (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                ]),
                GetProseCssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 18)),
                ]),
                GetProseCssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(16, 18)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 18)),
                ]),
                GetProseCssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                GetProseCssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                GetProseCssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)), (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                ]),
                GetProseCssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(18, 20)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(18, 18)),
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