using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the style of text.
/// </summary>
internal class FontStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "italic", ("font-style", "italic") },
            { "not-italic", ("font-style", "normal") },
        }.ToImmutableDictionary();
}