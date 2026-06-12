using System.Collections.Immutable;

namespace MonorailCss.Merging;

/// <summary>
/// Explicit merge-conflict metadata for a compiled candidate, returned by
/// <see cref="Utilities.IUtility.GetMergeInfo"/> and consumed by <see cref="ClassMerger"/>.
/// </summary>
/// <remarks>
/// Most utilities never need this: the merger derives the conflict keys a class writes from its
/// compiled declarations. Return an instance only when a utility's merge semantics cannot be
/// inferred from its output — typically reset utilities (e.g. <c>touch-none</c>,
/// <c>normal-nums</c>) that must override sibling classes composing through <c>--tw-*</c>
/// custom properties they do not themselves declare.
/// </remarks>
public sealed record MergeConflictInfo
{
    /// <summary>
    /// Gets the conflict keys (CSS property names or <c>--tw-*</c> custom properties) this class
    /// writes. Null means the keys are still derived from the compiled declarations and only
    /// <see cref="Covers"/> supplements them.
    /// </summary>
    public ImmutableHashSet<string>? Writes { get; init; }

    /// <summary>
    /// Gets additional conflict keys this class overrides or resets without declaring them.
    /// A later class removes an earlier one when its writes plus covers form a superset of the
    /// earlier class's writes.
    /// </summary>
    public ImmutableHashSet<string> Covers { get; init; } = [];

    /// <summary>
    /// Creates an instance that supplements the derived writes with additional covered keys.
    /// </summary>
    /// <param name="keys">The conflict keys this class overrides without declaring.</param>
    /// <returns>A <see cref="MergeConflictInfo"/> with only <see cref="Covers"/> set.</returns>
    public static MergeConflictInfo CoversKeys(params string[] keys) =>
        new() { Covers = [.. keys] };

    /// <summary>
    /// Creates an instance that replaces the derived writes entirely.
    /// </summary>
    /// <param name="keys">The conflict keys this class writes.</param>
    /// <returns>A <see cref="MergeConflictInfo"/> with <see cref="Writes"/> set.</returns>
    public static MergeConflictInfo WritesKeys(params string[] keys) =>
        new() { Writes = [.. keys] };
}
