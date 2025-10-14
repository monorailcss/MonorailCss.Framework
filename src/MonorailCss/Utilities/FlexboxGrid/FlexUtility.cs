using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how flex items both grow and shrink.
/// </summary>
internal class FlexUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    public string[] GetNamespaces() => NamespaceResolver.FlexChain;

    public string[] GetFunctionalRoots() => ["flex"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility || functionalUtility.Root != "flex")
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        string flexValue;

        // Handle static values first
        if (functionalUtility.Value.Kind == ValueKind.Named)
        {
            var key = functionalUtility.Value.Value;

            flexValue = key switch
            {
                "auto" => "auto",
                "initial" => "0 auto",
                "none" => "none",
                _ => string.Empty,
            };

            // If it's a static value, use it
            if (!string.Empty.Equals(flexValue))
            {
                results = ImmutableList.Create<AstNode>(
                    new Declaration("flex", flexValue, candidate.Important));
                return true;
            }

            // Handle fractions (flex-1/2, flex-1/3, etc.)
            if (key.Contains('/') && TryParseFraction(key, out var fractionValue))
            {
                flexValue = fractionValue;
                results = ImmutableList.Create<AstNode>(
                    new Declaration("flex", flexValue, candidate.Important));
                return true;
            }

            // Handle numeric values (flex-1, flex-2, etc.)
            if (int.TryParse(key, out var numValue) && numValue >= 0)
            {
                flexValue = numValue.ToString();
                results = ImmutableList.Create<AstNode>(
                    new Declaration("flex", flexValue, candidate.Important));
                return true;
            }
        }

        // Handle arbitrary values (flex-[2], flex-[1_1_0%], etc.)
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = functionalUtility.Value.Value;

            // Replace underscores with spaces for complex flex values like "1_1_0%"
            flexValue = arbitrary.Replace('_', ' ');

            // Basic validation - should contain valid CSS flex values
            if (IsValidFlexValue(flexValue))
            {
                results = ImmutableList.Create<AstNode>(
                    new Declaration("flex", flexValue, candidate.Important));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid flex shorthand value.
    /// </summary>
    private static bool IsValidFlexValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "initial", "none", "inherit", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow single numbers (flex-grow)
        if (double.TryParse(value.Trim(), out _))
        {
            return true;
        }

        // Allow complex values like "1 1 0%", "2 2 auto", etc.
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 1 && parts.Length <= 3)
        {
            // First part should be flex-grow (number)
            if (!double.TryParse(parts[0], out _))
            {
                return false;
            }

            // If second part exists, should be flex-shrink (number)
            if (parts.Length >= 2 && !double.TryParse(parts[1], out _))
            {
                return false;
            }

            // Third part would be flex-basis (length, percentage, or keywords)
            if (parts.Length == 3)
            {
                var basis = parts[2];

                // Allow keywords
                if (new[] { "auto", "content" }.Contains(basis))
                {
                    return true;
                }

                // Allow lengths and percentages - basic check for units
                if (basis.EndsWith("px") || basis.EndsWith("em") || basis.EndsWith("rem") ||
                    basis.EndsWith("%") || basis.EndsWith("ch") || basis.EndsWith("vw") ||
                    basis.EndsWith("vh") || double.TryParse(basis.TrimEnd('0'), out _))
                {
                    return true;
                }
            }

            return true;
        }

        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to parse a fraction value for flex properties.
    /// </summary>
    private static bool TryParseFraction(string key, out string fractionValue)
    {
        fractionValue = string.Empty;

        var parts = key.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var numerator) ||
            !int.TryParse(parts[1], out var denominator) ||
            denominator <= 0)
        {
            return false;
        }

        fractionValue = $"calc({numerator}/{denominator} * 100%)";
        return true;
    }

    /// <summary>
    /// Returns examples of flex utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("flex-auto", "Allow flex item to grow and shrink as needed"),
            new("flex-initial", "Allow flex item to shrink but not grow"),
            new("flex-none", "Prevent flex item from growing or shrinking"),
            new("flex-1", "Allow flex item to grow and shrink equally (flex: 1)"),
            new("flex-2", "Set flex grow factor to 2"),
            new("flex-[2]", "Set flex value with arbitrary number"),
            new("flex-[1_1_0%]", "Set flex with custom grow, shrink, and basis values"),
        };

        return examples;
    }

    public string[]? GetDocumentedProperties() => ["flex"];
}