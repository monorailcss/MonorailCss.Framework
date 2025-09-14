using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utility for font-family values.
/// Handles: font-sans, font-serif, font-mono, font-*
/// CSS: font-family: var(--font-sans), font-family: var(--font-serif), etc.
/// </summary>
internal class FontFamilyUtility : BaseFunctionalUtility
{
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    protected override string[] Patterns => ["font"];
    protected override string[] ThemeKeys => ["--font"];

    /// <summary>
    /// Check if a value represents a font-family (not font-weight).
    /// </summary>
    private static bool IsFontFamilyValue(string value)
    {
        // If it's a numeric value or known weight name, it's font-weight
        if (int.TryParse(value, out _) || IsWeightName(value))
        {
            return false;
        }

        // For other values, assume it's font-family if it's not obviously weight
        // This allows custom font families like "display", "body", "code" etc.
        return true;
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

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("font-family", value, important));
    }
}