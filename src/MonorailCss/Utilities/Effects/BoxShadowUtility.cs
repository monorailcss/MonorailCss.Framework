using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the box shadow of an element.
/// </summary>
internal class BoxShadowUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["shadow"];

    protected override string[] ThemeKeys => ["--shadow"];

    protected override string DefaultValue => _builtInShadows["sm"];

    /// <summary>
    /// Hardcoded Tailwind v4 default shadow scale. Used as fallbacks for the bare keywords
    /// (<c>shadow-sm</c>, <c>shadow-md</c>, etc.) so they keep working without any theme
    /// configuration. Theme entries (<c>--shadow-<i>name</i></c>) take precedence — see
    /// <see cref="HandleBareValue"/>.
    /// </summary>
    private static readonly Dictionary<string, string> _builtInShadows = new()
    {
        ["none"] = "0 0 #0000",
        ["inner"] = "inset 0 2px 4px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.05))",
        ["sm"] = "0 1px 3px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 1px 2px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
        ["md"] = "0 4px 6px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 2px 4px -2px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
        ["lg"] = "0 10px 15px -3px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 4px 6px -4px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
        ["xl"] = "0 20px 25px -5px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 8px 10px -6px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
        ["2xl"] = "0 25px 50px -12px var(--tw-shadow-color, rgb(0 0 0 / 0.25))",
    };

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-shadow", value, important),
            new Declaration("box-shadow", "var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)", important));
    }

    protected override string? HandleBareValue(string value)
    {
        // Built-in keywords always resolve to their hardcoded values so the default scale
        // works without any theme keys. The theme namespace (`--shadow-*`) is consulted by
        // BaseFunctionalUtility.TryResolveValue when this returns null, letting users add
        // (or override) named shadows via `@theme { --shadow-foo: …; }`.
        return _builtInShadows.GetValueOrDefault(value);
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        if (isNegative)
        {
            return false;
        }

        // Custom validation path for arbitrary values: a bare color token like `[red]`,
        // `[#0088cc]`, or `(color:--c)` belongs to ShadowColorUtility. Everything else — including
        // a hint-less parens shorthand `(--my-shadow)` or `[var(--x)]` — is a shadow value.
        // ShadowValueResolver keeps this split in sync with ShadowColorUtility's rejection so the
        // two utilities never both claim (or both drop) the same candidate.
        if (value.Kind == ValueKind.Arbitrary)
        {
            if (ShadowValueResolver.IsArbitraryShadowValue(value))
            {
                resolvedValue = value.Value;
                return true;
            }

            return false;
        }

        return base.TryResolveValue(value, theme, isNegative, out resolvedValue);
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid box-shadow value (not just a color).
    /// </summary>
    private static bool IsValidBoxShadowValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();

        // CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(trimmed))
        {
            return true;
        }

        // A single color token (`#0088cc`, `red`, `rgb(...)`, `hsl(...)`) belongs
        // to ShadowColorUtility, not the box-shadow definition path. Detect by
        // looking for offset/blur tokens — a real box-shadow value has spaces
        // separating multiple tokens or starts with `inset`.
        if (trimmed.StartsWith("inset "))
        {
            return true;
        }

        // Multi-token values (offsets, blur, spread, color) are box-shadow defs.
        if (trimmed.Contains(' '))
        {
            return true;
        }

        // Single-token CSS function: opaque, defer to ShadowColorUtility unless
        // it looks like a shadow keyword.
        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidBoxShadowValue(value);
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "box-shadow: [value]";
}