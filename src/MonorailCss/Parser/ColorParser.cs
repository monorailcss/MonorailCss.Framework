namespace MonorailCss.Parser;

internal static class ColorParser
{
    public static (string Color, string? Opacity) SplitColor(string value)
    {
        var split = value.Split('/');
        return split.Length == 2 ? (split[0], split[1]) : (value, null);
    }
}