using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask directional to color utilities.
/// Handles: mask-l-to-*, mask-r-to-*, mask-t-to-*, mask-b-to-*, mask-x-to-*, mask-y-to-*.
/// </summary>
internal class MaskDirectionalToUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.StandardFunctional;

    public string[] GetNamespaces() => [];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        // Parse the pattern (e.g., mask-b-to, mask-x-to)
        var parts = functionalUtility.Root.Split('-');
        if (parts.Length != 3 || parts[0] != "mask" || parts[2] != "to")
        {
            return false;
        }

        var direction = parts[1];
        if (!IsValidDirection(direction))
        {
            return false;
        }

        // Handle the color value
        var colorValue = GetColorValue(functionalUtility.Value?.Value);
        if (colorValue == null)
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Add the mask-image declaration with all three gradient variables
        declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", candidate.Important));

        // Add mask-composite
        declarations.Add(new Declaration("mask-composite", "intersect", candidate.Important));

        // Add the linear mask with all four directions
        declarations.Add(new Declaration("--tw-mask-linear", "var(--tw-mask-left), var(--tw-mask-right), var(--tw-mask-bottom), var(--tw-mask-top)", candidate.Important));

        // Handle x and y directions (which set multiple gradients)
        if (direction == "x")
        {
            // Set both left and right
            var leftGradient = "linear-gradient(to left, var(--tw-mask-left-from-color) var(--tw-mask-left-from-position), var(--tw-mask-left-to-color) var(--tw-mask-left-to-position))";
            var rightGradient = "linear-gradient(to right, var(--tw-mask-right-from-color) var(--tw-mask-right-from-position), var(--tw-mask-right-to-color) var(--tw-mask-right-to-position))";

            declarations.Add(new Declaration("--tw-mask-left", leftGradient, candidate.Important));
            declarations.Add(new Declaration("--tw-mask-left-to-color", colorValue, candidate.Important));
            declarations.Add(new Declaration("--tw-mask-right", rightGradient, candidate.Important));
            declarations.Add(new Declaration("--tw-mask-right-to-color", colorValue, candidate.Important));
        }
        else if (direction == "y")
        {
            // Set both top and bottom
            var topGradient = "linear-gradient(to top, var(--tw-mask-top-from-color) var(--tw-mask-top-from-position), var(--tw-mask-top-to-color) var(--tw-mask-top-to-position))";
            var bottomGradient = "linear-gradient(to bottom, var(--tw-mask-bottom-from-color) var(--tw-mask-bottom-from-position), var(--tw-mask-bottom-to-color) var(--tw-mask-bottom-to-position))";

            declarations.Add(new Declaration("--tw-mask-top", topGradient, candidate.Important));
            declarations.Add(new Declaration("--tw-mask-top-to-color", colorValue, candidate.Important));
            declarations.Add(new Declaration("--tw-mask-bottom", bottomGradient, candidate.Important));
            declarations.Add(new Declaration("--tw-mask-bottom-to-color", colorValue, candidate.Important));
        }
        else
        {
            // Single direction
            var (gradientDirection, fromColorVar, toColorVar, fromPosVar, toPosVar) = GetDirectionalVars(direction);
            declarations.Add(new Declaration(
                $"--tw-mask-{GetFullDirection(direction)}",
                $"linear-gradient({gradientDirection}, var({fromColorVar}) var({fromPosVar}), var({toColorVar}) var({toPosVar}))",
                candidate.Important));
            declarations.Add(new Declaration(toColorVar, colorValue, candidate.Important));
        }

        results = declarations.ToImmutable();
        return true;
    }

    private static bool IsValidDirection(string direction)
    {
        return direction is "l" or "r" or "t" or "b" or "x" or "y";
    }

    private static string? GetColorValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Handle percentage values (0%, 5%, 10%, etc.)
        if (value.EndsWith('%'))
        {
            return value;
        }

        // Handle special color values
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

    private static (string Direction, string FromColor, string ToColor, string FromPos, string ToPos) GetDirectionalVars(string direction)
    {
        var fullDir = GetFullDirection(direction);
        var gradientDir = direction switch
        {
            "l" => "to left",
            "r" => "to right",
            "t" => "to top",
            _ => "to bottom",
        };

        return (
            gradientDir,
            $"--tw-mask-{fullDir}-from-color",
            $"--tw-mask-{fullDir}-to-color",
            $"--tw-mask-{fullDir}-from-position",
            $"--tw-mask-{fullDir}-to-position");
    }
}