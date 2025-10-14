using System.Collections.Immutable;
using System.Text;

namespace MonorailCss.Ast;

/// <summary>
/// Represents the base class for all nodes in the Abstract Syntax Tree (AST) used to define CSS rules and declarations.
/// Provides common properties and behaviors for specific types of nodes.
/// </summary>
public abstract record AstNode
{
    /// <summary>
    /// Gets or initializes the source location of the AST node.
    /// This property provides information about where the node appears within a source file,
    /// including its line, column, offset, and length.
    /// </summary>
    public SourceLocation? Src { get; init; }

    /// <summary>
    /// Gets or initializes the destination location of the AST node.
    /// This property indicates where the node ends within a source file,
    /// including its ending line, column, offset, and length.
    /// </summary>
    public SourceLocation? Dst { get; init; }

    /// <summary>
    /// Converts the current AST node and its children, if applicable, to its CSS representation as a string.
    /// </summary>
    /// <param name="indentLevel">The current indentation level to apply to the CSS representation. Defaults to 0.</param>
    /// <returns>A string representing the CSS output of the node and its children, formatted with the specified indentation.</returns>
    public abstract string ToCss(int indentLevel = 0);

    /// <summary>
    /// Generates a string consisting of a specified number of spaces for use as an indentation in the CSS representation.
    /// Each level corresponds to two spaces.
    /// </summary>
    /// <param name="level">The number of indentation levels to generate. Each level adds two spaces.</param>
    /// <returns>A string of spaces representing the specified indentation level.</returns>
    protected static string GetIndent(int level) => new string(' ', level * 2);
}

/// <summary>
/// Represents a CSS declaration consisting of a property, value, and optional importance flag.
/// Used as a node in the Abstract Syntax Tree (AST) for constructing and manipulating CSS rules.
/// </summary>
public record Declaration(string Property, string Value, bool Important = false) : AstNode
{
    /// <inheritdoc />
    public override string ToString() => $"{Property}: {Value}{(Important ? " !important" : string.Empty)}";

    /// <inheritdoc />
    public override string ToCss(int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        var importantStr = Important ? " !important" : string.Empty;
        return $"{indent}{Property}: {Value}{importantStr};";
    }
}

internal record StyleRule(string Selector, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"{Selector} {{ {Nodes.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        var indent = GetIndent(indentLevel);

        sb.AppendLine($"{indent}{Selector} {{");

        foreach (var node in Nodes)
        {
            sb.AppendLine(node.ToCss(indentLevel + 1));
        }

        sb.Append($"{indent}}}");

        return sb.ToString();
    }
}

internal record NestedRule(string Selector, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"{Selector} {{ {Nodes.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        var indent = GetIndent(indentLevel);

        // Nested rules use & parent selector syntax
        sb.AppendLine($"{indent}{Selector} {{");

        foreach (var node in Nodes)
        {
            sb.AppendLine(node.ToCss(indentLevel + 1));
        }

        sb.Append($"{indent}}}");

        return sb.ToString();
    }
}

internal record AtRule(string Name, string Params, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"{Name} {Params} {{ {Nodes.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        var indent = GetIndent(indentLevel);
        var paramsStr = string.IsNullOrEmpty(Params) ? string.Empty : $" {Params}";

        if (Nodes.IsEmpty)
        {
            sb.Append($"{indent}@{Name}{paramsStr};");
        }
        else
        {
            sb.AppendLine($"{indent}@{Name}{paramsStr} {{");

            foreach (var node in Nodes)
            {
                sb.AppendLine(node.ToCss(indentLevel + 1));
            }

            sb.Append($"{indent}}}");
        }

        return sb.ToString();
    }
}

internal record Comment(string Value) : AstNode
{
    public override string ToString() => $"/* {Value} */";

    public override string ToCss(int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        return $"{indent}/* {Value} */";
    }
}

internal record Context(ImmutableDictionary<string, object> Metadata, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"Context {{ {Nodes.Count} nodes, {Metadata.Count} metadata }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();

        foreach (var node in Nodes)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.Append(node.ToCss(indentLevel));
        }

        return sb.ToString();
    }
}

internal record RawCss(string Content) : AstNode
{
    public override string ToString() => $"RawCss {{ {Content.Length} chars }}";

    public override string ToCss(int indentLevel = 0)
    {
        var indent = GetIndent(indentLevel);
        var lines = Content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        if (lines.Length == 1)
        {
            return $"{indent}{Content}";
        }

        var sb = new StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                sb.Append($"{indent}{lines[i]}");
            }
            else if (i < lines.Length - 1)
            {
                // Preserve empty lines except at the end
                sb.Append(string.Empty);
            }
        }

        return sb.ToString();
    }
}