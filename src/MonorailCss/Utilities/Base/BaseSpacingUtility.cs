using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.DataTypes;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for utilities that resolve spacing values from the theme.
/// </summary>
internal abstract class BaseSpacingUtility : IUtility
{
    public virtual UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    /// <summary>
    /// Gets the utility patterns this handles (e.g., ["p", "px", "py"] for padding).
    /// </summary>
    protected abstract string[] Patterns { get; }

    /// <summary>
    /// Gets the namespace chain for resolving spacing values.
    /// Default is NamespaceResolver.BuildChain(NamespaceResolver.Spacing) but can be overridden.
    /// </summary>
    protected virtual string[] SpacingNamespaces => NamespaceResolver.BuildChain(NamespaceResolver.Spacing);

    public virtual string[] GetNamespaces() => SpacingNamespaces;

    public virtual string[] GetFunctionalRoots()
    {
        // Return both positive and negative versions of each pattern
        // Negative handling is now done by NegativeValueNormalizationStage
        var roots = new List<string>();
        foreach (var pattern in Patterns)
        {
            roots.Add(pattern);
            roots.Add($"-{pattern}");
        }

        return roots.ToArray();
    }

    public virtual bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        // Check if it's a negative utility (for pattern matching only)
        // Actual negative value handling is done by NegativeValueNormalizationStage
        var isNegative = functionalUtility.Root.StartsWith('-');
        var basePattern = isNegative ? functionalUtility.Root[1..] : functionalUtility.Root;

        if (!Patterns.Contains(basePattern))
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        if (!TryResolveSpacing(functionalUtility.Value, theme, out var spacing))
        {
            return false;
        }

        var declarations = GenerateDeclarations(basePattern, spacing, candidate.Important);
        if (declarations.Count == 0)
        {
            return false;
        }

        results = declarations;
        return true;
    }

    public virtual bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        // Check if it's a negative utility (for pattern matching only)
        // Actual negative value handling is done by NegativeValueNormalizationStage
        var isNegative = functionalUtility.Root.StartsWith('-');
        var basePattern = isNegative ? functionalUtility.Root[1..] : functionalUtility.Root;

        if (!Patterns.Contains(basePattern))
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        if (!TryResolveSpacing(functionalUtility.Value, theme, out var spacing))
        {
            return false;
        }

        var declarations = GenerateDeclarations(basePattern, spacing, candidate.Important, propertyRegistry);
        if (declarations.Count == 0)
        {
            return false;
        }

        results = declarations;
        return true;
    }

    /// <summary>
    /// Resolves a spacing value using the utility's namespace chain.
    /// </summary>
    protected virtual bool TryResolveSpacing(CandidateValue value, Theme.Theme theme, out string spacing)
    {
        spacing = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            // Check if it looks like a length value using DataTypeInference
            var arbitrary = value.Value;
            var inferredType = DataTypeInference.InferDataType(arbitrary, [DataType.Length, DataType.Percentage, DataType.Number]);

            if (inferredType is DataType.Length or DataType.Percentage or DataType.Number)
            {
                // Negative values are handled by NegativeValueNormalizationStage
                spacing = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values from theme
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Special handling for auto
            if (key == "auto")
            {
                spacing = "auto";
                return true;
            }

            // Special handling for px (1px)
            if (key == "px")
            {
                spacing = "1px";
                return true;
            }

            // Check if it's a numeric value (including decimals)
            if (IsNumericValue(key))
            {
                // Generate calc expression: calc(var(--spacing) * value)
                // Negative values are handled by NegativeValueNormalizationStage
                spacing = $"calc(var({NamespaceResolver.Spacing}) * {key})";

                // ThemeVariableTrackingStage automatically tracks var() references
                return true;
            }

            // Try to resolve from theme namespaces for non-numeric keys
            foreach (var ns in SpacingNamespaces)
            {
                var themeKey = $"{ns}-{key}";
                if (theme.ContainsKey(themeKey))
                {
                    spacing = $"var({themeKey})";
                    return true;
                }

                // Also try without dash for base namespace values
                if (ns == NamespaceResolver.Spacing && theme.ContainsKey($"{ns}{key}"))
                {
                    spacing = $"var({ns}{key})";
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a string represents a numeric value (including decimals).
    /// </summary>
    private static bool IsNumericValue(string value)
    {
        // Handle values like "0", "4", "0.5", "1.5", etc.
        return double.TryParse(value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out _);
    }

    /// <summary>
    /// Generates CSS declarations based on the utility pattern.
    /// Must be implemented by derived classes to handle specific spacing utilities.
    /// </summary>
    protected abstract ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important);

    /// <summary>
    /// Generates CSS declarations with property registry access.
    /// Default implementation calls the standard GenerateDeclarations method.
    /// Override to register custom properties.
    /// </summary>
    protected virtual ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        return GenerateDeclarations(pattern, value, important);
    }
}