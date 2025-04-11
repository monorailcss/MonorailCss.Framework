using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-family plugin.
/// </summary>
public class FontFamily : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-family";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
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