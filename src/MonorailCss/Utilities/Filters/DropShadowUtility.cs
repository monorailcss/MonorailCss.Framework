using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utilities for controlling the drop shadow filter of an element (<c>drop-shadow</c>,
/// <c>drop-shadow-xs</c> … <c>drop-shadow-2xl</c>, plus <c>drop-shadow-none</c>).
/// </summary>
/// <remarks>
/// Drop shadow composes through the shared <c>filter</c> stack. A sized utility sets two variables:
/// <c>--tw-drop-shadow-size</c> (the shadow with the <c>--tw-drop-shadow-color</c> injection, used
/// by <see cref="MonorailCss.Utilities.Effects.DropShadowColorUtility"/> when a color is applied)
/// and <c>--tw-drop-shadow</c> (what the filter stack actually references). Registered as exact
/// static names; <c>@property</c> blocks for both variables are registered centrally by
/// <c>PropertyRegistrationStage</c>.
/// </remarks>
internal class DropShadowUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    public string[] GetNamespaces() => [];

    public string[] GetUtilityNames() => [.. _dropShadows.Keys];

    public string[] GetFunctionalRoots() => ["drop-shadow"];

    private const string FilterStack =
        "var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)";

    /// <summary>
    /// A drop-shadow definition. <see cref="Size"/> is the color-injected value assigned to
    /// <c>--tw-drop-shadow-size</c> (null for <c>drop-shadow-none</c>, which sets no size layer);
    /// <see cref="DropShadow"/> is the value assigned to <c>--tw-drop-shadow</c>.
    /// </summary>
    private readonly record struct DropShadowDef(string? Size, string DropShadow);

    /// <summary>
    /// Hardcoded Tailwind v4 drop-shadow scale. Sized entries point <c>--tw-drop-shadow</c> at the
    /// theme variable (<c>var(--drop-shadow-xs)</c>); the bare <c>drop-shadow</c> default inlines its
    /// two-layer value to match Tailwind's output exactly.
    /// </summary>
    private static readonly Dictionary<string, DropShadowDef> _dropShadows = new()
    {
        ["drop-shadow-none"] = new(null, " "),
        ["drop-shadow"] = new(
            "drop-shadow(0 1px 2px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.1))) drop-shadow(0 1px 1px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.06)))",
            "drop-shadow(0 1px 2px rgb(0 0 0 / 0.1)) drop-shadow( 0 1px 1px rgb(0 0 0 / 0.06))"),
        ["drop-shadow-xs"] = new("drop-shadow(0 1px 1px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.05)))", "drop-shadow(var(--drop-shadow-xs))"),
        ["drop-shadow-sm"] = new("drop-shadow(0 1px 2px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.15)))", "drop-shadow(var(--drop-shadow-sm))"),
        ["drop-shadow-md"] = new("drop-shadow(0 3px 3px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.12)))", "drop-shadow(var(--drop-shadow-md))"),
        ["drop-shadow-lg"] = new("drop-shadow(0 4px 4px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.15)))", "drop-shadow(var(--drop-shadow-lg))"),
        ["drop-shadow-xl"] = new("drop-shadow(0 9px 7px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.1)))", "drop-shadow(var(--drop-shadow-xl))"),
        ["drop-shadow-2xl"] = new("drop-shadow(0 25px 25px var(--tw-drop-shadow-color, rgb(0 0 0 / 0.15)))", "drop-shadow(var(--drop-shadow-2xl))"),
    };

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        string? size;
        string dropShadow;
        switch (candidate)
        {
            case StaticUtility staticUtility when _dropShadows.TryGetValue(staticUtility.Root, out var def):
                size = def.Size;
                dropShadow = def.DropShadow;
                break;

            // Arbitrary value, e.g. drop-shadow-[0_4px_4px_red] or drop-shadow-(--my-shadow).
            case FunctionalUtility { Root: "drop-shadow", Value: { } value }
                when ShadowValueResolver.TryResolveDropShadow(value, out var resolved):
                size = resolved;
                dropShadow = "var(--tw-drop-shadow-size)";
                break;

            default:
                return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        if (size != null)
        {
            declarations.Add(new Declaration("--tw-drop-shadow-size", size, candidate.Important));
        }

        declarations.Add(new Declaration("--tw-drop-shadow", dropShadow, candidate.Important));
        declarations.Add(new Declaration("filter", FilterStack, candidate.Important));

        results = declarations.ToImmutable();
        return true;
    }

    /// <summary>
    /// Returns examples of drop shadow utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme) =>
    [
        new("drop-shadow", "Apply the default drop shadow filter"),
        new("drop-shadow-lg", "Apply a large drop shadow filter"),
        new("drop-shadow-none", "Remove drop shadow filter"),
    ];
}
