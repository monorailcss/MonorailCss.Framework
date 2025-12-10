using System.Collections.Immutable;
using MonorailCss.Docs.Components;
using MonorailCss.Docs.Models;
using MonorailCss.Docs.Services;
using MonorailCss.Theme;
using MyLittleContentEngine;
using MyLittleContentEngine.MonorailCss;
using MyLittleContentEngine.Services.Content;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

// Configure content engine with MonorailCss
builder.Services.AddContentEngineService(_ => new ContentEngineOptions
{
    SiteTitle = "MonorailCss Documentation",
    SiteDescription = "A JIT CSS compiler that aims to be Tailwind CSS 4.1 compatible, written in .NET",
    ContentRootPath = "Content",
})
    .WithMarkdownContentService(_ => new MarkdownContentOptions<DocFrontMatter>
    {
        ContentPath = "Content",
        BasePageUrl = string.Empty,
    })
    .AddSingleton<UtilityContentService>()
    .AddSingleton<IContentService>(sp => sp.GetRequiredService<UtilityContentService>());

builder.Services.AddMonorailCss(_ =>
{
    return new MonorailCssOptions()
    {
        ColorScheme = new AlgorithmicColorScheme()
        {
            PrimaryHue = 45,
            ColorSchemeGenerator = i => (i + 1, i + 45, i - 45),
            BaseColorName = "brick"
        },
        CustomCssFrameworkSettings = (settings => settings with
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
        }),
    };
});

// Register CssFramework for utility documentation generation
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
app.UseMonorailCss();

await app.RunOrBuildContent(args);
