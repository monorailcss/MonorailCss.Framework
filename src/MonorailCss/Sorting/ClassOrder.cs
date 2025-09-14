using MonorailCss.Utilities;

namespace MonorailCss.Sorting;

/// <summary>
/// Represents the ordering information for a CSS class.
/// </summary>
internal record ClassOrder : IComparable<ClassOrder>
{
    /// <summary>
    /// Gets the variant weights in order (e.g., [100, 300] for rtl:hover:bg-red-500).
    /// </summary>
    private int[] VariantWeights { get; }

    /// <summary>
    /// Gets the layer this utility belongs to (Component or Utility).
    /// </summary>
    private UtilityLayer Layer { get; }

    /// <summary>
    /// Gets the utility/property order key.
    /// </summary>
    private int UtilityOrder { get; }

    /// <summary>
    /// Gets the normalized class name for tie-breaking.
    /// </summary>
    private string NormalizedClassName { get; }

    /// <summary>
    /// Gets the original index in the input for stable sorting.
    /// </summary>
    private int OriginalIndex { get; }

    public ClassOrder(int[] variantWeights, UtilityLayer layer, int utilityOrder, string normalizedClassName, int originalIndex)
    {
        VariantWeights = variantWeights;
        Layer = layer;
        UtilityOrder = utilityOrder;
        NormalizedClassName = normalizedClassName;
        OriginalIndex = originalIndex;
    }

    public int CompareTo(ClassOrder? other)
    {
        if (other == null)
        {
            return -1;
        }

        // First, compare variant weights lexicographically
        var minLength = Math.Min(VariantWeights.Length, other.VariantWeights.Length);
        for (var i = 0; i < minLength; i++)
        {
            var cmp = VariantWeights[i].CompareTo(other.VariantWeights[i]);
            if (cmp != 0)
            {
                return cmp;
            }
        }

        // If one has more variants, it comes later
        var lengthCmp = VariantWeights.Length.CompareTo(other.VariantWeights.Length);
        if (lengthCmp != 0)
        {
            return lengthCmp;
        }

        // Then compare layer (Component utilities come before regular utilities)
        var layerCmp = Layer.CompareTo(other.Layer);
        if (layerCmp != 0)
        {
            return layerCmp;
        }

        // Then compare utility order
        var utilityCmp = UtilityOrder.CompareTo(other.UtilityOrder);
        if (utilityCmp != 0)
        {
            return utilityCmp;
        }

        // Then normalized class name for deterministic ordering
        var nameCmp = string.Compare(NormalizedClassName, other.NormalizedClassName, StringComparison.Ordinal);
        if (nameCmp != 0)
        {
            return nameCmp;
        }

        // Finally, preserve original order for stable sorting
        return OriginalIndex.CompareTo(other.OriginalIndex);
    }
}