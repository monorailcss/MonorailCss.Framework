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

        // Index static utilities that expose exact names but aren't BaseStaticUtility (built-ins
        // like inset-shadow/drop-shadow/mask/outline-hidden, plus custom @utility definitions).
        // Implementing IStaticUtilityNameProvider replaces the previous reflection-based duck typing,
        // keeping this path trim- and AOT-safe.
        foreach (var customUtility in _utilities)
        {
            if (customUtility is BaseStaticUtility)
            {
                continue;
            }

            if (customUtility is IStaticUtilityNameProvider nameProvider)
            {
                foreach (var utilityName in nameProvider.GetUtilityNames())
                {
                    if (!string.IsNullOrEmpty(utilityName))
                    {
                        dictionary.TryAdd(utilityName, customUtility);
                    }
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