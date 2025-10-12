using System.Text.RegularExpressions;

namespace MonorailCss.Documentation;

/// <summary>
/// Contains metadata about a utility class.
/// </summary>
public class UtilityMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityMetadata"/> class.
    /// </summary>
    public UtilityMetadata(
        string name,
        string category,
        string description,
        bool supportsModifiers = false,
        bool supportsArbitraryValues = false)
    {
        Name = name;
        Category = category;
        Description = description;
        SupportsModifiers = supportsModifiers;
        SupportsArbitraryValues = supportsArbitraryValues;
    }

    /// <summary>
    /// Gets the utility name (e.g., "DisplayUtility", "BackgroundColorUtility").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category this utility belongs to (e.g., "Layout", "Typography", "Backgrounds").
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets a human-readable description of what this utility does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether this utility supports modifiers like opacity (e.g., bg-red-500/50).
    /// </summary>
    public bool SupportsModifiers { get; }

    /// <summary>
    /// Gets a value indicating whether this utility supports arbitrary values (e.g., w-[100px]).
    /// </summary>
    public bool SupportsArbitraryValues { get; }

    /// <summary>
    /// Creates metadata from a utility type by inferring category and name.
    /// </summary>
    public static UtilityMetadata FromUtilityType(Type utilityType)
    {
        var name = utilityType.Name;
        var category = InferCategory(utilityType);
        var description = GenerateDescription(utilityType);

        return new UtilityMetadata(name, category, description);
    }

    private static string InferCategory(Type utilityType)
    {
        // Try to infer category from namespace
        var ns = utilityType.Namespace ?? string.Empty;
        var parts = ns.Split('.');

        // Look for category after "Utilities"
        var utilitiesIndex = Array.IndexOf(parts, "Utilities");
        if (utilitiesIndex >= 0 && utilitiesIndex < parts.Length - 1)
        {
            return parts[utilitiesIndex + 1];
        }

        // Fallback to "General"
        return "General";
    }

    private static string GenerateDescription(Type utilityType)
    {
        // Remove "Utility" suffix and convert to readable format
        var name = utilityType.Name;
        if (name.EndsWith("Utility"))
        {
            name = name[..^7]; // Remove "Utility"
        }

        // Convert PascalCase to space-separated words
        var result = Regex.Replace(name, "([A-Z])", " $1").Trim();
        return $"Handles {result.ToLowerInvariant()} utilities";
    }
}
