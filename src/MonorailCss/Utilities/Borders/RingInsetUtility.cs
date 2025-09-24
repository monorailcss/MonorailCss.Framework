using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles ring-inset utility.
/// Sets rings to render as inset rather than outset.
/// </summary>
internal class RingInsetUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["ring-inset"] = ("--tw-ring-inset", "inset"),
        });
}