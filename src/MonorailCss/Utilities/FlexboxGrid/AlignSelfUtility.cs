using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles align-self utilities (self-center, self-start, etc.).
/// </summary>
internal class AlignSelfUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "self-auto", ("align-self", "auto") },
            { "self-center", ("align-self", "center") },
            { "self-start", ("align-self", "flex-start") },
            { "self-end", ("align-self", "flex-end") },
            { "self-baseline", ("align-self", "baseline") },
            { "self-stretch", ("align-self", "stretch") },
        }.ToImmutableDictionary();
}