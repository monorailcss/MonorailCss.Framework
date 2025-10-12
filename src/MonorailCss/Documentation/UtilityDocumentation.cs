using MonorailCss.Utilities;

namespace MonorailCss.Documentation;

/// <summary>
/// Complete documentation for a utility, including metadata, examples, and theme integration.
/// </summary>
public class UtilityDocumentation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityDocumentation"/> class.
    /// </summary>
    /// <param name="metadata">The metadata about this utility.</param>
    /// <param name="examples">Example usages of this utility.</param>
    /// <param name="themeNamespaces">The theme namespaces this utility uses.</param>
    /// <param name="priority">The priority of the utility.</param>
    /// <param name="type">The utility type (e.g., "Static", "Functional", "Color", "Sizing").</param>
    public UtilityDocumentation(
        UtilityMetadata metadata,
        IEnumerable<UtilityExample> examples,
        string[] themeNamespaces,
        UtilityPriority priority,
        string type)
    {
        Metadata = metadata;
        Examples = examples.ToArray();
        ThemeNamespaces = themeNamespaces;
        Priority = priority;
        Type = type;
    }

    /// <summary>
    /// Gets the metadata about this utility.
    /// </summary>
    public UtilityMetadata Metadata { get; }

    /// <summary>
    /// Gets the example usages of this utility.
    /// </summary>
    public UtilityExample[] Examples { get; }

    /// <summary>
    /// Gets the theme namespaces this utility uses (e.g., "--color", "--spacing").
    /// </summary>
    public string[] ThemeNamespaces { get; }

    /// <summary>
    /// Gets the priority of this utility (determines evaluation order).
    /// </summary>
    public UtilityPriority Priority { get; }

    /// <summary>
    /// Gets the utility type (e.g., "Static", "Functional", "Color", "Sizing").
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="UtilityDocumentation"/> class with default values indicating an empty state.
    /// </summary>
    /// <param name="utilityType">The type of the utility to initialize metadata for.</param>
    /// <returns>A new instance of <see cref="UtilityDocumentation"/> initialized with empty examples, theme namespaces, and default priority.</returns>
    public static UtilityDocumentation Empty(Type utilityType)
    {
        return new UtilityDocumentation(
            UtilityMetadata.FromUtilityType(utilityType),
            [],
            [],
            UtilityPriority.Fallback,
            "Unknown");
    }
}
