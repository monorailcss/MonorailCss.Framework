using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// A base helper class for working with utilities that map to the color system.
/// </summary>
public abstract class BaseColorNamespacePlugin : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
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
        if (syntax is not NamespaceSyntax namespaceSyntax ||
            !namespaceSyntax.NamespaceEquals(Namespace()))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

        var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            yield break;
        }

        var declarations = GetDeclarations(color, opacityValue);
        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    private CssDeclarationList GetDeclarations(CssColor color, string? opacityValue)
    {
        CssDeclarationList declarations;

        if (ShouldSplitOpacityIntoOwnProperty(out var opacityPropertyName))
        {
            if (opacityValue != default)
            {
                // if this utility splits and it is specified then we will still write the property
                // with one value with the opacity defined.
                var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
                declarations = new CssDeclarationList
                {
                    new(ColorPropertyName(), color.AsRgbWithOpacity(opacity)),
                };
            }
            else
            {
                // include a variable here so that if the text-opacity add-on is used it gets applied
                // it'll override this value and get applied properly.
                var varName = CssFramework.GetVariableNameWithPrefix(opacityPropertyName);
                declarations = new CssDeclarationList
                {
                    new(varName, "1"), new(ColorPropertyName(), color.AsRgbWithOpacity($"var({varName})")),
                };
            }
        }
        else
        {
            // this plug-in doesn't support an opacity property so either it has an opacity or it doesn't.
            declarations = opacityValue == default
                ? new CssDeclarationList { new(ColorPropertyName(), color.AsRgb()), }
                : new CssDeclarationList { new(ColorPropertyName(), color.AsRgbWithOpacity(opacityValue)) };
        }

        return declarations;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        var ns = Namespace();
        foreach (var flattenedColor in _flattenedColors)
        {
            var colorName = $"{ns}-{flattenedColor.Key}";
            yield return new CssRuleSet(colorName, GetDeclarations(flattenedColor.Value, default));
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ImmutableArray.Create(Namespace());
}

/// <summary>
/// Background color plugin.
/// </summary>
public class BackgroundColor : BaseColorNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BackgroundColor(DesignSystem designSystem)
        : base(designSystem)
    {
    }

    /// <inheritdoc />
    protected override string Namespace() => "bg";

    /// <inheritdoc />
    protected override string ColorPropertyName() => "background-color";

    /// <inheritdoc />
    protected override bool ShouldSplitOpacityIntoOwnProperty([NotNullWhen(true)] out string? propertyName)
    {
        propertyName = "bg-opacity";
        return true;
    }
}