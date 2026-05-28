using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Shared base for the scrollbar-thumb-* and scrollbar-track-* color utilities.
/// Each emits two declarations — the per-utility --tw-scrollbar-{thumb,track}
/// variable plus the paired scrollbar-color shorthand — and registers an
/// @property block for its variable so the unused side falls back to #0000.
/// </summary>
internal abstract class BaseScrollbarColorUtility : BaseColorUtility, IUtility
{
    /// <summary>
    /// Gets the --tw-scrollbar-{thumb,track} variable name this utility writes.
    /// </summary>
    protected abstract string TwVariable { get; }

    protected override string CssProperty => TwVariable;

    protected override string[] ColorNamespaces { get; } = [NamespaceResolver.Color];

    // The parser's FindRoots fallback splits on the first dash and would treat
    // `scrollbar-thumb-red-500` as Root="scrollbar", Value="thumb-red-500".
    // Explicitly registering `scrollbar-thumb`/`scrollbar-track` as functional
    // roots makes the matcher consume the full prefix.
    public string[] GetFunctionalRoots() => [Pattern];

    protected override Declaration CreateDeclaration(string color, bool important)
    {
        return new Declaration(TwVariable, color, important);
    }

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        if (!base.TryCompile(candidate, theme, out results) || results == null)
        {
            return false;
        }

        // scrollbar-color shorthand must reference both --tw-scrollbar-thumb and
        // --tw-scrollbar-track in the same declaration; the unused side relies on
        // its @property initial-value of #0000.
        results = results.Add(new Declaration(
            "scrollbar-color",
            "var(--tw-scrollbar-thumb) var(--tw-scrollbar-track)",
            candidate.Important));
        return true;
    }

    // Implements IUtility's registry-aware overload (BaseColorUtility doesn't
    // declare it virtual). The @property declaration must be registered up-front
    // so the unused scrollbar-color side (thumb when only track is set, or vice
    // versa) resolves to its initial-value of #0000.
    bool IUtility.TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        if (!TryCompile(candidate, theme, out results))
        {
            return false;
        }

        propertyRegistry.Register(TwVariable, "<color>", false, "#0000");
        return true;
    }
}
