using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask radial shape and size utilities.
/// Sets the --tw-mask-radial-shape and --tw-mask-radial-size CSS variables.
/// </summary>
internal class MaskRadialShapeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // Shape utilities
            { "mask-circle", ("--tw-mask-radial-shape", "circle") },
            { "mask-ellipse", ("--tw-mask-radial-shape", "ellipse") },

            // Size utilities
            { "mask-radial-closest-corner", ("--tw-mask-radial-size", "closest-corner") },
            { "mask-radial-closest-side", ("--tw-mask-radial-size", "closest-side") },
            { "mask-radial-farthest-corner", ("--tw-mask-radial-size", "farthest-corner") },
            { "mask-radial-farthest-side", ("--tw-mask-radial-size", "farthest-side") },
        }.ToImmutableDictionary();
}