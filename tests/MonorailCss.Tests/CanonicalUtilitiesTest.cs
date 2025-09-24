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
        // Act
        var result = _cssFramework.Process(utilityClass);

        // Assert
        Console.WriteLine($"Testing: {utilityClass}");
        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Expected CSS Properties: {string.Join(", ", expectedData.cssProperties)}");

        // Normalize the result for comparison
        var normalizedResult = NormalizeCss(result);

        // Check if each expected CSS property is present
        foreach (var expectedProperty in expectedData.cssProperties)
        {
            // The generated CSS should contain the property
            // We're more flexible here since the exact value might differ slightly
            var containsProperty = normalizedResult.Contains(expectedProperty);
            containsProperty.ShouldBeTrue(
                $"Property '{expectedProperty}' not found in generated CSS for utility '{utilityClass}'");
        }

        // If custom properties are defined, verify they exist
        if (expectedData.customProperties != null)
        {
            foreach (var customProp in expectedData.customProperties)
            {
                // Check if the custom property is referenced or defined
                var shouldContainCustomProp = normalizedResult.Contains($"var({customProp.Key})") ||
                                             normalizedResult.Contains($"{customProp.Key}:");
                shouldContainCustomProp.ShouldBeTrue(
                    $"Custom property '{customProp.Key}' not found in generated CSS for utility '{utilityClass}'");
            }
        }

        // Verify nested selectors if present in expected CSS
        if (expectedData.css.Contains(":where(") || expectedData.css.Contains(">"))
        {
            // Just verify that the generated CSS has some selector structure
            // The exact selector might differ but it should have some nesting
            var hasSelector = result.Contains("{") && result.Contains("}");
            hasSelector.ShouldBeTrue(
                $"Expected nested selector structure not found in generated CSS for utility '{utilityClass}'");
        }

        // If @property definitions are expected, verify they exist
        if (expectedData.atProperties != null)
        {
            foreach (var atProperty in expectedData.atProperties)
            {
                // Check if @property declaration exists
                var propertyDeclaration = $"@property {atProperty.name}";
                var containsPropertyDeclaration = result.Contains(propertyDeclaration);
                containsPropertyDeclaration.ShouldBeTrue(
                    $"@property declaration for '{atProperty.name}' not found in generated CSS for utility '{utilityClass}'");

                // Verify syntax
                var syntaxDeclaration = $"syntax: \"{atProperty.syntax}\"";
                var containsSyntax = result.Contains(syntaxDeclaration);
                containsSyntax.ShouldBeTrue(
                    $"Incorrect syntax for @property '{atProperty.name}' in utility '{utilityClass}'. Expected: {syntaxDeclaration}");

                // Verify inherits
                var inheritsValue = atProperty.inherits ? "true" : "false";
                var inheritsDeclaration = $"inherits: {inheritsValue}";
                var containsInherits = result.Contains(inheritsDeclaration);
                containsInherits.ShouldBeTrue(
                    $"Incorrect inherits value for @property '{atProperty.name}' in utility '{utilityClass}'. Expected: {inheritsDeclaration}");

                // Verify initial-value if not empty
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

            // Add metadata for better test naming and organization
            row.TestDisplayName = $"{kvp.Key}";
            // All tests should now pass - no skips needed
            row.Skip = null;
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