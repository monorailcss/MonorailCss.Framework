using System.Diagnostics.CodeAnalysis;

namespace MonorailCss.Css;

/// <summary>
/// Registry for tracking CSS custom properties used during compilation. A fresh instance is
/// created per compile (per <c>Process</c> call) and used single-threaded within that call, so a
/// plain dictionary is sufficient — the insertion log + Checkpoint/RollbackTo below would not be
/// concurrency-safe and don't need to be.
/// </summary>
public class CssPropertyRegistry
{
    // Plain Dictionary (not ConcurrentDictionary): the matching loop checkpoints before each utility
    // probe and rolls back registrations from probes that don't match. A Dictionary reuses freed
    // entry slots on the subsequent re-add, so this rollback churn allocates nothing; a
    // ConcurrentDictionary would allocate a fresh node on every re-add.
    private readonly Dictionary<string, CssPropertyDefinition> _properties = new();

    // Insertion log backing Checkpoint/RollbackTo. The main matching loop probes every utility
    // per candidate; utilities that register @property metadata before confirming a match would
    // otherwise leak those declarations into unrelated output. The loop checkpoints before each
    // probe and rolls back if the utility doesn't match.
    private readonly List<string> _insertionOrder = new();

    /// <summary>
    /// Registers a CSS custom property with its metadata.
    /// </summary>
    /// <param name="definition">The property definition to register.</param>
    public void Register(CssPropertyDefinition definition)
    {
        if (_properties.TryAdd(definition.Name, definition))
        {
            _insertionOrder.Add(definition.Name);
        }
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
        _insertionOrder.Clear();
    }

    /// <summary>
    /// Captures the current registration count so registrations made after this point can be
    /// discarded with <see cref="RollbackTo"/>. Used by the compile loop to undo property
    /// registrations performed by a utility that turns out not to match the candidate.
    /// </summary>
    /// <returns>An opaque checkpoint token.</returns>
    internal int Checkpoint() => _insertionOrder.Count;

    /// <summary>
    /// Removes every property registered since the given <see cref="Checkpoint"/>.
    /// </summary>
    /// <param name="checkpoint">A token previously returned by <see cref="Checkpoint"/>.</param>
    internal void RollbackTo(int checkpoint)
    {
        for (var i = _insertionOrder.Count - 1; i >= checkpoint; i--)
        {
            _properties.Remove(_insertionOrder[i]);
        }

        _insertionOrder.RemoveRange(checkpoint, _insertionOrder.Count - checkpoint);
    }

    /// <summary>
    /// Gets the number of registered properties.
    /// </summary>
    public int Count => _properties.Count;
}