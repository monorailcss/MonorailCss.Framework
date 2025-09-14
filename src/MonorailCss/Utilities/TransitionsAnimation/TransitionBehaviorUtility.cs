using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Handles transition-behavior utilities (transition-normal, transition-discrete).
/// </summary>
internal class TransitionBehaviorUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "transition-normal", ("transition-behavior", "normal") },
            { "transition-discrete", ("transition-behavior", "allow-discrete") },
        }.ToImmutableDictionary();
}