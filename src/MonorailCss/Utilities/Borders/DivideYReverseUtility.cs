using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utility for reversing vertical divide direction (divide-y-reverse).
/// Sets --tw-divide-y-reverse: 1 on child elements.
/// </summary>
internal class DivideYReverseUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        ImmutableDictionary<string, (string Property, string Value)>.Empty
            .Add("divide-y-reverse", ("--tw-divide-y-reverse", "1"));

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for vertical divide reverse
        propertyRegistry.Register("--tw-divide-y-reverse", "*", false, "0");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }

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

        // Create child selector with the CSS variable declaration
        var childSelector = ":where(& > :not(:last-child))";
        var declarations = ImmutableList.Create<AstNode>(
            new Declaration(cssDeclaration.Property, cssDeclaration.Value, candidate.Important));

        results = ImmutableList.Create<AstNode>(new NestedRule(childSelector, declarations));
        return true;
    }
}