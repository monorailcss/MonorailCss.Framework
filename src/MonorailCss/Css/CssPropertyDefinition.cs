namespace MonorailCss.Css;

/// <summary>
/// Defines metadata for a CSS custom property used by Tailwind utilities.
/// </summary>
/// <param name="Name">The CSS custom property name (e.g., "--tw-border-spacing-x").</param>
/// <param name="Syntax">The CSS syntax type (e.g., "&lt;length&gt;", "*").</param>
/// <param name="Inherits">Whether the property should inherit from parent elements.</param>
/// <param name="InitialValue">The initial value for the property (null if no initial value should be output).</param>
/// <param name="NeedsFallback">Whether this property needs fallback initialization for older browsers.</param>
public record CssPropertyDefinition(
    string Name,
    string Syntax,
    bool Inherits,
    string? InitialValue,
    bool NeedsFallback = true);