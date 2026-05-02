using System.Collections.Immutable;
using MonorailCss.Theme;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Prose;

/// <summary>
/// Customizes the typography (<c>prose</c>) plugin. The
/// <see cref="ProseCustomization.Customization"/> delegate receives the
/// theme and returns a per-modifier map of element rules; modifier keys
/// are <c>"DEFAULT"</c>, <c>"base"</c>, <c>"sm"</c>, <c>"lg"</c>,
/// <c>"xl"</c>, <c>"2xl"</c>, and <c>"invert"</c>.
/// </summary>
public static class BoldLinks
{
    /// <summary>
    /// Makes every link inside a <c>prose</c> block bold and underlined,
    /// and bumps blockquote font-weight on the <c>lg</c> size variant.
    /// </summary>
    public static CssFramework Build()
    {
        var customization = new ProseCustomization
        {
            Customization = _ => new Dictionary<string, ProseElementRules>
            {
                ["DEFAULT"] = new()
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "a",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-weight", Value = "700" },
                                new ProseDeclaration { Property = "text-decoration-line", Value = "underline" }),
                        }),
                },
                ["lg"] = new()
                {
                    Rules = ImmutableList.Create(
                        new ProseElementRule
                        {
                            Selector = "blockquote",
                            Declarations = ImmutableList.Create(
                                new ProseDeclaration { Property = "font-weight", Value = "600" }),
                        }),
                },
            }.ToImmutableDictionary(),
        };

        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            ProseCustomization = customization,
        });
    }
}
