using MonorailCss.Utilities;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Documentation;

/// <summary>
/// Generates documentation for all utilities using reflection and theme integration.
/// </summary>
public class UtilityDocumentationEngine
{
    /// <summary>
    /// Generates complete documentation for all discovered utilities.
    /// </summary>
    /// <param name="theme">The theme to use for generating theme-aware examples.</param>
    /// <returns>An enumerable of utility documentation for all utilities.</returns>
    public static IEnumerable<UtilityDocumentation> GenerateDocumentation(Theme.Theme? theme = null)
    {
        theme ??= new Theme.Theme(); // Use default theme if none provided

        var utilities = UtilityDiscovery.DiscoverAllUtilities();
        return utilities.Select(utility => GenerateDocumentationFor(utility, theme));
    }

    /// <summary>
    /// Generates documentation for a specific utility.
    /// </summary>
    /// <param name="utility">The utility to document.</param>
    /// <param name="theme">The theme to use for generating theme-aware examples.</param>
    /// <returns>Complete documentation for the utility.</returns>
    public static UtilityDocumentation GenerateDocumentationFor(IUtility utility, Theme.Theme theme)
    {
        // Get metadata from utility, but enhance it with utility instance for property extraction
        var metadata = utility.GetMetadata();

        // If metadata doesn't have documented properties yet, generate them
        if (metadata.DocumentedProperties.Length == 0)
        {
            metadata = UtilityMetadata.FromUtilityType(utility.GetType(), utility);
        }

        var examples = utility.GetExamples(theme);
        var themeNamespaces = utility.GetNamespaces();
        var priority = utility.Priority;
        var type = InferUtilityType(utility);

        return new UtilityDocumentation(
            metadata,
            examples,
            themeNamespaces,
            priority,
            type);
    }

    /// <summary>
    /// Generates documentation grouped by category.
    /// </summary>
    /// <param name="theme">The theme to use for generating theme-aware examples.</param>
    /// <returns>A dictionary mapping category names to their utilities' documentation.</returns>
    public static Dictionary<string, List<UtilityDocumentation>> GenerateDocumentationByCategory(Theme.Theme? theme = null)
    {
        theme ??= new Theme.Theme();

        var allDocs = GenerateDocumentation(theme);
        return allDocs
            .GroupBy(doc => doc.Metadata.Category)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(doc => doc.Metadata.Name).ToList());
    }

    /// <summary>
    /// Generates documentation grouped by CSS property.
    /// Multiple utilities that document the same CSS property will be grouped together.
    /// </summary>
    /// <param name="theme">The theme to use for generating theme-aware examples.</param>
    /// <returns>A dictionary mapping CSS property names to their utilities' documentation, organized by category.</returns>
    public static Dictionary<string, Dictionary<string, List<UtilityDocumentation>>> GenerateDocumentationByProperty(Theme.Theme? theme = null)
    {
        theme ??= new Theme.Theme();

        var allDocs = GenerateDocumentation(theme);

        // Group by category first, then by CSS property within each category
        var result = new Dictionary<string, Dictionary<string, List<UtilityDocumentation>>>();

        foreach (var doc in allDocs)
        {
            var category = doc.Metadata.Category;

            if (!result.ContainsKey(category))
            {
                result[category] = new Dictionary<string, List<UtilityDocumentation>>();
            }

            var categoryDocs = result[category];

            // Only include utilities that have documented properties
            // Utilities without documented properties are excluded from property-based documentation
            if (doc.Metadata.DocumentedProperties.Length > 0)
            {
                // Use the primary property for grouping
                var property = doc.Metadata.PrimaryProperty;

                if (!categoryDocs.ContainsKey(property))
                {
                    categoryDocs[property] = new List<UtilityDocumentation>();
                }

                categoryDocs[property].Add(doc);
            }

            // Note: Utilities without documented properties are intentionally excluded
            // to prevent duplicate TOC entries
        }

        // Sort categories and properties
        return result
            .OrderBy(kvp => kvp.Key)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
    }

    /// <summary>
    /// Generates documentation for utilities that use a specific theme namespace.
    /// </summary>
    /// <param name="namespacePrefix">The namespace prefix to filter by (e.g., "--color", "--spacing").</param>
    /// <param name="theme">The theme to use for generating theme-aware examples.</param>
    /// <returns>An enumerable of utility documentation for utilities using the specified namespace.</returns>
    public static IEnumerable<UtilityDocumentation> GenerateDocumentationForNamespace(
        string namespacePrefix,
        Theme.Theme? theme = null)
    {
        theme ??= new Theme.Theme();

        var allDocs = GenerateDocumentation(theme);
        return allDocs.Where(doc => doc.ThemeNamespaces.Any(ns => ns.StartsWith(namespacePrefix)));
    }

    private static string InferUtilityType(IUtility utility)
    {
        return utility switch
        {
            BaseStaticUtility => "Static",
            BaseFunctionalUtility => "Functional",
            BaseColorUtility => "Color",
            BaseSizingUtility => "Sizing",
            _ => "Custom",
        };
    }

    /// <summary>
    /// Generates a summary report of all utilities grouped by type.
    /// </summary>
    /// <param name="theme">The theme to use for generating theme-aware examples.</param>
    /// <returns>A dictionary mapping utility types to counts.</returns>
    public static Dictionary<string, int> GenerateUtilitySummary(Theme.Theme? theme = null)
    {
        theme ??= new Theme.Theme();

        var allDocs = GenerateDocumentation(theme);
        return allDocs
            .GroupBy(doc => doc.Type)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
