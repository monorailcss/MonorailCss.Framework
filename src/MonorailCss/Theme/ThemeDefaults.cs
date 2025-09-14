using System.Collections.Immutable;

namespace MonorailCss.Theme;

/// <summary>
/// Provides a static collection of color names.
/// </summary>
public static class ColorNames
{
    /// <summary>
    /// Specifies the color name "red".
    /// </summary>
    public const string Red = "red";

    /// <summary>
    /// Represents the color name "orange".
    /// </summary>
    public const string Orange = "orange";

    /// <summary>
    /// Represents the color name "amber".
    /// </summary>
    public const string Amber = "amber";

    /// <summary>
    /// Represents the color name "yellow".
    /// </summary>
    public const string Yellow = "yellow";

    /// <summary>
    /// Represents the color name "lime".
    /// </summary>
    public const string Lime = "lime";

    /// <summary>
    /// Represents the color name "green".
    /// </summary>
    public const string Green = "green";

    /// <summary>
    /// Represents the color name "emerald".
    /// </summary>
    public const string Emerald = "emerald";

    /// <summary>
    /// Represents the color name "teal".
    /// </summary>
    public const string Teal = "teal";

    /// <summary>
    /// Represents the color name "cyan".
    /// </summary>
    public const string Cyan = "cyan";

    /// <summary>
    /// Represents the color name "sky".
    /// </summary>
    public const string Sky = "sky";

    /// <summary>
    /// Represents the color name "blue".
    /// </summary>
    public const string Blue = "blue";

    /// <summary>
    /// Represents the color name "indigo".
    /// </summary>
    public const string Indigo = "indigo";

    /// <summary>
    /// Represents the color name "violet".
    /// </summary>
    public const string Violet = "violet";

    /// <summary>
    /// Represents the color name "purple".
    /// </summary>
    public const string Purple = "purple";

    /// <summary>
    /// Represents the color name "fuchsia".
    /// </summary>
    public const string Fuchsia = "fuchsia";

    /// <summary>
    /// Represents the color name "pink".
    /// </summary>
    public const string Pink = "pink";

    /// <summary>
    /// Represents the color name "rose".
    /// </summary>
    public const string Rose = "rose";

    /// <summary>
    /// Represents the color name "slate".
    /// </summary>
    public const string Slate = "slate";

    /// <summary>
    /// Represents the color name "gray".
    /// </summary>
    public const string Gray = "gray";

    /// <summary>
    /// Represents the color name "zinc".
    /// </summary>
    public const string Zinc = "zinc";

    /// <summary>
    /// Represents the color name "neutral".
    /// </summary>
    public const string Neutral = "neutral";

    /// <summary>
    /// Represents the object or entity named "Stone".
    /// </summary>
    public const string Stone = "stone";

    /// <summary>
    /// Represents the color name "black".
    /// </summary>
    public const string Black = "black";

    /// <summary>
    /// Represents the color name "white".
    /// </summary>
    public const string White = "white";
}

/// <summary>
/// Provides a set of default theme values for the MonorailCss library.
/// </summary>
public static class ThemeDefaults
{
    /// <summary>
    /// Retrieves the default theme configurations as an immutable dictionary.
    /// </summary>
    /// <returns>An immutable dictionary containing the default theme settings as key-value pairs.</returns>
    public static ImmutableDictionary<string, string> GetDefaults()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();

        // Font families
        builder.Add("--font-sans", "ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'");
        builder.Add("--font-serif", "ui-serif, Georgia, Cambria, 'Times New Roman', Times, serif");
        builder.Add("--font-mono", "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace");

        // Colors - Red
        builder.Add("--color-red-50", "oklch(97.1% 0.013 17.38)");
        builder.Add("--color-red-100", "oklch(93.6% 0.032 17.717)");
        builder.Add("--color-red-200", "oklch(88.5% 0.062 18.334)");
        builder.Add("--color-red-300", "oklch(80.8% 0.114 19.571)");
        builder.Add("--color-red-400", "oklch(70.4% 0.191 22.216)");
        builder.Add("--color-red-500", "oklch(63.7% 0.237 25.331)");
        builder.Add("--color-red-600", "oklch(57.7% 0.245 27.325)");
        builder.Add("--color-red-700", "oklch(50.5% 0.213 27.518)");
        builder.Add("--color-red-800", "oklch(44.4% 0.177 26.899)");
        builder.Add("--color-red-900", "oklch(39.6% 0.141 25.723)");
        builder.Add("--color-red-950", "oklch(25.8% 0.092 26.042)");

