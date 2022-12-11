using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins;

/// <summary>
/// A base helper class for working with utilities that map to the color system.
/// </summary>
public abstract class BaseColorNamespacePlugin : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private ImmutableDictionary<string, CssColor>? _completeColors;
    private readonly ImmutableDictionary<string, string> _opacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseColorNamespacePlugin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    protected BaseColorNamespacePlugin(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacity = designSystem.Opacities;
    }

    /// <summary>
    /// The namespace for the plugin.
    /// </summary>
    /// <returns>The namespace.</returns>
    protected abstract string Namespace();

    /// <summary>
    /// The property name of the color field of the plugin.
    /// </summary>
    /// <returns>The property name.</returns>
    protected abstract string ColorPropertyName();

    /// <summary>
    /// Whether this plugin supports a separate opacity utility.
    /// </summary>
    /// <param name="propertyName">If so, the property name to use for the opacity.</param>
    /// <returns>True if it should split, false if not.</returns>
    protected virtual bool ShouldSplitOpacityIntoOwnProperty([NotNullWhen(true)] out string? propertyName)
    {
        propertyName = default;
        return false;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when arbitraryValueSyntax.Namespace.Equals(Namespace()):
                {
                    var color = new CssColor(arbitraryValueSyntax.ArbitraryValue);
                    var declarations = GetDeclarations(color, null);
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, declarations);

                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals(Namespace()):
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

    private CssDeclarationList GetDeclarations(CssColor color, string? opacityValue)
    {
        CssDeclarationList declarations;

        if (color.HasAlpha() == false && color.IsValid() && ShouldSplitOpacityIntoOwnProperty(out var opacityPropertyName))
        {
            if (opacityValue != null)
            {
                // if this utility splits and it is specified then we will still write the property
                // with one value with the opacity defined.
                var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
                declarations = new CssDeclarationList
                {
                    (ColorPropertyName(), color.AsRgbWithOpacity(opacity)),
                };
            }
            else
            {
                // include a variable here so that if the text-opacity add-on is used it gets applied
                // it'll override this value and get applied properly.
                var varName = CssFramework.GetVariableNameWithPrefix(opacityPropertyName);
                declarations = new CssDeclarationList
                {
                    (varName, "1"), (ColorPropertyName(), color.AsRgbWithOpacity($"var({varName})")),
                };
            }
        }
        else
        {
            // this plug-in doesn't support an opacity property so either it has an opacity or it doesn't.
            declarations = opacityValue == null
                ? new CssDeclarationList { (ColorPropertyName(), color.AsRgb()), }
                : new CssDeclarationList { (ColorPropertyName(), color.AsRgbWithOpacity(opacityValue)) };
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

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        var ns = Namespace();
        foreach (var flattenedColor in AllColors())
        {
            var colorName = $"{ns}-{flattenedColor.Key}";
            yield return new CssRuleSet(colorName, GetDeclarations(flattenedColor.Value, default));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ImmutableArray.Create(Namespace());

    /// <summary>
    /// Additional colors to add to the design system.
    /// </summary>
    /// <remarks>By default this will be "inherit", "current" and "transparent".</remarks>
    /// <returns>A dictionary of additional colors.</returns>
    protected virtual ImmutableDictionary<string, string> AdditionalColors()
    {
        var b = ImmutableDictionary.CreateBuilder<string, string>();
        b.Add("inherit", "inherit");
        b.Add("current", "currentColor");
        b.Add("transparent", "transparent");

        return b.ToImmutable();
    }
}