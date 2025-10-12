using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.DataTypes;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for sizing utilities that handle width, height, and size properties.
/// Provides specialized handling for fractions, viewport units, and sizing-specific values.
/// </summary>
internal abstract class BaseSizingUtility : IUtility
{
    public virtual UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    /// <summary>
    /// Gets the utility patterns this handles (e.g., ["w"] for width, ["h"] for height).
    /// </summary>
    protected abstract string[] Patterns { get; }

    /// <summary>
    /// Gets the namespace chain for resolving sizing values.
    /// Override to provide custom namespaces like ["--width", "--spacing"] for width utilities.
    /// </summary>
    protected abstract string[] SizingNamespaces { get; }

    /// <summary>
    /// Gets the dimension type for viewport units (e.g., "Width" or "Height").
    /// Used to determine whether to use vw/vh units.
    /// </summary>
    protected abstract SizingDimension Dimension { get; }

    public virtual string[] GetNamespaces() => SizingNamespaces;

    public virtual string[] GetFunctionalRoots()
    {
        // Sizing utilities don't support negative values
        return Patterns;
    }

    public virtual bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (!Patterns.Contains(functionalUtility.Root))
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        if (!TryResolveSizing(functionalUtility.Value, theme, out var sizing))
        {
            return false;
        }

        var declarations = GenerateDeclarations(functionalUtility.Root, sizing, candidate.Important);
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

        if (!Patterns.Contains(functionalUtility.Root))
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        if (!TryResolveSizing(functionalUtility.Value, theme, out var sizing))
        {
            return false;
        }

        var declarations = GenerateDeclarations(functionalUtility.Root, sizing, candidate.Important, propertyRegistry);
        if (declarations.Count == 0)
        {
            return false;
        }

        results = declarations;
        return true;
    }

    /// <summary>
    /// Resolves a sizing value using specialized sizing logic.
    /// Handles fractions, viewport units, special keywords, and theme values.
    /// </summary>
    protected virtual bool TryResolveSizing(CandidateValue value, Theme.Theme theme, out string sizing)
    {
        sizing = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Allow CSS functions (var(), calc(), min(), max(), clamp(), etc.)
            if (IsCssFunction(arbitrary))
            {
                sizing = arbitrary;
                return true;
            }

            // Check if it looks like a length/percentage value using DataTypeInference
            var inferredType = DataTypeInference.InferDataType(arbitrary, [DataType.Length, DataType.Percentage, DataType.Number]);

            if (inferredType is DataType.Length or DataType.Percentage or DataType.Number)
            {
                sizing = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle special sizing keywords
            var specialValue = GetSpecialSizingValue(key);
            if (!string.IsNullOrEmpty(specialValue))
            {
                sizing = specialValue;
                return true;
            }

            // Handle fractions (e.g., "1/2", "1/3", "2/3", etc.)
            if (TryParseFraction(key, out var fractionValue))
            {
                sizing = fractionValue;
                return true;
            }

            // Special handling for px (1px)
            if (key == "px")
            {
                sizing = "1px";
                return true;
            }

            // Check if it's a numeric value (including decimals)
            if (IsNumericValue(key))
            {
                // Generate calc expression: calc(var(--spacing) * value)
                sizing = $"calc(var(--spacing) * {key})";

                // ThemeVariableTrackingStage automatically tracks var() references
                return true;
            }

            // Try to resolve from theme namespaces
            foreach (var ns in SizingNamespaces)
            {
                var themeKey = $"{ns}-{key}";
                if (theme.ContainsKey(themeKey))
                {
                    sizing = $"var({themeKey})";

                    // ThemeVariableTrackingStage automatically tracks var() references
                    return true;
                }

                // Also try without dash for base namespace values
                if (theme.ContainsKey($"{ns}{key}"))
                {
                    sizing = $"var({ns}{key})";

                    // ThemeVariableTrackingStage automatically tracks var() references
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets special sizing values based on the key and dimension.
    /// </summary>
    protected virtual string GetSpecialSizingValue(string key)
    {
        // Common sizing values
        var commonValue = key switch
        {
            "auto" => "auto",
            "full" => "100%",
            "min" => "min-content",
            "max" => "max-content",
            "fit" => "fit-content",
            "lh" => "1lh",
            _ => string.Empty,
        };

        if (!string.IsNullOrEmpty(commonValue))
        {
            return commonValue;
        }

        // Dimension-specific viewport units
        // Note: Width utilities can use height viewport units and vice versa (e.g., w-dvh, h-dvw)
        return key switch
        {
            // Width viewport units (can be used by any dimension)
            "svw" => "100svw",
            "lvw" => "100lvw",
            "dvw" => "100dvw",

            // Height viewport units (can be used by any dimension)
            "svh" => "100svh",
            "lvh" => "100lvh",
            "dvh" => "100dvh",

            // Dimension-specific screen unit
            "screen" => Dimension switch
            {
                SizingDimension.Width => "100vw",
                SizingDimension.Height => "100vh",
                _ => string.Empty,
            },
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Tries to parse a fraction string and convert it to a calc expression.
    /// </summary>
    protected virtual bool TryParseFraction(string key, out string fractionValue)
    {
        fractionValue = string.Empty;

        if (!key.Contains('/'))
        {
            return false;
        }

        var parts = key.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var numerator))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var denominator) || denominator <= 0)
        {
            return false;
        }

        fractionValue = $"calc({numerator}/{denominator} * 100%)";
        return true;
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
    /// Checks if a value is a CSS function (var, calc, min, max, clamp, etc.).
    /// </summary>
    private static bool IsCssFunction(string value)
    {
        if (!value.Contains('('))
        {
            return false;
        }

        return value.StartsWith("var(") ||
               value.Contains("calc(") ||
               value.Contains("min(") ||
               value.Contains("max(") ||
               value.Contains("clamp(");
    }

    /// <summary>
    /// Generates CSS declarations based on the utility pattern.
    /// Must be implemented by derived classes to handle specific sizing utilities.
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

    /// <summary>
    /// Enum to specify the dimension type for viewport unit handling.
    /// </summary>
    protected enum SizingDimension
    {
        Width,
        Height,
        Both,
    }

    /// <summary>
    /// Returns examples of this sizing utility with theme-aware values.
    /// Default implementation shows examples with common sizing values.
    /// Override to provide custom sizing examples.
    /// </summary>
    public virtual IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>();

        foreach (var pattern in Patterns.Take(1))
        {
            // Special keywords (limited to most common)
            var keywords = new[] { "auto", "full", "screen" };
            foreach (var keyword in keywords)
            {
                examples.Add(new Documentation.UtilityExample(
                    $"{pattern}-{keyword}",
                    $"Set {pattern} to {GetSpecialSizingValue(keyword)}"));
            }

            // Numeric spacing values (limited to most common)
            var numericValues = new[] { "0", "4", "8" };
            foreach (var value in numericValues)
            {
                examples.Add(new Documentation.UtilityExample(
                    $"{pattern}-{value}",
                    $"Set {pattern} to {value} spacing units"));
            }

            // Fractions
            examples.Add(new Documentation.UtilityExample(
                $"{pattern}-1/2",
                $"Set {pattern} to 50%"));
            examples.Add(new Documentation.UtilityExample(
                $"{pattern}-1/3",
                $"Set {pattern} to 33.33%"));

            // Arbitrary values
            examples.Add(new Documentation.UtilityExample(
                $"{pattern}-[100px]",
                $"Set {pattern} with arbitrary value"));
        }

        return examples.Take(10);
    }
}