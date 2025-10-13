using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the indentation of text.
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