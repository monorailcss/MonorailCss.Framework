using System.Collections.Immutable;

namespace MonorailCss.Css;

/// <summary>
/// Represents an at-rule wrapper for CSS rules (e.g., @media, @supports, @container).
/// </summary>
internal record AtRuleWrapper(string Name, string Params)
{
    /// <summary>
    /// Creates a media query wrapper.
    /// </summary>
    public static AtRuleWrapper Media(string query) => new("media", query);

    /// <summary>
    /// Creates a supports query wrapper.
    /// </summary>
    public static AtRuleWrapper Supports(string condition) => new("supports", condition);

    /// <summary>
    /// Creates a container query wrapper.
    /// </summary>
    public static AtRuleWrapper Container(string query) => new("container", query);

    /// <summary>
    /// Creates a layer wrapper.
    /// </summary>
    public static AtRuleWrapper Layer(string layerName) => new("layer", layerName);

    /// <summary>
    /// Generates the CSS at-rule string.
    /// </summary>
    public string ToCss() => string.IsNullOrEmpty(Params) ? $"@{Name}" : $"@{Name} {Params}";

    public override string ToString() => ToCss();
}

/// <summary>
/// Represents a selector with its associated at-rule wrappers.
/// This is the result of applying variants to a base selector.
/// </summary>
internal record AppliedSelector(Selector Selector, ImmutableArray<AtRuleWrapper> Wrappers)
{
    /// <summary>
    /// Creates an AppliedSelector with just a selector and no wrappers.
    /// </summary>
    public static AppliedSelector FromSelector(Selector selector) =>
        new(selector, ImmutableArray<AtRuleWrapper>.Empty);

    /// <summary>
    /// Creates an AppliedSelector from a class name.
    /// </summary>
    public static AppliedSelector FromClass(string escapedClassName) =>
        FromSelector(Selector.FromClass(escapedClassName));

    /// <summary>
    /// Adds an at-rule wrapper to this applied selector.
    /// </summary>
    public AppliedSelector WithWrapper(AtRuleWrapper wrapper) =>
        new(Selector, Wrappers.Add(wrapper));

    /// <summary>
    /// Adds multiple at-rule wrappers to this applied selector.
    /// </summary>
    public AppliedSelector WithWrappers(params AtRuleWrapper[] wrappers) =>
        new(Selector, Wrappers.AddRange(wrappers));

    /// <summary>
    /// Transforms the selector while keeping the wrappers.
    /// </summary>
    public AppliedSelector WithSelector(Selector selector) =>
        new(selector, Wrappers);

    /// <summary>
    /// Applies a transformation to the selector.
    /// </summary>
    public AppliedSelector TransformSelector(Func<Selector, Selector> transform) =>
        new(transform(Selector), Wrappers);

    /// <summary>
    /// Gets a value indicating whether checks if this applied selector has any at-rule wrappers.
    /// </summary>
    public bool HasWrappers => !Wrappers.IsEmpty;

    public override string ToString() =>
        HasWrappers
            ? $"{string.Join(" ", Wrappers.Select(w => w.ToCss()))} {{ {Selector} }}"
            : Selector.ToString();
}