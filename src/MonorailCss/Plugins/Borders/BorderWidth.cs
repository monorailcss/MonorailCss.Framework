using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-width plugin.
/// </summary>
public class BorderWidth : BaseUtilityNamespacePlugin, IUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("border", "border-width"),
        new("border-x", ("border-left-width", "border-right-width")),
        new("border-y", ("border-top-width", "border-bottom-width")),
        new("border-r", "border-right-width"),
        new("border-t", "border-top-width"),
        new("border-b", "border-bottom-width"),
        new("border-l", "border-left-width"),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() => new()
        {
            { "0", "0px" },
            { "DEFAULT", "1px" },
            { "2", "2px" },
            { "4", "4px" },
            { "8", "8px" },
        };

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = string.Empty;
        calculationPattern = "{1}px";
        return true;
    }

    /// <summary>
    /// Validates if a suffix is a valid dynamic value for this plugin.
    /// Only numeric values or values with valid CSS units should be processed as dynamic values.
    /// </summary>
    /// <param name="suffix">The suffix to validate.</param>
    /// <returns>True if it's a valid dynamic value, false otherwise.</returns>
    private static bool IsValidDynamicValue(string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
        {
            return false;
        }

        // Check if it's a color suffix first - these should never be width values
        if (IsColorSuffix(suffix))
        {
            return false;
        }

        // Handle negative values
        var valueToCheck = suffix.StartsWith('-') ? suffix[1..] : suffix;
        
        // Check if it's a pure number (most common case for border widths)
        if (int.TryParse(valueToCheck, out _))
        {
            return true;
        }

        // Check if it has valid CSS units for width/size values
        var sizeUnits = new[] { "px", "em", "rem", "ex", "ch", "lh", "rlh", "vw", "vh", "vmin", "vmax", "vi", "vb", "dvw", "dvh", "lvw", "lvh", "svw", "svh", "%", "fr", "cm", "mm", "in", "pt", "pc" };
        
        foreach (var unit in sizeUnits)
        {
            if (valueToCheck.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                var numericPart = valueToCheck[..^unit.Length];
                if (double.TryParse(numericPart, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <inheritdoc />
    public new IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        var namespacePropertyMapList = GetNamespacePropertyMapList();
        var cssSuffixToValuesMap = GetValues();

        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax:
                {
                    if (!namespacePropertyMapList.ContainsNamespace(arbitraryValueSyntax.Namespace))
                    {
                        yield break;
                    }

                    if (!IsValidArbitraryValue(arbitraryValueSyntax.ArbitraryValue, cssSuffixToValuesMap))
                    {
                        yield break;
                    }

                    var arbitraryMapping = namespacePropertyMapList[arbitraryValueSyntax.Namespace];
                    var arbitraryDeclarationList = new CssDeclarationList();
                    foreach (var property in arbitraryMapping.Values.Values)
                    {
                        arbitraryDeclarationList.Add(new CssDeclaration(property, ProcessArbitraryValue(arbitraryValueSyntax.ArbitraryValue)));
                    }
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, arbitraryDeclarationList, arbitraryMapping.Importance);
                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespacePropertyMapList.ContainsNamespace(namespaceSyntax.Namespace):
                {
                    var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

                    // Check if this suffix looks like a color - if so, don't process it
                    if (IsColorSuffix(suffix))
                    {
                        yield break;
                    }

                    string value;

                    // if we don't have a matching suffix, we need to check if we support dynamic values.
                    if (!cssSuffixToValuesMap.ContainsSuffix(suffix))
                    {
                        // Only process if it's a valid dynamic value (numeric or with valid units)
                        if (SupportsDynamicValues(out var variableName, out var pattern) && IsValidDynamicValue(suffix))
                        {
                            if (suffix.EndsWith('-'))
                            {
                                suffix = '-' + suffix[..^1]; // swap the negation to before the number.
                            }

                            var fullVariableName = CssFramework.GetVariableNameWithPrefix(variableName);
                            value = string.Format(pattern, fullVariableName, suffix);
                        }
                        else
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        value = cssSuffixToValuesMap[suffix];
                    }

                    var mapping = namespacePropertyMapList[namespaceSyntax.Namespace];
                    var declarationList = new CssDeclarationList();
                    foreach (var property in mapping.Values.Values)
                    {
                        declarationList.Add(new CssDeclaration(property, value));
                    }

                    yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarationList, mapping.Importance);
                    break;
                }

            default:
                yield break;
        }
    }

    /// <summary>
    /// Processes an arbitrary value by applying transformations.
    /// </summary>
    /// <param name="arbitraryValue">The arbitrary value to process.</param>
    /// <returns>The processed arbitrary value.</returns>
    private static string ProcessArbitraryValue(string arbitraryValue)
    {
        // Convert underscores to spaces and handle calc spacing
        var processed = arbitraryValue.Replace("_", " ");
        
        if (processed.Contains("calc", StringComparison.OrdinalIgnoreCase))
        {
            return processed.Replace("+", " + ")
                    .Replace("-", " - ")
                    .Replace("*", " * ")
                    .Replace("/", " / ");
        }

        return processed;
    }


    /// <summary>
    /// Checks if a suffix looks like a color value instead of a width value.
    /// </summary>
    /// <param name="suffix">The suffix to check.</param>
    /// <returns>True if it looks like a color value, false otherwise.</returns>
    private static bool IsColorSuffix(string? suffix)
    {
        if (string.IsNullOrEmpty(suffix))
        {
            return false;
        }

        // Common color patterns that could be confused with widths
        // Pattern: colorname-number (e.g., "red-500", "blue-300")
        var colorKeywords = new[] { "red", "green", "blue", "white", "black", "gray", "grey", "yellow", "orange", "purple", "pink", "brown", "cyan", "magenta", "lime", "navy", "teal", "silver", "maroon", "olive", "aqua", "fuchsia", "slate", "zinc", "neutral", "stone", "amber", "emerald", "indigo", "violet", "rose", "sky" };
        
        foreach (var colorKeyword in colorKeywords)
        {
            if (suffix.StartsWith(colorKeyword + "-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (suffix.Equals(colorKeyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    protected override bool IsValidArbitraryValue(string arbitraryValue, CssSuffixToValueMap cssSuffixToValuesMap)
    {
        // Don't process color keywords as width values
        var colorKeywords = new[] { "transparent", "inherit", "current", "red", "green", "blue", "white", "black", "gray", "grey", "yellow", "orange", "purple", "pink", "brown", "cyan", "magenta", "lime", "navy", "teal", "silver", "maroon", "olive", "aqua", "fuchsia" };
        if (colorKeywords.Any(keyword => arbitraryValue.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Use the base implementation for other validation
        return base.IsValidArbitraryValue(arbitraryValue, cssSuffixToValuesMap);
    }
}