using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities;

namespace MonorailCss.Parser.Custom;

/// <summary>
/// A custom utility implementation for static CSS patterns defined via @utility directives.
/// Handles both simple property mappings and complex nested selectors.
/// </summary>
public class StaticCustomUtility : IUtility
{
    private readonly UtilityDefinition _definition;

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticCustomUtility"/> class.
    /// </summary>
    /// <param name="definition">The utility definition parsed from CSS.</param>
    public StaticCustomUtility(UtilityDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (definition.IsWildcard)
        {
            throw new ArgumentException("Static custom utilities cannot use wildcard patterns", nameof(definition));
        }

        _definition = definition;
    }

    /// <summary>
    /// Gets the utility priority. Static custom utilities use ExactStatic priority.
    /// </summary>
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    /// <summary>
    /// Gets the utility layer. Custom utilities belong to the Utility layer.
    /// </summary>
    public UtilityLayer Layer => UtilityLayer.Utility;

    /// <summary>
    /// Retrieves the namespaces associated with the utility.
    /// For static custom utilities, this typically returns an empty array.
    /// </summary>
    /// <returns>
    /// An array of strings representing the namespaces.
    /// </returns>
    public string[] GetNamespaces() => [];

    /// <summary>
    /// Gets the utility name that this custom utility handles,
    /// which corresponds to the defined pattern in the utility definition.
    /// </summary>
    /// <returns>The utility name as a string, derived from the pattern in the definition.</returns>
    public string GetUtilityName() => _definition.Pattern;

    /// <summary>
    /// Attempts to compile the specified candidate into Abstract Syntax Tree (AST) nodes
    /// based on the provided theme and utility definition.
    /// </summary>
    /// <param name="candidate">The candidate to be compiled.</param>
    /// <param name="theme">The theme to assist in compilation and determining utility rules.</param>
    /// <param name="results">When this method returns, contains the list of compiled AST nodes if the compilation is successful, or null if it fails.</param>
    /// <returns>True if the candidate was successfully compiled into AST nodes; otherwise, false.</returns>
    public bool TryCompile(Candidate candidate, Theme.Theme theme, [NotNullWhen(true)] out ImmutableList<AstNode>? results)
    {
        results = null;

        // Check if this utility handles the candidate
        if (candidate is not StaticUtility staticUtility || staticUtility.Root != _definition.Pattern)
        {
            return false;
        }

        var nodes = new List<AstNode>();

        // Add root-level declarations
        foreach (var declaration in _definition.Declarations)
        {
            nodes.Add(new Declaration(declaration.Property, declaration.Value, candidate.Important));
        }

        // Add nested selectors
        foreach (var nestedSelector in _definition.NestedSelectors)
        {
            // Convert nested declarations to AST nodes
            var nestedDeclarations = new List<AstNode>();
            foreach (var declaration in nestedSelector.Declarations)
            {
                nestedDeclarations.Add(new Declaration(declaration.Property, declaration.Value, candidate.Important));
            }

            // Replace & with the appropriate selector reference
            // For pseudo-elements like ::-webkit-scrollbar, we don't need :where()
            var selector = nestedSelector.Selector;
            if (selector.StartsWith("&::"))
            {
                // Pseudo-element selectors don't need :where()
                selector = selector.Substring(1); // Remove the & prefix
            }
            else if (selector.StartsWith("&:"))
            {
                // Pseudo-class selectors might benefit from :where()
                selector = selector.Substring(1); // Remove the & prefix
            }
            else if (selector.StartsWith("&"))
            {
                // Other selectors with & parent reference
                selector = selector.Substring(1).TrimStart();
            }

            // Create the nested rule
            nodes.Add(new NestedRule(selector, nestedDeclarations.ToImmutableList()));
        }

        results = nodes.ToImmutableList();
        return true;
    }
}