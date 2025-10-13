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
        ContentPath = "Content", BasePageUrl = string.Empty,
    })
    .AddSingleton<UtilityContentService>()
    .AddSingleton<IContentService>(sp => sp.GetRequiredService<UtilityContentService>());

builder.Services.AddMonorailCss(provider =>
{
    return new MonorailCssOptions()
    {
        BaseColorName = () => ColorNames.Zinc,
        PrimaryHue = () => 45,
        ColorSchemeGenerator = i => (i + 90, i + 45, i -45),
    };
} );

// Register CssFramework for utility documentation generation
builder.Services.AddSingleton<MonorailCss.CssFramework>(_ =>
{
    var settings = new MonorailCss.CssFrameworkSettings
    {
        Theme = MonorailCss.Theme.Theme.CreateWithDefaults(),
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