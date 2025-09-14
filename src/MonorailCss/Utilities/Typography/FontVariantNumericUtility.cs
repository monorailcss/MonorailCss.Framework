using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles font variant numeric utilities with CSS custom property composition system.
/// Uses CSS variables to allow multiple utilities to work together composably.
/// </summary>
internal class FontVariantNumericUtility : BaseStaticUtility
{
    /// <summary>
    /// Map of utility names to their CSS variable names and values.
    /// </summary>
    private static readonly ImmutableDictionary<string, (string Variable, string Value)> _variantValues =
        new Dictionary<string, (string, string)>
        {
            { "ordinal", ("--tw-ordinal", "ordinal") },
            { "slashed-zero", ("--tw-slashed-zero", "slashed-zero") },
            { "lining-nums", ("--tw-numeric-figure", "lining-nums") },
            { "oldstyle-nums", ("--tw-numeric-figure", "oldstyle-nums") },
            { "proportional-nums", ("--tw-numeric-spacing", "proportional-nums") },
            { "tabular-nums", ("--tw-numeric-spacing", "tabular-nums") },
            { "diagonal-fractions", ("--tw-numeric-fraction", "diagonal-fractions") },
            { "stacked-fractions", ("--tw-numeric-fraction", "stacked-fractions") },
        }.ToImmutableDictionary();

    // Base class required property - includes the normal-nums reset utility
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "normal-nums", ("font-variant-numeric", "normal") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        // Handle normal-nums reset utility
        if (StaticValues.TryGetValue(staticUtility.Root, out var staticValue))
        {
            results = ImmutableList.Create<AstNode>(
                new Declaration(staticValue.Property, staticValue.Value, candidate.Important));
            return true;
        }

        // Handle composable variant utilities with CSS variables
        if (_variantValues.TryGetValue(staticUtility.Root, out var variantConfig))
        {
            var (variable, value) = variantConfig;

            var declarations = ImmutableList.Create<AstNode>(
                new Declaration(variable, value, candidate.Important),
                new Declaration(
                    "font-variant-numeric",
                    "var(--tw-ordinal,) var(--tw-slashed-zero,) var(--tw-numeric-figure,) var(--tw-numeric-spacing,) var(--tw-numeric-fraction,)",
                    candidate.Important));

            results = declarations;
            return true;
        }

        return false;
    }

    public override ImmutableHashSet<string> GetUtilityNames()
    {
        return StaticValues.Keys.Concat(_variantValues.Keys).ToImmutableHashSet();
    }
}