using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The word break plugin.
/// </summary>
public class WordBreak : BaseLookupPlugin
{
    /// <inheritdoc />
    protected override ImmutableDictionary<string, CssDeclarationList> GetLookups()
    {
        return new Dictionary<string, CssDeclarationList>
        {
            {
                "break-normal", [("overflow-wrap", "normal"), ("break-words", "normal")]
            },
            {
                "break-words", [("overflow-wrap", "break-word")]
            },
            {
                "break-all", [("word-break", "break-all")]
            },
        }.ToImmutableDictionary();
    }
}