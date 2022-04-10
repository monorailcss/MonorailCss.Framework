namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class Height : BaseUtilityNamespacePlugin
{
    private const string Namespace = "h";

    /// <summary>
    /// Initializes a new instance of the <see cref="Height"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Height(DesignSystem designSystem)
    {
        Values = SizeHelpers.Percentages
            .AddRange(designSystem.Spacing)
            .AddRange(new Dictionary<string, string>()
            {
                { "auto", "auto" },
                { "full", "100%" },
                { "screen", "100vh" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
            });
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new() { { Namespace, "height" }, };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}