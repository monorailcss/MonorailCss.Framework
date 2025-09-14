using System.Text.RegularExpressions;

namespace MonorailCss.DataTypes;

/// <summary>
/// Provides CSS data type inference and validation based on Tailwind CSS implementation.
/// </summary>
internal static partial class DataTypeInference
{
    // Named colors from CSS specifications
    private static readonly HashSet<string> _namedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        // CSS Level 1
        "black", "silver", "gray", "white", "maroon", "red", "purple", "fuchsia",
        "green", "lime", "olive", "yellow", "navy", "blue", "teal", "aqua",

        // CSS Level 2/3
        "aliceblue", "antiquewhite", "aquamarine", "azure", "beige", "bisque",
        "blanchedalmond", "blueviolet", "brown", "burlywood", "cadetblue", "chartreuse",
        "chocolate", "coral", "cornflowerblue", "cornsilk", "crimson", "cyan",
        "darkblue", "darkcyan", "darkgoldenrod", "darkgray", "darkgreen", "darkgrey",
        "darkkhaki", "darkmagenta", "darkolivegreen", "darkorange", "darkorchid",
        "darkred", "darksalmon", "darkseagreen", "darkslateblue", "darkslategray",
        "darkslategrey", "darkturquoise", "darkviolet", "deeppink", "deepskyblue",
        "dimgray", "dimgrey", "dodgerblue", "firebrick", "floralwhite", "forestgreen",
        "gainsboro", "ghostwhite", "gold", "goldenrod", "greenyellow", "grey",
        "honeydew", "hotpink", "indianred", "indigo", "ivory", "khaki",
        "lavender", "lavenderblush", "lawngreen", "lemonchiffon", "lightblue",
        "lightcoral", "lightcyan", "lightgoldenrodyellow", "lightgray", "lightgreen",
        "lightgrey", "lightpink", "lightsalmon", "lightseagreen", "lightskyblue",
        "lightslategray", "lightslategrey", "lightsteelblue", "lightyellow",
        "limegreen", "linen", "magenta", "mediumaquamarine", "mediumblue",
        "mediumorchid", "mediumpurple", "mediumseagreen", "mediumslateblue",
        "mediumspringgreen", "mediumturquoise", "mediumvioletred", "midnightblue",
        "mintcream", "mistyrose", "moccasin", "navajowhite", "oldlace",
        "olivedrab", "orange", "orangered", "orchid", "palegoldenrod",
        "palegreen", "paleturquoise", "palevioletred", "papayawhip", "peachpuff",
        "peru", "pink", "plum", "powderblue", "rebeccapurple", "rosybrown",
        "royalblue", "saddlebrown", "salmon", "sandybrown", "seagreen",
        "seashell", "sienna", "skyblue", "slateblue", "slategray",
        "slategrey", "snow", "springgreen", "steelblue", "tan",
        "thistle", "tomato", "turquoise", "violet", "wheat",
        "whitesmoke", "yellowgreen",

        // Keywords
        "transparent", "currentcolor",

