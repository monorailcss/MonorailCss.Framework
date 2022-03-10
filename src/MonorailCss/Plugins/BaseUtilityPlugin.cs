using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// Helper class for plugins that register a group of utilities.
/// </summary>
public abstract class BaseUtilityPlugin : IUtilityPlugin
{
    /// <summary>
    /// Gets the CSS property this utility returns.
    /// </summary>
    protected abstract string Property { get; }

    /// <summary>
    /// Gets a list of utilities and their values.
    /// </summary>
    protected abstract ImmutableDictionary<string, string> Utilities { get; }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        var utility = utilitySyntax.Name.ToLowerInvariant();
        if (!Utilities.ContainsKey(utility))
        {
            yield break;
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, new CssDeclarationList { new(Property, Utilities[utility]), });
    }
}