        // Colors - Orange
        builder.Add("--color-orange-50", "oklch(98% 0.016 73.684)");
        builder.Add("--color-orange-100", "oklch(95.4% 0.038 75.164)");
        builder.Add("--color-orange-200", "oklch(90.1% 0.076 70.697)");
        builder.Add("--color-orange-300", "oklch(83.7% 0.128 66.29)");
        builder.Add("--color-orange-400", "oklch(75% 0.183 55.934)");
        builder.Add("--color-orange-500", "oklch(70.5% 0.213 47.604)");
        builder.Add("--color-orange-600", "oklch(64.6% 0.222 41.116)");
        builder.Add("--color-orange-700", "oklch(55.3% 0.195 38.402)");
        builder.Add("--color-orange-800", "oklch(47% 0.157 37.304)");
        builder.Add("--color-orange-900", "oklch(40.8% 0.123 38.172)");
        builder.Add("--color-orange-950", "oklch(26.6% 0.079 36.259)");

        // Colors - Amber
        builder.Add("--color-amber-50", "oklch(98.7% 0.022 95.277)");
        builder.Add("--color-amber-100", "oklch(96.2% 0.059 95.617)");
        builder.Add("--color-amber-200", "oklch(92.4% 0.12 95.746)");
        builder.Add("--color-amber-300", "oklch(87.9% 0.169 91.605)");
        builder.Add("--color-amber-400", "oklch(82.8% 0.189 84.429)");
        builder.Add("--color-amber-500", "oklch(76.9% 0.188 70.08)");
        builder.Add("--color-amber-600", "oklch(66.6% 0.179 58.318)");
        builder.Add("--color-amber-700", "oklch(55.5% 0.163 48.998)");
        builder.Add("--color-amber-800", "oklch(47.3% 0.137 46.201)");
        builder.Add("--color-amber-900", "oklch(41.4% 0.112 45.904)");
        builder.Add("--color-amber-950", "oklch(27.9% 0.077 45.635)");

        // Colors - Yellow
        builder.Add("--color-yellow-50", "oklch(98.7% 0.026 102.212)");
        builder.Add("--color-yellow-100", "oklch(97.3% 0.071 103.193)");
        builder.Add("--color-yellow-200", "oklch(94.5% 0.129 101.54)");
        builder.Add("--color-yellow-300", "oklch(90.5% 0.182 98.111)");
        builder.Add("--color-yellow-400", "oklch(85.2% 0.199 91.936)");
        builder.Add("--color-yellow-500", "oklch(79.5% 0.184 86.047)");
        builder.Add("--color-yellow-600", "oklch(68.1% 0.162 75.834)");
        builder.Add("--color-yellow-700", "oklch(55.4% 0.135 66.442)");
        builder.Add("--color-yellow-800", "oklch(47.6% 0.114 61.907)");
        builder.Add("--color-yellow-900", "oklch(42.1% 0.095 57.708)");
        builder.Add("--color-yellow-950", "oklch(28.6% 0.066 53.813)");

        // Colors - Lime
        builder.Add("--color-lime-50", "oklch(98.6% 0.031 120.757)");
        builder.Add("--color-lime-100", "oklch(96.7% 0.067 122.328)");
        builder.Add("--color-lime-200", "oklch(93.8% 0.127 124.321)");
        builder.Add("--color-lime-300", "oklch(89.7% 0.196 126.665)");
        builder.Add("--color-lime-400", "oklch(84.1% 0.238 128.85)");
        builder.Add("--color-lime-500", "oklch(76.8% 0.233 130.85)");
        builder.Add("--color-lime-600", "oklch(64.8% 0.2 131.684)");
        builder.Add("--color-lime-700", "oklch(53.2% 0.157 131.589)");
        builder.Add("--color-lime-800", "oklch(45.3% 0.124 130.933)");
        builder.Add("--color-lime-900", "oklch(40.5% 0.101 131.063)");
        builder.Add("--color-lime-950", "oklch(27.4% 0.072 132.109)");

