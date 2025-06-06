using System.Collections.Immutable;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The scroll-margin plugin.
/// </summary>
public class ScrollMargin : BaseUtilityNamespacePlugin
{
    private readonly CssSuffixToValueMap _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollMargin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public ScrollMargin(DesignSystem designSystem)
    {
        _values = ImmutableDictionary.Create<string, string>()
            .Add("auto", "auto");
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("scroll-m", "scroll-margin", 0),
        new("scroll-mx", ("scroll-margin-inline", "margin-right"), 100),
        new("scroll-my", ("scroll-margin-block", "margin-bottom"), 100),
    ];

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