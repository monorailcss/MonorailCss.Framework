using System.Collections.Immutable;

namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-blur plugin.
/// </summary>
public class BackdropBlur : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "backdrop-filter";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-blur-none", "blur(0)" },
            { "backdrop-blur-sm", "blur(4px)" },
            { "backdrop-blur", "blur(8px)" },
            { "backdrop-blur-md", "blur(12px)" },
            { "backdrop-blur-lg", "blur(16px)" },
            { "backdrop-blur-xl", "blur(24px)" },
            { "backdrop-blur-2xl", "blur(40px)" },
            { "backdrop-blur-3xl", "blur(64px)" },
        }.ToImmutableDictionary();
}