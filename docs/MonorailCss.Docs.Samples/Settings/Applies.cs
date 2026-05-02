using System.Collections.Immutable;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Settings;

/// <summary>
/// Bundles utility classes into reusable component selectors. Each
/// dictionary entry maps a CSS selector to a space-separated list of
/// utilities; variants like <c>hover:</c> and <c>focus:</c> are honored
/// inside that list. The expanded rules land in the
/// <c>@layer components</c> bucket so utility classes still win when
/// applied directly to an element.
/// </summary>
public static class Applies
{
    /// <summary>
    /// Defines a small set of component classes &#8212; <c>.btn</c>,
    /// <c>.btn-primary</c>, <c>.card</c> &#8212; alongside a global
    /// <c>body</c> rule.
    /// </summary>
    public static CssFramework Build()
    {
        var applies = new Dictionary<string, string>
        {
            { "body", "font-sans text-gray-900 antialiased" },
            { ".btn", "px-4 py-2 rounded-lg font-semibold transition" },
            { ".btn-primary", "bg-blue-500 text-white hover:bg-blue-600" },
            { ".card", "bg-white shadow-lg rounded-xl p-6" },
        }.ToImmutableDictionary();

        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            Applies = applies,
        });
    }
}
