namespace MonorailCss.Plugins;

/// <summary>
/// The inset plugin.
/// </summary>
public class Inset : BaseUtilityNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Inset"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Inset(DesignSystem designSystem)
    {
        Values = designSystem
            .Spacing
            .Add("auto", "auto")
            .AddRange(designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new()
    {
        { "top", "top" },
        { "right", "right" },
        { "bottom", "bottom" },
        { "left", "left" },
        { "inset-x", ("left", "right") },
        { "inset-y", ("top", "bottom") },
        { "inset", ("top", "right", "bottom", "left") },
    };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}

/// <summary>
/// Margin plugin.
/// </summary>
public class Margin : BaseUtilityNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Margin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Margin(DesignSystem designSystem)
    {
        Values = designSystem
            .Spacing
            .Add("auto", "auto")
            .AddRange(designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList
    {
        get => new()
            {
                { "m", "margin" },
                { "mx", ("margin-left", "margin-right") },
                { "my", ("margin-top", "margin-bottom") },
                { "ml", "margin-left" },
                { "mr", "margin-right" },
                { "mt", "margin-top" },
                { "mb", "margin-bottom" },
            };
    }

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}