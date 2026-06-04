using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for inset-shadow / drop-shadow color utilities. Sets a <c>--tw-*-shadow-color</c>
/// custom property with the Tailwind v4 fallback + <c>@supports</c> <c>color-mix</c> pattern, so a
/// shadow can be tinted by a theme color, an arbitrary color, or a color keyword.
/// </summary>
/// <remarks>
/// Shares the functional root with the corresponding shadow <em>value</em> utility; arbitrary
/// shadow values are rejected here (via <see cref="ShadowValueResolver.IsArbitraryShadowValue"/>) so
/// they flow to the value utility, keeping the dispatch order-independent. Mirrors
/// <c>ShadowColorUtility</c> for box-shadow.
/// </remarks>
internal abstract class BaseShadowColorUtility : IUtility
{
    public virtual UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    /// <summary>Gets the utility prefix, e.g. <c>inset-shadow</c> or <c>drop-shadow</c>.</summary>
    protected abstract string Pattern { get; }

    /// <summary>Gets the custom property set, e.g. <c>--tw-inset-shadow-color</c>.</summary>
    protected abstract string CssProperty { get; }

    /// <summary>Gets the alpha variable referenced inside <c>color-mix</c>, e.g. <c>--tw-inset-shadow-alpha</c>.</summary>
    protected abstract string AlphaVariable { get; }

    /// <summary>Gets the theme namespace chain used to resolve named colors.</summary>
    protected abstract string[] ColorNamespaces { get; }

    public string[] GetNamespaces() => ColorNamespaces;

    public string[] GetFunctionalRoots() => [Pattern];

    /// <summary>
    /// Extra declarations appended after the color is set. Drop-shadow overrides this to re-point
    /// <c>--tw-drop-shadow</c> at the (color-injected) <c>--tw-drop-shadow-size</c>.
    /// </summary>
    protected virtual IEnumerable<AstNode> ExtraDeclarations(bool important) => [];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility { Root: var root, Value: { } value } || root != Pattern)
        {
            return false;
        }

        // Arbitrary shadow values (multi-token, inset, hint-less parens) belong to the value utility.
        if (value.Kind == ValueKind.Arbitrary && ShadowValueResolver.IsArbitraryShadowValue(value))
        {
            return false;
        }

        // `initial` resets the color with no color-mix.
        if (value is { Kind: ValueKind.Named, Value: "initial" })
        {
            results = ImmutableList.Create<AstNode>(new Declaration(CssProperty, "initial", candidate.Important))
                .AddRange(ExtraDeclarations(candidate.Important));
            return true;
        }

        if (!TryResolveColor(value, theme, out var fallback, out var mixSource))
        {
            return false;
        }

        var colorMix = $"color-mix(in oklab, {mixSource} var({AlphaVariable}), transparent)";
        var supports = new AtRule(
            "supports",
            "(color: color-mix(in lab, red, red))",
            ImmutableList.Create<AstNode>(new Declaration(CssProperty, colorMix, candidate.Important)));

        results = ImmutableList.Create<AstNode>(new Declaration(CssProperty, fallback, candidate.Important), supports)
            .AddRange(ExtraDeclarations(candidate.Important));
        return true;
    }

    /// <summary>
    /// Resolves a candidate value to a fallback color (used outside <c>@supports</c>) and the source
    /// expression fed into <c>color-mix</c>.
    /// </summary>
    private bool TryResolveColor(CandidateValue value, Theme.Theme theme, out string fallback, out string mixSource)
    {
        fallback = string.Empty;
        mixSource = string.Empty;

        if (value.Kind == ValueKind.Arbitrary)
        {
            fallback = value.Value;
            mixSource = value.Value;
            return true;
        }

        switch (value.Value)
        {
            case "current":
                fallback = mixSource = "currentcolor";
                return true;
            case "inherit":
                fallback = mixSource = "inherit";
                return true;
            case "transparent":
                fallback = mixSource = "transparent";
                return true;
        }

        // Named theme color: oklch value as the fallback, var(--color-*) inside color-mix.
        var resolved = theme.ResolveValue(value.Value, ColorNamespaces);
        if (resolved == null)
        {
            return false;
        }

        fallback = resolved;
        mixSource = $"var(--color-{value.Value})";
        return true;
    }
}
