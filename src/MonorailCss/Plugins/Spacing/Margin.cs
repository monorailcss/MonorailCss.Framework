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
            { "top", "top" },
            { "right", "right" },
            { "bottom", "bottom" },
            { "left", "left" },
            { "inset-x", ("left", "right") },
            { "inset-y", ("top", "bottom") },
            { "inset", ("top", "right", "bottom", "left") },
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

/// <summary>
/// Margin plugin.
/// </summary>
public class Margin : BaseUtilityNamespacePlugin
{
    private readonly CssSuffixToValueMap _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Margin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Margin(DesignSystem designSystem)
    {
        _values = designSystem
            .Spacing
            .Add("auto", "auto")
            .AddRange(designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            { "m", "margin" },
            { "mx", ("margin-left", "margin-right") },
            { "my", ("margin-top", "margin-bottom") },
            { "ml", "margin-left" },
            { "mr", "margin-right" },
            { "mt", "margin-top" },
            { "mb", "margin-bottom" },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _values;
    }
}