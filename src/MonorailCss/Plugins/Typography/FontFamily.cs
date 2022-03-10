using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// The font-family plugin.
/// </summary>
public class FontFamily : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-family";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>()
    {
        {
            "font-sans",
            "-apple-system, BlinkMacSystemFont, avenir next, avenir, segoe ui, helvetica neue, helvetica, Ubuntu, roboto, noto, arial, sans-serif"
        },
        {
            "font-serif",
            "Iowan Old Style, Apple Garamond, Baskerville, Times New Roman, Droid Serif, Times, Source Serif Pro, serif, Apple Color Emoji, Segoe UI Emoji, Segoe UI Symbol"
        },
        { "font-mono", "Cascadia Code, Menlo, Consolas, Monaco, Liberation Mono, Lucida Console, monospace" },
    }.ToImmutableDictionary();
}

/// <summary>
/// The font-weight plugin.
/// </summary>
public class FontWeight : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-weight";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>()
    {
        { "font-thin", "100" },
        { "font-extralight", "200" },
        { "font-light", "300" },
        { "font-normal", "400" },
        { "font-medium", "500" },
        { "font-semibold", "600" },
        { "font-bold", "700" },
        { "font-extrabold", "800" },
        { "font-black", "900" },
    }.ToImmutableDictionary();
}