        // Colors - Green
        builder.Add("--color-green-50", "oklch(98.2% 0.018 155.826)");
        builder.Add("--color-green-100", "oklch(96.2% 0.044 156.743)");
        builder.Add("--color-green-200", "oklch(92.5% 0.084 155.995)");
        builder.Add("--color-green-300", "oklch(87.1% 0.15 154.449)");
        builder.Add("--color-green-400", "oklch(79.2% 0.209 151.711)");
        builder.Add("--color-green-500", "oklch(72.3% 0.219 149.579)");
        builder.Add("--color-green-600", "oklch(62.7% 0.194 149.214)");
        builder.Add("--color-green-700", "oklch(52.7% 0.154 150.069)");
        builder.Add("--color-green-800", "oklch(44.8% 0.119 151.328)");
        builder.Add("--color-green-900", "oklch(39.3% 0.095 152.535)");
        builder.Add("--color-green-950", "oklch(26.6% 0.065 152.934)");

        // Colors - Emerald
        builder.Add("--color-emerald-50", "oklch(97.9% 0.021 166.113)");
        builder.Add("--color-emerald-100", "oklch(95% 0.052 163.051)");
        builder.Add("--color-emerald-200", "oklch(90.5% 0.093 164.15)");
        builder.Add("--color-emerald-300", "oklch(84.5% 0.143 164.978)");
        builder.Add("--color-emerald-400", "oklch(76.5% 0.177 163.223)");
        builder.Add("--color-emerald-500", "oklch(69.6% 0.17 162.48)");
        builder.Add("--color-emerald-600", "oklch(59.6% 0.145 163.225)");
        builder.Add("--color-emerald-700", "oklch(50.8% 0.118 165.612)");
        builder.Add("--color-emerald-800", "oklch(43.2% 0.095 166.913)");
        builder.Add("--color-emerald-900", "oklch(37.8% 0.077 168.94)");
        builder.Add("--color-emerald-950", "oklch(26.2% 0.051 172.552)");

        // Colors - Teal
        builder.Add("--color-teal-50", "oklch(98.4% 0.014 180.72)");
        builder.Add("--color-teal-100", "oklch(95.3% 0.051 180.801)");
        builder.Add("--color-teal-200", "oklch(91% 0.096 180.426)");
        builder.Add("--color-teal-300", "oklch(85.5% 0.138 181.071)");
        builder.Add("--color-teal-400", "oklch(77.7% 0.152 181.912)");
        builder.Add("--color-teal-500", "oklch(70.4% 0.14 182.503)");
        builder.Add("--color-teal-600", "oklch(60% 0.118 184.704)");
        builder.Add("--color-teal-700", "oklch(51.1% 0.096 186.391)");
        builder.Add("--color-teal-800", "oklch(43.7% 0.078 188.216)");
        builder.Add("--color-teal-900", "oklch(38.6% 0.063 188.416)");
        builder.Add("--color-teal-950", "oklch(27.7% 0.046 192.524)");

        // Colors - Cyan
        builder.Add("--color-cyan-50", "oklch(98.4% 0.019 200.873)");
        builder.Add("--color-cyan-100", "oklch(95.6% 0.045 203.388)");
        builder.Add("--color-cyan-200", "oklch(91.7% 0.08 205.041)");
        builder.Add("--color-cyan-300", "oklch(86.5% 0.127 207.078)");
        builder.Add("--color-cyan-400", "oklch(78.9% 0.154 211.53)");
        builder.Add("--color-cyan-500", "oklch(71.5% 0.143 215.221)");
        builder.Add("--color-cyan-600", "oklch(60.9% 0.126 221.723)");
        builder.Add("--color-cyan-700", "oklch(52% 0.105 223.128)");
        builder.Add("--color-cyan-800", "oklch(45% 0.085 224.283)");
        builder.Add("--color-cyan-900", "oklch(39.8% 0.07 227.392)");
        builder.Add("--color-cyan-950", "oklch(30.2% 0.056 229.695)");

        // Colors - Sky
        builder.Add("--color-sky-50", "oklch(97.7% 0.013 236.62)");
        builder.Add("--color-sky-100", "oklch(95.1% 0.026 236.824)");
        builder.Add("--color-sky-200", "oklch(90.1% 0.058 230.902)");
        builder.Add("--color-sky-300", "oklch(82.8% 0.111 230.318)");
        builder.Add("--color-sky-400", "oklch(74.6% 0.16 232.661)");
        builder.Add("--color-sky-500", "oklch(68.5% 0.169 237.323)");
        builder.Add("--color-sky-600", "oklch(58.8% 0.158 241.966)");
        builder.Add("--color-sky-700", "oklch(50% 0.134 242.749)");
        builder.Add("--color-sky-800", "oklch(44.3% 0.11 240.79)");
        builder.Add("--color-sky-900", "oklch(39.1% 0.09 240.876)");
        builder.Add("--color-sky-950", "oklch(29.3% 0.066 243.157)");

