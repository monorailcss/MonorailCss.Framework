using System.Collections.Immutable;

namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// Margin plugin.
/// </summary>
public class Padding : BaseUtilityNamespacePlugin
{
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Padding"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Padding(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("p", "padding", 0),
        new("px", new[]
        {
            "padding-left", "padding-right"
        }, 100),
        new("py", new[]
        {
            "padding-top", "padding-bottom"
        }, 100),
        new("pl", "padding-left", 999),
        new("pr", "padding-right", 999),
        new("pt", "padding-top", 999),
        new("pb", "padding-bottom", 999),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return CssSuffixToValueMap.Empty;
    }

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = "spacing";
        calculationPattern = "calc(var({0}) * {1})";
        return true;
    }
}