using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles text-wrap utilities (text-wrap, text-nowrap, text-balance, text-pretty).
/// </summary>
internal class TextWrapUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "text-wrap", ("text-wrap", "wrap") },
            { "text-nowrap", ("text-wrap", "nowrap") },
            { "text-balance", ("text-wrap", "balance") },
            { "text-pretty", ("text-wrap", "pretty") },
        }.ToImmutableDictionary();
}