using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles gradient via position utilities (via-0% through via-100%).
/// Sets the middle position for gradient color stops.
/// CSS: --tw-gradient-via-position: value.
/// </summary>
internal class GradientViaPositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["via-0%"] = ("--tw-gradient-via-position", "0%"),
            ["via-5%"] = ("--tw-gradient-via-position", "5%"),
            ["via-10%"] = ("--tw-gradient-via-position", "10%"),
            ["via-15%"] = ("--tw-gradient-via-position", "15%"),
            ["via-20%"] = ("--tw-gradient-via-position", "20%"),
            ["via-25%"] = ("--tw-gradient-via-position", "25%"),
            ["via-30%"] = ("--tw-gradient-via-position", "30%"),
            ["via-35%"] = ("--tw-gradient-via-position", "35%"),
            ["via-40%"] = ("--tw-gradient-via-position", "40%"),
            ["via-45%"] = ("--tw-gradient-via-position", "45%"),
            ["via-50%"] = ("--tw-gradient-via-position", "50%"),
            ["via-55%"] = ("--tw-gradient-via-position", "55%"),
            ["via-60%"] = ("--tw-gradient-via-position", "60%"),
            ["via-65%"] = ("--tw-gradient-via-position", "65%"),
            ["via-70%"] = ("--tw-gradient-via-position", "70%"),
            ["via-75%"] = ("--tw-gradient-via-position", "75%"),
            ["via-80%"] = ("--tw-gradient-via-position", "80%"),
            ["via-85%"] = ("--tw-gradient-via-position", "85%"),
            ["via-90%"] = ("--tw-gradient-via-position", "90%"),
            ["via-95%"] = ("--tw-gradient-via-position", "95%"),
            ["via-100%"] = ("--tw-gradient-via-position", "100%"),
        });
}