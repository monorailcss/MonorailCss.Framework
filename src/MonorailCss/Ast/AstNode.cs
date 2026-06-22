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
    /// Writes this node's CSS representation directly into <paramref name="sb"/>, without the
    /// trailing newline (callers add the separator, mirroring the old <c>AppendLine(node.ToCss())</c>
    /// pattern). This is the allocation-free serialization path: nodes append straight into one
    /// shared buffer instead of each building its own string for the parent to copy. The default
    /// delegates to <see cref="ToCss"/> so any node type that hasn't overridden it still works.
    /// </summary>
    internal virtual void WriteCss(StringBuilder sb, int indentLevel = 0) => sb.Append(ToCss(indentLevel));

    /// <summary>
    /// Generates a string consisting of a specified number of spaces for use as an indentation in the CSS representation.
    /// Each level corresponds to two spaces.
    /// </summary>
    /// <param name="level">The number of indentation levels to generate. Each level adds two spaces.</param>
    /// <returns>A string of spaces representing the specified indentation level.</returns>
    protected static string GetIndent(int level) => new string(' ', level * 2);

    /// <summary>Appends <paramref name="level"/> levels of indentation (two spaces each) directly,
    /// without allocating an intermediate string the way <see cref="GetIndent"/> does.</summary>
    private protected static void AppendIndent(StringBuilder sb, int level) => sb.Append(' ', level * 2);
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
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    /// <inheritdoc />
    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        AppendIndent(sb, indentLevel);
        sb.Append(Property).Append(": ").Append(Value);
        if (Important)
        {
            sb.Append(" !important");
        }

        sb.Append(';');
    }
}

internal record StyleRule(string Selector, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"{Selector} {{ {Nodes.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        AppendIndent(sb, indentLevel);
        sb.Append(Selector).Append(" {").AppendLine();

        foreach (var node in Nodes)
        {
            node.WriteCss(sb, indentLevel + 1);
            sb.AppendLine();
        }

        AppendIndent(sb, indentLevel);
        sb.Append('}');
    }
}

internal record NestedRule(string Selector, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"{Selector} {{ {Nodes.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        // Nested rules use & parent selector syntax
        AppendIndent(sb, indentLevel);
        sb.Append(Selector).Append(" {").AppendLine();

        foreach (var node in Nodes)
        {
            node.WriteCss(sb, indentLevel + 1);
            sb.AppendLine();
        }

        AppendIndent(sb, indentLevel);
        sb.Append('}');
    }
}

internal record AtRule(string Name, string Params, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"{Name} {Params} {{ {Nodes.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        AppendIndent(sb, indentLevel);
        sb.Append('@').Append(Name);
        if (!string.IsNullOrEmpty(Params))
        {
            sb.Append(' ').Append(Params);
        }

        if (Nodes.IsEmpty)
        {
            sb.Append(';');
            return;
        }

        sb.Append(" {").AppendLine();

        foreach (var node in Nodes)
        {
            node.WriteCss(sb, indentLevel + 1);
            sb.AppendLine();
        }

        AppendIndent(sb, indentLevel);
        sb.Append('}');
    }
}

internal record Comment(string Value) : AstNode
{
    public override string ToString() => $"/* {Value} */";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        AppendIndent(sb, indentLevel);
        sb.Append("/* ").Append(Value).Append(" */");
    }
}

internal record Context(ImmutableDictionary<string, object> Metadata, ImmutableList<AstNode> Nodes) : AstNode
{
    public override string ToString() => $"Context {{ {Nodes.Count} nodes, {Metadata.Count} metadata }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        // Separate children with a newline, no leading or trailing newline. (The old ToCss used a
        // fresh builder, so its `sb.Length > 0` guard meant "not the first child"; with a shared
        // buffer we track the index explicitly instead.)
        for (var i = 0; i < Nodes.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            Nodes[i].WriteCss(sb, indentLevel);
        }
    }
}

internal record RawCss(string Content) : AstNode
{
    public override string ToString() => $"RawCss {{ {Content.Length} chars }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        WriteCss(sb, indentLevel);
        return sb.ToString();
    }

    internal override void WriteCss(StringBuilder sb, int indentLevel = 0)
    {
        var lines = Content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        if (lines.Length == 1)
        {
            AppendIndent(sb, indentLevel);
            sb.Append(Content);
            return;
        }

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }

            // Non-blank lines get indented; blank interior lines are preserved as empty (the
            // newline above already emitted them) — matching the previous line-by-line behavior.
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                AppendIndent(sb, indentLevel);
                sb.Append(lines[i]);
            }
        }
    }
}
