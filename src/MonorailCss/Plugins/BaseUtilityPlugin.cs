using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// Helper class for plugins that register a group of utilities.
/// </summary>
public abstract class BaseUtilityPlugin : IUtilityPlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUtilityPlugin"/> class.
    /// </summary>
    protected BaseUtilityPlugin()
    {
        _utilityValues = new Lazy<ImmutableDictionary<string, string>>(GetUtilities);
    }

    private readonly Lazy<ImmutableDictionary<string, string>> _utilityValues;

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
        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        var utility = utilitySyntax.Name;

        if (!_utilityValues.Value.ContainsKey(utility))
        {
            yield break;
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, new CssDeclarationList { new(Property, _utilityValues.Value[utility]), });
    }
}