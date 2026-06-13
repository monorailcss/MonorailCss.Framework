namespace MonorailCss.Merging;

/// <summary>
/// Resolves conflicts in a Tailwind utility class list, keeping only the classes that survive
/// the cascade — the equivalent of tailwind-merge: <c>"px-2 p-4 bg-red-500 bg-blue-500"</c>
/// merges to <c>"p-4 bg-blue-500"</c>.
/// </summary>
/// <remarks>
/// <para>
/// Unlike tailwind-merge's hand-maintained class-group config, conflicts are derived from what
/// each class actually compiles to in the owning <see cref="CssFramework"/>: a later class
/// removes an earlier one when both target the same variant chain and the later class writes
/// (or explicitly covers, see <see cref="Utilities.IUtility.GetMergeInfo"/>) every CSS property
/// the earlier one writes. Custom utilities registered with the framework participate
/// automatically.
/// </para>
/// <para>
/// Classes that don't parse or that no utility recognizes pass through untouched, as do
/// component-layer classes such as <c>prose</c>. Instances are safe for concurrent use. A merger
/// is bound to the framework it was created from; utilities added to the framework after results
/// have been cached may not be observed.
/// </para>
/// <para>
/// Inspired by <see href="https://github.com/Zettersten/TailwindMerge">TailwindMerge</see>, a .NET
/// take on the original <see href="https://github.com/dcastil/tailwind-merge">tailwind-merge</see>
/// JavaScript library; the "later class wins" semantics are shared, but the conflict model here is
/// derived from compiled output rather than a hand-maintained class-group config.
/// </para>
/// </remarks>
public sealed class ClassMerger
{
    private static readonly char[] _whitespace = [' ', '\t', '\r', '\n'];

    private readonly SignatureBuilder _signatureBuilder;
    private readonly LruCache<string, MergeSignature?> _signatureCache = new(2048);
    private readonly LruCache<string, string> _resultCache = new(512);

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassMerger"/> class bound to the theme and
    /// registered utilities of the given framework.
    /// </summary>
    /// <param name="framework">The framework whose utility model defines the conflicts.</param>
    public ClassMerger(CssFramework framework)
    {
        ArgumentNullException.ThrowIfNull(framework);
        _signatureBuilder = new SignatureBuilder(framework.Parser, framework.UtilityRegistry, framework.Settings.Theme);
    }

    /// <summary>
    /// Merges a space-separated class list, removing earlier classes that are overridden by
    /// later conflicting ones. The relative order of the surviving classes is preserved.
    /// </summary>
    /// <param name="classList">The class list, e.g. "px-2 p-4 hover:p-2".</param>
    /// <returns>The merged class list.</returns>
    public string Merge(string? classList)
    {
        if (string.IsNullOrWhiteSpace(classList))
        {
            return string.Empty;
        }

        if (_resultCache.TryGetValue(classList, out var cached))
        {
            return cached;
        }

        var result = MergeCore(classList);
        _resultCache.Set(classList, result);
        return result;
    }

    /// <summary>
    /// Merges multiple class lists as if they were joined into one, with later lists taking
    /// precedence over earlier ones.
    /// </summary>
    /// <param name="classLists">The class lists to merge; null or blank entries are skipped.</param>
    /// <returns>The merged class list.</returns>
    public string Merge(params string?[] classLists)
    {
        return Merge(string.Join(' ', classLists.Where(s => !string.IsNullOrWhiteSpace(s))));
    }

