using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Transforms;

/// <summary>
/// The scale plugin.
/// </summary>
public class Scale : BaseUtilityNamespacePlugin, IRegisterDefaults
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            new("scale", (CssFramework.GetVariableNameWithPrefix("scale-x"), CssFramework.GetVariableNameWithPrefix("scale-y"))),
            new("scale-x", CssFramework.GetVariableNameWithPrefix("scale-x")),
            new("scale-y", CssFramework.GetVariableNameWithPrefix("scale-y")),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        var map = new Dictionary<string, string>()
        {
            { "0", "0" },
            { "50", ".5" },
            { "75", ".75" },
            { "90", ".9" },
            { "95", ".95" },
            { "100", "1" },
            { "105", "1.05" },
            { "110", "1.1" },
            { "125", "1.25" },
            { "150", "1.5" },
            { "200", "2" },
        };

        return new CssSuffixToValueMap(
            map.Concat(map.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")))
                .ToImmutableDictionary());
    }

    /// <inheritdoc />
    protected override CssDeclarationList AdditionalDeclarations()
    {
        return new CssDeclarationList { ("transform", Transform.TransformValue) };
    }

    /// <inheritdoc />
    public CssDeclarationList GetDefaults()
    {
        return new CssDeclarationList
        {
            (CssFramework.GetVariableNameWithPrefix("scale-x"), "1"),
            (CssFramework.GetVariableNameWithPrefix("scale-y"), "1"),
            (CssFramework.GetVariableNameWithPrefix("skew-x"), "0"),
            (CssFramework.GetVariableNameWithPrefix("skew-y"), "0"),
            (CssFramework.GetVariableNameWithPrefix("rotate"), "0"),
            (CssFramework.GetVariableNameWithPrefix("translate-x"), "0"),
            (CssFramework.GetVariableNameWithPrefix("translate-y"), "0"),
        };
    }
}