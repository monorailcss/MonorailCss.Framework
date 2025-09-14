using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.DataTypes;

namespace MonorailCss.Utilities.Resolvers;

internal static partial class ValueResolver
{
    public static bool TryResolveColor(
        CandidateValue value,
        Theme.Theme theme,
        string[]? namespaces,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        if (value.Value == "transparent")
        {
            result = "transparent";
            return true;
        }

        if (value.Value == "current")
        {
            result = "currentColor";
            return true;
        }

        if (value.Value == "inherit")
        {
            result = "inherit";
            return true;
        }

        if (value.Kind == ValueKind.Arbitrary)
        {
            result = value.Value;
            return true;
        }

        var resolved = theme.Resolve(value.Value, namespaces ?? []);
        if (resolved == null)
        {
            return false;
        }

        result = resolved;
        return true;
    }

    public static bool TryResolveSpacing(
        CandidateValue value,
        Theme.Theme theme,
        string[]? namespaces,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        var resolved = theme.Resolve(value.Value, namespaces ?? []);
        if (resolved != null)
        {
            result = resolved;
            return true;
        }

        if (IsValidMultiplier(value.Value))
        {
            var baseSpacing = theme.Resolve(null, NamespaceResolver.BuildChain(NamespaceResolver.Spacing));
            if (baseSpacing != null)
            {
                result = $"calc({baseSpacing} * {value.Value})";
                return true;
            }
        }

        if (value.Kind == ValueKind.Arbitrary)
        {
            result = value.Value;
            return true;
        }

        return false;
    }

