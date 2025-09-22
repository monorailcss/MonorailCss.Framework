using System.Text.Json;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Tests for canonical utilities loaded from canonical-v4.json file.
/// Uses xUnit 3 Theory Data Rows with metadata for better test organization.
/// </summary>
public class CanonicalUtilitiesTest(CssFrameworkFixture fixture) : IClassFixture<CssFrameworkFixture>
{
    private readonly CssFramework _cssFramework = fixture.CssFramework;

    [Theory]
    [MemberData(nameof(GetCanonicalTestData))]
    public void CanonicalUtility_ShouldGenerateExpectedCss(string utilityClass, Dictionary<string, string> expectedProperties)
    {
        // Act
        var result = _cssFramework.Process(utilityClass);

        // Assert
        Console.WriteLine(result);

        // Normalize the result for comparison
        var normalizedResult = NormalizeCss(result);

        // Check if each expected CSS property is present with its value
        foreach (var expectedProperty in expectedProperties)
        {
            var expectedDeclaration = $"{expectedProperty.Key}:{NormalizeValue(expectedProperty.Value)}";
            normalizedResult.ShouldContain(expectedDeclaration);
        }
    }

    public static IEnumerable<TheoryDataRow<string, Dictionary<string, string>>> GetCanonicalTestData()
    {
        // Load the canonical-v4.json file
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "canonical-v4.json");

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"canonical-v4.json not found at: {jsonPath}");
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var canonicalData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

        if (canonicalData == null)
        {
            throw new InvalidOperationException("Failed to deserialize canonical-v4.json");
        }

        foreach (var kvp in canonicalData)
        {
            var row = new TheoryDataRow<string, Dictionary<string, string>>(kvp.Key, kvp.Value);

            // Add metadata for better test naming and organization
            row.TestDisplayName = $"{kvp.Key}";
            row.Skip = null; // Can be used to skip specific tests if needed
            row.Traits.Add("Category", ["Canonical"]);

            // Add trait based on utility prefix for better grouping
            var prefix = kvp.Key.TrimStart('-').Split('-', '/', '[').FirstOrDefault() ?? "misc";
            row.Traits.Add("UtilityType", [prefix]);

            yield return row;
        }
    }

    private static string NormalizeCss(string css)
    {
        // Remove extra whitespace and normalize line endings
        return css
            .Replace("\r\n", "\n")
            .Replace("\n", " ")
            .Replace("  ", " ")
            .Replace(": ", ":")
            .Replace("; ", ";")
            .Replace(" {", "{")
            .Replace(" }", "}")
            .Trim();
    }

    private static string NormalizeValue(string value)
    {
        // Remove extra whitespace from the value
        return value.Trim()
            .Replace("  ", " ");
    }
}