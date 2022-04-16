using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Effects;

/// <summary>
/// The ring-width plugin.
/// </summary>
public class Ring : IUtilityNamespacePlugin, IRegisterDefaults
{
    private const string Namespace = "ring";

    private readonly ImmutableDictionary<string, string> _values = new Dictionary<string, string>()
    {
        { "0", "0px" },
        { "1", "1px" },
        { "2", "2px" },
        { "DEFAULT", "3px" },
        { "4", "4px" },
        { "8", "8px" },
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace))
        {
            yield break;
        }

        if (namespaceSyntax.Suffix == "inset")
        {
            yield return new CssRuleSet(
                namespaceSyntax.OriginalSyntax,
                new CssDeclarationList() { new(CssFramework.GetVariableNameWithPrefix("ring-inset"), "inset"), });

            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
        if (!_values.TryGetValue(suffix, out var size))
        {
            yield break;
        }

        var varRingInset = CssFramework.GetCssVariableWithPrefix("ring-inset");
        var varRingOffsetWidth = CssFramework.GetCssVariableWithPrefix("ring-offset-width");
        var varRingOffsetColor = CssFramework.GetCssVariableWithPrefix("ring-offset-color");
        var varRingOffsetShadow = CssFramework.GetCssVariableWithPrefix("ring-offset-shadow");
        var varRingShadow = CssFramework.GetCssVariableWithPrefix("ring-shadow");
        var varRingColor = CssFramework.GetCssVariableWithPrefix("ring-color");
        var varShadow = CssFramework.GetCssVariableWithPrefix("shadow");

        var shadowVarDec = new CssDeclaration(
            CssFramework.GetVariableNameWithPrefix("ring-offset-shadow"),
            $"{varRingInset} 0 0 0 {varRingOffsetWidth} {varRingOffsetColor}");
        var ringVarDec = new CssDeclaration(
            CssFramework.GetVariableNameWithPrefix("ring-shadow"),
            $"{varRingInset} 0 0 0 calc({size} + {varRingOffsetWidth}) {varRingColor}");

        var boxShadowDev = new CssDeclaration(
            "box-shadow",
            $"{varRingOffsetShadow}, {varRingShadow}, {varShadow}");

        yield return new CssRuleSet(
            namespaceSyntax.OriginalSyntax,
            new CssDeclarationList { shadowVarDec, ringVarDec, boxShadowDev, });
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // todo - return all the rules.
        yield break;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { Namespace }.ToImmutableArray();

    /// <inheritdoc />
    public CssDeclarationList GetDefaults()
    {
        return new CssDeclarationList()
        {
            new(CssFramework.GetVariableNameWithPrefix("ring-inset"), string.Empty),
            new(CssFramework.GetVariableNameWithPrefix("ring-offset-width"), "0px"),
            new(CssFramework.GetVariableNameWithPrefix("ring-offset-color"), "#fff"),
            new(CssFramework.GetVariableNameWithPrefix("ring-color"), "rgb(59 130 246 / 0.5)"),
            new(CssFramework.GetVariableNameWithPrefix("ring-offset-shadow"), "0 0 #0000"),
            new(CssFramework.GetVariableNameWithPrefix("ring-shadow"), "0 0 #0000"),
            new(CssFramework.GetVariableNameWithPrefix("shadow"), "0 0 #0000"),
            new(CssFramework.GetVariableNameWithPrefix("shadow-colored"), "0 0 #0000"),
        };
    }
}

/// <summary>
/// The ring-offset plugin.
/// </summary>
public class RingOffset : BaseUtilityNamespacePlugin
{
    private const string Namespace = "ring-offset";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new()
    {
        new(Namespace, CssFramework.GetVariableNameWithPrefix("ring-offset-width")),
    };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new CssSuffixToValueMap()
        {
            { "0", "0px" },
            { "1", "1px" },
            { "2", "2px" },
            { "4", "4px" },
            { "8", "8px" },
        };
    }
}

/// <summary>
/// The ring-color plugin.
/// </summary>
public class RingColor : IUtilityNamespacePlugin
{
    private const string Namespace = "ring";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="RingColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="cssFramework">The theme.</param>
    public RingColor(DesignSystem designSystem, CssFramework cssFramework)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacities = designSystem.Opacities;
    }

    /// <inheritdoc/>
    public ImmutableArray<string> Namespaces => new[] { Namespace, }.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace) ||
            namespaceSyntax.Suffix == default)
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix;

        var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            yield break;
        }

        var property = CssFramework.GetVariableNameWithPrefix("ring-color");

        CssDeclarationList declarations;
        if (opacityValue != default)
        {
            var opacity = _opacities.GetValueOrDefault(opacityValue, "1");
            declarations = new CssDeclarationList { new(property, color.AsRgbWithOpacity(opacity)), };
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            var varName = CssFramework.GetVariableNameWithPrefix("ring-opacity");
            declarations = new CssDeclarationList
            {
                new(varName, "1"), new(property, color.AsRgbWithOpacity($"var({varName})")),
            };
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color =>
            new CssRuleSet("ring-" + color.Key, new CssDeclarationList { new("color", color.Value.AsRgb()), }));
    }
}

/// <summary>
/// The ring-color plugin.
/// </summary>
public class RingOffsetColor : IUtilityNamespacePlugin
{
    private const string Namespace = "ring-offset";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="RingOffsetColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="cssFramework">The theme.</param>
    public RingOffsetColor(DesignSystem designSystem, CssFramework cssFramework)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacities = designSystem.Opacities;
    }

    /// <inheritdoc/>
    public ImmutableArray<string> Namespaces => new[] { Namespace, }.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace) ||
            namespaceSyntax.Suffix == default)
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix;

        var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            yield break;
        }

        var property = CssFramework.GetVariableNameWithPrefix("ring-offset-color");

        CssDeclarationList declarations;
        if (opacityValue != default)
        {
            var opacity = _opacities.GetValueOrDefault(opacityValue, "1");
            declarations = new CssDeclarationList { new(property, color.AsRgbWithOpacity(opacity)), };
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            var varName = CssFramework.GetVariableNameWithPrefix("ring-opacity");
            declarations = new CssDeclarationList
            {
                new(varName, "1"), new(property, color.AsRgbWithOpacity($"var({varName})")),
            };
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color =>
            new CssRuleSet("ring-offset-" + color.Key, new CssDeclarationList { new("color", color.Value.AsRgb()), }));
    }
}