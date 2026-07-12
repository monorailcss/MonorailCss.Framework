using System.Collections.Immutable;
using Beck;
using MonorailCss;
using MonorailCss.Docs;
using MonorailCss.Docs.Components;
using MonorailCss.Docs.Services;
using MonorailCss.Theme;
using Pennington.ApiMetadata;
using Pennington.ApiMetadata.Reflection;
using Pennington.Beck;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.LlmsTxt;
using Pennington.MonorailCss;
using Pennington.SocialCards;
using Pennington.TreeSitter;

var builder = WebApplication.CreateBuilder(args);

// Static SSR for the whole site PLUS interactive-WebAssembly components: the /playground
// island (MonorailCss.Docs.Client) compiles utility classes to CSS entirely in the browser
// via MonorailCss compiled to WASM. Every other page stays static SSR, so the site still
// deploys as static files.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var colorScheme = new NamedColorScheme
{
    PrimaryColorName = "sanibel",
    AccentColorName = "mixed",
    BaseColorName = "frosted",
    AdditionalMappings = new Dictionary<string, ColorName>
    {
        ["tertiary-one"] = "happily",
    },
};

Theme ApplyDocsTheme(Theme baseTheme) => baseTheme
    .AddColorPalette("sanibel", new Dictionary<string, string>
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
    }.ToImmutableDictionary())
    .AddColorPalette("frosted", new Dictionary<string, string>
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
    }.ToImmutableDictionary())
    .AddColorPalette("mixed", new Dictionary<string, string>
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
    }.ToImmutableDictionary())
    .AddColorPalette("happily", new Dictionary<string, string>
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
    }.ToImmutableDictionary())
    .AddFontFamily("display", "'Jost', -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif")
    .AddFontFamily("sans", "'Nunito', -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif")
    .AddFontFamily("mono", "'JetBrains Mono', ui-monospace, 'SF Mono', Menlo, monospace");

// Bare AddPennington host: we own the layout, routes, and chrome (custom App.razor +
// MainLayout), so we take the core engine directly rather than the DocSite template.
// DocSite used to register the markdown content source for us from DocSiteOptions;
// here we register it explicitly.
builder.Services.AddPennington(options =>
{
    options.SiteTitle = "MonorailCss Documentation";
    options.SiteDescription = "A JIT CSS compiler that aims to be Tailwind CSS 4.3 compatible, written in .NET";
    options.ContentRootPath = new Pennington.Routing.FilePath("Content");

    // The docs pages: Content/**/*.md bound to the core DocFrontMatter shape, which
    // carries every key this content uses (title/description/order/uid/tags).
    options.AddMarkdownContent<DocFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
    });

    options.SocialCards = new SocialCardOptions
    {
        Render = (request, sp, _) =>
        {
            var environment = sp.GetRequiredService<IWebHostEnvironment>();
            return SocialCardGenerator.Build(request, environment);
        },
    };

    // /llms.txt + /_llms/*.md sidecars. On a bare host this is opt-in (AddDocSite
    // used to turn it on for us). The /utility/ subtree split is layered on below
    // via AddLlmsSubtree, and Home.razor links to both.
    options.AddLlmsTxt(_ => { });
});

// Single, first-class MonorailCss registration. Installs the docs color palettes and
// ColorEmissionMode.All — what the DocSiteOptions.ColorScheme slot couldn't express,
// which is why this used to be an awkward post-AddDocSite override.
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = colorScheme,
    CustomCssFrameworkSettings = settings => settings with
    {
        Theme = ApplyDocsTheme(settings.Theme),
        ColorEmission = ColorEmissionMode.All,
    },
});
// This site's dark mode is a `.dark` class stamped on <html> by App.razor's bootstrap
// script (always authoritative — it resolves the OS preference itself), not the
// `data-theme` attribute Beck assumes by default. ThemeHooks.Class keys the diagrams'
// dark tokens — and the zoom lightbox chrome — off that class instead.
builder.Services.AddPenningtonBeck(beck =>
{
    beck.RenderOptions = new SvgRenderOptions { ThemeHooks = ThemeHooks.Class };
});

builder.Services.AddTreeSitter(treeSitter =>
{
    treeSitter.ContentRoot = "../MonorailCss.Docs.Samples";
});

builder.Services.AddSingleton<UtilityContentService>();
builder.Services.AddTransient<IContentService>(sp => sp.GetRequiredService<UtilityContentService>());

// API reference data is reflected from the compiled MonorailCss assembly plus its
// XML docs (Pennington.ApiMetadata.Reflection) — the native replacement for the old
// Roslyn source-parsing service. MonorailCss.dll/.xml are copied next to the app via
// the project reference. AddApiMetadataFromCompiledAssembly registers the provider
// keyed "default"; bridge it to a plain registration so components can @inject it.
builder.Services.AddApiMetadataFromCompiledAssembly(options =>
    options.AssemblyFiles.Add(Path.Combine(AppContext.BaseDirectory, "MonorailCss.dll")));
builder.Services.AddSingleton<IApiMetadataProvider>(sp =>
    sp.GetRequiredKeyedService<IApiMetadataProvider>("default"));

// Split the ~150 utility pages out of the main /llms.txt into /utility/llms.txt
// so the front door stays scannable for an LLM. The main index gets a single
// see-also pointer; consumers fetching that pointer land on the full utility list.
builder.Services.AddLlmsSubtree(new LlmsSubtree(
    routePrefix: "/utility/",
    title: "Utilities",
    description: "Tailwind 4.3-compatible utility classes — one page per CSS property, grouped by category."));

// CssFramework consumed by UtilityDetailContent.razor to render the example
// CSS pane on every utility-detail page. Mirrors the docs theme so examples
// reflect the configured palette (sanibel, frosted, mixed, happily) rather
// than Tailwind defaults. Safe to register alongside Pennington 0.72+: that
// release moved Pennington's own framework behind MonorailCssEngine and no
// longer puts a CssFramework in DI, so this registration can't shadow it.
builder.Services.AddSingleton<CssFramework>(_ =>
{
    var theme = ApplyDocsTheme(colorScheme.ApplyToTheme(Theme.CreateWithDefaults()));
    return new CssFramework(new CssFrameworkSettings
    {
        Theme = theme,
        IncludePreflight = false,
    });
});

var app = builder.Build();

// UsePennington MUST precede MapRazorComponents: DocPage's "/{*Slug}" is a Blazor
// catch-all that would otherwise swallow the sitemap.xml / llms.txt / redirect routes.
// MapStaticAssets serves the /_content assets (Pennington.UI search modal, Beck).
app.UsePennington();
app.UseMonorailCss();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    // The /playground route is a static-SSR page in this host; its interactive UI is a
    // WebAssembly island (MonorailCss.Docs.Client) bundled via the project reference. No
    // routable components live in the client assembly, so no AddAdditionalAssemblies is needed.
    .AddInteractiveWebAssemblyRenderMode();

await app.RunOrBuildAsync(args);
