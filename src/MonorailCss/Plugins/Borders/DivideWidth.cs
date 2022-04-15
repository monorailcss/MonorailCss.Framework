using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The divide-width plugin.
/// </summary>
public class DivideWidth : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, string> _widths = new Dictionary<string, string>()
    {
        { "0", "0px" },
        { "DEFAULT", "1px" },
        { "2", "2px" },
        { "4", "4px" },
        { "8", "8px" },
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax
            || (!namespaceSyntax.NamespaceEquals("divide-x") && !namespaceSyntax.NamespaceEquals("divide-y")))
        {
            yield break;
        }

        var isVertical = namespaceSyntax.Namespace == "divide-y";
        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
        if (!_widths.TryGetValue(suffix, out var width))
        {
            yield break;
        }

        var cssDeclarationList = CssDeclarationList(width, isVertical);

        var cssRuleSet = new CssRuleSet(
            new CssSelector(namespaceSyntax.OriginalSyntax, " > :not([hidden]) ~ :not([hidden])"),
            cssDeclarationList);
        yield return cssRuleSet;
    }

    private CssDeclarationList CssDeclarationList(string width, bool isVertical)
    {
        string varReverse;
        string borderedProperty;
        string blankBorderProperty;
        if (isVertical)
        {
            varReverse = CssFramework.GetVariableNameWithPrefix("divide-y-reverse");
            borderedProperty = CssProperties.BorderTopWidth;
            blankBorderProperty = CssProperties.BorderBottomWidth;
        }
        else
        {
            varReverse = CssFramework.GetVariableNameWithPrefix("divide-x-reverse");
            borderedProperty = CssProperties.BorderRightWidth;
            blankBorderProperty = CssProperties.BorderLeftWidth;
        }

        // properties are a bit more complex than you'd think thanks to the reverse functionality. need to be able to
        // swap which one has a valid and a zero based on it.
        var blankBorderPropValue = $"calc({width} * var({varReverse}))";
        var borderPropValue = $"calc({width} * calc(1 - var({varReverse})))";

        var cssDeclarationList = new CssDeclarationList()
        {
            new(varReverse, "0"),
            new(borderedProperty, borderPropValue),
            new(blankBorderProperty, blankBorderPropValue),
        };
        return cssDeclarationList;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var width in _widths)
        {
            var suffix = string.Empty;
            if (width.Key != "DEFAULT")
            {
                suffix = $"-{width.Key}";
            }

            yield return new CssRuleSet("divide-x" + suffix, CssDeclarationList(width.Value, false));
            yield return new CssRuleSet("divide-y" + suffix, CssDeclarationList(width.Value, true));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { "divide-x", "divide-y" }.ToImmutableArray();
}

/// <summary>
/// The border-color plugin.
/// </summary>
public class DivideColor : IUtilityNamespacePlugin
{
    private const string Namespace = "divide";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="DivideColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public DivideColor(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacities = designSystem.Opacities;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
        var (colorValue, opacityValue) = ClassHelper.SplitColor(suffix);

        CssDeclarationList declarations;
        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            var additionalValues = new Dictionary<string, string>
            {
                { "inherit", "inherit" },
                { "current", "currentColor" },
                { "transparent", "transparent" },
            };

            if (!additionalValues.ContainsKey(suffix))
            {
                yield break;
            }

            declarations = new CssDeclarationList
            {
                new(CssProperties.BorderColor, additionalValues[suffix]),
            };
        }
        else if (opacityValue != default)
        {
            var opacity = _opacities.GetValueOrDefault(opacityValue, "1");
            declarations = new CssDeclarationList
            {
                new(CssProperties.BorderColor, color.AsRgbWithOpacity(opacity)),
            };
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            declarations = new CssDeclarationList { new(CssProperties.BorderColor, color.AsRgb()), };
        }

        yield return new CssRuleSet(new CssSelector(namespaceSyntax.OriginalSyntax, " > :not([hidden]) ~ :not([hidden])"), declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color => new CssRuleSet("divide-" + color.Key, new CssDeclarationList
        {
            new("border-color", color.Value.AsRgb()),
        }));
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ImmutableArray.Create<string>(Namespace);
}