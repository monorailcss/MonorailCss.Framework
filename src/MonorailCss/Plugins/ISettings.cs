using JetBrains.Annotations;

namespace MonorailCss.Plugins;

/// <summary>
/// Represents the settings for a plugin.
/// </summary>
/// <typeparam name="T">A plugin type.</typeparam>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public interface ISettings<T>
    where T : class, IUtilityPlugin
{
}