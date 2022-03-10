using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss;

/// <summary>
/// Extensions for working with creating a new design system easier.
/// </summary>
public static class DesignSystemExtensions
{
    /// <summary>
    /// Flattens a dictionary of color definitions into one large dictionary..
    /// </summary>
    /// <param name="colors">The color dictionary to flatten.</param>
    /// <returns>The flattened colors.</returns>
    public static ImmutableDictionary<string, CssColor> Flatten(
        this ImmutableDictionary<string, ImmutableDictionary<string, CssColor>> colors)
    {
        return colors.SelectMany(kvp => kvp.Value, (pair, tuple) => (pair.Key + "-" + tuple.Key, tuple.Value))
            .ToImmutableDictionary(t => t.Item1.ToLowerInvariant(), t => t.Value);
    }
}