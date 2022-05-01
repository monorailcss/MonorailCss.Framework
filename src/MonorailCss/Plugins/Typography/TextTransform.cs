using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-transform plugin.
/// </summary>
public class TextTransform : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "text-transform";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "uppercase", "uppercase" },
            { "lowercase", "lowercase" },
            { "capitalize", "capitalize" },
            { "normal-case", "none" },
        }.ToImmutableDictionary();
    }
}