    private string MergeCore(string classList)
    {
        var tokens = classList.Split(_whitespace, StringSplitOptions.RemoveEmptyEntries);

        // Each input position maps to the tokens that survive there: the token itself when kept, an
        // empty list when dropped, or several longhand tokens when a partially overridden shorthand
        // is decomposed (my-4 -> mb-4). Flattening these in index order preserves relative order.
        var outputs = new List<string>[tokens.Length];

        // Right-to-left, mirroring tailwind-merge: a class is dropped when a single later KEPT class
        // with the same variant chain and importance overrides everything it writes. A class that is
        // only PARTIALLY overridden is rewritten into the longhand classes that survive — see
        // DecomposeAndMerge.
        var keptByVariant = new Dictionary<(string VariantKey, bool Important), List<MergeSignature>>();
        for (var i = tokens.Length - 1; i >= 0; i--)
        {
            var signature = GetSignature(tokens[i]);
            if (signature is null)
            {
                outputs[i] = [tokens[i]];
                continue;
            }

            var bucket = (signature.VariantKey, signature.Important);
            if (!keptByVariant.TryGetValue(bucket, out var laterSignatures))
            {
                laterSignatures = [];
                keptByVariant[bucket] = laterSignatures;
            }

            if (laterSignatures.Any(later => later.Overrides(signature)))
            {
                // Fully overridden by a single later class — drop it.
                outputs[i] = [];
                continue;
            }

            if (PartiallyOverlaps(signature, laterSignatures) &&
                DecomposeAndMerge(tokens[i], signature, laterSignatures) is { } decomposed)
            {
                outputs[i] = decomposed.Tokens;
                laterSignatures.AddRange(decomposed.Signatures);
                continue;
            }

            outputs[i] = [tokens[i]];
            laterSignatures.Add(signature);
        }

        var kept = new List<string>(tokens.Length);
        foreach (var output in outputs)
        {
            kept.AddRange(output);
        }

        return string.Join(' ', kept);
    }

    // True when at least one later kept class writes (or covers) one of this class's keys without a
    // single later class being a full superset — i.e. the class is partially overridden and a
    // candidate for decomposition.
    private static bool PartiallyOverlaps(MergeSignature signature, List<MergeSignature> laterSignatures)
    {
        foreach (var later in laterSignatures)
        {
            foreach (var write in signature.Writes)
            {
                if (later.Writes.Contains(write) || later.Covers.Contains(write))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Rewrites a partially overridden shorthand into the longhand classes that survive, recursing so
    // only the conflicting sub-axis breaks down (m-4 mt-6 -> mx-4 mb-4 mt-6). Returns null — meaning
    // "keep the original token unchanged" — unless the rewrite actually drops a conflicting side.
    private (List<string> Tokens, List<MergeSignature> Signatures)? DecomposeAndMerge(
        string token,
        MergeSignature parentSignature,
        List<MergeSignature> laterSignatures)
    {
        var children = _signatureBuilder.TryDecompose(token);
        if (children is null)
        {
            return null;
        }

        var survivorTokens = new List<string>();
        var survivorSignatures = new List<MergeSignature>();

        // Children share the later-class context and may themselves be kept; track them alongside so
        // a surviving child participates in conflict checks for the children processed after it.
        var localLater = new List<MergeSignature>(laterSignatures);
        var anyOverrideDrop = false;

        foreach (var childToken in children)
        {
            var childSignature = GetSignature(childToken);
            if (childSignature is null)
            {
                // Synthesized child didn't round-trip — abandon decomposition, keep the original.
                return null;
            }

            if (!childSignature.Writes.IsSubsetOf(parentSignature.Writes))
            {
                // Soundness: a child that writes a key the parent didn't (an overloaded root inferring
                // a different property family) is not a valid decomposition of it — abandon.
                return null;
            }

            if (localLater.Any(later => later.Overrides(childSignature)))
            {
                // This longhand side is fully overridden by a later class — drop it.
                anyOverrideDrop = true;
                continue;
            }

            if (PartiallyOverlaps(childSignature, localLater) &&
                DecomposeAndMerge(childToken, childSignature, localLater) is { } nested)
            {
                anyOverrideDrop = true;
                survivorTokens.AddRange(nested.Tokens);
                survivorSignatures.AddRange(nested.Signatures);
                localLater.AddRange(nested.Signatures);
                continue;
            }

            survivorTokens.Add(childToken);
            survivorSignatures.Add(childSignature);
            localLater.Add(childSignature);
        }

        // Only commit when decomposition removed a conflicting side; otherwise the rewrite is noise
        // (e.g. a physical/logical mismatch like mx-4 ms-2 where no child is overridden).
        return anyOverrideDrop ? (survivorTokens, survivorSignatures) : null;
    }

    private MergeSignature? GetSignature(string token)
    {
        if (_signatureCache.TryGetValue(token, out var cached))
        {
            return cached;
        }

        var signature = _signatureBuilder.ComputeSignature(token);
        _signatureCache.Set(token, signature);
        return signature;
    }
}
