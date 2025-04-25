using MonorailCss.Css;

namespace MonorailCss.Plugins.Prose;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public partial class Prose
{
    private CssSettings Get2XlCssSettings()
    {
        return new CssSettings
        {
            Css =
            [
                (CssProperties.FontSize, ModifierSettings.Rem(24)),
                (CssProperties.LineHeight, ModifierSettings.Rounds(40 / 24m)),
            ],
            ChildRules =
            [
                new CssRuleSet("p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.FontSize, ModifierSettings.Em(30, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(44 / 30m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 30)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 30)),
                ]),
                new CssRuleSet("blockquote", [
                    (CssProperties.MarginTop, ModifierSettings.Em(64, 36)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(64, 36)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(40, 36)),
                ]),
                new CssRuleSet("h1", [
                    (CssProperties.FontSize, ModifierSettings.Em(64, 24)),
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, ModifierSettings.Em(56, 64)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(64 / 64m)),
                ]),
                new CssRuleSet("h2", [
                    (CssProperties.FontSize, ModifierSettings.Em(48, 24)),
                    (CssProperties.MarginTop, ModifierSettings.Em(72, 48)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(40, 48)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(52 / 48m)),
                ]),
                new CssRuleSet("h3", [
                    (CssProperties.FontSize, ModifierSettings.Em(36, 24)),
                    (CssProperties.MarginTop, ModifierSettings.Em(56, 36)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(24, 36)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(44 / 36m)),
                ]),
                new CssRuleSet("h4", [
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(16, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 24m)),
                ]),
                new CssRuleSet("img", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(48, 24)),
                ]),
                new CssRuleSet("picture", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(48, 24)),
                ]),
                new CssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("video", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(48, 24)),
                ]),
                new CssRuleSet("kbd", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 24)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(6)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(6, 24)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(8, 24)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(6, 24)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(8, 24)),
                ]),
                new CssRuleSet("code", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 24)),
                ]),
                new CssRuleSet("h2 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(42, 48)),
                ]),
                new CssRuleSet("h3 code", [
                    (CssProperties.FontSize, ModifierSettings.Em(32, 36)),
                ]),
                new CssRuleSet("pre", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(36 / 20m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(40, 20)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(40, 20)),
                    (CssProperties.BorderRadius, ModifierSettings.Rem(8)),
                    (CssProperties.PaddingTop, ModifierSettings.Em(24, 20)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(32, 20)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(24, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(32, 20)),
                ]),
                new CssRuleSet("ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 24)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(38, 24)),
                ]),
                new CssRuleSet("ul", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)),
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 24)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(38, 24)),
                ]),
                new CssRuleSet("li", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(12, 24)),
                ]),
                new CssRuleSet("ol > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(10, 24)),
                ]),
                new CssRuleSet("ul > li", [
                    (CssProperties.PaddingLeft, ModifierSettings.Em(10, 24)),
                ]),
                new CssRuleSet("> ul > li p", [
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(20, 24)),
                ]),
                new CssRuleSet("> ul > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("> ul > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("> ol > li > *:first-child", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("> ol > li > *:last-child", [
                    (CssProperties.MarginBottom, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("ul ul, ul ol, ol ul, ol ol", [
                    (CssProperties.MarginTop, ModifierSettings.Em(16, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(16, 24)),
                ]),
                new CssRuleSet("dl", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("dt", [
                    (CssProperties.MarginTop, ModifierSettings.Em(32, 24)),
                ]),
                new CssRuleSet("dd", [
                    (CssProperties.MarginTop, ModifierSettings.Em(12, 24)), (CssProperties.PaddingLeft, ModifierSettings.Em(38, 24)),
                ]),
                new CssRuleSet("hr", [
                    (CssProperties.MarginTop, ModifierSettings.Em(72, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(72, 24)),
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
                    (CssProperties.FontSize, ModifierSettings.Em(20, 24)), (CssProperties.LineHeight, ModifierSettings.Rounds(28 / 20m)),
                ]),
                new CssRuleSet("thead th", [
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 20)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 20)),
                ]),
                new CssRuleSet("thead th:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("thead th:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("tbody td, tfoot td", [
                    (CssProperties.PaddingTop, ModifierSettings.Em(16, 20)),
                    (CssProperties.PaddingRight, ModifierSettings.Em(12, 20)),
                    (CssProperties.PaddingBottom, ModifierSettings.Em(16, 20)),
                    (CssProperties.PaddingLeft, ModifierSettings.Em(12, 20)),
                ]),
                new CssRuleSet("tbody td:first-child, tfoot td:first-child", [
                    (CssProperties.PaddingLeft, "0"),
                ]),
                new CssRuleSet("tbody td:last-child, tfoot td:last-child", [
                    (CssProperties.PaddingRight, "0"),
                ]),
                new CssRuleSet("figure", [
                    (CssProperties.MarginTop, ModifierSettings.Em(48, 24)), (CssProperties.MarginBottom, ModifierSettings.Em(48, 24)),
                ]),
                new CssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"), (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("figcaption", [
                    (CssProperties.FontSize, ModifierSettings.Em(20, 24)),
                    (CssProperties.LineHeight, ModifierSettings.Rounds(32 / 20m)),
                    (CssProperties.MarginTop, ModifierSettings.Em(20, 20)),
                ]),
                new CssRuleSet("> :first-child", [
                    (CssProperties.MarginTop, "0"),
                ]),
                new CssRuleSet("> :last-child", [
                    (CssProperties.MarginBottom, "0"),
                ]),
            ],
        };
    }
}