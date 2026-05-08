using MonorailCss.Parser.Custom;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Custom;

/// <summary>
/// Demonstrates a custom utility with a nested selector. The <c>&amp;</c>
/// in <see cref="NestedSelector.Selector"/> is replaced by the parent
/// utility's selector at output time, which is exactly how WebKit
/// scrollbar pseudo-elements need to be addressed.
/// </summary>
public static class ScrollbarUtilitiesNested
{
    /// <summary>
    /// Builds a framework with a single <c>scrollbar-none</c> utility that
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
                Pattern = "scrollbar-none",
                Declarations = new CssDeclaration("scrollbar-width", "none"),
                NestedSelectors = new NestedSelector(
                    "&::-webkit-scrollbar",
                    new CssDeclaration("display", "none")),
            },
        });
    }
}
