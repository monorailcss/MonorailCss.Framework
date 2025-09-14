using System.Collections.Concurrent;

namespace MonorailCss.Css;

/// <summary>
/// Registry for tracking CSS custom properties used during compilation.
/// Thread-safe for concurrent access.
/// </summary>
internal class CssPropertyRegistry
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
    /// Gets all registered properties.
    /// </summary>
    public IReadOnlyCollection<CssPropertyDefinition> GetAll()
    {
        return _properties.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets properties that need fallback initialization.
    /// </summary>
    public IReadOnlyCollection<CssPropertyDefinition> GetPropertiesNeedingFallback()
    {
        return _properties.Values.Where(p => p.NeedsFallback && p.InitialValue != null).ToList().AsReadOnly();
    }

    /// <summary>
    /// Checks if a property is registered.
    /// </summary>
    public bool IsRegistered(string name)
    {
        return _properties.ContainsKey(name);
    }

    /// <summary>
    /// Attempts to get a registered property definition.
    /// </summary>
    public bool TryGet(string name, out CssPropertyDefinition? definition)
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