using MonorailCss.Candidates;

namespace MonorailCss.Parser;

/// <summary>
/// Handles parsing of utility values including arbitrary values, CSS variables, fractions, and named values.
/// </summary>
internal sealed class CandidateValueParser
{
    private readonly ArbitraryValueParser _arbitraryValueParser;

    public CandidateValueParser(ArbitraryValueParser arbitraryValueParser)
    {
        _arbitraryValueParser = arbitraryValueParser;
    }

    /// <summary>
    /// Parses a value string into a CandidateValue.
    /// Returns null if the value is invalid.
    /// </summary>
    public CandidateValue? ParseValue(string? value)
    {
        if (value == null)
        {
            return null;
        }

        // Check if value is arbitrary with brackets [value]
        if (value.StartsWith('[') && value.EndsWith(']'))
        {
            var innerValue = value[1..^1];

            // Validate for empty arbitrary value
            if (string.IsNullOrWhiteSpace(innerValue) || innerValue == "_")
            {
                return null; // Invalid arbitrary value
            }

            var parsed = _arbitraryValueParser.Parse(innerValue, ArbitraryValueType.Brackets);
            if (!parsed.IsValid)
            {
                return null; // Parsing failed
            }

            return CandidateValue.Arbitrary(parsed.Value!);
        }

        // Check if value is CSS variable shorthand with parentheses (value)
        if (value.StartsWith('(') && value.EndsWith(')'))
        {
            var innerValue = value[1..^1];

            // Empty parentheses are invalid
            if (string.IsNullOrWhiteSpace(innerValue))
            {
                return null;
            }

            // Parse parentheses shorthand
            var parsed = _arbitraryValueParser.Parse(innerValue, ArbitraryValueType.Parentheses);
            if (!parsed.IsValid)
            {
                return null; // Not valid CSS variable syntax
            }

            // Store as arbitrary value with the expanded var() syntax
            return CandidateValue.Arbitrary(parsed.Value!);
        }

        // Check if this is a fraction value (e.g., "1/2")
        string? fraction = null;
        if (IsFractionValue(value))
        {
            fraction = value;
        }

        return CandidateValue.Named(value, fraction);
    }

    /// <summary>
    /// Determines if a string represents a fraction value (e.g., "1/2", "3/4").
    /// </summary>
    public static bool IsFractionValue(string value)
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
}