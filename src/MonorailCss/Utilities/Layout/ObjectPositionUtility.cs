using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles object-position utilities (object-bottom, object-center, object-left, etc.).
/// </summary>
internal class ObjectPositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "object-bottom", ("object-position", "bottom") },
            { "object-bottom-left", ("object-position", "left bottom") },
            { "object-bottom-right", ("object-position", "right bottom") },
            { "object-center", ("object-position", "center") },
            { "object-left", ("object-position", "left") },
            { "object-left-bottom", ("object-position", "left bottom") },
            { "object-left-top", ("object-position", "left top") },
            { "object-right", ("object-position", "right") },
            { "object-right-bottom", ("object-position", "right bottom") },
            { "object-right-top", ("object-position", "right top") },
            { "object-top", ("object-position", "top") },
            { "object-top-left", ("object-position", "left top") },
            { "object-top-right", ("object-position", "right top") },
        }.ToImmutableDictionary();
}