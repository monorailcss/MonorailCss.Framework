using System.Collections.Immutable;
using MonorailCss.Docs.Components;
using MonorailCss.Docs.Models;
using MonorailCss.Docs.Services;
using MonorailCss.Parser.Custom;
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
        BaseColorName = () => "brick",
        PrimaryHue = () => 45,
        ColorSchemeGenerator = i => (i + 90, i + 45, i - 45),
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
            CustomUtilities = ScrollbarUtilityDefinitions(),
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
return;

ImmutableList<UtilityDefinition> ScrollbarUtilityDefinitions()
{
    var utilityDefinitions = ImmutableList.Create(
        // scrollbar-none
        new UtilityDefinition
        {
            Pattern = "scrollbar-none",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "none")
            ),
            NestedSelectors = ImmutableList.Create(
                new NestedSelector("&::-webkit-scrollbar", ImmutableList.Create(
                    new CssDeclaration("display", "none")
                ))
            )
        },
        // scrollbar-thin
        new UtilityDefinition
        {
            Pattern = "scrollbar-thin",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "thin")
            )
        },
        // scrollbar-width-auto
        new UtilityDefinition
        {
            Pattern = "scrollbar-width-auto",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "auto")
            )
        },
        // scrollbar-gutter-auto
        new UtilityDefinition
        {
            Pattern = "scrollbar-gutter-auto",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-gutter", "auto")
            )
        },
        // scrollbar-stable
        new UtilityDefinition
        {
            Pattern = "scrollbar-stable",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-gutter", "stable")
            )
        },
        // scrollbar-both-edges
        new UtilityDefinition
        {
            Pattern = "scrollbar-both-edges",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-gutter-modifier", "both-edges")
            )
        },
        // scrollbar-color-auto
        new UtilityDefinition
        {
            Pattern = "scrollbar-color-auto",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-color", "auto")
            )
        },
        // scrollbar-color
        new UtilityDefinition
        {
            Pattern = "scrollbar-color",
            IsWildcard = false,
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-color", "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)")
            )
        },
        // scrollbar-thumb-*
        new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")
            )
        },
        // scrollbar-track-*
        new UtilityDefinition
        {
            Pattern = "scrollbar-track-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-track-color", "--value(--color-*)")
            )
        }
    );
    return utilityDefinitions;
}