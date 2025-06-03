namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// The inset plugin.
/// </summary>
public class Inset : BaseUtilityNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Inset"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Inset(DesignSystem designSystem)
    {
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("inset", ("top", "right", "bottom", "left")),
        new("inset-x", ("left", "right"), 100),
        new("inset-y", ("top", "bottom"), 100),
        new("top", "top", 999),
        new("right", "right", 999),
        new("bottom", "bottom", 999),
        new("left", "left", 999),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return SizeHelpers.Percentages
            .Add("auto", "auto")
            .AddRange(SizeHelpers.Percentages);
    }

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = "spacing";
        calculationPattern = "calc(var({0}) * {1})";
        return true;
    }
}