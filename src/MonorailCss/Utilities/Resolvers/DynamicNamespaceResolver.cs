using System.Collections.Concurrent;

namespace MonorailCss.Utilities.Resolvers;

/// <summary>
/// Dynamically resolves namespaces based on registered utilities.
/// Replaces the static ThemeNamespaceRegistry with a dynamic approach.
/// </summary>
internal class DynamicNamespaceResolver
{
    private readonly ConcurrentDictionary<string, string[]> _namespaceCache = new();
    private readonly List<IUtility> _utilities = new();

    /// <summary>
    /// Registers a utility and its namespaces.
    /// </summary>
    public void RegisterUtility(IUtility utility)
    {
        _utilities.Add(utility);

        // Clear cache when new utility is registered to ensure fresh resolution
        _namespaceCache.Clear();
    }

    /// <summary>
    /// Gets namespace chains for a given utility pattern.
    /// Returns the first matching utility's namespaces.
    /// </summary>
    public string[] GetNamespaces(string pattern)
    {
        return _namespaceCache.GetOrAdd(pattern, _ =>
        {
            // Find the first utility that could handle this pattern
            // This is a simplified approach - in production, you might want
            // to match based on the utility's actual pattern matching logic
            foreach (var utility in _utilities)
            {
                var namespaces = utility.GetNamespaces();
                if (namespaces.Length > 0)
                {
                    // Check if this utility might handle the pattern
                    // This is where you'd implement more sophisticated matching
                    // For now, we'll return the namespaces if they exist
                    return namespaces;
                }
            }

            return [];
        });
    }

    /// <summary>
    /// Gets all registered namespace chains.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> GetAllNamespaces()
    {
        var result = new Dictionary<string, string[]>();

        foreach (var utility in _utilities)
        {
            var namespaces = utility.GetNamespaces();
            if (namespaces.Length > 0)
            {
                var utilityType = utility.GetType().Name;
                result[utilityType] = namespaces;
            }
        }

        return result;
    }

    /// <summary>
    /// Clears the namespace cache.
    /// </summary>
    public void ClearCache()
    {
        _namespaceCache.Clear();
    }
}