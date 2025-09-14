using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for filter utilities that use the CSS variable composition system.
/// Each filter utility sets its own CSS variable (e.g., --tw-blur) and includes
/// the combined filter property with placeholders for all filter types.
/// </summary>
internal abstract class BaseFilterUtility : BaseFunctionalUtility
{
    /// <summary>
    /// Gets the CSS variable name for this filter type (e.g., "blur", "brightness").
    /// </summary>
    protected abstract string FilterVariableName { get; }

    /// <summary>
    /// Gets a value indicating whether gets whether this is a backdrop filter utility.
    /// </summary>
    protected virtual bool IsBackdropFilter => false;

    /// <summary>
    /// Generates the filter CSS declarations with the filter CSS variable system.
    /// </summary>
    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var filterVariableName = IsBackdropFilter ? $"--tw-backdrop-{FilterVariableName}" : $"--tw-{FilterVariableName}";
        var filterPropertyName = IsBackdropFilter ? "backdrop-filter" : "filter";
        var filterPropertyValue = GetFilterPropertyValue();

        return ImmutableList.Create<AstNode>(
            new Declaration(filterVariableName, value, important),
            new Declaration(filterPropertyName, filterPropertyValue, important));
    }

    /// <summary>
    /// Gets the complete filter property value with placeholders for all filter types.
    /// This matches Tailwind's approach of including all possible filter functions.
    /// </summary>
    private string GetFilterPropertyValue()
    {
        if (IsBackdropFilter)
        {
            return "var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)";
        }

        return "var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)";
    }

    /// <summary>
    /// Validates filter-related arbitrary values.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // Allow numeric values with optional units
        if (IsValidFilterValue(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if the value is a valid filter function value.
    /// </summary>
    protected virtual bool IsValidFilterValue(string value)
    {
        // Basic validation for common filter values
        return !value.Contains(';') &&
               !value.Contains('{') &&
               !value.Contains('}');
    }
}