using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Unified mask utility that handles all mask-related CSS properties including:
/// mask-clip, mask-composite, mask-image, mask-mode, mask-origin, mask-position,
/// mask-repeat, mask-size, mask-type, and gradient utilities (linear, radial, conic).
/// </summary>
internal class MaskUtility : IUtility
{
    private static readonly Dictionary<string, (string Property, string Value)> _staticMappings = new()
    {
        // mask-clip utilities
        ["mask-clip-border"] = ("mask-clip", "border-box"),
        ["mask-clip-padding"] = ("mask-clip", "padding-box"),
        ["mask-clip-content"] = ("mask-clip", "content-box"),
        ["mask-clip-fill"] = ("mask-clip", "fill-box"),
        ["mask-clip-stroke"] = ("mask-clip", "stroke-box"),
        ["mask-clip-view"] = ("mask-clip", "view-box"),
        ["mask-no-clip"] = ("mask-clip", "no-clip"),

        // mask-composite utilities
        ["mask-add"] = ("mask-composite", "add"),
        ["mask-subtract"] = ("mask-composite", "subtract"),
        ["mask-intersect"] = ("mask-composite", "intersect"),
        ["mask-exclude"] = ("mask-composite", "exclude"),

        // mask-image utilities
        ["mask-none"] = ("mask-image", "none"),

        // mask-mode utilities
        ["mask-alpha"] = ("mask-mode", "alpha"),
        ["mask-luminance"] = ("mask-mode", "luminance"),
        ["mask-match"] = ("mask-mode", "match-source"),

        // mask-origin utilities
        ["mask-origin-border"] = ("mask-origin", "border-box"),
        ["mask-origin-padding"] = ("mask-origin", "padding-box"),
        ["mask-origin-content"] = ("mask-origin", "content-box"),
        ["mask-origin-fill"] = ("mask-origin", "fill-box"),
        ["mask-origin-stroke"] = ("mask-origin", "stroke-box"),
        ["mask-origin-view"] = ("mask-origin", "view-box"),

        // mask-position utilities
        ["mask-top-left"] = ("mask-position", "left top"),
        ["mask-top"] = ("mask-position", "top"),
        ["mask-top-right"] = ("mask-position", "right top"),
        ["mask-left"] = ("mask-position", "left"),
        ["mask-center"] = ("mask-position", "center"),
        ["mask-right"] = ("mask-position", "right"),
        ["mask-bottom-left"] = ("mask-position", "left bottom"),
        ["mask-bottom"] = ("mask-position", "bottom"),
        ["mask-bottom-right"] = ("mask-position", "right bottom"),

        // mask-repeat utilities
        ["mask-repeat"] = ("mask-repeat", "repeat"),
        ["mask-no-repeat"] = ("mask-repeat", "no-repeat"),
        ["mask-repeat-x"] = ("mask-repeat", "repeat-x"),
        ["mask-repeat-y"] = ("mask-repeat", "repeat-y"),
        ["mask-repeat-round"] = ("mask-repeat", "round"),
        ["mask-repeat-space"] = ("mask-repeat", "space"),

        // mask-size utilities
        ["mask-auto"] = ("mask-size", "auto"),
        ["mask-cover"] = ("mask-size", "cover"),
        ["mask-contain"] = ("mask-size", "contain"),

        // mask-type utilities
        ["mask-type-luminance"] = ("mask-type", "luminance"),
        ["mask-type-alpha"] = ("mask-type", "alpha"),

        // mask gradient utilities
        ["mask-gradient"] = ("mask-image", "none"),
        ["mask-gradient-none"] = ("mask-image", "none"),

        // mask shape utilities (radial shortcuts)
        ["mask-circle"] = ("--tw-mask-radial-shape", "circle"),
        ["mask-ellipse"] = ("--tw-mask-radial-shape", "ellipse"),
    };

