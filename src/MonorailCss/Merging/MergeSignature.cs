using System.Collections.Immutable;

namespace MonorailCss.Merging;

/// <summary>
/// The conflict identity of a single class in a merge: two classes can only conflict when their
/// <see cref="VariantKey"/> and <see cref="Important"/> match, and a later class removes an
/// earlier one when its <see cref="Writes"/> plus <see cref="Covers"/> form a superset of the
/// earlier class's <see cref="Writes"/>.
/// </summary>
internal sealed record MergeSignature(
    string VariantKey,
    bool Important,
    ImmutableHashSet<string> Writes,
    ImmutableHashSet<string> Covers)
{
    /// <summary>
    /// Determines whether this signature (the later class) overrides every conflict key the
    /// earlier signature writes. Variant key and important equality are checked by the caller.
    /// </summary>
    /// <param name="earlier">The signature of an earlier class in the list.</param>
    /// <returns>True when the earlier class is fully overridden by this one.</returns>
    public bool Overrides(MergeSignature earlier)
    {
        foreach (var write in earlier.Writes)
        {
            if (!Writes.Contains(write) && !Covers.Contains(write))
            {
                return false;
            }
        }

        return true;
    }
}
