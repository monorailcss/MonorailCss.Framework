using System.Collections.Immutable;
using MonorailCss.Documentation;
using MyLittleContentEngine.Models;
using MyLittleContentEngine.Services.Content;
using MyLittleContentEngine.Services.Content.TableOfContents;

namespace MonorailCss.Docs.Services;

/// <summary>
/// Custom content service that generates documentation pages for MonorailCSS utilities.
/// </summary>
public class UtilityContentService : IContentService
{
    private readonly Lazy<Dictionary<string, List<UtilityDocumentation>>> _utilitiesByCategory;

    public UtilityContentService()
    {
        var theme = new MonorailCss.Theme.Theme();
        _utilitiesByCategory = new Lazy<Dictionary<string, List<UtilityDocumentation>>>(() =>
            UtilityDocumentationEngine.GenerateDocumentationByCategory(theme));
    }

    public int SearchPriority => 10;

    public Task<ImmutableList<PageToGenerate>> GetPagesToGenerateAsync()
    {
        var pages = new List<PageToGenerate>
        {
            // Add main utilities index page
            new(
                Url: "/utilities",
                OutputFile: "utilities/index.html",
                Metadata: new Metadata
                {
                    Title = "Utilities Reference",
                    Description = "Complete reference of all MonorailCSS utility classes",
                    Order = 100
                })
        };

        // Add category pages and individual utility pages
        foreach (var (category, utilities) in _utilitiesByCategory.Value)
        {
            var categorySlug = ToSlug(category);

            // Category page
            pages.Add(new PageToGenerate(
                Url: $"/utilities/{categorySlug}",
                OutputFile: $"utilities/{categorySlug}/index.html",
                Metadata: new Metadata
                {
                    Title = $"{category} Utilities",
                    Description = $"Reference for {category} utility classes in MonorailCSS",
                    Order = 101
                }));

            // Individual utility pages
            foreach (var utility in utilities)
            {
                var utilitySlug = ToSlug(utility.Metadata.Name);
                pages.Add(new PageToGenerate(
                    Url: $"/utilities/{categorySlug}/{utilitySlug}",
                    OutputFile: $"utilities/{categorySlug}/{utilitySlug}.html",
                    Metadata: new Metadata
                    {
                        Title = $"{utility.Metadata.Name.Replace("Utility", "")}",
                        Description = utility.Metadata.Description,
                        Order = 102
                    }));
            }
        }

        return Task.FromResult(pages.ToImmutableList());
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var entries = new List<ContentTocItem>
        {
            // Create main Utilities section
            new ContentTocItem(
                Title: "Utilities Reference",
                Url: "/utilities",
                Order: 100,
                HierarchyParts: ["Utilities"])
        };

        // Add category entries
        var order = 0;
        foreach (var (category, utilities) in _utilitiesByCategory.Value.OrderBy(x => x.Key))
        {
            var categorySlug = ToSlug(category);

            entries.Add(new ContentTocItem(
                Title: category,
                Url: $"/utilities/{categorySlug}",
                Order: order++,
                HierarchyParts: ["Utilities", category]
            ));

            // Add individual utility entries
            var utilityOrder = 0;
            foreach (var utility in utilities.OrderBy(u => u.Metadata.Name))
            {
                var utilitySlug = ToSlug(utility.Metadata.Name);
                var utilityTitle = utility.Metadata.Name.Replace("Utility", "");

                entries.Add(new ContentTocItem(
                    Title: utilityTitle,
                    Url: $"/utilities/{categorySlug}/{utilitySlug}",
                    Order: utilityOrder++,
                    HierarchyParts: ["Utilities", category, utilityTitle]
                ));
            }
        }

        return Task.FromResult(entries.ToImmutableList());
    }

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
    {
        // Could add cross-references between related utilities in the future
        return Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
    {
        // No static assets to copy
        return Task.FromResult(ImmutableList<ContentToCopy>.Empty);
    }

    // Data access methods for Razor pages to use

    /// <summary>
    /// Gets all utility categories with their utilities.
    /// </summary>
    public Task<Dictionary<string, List<UtilityDocumentation>>> GetAllCategoriesAsync()
    {
        return Task.FromResult(_utilitiesByCategory.Value);
    }

    /// <summary>
    /// Gets utilities for a specific category.
    /// </summary>
    public Task<List<UtilityDocumentation>?> GetUtilitiesByCategoryAsync(string category)
    {
        var normalizedCategory = _utilitiesByCategory.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == category.ToLowerInvariant());

        if (normalizedCategory != null && _utilitiesByCategory.Value.TryGetValue(normalizedCategory, out var utilities))
        {
            return Task.FromResult<List<UtilityDocumentation>?>(utilities);
        }

        return Task.FromResult<List<UtilityDocumentation>?>(null);
    }

    /// <summary>
    /// Gets a specific utility by category and name.
    /// </summary>
    public Task<UtilityDocumentation?> GetUtilityAsync(string category, string utilityName)
    {
        var normalizedCategory = _utilitiesByCategory.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == category.ToLowerInvariant());

        if (normalizedCategory != null && _utilitiesByCategory.Value.TryGetValue(normalizedCategory, out var utilities))
        {
            var utility = utilities.FirstOrDefault(u => ToSlug(u.Metadata.Name) == utilityName.ToLowerInvariant());
            return Task.FromResult(utility);
        }

        return Task.FromResult<UtilityDocumentation?>(null);
    }

    /// <summary>
    /// Gets the category name from a slug.
    /// </summary>
    public Task<string?> GetCategoryNameFromSlugAsync(string categorySlug)
    {
        var category = _utilitiesByCategory.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == categorySlug.ToLowerInvariant());
        return Task.FromResult(category);
    }

    private static string ToSlug(string text)
    {
        return text
            .Replace("Utility", "")
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
    }
}
