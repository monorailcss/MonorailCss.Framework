namespace MonorailCss.Parser;

/// <summary>
/// Handles extraction of tokens from class strings.
/// </summary>
internal sealed class TokenExtractor
{
    /// <summary>
    /// Splits a class string into individual class tokens.
    /// </summary>
    public string[] ExtractClassTokens(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Array.Empty<string>();
        }

        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Extracts the important flag (!) from a class name.
    /// </summary>
    public (string BaseUtilitiy, bool Important) ExtractImportantFlag(string className)
    {
        if (className.EndsWith('!'))
        {
            return (className[..^1], true);
        }

        if (className.StartsWith('!'))
        {
            return (className[1..], true);
        }

        return (className, false);
    }

    /// <summary>
    /// Result of tokenizing a utility class.
    /// </summary>
    internal record TokenizedUtility
    {
        public string Raw { get; init; } = string.Empty;
        public string BaseUtility { get; init; } = string.Empty;
        public bool Important { get; init; }
    }

    /// <summary>
    /// Tokenizes a utility class by extracting important flag.
    /// </summary>
    public TokenizedUtility TokenizeUtility(string utilityClass)
    {
        var (baseUtility, important) = ExtractImportantFlag(utilityClass);

        return new TokenizedUtility
        {
            Raw = utilityClass,
            BaseUtility = baseUtility,
            Important = important,
        };
    }
}