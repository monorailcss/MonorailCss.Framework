using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles list style image utilities (list-image-none).
/// </summary>
internal class ListStyleImageUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "list-image-none", ("list-style-image", "none") },
        }.ToImmutableDictionary();
}