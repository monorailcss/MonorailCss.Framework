using System.Collections.Concurrent;
using System.Text.Json;
using Shouldly;

namespace MonorailCss.Tests;

public class CanonicalTypographyData
{
    public string css { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<string, string>> elements { get; set; } = new();
    public List<string> cssProperties { get; set; } = new();
}

public class CanonicalTypographyTest(CssFrameworkFixture fixture) : IClassFixture<CssFrameworkFixture>
{
    private readonly CssFramework _cssFramework = fixture.CssFramework;

    private static readonly ConcurrentDictionary<string, string> ProcessCache = new();

    [Theory]
    [MemberData(nameof(GetCanonicalTypographyData))]
    public void CanonicalTypography_ShouldGenerateExpectedCss(
        string utilityClass,
        string elementSelector,
        TypographyDeclarations declarations)
    {
        var css = ProcessCache.GetOrAdd(utilityClass, _cssFramework.Process);

        var blockBody = ExtractBlockBody(css, utilityClass, elementSelector);
        blockBody.ShouldNotBeNull(
            $"Could not locate selector '{elementSelector}' for utility '{utilityClass}'. " +
            $"Generated CSS excerpt: {Excerpt(css)}");

        var normalizedBody = NormalizeCss(blockBody!);

        foreach (var (property, value) in declarations.Values)
        {
            var expected = $"{property}:{value}";
            normalizedBody.Contains(expected).ShouldBeTrue(
                $"Utility '{utilityClass}' selector '{elementSelector}' should declare '{property}: {value}' " +
                $"but block body was: {normalizedBody}");
        }
    }

    // Tracking dictionary for known gaps. Key is "{utilityClass}::{elementSelector}".
    // Removing an entry means MonorailCss now matches Tailwind for that pair.
    private static readonly Dictionary<string, string> KnownGaps = new();

    public static IEnumerable<TheoryDataRow<string, string, TypographyDeclarations>> GetCanonicalTypographyData()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "canonical-typography.json");

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"canonical-typography.json not found at: {jsonPath}");
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<Dictionary<string, CanonicalTypographyData>>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize canonical-typography.json");

        foreach (var (utilityClass, entry) in data)
        {
            foreach (var (selector, declarations) in entry.elements)
            {
                var row = new TheoryDataRow<string, string, TypographyDeclarations>(
                    utilityClass, selector, new TypographyDeclarations(declarations));

                row.TestDisplayName = $"{utilityClass} :: {selector}";

                var gapKey = $"{utilityClass}::{selector}";
                row.Skip = KnownGaps.TryGetValue(gapKey, out var reason) ? reason : null;

                row.Traits.Add("Category", ["CanonicalTypography"]);
                row.Traits.Add("UtilityClass", [utilityClass]);
                row.Traits.Add("Selector", [selector]);

                yield return row;
            }
        }
    }

    private static string? ExtractBlockBody(string css, string utilityClass, string elementSelector)
    {
        if (elementSelector == "root")
        {
            // The bare `.<utilityClass>` block — the one whose opener is `.foo {`
            // with no descendant selector between the class and the brace.
            var pattern = $".{utilityClass} {{";
            var hint = css.IndexOf(pattern, StringComparison.Ordinal);
            if (hint < 0) return null;
            return ExtractBalanced(css, hint + pattern.Length - 1);
        }

        var whereHint = $":where({elementSelector})";
        var idx = css.IndexOf(whereHint, StringComparison.Ordinal);
        if (idx < 0) return null;

        var brace = css.IndexOf('{', idx);
        return brace < 0 ? null : ExtractBalanced(css, brace);
    }

    // Given an index pointing AT '{', returns the substring inside the matching '}'.
    private static string? ExtractBalanced(string css, int openBraceIndex)
    {
        if (openBraceIndex < 0 || openBraceIndex >= css.Length || css[openBraceIndex] != '{')
            return null;

        var depth = 0;
        var bodyStart = openBraceIndex + 1;
        for (var i = openBraceIndex; i < css.Length; i++)
        {
            if (css[i] == '{') depth++;
            else if (css[i] == '}')
            {
                depth--;
                if (depth == 0) return css.Substring(bodyStart, i - bodyStart);
            }
        }
        return null;
    }

    private static string Excerpt(string css) =>
        css.Length <= 400 ? css : css.Substring(0, 400) + "…";

    private static string NormalizeCss(string css)
    {
        var normalized = css
            .Replace("\r\n", "\n")
            .Replace("\n", " ")
            .Replace("\t", " ");

        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        normalized = normalized
            .Replace(": ", ":")
            .Replace("; ", ";")
            .Replace(" {", "{")
            .Replace(" }", "}")
            .Replace(", ", ",")
            .Replace(",", ", ")
            .Trim();

        return normalized;
    }
}
