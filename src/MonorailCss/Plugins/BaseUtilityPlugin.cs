using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins;

/// <summary>
/// Helper class for plugins that register a group of utilities.
/// </summary>
public abstract class BaseUtilityPlugin : IUtilityPlugin
{
    private ImmutableDictionary<string, string>? _utilityValues;

    /// <summary>
    /// Gets the CSS property this utility returns.
    /// </summary>
    protected abstract string Property { get; }

    /// <summary>
    /// Gets a list of utilities and their values.
    /// </summary>
    /// <returns>The list of utilities.</returns>
    protected abstract ImmutableDictionary<string, string> GetUtilities();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        _utilityValues ??= GetUtilities();

        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        var utility = utilitySyntax.Name;

        if (!_utilityValues.ContainsKey(utility))
        {
            yield break;
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, new CssDeclarationList { new(Property, _utilityValues[utility]), });
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        _utilityValues ??= GetUtilities();
        return _utilityValues.ToArray().Select(i => new CssRuleSet(
            i.Key,
            new CssDeclarationList { new(Property, i.Value), }));
    }
}