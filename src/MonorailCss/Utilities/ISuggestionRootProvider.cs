namespace MonorailCss.Utilities;

/// <summary>
/// Implemented by functional utilities whose root is not registered through
/// <see cref="IUtility.GetFunctionalRoots"/> (they rely on the parser's first-dash fallback —
/// <c>text-</c>, <c>accent-</c>, <c>from-</c>, the <see cref="Base.BaseColorUtility"/> family) so
/// <see cref="UtilityClassNameEnumerator"/> can still expand them into suggestions. Deliberately
/// separate from <see cref="IUtility.GetFunctionalRoots"/>: registering these as functional roots
/// would change how the parser splits candidates, whereas this only feeds autocomplete.
/// </summary>
internal interface ISuggestionRootProvider
{
    /// <summary>
    /// Gets the roots to expand over this utility's theme namespaces (e.g. <c>["bg"]</c>).
    /// </summary>
    /// <returns>The suggestion roots.</returns>
    string[] GetSuggestionRoots();
}
