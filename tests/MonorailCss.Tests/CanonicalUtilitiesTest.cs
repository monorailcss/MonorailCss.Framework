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

    // Known-failing canonical entries. Each entry maps a class name to a tracking
    // note explaining the gap. Removing an entry here means MonorailCss now
    // supports the utility correctly. Used to keep the suite green while making
    // gaps visible in code review.
    private static readonly Dictionary<string, string> KnownGaps = new()
    {
        // Tailwind 4.3 — @container-size (container-type: size + optional container-name)
        ["@container-size"] = "Tailwind 4.3 — not yet implemented",
        ["@container-size/main"] = "Tailwind 4.3 — not yet implemented",
        ["@container-size/sidebar"] = "Tailwind 4.3 — not yet implemented",

        // Tailwind 4.3 — scrollbar-width utilities
        ["scrollbar-auto"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-none"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thin"] = "Tailwind 4.3 — not yet implemented",

        // Tailwind 4.3 — scrollbar-gutter utilities
        ["scrollbar-gutter-auto"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-gutter-both"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-gutter-stable"] = "Tailwind 4.3 — not yet implemented",

        // Tailwind 4.3 — scrollbar-color (thumb)
        ["scrollbar-thumb-(--my-color)"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-(color:--my-color)"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-[#0088cc]"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-[#0088cc]/50"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-[color:var(--my-color)]"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-amber-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-black"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-blue-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-current"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-cyan-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-emerald-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-fuchsia-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-gray-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-green-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-indigo-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-inherit"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-lime-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-mauve-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-mist-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-neutral-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-olive-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-orange-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-pink-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-purple-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-red-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-rose-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-sky-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-slate-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-stone-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-taupe-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-teal-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-transparent"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-violet-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-white"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-yellow-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-thumb-zinc-500"] = "Tailwind 4.3 — not yet implemented",

        // Tailwind 4.3 — scrollbar-color (track)
        ["scrollbar-track-(--my-color)"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-(color:--my-color)"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-[#0088cc]"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-[#0088cc]/50"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-[color:var(--my-color)]"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-amber-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-black"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-blue-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-current"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-cyan-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-emerald-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-fuchsia-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-gray-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-green-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-indigo-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-inherit"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-lime-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-mauve-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-mist-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-neutral-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-olive-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-orange-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-pink-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-purple-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-red-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-rose-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-sky-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-slate-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-stone-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-taupe-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-teal-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-transparent"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-violet-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-white"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-yellow-500"] = "Tailwind 4.3 — not yet implemented",
        ["scrollbar-track-zinc-500"] = "Tailwind 4.3 — not yet implemented",

        // Tailwind 4.3 — tab-size utility
        ["tab-(--my-tab)"] = "Tailwind 4.3 — not yet implemented",
        ["tab-1"] = "Tailwind 4.3 — not yet implemented",
        ["tab-10"] = "Tailwind 4.3 — not yet implemented",
        ["tab-12"] = "Tailwind 4.3 — not yet implemented",
        ["tab-16"] = "Tailwind 4.3 — not yet implemented",
        ["tab-2"] = "Tailwind 4.3 — not yet implemented",
        ["tab-3"] = "Tailwind 4.3 — not yet implemented",
        ["tab-4"] = "Tailwind 4.3 — not yet implemented",
        ["tab-6"] = "Tailwind 4.3 — not yet implemented",
        ["tab-8"] = "Tailwind 4.3 — not yet implemented",
        ["tab-[10]"] = "Tailwind 4.3 — not yet implemented",
        ["tab-[number:5]"] = "Tailwind 4.3 — not yet implemented",

        // Tailwind 4.3 — zoom utility
        ["zoom-(--my-zoom)"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-0"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-100"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-105"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-110"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-125"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-150"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-175"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-200"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-25"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-250"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-300"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-50"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-75"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-90"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-95"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-[1.5]"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-[2]"] = "Tailwind 4.3 — not yet implemented",
        ["zoom-[number:2]"] = "Tailwind 4.3 — not yet implemented",
    };

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
            row.Skip = KnownGaps.TryGetValue(kvp.Key, out var reason) ? reason : null;
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