using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for spacing utilities that need to handle fraction values correctly.
/// This class extracts the full raw value from the candidate to preserve fractions
/// that get truncated during parsing.
/// </summary>
internal abstract class BaseFractionAwareSpacingUtility : BaseSpacingUtility
{
    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        // Check if it's a negative utility
        var isNegative = functionalUtility.Root.StartsWith('-');
        var basePattern = isNegative ? functionalUtility.Root[1..] : functionalUtility.Root;

        if (!Patterns.Contains(basePattern))
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        if (!TryResolveSpacing(functionalUtility.Value, theme, out var spacing))
        {
            return false;
        }

        // Pass the base pattern but keep the candidate for negative detection
        var declarations = GenerateDeclarations(basePattern, spacing, candidate.Important);
        if (declarations.Count == 0)
        {
            return false;
        }

        results = declarations;
        return true;
    }
}