using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles text overflow utilities (truncate, text-ellipsis, text-clip).
/// The truncate utility sets multiple properties for complete text truncation.
/// </summary>
internal class TextOverflowUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "text-ellipsis", ("text-overflow", "ellipsis") },
            { "text-clip", ("text-overflow", "clip") },
        }.ToImmutableDictionary();

    /// <summary>
    /// Generate CSS for text overflow utilities, handling truncate's multiple properties.
    /// </summary>
    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        // Handle truncate specially - it sets multiple properties
        if (staticUtility.Root == "truncate")
        {
            results = ImmutableList.Create<AstNode>(
                new Declaration("overflow", "hidden", candidate.Important),
                new Declaration("text-overflow", "ellipsis", candidate.Important),
                new Declaration("white-space", "nowrap", candidate.Important));
            return true;
        }

        // Handle regular static values
        if (StaticValues.TryGetValue(staticUtility.Root, out var cssDeclaration))
        {
            var declaration = new Declaration(
                cssDeclaration.Property,
                cssDeclaration.Value,
                candidate.Important);
            results = ImmutableList.Create<AstNode>(declaration);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all utility names this static utility handles, including truncate.
    /// </summary>
    public override ImmutableHashSet<string> GetUtilityNames()
    {
        return StaticValues.Keys.Append("truncate").ToImmutableHashSet();
    }
}