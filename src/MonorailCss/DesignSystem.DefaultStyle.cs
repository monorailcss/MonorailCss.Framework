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

    private static ImmutableDictionary<string, string> DefaultOpacities => new Dictionary<string, string>
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

    private static ImmutableDictionary<string, string> DefaultFontWeights => new Dictionary<string, string>
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
                "White",
                new Dictionary<string, CssColor> { { _Default, new CssColor("#fff") }, }.ToImmutableDictionary()
            },
            {
                "Black",
                new Dictionary<string, CssColor> { { _Default, new CssColor("#000") }, }.ToImmutableDictionary()
            },
            {
                Slate, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f8fafc") },
                    { _100, new CssColor("#f1f5f9") },
                    { _200, new CssColor("#e2e8f0") },
                    { _300, new CssColor("#cbd5e1") },
                    { _400, new CssColor("#94a3b8") },
                    { _500, new CssColor("#64748b") },
                    { _600, new CssColor("#475569") },
                    { _700, new CssColor("#334155") },
                    { _800, new CssColor("#1e293b") },
                    { _900, new CssColor("#0f172a") },
                }.ToImmutableDictionary()
            },
            {
                Zinc, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fafafa") },
                    { _100, new CssColor("#f4f4f5") },
                    { _200, new CssColor("#e4e4e7") },
                    { _300, new CssColor("#d4d4d8") },
                    { _400, new CssColor("#a1a1aa") },
                    { _500, new CssColor("#71717a") },
                    { _600, new CssColor("#52525b") },
                    { _700, new CssColor("#3f3f46") },
                    { _800, new CssColor("#27272a") },
                    { _900, new CssColor("#18181b") },
                }.ToImmutableDictionary()
            },
            {
                Neutral, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fafafa") },
                    { _100, new CssColor("#f5f5f5") },
                    { _200, new CssColor("#e5e5e5") },
                    { _300, new CssColor("#d4d4d4") },
                    { _400, new CssColor("#a3a3a3") },
                    { _500, new CssColor("#737373") },
                    { _600, new CssColor("#525252") },
                    { _700, new CssColor("#404040") },
                    { _800, new CssColor("#262626") },
                    { _900, new CssColor("#171717") },
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
                Stone, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fafaf9") },
                    { _100, new CssColor("#f5f5f4") },
                    { _200, new CssColor("#e7e5e4") },
                    { _300, new CssColor("#d6d3d1") },
                    { _400, new CssColor("#a8a29e") },
                    { _500, new CssColor("#78716c") },
                    { _600, new CssColor("#57534e") },
                    { _700, new CssColor("#44403c") },
                    { _800, new CssColor("#292524") },
                    { _900, new CssColor("#1c1917") },
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
                Amber, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fffbeb") },
                    { _100, new CssColor("#fef3c7") },
                    { _200, new CssColor("#fde68a") },
                    { _300, new CssColor("#fcd34d") },
                    { _400, new CssColor("#fbbf24") },
                    { _500, new CssColor("#f59e0b") },
                    { _600, new CssColor("#d97706") },
                    { _700, new CssColor("#b45309") },
                    { _800, new CssColor("#92400e") },
                    { _900, new CssColor("#78350f") },
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
                Lime, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f7fee7") },
                    { _100, new CssColor("#ecfccb") },
                    { _200, new CssColor("#d9f99d") },
                    { _300, new CssColor("#bef264") },
                    { _400, new CssColor("#a3e635") },
                    { _500, new CssColor("#84cc16") },
                    { _600, new CssColor("#65a30d") },
                    { _700, new CssColor("#4d7c0f") },
                    { _800, new CssColor("#3f6212") },
                    { _900, new CssColor("#365314") },
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
            {
                Emerald, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#ecfdf5") },
                    { _100, new CssColor("#d1fae5") },
                    { _200, new CssColor("#a7f3d0") },
                    { _300, new CssColor("#6ee7b7") },
                    { _400, new CssColor("#34d399") },
                    { _500, new CssColor("#10b981") },
                    { _600, new CssColor("#059669") },
                    { _700, new CssColor("#047857") },
                    { _800, new CssColor("#065f46") },
                    { _900, new CssColor("#064e3b") },
                }.ToImmutableDictionary()
            },
            {
                Teal, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f0fdfa") },
                    { _100, new CssColor("#ccfbf1") },
                    { _200, new CssColor("#99f6e4") },
                    { _300, new CssColor("#5eead4") },
                    { _400, new CssColor("#2dd4bf") },
                    { _500, new CssColor("#14b8a6") },
                    { _600, new CssColor("#0d9488") },
                    { _700, new CssColor("#0f766e") },
                    { _800, new CssColor("#115e59") },
                    { _900, new CssColor("#134e4a") },
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
                Violet, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#f5f3ff") },
                    { _100, new CssColor("#ede9fe") },
                    { _200, new CssColor("#ddd6fe") },
                    { _300, new CssColor("#c4b5fd") },
                    { _400, new CssColor("#a78bfa") },
                    { _500, new CssColor("#8b5cf6") },
                    { _600, new CssColor("#7c3aed") },
                    { _700, new CssColor("#6d28d9") },
                    { _800, new CssColor("#5b21b6") },
                    { _900, new CssColor("#4c1d95") },
                }.ToImmutableDictionary()
            },
            {
                Purple, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#faf5ff") },
                    { _100, new CssColor("#f3e8ff") },
                    { _200, new CssColor("#e9d5ff") },
                    { _300, new CssColor("#d8b4fe") },
                    { _400, new CssColor("#c084fc") },
                    { _500, new CssColor("#a855f7") },
                    { _600, new CssColor("#9333ea") },
                    { _700, new CssColor("#7e22ce") },
                    { _800, new CssColor("#6b21a8") },
                    { _900, new CssColor("#581c87") },
                }.ToImmutableDictionary()
            },
            {
                Fuchsia, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fdf4ff") },
                    { _100, new CssColor("#fae8ff") },
                    { _200, new CssColor("#f5d0fe") },
                    { _300, new CssColor("#f0abfc") },
                    { _400, new CssColor("#e879f9") },
                    { _500, new CssColor("#d946ef") },
                    { _600, new CssColor("#c026d3") },
                    { _700, new CssColor("#a21caf") },
                    { _800, new CssColor("#86198f") },
                    { _900, new CssColor("#701a75") },
                }.ToImmutableDictionary()
            },
            {
                Pink, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fdf2f8") },
                    { _100, new CssColor("#fce7f3") },
                    { _200, new CssColor("#fbcfe8") },
                    { _300, new CssColor("#f9a8d4") },
                    { _400, new CssColor("#f472b6") },
                    { _500, new CssColor("#ec4899") },
                    { _600, new CssColor("#db2777") },
                    { _700, new CssColor("#be185d") },
                    { _800, new CssColor("#9d174d") },
                    { _900, new CssColor("#831843") },
                }.ToImmutableDictionary()
            },
            {
                Rose, new Dictionary<string, CssColor>
                {
                    { _50, new CssColor("#fff1f2") },
                    { _100, new CssColor("#ffe4e6") },
                    { _200, new CssColor("#fecdd3") },
                    { _300, new CssColor("#fda4af") },
                    { _400, new CssColor("#fb7185") },
                    { _500, new CssColor("#f43f5e") },
                    { _600, new CssColor("#e11d48") },
                    { _700, new CssColor("#be123c") },
                    { _800, new CssColor("#9f1239") },
                    { _900, new CssColor("#881337") },
                }.ToImmutableDictionary()
            },
        }.ToImmutableDictionary();
}