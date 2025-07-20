using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// The background-image plugin.
/// </summary>
public class BackgroundImage : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when arbitraryValueSyntax.Namespace.Equals("bg"):
                {
                    // Only handle non-color arbitrary values for background-image
                    var value = arbitraryValueSyntax.ArbitraryValue;

                    // Check if it's a background-image value (url, gradient, etc.)
                    if (IsBackgroundImageValue(value))
                    {
                        var declarations = new CssDeclarationList
                        {
                            (CssProperties.BackgroundImage, value),
                        };
                        yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, declarations);
                    }

                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals("bg") && namespaceSyntax.Suffix != null:
                {
                    // Handle predefined gradient values
                    var suffix = namespaceSyntax.Suffix;
                    var value = GetGradientValue(suffix);
                    if (value != null)
                    {
                        var declarations = new CssDeclarationList
                        {
                            (CssProperties.BackgroundImage, value),
                        };

                        // Add gradient position variable for new syntax
                        var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-position");
                        var gradientPosition = GetGradientPosition(suffix);
                        if (gradientPosition != null)
                        {
                            declarations[positionVar] = gradientPosition;
                        }

                        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
                    }

                    break;
                }

            default:
                yield break;
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        var stopsVar = CssFramework.GetVariableNameWithPrefix("gradient-stops");
        var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-position");

        // Linear gradients
        var linearGradients = new Dictionary<string, string>
        {
            { "bg-linear-to-t", "to top" },
            { "bg-linear-to-tr", "to top right" },
            { "bg-linear-to-r", "to right" },
            { "bg-linear-to-br", "to bottom right" },
            { "bg-linear-to-b", "to bottom" },
            { "bg-linear-to-bl", "to bottom left" },
            { "bg-linear-to-l", "to left" },
            { "bg-linear-to-tl", "to top left" },
        };

        foreach (var gradient in linearGradients)
        {
            var declarations = new CssDeclarationList
            {
                (positionVar, gradient.Value),
                (CssProperties.BackgroundImage, $"linear-gradient(var({positionVar}), var({stopsVar}))"),
            };
            yield return new CssRuleSet(gradient.Key, declarations);
        }

        // Radial gradients
        var radialDeclarations = new CssDeclarationList
        {
            (positionVar, "ellipse at center"),
            (CssProperties.BackgroundImage, $"radial-gradient(var({positionVar}), var({stopsVar}))"),
        };
        yield return new CssRuleSet("bg-radial", radialDeclarations);

        // Conic gradients
        var conicDeclarations = new CssDeclarationList
        {
            (positionVar, "from 0deg at center"),
            (CssProperties.BackgroundImage, $"conic-gradient(var({positionVar}), var({stopsVar}))"),
        };
        yield return new CssRuleSet("bg-conic", conicDeclarations);

        // Background none
        var noneDeclarations = new CssDeclarationList
        {
            (CssProperties.BackgroundImage, "none"),
        };
        yield return new CssRuleSet("bg-none", noneDeclarations);

        // Keep legacy gradient-to-* for backwards compatibility
        var legacyGradients = new Dictionary<string, string>
        {
            { "bg-gradient-to-t", $"linear-gradient(to top, var({stopsVar}))" },
            { "bg-gradient-to-tr", $"linear-gradient(to top right, var({stopsVar}))" },
            { "bg-gradient-to-r", $"linear-gradient(to right, var({stopsVar}))" },
            { "bg-gradient-to-br", $"linear-gradient(to bottom right, var({stopsVar}))" },
            { "bg-gradient-to-b", $"linear-gradient(to bottom, var({stopsVar}))" },
            { "bg-gradient-to-bl", $"linear-gradient(to bottom left, var({stopsVar}))" },
            { "bg-gradient-to-l", $"linear-gradient(to left, var({stopsVar}))" },
            { "bg-gradient-to-tl", $"linear-gradient(to top left, var({stopsVar}))" },
        };

        foreach (var gradient in legacyGradients)
        {
            var declarations = new CssDeclarationList
            {
                (CssProperties.BackgroundImage, gradient.Value),
            };
            yield return new CssRuleSet(gradient.Key, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg" }];

    private static bool IsBackgroundImageValue(string value)
    {
        // Check if the value is a background-image specific value
        return value.StartsWith("url(") ||
               value.StartsWith("linear-gradient(") ||
               value.StartsWith("radial-gradient(") ||
               value.StartsWith("conic-gradient(") ||
               value.StartsWith("repeating-linear-gradient(") ||
               value.StartsWith("repeating-radial-gradient(") ||
               value.Equals("none", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetGradientValue(string suffix)
    {
        var stopsVar = CssFramework.GetVariableNameWithPrefix("gradient-stops");
        var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-position");

        return suffix switch
        {
            "none" => "none",

            // New gradient syntax
            "linear-to-t" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-tr" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-r" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-br" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-b" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-bl" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-l" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "linear-to-tl" => $"linear-gradient(var({positionVar}), var({stopsVar}))",
            "radial" => $"radial-gradient(var({positionVar}), var({stopsVar}))",
            "conic" => $"conic-gradient(var({positionVar}), var({stopsVar}))",

            // Legacy gradient syntax
            "gradient-to-t" => $"linear-gradient(to top, var({stopsVar}))",
            "gradient-to-tr" => $"linear-gradient(to top right, var({stopsVar}))",
            "gradient-to-r" => $"linear-gradient(to right, var({stopsVar}))",
            "gradient-to-br" => $"linear-gradient(to bottom right, var({stopsVar}))",
            "gradient-to-b" => $"linear-gradient(to bottom, var({stopsVar}))",
            "gradient-to-bl" => $"linear-gradient(to bottom left, var({stopsVar}))",
            "gradient-to-l" => $"linear-gradient(to left, var({stopsVar}))",
            "gradient-to-tl" => $"linear-gradient(to top left, var({stopsVar}))",
            _ => null,
        };
    }

    private static string? GetGradientPosition(string suffix)
    {
        return suffix switch
        {
            // New gradient syntax positions
            "linear-to-t" => "to top",
            "linear-to-tr" => "to top right",
            "linear-to-r" => "to right",
            "linear-to-br" => "to bottom right",
            "linear-to-b" => "to bottom",
            "linear-to-bl" => "to bottom left",
            "linear-to-l" => "to left",
            "linear-to-tl" => "to top left",
            "radial" => "ellipse at center",
            "conic" => "from 0deg at center",
            _ => null,
        };
    }
}

/// <summary>
/// The gradient-from plugin.
/// </summary>
public class GradientFromPlugin : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="GradientFromPlugin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public GradientFromPlugin(DesignSystem designSystem)
    {
        _flattenColors = designSystem.GetFlattenColors();
        _opacities = designSystem.Opacities;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("from") || namespaceSyntax.Suffix == null)
        {
            yield break;
        }

        var split = ColorParser.SplitColor(namespaceSyntax.Suffix);
        if (!_flattenColors.TryGetValue(split.Color, out var color))
        {
            yield break;
        }

        if (!_opacities.TryGetValue(split.Opacity ?? "DEFAULT", out var opacity))
        {
            opacity = "1";
        }

        var declarations = GetDeclaration(color, opacity);

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    private static CssDeclarationList GetDeclaration(CssColor color, string opacity)
    {
        var colorValue = color.AsStringWithOpacity(opacity);

        var fromVar = CssFramework.GetVariableNameWithPrefix("gradient-from");
        var toVar = CssFramework.GetVariableNameWithPrefix("gradient-to");
        var stopsVar = CssFramework.GetVariableNameWithPrefix("gradient-stops");

        var declarations = new CssDeclarationList
        {
            (fromVar, colorValue),
            (stopsVar, $"var({fromVar}), var({toVar}, {colorValue})"),
        };
        return declarations;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var color in _flattenColors)
        {
            yield return new CssRuleSet($"from-{color.Key}", GetDeclaration(color.Value, "1"));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "from" }];
}

/// <summary>
/// The gradient-from plugin.
/// </summary>
public class GradientToPlugin : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="GradientToPlugin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public GradientToPlugin(DesignSystem designSystem)
    {
        _flattenColors = designSystem.GetFlattenColors();
        _opacities = designSystem.Opacities;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("to") || namespaceSyntax.Suffix == null)
        {
            yield break;
        }

        var split = ColorParser.SplitColor(namespaceSyntax.Suffix);
        if (!_flattenColors.TryGetValue(split.Color, out var color))
        {
            yield break;
        }

        if (!_opacities.TryGetValue(split.Opacity ?? "DEFAULT", out var opacity))
        {
            opacity = "1";
        }

        var declarations = GetDeclaration(color, opacity);

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    private static CssDeclarationList GetDeclaration(CssColor color, string opacity)
    {
        var colorValue = color.AsStringWithOpacity(opacity);

        var toVar = CssFramework.GetVariableNameWithPrefix("gradient-to");

        var declarations = new CssDeclarationList
        {
            (toVar, colorValue),
        };
        return declarations;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var color in _flattenColors)
        {
            yield return new CssRuleSet($"to-{color.Key}", GetDeclaration(color.Value, "1"));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "to" }];
}

