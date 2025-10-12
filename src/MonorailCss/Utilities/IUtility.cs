using System.Collections.Immutable;
using JetBrains.Annotations;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;

namespace MonorailCss.Utilities;

/// <summary>
/// Represents a utility that can compile CSS class candidates into AST nodes.
/// Each utility is self-contained with its own namespace registration and compilation logic.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface IUtility
{
    /// <summary>
    /// Gets the priority of this utility determines the order in which utilities are evaluated.
    /// Lower priority values are evaluated first.
    /// </summary>
    UtilityPriority Priority { get; }

    /// <summary>
    /// Gets the layer this utility belongs to, which affects CSS output ordering.
    /// Component layer utilities are output before standard utilities.
    /// </summary>
    UtilityLayer Layer => UtilityLayer.Utility;

    /// <summary>
    /// The namespace chains this utility uses for theme value resolution.
    /// Returns namespaces specific to the utility type, such as colors, typography, or SVG properties.
    /// Returns null or an empty array if the utility does not use theme namespaces.
    /// </summary>
    /// <returns>An array of strings representing the namespace chains used by the utility.</returns>
    string[] GetNamespaces();

    /// <summary>
    /// Attempts to compile the provided candidate into a collection of CSS AST nodes.
    /// This method evaluates the candidate based on the given theme and outputs the compilation results.
    /// Returns a value indicating whether the candidate was successfully compiled.
    /// </summary>
    /// <param name="candidate">The candidate to be compiled, representing a potential utility or style.</param>
    /// <param name="theme">The theme object that provides contextual information for the compilation process.</param>
    /// <param name="results">The output parameter containing the compiled AST nodes if compilation is successful; otherwise, null.</param>
    /// <returns>True if the candidate was successfully compiled; otherwise, false.</returns>
    bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results);

    /// <summary>
    /// Attempts to compile the specified candidate into one or more CSS AST (Abstract Syntax Tree) nodes
    /// while taking the provided theme and property registry into account.
    /// This method determines if the candidate can be processed successfully and outputs the resulting nodes.
    /// </summary>
    /// <param name="candidate">The candidate to be compiled into CSS AST nodes.</param>
    /// <param name="theme">The theme instance providing contextual resolution for the candidate.</param>
    /// <param name="propertyRegistry">The CSS property registry used for property lookups and validation.</param>
    /// <param name="results">The output collection of CSS AST nodes that represent the compiled candidate. Returns null if compilation fails.</param>
    /// <returns>True if the compilation is successful and results are produced, otherwise false.</returns>
    bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        return TryCompile(candidate, theme, out results);
    }

    /// <summary>
    /// Retrieves the functional roots associated with this utility.
    /// Functional roots represent prefixes or keys that this utility uses to apply specific functionality,
    /// such as "m", "ring", or "content".
    /// Returns an empty array if no functional roots are defined for the utility.
    /// </summary>
    /// <returns>An array of strings representing the functional roots handled by the utility.</returns>
    string[] GetFunctionalRoots() => [];

    /// <summary>
    /// Returns examples of this utility with theme-aware values.
    /// Override this method to provide custom examples for your utility.
    /// Default implementation returns an empty array.
    /// </summary>
    /// <param name="theme">The theme to use for generating examples with actual theme values.</param>
    /// <returns>An enumerable of utility examples showing how to use this utility.</returns>
    IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme) => [];

    /// <summary>
    /// Returns metadata about this utility, including name, category, and description.
    /// Override this method to provide custom metadata for your utility.
    /// Default implementation generates metadata from the utility type.
    /// </summary>
    /// <returns>Metadata describing this utility.</returns>
    Documentation.UtilityMetadata GetMetadata() => Documentation.UtilityMetadata.FromUtilityType(GetType());
}