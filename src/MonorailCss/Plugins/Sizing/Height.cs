namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class Height : BaseUtilityNamespacePlugin
{
    private const string Namespace = "h";
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Height"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Height(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new() { new(Namespace, "height"), };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return SizeHelpers.Percentages
            .AddRange(new Dictionary<string, string>
            {
                { "auto", "auto" },
                { "px", "1px" },
                { "full", "100%" },
                { "screen", "100vh" },
                { "dvh", "100dvh" },
                { "dvw", "100dvw" },
                { "lvh", "100lvh" },
                { "lvw", "100lvw" },
                { "svh", "100svh" },
                { "svw", "100svw" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
                { "lh", "1lh" },
            });
    }

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = "spacing";
        calculationPattern = "calc(var({0}) * {1})";
        return true;
    }
}