using System.Text;

namespace MonorailCss.Parser;

internal static class MathOperatorProcessor
{
    private static readonly HashSet<string> _mathFunctions =
    [
        "calc", "min", "max", "clamp", "mod", "rem",
        "sin", "cos", "tan", "asin", "acos", "atan", "atan2",
        "pow", "sqrt", "hypot", "log", "exp", "round",
    ];

    public static string AddWhitespaceAroundMathOperators(string input)
    {
        // Bail early if there are no math functions in the input
        if (!ContainsMathFunction(input))
        {
            return input;
        }

        var result = new StringBuilder();
        var formattableStack = new Stack<bool>();
        int? valuePos = null;
        int? lastValuePos = null;

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            // Track if we see a number followed by a unit
            if (char.IsDigit(c))
            {
                valuePos = i;
            }

            // If we saw a number before and see a letter or %, assume it's a value like "123px"
            else if (valuePos != null && (char.IsLetter(c) || c == '%'))
            {
                valuePos = i;
            }
            else
            {
                lastValuePos = valuePos;
                valuePos = null;
            }

            // Handle opening parenthesis
            if (c == '(')
            {
                result.Append(c);

                // Scan backwards to determine the function name
                // Only include lowercase letters and digits (matching Tailwind's behavior)
                var start = i;
                for (var j = i - 1; j >= 0; j--)
                {
                    var inner = input[j];
                    if (char.IsDigit(inner) || (char.IsLetter(inner) && char.IsLower(inner)))
                    {
                        start = j;
                    }
                    else
                    {
                        break;
                    }
                }

                var functionName = input[start..i];

                // Check if this is a math function
                if (_mathFunctions.Contains(functionName.ToLowerInvariant()))
                {
                    formattableStack.Push(true);
                }

                // Nested parens inside a math function
                else if (formattableStack.Count > 0 && formattableStack.Peek() && string.IsNullOrEmpty(functionName))
                {
                    formattableStack.Push(true);
                }
                else
                {
                    formattableStack.Push(false);
                }

                continue;
            }

            // Handle closing parenthesis
            if (c == ')')
            {
                result.Append(c);
                if (formattableStack.Count > 0)
                {
                    formattableStack.Pop();
                }

                continue;
            }

            // Add spaces after commas in math functions
            if (c == ',' && formattableStack.Count > 0 && formattableStack.Peek())
            {
                result.Append(", ");
                continue;
            }

            // Skip consecutive whitespace in math functions
            if (c == ' ' && formattableStack.Count > 0 && formattableStack.Peek() &&
                result.Length > 0 && result[^1] == ' ')
            {
                continue;
            }

            // Handle math operators
            if (c is '+' or '-' or '*' or '/' &&
                formattableStack.Count > 0 && formattableStack.Peek())
            {
                // Get previous and next characters for context
                var trimmed = result.ToString().TrimEnd();
                var prev = trimmed.Length > 0 ? trimmed[^1] : '\0';
                var prevPrev = trimmed.Length > 1 ? trimmed[^2] : '\0';
                var next = i + 1 < input.Length ? input[i + 1] : '\0';

                // Don't add spaces for scientific notation (e.g., -3.4e-2)
                if (prev is 'e' or 'E' && char.IsDigit(prevPrev))
                {
                    result.Append(c);
                    continue;
                }

                // If preceded by an operator, handle special cases
                if (prev is '+' or '-' or '*' or '/')
                {
                    // Check for double operator cases that need spacing (e.g., --, +-, -+, ++)
                    if (c is '-' or '+' && prev is '-' or '+')
                    {
                        // Double operator case - need space between them
                        // But check if we already have a space
                        if (result.Length > 0 && result[^1] != ' ')
                        {
                            result.Append(' ');
                        }

                        result.Append(c);
                    }
                    else
                    {
                        // Regular case - no spacing needed
                        result.Append(c);
                    }

                    continue;
                }

                // At the beginning of an argument (after paren or comma)
                if (prev is '(' or ',')
                {
                    result.Append(c);
                    continue;
                }

                // Add spaces only after if we already have spaces before
                if (i > 0 && input[i - 1] == ' ')
                {
                    result.Append(c).Append(' ');
                    continue;
                }

                // Add spaces around the operator when appropriate
                var shouldAddSpaces =
                    char.IsDigit(prev) ||
                    char.IsDigit(next) ||
                    prev == ')' ||
                    next == '(' ||
                    next == '-' || // Handle double negative by always spacing before negative
                    next == '+' || next == '*' || next == '/' ||
                    (lastValuePos != null && lastValuePos == i - 1);

                if (shouldAddSpaces)
                {
                    // Trim any trailing whitespace before adding operator with spaces
                    if (result.Length > 0 && result[^1] == ' ')
                    {
                        result.Length--;
                    }

                    result.Append(' ').Append(c).Append(' ');
                }
                else
                {
                    result.Append(c);
                }
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    private static bool ContainsMathFunction(string input)
    {
        if (!input.Contains('('))
        {
            return false;
        }

        foreach (var fn in _mathFunctions)
        {
            if (input.Contains($"{fn}(", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}