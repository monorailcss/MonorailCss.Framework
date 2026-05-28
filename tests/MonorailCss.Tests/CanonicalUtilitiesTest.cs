using System.Text.Json;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Test models for the canonical-v4.json structure
/// </summary>
public class CanonicalUtilityData
{
    public string css { get; set; } = string.Empty;
    public List<string> cssProperties { get; set; } = new();
    public Dictionary<string, string>? customProperties { get; set; }
    public List<AtPropertyDefinition>? atProperties { get; set; }
}

public class AtPropertyDefinition
{
    public string name { get; set; } = string.Empty;
    public string syntax { get; set; } = string.Empty;
    public bool inherits { get; set; }
    public string initialValue { get; set; } = string.Empty;
}


public class CanonicalUtilitiesTest(CssFrameworkFixture fixture) : IClassFixture<CssFrameworkFixture>
{
    private readonly CssFramework _cssFramework = fixture.CssFramework;

    [Theory]
    [MemberData(nameof(GetCanonicalTestData))]
    public void CanonicalUtility_ShouldGenerateExpectedCss(string utilityClass, CanonicalUtilityData expectedData)
    {
        var result = _cssFramework.Process(utilityClass);

        Console.WriteLine($"Testing: {utilityClass}");
        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Expected CSS Properties: {string.Join(", ", expectedData.cssProperties)}");

        var normalizedResult = NormalizeCss(result);

        foreach (var expectedProperty in expectedData.cssProperties)
        {
            var containsProperty = normalizedResult.Contains(expectedProperty);
            containsProperty.ShouldBeTrue(
                $"Property '{expectedProperty}' not found in generated CSS for utility '{utilityClass}'");
        }

        if (expectedData.customProperties != null)
        {
            foreach (var customProp in expectedData.customProperties)
            {
                var shouldContainCustomProp = normalizedResult.Contains($"var({customProp.Key})") ||
                                             normalizedResult.Contains($"{customProp.Key}:");
                shouldContainCustomProp.ShouldBeTrue(
                    $"Custom property '{customProp.Key}' not found in generated CSS for utility '{utilityClass}'");
            }
        }

        if (expectedData.css.Contains(":where(") || expectedData.css.Contains(">"))
        {
            var hasSelector = result.Contains("{") && result.Contains("}");
            hasSelector.ShouldBeTrue(
                $"Expected nested selector structure not found in generated CSS for utility '{utilityClass}'");
        }

        if (expectedData.atProperties != null)
        {
            foreach (var atProperty in expectedData.atProperties)
            {
                var propertyDeclaration = $"@property {atProperty.name}";
                var containsPropertyDeclaration = result.Contains(propertyDeclaration);
                containsPropertyDeclaration.ShouldBeTrue(
                    $"@property declaration for '{atProperty.name}' not found in generated CSS for utility '{utilityClass}'");

                var syntaxDeclaration = $"syntax: \"{atProperty.syntax}\"";
                var containsSyntax = result.Contains(syntaxDeclaration);
                containsSyntax.ShouldBeTrue(
                    $"Incorrect syntax for @property '{atProperty.name}' in utility '{utilityClass}'. Expected: {syntaxDeclaration}");

                var inheritsValue = atProperty.inherits ? "true" : "false";
                var inheritsDeclaration = $"inherits: {inheritsValue}";
                var containsInherits = result.Contains(inheritsDeclaration);
                containsInherits.ShouldBeTrue(
                    $"Incorrect inherits value for @property '{atProperty.name}' in utility '{utilityClass}'. Expected: {inheritsDeclaration}");

                if (!string.IsNullOrEmpty(atProperty.initialValue))
                {
                    var initialValueDeclaration = $"initial-value: {atProperty.initialValue}";
                    var containsInitialValue = result.Contains(initialValueDeclaration);
                    containsInitialValue.ShouldBeTrue(
                        $"Incorrect initial-value for @property '{atProperty.name}' in utility '{utilityClass}'. Expected: {initialValueDeclaration}");
                }
            }
        }
    }

    public static IEnumerable<TheoryDataRow<string, CanonicalUtilityData>> GetCanonicalTestData()
    {
        // Load the canonical-v4.json file
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "canonical-v4.json");

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"canonical-v4.json not found at: {jsonPath}");
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var canonicalData = JsonSerializer.Deserialize<Dictionary<string, CanonicalUtilityData>>(jsonContent, options);

        if (canonicalData == null)
        {
            throw new InvalidOperationException("Failed to deserialize canonical-v4.json");
        }

        foreach (var kvp in canonicalData)
        {
            var row = new TheoryDataRow<string, CanonicalUtilityData>(kvp.Key, kvp.Value);

            row.TestDisplayName = $"{kvp.Key}";
            row.Traits.Add("Category", ["Canonical"]);

            // Add trait based on utility prefix for better grouping
            var prefix = kvp.Key.TrimStart('-').Split('-', '/', '[').FirstOrDefault() ?? "misc";
            row.Traits.Add("UtilityType", [prefix]);

            // Add traits for special features
            var features = new HashSet<string>();
            if (kvp.Value.customProperties != null && kvp.Value.customProperties.Count > 0)
            {
                features.Add("CustomProperties");
            }
            if (kvp.Value.atProperties != null && kvp.Value.atProperties.Count > 0)
            {
                features.Add("AtProperties");
            }
            if (kvp.Value.css.Contains(":where("))
            {
                features.Add("NestedSelectors");
            }
            if (features.Count > 0)
            {
                row.Traits.Add("Features", features);
            }

            yield return row;
        }
    }

    private static string NormalizeCss(string css)
    {
        // Remove extra whitespace and normalize line endings
        // But preserve some structure for nested selectors
        var normalized = css
            .Replace("\r\n", "\n")
            .Replace("\n", " ")
            .Replace("\t", " ");

        // Collapse multiple spaces
        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        // Normalize CSS syntax spacing
        normalized = normalized
            .Replace(": ", ":")
            .Replace("; ", ";")
            .Replace(" {", "{")
            .Replace(" }", "}")
            .Replace(",", ", ") // Normalize comma spacing for selectors
            .Trim();

        return normalized;
    }
}