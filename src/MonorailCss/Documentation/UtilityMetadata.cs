using System.Text.RegularExpressions;
using System.Xml.Linq;

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
    /// This is the first property in DocumentedProperties, or empty string if none are specified.
    /// </summary>
    public string PrimaryProperty => DocumentedProperties.Length > 0 ? DocumentedProperties[0] : string.Empty;

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
        var category = InferCategory(utilityType);
        var description = GenerateDescription(utilityType);

        // Detect support for modifiers and arbitrary values based on base class
        var supportsModifiers = IsColorUtility(utilityType);
        var supportsArbitraryValues = IsFunctionalUtility(utilityType) || IsColorUtility(utilityType);

        // Extract documented properties
        string[]? documentedProperties = null;
        if (utilityInstance != null)
        {
            // First, try to get documented properties from the utility itself
            documentedProperties = utilityInstance.GetDocumentedProperties();

            // If not specified, extract from compiled CSS
            if (documentedProperties == null)
            {
                documentedProperties = ExtractDocumentedPropertiesFromUtility(utilityInstance);
            }
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

    private static string GenerateDescription(Type utilityType)
    {
        // Try to extract XML documentation summary first
        if (TryGetXmlDocSummary(utilityType, out var xmlSummary))
        {
            return xmlSummary;
        }

        // Fallback: Remove "Utility" suffix and convert to readable format
        var name = utilityType.Name;
        if (name.EndsWith("Utility"))
        {
            name = name[..^7]; // Remove "Utility"
        }

        // Convert PascalCase to space-separated words
        var result = ConvertPascalCaseToSpaceSeperatedRegex().Replace(name, " $1").Trim();
        return $"Handles {result.ToLowerInvariant()} utilities";
    }

    private static bool TryGetXmlDocSummary(Type type, out string summary)
    {
        summary = string.Empty;

        try
        {
            // Get the XML documentation file path
            var assemblyLocation = type.Assembly.Location;
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                return false;
            }

            var xmlPath = Path.ChangeExtension(assemblyLocation, ".xml");
            if (!File.Exists(xmlPath))
            {
                return false;
            }

            // Load and parse the XML documentation
            var doc = XDocument.Load(xmlPath);

            // Construct the member name for the type (format: "T:Namespace.TypeName")
            var memberName = $"T:{type.FullName}";

            // Find the member element with matching name
            var memberElement = doc.Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);

            if (memberElement == null)
            {
                return false;
            }

            // Extract the summary element
            var summaryElement = memberElement.Element("summary");
            if (summaryElement == null)
            {
                return false;
            }

            // Get the text content and clean it up
            summary = CleanXmlDocText(summaryElement.Value);
            return !string.IsNullOrWhiteSpace(summary);
        }
        catch
        {
            // If anything goes wrong, return false to use fallback
            return false;
        }
    }

    private static string CleanXmlDocText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Split into lines and trim each
        var lines = text.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line));

        // Join with a single space
        return string.Join(" ", lines);
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