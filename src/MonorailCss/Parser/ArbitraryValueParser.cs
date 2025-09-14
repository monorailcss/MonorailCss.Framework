using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace MonorailCss.Parser;

internal partial class ArbitraryValueParser
{
    private const char UnderscoreChar = '_';
    private const char SpaceChar = ' ';
    private const string VarPrefix = "--";
    private const string VarFunction = "var";

    [GeneratedRegex(@"^([\w-]+):(.+)$")]
    private static partial Regex DataTypeHintPattern();

    public ParsedArbitraryValue Parse(string value, ArbitraryValueType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ParsedArbitraryValue
            {
                IsValid = false,
                ErrorMessage = "Empty arbitrary value",
            };
        }

        // Reject single underscore or only whitespace
        if (value == "_" || string.IsNullOrWhiteSpace(value))
        {
            return new ParsedArbitraryValue
            {
                IsValid = false,
                ErrorMessage = "Invalid arbitrary value",
            };
        }

        return type switch
        {
            ArbitraryValueType.Brackets => ParseBracketValue(value),
            ArbitraryValueType.Parentheses => ParseParenthesesValue(value),
            _ => new ParsedArbitraryValue { IsValid = false, ErrorMessage = "Unknown arbitrary value type" },
        };
    }

    private ParsedArbitraryValue ParseBracketValue(string value)
    {
        // Check for invalid characters that shouldn't be in arbitrary values
        if (ContainsInvalidCharacters(value))
        {
            return new ParsedArbitraryValue
            {
                IsValid = false,
                ErrorMessage = "Arbitrary value contains invalid characters (semicolons or braces)",
            };
        }

        // Check for data type hint: [color:var(--value)] or [length:100px]
        var dataTypeMatch = DataTypeHintPattern().Match(value);
        if (dataTypeMatch.Success)
        {
            var dataType = dataTypeMatch.Groups[1].Value;
            var actualValue = dataTypeMatch.Groups[2].Value;

            return new ParsedArbitraryValue
            {
                IsValid = true,
                Value = DecodeArbitraryValue(actualValue),
                DataTypeHint = dataType,
                OriginalValue = value,
            };
        }

        // Regular arbitrary value
        return new ParsedArbitraryValue
        {
            IsValid = true,
            Value = DecodeArbitraryValue(value),
            OriginalValue = value,
        };
    }

    private ParsedArbitraryValue ParseParenthesesValue(string value)
    {
        // Parentheses shorthand only works with CSS variables (--prefix)
        // Format: (--my-color) or (--my-color,fallback) or (color:--my-color)

        // Check for data type hint in parentheses: (color:--my-color)
        var dataTypeMatch = DataTypeHintPattern().Match(value);
        if (dataTypeMatch.Success)
        {
            var dataType = dataTypeMatch.Groups[1].Value;
            var varContent = dataTypeMatch.Groups[2].Value;

            if (!varContent.StartsWith(VarPrefix))
            {
                return new ParsedArbitraryValue
                {
                    IsValid = false,
                    ErrorMessage = $"Parentheses shorthand with type hint requires CSS variable (--prefix), got: {varContent}",
                };
            }

            // Convert to var() function with fallback support
            var processedValue = ProcessCssVariableContent(varContent);
            return new ParsedArbitraryValue
            {
                IsValid = true,
                Value = $"{VarFunction}({processedValue})",
                DataTypeHint = dataType,
                OriginalValue = value,
                IsParenthesesShorthand = true,
            };
        }

        // Regular parentheses shorthand: (--my-color) or (--my-color,fallback)
        if (!value.StartsWith(VarPrefix))
        {
            return new ParsedArbitraryValue
            {
                IsValid = false,
                ErrorMessage = $"Parentheses shorthand only works with CSS variables (--prefix), got: {value}",
            };
        }

        // Convert to var() function
        var processed = ProcessCssVariableContent(value);
        return new ParsedArbitraryValue
        {
            IsValid = true,
            Value = $"{VarFunction}({processed})",
            OriginalValue = value,
            IsParenthesesShorthand = true,
        };
    }

    private string ProcessCssVariableContent(string content)
    {
        // Handle CSS variable with fallback: --my-color,fallback_value
        var parts = SplitCssVariableParts(content);
        if (parts.Count == 1)
        {
            // No fallback, just the variable
            return parts[0];
        }

        // Has fallback value(s)
        var variable = parts[0];
        var fallback = string.Join(",", parts.Skip(1).Select(p => DecodeArbitraryValue(p)));
        return $"{variable},{fallback}";
    }

    private List<string> SplitCssVariableParts(string content)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var depth = 0;
        var inString = false;
        char? stringChar = null;

        for (var i = 0; i < content.Length; i++)
        {
            var ch = content[i];

            // Handle string boundaries
            if (ch is '"' or '\'' && (i == 0 || content[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = ch;
                }
                else if (ch == stringChar)
                {
                    inString = false;
                    stringChar = null;
                }
            }

            // Track parentheses depth
            if (!inString)
            {
                if (ch == '(')
                {
                    depth++;
                }
                else if (ch == ')')
                {
                    depth--;
                }
            }

            // Split on comma only at depth 0 and not in string
            if (ch == ',' && depth == 0 && !inString)
            {
                parts.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        return parts;
    }

    public string DecodeArbitraryValue(string input)
    {
        // There are definitely no functions in the input, so bail early
        if (!input.Contains('('))
        {
            return ConvertUnderscoresToWhitespace(input);
        }

        // Parse the value into an AST
        var ast = ValueParser.Parse(input);

        // Recursively decode arbitrary values in the AST
        RecursivelyDecodeArbitraryValues(ast);

        // Convert back to CSS string
        var result = ValueParser.ToCss(ast);

        // Add whitespace around math operators
        result = MathOperatorProcessor.AddWhitespaceAroundMathOperators(result);

        return result;
    }

    private void RecursivelyDecodeArbitraryValues(List<ValueAstNode> ast)
    {
        foreach (var node in ast)
        {
            switch (node)
            {
                case FunctionNode func:
                    // Don't decode underscores in url() functions
                    if (func.Value.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                        func.Value.EndsWith("_url", StringComparison.OrdinalIgnoreCase))
                    {
                        func.Value = ConvertUnderscoresToWhitespace(func.Value);
                        break;
                    }

                    // Special handling for var() and theme() functions
                    if (func.Value.Equals("var", StringComparison.OrdinalIgnoreCase) ||
                        func.Value.EndsWith("_var", StringComparison.OrdinalIgnoreCase) ||
                        func.Value.Equals("theme", StringComparison.OrdinalIgnoreCase) ||
                        func.Value.EndsWith("_theme", StringComparison.OrdinalIgnoreCase))
                    {
                        func.Value = ConvertUnderscoresToWhitespace(func.Value);

                        // Don't decode underscores in the first argument
                        for (var i = 0; i < func.Nodes.Count; i++)
                        {
                            if (i == 0 && func.Nodes[i] is WordNode firstArg)
                            {
                                // Preserve underscores in first argument (skip conversion)
                                firstArg.Value = ConvertUnderscoresToWhitespace(firstArg.Value, skipUnderscoreToSpace: true);
                            }
                            else
                            {
                                RecursivelyDecodeArbitraryValues([func.Nodes[i]]);
                            }
                        }

                        break;
                    }

                    // Regular function - process everything
                    func.Value = ConvertUnderscoresToWhitespace(func.Value);
                    RecursivelyDecodeArbitraryValues(func.Nodes);
                    break;

                case WordNode:
                case SeparatorNode:
                    node.Value = ConvertUnderscoresToWhitespace(node.Value);
                    break;
            }
        }
    }

    private string ConvertUnderscoresToWhitespace(string input, bool skipUnderscoreToSpace = false)
    {
        var output = new StringBuilder();

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            // Escaped underscore
            if (c == '\\' && i + 1 < input.Length && input[i + 1] == '_')
            {
                output.Append('_');
                i++; // Skip the underscore
            }

            // Unescaped underscore
            else if (c == '_' && !skipUnderscoreToSpace)
            {
                output.Append(' ');
            }

            // All other characters
            else
            {
                output.Append(c);
            }
        }

        return output.ToString();
    }

    private string ProcessFunctionWithPreservedFirstArg(string value)
    {
        // For var(--foo_bar) or theme(--foo_bar), preserve underscores in first argument
        var functionName = value.Contains("var(") ? "var" : "theme";
        var startIdx = functionName.Length + 1; // After "var(" or "theme("

        // Find the end of the first argument
        var depth = 1;
        var firstArgEnd = -1;
        var inString = false;
        char? stringChar = null;

        for (var i = startIdx; i < value.Length; i++)
        {
            var ch = value[i];

            // Handle string boundaries
            if (ch is '"' or '\'' && (i == 0 || value[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = ch;
                }
                else if (ch == stringChar)
                {
                    inString = false;
                    stringChar = null;
                }
            }

            if (!inString)
            {
                if (ch == '(')
                {
                    depth++;
                }
                else if (ch == ')')
                {
                    depth--;
                }
                else if (ch == ',' && depth == 1)
                {
                    firstArgEnd = i;
                    break;
                }
            }

            if (depth == 0)
            {
                firstArgEnd = i;
                break;
            }
        }

        if (firstArgEnd == -1)
        {
            // No comma found, entire content is the first argument
            return value;
        }

        // Preserve first argument, process the rest
        var firstArg = value[..firstArgEnd];
        var rest = value[firstArgEnd..];
        return firstArg + ReplaceNonEscapedUnderscores(rest);
    }

    private string ReplaceNonEscapedUnderscores(string value)
    {
        // First, mark escaped underscores with a placeholder
        var placeholder = "\u0001"; // Use a control character as placeholder
        var withPlaceholders = value.Replace("\\_", placeholder);

        // Replace non-escaped underscores with spaces
        var withSpaces = withPlaceholders.Replace(UnderscoreChar, SpaceChar);

        // Restore escaped underscores (without the backslash)
        return withSpaces.Replace(placeholder, "_");
    }

    private bool ContainsInvalidCharacters(string value)
    {
        // Check for semicolons or braces which are not allowed in arbitrary values
        // (they could be used for CSS injection)
        return value.Contains(';') || value.Contains('{') || value.Contains('}');
    }

    public bool TryParseCalcExpression(string value, [NotNullWhen(true)] out string? processed)
    {
        processed = null;

        if (!value.StartsWith("calc(", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // For now, just validate that parentheses are balanced
        // and process underscores in the calc expression
        if (!AreParenthesesBalanced(value))
        {
            return false;
        }

        processed = DecodeArbitraryValue(value);
        return true;
    }

    public bool TryParseThemeFunction(string value, [NotNullWhen(true)] out string? processed)
    {
        processed = null;

        if (!value.StartsWith("theme("))
        {
            return false;
        }

        if (!AreParenthesesBalanced(value))
        {
            return false;
        }

        // Preserve underscores in the theme path (first argument)
        processed = ProcessFunctionWithPreservedFirstArg(value);
        return true;
    }

    private bool AreParenthesesBalanced(string value)
    {
        var depth = 0;
        var inString = false;
        char? stringChar = null;

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];

            // Handle string boundaries
            if (ch is '"' or '\'' && (i == 0 || value[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = ch;
                }
                else if (ch == stringChar)
                {
                    inString = false;
                    stringChar = null;
                }
            }

            if (!inString)
            {
                if (ch == '(')
                {
                    depth++;
                }
                else if (ch == ')')
                {
                    depth--;
                }

                if (depth < 0)
                {
                    return false; // More closing than opening
                }
            }
        }

        return depth == 0;
    }
}

internal enum ArbitraryValueType
{
    Brackets,    // [value] syntax
    Parentheses,  // (value) syntax - CSS variable shorthand
}

internal class ParsedArbitraryValue
{
    public bool IsValid { get; init; }
    public string? Value { get; init; }
    public string? OriginalValue { get; init; }
    public string? DataTypeHint { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsParenthesesShorthand { get; init; }
}