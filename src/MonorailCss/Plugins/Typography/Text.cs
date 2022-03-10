using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text plugin.
/// </summary>
internal class Text : IUtilityNamespacePlugin
{
    private const string Namespace = "text";
    private readonly CssFramework _cssFramework;
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Text"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="cssFramework">The theme.</param>
    public Text(DesignSystem designSystem, CssFramework cssFramework)
    {
        _cssFramework = cssFramework;
        _designSystem = designSystem;
        _flattenedColors = designSystem.Colors.Flatten();
    }

    /// <inheritdoc/>
    public ImmutableArray<string> Namespaces => new[]
    {
        Namespace,
    }.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace) || namespaceSyntax.Suffix == default)
        {
            yield break;
        }

        if (TryGetTextColor(namespaceSyntax, out var textColorDeclaration))
        {
            yield return textColorDeclaration;
        }
        else if (TryGetTextSize(namespaceSyntax, out var textSizeDeclaration))
        {
            yield return textSizeDeclaration;
        }
        else if (TryGetTextAlignment(namespaceSyntax, out var textAlignmentDeclaration))
        {
            yield return textAlignmentDeclaration;
        }
        else
        {
            yield break;
        }
    }

    private bool TryGetTextAlignment(NamespaceSyntax namespaceSyntax, [NotNullWhen(true)] out CssRuleSet? cssDeclaration)
    {
        if (namespaceSyntax.Suffix == default)
        {
            cssDeclaration = default;
            return false;
        }

        var value = namespaceSyntax.Suffix switch
        {
            "left" => "left",
            "center" => "center",
            "right" => "right",
            "justify" => "justify",
            _ => default,
        };

        if (value == default)
        {
            cssDeclaration = default;
            return false;
        }

        cssDeclaration = new CssRuleSet(
            namespaceSyntax.OriginalSyntax,
            new CssDeclarationList { new("text-align", value), });

        return true;
    }

    private bool TryGetTextSize(NamespaceSyntax namespaceSyntax, [NotNullWhen(true)] out CssRuleSet? cssDeclaration)
    {
        if (namespaceSyntax.Suffix == default || !_designSystem.Typography.ContainsKey(namespaceSyntax.Suffix))
        {
            cssDeclaration = default;
            return false;
        }

        var value = _designSystem.Typography[namespaceSyntax.Suffix];
        cssDeclaration = new CssRuleSet(namespaceSyntax.OriginalSyntax, new CssDeclarationList
        {
            new(CssProperties.FontSize, value.FontSize), new(CssProperties.LineHeight, value.LineHeight),
        });

        return true;
    }

    private bool TryGetTextColor(NamespaceSyntax namespaceSyntax, [NotNullWhen(true)] out CssRuleSet? cssDeclaration)
    {
        if (namespaceSyntax.Suffix == default)
        {
            cssDeclaration = default;
            return false;
        }

        var (colorValue, opacityValue) = ClassHelper.SplitColor(namespaceSyntax.Suffix);

        if (!_flattenedColors.TryGetValue(colorValue, out var color))
        {
            cssDeclaration = default;
            return false;
        }

        CssDeclarationList declarations;
        if (opacityValue != default)
        {
            declarations = new CssDeclarationList
            {
                new(CssProperties.Color, color.AsRgbWithOpacity(opacityValue)),
            };
        }
        else
        {
            // include a variable here so that if the text-opacity add-on is used it gets applied
            // it'll override this value and get applied properly.
            var varName = _cssFramework.GetVariableNameWithPrefix("text-opacity");
            declarations = new CssDeclarationList
            {
                new(varName, "1"), new(CssProperties.Color, color.AsRgbWithOpacity($"var({varName})")),
            };
        }

        cssDeclaration = new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
        return true;
    }
}