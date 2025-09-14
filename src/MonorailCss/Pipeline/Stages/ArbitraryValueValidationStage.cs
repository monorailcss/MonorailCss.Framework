using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that centralizes arbitrary value validation logic.
/// This eliminates duplicate validation code across multiple utilities.
/// </summary>
internal partial class ArbitraryValueValidationStage : IPipelineStage
{
    public string Name => "Arbitrary Value Validation";

    private static readonly HashSet<string> _cssKeywords = new(["none", "inherit", "initial", "unset", "revert", "revert-layer"], StringComparer.OrdinalIgnoreCase);

    private static readonly string[] _angleUnits = ["deg", "rad", "grad", "turn"];
    private static readonly string[] _lengthUnits = ["px", "em", "rem", "vh", "vw", "vmin", "vmax", "ch", "ex", "cm", "mm", "in", "pt", "pc", "svh", "svw", "lvh", "lvw", "dvh", "dvw"];

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Get processed classes from context
        if (!context.Metadata.TryGetValue("processedClasses", out var classesObj) ||
            classesObj is not List<ProcessedClass> processedClasses)
        {
            return nodes;
        }

        // Process each class that has arbitrary values
        var classesToProcess = processedClasses.ToList();
        for (var i = 0; i < classesToProcess.Count; i++)
        {
            var processedClass = classesToProcess[i];
            var candidate = processedClass.Candidate;

            // Check if this is a functional utility with an arbitrary value
            if (candidate is FunctionalUtility functional)
            {
                if (functional.Value?.Kind != ValueKind.Arbitrary)
                {
                    continue;
                }

                // Store validation results in context for utilities to use
                var validationKey = $"validation_{functional.Root}_{functional.Value.Value}";

                // Determine validation type based on utility root
                var validationType = DetermineValidationType(functional.Root);
                var isValid = ValidateArbitraryValue(functional.Value.Value, validationType);

                // Store validation result in context
                context.Metadata[validationKey] = isValid;

                // If invalid, we could filter out the nodes or mark them
                // For now, we'll let utilities decide what to do with invalid values
                if (!isValid)
                {
                    // Add a metadata marker for invalid arbitrary values
                    context.Metadata[$"invalid_arbitrary_{i}"] = true;
                }
            }
            else if (candidate is ArbitraryProperty arbitrary)
            {
                // For arbitrary properties, we can validate the value part
                var validationKey = $"validation_arbitrary_{arbitrary.Property}_{arbitrary.Value}";

                // Arbitrary properties are generally allowed, but we could add validation here
                var isValid = true; // Accept all arbitrary properties for now

                context.Metadata[validationKey] = isValid;
            }
        }

