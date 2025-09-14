using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Handles field-sizing utilities (field-sizing-content, field-sizing-fixed).
/// Maps to the CSS field-sizing property for modern form field sizing control.
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