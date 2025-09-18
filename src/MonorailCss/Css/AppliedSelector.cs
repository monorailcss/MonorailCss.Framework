using System.Collections.Immutable;

namespace MonorailCss.Css;

/// <summary>
/// Represents an at-rule wrapper for CSS rules (e.g., @media, @supports, @container).
/// </summary>
public record AtRuleWrapper(string Name, string Params)
{
    /// <summary>
    /// Creates a media query wrapper.
    /// </summary>
    /// <param name="query">The media query string to apply, defining specific conditions for rendering styles.</param>
    /// <returns>Returns an instance of <see cref="AtRuleWrapper"/> configured with the specified media query.</returns>
    public static AtRuleWrapper Media(string query) => new("media", query);

    /// <summary>
    /// Creates a supports query wrapper.
    /// </summary>
    /// <param name="condition">The condition to evaluate within the supports query, defining specific feature requirements.</param>
    /// <returns>Returns an instance of <see cref="AtRuleWrapper"/> configured with the specified supports condition.</returns>
    public static AtRuleWrapper Supports(string condition) => new("supports", condition);

    /// <summary>
    /// Creates a container query wrapper.
    /// </summary>
    /// <param name="query">The container query string to apply, defining specific conditions for rendering styles based on container properties.</param>
    /// <returns>Returns an instance of <see cref="AtRuleWrapper"/> configured with the specified container query.</returns>
    public static AtRuleWrapper Container(string query) => new("container", query);

    /// <summary>
    /// Creates a layer wrapper.
    /// </summary>
    /// <param name="layerName">The name of the CSS layer to define for grouping styles with hierarchical control.</param>
    /// <returns>Returns an instance of <see cref="AtRuleWrapper"/> configured with the specified layer name.</returns>
    public static AtRuleWrapper Layer(string layerName) => new("layer", layerName);

    /// <summary>
    /// Generates the CSS representation of the at-rule, including its parameters if provided.
    /// </summary>
    /// <returns>The CSS string for the at-rule, formatted as "@{Name}" if no parameters are present, or "@{Name} {Params}" otherwise.</returns>
    public string ToCss() => string.IsNullOrEmpty(Params) ? $"@{Name}" : $"@{Name} {Params}";

    /// <inheritdoc />
    public override string ToString() => ToCss();
}

/// <summary>
/// Represents a selector with its associated at-rule wrappers.
/// This is the result of applying variants to a base selector.
/// </summary>
public record AppliedSelector(Selector Selector, ImmutableArray<AtRuleWrapper> Wrappers)
{
    /// <summary>
    /// Creates an instance of <see cref="AppliedSelector"/> using the specified selector and initializes it with no wrappers.
    /// </summary>
    /// <param name="selector">The base selector to use for creating the AppliedSelector.</param>
    /// <returns>Returns an <see cref="AppliedSelector"/> instance with the provided selector and an empty wrappers collection.</returns>
    public static AppliedSelector FromSelector(Selector selector) =>
        new(selector, ImmutableArray<AtRuleWrapper>.Empty);

    /// <summary>
    /// Creates an AppliedSelector from a given class name.
    /// </summary>
    /// <param name="escapedClassName">The escaped class name representing the CSS selector, including any necessary escape sequences.</param>
    /// <returns>Returns a new instance of <see cref="AppliedSelector"/> with the specified class name and no wrappers.</returns>
    public static AppliedSelector FromClass(string escapedClassName) =>
        FromSelector(Selector.FromClass(escapedClassName));

    /// <summary>
    /// Adds an at-rule wrapper to this applied selector and returns a new instance with the wrapper applied.
    /// </summary>
    /// <param name="wrapper">The <see cref="AtRuleWrapper"/> to be added, defining a CSS at-rule for the selector.</param>
    /// <returns>Returns a new instance of <see cref="AppliedSelector"/> with the specified at-rule wrapper included.</returns>
    public AppliedSelector WithWrapper(AtRuleWrapper wrapper) =>
        new(Selector, Wrappers.Add(wrapper));

    /// <summary>
    /// Adds an array of at-rule wrappers to the current applied selector, extending its scope with the specified rules.
    /// </summary>
    /// <param name="wrappers">The array of <see cref="AtRuleWrapper"/> instances to add. Each wrapper represents a specific at-rule to associate with the selector.</param>
    /// <returns>A new instance of <see cref="AppliedSelector"/> containing the updated at-rule wrappers alongside the original selector.</returns>
    public AppliedSelector WithWrappers(params AtRuleWrapper[] wrappers) =>
        new(Selector, Wrappers.AddRange(wrappers));

    /// <summary>
    /// Replaces the current selector with a new selector while preserving the existing wrappers.
    /// </summary>
    /// <param name="selector">The new selector to apply.</param>
    /// <returns>Returns a new instance of <see cref="AppliedSelector"/> with the updated selector and the original wrappers.</returns>
    public AppliedSelector WithSelector(Selector selector) =>
        new(selector, Wrappers);

    /// <summary>
    /// Applies a transformation function to the current selector and returns a new <see cref="AppliedSelector"/> with the transformed selector.
    /// </summary>
    /// <param name="transform">A function that defines the transformation to apply to the selector.</param>
    /// <returns>A new <see cref="AppliedSelector"/> with the transformed selector while retaining the existing wrappers.</returns>
    public AppliedSelector TransformSelector(Func<Selector, Selector> transform) =>
        new(transform(Selector), Wrappers);

    /// <summary>
    /// Gets a value indicating whether checks if this applied selector has any at-rule wrappers.
    /// </summary>
    public bool HasWrappers => !Wrappers.IsEmpty;

    /// <inheritdoc />
    public override string ToString() =>
        HasWrappers
            ? $"{string.Join(" ", Wrappers.Select(w => w.ToCss()))} {{ {Selector} }}"
            : Selector.ToString();
}