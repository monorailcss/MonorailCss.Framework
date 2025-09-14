using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utility for text-indent values.
/// Handles: indent-0, indent-px, indent-0.5, indent-1, indent-2, indent-4, indent-8, -indent-4, indent-[2rem], etc.
/// CSS: text-indent: 0, text-indent: 1px, text-indent: var(--spacing-1), text-indent: -var(--spacing-4), etc.
/// </summary>
internal class TextIndentUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["indent"];

    protected override string[] SpacingNamespaces => NamespaceResolver.TextIndentChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("text-indent", value, important));
    }
}