using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The Order plugin.
/// </summary>
public class Order : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "order";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "order-1", "1" },
            { "order-2", "2" },
            { "order-3", "3" },
            { "order-4", "4" },
            { "order-5", "5" },
            { "order-6", "6" },
            { "order-7", "7" },
            { "order-8", "8" },
            { "order-9", "9" },
            { "order-10", "10" },
            { "order-11", "11" },
            { "order-12", "12" },
            { "order-first", "-9999" },
            { "order-last", "9999" },
            { "order-none", "0" },
        }.ToImmutableDictionary();
}
