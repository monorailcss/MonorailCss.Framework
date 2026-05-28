using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Custom;

/// <summary>
/// MonorailCss ships scrollbar-thin/none/thumb-*/track-*/gutter-* as built-in
/// utilities, but the underlying recipe — wildcard prefixes plus a shared
/// composing utility — is still the canonical way to build any other family
/// of related utilities. This sample re-implements the same shape under the
/// <c>my-scrollbar-*</c> prefix so it doesn't shadow the built-ins.
/// </summary>
public static class ScrollbarUtilities
{
    /// <summary>
    /// Builds a framework with a four-utility set demonstrating composition
    /// via shared CSS variables. Use like
    /// <c>my-scrollbar-tiny my-scrollbar-thumb-blue-500 my-scrollbar-track-slate-200 my-scrollbar-color</c>.
    /// </summary>
    public static CssFramework Build()
    {
        ImmutableList<UtilityDefinition> customUtilities =
        [
            new UtilityDefinition
            {
                Pattern = "my-scrollbar-tiny",
                Declarations = new CssDeclaration("scrollbar-width", "thin"),
            },
            new UtilityDefinition
            {
                Pattern = "my-scrollbar-thumb-*",
                IsWildcard = true,
                Declarations = new CssDeclaration("--my-scrollbar-thumb-color", "--value(--color-*)"),
            },
            new UtilityDefinition
            {
                Pattern = "my-scrollbar-track-*",
                IsWildcard = true,
                Declarations = new CssDeclaration("--my-scrollbar-track-color", "--value(--color-*)"),
            },
            new UtilityDefinition
            {
                Pattern = "my-scrollbar-color",
                Declarations = new CssDeclaration(
                    "scrollbar-color",
                    "var(--my-scrollbar-thumb-color) var(--my-scrollbar-track-color)"),
            },
        ];

        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            CustomUtilities = customUtilities,
        });
    }
}
