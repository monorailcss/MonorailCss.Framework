namespace MonorailCss.Parser;

internal class ClassParser
{
    private readonly string[] _namespaces;
    private readonly string _prefix;
    private readonly char _separator;

    public ClassParser(string[] namespaces, string prefix, char separator)
    {
        _namespaces = namespaces;
        _prefix = prefix;
        _separator = separator;
    }

    public IParsedClassNameSyntax? Extract(string className)
    {
        // Regular utilities
        // {{modifier}:}*{namespace}{-{suffix}}*{/{opacityModifier}}?

        // Arbitrary values
        // {{modifier}:}*{namespace}-[{arbitraryValue}]{/{opacityModifier}}?
        // arbitraryValue: no whitespace, balanced quotes unless within quotes, balanced brackets unless within quotes

        // Arbitrary properties
        // {{modifier}:}*[{validCssPropertyName}:{arbitraryValue}]
        var value = className;

        if (value.EndsWith(_separator) || value.StartsWith(_separator))
        {
            // can't begin or end with the separator character.
            return null;
        }

        var sections = value.Split(
            _separator,
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
            modifiers = [];
            value = sections[0];
        }

        value = _prefix.Length > 0 && value.StartsWith(_prefix, StringComparison.InvariantCultureIgnoreCase)
            ? value[_prefix.Length..]
            : value;

        if (value.StartsWith('['))
        {
            // if the first character after the modifiers is a bracket then we have an arbitrary
            // css function and value e.g. sm:[perspective: length]
            var arbitraryPropertyUtility = ParseArbitraryProperty(value);
            return arbitraryPropertyUtility == null
                ? null
                : new ArbitraryPropertySyntax(
                    className,
                    modifiers,
                    arbitraryPropertyUtility.Value.Property,
                    arbitraryPropertyUtility.Value.Value);
        }

        if (value.StartsWith("-"))
        {
            value = value[1..] + '-';
        }

        // find the namespace from a list of valid namespace. we need to whitelist these
        // so we can tell the difference between bg-blue which is namespaced and line-through which is not.
        var ns = _namespaces.FirstOrDefault(n => value.Equals(n, StringComparison.Ordinal) || (value.StartsWith(n, StringComparison.Ordinal) && value.Length > n.Length && value[n.Length] == '-'));
        var dashSearchStartPos = ns?.Length - 1 ?? 0;
        var firstDashIndex = value.IndexOf('-', dashSearchStartPos);
        if (firstDashIndex < 0)
        {
            if (ns != null)
            {
                return new NamespaceSyntax(className, modifiers, ns, null);
            }

            // no dash. so either the root of a namespace or a regular utility.
            return new UtilitySyntax(className, modifiers, value);
        }

        if (ns == null)
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