using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles whitespace utilities (whitespace-normal, whitespace-nowrap, etc.).
/// </summary>
internal class WhitespaceUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "whitespace-normal", ("white-space", "normal") },
            { "whitespace-nowrap", ("white-space", "nowrap") },
            { "whitespace-pre", ("white-space", "pre") },
            { "whitespace-pre-line", ("white-space", "pre-line") },
            { "whitespace-pre-wrap", ("white-space", "pre-wrap") },
            { "whitespace-break-spaces", ("white-space", "break-spaces") },
        }.ToImmutableDictionary();
}