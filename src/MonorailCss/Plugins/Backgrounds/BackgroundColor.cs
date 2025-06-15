using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background color plugin.
/// </summary>
public class BackgroundColor : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacity;
    private ImmutableDictionary<string, CssColor>? _completeColors;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BackgroundColor(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacity = designSystem.Opacities;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when arbitraryValueSyntax.Namespace.Equals("bg"):
                {
                    // Don't process values that are clearly not colors
                    if (IsBackgroundImageValue(arbitraryValueSyntax.ArbitraryValue))
                    {
                        yield break;
                    }

                    var color = new CssColor(arbitraryValueSyntax.ArbitraryValue);
                    var declarations = GetDeclarations(color, null);
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, declarations);

                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals("bg"):
                {
                    var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

                    var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

                    if (!AllColors().TryGetValue(colorValue, out var color))
                    {
                        yield break;
                    }

                    var declarations = GetDeclarations(color, opacityValue);
                    yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);

                    break;
                }

            default:
                yield break;
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var flattenedColor in AllColors())
        {
            var colorName = $"bg-{flattenedColor.Key}";
            yield return new CssRuleSet(colorName, GetDeclarations(flattenedColor.Value, null));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg" }];

    private CssDeclarationList GetDeclarations(CssColor color, string? opacityValue)
    {
        CssDeclarationList declarations;

        if (color.HasAlpha() == false && color.IsValid())
        {
            if (opacityValue != null)
            {
                // if this utility splits and it is specified then we will still write the property
                // with one value with the opacity defined.
                var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
                declarations =
                [
                    ("background-color", color.AsStringWithOpacity(opacity)),
                ];
            }
            else
            {
                // include a variable here so that if the text-opacity add-on is used it gets applied
                // it'll override this value and get applied properly.
                var varName = CssFramework.GetVariableNameWithPrefix("bg-opacity");
                declarations =
                [
                    (varName, "1"), ("background-color", color.AsStringWithOpacity($"var({varName})")),
                ];
            }
        }
        else
        {
            // this plug-in doesn't support an opacity property so either it has an opacity or it doesn't.
            if (opacityValue == null)
            {
                declarations =
                [
                    ("background-color", color.AsString()),
                ];
            }
            else
            {
                var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
                declarations =
                [
                    ("background-color", color.AsStringWithOpacity(opacity)),
                ];
            }
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

    private static bool IsBackgroundImageValue(string value)
    {
        // Check if the value is a background-image specific value (not a color)
        return value.StartsWith("url(") ||
               value.StartsWith("linear-gradient(") ||
               value.StartsWith("radial-gradient(") ||
               value.StartsWith("conic-gradient(") ||
               value.StartsWith("repeating-linear-gradient(") ||
               value.StartsWith("repeating-radial-gradient(") ||
               value.Equals("none", StringComparison.OrdinalIgnoreCase);
    }
}