namespace MonorailCss.Utilities;

/// <summary>
/// Implemented by utilities that expose a fixed set of exact (static) class names but are not
/// <see cref="Base.BaseStaticUtility"/> subclasses. <see cref="UtilityRegistry"/> uses this to
/// index those names without reflection. Implement this on a custom <see cref="IUtility"/> when its
/// names must be matched exactly by the parser before functional roots are considered.
/// </summary>
public interface IStaticUtilityNameProvider
{
    /// <summary>
    /// Gets the exact class names this utility handles (e.g. <c>inset-shadow-sm</c>,
    /// <c>outline-hidden</c>).
    /// </summary>
    /// <returns>The static class names handled by this utility.</returns>
    IEnumerable<string> GetUtilityNames();
}
