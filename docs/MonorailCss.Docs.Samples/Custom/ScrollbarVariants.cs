using System.Collections.Immutable;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Custom;

/// <summary>
/// Registers custom variants for the WebKit scrollbar pseudo-elements
/// so any utility can target them via <c>scrollbar:</c>,
/// <c>scrollbar-thumb:</c>, or <c>scrollbar-track:</c>. Pair with
/// background, border, or radius utilities, e.g.
/// <c>scrollbar-thumb:bg-blue-500</c>.
/// </summary>
public static class ScrollbarVariants
{
    /// <summary>
    /// Builds a framework with three pseudo-element variants. The
    /// <see cref="CustomVariantDefinition.Weight"/> controls output order;
    /// 490 places these just before MonorailCss's built-in pseudo-element
    /// variants (which sit at 500+).
    /// </summary>
    public static CssFramework Build()
    {
        var customVariants = ImmutableList.Create(
            new CustomVariantDefinition
            {
                Name = "scrollbar",
                Selector = "&::-webkit-scrollbar",
                Weight = 490,
            },
            new CustomVariantDefinition
            {
                Name = "scrollbar-thumb",
                Selector = "&::-webkit-scrollbar-thumb",
                Weight = 491,
            },
            new CustomVariantDefinition
            {
                Name = "scrollbar-track",
                Selector = "&::-webkit-scrollbar-track",
                Weight = 492,
            });

        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            CustomVariants = customVariants,
        });
    }
}
