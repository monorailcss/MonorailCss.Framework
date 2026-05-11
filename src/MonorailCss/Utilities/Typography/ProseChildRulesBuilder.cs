using System.Collections.Immutable;
using MonorailCss.Ast;
using static MonorailCss.Utilities.Typography.ProseUtilityHelpers;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Builds the canonical Tailwind v4.2 prose child-rule list shared by ProseBaseUtility (the
/// `.prose` root) and ProseSizeUtility (`prose-sm`, `prose-base`, `prose-lg`, `prose-xl`,
/// `prose-2xl`). The root utility emits the full set including color/font-weight/static-only
/// declarations; size variants emit only the size-dependent subset.
/// </summary>
internal static class ProseChildRulesBuilder
{
    public static List<ChildRule> Build(string size, Theme.Theme theme, bool isRoot)
    {
        var rules = new List<ChildRule>();

        rules.Add(Rule("p", theme, [
            ("margin-top", $"--typography-{size}-p-margin-top"),
            ("margin-bottom", $"--typography-{size}-p-margin-bottom"),
        ]));

        var leadDecls = new List<(string, string)>
        {
            ("font-size", $"--typography-{size}-lead-font-size"),
            ("line-height", $"--typography-{size}-lead-line-height"),
            ("margin-top", $"--typography-{size}-lead-margin-top"),
            ("margin-bottom", $"--typography-{size}-lead-margin-bottom"),
        };
        if (isRoot) leadDecls.Add(("color", "var(--tw-prose-lead)"));
        rules.Add(Rule("[class~=\"lead\"]", theme, leadDecls.ToArray()));

        if (isRoot)
        {
            rules.Add(Rule("a", theme, [
                ("color", "var(--tw-prose-links)"),
                ("text-decoration", "underline"),
                ("font-weight", "500"),
            ]));

            rules.Add(Rule("strong", theme, [
                ("color", "var(--tw-prose-bold)"),
                ("font-weight", "600"),
            ]));

            rules.Add(Rule("a strong", theme, [("color", "inherit")]));
            rules.Add(Rule("blockquote strong", theme, [("color", "inherit")]));
            rules.Add(Rule("thead th strong", theme, [("color", "inherit")]));

            rules.Add(Rule("ol[type=\"A\"]", theme, [("list-style-type", "upper-alpha")]));
            rules.Add(Rule("ol[type=\"a\"]", theme, [("list-style-type", "lower-alpha")]));
            rules.Add(Rule("ol[type=\"A\" s]", theme, [("list-style-type", "upper-alpha")]));
            rules.Add(Rule("ol[type=\"a\" s]", theme, [("list-style-type", "lower-alpha")]));
            rules.Add(Rule("ol[type=\"I\"]", theme, [("list-style-type", "upper-roman")]));
            rules.Add(Rule("ol[type=\"i\"]", theme, [("list-style-type", "lower-roman")]));
            rules.Add(Rule("ol[type=\"I\" s]", theme, [("list-style-type", "upper-roman")]));
            rules.Add(Rule("ol[type=\"i\" s]", theme, [("list-style-type", "lower-roman")]));
            rules.Add(Rule("ol[type=\"1\"]", theme, [("list-style-type", "decimal")]));
        }

        rules.Add(Rule("ol", theme, isRoot
            ? [
                ("margin-top", $"--typography-{size}-ol-margin-top"),
                ("margin-bottom", $"--typography-{size}-ol-margin-bottom"),
                ("padding-inline-start", $"--typography-{size}-ol-padding-inline-start"),
                ("list-style-type", "decimal"),
            ]
            : [
                ("margin-top", $"--typography-{size}-ol-margin-top"),
                ("margin-bottom", $"--typography-{size}-ol-margin-bottom"),
                ("padding-inline-start", $"--typography-{size}-ol-padding-inline-start"),
            ]));

        rules.Add(Rule("ul", theme, isRoot
            ? [
                ("margin-top", $"--typography-{size}-ul-margin-top"),
                ("margin-bottom", $"--typography-{size}-ul-margin-bottom"),
                ("padding-inline-start", $"--typography-{size}-ul-padding-inline-start"),
                ("list-style-type", "disc"),
            ]
            : [
                ("margin-top", $"--typography-{size}-ul-margin-top"),
                ("margin-bottom", $"--typography-{size}-ul-margin-bottom"),
                ("padding-inline-start", $"--typography-{size}-ul-padding-inline-start"),
            ]));

        if (isRoot)
        {
            rules.Add(Rule("ol > li::marker", theme, [
                ("font-weight", "400"),
                ("color", "var(--tw-prose-counters)"),
            ]));
            rules.Add(Rule("ul > li::marker", theme, [
                ("color", "var(--tw-prose-bullets)"),
            ]));
        }

        if (isRoot)
        {
            rules.Add(Rule("dt", theme, [
                ("margin-top", $"--typography-{size}-dt-margin-top"),
                ("color", "var(--tw-prose-headings)"),
                ("font-weight", "600"),
            ]));
        }
        else
        {
            rules.Add(Rule("dt", theme, [
                ("margin-top", $"--typography-{size}-dt-margin-top"),
            ]));
        }

        rules.Add(Rule("hr", theme, isRoot
            ? [
                ("margin-top", $"--typography-{size}-hr-margin-top"),
                ("margin-bottom", $"--typography-{size}-hr-margin-bottom"),
                ("border-color", "var(--tw-prose-hr)"),
                ("border-top-width", "1px"),
            ]
            : [
                ("margin-top", $"--typography-{size}-hr-margin-top"),
                ("margin-bottom", $"--typography-{size}-hr-margin-bottom"),
            ]));

        if (isRoot)
        {
            rules.Add(Rule("blockquote", theme, [
                ("margin-top", $"--typography-{size}-blockquote-margin-top"),
                ("margin-bottom", $"--typography-{size}-blockquote-margin-bottom"),
                ("padding-inline-start", $"--typography-{size}-blockquote-padding-inline-start"),
                ("border-inline-start-width", "0.25rem"),
                ("border-inline-start-color", "var(--tw-prose-quote-borders)"),
                ("quotes", "\"\\201C\"\"\\201D\"\"\\2018\"\"\\2019\""),
                ("font-style", "italic"),
                ("font-weight", "500"),
                ("color", "var(--tw-prose-quotes)"),
            ]));

            rules.Add(Rule("blockquote p:first-of-type::before", theme, [
                ("content", "open-quote"),
            ]));
            rules.Add(Rule("blockquote p:last-of-type::after", theme, [
                ("content", "close-quote"),
            ]));
        }
        else
        {
            rules.Add(Rule("blockquote", theme, [
                ("margin-top", $"--typography-{size}-blockquote-margin-top"),
                ("margin-bottom", $"--typography-{size}-blockquote-margin-bottom"),
                ("padding-inline-start", $"--typography-{size}-blockquote-padding-inline-start"),
            ]));
        }

        AddHeading(rules, "h1", size, theme, isRoot, fontWeight: "800", strongFontWeight: "900");
        AddHeading(rules, "h2", size, theme, isRoot, fontWeight: "700", strongFontWeight: "800");
        AddHeading(rules, "h3", size, theme, isRoot, fontWeight: "600", strongFontWeight: "700");
        AddHeading(rules, "h4", size, theme, isRoot, fontWeight: "600", strongFontWeight: "700", isH4OrLater: true);

        rules.Add(Rule("img", theme, [
            ("margin-top", $"--typography-{size}-img-margin-top"),
            ("margin-bottom", $"--typography-{size}-img-margin-bottom"),
        ]));

        rules.Add(Rule("picture", theme, isRoot
            ? [
                ("display", "block"),
                ("margin-top", $"--typography-{size}-img-margin-top"),
                ("margin-bottom", $"--typography-{size}-img-margin-bottom"),
            ]
            : [
                ("margin-top", $"--typography-{size}-img-margin-top"),
                ("margin-bottom", $"--typography-{size}-img-margin-bottom"),
            ]));

        rules.Add(Rule("picture > img", theme, [
            ("margin-top", "0"),
            ("margin-bottom", "0"),
        ]));

        rules.Add(Rule("video", theme, [
            ("margin-top", $"--typography-{size}-video-margin-top"),
            ("margin-bottom", $"--typography-{size}-video-margin-bottom"),
        ]));

        if (isRoot)
        {
            rules.Add(Rule("kbd", theme, [
                ("font-size", $"--typography-{size}-kbd-font-size"),
                ("border-radius", $"--typography-{size}-kbd-border-radius"),
                ("padding-top", $"--typography-{size}-kbd-padding-top"),
                ("padding-inline-end", $"--typography-{size}-kbd-padding-inline-end"),
                ("padding-bottom", $"--typography-{size}-kbd-padding-bottom"),
                ("padding-inline-start", $"--typography-{size}-kbd-padding-inline-start"),
                ("font-family", "inherit"),
                ("color", "var(--tw-prose-kbd)"),
                ("font-weight", "500"),
                ("box-shadow", "0 0 0 1px var(--tw-prose-kbd-shadows), 0 3px 0 var(--tw-prose-kbd-shadows)"),
            ]));

            rules.Add(Rule("code", theme, [
                ("font-size", $"--typography-{size}-code-font-size"),
                ("color", "var(--tw-prose-code)"),
                ("font-weight", "600"),
            ]));

            rules.Add(Rule("code::before", theme, [("content", "\"`\"")]));
            rules.Add(Rule("code::after", theme, [("content", "\"`\"")]));

            rules.Add(Rule("a code", theme, [("color", "inherit")]));
            rules.Add(Rule("h1 code", theme, [("color", "inherit")]));
            rules.Add(Rule("h2 code", theme, [
                ("color", "inherit"),
                ("font-size", $"--typography-{size}-h2-code-font-size"),
            ]));
            rules.Add(Rule("h3 code", theme, [
                ("color", "inherit"),
                ("font-size", $"--typography-{size}-h3-code-font-size"),
            ]));
            rules.Add(Rule("h4 code", theme, [("color", "inherit")]));
            rules.Add(Rule("blockquote code", theme, [("color", "inherit")]));
            rules.Add(Rule("thead th code", theme, [("color", "inherit")]));

            rules.Add(Rule("pre", theme, [
                ("font-size", $"--typography-{size}-pre-font-size"),
                ("line-height", $"--typography-{size}-pre-line-height"),
                ("margin-top", $"--typography-{size}-pre-margin-top"),
                ("margin-bottom", $"--typography-{size}-pre-margin-bottom"),
                ("border-radius", $"--typography-{size}-pre-border-radius"),
                ("padding-top", $"--typography-{size}-pre-padding-top"),
                ("padding-inline-end", $"--typography-{size}-pre-padding-inline-end"),
                ("padding-bottom", $"--typography-{size}-pre-padding-bottom"),
                ("padding-inline-start", $"--typography-{size}-pre-padding-inline-start"),
                ("color", "var(--tw-prose-pre-code)"),
                ("background-color", "var(--tw-prose-pre-bg)"),
                ("overflow-x", "auto"),
                ("font-weight", "400"),
            ]));

            rules.Add(Rule("pre code", theme, [
                ("background-color", "transparent"),
                ("border-width", "0"),
                ("border-radius", "0"),
                ("padding", "0"),
                ("font-weight", "inherit"),
                ("color", "inherit"),
                ("font-size", "inherit"),
                ("font-family", "inherit"),
                ("line-height", "inherit"),
            ]));

            rules.Add(Rule("pre code::before", theme, [("content", "none")]));
            rules.Add(Rule("pre code::after", theme, [("content", "none")]));
        }
        else
        {
            rules.Add(Rule("kbd", theme, [
                ("font-size", $"--typography-{size}-kbd-font-size"),
                ("border-radius", $"--typography-{size}-kbd-border-radius"),
                ("padding-top", $"--typography-{size}-kbd-padding-top"),
                ("padding-inline-end", $"--typography-{size}-kbd-padding-inline-end"),
                ("padding-bottom", $"--typography-{size}-kbd-padding-bottom"),
                ("padding-inline-start", $"--typography-{size}-kbd-padding-inline-start"),
            ]));

            rules.Add(Rule("code", theme, [
                ("font-size", $"--typography-{size}-code-font-size"),
            ]));

            rules.Add(Rule("h2 code", theme, [
                ("font-size", $"--typography-{size}-h2-code-font-size"),
            ]));

            rules.Add(Rule("h3 code", theme, [
                ("font-size", $"--typography-{size}-h3-code-font-size"),
            ]));

            rules.Add(Rule("pre", theme, [
                ("font-size", $"--typography-{size}-pre-font-size"),
                ("line-height", $"--typography-{size}-pre-line-height"),
                ("margin-top", $"--typography-{size}-pre-margin-top"),
                ("margin-bottom", $"--typography-{size}-pre-margin-bottom"),
                ("border-radius", $"--typography-{size}-pre-border-radius"),
                ("padding-top", $"--typography-{size}-pre-padding-top"),
                ("padding-inline-end", $"--typography-{size}-pre-padding-inline-end"),
                ("padding-bottom", $"--typography-{size}-pre-padding-bottom"),
                ("padding-inline-start", $"--typography-{size}-pre-padding-inline-start"),
            ]));
        }

        rules.Add(Rule("li", theme, [
            ("margin-top", $"--typography-{size}-li-margin-top"),
            ("margin-bottom", $"--typography-{size}-li-margin-bottom"),
        ]));

        rules.Add(Rule("ol > li", theme, [
            ("padding-inline-start", $"--typography-{size}-ol-li-padding-inline-start"),
        ]));

        rules.Add(Rule("ul > li", theme, [
            ("padding-inline-start", $"--typography-{size}-ul-li-padding-inline-start"),
        ]));

        rules.Add(Rule("> ul > li p", theme, [
            ("margin-top", $"--typography-{size}-list-paragraph-margin-top"),
            ("margin-bottom", $"--typography-{size}-list-paragraph-margin-bottom"),
        ]));

        rules.Add(Rule("> ul > li > p:first-child", theme, [
            ("margin-top", $"--typography-{size}-p-margin-top"),
        ]));

        rules.Add(Rule("> ul > li > p:last-child", theme, [
            ("margin-bottom", $"--typography-{size}-p-margin-bottom"),
        ]));

        rules.Add(Rule("> ol > li > p:first-child", theme, [
            ("margin-top", $"--typography-{size}-p-margin-top"),
        ]));

        rules.Add(Rule("> ol > li > p:last-child", theme, [
            ("margin-bottom", $"--typography-{size}-p-margin-bottom"),
        ]));

        rules.Add(Rule("ul ul, ul ol, ol ul, ol ol", theme, [
            ("margin-top", $"--typography-{size}-nested-list-margin-top"),
            ("margin-bottom", $"--typography-{size}-nested-list-margin-bottom"),
        ]));

        rules.Add(Rule("dl", theme, [
            ("margin-top", $"--typography-{size}-dl-margin-top"),
            ("margin-bottom", $"--typography-{size}-dl-margin-bottom"),
        ]));

        rules.Add(Rule("dd", theme, isRoot
            ? [
                ("margin-top", $"--typography-{size}-dd-margin-top"),
                ("padding-inline-start", $"--typography-{size}-dd-padding-inline-start"),
                ("color", "var(--tw-prose-body)"),
            ]
            : [
                ("margin-top", $"--typography-{size}-dd-margin-top"),
                ("padding-inline-start", $"--typography-{size}-dd-padding-inline-start"),
            ]));

        rules.Add(Rule("hr + *", theme, [("margin-top", "0")]));
        rules.Add(Rule("h2 + *", theme, [("margin-top", "0")]));
        rules.Add(Rule("h3 + *", theme, [("margin-top", "0")]));
        rules.Add(Rule("h4 + *", theme, [("margin-top", "0")]));

        rules.Add(Rule("table", theme, isRoot
            ? [
                ("width", "100%"),
                ("table-layout", "auto"),
                ("margin-top", Em(32, GetBaseFontSize(size))),
                ("margin-bottom", Em(32, GetBaseFontSize(size))),
                ("font-size", $"--typography-{size}-table-font-size"),
                ("line-height", $"--typography-{size}-table-line-height"),
            ]
            : [
                ("font-size", $"--typography-{size}-table-font-size"),
                ("line-height", $"--typography-{size}-table-line-height"),
            ]));

        if (isRoot)
        {
            rules.Add(Rule("thead", theme, [
                ("border-bottom-width", "1px"),
                ("border-bottom-color", "var(--tw-prose-th-borders)"),
            ]));

            rules.Add(Rule("thead th", theme, [
                ("color", "var(--tw-prose-headings)"),
                ("font-weight", "600"),
                ("vertical-align", "bottom"),
                ("padding-inline-end", $"--typography-{size}-thead-th-padding-inline-end"),
                ("padding-bottom", $"--typography-{size}-thead-th-padding-bottom"),
                ("padding-inline-start", $"--typography-{size}-thead-th-padding-inline-start"),
            ]));

            rules.Add(Rule("tbody tr", theme, [
                ("border-bottom-width", "1px"),
                ("border-bottom-color", "var(--tw-prose-td-borders)"),
            ]));

            rules.Add(Rule("tbody tr:last-child", theme, [("border-bottom-width", "0")]));

            rules.Add(Rule("tbody td", theme, [("vertical-align", "baseline")]));
            rules.Add(Rule("tfoot", theme, [
                ("border-top-width", "1px"),
                ("border-top-color", "var(--tw-prose-th-borders)"),
            ]));
            rules.Add(Rule("tfoot td", theme, [("vertical-align", "top")]));
            rules.Add(Rule("th, td", theme, [("text-align", "start")]));
        }
        else
        {
            rules.Add(Rule("thead th", theme, [
                ("padding-inline-end", $"--typography-{size}-thead-th-padding-inline-end"),
                ("padding-bottom", $"--typography-{size}-thead-th-padding-bottom"),
                ("padding-inline-start", $"--typography-{size}-thead-th-padding-inline-start"),
            ]));
        }

        rules.Add(Rule("thead th:first-child", theme, [("padding-inline-start", "0")]));
        rules.Add(Rule("thead th:last-child", theme, [("padding-inline-end", "0")]));

        rules.Add(Rule("tbody td, tfoot td", theme, [
            ("padding-top", $"--typography-{size}-tbody-td-padding-top"),
            ("padding-inline-end", $"--typography-{size}-tbody-td-padding-inline-end"),
            ("padding-bottom", $"--typography-{size}-tbody-td-padding-bottom"),
            ("padding-inline-start", $"--typography-{size}-tbody-td-padding-inline-start"),
        ]));

        rules.Add(Rule("tbody td:first-child, tfoot td:first-child", theme, [("padding-inline-start", "0")]));
        rules.Add(Rule("tbody td:last-child, tfoot td:last-child", theme, [("padding-inline-end", "0")]));

        rules.Add(Rule("figure", theme, [
            ("margin-top", $"--typography-{size}-figure-margin-top"),
            ("margin-bottom", $"--typography-{size}-figure-margin-bottom"),
        ]));

        rules.Add(Rule("figure > *", theme, [
            ("margin-top", "0"),
            ("margin-bottom", "0"),
        ]));

        rules.Add(Rule("figcaption", theme, isRoot
            ? [
                ("font-size", $"--typography-{size}-figcaption-font-size"),
                ("line-height", $"--typography-{size}-figcaption-line-height"),
                ("margin-top", $"--typography-{size}-figcaption-margin-top"),
                ("color", "var(--tw-prose-captions)"),
            ]
            : [
                ("font-size", $"--typography-{size}-figcaption-font-size"),
                ("line-height", $"--typography-{size}-figcaption-line-height"),
                ("margin-top", $"--typography-{size}-figcaption-margin-top"),
            ]));

        rules.Add(Rule("> :first-child", theme, [("margin-top", "0")]));
        rules.Add(Rule("> :last-child", theme, [("margin-bottom", "0")]));

        return rules.Where(r => r.Declarations.Count > 0).ToList();
    }

    private static double GetBaseFontSize(string size) => size switch
    {
        "sm" => 14,
        "lg" => 18,
        "xl" => 20,
        "2xl" => 24,
        _ => 16,
    };

    private static void AddHeading(
        List<ChildRule> rules,
        string h,
        string size,
        Theme.Theme theme,
        bool isRoot,
        string fontWeight,
        string strongFontWeight,
        bool isH4OrLater = false)
    {
        var sizeKey = isH4OrLater ? "h4" : h;
        var decls = new List<(string, string)>();
        if (!isH4OrLater)
        {
            decls.Add(("font-size", $"--typography-{size}-{sizeKey}-font-size"));
        }

        decls.Add(("margin-top", $"--typography-{size}-{sizeKey}-margin-top"));
        decls.Add(("margin-bottom", $"--typography-{size}-{sizeKey}-margin-bottom"));
        decls.Add(("line-height", $"--typography-{size}-{sizeKey}-line-height"));

        if (isRoot)
        {
            decls.Add(("color", "var(--tw-prose-headings)"));
            decls.Add(("font-weight", fontWeight));
        }

        rules.Add(Rule(h, theme, decls.ToArray()));

        if (isRoot)
        {
            rules.Add(Rule($"{h} strong", theme, [
                ("color", "inherit"),
                ("font-weight", strongFontWeight),
            ]));
        }
    }

    private static ChildRule Rule(string selector, Theme.Theme theme, (string Property, string ValueOrThemeKey)[] styles)
    {
        if (styles.Length == 0)
        {
            return new ChildRule(selector, ImmutableList<Declaration>.Empty);
        }

        var declarations = ImmutableList.CreateBuilder<Declaration>();
        foreach (var (property, valueOrThemeKey) in styles)
        {
            string? value;
            if (valueOrThemeKey.StartsWith("--typography-"))
            {
                value = theme.ResolveValue(valueOrThemeKey, ["--typography"]);
            }
            else
            {
                value = valueOrThemeKey;
            }

            if (value != null)
            {
                declarations.Add(new Declaration(property, value));
            }
        }

        return new ChildRule(selector, declarations.ToImmutable());
    }
}
