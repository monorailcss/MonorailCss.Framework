using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the ending position of gradient color stops.
/// </summary>
internal class GradientToPositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["to-0%"] = ("--tw-gradient-to-position", "0%"),
            ["to-5%"] = ("--tw-gradient-to-position", "5%"),
            ["to-10%"] = ("--tw-gradient-to-position", "10%"),
            ["to-15%"] = ("--tw-gradient-to-position", "15%"),
            ["to-20%"] = ("--tw-gradient-to-position", "20%"),
            ["to-25%"] = ("--tw-gradient-to-position", "25%"),
            ["to-30%"] = ("--tw-gradient-to-position", "30%"),
            ["to-35%"] = ("--tw-gradient-to-position", "35%"),
            ["to-40%"] = ("--tw-gradient-to-position", "40%"),
            ["to-45%"] = ("--tw-gradient-to-position", "45%"),
            ["to-50%"] = ("--tw-gradient-to-position", "50%"),
            ["to-55%"] = ("--tw-gradient-to-position", "55%"),
            ["to-60%"] = ("--tw-gradient-to-position", "60%"),
            ["to-65%"] = ("--tw-gradient-to-position", "65%"),
            ["to-70%"] = ("--tw-gradient-to-position", "70%"),
            ["to-75%"] = ("--tw-gradient-to-position", "75%"),
            ["to-80%"] = ("--tw-gradient-to-position", "80%"),
            ["to-85%"] = ("--tw-gradient-to-position", "85%"),
            ["to-90%"] = ("--tw-gradient-to-position", "90%"),
            ["to-95%"] = ("--tw-gradient-to-position", "95%"),
            ["to-100%"] = ("--tw-gradient-to-position", "100%"),
        });

    /// <summary>
    /// This utility contributes to the background-image CSS property via gradients.
    /// </summary>
    public string[] GetDocumentedProperties() => ["background-image"];
}