using System.Text;

namespace MonorailCss.Parser;

internal abstract class ValueAstNode
{
    public abstract string Kind { get; }
    public string Value { get; set; } = string.Empty;
}

internal class WordNode : ValueAstNode
{
    public override string Kind => "word";
}

internal class FunctionNode : ValueAstNode
{
    public override string Kind => "function";
    public List<ValueAstNode> Nodes { get; set; } = [];
}

internal class SeparatorNode : ValueAstNode
{
    public override string Kind => "separator";
}

internal static class ValueParser
{
    public static List<ValueAstNode> Parse(string input)
    {
        var nodes = new List<ValueAstNode>();
        var current = new StringBuilder();
        var stack = new Stack<List<ValueAstNode>>();
        var functionNameStack = new Stack<string>();
        var inString = false;
        char? stringChar = null;

        stack.Push(nodes);

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            // Handle string boundaries
            if (c is '"' or '\'' && (i == 0 || input[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = c;
                    current.Append(c);
                }
                else if (c == stringChar)
                {
                    inString = false;
                    stringChar = null;
                    current.Append(c);
                }
                else
                {
                    current.Append(c);
                }

                continue;
            }

            if (inString)
            {
                current.Append(c);
                continue;
            }

            // Handle function start
            if (c == '(')
            {
                if (current.Length > 0)
                {
                    // Check if we have a function name or if it's part of a word
                    // Look for a function pattern: word followed by '('
                    var potentialFunctionName = current.ToString();

                    // Find the last occurrence of common separators that indicate a function boundary
                    var functionStart = -1;
                    for (var j = potentialFunctionName.Length - 1; j >= 0; j--)
                    {
                        var ch = potentialFunctionName[j];
                        if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != '_')
                        {
                            functionStart = j + 1;
                            break;
                        }
                    }

                    if (functionStart > 0)
                    {
                        // We have content before the function name
                        var beforeFunction = potentialFunctionName[..functionStart];
                        var functionName = potentialFunctionName[functionStart..];

                        if (!string.IsNullOrEmpty(beforeFunction))
                        {
                            stack.Peek().Add(new WordNode { Value = beforeFunction });
                        }

                        var functionNode = new FunctionNode { Value = functionName };
                        stack.Peek().Add(functionNode);
                        stack.Push(functionNode.Nodes);
                        functionNameStack.Push(functionName);
                    }
                    else
                    {
                        // Entire content is the function name
                        var functionNode = new FunctionNode { Value = potentialFunctionName };
                        stack.Peek().Add(functionNode);
                        stack.Push(functionNode.Nodes);
                        functionNameStack.Push(potentialFunctionName);
                    }

                    current.Clear();
                }
                else
                {
                    // Opening paren without a function name - just add it as text
                    current.Append(c);
                }

                continue;
            }

            // Handle function end
            if (c == ')')
            {
                // Add any remaining content as a word node
                if (current.Length > 0)
                {
                    stack.Peek().Add(new WordNode { Value = current.ToString() });
                    current.Clear();
                }

                if (stack.Count > 1)
                {
                    stack.Pop();
                    if (functionNameStack.Count > 0)
                    {
                        functionNameStack.Pop();
                    }
                }
                else
                {
                    // We have a closing paren without a matching function, treat it as text
                    nodes.Add(new WordNode { Value = ")" });
                }

                continue;
            }

            // Handle comma separator
            if (c == ',' && stack.Count > 1)
            {
                // Add any content before the comma
                if (current.Length > 0)
                {
                    stack.Peek().Add(new WordNode { Value = current.ToString() });
                    current.Clear();
                }

                // Add the separator
                stack.Peek().Add(new SeparatorNode { Value = "," });
                continue;
            }

            // Regular character
            current.Append(c);
        }

        // Add any remaining content
        if (current.Length > 0)
        {
            nodes.Add(new WordNode { Value = current.ToString() });
        }

        return nodes;
    }

    public static string ToCss(List<ValueAstNode> nodes)
    {
        var result = new StringBuilder();

        foreach (var node in nodes)
        {
            switch (node)
            {
                case WordNode word:
                    result.Append(word.Value);
                    break;

                case FunctionNode func:
                    result.Append(func.Value);
                    result.Append('(');
                    result.Append(ToCss(func.Nodes));
                    result.Append(')');
                    break;

                case SeparatorNode sep:
                    result.Append(sep.Value);
                    break;
            }
        }

        return result.ToString();
    }

    public static void RecursivelyProcessNodes(List<ValueAstNode> nodes, Func<ValueAstNode, int, bool> processor)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            // Process current node
            var skipFirstArgument = processor(node, i);

            // Recursively process function nodes
            if (node is FunctionNode func)
            {
                // For var() and theme() functions, skip processing the first argument
                if (skipFirstArgument && func.Nodes.Count > 0)
                {
                    // Process arguments after the first one
                    for (var j = 0; j < func.Nodes.Count; j++)
                    {
                        if (j == 0 && func.Nodes[j] is WordNode)
                        {
                            // Skip processing the first word argument
                            continue;
                        }

                        if (func.Nodes[j] is FunctionNode nestedFunc)
                        {
                            RecursivelyProcessNodes([nestedFunc], processor);
                        }
                        else if (j > 0 || !(func.Nodes[j] is WordNode))
                        {
                            processor(func.Nodes[j], j);
                        }
                    }
                }
                else
                {
                    RecursivelyProcessNodes(func.Nodes, processor);
                }
            }
        }
    }
}