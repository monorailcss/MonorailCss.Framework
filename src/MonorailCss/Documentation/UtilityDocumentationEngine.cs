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
        var metadata = utility.GetMetadata();
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
            _ => "Custom"
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
