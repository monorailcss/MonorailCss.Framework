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
                "antialiased", new CssDeclarationList
                {
                    new("-webkit-font-smoothing", "antialiased"), new("-moz-osx-font-smoothing", "grayscale"),
                }
            },
            {
                "subpixel-antialiased", new CssDeclarationList
                {
                    new("-webkit-font-smoothing", "auto"), new("-moz-osx-font-smoothing", "auto"),
                }
            },
        }.ToImmutableDictionary();
    }
}