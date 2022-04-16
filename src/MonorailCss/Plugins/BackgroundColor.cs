using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins;

/// <summary>
/// Background color plugin.
/// </summary>
internal class BackgroundColor : IUtilityNamespacePlugin
{
    private const string Namespace = "bg";
    private readonly CssFramework _cssFramework;
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="cssFramework">The theme.</param>
    public BackgroundColor(DesignSystem designSystem, CssFramework cssFramework)
    {
        _cssFramework = cssFramework;
        _flattenedColors = designSystem.GetFlattenColors();
        _opacity = designSystem.Opacities;
    }

    /// <inheritdoc/>
    public ImmutableArray<string> Namespaces => new[] { Namespace }.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax ||
            !namespaceSyntax.NamespaceEquals(Namespace))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

        var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            yield break;
        }

        CssDeclarationList declarations;
        if (opacityValue != default)
        {
            var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
            declarations = new CssDeclarationList
            {
                new(CssProperties.BackgroundColor, color.AsRgbWithOpacity(opacity)),
            };
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            var varName = CssFramework.GetVariableNameWithPrefix("bg-opacity");
            declarations = new CssDeclarationList
            {
                new(varName, "1"), new(CssProperties.BackgroundColor, color.AsRgbWithOpacity($"var({varName})")),
            };
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color =>
            new CssRuleSet(
                "bg-" + color.Key,
                new CssDeclarationList { new("background-color", color.Value.AsRgb()), }));
    }
}