        return nodes;
    }

    private static ValidationType DetermineValidationType(string? root)
    {
        if (root == null)
        {
            return ValidationType.Any;
        }

        return root switch
        {
            // Angle utilities
            "rotate" or "rotate-x" or "rotate-y" or "skew-x" or "skew-y" => ValidationType.Angle,

            // Length utilities
            "w" or "h" or "min-w" or "min-h" or "max-w" or "max-h" or
            "p" or "px" or "py" or "pt" or "pr" or "pb" or "pl" or
            "m" or "mx" or "my" or "mt" or "mr" or "mb" or "ml" or
            "gap" or "gap-x" or "gap-y" or
            "space-x" or "space-y" or
            "top" or "right" or "bottom" or "left" or "inset" or "inset-x" or "inset-y" or
            "translate-x" or "translate-y" => ValidationType.LengthOrPercentage,

            // Percentage utilities
            "opacity" or "scale" or "scale-x" or "scale-y" => ValidationType.Percentage,

            // Color utilities
            "text" or "bg" or "border" or "outline" or "fill" or "stroke" or
            "from" or "via" or "to" or "accent" or "caret" => ValidationType.Color,

            // Number utilities
            "z" or "order" or "flex-grow" or "flex-shrink" or "font-weight" or
            "line-clamp" or "columns" => ValidationType.Number,

            // Shadow utilities (complex values)
            "shadow" or "drop-shadow" or "text-shadow" => ValidationType.Shadow,

            // Filter utilities
            "blur" or "brightness" or "contrast" or "grayscale" or "hue-rotate" or
            "invert" or "saturate" or "sepia" => ValidationType.Filter,

            // Default to any
            _ => ValidationType.Any,
        };
    }

    private static bool ValidateArbitraryValue(string value, ValidationType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Always allow CSS keywords, variables, and calc()
        if (IsCssKeyword(value) || IsCssFunction(value))
        {
            return true;
        }

        return type switch
        {
            ValidationType.Angle => IsValidAngle(value),
            ValidationType.Length => IsValidLength(value),
            ValidationType.Percentage => IsValidPercentage(value),
            ValidationType.LengthOrPercentage => IsValidLength(value) || IsValidPercentage(value),
            ValidationType.Color => IsValidColor(value),
            ValidationType.Number => IsValidNumber(value),
            ValidationType.Shadow => IsValidShadow(value),
            ValidationType.Filter => IsValidFilter(value),
            ValidationType.Any => true, // Accept any value for generic utilities
            _ => true,
        };
    }

    private static bool IsCssKeyword(string value)
    {
        return _cssKeywords.Contains(value.Trim());
    }

    private static bool IsCssFunction(string value)
    {
        return value.StartsWith("var(") ||
               value.Contains("calc(") ||
               value.Contains("min(") ||
               value.Contains("max(") ||
               value.Contains("clamp(");
    }

    private static bool IsValidAngle(string value)
    {
        value = value.Trim();

        // Allow 0 without unit
        if (value == "0")
        {
            return true;
        }

        // Check for angle values with units
        foreach (var unit in _angleUnits)
        {
            if (value.EndsWith(unit))
            {
                var numPart = value[..^unit.Length];
                if (IsValidNumber(numPart))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsValidLength(string value)
    {
        value = value.Trim();

        // Allow 0 without unit
        if (value == "0")
        {
            return true;
        }

        // Check for length values with units
        foreach (var unit in _lengthUnits)
        {
            if (value.EndsWith(unit))
            {
                var numPart = value[..^unit.Length];
                if (IsValidNumber(numPart))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsValidPercentage(string value)
    {
        value = value.Trim();

        // Check for percentage values
        if (value.EndsWith("%"))
        {
            var numPart = value[..^1];
            return IsValidNumber(numPart);
        }

        // Also accept decimal values for opacity-like properties
        if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var num))
        {
            return num >= 0 && num <= 1;
        }

        return false;
    }

    private static bool IsValidNumber(string value)
    {
        // Handle negative values
        if (value.StartsWith("-"))
        {
            value = value[1..];
        }

        return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsValidColor(string value)
    {
        value = value.Trim();

        // Common color keywords
        if (value is "transparent" or "currentColor" or "current")
        {
            return true;
        }

        // Hex colors
        if (HexColorRegexDefinition().IsMatch(value))
        {
            return true;
        }

        // RGB/RGBA functions
        if (RgbRegexDefinition().IsMatch(value))
        {
            return true;
        }

        // HSL/HSLA functions
        if (HslRegexDefinition().IsMatch(value))
        {
            return true;
        }

        // Modern color functions
        if (value.StartsWith("rgb(") || value.StartsWith("hsl(") ||
            value.StartsWith("hwb(") || value.StartsWith("lab(") ||
            value.StartsWith("lch(") || value.StartsWith("oklab(") ||
            value.StartsWith("oklch(") || value.StartsWith("color("))
        {
            return true;
        }

        return false;
    }

    private static bool IsValidShadow(string value)
    {
        // Shadow values are complex, accept most values
        // Could be enhanced with more specific validation
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsValidFilter(string value)
    {
        // Filter values can be complex, accept most values
        // Could be enhanced with more specific validation
        return !string.IsNullOrWhiteSpace(value);
    }

    private enum ValidationType
    {
        Any,
        Angle,
        Length,
        Percentage,
        LengthOrPercentage,
        Color,
        Number,
        Shadow,
        Filter,
    }

    [GeneratedRegex(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorRegexDefinition();
    [GeneratedRegex(@"^rgba?\(.*\)$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RgbRegexDefinition();
    [GeneratedRegex(@"^hsla?\(.*\)$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex HslRegexDefinition();
}