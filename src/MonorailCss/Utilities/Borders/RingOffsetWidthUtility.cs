using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.DataTypes;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles ring-offset-width utilities.
///
/// Width patterns: ring-offset-0, ring-offset-1, ring-offset-2, ring-offset-4, ring-offset-8
/// Arbitrary: ring-offset-[10px]
///
/// Sets the --tw-ring-offset-width CSS variable.
/// </summary>
internal class RingOffsetWidthUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["ring-offset"];

    protected override string[] SpacingNamespaces => NamespaceResolver.RingOffsetWidthChain;

    protected override bool TryResolveSpacing(CandidateValue value, Theme.Theme theme, out string spacing)
    {
        spacing = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;
            var inferredType = DataTypeInference.InferDataType(arbitrary, [DataType.Length, DataType.Percentage]);

            if (inferredType is DataType.Length or DataType.Percentage)
            {
                spacing = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Direct pixel values for common ring offset widths
            spacing = key switch
            {
                "0" => "0px",
                "1" => "1px",
                "2" => "2px",
                "4" => "4px",
                "8" => "8px",
                _ => string.Empty,
            };

            if (!string.IsNullOrEmpty(spacing))
            {
                return true;
            }

            // Fall back to base implementation for other values
            return base.TryResolveSpacing(value, theme, out spacing);
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-ring-offset-width", value, important),
            new Declaration("--tw-ring-offset-shadow", $"var(--tw-ring-inset,) 0 0 0 var(--tw-ring-offset-width) var(--tw-ring-offset-color)", important));
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Register default value for ring offset width
        propertyRegistry.Register("--tw-ring-offset-width", "<length>", false, "0px");

        return GenerateDeclarations(pattern, value, important);
    }
}