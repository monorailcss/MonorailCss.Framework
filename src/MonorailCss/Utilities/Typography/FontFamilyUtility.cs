using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.DataTypes;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the font family of an element.
/// </summary>
internal class FontFamilyUtility : BaseFunctionalUtility
{
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    protected override string[] Patterns => ["font"];
    protected override string[] ThemeKeys => ["--font"];

    /// <summary>
    /// Routes arbitrary values: only family/generic values become a font-family.
    /// Numbers, var()/calc() and other type hints fall through (return false) so
    /// <see cref="FontWeightUtility"/> handles them as font-weight — matching Tailwind,
    /// where `font` infers ['number','generic-name','family-name'] and only the last
    /// two map to font-family.
    /// </summary>
    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        if (value.Kind == ValueKind.Arbitrary)
        {
            resolvedValue = string.Empty;
            if (!AcceptsAsFamily(value))
            {
                return false;
            }

            resolvedValue = value.Value;
            return true;
        }

        return base.TryResolveValue(value, theme, isNegative, out resolvedValue);
    }

    /// <summary>
    /// Determines whether an arbitrary value should be treated as a font-family.
    /// </summary>
    private static bool AcceptsAsFamily(CandidateValue value)
    {
        if (value.DataTypeHint is "family-name" or "generic-name")
        {
            return true;
        }

        // Any other explicit hint (length, number, color, …) belongs to a sibling utility.
        if (value.DataTypeHint != null)
        {
            return false;
        }

        // No hint: infer. var()/calc() resolve to null here, so they fall through
        // to font-weight just like bare numbers do.
        var inferred = DataTypeInference.InferDataType(
            value.Value,
            [DataType.Number, DataType.GenericName, DataType.FamilyName]);
        return inferred is DataType.FamilyName or DataType.GenericName;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("font-family", value, important));
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "font-family: [value]";
}
