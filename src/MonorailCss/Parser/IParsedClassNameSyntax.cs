namespace MonorailCss.Parser;

/// <summary>
/// Represents a parsed class name.
/// </summary>
public interface IParsedClassNameSyntax
{
    /// <summary>
    /// Gets the original syntax of the class name.
    /// </summary>
    string OriginalSyntax { get; init; }

    /// <summary>
    /// Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.
    /// </summary>
    string[] Modifiers { get; init; }
}