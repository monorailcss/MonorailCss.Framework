using System.Collections.Immutable;

namespace MonorailCss.Theme;

/// <summary>
/// CSS <c>@keyframes</c> definitions that the default theme's <c>--animate-*</c>
/// variables reference (<c>spin</c>, <c>ping</c>, <c>pulse</c>, <c>bounce</c>).
/// </summary>
/// <remarks>
/// Tailwind v4 ships these inside its <c>@theme</c> block so the rule set lives next
/// to the variables that name them. MonorailCss exposes the keyframes through this
/// dictionary so the CSS generator can emit them on demand — once a class like
/// <c>animate-spin</c> is used, the theme variable <c>--animate-spin</c> becomes part
/// of the output, and the matching keyframes are hoisted to the top level alongside it.
/// </remarks>
internal static class DefaultKeyframes
{
    /// <summary>Animation name → keyframes body (the part inside the braces).</summary>
    public static readonly ImmutableDictionary<string, string> Defaults =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["spin"] = "to { transform: rotate(360deg); }",
            ["ping"] = "75%, 100% { transform: scale(2); opacity: 0; }",
            ["pulse"] = "50% { opacity: 0.5; }",
            ["bounce"] =
                "0%, 100% {\n" +
                "    transform: translateY(-25%);\n" +
                "    animation-timing-function: cubic-bezier(0.8, 0, 1, 1);\n" +
                "  }\n" +
                "  50% {\n" +
                "    transform: none;\n" +
                "    animation-timing-function: cubic-bezier(0, 0, 0.2, 1);\n" +
                "  }",
        }.ToImmutableDictionary();

    /// <summary>
    /// Maps a theme variable name (e.g. <c>--animate-spin</c>) to the animation name
    /// it references (e.g. <c>spin</c>) when one of the defaults is in use.
    /// </summary>
    /// <param name="themeVariable">A theme variable key (with <c>--</c> prefix).</param>
    /// <returns>The animation name, or null when this variable doesn't correspond to a default.</returns>
    public static string? AnimationNameFor(string themeVariable)
    {
        const string prefix = "--animate-";
        if (!themeVariable.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var name = themeVariable[prefix.Length..];
        return Defaults.ContainsKey(name) ? name : null;
    }
}
