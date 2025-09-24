using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles the via-none utility to reset gradient via stops.
/// Sets --tw-gradient-via-stops to initial, removing intermediate color stops.
/// CSS: --tw-gradient-via-stops: initial.
/// </summary>
internal class GradientViaNoneUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["via-none"] = ("--tw-gradient-via-stops", "initial"),
        });
}