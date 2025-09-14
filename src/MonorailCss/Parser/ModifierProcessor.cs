using MonorailCss.Candidates;

namespace MonorailCss.Parser;

/// <summary>
/// Handles extraction and processing of modifiers from utility class names.
/// Distinguishes between opacity modifiers (e.g., bg-red-500/50) and fraction values (e.g., w-1/2).
/// </summary>
internal sealed class ModifierProcessor
{
    private readonly ArbitraryValueParser _arbitraryValueParser;

    public ModifierProcessor(ArbitraryValueParser arbitraryValueParser)
    {
        _arbitraryValueParser = arbitraryValueParser;
    }

    /// <summary>
    /// Extracts the modifier from a utility class name.
    /// </summary>
    public ModifierExtractionResult ExtractModifier(string baseUtility)
    {
        // Check if this might be a fraction utility by looking for patterns that indicate fractions
        // We need to distinguish between fractions (w-1/2) and opacity modifiers (bg-red-500/50)
        var mightBeFractionUtility = false;

        if (baseUtility.Contains('/') && baseUtility.Contains('-'))
        {
            // Get the segments split by slash
            var slashSegments = baseUtility.Split('/');
            if (slashSegments.Length >= 2)
            {
                var beforeSlash = slashSegments[0];
                var afterSlash = slashSegments[1];

                // Extract potential value (everything after the last dash)
                var lastDashIndex = beforeSlash.LastIndexOf('-');
                if (lastDashIndex > 0)
                {
                    var potentialValue = beforeSlash[(lastDashIndex + 1)..];
                    var fullFractionCandidate = potentialValue + "/" + afterSlash;

                    // Only consider it a fraction if:
                    // 1. The value part looks like a valid fraction
                    // 2. The numerator is very small (fractions are typically 1/2, 1/3, 2/3, 3/4, etc.)
                    // 3. The denominator is also small (typical fractions use denominators 2-12)
                    if (IsFractionValue(fullFractionCandidate))
                    {
                        var slashIndex = fullFractionCandidate.IndexOf('/');
                        var numeratorPart = fullFractionCandidate[..slashIndex];
                        var denominatorPart = fullFractionCandidate[(slashIndex + 1)..];

                        // Fraction heuristic: small numbers on both sides
                        // This avoids things like -m-4/50 (margin with opacity modifier)
                        // but allows w-1/2, size-3/4, etc.
                        if (int.TryParse(numeratorPart, out var numerator) &&
                            int.TryParse(denominatorPart, out var denominator) &&
                            numerator <= 12 && denominator <= 12 &&
                            numerator < denominator)
                        {
                            // numerator should be less than denominator for typical fractions
                            mightBeFractionUtility = true;
                        }
                    }
                }
            }
        }

        // Split by slash to check for modifiers
        var segments = SegmentHelper.Segment(baseUtility, '/');

        // More than 2 segments could mean either:
        // 1. Invalid multiple modifiers (bg-red-500/50/75)
        // 2. Fraction with modifier (w-1/2/50)
        if (segments.Count > 3)
        {
            return new ModifierExtractionResult
            {
                IsValid = false,
            }; // Too many slashes
        }

        if (segments.Count == 3)
        {
            if (mightBeFractionUtility && IsFractionValue(segments[0].Split('-').Last() + "/" + segments[1]))
            {
                // This is w-1/2/50 (fraction with modifier)
                // Reconstruct the fraction part
                var fractionUtility = segments[0] + "/" + segments[1];
                var modifierValue = segments[2];

                // Parse the modifier
                if (modifierValue.StartsWith('[') && modifierValue.EndsWith(']'))
                {
                    var innerValue = modifierValue[1..^1];
                    if (string.IsNullOrEmpty(innerValue))
                    {
                        return new ModifierExtractionResult
                        {
                            IsValid = false,
                        };
                    }

                    return new ModifierExtractionResult
                    {
                        IsValid = true,
                        BaseUtility = fractionUtility,
                        Modifier = Modifier.Arbitrary(innerValue),
                    };
                }

                return new ModifierExtractionResult
                {
                    IsValid = true,
                    BaseUtility = fractionUtility,
                    Modifier = Modifier.Named(modifierValue),
                };
            }

            // Invalid: multiple modifiers like bg-red-500/50/75
            return new ModifierExtractionResult
            {
                IsValid = false,
            };
        }

        if (segments.Count == 2)
        {
            var utilityPart = segments[0];
            var modifierValue = segments[1];

            // Check for different modifier types
            if (string.IsNullOrEmpty(modifierValue))
            {
                // Empty modifier is invalid
                return new ModifierExtractionResult
                {
                    IsValid = false,
                };
            }

            if (modifierValue.StartsWith('(') && modifierValue.EndsWith(')'))
            {
                // Parentheses shorthand for CSS variable modifier: /(--opacity)
                var innerValue = modifierValue[1..^1];
                var parsed = _arbitraryValueParser.Parse(innerValue, ArbitraryValueType.Parentheses);
                if (parsed.IsValid)
                {
                    return new ModifierExtractionResult
                    {
                        IsValid = true,
                        BaseUtility = utilityPart,
                        Modifier = Modifier.Arbitrary(parsed.Value!),
                    };
                }

                return new ModifierExtractionResult
                {
                    IsValid = false,
                }; // Invalid parentheses modifier
            }

            if (modifierValue.StartsWith('[') && modifierValue.EndsWith(']'))
            {
                // Arbitrary modifier: /[0.5] or /[var(--opacity)]
                var innerValue = modifierValue[1..^1];
                if (string.IsNullOrEmpty(innerValue))
                {
                    return new ModifierExtractionResult
                    {
                        IsValid = false,
                    }; // Empty arbitrary modifier
                }

                return new ModifierExtractionResult
                {
                    IsValid = true,
                    BaseUtility = utilityPart,
                    Modifier = Modifier.Arbitrary(innerValue),
                };
            }

            // Check if this might be a fraction (for utilities that support it)
            if (mightBeFractionUtility && IsFractionValue(segments[0].Split('-').Last() + "/" + modifierValue))
            {
                // It's actually a fraction, not a modifier (e.g., w-1/2)
                // Don't split it, keep the full utility
                return new ModifierExtractionResult
                {
                    IsValid = true,
                    BaseUtility = segments[0] + "/" + segments[1],
                    Modifier = null,
                };
            }

            // Named modifier: /50
            return new ModifierExtractionResult
            {
                IsValid = true,
                BaseUtility = utilityPart,
                Modifier = Modifier.Named(modifierValue),
            };
        }

        return new ModifierExtractionResult
        {
            IsValid = true,
            BaseUtility = baseUtility,
            Modifier = null,
        };
    }

    /// <summary>
    /// Determines if a string represents a fraction value (e.g., "1/2", "3/4").
    /// </summary>
    private static bool IsFractionValue(string value)
    {
        // Must contain exactly one slash
        var slashIndex = value.IndexOf('/');
        if (slashIndex <= 0 || value.IndexOf('/', slashIndex + 1) != -1)
        {
            return false;
        }

        var numerator = value[..slashIndex];
        var denominator = value[(slashIndex + 1)..];

        // Both parts must be non-empty and numeric
        if (string.IsNullOrEmpty(numerator) || string.IsNullOrEmpty(denominator))
        {
            return false;
        }

        // Check if both parts are valid numbers (integers)
        return IsNumeric(numerator) && IsNumeric(denominator);
    }

    private static bool IsNumeric(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    internal record ModifierExtractionResult
    {
        public bool IsValid { get; init; }
        public string BaseUtility { get; init; } = string.Empty;
        public Modifier? Modifier { get; init; }
    }
}