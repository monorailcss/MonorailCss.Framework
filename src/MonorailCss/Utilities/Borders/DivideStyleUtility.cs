using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the border style between elements.
/// </summary>
internal class DivideStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["divide-solid"] = ("border-style", "solid"),
            ["divide-dashed"] = ("border-style", "dashed"),
            ["divide-dotted"] = ("border-style", "dotted"),
            ["divide-double"] = ("border-style", "double"),
            ["divide-none"] = ("border-style", "none"),
        });

    /// <summary>
    /// Override TryCompile to wrap declarations in child selector and set both CSS variable and property.
    /// </summary>
    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (!StaticValues.TryGetValue(staticUtility.Root, out var cssDeclaration))
        {
            return false;
        }

        // Create both the CSS variable and the property declarations
        var declarations = new List<AstNode>
        {
            new Declaration("--tw-border-style", cssDeclaration.Value, candidate.Important),
            new Declaration("border-style", cssDeclaration.Value, candidate.Important),
        };

        // Wrap in child selector
        var childSelector = ":where(& > :not(:last-child))";
        results = ImmutableList.Create<AstNode>(new NestedRule(childSelector, declarations.ToImmutableList()));
        return true;
    }
}