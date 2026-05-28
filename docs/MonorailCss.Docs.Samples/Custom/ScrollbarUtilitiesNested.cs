using MonorailCss.Parser.Custom;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Custom;

/// <summary>
/// Demonstrates a custom utility with a nested selector. The <c>&amp;</c>
/// in <see cref="NestedSelector.Selector"/> is replaced by the parent
/// utility's selector at output time. MonorailCss already ships
/// <c>scrollbar-none</c> as a single-property built-in, so the example uses
/// <c>scrollbar-hide</c> to also drop the WebKit pseudo-element — the
/// canonical "hide it in every browser" recipe.
/// </summary>
public static class ScrollbarUtilitiesNested
{
    /// <summary>
    /// Builds a framework with a single <c>scrollbar-hide</c> utility that
    /// hides the scrollbar in both Firefox (<c>scrollbar-width: none</c>)
    /// and WebKit browsers (<c>::-webkit-scrollbar { display: none; }</c>).
    /// </summary>
    public static CssFramework Build()
    {
        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            CustomUtilities = new UtilityDefinition
            {
                Pattern = "scrollbar-hide",
                Declarations = new CssDeclaration("scrollbar-width", "none"),
                NestedSelectors = new NestedSelector(
                    "&::-webkit-scrollbar",
                    new CssDeclaration("display", "none")),
            },
        });
    }
}
