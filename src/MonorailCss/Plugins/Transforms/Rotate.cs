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
    [
        new("rotate", CssFramework.GetVariableNameWithPrefix("rotate")),
    ];

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
        return [("transform", Transform.TransformValue)];
    }

    /// <inheritdoc />
    public CssDeclarationList GetDefaults()
    {
        return
        [
            (CssFramework.GetVariableNameWithPrefix("scale-x"), "1"),
            (CssFramework.GetVariableNameWithPrefix("scale-y"), "1"),
            (CssFramework.GetVariableNameWithPrefix("skew-x"), "0"),
            (CssFramework.GetVariableNameWithPrefix("skew-y"), "0"),
            (CssFramework.GetVariableNameWithPrefix("rotate"), "0"),
            (CssFramework.GetVariableNameWithPrefix("translate-x"), "0"),
            (CssFramework.GetVariableNameWithPrefix("translate-y"), "0"),
        ];
    }
}