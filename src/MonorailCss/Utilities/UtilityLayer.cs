namespace MonorailCss.Utilities;

/// <summary>
/// Defines the layer/category for a utility, which affects CSS output ordering.
/// This ensures proper cascade behavior where utilities can override component styles.
/// </summary>
public enum UtilityLayer
{
    /// <summary>
    /// Component-level utilities that provide complex, multi-property styles.
    /// These are output before regular utilities to allow overrides.
    /// Examples: prose, container.
    /// </summary>
    Component = 0,

    /// <summary>
    /// Standard utilities that set individual CSS properties.
    /// These are output after components to ensure they can override component styles.
    /// Examples: text-red-500, p-4, max-w-full.
    /// </summary>
    Utility = 100,
}