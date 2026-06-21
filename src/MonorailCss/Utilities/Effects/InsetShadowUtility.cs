using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the inset shadow of an element (<c>inset-shadow-2xs</c> …
/// <c>inset-shadow-sm</c>, <c>inset-shadow-none</c>, and arbitrary values such as
/// <c>inset-shadow-[0_2px_4px_red]</c> / <c>inset-shadow-(--my-inset)</c>). Each sets
/// <c>--tw-inset-shadow</c> and re-emits the shared box-shadow composition stack, mirroring
/// <see cref="BoxShadowUtility"/>.
/// </summary>
/// <remarks>
/// Named sizes are registered as exact static names so the parser matches them before the
/// <c>inset</c> positioning utility. Arbitrary shadow values arrive as functional candidates
/// (root <c>inset-shadow</c>); single-color arbitraries are rejected here so they fall through to
/// <see cref="InsetShadowColorUtility"/>. <c>@property --tw-inset-shadow</c> is registered centrally
/// by <c>PropertyRegistrationStage</c>.
/// </remarks>
internal class InsetShadowUtility : IUtility, IStaticUtilityNameProvider
{
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    public string[] GetNamespaces() => [];

    public IEnumerable<string> GetUtilityNames() => [.. _insetShadows.Keys];

    public string[] GetFunctionalRoots() => ["inset-shadow"];

    private const string BoxShadowStack =
        "var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)";

    /// <summary>
    /// Hardcoded Tailwind v4 inset-shadow scale. Size values carry the
    /// <c>--tw-inset-shadow-color</c> injection so an inset-shadow color can tint them;
    /// <c>inset-shadow-none</c> clears the layer.
    /// </summary>
    private static readonly Dictionary<string, string> _insetShadows = new()
    {
        ["inset-shadow-none"] = "0 0 #0000",
        ["inset-shadow-2xs"] = "inset 0 1px var(--tw-inset-shadow-color, rgb(0 0 0 / 0.05))",
        ["inset-shadow-xs"] = "inset 0 1px 1px var(--tw-inset-shadow-color, rgb(0 0 0 / 0.05))",
        ["inset-shadow-sm"] = "inset 0 2px 4px var(--tw-inset-shadow-color, rgb(0 0 0 / 0.05))",
    };

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        string? shadowValue = candidate switch
        {
            StaticUtility staticUtility when _insetShadows.TryGetValue(staticUtility.Root, out var v) => v,
            FunctionalUtility { Root: "inset-shadow", Value: { } value } when ShadowValueResolver.TryResolveInsetShadow(value, out var v) => v,
            _ => null,
        };

        if (shadowValue == null)
        {
            return false;
        }

        results = ImmutableList.Create<AstNode>(
            new Declaration("--tw-inset-shadow", shadowValue, candidate.Important),
            new Declaration("box-shadow", BoxShadowStack, candidate.Important));
        return true;
    }

    /// <summary>
    /// Returns examples of inset shadow utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme) =>
    [
        new("inset-shadow-2xs", "Apply a 2x-small inset shadow"),
        new("inset-shadow-sm", "Apply a small inset shadow"),
        new("inset-shadow-none", "Remove inset shadow"),
    ];
}
