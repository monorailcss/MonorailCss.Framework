using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-template-rows plugin.
/// </summary>
public class GridRows : BaseUtilityNamespacePlugin
{
    private const string Namespace = "grid-rows";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new(Namespace, "grid-template-rows");

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
        // Grid template rows allow any valid CSS grid-template-rows value
        return true;
    }
}