using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for clamping text to a specific number of lines.
/// </summary>
internal class LineClampUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["line-clamp"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        if (value == "none")
        {
            // line-clamp-none: disable line clamping
            declarations.Add(new Declaration("overflow", "visible", important));
            declarations.Add(new Declaration("display", "block", important));
            declarations.Add(new Declaration("-webkit-box-orient", "horizontal", important));
            declarations.Add(new Declaration("-webkit-line-clamp", "unset", important));
        }
        else
        {
            // line-clamp-{n}: enable line clamping with n lines
            declarations.Add(new Declaration("overflow", "hidden", important));
            declarations.Add(new Declaration("display", "-webkit-box", important));
            declarations.Add(new Declaration("-webkit-box-orient", "vertical", important));
            declarations.Add(new Declaration("-webkit-line-clamp", value, important));
        }

        return declarations.ToImmutable();
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle special static value
            if (key == "none")
            {
                resolvedValue = "none";
                return true;
            }

            // Handle numeric values (line-clamp-1, line-clamp-2, etc.)
            if (int.TryParse(key, out var numValue) && numValue >= 1)
            {
                resolvedValue = numValue.ToString();
                return true;
            }
        }

        // Handle arbitrary values (line-clamp-[7])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Check if it's a valid number
            if (int.TryParse(arbitrary, out var arbitraryValue) && arbitraryValue >= 1)
            {
                resolvedValue = arbitraryValue.ToString();
                return true;
            }
        }

        // No theme resolution needed for line-clamp
        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        // Only allow positive integers
        return int.TryParse(value, out var numValue) && numValue >= 1;
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "-webkit-line-clamp: [value]";
}