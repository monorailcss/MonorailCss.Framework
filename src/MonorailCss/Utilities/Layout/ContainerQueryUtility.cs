using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for creating container query contexts (@container, @container-normal, @container-size).
/// Each supports an optional `/name` modifier that emits a paired container-name declaration.
/// </summary>
internal class ContainerQueryUtility : BaseStaticUtility
{
    private static readonly ImmutableDictionary<string, string> ContainerTypes =
        new Dictionary<string, string>
        {
            { "@container", "inline-size" },
            { "@container-normal", "normal" },
            { "@container-size", "size" },
        }.ToImmutableDictionary();

    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        ContainerTypes.ToImmutableDictionary(kvp => kvp.Key, kvp => ("container-type", kvp.Value));

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (!ContainerTypes.TryGetValue(staticUtility.Root, out var containerType))
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        declarations.Add(new Declaration("container-type", containerType, candidate.Important));

        // A named modifier (e.g. `@container-size/sidebar`) emits a paired
        // container-name declaration. Arbitrary modifiers are not supported by
        // Tailwind for container utilities.
        if (candidate.Modifier is { Kind: ModifierKind.Named, Value: var name } && !string.IsNullOrEmpty(name))
        {
            declarations.Add(new Declaration("container-name", name, candidate.Important));
        }

        results = declarations.ToImmutable();
        return true;
    }
}
