using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-color plugin.
/// </summary>
public class BorderColor : IUtilityNamespacePlugin
{
    private const string Namespace = "border";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacities;

    /// <summary>
    /// Initializes a new instance of the <see cref="BorderColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BorderColor(DesignSystem designSystem)
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

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color => new CssRuleSet("border-" + color.Key, new CssDeclarationList
        {
            new("border-color", color.Value.AsRgb()),
        }));
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ImmutableArray.Create<string>(Namespace);
}