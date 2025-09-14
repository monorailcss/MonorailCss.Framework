using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles text-decoration utilities (underline, line-through, etc.).
/// </summary>
internal class TextDecorationUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "underline", ("text-decoration-line", "underline") },
            { "overline", ("text-decoration-line", "overline") },
            { "line-through", ("text-decoration-line", "line-through") },
            { "no-underline", ("text-decoration-line", "none") },
        }.ToImmutableDictionary();
}