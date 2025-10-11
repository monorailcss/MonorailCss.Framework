using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utility for CSS content property.
/// Handles: content-none, content-['text'], content-[url('/image.png')], content-[attr(data-content)], etc.
/// CSS: content: none; --tw-content: value; content: var(--tw-content).
/// </summary>
internal class ContentUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.StandardFunctional;

    public string[] GetNamespaces() => NamespaceResolver.ContentChain;

    public string[] GetFunctionalRoots() => ["content"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        // Handle functional content utilities
        if (candidate is FunctionalUtility functionalUtility && functionalUtility.Root == "content")
        {
            // Handle content-none (parsed as content with value="none")
            if (functionalUtility.Value?.Kind == ValueKind.Named && functionalUtility.Value.Value == "none")
            {
                results = ImmutableList.Create<AstNode>(
                    new Declaration("--tw-content", "none", candidate.Important),
                    new Declaration("content", "none", candidate.Important));
                return true;
            }

            // Handle content-[arbitrary] values
            if (functionalUtility.Value?.Kind == ValueKind.Arbitrary)
            {
                var arbitraryValue = functionalUtility.Value.Value;

                if (!IsValidContentValue(arbitraryValue))
                {
                    return false;
                }

                var processedValue = ProcessContentValue(arbitraryValue);

                var declarations = new List<AstNode>
                {
                    new Declaration("--tw-content", processedValue, candidate.Important),
                    new Declaration("content", "var(--tw-content)", candidate.Important),
                };

                results = declarations.ToImmutableList();
                return true;
            }
        }

        return false;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register the --tw-content CSS property
        propertyRegistry.Register("--tw-content", "*", false, null);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid CSS content value.
    /// </summary>
    private static bool IsValidContentValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow various CSS content value types:
        // - String literals (quoted)
        // - url() functions
        // - attr() functions
        // - counter() functions
        // - counters() functions
        // - CSS keywords
        // - CSS variables

        // Allow CSS variables
        if (value.StartsWith("var("))
        {
            return true;
        }

        // Allow quoted strings
        if ((value.StartsWith("'") && value.EndsWith("'")) ||
            (value.StartsWith("\"") && value.EndsWith("\"")))
        {
            return true;
        }

        // Allow CSS functions
        if (value.Contains("url(") ||
            value.Contains("attr(") ||
            value.Contains("counter(") ||
            value.Contains("counters("))
        {
            return true;
        }

        // Allow CSS keywords
        var keywords = new[] { "normal", "none", "initial", "inherit", "unset", "revert", "open-quote", "close-quote", "no-open-quote", "no-close-quote" };
        if (keywords.Contains(value))
        {
            return true;
        }

        // For other cases, be permissive to allow future CSS values
        return true;
    }

    /// <summary>
    /// Processes the content value, handling underscore replacements and escaping.
    /// </summary>
    private static string ProcessContentValue(string value)
    {
        // Replace underscores with spaces (Tailwind convention for arbitrary values)
        var processed = value.Replace("_", " ");

        return processed;
    }
}