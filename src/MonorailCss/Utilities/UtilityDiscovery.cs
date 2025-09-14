using System.Reflection;

namespace MonorailCss.Utilities;

/// <summary>
/// Discovers and creates instances of all utilities that implement IUtility.
/// </summary>
internal static class UtilityDiscovery
{
    /// <summary>
    /// Discovers all concrete utilities that implement IUtility in the current assembly.
    /// </summary>
    public static IEnumerable<IUtility> DiscoverAllUtilities()
    {
        return DiscoverUtilitiesFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Discovers all utilities from specified assemblies.
    /// </summary>
    public static IEnumerable<IUtility> DiscoverUtilitiesFromAssemblies(params Assembly[] assemblies)
    {
        var utilities = new List<IUtility>();

        foreach (var assembly in assemblies)
        {
            utilities.AddRange(DiscoverUtilitiesFromAssembly(assembly));
        }

        // Sort by priority (the utilities themselves define their priority)
        return utilities
            .OrderBy(u => u.Priority)
            .ThenBy(u => u.GetType().Name);
    }

    /// <summary>
    /// Discovers utilities from a specific assembly.
    /// </summary>
    public static IEnumerable<IUtility> DiscoverUtilitiesFromAssembly(Assembly assembly)
    {
        var utilityTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(IUtility).IsAssignableFrom(t))
            .Where(t => t.GetConstructor(Type.EmptyTypes) != null) // Has parameterless constructor
            .ToList();

        var utilities = new List<IUtility>();

        foreach (var type in utilityTypes)
        {
            if (TryCreateInstance(type, out var instance))
            {
                utilities.Add(instance);
            }
        }

        return utilities
            .OrderBy(u => u.Priority)
            .ThenBy(u => u.GetType().Name);
    }

    private static bool TryCreateInstance(Type type, out IUtility instance)
    {
        instance = null!;

        try
        {
            // Look for a parameterless constructor
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                instance = (IUtility)Activator.CreateInstance(type)!;
                return true;
            }
        }
        catch (Exception ex)
        {
            // Could log the error if needed
            Console.WriteLine($"Failed to create instance of {type.Name}: {ex.Message}");
        }

        return false;
    }
}