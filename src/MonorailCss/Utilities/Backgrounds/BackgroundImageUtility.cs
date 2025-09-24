using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utility for background-image values.
/// Handles: bg-none, bg-gradient-to-*, bg-radial, bg-conic, bg-[url(...)], bg-[linear-gradient(...)]
/// CSS: background-image: none, background-image: linear-gradient(...), background-image: radial-gradient(...), background-image: conic-gradient(...), background-image: url(...)
/// </summary>
internal class BackgroundImageUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["bg"];

    protected override string[] ThemeKeys => [];

    protected override bool SupportsNegative => true;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Check if value contains negative marker
        var isNegative = value.StartsWith("NEG:");
        if (isNegative)
        {
            value = value.Substring(4); // Remove "NEG:" prefix
        }

        // Handle gradient directions - these set both background-image and gradient position
        if (IsGradientDirection(value))
        {
            var gradientPosition = GetGradientPosition(value);
            declarations.Add(new Declaration("--tw-gradient-position", gradientPosition, important));
            declarations.Add(new Declaration("background-image", "linear-gradient(var(--tw-gradient-stops))", important));
        }

        // Handle linear gradients with directions (e.g., bg-linear-to-b)
        else if (value.StartsWith("linear-to-"))
        {
            var direction = value.Substring(10); // Remove "linear-to-" prefix
            var gradientPosition = GetLinearGradientPosition(direction);
            declarations.Add(new Declaration("--tw-gradient-position", gradientPosition, important));
            declarations.Add(new Declaration("background-image", "linear-gradient(var(--tw-gradient-stops))", important));
        }

        // Handle linear gradients with angles (e.g., bg-linear-30)
        else if (value.StartsWith("linear-"))
        {
            var anglePart = value.Substring(7); // Remove "linear-" prefix
            if (int.TryParse(anglePart, out var angle))
            {
                var anglePosition = isNegative ? $"calc({angle}deg * -1) in oklab" : $"{angle}deg in oklab";
                declarations.Add(new Declaration("--tw-gradient-position", anglePosition, important));
                declarations.Add(new Declaration("background-image", "linear-gradient(var(--tw-gradient-stops))", important));
            }
        }

        // Handle conic gradients with angles (e.g., bg-conic-30)
        else if (value.StartsWith("conic-"))
        {
            var anglePart = value.Substring(6); // Remove "conic-" prefix
            if (int.TryParse(anglePart, out var angle))
            {
                var anglePosition = isNegative ? $"from calc({angle}deg * -1) in oklab" : $"from {angle}deg in oklab";
                declarations.Add(new Declaration("--tw-gradient-position", anglePosition, important));
                declarations.Add(new Declaration("background-image", "conic-gradient(var(--tw-gradient-stops))", important));
            }
        }

        // Handle radial gradients with positions (e.g., bg-radial-at-t)
        else if (value.StartsWith("radial-"))
        {
            var positionPart = value.Substring(7); // Remove "radial-" prefix
            if (positionPart.StartsWith("at-"))
            {
                var position = GetRadialGradientPosition(positionPart.Substring(3));
                declarations.Add(new Declaration("--tw-gradient-position", $"at {position} in oklab", important));
                declarations.Add(new Declaration("background-image", "radial-gradient(var(--tw-gradient-stops))", important));
            }
        }

        // Handle radial gradients
        else if (IsRadialGradient(value))
        {
            declarations.Add(new Declaration("--tw-gradient-position", "in oklab", important));
            declarations.Add(new Declaration("background-image", "radial-gradient(var(--tw-gradient-stops))", important));
        }

        // Handle conic gradients
        else if (IsConicGradient(value))
        {
            declarations.Add(new Declaration("--tw-gradient-position", "in oklab", important));
            declarations.Add(new Declaration("background-image", "conic-gradient(var(--tw-gradient-stops))", important));
        }
        else
        {
            // Handle regular background-image values
            declarations.Add(new Declaration("background-image", value, important));
        }

        return declarations.ToImmutable();
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle special static values
            resolvedValue = key switch
            {
                "none" => "none",
                "gradient-to-r" => "gradient-to-r",      // Will be handled in GenerateDeclarations
                "gradient-to-l" => "gradient-to-l",
                "gradient-to-t" => "gradient-to-t",
                "gradient-to-b" => "gradient-to-b",
                "gradient-to-tr" => "gradient-to-tr",
                "gradient-to-tl" => "gradient-to-tl",
                "gradient-to-br" => "gradient-to-br",
                "gradient-to-bl" => "gradient-to-bl",
                "radial" => "radial",                    // Will be handled in GenerateDeclarations
                "conic" => "conic",                      // Will be handled in GenerateDeclarations
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }

            // Check for new patterns: bg-linear-*, bg-conic-*, bg-radial-*
            if (key.StartsWith("linear-to-"))
            {
                resolvedValue = isNegative ? $"NEG:{key}" : key; // Will be handled in GenerateDeclarations
                return true;
            }

            if (key.StartsWith("linear-") || key.StartsWith("conic-") || key.StartsWith("radial-"))
            {
                // Mark negative values for proper handling in GenerateDeclarations
                resolvedValue = isNegative ? $"NEG:{key}" : key; // Will be handled in GenerateDeclarations
                return true;
            }
        }

        // Handle arbitrary values (bg-[url(...)], bg-[linear-gradient(...)])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Allow arbitrary background-image values
            if (IsValidBackgroundImageValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }
        }

        // No theme resolution needed for background-image
        return false;
    }

    /// <summary>
    /// Checks if the value is a gradient direction.
    /// </summary>
    private static bool IsGradientDirection(string value)
    {
        return value.StartsWith("gradient-to-");
    }

    /// <summary>
    /// Checks if the value is a radial gradient.
    /// </summary>
    private static bool IsRadialGradient(string value)
    {
        return value == "radial";
    }

    /// <summary>
    /// Checks if the value is a conic gradient.
    /// </summary>
    private static bool IsConicGradient(string value)
    {
        return value == "conic";
    }

    /// <summary>
    /// Gets the CSS gradient position for a gradient direction.
    /// </summary>
    private static string GetGradientPosition(string value)
    {
        return value switch
        {
            "gradient-to-r" => "to right in oklab",
            "gradient-to-l" => "to left in oklab",
            "gradient-to-t" => "to top in oklab",
            "gradient-to-b" => "to bottom in oklab",
            "gradient-to-tr" => "to top right in oklab",
            "gradient-to-tl" => "to top left in oklab",
            "gradient-to-br" => "to bottom right in oklab",
            "gradient-to-bl" => "to bottom left in oklab",
            _ => throw new ArgumentException($"Unknown gradient direction: {value}"),
        };
    }

    /// <summary>
    /// Gets the CSS gradient position for linear gradient direction.
    /// </summary>
    private static string GetLinearGradientPosition(string direction)
    {
        return direction switch
        {
            "r" => "to right in oklab",
            "l" => "to left in oklab",
            "t" => "to top in oklab",
            "b" => "to bottom in oklab",
            "tr" => "to top right in oklab",
            "tl" => "to top left in oklab",
            "br" => "to bottom right in oklab",
            "bl" => "to bottom left in oklab",
            _ => throw new ArgumentException($"Unknown linear gradient direction: {direction}"),
        };
    }

    /// <summary>
    /// Gets the CSS gradient position for radial gradient position.
    /// </summary>
    private static string GetRadialGradientPosition(string position)
    {
        return position switch
        {
            "t" => "top",
            "b" => "bottom",
            "l" => "left",
            "r" => "right",
            "tl" => "top left",
            "tr" => "top right",
            "bl" => "bottom left",
            "br" => "bottom right",
            "c" => "center",
            _ => throw new ArgumentException($"Unknown radial gradient position: {position}"),
        };
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid background-image value.
    /// </summary>
    private static bool IsValidBackgroundImageValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow url() functions
        if (value.StartsWith("url(") && value.EndsWith(")"))
        {
            return true;
        }

        // Allow gradient functions
        if (value.Contains("gradient("))
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidBackgroundImageValue(value);
    }

    // Higher priority to handle bg-* patterns before color utilities
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;
}