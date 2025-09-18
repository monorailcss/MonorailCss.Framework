namespace MonorailCss.Utilities;

/// <summary>
/// Defines the priority levels for utility evaluation order in the CSS framework.
/// Lower values indicate higher priority and are evaluated first.
/// </summary>
public enum UtilityPriority
{
    /// <summary>
    /// Highest priority for exact static utilities with fixed property/value mappings.
    /// </summary>
    ExactStatic = 0,

    /// <summary>
    /// Functional utilities with constrained values from a predefined set.
    /// </summary>
    ConstrainedFunctional = 100,

    /// <summary>
    /// Utilities that handle negative value variants (e.g., -m-4).
    /// </summary>
    NegativeVariant = 200,

    /// <summary>
    /// Standard functional utilities with theme-aware dynamic values.
    /// </summary>
    StandardFunctional = 300,

    /// <summary>
    /// Utilities that handle namespace-based patterns (e.g., text-red-500).
    /// </summary>
    NamespaceHandler = 400,

    /// <summary>
    /// Utilities that process arbitrary values in square brackets (e.g., p-[10px]).
    /// </summary>
    ArbitraryHandler = 500,

    /// <summary>
    /// Lowest priority fallback handlers for catch-all scenarios.
    /// </summary>
    Fallback = 1000,
}