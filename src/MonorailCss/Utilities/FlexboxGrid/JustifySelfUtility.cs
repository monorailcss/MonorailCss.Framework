using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles justify self utilities (justify-self-auto, justify-self-start, justify-self-end, justify-self-center, justify-self-stretch).
/// </summary>
internal class JustifySelfUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "justify-self-auto", ("justify-self", "auto") },
            { "justify-self-start", ("justify-self", "start") },
            { "justify-self-end", ("justify-self", "end") },
            { "justify-self-center", ("justify-self", "center") },
            { "justify-self-stretch", ("justify-self", "stretch") },
        }.ToImmutableDictionary();
}