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
        var keep = new bool[tokens.Length];

        // Right-to-left, mirroring tailwind-merge: a class is dropped when a single later KEPT
        // class with the same variant chain and importance overrides everything it writes.
        var keptByVariant = new Dictionary<(string VariantKey, bool Important), List<MergeSignature>>();
        for (var i = tokens.Length - 1; i >= 0; i--)
        {
            var signature = GetSignature(tokens[i]);
            if (signature is null)
            {
                keep[i] = true;
                continue;
            }

            var bucket = (signature.VariantKey, signature.Important);
            if (keptByVariant.TryGetValue(bucket, out var laterSignatures))
            {
                if (laterSignatures.Any(later => later.Overrides(signature)))
                {
                    continue;
                }

                laterSignatures.Add(signature);
            }
            else
            {
                keptByVariant[bucket] = [signature];
            }

            keep[i] = true;
        }

        var kept = new List<string>(tokens.Length);
        for (var i = 0; i < tokens.Length; i++)
        {
            if (keep[i])
            {
                kept.Add(tokens[i]);
            }
        }

        return string.Join(' ', kept);
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
