using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles text underline offset utilities.
/// Maps underline-offset-* to text-underline-offset CSS property with specific pixel values.
/// </summary>
internal class TextUnderlineOffsetUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "underline-offset-auto", ("text-underline-offset", "auto") },
            { "underline-offset-0", ("text-underline-offset", "0px") },
            { "underline-offset-1", ("text-underline-offset", "1px") },
            { "underline-offset-2", ("text-underline-offset", "2px") },
            { "underline-offset-4", ("text-underline-offset", "4px") },
            { "underline-offset-8", ("text-underline-offset", "8px") },
        }.ToImmutableDictionary();
}