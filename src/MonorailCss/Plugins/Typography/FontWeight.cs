using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-weight plugin.
/// </summary>
public class FontWeight : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-weight";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "font-thin", "100" },
            { "font-extralight", "200" },
            { "font-light", "300" },
            { "font-normal", "400" },
            { "font-medium", "500" },
            { "font-semibold", "600" },
            { "font-bold", "700" },
            { "font-extrabold", "800" },
            { "font-black", "900" },
        }.ToImmutableDictionary();
}