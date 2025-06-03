using System.Collections.Immutable;

namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// Margin plugin.
/// </summary>
public class Margin : BaseUtilityNamespacePlugin
{
    private readonly CssSuffixToValueMap _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Margin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Margin(DesignSystem designSystem)
    {
        _values = ImmutableDictionary.Create<string, string>()
            .Add("auto", "auto");
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            new("m", "margin", 0),
            new("mx", ("margin-left", "margin-right"), 100),
            new("my", ("margin-top", "margin-bottom"), 100),
            new("ml", "margin-left", 999),
            new("mr", "margin-right", 999),
            new("mt", "margin-top", 999),
            new("mb", "margin-bottom", 999),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _values;
    }

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = "spacing";
        calculationPattern = "calc(var({0}) * {1})";
        return true;
    }
}