namespace MonorailCss.Plugins;

/// <summary>
/// The inset plugin.
/// </summary>
public class Inset : BaseUtilityNamespacePlugin
{
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Inset"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Inset(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            new("inset", ("top", "right", "bottom", "left"), 0),
            new("inset-x", ("left", "right"), 100),
            new("inset-y", ("top", "bottom"), 100),
            new("top", "top", 999),
            new("right", "right", 999),
            new("bottom", "bottom", 999),
            new("left", "left", 999),
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _designSystem
            .Spacing
            .Add("auto", "auto")
            .AddRange(_designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }
}