using System.Text.RegularExpressions;

namespace MonorailCss.Utilities.Resolvers;

/// <summary>
/// Provides value type inference for CSS values to determine appropriate properties.
/// </summary>
internal static partial class ValueTypeInference
{
    public enum ValueType
    {
        Color,
        Length,
        Percentage,
        Angle,
        Image,
        Position,
        Size,
        Url,
        Number,
        Keyword,
        Unknown,
    }

    public static ValueType InferType(string value, ValueType[]? candidates = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValueType.Unknown;
        }

        // If candidates are provided, check them in order
        if (candidates != null)
        {
            foreach (var candidate in candidates)
            {
                if (IsType(value, candidate))
                {
                    return candidate;
                }
            }

            return ValueType.Unknown;
        }

        // Otherwise, check all types
        if (IsUrl(value))
        {
            return ValueType.Url;
        }

        if (IsImage(value))
        {
            return ValueType.Image;
        }

        if (IsColor(value))
        {
            return ValueType.Color;
        }

        if (IsPercentage(value))
        {
            return ValueType.Percentage;
        }

        if (IsAngle(value))
        {
            return ValueType.Angle;
        }

        if (IsLength(value))
        {
            return ValueType.Length;
        }

        if (IsPosition(value))
        {
            return ValueType.Position;
        }

        if (IsSize(value))
        {
            return ValueType.Size;
        }

        if (IsNumber(value))
        {
            return ValueType.Number;
        }

        if (IsKeyword(value))
        {
            return ValueType.Keyword;
        }

        return ValueType.Unknown;
    }

    private static bool IsType(string value, ValueType type)
    {
        return type switch
        {
            ValueType.Color => IsColor(value),
            ValueType.Length => IsLength(value),
            ValueType.Percentage => IsPercentage(value),
            ValueType.Angle => IsAngle(value),
            ValueType.Image => IsImage(value),
            ValueType.Position => IsPosition(value),
            ValueType.Size => IsSize(value),
            ValueType.Url => IsUrl(value),
            ValueType.Number => IsNumber(value),
            ValueType.Keyword => IsKeyword(value),
            _ => false,
        };
    }

    private static bool IsColor(string value)
    {
        // Hex colors
        if (HexColorPattern().IsMatch(value))
        {
            return true;
        }

        // RGB/RGBA
        if (value.StartsWith("rgb(") || value.StartsWith("rgba("))
        {
            return true;
        }

        // HSL/HSLA
        if (value.StartsWith("hsl(") || value.StartsWith("hsla("))
        {
            return true;
        }

        // LAB/LCH/OKLAB/OKLCH
        if (value.StartsWith("lab(") || value.StartsWith("lch(") ||
            value.StartsWith("oklab(") || value.StartsWith("oklch("))
        {
            return true;
        }

        // Color functions
        if (value.StartsWith("color(") || value.StartsWith("color-mix("))
        {
            return true;
        }

        // Named colors and special values
        if (value is "transparent" or "currentColor" or "inherit")
        {
            return true;
        }

        // CSS variables that might be colors
        if (value.StartsWith("var(--") && value.Contains("color"))
        {
            return true;
        }

        return false;
    }

    private static bool IsLength(string value)
    {
        return LengthPattern().IsMatch(value) ||
               value == "0" ||
               value.StartsWith("calc(") ||
               value.StartsWith("var(--");
    }

    private static bool IsPercentage(string value)
    {
        return PercentagePattern().IsMatch(value);
    }

    private static bool IsAngle(string value)
    {
        return AnglePattern().IsMatch(value);
    }

    private static bool IsImage(string value)
    {
        return value.StartsWith("linear-gradient(") ||
               value.StartsWith("radial-gradient(") ||
               value.StartsWith("conic-gradient(") ||
               value.StartsWith("repeating-linear-gradient(") ||
               value.StartsWith("repeating-radial-gradient(") ||
               value.StartsWith("repeating-conic-gradient(") ||
               value.StartsWith("image(") ||
               value.StartsWith("image-set(") ||
               value.StartsWith("cross-fade(") ||
               value.StartsWith("url(");
    }

    private static bool IsPosition(string value)
    {
        var positionKeywords = new[] { "top", "right", "bottom", "left", "center" };

        // Single keyword
        if (Array.Exists(positionKeywords, k => k == value))
        {
            return true;
        }

        // Multiple keywords
        var parts = value.Split(' ');
        if (parts.Length == 2 &&
            Array.Exists(positionKeywords, k => k == parts[0]) &&
            Array.Exists(positionKeywords, k => k == parts[1]))
        {
            return true;
        }

        // Percentage or length values
        return IsPercentage(value) || IsLength(value);
    }

    private static bool IsSize(string value)
    {
        var sizeKeywords = new[] { "auto", "contain", "cover", "min-content", "max-content", "fit-content" };

        if (Array.Exists(sizeKeywords, k => k == value))
        {
            return true;
        }

        // Two-value syntax (width height)
        var parts = value.Split(' ');
        if (parts.Length == 2)
        {
            return (IsLength(parts[0]) || IsPercentage(parts[0]) || parts[0] == "auto") &&
                   (IsLength(parts[1]) || IsPercentage(parts[1]) || parts[1] == "auto");
        }

        return IsLength(value) || IsPercentage(value);
    }

    private static bool IsUrl(string value)
    {
        return value.StartsWith("url(") && value.EndsWith(")");
    }

    private static bool IsNumber(string value)
    {
        return NumberPattern().IsMatch(value);
    }

    private static bool IsKeyword(string value)
    {
        // Simple check for CSS keywords (alphabetic with optional hyphens)
        return KeywordPattern().IsMatch(value);
    }

    // Regex patterns
    [GeneratedRegex(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorPattern();

    [GeneratedRegex(@"^-?\d*\.?\d+(px|em|rem|vh|vw|vmin|vmax|ex|ch|cm|mm|in|pt|pc|svh|svw|lvh|lvw|dvh|dvw)$")]
    private static partial Regex LengthPattern();

    [GeneratedRegex(@"^-?\d*\.?\d+%$")]
    private static partial Regex PercentagePattern();

    [GeneratedRegex(@"^-?\d*\.?\d+(deg|rad|grad|turn)$")]
    private static partial Regex AnglePattern();

    [GeneratedRegex(@"^-?\d*\.?\d+$")]
    private static partial Regex NumberPattern();

    [GeneratedRegex(@"^[a-z][a-z-]*$", RegexOptions.IgnoreCase)]
    private static partial Regex KeywordPattern();
}