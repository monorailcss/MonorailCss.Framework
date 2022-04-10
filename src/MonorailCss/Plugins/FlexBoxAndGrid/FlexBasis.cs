namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-basis plugin.
/// </summary>
public class FlexBasis : BaseUtilityNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlexBasis"/> class.
    /// </summary>
    /// <param name="designSystem">The design system</param>
    public FlexBasis(DesignSystem designSystem)
    {
        Values = designSystem.Spacing.AddRange(SizeHelpers.Percentages).Add("auto", "auto");
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new()
    {
        { "basis", "flex-basis" },
    };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}

/// <summary>
/// The gap plugin.
/// </summary>
public class Gap : BaseUtilityNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gap"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Gap(DesignSystem designSystem)
    {
        Values = designSystem.Spacing;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new()
    {
        { "gap", "gap" },
        { "gap-x", "column-gap" },
        { "gap-y", "row-gap" },
    };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}