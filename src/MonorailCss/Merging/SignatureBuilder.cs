using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Parser;
using MonorailCss.Utilities;
using MonorailCss.Variants;

namespace MonorailCss.Merging;

/// <summary>
/// Computes the <see cref="MergeSignature"/> of a single class for <see cref="ClassMerger"/>.
/// The conflict keys a class writes are taken from <see cref="IUtility.GetMergeInfo"/> when a
/// utility provides them explicitly, and otherwise derived from the declarations the utility
/// compiles the candidate to.
/// </summary>
internal sealed class SignatureBuilder
{
    // Variants whose position in the chain is semantically significant: pseudo-elements, child
    // selectors, and prose element variants target different elements depending on order, and
    // arbitrary variants are opaque. Everything else is sorted so hover:focus: == focus:hover:,
    // mirroring tailwind-merge's sortModifiers.
    private static readonly HashSet<string> _orderSensitiveVariants = new(StringComparer.Ordinal)
    {
        "before", "after", "first-line", "first-letter", "marker",
        "selection", "placeholder", "backdrop", "*", "**",
    };

    private readonly CandidateParser _parser;
    private readonly UtilityRegistry _registry;
    private readonly Theme.Theme _theme;

    public SignatureBuilder(CandidateParser parser, UtilityRegistry registry, Theme.Theme theme)
    {
        _parser = parser;
        _registry = registry;
        _theme = theme;
    }

    /// <summary>
    /// Computes the merge signature for a single class token, or null when the token should pass
    /// through merging untouched (unparseable, unknown to every utility, or a component-layer
    /// class such as prose).
    /// </summary>
    /// <param name="token">The raw class token, e.g. "hover:p-4".</param>
    /// <returns>The signature, or null for passthrough.</returns>
    public MergeSignature? ComputeSignature(string token)
    {
        if (!_parser.TryParseCandidate(token, out var candidate))
        {
            return null;
        }

        ImmutableHashSet<string> writes;
        var covers = ImmutableHashSet<string>.Empty;

        if (candidate is ArbitraryProperty arbitraryProperty)
        {
            // [color:red] declares the property directly; no utility is involved.
            writes = [.. ShorthandExpansion.Expand(arbitraryProperty.Property)];
        }
        else
        {
            var utility = ResolveUtility(candidate, out var nodes);
            if (utility is null || nodes is null || utility.Layer == UtilityLayer.Component)
            {
                return null;
            }

            var mergeInfo = utility.GetMergeInfo(candidate, _theme);
            if (mergeInfo?.Writes is { } explicitWrites)
            {
                writes = [.. explicitWrites.SelectMany(ShorthandExpansion.Expand)];
            }
            else if (!TryDeriveWrites(nodes, out writes))
            {
                return null;
            }

            if (mergeInfo is not null)
            {
                covers = [.. mergeInfo.Covers.SelectMany(ShorthandExpansion.Expand)];
            }
        }

        if (writes.IsEmpty)
        {
            return null;
        }

        return new MergeSignature(BuildVariantKey(candidate.Variants), candidate.Important, writes, covers);
    }

    /// <summary>
    /// Decomposes a shorthand class token into its immediate longhand child tokens (e.g. <c>my-4</c>
    /// into <c>mt-4</c> and <c>mb-4</c>), preserving variants, negative sign, value, modifier, and
    /// importance. Returns null when the token is not a decomposable functional shorthand. Used by
    /// <see cref="ClassMerger"/> to rewrite a partially overridden shorthand into the longhand
    /// classes that survive; callers re-validate each child via <see cref="ComputeSignature"/>, so an
    /// imperfectly rendered child simply fails to round-trip and the decomposition is abandoned.
    /// </summary>
    /// <param name="token">The raw shorthand class token, e.g. "hover:-my-4".</param>
    /// <returns>The immediate child tokens in axis order, or null when the token does not decompose.</returns>
    public IReadOnlyList<string>? TryDecompose(string token)
    {
        if (!_parser.TryParseCandidate(token, out var candidate) ||
            candidate is not FunctionalUtility functional)
        {
            return null;
        }

        var root = functional.Root;
        var sign = root.StartsWith('-') ? "-" : string.Empty;
        var positiveRoot = sign.Length > 0 ? root[1..] : root;

        if (ShorthandDecomposition.GetChildPrefixes(positiveRoot) is not { } childPrefixes)
        {
            return null;
        }

        var valuePart = RenderValue(functional.Value);
        var modifierPart = functional.Modifier is { } modifier ? $"/{modifier}" : string.Empty;
        var importantPart = functional.Important ? "!" : string.Empty;
        var variantPart = functional.Variants.IsEmpty
            ? string.Empty
            : string.Join(":", functional.Variants.Select(v => v.Raw)) + ":";

        var result = new List<string>(childPrefixes.Length);
        foreach (var childPrefix in childPrefixes)
        {
            result.Add($"{variantPart}{sign}{childPrefix}{valuePart}{modifierPart}{importantPart}");
        }

        return result;
    }

    private static string RenderValue(CandidateValue? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value.Kind == ValueKind.Arbitrary)
        {
            if (value.IsParenthesesShorthand)
            {
                return $"-({value.Value})";
            }

            return value.DataTypeHint is { } hint ? $"-[{hint}:{value.Value}]" : $"-[{value.Value}]";
        }

