using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles flex-wrap utilities (flex-wrap, flex-nowrap, flex-wrap-reverse).
/// </summary>
internal class FlexWrapUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "flex-wrap", ("flex-wrap", "wrap") },
            { "flex-nowrap", ("flex-wrap", "nowrap") },
            { "flex-wrap-reverse", ("flex-wrap", "wrap-reverse") },
        }.ToImmutableDictionary();
}