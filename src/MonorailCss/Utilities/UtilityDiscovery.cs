namespace MonorailCss.Utilities;

/// <summary>
/// Provides the set of built-in utilities. The list is produced at compile time by the
/// MonorailCss source generator (<c>GeneratedUtilityRegistry</c>) rather than by reflecting over
/// the assembly, so the library is trim-safe and AOT-compatible and construction avoids an
/// assembly scan plus a per-utility <c>Activator.CreateInstance</c>.
/// </summary>
internal static class UtilityDiscovery
{
    /// <summary>
    /// Returns one instance of every built-in utility, ordered by priority then type name to
    /// preserve the deterministic evaluation order the reflection-based discovery used.
    /// </summary>
    public static IEnumerable<IUtility> DiscoverAllUtilities()
    {
        return GeneratedUtilityRegistry.CreateAll()
            .OrderBy(u => u.Priority)
            .ThenBy(u => u.GetType().Name, StringComparer.Ordinal);
    }
}
