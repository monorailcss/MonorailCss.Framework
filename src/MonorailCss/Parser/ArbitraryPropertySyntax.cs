namespace MonorailCss.Parser;

/// <summary>
/// Represents a parsed CSS class that is an arbitrary property.
/// </summary>
/// <param name="OriginalSyntax">Gets the original syntax of the class name.</param>
/// <param name="Modifiers">Gets the modifiers (e.g. dark, sm, etc) from the parsed class name.</param>
/// <param name="PropertyName">The CSS property name.</param>
/// <param name="ArbitraryValue">The value.</param>
public record ArbitraryPropertySyntax(
    string OriginalSyntax,
    string[] Modifiers,
    string PropertyName,
    string ArbitraryValue) : IParsedClassNameSyntax;