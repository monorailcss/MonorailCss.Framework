using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Transforms;

/// <summary>
/// The rotate plugin.
/// </summary>
public class Rotate : BaseUtilityNamespacePlugin, IRegisterDefaults
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            new("rotate", CssFramework.GetVariableNameWithPrefix("rotate")),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        var map = new Dictionary<string, string>()
        {
            { "0", "0" },
            { "1", "1deg" },
            { "2", "2deg" },
            { "3", "3deg" },
            { "6", "6deg" },
            { "12", "12deg" },
            { "45", "45deg" },
            { "90", "90deg" },
            { "180", "180deg" },
        };

        return new CssSuffixToValueMap(
            map.Concat(map.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")))
                .ToImmutableDictionary());
    }

    /// <inheritdoc />
    protected override CssDeclarationList AdditionalDeclarations()
    {
        return new CssDeclarationList { new("transform", Transform.TransformValue) };
    }

    /// <inheritdoc />
    public CssDeclarationList GetDefaults()
    {
        return new CssDeclarationList
        {
            new(CssFramework.GetVariableNameWithPrefix("rotate"), "0"),
        };
    }
}