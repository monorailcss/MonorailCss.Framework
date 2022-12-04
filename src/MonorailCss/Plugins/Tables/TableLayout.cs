using System.Collections.Immutable;

namespace MonorailCss.Plugins.Tables;

/// <summary>
/// The table layout plugin.
/// </summary>
public class TableLayout : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "table-layout";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "table-auto", "auto" },
            { "table-fixed", "fixed" },
        }.ToImmutableDictionary();
}