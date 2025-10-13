using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling whether the user can select text.
/// </summary>
internal class UserSelectUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "select-none", ("user-select", "none") },
            { "select-text", ("user-select", "text") },
            { "select-all", ("user-select", "all") },
            { "select-auto", ("user-select", "auto") },
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

        // Generate both vendor-prefixed and standard properties
        var declarations = new List<AstNode>
        {
            new Declaration("-webkit-user-select", cssDeclaration.Value, candidate.Important),
            new Declaration("user-select", cssDeclaration.Value, candidate.Important),
        };

        results = declarations.ToImmutableList();
        return true;
    }
}