/// <summary>
/// The gradient-from position plugin.
/// </summary>
public class GradientFromPositionPlugin : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("from") || namespaceSyntax.Suffix == null)
        {
            yield break;
        }

        // Check if suffix is a percentage
        if (namespaceSyntax.Suffix.EndsWith("%"))
        {
            var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-from-position");
            var declarations = new CssDeclarationList
            {
                (positionVar, namespaceSyntax.Suffix),
            };
            yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // Generate common percentages
        var percentages = new[] { "0%", "10%", "20%", "25%", "30%", "33%", "40%", "50%", "60%", "66%", "70%", "75%", "80%", "90%", "100%" };
        var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-from-position");

        foreach (var percentage in percentages)
        {
            var declarations = new CssDeclarationList
            {
                (positionVar, percentage),
            };
            yield return new CssRuleSet($"from-{percentage}", declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "from" }];
}

/// <summary>
/// The gradient-to position plugin.
/// </summary>
public class GradientToPositionPlugin : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("to") || namespaceSyntax.Suffix == null)
        {
            yield break;
        }

        // Check if suffix is a percentage
        if (namespaceSyntax.Suffix.EndsWith("%"))
        {
            var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-to-position");
            var declarations = new CssDeclarationList
            {
                (positionVar, namespaceSyntax.Suffix),
            };
            yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // Generate common percentages
        var percentages = new[] { "0%", "10%", "20%", "25%", "30%", "33%", "40%", "50%", "60%", "66%", "70%", "75%", "80%", "90%", "100%" };
        var positionVar = CssFramework.GetVariableNameWithPrefix("gradient-to-position");

        foreach (var percentage in percentages)
        {
            var declarations = new CssDeclarationList
            {
                (positionVar, percentage),
            };
            yield return new CssRuleSet($"to-{percentage}", declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "to" }];
}

