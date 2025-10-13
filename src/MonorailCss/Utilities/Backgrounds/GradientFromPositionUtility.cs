using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the starting position of gradient color stops.
/// </summary>
internal class GradientFromPositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["from-0%"] = ("--tw-gradient-from-position", "0%"),
            ["from-5%"] = ("--tw-gradient-from-position", "5%"),
            ["from-10%"] = ("--tw-gradient-from-position", "10%"),
            ["from-15%"] = ("--tw-gradient-from-position", "15%"),
            ["from-20%"] = ("--tw-gradient-from-position", "20%"),
            ["from-25%"] = ("--tw-gradient-from-position", "25%"),
            ["from-30%"] = ("--tw-gradient-from-position", "30%"),
            ["from-35%"] = ("--tw-gradient-from-position", "35%"),
            ["from-40%"] = ("--tw-gradient-from-position", "40%"),
            ["from-45%"] = ("--tw-gradient-from-position", "45%"),
            ["from-50%"] = ("--tw-gradient-from-position", "50%"),
            ["from-55%"] = ("--tw-gradient-from-position", "55%"),
            ["from-60%"] = ("--tw-gradient-from-position", "60%"),
            ["from-65%"] = ("--tw-gradient-from-position", "65%"),
            ["from-70%"] = ("--tw-gradient-from-position", "70%"),
            ["from-75%"] = ("--tw-gradient-from-position", "75%"),
            ["from-80%"] = ("--tw-gradient-from-position", "80%"),
            ["from-85%"] = ("--tw-gradient-from-position", "85%"),
            ["from-90%"] = ("--tw-gradient-from-position", "90%"),
            ["from-95%"] = ("--tw-gradient-from-position", "95%"),
            ["from-100%"] = ("--tw-gradient-from-position", "100%"),
        });
}