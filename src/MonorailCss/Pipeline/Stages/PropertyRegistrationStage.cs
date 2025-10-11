using System.Collections.Immutable;
using System.Text.RegularExpressions;
using MonorailCss.Ast;
using MonorailCss.Css;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically registers CSS custom properties used in declarations
/// with the CssPropertyRegistry. This eliminates the need for manual registration in utilities.
/// </summary>
internal partial class PropertyRegistrationStage : IPipelineStage
{
    // Only register @property for Tailwind properties that actually need them
    // Based on Tailwind v4 actual behavior - only properties used for animations,
    // type safety, or complex composition get @property declarations
    private static readonly Dictionary<string, (string Syntax, bool Inherits, string? Initial)> _knownProperties = new()
    {
        // Transform properties - needed for animation and composition
        ["--tw-translate-x"] = ("*", false, "0"),
        ["--tw-translate-y"] = ("*", false, "0"),
        ["--tw-translate-z"] = ("*", false, "0"),
        ["--tw-rotate"] = ("<angle>", false, "0deg"),
        ["--tw-rotate-x"] = ("*", false, null),
        ["--tw-rotate-y"] = ("*", false, null),
        ["--tw-rotate-z"] = ("*", false, null),
        ["--tw-skew-x"] = ("*", false, null),
        ["--tw-skew-y"] = ("*", false, null),
        ["--tw-scale-x"] = ("*", false, "1"),
        ["--tw-scale-y"] = ("*", false, "1"),
        ["--tw-scale-z"] = ("*", false, "1"),

        // Filter properties - needed for filter stack composition
        ["--tw-blur"] = ("*", false, null),
        ["--tw-brightness"] = ("*", false, null),
        ["--tw-contrast"] = ("*", false, null),
        ["--tw-grayscale"] = ("*", false, null),
        ["--tw-hue-rotate"] = ("*", false, null),
        ["--tw-invert"] = ("*", false, null),
        ["--tw-saturate"] = ("*", false, null),
        ["--tw-sepia"] = ("*", false, null),
        ["--tw-drop-shadow"] = ("*", false, null),

        // Backdrop filter properties - needed for backdrop-filter stack
        ["--tw-backdrop-blur"] = ("*", false, null),
        ["--tw-backdrop-brightness"] = ("*", false, null),
        ["--tw-backdrop-contrast"] = ("*", false, null),
        ["--tw-backdrop-grayscale"] = ("*", false, null),
        ["--tw-backdrop-hue-rotate"] = ("*", false, null),
        ["--tw-backdrop-invert"] = ("*", false, null),
        ["--tw-backdrop-opacity"] = ("*", false, null),
        ["--tw-backdrop-saturate"] = ("*", false, null),
        ["--tw-backdrop-sepia"] = ("*", false, null),

        // Shadow properties - needed for shadow stack composition
        ["--tw-shadow"] = ("*", false, "0 0 #0000"),
        ["--tw-shadow-colored"] = ("*", false, "0 0 #0000"),
        ["--tw-ring-inset"] = ("*", false, null),
        ["--tw-ring-color"] = ("<color>", false, "currentColor"),
        ["--tw-ring-offset-width"] = ("<length>", false, "0px"),
        ["--tw-ring-offset-color"] = ("<color>", false, "#fff"),
        ["--tw-ring-offset-shadow"] = ("*", false, "0 0 #0000"),
        ["--tw-ring-shadow"] = ("*", false, "0 0 #0000"),
        ["--tw-inset-shadow"] = ("*", false, "0 0 #0000"),
        ["--tw-inset-ring-shadow"] = ("*", false, "0 0 #0000"),
        ["--tw-shadow-color"] = ("*", false, null),
        ["--tw-inset-shadow-color"] = ("*", false, null),
        ["--tw-text-shadow-color"] = ("*", false, null),

        // Gradient properties - needed for gradient composition and type safety
        ["--tw-gradient-from"] = ("<color>", false, "#0000"),
        ["--tw-gradient-to"] = ("<color>", false, "#0000"),
        ["--tw-gradient-via"] = ("<color>", false, "#0000"),
        ["--tw-gradient-stops"] = ("*", false, null!),  // No initial-value for syntax: "*"
        ["--tw-gradient-position"] = ("*", false, null!),  // No initial-value for syntax: "*"
        ["--tw-gradient-from-position"] = ("<length-percentage>", false, "0%"),
        ["--tw-gradient-via-position"] = ("<length-percentage>", false, "50%"),
        ["--tw-gradient-to-position"] = ("<length-percentage>", false, "100%"),
        ["--tw-gradient-via-stops"] = ("*", false, null!),  // No initial-value for syntax: "*"

        // Border spacing - needed for table border-spacing property
        ["--tw-border-spacing-x"] = ("<length>", false, "0"),
        ["--tw-border-spacing-y"] = ("<length>", false, "0"),

        // Layout and spacing properties
        ["--tw-border-style"] = ("*", false, "solid"),
        ["--tw-outline-style"] = ("*", false, "solid"),
        ["--tw-contain-layout"] = ("*", false, null),
        ["--tw-contain-paint"] = ("*", false, null),
        ["--tw-contain-size"] = ("*", false, null),
        ["--tw-contain-style"] = ("*", false, null),
        ["--tw-divide-x-reverse"] = ("*", false, "0"),
        ["--tw-divide-y-reverse"] = ("*", false, "0"),
        ["--tw-space-x-reverse"] = ("*", false, "0"),
        ["--tw-space-y-reverse"] = ("*", false, "0"),

        // Animation properties
        ["--tw-duration"] = ("*", false, null),
        ["--tw-ease"] = ("*", false, null),

        // Touch and scroll properties
        ["--tw-pan-x"] = ("*", false, null),
        ["--tw-pan-y"] = ("*", false, null),
        ["--tw-pinch-zoom"] = ("*", false, null),
        ["--tw-scroll-snap-strictness"] = ("*", false, "proximity"),

        // Content property - needed for before/after content
        ["--tw-content"] = ("*", false, null),

        // Typography properties - needed for font features
        ["--tw-leading"] = ("*", false, null),
        ["--tw-ordinal"] = ("*", false, null),
        ["--tw-slashed-zero"] = ("*", false, null),
        ["--tw-numeric-figure"] = ("*", false, null),
        ["--tw-numeric-fraction"] = ("*", false, null),
        ["--tw-numeric-spacing"] = ("*", false, null),
        ["--tw-font-weight"] = ("*", false, null),

        // Mask gradient properties - needed for mask composition
        ["--tw-mask-linear"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-linear-position"] = ("*", false, "0deg"),
        ["--tw-mask-linear-from-position"] = ("*", false, "0%"),
        ["--tw-mask-linear-to-position"] = ("*", false, "100%"),
        ["--tw-mask-radial"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-radial-position"] = ("*", false, "center"),
        ["--tw-mask-radial-shape"] = ("*", false, "ellipse"),
        ["--tw-mask-radial-size"] = ("*", false, "farthest-corner"),
        ["--tw-mask-radial-from-position"] = ("*", false, "0%"),
        ["--tw-mask-radial-to-position"] = ("*", false, "100%"),
        ["--tw-mask-conic"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-conic-position"] = ("*", false, "0deg"),
        ["--tw-mask-conic-from-position"] = ("*", false, "0%"),
        ["--tw-mask-conic-to-position"] = ("*", false, "100%"),

        // Directional mask properties
        ["--tw-mask-top"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-top-from-position"] = ("*", false, "0%"),
        ["--tw-mask-top-to-position"] = ("*", false, "100%"),
        ["--tw-mask-bottom"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-bottom-from-position"] = ("*", false, "0%"),
        ["--tw-mask-bottom-to-position"] = ("*", false, "100%"),
        ["--tw-mask-left"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-left-from-position"] = ("*", false, "0%"),
        ["--tw-mask-left-to-position"] = ("*", false, "100%"),
        ["--tw-mask-right"] = ("*", false, "linear-gradient(#fff, #fff)"),
        ["--tw-mask-right-from-position"] = ("*", false, "0%"),
        ["--tw-mask-right-to-position"] = ("*", false, "100%"),
    };

    public string Name => "Property Registration";

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Get the property registry from the context
        if (!context.Metadata.TryGetValue("propertyRegistry", out var registryObj) ||
            registryObj is not CssPropertyRegistry propertyRegistry)
        {
            // If no registry in context, can't register properties
            return nodes;
        }

        // Track properties from all nodes
        foreach (var node in nodes)
        {
            RegisterPropertiesInNode(node, propertyRegistry);
        }

        // Also track from processed classes if available
        if (context.Metadata.TryGetValue("processedClasses", out var classesObj) &&
            classesObj is List<ProcessedClass> processedClasses)
        {
            foreach (var processedClass in processedClasses)
            {
                foreach (var astNode in processedClass.AstNodes)
                {
                    RegisterPropertiesInNode(astNode, propertyRegistry);
                }
            }
        }

        // Return nodes unchanged - this stage only registers properties
        return nodes;
    }

    private void RegisterPropertiesInNode(AstNode node, CssPropertyRegistry propertyRegistry)
    {
        switch (node)
        {
            case Declaration declaration:
                // Check if this declaration uses or sets a Tailwind custom property
                RegisterPropertiesInDeclaration(declaration, propertyRegistry);
                break;

            case StyleRule styleRule:
                foreach (var child in styleRule.Nodes)
                {
                    RegisterPropertiesInNode(child, propertyRegistry);
                }

                break;

            case NestedRule nestedRule:
                foreach (var child in nestedRule.Nodes)
                {
                    RegisterPropertiesInNode(child, propertyRegistry);
                }

                break;

            case AtRule atRule:
                foreach (var child in atRule.Nodes)
                {
                    RegisterPropertiesInNode(child, propertyRegistry);
                }

                break;

            case Context contextNode:
                foreach (var child in contextNode.Nodes)
                {
                    RegisterPropertiesInNode(child, propertyRegistry);
                }

                break;
        }
    }

    private void RegisterPropertiesInDeclaration(Declaration declaration, CssPropertyRegistry propertyRegistry)
    {
        // If the property being set is a Tailwind custom property, register it
        if (declaration.Property.StartsWith("--tw-"))
        {
            RegisterProperty(declaration.Property, propertyRegistry);

            // If this is a gradient property, register ALL gradient properties
            // This matches Tailwind's behavior of outputting all gradient properties together
            if (IsGradientProperty(declaration.Property))
            {
                RegisterAllGradientProperties(propertyRegistry);
            }
        }

        // Also check the value for any custom properties that might need registration
        // This handles cases where utilities set up initial values
        var matches = TailwindPropertyPatternRegExDefinition().Matches(declaration.Value);
        foreach (Match match in matches)
        {
            RegisterProperty(match.Value, propertyRegistry);

            // If this references a gradient property, register ALL gradient properties
            if (IsGradientProperty(match.Value))
            {
                RegisterAllGradientProperties(propertyRegistry);
            }
        }
    }

    private bool IsGradientProperty(string propertyName)
    {
        return propertyName.StartsWith("--tw-gradient-");
    }

    private void RegisterAllGradientProperties(CssPropertyRegistry propertyRegistry)
    {
        // Register all gradient properties whenever any gradient property is used
        // This matches Tailwind's behavior
        RegisterProperty("--tw-gradient-position", propertyRegistry);
        RegisterProperty("--tw-gradient-from", propertyRegistry);
        RegisterProperty("--tw-gradient-via", propertyRegistry);
        RegisterProperty("--tw-gradient-to", propertyRegistry);
        RegisterProperty("--tw-gradient-stops", propertyRegistry);
        RegisterProperty("--tw-gradient-via-stops", propertyRegistry);
        RegisterProperty("--tw-gradient-from-position", propertyRegistry);
        RegisterProperty("--tw-gradient-via-position", propertyRegistry);
        RegisterProperty("--tw-gradient-to-position", propertyRegistry);
    }

    private void RegisterProperty(string propertyName, CssPropertyRegistry propertyRegistry)
    {
        // Skip if already registered
        if (propertyRegistry.IsRegistered(propertyName))
        {
            return;
        }

        // Only register properties that are explicitly known to need @property declarations
        // Unknown --tw-* properties are just CSS variables that don't need @property
        if (_knownProperties.TryGetValue(propertyName, out var config))
        {
            // Properties with syntax "*" and null initial value don't need fallback
            propertyRegistry.Register(propertyName, config.Syntax, config.Inherits, config.Initial);
        }

        // Don't automatically register unknown --tw-* properties
        // They're simple CSS variables that don't need @property declarations
    }

    [GeneratedRegex(@"--tw-[\w-]+", RegexOptions.Compiled)]
    private static partial Regex TailwindPropertyPatternRegExDefinition();
}