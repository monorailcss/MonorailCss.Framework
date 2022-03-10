using System.Collections.Immutable;
using MonorailCss.Css;
using static MonorailCss.ColorLevels;
using static MonorailCss.ColorNames;

namespace MonorailCss;

public partial record DesignSystem
{
    private static readonly Lazy<DesignSystem> _default = new(GetDefaultDesignSystem);

    /// <summary>
    /// Gets the default design system.
    /// </summary>
    public static readonly DesignSystem Default = _default.Value;

    private static DesignSystem GetDefaultDesignSystem()
    {
        return new DesignSystem
        {
            Screens = DefaultScreens,
            Colors = DefaultColors,
            Spacing = DefaultSpacing,
            Typography = DefaultTypographies,
            FontWeights = DefaultFontWeights,
            Opacities = DefaultOpacities,
        };
    }

    private static ImmutableDictionary<string, string> DefaultOpacities => new Dictionary<string, string>()
    {
        { "0", "0" },
        { "5", "0.05" },
        { "10", "0.1" },
        { "20", "0.2" },
        { "25", "0.25" },
        { "30", "0.3" },
        { "40", "0.4" },
        { "50", "0.5" },
        { "60", "0.6" },
        { "70", "0.7" },
        { "75", "0.75" },
        { "80", "0.8" },
        { "90", "0.9" },
        { "95", "0.95" },
        { "100", "1" },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, string> DefaultScreens => new Dictionary<string, string>
    {
        { "sm", "640px" },
        { "md", "768px" },
        { "lg", "1024px" },
        { "xl", "1280px" },
        { "2xl", "1536px" },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, string> DefaultFontWeights => new Dictionary<string, string>()
    {
        { "thin", "100" },
        { "extralight", "200" },
        { "light", "300" },
        { "normal", "400" },
        { "medium", "500" },
        { "semibold", "600" },
        { "bold", "700" },
        { "extrabold", "800" },
        { "black", "900" },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, string> DefaultBorderWidths => new Dictionary<string, string>
    {
        { "DEFAULT", "1px" },
        { "0", "0px" },
        { "2", "2px" },
        { "4", "4px" },
        { "8", "8px" },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, string> DefaultBorderRadii => new Dictionary<string, string>
    {
        { "none", "0px" },
        { "sm", "0.125rem" },
        { "DEFAULT", "0.25rem" },
        { "md", "0.375rem" },
        { "lg", "0.5rem" },
        { "xl", "0.75rem" },
        { "2xl", "1rem" },
        { "3xl", "1.5rem" },
        { "full", "9999px" },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, Typography> DefaultTypographies => new Dictionary<string, Typography>
    {
        { "xs", new Typography(".75rem", "1rem") },
        { "sm", new Typography(".875rem", "1.25rem") },
        { "base", new Typography("1rem", "1.5rem") },
        { "lg", new Typography("1.125rem", "1.75rem") },
        { "xl", new Typography("1.25rem", "1.75rem") },
        { "2xl", new Typography("1.5rem", "2rem") },
        { "3xl", new Typography("1.875rem", "2.25rem") },
        { "4xl", new Typography("2.25rem", "2.5rem") },
        { "5xl", new Typography("3rem", "1") },
        { "6xl", new Typography("3.75rem", "1") },
        { "7xl", new Typography("4.5rem", "1") },
        { "8xl", new Typography("6rem", "1") },
        { "9xl", new Typography("7rem", "1") },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, ImmutableDictionary<string, CssColor>> DefaultColors =>
        new Dictionary<string, ImmutableDictionary<string, CssColor>>
        {
            {
                Blue, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#eff6ff") },
                    { _100, new CssColor("#dbeafe") },
                    { _200, new CssColor("#bfdbfe") },
                    { _300, new CssColor("#93c5fd") },
                    { _400, new CssColor("#60a5fa") },
                    { _500, new CssColor("#3b82f6") },
                    { _600, new CssColor("#2563eb") },
                    { _700, new CssColor("#1d4ed8") },
                    { _800, new CssColor("#1e40af") },
                    { _900, new CssColor("#1e3a8a") },
                }.ToImmutableDictionary()
            },
            {
                Gray, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f9fafb") },
                    { _100, new CssColor("#f3f4f6") },
                    { _200, new CssColor("#e5e7eb") },
                    { _300, new CssColor("#d1d5db") },
                    { _400, new CssColor("#9ca3af") },
                    { _500, new CssColor("#6b7280") },
                    { _600, new CssColor("#4b5563") },
                    { _700, new CssColor("#374151") },
                    { _800, new CssColor("#1f2937") },
                    { _900, new CssColor("#111827") },
                }.ToImmutableDictionary()
            },
            {
                Sky, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f0f9ff") },
                    { _100, new CssColor("#e0f2fe") },
                    { _200, new CssColor("#bae6fd") },
                    { _300, new CssColor("#7dd3fc") },
                    { _400, new CssColor("#38bdf8") },
                    { _500, new CssColor("#0ea5e9") },
                    { _600, new CssColor("#0284c7") },
                    { _700, new CssColor("#0369a1") },
                    { _800, new CssColor("#075985") },
                    { _900, new CssColor("#0c4a6e") },
                }.ToImmutableDictionary()
            },
            {
                Yellow, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fefce8") },
                    { _100, new CssColor("#fef9c3") },
                    { _200, new CssColor("#fef08a") },
                    { _300, new CssColor("#fde047") },
                    { _400, new CssColor("#facc15") },
                    { _500, new CssColor("#eab308") },
                    { _600, new CssColor("#ca8a04") },
                    { _700, new CssColor("#a16207") },
                    { _800, new CssColor("#854d0e") },
                    { _900, new CssColor("#713f12") },
                }.ToImmutableDictionary()
            },
            {
                Red, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fef2f2") },
                    { _100, new CssColor("#fee2e2") },
                    { _200, new CssColor("#fecaca") },
                    { _300, new CssColor("#fca5a5") },
                    { _400, new CssColor("#f87171") },
                    { _500, new CssColor("#ef4444") },
                    { _600, new CssColor("#dc2626") },
                    { _700, new CssColor("#b91c1c") },
                    { _800, new CssColor("#991b1b") },
                    { _900, new CssColor("#7f1d1d") },
                }.ToImmutableDictionary()
            },
            {
                Indigo, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#eef2ff") },
                    { _100, new CssColor("#e0e7ff") },
                    { _200, new CssColor("#c7d2fe") },
                    { _300, new CssColor("#a5b4fc") },
                    { _400, new CssColor("#818cf8") },
                    { _500, new CssColor("#6366f1") },
                    { _600, new CssColor("#4f46e5") },
                    { _700, new CssColor("#4338ca") },
                    { _800, new CssColor("#3730a3") },
                    { _900, new CssColor("#312e81") },
                }.ToImmutableDictionary()
            },
            {
                Cyan, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#ecfeff") },
                    { _100, new CssColor("#cffafe") },
                    { _200, new CssColor("#a5f3fc") },
                    { _300, new CssColor("#67e8f9") },
                    { _400, new CssColor("#22d3ee") },
                    { _500, new CssColor("#06b6d4") },
                    { _600, new CssColor("#0891b2") },
                    { _700, new CssColor("#0e7490") },
                    { _800, new CssColor("#155e75") },
                    { _900, new CssColor("#164e63") },
                }.ToImmutableDictionary()
            },
            {
                Orange, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fff7ed") },
                    { _100, new CssColor("#ffedd5") },
                    { _200, new CssColor("#fed7aa") },
                    { _300, new CssColor("#fdba74") },
                    { _400, new CssColor("#fb923c") },
                    { _500, new CssColor("#f97316") },
                    { _600, new CssColor("#ea580c") },
                    { _700, new CssColor("#c2410c") },
                    { _800, new CssColor("#9a3412") },
                    { _900, new CssColor("#7c2d12") },
                }.ToImmutableDictionary()
            },
            {
                Green, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f0fdf4") },
                    { _100, new CssColor("#dcfce7") },
                    { _200, new CssColor("#bbf7d0") },
                    { _300, new CssColor("#86efac") },
                    { _400, new CssColor("#4ade80") },
                    { _500, new CssColor("#22c55e") },
                    { _600, new CssColor("#16a34a") },
                    { _700, new CssColor("#15803d") },
                    { _800, new CssColor("#166534") },
                    { _900, new CssColor("#14532d") },
                }.ToImmutableDictionary()
            },
        }.ToImmutableDictionary();
}