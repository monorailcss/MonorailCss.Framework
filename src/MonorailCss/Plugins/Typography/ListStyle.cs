using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The list-style plugin.
/// </summary>
public class ListStyle : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "list-style-type";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>()
        {
            { "list-none", "none" }, { "list-disc", "disc" }, { "list-decimal", "decimal" },
        }.ToImmutableDictionary();
    }
}

/// <summary>
/// The list-style-position plugin.
/// </summary>
public class ListStylePosition : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "list-style-position";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>() { { "list-inside", "inside" }, { "list-outside", "outside" }, }
            .ToImmutableDictionary();
    }
}