using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for reversing the direction of horizontal borders between elements.
/// </summary>
internal class DivideXReverseUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        ImmutableDictionary<string, (string Property, string Value)>.Empty
            .Add("divide-x-reverse", ("--tw-divide-x-reverse", "1"));

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (!StaticValues.TryGetValue(staticUtility.Root, out _))
        {
            return false;
        }

        // Register CSS variables for horizontal divide reverse
        propertyRegistry.Register("--tw-divide-x-reverse", "*", false, "0");

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