using JetBrains.Annotations;
using MonorailCss.Css;
using MonorailCss.Parser;

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

    /// <summary>
    /// Gets all the possible rules of syntax this plugin supports.
    /// </summary>
    /// <returns>An enumerable of all rules.</returns>
    IEnumerable<CssRuleSet> GetAllRules();
}

/// <summary>
/// Marks a plugin as having default values that will be applied to body, ::before and ::after.
/// </summary>
public interface IRegisterDefaults
{
    /// <summary>
    /// Gets the defaults.
    /// </summary>
    /// <returns>The defaults.</returns>
    CssDeclarationList GetDefaults();
}