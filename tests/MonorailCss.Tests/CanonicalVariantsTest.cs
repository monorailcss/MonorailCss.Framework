using System.Text.Json;
using Shouldly;

namespace MonorailCss.Tests;

public class CanonicalVariantsTest(CssFrameworkFixture fixture) : IClassFixture<CssFrameworkFixture>
{
    private readonly CssFramework _cssFramework = fixture.CssFramework;

    [Theory]
    [MemberData(nameof(GetCanonicalVariantData))]
    public void CanonicalVariant_ShouldGenerateExpectedCss(string variantClass, CanonicalUtilityData expectedData)
    {
        var result = _cssFramework.Process(variantClass);

        Console.WriteLine($"Testing: {variantClass}");
        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Expected CSS Properties: {string.Join(", ", expectedData.cssProperties)}");

        var normalizedResult = NormalizeCss(result);

        foreach (var expectedProperty in expectedData.cssProperties)
        {
            normalizedResult.Contains(expectedProperty).ShouldBeTrue(
                $"Property '{expectedProperty}' not found in generated CSS for variant '{variantClass}'");
        }

        if (expectedData.customProperties != null)
        {
            foreach (var customProp in expectedData.customProperties)
            {
                var present = normalizedResult.Contains($"var({customProp.Key})") ||
                              normalizedResult.Contains($"{customProp.Key}:");
                present.ShouldBeTrue(
                    $"Custom property '{customProp.Key}' not found in generated CSS for variant '{variantClass}'");
            }
        }
    }

    // Variants where MonorailCss does not yet emit the expected CSS. Each entry
    // is a TODO; removing one means MonorailCss now compiles that variant
    // correctly. Keeps the suite green while the gaps stay visible.
    private static readonly Dictionary<string, string> KnownGaps = new();

    public static IEnumerable<TheoryDataRow<string, CanonicalUtilityData>> GetCanonicalVariantData()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "canonical-variants.json");

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"canonical-variants.json not found at: {jsonPath}");
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var canonicalData = JsonSerializer.Deserialize<Dictionary<string, CanonicalUtilityData>>(jsonContent, options);

        if (canonicalData == null)
        {
            throw new InvalidOperationException("Failed to deserialize canonical-variants.json");
        }

        foreach (var kvp in canonicalData)
        {
            var row = new TheoryDataRow<string, CanonicalUtilityData>(kvp.Key, kvp.Value);
            row.TestDisplayName = kvp.Key;
            row.Skip = KnownGaps.TryGetValue(kvp.Key, out var reason) ? reason : null;
            row.Traits.Add("Category", ["CanonicalVariants"]);

            // Group by variant prefix (everything before the final `:`) so
            // breakpoint/pseudo/compound failures cluster in test reports.
            var lastColon = kvp.Key.LastIndexOf(':');
            var variantPrefix = lastColon > 0 ? kvp.Key[..lastColon] : kvp.Key;
            row.Traits.Add("Variant", [variantPrefix]);

            yield return row;
        }
    }

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
            .Replace(",", ", ")
            .Trim();

        return normalized;
    }
}
