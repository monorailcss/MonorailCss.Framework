﻿using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;
using MonorailCss.Variants;
using static MonorailCss.Plugins.ModifierSettings;

namespace MonorailCss.Plugins.Prose;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public partial class Prose : IUtilityNamespacePlugin, IVariantPluginProvider
{
    /// <inheritdoc />
    public class Settings : ISettings<Prose>
    {
        /// <summary>
        /// Gets the namespace to be used for CSS classes.
        /// </summary>
        public string Namespace { get; init; } = "prose";

        /// <summary>
        /// Gets the names of the colors to create gray scale prose modifiers.
        /// </summary>
        public string[] GrayScales { get; init; } = [ColorNames.Gray, ColorNames.Slate, ColorNames.Zinc, ColorNames.Neutral, ColorNames.Stone];

        /// <summary>
        /// Gets the custom settings to apply to the default settings.
        /// </summary>
        public Func<DesignSystem, ImmutableDictionary<string, CssSettings>> CustomSettings { get; init; } =
            _ => ImmutableDictionary<string, CssSettings>.Empty;

        /// <summary>
        /// Gets the not prose class name.
        /// </summary>
        public string NotProseClassName { get; init; } = "not-prose";

        /// <summary>
        /// Gets the max width for prose.
        /// </summary>
        public string MaxWidth { get; init; } = "65ch";
    }

    private readonly DesignSystem _designSystem;
    private readonly Settings _settings;

    private readonly Func<string, string> _cssVar = CssFramework.GetCssVariableWithPrefix;
    private readonly Func<string, string> _var = CssFramework.GetVariableNameWithPrefix;

    private readonly ImmutableDictionary<string, CssSettings> _configuredSettings;

