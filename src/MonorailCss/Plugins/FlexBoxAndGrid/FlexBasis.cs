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
            new("basis", "flex-basis"),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _designSystem.Spacing.AddRange(SizeHelpers.Percentages).Add("auto", "auto");
    }
}