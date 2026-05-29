using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;

namespace MonorailCss.Utilities;

/// <summary>
/// Handles arbitrary CSS properties using the [property:value] syntax.
/// Examples:
/// - [display:flex] → display: flex;
/// - [font-family:Inter] → font-family: Inter;
/// - [grid-template-columns:1fr_2fr_1fr] → grid-template-columns: 1fr 2fr 1fr;
/// - [--custom-property:value] → --custom-property: value;
/// - [color:var(--primary)] → color: var(--primary); (with theme variable output).
/// </summary>
internal class ArbitraryPropertyUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ArbitraryHandler;

    public string[] GetNamespaces() => [];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not ArbitraryProperty arbitraryProperty)
        {
            return false;
        }

        // Process the CSS property name - no transformation needed
        var property = arbitraryProperty.Property;

        // Process the CSS value - convert underscores to spaces for multi-value properties
        var value = ProcessValue(arbitraryProperty.Value);

        // Validate that the property name is not empty and doesn't start with uppercase
        // (following the same validation as in CandidateParser)
        if (string.IsNullOrWhiteSpace(property) || (property.Length > 0 && char.IsUpper(property[0])))
        {
            return false;
        }

        // Theme variable usage (var() references) is tracked centrally by ThemeVariableTrackingStage.

        // Create the CSS declaration
        var declaration = new Declaration(property, value, candidate.Important);

        results = ImmutableList.Create<AstNode>(declaration);
        return true;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        return TryCompile(candidate, theme, out results);
    }

    /// <summary>
    /// Processes the CSS value by converting underscores to spaces.
    /// This allows values like "1fr_2fr_1fr" to become "1fr 2fr 1fr".
    /// </summary>
    private static string ProcessValue(string value)
    {
        return value.Replace('_', ' ');
    }
}