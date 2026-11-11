using System.Collections.Frozen;
using System.Globalization;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Parser;
using MonorailCss.Utilities.Base;
using MonorailCss.Utilities.Spacing;
using MonorailCss.Utilities.Transforms;

namespace MonorailCss.Utilities;

/// <summary>
/// Enumerates concrete utility class names a framework instance compiles — the list behind
/// editor autocomplete (see <see cref="CssFramework.GetSuggestedClassNames"/>).
///
/// Static names come straight from the registry. Functional roots (from
/// <see cref="IUtility.GetFunctionalRoots"/>, plus <see cref="ISuggestionRootProvider"/> for the
/// colour utilities whose roots the parser only finds by its first-dash fallback) are expanded over:
/// <list type="bullet">
/// <item>the keys of the utility's own theme namespaces — so <c>bg-</c> picks up every
/// <c>--color-*</c> token, <c>text-</c> every <c>--text-*</c> size, <c>rounded-</c> every
/// <c>--radius-*</c>, including palettes a custom theme adds;</item>
/// <item>the Tailwind spacing scale (and fractions) for utilities that multiply <c>--spacing</c>;</item>
/// <item>for bare-number families (opacity, z, rotate, duration, border widths, ...) the values
/// Tailwind's own IntelliSense suggests, from <see cref="BareValues"/> — those utilities accept any
/// number, so the list is a curated seed rather than derivable;</item>
/// <item>a few keywords (<c>auto</c>, <c>full</c>, <c>none</c>, ...).</item>
/// </list>
/// Every generated name is compiled with the utility that produced it and only names that emit CSS
/// are kept — so the list is "suggestions that work", not an exhaustive grammar (no arbitrary
/// values, modifiers, negatives or variants).
/// </summary>
internal static class UtilityClassNameEnumerator
{
    // Tailwind's documented spacing scale, offered for utilities that multiply --spacing.
    private static readonly string[] SpacingScale =
    [
        "0", "px", "0.5", "1", "1.5", "2", "2.5", "3", "3.5", "4", "5", "6", "7", "8", "9", "10", "11", "12",
        "14", "16", "20", "24", "28", "32", "36", "40", "44", "48", "52", "56", "60", "64", "72", "80", "96",
    ];

    // Fractions sizing/inset/translate utilities accept; validated per utility like everything else.
    private static readonly string[] Fractions =
    [
        "1/2", "1/3", "2/3", "1/4", "3/4", "1/5", "2/5", "3/5", "4/5", "1/6", "5/6",
    ];

    // Keyword values several functional families accept (w-full, basis-auto, z-auto, rotate-none).
    private static readonly string[] Keywords =
    [
        "auto", "full", "screen", "none", "min", "max", "fit",
    ];

    // Bare values per root for families that take any number. Mirrors the per-utility
    // suggestion lists in Tailwind's utilities.ts so the popup offers the familiar steps
    // (opacity-5..100, duration-75..1000) rather than every integer. A root listed here gets
    // these instead of the spacing scale (ring-/ring-offset- resolve through --spacing too,
    // but nobody wants ring-14 suggested).
    private static readonly FrozenDictionary<string, string[]> BareValues = BuildBareValues();

    /// <summary>
    /// Streams the names grouped by root in natural root order (<c>absolute</c>, <c>accent-*</c>,
    /// ..., <c>bg</c>, <c>bg-amber-50</c>, ..., <c>block</c>, ...), each group naturally sorted.
    /// Work is done per group as the sequence is consumed, so a caller on a single thread (Blazor
    /// WebAssembly) can build the list in slices without blocking. The registry is snapshotted
    /// when this is called, not on first enumeration.
    /// </summary>
    public static IEnumerable<string> Enumerate(UtilityRegistry registry, CandidateParser parser, Theme.Theme theme)
    {
        var staticLookup = registry.StaticUtilitiesLookup;

        // Plan the groups up front — cheap, nothing is compiled yet: which utilities own each
        // functional root (bg- is owned by the colour, image, position and size utilities).
        var owners = new Dictionary<string, List<IUtility>>(StringComparer.Ordinal);
        foreach (var utility in registry.RegisteredUtilities)
        {
            var roots = utility is ISuggestionRootProvider provider
                ? [.. utility.GetFunctionalRoots(), .. provider.GetSuggestionRoots()]
                : utility.GetFunctionalRoots();

            foreach (var root in roots)
            {
                // Negative roots (-mt-, -translate-x-) double the list for little benefit; the
                // positive form is what an author starts typing from.
                if (root.Length == 0 || root[0] == '-')
                {
                    continue;
                }

                if (!owners.TryGetValue(root, out var list))
                {
                    owners[root] = list = [];
                }

                if (!list.Contains(utility))
                {
                    list.Add(utility);
                }
            }
        }

        var keys = staticLookup.Keys.Union(owners.Keys, StringComparer.Ordinal).ToList();
        keys.Sort(NaturalClassNameComparer.Instance);

        return Stream(keys, staticLookup, owners, parser, theme);
    }

