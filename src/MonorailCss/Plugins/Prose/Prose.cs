using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;
using MonorailCss.Variants;
using static MonorailCss.Plugins.ModifierSettings;

namespace MonorailCss.Plugins.Prose;

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
    private readonly Settings _settings;

    private readonly Func<string, string> _cssVar = CssFramework.GetCssVariableWithPrefix;
    private readonly Func<string, string> _var = CssFramework.GetVariableNameWithPrefix;

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
        _settings = settings;

        var standardSettings = GetStandardSettings();
        var defaultSettings = GetDefaultSettings() + standardSettings["base"] + standardSettings[ColorNames.Gray];

        var customSettings = _settings.CustomSettings(_designSystem);

        _configuredSettings = standardSettings.Add("DEFAULT", defaultSettings);
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
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // this might not fit the mold...
        yield break;
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

    private CssSettings GetDefaultSettings()
    {
        var defaultSettings = new CssSettings
        {
            Css =
                new CssDeclarationList { (CssProperties.Color, _cssVar("prose-body")), },
            ChildRules = new CssRuleSetList
            {
                new(
                    "a",
                    new CssDeclarationList
                    {
                        (CssProperties.Color, _cssVar("prose-links")),
                        (CssProperties.TextDecoration, "underline"),
                    }),
                new("pre", new CssDeclarationList
                {
                    (CssProperties.Color, _cssVar("prose-pre-code")),
                    (CssProperties.BackgroundColor, _cssVar("prose-pre-bg")),
                    ("overflow-x", "auto"),
                    (CssProperties.FontWeight, "300"),
                }),
                new("pre code", new CssDeclarationList
                {
                    (CssProperties.BackgroundColor, "transparent"),
                    (CssProperties.BorderWidth, "0"),
                    (CssProperties.BorderRadius, "0"),
                    (CssProperties.Padding, "0"),
                    (CssProperties.FontWeight, "inherit"),
                    (CssProperties.Color, "inherit"),
                    (CssProperties.FontSize, "inherit"),
                    (CssProperties.FontFamily, "inherit"),
                    (CssProperties.LineHeight, "inherit"),
                }),
                new("ol", new CssDeclarationList { (CssProperties.ListStyleType, "decimal") }),
                new(
                    "ol[type=\"A\"]",
                    new CssDeclarationList { (CssProperties.ListStyleType, "upper-alpha") }),
                new(
                    "ol[type=\"a\"]",
                    new CssDeclarationList { (CssProperties.ListStyleType, "lower-alpha") }),
                new(
                    "ol[type=\"I\"]",
                    new CssDeclarationList { (CssProperties.ListStyleType, "upper-roman") }),
                new(
                    "ol[type=\"i\"]",
                    new CssDeclarationList { (CssProperties.ListStyleType, "lower-roman") }),
                new(
                    "ol[type=\"1\"]",
                    new CssDeclarationList { (CssProperties.ListStyleType, "decimal") }),
                new(
                    "ul", new CssDeclarationList { (CssProperties.ListStyleType, "disc") }),
                new(
                    "h1",
                    new CssDeclarationList
                    {
                        (CssProperties.FontWeight, "800"),
                        (CssProperties.Color, _cssVar("prose-headings")),
                    }),
                new(
                    "h1 strong",
                    new CssDeclarationList { (CssProperties.FontWeight, "900"), }),
                new(
                    "h2",
                    new CssDeclarationList
                    {
                        (CssProperties.FontWeight, "700"),
                        (CssProperties.Color, _cssVar("prose-headings")),
                    }),
                new("h2 strong", new CssDeclarationList { (CssProperties.FontWeight, "800"), }),
                new(
                    "h3",
                    new CssDeclarationList
                    {
                        (CssProperties.FontWeight, "600"),
                        (CssProperties.Color, _cssVar("prose-headings")),
                    }),
                new("h3 strong", new CssDeclarationList { (CssProperties.FontWeight, "700"), }),
                new(
                    "h4",
                    new CssDeclarationList
                    {
                        (CssProperties.FontWeight, "600"),
                        (CssProperties.Color, _cssVar("prose-headings")),
                    }),
                new("h4 strong", new CssDeclarationList { (CssProperties.FontWeight, "700"), }),
                new("code", new CssDeclarationList
                {
                    (CssProperties.Color, _cssVar("prose-code")),
                    (CssProperties.FontWeight, "600"),
                }),
                new("table", new CssDeclarationList
                {
                    (CssProperties.Width, "100%"),
                    (CssProperties.TableLayout, "auto"),
                    (CssProperties.TextAlign, "left"),
                    (CssProperties.MarginTop, Em(32, 16)),
                    (CssProperties.MarginBottom, Em(32, 16)),
                }),
                new("thead", new CssDeclarationList
                {
                    (CssProperties.BorderBottomColor, _cssVar("prose-th-borders")),
                    (CssProperties.BorderBottomWidth, "1px"),
                }),
                new("thead th", new CssDeclarationList
                {
                    (CssProperties.Color, _cssVar("prose-headings")),
                    (CssProperties.FontWeight, "600"),
                    (CssProperties.VerticalAlign, "bottom"),
                }),
                new("tbody tr", new CssDeclarationList
                {
                    (CssProperties.BorderBottomColor, _cssVar("prose-td-borders")),
                    (CssProperties.BorderBottomWidth, "1px"),
                }),
                new(new CssSelector("tbody tr", ":last-child"), new CssDeclarationList
                {
                    (CssProperties.BorderBottomWidth, "0px"),
                }),
                new("tbody td", new CssDeclarationList
                {
                    (CssProperties.VerticalAlign, "baseline"),
                }),
                new("tfoot", new CssDeclarationList
                {
                    (CssProperties.BorderTopWidth, "1px"),
                    (CssProperties.BorderTopColor, _cssVar("prose-th-borders")),
                }),
            },
        };
        return defaultSettings;
    }

    private ImmutableDictionary<string, CssSettings> GetStandardSettings()
    {
        var baseSettings = new Dictionary<string, CssSettings>
        {
            {
                "base", new CssSettings
                {
                    Css =
                        new CssDeclarationList
                        {
                            (CssProperties.FontSize, Rem(16)), (CssProperties.LineHeight, Rounds(28 / 18m)),
                        },
                    ChildRules = new CssRuleSetList
                    {
                        new(
                            "p",
                            new CssDeclarationList
                            {
                                (CssProperties.MarginTop, Em(20, 16)),
                                (CssProperties.MarginBottom, Em(20, 16)),
                            }),
                        new(
                            "h1", new CssDeclarationList
                            {
                                (CssProperties.FontSize, Em(36, 16)),
                                (CssProperties.MarginTop, "0"),
                                (CssProperties.MarginBottom, Em(32, 36)),
                                (CssProperties.LineHeight, Rounds(40 / 36m)),
                            }),
                        new("h2", new CssDeclarationList
                        {
                            (CssProperties.FontSize, Em(24, 16)),
                            (CssProperties.MarginTop, Em(48, 24)),
                            (CssProperties.MarginBottom, Em(12, 20)),
                            (CssProperties.LineHeight, Rounds(32 / 24m)),
                        }),
                        new("h3", new CssDeclarationList
                        {
                            (CssProperties.FontSize, Em(20, 16)),
                            (CssProperties.MarginTop, Em(32, 20)),
                            (CssProperties.MarginBottom, Em(12, 20)),
                            (CssProperties.LineHeight, Rounds(32 / 20m)),
                        }),
                        new(
                            "h4",
                            new CssDeclarationList
                            {
                                (CssProperties.MarginTop, Em(24, 16)),
                                (CssProperties.MarginBottom, Em(8, 16)),
                                (CssProperties.LineHeight, Rounds(24 / 16m)),
                            }),
                        new("pre", new CssDeclarationList
                        {
                            (CssProperties.FontSize, Em(14, 16)),
                            (CssProperties.LineHeight, Rounds(24 / 14m)),
                            (CssProperties.MarginTop, Em(24, 14)),
                            (CssProperties.MarginBottom, Em(24, 14)),
                            (CssProperties.PaddingTop, Em(12, 14)),
                            (CssProperties.PaddingBottom, Em(12, 14)),
                            (CssProperties.PaddingLeft, Em(16, 14)),
                            (CssProperties.PaddingRight, Em(16, 14)),
                        }),
                        new(
                            "ol",
                            new CssDeclarationList
                            {
                                (CssProperties.MarginTop, Em(20, 16)),
                                (CssProperties.MarginBottom, Em(20, 16)),
                                (CssProperties.PaddingLeft, Em(20, 16)),
                            }),
                        new(
                            "ul",
                            new CssDeclarationList
                            {
                                (CssProperties.MarginTop, Em(20, 16)),
                                (CssProperties.MarginBottom, Em(20, 16)),
                                (CssProperties.PaddingLeft, Em(20, 16)),
                            }),
                        new("ol > li", new CssDeclarationList { (CssProperties.PaddingLeft, Em(6, 16)) }),
                        new("ul > li", new CssDeclarationList { (CssProperties.PaddingLeft, Em(6, 16)) }),
                    },
                }
            },
            {
                "invert", new CssSettings
                {
                    Css = new CssDeclarationList
                    {
                        (
                            _var("prose-body"),
                            _cssVar("prose-invert-body")),
                        (
                            _var("prose-links"),
                            _cssVar("prose-invert-links")),
                        (
                            _var("prose-headings"),
                            _cssVar("prose-invert-headings")),
                        (
                            _var("prose-code"),
                            _cssVar("prose-invert-code")),
                        (
                            _var("prose-pre-code"),
                            _cssVar("prose-invert-pre-code")),
                        (
                            _var("prose-pre-bg"),
                            _cssVar("prose-invert-pre-bg")),
                        (
                            _var("prose-th-borders"),
                            _cssVar("prose-invert-th-borders")),
                        (
                            _var("prose-td-borders"),
                            _cssVar("prose-invert-td-borders")),
                    },
                }
            },
            {
                "sm",
                new CssSettings
                {
                    Css = new CssDeclarationList
                    {
                        (CssProperties.FontSize, Rem(14)), (CssProperties.LineHeight, Rounds(24 / 14m)),
                    },
                }
            },
            {
                "lg",
                new CssSettings
                {
                    Css = new CssDeclarationList
                    {
                        (CssProperties.FontSize, Rem(18)), (CssProperties.LineHeight, Rounds(32 / 18m)),
                    },
                }
            },
            {
                "xl",
                new CssSettings
                {
                    Css = new CssDeclarationList
                    {
                        (CssProperties.FontSize, Rem(20)), (CssProperties.LineHeight, Rounds(36 / 20m)),
                    },
                }
            },
            {
                "2xl",
                new CssSettings
                {
                    Css = new CssDeclarationList
                    {
                        (CssProperties.FontSize, Rem(24)), (CssProperties.LineHeight, Rounds(40 / 24m)),
                    },
                }
            },
        }.ToImmutableDictionary();

        return _settings.GrayScales.Aggregate(baseSettings, (current, scale) => new Dictionary<string, CssSettings>
            {
                {
                    scale, new CssSettings
                    {
                        Css = new CssDeclarationList
                        {
                            (
                                _var("prose-body"),
                                _designSystem.Colors[scale][ColorLevels._700].AsString()),
                            (
                                _var("prose-headings"),
                                _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (
                                _var("prose-links"),
                                _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (
                                _var("prose-code"),
                                _designSystem.Colors[scale][ColorLevels._900].AsString()),
                            (
                                _var("prose-pre-code"),
                                _designSystem.Colors[scale][ColorLevels._200].AsString()),
                            (
                                _var("prose-pre-bg"),
                                _designSystem.Colors[scale][ColorLevels._800].AsString()),
                            (
                                _var("prose-th-borders"),
                                _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (
                                _var("prose-td-borders"),
                                _designSystem.Colors[scale][ColorLevels._200].AsString()),

                            // inverts
                            (
                                _var("prose-invert-body"),
                                _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (_var("prose-invert-headings"), "white"),
                            (_var("prose-invert-links"), "white"),
                            (_var("prose-invert-code"), "white"),
                            (
                                _var("prose-invert-pre-code"),
                                _designSystem.Colors[scale][ColorLevels._300].AsString()),
                            (_var("prose-invert-pre-bg"), "rgb(0 0 0 / 50%)"),
                            (
                                _var("prose-invert-th-borders"),
                                _designSystem.Colors[scale][ColorLevels._600].AsString()),
                            (
                                _var("prose-invert-td-borders"),
                                _designSystem.Colors[scale][ColorLevels._700].AsString()),
                        },
                    }
                },
            }.ToImmutableDictionary()
            .AddRange(current));
    }
}