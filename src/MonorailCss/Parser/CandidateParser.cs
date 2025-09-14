using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Candidates;
using MonorailCss.Variants;

namespace MonorailCss.Parser;

internal sealed class CandidateParser
{
    private readonly TokenExtractor _tokenExtractor;
    private readonly VariantTokenizer _variantTokenizer;
    private readonly ModifierProcessor _modifierProcessor;
    private readonly UtilityMatcher _utilityMatcher;
    private readonly CandidateValueParser _valueParser;

    public CandidateParser(UtilityRegistry utilityRegistry)
    {
        var arbitraryValueParser = new ArbitraryValueParser();

        _tokenExtractor = new TokenExtractor();
        _variantTokenizer = new VariantTokenizer();
        _modifierProcessor = new ModifierProcessor(arbitraryValueParser);
        _utilityMatcher = new UtilityMatcher(utilityRegistry, arbitraryValueParser);
        _valueParser = new CandidateValueParser(arbitraryValueParser);
    }

    public IEnumerable<Candidate> ParseCandidates(string input)
    {
        var classes = _tokenExtractor.ExtractClassTokens(input);

        foreach (var className in classes)
        {
            if (TryParseCandidate(className, out var candidate))
            {
                yield return candidate;
            }
        }
    }

    public bool TryParseCandidate(string input, [NotNullWhen(true)] out Candidate? candidate)
    {
        candidate = null;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // Step 1: Extract variants and base utility
        var (variants, baseUtility) = _variantTokenizer.Tokenize(input);

        // Step 2: Extract important flag
        var tokenized = _tokenExtractor.TokenizeUtility(baseUtility);

        // Step 3: Process modifiers
        var modifierResult = _modifierProcessor.ExtractModifier(tokenized.BaseUtility);
        if (!modifierResult.IsValid)
        {
            return false;
        }

        // Step 4: Match utility type
        var utilityMatch = _utilityMatcher.Match(modifierResult.BaseUtility);

        // Step 5: Create appropriate candidate based on utility type
        switch (utilityMatch.Type)
        {
            case UtilityMatcher.UtilityType.Static:
                if (modifierResult.Modifier != null)
                {
                    return false; // Static utilities don't support modifiers
                }

                candidate = CreateStaticUtility(input, utilityMatch, variants, tokenized.Important);
                return true;

            case UtilityMatcher.UtilityType.ArbitraryProperty:
                candidate = CreateArbitraryProperty(input, utilityMatch, variants, tokenized.Important, modifierResult.Modifier);
                return true;

            case UtilityMatcher.UtilityType.ParenthesesShorthand:
                // For parentheses shorthand, the value is already parsed by UtilityMatcher
                var parsedValue = utilityMatch.Value != null ? CandidateValue.Arbitrary(utilityMatch.Value) : null;
                candidate = CreateFunctionalUtility(input, utilityMatch, parsedValue, variants, tokenized.Important, modifierResult.Modifier);
                return true;

            case UtilityMatcher.UtilityType.Functional:
                var value = _valueParser.ParseValue(utilityMatch.Value);

                // For functional utilities, if value parsing fails on what should be a valid value, reject it
                if (utilityMatch.Value != null && value == null)
                {
                    return false;
                }

                candidate = CreateFunctionalUtility(input, utilityMatch, value, variants, tokenized.Important, modifierResult.Modifier);
                return true;

            default:
                return false;
        }
    }

    private StaticUtility CreateStaticUtility(string raw, UtilityMatcher.UtilityMatch utilityMatch, ImmutableList<VariantToken> variants, bool important)
    {
        return new StaticUtility
        {
            Raw = raw,
            Root = utilityMatch.Root,
            Variants = [.. variants],
            Important = important,
            Modifier = null,
            Normalized = GetNormalizedForm(variants, utilityMatch.Root, important),
        };
    }

    private ArbitraryProperty CreateArbitraryProperty(string raw, UtilityMatcher.UtilityMatch utilityMatch, ImmutableList<VariantToken> variants, bool important, Modifier? modifier)
    {
        return new ArbitraryProperty
        {
            Raw = raw,
            Property = utilityMatch.Property!,
            Value = utilityMatch.Value!,
            Variants = [.. variants],
            Important = important,
            Modifier = modifier,
            Normalized = GetNormalizedForm(variants, raw[raw.IndexOf('[')..], important),
        };
    }

    private FunctionalUtility CreateFunctionalUtility(string raw, UtilityMatcher.UtilityMatch utilityMatch, CandidateValue? value, ImmutableList<VariantToken> variants, bool important, Modifier? modifier)
    {
        return new FunctionalUtility
        {
            Raw = raw,
            Root = utilityMatch.Root,
            Value = value,
            Variants = [.. variants],
            Important = important,
            Modifier = modifier,
            Normalized = GetNormalizedForm(variants, utilityMatch.Root + (utilityMatch.Value != null ? "-" + utilityMatch.Value : string.Empty), important),
        };
    }

    private static string GetNormalizedForm(ImmutableList<VariantToken> variants, string baseUtility, bool important)
    {
        // Build canonical form: variants:utility with important marker
        var parts = variants.Select(variant => variant.Raw).ToList();

        if (important)
        {
            parts.Add($"!{baseUtility}");
        }
        else
        {
            parts.Add(baseUtility);
        }

        return string.Join(":", parts);
    }
}