/// <summary>
/// The gradient-via plugin.
/// </summary>
public class GradientViaPlugin : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="GradientViaPlugin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public GradientViaPlugin(DesignSystem designSystem)
    {
        _flattenColors = designSystem.GetFlattenColors();
        _opacities = designSystem.Opacities;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("via") || namespaceSyntax.Suffix == null)
        {
            yield break;
        }

        var split = ColorParser.SplitColor(namespaceSyntax.Suffix);
        if (!_flattenColors.TryGetValue(split.Color, out var color))
        {
            yield break;
        }

        if (!_opacities.TryGetValue(split.Opacity ?? "DEFAULT", out var opacity))
        {
            opacity = "1";
        }

        var declarations = GetDeclaration(color, opacity);

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    private static CssDeclarationList GetDeclaration(CssColor color, string opacity)
    {
        var colorValue = color.AsStringWithOpacity(opacity);

        var fromVar = CssFramework.GetVariableNameWithPrefix("gradient-from");
        var toVar = CssFramework.GetVariableNameWithPrefix("gradient-to");
        var stopsVar = CssFramework.GetVariableNameWithPrefix("gradient-stops");

        var declarations = new CssDeclarationList
        {
            (stopsVar, $"var({fromVar}), {colorValue}, var({toVar}, {colorValue})"),
        };
        return declarations;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var color in _flattenColors)
        {
            yield return new CssRuleSet($"via-{color.Key}", GetDeclaration(color.Value, "1"));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "via" }];
}