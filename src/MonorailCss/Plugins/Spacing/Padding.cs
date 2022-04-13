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
            new("p", "padding", 0),
            new("px", new[] { "padding-left", "padding-right" }, 100),
            new("py", new[] { "padding-top", "padding-bottom" }, 100),
            new("pl", "padding-left", 999),
            new("pr", "padding-right", 999),
            new("pt", "padding-top", 999),
            new("pb", "padding-bottom", 999),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _designSystem
            .Spacing
            .AddRange(_designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }
}