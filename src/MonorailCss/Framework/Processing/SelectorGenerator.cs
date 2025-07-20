using System.Text;
using MonorailCss.Css;
using MonorailCss.Variants;

namespace MonorailCss.Framework.Processing;

/// <summary>
/// Provides static methods for CSS selector generation, escaping, and CSS variable handling.
/// </summary>
public static class SelectorGenerator
{
    /// <summary>
    /// Returns a prefixed variable name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>A string in the format of --{prefix}-{name}.</returns>
    public static string GetVariableNameWithPrefix(string name)
    {
        return $"--monorail-{name}";
    }

    /// <summary>
    /// Returns a prefixed variable name along wrapped with var for using in CSS.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <returns>A string in the format of var(--{prefix}-{name}.</returns>
    public static string GetCssVariableWithPrefix(string name)
    {
        var cssVariableWithPrefix = $"var(--monorail-{name})";
        return cssVariableWithPrefix;
    }

    /// <summary>
    /// Generates CSS selector syntax with variants applied.
    /// </summary>
    /// <param name="original">The original CSS selector.</param>
    /// <param name="variants">List of variants to apply.</param>
    /// <returns>CSS selector string with variants applied.</returns>
    public static string GetSelectorSyntax(CssSelector original, IList<IVariant> variants)
    {
        if (original.Selector.StartsWith('@'))
        {
            // keyframe, so we don't want to mess with the name.
            return original.Selector;
        }

        var spaceIndex = original.Selector.IndexOf(' ');
        var selector = spaceIndex >= 0
            ? EscapeFirstWord(original.Selector, spaceIndex)
            : EscapeCssClassSelector(original.Selector);

        selector = $".{selector}";

        if (original.PseudoClass != null)
        {
            selector = $"{selector}{original.PseudoClass}";
        }

        // Process non-conditional variants first
        var conditionalVariant = variants.OfType<NameConditionalVariant>().FirstOrDefault();
        var nonConditionalVariants = variants.Where(v => v is not NameConditionalVariant);

        selector = nonConditionalVariants.OrderBy(v => typeof(PseudoElementVariant) == v.GetType() ? 1 : 0).Aggregate(selector, (current, variant) => variant switch
        {
            SelectorVariant selectorVariant => $"{selectorVariant.Selector} {current}",
            AttributeVariant attributeVariant => $"{current}{attributeVariant.AttributeSelector}",
            ProseElementVariant proseElementVariant => $"{current} {proseElementVariant.Selector}",
            PseudoClassVariant pseudoClassVariant => $"{current}{pseudoClassVariant.PseudoClass}",
            PseudoElementVariant pseudoElementVariant => $"{current}{pseudoElementVariant.PseudoElement}",
            _ => current,
        });

        if (original.PseudoElement != null)
        {
            selector = $"{selector}{original.PseudoElement}";
        }

        // Add conditional variant if present
        if (conditionalVariant != null)
        {
            var name = string.Empty;

            if (original.Selector.Contains('/'))
            {
                // Find the position of slash and colon
                var slashIndex = original.Selector.IndexOf('/');
                var colonIndex = original.Selector.IndexOf(':');

                // If there's no colon after the slash, return the original string
                if (colonIndex > slashIndex)
                {
                    name = $@"\/{original.Selector.Substring(slashIndex + 1, colonIndex - slashIndex - 1)}";
                }
            }

            selector = $"{selector} {conditionalVariant.Condition(name)}";
        }

        return selector;
    }

    /// <summary>
    /// Applies variants to an element selector for Applies functionality.
    /// </summary>
    /// <param name="elementSelector">The base element selector.</param>
    /// <param name="variants">List of variants to apply.</param>
    /// <returns>Element selector with variants applied.</returns>
    public static string ApplyVariantsToElementSelector(string elementSelector, IList<IVariant> variants)
    {
        if (!variants.Any())
        {
            return elementSelector;
        }

        // Process non-conditional variants first
        var conditionalVariant = variants.OfType<NameConditionalVariant>().FirstOrDefault();
        var nonConditionalVariants = variants.Where(v => v is not NameConditionalVariant);

        var selector = elementSelector;

        // Apply variants in the correct order (similar to GetSelectorSyntax)
        selector = nonConditionalVariants.OrderBy(v => typeof(PseudoElementVariant) == v.GetType() ? 1 : 0).Aggregate(selector, (current, variant) => variant switch
        {
            SelectorVariant selectorVariant => $"{selectorVariant.Selector} {current}",
            AttributeVariant attributeVariant => $"{current}{attributeVariant.AttributeSelector}",
            ProseElementVariant proseElementVariant => $"{current} {proseElementVariant.Selector}",
            PseudoClassVariant pseudoClassVariant => $"{current}{pseudoClassVariant.PseudoClass}",
            PseudoElementVariant pseudoElementVariant => $"{current}{pseudoElementVariant.PseudoElement}",
            _ => current,
        });

        // Add conditional variant if present
        if (conditionalVariant != null)
        {
            var name = string.Empty;
            selector = $"{selector} {conditionalVariant.Condition(name)}";
        }

        return selector;
    }

    /// <summary>
    /// Strips named variant from a modifier string.
    /// </summary>
    /// <param name="original">The original modifier string.</param>
    /// <returns>Modifier string without named variant.</returns>
    public static string StripNamedVariant(string original)
    {
        var colonIndex = original.IndexOf('/');
        if (colonIndex < 0)
        {
            return original;
        }

        return original[..colonIndex];
    }

    /// <summary>
    /// Escapes the first word of a CSS selector and preserves the rest.
    /// </summary>
    /// <param name="originalSelector">The original selector.</param>
    /// <param name="spaceIndex">Index of the first space.</param>
    /// <returns>Selector with first word escaped.</returns>
    private static string EscapeFirstWord(string originalSelector, int spaceIndex)
    {
        var firstWord = originalSelector[..spaceIndex];

        firstWord = EscapeCssClassSelector(firstWord);

        return $"{firstWord}{originalSelector[spaceIndex..]}";
    }

    private static string EscapeCssClassSelector(string firstWord)
    {
        if (string.IsNullOrWhiteSpace(firstWord))
        {
            return firstWord;
        }

        var result = new StringBuilder();

        // Handle the first character specially
        var firstChar = firstWord[0];

        // Check if the first character needs escaping (digits 0-9)
        if (char.IsDigit(firstChar))
        {
            // Escape as a hex code point followed by space
            result.Append($"\\{(int)firstChar:x} ");
        }
        else if (NeedsEscaping(firstChar))
        {
            result.Append('\\').Append(firstChar);
        }
        else
        {
            result.Append(firstChar);
        }

        // Process remaining characters
        for (var i = 1; i < firstWord.Length; i++)
        {
            var c = firstWord[i];
            if (NeedsEscaping(c))
            {
                if (c == ',')
                {
                    result.Append("\\2c ");
                }
                else
                {
                    result.Append('\\').Append(c);
                }
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    private static bool NeedsEscaping(char c)
    {
        return c switch
        {
            '*' or ':' or '/' or '[' or ']' or '#' or
                '(' or ')' or '.' or ',' or '?' or '=' or '&' => true,
            _ => false,
        };
    }
}