        // Colors - Blue
        builder.Add("--color-blue-50", "oklch(97% 0.014 254.604)");
        builder.Add("--color-blue-100", "oklch(93.2% 0.032 255.585)");
        builder.Add("--color-blue-200", "oklch(88.2% 0.059 254.128)");
        builder.Add("--color-blue-300", "oklch(80.9% 0.105 251.813)");
        builder.Add("--color-blue-400", "oklch(70.7% 0.165 254.624)");
        builder.Add("--color-blue-500", "oklch(62.3% 0.214 259.815)");
        builder.Add("--color-blue-600", "oklch(54.6% 0.245 262.881)");
        builder.Add("--color-blue-700", "oklch(48.8% 0.243 264.376)");
        builder.Add("--color-blue-800", "oklch(42.4% 0.199 265.638)");
        builder.Add("--color-blue-900", "oklch(37.9% 0.146 265.522)");
        builder.Add("--color-blue-950", "oklch(28.2% 0.091 267.935)");

        // Colors - Indigo
        builder.Add("--color-indigo-50", "oklch(96.2% 0.018 272.314)");
        builder.Add("--color-indigo-100", "oklch(93% 0.034 272.788)");
        builder.Add("--color-indigo-200", "oklch(87% 0.065 274.039)");
        builder.Add("--color-indigo-300", "oklch(78.5% 0.115 274.713)");
        builder.Add("--color-indigo-400", "oklch(67.3% 0.182 276.935)");
        builder.Add("--color-indigo-500", "oklch(58.5% 0.233 277.117)");
        builder.Add("--color-indigo-600", "oklch(51.1% 0.262 276.966)");
        builder.Add("--color-indigo-700", "oklch(45.7% 0.24 277.023)");
        builder.Add("--color-indigo-800", "oklch(39.8% 0.195 277.366)");
        builder.Add("--color-indigo-900", "oklch(35.9% 0.144 278.697)");
        builder.Add("--color-indigo-950", "oklch(25.7% 0.09 281.288)");

        // Colors - Violet
        builder.Add("--color-violet-50", "oklch(96.9% 0.016 293.756)");
        builder.Add("--color-violet-100", "oklch(94.3% 0.029 294.588)");
        builder.Add("--color-violet-200", "oklch(89.4% 0.057 293.283)");
        builder.Add("--color-violet-300", "oklch(81.1% 0.111 293.571)");
        builder.Add("--color-violet-400", "oklch(70.2% 0.183 293.541)");
        builder.Add("--color-violet-500", "oklch(60.6% 0.25 292.717)");
        builder.Add("--color-violet-600", "oklch(54.1% 0.281 293.009)");
        builder.Add("--color-violet-700", "oklch(49.1% 0.27 292.581)");
        builder.Add("--color-violet-800", "oklch(43.2% 0.232 292.759)");
        builder.Add("--color-violet-900", "oklch(38% 0.189 293.745)");
        builder.Add("--color-violet-950", "oklch(28.3% 0.141 291.089)");

        // Colors - Purple
        builder.Add("--color-purple-50", "oklch(97.7% 0.014 308.299)");
        builder.Add("--color-purple-100", "oklch(94.6% 0.033 307.174)");
        builder.Add("--color-purple-200", "oklch(90.2% 0.063 306.703)");
        builder.Add("--color-purple-300", "oklch(82.7% 0.119 306.383)");
        builder.Add("--color-purple-400", "oklch(71.4% 0.203 305.504)");
        builder.Add("--color-purple-500", "oklch(62.7% 0.265 303.9)");
        builder.Add("--color-purple-600", "oklch(55.8% 0.288 302.321)");
        builder.Add("--color-purple-700", "oklch(49.6% 0.265 301.924)");
        builder.Add("--color-purple-800", "oklch(43.8% 0.218 303.724)");
        builder.Add("--color-purple-900", "oklch(38.1% 0.176 304.987)");
        builder.Add("--color-purple-950", "oklch(29.1% 0.149 302.717)");

