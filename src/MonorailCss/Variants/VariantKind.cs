namespace MonorailCss.Variants;

/// <summary>
/// Represents the kind of variant for classification and processing.
/// </summary>
public enum VariantKind
{
    /// <summary>
    /// Static variants with fixed selectors (e.g., hover, focus, active).
    /// </summary>
    Static,

    /// <summary>
    /// Functional variants with parameters (e.g., aria-[checked], data-[state=open]).
    /// </summary>
    Functional,

    /// <summary>
    /// Compound variants combining multiple parts (e.g., group-hover, peer-focus).
    /// </summary>
    Compound,

    /// <summary>
    /// Arbitrary variants with custom selectors (e.g., [&amp;&gt;*], [@media(...)]).
    /// </summary>
    Arbitrary,
}

/// <summary>
/// Constraints on where a variant can be applied.
/// </summary>
[Flags]
public enum VariantConstraints
{
    /// <summary>
    /// Variant cannot be applied anywhere (used for invalid combinations).
    /// </summary>
    Never = 0,

    /// <summary>
    /// Variant can be applied to style rules.
    /// </summary>
    StyleRules = 1,

    /// <summary>
    /// Variant can create or nest within at-rules.
    /// </summary>
    AtRules = 2,

    /// <summary>
    /// Variant can be applied anywhere (default).
    /// </summary>
    Any = StyleRules | AtRules,
}