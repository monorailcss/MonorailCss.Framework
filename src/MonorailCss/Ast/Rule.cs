using System.Collections.Immutable;
using System.Text;

namespace MonorailCss.Ast;

/// <summary>
/// Represents a CSS rule with a selector and declarations.
/// Used for utilities that target child elements like divide utilities.
/// </summary>
internal record Rule(string Selector, ImmutableList<AstNode> Children) : AstNode
{
    public override string ToString() => $"{Selector} {{ {Children.Count} nodes }}";

    public override string ToCss(int indentLevel = 0)
    {
        var sb = new StringBuilder();
        var indent = GetIndent(indentLevel);

        sb.AppendLine($"{indent}{Selector} {{");

        foreach (var child in Children)
        {
            sb.AppendLine(child.ToCss(indentLevel + 1));
        }

        sb.Append($"{indent}}}");

        return sb.ToString();
    }
}