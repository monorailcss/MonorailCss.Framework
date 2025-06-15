using System.Collections.Immutable;
using MonorailCss.Variants;

namespace MonorailCss.Framework.Processing;

/// <summary>
/// Handles variant processing and media query organization.
/// </summary>
internal class VariantProcessor
{
    private readonly VariantSystem _variantSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariantProcessor"/> class.
    /// </summary>
    /// <param name="variantSystem">The variant system to use for processing.</param>
    public VariantProcessor(VariantSystem variantSystem)
    {
        _variantSystem = variantSystem;
    }

    /// <summary>
    /// Gets media modifiers from a list of modifiers.
    /// </summary>
    /// <param name="modifiers">The modifiers to filter.</param>
    /// <param name="variants">The available variants.</param>
    /// <returns>Array of media modifier strings.</returns>
    public string[] GetMediaModifiers(IEnumerable<string> modifiers, ImmutableDictionary<string, IVariant> variants)
    {
        List<string> mediaModifiers = [];
        foreach (var modifier in modifiers)
        {
            var variant = _variantSystem.TryGetVariant(modifier);
            if (variant == null)
            {
                continue;
            }

            if (variant is MediaQueryVariant)
            {
                mediaModifiers.Add(modifier);
            }
        }

        return mediaModifiers.ToArray();
    }

    /// <summary>
    /// Converts media modifiers to MediaQueryVariant list.
    /// </summary>
    /// <param name="mediaModifiers">The media modifier strings.</param>
    /// <param name="variants">The available variants.</param>
    /// <returns>List of MediaQueryVariant objects.</returns>
    public ImmutableList<MediaQueryVariant> GetFeatureList(
        string[] mediaModifiers,
        ImmutableDictionary<string, IVariant> variants) =>
        mediaModifiers.Select(m => _variantSystem.TryGetVariant(m))
            .OfType<MediaQueryVariant>()
            .Select(i => i).ToImmutableList();
}