namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// Margin plugin.
/// </summary>
public class Padding : BaseUtilityNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Padding"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Padding(DesignSystem designSystem)
    {
        Values = designSystem
            .Spacing
            .AddRange(designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList
    {
        get => new()
            {
                { "p", "padding" },
                { "px", new[] { "padding-left", "padding-right" } },
                { "py", new[] { "padding-top", "padding-bottom" } },
                { "pl", "padding-left" },
                { "pr", "padding-right" },
                { "pt", "padding-top" },
                { "pb", "padding-bottom" },
            };
    }

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}