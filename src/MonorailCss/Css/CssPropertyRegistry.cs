using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MonorailCss.Css;

/// <summary>
/// Registry for tracking CSS custom properties used during compilation.
/// Thread-safe for concurrent access.
/// </summary>
public class CssPropertyRegistry
{
    private readonly ConcurrentDictionary<string, CssPropertyDefinition> _properties = new();

    /// <summary>
    /// Registers a CSS custom property with its metadata.
    /// </summary>
    /// <param name="definition">The property definition to register.</param>
    public void Register(CssPropertyDefinition definition)
    {
        _properties.TryAdd(definition.Name, definition);
    }

    /// <summary>
    /// Registers a CSS custom property with inline parameters.
    /// </summary>
    /// <param name="name">The CSS custom property name.</param>
    /// <param name="syntax">The CSS syntax type.</param>
    /// <param name="inherits">Whether the property should inherit.</param>
    /// <param name="initialValue">The initial value (null if no initial value should be output).</param>
    /// <param name="needsFallback">Whether fallback initialization is needed.</param>
    public void Register(string name, string syntax, bool inherits, string? initialValue, bool needsFallback = true)
    {
        Register(new CssPropertyDefinition(name, syntax, inherits, initialValue, needsFallback));
    }

    /// <summary>
    /// Retrieves all registered CSS property definitions.
    /// </summary>
    /// <returns>A read-only collection of all registered CSS property definitions.</returns>
    public IReadOnlyCollection<CssPropertyDefinition> GetAll()
    {
        return _properties.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Retrieves the collection of CSS property definitions that require fallback initialization.
    /// </summary>
    /// <returns>
    /// A read-only collection of CSS property definitions marked as needing fallback,
    /// and that have an initial value specified.
    /// </returns>
    public IReadOnlyCollection<CssPropertyDefinition> GetPropertiesNeedingFallback()
    {
        return _properties.Values.Where(p => p.NeedsFallback && p.InitialValue != null).ToList().AsReadOnly();
    }

    /// <summary>
    /// Determines whether a CSS property is registered in the registry.
    /// </summary>
    /// <param name="name">The name of the CSS property to check for registration.</param>
    /// <returns>True if the property is registered; otherwise, false.</returns>
    public bool IsRegistered(string name)
    {
        return _properties.ContainsKey(name);
    }

    /// <summary>
    /// Attempts to retrieve a registered CSS property definition by its name.
    /// </summary>
    /// <param name="name">The name of the CSS custom property to retrieve.</param>
    /// <param name="definition">
    /// When this method returns, contains the <see cref="CssPropertyDefinition"/> associated with the specified name if it exists,
    /// or null if the registration does not exist.
    /// </param>
    /// <returns>
    /// true if the property with the specified name is found; otherwise, false.
    /// </returns>
    public bool TryGet(string name, [NotNullWhen(true)] out CssPropertyDefinition? definition)
    {
        return _properties.TryGetValue(name, out definition);
    }

    /// <summary>
    /// Clears all registered properties.
    /// </summary>
    public void Clear()
    {
        _properties.Clear();
    }

    /// <summary>
    /// Gets the number of registered properties.
    /// </summary>
    public int Count => _properties.Count;
}