        // Colors - Fuchsia
        builder.Add("--color-fuchsia-50", "oklch(97.7% 0.017 320.058)");
        builder.Add("--color-fuchsia-100", "oklch(95.2% 0.037 318.852)");
        builder.Add("--color-fuchsia-200", "oklch(90.3% 0.076 319.62)");
        builder.Add("--color-fuchsia-300", "oklch(83.3% 0.145 321.434)");
        builder.Add("--color-fuchsia-400", "oklch(74% 0.238 322.16)");
        builder.Add("--color-fuchsia-500", "oklch(66.7% 0.295 322.15)");
        builder.Add("--color-fuchsia-600", "oklch(59.1% 0.293 322.896)");
        builder.Add("--color-fuchsia-700", "oklch(51.8% 0.253 323.949)");
        builder.Add("--color-fuchsia-800", "oklch(45.2% 0.211 324.591)");
        builder.Add("--color-fuchsia-900", "oklch(40.1% 0.17 325.612)");
        builder.Add("--color-fuchsia-950", "oklch(29.3% 0.136 325.661)");

        // Colors - Pink
        builder.Add("--color-pink-50", "oklch(97.1% 0.014 343.198)");
        builder.Add("--color-pink-100", "oklch(94.8% 0.028 342.258)");
        builder.Add("--color-pink-200", "oklch(89.9% 0.061 343.231)");
        builder.Add("--color-pink-300", "oklch(82.3% 0.12 346.018)");
        builder.Add("--color-pink-400", "oklch(71.8% 0.202 349.761)");
        builder.Add("--color-pink-500", "oklch(65.6% 0.241 354.308)");
        builder.Add("--color-pink-600", "oklch(59.2% 0.249 0.584)");
        builder.Add("--color-pink-700", "oklch(52.5% 0.223 3.958)");
        builder.Add("--color-pink-800", "oklch(45.9% 0.187 3.815)");
        builder.Add("--color-pink-900", "oklch(40.8% 0.153 2.432)");
        builder.Add("--color-pink-950", "oklch(28.4% 0.109 3.907)");

        // Colors - Rose
        builder.Add("--color-rose-50", "oklch(96.9% 0.015 12.422)");
        builder.Add("--color-rose-100", "oklch(94.1% 0.03 12.58)");
        builder.Add("--color-rose-200", "oklch(89.2% 0.058 10.001)");
        builder.Add("--color-rose-300", "oklch(81% 0.117 11.638)");
        builder.Add("--color-rose-400", "oklch(71.2% 0.194 13.428)");
        builder.Add("--color-rose-500", "oklch(64.5% 0.246 16.439)");
        builder.Add("--color-rose-600", "oklch(58.6% 0.253 17.585)");
        builder.Add("--color-rose-700", "oklch(51.4% 0.222 16.935)");
        builder.Add("--color-rose-800", "oklch(45.5% 0.188 13.697)");
        builder.Add("--color-rose-900", "oklch(41% 0.159 10.272)");
        builder.Add("--color-rose-950", "oklch(27.1% 0.105 12.094)");

        // Colors - Slate
        builder.Add("--color-slate-50", "oklch(98.4% 0.003 247.858)");
        builder.Add("--color-slate-100", "oklch(96.8% 0.007 247.896)");
        builder.Add("--color-slate-200", "oklch(92.9% 0.013 255.508)");
        builder.Add("--color-slate-300", "oklch(86.9% 0.022 252.894)");
        builder.Add("--color-slate-400", "oklch(70.4% 0.04 256.788)");
        builder.Add("--color-slate-500", "oklch(55.4% 0.046 257.417)");
        builder.Add("--color-slate-600", "oklch(44.6% 0.043 257.281)");
        builder.Add("--color-slate-700", "oklch(37.2% 0.044 257.287)");
        builder.Add("--color-slate-800", "oklch(27.9% 0.041 260.031)");
        builder.Add("--color-slate-900", "oklch(20.8% 0.042 265.755)");
        builder.Add("--color-slate-950", "oklch(12.9% 0.042 264.695)");

