using System.Collections.Immutable;
using MonorailCss.Docs.Components;
using MonorailCss.Docs.Services;
using MonorailCss.Theme;
using Pennington.Content;
using Pennington.DocSite;
using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.MonorailCss;
using Pennington.Roslyn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "MonorailCss Documentation",
    Description = "A JIT CSS compiler that aims to be Tailwind CSS 4.1 compatible, written in .NET",
    ContentRootPath = new Pennington.Routing.FilePath("Content"),
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 45,
        ColorSchemeGenerator = i => (i + 1, i + 60, i - 60),
        BaseColorName = "brick",
    },
});

// Override MonorailCss wiring to install the brick color palette, which DocSiteOptions
// can't express. AddMonorailCss after AddDocSite replaces the DI registration.
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = new AlgorithmicColorScheme
    {
        PrimaryHue = 45,
        ColorSchemeGenerator = i => (i + 1, i + 60, i - 60),
        BaseColorName = "brick",
    },
    CustomCssFrameworkSettings = settings => settings with
    {
        Theme = settings.Theme.AddColorPalette("brick", new Dictionary<string, string>
        {
            { "50", "oklch(0.985 0.003 20)" },
            { "100", "oklch(0.967 0.004 20)" },
            { "200", "oklch(0.929 0.006 20)" },
            { "300", "oklch(0.870 0.009 20)" },
            { "400", "oklch(0.705 0.015 20)" },
            { "500", "oklch(0.554 0.020 20)" },
            { "600", "oklch(0.446 0.018 20)" },
            { "700", "oklch(0.369 0.015 20)" },
            { "800", "oklch(0.283 0.012 20)" },
            { "900", "oklch(0.220 0.010 20)" },
            { "950", "oklch(0.145 0.008 20)" },
        }.ToImmutableDictionary()),
    },
});

builder.Services.AddPenningtonRoslyn(roslyn =>
{
    roslyn.SolutionPath = "MonorailCss.Docs.Samples.slnx";
});

builder.Services.AddFileWatched<UtilityContentService>();
builder.Services.AddTransient<IContentService>(sp => sp.GetRequiredService<UtilityContentService>());

// SPA island that swaps content on client-side nav without a full page reload.
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<IIslandRenderer, ContentIslandRenderer>();

builder.Services.AddSingleton<MonorailCss.CssFramework>(_ =>
{
    var settings = new MonorailCss.CssFrameworkSettings
    {
        Theme = Theme.CreateWithDefaults(),
        IncludePreflight = false,
    };
    return new MonorailCss.CssFramework(settings);
});

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UsePennington();
app.UseMonorailCss();
app.UseSpaNavigation();

await app.RunOrBuildAsync(args);
