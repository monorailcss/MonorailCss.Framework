namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-radius plugin.
/// </summary>
public class BorderRadius : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList()
    {
        return new CssNamespaceToPropertyMap
        {
            new("rounded", "border-radius", 0),
            new("rounded-t", ("border-top-left-radius", "border-top-right-radius"), 10),
            new("rounded-r", ("border-top-right-radius", "border-bottom-right-radius"), 10),
            new("rounded-b", ("border-bottom-right-radius", "border-bottom-left-radius"), 10),
            new("rounded-l", ("border-top-left-radius", "border-bottom-left-radius"), 10),
            new("rounded-tl", "border-top-left-radius", 1000),
            new("rounded-tr", "border-top-right-radius", 1000),
            new("rounded-br", "border-bottom-right-radius", 1000),
            new("rounded-bl", "border-bottom-left-radius", 1000),
        };
    }

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new CssSuffixToValueMap
        {
            { "none", "0px" },
            { "sm", "0.125rem" },
            { "DEFAULT", "0.25rem" },
            { "md", "0.375rem" },
            { "lg", "0.5rem" },
            { "xl", "0.75rem" },
            { "2xl", "1rem" },
            { "3xl", "1.5rem" },
            { "full", "9999px" },
        };
    }
}