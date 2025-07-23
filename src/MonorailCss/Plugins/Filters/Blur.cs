using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The blur filter plugin.
/// </summary>
public class Blur : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "blur-none", string.Empty },
            { "blur-sm", "blur(4px)" },
            { "blur", "blur(8px)" },
            { "blur-md", "blur(12px)" },
            { "blur-lg", "blur(16px)" },
            { "blur-xl", "blur(24px)" },
            { "blur-2xl", "blur(40px)" },
            { "blur-3xl", "blur(64px)" },
        }.ToImmutableDictionary();
}