    /// <summary>
    /// Creates the not-prose part of the selector.
    /// </summary>
    /// <param name="selector">The base selector.</param>
    /// <returns>The selector with not-prose exclusion.</returns>
    private string WithNotProse(string selector)
    {
        return $":where({selector}):not(:where([class~=\"{_settings.NotProseClassName}\"],[class~=\"{_settings.NotProseClassName}\"] *))";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Prose"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="framework">The framework.</param>
    /// <param name="settings">The settings.</param>
    public Prose(DesignSystem designSystem, CssFramework framework, Settings settings)
    {
        _designSystem = designSystem;
        _settings = settings;

        var standardSettings = GetStandardSettings();
        var defaultSettings = GetDefaultSettings() + standardSettings["base"] + standardSettings[ColorNames.Gray];

        var customSettings = _settings.CustomSettings(_designSystem);

        _configuredSettings = standardSettings.Add("DEFAULT", defaultSettings);
        foreach (var customSetting in customSettings)
        {
            _configuredSettings = _configuredSettings.TryGetValue(customSetting.Key, out var setting)
                ? _configuredSettings.SetItem(
                    customSetting.Key,
                    setting + customSetting.Value)
                : _configuredSettings.Add(customSetting.Key, customSetting.Value);
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        string? suffix;
        if (syntax is NamespaceSyntax namespaceSyntax && namespaceSyntax.NamespaceEquals(_settings.Namespace))
        {
            suffix = namespaceSyntax.Suffix ?? "DEFAULT";
        }
        else
        {
            yield break;
        }

        if (!_configuredSettings.TryGetValue(suffix, out var settings))
        {
            yield break;
        }

        yield return new CssRuleSet(syntax.OriginalSyntax, settings.Css);

        foreach (var childRule in settings.ChildRules)
        {
            yield return childRule with
            {
                Selector = $"{syntax.OriginalSyntax} {WithNotProse(childRule.Selector.ToString())}",
            };
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // this might not fit the mold...
        yield break;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces =>
    [
        _settings.Namespace,
    ];

    /// <inheritdoc />
    public IEnumerable<(string Modifier, IVariant Variant)> GetVariants()
    {
        return
        [

            // Existing variants
            GetProseVariant("headings", "h1", "h2", "h3", "h4", "th"),
            GetProseVariant("lead", "[class~=\"lead\"]"),
            GetProseVariant("h1"),
            GetProseVariant("h2"),
            GetProseVariant("h3"),
            GetProseVariant("h4"),
            GetProseVariant("p"),

            // Additional variants
            GetProseVariant("a"),
            GetProseVariant("blockquote"),
            GetProseVariant("figure"),
            GetProseVariant("figcaption"),
            GetProseVariant("strong"),
            GetProseVariant("em"),
            GetProseVariant("kbd"),
            GetProseVariant("code"),
            GetProseVariant("pre"),
            GetProseVariant("ol"),
            GetProseVariant("ul"),
            GetProseVariant("li"),
            GetProseVariant("table"),
            GetProseVariant("thead"),
            GetProseVariant("tr"),
            GetProseVariant("th"),
            GetProseVariant("td"),
            GetProseVariant("img"),
            GetProseVariant("video"),
            GetProseVariant("hr"),
        ];
    }

    private (string Modifier, IVariant Variant) GetProseVariant(string modifer, params string[]? targets)
    {
        if (targets is not { Length: not 0 })
        {
            targets = [modifer];
        }

        var ns = Namespaces[0];
        var targetsStr = string.Join(", ", targets);
        var selector = $":is(:where({targetsStr}):not(:where([class~=\"{_settings.NotProseClassName}\"],[class~=\"{_settings.NotProseClassName}\"] *)))";
        return ($"{ns}-{modifer}", new ProseElementVariant(selector));
    }

    private CssSettings GetDefaultSettings()
    {
        return new CssSettings
        {
            Css =
            {
                [CssProperties.MaxWidth] = _settings.MaxWidth,
                [CssProperties.Color] = _cssVar("prose-body"),
                [CssProperties.LineHeight] = Rounds(28 / 16m),
            },
            ChildRules =
            [

                // Empty paragraph rule to maintain correct merging order
                new CssRuleSet("p", []),

                // Lead text
                new CssRuleSet("[class~=\"lead\"]", [
                    (CssProperties.Color, _cssVar("prose-lead")),
                ]),

                // Links
                new CssRuleSet("a", [
                    (CssProperties.Color, _cssVar("prose-links")),
                    (CssProperties.TextDecoration, "underline"),
                    (CssProperties.FontWeight, "500"),
                ]),

                // Strong text
                new CssRuleSet("strong", [
                    (CssProperties.Color, _cssVar("prose-bold")),
                    (CssProperties.FontWeight, "600"),
                ]),

                // Inherit strong color in specific contexts
                new CssRuleSet("a strong", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("blockquote strong", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("thead th strong", [
                    (CssProperties.Color, "inherit"),
                ]),

                // Lists
                new CssRuleSet("ol", [
                    (CssProperties.ListStyleType, "decimal"),
                ]),
                new CssRuleSet("ol[type=\"A\"]", [
                    (CssProperties.ListStyleType, "upper-alpha"),
                ]),
                new CssRuleSet("ol[type=\"a\"]", [
                    (CssProperties.ListStyleType, "lower-alpha"),
                ]),
                new CssRuleSet("ol[type=\"A\" s]", [
                    (CssProperties.ListStyleType, "upper-alpha"),
                ]),
                new CssRuleSet("ol[type=\"a\" s]", [
                    (CssProperties.ListStyleType, "lower-alpha"),
                ]),
                new CssRuleSet("ol[type=\"I\"]", [
                    (CssProperties.ListStyleType, "upper-roman"),
                ]),
                new CssRuleSet("ol[type=\"i\"]", [
                    (CssProperties.ListStyleType, "lower-roman"),
                ]),
                new CssRuleSet("ol[type=\"I\" s]", [
                    (CssProperties.ListStyleType, "upper-roman"),
                ]),
                new CssRuleSet("ol[type=\"i\" s]", [
                    (CssProperties.ListStyleType, "lower-roman"),
                ]),
                new CssRuleSet("ol[type=\"1\"]", [
                    (CssProperties.ListStyleType, "decimal"),
                ]),
                new CssRuleSet("ul", [
                    (CssProperties.ListStyleType, "disc"),
                ]),

                // List markers
                new CssRuleSet("ol > li::marker", [
                    (CssProperties.FontWeight, "400"),
                    (CssProperties.Color, _cssVar("prose-counters")),
                ]),
                new CssRuleSet("ul > li::marker", [
                    (CssProperties.Color, _cssVar("prose-bullets")),
                ]),

                // Definition lists
                new CssRuleSet("dt", [
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "600"),
                ]),

                // Horizontal rule
                new CssRuleSet("hr", [
                    (CssProperties.BorderColor, _cssVar("prose-hr")),
                    (CssProperties.BorderTopWidth, "1px"),
                ]),

                // Blockquotes
                new CssRuleSet("blockquote", [
                    (CssProperties.FontWeight, "500"),
                    (CssProperties.FontStyle, "italic"),
                    (CssProperties.Color, _cssVar("prose-quotes")),
                    (CssProperties.BorderLeftWidth, "0.25rem"),
                    (CssProperties.BorderLeftColor, _cssVar("prose-quote-borders")),
                    (CssProperties.Quotes, "\"\\201C\"\"\\201D\"\"\\2018\"\"\\2019\""),
                ]),
                new CssRuleSet("blockquote p:first-of-type::before", [
                    (CssProperties.Content, "open-quote"),
                ]),
                new CssRuleSet("blockquote p:last-of-type::after", [
                    (CssProperties.Content, "close-quote"),
                ]),

                // Headings
                new CssRuleSet("h1", [
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "800"),
                ]),
                new CssRuleSet("h1 strong", [
                    (CssProperties.FontWeight, "900"),
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h2", [
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "700"),
                ]),
                new CssRuleSet("h2 strong", [
                    (CssProperties.FontWeight, "800"),
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h3", [
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "600"),
                ]),
                new CssRuleSet("h3 strong", [
                    (CssProperties.FontWeight, "700"),
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h4", [
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "600"),
                ]),
                new CssRuleSet("h4 strong", [
                    (CssProperties.FontWeight, "700"),
                    (CssProperties.Color, "inherit"),
                ]),

                // Images and media
                new CssRuleSet("img", []),
                new CssRuleSet("picture", [
                    (CssProperties.Display, "block"),
                ]),
                new CssRuleSet("video", []),
                new CssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("figcaption", [
                    (CssProperties.Color, _cssVar("prose-captions")),
                ]),

                // Inline elements
                new CssRuleSet("kbd", [
                    (CssProperties.FontWeight, "500"),
                    (CssProperties.FontFamily, "inherit"),
                    (CssProperties.Color, _cssVar("prose-kbd")),
                    (CssProperties.BoxShadow, "0 0 0 1px rgb(var(--tw-prose-kbd-shadows) / 10%), 0 3px 0 rgb(var(--tw-prose-kbd-shadows) / 10%)"),
                ]),

                // Code
                new CssRuleSet("code", [
                    (CssProperties.Color, _cssVar("prose-code")),
                    (CssProperties.FontWeight, "600"),
                ]),
                new CssRuleSet("code::before", [
                    (CssProperties.Content, "`"),
                ]),
                new CssRuleSet("code::after", [
                    (CssProperties.Content, "`"),
                ]),

                // Code in specific contexts
                new CssRuleSet("a code", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h1 code", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h2 code", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h3 code", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("h4 code", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("blockquote code", [
                    (CssProperties.Color, "inherit"),
                ]),
                new CssRuleSet("thead th code", [
                    (CssProperties.Color, "inherit"),
                ]),

                // Pre blocks
                new CssRuleSet("pre", [
                    (CssProperties.Color, _cssVar("prose-pre-code")),
                    (CssProperties.BackgroundColor, _cssVar("prose-pre-bg")),
                    (CssProperties.OverflowX, "auto"),
                    (CssProperties.FontWeight, "400"),
                ]),
                new CssRuleSet("pre code", [
                    (CssProperties.BackgroundColor, "transparent"),
                    (CssProperties.BorderWidth, "0"),
                    (CssProperties.BorderRadius, "0"),
                    (CssProperties.Padding, "0"),
                    (CssProperties.FontWeight, "inherit"),
                    (CssProperties.Color, "inherit"),
                    (CssProperties.FontSize, "inherit"),
                    (CssProperties.FontFamily, "inherit"),
                    (CssProperties.LineHeight, "inherit"),
                ]),
                new CssRuleSet("pre code::before", [
                    (CssProperties.Content, "none"),
                ]),
                new CssRuleSet("pre code::after", [
                    (CssProperties.Content, "none"),
                ]),

                // Tables
                new CssRuleSet("table", [
                    (CssProperties.Width, "100%"),
                    (CssProperties.TableLayout, "auto"),
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
                new CssRuleSet("thead", [
                    (CssProperties.BorderBottomWidth, "1px"),
                    (CssProperties.BorderBottomColor, _cssVar("prose-th-borders")),
                ]),
                new CssRuleSet("thead th", [
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "600"),
                    (CssProperties.VerticalAlign, "bottom"),
                ]),
                new CssRuleSet("tbody tr", [
                    (CssProperties.BorderBottomWidth, "1px"),
                    (CssProperties.BorderBottomColor, _cssVar("prose-td-borders")),
                ]),
                new CssRuleSet("tbody tr:last-child", [
                    (CssProperties.BorderBottomWidth, "0"),
                ]),
                new CssRuleSet("tbody td", [
                    (CssProperties.VerticalAlign, "baseline"),
                ]),
                new CssRuleSet("tfoot", [
                    (CssProperties.BorderTopWidth, "1px"),
                    (CssProperties.BorderTopColor, _cssVar("prose-th-borders")),
                ]),
                new CssRuleSet("tfoot td", [
                    (CssProperties.VerticalAlign, "top"),
                ]),
                new CssRuleSet("th, td", [
                    (CssProperties.TextAlign, "start"),
                ]),

                // Images, videos, and figures
                new CssRuleSet("img, video, figure", [
                    (CssProperties.MaxWidth, "100%"),
                    (CssProperties.Height, "auto"),
                ]),
                new CssRuleSet("img", [
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
                new CssRuleSet("picture", [
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
                new CssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, "0"),
                ]),
                new CssRuleSet("video", [
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
            ],
        };
    }

    private ImmutableDictionary<string, CssSettings> GetStandardSettings()
    {
        var baseSettings = new Dictionary<string, CssSettings>
        {
            {
                "base", GetBaseCssSettings()
            },
            {
                "sm", GetSmCssSettings()
            },
            {
                "lg", GetLgCssSettings()
            },
            {
                "xl", GetXlCssSettings()
            },
            {
                "2xl", Get2XlCssSettings()
            },
            {
                "invert", new CssSettings
                {
                    Css =
                    [
                        (_var("prose-body"), _cssVar("prose-invert-body")),
                        (_var("prose-lead"), _cssVar("prose-invert-lead")),
                        (_var("prose-links"), _cssVar("prose-invert-links")),
                        (_var("prose-bold"), _cssVar("prose-invert-bold")),
                        (_var("prose-counters"), _cssVar("prose-invert-counters")),
                        (_var("prose-bullets"), _cssVar("prose-invert-bullets")),
                        (_var("prose-hr"), _cssVar("prose-invert-hr")),
                        (_var("prose-quotes"), _cssVar("prose-invert-quotes")),
                        (_var("prose-quote-borders"), _cssVar("prose-invert-quote-borders")),
                        (_var("prose-captions"), _cssVar("prose-invert-captions")),
                        (_var("prose-kbd"), _cssVar("prose-invert-kbd")),
                        (_var("prose-kbd-shadows"), _cssVar("prose-invert-kbd-shadows")),
                        (_var("prose-headings"), _cssVar("prose-invert-headings")),
                        (_var("prose-code"), _cssVar("prose-invert-code")),
                        (_var("prose-pre-code"), _cssVar("prose-invert-pre-code")),
                        (_var("prose-pre-bg"), _cssVar("prose-invert-pre-bg")),
                        (_var("prose-th-borders"), _cssVar("prose-invert-th-borders")),
                        (_var("prose-td-borders"), _cssVar("prose-invert-td-borders")),
                    ],
                }
            },
        }.ToImmutableDictionary();

        return _settings.GrayScales.Aggregate(baseSettings, (current, scale) => new Dictionary<string, CssSettings>
            {
                {
                    scale, new CssSettings
                    {
                        Css =
                        [
                            (_var("prose-body"), _designSystem.Colors[scale][ColorLevels._700].AsString()),
                            (_var("prose-headings"), _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (_var("prose-lead"), _designSystem.Colors[scale][ColorLevels._600].AsString()),
                            (_var("prose-links"), _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (_var("prose-bold"), _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (_var("prose-counters"), _designSystem.Colors[scale][ColorLevels._500].AsString()),
                            (_var("prose-bullets"), _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (_var("prose-hr"), _designSystem.Colors[scale][ColorLevels._200].AsString()),
                            (_var("prose-quotes"), _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (_var("prose-quote-borders"), _designSystem.Colors[scale][ColorLevels._200].AsString()),
                            (_var("prose-captions"), _designSystem.Colors[scale][ColorLevels._500].AsString()),
                            (_var("prose-kbd"), _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (_var("prose-kbd-shadows"), "17 24 39"),
                            (_var("prose-code"), _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (_var("prose-pre-code"), _designSystem.Colors[scale][ColorLevels._200].AsString()),
                            (_var("prose-pre-bg"), _designSystem.Colors[scale][ColorLevels._800].AsString()),
                            (_var("prose-th-borders"), _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (_var("prose-td-borders"), _designSystem.Colors[scale][ColorLevels._200].AsString()),

                            // inverts
                            (_var("prose-invert-body"), _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (_var("prose-invert-headings"), "white"),
                            (_var("prose-invert-lead"), _designSystem.Colors[scale][ColorLevels._400].AsString()),
                            (_var("prose-invert-links"), "white"),
                            (_var("prose-invert-bold"), "white"),
                            (_var("prose-invert-counters"), _designSystem.Colors[scale][ColorLevels._400].AsString()),
                            (_var("prose-invert-bullets"), _designSystem.Colors[scale][ColorLevels._600].AsString()),
                            (_var("prose-invert-hr"), _designSystem.Colors[scale][ColorLevels._700].AsString()),
                            (_var("prose-invert-quotes"), _designSystem.Colors[scale][ColorLevels._100].AsString()),
                            (_var("prose-invert-quote-borders"), _designSystem.Colors[scale][ColorLevels._700].AsString()),
                            (_var("prose-invert-captions"), _designSystem.Colors[scale][ColorLevels._400].AsString()),
                            (_var("prose-invert-kbd"), "white"),
                            (_var("prose-invert-kbd-shadows"), "255 255 255"),
                            (_var("prose-invert-code"), "white"),
                            (_var("prose-invert-pre-code"), _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (_var("prose-invert-pre-bg"), "rgb(0 0 0 / 50%)"),
                            (_var("prose-invert-th-borders"), _designSystem.Colors[scale][ColorLevels._600].AsString()),
                            (_var("prose-invert-td-borders"), _designSystem.Colors[scale][ColorLevels._700].AsString()),
                        ],
                    }
                },
            }.ToImmutableDictionary()
            .AddRange(current));
    }
}
