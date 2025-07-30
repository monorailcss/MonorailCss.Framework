namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-width plugin.
/// </summary>
public class BorderWidth : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("border", "border-width"),
        new("border-x", ("border-left-width", "border-right-width")),
        new("border-y", ("border-top-width", "border-bottom-width")),
        new("border-r", "border-right-width"),
        new("border-t", "border-top-width"),
        new("border-b", "border-bottom-width"),
        new("border-l", "border-left-width"),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() => new()
        {
            { "0", "0px" },
            { "DEFAULT", "1px" },
            { "2", "2px" },
            { "4", "4px" },
            { "8", "8px" },
        };

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = string.Empty;
        calculationPattern = "{1}px";
        return true;
    }

    /// <inheritdoc />
    protected override bool IsValidArbitraryValue(string arbitraryValue, CssSuffixToValueMap cssSuffixToValuesMap)
    {
        // Don't process color keywords as width values
        var colorKeywords = new[] { "transparent", "inherit", "current", "red", "green", "blue", "white", "black", "gray", "grey", "yellow", "orange", "purple", "pink", "brown", "cyan", "magenta", "lime", "navy", "teal", "silver", "maroon", "olive", "aqua", "fuchsia" };
        if (colorKeywords.Any(keyword => arbitraryValue.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Use the base implementation for other validation
        return base.IsValidArbitraryValue(arbitraryValue, cssSuffixToValuesMap);
    }
}