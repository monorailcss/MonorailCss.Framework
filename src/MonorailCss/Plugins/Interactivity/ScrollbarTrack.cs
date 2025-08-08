using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The scrollbar-track color plugin.
/// </summary>
public class ScrollbarTrack : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacity;
    private ImmutableDictionary<string, CssColor>? _completeColors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollbarTrack"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public ScrollbarTrack(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacity = designSystem.Opacities;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        var thumbVar = CssFramework.GetVariableNameWithPrefix("scrollbar-thumb");
        var trackVar = CssFramework.GetVariableNameWithPrefix("scrollbar-track");

        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when arbitraryValueSyntax.Namespace.Equals("scrollbar-track"):
                {
                    var color = new CssColor(arbitraryValueSyntax.ArbitraryValue);
                    var declarations = new CssDeclarationList
                    {
                        (trackVar, color.AsString()),
                        ("scrollbar-color", $"var({thumbVar}, initial) var({trackVar})")
                    };
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, declarations);
                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals("scrollbar-track"):
                {
                    var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
                    var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

                    if (!AllColors().TryGetValue(colorValue, out var color))
                    {
                        yield break;
                    }

                    var declarations = GetDeclarations(color, opacityValue, thumbVar, trackVar);
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
        var thumbVar = CssFramework.GetVariableNameWithPrefix("scrollbar-thumb");
        var trackVar = CssFramework.GetVariableNameWithPrefix("scrollbar-track");

        foreach (var flattenedColor in AllColors())
        {
            var colorName = $"scrollbar-track-{flattenedColor.Key}";
            yield return new CssRuleSet(colorName, GetDeclarations(flattenedColor.Value, null, thumbVar, trackVar));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "scrollbar-track" }];

    private CssDeclarationList GetDeclarations(CssColor color, string? opacityValue, string thumbVar, string trackVar)
    {
        CssDeclarationList declarations;

        if (opacityValue == null)
        {
            declarations =
            [
                (trackVar, color.AsString()),
                ("scrollbar-color", $"var({thumbVar}, initial) var({trackVar})")
            ];
        }
        else
        {
            var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
            declarations =
            [
                (trackVar, color.AsStringWithOpacity(opacity)),
                ("scrollbar-color", $"var({thumbVar}, initial) var({trackVar})")
            ];
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
}