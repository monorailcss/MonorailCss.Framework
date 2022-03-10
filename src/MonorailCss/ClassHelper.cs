using MonorailCss.Css;
using MonorailCss.Variants;

namespace MonorailCss;

/// <summary>
/// Represents a parsed class name.
/// </summary>
public interface IParsedClassNameSyntax
{
    /// <summary>
    /// Gets the original syntax of the class name.
    /// </summary>
    string OriginalSyntax { get; init; }

    /// <summary>
    /// Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.
    /// </summary>
    string[] Modifiers { get; init; }
}

/// <summary>
/// Represents a parsed CSS class that is a utility function.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="Name">Gets the name of the utility.</param>
public record UtilitySyntax(string OriginalSyntax, string[] Modifiers, string Name) : IParsedClassNameSyntax
{
    /// <summary>
    /// Checks if a utility name matches for this utility.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if they match case-insensitive, false otherwise.</returns>
    public bool NameEquals(string value) => value.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
}

/// <summary>
/// Represents a parsed CSS class that is a namespaced utility.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="Namespace">The namespace of the utility.</param>
/// <param name="Suffix">The suffix of the utility.</param>
public record NamespaceSyntax(
    string OriginalSyntax,
    string[] Modifiers,
    string Namespace,
    string? Suffix) : IParsedClassNameSyntax
{
    /// <summary>
    /// Checks if a namespace matches for this syntax.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <returns>True if they match case-insensitive, false otherwise.</returns>
    public bool NamespaceEquals(string value) => value.Equals(Namespace, StringComparison.InvariantCultureIgnoreCase);
}

/// <summary>
/// Represents a parsed CSS class that has a namespace and arbitrary value.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="Namespace">The namespace.</param>
/// <param name="ArbitraryValue">The arbitrary value.</param>
public record ArbitraryValueSyntax(
    string OriginalSyntax,
    string[] Modifiers,
    string Namespace,
    string ArbitraryValue) : IParsedClassNameSyntax;

/// <summary>
/// Represents a parsed CSS class that is an arbitrary property.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="PropertyName">The CSS property name.</param>
/// <param name="ArbitraryValue">The value.</param>
public record ArbitraryPropertySyntax(
    string OriginalSyntax,
    string[] Modifiers,
    string PropertyName,
    string ArbitraryValue) : IParsedClassNameSyntax;

internal static class ClassHelper
{
    public static (string Color, string? Opacity) SplitColor(string value)
    {
        var split = value.Split('/');
        return split.Length == 2 ? (split[0], split[1]) : (value, default);
    }

    public static string GetSelectorSyntax(CssSelector original, IEnumerable<IVariant> variants)
    {
        var selector = $".{original.Selector.Replace(":", "\\:").Replace("/", "\\/")}";
        if (original.PseudoClass != default)
        {
            selector += ":" + original.PseudoClass;
        }

        selector = variants.Aggregate(selector, (current, variant) => variant switch
        {
            SelectorVariant selectorVariant => $"{selectorVariant.Selector} {current}",
            PseudoClassVariant pseudoClassVariant => $"{current}{pseudoClassVariant.PseudoClass}",
            _ => current,
        });

        if (original.PseudoElement != default)
        {
            selector += "::";
        }

        return selector;
    }

    public static IParsedClassNameSyntax? Extract(string className, string[] namespaces, string prefix, char separator)
    {
        // Regular utilities
        // {{modifier}:}*{namespace}{-{suffix}}*{/{opacityModifier}}?

        // Arbitrary values
        // {{modifier}:}*{namespace}-[{arbitraryValue}]{/{opacityModifier}}?
        // arbitraryValue: no whitespace, balanced quotes unless within quotes, balanced brackets unless within quotes

        // Arbitrary properties
        // {{modifier}:}*[{validCssPropertyName}:{arbitraryValue}]
        var value = className;
        if (value.EndsWith(separator) || value.StartsWith(separator))
        {
            // can't begin or end with the separator character.
            return null;
        }

        var sections = value.Split(
            separator,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        string[]? modifiers;
        if (sections.Length > 1)
        {
            if (sections[^2].Contains('[') && sections[^1].EndsWith(']'))
            {
                // oops, accidentally split an arbitrary value as the last two items...join them back up.
                modifiers = sections[..^2];
                value = $"{sections[^2]}:{sections[^1]}";
            }
            else
            {
                modifiers = sections[..^1];
                value = sections[^1];
            }
        }
        else
        {
            // only one item, no modifiers and set the value to parse to the only item.
            modifiers = Array.Empty<string>();
            value = sections[0];
        }

        value = prefix.Length > 0 && value.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)
            ? value[prefix.Length..]
            : value;

        if (value.StartsWith('['))
        {
            // if the first character after the modifiers is a bracket then we have an arbitrary
            // css function and value e.g. sm:[perspective: length]
            var arbitraryPropertyUtility = ParseArbitraryProperty(value);
            return arbitraryPropertyUtility == null
                ? default(IParsedClassNameSyntax?)
                : new ArbitraryPropertySyntax(
                    className,
                    modifiers,
                    arbitraryPropertyUtility.Value.Property,
                    arbitraryPropertyUtility.Value.Value);
        }

        // find the namespace from a list of valid namespace. we need to whitelist these
        // so we can tell the difference between bg-blue which is namespaced and line-through which is not.
        var ns = Array.Find(namespaces.OrderByDescending(i => i.Length).ToArray(), n => value.StartsWith($"{n}-") || value == n);
        var dashSearchStartPos = ns?.Length - 1 ?? 0;
        var firstDashIndex = value.IndexOf("-", dashSearchStartPos, StringComparison.Ordinal);
        if (firstDashIndex < 0)
        {
            if (ns != default)
            {
                return new NamespaceSyntax(className, modifiers, ns, default);
            }

            // no dash. so either the root of a namespace or a regular utility.
            return new UtilitySyntax(className, modifiers, value);
        }

        if (ns == default)
        {
            return !string.IsNullOrWhiteSpace(value)
                ? new UtilitySyntax(className, modifiers, value)
                : null;
        }

        value = value[(firstDashIndex + 1)..];
        if (!value.StartsWith('['))
        {
            // not an arbitrary value so we can just return.
            return !string.IsNullOrWhiteSpace(value)
                ? new NamespaceSyntax(className, modifiers, ns, value)
                : null;
        }

        if (!value.EndsWith(']'))
        {
            // gotta close the brackets.
            return null;
        }

        value = value[1..^1];
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return new ArbitraryValueSyntax(className, modifiers, ns, value);
    }

    private static bool TryParseOpacity(ref string value, out string? opacity)
    {
        var opacitySplit = value.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        switch (opacitySplit)
        {
            case { Length: 1 }:
                value = opacitySplit[0];
                opacity = default;
                break;
            case { Length: 2 }:
                value = opacitySplit[0];
                opacity = opacitySplit[1];
                break;
            default:
                opacity = default;
                return false;
        }

        return true;
    }

    private static (string Property, string Value)? ParseArbitraryProperty(string value)
    {
        var split = value.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            // either no colon or more than one, neither of which are valid.
            return null;
        }

        if (split[0] == "[" || split[1] == "]")
        {
            // empty value for prop or value should be ignored.
            return null;
        }

        return (split[0][1..], split[1][..^1]);
    }
}