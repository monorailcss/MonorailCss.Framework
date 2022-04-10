using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The line-height plugin.
/// </summary>
public class LineHeight : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "line-height";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>()
    {
        { "leading-3", ".75rem" }, /* 12px */
        { "leading-4", "1rem" }, /* 16px */
        { "leading-5", "1.25rem" }, /* 20px */
        { "leading-6", "1.5rem" }, /* 24px */
        { "leading-7", "1.75rem" }, /* 28px */
        { "leading-8", "2rem" }, /* 32px */
        { "leading-9", "2.25rem" }, /* 36px */
        { "leading-10", "2.5rem" }, /* 40px */
        { "leading-none", "1" },
        { "leading-tight", "1.25" },
        { "leading-snug", "1.375" },
        { "leading-normal", "1.5" },
        { "leading-relaxed", "1.625" },
        { "leading-loose", "2" },
    }.ToImmutableDictionary();
}