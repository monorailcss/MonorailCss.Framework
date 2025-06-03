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
            Variables = DefaultVariables,
            Screens = DefaultScreens,
            Colors = DefaultColors,
            Typography = DefaultTypographies,
            FontWeights = DefaultFontWeights,
            Opacities = DefaultOpacities,
            FontFamilies = DefaultFontFamilies,
        };
    }

    private static ImmutableDictionary<string, string> DefaultVariables => new Dictionary<string, string>()
            {
                { "spacing", "0.25rem" },
            }
        .ToImmutableDictionary();

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
                { _50, new CssColor("oklch(0.984 0.003 247.858)") },
                { _100, new CssColor("oklch(0.968 0.007 247.896)") },
                { _200, new CssColor("oklch(0.929 0.013 255.508)") },
                { _300, new CssColor("oklch(0.869 0.022 252.894)") },
                { _400, new CssColor("oklch(0.704 0.04 256.788)") },
                { _500, new CssColor("oklch(0.554 0.046 257.417)") },
                { _600, new CssColor("oklch(0.446 0.043 257.281)") },
                { _700, new CssColor("oklch(0.372 0.044 257.287)") },
                { _800, new CssColor("oklch(0.279 0.041 260.031)") },
                { _900, new CssColor("oklch(0.208 0.042 265.755)") },
                { _950, new CssColor("oklch(0.129 0.042 264.695)") },
            }.ToImmutableDictionary()
        },
        {
            Zinc, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.985 0 0)") },
                { _100, new CssColor("oklch(0.967 0.001 286.375)") },
                { _200, new CssColor("oklch(0.92 0.004 286.32)") },
                { _300, new CssColor("oklch(0.871 0.006 286.286)") },
                { _400, new CssColor("oklch(0.705 0.015 286.067)") },
                { _500, new CssColor("oklch(0.552 0.016 285.938)") },
                { _600, new CssColor("oklch(0.442 0.017 285.786)") },
                { _700, new CssColor("oklch(0.37 0.013 285.805)") },
                { _800, new CssColor("oklch(0.274 0.006 286.033)") },
                { _900, new CssColor("oklch(0.21 0.006 285.885)") },
                { _950, new CssColor("oklch(0.141 0.005 285.823)") },
            }.ToImmutableDictionary()
        },
        {
            Neutral, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.985 0 0)") },
                { _100, new CssColor("oklch(0.97 0 0)") },
                { _200, new CssColor("oklch(0.922 0 0)") },
                { _300, new CssColor("oklch(0.87 0 0)") },
                { _400, new CssColor("oklch(0.708 0 0)") },
                { _500, new CssColor("oklch(0.556 0 0)") },
                { _600, new CssColor("oklch(0.439 0 0)") },
                { _700, new CssColor("oklch(0.371 0 0)") },
                { _800, new CssColor("oklch(0.269 0 0)") },
                { _900, new CssColor("oklch(0.205 0 0)") },
                { _950, new CssColor("oklch(0.145 0 0)") },
            }.ToImmutableDictionary()
        },
        {
            Gray, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.985 0.002 247.839)") },
                { _100, new CssColor("oklch(0.967 0.003 264.542)") },
                { _200, new CssColor("oklch(0.928 0.006 264.531)") },
                { _300, new CssColor("oklch(0.872 0.01 258.338)") },
                { _400, new CssColor("oklch(0.707 0.022 261.325)") },
                { _500, new CssColor("oklch(0.551 0.027 264.364)") },
                { _600, new CssColor("oklch(0.446 0.03 256.802)") },
                { _700, new CssColor("oklch(0.373 0.034 259.733)") },
                { _800, new CssColor("oklch(0.278 0.033 256.848)") },
                { _900, new CssColor("oklch(0.21 0.034 264.665)") },
                { _950, new CssColor("oklch(0.13 0.028 261.692)") },
            }.ToImmutableDictionary()
        },
        {
            Stone, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.985 0.001 106.423)") },
                { _100, new CssColor("oklch(0.97 0.001 106.424)") },
                { _200, new CssColor("oklch(0.923 0.003 48.717)") },
                { _300, new CssColor("oklch(0.869 0.005 56.366)") },
                { _400, new CssColor("oklch(0.709 0.01 56.259)") },
                { _500, new CssColor("oklch(0.553 0.013 58.071)") },
                { _600, new CssColor("oklch(0.444 0.011 73.639)") },
                { _700, new CssColor("oklch(0.374 0.01 67.558)") },
                { _800, new CssColor("oklch(0.268 0.007 34.298)") },
                { _900, new CssColor("oklch(0.216 0.006 56.043)") },
                { _950, new CssColor("oklch(0.147 0.004 49.25)") },
            }.ToImmutableDictionary()
        },
        {
            Red, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.971 0.013 17.38)") },
                { _100, new CssColor("oklch(0.936 0.032 17.717)") },
                { _200, new CssColor("oklch(0.885 0.062 18.334)") },
                { _300, new CssColor("oklch(0.808 0.114 19.571)") },
                { _400, new CssColor("oklch(0.704 0.191 22.216)") },
                { _500, new CssColor("oklch(0.637 0.237 25.331)") },
                { _600, new CssColor("oklch(0.577 0.245 27.325)") },
                { _700, new CssColor("oklch(0.505 0.213 27.518)") },
                { _800, new CssColor("oklch(0.444 0.177 26.899)") },
                { _900, new CssColor("oklch(0.396 0.141 25.723)") },
                { _950, new CssColor("oklch(0.258 0.092 26.042)") },
            }.ToImmutableDictionary()
        },
        {
            Orange, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.98 0.016 73.684)") },
                { _100, new CssColor("oklch(0.954 0.038 75.164)") },
                { _200, new CssColor("oklch(0.901 0.076 70.697)") },
                { _300, new CssColor("oklch(0.837 0.128 66.29)") },
                { _400, new CssColor("oklch(0.75 0.183 55.934)") },
                { _500, new CssColor("oklch(0.705 0.213 47.604)") },
                { _600, new CssColor("oklch(0.646 0.222 41.116)") },
                { _700, new CssColor("oklch(0.553 0.195 38.402)") },
                { _800, new CssColor("oklch(0.47 0.157 37.304)") },
                { _900, new CssColor("oklch(0.408 0.123 38.172)") },
                { _950, new CssColor("oklch(0.266 0.079 36.259)") },
            }.ToImmutableDictionary()
        },
        {
            Amber, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.987 0.022 95.277)") },
                { _100, new CssColor("oklch(0.962 0.059 95.617)") },
                { _200, new CssColor("oklch(0.924 0.12 95.746)") },
                { _300, new CssColor("oklch(0.879 0.169 91.605)") },
                { _400, new CssColor("oklch(0.828 0.189 84.429)") },
                { _500, new CssColor("oklch(0.769 0.188 70.08)") },
                { _600, new CssColor("oklch(0.666 0.179 58.318)") },
                { _700, new CssColor("oklch(0.555 0.163 48.998)") },
                { _800, new CssColor("oklch(0.473 0.137 46.201)") },
                { _900, new CssColor("oklch(0.414 0.112 45.904)") },
                { _950, new CssColor("oklch(0.279 0.077 45.635)") },
            }.ToImmutableDictionary()
        },
        {
            Yellow, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.987 0.026 102.212)") },
                { _100, new CssColor("oklch(0.973 0.071 103.193)") },
                { _200, new CssColor("oklch(0.945 0.129 101.54)") },
                { _300, new CssColor("oklch(0.905 0.182 98.111)") },
                { _400, new CssColor("oklch(0.852 0.199 91.936)") },
                { _500, new CssColor("oklch(0.795 0.184 86.047)") },
                { _600, new CssColor("oklch(0.681 0.162 75.834)") },
                { _700, new CssColor("oklch(0.554 0.135 66.442)") },
                { _800, new CssColor("oklch(0.476 0.114 61.907)") },
                { _900, new CssColor("oklch(0.421 0.095 57.708)") },
                { _950, new CssColor("oklch(0.286 0.066 53.813)") },
            }.ToImmutableDictionary()
        },
        {
            Lime, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.986 0.031 120.757)") },
                { _100, new CssColor("oklch(0.967 0.067 122.328)") },
                { _200, new CssColor("oklch(0.938 0.127 124.321)") },
                { _300, new CssColor("oklch(0.897 0.196 126.665)") },
                { _400, new CssColor("oklch(0.841 0.238 128.85)") },
                { _500, new CssColor("oklch(0.768 0.233 130.85)") },
                { _600, new CssColor("oklch(0.648 0.2 131.684)") },
                { _700, new CssColor("oklch(0.532 0.157 131.589)") },
                { _800, new CssColor("oklch(0.453 0.124 130.933)") },
                { _900, new CssColor("oklch(0.405 0.101 131.063)") },
                { _950, new CssColor("oklch(0.274 0.072 132.109)") },
            }.ToImmutableDictionary()
        },
        {
            Green, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.982 0.018 155.826)") },
                { _100, new CssColor("oklch(0.962 0.044 156.743)") },
                { _200, new CssColor("oklch(0.925 0.084 155.995)") },
                { _300, new CssColor("oklch(0.871 0.15 154.449)") },
                { _400, new CssColor("oklch(0.792 0.209 151.711)") },
                { _500, new CssColor("oklch(0.723 0.219 149.579)") },
                { _600, new CssColor("oklch(0.627 0.194 149.214)") },
                { _700, new CssColor("oklch(0.527 0.154 150.069)") },
                { _800, new CssColor("oklch(0.448 0.119 151.328)") },
                { _900, new CssColor("oklch(0.393 0.095 152.535)") },
                { _950, new CssColor("oklch(0.266 0.065 152.934)") },
            }.ToImmutableDictionary()
        },
        {
            Emerald, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.979 0.021 166.113)") },
                { _100, new CssColor("oklch(0.95 0.052 163.051)") },
                { _200, new CssColor("oklch(0.905 0.093 164.15)") },
                { _300, new CssColor("oklch(0.845 0.143 164.978)") },
                { _400, new CssColor("oklch(0.765 0.177 163.223)") },
                { _500, new CssColor("oklch(0.696 0.17 162.48)") },
                { _600, new CssColor("oklch(0.596 0.145 163.225)") },
                { _700, new CssColor("oklch(0.508 0.118 165.612)") },
                { _800, new CssColor("oklch(0.432 0.095 166.913)") },
                { _900, new CssColor("oklch(0.378 0.077 168.94)") },
                { _950, new CssColor("oklch(0.262 0.051 172.552)") },
            }.ToImmutableDictionary()
        },
        {
            Teal, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.984 0.014 180.72)") },
                { _100, new CssColor("oklch(0.953 0.051 180.801)") },
                { _200, new CssColor("oklch(0.91 0.096 180.426)") },
                { _300, new CssColor("oklch(0.855 0.138 181.071)") },
                { _400, new CssColor("oklch(0.777 0.152 181.912)") },
                { _500, new CssColor("oklch(0.704 0.14 182.503)") },
                { _600, new CssColor("oklch(0.6 0.118 184.704)") },
                { _700, new CssColor("oklch(0.511 0.096 186.391)") },
                { _800, new CssColor("oklch(0.437 0.078 188.216)") },
                { _900, new CssColor("oklch(0.386 0.063 188.416)") },
                { _950, new CssColor("oklch(0.277 0.046 192.524)") },
            }.ToImmutableDictionary()
        },
        {
            Cyan, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.984 0.019 200.873)") },
                { _100, new CssColor("oklch(0.956 0.045 203.388)") },
                { _200, new CssColor("oklch(0.917 0.08 205.041)") },
                { _300, new CssColor("oklch(0.865 0.127 207.078)") },
                { _400, new CssColor("oklch(0.789 0.154 211.53)") },
                { _500, new CssColor("oklch(0.715 0.143 215.221)") },
                { _600, new CssColor("oklch(0.609 0.126 221.723)") },
                { _700, new CssColor("oklch(0.52 0.105 223.128)") },
                { _800, new CssColor("oklch(0.45 0.085 224.283)") },
                { _900, new CssColor("oklch(0.398 0.07 227.392)") },
                { _950, new CssColor("oklch(0.302 0.056 229.695)") },
            }.ToImmutableDictionary()
        },
        {
            Sky, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.977 0.013 236.62)") },
                { _100, new CssColor("oklch(0.951 0.026 236.824)") },
                { _200, new CssColor("oklch(0.901 0.058 230.902)") },
                { _300, new CssColor("oklch(0.828 0.111 230.318)") },
                { _400, new CssColor("oklch(0.746 0.16 232.661)") },
                { _500, new CssColor("oklch(0.685 0.169 237.323)") },
                { _600, new CssColor("oklch(0.588 0.158 241.966)") },
                { _700, new CssColor("oklch(0.5 0.134 242.749)") },
                { _800, new CssColor("oklch(0.443 0.11 240.79)") },
                { _900, new CssColor("oklch(0.391 0.09 240.876)") },
                { _950, new CssColor("oklch(0.293 0.066 243.157)") },
            }.ToImmutableDictionary()
        },
        {
            Blue, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.97 0.014 254.604)") },
                { _100, new CssColor("oklch(0.932 0.032 255.585)") },
                { _200, new CssColor("oklch(0.882 0.059 254.128)") },
                { _300, new CssColor("oklch(0.809 0.105 251.813)") },
                { _400, new CssColor("oklch(0.707 0.165 254.624)") },
                { _500, new CssColor("oklch(0.623 0.214 259.815)") },
                { _600, new CssColor("oklch(0.546 0.245 262.881)") },
                { _700, new CssColor("oklch(0.488 0.243 264.376)") },
                { _800, new CssColor("oklch(0.424 0.199 265.638)") },
                { _900, new CssColor("oklch(0.379 0.146 265.522)") },
                { _950, new CssColor("oklch(0.282 0.091 267.935)") },
            }.ToImmutableDictionary()
        },
        {
            Indigo, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.962 0.018 272.314)") },
                { _100, new CssColor("oklch(0.93 0.034 272.788)") },
                { _200, new CssColor("oklch(0.87 0.065 274.039)") },
                { _300, new CssColor("oklch(0.785 0.115 274.713)") },
                { _400, new CssColor("oklch(0.673 0.182 276.935)") },
                { _500, new CssColor("oklch(0.585 0.233 277.117)") },
                { _600, new CssColor("oklch(0.511 0.262 276.966)") },
                { _700, new CssColor("oklch(0.457 0.24 277.023)") },
                { _800, new CssColor("oklch(0.398 0.195 277.366)") },
                { _900, new CssColor("oklch(0.359 0.144 278.697)") },
                { _950, new CssColor("oklch(0.257 0.09 281.288)") },
            }.ToImmutableDictionary()
        },
        {
            Violet, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.969 0.016 293.756)") },
                { _100, new CssColor("oklch(0.943 0.029 294.588)") },
                { _200, new CssColor("oklch(0.894 0.057 293.283)") },
                { _300, new CssColor("oklch(0.811 0.111 293.571)") },
                { _400, new CssColor("oklch(0.702 0.183 293.541)") },
                { _500, new CssColor("oklch(0.606 0.25 292.717)") },
                { _600, new CssColor("oklch(0.541 0.281 293.009)") },
                { _700, new CssColor("oklch(0.491 0.27 292.581)") },
                { _800, new CssColor("oklch(0.432 0.232 292.759)") },
                { _900, new CssColor("oklch(0.38 0.189 293.745)") },
                { _950, new CssColor("oklch(0.283 0.141 291.089)") },
            }.ToImmutableDictionary()
        },
        {
            Purple, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.977 0.014 308.299)") },
                { _100, new CssColor("oklch(0.946 0.033 307.174)") },
                { _200, new CssColor("oklch(0.902 0.063 306.703)") },
                { _300, new CssColor("oklch(0.827 0.119 306.383)") },
                { _400, new CssColor("oklch(0.714 0.203 305.504)") },
                { _500, new CssColor("oklch(0.627 0.265 303.9)") },
                { _600, new CssColor("oklch(0.558 0.288 302.321)") },
                { _700, new CssColor("oklch(0.496 0.265 301.924)") },
                { _800, new CssColor("oklch(0.438 0.218 303.724)") },
                { _900, new CssColor("oklch(0.381 0.176 304.987)") },
                { _950, new CssColor("oklch(0.291 0.149 302.717)") },
            }.ToImmutableDictionary()
        },
        {
            Fuchsia, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.977 0.017 320.058)") },
                { _100, new CssColor("oklch(0.952 0.037 318.852)") },
                { _200, new CssColor("oklch(0.903 0.076 319.62)") },
                { _300, new CssColor("oklch(0.833 0.145 321.434)") },
                { _400, new CssColor("oklch(0.74 0.238 322.16)") },
                { _500, new CssColor("oklch(0.667 0.295 322.15)") },
                { _600, new CssColor("oklch(0.591 0.293 322.896)") },
                { _700, new CssColor("oklch(0.518 0.253 323.949)") },
                { _800, new CssColor("oklch(0.452 0.211 324.591)") },
                { _900, new CssColor("oklch(0.401 0.17 325.612)") },
                { _950, new CssColor("oklch(0.293 0.136 325.661)") },
            }.ToImmutableDictionary()
        },
        {
            Pink, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.971 0.014 343.198)") },
                { _100, new CssColor("oklch(0.948 0.028 342.258)") },
                { _200, new CssColor("oklch(0.899 0.061 343.231)") },
                { _300, new CssColor("oklch(0.823 0.12 346.018)") },
                { _400, new CssColor("oklch(0.718 0.202 349.761)") },
                { _500, new CssColor("oklch(0.656 0.241 354.308)") },
                { _600, new CssColor("oklch(0.592 0.249 0.584)") },
                { _700, new CssColor("oklch(0.525 0.223 3.958)") },
                { _800, new CssColor("oklch(0.459 0.187 3.815)") },
                { _900, new CssColor("oklch(0.408 0.153 2.432)") },
                { _950, new CssColor("oklch(0.284 0.109 3.907)") },
            }.ToImmutableDictionary()
        },
        {
            Rose, new Dictionary<string, CssColor>
            {
                { _50, new CssColor("oklch(0.969 0.015 12.422)") },
                { _100, new CssColor("oklch(0.941 0.03 12.58)") },
                { _200, new CssColor("oklch(0.892 0.058 10.001)") },
                { _300, new CssColor("oklch(0.81 0.117 11.638)") },
                { _400, new CssColor("oklch(0.712 0.194 13.428)") },
                { _500, new CssColor("oklch(0.645 0.246 16.439)") },
                { _600, new CssColor("oklch(0.586 0.253 17.585)") },
                { _700, new CssColor("oklch(0.514 0.222 16.935)") },
                { _800, new CssColor("oklch(0.455 0.188 13.697)") },
                { _900, new CssColor("oklch(0.41 0.159 10.272)") },
                { _950, new CssColor("oklch(0.271 0.105 12.094)") },
            }.ToImmutableDictionary()
        },
    }.ToImmutableDictionary();

    private static ImmutableDictionary<string, FontFamilyDefinition> DefaultFontFamilies => new Dictionary<string, FontFamilyDefinition>
    {
        { "sans", new FontFamilyDefinition("-apple-system, BlinkMacSystemFont, avenir next, avenir, segoe ui, helvetica neue, helvetica, Ubuntu, roboto, noto, arial, sans-serif") },
        { "serif", new FontFamilyDefinition("Iowan Old Style, Apple Garamond, Baskerville, Times New Roman, Droid Serif, Times, Source Serif Pro, serif, Apple Color Emoji, Segoe UI Emoji, Segoe UI Symbol") },
        { "mono", new FontFamilyDefinition("Cascadia Code, Menlo, Consolas, Monaco, Liberation Mono, Lucida Console, monospace") },
    }.ToImmutableDictionary();
}