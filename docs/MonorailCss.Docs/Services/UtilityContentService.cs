using System.Collections.Immutable;
using MonorailCss.Documentation;
using Pennington.Content;
using Pennington.Pipeline;
using Pennington.Routing;

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

    public string DefaultSection => "Utilities";

    public int SearchPriority => 10;

    public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
    {
        foreach (var (category, propertiesDict) in _utilitiesByProperty.Value)
        {
            var categorySlug = ToSlug(category);

            foreach (var (property, _) in propertiesDict)
            {
                var propertySlug = ToSlug(property);
                var url = $"/{categorySlug}/{propertySlug}";
                var route = ContentRouteFactory.FromUrl(new UrlPath(url), string.Empty);
                yield return new DiscoveredItem(route, new ContentSource(new RazorPageSource("MonorailCss.Docs.Components.Pages.Doc")));
            }
        }

        await Task.CompletedTask;
    }

    public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
    {
        var entries = new List<ContentTocItem>();

        var order = 1000;
        foreach (var (category, propertiesDict) in _utilitiesByProperty.Value.OrderBy(x => x.Key))
        {
            var categorySlug = ToSlug(category);

            var propertyOrder = order + 1;
            foreach (var (property, _) in propertiesDict.OrderBy(p => p.Key))
            {
                var propertySlug = ToSlug(property);
                var propertyDisplayName = GetPropertyDisplayName(property);
                var route = ContentRouteFactory.FromUrl(new UrlPath($"/{categorySlug}/{propertySlug}"), string.Empty);

                entries.Add(new ContentTocItem(
                    Title: propertyDisplayName,
                    Route: route,
                    Order: propertyOrder++,
                    HierarchyParts: [category, propertyDisplayName],
                    Section: "Utilities",
                    Locale: string.Empty));
            }
        }

        return Task.FromResult(entries.ToImmutableList());
    }

    public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
        Task.FromResult(ImmutableList<CrossReference>.Empty);

    public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() =>
        Task.FromResult(ImmutableList<ContentToCopy>.Empty);

    public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() =>
        Task.FromResult(ImmutableList<ContentToCreate>.Empty);

    /// <summary>
    /// Gets utilities for a specific CSS property within a category.
    /// </summary>
    public Task<PropertyUtilities?> GetUtilitiesForPropertyAsync(string category, string property)
    {
        var normalizedCategory = _utilitiesByProperty.Value.Keys
            .FirstOrDefault(k => ToSlug(k) == category);

        if (normalizedCategory != null && _utilitiesByProperty.Value.TryGetValue(normalizedCategory, out var propertiesDict))
        {
            var normalizedProperty = propertiesDict.Keys
                .FirstOrDefault(k => ToSlug(k) == property);

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
            .FirstOrDefault(k => ToSlug(k) == categorySlug);
        return Task.FromResult(category);
    }

    private static string ToSlug(string text)
    {
        text = text.Replace("Utility", "");
        text = SlugifyRegexDefinition().Replace(text, "-$1");
        return text
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    private static string GetPropertyDisplayName(string property)
    {
        if (property.EndsWith("Utility"))
        {
            var nameWithoutSuffix = property.Replace("Utility", "");
            var withSpaces = SlugifyRegexDefinition().Replace(nameWithoutSuffix, " $1").Trim();
            return withSpaces;
        }

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
