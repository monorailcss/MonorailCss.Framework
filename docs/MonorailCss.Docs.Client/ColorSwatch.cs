using System.Globalization;

namespace MonorailCss.Docs.Client;

/// <summary>
/// Turns a colour utility (<c>bg-primary-500</c>, <c>text-red-600</c>) into a <c>#rrggbb</c>
/// string for the autocomplete colour swatch. Monaco only paints a swatch when an item's
/// detail is a hex/rgb/hsl colour, but the theme's palettes are <c>oklch()</c> (and the docs'
/// semantic colours are <c>var()</c> aliases of them), so this resolves the alias chain through
/// the theme and converts OKLCH to sRGB. Anything it can't parse simply gets no swatch.
/// </summary>
internal static class ColorSwatch
{
    /// <summary>
    /// Finds the <c>--color-*</c> token a class name ends with and returns it as hex, or null
    /// when the class isn't a colour utility (or the colour isn't a plain oklch/hex value).
    /// </summary>
    public static string? TryGetHex(string className, Theme.Theme theme)
    {
        // Try each suffix after a dash: bg-[primary-500], border-x-[red-500], text-[white].
        for (var dash = className.IndexOf('-'); dash >= 0; dash = className.IndexOf('-', dash + 1))
        {
            var key = "--color-" + className[(dash + 1)..];
            if (theme.Values.TryGetValue(key, out var value))
            {
                return ToHex(theme.ResolveInlineValue(value));
            }
        }

        return null;
    }

    private static string? ToHex(string value)
    {
        value = value.Trim();
        if (value.StartsWith('#'))
        {
            return value;
        }

        if (!value.StartsWith("oklch(", StringComparison.OrdinalIgnoreCase) || !value.EndsWith(')'))
        {
            return null;
        }

        // oklch(L C H [/ alpha]) — alpha is ignored for the swatch.
        var body = value[6..^1];
        var slash = body.IndexOf('/');
        if (slash >= 0)
        {
            body = body[..slash];
        }

        var parts = body.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3
            || !TryParseComponent(parts[0], percentScale: 1.0, out var l)
            || !TryParseComponent(parts[1], percentScale: 0.4, out var c)
            || !TryParseComponent(parts[2], percentScale: 1.0, out var h))
        {
            return null;
        }

        // OKLCH -> OKLab -> linear sRGB (Björn Ottosson's matrices) -> gamma-encoded sRGB.
        var hr = h * Math.PI / 180.0;
        var a = c * Math.Cos(hr);
        var b = c * Math.Sin(hr);

        var l_ = l + (0.3963377774 * a) + (0.2158037573 * b);
        var m_ = l - (0.1055613458 * a) - (0.0638541728 * b);
        var s_ = l - (0.0894841775 * a) - (1.2914855480 * b);

        var lc = l_ * l_ * l_;
        var mc = m_ * m_ * m_;
        var sc = s_ * s_ * s_;

        var r = (4.0767416621 * lc) - (3.3077115913 * mc) + (0.2309699292 * sc);
        var g = (-1.2684380046 * lc) + (2.6097574011 * mc) - (0.3413193965 * sc);
        var bl = (-0.0041960863 * lc) - (0.7034186147 * mc) + (1.7076147010 * sc);

        return $"#{Channel(r):x2}{Channel(g):x2}{Channel(bl):x2}";

        static int Channel(double linear)
        {
            var encoded = linear <= 0.0031308 ? 12.92 * linear : (1.055 * Math.Pow(linear, 1 / 2.4)) - 0.055;
            return (int)Math.Round(Math.Clamp(encoded, 0, 1) * 255);
        }
    }

    // "63.7%" -> 0.637 (for L; C's 100% is 0.4 per CSS Color 4), "0.237" -> 0.237, "25.3deg" -> 25.3.
    private static bool TryParseComponent(string text, double percentScale, out double value)
    {
        if (text.EndsWith('%'))
        {
            var ok = double.TryParse(text[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var pct);
            value = pct / 100.0 * percentScale;
            return ok;
        }

        if (text.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            text = text[..^3];
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
