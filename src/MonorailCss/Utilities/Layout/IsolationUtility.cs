using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling whether an element should create a new stacking context.
/// </summary>
internal class IsolationUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "isolate", ("isolation", "isolate") },
            { "isolation-auto", ("isolation", "auto") },
        }.ToImmutableDictionary();
}