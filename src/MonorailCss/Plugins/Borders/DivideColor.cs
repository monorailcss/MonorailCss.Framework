using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

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

            declarations =
            [
                (CssProperties.BorderColor, additionalValues[suffix]),
            ];
        }
        else if (opacityValue != null)
        {
            var opacity = _opacities.GetValueOrDefault(opacityValue, "1");
            declarations =
            [
                (CssProperties.BorderColor, color.AsStringWithOpacity(opacity)),
            ];
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            declarations = [(CssProperties.BorderColor, color.AsString())];
        }

        yield return new CssRuleSet(new CssSelector(namespaceSyntax.OriginalSyntax, " > :not([hidden]) ~ :not([hidden])"), declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color => new CssRuleSet("divide-" + color.Key, [
            ("border-color", color.Value.AsString()),
        ]));
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [Namespace];
}