using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-color plugin.
/// </summary>
internal class TextColor : IUtilityNamespacePlugin
{
    private const string Namespace = "text";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="cssFramework">The theme.</param>
    public TextColor(DesignSystem designSystem, CssFramework cssFramework)
    {
        _flattenedColors = designSystem.Colors.Flatten();
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

        if (namespaceSyntax.Suffix == default)
        {
            yield break;
        }

        var (colorValue, opacityValue) = ClassHelper.SplitColor(namespaceSyntax.Suffix);

        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            yield break;
        }

        CssDeclarationList declarations;
        if (opacityValue != default)
        {
            declarations = new CssDeclarationList { new(CssProperties.Color, color.AsRgbWithOpacity(opacityValue)), };
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            var varName = CssFramework.GetVariableNameWithPrefix("text-opacity");
            declarations = new CssDeclarationList
            {
                new(varName, "1"), new(CssProperties.Color, color.AsRgbWithOpacity($"var({varName})")),
            };
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return _flattenedColors.Select(color => new CssRuleSet("text-" + color.Key, new CssDeclarationList()
        {
            new("color", color.Value.AsRgb()),
        }));
    }
}