using MonorailCss.Utilities;

namespace MonorailCss.Parser.Custom;

/// <summary>
/// Factory for creating custom utility instances from utility definitions.
/// </summary>
public static class CustomUtilityFactory
{
    /// <summary>
    /// Creates a static custom utility from a utility definition.
    /// </summary>
    /// <param name="definition">The utility definition parsed from CSS.</param>
    /// <returns>A static custom utility instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    /// <exception cref="ArgumentException">Thrown when definition uses wildcard patterns.</exception>
    public static IUtility CreateStaticUtility(UtilityDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (definition.IsWildcard)
        {
            throw new ArgumentException(
                $"Cannot create static utility for wildcard pattern '{definition.Pattern}'. " +
                "Use CreateDynamicUtility for wildcard patterns.",
                nameof(definition));
        }

        return new StaticCustomUtility(definition);
    }

    /// <summary>
    /// Creates a dynamic custom utility from a utility definition with wildcard patterns.
    /// </summary>
    /// <param name="definition">The utility definition parsed from CSS.</param>
    /// <returns>A dynamic custom utility instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    public static IUtility CreateDynamicUtility(UtilityDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (!definition.IsWildcard)
        {
            throw new ArgumentException(
                $"Pattern '{definition.Pattern}' does not contain wildcards. " +
                "Use CreateStaticUtility for static patterns.",
                nameof(definition));
        }

        return new DynamicCustomUtility(definition);
    }

    /// <summary>
    /// Creates an appropriate utility instance based on the definition's characteristics.
    /// </summary>
    /// <param name="definition">The utility definition parsed from CSS.</param>
    /// <returns>An appropriate utility instance (static or dynamic).</returns>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    public static IUtility CreateUtility(UtilityDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        return definition.IsWildcard
            ? CreateDynamicUtility(definition)
            : CreateStaticUtility(definition);
    }

    /// <summary>
    /// Creates multiple utility instances from a collection of definitions.
    /// </summary>
    /// <param name="definitions">The utility definitions to create utilities from.</param>
    /// <returns>A collection of created utility instances.</returns>
    public static IEnumerable<IUtility> CreateUtilities(IEnumerable<UtilityDefinition> definitions)
    {
        if (definitions == null)
        {
            throw new ArgumentNullException(nameof(definitions));
        }

        var utilities = new List<IUtility>();

        foreach (var definition in definitions)
        {
            utilities.Add(CreateUtility(definition));
        }

        return utilities;
    }
}