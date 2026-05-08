using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Custom;

/// <summary>
/// Adds a small family of scrollbar utilities. Two static utilities
/// (<c>scrollbar-thin</c> and <c>scrollbar-color</c>) plus two wildcard
/// ones (<c>scrollbar-thumb-*</c>, <c>scrollbar-track-*</c>) that pick up
/// theme colors via <c>--value(--color-*)</c>.
/// </summary>
public static class ScrollbarUtilities
{
    /// <summary>
    /// Builds a framework with the four scrollbar utilities wired up.
    /// Use them like <c>scrollbar-thin scrollbar-thumb-blue-500
    /// scrollbar-track-slate-200 scrollbar-color</c>.
    /// </summary>
    public static CssFramework Build()
    {
        ImmutableList<UtilityDefinition> customUtilities =
        [
            new UtilityDefinition
            {
                Pattern = "scrollbar-thin",
                Declarations = new CssDeclaration("scrollbar-width", "thin"),
            },
            new UtilityDefinition
            {
                Pattern = "scrollbar-thumb-*",
                IsWildcard = true,
                Declarations = new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)"),
            },
            new UtilityDefinition
            {
                Pattern = "scrollbar-track-*",
                IsWildcard = true,
                Declarations = new CssDeclaration("--tw-scrollbar-track-color", "--value(--color-*)"),
            },
            new UtilityDefinition
            {
                Pattern = "scrollbar-color",
                Declarations = new CssDeclaration(
                    "scrollbar-color",
                    "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)"),
            },
        ];

        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            CustomUtilities = customUtilities,
        });
    }
}
