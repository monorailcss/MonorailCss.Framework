using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling how a replaced element's content should be resized.
/// </summary>
internal class ObjectFitUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "object-contain", ("object-fit", "contain") },
            { "object-cover", ("object-fit", "cover") },
            { "object-fill", ("object-fit", "fill") },
            { "object-none", ("object-fit", "none") },
            { "object-scale-down", ("object-fit", "scale-down") },
        }.ToImmutableDictionary();
}