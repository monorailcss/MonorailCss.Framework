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

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Handle gradient directions - these set both background-image and gradient position
        if (IsGradientDirection(value))
        {
            var gradientPosition = GetGradientPosition(value);
            declarations.Add(new Declaration("--tw-gradient-position", gradientPosition, important));
            declarations.Add(new Declaration("background-image", "linear-gradient(var(--tw-gradient-stops))", important));
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