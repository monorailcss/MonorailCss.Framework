namespace MonorailCss.Parser;

/// <summary>
/// Represents a parsed CSS class that is a utility function.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="Name">Gets the name of the utility.</param>
public record UtilitySyntax(string OriginalSyntax, string[] Modifiers, string Name) : IParsedClassNameSyntax
{
    /// <summary>
    /// Checks if a utility name matches for this utility.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if they match case-insensitive, false otherwise.</returns>
    public bool NameEquals(string value) => value.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
}