using System.Collections.Immutable;
using MonorailCss.Variants;

namespace MonorailCss.Plugins;

/// <summary>
/// A plugin with a namespace.
/// </summary>
public interface IUtilityNamespacePlugin : IUtilityPlugin
{
    /// <summary>
    /// Gets the namespaces for a namespaced plugin.
    /// </summary>
    /// <returns>The namespaces.</returns>
    ImmutableArray<string> Namespaces { get; }
}

/// <summary>
/// A plugin that configures variants.
/// </summary>
public interface IVariantPluginProvider
{
    /// <summary>
    /// Gets a list of variants this plugin provides.
    /// </summary>
    /// <returns>An enumerable of the variants provided.</returns>
    IEnumerable<(string Modifier, IVariant Variant)> GetVariants();
}