        // Colors - Gray
        builder.Add("--color-gray-50", "oklch(98.5% 0.002 247.839)");
        builder.Add("--color-gray-100", "oklch(96.7% 0.003 264.542)");
        builder.Add("--color-gray-200", "oklch(92.8% 0.006 264.531)");
        builder.Add("--color-gray-300", "oklch(87.2% 0.01 258.338)");
        builder.Add("--color-gray-400", "oklch(70.7% 0.022 261.325)");
        builder.Add("--color-gray-500", "oklch(55.1% 0.027 264.364)");
        builder.Add("--color-gray-600", "oklch(44.6% 0.03 256.802)");
        builder.Add("--color-gray-700", "oklch(37.3% 0.034 259.733)");
        builder.Add("--color-gray-800", "oklch(27.8% 0.033 256.848)");
        builder.Add("--color-gray-900", "oklch(21% 0.034 264.665)");
        builder.Add("--color-gray-950", "oklch(13% 0.028 261.692)");

        // Colors - Zinc
        builder.Add("--color-zinc-50", "oklch(98.5% 0 0)");
        builder.Add("--color-zinc-100", "oklch(96.7% 0.001 286.375)");
        builder.Add("--color-zinc-200", "oklch(92% 0.004 286.32)");
        builder.Add("--color-zinc-300", "oklch(87.1% 0.006 286.286)");
        builder.Add("--color-zinc-400", "oklch(70.5% 0.015 286.067)");
        builder.Add("--color-zinc-500", "oklch(55.2% 0.016 285.938)");
        builder.Add("--color-zinc-600", "oklch(44.2% 0.017 285.786)");
        builder.Add("--color-zinc-700", "oklch(37% 0.013 285.805)");
        builder.Add("--color-zinc-800", "oklch(27.4% 0.006 286.033)");
        builder.Add("--color-zinc-900", "oklch(21% 0.006 285.885)");
        builder.Add("--color-zinc-950", "oklch(14.1% 0.005 285.823)");

        // Colors - Neutral
        builder.Add("--color-neutral-50", "oklch(98.5% 0 0)");
        builder.Add("--color-neutral-100", "oklch(97% 0 0)");
        builder.Add("--color-neutral-200", "oklch(92.2% 0 0)");
        builder.Add("--color-neutral-300", "oklch(87% 0 0)");
        builder.Add("--color-neutral-400", "oklch(70.8% 0 0)");
        builder.Add("--color-neutral-500", "oklch(55.6% 0 0)");
        builder.Add("--color-neutral-600", "oklch(43.9% 0 0)");
        builder.Add("--color-neutral-700", "oklch(37.1% 0 0)");
        builder.Add("--color-neutral-800", "oklch(26.9% 0 0)");
        builder.Add("--color-neutral-900", "oklch(20.5% 0 0)");
        builder.Add("--color-neutral-950", "oklch(14.5% 0 0)");

        // Colors - Stone
        builder.Add("--color-stone-50", "oklch(98.5% 0.001 106.423)");
        builder.Add("--color-stone-100", "oklch(97% 0.001 106.424)");
        builder.Add("--color-stone-200", "oklch(92.3% 0.003 48.717)");
        builder.Add("--color-stone-300", "oklch(86.9% 0.005 56.366)");
        builder.Add("--color-stone-400", "oklch(70.9% 0.01 56.259)");
        builder.Add("--color-stone-500", "oklch(55.3% 0.013 58.071)");
        builder.Add("--color-stone-600", "oklch(44.4% 0.011 73.639)");
        builder.Add("--color-stone-700", "oklch(37.4% 0.01 67.558)");
        builder.Add("--color-stone-800", "oklch(26.8% 0.007 34.298)");
        builder.Add("--color-stone-900", "oklch(21.6% 0.006 56.043)");
        builder.Add("--color-stone-950", "oklch(14.7% 0.004 49.25)");

        // Special colors
        builder.Add("--color-black", "#000");
        builder.Add("--color-white", "#fff");

        // Spacing
        builder.Add("--spacing", "0.25rem");

        // Breakpoints
        builder.Add("--breakpoint-sm", "40rem");
        builder.Add("--breakpoint-md", "48rem");
        builder.Add("--breakpoint-lg", "64rem");
        builder.Add("--breakpoint-xl", "80rem");
        builder.Add("--breakpoint-2xl", "96rem");

