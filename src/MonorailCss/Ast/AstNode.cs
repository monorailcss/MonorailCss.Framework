using System.Collections.Immutable;
using System.Text;

namespace MonorailCss.Ast;

internal abstract record AstNode
{
    public SourceLocation? Src { get; init; }
    public SourceLocation? Dst { get; init; }

    public abstract string ToCss(int indentLevel = 0);

    protected static string GetIndent(int level) => new string(' ', level * 2);
}

internal record Declaration(string Property, string Value, bool Important = false) : AstNode
{
    public override string ToString() => $"{Property}: {Value}{(Important ? " !important" : string.Empty)}";

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
        var lines = Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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