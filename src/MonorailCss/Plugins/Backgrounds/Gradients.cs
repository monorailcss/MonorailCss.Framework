using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// The background-image plugin.
/// </summary>
public class BackgroundImage : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        [new("bg", CssProperties.BackgroundImage)];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        var stopsVar = CssFramework.GetVariableNameWithPrefix("gradient-stops");
        return new CssSuffixToValueMap
        {
            { "none", "none" },
            { "gradient-to-t", $"linear-gradient(to top, var({stopsVar}))" },
            { "gradient-to-tr", $"linear-gradient(to top right, var({stopsVar}))" },
            { "gradient-to-r", $"linear-gradient(to right, var({stopsVar}))" },
            { "gradient-to-br", $"linear-gradient(to bottom right, var({stopsVar}))" },
            { "gradient-to-b", $"linear-gradient(to bottom, var({stopsVar}))" },
            { "gradient-to-bl", $"linear-gradient(to bottom left, var({stopsVar}))" },
            { "gradient-to-l", $"linear-gradient(to left, var({stopsVar}))" },
            { "gradient-to-tl", $"linear-gradient(to top left, var({stopsVar}))" },
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
            (fromVar, colorValue), (stopsVar, $"var({fromVar}), var({toVar}, {colorValue})"),
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
/// The gradient-from plugin.
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
            (stopsVar, $"var({fromVar}), {colorValue}, var({toVar}, {colorValue});"),
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