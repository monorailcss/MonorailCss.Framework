namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class Width : BaseUtilityNamespacePlugin
{
    private const string Namespace = "w";
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Width"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Width(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new(Namespace, "width");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return SizeHelpers.Percentages
            .AddRange(_designSystem.Spacing)
            .AddRange(new Dictionary<string, string>
            {
                { "3xs", "16rem" },
                { "2xs", "18rem" },
                { "xs", "20rem" },
                { "sm", "24rem" },
                { "md", "28rem" },
                { "lg", "32rem" },
                { "xl", "36rem" },
                { "2xl", "42rem" },
                { "3xl", "48rem" },
                { "4xl", "56rem" },
                { "5xl", "64rem" },
                { "6xl", "72rem" },
                { "7xl", "80rem" },
                { "auto", "auto" },
                { "px", "1px" },
                { "full", "100%" },
                { "screen", "100vw" },
                { "dvw", "100dvw" },
                { "dvh", "100dvh" },
                { "lvw", "100lvw" },
                { "lvh", "100lvh" },
                { "svw", "100svw" },
                { "svh", "100svh" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
            });
    }
}