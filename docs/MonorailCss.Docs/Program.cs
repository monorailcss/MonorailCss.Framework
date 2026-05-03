using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Docs.Components;
using MonorailCss.Docs.Services;
using MonorailCss.Parser.Custom;
using MonorailCss.Theme;
using Pennington.Content;
using Pennington.DocSite;
using Pennington.Infrastructure;
using Pennington.MonorailCss;
using Pennington.Roslyn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

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

// Override MonorailCss wiring to install the docs color palettes, which DocSiteOptions
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

// CssFramework consumed by UtilityDetailContent.razor to render the example
// CSS pane on every utility-detail page. Must mirror the AddMonorailCss theme
// so the rendered examples reflect the actual configured palette (sanibel,
// frosted, mixed, happily) rather than Tailwind defaults.
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

await app.RunOrBuildAsync(args);
