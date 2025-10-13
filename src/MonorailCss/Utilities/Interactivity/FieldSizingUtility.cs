using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling how form field heights are determined.
/// </summary>
internal class FieldSizingUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "field-sizing-content", ("field-sizing", "content") },
            { "field-sizing-fixed", ("field-sizing", "fixed") },
        }.ToImmutableDictionary();
}