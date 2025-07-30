using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-color plugin.
/// </summary>
public class BorderColor : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacity;
    private ImmutableDictionary<string, CssColor>? _completeColors;

    private static readonly ImmutableDictionary<string, string[]> NamespaceToProperties = new Dictionary<string, string[]>
    {
        { "border", [CssProperties.BorderColor] },
        { "border-x", [CssProperties.BorderLeftColor, CssProperties.BorderRightColor] },
        { "border-y", [CssProperties.BorderTopColor, CssProperties.BorderBottomColor] },
        { "border-r", [CssProperties.BorderRightColor] },
        { "border-t", [CssProperties.BorderTopColor] },
        { "border-b", [CssProperties.BorderBottomColor] },
        { "border-l", [CssProperties.BorderLeftColor] },
    }.ToImmutableDictionary();

    /// <summary>
    /// Initializes a new instance of the <see cref="BorderColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BorderColor(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacity = designSystem.Opacities;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..NamespaceToProperties.Keys];

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when NamespaceToProperties.ContainsKey(arbitraryValueSyntax.Namespace):
                {
                    var color = new CssColor(arbitraryValueSyntax.ArbitraryValue);
                    var properties = NamespaceToProperties[arbitraryValueSyntax.Namespace];
                    var declarations = GetDeclarations(color, null, properties);
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, declarations);
                    break;
                }

            case NamespaceSyntax namespaceSyntax when NamespaceToProperties.ContainsKey(namespaceSyntax.Namespace):
                {
                    var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
                    var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

                    if (!AllColors().TryGetValue(colorValue, out var color))
                    {
                        yield break;
                    }

                    var properties = NamespaceToProperties[namespaceSyntax.Namespace];
                    var declarations = GetDeclarations(color, opacityValue, properties);
                    yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
                    break;
                }

            default:
                yield break;
        }
    }

    private CssDeclarationList GetDeclarations(CssColor color, string? opacityValue, string[] properties)
    {
        var declarations = new CssDeclarationList();

        string colorValue;
        if (opacityValue == null)
        {
            colorValue = color.AsString();
        }
        else
        {
            var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
            colorValue = color.AsStringWithOpacity(opacity);
        }

        foreach (var property in properties)
        {
            declarations.Add(new CssDeclaration(property, colorValue));
        }

        return declarations;
    }

    private ImmutableDictionary<string, CssColor> AllColors()
    {
        if (_completeColors == null)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, CssColor>();
            builder.AddRange(_flattenedColors);
            builder.AddRange(AdditionalColors().Select(i => new KeyValuePair<string, CssColor>(i.Key, new CssColor(i.Value))));
            _completeColors = builder.ToImmutable();
        }

        return _completeColors;
    }

    private static ImmutableDictionary<string, string> AdditionalColors()
    {
        var b = ImmutableDictionary.CreateBuilder<string, string>();
        b.Add("inherit", "inherit");
        b.Add("current", "currentColor");
        b.Add("transparent", "transparent");
        return b.ToImmutable();
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var (ns, properties) in NamespaceToProperties)
        {
            foreach (var flattenedColor in AllColors())
            {
                var colorName = $"{ns}-{flattenedColor.Key}";
                yield return new CssRuleSet(colorName, GetDeclarations(flattenedColor.Value, null, properties));
            }
        }
    }
}