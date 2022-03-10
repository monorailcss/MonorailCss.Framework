using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace MonorailCss.Plugins;

/// <summary>
/// Represents a mapping of a namespace to property values.
/// </summary>
public record CssNamespaceToPropertyMap : IEnumerable
{
    private readonly ConcurrentDictionary<string, CssMultipleValue> _items = new();

    /// <summary>
    /// Adds a new namespace with a property map.
    /// </summary>
    /// <param name="ns">The namespace.</param>
    /// <param name="map">The property map.</param>
    public void Add(string ns, CssMultipleValue map)
    {
        _items.AddOrUpdate(ns, _ => map, (_, _) => map);
    }

    /// <summary>
    /// Gets all the namespaces for this mapping.
    /// </summary>
    public IEnumerable<string> Namespaces => _items.Keys;

    /// <inheritdoc />
    public IEnumerator GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Gets whether this namespace is defined.
    /// </summary>
    /// <param name="ns">The namespace.</param>
    /// <returns>True if it does contain, false if not.</returns>
    public bool ContainsNamespace(string ns) => _items.ContainsKey(ns);

    /// <summary>
    /// Gets the property value for this namespace.
    /// </summary>
    /// <param name="ns">The namespace.</param>
    public CssMultipleValue this[string ns]
    {
        get => _items[ns];
    }

    /// <summary>
    /// Represents a CSS property value list.
    /// </summary>
    /// <param name="Values">The values for the CSS property.</param>
    public record CssMultipleValue(string[] Values)
    {
        /// <summary>
        /// Converts a string to a property value list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new <see cref="CssMultipleValue"/>.</returns>
        public static implicit operator CssMultipleValue(string value) => new(new[] { value });

        /// <summary>
        /// Converts a string array to a property value list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new <see cref="CssMultipleValue"/>.</returns>
        public static implicit operator CssMultipleValue(string[] value) => new(value);

        /// <summary>
        /// Converts a tuple to a property value list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new <see cref="CssMultipleValue"/>.</returns>
        public static implicit operator CssMultipleValue((string Value1, string Value2) value) =>
            new(new[] { value.Value1, value.Value2 });

        /// <summary>
        /// Converts a tuple to a property value list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new <see cref="CssMultipleValue"/>.</returns>
        public static implicit operator CssMultipleValue((string Value1, string Value2, string Value3) value) =>
            new(new[] { value.Value1, value.Value2, value.Value3 });

        /// <summary>
        /// Converts a tuple to a property value list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new <see cref="CssMultipleValue"/>.</returns>
        public static implicit operator CssMultipleValue(
            (string Value1, string Value2, string Value3, string Value4) value) =>
            new(new[] { value.Value1, value.Value2, value.Value3, value.Value4 });
    }
}

    /// <summary>
    /// Represents a collection of suffixes to values for namespace mapping.
    /// </summary>
public record CssSuffixToValueMap : IEnumerable
    {
        private ImmutableDictionary<string, string> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="CssSuffixToValueMap"/> class.
        /// </summary>
        /// <param name="initialValues">Initial values, if available.</param>
        public CssSuffixToValueMap(ImmutableDictionary<string, string>? initialValues = default)
        {
            _items = initialValues ?? ImmutableDictionary<string, string>.Empty;
        }

        /// <summary>
        /// Adds a new suffix and value pair to the map.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        /// <param name="value">The value.</param>
        public void Add(string suffix, string value)
        {
            _items = _items.Add(suffix, value);
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// Determines whether the map contains the suffix.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        /// <returns>True if the mapping contains the suffix, otherwise false.</returns>
        public bool ContainsSuffix(string suffix)
        {
            return _items.ContainsKey(suffix);
        }

        /// <summary>
        /// Gets the mapping for a suffix.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        public string this[string suffix]
        {
            get => _items[suffix];
        }

        /// <summary>
        /// Converts an <see cref="ImmutableDictionary{TKey,TValue}"/> to a <see cref="CssSuffixToValueMap"/>.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>A new instance of <see cref="CssSuffixToValueMap"/>.</returns>
        public static implicit operator CssSuffixToValueMap(ImmutableDictionary<string, string> values) => new(values);
    }