    private static readonly HashSet<string> FunctionalPatterns =
    [
        "mask-linear",
        "mask-linear-from",
        "mask-linear-to",
        "mask-radial",
        "mask-radial-from",
        "mask-radial-to",
        "mask-radial-position",
        "mask-radial-shape",
        "mask-conic",
        "mask-conic-from",
        "mask-conic-to",
        "mask-directional",
        "mask-directional-from",
        "mask-directional-to",

        // Directional mask patterns
        "mask-b-from", "mask-b-to", // bottom
        "mask-t-from", "mask-t-to", // top
        "mask-l-from", "mask-l-to", // left
        "mask-r-from", "mask-r-to", // right
        "mask-x-from", "mask-x-to", // horizontal
        "mask-y-from", "mask-y-to", // vertical

        // Additional radial patterns
        "mask-radial-at",
        "mask-radial-closest", "mask-radial-farthest",
    ];

    /// <inheritdoc/>
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    /// <inheritdoc/>
    public string[] GetNamespaces() => [];

    /// <inheritdoc/>
    public string[] GetFunctionalRoots() => FunctionalPatterns.ToArray();

    /// <inheritdoc/>
    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        // Handle static utilities
        if (candidate is StaticUtility staticUtility)
        {
            if (_staticMappings.TryGetValue(staticUtility.Root, out var staticMapping))
            {
                results = ImmutableList.Create<AstNode>(
                    new Declaration(staticMapping.Property, staticMapping.Value, candidate.Important));
                return true;
            }
        }

        // Handle functional utilities
        if (candidate is FunctionalUtility functionalUtility)
        {
            var root = functionalUtility.Root;

            // Check for functional patterns
            if (FunctionalPatterns.Contains(root))
            {
                results = CompileFunctionalPattern(root, functionalUtility, theme);
                return results.Count > 0;
            }

            // Check for negative patterns
            if (root.StartsWith('-'))
            {
                var positiveRoot = root.Substring(1);
                if (FunctionalPatterns.Contains(positiveRoot))
                {
                    results = CompileFunctionalPattern(positiveRoot, functionalUtility, theme, true);
                    return results.Count > 0;
                }
            }
        }

