namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-basis plugin.
/// </summary>
public class FlexBasis : BaseUtilityNamespacePlugin
{
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlexBasis"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public FlexBasis(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            { "basis", "flex-basis" },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _designSystem.Spacing.AddRange(SizeHelpers.Percentages).Add("auto", "auto");
    }
}

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
            { "gap", "gap" },
            { "gap-x", "column-gap" },
            { "gap-y", "row-gap" },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _values;
    }
}