        // Containers
        builder.Add("--container-3xs", "16rem");
        builder.Add("--container-2xs", "18rem");
        builder.Add("--container-xs", "20rem");
        builder.Add("--container-sm", "24rem");
        builder.Add("--container-md", "28rem");
        builder.Add("--container-lg", "32rem");
        builder.Add("--container-xl", "36rem");
        builder.Add("--container-2xl", "42rem");
        builder.Add("--container-3xl", "48rem");
        builder.Add("--container-4xl", "56rem");
        builder.Add("--container-5xl", "64rem");
        builder.Add("--container-6xl", "72rem");
        builder.Add("--container-7xl", "80rem");

        // Text sizes with line heights
        builder.Add("--text-xs", "0.75rem");
        builder.Add("--text-xs--line-height", "calc(1 / 0.75)");
        builder.Add("--text-sm", "0.875rem");
        builder.Add("--text-sm--line-height", "calc(1.25 / 0.875)");
        builder.Add("--text-base", "1rem");
        builder.Add("--text-base--line-height", "calc(1.5 / 1)");
        builder.Add("--text-lg", "1.125rem");
        builder.Add("--text-lg--line-height", "calc(1.75 / 1.125)");
        builder.Add("--text-xl", "1.25rem");
        builder.Add("--text-xl--line-height", "calc(1.75 / 1.25)");
        builder.Add("--text-2xl", "1.5rem");
        builder.Add("--text-2xl--line-height", "calc(2 / 1.5)");
        builder.Add("--text-3xl", "1.875rem");
        builder.Add("--text-3xl--line-height", "calc(2.25 / 1.875)");
        builder.Add("--text-4xl", "2.25rem");
        builder.Add("--text-4xl--line-height", "calc(2.5 / 2.25)");
        builder.Add("--text-5xl", "3rem");
        builder.Add("--text-5xl--line-height", "1");
        builder.Add("--text-6xl", "3.75rem");
        builder.Add("--text-6xl--line-height", "1");
        builder.Add("--text-7xl", "4.5rem");
        builder.Add("--text-7xl--line-height", "1");
        builder.Add("--text-8xl", "6rem");
        builder.Add("--text-8xl--line-height", "1");
        builder.Add("--text-9xl", "8rem");
        builder.Add("--text-9xl--line-height", "1");

        // Font weights
        builder.Add("--font-weight-thin", "100");
        builder.Add("--font-weight-extralight", "200");
        builder.Add("--font-weight-light", "300");
        builder.Add("--font-weight-normal", "400");
        builder.Add("--font-weight-medium", "500");
        builder.Add("--font-weight-semibold", "600");
        builder.Add("--font-weight-bold", "700");
        builder.Add("--font-weight-extrabold", "800");
        builder.Add("--font-weight-black", "900");

        // Letter spacing (tracking)
        builder.Add("--tracking-tighter", "-0.05em");
        builder.Add("--tracking-tight", "-0.025em");
        builder.Add("--tracking-normal", "0em");
        builder.Add("--tracking-wide", "0.025em");
        builder.Add("--tracking-wider", "0.05em");
        builder.Add("--tracking-widest", "0.1em");

        // Line heights (leading)
        builder.Add("--leading-tight", "1.25");
        builder.Add("--leading-snug", "1.375");
        builder.Add("--leading-normal", "1.5");
        builder.Add("--leading-relaxed", "1.625");
        builder.Add("--leading-loose", "2");

        // Font stretch properties removed - utilities now generate inline values

        // Border radius
        builder.Add("--radius-xs", "0.125rem");
        builder.Add("--radius-sm", "0.25rem");
        builder.Add("--radius-md", "0.375rem");
        builder.Add("--radius-lg", "0.5rem");
        builder.Add("--radius-xl", "0.75rem");
        builder.Add("--radius-2xl", "1rem");
        builder.Add("--radius-3xl", "1.5rem");
        builder.Add("--radius-4xl", "2rem");

        // Box shadows
        builder.Add("--shadow-2xs", "0 1px rgb(0 0 0 / 0.05)");
        builder.Add("--shadow-xs", "0 1px 2px 0 rgb(0 0 0 / 0.05)");
        builder.Add("--shadow-sm", "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)");
        builder.Add("--shadow-md", "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)");
        builder.Add("--shadow-lg", "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)");
        builder.Add("--shadow-xl", "0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)");
        builder.Add("--shadow-2xl", "0 25px 50px -12px rgb(0 0 0 / 0.25)");

        // Inset shadows
        builder.Add("--inset-shadow-2xs", "inset 0 1px rgb(0 0 0 / 0.05)");
        builder.Add("--inset-shadow-xs", "inset 0 1px 1px rgb(0 0 0 / 0.05)");
        builder.Add("--inset-shadow-sm", "inset 0 2px 4px rgb(0 0 0 / 0.05)");