    private static IEnumerable<string> Stream(
        List<string> keys,
        IReadOnlyDictionary<string, IUtility> staticLookup,
        Dictionary<string, List<IUtility>> owners,
        CandidateParser parser,
        Theme.Theme theme)
    {
        var emitted = new HashSet<string>(StringComparer.Ordinal);
        var group = new List<string>();
        var inGroup = new HashSet<string>(StringComparer.Ordinal);
        var plans = new Dictionary<IUtility, (List<string> ThemeValues, bool UsesSpacing, bool TakesKeywords)>();

        foreach (var key in keys)
        {
            group.Clear();
            inGroup.Clear();

            if (staticLookup.TryGetValue(key, out var staticUtility) && Compiles(key, staticUtility, parser, theme))
            {
                group.Add(key);
                inGroup.Add(key);
            }

            if (owners.TryGetValue(key, out var rootOwners))
            {
                foreach (var utility in rootOwners)
                {
                    ExpandRoot(key, utility, GetPlan(utility));
                }
            }

            group.Sort(NaturalClassNameComparer.Instance);
            foreach (var name in group)
            {
                if (emitted.Add(name))
                {
                    yield return name;
                }
            }
        }

        (List<string> ThemeValues, bool UsesSpacing, bool TakesKeywords) GetPlan(IUtility utility)
        {
            if (!plans.TryGetValue(utility, out var plan))
            {
                var themeValues = CollectThemeValues(utility, theme, out var usesSpacing);

                // Sizing/inset/basis/translate/margin are the spacing utilities with keyword
                // values (w-full, inset-auto, translate-full, mx-auto). Padding/gap/space only
                // resolve `auto` by accident of the shared spacing resolver, so they don't.
                var takesKeywords = !usesSpacing
                                    || utility is BaseSizingUtility or BaseFractionAwareSpacingUtility or MarginUtility or TranslateUtility;
                plans[utility] = plan = (themeValues, usesSpacing, takesKeywords);
            }

            return plan;
        }

        void ExpandRoot(string root, IUtility utility, (List<string> ThemeValues, bool UsesSpacing, bool TakesKeywords) plan)
        {
            // Roots with a default value compile bare (shadow, rounded, border, grow, ...).
            TryAdd(root);

            foreach (var value in plan.ThemeValues)
            {
                TryAdd($"{root}-{value}");
            }

            if (BareValues.TryGetValue(root, out var bare))
            {
                foreach (var value in bare)
                {
                    TryAdd($"{root}-{value}");
                }
            }
            else if (plan.UsesSpacing)
            {
                foreach (var value in SpacingScale.Concat(Fractions))
                {
                    TryAdd($"{root}-{value}");
                }
            }

            // A root that compiles any identifier (some mask-* helpers pass the value straight
            // through) would "accept" every keyword, so it only gets its theme values.
            if (plan.TakesKeywords && !Compiles($"{root}-zzqx", utility, parser, theme))
            {
                foreach (var value in Keywords)
                {
                    TryAdd($"{root}-{value}");
                }
            }

            void TryAdd(string name)
            {
                // A name another owner of this root already produced needn't be compiled again.
                // (One it rejected still must be: bg-red-500 fails for bg-image, passes for bg-color.)
                if (!emitted.Contains(name) && !inGroup.Contains(name) && Compiles(name, utility, parser, theme))
                {
                    group.Add(name);
                    inGroup.Add(name);
                }
            }
        }
    }

    // Value suffixes from the utility's theme namespaces (--color-red-500 → red-500 for bg-).
    // --spacing itself has no keyed values; it just flags the utility as spacing-scaled.
    private static List<string> CollectThemeValues(IUtility utility, Theme.Theme theme, out bool usesSpacing)
    {
        var values = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        usesSpacing = utility is BaseSizingUtility;

        foreach (var ns in utility.GetNamespaces() ?? [])
        {
            if (string.IsNullOrEmpty(ns))
            {
                continue;
            }

            if (ns == NamespaceResolver.Spacing)
            {
                usesSpacing = true;
                continue;
            }

            var prefix = ns + "-";
            foreach (var (key, _) in theme.EnumerateNamespace(prefix))
            {
                var suffix = key[prefix.Length..];

                // Skip companion tokens such as --text-lg--line-height.
                if (suffix.Length == 0 || suffix.Contains("--", StringComparison.Ordinal))
                {
                    continue;
                }

                if (seen.Add(suffix))
                {
                    values.Add(suffix);
                }
            }
        }

        return values;
    }

