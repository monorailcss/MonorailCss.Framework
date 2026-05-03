using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Docs.Components;
using MonorailCss.Docs.Services;
using MonorailCss.Parser.Custom;
using MonorailCss.Theme;
using Pennington.Content;
using Pennington.DocSite;
using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.MonorailCss;
using Pennington.Roslyn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

var colorScheme = new AlgorithmicColorScheme
{
    PrimaryHue = 45,
    ColorSchemeGenerator = i => (i + 1, i + 60, i - 60),
    BaseColorName = "brick",
};

ImmutableList<UtilityDefinition> customUtilities =
[
    new UtilityDefinition
    {
        Pattern = "scrollbar-thin",
        Declarations = ImmutableList.Create(
            new CssDeclaration("scrollbar-width", "thin")),
    },
    new UtilityDefinition
    {
        Pattern = "scrollbar-thumb-*",
        IsWildcard = true,
        Declarations = ImmutableList.Create(
            new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")),
    },
    new UtilityDefinition
    {
        Pattern = "scrollbar-track-*",
        IsWildcard = true,
        Declarations = ImmutableList.Create(
            new CssDeclaration("--tw-scrollbar-track-color", "--value(--color-*)")),
    },
    new UtilityDefinition
    {
        Pattern = "scrollbar-color",
        Declarations = ImmutableList.Create(
            new CssDeclaration(
                "scrollbar-color",
                "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)")),
    },
];

Theme ApplyDocsTheme(Theme baseTheme) => baseTheme
    .AddColorPalette("brick", new Dictionary<string, string>
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
    }.ToImmutableDictionary())
    // Landing-page palettes (warm-charcoal design system).
    .AddColorPalette("warm", new Dictionary<string, string>
    {
        { "50",  "#f5ede2" },
        { "100", "#c9bfb2" },
        { "300", "#948578" },
        { "500", "#6b5e54" },
        { "700", "#4a3d35" },
        { "800", "#3a2f29" },
        { "900", "#221b17" },
        { "950", "#17110e" },
    }.ToImmutableDictionary())
    .AddColorPalette("terra", new Dictionary<string, string>
    {
        { "500", "#d65763" },
        { "600", "#c44552" },
    }.ToImmutableDictionary())
    // Override Tailwind's default orange with the design's warmer terracotta-orange.
    .AddColorPalette("orange", new Dictionary<string, string>
    {
        { "500", "#e8945f" },
        { "600", "#d97742" },
    }.ToImmutableDictionary())
    .AddFontFamily("display", "'Newsreader', 'Iowan Old Style', 'Charter', Georgia, serif")
    .AddFontFamily("sans", "'Geist', -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif")
    .AddFontFamily("mono", "'JetBrains Mono', ui-monospace, 'SF Mono', Menlo, monospace");

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "MonorailCss Documentation",
    Description = "A JIT CSS compiler that aims to be Tailwind CSS 4.1 compatible, written in .NET",
    ContentRootPath = new Pennington.Routing.FilePath("Content"),
    ColorScheme = colorScheme,
});

// Override MonorailCss wiring to install the brick color palette, which DocSiteOptions
// can't express. AddMonorailCss after AddDocSite replaces the DI registration.
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ColorScheme = colorScheme,
    CustomCssFrameworkSettings = settings => settings with
    {
        CustomUtilities = customUtilities,
        Theme = ApplyDocsTheme(settings.Theme),
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

// CssFramework consumed by UtilityDetailContent.razor to render the example
// CSS pane on every utility-detail page. Must mirror the AddMonorailCss theme
// so the rendered examples reflect the actual configured palette (brick +
// warm/terra/orange) rather than Tailwind defaults.
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

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UsePennington();
app.UseMonorailCss();
app.UseSpaNavigation();

await app.RunOrBuildAsync(args);
