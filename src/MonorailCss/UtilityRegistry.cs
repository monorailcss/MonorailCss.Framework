using System.Collections.Immutable;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Base;

namespace MonorailCss;

internal class UtilityRegistry
{
    private ImmutableList<IUtility> _utilities = ImmutableList<IUtility>.Empty;

    public UtilityRegistry(bool autoRegisterUtilities = true)
    {
        if (autoRegisterUtilities)
        {
            RegisterAllUtilities();
        }
    }

    public IReadOnlyList<IUtility> RegisteredUtilities => _utilities;

    public ImmutableDictionary<string, IUtility> StaticUtilitiesLookup { get; private set; } = ImmutableDictionary<string, IUtility>.Empty;

    public ImmutableHashSet<string> FunctionalRoots { get; private set; } = ImmutableHashSet<string>.Empty;

    public void RegisterUtility(IUtility utility)
    {
        _utilities = _utilities.Add(utility)
            .OrderBy(u => u.Priority)
            .ToImmutableList();

        RebuildIndexes();
    }

    private void RegisterAllUtilities()
    {
        var discoveredUtilities = UtilityDiscovery.DiscoverAllUtilities();

        _utilities = discoveredUtilities
            .OrderBy(u => u.Priority)
            .ToImmutableList();

        RebuildIndexes();
    }

    private void RebuildIndexes()
    {
        var dictionary = new Dictionary<string, IUtility>();

        // Index BaseStaticUtility types
        foreach (var baseStaticUtility in _utilities.OfType<BaseStaticUtility>())
        {
            foreach (var utilityName in baseStaticUtility.GetUtilityNames())
            {
                dictionary.Add(utilityName, baseStaticUtility);
            }
        }

        // Index custom static utilities that have a GetUtilityName method
        foreach (var customUtility in _utilities)
        {
            // Skip if already handled as BaseStaticUtility
            if (customUtility is BaseStaticUtility)
            {
                continue;
            }

            // Check if it has a GetUtilityName method (like our StaticCustomUtility)
            var getUtilityNameMethod = customUtility.GetType().GetMethod("GetUtilityName");
            if (getUtilityNameMethod != null && getUtilityNameMethod.ReturnType == typeof(string))
            {
                var utilityName = getUtilityNameMethod.Invoke(customUtility, null) as string;
                if (!string.IsNullOrEmpty(utilityName))
                {
                    dictionary.TryAdd(utilityName, customUtility);
                }
            }
        }

        StaticUtilitiesLookup = dictionary.ToImmutableDictionary();

        var functionalRoots = _utilities
            .SelectMany(u => u.GetFunctionalRoots())
            .ToImmutableHashSet();
        FunctionalRoots = functionalRoots;
    }
}