    public static bool TryResolveSize(
        CandidateValue value,
        Theme.Theme theme,
        string[]? namespaces,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Check the Fraction property first if it's set
        var fractionValue = value.Fraction ?? value.Value;

        if (fractionValue.Contains('/'))
        {
            var parts = fractionValue.Split('/');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var numerator) &&
                double.TryParse(parts[1], out var denominator) &&
                denominator != 0)
            {
                var percentage = (numerator / denominator) * 100;

                // Format to match Tailwind's output
                result = Math.Abs(percentage - Math.Floor(percentage)) < 0.01
                    ? $"{percentage:0}%"
                    : $"{percentage}%";
                return true;
            }
        }

        switch (value.Value)
        {
            case "full":
                result = "100%";
                return true;
            case "screen":
                result = "100vh";
                return true;
            case "auto":
                result = "auto";
                return true;
            case "min":
                result = "min-content";
                return true;
            case "max":
                result = "max-content";
                return true;
            case "fit":
                result = "fit-content";
                return true;
        }

        var resolved = theme.Resolve(value.Value, namespaces ?? []);
        if (resolved != null)
        {
            result = resolved;
            return true;
        }

        if (value.Kind == ValueKind.Arbitrary)
        {
            result = value.Value;
            return true;
        }

        return false;
    }

    public static bool TryResolveBorderWidth(
        CandidateValue value,
        Theme.Theme theme,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        if (value.Kind == ValueKind.Arbitrary)
        {
            result = value.Value;
            return true;
        }

        // Generate inline values for common widths
        result = value.Value switch
        {
            "0" => "0px",
            "1" => "1px",
            "2" => "2px",
            "4" => "4px",
            "8" => "8px",
            _ => null,
        };

        return result != null;
    }

    public static bool TryResolveBorderRadius(
        CandidateValue value,
        Theme.Theme theme,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        switch (value.Value)
        {
            case "none":
                result = "0";
                return true;
            case "full":
                result = "calc(infinity * 1px)";
                return true;
        }

        var resolved = theme.Resolve(value.Value, NamespaceResolver.BorderRadiusChain);
        if (resolved != null)
        {
            result = resolved;
            return true;
        }

        if (value.Kind == ValueKind.Arbitrary)
        {
            result = value.Value;
            return true;
        }

        return false;
    }

    private static bool IsValidMultiplier(string? value)
    {
        return !string.IsNullOrEmpty(value) && MultiplierPattern().IsMatch(value);
    }

    /// <summary>
    /// Resolves an opacity value from a modifier to a CSS percentage.
    /// </summary>
    /// <param name="modifier">The modifier containing the opacity value.</param>
    /// <param name="theme">The theme for resolving values.</param>
    /// <param name="result">The resulting CSS opacity percentage.</param>
    /// <returns>True if the opacity was successfully resolved.</returns>
    public static bool TryResolveOpacity(
        Modifier modifier,
        Theme.Theme theme,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        var value = modifier.Value;

        // Handle named opacity values (e.g., 50, 75, 25)
        if (modifier.Kind == ModifierKind.Named)
        {
            // Check if it's a theme opacity value
            var resolved = theme.Resolve(value, ["opacity"]);
            if (resolved != null)
            {
                // Theme returns var(--opacity-50), extract the value
                var rawValue = theme.ResolveValue(value, ["opacity"]);
                if (rawValue != null)
                {
                    result = ConvertToPercentage(rawValue);
                    return result != null;
                }
            }

            // Try to parse as a number (0-100)
            if (int.TryParse(value, out var intValue) && intValue >= 0 && intValue <= 100)
            {
                result = $"{intValue}%";
                return true;
            }
        }

        // Handle arbitrary opacity values (e.g., [0.5], [0.75])
        else if (modifier.Kind == ModifierKind.Arbitrary)
        {
            // Try to parse as decimal (0.0 - 1.0)
            if (decimal.TryParse(value, out var decimalValue) && decimalValue >= 0 && decimalValue <= 1)
            {
                var percentage = (int)(decimalValue * 100);
                result = $"{percentage}%";
                return true;
            }

            // Try to parse as percentage directly ([50%])
            if (value.EndsWith('%'))
            {
                var percentStr = value[..^1];
                if (int.TryParse(percentStr, out var percentValue) && percentValue >= 0 && percentValue <= 100)
                {
                    result = value;
                    return true;
                }
            }
        }

        return false;
    }

    private static string? ConvertToPercentage(string value)
    {
        // If it's already a percentage, return it
        if (value.EndsWith('%'))
        {
            return value;
        }

        // Try to parse as decimal (0.0 - 1.0)
        if (decimal.TryParse(value, out var decimalValue) && decimalValue >= 0 && decimalValue <= 1)
        {
            var percentage = (int)(decimalValue * 100);
            return $"{percentage}%";
        }

        // Try to parse as integer (0 - 100)
        if (int.TryParse(value, out var intValue) && intValue >= 0 && intValue <= 100)
        {
            return $"{intValue}%";
        }

        return null;
    }

    /// <summary>
    /// Infers the data type of an arbitrary value and resolves it appropriately.
    /// </summary>
    /// <param name="value">The candidate value to resolve.</param>
    /// <param name="theme">The theme for resolving values.</param>
    /// <param name="allowedTypes">The allowed data types to check.</param>
    /// <param name="namespaces">Optional namespaces for theme resolution.</param>
    /// <param name="result">The resolved CSS value.</param>
    /// <param name="inferredType">The inferred data type.</param>
    /// <returns>True if the value was successfully resolved.</returns>
    public static bool TryInferAndResolve(
        CandidateValue value,
        Theme.Theme theme,
        DataType[] allowedTypes,
        string[]? namespaces,
        [NotNullWhen(true)] out string? result,
        out DataType? inferredType)
    {
        result = null;
        inferredType = null;

        // For arbitrary values, infer the data type
        if (value.Kind == ValueKind.Arbitrary)
        {
            inferredType = DataTypeInference.InferDataType(value.Value, allowedTypes);

            if (inferredType == null)
            {
                // If we can't infer, try each type in order
                foreach (var type in allowedTypes)
                {
                    if (TryResolveAsType(value, theme, type, namespaces, out result))
                    {
                        inferredType = type;
                        return true;
                    }
                }

                return false;
            }

            return TryResolveAsType(value, theme, inferredType.Value, namespaces, out result);
        }

        // For named values, try to resolve through normal channels
        foreach (var type in allowedTypes)
        {
            if (TryResolveAsType(value, theme, type, namespaces, out result))
            {
                inferredType = type;
                return true;
            }
        }

        return false;
    }

    private static bool TryResolveAsType(
        CandidateValue value,
        Theme.Theme theme,
        DataType type,
        string[]? namespaces,
        [NotNullWhen(true)] out string? result)
    {
        result = null;

        switch (type)
        {
            case DataType.Color:
                return TryResolveColor(value, theme, namespaces, out result);

            case DataType.Length:
            case DataType.LineWidth:
                // For arbitrary values, just pass through if it's a valid length/line-width
                if (value.Kind == ValueKind.Arbitrary)
                {
                    result = value.Value;
                    return true;
                }

                // For named values, try border width resolution
                return TryResolveBorderWidth(value, theme, out result);

            case DataType.Percentage:
            case DataType.Number:
            case DataType.Integer:
                if (value.Kind == ValueKind.Arbitrary)
                {
                    result = value.Value;
                    return true;
                }

                return false;

            default:
                return false;
        }
    }

    [GeneratedRegex(@"^\d+(\.\d+)?$")]
    private static partial Regex MultiplierPattern();
}