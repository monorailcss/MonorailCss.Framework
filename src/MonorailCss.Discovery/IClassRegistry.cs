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
    /// Gets an opaque token derived from the current generated CSS — suitable for an HTTP
    /// <c>ETag</c> / <c>If-None-Match</c> round-trip. The token changes whenever the class set
    /// or source CSS changes, but is content-derived rather than monotonic, so reverting to a
    /// prior class set yields the prior token. Embedders compare against the previously-seen
    /// value to decide whether to re-emit cached CSS.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the fully assembled CSS the discovery pipeline produced — the configured
    /// <see cref="MonorailDiscoveryOptions.SourceCss"/> (or the auto-loaded
    /// <c>wwwroot/app.css</c>) prefix plus the utilities generated from the discovered
    /// class set. Custom MapGet/MapMethods endpoints can return this directly to match
    /// what the built-in middleware serves at <see cref="MonorailDiscoveryOptions.CssEndpoint"/>.
    /// The string is recomputed only when the class set changes, so cheap to read.
    /// </summary>
    string Css { get; }
}