        // Named values carry the full token text (fractions included), so emit them verbatim.
        return $"-{value.Value}";
    }

    private static bool TryDeriveWrites(ImmutableList<AstNode> nodes, out ImmutableHashSet<string> writes)
    {
        var declarations = new List<(string Context, Declaration Declaration)>();
        if (!TryCollectDeclarations(nodes, string.Empty, declarations))
        {
            // ComponentRule/ChildRule (prose-style output) — treat the class as passthrough.
            writes = [];
            return false;
        }

        // Scaffold heuristic: utilities that compose through custom properties re-declare a
        // shared combined property whose value is a var() chain over those same custom
        // properties (touch-pan-x -> --tw-pan-x + touch-action: var(--tw-pan-x,) ...). That
        // combined declaration is shared scaffolding, not a write this class owns — keeping it
        // would make composable siblings (touch-pan-x/touch-pan-y, blur/grayscale) conflict.
        var customProperties = declarations
            .Select(d => d.Declaration.Property)
            .Where(p => p.StartsWith("--", StringComparison.Ordinal))
            .ToList();

        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (context, declaration) in declarations)
        {
            if (customProperties.Count > 0 &&
                !declaration.Property.StartsWith("--", StringComparison.Ordinal) &&
                customProperties.Any(custom => ReferencesVariable(declaration.Value, custom)))
            {
                continue;
            }

            foreach (var expanded in ShorthandExpansion.Expand(declaration.Property))
            {
                keys.Add(context.Length == 0 ? expanded : $"{expanded}@{context}");
            }
        }

        writes = [.. keys];
        return true;
    }

    private static bool TryCollectDeclarations(
        IEnumerable<AstNode> nodes,
        string context,
        List<(string Context, Declaration Declaration)> declarations)
    {
        foreach (var node in nodes)
        {
            var handled = node switch
            {
                Declaration declaration => Add(declaration),
                StyleRule styleRule => TryCollectDeclarations(styleRule.Nodes, Nest(styleRule.Selector), declarations),
                NestedRule nestedRule => TryCollectDeclarations(nestedRule.Nodes, Nest(nestedRule.Selector), declarations),
                AtRule atRule => TryCollectDeclarations(atRule.Nodes, Nest($"@{atRule.Name} {atRule.Params}"), declarations),
                Context contextNode => TryCollectDeclarations(contextNode.Nodes, context, declarations),
                ComponentRule or ChildRule => false,
                _ => true,
            };

            if (!handled)
            {
                return false;
            }
        }

        return true;

        bool Add(Declaration declaration)
        {
            declarations.Add((context, declaration));
            return true;
        }

        string Nest(string inner) => context.Length == 0 ? inner : $"{context} {inner}";
    }

    private static bool ReferencesVariable(string value, string variable)
    {
        var reference = $"var({variable}";
        var searchStart = 0;
        while (true)
        {
            var index = value.IndexOf(reference, searchStart, StringComparison.Ordinal);
            if (index < 0 || index + reference.Length >= value.Length)
            {
                return false;
            }

            // Require a delimiter so --tw-shadow doesn't match var(--tw-shadow-color).
            var next = value[index + reference.Length];
            if (next is ',' or ')' or ' ')
            {
                return true;
            }

            searchStart = index + 1;
        }
    }

    private static string BuildVariantKey(ImmutableArray<VariantToken> variants)
    {
        if (variants.IsEmpty)
        {
            return string.Empty;
        }

        var ordered = new List<string>(variants.Length);
        var sortable = new List<string>();
        foreach (var variant in variants)
        {
            if (IsOrderSensitive(variant))
            {
                sortable.Sort(StringComparer.Ordinal);
                ordered.AddRange(sortable);
                sortable.Clear();
                ordered.Add(variant.Raw);
            }
            else
            {
                sortable.Add(variant.Raw);
            }
        }

        sortable.Sort(StringComparer.Ordinal);
        ordered.AddRange(sortable);
        return string.Join(':', ordered);
    }

    private static bool IsOrderSensitive(VariantToken variant) =>
        _orderSensitiveVariants.Contains(variant.Name) ||
        variant.IsArbitrary ||
        variant.Raw.StartsWith("prose-", StringComparison.Ordinal);

    private IUtility? ResolveUtility(Candidate candidate, out ImmutableList<AstNode>? nodes)
    {
        // Same lookup shape as CssFramework.CompileUtilityClass: static fast path, then the
        // priority-ordered probe. A fresh CssPropertyRegistry per call keeps this stateless.
        var propertyRegistry = new CssPropertyRegistry();

        if (candidate is StaticUtility staticUtility &&
            _registry.StaticUtilitiesLookup.TryGetValue(staticUtility.Root, out var staticMatch) &&
            staticMatch.TryCompile(candidate, _theme, propertyRegistry, out nodes) &&
            nodes is not null)
        {
            return staticMatch;
        }

        foreach (var utility in _registry.RegisteredUtilities)
        {
            if (utility.TryCompile(candidate, _theme, propertyRegistry, out nodes) && nodes is not null)
            {
                return utility;
            }
        }

        nodes = null;
        return null;
    }
}
