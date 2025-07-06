using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins;

/// <summary>
/// Helper class to handle common utility namespace plugins.
/// </summary>
public abstract class BaseUtilityNamespacePlugin : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => _namespaces.Value;

    private readonly Lazy<ImmutableArray<string>> _namespaces;
    private readonly Lazy<CssNamespaceToPropertyMap> _namespacePropertyMapList;
    private readonly Lazy<CssSuffixToValueMap> _suffixToValueMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUtilityNamespacePlugin"/> class.
    /// </summary>
    protected BaseUtilityNamespacePlugin()
    {
        _namespaces =
            new Lazy<ImmutableArray<string>>(() => [..GetNamespacePropertyMapList().Namespaces]);
        _suffixToValueMap = new Lazy<CssSuffixToValueMap>(GetValues);
        _namespacePropertyMapList = new Lazy<CssNamespaceToPropertyMap>(GetNamespacePropertyMapList);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        var namespacePropertyMapList = _namespacePropertyMapList.Value;
        var cssSuffixToValuesMap = _suffixToValueMap.Value;

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
                        yield break; // we don't have a valid value for this arbitrary value.
                    }

                    var arbitraryMapping = namespacePropertyMapList[arbitraryValueSyntax.Namespace];
                    var arbitraryDeclarationList = CssDeclarationList(
                        ProcessArbitraryValue(arbitraryValueSyntax.ArbitraryValue),
                        arbitraryMapping.Values.Values.ToArray());
                    yield return new CssRuleSet(GetSelector(arbitraryValueSyntax), arbitraryDeclarationList,
                        arbitraryMapping.Importance);

                    break;
                }

            // this utility only works for namespace syntax whose namespaces are defined here..
            case NamespaceSyntax namespaceSyntax
                when namespacePropertyMapList.ContainsNamespace(namespaceSyntax.Namespace):
                {
                    foreach (var cssRuleSet in GetNamespaceCssRuleSets(namespaceSyntax, cssSuffixToValuesMap,
                                 namespacePropertyMapList))
                    {
                        yield return cssRuleSet;
                    }

                    break;
                }

            default:
                yield break;
        }
    }

    private static string FixCalcSpacing(string s)
    {
        if (s.Contains("calc", StringComparison.OrdinalIgnoreCase))
        {
            return s.Replace("+", " + ")
                    .Replace("-", " - ")
                    .Replace("*", " * ")
                    .Replace("/", " / ");
        }

        return s;
    }

    /// <summary>
    /// Processes an arbitrary value by applying transformations like calc spacing and underscore-to-space conversion.
    /// </summary>
    /// <param name="arbitraryValue">The arbitrary value to process.</param>
    /// <returns>The processed arbitrary value.</returns>
    protected virtual string ProcessArbitraryValue(string arbitraryValue)
    {
        // First apply calc spacing if needed
        var processed = FixCalcSpacing(arbitraryValue);

        // Convert underscores to spaces for grid templates and other space-separated values
        // This is the standard Tailwind behavior for arbitrary values
        processed = processed.Replace("_", " ");

        return processed;
    }

    private IEnumerable<CssRuleSet> GetNamespaceCssRuleSets(
        NamespaceSyntax namespaceSyntax,
        CssSuffixToValueMap cssSuffixToValuesMap,
        CssNamespaceToPropertyMap namespacePropertyMapList)
    {
        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

        string value;

        // if we don't have a matching suffix, we need to check if we support dynamic values.
        if (!cssSuffixToValuesMap.ContainsSuffix(suffix))
        {
            if (SupportsDynamicValues(out var variableName, out var pattern))
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
        var declarationList = CssDeclarationList(value, mapping.Values.Values);
        declarationList += AdditionalDeclarations();

        yield return new CssRuleSet(GetSelector(namespaceSyntax), declarationList, mapping.Importance);
    }

    private static CssDeclarationList CssDeclarationList(string value, string[] propsValues)
    {
        var declarationList = new CssDeclarationList();
        foreach (var property in propsValues)
        {
            declarationList.Add(new CssDeclaration(property, value));
        }

        return declarationList;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var (ns, properties, _) in _namespacePropertyMapList.Value.ToArray())
        {
            foreach (var (suffix, value) in _suffixToValueMap.Value.ToArray())
            {
                yield return new CssRuleSet($"{ns}-{suffix}", CssDeclarationList(value, properties.Values) + AdditionalDeclarations());
            }
        }
    }

    /// <summary>
    /// Gets the selector given a syntax.
    /// </summary>
    /// <param name="namespaceSyntax">The parsed namespace syntax.</param>
    /// <returns>A string with the current selector name.</returns>
    protected virtual string GetSelector(IParsedClassNameSyntax namespaceSyntax)
    {
        return namespaceSyntax.OriginalSyntax;
    }

    /// <summary>
    /// Gets additional declarations that should be included with the generated rules.
    /// </summary>
    /// <returns>A list of declarations or null if none.</returns>
    protected virtual CssDeclarationList? AdditionalDeclarations()
    {
        return null;
    }

    /// <summary>
    /// Gets the property mapping lists.
    /// </summary>
    /// <returns>The mapped namespaces to properties.</returns>
    protected abstract CssNamespaceToPropertyMap GetNamespacePropertyMapList();

    /// <summary>
    /// Gets the values to map to the namespace property map.
    /// </summary>
    /// <returns>The mapped values.</returns>
    protected abstract CssSuffixToValueMap GetValues();

    /// <summary>
    /// Indicates whether this plugin supports dynamic values (e.g., CSS variables).
    /// </summary>
    /// <param name="cssVariableName">The variable name to use.</param>
    /// <param name="calculationPattern">
    /// The CSS variable name to use as a dynamic value, e.g. `calc(var({0}) * {1})`, where `{0}` is the CSS variable and
    /// `{1}` is the dynamic value placeholder pulled from the suffix of the namespaced CSS type.
    /// </param>
    /// <returns>True if supported, false if not.</returns>
    protected virtual bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = string.Empty;
        calculationPattern = string.Empty;
        return false;
    }

    /// <summary>
    /// Validates whether an arbitrary value is valid for this plugin.
    /// </summary>
    /// <param name="arbitraryValue">The arbitrary value to validate.</param>
    /// <param name="cssSuffixToValuesMap">The suffix to values map for predefined values.</param>
    /// <returns>True if the arbitrary value is valid, false otherwise.</returns>
    protected virtual bool IsValidArbitraryValue(string arbitraryValue, CssSuffixToValueMap cssSuffixToValuesMap)
    {
        var isNumeric = int.TryParse(arbitraryValue, out _);
        var hasCalc = arbitraryValue.Contains("calc");
        var hasCssUnit = HasValidCssUnit(arbitraryValue);

        return hasCalc || isNumeric || hasCssUnit || cssSuffixToValuesMap.ContainsSuffix(arbitraryValue);
    }

    /// <summary>
    /// Checks if the arbitrary value contains a valid CSS unit.
    /// </summary>
    /// <param name="arbitraryValue">The arbitrary value to check.</param>
    /// <returns>True if the value contains a valid CSS unit, false otherwise.</returns>
    protected virtual bool HasValidCssUnit(string arbitraryValue)
    {
        // Common CSS units for sizing and spacing
        var cssUnits = new[]
        {
            "px", "em", "rem", "ex", "ch", "lh", "rlh", // Absolute and relative length units
            "vw", "vh", "vmin", "vmax", "vi", "vb", "dvw", "dvh", "lvw", "lvh", "svw", "svh", // Viewport units
            "%", // Percentage unit
            "fr", // Grid fractional unit
            "cm", "mm", "in", "pt", "pc", // Physical units
            "deg", "rad", "turn", "grad", // Angle units
            "s", "ms", // Time units
            "Hz", "kHz", // Frequency units
            "dpi", "dpcm", "dppx", // Resolution units
        };

        // Check if the value ends with any of the valid CSS units
        // Also handle negative values (starting with -)
        var valueToCheck = arbitraryValue.StartsWith('-') ? arbitraryValue[1..] : arbitraryValue;

        foreach (var unit in cssUnits)
        {
            if (valueToCheck.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                // Extract the numeric part before the unit
                var numericPart = valueToCheck[..^unit.Length];

                // Check if the numeric part is valid (can be integer or decimal)
                if (double.TryParse(numericPart, out _) || numericPart == "0")
                {
                    return true;
                }
            }
        }

        return false;
    }
}