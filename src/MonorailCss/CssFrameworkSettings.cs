using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using MonorailCss.Theme;

namespace MonorailCss;

/// <summary>
/// Represents configuration settings for the CSS framework, including theme customization,
/// variant handling, and other framework-level options.
/// </summary>
public record CssFrameworkSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CssFrameworkSettings"/> class.
    /// </summary>
    public CssFrameworkSettings()
    {
        Theme = new Theme.Theme();
        Variants = ImmutableHashSet<string>.Empty;
        Important = false;
        IncludePreflight = true;
        Applies = ImmutableDictionary<string, string>.Empty;
        ProseCustomization = null;
        CustomUtilities = ImmutableList<UtilityDefinition>.Empty;
    }

    /// <summary>
    /// Gets the theme configuration used by the framework, including design tokens and scales.
    /// </summary>
    public Theme.Theme Theme { get; init; }

    /// <summary>
    /// Gets the set of enabled variant identifiers (e.g., responsive breakpoints, state variants).
    /// </summary>
    public ImmutableHashSet<string> Variants { get; init; }

    /// <summary>
    /// Gets a value indicating whether generated utilities should be emitted with the !important flag.
    /// </summary>
    public bool Important { get; init; }

    /// <summary>
    /// Gets a value indicating whether the framework's base (preflight) styles should be included.
    /// </summary>
    public bool IncludePreflight { get; init; }

    /// <summary>
    /// Gets a map of custom apply rules where the key is a selector or alias and the value is a space-separated list of utilities or raw CSS to apply.
    /// </summary>
    public ImmutableDictionary<string, string> Applies { get; init; }

    /// <summary>
    /// Gets the optional customization for prose/typographic styles.
    /// </summary>
    public ProseCustomization? ProseCustomization { get; init; }

    /// <summary>
    /// Gets a list of custom utility definitions to be registered with the framework.
    /// These are typically parsed from @utility blocks in CSS source files.
    /// </summary>
    public ImmutableList<UtilityDefinition> CustomUtilities { get; init; }
}