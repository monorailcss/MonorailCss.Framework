using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling how an element is masked or clipped using mask images and gradients.
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

    private static readonly HashSet<string> _functionalPatterns =
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
    public string[] GetFunctionalRoots() => _functionalPatterns.ToArray();

    /// <summary>
    /// Gets the names of all static utilities handled by this class.
    /// Required for UtilityRegistry to properly index static utilities.
    /// </summary>
    public static string[] GetUtilityNames() => _staticMappings.Keys.ToArray();

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
            if (_functionalPatterns.Contains(root))
            {
                results = CompileFunctionalPattern(root, functionalUtility, theme);
                return results.Count > 0;
            }

            // Check for negative patterns
            if (root.StartsWith('-'))
            {
                var positiveRoot = root.Substring(1);
                if (_functionalPatterns.Contains(positiveRoot))
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
                    BuildLinearGradient(declarations, important, position: linearAngle);
                }

                break;

            case "mask-linear-from":
                if (TryParsePosition(value, out var fromPosition))
                {
                    BuildLinearGradient(declarations, important, fromPosition: fromPosition);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var linearFromColor))
                {
                    BuildLinearGradient(declarations, important, fromColor: linearFromColor);
                }

                break;

            case "mask-linear-to":
                if (TryParsePosition(value, out var toPosition))
                {
                    BuildLinearGradient(declarations, important, toPosition: toPosition);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var linearToColor))
                {
                    BuildLinearGradient(declarations, important, toColor: linearToColor);
                }

                break;

            case "mask-radial":
                BuildRadialGradient(declarations, important);
                break;

            case "mask-radial-from":
                if (TryParsePosition(value, out var radialFromPos))
                {
                    BuildRadialGradient(declarations, important, fromPosition: radialFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var radialFromColor))
                {
                    BuildRadialGradient(declarations, important, fromColor: radialFromColor);
                }

                break;

            case "mask-radial-to":
                if (TryParsePosition(value, out var radialToPos))
                {
                    BuildRadialGradient(declarations, important, toPosition: radialToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var radialToColor))
                {
                    BuildRadialGradient(declarations, important, toColor: radialToColor);
                }

                break;

            case "mask-radial-position":
            case "mask-radial-at":
                if (TryParseRadialPosition(value, out var radialPos))
                {
                    BuildRadialGradient(declarations, important, position: radialPos);
                }

                break;

            case "mask-radial-shape":
                if (value is "circle" or "ellipse")
                {
                    BuildRadialGradient(declarations, important, shape: value);
                }

                break;

            case "mask-radial-closest":
                BuildRadialGradient(declarations, important, size: value.Contains("corner") ? "closest-corner" : "closest-side");
                break;

            case "mask-radial-farthest":
                BuildRadialGradient(declarations, important, size: value.Contains("corner") ? "farthest-corner" : "farthest-side");
                break;

            case "mask-conic":
                if (TryParseAngle(value, isNegative, out var conicAngle))
                {
                    BuildConicGradient(declarations, important, position: conicAngle);
                }

                break;

            case "mask-conic-from":
                if (TryParsePosition(value, out var conicFromPos))
                {
                    BuildConicGradient(declarations, important, fromPosition: conicFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var conicFromColor))
                {
                    BuildConicGradient(declarations, important, fromColor: conicFromColor);
                }

                break;

            case "mask-conic-to":
                if (TryParsePosition(value, out var conicToPos))
                {
                    BuildConicGradient(declarations, important, toPosition: conicToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var conicToColor))
                {
                    BuildConicGradient(declarations, important, toColor: conicToColor);
                }

                break;

            case "mask-directional":
                if (TryParseDirection(value, out var direction))
                {
                    BuildLinearGradient(declarations, important, position: direction);
                }

                break;

            case "mask-directional-from":
                if (TryParsePosition(value, out var dirFromPos))
                {
                    BuildLinearGradient(declarations, important, fromPosition: dirFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var dirFromColor))
                {
                    BuildLinearGradient(declarations, important, fromColor: dirFromColor);
                }

                break;

            case "mask-directional-to":
                if (TryParsePosition(value, out var dirToPos))
                {
                    BuildLinearGradient(declarations, important, toPosition: dirToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var dirToColor))
                {
                    BuildLinearGradient(declarations, important, toColor: dirToColor);
                }

                break;

            // Directional mask patterns. from/to accept either a position (%) or a color; a color
            // tints the gradient stop via --tw-mask-<edge>-from/to-color.
            case "mask-b-from":
                if (TryParsePosition(value, out var bFromPos))
                {
                    BuildDirectionalMask(declarations, important, "bottom", fromPosition: bFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var bFromColor))
                {
                    BuildDirectionalMask(declarations, important, "bottom", fromColor: bFromColor);
                }

                break;

            case "mask-b-to":
                if (TryParsePosition(value, out var bToPos))
                {
                    BuildDirectionalMask(declarations, important, "bottom", toPosition: bToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var bToColor))
                {
                    BuildDirectionalMask(declarations, important, "bottom", toColor: bToColor);
                }

                break;

            case "mask-t-from":
                if (TryParsePosition(value, out var tFromPos))
                {
                    BuildDirectionalMask(declarations, important, "top", fromPosition: tFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var tFromColor))
                {
                    BuildDirectionalMask(declarations, important, "top", fromColor: tFromColor);
                }

                break;

            case "mask-t-to":
                if (TryParsePosition(value, out var tToPos))
                {
                    BuildDirectionalMask(declarations, important, "top", toPosition: tToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var tToColor))
                {
                    BuildDirectionalMask(declarations, important, "top", toColor: tToColor);
                }

                break;

            case "mask-l-from":
                if (TryParsePosition(value, out var lFromPos))
                {
                    BuildDirectionalMask(declarations, important, "left", fromPosition: lFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var lFromColor))
                {
                    BuildDirectionalMask(declarations, important, "left", fromColor: lFromColor);
                }

                break;

            case "mask-l-to":
                if (TryParsePosition(value, out var lToPos))
                {
                    BuildDirectionalMask(declarations, important, "left", toPosition: lToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var lToColor))
                {
                    BuildDirectionalMask(declarations, important, "left", toColor: lToColor);
                }

                break;

            case "mask-r-from":
                if (TryParsePosition(value, out var rFromPos))
                {
                    BuildDirectionalMask(declarations, important, "right", fromPosition: rFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var rFromColor))
                {
                    BuildDirectionalMask(declarations, important, "right", fromColor: rFromColor);
                }

                break;

            case "mask-r-to":
                if (TryParsePosition(value, out var rToPos))
                {
                    BuildDirectionalMask(declarations, important, "right", toPosition: rToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var rToColor))
                {
                    BuildDirectionalMask(declarations, important, "right", toColor: rToColor);
                }

                break;

            // X/Y axis patterns
            case "mask-x-from":
                if (TryParsePosition(value, out var xFromPos))
                {
                    BuildAxisMask(declarations, important, "x", fromPosition: xFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var xFromColor))
                {
                    BuildAxisMask(declarations, important, "x", fromColor: xFromColor);
                }

                break;

            case "mask-x-to":
                if (TryParsePosition(value, out var xToPos))
                {
                    BuildAxisMask(declarations, important, "x", toPosition: xToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var xToColor))
                {
                    BuildAxisMask(declarations, important, "x", toColor: xToColor);
                }

                break;

            case "mask-y-from":
                if (TryParsePosition(value, out var yFromPos))
                {
                    BuildAxisMask(declarations, important, "y", fromPosition: yFromPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var yFromColor))
                {
                    BuildAxisMask(declarations, important, "y", fromColor: yFromColor);
                }

                break;

            case "mask-y-to":
                if (TryParsePosition(value, out var yToPos))
                {
                    BuildAxisMask(declarations, important, "y", toPosition: yToPos);
                }
                else if (TryResolveMaskColor(functionalUtility.Value, theme, out var yToColor))
                {
                    BuildAxisMask(declarations, important, "y", toColor: yToColor);
                }

                break;
        }

        return declarations.ToImmutable();
    }

    private static void AddCommonMaskDeclarations(ImmutableList<AstNode>.Builder declarations, bool important, params string[] maskVars)
    {
        var maskImage = maskVars.Length > 0 ? string.Join(", ", maskVars) : "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)";
        declarations.Add(new Declaration("mask-image", maskImage, important));
        declarations.Add(new Declaration("mask-composite", "intersect", important));
    }

    // Resolves a mask from/to value to a color — theme color (→ var(--color-<name>)), the
    // current/inherit/transparent keywords, or an arbitrary value. Returns false when the value
    // isn't a color (e.g. a position like 50%), so the caller falls back to position handling.
    private static bool TryResolveMaskColor(CandidateValue? value, Theme.Theme theme, out string color)
    {
        color = string.Empty;
        if (value is null)
        {
            return false;
        }

        switch (value.Value)
        {
            case "current":
                color = "currentcolor";
                return true;
            case "inherit":
                color = "inherit";
                return true;
            case "transparent":
                color = "transparent";
                return true;
        }

        if (value.Kind == ValueKind.Arbitrary)
        {
            color = value.Value;
            return true;
        }

        var resolved = theme.Resolve(value.Value, ["--color"]);
        if (!string.IsNullOrEmpty(resolved))
        {
            color = resolved;
            return true;
        }

        return false;
    }

    private static void BuildLinearGradient(ImmutableList<AstNode>.Builder declarations, bool important, string? position = null, string? fromPosition = null, string? toPosition = null, string? fromColor = null, string? toColor = null)
    {
        AddCommonMaskDeclarations(declarations, important);

        if (position != null)
        {
            declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops, var(--tw-mask-linear-position)))", important));
            declarations.Add(new Declaration("--tw-mask-linear-position", position, important));
        }

        if (fromPosition != null || toPosition != null || fromColor != null || toColor != null)
        {
            declarations.Add(new Declaration("--tw-mask-linear-stops", "var(--tw-mask-linear-position), var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)", important));
            declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-stops))", important));

            if (fromPosition != null)
            {
                declarations.Add(new Declaration("--tw-mask-linear-from-position", fromPosition, important));
            }

            if (toPosition != null)
            {
                declarations.Add(new Declaration("--tw-mask-linear-to-position", toPosition, important));
            }

            if (fromColor != null)
            {
                declarations.Add(new Declaration("--tw-mask-linear-from-color", fromColor, important));
            }

            if (toColor != null)
            {
                declarations.Add(new Declaration("--tw-mask-linear-to-color", toColor, important));
            }
        }
    }

    private static void BuildRadialGradient(ImmutableList<AstNode>.Builder declarations, bool important, string? position = null, string? fromPosition = null, string? toPosition = null, string? shape = null, string? size = null, string? fromColor = null, string? toColor = null)
    {
        AddCommonMaskDeclarations(declarations, important);
        declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-stops))", important));

        if (position != null)
        {
            declarations.Add(new Declaration("--tw-mask-radial-position", position, important));
        }

        if (fromPosition != null || toPosition != null || fromColor != null || toColor != null)
        {
            declarations.Add(new Declaration("--tw-mask-radial-stops", "var(--tw-mask-radial-from-color) var(--tw-mask-radial-from-position), var(--tw-mask-radial-to-color) var(--tw-mask-radial-to-position)", important));

            if (fromPosition != null)
            {
                declarations.Add(new Declaration("--tw-mask-radial-from-position", fromPosition, important));
            }

            if (toPosition != null)
            {
                declarations.Add(new Declaration("--tw-mask-radial-to-position", toPosition, important));
            }

            if (fromColor != null)
            {
                declarations.Add(new Declaration("--tw-mask-radial-from-color", fromColor, important));
            }

            if (toColor != null)
            {
                declarations.Add(new Declaration("--tw-mask-radial-to-color", toColor, important));
            }
        }

        if (shape != null)
        {
            declarations.Add(new Declaration("--tw-mask-radial-shape", shape, important));
        }

        if (size != null)
        {
            declarations.Add(new Declaration("--tw-mask-radial-size", size, important));
        }
    }

    private static void BuildConicGradient(ImmutableList<AstNode>.Builder declarations, bool important, string? position = null, string? fromPosition = null, string? toPosition = null, string? fromColor = null, string? toColor = null)
    {
        AddCommonMaskDeclarations(declarations, important);

        if (position != null)
        {
            declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(from var(--tw-mask-conic-position) at var(--tw-mask-conic-center), var(--tw-mask-conic-stops))", important));
            declarations.Add(new Declaration("--tw-mask-conic-position", position, important));
        }

        if (fromPosition != null || toPosition != null || fromColor != null || toColor != null)
        {
            declarations.Add(new Declaration("--tw-mask-conic-stops", "from var(--tw-mask-conic-position), var(--tw-mask-conic-from-color) var(--tw-mask-conic-from-position), var(--tw-mask-conic-to-color) var(--tw-mask-conic-to-position)", important));
            declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(var(--tw-mask-conic-stops))", important));

            if (fromPosition != null)
            {
                declarations.Add(new Declaration("--tw-mask-conic-from-position", fromPosition, important));
            }

            if (toPosition != null)
            {
                declarations.Add(new Declaration("--tw-mask-conic-to-position", toPosition, important));
            }

            if (fromColor != null)
            {
                declarations.Add(new Declaration("--tw-mask-conic-from-color", fromColor, important));
            }

            if (toColor != null)
            {
                declarations.Add(new Declaration("--tw-mask-conic-to-color", toColor, important));
            }
        }
    }

    private static void BuildDirectionalMask(ImmutableList<AstNode>.Builder declarations, bool important, string edge, string? fromPosition = null, string? toPosition = null, string? fromColor = null, string? toColor = null)
    {
        var maskVars = "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)";
        AddCommonMaskDeclarations(declarations, important, maskVars);

        EmitEdgeGradient(declarations, important, edge, fromPosition, toPosition, fromColor, toColor);
    }

    // Emits one edge's gradient var plus whichever stop (position or color) was supplied. Each edge
    // fades toward itself ("to top" for the top edge); stop colors/positions are theme vars so a
    // color utility can tint them (defaults: from-color black, to-color transparent — see
    // PropertyRegistrationStage). Matches Tailwind's mask gradient shape.
    private static void EmitEdgeGradient(ImmutableList<AstNode>.Builder declarations, bool important, string edge, string? fromPosition, string? toPosition, string? fromColor, string? toColor)
    {
        var v = $"--tw-mask-{edge}";
        declarations.Add(new Declaration(v, $"linear-gradient(to {edge}, var({v}-from-color) var({v}-from-position), var({v}-to-color) var({v}-to-position))", important));

        if (fromPosition != null)
        {
            declarations.Add(new Declaration($"{v}-from-position", fromPosition, important));
        }

        if (toPosition != null)
        {
            declarations.Add(new Declaration($"{v}-to-position", toPosition, important));
        }

        if (fromColor != null)
        {
            declarations.Add(new Declaration($"{v}-from-color", fromColor, important));
        }

        if (toColor != null)
        {
            declarations.Add(new Declaration($"{v}-to-color", toColor, important));
        }
    }

    private static void BuildAxisMask(ImmutableList<AstNode>.Builder declarations, bool important, string axis, string? fromPosition = null, string? toPosition = null, string? fromColor = null, string? toColor = null)
    {
        var maskVars = "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic), var(--tw-mask-top), var(--tw-mask-bottom), var(--tw-mask-left), var(--tw-mask-right)";
        AddCommonMaskDeclarations(declarations, important, maskVars);

        // x → right + left, y → top + bottom (each edge fades toward itself).
        var edges = axis == "x" ? new[] { "right", "left" } : new[] { "top", "bottom" };
        foreach (var edge in edges)
        {
            EmitEdgeGradient(declarations, important, edge, fromPosition, toPosition, fromColor, toColor);
        }
    }

    private static bool TryParseAngle(string value, bool isNegative, out string angle)
    {
        angle = string.Empty;
        if (int.TryParse(value, out var angleNum))
        {
            angle = isNegative ? $"calc(1deg * -{angleNum})" : $"calc(1deg * {angleNum})";
            return true;
        }

        return false;
    }

    private static bool TryParsePosition(string value, out string position)
    {
        position = string.Empty;
        if (value.EndsWith('%'))
        {
            position = value;
            return true;
        }

        return false;
    }

    private static bool TryParseRadialPosition(string value, out string position)
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

    private static bool TryParseDirection(string value, out string direction)
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

    /// <summary>
    /// Returns examples of mask utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("mask-none", "Remove mask image"),
            new("mask-linear", "Apply linear gradient mask"),
            new("mask-radial", "Apply radial gradient mask"),
            new("mask-radial-at-center", "Apply radial gradient mask at center"),
            new("mask-center", "Set mask position to center"),
            new("mask-repeat", "Repeat mask image"),
            new("mask-no-repeat", "Do not repeat mask image"),
            new("mask-cover", "Cover element with mask"),
        };

        return examples;
    }
}