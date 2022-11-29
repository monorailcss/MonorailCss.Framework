using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The outline-color plugin.
/// </summary>
public class OutlineColor : IUtilityNamespacePlugin
{
    private const string Namespace = "outline";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutlineColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public OutlineColor(DesignSystem designSystem)
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
        var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

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
                new(CssProperties.OutlineColor, additionalValues[suffix]),
            };
        }
        else if (opacityValue != default)
        {
            var opacity = _opacities.GetValueOrDefault(opacityValue, "1");
            declarations = new CssDeclarationList
            {
                new(CssProperties.OutlineColor, color.AsRgbWithOpacity(opacity)),
            };
        }
        else
        {
            declarations = new CssDeclarationList { new(CssProperties.OutlineColor, color.AsRgb()), };
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color => new CssRuleSet("outline-" + color.Key, new CssDeclarationList
        {
            new("outline-color", color.Value.AsRgb()),
        }));
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ImmutableArray.Create<string>(Namespace);
}