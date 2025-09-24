using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Comprehensive utility for all mask gradient patterns.
/// Handles directional, radial, linear, and conic mask gradients with from/to colors.
/// </summary>
internal class MaskGradientUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.StandardFunctional;

    public string[] GetNamespaces() => [];

    public string[] GetFunctionalRoots() =>
    [

        // Directional from patterns
        "mask-b-from", "mask-t-from", "mask-l-from", "mask-r-from", "mask-x-from", "mask-y-from",

        // Directional to patterns
        "mask-b-to", "mask-t-to", "mask-l-to", "mask-r-to", "mask-x-to", "mask-y-to",

        // Radial patterns
        "mask-radial-from", "mask-radial-to",

        // Linear patterns
        "mask-linear-from", "mask-linear-to",

        // Conic patterns
        "mask-conic-from", "mask-conic-to",
    ];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        var root = functionalUtility.Root;
        var value = functionalUtility.Value?.Value;

        // Parse the pattern to determine type
        if (root.StartsWith("mask-") && (root.EndsWith("-from") || root.EndsWith("-to")))
        {
            var isFrom = root.EndsWith("-from");
            var basePattern = isFrom ? root[..^5] : root[..^3]; // Remove "-from" or "-to"

            // Get color value
            var colorValue = GetColorValue(value);
            if (colorValue == null)
            {
                return false;
            }

            var declarations = ImmutableList.CreateBuilder<AstNode>();

            // Add common mask properties
            declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", candidate.Important));
            declarations.Add(new Declaration("mask-composite", "intersect", candidate.Important));

            // Handle different pattern types
            switch (basePattern)
            {
                // Directional patterns (b, t, l, r, x, y)
                case "mask-b":
                case "mask-t":
                case "mask-l":
                case "mask-r":
                case "mask-x":
                case "mask-y":
                    HandleDirectionalPattern(declarations, basePattern[5..], isFrom, colorValue, candidate.Important);
                    break;

                case "mask-radial":
                    HandleRadialPattern(declarations, isFrom, colorValue, candidate.Important);
                    break;

                case "mask-linear":
                    HandleLinearPattern(declarations, isFrom, colorValue, candidate.Important);
                    break;

                case "mask-conic":
                    HandleConicPattern(declarations, isFrom, colorValue, candidate.Important);
                    break;

                default:
                    return false;
            }

            results = declarations.ToImmutable();
            return true;
        }

        return false;
    }

    private static void HandleDirectionalPattern(ImmutableList<AstNode>.Builder declarations, string direction, bool isFrom, string colorValue, bool important)
    {
        // Add the linear mask with all four directions
        declarations.Add(new Declaration("--tw-mask-linear", "var(--tw-mask-left), var(--tw-mask-right), var(--tw-mask-bottom), var(--tw-mask-top)", important));

        // Handle x and y directions (which set multiple gradients)
        if (direction == "x")
        {
            var leftGradient = "linear-gradient(to left, var(--tw-mask-left-from-color) var(--tw-mask-left-from-position), var(--tw-mask-left-to-color) var(--tw-mask-left-to-position))";
            var rightGradient = "linear-gradient(to right, var(--tw-mask-right-from-color) var(--tw-mask-right-from-position), var(--tw-mask-right-to-color) var(--tw-mask-right-to-position))";

            declarations.Add(new Declaration("--tw-mask-left", leftGradient, important));
            declarations.Add(new Declaration("--tw-mask-right", rightGradient, important));

            if (isFrom)
            {
                declarations.Add(new Declaration("--tw-mask-left-from-color", colorValue, important));
                declarations.Add(new Declaration("--tw-mask-right-from-color", colorValue, important));
            }
            else
            {
                declarations.Add(new Declaration("--tw-mask-left-to-color", colorValue, important));
                declarations.Add(new Declaration("--tw-mask-right-to-color", colorValue, important));
            }
        }
        else if (direction == "y")
        {
            var topGradient = "linear-gradient(to top, var(--tw-mask-top-from-color) var(--tw-mask-top-from-position), var(--tw-mask-top-to-color) var(--tw-mask-top-to-position))";
            var bottomGradient = "linear-gradient(to bottom, var(--tw-mask-bottom-from-color) var(--tw-mask-bottom-from-position), var(--tw-mask-bottom-to-color) var(--tw-mask-bottom-to-position))";

            declarations.Add(new Declaration("--tw-mask-top", topGradient, important));
            declarations.Add(new Declaration("--tw-mask-bottom", bottomGradient, important));

            if (isFrom)
            {
                declarations.Add(new Declaration("--tw-mask-top-from-color", colorValue, important));
                declarations.Add(new Declaration("--tw-mask-bottom-from-color", colorValue, important));
            }
            else
            {
                declarations.Add(new Declaration("--tw-mask-top-to-color", colorValue, important));
                declarations.Add(new Declaration("--tw-mask-bottom-to-color", colorValue, important));
            }
        }
        else
        {
            // Single direction (b, t, l, r)
            var fullDir = GetFullDirection(direction);
            var gradientDir = GetGradientDirection(direction);

            var gradient = $"linear-gradient({gradientDir}, var(--tw-mask-{fullDir}-from-color) var(--tw-mask-{fullDir}-from-position), var(--tw-mask-{fullDir}-to-color) var(--tw-mask-{fullDir}-to-position))";
            declarations.Add(new Declaration($"--tw-mask-{fullDir}", gradient, important));

            var colorVar = isFrom ? $"--tw-mask-{fullDir}-from-color" : $"--tw-mask-{fullDir}-to-color";
            declarations.Add(new Declaration(colorVar, colorValue, important));
        }
    }

    private static void HandleRadialPattern(ImmutableList<AstNode>.Builder declarations, bool isFrom, string colorValue, bool important)
    {
        declarations.Add(new Declaration("--tw-mask-radial-stops", "var(--tw-mask-radial-shape) var(--tw-mask-radial-size) at var(--tw-mask-radial-position), var(--tw-mask-radial-from-color) var(--tw-mask-radial-from-position), var(--tw-mask-radial-to-color) var(--tw-mask-radial-to-position)", important));
        declarations.Add(new Declaration("--tw-mask-radial", "radial-gradient(var(--tw-mask-radial-stops))", important));

        var colorVar = isFrom ? "--tw-mask-radial-from-color" : "--tw-mask-radial-to-color";
        declarations.Add(new Declaration(colorVar, colorValue, important));
    }

    private static void HandleLinearPattern(ImmutableList<AstNode>.Builder declarations, bool isFrom, string colorValue, bool important)
    {
        declarations.Add(new Declaration("--tw-mask-linear", "linear-gradient(var(--tw-mask-linear-position), var(--tw-mask-linear-stops, var(--tw-mask-linear-from-color) var(--tw-mask-linear-from-position), var(--tw-mask-linear-to-color) var(--tw-mask-linear-to-position)))", important));

        var colorVar = isFrom ? "--tw-mask-linear-from-color" : "--tw-mask-linear-to-color";
        declarations.Add(new Declaration(colorVar, colorValue, important));
    }

    private static void HandleConicPattern(ImmutableList<AstNode>.Builder declarations, bool isFrom, string colorValue, bool important)
    {
        declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(var(--tw-mask-conic-stops, var(--tw-mask-conic-from-color) var(--tw-mask-conic-from-position), var(--tw-mask-conic-to-color) var(--tw-mask-conic-to-position)))", important));

        var colorVar = isFrom ? "--tw-mask-conic-from-color" : "--tw-mask-conic-to-color";
        declarations.Add(new Declaration(colorVar, colorValue, important));
    }

    private static string? GetColorValue(string? value)
    {
        return value switch
        {
            "current" => "currentColor",
            "inherit" => "inherit",
            "transparent" => "transparent",
            _ => null,
        };
    }

    private static string GetFullDirection(string shortDirection)
    {
        return shortDirection switch
        {
            "l" => "left",
            "r" => "right",
            "t" => "top",
            "b" => "bottom",
            _ => shortDirection,
        };
    }

    private static string GetGradientDirection(string direction)
    {
        return direction switch
        {
            "l" => "to left",
            "r" => "to right",
            "t" => "to top",
            _ => "to bottom",
        };
    }
}