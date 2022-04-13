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
            { "break-normal", new CssDeclarationList { new("overflow-wrap", "normal"), new("break-words", "normal"), } },
            { "break-words", new CssDeclarationList { new("overflow-wrap", "break-word") } },
            { "break-all", new CssDeclarationList { new("word-break", "break-all") } },
        }.ToImmutableDictionary();
    }
}