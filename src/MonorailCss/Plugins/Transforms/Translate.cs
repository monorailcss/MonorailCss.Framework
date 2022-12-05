using MonorailCss.Css;

namespace MonorailCss.Plugins.Transforms;

/// <summary>
/// Translate plugin.
/// </summary>
public class Translate : BaseUtilityNamespacePlugin, IRegisterDefaults
{
    private readonly CssSuffixToValueMap _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Translate"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Translate(DesignSystem designSystem)
    {
        _values = SizeHelpers.Percentages
            .AddRange(designSystem.Spacing)
            .AddRange(designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")))
            .AddRange(SizeHelpers.Percentages.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")))
            .AddRange(new Dictionary<string, string>
            {
                { "full", "100%" },
            });
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            new("translate-x", CssFramework.GetVariableNameWithPrefix("translate-x")),
            new("translate-y", CssFramework.GetVariableNameWithPrefix("translate-y")),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _values;
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
            new(CssFramework.GetVariableNameWithPrefix("translate-x"), "0"),
            new(CssFramework.GetVariableNameWithPrefix("translate-y"), "0"),
        };
    }
}