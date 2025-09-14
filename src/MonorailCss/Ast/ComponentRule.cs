using System.Collections.Immutable;

namespace MonorailCss.Ast;

/// <summary>
/// Represents a component with multiple child rules that need special selector handling.
/// Used by utilities like prose that need to generate styles for child elements.
/// </summary>
internal record ComponentRule(
    ImmutableList<Declaration> BaseDeclarations,
    ImmutableList<ChildRule> ChildRules) : AstNode
{
    public override string ToString() =>
        $"ComponentRule(base: {BaseDeclarations.Count} declarations, children: {ChildRules.Count} rules)";

    public override string ToCss(int indentLevel = 0)
    {
        // This will be handled by the post-processor
        throw new NotSupportedException("ComponentRule must be processed by PostProcessor");
    }
}

/// <summary>
/// Represents a child element rule within a component.
/// </summary>
internal record ChildRule(
    string ChildSelector,  // e.g., "p", "h1", ":where(h1, h2, h3)"
    ImmutableList<Declaration> Declarations,
    bool UseWhereWrapper = true,  // Whether to wrap in :where()
    string? ExcludeClass = "not-prose") // Class to exclude (null = no exclusion)
: AstNode
{
    public override string ToString() =>
        $"ChildRule({ChildSelector}: {Declarations.Count} declarations)";

    public override string ToCss(int indentLevel = 0)
    {
        // This will be handled by the post-processor
        throw new NotSupportedException("ChildRule must be processed by PostProcessor");
    }
}