using System.Collections.Immutable;

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
    [
        new("basis", "flex-basis"),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return ImmutableDictionary.Create<string, string>()
            .AddRange(SizeHelpers.Percentages)
            .Add("auto", "auto")
            .Add("full", "100%");
    }

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = "spacing";
        calculationPattern = "calc(var({0}) * {1})";
        return true;
    }
}