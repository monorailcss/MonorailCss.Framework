namespace MonorailCss.Parser;

/// <summary>
/// Represents a parsed CSS class that is a namespaced utility.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="Namespace">The namespace of the utility.</param>
/// <param name="Suffix">The suffix of the utility.</param>
public record NamespaceSyntax(
    string OriginalSyntax,
    string[] Modifiers,
    string Namespace,
    string? Suffix) : IParsedClassNameSyntax
{
    /// <summary>
    /// Checks if a namespace matches for this syntax.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <returns>True if they match case-insensitive, false otherwise.</returns>
    public bool NamespaceEquals(string value) => value.Equals(Namespace, StringComparison.InvariantCultureIgnoreCase);
}