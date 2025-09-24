using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask conic gradient utilities.
/// Handles: mask-conic-0, mask-conic-45, mask-conic-90, etc.
/// CSS: Sets mask-image with conic gradient and associated CSS custom properties.
/// </summary>
internal class MaskConicUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["mask-conic"];

    protected override string[] ThemeKeys => [];

    protected override bool SupportsNegative => true;

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle named values (angle values like 0, 45, 90, 180, etc.)
        if (value.Kind == ValueKind.Named)
        {
            if (int.TryParse(value.Value, out var angle))
            {
                // Always use calc format for consistency with Tailwind
                resolvedValue = isNegative ? $"calc(1deg * -{angle})" : $"calc(1deg * {angle})";
                return true;
            }
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Add the mask-image declaration with all three gradient variables
        declarations.Add(new Declaration("mask-image", "var(--tw-mask-linear), var(--tw-mask-radial), var(--tw-mask-conic)", important));

        // Add mask-composite
        declarations.Add(new Declaration("mask-composite", "intersect", important));

        // Add the CSS custom properties for the conic gradient
        declarations.Add(new Declaration("--tw-mask-conic", "conic-gradient(var(--tw-mask-conic-stops, var(--tw-mask-conic-position)))", important));
        declarations.Add(new Declaration("--tw-mask-conic-position", value, important));

        return declarations.ToImmutable();
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        // This utility doesn't support arbitrary values since it only handles angle numbers
        return false;
    }
}