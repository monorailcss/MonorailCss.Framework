using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the font weight of an element.
/// </summary>
internal class FontWeightUtility : BaseFunctionalUtility
{
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    protected override string[] Patterns => ["font"];
    protected override string[] ThemeKeys => NamespaceResolver.FontWeightChain;

    /// <summary>
    /// Check if a value represents a font-weight.
    /// </summary>
    private static bool IsFontWeightValue(string value)
    {
        // Check if it's a numeric weight value
        if (int.TryParse(value, out var numValue) &&
            numValue >= 100 && numValue <= 900 && numValue % 100 == 0)
        {
            return true;
        }

        // Check if it's a known weight name
        return IsWeightName(value);
    }

    /// <summary>
    /// Check if a value is a font-weight name.
    /// </summary>
    private static bool IsWeightName(string value)
    {
        var weightNames = new HashSet<string>
        {
            "thin", "extralight", "light", "normal", "medium",
            "semibold", "bold", "extrabold", "black",
        };

        return weightNames.Contains(value);
    }

    /// <summary>
    /// Check if an arbitrary value looks like a valid font-weight value.
    /// </summary>
    private static bool IsValidWeightValue(string value)
    {
        // Allow numeric values
        if (int.TryParse(value, out var numValue) && numValue >= 1 && numValue <= 1000)
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles bare numeric values and named weight values.
    /// Examples: "100" -> "100", "thin" -> "var(--font-weight-thin)", "bold" -> "var(--font-weight-bold)".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle numeric values
        if (int.TryParse(value, out var numValue))
        {
            // Validate range
            if (numValue >= 1 && numValue <= 1000)
            {
                return numValue.ToString();
            }
        }

        // Handle named weight values with theme variables
        var weightMap = new Dictionary<string, string>
        {
            ["thin"] = "var(--font-weight-thin)",
            ["extralight"] = "var(--font-weight-extralight)",
            ["light"] = "var(--font-weight-light)",
            ["normal"] = "var(--font-weight-normal)",
            ["medium"] = "var(--font-weight-medium)",
            ["semibold"] = "var(--font-weight-semibold)",
            ["bold"] = "var(--font-weight-bold)",
            ["extrabold"] = "var(--font-weight-extrabold)",
            ["black"] = "var(--font-weight-black)",
        };

        if (weightMap.TryGetValue(value, out var weightValue))
        {
            return weightValue;
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for font-weight.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidWeightValue(value);
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Set both the custom property and the font-weight property
        declarations.Add(new Declaration("--tw-font-weight", value, important));
        declarations.Add(new Declaration("font-weight", value, important));

        return declarations.ToImmutable();
    }

    /// <summary>
    /// Override to register the --tw-font-weight property.
    /// </summary>
    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register the font weight custom property
        propertyRegistry.Register("--tw-font-weight", "*", false, null);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}