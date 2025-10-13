using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the color of prose content.
/// </summary>
internal class ProseColorUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public UtilityLayer Layer => UtilityLayer.Component;

    private static readonly HashSet<string> _colorThemes =
    [
        "slate", "gray", "zinc", "neutral", "stone",
        "red", "orange", "amber", "yellow", "lime", "green", "emerald",
        "teal", "cyan", "sky", "blue", "indigo", "violet", "purple",
        "fuchsia", "pink", "rose",
    ];

    private static readonly HashSet<string> _fullColorThemes = ["slate", "gray", "zinc", "neutral", "stone"];

    public string[] GetNamespaces() => ["--typography-color"];

    public string[] GetFunctionalRoots() => ["prose"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility { Root: "prose" } functional || functional.Value == null)
        {
            return false;
        }

        var colorTheme = functional.Value.Value;
        if (!_colorThemes.Contains(colorTheme))
        {
            return false;
        }

        // Generate only color variable declarations
        var declarations = MapColorVariables(colorTheme, theme).ToList();
        if (declarations.Count == 0)
        {
            return false;
        }

        results = declarations.Cast<AstNode>().ToImmutableList();
        return true;
    }

    private IEnumerable<Declaration> MapColorVariables(string colorTheme, Theme.Theme theme)
    {
        var declarations = new List<Declaration>();

        // Map the color theme to CSS variables
        if (_fullColorThemes.Contains(colorTheme))
        {
            // Full color theme - map all color properties
            var properties = new[]
            {
                "body", "headings", "lead", "links", "bold", "counters",
                "bullets", "hr", "quotes", "quote-borders", "captions",
                "kbd", "kbd-shadows", "code", "pre-code", "pre-bg",
                "th-borders", "td-borders",
            };

            foreach (var prop in properties)
            {
                var value = theme.ResolveValue(
                    colorTheme == "gray" ?
                        $"--typography-color-{prop}" :
                        $"--typography-color-{colorTheme}-{prop}",
                    ["--typography-color"]);
                if (value != null)
                {
                    var resolvedValue = ResolveColorValue(value, theme);
                    declarations.Add(new Declaration($"--tw-prose-{prop}", resolvedValue));
                }
            }
        }
        else
        {
            // Link-only color theme - only affects link color
            var linkValue = theme.ResolveValue(
                $"--typography-color-{colorTheme}-links",
                ["--typography-color"]);
            if (linkValue != null)
            {
                var resolvedValue = ResolveColorValue(linkValue, theme);
                declarations.Add(new Declaration($"--tw-prose-links", resolvedValue));
            }
        }

        return declarations;
    }

    private string ResolveColorValue(string value, Theme.Theme theme)
    {
        // If the value contains var() references, recursively resolve them
        if (value.StartsWith("var(--") && value.EndsWith(")"))
        {
            // Extract the variable name
            var varName = value.Substring(4, value.Length - 5); // Remove "var(" and ")"

            // Try to resolve this variable from the theme
            var resolvedValue = theme.ResolveValue(varName, []);
            if (resolvedValue != null)
            {
                // Recursively resolve if this also contains var()
                return ResolveColorValue(resolvedValue, theme);
            }
        }

        return value;
    }

    /// <summary>
    /// Returns examples of prose color utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("prose-slate", "Apply slate color theme to prose"),
            new("prose-gray", "Apply gray color theme to prose"),
            new("prose-zinc", "Apply zinc color theme to prose"),
            new("prose-red", "Apply red color theme to prose links"),
            new("prose-blue", "Apply blue color theme to prose links"),
        };

        return examples;
    }
}