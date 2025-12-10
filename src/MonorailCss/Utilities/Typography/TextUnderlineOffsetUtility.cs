using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the offset of text underlines.
/// </summary>
internal class TextUnderlineOffsetUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["underline-offset"];

    protected override string[] ThemeKeys => [];

    protected override bool SupportsNegative => true;

    /// <summary>
    /// Static mapping of values to pixel values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticValues =
        new Dictionary<string, string>
        {
            ["auto"] = "auto",
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
            new Declaration("text-underline-offset", value, important));
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "text-underline-offset: [value]";
}