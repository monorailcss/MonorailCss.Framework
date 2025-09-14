using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utility for font-weight values.
/// Handles: font-thin, font-light, font-normal, font-medium, font-semibold, font-bold, font-extrabold, font-black, font-*
/// CSS: font-weight: 100, font-weight: 300, font-weight: 400, font-weight: 500, font-weight: 600, font-weight: 700, font-weight: 800, font-weight: 900.
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
    /// Examples: "100" -> "100", "thin" -> "100", "bold" -> "700".
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

        // Handle named weight values
        var weightMap = new Dictionary<string, string>
        {
            ["thin"] = "100",
            ["extralight"] = "200",
            ["light"] = "300",
            ["normal"] = "400",
            ["medium"] = "500",
            ["semibold"] = "600",
            ["bold"] = "700",
            ["extrabold"] = "800",
            ["black"] = "900",
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
        return ImmutableList.Create<AstNode>(
            new Declaration("font-weight", value, important));
    }
}