        return false;
    }

    private ImmutableList<AstNode> CompileFunctionalPattern(string pattern, FunctionalUtility functionalUtility, Theme.Theme theme, bool isNegative = false)
    {
        var value = functionalUtility.Value?.Value ?? string.Empty;

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        var important = functionalUtility.Important;

        switch (pattern)
        {
            case "mask-linear":
                if (TryParseAngle(value, isNegative, out var linearAngle))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops, var(--tw-mask-linear-position)))", important));
                    declarations.Add(new Declaration("--tw-mask-linear-position", linearAngle, important));
                }

                break;

            case "mask-linear-from":
                if (TryParsePosition(value, out var fromPosition))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-linear-stops", "var(--tw-mask-linear-position), var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-linear-from-position", fromPosition, important));
                }

                break;

            case "mask-linear-to":
                if (TryParsePosition(value, out var toPosition))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-linear-stops", "var(--tw-mask-linear-position), var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-linear-to-position", toPosition, important));
                }

                break;

            case "mask-radial":
                declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                declarations.Add(new Declaration("mask-composite", "intersect", important));
                declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                break;

            case "mask-radial-from":
                if (TryParsePosition(value, out var radialFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-radial-stops", "var(--tw-mask-radial-from-color) var(--tw-mask-radial-from-position), var(--tw-mask-radial-to-color) var(--tw-mask-radial-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-radial-from-position", radialFromPos, important));
                }

                break;

            case "mask-radial-to":
                if (TryParsePosition(value, out var radialToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-radial-stops", "var(--tw-mask-radial-from-color) var(--tw-mask-radial-from-position), var(--tw-mask-radial-to-color) var(--tw-mask-radial-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-radial-to-position", radialToPos, important));
                }

                break;

            case "mask-radial-position":
                if (TryParseRadialPosition(value, out var radialPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-radial-position", radialPos, important));
                }

                break;

            case "mask-radial-shape":
                if (value == "circle" || value == "ellipse")
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-radial-shape", value, important));
                }

                break;

            case "mask-conic":
                if (TryParseAngle(value, isNegative, out var conicAngle))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(from var(--tw-mask-conic-position) at var(--tw-mask-conic-center), var(--tw-mask-conic-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-conic-position", conicAngle, important));
                }

                break;

            case "mask-conic-from":
                if (TryParsePosition(value, out var conicFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-conic-stops", "from var(--tw-mask-conic-position), var(--tw-mask-conic-from-color) var(--tw-mask-conic-from-position), var(--tw-mask-conic-to-color) var(--tw-mask-conic-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(var(--tw-mask-conic-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-conic-from-position", conicFromPos, important));
                }

                break;

            case "mask-conic-to":
                if (TryParsePosition(value, out var conicToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-conic-stops", "from var(--tw-mask-conic-position), var(--tw-mask-conic-from-color) var(--tw-mask-conic-from-position), var(--tw-mask-conic-to-color) var(--tw-mask-conic-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(var(--tw-mask-conic-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-conic-to-position", conicToPos, important));
                }

                break;

            case "mask-directional":
                if (TryParseDirection(value, out var direction))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops, var(--tw-mask-linear-position)))", important));
                    declarations.Add(new Declaration("--tw-mask-linear-position", direction, important));
                }

                break;

            case "mask-directional-from":
                if (TryParsePosition(value, out var dirFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-linear-stops", "var(--tw-mask-linear-position), var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-linear-from-position", dirFromPos, important));
                }

                break;

            case "mask-directional-to":
                if (TryParsePosition(value, out var dirToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-linear-stops", "var(--tw-mask-linear-position), var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)", important));
                    declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-linear-to-position", dirToPos, important));
                }

                break;

            // Bottom mask patterns
            case "mask-b-from":
                if (TryParsePosition(value, out var bFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-bottom", "linear-gradient(to top, transparent var(--tw-mask-bottom-from-position), black var(--tw-mask-bottom-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-bottom-from-position", bFromPos, important));
                }

                break;

            case "mask-b-to":
                if (TryParsePosition(value, out var bToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-bottom", "linear-gradient(to top, transparent var(--tw-mask-bottom-from-position), black var(--tw-mask-bottom-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-bottom-to-position", bToPos, important));
                }

                break;

            // Top mask patterns
            case "mask-t-from":
                if (TryParsePosition(value, out var tFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-top", "linear-gradient(to bottom, transparent var(--tw-mask-top-from-position), black var(--tw-mask-top-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-top-from-position", tFromPos, important));
                }

                break;

            case "mask-t-to":
                if (TryParsePosition(value, out var tToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-top", "linear-gradient(to bottom, transparent var(--tw-mask-top-from-position), black var(--tw-mask-top-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-top-to-position", tToPos, important));
                }

                break;

            // Left/Right mask patterns (similar pattern)
            case "mask-l-from":
                if (TryParsePosition(value, out var lFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-left", "linear-gradient(to right, transparent var(--tw-mask-left-from-position), black var(--tw-mask-left-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-left-from-position", lFromPos, important));
                }

                break;

            case "mask-l-to":
                if (TryParsePosition(value, out var lToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-left", "linear-gradient(to right, transparent var(--tw-mask-left-from-position), black var(--tw-mask-left-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-left-to-position", lToPos, important));
                }

                break;

            case "mask-r-from":
                if (TryParsePosition(value, out var rFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-right", "linear-gradient(to left, transparent var(--tw-mask-right-from-position), black var(--tw-mask-right-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-right-from-position", rFromPos, important));
                }

                break;

            case "mask-r-to":
                if (TryParsePosition(value, out var rToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-right", "linear-gradient(to left, transparent var(--tw-mask-right-from-position), black var(--tw-mask-right-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-right-to-position", rToPos, important));
                }

                break;

            // X/Y axis patterns (combine horizontal/vertical)
            case "mask-x-from":
                if (TryParsePosition(value, out var xFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-left", "linear-gradient(to right, transparent var(--tw-mask-left-from-position), black var(--tw-mask-left-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-right", "linear-gradient(to left, transparent var(--tw-mask-right-from-position), black var(--tw-mask-right-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-left-from-position", xFromPos, important));
                    declarations.Add(new Declaration("--tw-mask-right-from-position", xFromPos, important));
                }

                break;

            case "mask-x-to":
                if (TryParsePosition(value, out var xToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-left", "linear-gradient(to right, transparent var(--tw-mask-left-from-position), black var(--tw-mask-left-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-right", "linear-gradient(to left, transparent var(--tw-mask-right-from-position), black var(--tw-mask-right-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-left-to-position", xToPos, important));
                    declarations.Add(new Declaration("--tw-mask-right-to-position", xToPos, important));
                }

                break;

            case "mask-y-from":
                if (TryParsePosition(value, out var yFromPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-top", "linear-gradient(to bottom, transparent var(--tw-mask-top-from-position), black var(--tw-mask-top-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-bottom", "linear-gradient(to top, transparent var(--tw-mask-bottom-from-position), black var(--tw-mask-bottom-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-top-from-position", yFromPos, important));
                    declarations.Add(new Declaration("--tw-mask-bottom-from-position", yFromPos, important));
                }

                break;

            case "mask-y-to":
                if (TryParsePosition(value, out var yToPos))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-top", "linear-gradient(to bottom, transparent var(--tw-mask-top-from-position), black var(--tw-mask-top-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-bottom", "linear-gradient(to top, transparent var(--tw-mask-bottom-from-position), black var(--tw-mask-bottom-to-position))", important));
                    declarations.Add(new Declaration("--tw-mask-top-to-position", yToPos, important));
                    declarations.Add(new Declaration("--tw-mask-bottom-to-position", yToPos, important));
                }

                break;

            // Additional radial patterns
            case "mask-radial-at":
                if (TryParseRadialPosition(value, out var atPosition))
                {
                    declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                    declarations.Add(new Declaration("mask-composite", "intersect", important));
                    declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                    declarations.Add(new Declaration("--tw-mask-radial-position", atPosition, important));
                }

                break;

            case "mask-radial-closest":
                declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                declarations.Add(new Declaration("mask-composite", "intersect", important));
                declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                declarations.Add(new Declaration("--tw-mask-radial-size", value.Contains("corner") ? "closest-corner" : "closest-side", important));
                break;

            case "mask-radial-farthest":
                declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));
                declarations.Add(new Declaration("mask-composite", "intersect", important));
                declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));
                declarations.Add(new Declaration("--tw-mask-radial-size", value.Contains("corner") ? "farthest-corner" : "farthest-side", important));
                break;
        }

        return declarations.ToImmutable();
    }

    private bool TryParseAngle(string value, bool isNegative, out string angle)
    {
        angle = string.Empty;
        if (int.TryParse(value, out var angleNum))
        {
            angle = isNegative ? $"calc(1deg * -{angleNum})" : $"calc(1deg * {angleNum})";
            return true;
        }

        return false;
    }

    private bool TryParsePosition(string value, out string position)
    {
        position = string.Empty;
        if (value.EndsWith('%'))
        {
            position = value;
            return true;
        }

        return false;
    }

    private bool TryParseRadialPosition(string value, out string position)
    {
        position = string.Empty;
        var positions = new Dictionary<string, string>
        {
            ["center"] = "center",
            ["top-left"] = "left top",
            ["top"] = "top",
            ["top-right"] = "right top",
            ["left"] = "left",
            ["right"] = "right",
            ["bottom-left"] = "left bottom",
            ["bottom"] = "bottom",
            ["bottom-right"] = "right bottom",
        };

        if (positions.TryGetValue(value, out var pos))
        {
            position = pos;
            return true;
        }

        return false;
    }

    private bool TryParseDirection(string value, out string direction)
    {
        direction = string.Empty;
        var directions = new Dictionary<string, string>
        {
            ["to-t"] = "to top",
            ["to-tr"] = "to top right",
            ["to-r"] = "to right",
            ["to-br"] = "to bottom right",
            ["to-b"] = "to bottom",
            ["to-bl"] = "to bottom left",
            ["to-l"] = "to left",
            ["to-tl"] = "to top left",
        };

        if (directions.TryGetValue(value, out var dir))
        {
            direction = dir;
            return true;
        }

        return false;
    }
}