    private static FrozenDictionary<string, string[]> BuildBareValues()
    {
        static string[] Range(int from, int to, int step = 1) =>
            [.. Enumerable.Range(0, ((to - from) / step) + 1).Select(i => (from + (i * step)).ToString(CultureInfo.InvariantCulture))];

        var map = new Dictionary<string, string[]>(StringComparer.Ordinal);

        void Set(string[] roots, string[] values)
        {
            foreach (var root in roots)
            {
                map[root] = values;
            }
        }

        Set(["opacity", "backdrop-opacity"], Range(0, 100, 5));
        Set(["z"], ["0", "10", "20", "30", "40", "50"]);
        Set(["order", "grid-cols", "grid-rows", "col-span", "row-span", "columns"], Range(1, 12));
        Set(["col-start", "col-end", "row-start", "row-end"], Range(1, 13));
        Set(["line-clamp"], Range(1, 6));
        Set(
            ["border", "border-x", "border-y", "border-t", "border-r", "border-b", "border-l", "border-s", "border-e", "border-bs", "border-be", "divide-x", "divide-y"],
            ["0", "2", "4", "8"]);
        Set(["ring", "inset-ring", "ring-offset", "outline", "outline-offset", "decoration", "underline-offset"], ["0", "1", "2", "4", "8"]);
        Set(["stroke"], ["0", "1", "2"]);
        Set(["duration", "delay"], ["0", "75", "100", "150", "200", "300", "500", "700", "1000"]);
        Set(["rotate", "rotate-x", "rotate-y", "rotate-z"], ["0", "1", "2", "3", "6", "12", "45", "90", "180"]);
        Set(["skew", "skew-x", "skew-y"], ["0", "1", "2", "3", "6", "12"]);
        Set(["scale", "scale-x", "scale-y", "scale-z"], ["0", "50", "75", "90", "95", "100", "105", "110", "125", "150"]);
        Set(["hue-rotate", "backdrop-hue-rotate"], ["0", "15", "30", "60", "90", "180"]);
        Set(["brightness", "backdrop-brightness"], ["0", "50", "75", "90", "95", "100", "105", "110", "125", "150", "200"]);
        Set(["contrast", "backdrop-contrast"], ["0", "50", "75", "100", "125", "150", "200"]);
        Set(["saturate", "backdrop-saturate"], ["0", "50", "100", "150", "200"]);
        Set(["grayscale", "invert", "sepia", "backdrop-grayscale", "backdrop-invert", "backdrop-sepia"], ["0", "25", "50", "75", "100"]);
        Set(["grow", "shrink", "flex-grow", "flex-shrink"], ["0"]);
        Set(["leading"], Range(3, 10));
        Set(["tab"], ["2", "4", "8"]);

        return map.ToFrozenDictionary(StringComparer.Ordinal);
    }

    // A name counts only if the utility that generated it compiles it to at least one node —
    // the same check CssFramework's compile path makes, minus the priority-ordered probe.
    private static bool Compiles(string name, IUtility utility, CandidateParser parser, Theme.Theme theme)
    {
        if (!parser.TryParseCandidate(name, out var candidate))
        {
            return false;
        }

        try
        {
            return utility.TryCompile(candidate, theme, new CssPropertyRegistry(), out var nodes)
                   && nodes is { Count: > 0 };
        }
        catch (Exception)
        {
            // A utility that throws on a probe value just doesn't get that suggestion.
            return false;
        }
    }

    /// <summary>
    /// Orders class names the way a person scans them: <c>p-2</c> before <c>p-10</c>,
    /// <c>w-1/2</c> before <c>w-2/3</c>, <c>bg-red-50</c> before <c>bg-red-500</c>.
    /// </summary>
    private sealed class NaturalClassNameComparer : IComparer<string>
    {
        public static readonly NaturalClassNameComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            int i = 0, j = 0;
            while (i < x.Length && j < y.Length)
            {
                if (char.IsAsciiDigit(x[i]) && char.IsAsciiDigit(y[j]))
                {
                    var xEnd = ScanNumber(x, i);
                    var yEnd = ScanNumber(y, j);
                    var xNum = decimal.Parse(x.AsSpan(i, xEnd - i), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    var yNum = decimal.Parse(y.AsSpan(j, yEnd - j), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    var byNumber = xNum.CompareTo(yNum);
                    if (byNumber != 0)
                    {
                        return byNumber;
                    }

                    i = xEnd;
                    j = yEnd;
                    continue;
                }

                var byChar = x[i].CompareTo(y[j]);
                if (byChar != 0)
                {
                    return byChar;
                }

                i++;
                j++;
            }

            return (x.Length - i).CompareTo(y.Length - j);
        }

        // Digits with at most one interior decimal point ("0.5", "2.5").
        private static int ScanNumber(string s, int start)
        {
            var end = start;
            var sawPoint = false;
            while (end < s.Length)
            {
                if (char.IsAsciiDigit(s[end]))
                {
                    end++;
                }
                else if (s[end] == '.' && !sawPoint && end + 1 < s.Length && char.IsAsciiDigit(s[end + 1]))
                {
                    sawPoint = true;
                    end++;
                }
                else
                {
                    break;
                }
            }

            return end;
        }
    }
}
