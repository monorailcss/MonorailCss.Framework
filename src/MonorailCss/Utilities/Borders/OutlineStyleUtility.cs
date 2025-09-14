using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles outline style utilities (outline-none, outline-dashed, etc.).
/// Maps to the CSS outline-style property and manages CSS custom properties.
/// </summary>
internal class OutlineStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "outline-none", ("outline-style", "none") },
            { "outline-dashed", ("outline-style", "dashed") },
            { "outline-dotted", ("outline-style", "dotted") },
            { "outline-double", ("outline-style", "double") },
        }.ToImmutableDictionary();

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

        var declarations = new List<AstNode>();

        // Handle outline-none specially - it sets both custom property and direct property
        if (staticUtility.Root == "outline-none")
        {
            declarations.Add(new Declaration("--tw-outline-style", "none", candidate.Important));
            declarations.Add(new Declaration("outline-style", "none", candidate.Important));
        }
        else
        {
            // For other styles, set both the custom property and the direct property
            declarations.Add(new Declaration("--tw-outline-style", cssDeclaration.Value, candidate.Important));
            declarations.Add(new Declaration("outline-style", cssDeclaration.Value, candidate.Important));
        }

        results = declarations.ToImmutableList();
        return true;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
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

        // Register the outline style custom property
        propertyRegistry.Register("--tw-outline-style", "*", false, "solid");

        var declarations = new List<AstNode>();

        // Handle outline-none specially
        if (staticUtility.Root == "outline-none")
        {
            declarations.Add(new Declaration("--tw-outline-style", "none", candidate.Important));
            declarations.Add(new Declaration("outline-style", "none", candidate.Important));
        }
        else
        {
            // For other styles, set both the custom property and the direct property
            declarations.Add(new Declaration("--tw-outline-style", cssDeclaration.Value, candidate.Important));
            declarations.Add(new Declaration("outline-style", cssDeclaration.Value, candidate.Important));
        }

        results = declarations.ToImmutableList();
        return true;
    }
}