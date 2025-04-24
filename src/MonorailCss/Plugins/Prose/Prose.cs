using System.Collections.Immutable;
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
            yield return childRule with { Selector = $"{syntax.OriginalSyntax} {childRule.Selector}", };
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // this might not fit the mold...
        yield break;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [.. new[] { _settings.Namespace }];

    /// <inheritdoc />
    public IEnumerable<(string Modifier, IVariant Variant)> GetVariants()
    {
        return
        [
            GetProseVariant("headings", "h1", "h2", "h3", "h4", "th"),
            GetProseVariant("lead", "[class~=\"lead\"]"),
            GetProseVariant("h1"),
            GetProseVariant("h2"),
            GetProseVariant("h3"),
            GetProseVariant("h4"),
            GetProseVariant("p"),
        ];
    }

    private (string Modifier, IVariant Variant) GetProseVariant(string modifer, params string[]? targets)
    {
        if (targets is not { Length: not 0 })
        {
            targets = [modifer];
        }

        var ns = Namespaces[0];
        var selector = string.Join(", ", targets.Select(target => $".{ns} {target}"));
        return ($"{ns}-{modifer}", new SelectorVariant(selector));
    }

    private CssRuleSet GetProseCssRuleSet(string selector, CssDeclarationList declarations)
    {
        return new CssRuleSet(WithNotProse(selector), declarations);
    }

    private CssSettings GetDefaultSettings()
    {
        return new CssSettings
        {
            Css =
            {
                [CssProperties.MaxWidth] = _settings.MaxWidth,
                [CssProperties.Color] = _cssVar(CssFramework.GetVariableNameWithPrefix("prose-body")),
                [CssProperties.LineHeight] = Rounds(28 / 16m),
            },
            ChildRules =
            [
                GetProseCssRuleSet("a", [
                    (CssProperties.Color, _cssVar(_var("prose-links"))),
                    (CssProperties.TextDecoration, "underline"),
                    (CssProperties.FontWeight, "500"),
                ]),
                GetProseCssRuleSet("strong", [
                    (CssProperties.Color, _cssVar(_var("prose-bold"))),
                    (CssProperties.FontWeight, "600"),
                ]),
                GetProseCssRuleSet("ol[type=\"A\"]", [
                    (CssProperties.ListStyleType, "upper-alpha"),
                ]),
                GetProseCssRuleSet("ol[type=\"a\"]", [
                    (CssProperties.ListStyleType, "lower-alpha"),
                ]),
                GetProseCssRuleSet("ol[type=\"I\"]", [
                    (CssProperties.ListStyleType, "upper-roman"),
                ]),
                GetProseCssRuleSet("ol[type=\"i\"]", [
                    (CssProperties.ListStyleType, "lower-roman"),
                ]),
                GetProseCssRuleSet("ol[type=\"1\"]", [
                    (CssProperties.ListStyleType, "decimal"),
                ]),
                GetProseCssRuleSet("ol > li::marker", [
                    (CssProperties.FontWeight, "400"),
                    (CssProperties.Color, _cssVar(_var("prose-counters"))),
                ]),
                GetProseCssRuleSet("ul > li::marker", [
                    (CssProperties.Color, _cssVar(_var("prose-bullets"))),
                ]),
                GetProseCssRuleSet("hr", [
                    (CssProperties.BorderColor, _cssVar(_var("prose-hr"))),
                    (CssProperties.BorderTopWidth, "1px"),
                ]),
                GetProseCssRuleSet("blockquote", [
                    (CssProperties.FontWeight, "500"),
                    (CssProperties.FontStyle, "italic"),
                    (CssProperties.Color, _cssVar(_var("prose-quotes"))),
                    (CssProperties.BorderLeftWidth, "0.25rem"),
                    (CssProperties.BorderLeftColor, _cssVar(_var("prose-quote-borders"))),
                    (CssProperties.Quotes, "\"\\201C\"\"\\201D\"\"\\2018\"\"\\2019\""),
                ]),
                GetProseCssRuleSet("blockquote p:first-of-type::before", [
                    (CssProperties.Content, "open-quote"),
                ]),
                GetProseCssRuleSet("blockquote p:last-of-type::after", [
                    (CssProperties.Content, "close-quote"),
                ]),
                GetProseCssRuleSet("h1", [
                    (CssProperties.Color, _cssVar(_var("prose-headings"))),
                    (CssProperties.FontWeight, "800"),
                ]),
                GetProseCssRuleSet("h2", [
                    (CssProperties.Color, _cssVar(_var("prose-headings"))),
                    (CssProperties.FontWeight, "700"),
                ]),
                GetProseCssRuleSet("h3", [
                    (CssProperties.Color, _cssVar(_var("prose-headings"))),
                    (CssProperties.FontWeight, "600"),
                ]),
                GetProseCssRuleSet("h4", [
                    (CssProperties.Color, _cssVar(_var("prose-headings"))),
                    (CssProperties.FontWeight, "600"),
                ]),
                GetProseCssRuleSet("figure > *", [
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("figcaption", [
                    (CssProperties.Color, _cssVar(_var("prose-captions"))),
                ]),
                GetProseCssRuleSet("kbd", [
                    (CssProperties.FontWeight, "500"),
                    (CssProperties.FontFamily, "inherit"),
                    (CssProperties.Color, _cssVar(_var("prose-kbd"))),
                    (CssProperties.BoxShadow, _cssVar(_var("prose-kbd-shadows"))),
                ]),
                GetProseCssRuleSet("code", [
                    (CssProperties.Color, _cssVar(_var("prose-code"))),
                    (CssProperties.FontWeight, "600"),
                ]),
                GetProseCssRuleSet("code::before", [
                    (CssProperties.Content, "`"),
                ]),
                GetProseCssRuleSet("code::after", [
                    (CssProperties.Content, "`"),
                ]),
                GetProseCssRuleSet("a code", [
                    (CssProperties.Color, _cssVar(_var("prose-links"))),
                ]),
                GetProseCssRuleSet("pre", [
                    (CssProperties.Color, _cssVar(_var("prose-pre-code"))),
                    (CssProperties.BackgroundColor, _cssVar(_var("prose-pre-bg"))),
                    (CssProperties.OverflowX, "auto"),
                    (CssProperties.FontWeight, "400"),
                ]),
                GetProseCssRuleSet("pre code", [
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
                GetProseCssRuleSet("pre code::before", [
                    (CssProperties.Content, "none"),
                ]),
                GetProseCssRuleSet("pre code::after", [
                    (CssProperties.Content, "none"),
                ]),
                GetProseCssRuleSet("table", [
                    (CssProperties.Width, "100%"),
                    (CssProperties.TableLayout, "auto"),
                    (CssProperties.TextAlign, "left"),
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
                GetProseCssRuleSet("thead", [
                    (CssProperties.BorderBottomWidth, "1px"),
                    (CssProperties.BorderBottomColor, _cssVar(_var("prose-th-borders"))),
                ]),
                GetProseCssRuleSet("thead th", [
                    (CssProperties.Color, _cssVar(_var("prose-headings"))),
                    (CssProperties.FontWeight, "600"),
                    (CssProperties.VerticalAlign, "bottom"),
                ]),
                GetProseCssRuleSet("tbody tr", [
                    (CssProperties.BorderBottomWidth, "1px"),
                    (CssProperties.BorderBottomColor, _cssVar(_var("prose-td-borders"))),
                ]),
                GetProseCssRuleSet("tbody tr:last-child", [
                    (CssProperties.BorderBottomWidth, "0"),
                ]),
                GetProseCssRuleSet("tbody td", [
                    (CssProperties.VerticalAlign, "baseline"),
                ]),
                GetProseCssRuleSet("tfoot", [
                    (CssProperties.BorderTopWidth, "1px"),
                    (CssProperties.BorderTopColor, _cssVar(_var("prose-th-borders"))),
                ]),
                GetProseCssRuleSet("tfoot td", [
                    (CssProperties.VerticalAlign, "top"),
                ]),
                GetProseCssRuleSet("img, video, figure", [
                    (CssProperties.MaxWidth, "100%"),
                    (CssProperties.Height, "auto"),
                ]),
                GetProseCssRuleSet("img", [
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
                GetProseCssRuleSet("picture", [
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                ]),
                GetProseCssRuleSet("picture > img", [
                    (CssProperties.MarginTop, "0"),
                    (CssProperties.MarginBottom, "0"),
                ]),
                GetProseCssRuleSet("video", [
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
            { "base", GetBaseCssSettings() },
            { "sm", GetSmCssSettings() },
            { "lg", GetLgCssSettings() },
            { "xl", GetXlCssSettings() },
            { "2xl", Get2XlCssSettings() },
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