        // System colors
        "canvas", "canvastext", "linktext", "visitedtext", "activetext",
        "buttonface", "buttontext", "buttonborder", "field", "fieldtext",
        "highlight", "highlighttext", "selecteditem", "selecteditemtext",
        "mark", "marktext", "graytext", "accentcolor", "accentcolortext",
    };

    // Math functions supported in CSS
    private static readonly HashSet<string> _mathFunctions = new()
    {
        "calc", "min", "max", "clamp", "mod", "rem", "sin", "cos", "tan",
        "asin", "acos", "atan", "atan2", "pow", "sqrt", "hypot", "log", "exp", "round",
    };

    /// <summary>
    /// Infers the data type of a CSS value.
    /// </summary>
    public static DataType? InferDataType(string value, DataType[] allowedTypes)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Bail early for CSS variables
        if (value.StartsWith("var("))
        {
            return null;
        }

        // Check each allowed type in order
        foreach (var type in allowedTypes)
        {
            if (IsTypeMatch(type, value))
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a value matches a specific data type.
    /// </summary>
    private static bool IsTypeMatch(DataType type, string value)
    {
        return type switch
        {
            DataType.Color => IsColor(value),
            DataType.Length => IsLength(value),
            DataType.Percentage => IsPercentage(value),
            DataType.Ratio => IsFraction(value),
            DataType.Number => IsNumber(value),
            DataType.Integer => IsPositiveInteger(value),
            DataType.Url => IsUrl(value),
            DataType.Position => IsBackgroundPosition(value),
            DataType.BgSize => IsBackgroundSize(value),
            DataType.LineWidth => IsLineWidth(value),
            DataType.Image => IsImage(value),
            DataType.FamilyName => IsFamilyName(value),
            DataType.GenericName => IsGenericName(value),
            DataType.AbsoluteSize => IsAbsoluteSize(value),
            DataType.RelativeSize => IsRelativeSize(value),
            DataType.Angle => IsAngle(value),
            DataType.Vector => IsVector(value),
            DataType.Time => IsTime(value),
            _ => false,
        };
    }

    /// <summary>
    /// Validates if a value is a valid CSS color.
    /// </summary>
    public static bool IsColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Hex colors (#123, #123456, #12345678)
        if (value.StartsWith('#'))
        {
            return IsHexColorPattern().IsMatch(value);
        }

        // Named colors
        if (_namedColors.Contains(value))
        {
            return true;
        }

        // Color functions
        if (IsColorFunctionPattern().IsMatch(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if a value is a valid CSS length.
    /// </summary>
    public static bool IsLength(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Zero is always a valid length
        if (value == "0")
        {
            return true;
        }

        // Check for math functions
        if (HasMathFunction(value))
        {
            return true;
        }

        // Check for length pattern
        return IsLengthPattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS percentage.
    /// </summary>
    public static bool IsPercentage(string value)
    {
        if (HasMathFunction(value))
        {
            return true;
        }

        return IsPercentagePattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS fraction/ratio.
    /// </summary>
    public static bool IsFraction(string value)
    {
        if (HasMathFunction(value))
        {
            return true;
        }

        return IsFractionPattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS number.
    /// </summary>
    public static bool IsNumber(string value)
    {
        if (HasMathFunction(value))
        {
            return true;
        }

        return IsNumberPattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid positive integer.
    /// </summary>
    public static bool IsPositiveInteger(string value)
    {
        return IsPositiveIntegerPattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS URL.
    /// </summary>
    public static bool IsUrl(string value)
    {
        return IsUrlPattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS angle.
    /// </summary>
    public static bool IsAngle(string value)
    {
        if (HasMathFunction(value))
        {
            return true;
        }

        return IsAnglePattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS vector (3 space-separated numbers).
    /// </summary>
    public static bool IsVector(string value)
    {
        return IsVectorPattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS time value.
    /// </summary>
    public static bool IsTime(string value)
    {
        if (HasMathFunction(value))
        {
            return true;
        }

        return IsTimePattern().IsMatch(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS image.
    /// </summary>
    public static bool IsImage(string value)
    {
        if (IsUrl(value))
        {
            return true;
        }

        if (IsGradientFunctionPattern().IsMatch(value))
        {
            return true;
        }

        if (IsImageFunctionPattern().IsMatch(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if a value is a valid CSS background position.
    /// </summary>
    public static bool IsBackgroundPosition(string value)
    {
        var positionKeywords = new[] { "top", "right", "bottom", "left", "center" };

        // Single keyword
        if (Array.Exists(positionKeywords, k => k == value))
        {
            return true;
        }

        // Multiple keywords
        var parts = value.Split(' ');
        if (parts.Length == 2)
        {
            var allValid = parts.All(part =>
                Array.Exists(positionKeywords, k => k == part) ||
                IsLength(part) ||
                IsPercentage(part));
            return allValid;
        }

        // Single length or percentage
        return IsLength(value) || IsPercentage(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS background size.
    /// </summary>
    public static bool IsBackgroundSize(string value)
    {
        if (value is "cover" or "contain")
        {
            return true;
        }

        var parts = value.Split(' ');
        if (parts.Length is 1 or 2)
        {
            return parts.All(part =>
                part == "auto" || IsLength(part) || IsPercentage(part));
        }

        return false;
    }

    /// <summary>
    /// Validates if a value is a valid CSS line width.
    /// </summary>
    public static bool IsLineWidth(string value)
    {
        var lineWidthKeywords = new[] { "thin", "medium", "thick" };

        if (Array.Exists(lineWidthKeywords, k => k == value))
        {
            return true;
        }

        return IsLength(value) || IsNumber(value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS font family name.
    /// </summary>
    public static bool IsFamilyName(string value)
    {
        // If it starts with a digit, it's not a family name
        if (value.Length > 0 && char.IsDigit(value[0]))
        {
            return false;
        }

        // Basic validation - could be enhanced
        return true;
    }

    /// <summary>
    /// Validates if a value is a valid CSS generic font name.
    /// </summary>
    public static bool IsGenericName(string value)
    {
        var genericNames = new[]
        {
            "serif", "sans-serif", "monospace", "cursive", "fantasy",
            "system-ui", "ui-serif", "ui-sans-serif", "ui-monospace",
            "ui-rounded", "math", "emoji", "fangsong",
        };

        return Array.Exists(genericNames, n => n == value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS absolute size.
    /// </summary>
    public static bool IsAbsoluteSize(string value)
    {
        var absoluteSizes = new[]
        {
            "xx-small", "x-small", "small", "medium",
            "large", "x-large", "xx-large", "xxx-large",
        };

        return Array.Exists(absoluteSizes, s => s == value);
    }

    /// <summary>
    /// Validates if a value is a valid CSS relative size.
    /// </summary>
    public static bool IsRelativeSize(string value)
    {
        return value is "larger" or "smaller";
    }

    /// <summary>
    /// Checks if a value contains CSS math functions.
    /// </summary>
    public static bool HasMathFunction(string value)
    {
        if (!value.Contains('('))
        {
            return false;
        }

        return _mathFunctions.Any(fn =>
            value.Contains($"{fn}(", StringComparison.OrdinalIgnoreCase));
    }

    // Regex patterns - using GeneratedRegex for performance
    [GeneratedRegex(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex IsHexColorPattern();

    [GeneratedRegex(@"^url\(.*\)$")]
    private static partial Regex IsUrlPattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?$")]
    private static partial Regex IsNumberPattern();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex IsPositiveIntegerPattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?%$")]
    private static partial Regex IsPercentagePattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?\s*/\s*[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?$")]
    private static partial Regex IsFractionPattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?(cm|mm|Q|in|pc|pt|px|em|ex|ch|rem|lh|rlh|vw|vh|vmin|vmax|vb|vi|svw|svh|lvw|lvh|dvw|dvh|cqw|cqh|cqi|cqb|cqmin|cqmax)$")]
    private static partial Regex IsLengthPattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?(deg|rad|grad|turn)$")]
    private static partial Regex IsAnglePattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?\s+[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?\s+[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?$")]
    private static partial Regex IsVectorPattern();

    [GeneratedRegex(@"^(?:element|image|cross-fade|image-set)\(")]
    private static partial Regex IsImageFunctionPattern();

    [GeneratedRegex(@"^(repeating-)?(conic|linear|radial)-gradient\(")]
    private static partial Regex IsGradientFunctionPattern();

    [GeneratedRegex(@"^(rgba?|hsla?|hwb|color|(ok)?(lab|lch)|light-dark|color-mix)\(", RegexOptions.IgnoreCase)]
    private static partial Regex IsColorFunctionPattern();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(?:[eE][+-]?\d+)?(ms|s)$")]
    private static partial Regex IsTimePattern();
}