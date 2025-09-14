using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles isolation utilities (isolate, isolation-auto).
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