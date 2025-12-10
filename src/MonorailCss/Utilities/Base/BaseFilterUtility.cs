using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;

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
    /// Registers all filter-related CSS variables with the property registry.
    /// Call this from derived classes' TryCompile method when needed.
    /// </summary>
    protected void RegisterFilterVariables(CssPropertyRegistry propertyRegistry)
    {
        if (IsBackdropFilter)
        {
            // Register backdrop filter variables
            propertyRegistry.Register("--tw-backdrop-blur", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-brightness", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-contrast", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-grayscale", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-hue-rotate", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-invert", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-opacity", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-saturate", "*", false, null);
            propertyRegistry.Register("--tw-backdrop-sepia", "*", false, null);
        }
        else
        {
            // Register regular filter variables
            propertyRegistry.Register("--tw-blur", "*", false, null);
            propertyRegistry.Register("--tw-brightness", "*", false, null);
            propertyRegistry.Register("--tw-contrast", "*", false, null);
            propertyRegistry.Register("--tw-grayscale", "*", false, null);
            propertyRegistry.Register("--tw-hue-rotate", "*", false, null);
            propertyRegistry.Register("--tw-invert", "*", false, null);
            propertyRegistry.Register("--tw-saturate", "*", false, null);
            propertyRegistry.Register("--tw-sepia", "*", false, null);
            propertyRegistry.Register("--tw-drop-shadow", "*", false, null);
        }
    }

    /// <summary>
    /// Generates the filter CSS declarations with the filter CSS variable system.
    /// </summary>
    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var filterVariableName = IsBackdropFilter ? $"--tw-backdrop-{FilterVariableName}" : $"--tw-{FilterVariableName}";
        var filterPropertyValue = GetFilterPropertyValue();

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        declarations.Add(new Declaration(filterVariableName, value, important));

        if (IsBackdropFilter)
        {
            // Add webkit prefix for backdrop-filter
            declarations.Add(new Declaration("-webkit-backdrop-filter", filterPropertyValue, important));
            declarations.Add(new Declaration("backdrop-filter", filterPropertyValue, important));
        }
        else
        {
            declarations.Add(new Declaration("filter", filterPropertyValue, important));
        }

        return declarations.ToImmutable();
    }

    /// <summary>
    /// Gets the complete filter property value with placeholders for all filter types.
    /// This matches Tailwind's approach of including all possible filter functions.
    /// </summary>
    private string GetFilterPropertyValue()
    {
        if (IsBackdropFilter)
        {
            return "var(--tw-backdrop-blur,) var(--tw-backdrop-brightness,) var(--tw-backdrop-contrast,) var(--tw-backdrop-grayscale,) var(--tw-backdrop-hue-rotate,) var(--tw-backdrop-invert,) var(--tw-backdrop-opacity,) var(--tw-backdrop-saturate,) var(--tw-backdrop-sepia,)";
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

    /// <summary>
    /// Returns sample CSS output for documentation of arbitrary filter values.
    /// </summary>
    protected override string GetSampleCssForArbitraryValue(string pattern)
    {
        var varName = IsBackdropFilter ? $"--tw-backdrop-{FilterVariableName}" : $"--tw-{FilterVariableName}";
        return $"{varName}: [value]";
    }
}