        // Drop shadows
        builder.Add("--drop-shadow-xs", "0 1px 1px rgb(0 0 0 / 0.05)");
        builder.Add("--drop-shadow-sm", "0 1px 2px rgb(0 0 0 / 0.15)");
        builder.Add("--drop-shadow-md", "0 3px 3px rgb(0 0 0 / 0.12)");
        builder.Add("--drop-shadow-lg", "0 4px 4px rgb(0 0 0 / 0.15)");
        builder.Add("--drop-shadow-xl", "0 9px 7px rgb(0 0 0 / 0.1)");
        builder.Add("--drop-shadow-2xl", "0 25px 25px rgb(0 0 0 / 0.15)");

        // Text shadows
        builder.Add("--text-shadow-2xs", "0px 1px 0px rgb(0 0 0 / 0.15)");
        builder.Add("--text-shadow-xs", "0px 1px 1px rgb(0 0 0 / 0.2)");
        builder.Add("--text-shadow-sm", "0px 1px 0px rgb(0 0 0 / 0.075), 0px 1px 1px rgb(0 0 0 / 0.075), 0px 2px 2px rgb(0 0 0 / 0.075)");
        builder.Add("--text-shadow-md", "0px 1px 1px rgb(0 0 0 / 0.1), 0px 1px 2px rgb(0 0 0 / 0.1), 0px 2px 4px rgb(0 0 0 / 0.1)");
        builder.Add("--text-shadow-lg", "0px 1px 2px rgb(0 0 0 / 0.1), 0px 3px 2px rgb(0 0 0 / 0.1), 0px 4px 8px rgb(0 0 0 / 0.1)");

        // Easing functions
        builder.Add("--ease-in", "cubic-bezier(0.4, 0, 1, 1)");
        builder.Add("--ease-out", "cubic-bezier(0, 0, 0.2, 1)");
        builder.Add("--ease-in-out", "cubic-bezier(0.4, 0, 0.2, 1)");

        // Animations
        builder.Add("--animate-spin", "spin 1s linear infinite");
        builder.Add("--animate-ping", "ping 1s cubic-bezier(0, 0, 0.2, 1) infinite");
        builder.Add("--animate-pulse", "pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite");
        builder.Add("--animate-bounce", "bounce 1s infinite");

        // Blur values
        builder.Add("--blur-xs", "4px");
        builder.Add("--blur-sm", "8px");
        builder.Add("--blur-md", "12px");
        builder.Add("--blur-lg", "16px");
        builder.Add("--blur-xl", "24px");
        builder.Add("--blur-2xl", "40px");
        builder.Add("--blur-3xl", "64px");

        // Perspective values
        builder.Add("--perspective-dramatic", "100px");
        builder.Add("--perspective-near", "300px");
        builder.Add("--perspective-normal", "500px");
        builder.Add("--perspective-midrange", "800px");
        builder.Add("--perspective-distant", "1200px");

        // Aspect ratios - square removed, video kept for theme compatibility
        builder.Add("--aspect-video", "16 / 9");

        // Default transitions
        builder.Add("--default-transition-duration", "150ms");
        builder.Add("--default-transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)");

        // Grid auto columns and rows removed - utilities now generate inline values

        // Line clamp properties removed - utilities now generate inline values

        // Text decoration thickness properties removed - utilities now generate inline values

        // Background properties removed - utilities now generate inline values

        // Border width properties removed - utilities now generate inline values

        // Deprecated (but included for compatibility)
        builder.Add("--blur", "8px");
        builder.Add("--shadow", "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)");
        builder.Add("--shadow-inner", "inset 0 2px 4px 0 rgb(0 0 0 / 0.05)");
        builder.Add("--drop-shadow", "0 1px 2px rgb(0 0 0 / 0.1), 0 1px 1px rgb(0 0 0 / 0.06)");
        builder.Add("--radius", "0.25rem");
        builder.Add("--max-width-prose", "65ch");

        // Add prose typography defaults
        var proseDefaults = ProseThemeDefaults.GetProseDefaults();
        foreach (var kvp in proseDefaults)
        {
            builder.Add(kvp.Key, kvp.Value);
        }

        return builder.ToImmutable();
    }
}