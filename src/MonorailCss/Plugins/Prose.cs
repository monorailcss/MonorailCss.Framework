using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Variants;
using static MonorailCss.Plugins.ModifierSettings;

namespace MonorailCss.Plugins;

/// <summary>
/// Represents a typographical plugin.
/// </summary>
public class Prose : IUtilityNamespacePlugin, IVariantPluginProvider
{
    /// <inheritdoc />
    public class Settings : ISettings<Prose>
    {
        /// <summary>
        /// Gets the namespace to be used for CSS classes.
        /// </summary>
        public string Namespace { get; init; } = "prose";

        /// <summary>
        /// Gets the names of the colors to create a gray scale prose modifiers.
        /// </summary>
        public string[] GrayScales { get; init; } = new[] { ColorNames.Gray, };

        /// <summary>
        /// Gets ths custom settings to apply to the default settings..
        /// </summary>
        public Func<DesignSystem, ImmutableDictionary<string, CssSettings>> CustomSettings { get; init; } =
            _ => ImmutableDictionary<string, CssSettings>.Empty;
    }

    private readonly DesignSystem _designSystem;
    private readonly CssFramework _framework;
    private readonly Settings _settings;

    private readonly ImmutableDictionary<string, CssSettings> _configuredSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Prose"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="framework">The framework.</param>
    /// <param name="settings">The settings.</param>
    public Prose(DesignSystem designSystem, CssFramework framework, Settings settings)
    {
        _designSystem = designSystem;
        _framework = framework;
        _settings = settings;

        var defaultSettings = DefaultSettings() + StandardSettings["base"] + StandardSettings[ColorNames.Gray];

        var customSettings = _settings.CustomSettings(_designSystem);

        _configuredSettings = StandardSettings.Add("DEFAULT", defaultSettings);
        foreach (var customSetting in customSettings)
        {
            _configuredSettings = _configuredSettings.ContainsKey(customSetting.Key)
                ? _configuredSettings.SetItem(
                    customSetting.Key,
                    _configuredSettings[customSetting.Key] + customSetting.Value)
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

        if (!_configuredSettings.ContainsKey(suffix))
        {
            yield break;
        }

        var settings = _configuredSettings[suffix];
        yield return new CssRuleSet(syntax.OriginalSyntax, settings.Css);

        foreach (var childRule in settings.ChildRules)
        {
            yield return childRule with { Selector = $"{syntax.OriginalSyntax} {childRule.Selector}", };
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { _settings.Namespace, }.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<(string Modifier, IVariant Variant)> GetVariants()
    {
        return new[]
        {
            GetProseVariant("headings", "h1", "h2", "h3", "h4", "th"),
            GetProseVariant("lead", "[class~=\"lead\"]"),
            GetProseVariant("h1"),
            GetProseVariant("h2"),
            GetProseVariant("h3"),
            GetProseVariant("h4"),
            GetProseVariant("p"),
        };
    }

    private (string Modifier, IVariant Variant) GetProseVariant(string modifer, params string[]? targets)
    {
        if (targets is not { Length: not 0 })
        {
            targets = new[] { modifer, };
        }

        var ns = Namespaces[0];
        var selector = string.Join(", ", targets.Select(target => $".{ns} {target}"));
        return ($"{ns}-{modifer}", new SelectorVariant(selector));
    }

    private CssSettings DefaultSettings()
    {
        var defaultSettings = new CssSettings()
        {
            Css =
                new CssDeclarationList { new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-body")), },
            ChildRules = new CssRuleSetList()
            {
                new(
                    "a",
                    new CssDeclarationList
                    {
                        new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-links")),
                        new(CssProperties.TextDecoration, "underline"),
                    }),
                new("pre", new CssDeclarationList()
                {
                    new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-pre-code")),
                    new(CssProperties.BackgroundColor, _framework.GetCssVariableWithPrefix("prose-pre-bg")),
                    new("overflow-x", "auto"),
                    new(CssProperties.FontWeight, "300"),
                }),
                new("pre code", new CssDeclarationList()
                {
                    new(CssProperties.BackgroundColor, "transparent"),
                    new(CssProperties.BorderWidth, "0"),
                    new(CssProperties.BorderRadius, "0"),
                    new(CssProperties.Padding, "0"),
                    new(CssProperties.FontWeight, "inherit"),
                    new(CssProperties.Color, "inherit"),
                    new(CssProperties.FontSize, "inherit"),
                    new(CssProperties.FontFamily, "inherit"),
                    new(CssProperties.LineHeight, "inherit"),
                }),
                new("ol", new CssDeclarationList { new(CssProperties.ListStyleType, "decimal") }),
                new(
                    "ol[type=\"A\"]",
                    new CssDeclarationList { new(CssProperties.ListStyleType, "upper-alpha") }),
                new(
                    "ol[type=\"a\"]",
                    new CssDeclarationList { new(CssProperties.ListStyleType, "lower-alpha") }),
                new(
                    "ol[type=\"I\"]",
                    new CssDeclarationList { new(CssProperties.ListStyleType, "upper-roman") }),
                new(
                    "ol[type=\"i\"]",
                    new CssDeclarationList { new(CssProperties.ListStyleType, "lower-roman") }),
                new(
                    "ol[type=\"1\"]",
                    new CssDeclarationList { new(CssProperties.ListStyleType, "decimal") }),
                new(
                    "ul", new CssDeclarationList { new(CssProperties.ListStyleType, "disc") }),
                new(
                    "h1",
                    new CssDeclarationList
                    {
                        new(CssProperties.FontWeight, "800"),
                        new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-headings")),
                    }),
                new(
                    "h1 strong",
                    new CssDeclarationList { new(CssProperties.FontWeight, "900"), }),
                new(
                    "h2",
                    new CssDeclarationList
                    {
                        new(CssProperties.FontWeight, "700"),
                        new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-headings")),
                    }),
                new("h2 strong", new CssDeclarationList { new(CssProperties.FontWeight, "800"), }),
                new(
                    "h3",
                    new CssDeclarationList
                    {
                        new(CssProperties.FontWeight, "600"),
                        new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-headings")),
                    }),
                new("h3 strong", new CssDeclarationList { new(CssProperties.FontWeight, "700"), }),
                new(
                    "h4",
                    new CssDeclarationList
                    {
                        new(CssProperties.FontWeight, "600"),
                        new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-headings")),
                    }),
                new("h4 strong", new CssDeclarationList { new(CssProperties.FontWeight, "700"), }),
                new("code", new CssDeclarationList()
                {
                    new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-code")),
                    new(CssProperties.FontWeight, "600"),
                }),
                new("table", new CssDeclarationList()
                {
                    new(CssProperties.Width, "100%"),
                    new(CssProperties.TableLayout, "auto"),
                    new(CssProperties.TextAlign, "left"),
                    new(CssProperties.MarginTop, Em(32, 16)),
                    new(CssProperties.MarginBottom, Em(32, 16)),
                }),
                new("thead", new CssDeclarationList()
                {
                    new(CssProperties.BorderBottomColor, _framework.GetCssVariableWithPrefix("prose-th-borders")),
                    new(CssProperties.BorderBottomWidth, "1px"),
                }),
                new("thead th", new CssDeclarationList()
                {
                    new(CssProperties.Color, _framework.GetCssVariableWithPrefix("prose-headings")),
                    new(CssProperties.FontWeight, "600"),
                    new(CssProperties.VerticalAlign, "bottom"),
                }),
                new("tbody tr", new CssDeclarationList()
                {
                    new(CssProperties.BorderBottomColor, _framework.GetCssVariableWithPrefix("prose-td-borders")),
                    new(CssProperties.BorderBottomWidth, "1px"),
                }),
                new(new CssSelector("tbody tr", ":last-child"), new CssDeclarationList()
                {
                    new(CssProperties.BorderBottomWidth, "0px"),
                }),
                new("tbody td", new CssDeclarationList()
                {
                    new(CssProperties.VerticalAlign, "baseline"),
                }),
                new("tfoot", new CssDeclarationList()
                {
                    new(CssProperties.BorderTopWidth, "1px"),
                    new(CssProperties.BorderTopColor, _framework.GetCssVariableWithPrefix("prose-th-borders")),
                }),
            },
        };
        return defaultSettings;
    }

    private ImmutableDictionary<string, CssSettings> StandardSettings
    {
        get
        {
            var baseSettings = new Dictionary<string, CssSettings>
            {
                {
                    "base", new CssSettings()
                    {
                        Css =
                            new CssDeclarationList
                            {
                                new(CssProperties.FontSize, Rem(16)), new(CssProperties.LineHeight, Rounds(28 / 18m)),
                            },
                        ChildRules = new CssRuleSetList()
                        {
                            new(
                                "p",
                                new CssDeclarationList
                                {
                                    new(CssProperties.MarginTop, Em(20, 16)),
                                    new(CssProperties.MarginBottom, Em(20, 16)),
                                }),
                            new(
                                "h1", new CssDeclarationList
                                {
                                    new(CssProperties.FontSize, Em(36, 16)),
                                    new(CssProperties.MarginTop, "0"),
                                    new(CssProperties.MarginBottom, Em(32, 36)),
                                    new(CssProperties.LineHeight, Rounds(40 / 36m)),
                                }),
                            new("h2", new CssDeclarationList
                            {
                                new(CssProperties.FontSize, Em(24, 16)),
                                new(CssProperties.MarginTop, Em(48, 24)),
                                new(CssProperties.MarginBottom, Em(12, 20)),
                                new(CssProperties.LineHeight, Rounds(32 / 24m)),
                            }),
                            new("h3", new CssDeclarationList
                            {
                                new(CssProperties.FontSize, Em(20, 16)),
                                new(CssProperties.MarginTop, Em(32, 20)),
                                new(CssProperties.MarginBottom, Em(12, 20)),
                                new(CssProperties.LineHeight, Rounds(32 / 20m)),
                            }),
                            new(
                                "h4",
                                new CssDeclarationList
                                {
                                    new(CssProperties.MarginTop, Em(24, 16)),
                                    new(CssProperties.MarginBottom, Em(8, 16)),
                                    new(CssProperties.LineHeight, Rounds(24 / 16m)),
                                }),
                            new("pre", new CssDeclarationList
                            {
                                new(CssProperties.FontSize, Em(14, 16)),
                                new(CssProperties.LineHeight, Rounds(24 / 14m)),
                                new(CssProperties.MarginTop, Em(24, 14)),
                                new(CssProperties.MarginBottom, Em(24, 14)),
                                new(CssProperties.PaddingTop, Em(12, 14)),
                                new(CssProperties.PaddingBottom, Em(12, 14)),
                                new(CssProperties.PaddingLeft, Em(16, 14)),
                                new(CssProperties.PaddingRight, Em(16, 14)),
                            }),
                            new(
                                "ol",
                                new CssDeclarationList
                                {
                                    new(CssProperties.MarginTop, Em(20, 16)),
                                    new(CssProperties.MarginBottom, Em(20, 16)),
                                    new(CssProperties.PaddingLeft, Em(20, 16)),
                                }),
                            new(
                                "ul",
                                new CssDeclarationList
                                {
                                    new(CssProperties.MarginTop, Em(20, 16)),
                                    new(CssProperties.MarginBottom, Em(20, 16)),
                                    new(CssProperties.PaddingLeft, Em(20, 16)),
                                }),
                            new("ol > li", new CssDeclarationList { new(CssProperties.PaddingLeft, Em(6, 16)) }),
                            new("ul > li", new CssDeclarationList { new(CssProperties.PaddingLeft, Em(6, 16)) }),
                        },
                    }
                },
                {
                    "invert", new CssSettings
                    {
                        Css = new CssDeclarationList
                        {
                            new(
                                _framework.GetVariableNameWithPrefix("prose-body"),
                                _framework.GetCssVariableWithPrefix("prose-invert-body")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-links"),
                                _framework.GetCssVariableWithPrefix("prose-invert-links")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-headings"),
                                _framework.GetCssVariableWithPrefix("prose-invert-headings")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-code"),
                                _framework.GetCssVariableWithPrefix("prose-invert-code")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-pre-code"),
                                _framework.GetCssVariableWithPrefix("prose-invert-pre-code")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-pre-bg"),
                                _framework.GetCssVariableWithPrefix("prose-invert-pre-bg")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-th-borders"),
                                _framework.GetCssVariableWithPrefix("prose-invert-th-borders")),
                            new(
                                _framework.GetVariableNameWithPrefix("prose-td-borders"),
                                _framework.GetCssVariableWithPrefix("prose-invert-td-borders")),
                        },
                    }
                },
                {
                    "sm",
                    new CssSettings
                    {
                        Css = new CssDeclarationList
                        {
                            new(CssProperties.FontSize, Rem(14)), new(CssProperties.LineHeight, Rounds(24 / 14m)),
                        },
                    }
                },
            }.ToImmutableDictionary();

            return _settings.GrayScales.Aggregate(baseSettings, (current, scale) => new Dictionary<string, CssSettings>
                {
                    {
                        scale, new CssSettings()
                        {
                            Css = new CssDeclarationList
                            {
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-body"),
                                    _designSystem.Colors[scale][ColorLevels._700].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-headings"),
                                    _designSystem.Colors[scale][ColorLevels._900].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-links"),
                                    _designSystem.Colors[scale][ColorLevels._900].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-code"),
                                    _designSystem.Colors[scale][ColorLevels._900].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-pre-code"),
                                    _designSystem.Colors[scale][ColorLevels._200].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-pre-bg"),
                                    _designSystem.Colors[scale][ColorLevels._800].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-th-borders"),
                                    _designSystem.Colors[scale][ColorLevels._300].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-td-borders"),
                                    _designSystem.Colors[scale][ColorLevels._200].AsRgb()),

                                // inverts
                                new(_framework.GetVariableNameWithPrefix("prose-invert-body"), _designSystem.Colors[scale][ColorLevels._300].AsRgb()),
                                new(_framework.GetVariableNameWithPrefix("prose-invert-headings"), "white"),
                                new(_framework.GetVariableNameWithPrefix("prose-invert-links"), "white"),
                                new(_framework.GetVariableNameWithPrefix("prose-invert-code"), "white"),
                                new(_framework.GetVariableNameWithPrefix("prose-invert-pre-code"), _designSystem.Colors[scale][ColorLevels._300].AsRgb()),
                                new(_framework.GetVariableNameWithPrefix("prose-invert-pre-bg"), "rgb(0 0 0 / 50%)"),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-invert-th-borders"),
                                    _designSystem.Colors[scale][ColorLevels._600].AsRgb()),
                                new(
                                    _framework.GetVariableNameWithPrefix("prose-invert-td-borders"),
                                    _designSystem.Colors[scale][ColorLevels._700].AsRgb()),
                            },
                        }
                    },
                }.ToImmutableDictionary()
                .AddRange(current));
        }
    }
}