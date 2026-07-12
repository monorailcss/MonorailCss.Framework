using System.Collections.Immutable;

namespace MonorailCss.Docs.Client;

/// <summary>
/// Builds the CssFramework the playground uses to compile the user's classes in the browser.
/// The palettes (sanibel/frosted/mixed/happily), the semantic primary/accent/base mapping,
/// and the font families mirror the host's registration in
/// <c>docs/MonorailCss.Docs/Program.cs</c> so previews render in the same colour scheme and
/// type as the rest of the docs site.
///
/// The host uses Pennington's <c>NamedColorScheme.ApplyToTheme</c> for the semantic mapping,
/// which is a server-side package we don't want in the WASM client — so we reproduce the same
/// effect with core <see cref="Theme.Theme.MapColorPalette"/> calls
/// (--color-primary-* -> var(--color-sanibel-*), etc.). Keep the two in sync if either changes.
/// </summary>
internal static class DocsTheme
{
    /// <summary>
    /// A CssFramework themed to match the docs site. <paramref name="includePreflight"/> is on
    /// for the preview iframe (a clean reset, Tailwind-Play style) and off for the "generated
    /// CSS" pane and the per-class hover, which should show only the utilities the classes emit.
    /// </summary>
    public static CssFramework CreateFramework(bool includePreflight) => new(new CssFrameworkSettings
    {
        Theme = ApplyDocsTheme(Theme.Theme.CreateWithDefaults()),
        IncludePreflight = includePreflight,
    });

    private static Theme.Theme ApplyDocsTheme(Theme.Theme baseTheme) => baseTheme
        .AddColorPalette("sanibel", Sanibel)
        .AddColorPalette("frosted", Frosted)
        .AddColorPalette("mixed", Mixed)
        .AddColorPalette("happily", Happily)
        // Semantic aliases — the same mapping NamedColorScheme applies on the host.
        .MapColorPalette("sanibel", "primary")
        .MapColorPalette("mixed", "accent")
        .MapColorPalette("frosted", "base")
        .MapColorPalette("happily", "tertiary-one")
        .AddFontFamily("display", "'Jost', -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif")
        .AddFontFamily("sans", "'Nunito', -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif")
        .AddFontFamily("mono", "'JetBrains Mono', ui-monospace, 'SF Mono', Menlo, monospace");

    private static readonly ImmutableDictionary<string, string> Sanibel = new Dictionary<string, string>
    {
        { "50",  "oklch(97% 0.007 52.073)" },
        { "100", "oklch(94% 0.017 52.073)" },
        { "200", "oklch(89% 0.032 52.073)" },
        { "300", "oklch(82% 0.058 52.073)" },
        { "400", "oklch(72% 0.094 52.073)" },
        { "500", "oklch(64% 0.123 52.073)" },
        { "600", "oklch(56% 0.129 52.073)" },
        { "700", "oklch(49% 0.118 52.073)" },
        { "800", "oklch(43% 0.096 52.073)" },
        { "900", "oklch(38% 0.076 52.073)" },
        { "950", "oklch(27% 0.052 52.073)" },
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Frosted = new Dictionary<string, string>
    {
        { "50",  "oklch(98.5% 0.003 52.073)" },
        { "100", "oklch(97% 0.004 52.073)" },
        { "200", "oklch(92.5% 0.007 52.073)" },
        { "300", "oklch(87% 0.014 52.073)" },
        { "400", "oklch(71% 0.026 52.073)" },
        { "500", "oklch(55% 0.040 52.073)" },
        { "600", "oklch(44% 0.036 52.073)" },
        { "700", "oklch(37% 0.034 52.073)" },
        { "800", "oklch(27% 0.029 52.073)" },
        { "900", "oklch(21% 0.025 52.073)" },
        { "950", "oklch(14% 0.020 52.073)" },
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Mixed = new Dictionary<string, string>
    {
        { "50",  "oklch(97% 0.007 22.073)" },
        { "100", "oklch(94% 0.017 22.073)" },
        { "200", "oklch(89% 0.032 22.073)" },
        { "300", "oklch(82% 0.058 22.073)" },
        { "400", "oklch(72% 0.094 22.073)" },
        { "500", "oklch(64% 0.123 22.073)" },
        { "600", "oklch(56% 0.129 22.073)" },
        { "700", "oklch(49% 0.118 22.073)" },
        { "800", "oklch(43% 0.096 22.073)" },
        { "900", "oklch(38% 0.076 22.073)" },
        { "950", "oklch(27% 0.052 22.073)" },
    }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<string, string> Happily = new Dictionary<string, string>
    {
        { "50",  "oklch(97% 0.007 82.073)" },
        { "100", "oklch(94% 0.017 82.073)" },
        { "200", "oklch(89% 0.032 82.073)" },
        { "300", "oklch(82% 0.058 82.073)" },
        { "400", "oklch(72% 0.094 82.073)" },
        { "500", "oklch(64% 0.123 82.073)" },
        { "600", "oklch(56% 0.129 82.073)" },
        { "700", "oklch(49% 0.118 82.073)" },
        { "800", "oklch(43% 0.096 82.073)" },
        { "900", "oklch(38% 0.076 82.073)" },
        { "950", "oklch(27% 0.052 82.073)" },
    }.ToImmutableDictionary();
}
