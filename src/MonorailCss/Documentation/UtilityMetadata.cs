using System.Text.RegularExpressions;

namespace MonorailCss.Documentation;

/// <summary>
/// Contains metadata about a utility class.
/// </summary>
public partial class UtilityMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityMetadata"/> class.
    /// </summary>
    /// <param name="name">The utility name (e.g., "DisplayUtility", "BackgroundColorUtility").</param>
    /// <param name="category">The category this utility belongs to (e.g., "Layout", "Typography", "Backgrounds").</param>
    /// <param name="description">A human-readable description of what this utility does.</param>
    /// <param name="supportsModifiers">Whether this utility supports modifiers like opacity (e.g., bg-red-500/50).</param>
    /// <param name="supportsArbitraryValues">Whether this utility supports arbitrary values (e.g., w-[100px]).</param>
    /// <param name="documentedProperties">The CSS properties this utility documents (e.g., ["background-color"], ["width"]).</param>
    public UtilityMetadata(
        string name,
        string category,
        string description,
        bool supportsModifiers = false,
        bool supportsArbitraryValues = false,
        string[]? documentedProperties = null)
    {
        Name = name;
        Category = category;
        Description = description;
        SupportsModifiers = supportsModifiers;
        SupportsArbitraryValues = supportsArbitraryValues;
        DocumentedProperties = documentedProperties ?? [];
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
    /// Gets the CSS properties that this utility documents (e.g., ["background-color"], ["width"]).
    /// This is used for organizing documentation by CSS property rather than by utility name.
    /// </summary>
    public string[] DocumentedProperties { get; }

    /// <summary>
    /// Gets the primary CSS property this utility documents.
    /// For utilities with 1-3 properties, this returns the first property.
    /// For utilities with more than 3 properties, this returns the utility name (to avoid arbitrary property selection).
    /// Returns empty string if no properties are specified.
    /// </summary>
    public string PrimaryProperty
    {
        get
        {
            if (DocumentedProperties.Length == 0)
            {
                return string.Empty;
            }

            // If a utility has many unrelated properties, use the utility name instead of picking one arbitrarily
            if (DocumentedProperties.Length > 1)
            {
                return Name;
            }

            return DocumentedProperties[0];
        }
    }

    /// <summary>
    /// Generates a <see cref="UtilityMetadata"/> instance from a given utility type.
    /// </summary>
    /// <param name="utilityType">The type of the utility for which metadata is being generated.</param>
    /// <param name="utilityInstance">Optional utility instance to extract documented properties from. If not provided, properties are extracted from sample compilation.</param>
    /// <returns>A <see cref="UtilityMetadata"/> instance containing details such as name, category, description,
    /// and support for modifiers or arbitrary values based on the provided utility type.</returns>
    public static UtilityMetadata FromUtilityType(Type utilityType, Utilities.IUtility? utilityInstance = null)
    {
        var name = utilityType.Name;
        string category;
        string description;
        bool supportsModifiers;
        bool supportsArbitraryValues;

        // Built-in utilities have their category, summary, and support flags computed at compile
        // time by the source generator — no XML-doc file parse or base-type reflection at runtime.
        if (Utilities.GeneratedUtilityMetadata.TryGet(utilityType, out var generated))
        {
            category = generated.Category;
            description = generated.Description;
            supportsModifiers = generated.SupportsModifiers;
            supportsArbitraryValues = generated.SupportsArbitraryValues;
        }
        else
        {
            // Fallback for types the generator didn't see (e.g. consumer-authored custom utilities).
            // Uses only namespace + base-type inspection; no Assembly.Location / XML-doc file access.
            // Custom utilities that want a richer description can override IUtility.GetMetadata().
            category = InferCategory(utilityType);
            description = GenerateHeuristicDescription(utilityType);
            supportsModifiers = IsColorUtility(utilityType);
            supportsArbitraryValues = IsFunctionalUtility(utilityType) || IsColorUtility(utilityType);
        }

        // Extract documented properties from the live instance (requires compilation, so it stays
        // a runtime step regardless of where the rest of the metadata comes from).
        string[]? documentedProperties = null;
        if (utilityInstance != null)
        {
            documentedProperties = utilityInstance.GetDocumentedProperties()
                ?? ExtractDocumentedPropertiesFromUtility(utilityInstance);
        }

        return new UtilityMetadata(name, category, description, supportsModifiers, supportsArbitraryValues, documentedProperties);
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

    // Reflection-free description for types the generator didn't see: strip the "Utility" suffix
    // and humanise the PascalCase name. Built-in utilities never reach this — their description
    // (XML <summary> or this same heuristic) is baked in at compile time by the source generator.
    private static string GenerateHeuristicDescription(Type utilityType)
    {
        var name = utilityType.Name;
        if (name.EndsWith("Utility"))
        {
            name = name[..^7]; // Remove "Utility"
        }

        // Convert PascalCase to space-separated words
        var result = ConvertPascalCaseToSpaceSeperatedRegex().Replace(name, " $1").Trim();
        return $"Handles {result.ToLowerInvariant()} utilities";
    }

    private static bool IsFunctionalUtility(Type utilityType)
    {
        return IsSubclassOfType(utilityType, "BaseFunctionalUtility") ||
               IsSubclassOfType(utilityType, "BaseSpacingUtility") ||
               IsSubclassOfType(utilityType, "BaseFilterUtility");
    }

    private static bool IsColorUtility(Type utilityType)
    {
        return IsSubclassOfType(utilityType, "BaseColorUtility");
    }

    private static bool IsSubclassOfType(Type type, string baseTypeName)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.Name == baseTypeName)
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Extracts documented CSS properties from a utility by compiling sample candidates
    /// and analyzing the generated CSS declarations.
    /// </summary>
    /// <param name="utility">The utility to extract properties from.</param>
    /// <returns>An array of CSS property names, or empty array if extraction fails.</returns>
    private static string[] ExtractDocumentedPropertiesFromUtility(Utilities.IUtility utility)
    {
        try
        {
            var theme = new Theme.Theme();
            var properties = new HashSet<string>();

            // Get functional roots or examples to try compiling
            var examples = utility.GetExamples(theme).Take(5).ToList();
            var functionalRoots = utility.GetFunctionalRoots();

            // Try to compile examples and extract properties
            foreach (var example in examples)
            {
                if (TryExtractPropertiesFromClassName(utility, example.ClassName, theme, properties))
                {
                    break; // Found properties, no need to continue
                }
            }

            // If no examples worked, try functional roots with sample values
            if (properties.Count == 0 && functionalRoots.Length > 0)
            {
                foreach (var root in functionalRoots.Take(3))
                {
                    var sampleClasses = new[] { root, $"{root}-1", $"{root}-full", $"{root}-auto" };
                    foreach (var className in sampleClasses)
                    {
                        if (TryExtractPropertiesFromClassName(utility, className, theme, properties))
                        {
                            break;
                        }
                    }

                    if (properties.Count > 0)
                    {
                        break;
                    }
                }
            }

            // Filter out CSS variables (--tw-*, --*) as they are implementation details
            var documentedProperties = properties
                .Where(p => !p.StartsWith("--"))
                .OrderBy(p => p)
                .ToArray();

            return documentedProperties;
        }
        catch
        {
            // If extraction fails, return empty array
            return [];
        }
    }

    /// <summary>
    /// Tries to extract CSS properties from a specific class name by compiling it.
    /// </summary>
    private static bool TryExtractPropertiesFromClassName(
        Utilities.IUtility utility,
        string className,
        Theme.Theme theme,
        HashSet<string> properties)
    {
        try
        {
            // Create a minimal utility registry and candidate parser
            var utilityRegistry = new UtilityRegistry(autoRegisterUtilities: false);
            utilityRegistry.RegisterUtility(utility);
            var parser = new Parser.CandidateParser(utilityRegistry);

            if (!parser.TryParseCandidate(className, out var candidate))
            {
                return false;
            }

            if (!utility.TryCompile(candidate, theme, out var results) || results == null)
            {
                return false;
            }

            // Extract property names from declarations
            foreach (var node in results)
            {
                if (node is Ast.Declaration declaration)
                {
                    properties.Add(declaration.Property);
                }
            }

            return properties.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex ConvertPascalCaseToSpaceSeperatedRegex();
}