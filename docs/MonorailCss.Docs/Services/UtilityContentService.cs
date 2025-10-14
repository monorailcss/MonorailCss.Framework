using System.Collections.Immutable;
using MonorailCss.Documentation;
using MyLittleContentEngine.Models;
using MyLittleContentEngine.Services.Content;
using MyLittleContentEngine.Services.Content.TableOfContents;

namespace MonorailCss.Docs.Services;

/// <summary>
/// Custom content service that generates documentation pages for MonorailCSS utilities, organized by CSS property.
/// </summary>
public partial class UtilityContentService : IContentService
{
    private readonly Lazy<Dictionary<string, Dictionary<string, List<UtilityDocumentation>>>> _utilitiesByProperty;

    public UtilityContentService()
    {
        var theme = new MonorailCss.Theme.Theme();
        _utilitiesByProperty = new Lazy<Dictionary<string, Dictionary<string, List<UtilityDocumentation>>>>(() =>
            UtilityDocumentationEngine.GenerateDocumentationByProperty(theme));
    }

    public int SearchPriority => 10;

    public Task<ImmutableList<PageToGenerate>> GetPagesToGenerateAsync()
    {
        var pages = new List<PageToGenerate>();

        // Add category pages and individual CSS property pages
        foreach (var (category, propertiesDict) in _utilitiesByProperty.Value)
        {
            var categorySlug = ToSlug(category);

            // Category page
            pages.Add(new PageToGenerate(
                Url: $"/{categorySlug}",
                OutputFile: $"{categorySlug}/index.html",
                Metadata: new Metadata
                {
                    Title = $"{category} Utilities",
                    Description = $"Reference for {category} utility classes in MonorailCSS",
                    Order = 101
                }));

            // Individual CSS property pages
            foreach (var (property, _) in propertiesDict)
            {
                var propertySlug = ToSlug(property);
                var propertyDisplayName = GetPropertyDisplayName(property);

                pages.Add(new PageToGenerate(
                    Url: $"/{categorySlug}/{propertySlug}",
                    OutputFile: $"{categorySlug}/{propertySlug}.html",
                    Metadata: new Metadata
                    {
                        Title = propertyDisplayName,
                        Description = $"Utilities for controlling the {property} CSS property",
                        Order = 102
                    }));
            }
        }

        return Task.FromResult(pages.ToImmutableList());
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var entries = new List<ContentTocItem>();

        // Add category entries
        var order = 1000;
        foreach (var (category, propertiesDict) in _utilitiesByProperty.Value.OrderBy(x => x.Key))
        {
            var categorySlug = ToSlug(category);

            // Add individual CSS property entries
            var propertyOrder = order + 1;
            foreach (var (property, _) in propertiesDict.OrderBy(p => p.Key))
            {
                var propertySlug = ToSlug(property);
                var propertyDisplayName = GetPropertyDisplayName(property);

                entries.Add(new ContentTocItem(
                    Title: propertyDisplayName,
                    Url: $"/{categorySlug}/{propertySlug}",
                    Order: propertyOrder++,
                    HierarchyParts: [category, propertyDisplayName]
                ));
            }
        }

        return Task.FromResult(entries.ToImmutableList());
    }

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        // Could add cross-references between related properties in the future
        return Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
    {
        // No static assets to copy
        return Task.FromResult(ImmutableList<ContentToCopy>.Empty);
    }

    /// <summary>
    /// Gets all utility categories with their properties and utilities.
    /// </summary>
    public Task<Dictionary<string, Dictionary<string, List<UtilityDocumentation>>>> GetAllCategoriesAsync()
    {
        return Task.FromResult(_utilitiesByProperty.Value);
    }

    /// <summary>
    /// Gets CSS properties and their utilities for a specific category.
    /// </summary>
    public Task<Dictionary<string, List<UtilityDocumentation>>?> GetPropertiesByCategoryAsync(string category)
    {
        var normalizedCategory = _utilitiesByProperty.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == category.ToLowerInvariant());

        if (normalizedCategory != null && _utilitiesByProperty.Value.TryGetValue(normalizedCategory, out var properties))
        {
            return Task.FromResult<Dictionary<string, List<UtilityDocumentation>>?>(properties);
        }

        return Task.FromResult<Dictionary<string, List<UtilityDocumentation>>?>(null);
    }

    /// <summary>
    /// Gets utilities for a specific CSS property within a category.
    /// </summary>
    public Task<PropertyUtilities?> GetUtilitiesForPropertyAsync(string category, string property)
    {
        var normalizedCategory = _utilitiesByProperty.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == category.ToLowerInvariant());

        if (normalizedCategory != null && _utilitiesByProperty.Value.TryGetValue(normalizedCategory, out var propertiesDict))
        {
            var normalizedProperty = propertiesDict.Keys
                .FirstOrDefault(k => ToSlug(k) == property.ToLowerInvariant());

            if (normalizedProperty != null && propertiesDict.TryGetValue(normalizedProperty, out var utilities))
            {
                return Task.FromResult<PropertyUtilities?>(new PropertyUtilities(
                    PropertyName: normalizedProperty,
                    DisplayName: GetPropertyDisplayName(normalizedProperty),
                    Utilities: utilities));
            }
        }

        return Task.FromResult<PropertyUtilities?>(null);
    }

    /// <summary>
    /// Gets the category name from a slug.
    /// </summary>
    public Task<string?> GetCategoryNameFromSlugAsync(string categorySlug)
    {
        var category = _utilitiesByProperty.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == categorySlug.ToLowerInvariant());
        return Task.FromResult(category);
    }

    private static string ToSlug(string text)
    {
        // Remove "Utility" suffix first
        text = text.Replace("Utility", "");

        // Insert hyphens before uppercase letters (except at the start)
        text = SlugifyRegexDefinition().Replace(text, "-$1");

        // Convert to lowercase and normalize separators
        return text
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace("--", "-") // Clean up any double hyphens
            .Trim('-'); // Remove leading/trailing hyphens
    }

    private static string GetPropertyDisplayName(string property)
    {
        // For properties like "background-color", convert to "Background Color"
        // For utility names (fallback), clean them up
        if (property.EndsWith("Utility"))
        {
            return property.Replace("Utility", "");
        }

        // Convert CSS property names to title case
        var words = property.Split('-')
            .Where(word => !string.IsNullOrEmpty(word))
            .Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1));
        return string.Join(" ", words);
    }

    [System.Text.RegularExpressions.GeneratedRegex("(?<!^)([A-Z])")]
    private static partial System.Text.RegularExpressions.Regex SlugifyRegexDefinition();
}

/// <summary>
/// Represents utilities grouped by a CSS property.
/// </summary>
public record PropertyUtilities(
    string PropertyName,
    string DisplayName,
    List<UtilityDocumentation> Utilities);