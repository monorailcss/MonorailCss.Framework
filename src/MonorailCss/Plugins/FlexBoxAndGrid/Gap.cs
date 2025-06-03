using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

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