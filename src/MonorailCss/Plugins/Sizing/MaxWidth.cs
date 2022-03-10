using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MaxWidth : BaseUtilityNamespacePlugin
{
    private const string Namespace = "max-w";

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxWidth"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public MaxWidth(DesignSystem designSystem)
    {
        Values = new Dictionary<string, string>()
            {
                { "none", "none" },
                { "0", "0rem" },
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
                { "full", "100%" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
                { "prose", "65ch" },
            }.ToImmutableDictionary()
            .AddRange(designSystem.Screens.ToImmutableDictionary(i => $"screen-{i.Key}", i => i.Value));
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new() { { Namespace, "max-width" }, };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}

internal static class SizeHelpers
{
    public static readonly ImmutableDictionary<string, string> Percentages = new Dictionary<string, string>()
    {
        { "1/2", "50%" },
        { "1/3", "33.333333%" },
        { "2/3", "66.666667%" },
        { "1/4", "25%" },
        { "2/4", "50%" },
        { "3/4", "75%" },
        { "1/5", "20%" },
        { "2/5", "40%" },
        { "3/5", "60%" },
        { "4/5", "80%" },
        { "1/6", "16.666667%" },
        { "2/6", "33.333333%" },
        { "3/6", "50%" },
        { "4/6", "66.666667%" },
        { "5/6", "83.333333%" },
    }.ToImmutableDictionary();
}

/// <summary>
/// The max-width plugin.
/// </summary>
public class Width : BaseUtilityNamespacePlugin
{
    private const string Namespace = "w";

    /// <summary>
    /// Initializes a new instance of the <see cref="Width"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Width(DesignSystem designSystem)
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
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new() { { Namespace, "width" }, };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; }
}

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