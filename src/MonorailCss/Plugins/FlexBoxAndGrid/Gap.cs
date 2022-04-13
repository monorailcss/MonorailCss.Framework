namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The gap plugin.
/// </summary>
public class Gap : BaseUtilityNamespacePlugin
{
    private readonly CssSuffixToValueMap _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Gap"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Gap(DesignSystem designSystem)
    {
        _values = designSystem.Spacing;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            new("gap", "gap"),
            new("gap-x", "column-gap"),
            new("gap-y", "row-gap"),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _values;
    }
}