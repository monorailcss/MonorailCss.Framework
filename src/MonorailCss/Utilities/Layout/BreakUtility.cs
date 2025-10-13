using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling how a column or page break should behave.
/// </summary>
internal class BreakUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // Break Before utilities
            { "break-before-auto", ("break-before", "auto") },
            { "break-before-avoid", ("break-before", "avoid") },
            { "break-before-all", ("break-before", "all") },
            { "break-before-avoid-page", ("break-before", "avoid-page") },
            { "break-before-page", ("break-before", "page") },
            { "break-before-left", ("break-before", "left") },
            { "break-before-right", ("break-before", "right") },
            { "break-before-column", ("break-before", "column") },

            // Break After utilities
            { "break-after-auto", ("break-after", "auto") },
            { "break-after-avoid", ("break-after", "avoid") },
            { "break-after-all", ("break-after", "all") },
            { "break-after-avoid-page", ("break-after", "avoid-page") },
            { "break-after-page", ("break-after", "page") },
            { "break-after-left", ("break-after", "left") },
            { "break-after-right", ("break-after", "right") },
            { "break-after-column", ("break-after", "column") },

            // Break Inside utilities
            { "break-inside-auto", ("break-inside", "auto") },
            { "break-inside-avoid", ("break-inside", "avoid") },
            { "break-inside-avoid-page", ("break-inside", "avoid-page") },
            { "break-inside-avoid-column", ("break-inside", "avoid-column") },
        }.ToImmutableDictionary();
}