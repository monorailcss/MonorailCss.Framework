using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling how an element is positioned in the DOM.
/// </summary>
internal class PositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "static", ("position", "static") },
            { "fixed", ("position", "fixed") },
            { "absolute", ("position", "absolute") },
            { "relative", ("position", "relative") },
            { "sticky", ("position", "sticky") },
        }.ToImmutableDictionary();
}