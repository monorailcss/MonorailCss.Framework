using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles text-transform utilities (uppercase, lowercase, etc.).
/// </summary>
internal class TextTransformUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "uppercase", ("text-transform", "uppercase") },
            { "lowercase", ("text-transform", "lowercase") },
            { "capitalize", ("text-transform", "capitalize") },
            { "normal-case", ("text-transform", "none") },
        }.ToImmutableDictionary();
}