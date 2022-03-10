using JetBrains.Annotations;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// A plugin for processing utility classes.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Itself | ImplicitUseTargetFlags.WithInheritors)]
public interface IUtilityPlugin
{
    /// <summary>
    /// Returns a list of css properties for this utility.
    /// </summary>
    /// <param name="syntax">The parsed utility class.</param>
    /// <returns>A list of CSS properties for this syntax.</returns>
    IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax);
}