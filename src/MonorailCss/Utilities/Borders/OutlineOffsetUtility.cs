using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the offset of an element's outline.
/// </summary>
internal class OutlineOffsetUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["outline-offset"];

    protected override string[] ThemeKeys => [];

    protected override bool SupportsNegative => true;

    /// <summary>
    /// Static mapping of numeric values to px values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticValues =
        new Dictionary<string, string>
        {
            ["0"] = "0px",
            ["1"] = "1px",
            ["2"] = "2px",
            ["4"] = "4px",
            ["8"] = "8px",
        }.ToImmutableDictionary();

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        if (value.Kind == ValueKind.Named)
        {
            if (_staticValues.TryGetValue(value.Value, out var pixelValue))
            {
                // Let NegativeValueNormalizationStage handle the negative format
                resolvedValue = pixelValue;
                return true;
            }
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("outline-offset", value, important));
    }
}