using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the alignment of text.
/// </summary>
internal class TextAlignUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "text-left", ("text-align", "left") },
            { "text-center", ("text-align", "center") },
            { "text-right", ("text-align", "right") },
            { "text-justify", ("text-align", "justify") },
            { "text-start", ("text-align", "start") },
            { "text-end", ("text-align", "end") },
        }.ToImmutableDictionary();
}