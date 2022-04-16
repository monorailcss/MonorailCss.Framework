namespace MonorailCss.Parser;

/// <summary>
/// Represents a parsed CSS class that has a namespace and arbitrary value.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="Namespace">The namespace.</param>
/// <param name="ArbitraryValue">The arbitrary value.</param>
public record ArbitraryValueSyntax(
    string OriginalSyntax,
    string[] Modifiers,
    string Namespace,
    string ArbitraryValue) : IParsedClassNameSyntax;