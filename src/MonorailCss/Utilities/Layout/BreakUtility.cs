using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles break utilities for page break control (break-before-*, break-after-*, break-inside-*).
/// </summary>
internal class BreakUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // Break Before utilities
            { "break-before-auto", ("break-before", "auto") },
            { "break-before-avoid", ("break-before", "avoid") },
            { "break-before-page", ("break-before", "page") },

            // Break After utilities
            { "break-after-auto", ("break-after", "auto") },
            { "break-after-avoid", ("break-after", "avoid") },
            { "break-after-page", ("break-after", "page") },

            // Break Inside utilities
            { "break-inside-auto", ("break-inside", "auto") },
            { "break-inside-avoid", ("break-inside", "avoid") },
            { "break-inside-avoid-page", ("break-inside", "avoid-page") },
        }.ToImmutableDictionary();
}