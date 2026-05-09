using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;

namespace MonorailCss.Variants;

/// <summary>
/// Defines a variant that can transform selectors and add at-rule wrappers.
/// </summary>
public interface IVariant
{
    /// <summary>
    /// Gets the name of the variant (e.g., "hover", "focus", "dark").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the weight for sorting. Lower values appear earlier in the output.
    /// </summary>
    int Weight { get; }

    /// <summary>
    /// Gets the kind of variant for classification.
    /// </summary>
    VariantKind Kind { get; }

    /// <summary>
    /// Gets the constraints on where this variant can be applied.
    /// </summary>
    VariantConstraints Constraints { get; }

    /// <summary>
    /// Attempts to apply this variant to the current selector.
    /// </summary>
    /// <param name="current">The current applied selector.</param>
    /// <param name="token">The variant token to apply.</param>
    /// <param name="result">The resulting applied selector if successful.</param>
    /// <returns>True if the variant was successfully applied; otherwise, false.</returns>
    bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result);

    /// <summary>
    /// Checks if this variant can handle the given token.
    /// </summary>
    /// <param name="token">The variant token to check.</param>
    /// <returns>True if this variant can handle the token; otherwise, false.</returns>
    bool CanHandle(VariantToken token);

    /// <summary>
    /// Optionally injects extra declarations into the rule body. Used by pseudo-element
    /// variants like <c>before:</c> / <c>after:</c> to emit <c>content: var(--tw-content)</c>
    /// so the pseudo-element actually renders. Default implementation returns nodes unchanged.
    /// </summary>
    /// <param name="nodes">The current rule body declarations.</param>
    /// <returns>The (possibly augmented) declarations.</returns>
    ImmutableList<AstNode> TransformNodes(ImmutableList<AstNode> nodes) => nodes;
}