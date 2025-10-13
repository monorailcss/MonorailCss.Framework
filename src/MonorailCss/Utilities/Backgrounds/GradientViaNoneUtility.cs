using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for removing intermediate gradient color stops.
/// </summary>
internal class GradientViaNoneUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["via-none"] = ("--tw-gradient-via-stops", "initial"),
        });

    /// <summary>
    /// This utility contributes to the background-image CSS property via gradients.
    /// </summary>
    public string[]? GetDocumentedProperties() => ["background-image"];
}