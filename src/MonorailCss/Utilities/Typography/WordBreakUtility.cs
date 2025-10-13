using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling word breaks.
/// </summary>
internal class WordBreakUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "break-words", ("overflow-wrap", "break-word") },
            { "break-all", ("word-break", "break-all") },
            { "break-keep", ("word-break", "keep-all") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        // Handle the special case of break-normal which sets multiple properties
        if (staticUtility.Root == "break-normal")
        {
            var declarations = ImmutableList.Create<AstNode>(
                new Declaration("overflow-wrap", "normal", candidate.Important),
                new Declaration("word-break", "normal", candidate.Important));
            results = declarations;
            return true;
        }

        // For other utilities, use the base implementation
        return base.TryCompile(candidate, theme, out results);
    }

    public override ImmutableHashSet<string> GetUtilityNames()
    {
        // Include break-normal in addition to the base static values
        return base.GetUtilityNames().ToImmutableHashSet().Add("break-normal");
    }
}