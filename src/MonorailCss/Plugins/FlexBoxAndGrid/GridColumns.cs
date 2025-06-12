using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-template-columns plugin.
/// </summary>
public class GridColumns : BaseUtilityNamespacePlugin
{
    private const string Namespace = "grid-cols";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new(Namespace, "grid-template-columns");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        var dict = new Dictionary<string, string>();

        for (var i = 1; i <= 12; i++)
        {
            dict.Add(i.ToString(), $"repeat({i}, minmax(0, 1fr))");
        }

        dict.Add("none", "none");
        dict.Add("subgrid", "subgrid");
        return new CssSuffixToValueMap(dict.ToImmutableDictionary());
    }

    /// <inheritdoc />
    protected override bool IsValidArbitraryValue(string arbitraryValue, CssSuffixToValueMap cssSuffixToValuesMap)
    {
        // Grid template columns allow any valid CSS grid-template-columns value
        return true;
    }
}