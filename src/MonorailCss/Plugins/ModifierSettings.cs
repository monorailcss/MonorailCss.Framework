using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// Settings for modifying a plugin.
/// </summary>
public class ModifierSettings
{
    /// <summary>
    /// Gets ths list of the CSS declarations for the root element.
    /// </summary>
    public ImmutableList<CssDeclaration>? BaseRules { get; init; }

    /// <summary>
    /// Gets a mapping of additional elements to include.
    /// </summary>
    public ImmutableList<CssRuleSet> AdditionalElements { get; init; } = ImmutableList<CssRuleSet>.Empty;

    /// <summary>
    /// Converts a px value to rem.
    /// </summary>
    /// <param name="px">The pixel value.</param>
    /// <returns>The pixel value in rems.</returns>
    public static string Rem(int px)
    {
        return $"{Rounds(px / 16m)}rem";
    }

    /// <summary>
    /// Converts a pixel value to an em value given a base.
    /// </summary>
    /// <param name="px">The pixel value.</param>
    /// <param name="baseValue">The base value.</param>
    /// <returns>The em value i.e. (px / baseValue)em.</returns>
    public static string Em(int px, int baseValue)
    {
        return $"{Rounds(px / (decimal)baseValue)}em";
    }

    /// <summary>
    /// Rounds a number to seven places.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <returns>The rounded result.</returns>
    public static string Rounds(decimal number)
    {
        return Math.Round(number, 7).ToString(CultureInfo.InvariantCulture);
    }
}