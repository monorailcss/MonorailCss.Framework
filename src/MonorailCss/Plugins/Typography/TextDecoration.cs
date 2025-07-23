using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text decoration plugin.
/// </summary>
public class TextDecoration : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "text-decoration-line";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>()
        {
            { "underline", "underline" },
            { "overline", "overline" },
            { "line-through", "line-through" },
            { "no-underline", "none" },
        }.ToImmutableDictionary();
    }
}