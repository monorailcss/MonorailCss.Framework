using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-smoothing plugin.
/// </summary>
public class FontSmoothing : BaseLookupPlugin
{
    /// <inheritdoc />
    protected override ImmutableDictionary<string, CssDeclarationList> GetLookups()
    {
        return new Dictionary<string, CssDeclarationList>
        {
            {
                "antialiased", [
                    ("-webkit-font-smoothing", "antialiased"), ("-moz-osx-font-smoothing", "grayscale"),
                ]
            },
            {
                "subpixel-antialiased", [
                    ("-webkit-font-smoothing", "auto"), ("-moz-osx-font-smoothing", "auto"),
                ]
            },
        }.ToImmutableDictionary();
    }
}