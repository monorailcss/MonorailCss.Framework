using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the cursor style when hovering over an element.
/// </summary>
internal class CursorUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["cursor"];

    protected override string[] ThemeKeys => ["cursor"];

    private static readonly ImmutableDictionary<string, string> _staticCursors =
        new Dictionary<string, string>
        {
            ["auto"] = "auto",
            ["default"] = "default",
            ["pointer"] = "pointer",
            ["wait"] = "wait",
            ["text"] = "text",
            ["move"] = "move",
            ["help"] = "help",
            ["not-allowed"] = "not-allowed",
            ["none"] = "none",
            ["context-menu"] = "context-menu",
            ["progress"] = "progress",
            ["cell"] = "cell",
            ["crosshair"] = "crosshair",
            ["vertical-text"] = "vertical-text",
            ["alias"] = "alias",
            ["copy"] = "copy",
            ["no-drop"] = "no-drop",
            ["grab"] = "grab",
            ["grabbing"] = "grabbing",
            ["all-scroll"] = "all-scroll",
            ["col-resize"] = "col-resize",
            ["row-resize"] = "row-resize",
            ["n-resize"] = "n-resize",
            ["e-resize"] = "e-resize",
            ["s-resize"] = "s-resize",
            ["w-resize"] = "w-resize",
            ["ne-resize"] = "ne-resize",
            ["nw-resize"] = "nw-resize",
            ["se-resize"] = "se-resize",
            ["sw-resize"] = "sw-resize",
            ["ew-resize"] = "ew-resize",
            ["ns-resize"] = "ns-resize",
            ["nesw-resize"] = "nesw-resize",
            ["nwse-resize"] = "nwse-resize",
            ["zoom-in"] = "zoom-in",
            ["zoom-out"] = "zoom-out",
        }.ToImmutableDictionary();

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("cursor", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        if (isNegative)
        {
            // Cursor values don't support negatives
            return false;
        }

        // Handle static cursor values first
        if (value.Kind == ValueKind.Named)
        {
            if (_staticCursors.TryGetValue(value.Value, out var staticValue))
            {
                resolvedValue = staticValue;
                return true;
            }
        }

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Convert underscores to spaces and handle the arbitrary value
            var processedValue = arbitrary.Replace("_", " ");

            resolvedValue = processedValue;
            return true;
        }

        // Try theme resolution last (allows theme extension of cursor values)
        return base.TryResolveValue(value, theme, isNegative, out resolvedValue);
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return true;
    }

    // Standard priority for functional utilities
    public override UtilityPriority Priority => UtilityPriority.StandardFunctional;

    protected override string GetSampleCssForArbitraryValue(string pattern) => "cursor: [value]";
}