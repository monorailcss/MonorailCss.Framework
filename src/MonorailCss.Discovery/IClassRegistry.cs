namespace MonorailCss.Discovery;

/// <summary>
/// Live view of every CSS-utility-class candidate the discovery pipeline currently believes
/// the running app uses. Embedders (e.g. component frameworks that own their own CSS endpoint)
/// inject this and feed the set straight into <c>CssFramework.Process</c>, skipping the
/// built-in middleware.
/// </summary>
public interface IClassRegistry
{
    /// <summary>
    /// Gets the current set of discovered classes. The collection is an immutable snapshot —
    /// later updates produce a new instance. Safe to call from any thread.
    /// </summary>
    /// <returns>An immutable snapshot of every class the discovery pipeline has validated.</returns>
    IReadOnlyCollection<string> GetClasses();

    /// <summary>
    /// Gets a monotonically increasing token that changes whenever the class set is regenerated.
    /// Embedders use this to invalidate their own cached CSS without re-comparing the set.
    /// </summary>
    string Version { get; }
}
