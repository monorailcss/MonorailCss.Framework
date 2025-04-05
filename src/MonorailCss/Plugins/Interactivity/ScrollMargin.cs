namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The scroll-margin plugin.
/// </summary>
public class ScrollMargin : BaseUtilityNamespacePlugin
{
    private readonly CssSuffixToValueMap _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrollMargin"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public ScrollMargin(DesignSystem designSystem)
    {
        _values = designSystem
            .Spacing
            .Add("auto", "auto")
            .AddRange(designSystem.Spacing.Select(i => new KeyValuePair<string, string>($"{i.Key}-", $"-{i.Value}")));
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("scroll-m", "scroll-margin", 0),
        new("scroll-mx", ("scroll-margin-inline", "margin-right"), 100),
        new("scroll-my", ("scroll-margin-block", "margin-bottom"), 100),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return _values;
    }
}