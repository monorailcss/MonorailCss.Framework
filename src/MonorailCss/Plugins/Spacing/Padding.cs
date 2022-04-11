namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// Margin plugin.
/// </summary>
public class Padding : BaseUtilityNamespacePlugin
{
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Padding"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Padding(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            { "p", "padding" },
            { "px", new[] { "padding-left", "padding-right" } },
            { "py", new[] { "padding-top", "padding-bottom" } },
            { "pl", "padding-left" },
            { "pr", "padding-right" },
            { "pt", "padding-top" },
            { "pb", "padding-bottom" },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _designSystem
            .Spacing
            .AddRange(_designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }
}