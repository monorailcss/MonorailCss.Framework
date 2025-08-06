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
                new CssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(22, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 22m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 22)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 22)),
                ]),
                new CssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(40, 24)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(24, 24)),
                ]),
                new CssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(48, 18)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(40, 48)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(48 / 48m)),
                ]),
                new CssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 18)),
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 30)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 30m)),
                ]),
                new CssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 24m)),
                ]),
                new CssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(8, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 18m)),
                ]),
                new CssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                new CssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                new CssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                new CssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(5)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(4, 18)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 18)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(4, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 18)),
                ]),
                new CssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                ]),
                new CssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(26, 30)),
                ]),
                new CssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(21, 24)),
                ]),
                new CssRuleSet("pre", [
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
                new CssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(28, 18)),
                ]),
                new CssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(28, 18)),
                ]),
                new CssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 18)),
                ]),
                new CssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 18)),
                ]),
                new CssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 18)),
                ]),
                new CssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 18)),
                ]),
                new CssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 18)),
                ]),
                new CssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(24, 18)),
                ]),
                new CssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 18)), (CssProperties.PaddingLeft, ModifierSettings.Em(28, 18)),
                ]),
                new CssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(56, 18)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)), (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 16m)),
                ]),
                new CssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 16)),
                ]),
                new CssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(12, 16)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 16)),
                ]),
                new CssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 18)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 18)),
                ]),
                new CssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(16, 18)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(24 / 16m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 16)),
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