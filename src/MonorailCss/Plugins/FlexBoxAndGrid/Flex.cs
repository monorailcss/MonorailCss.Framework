using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex plugin.
/// </summary>
public class Flex : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>
    {
        { "flex-1", "1 1 0;" }, { "flex-auto", "1 1 auto" }, { "flex-initial", "0 1 auto" }, { "flex-none", "none" },
    }.ToImmutableDictionary();
}

/// <summary>
/// The flex-grow plugin.
/// </summary>
public class FlexGrow : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-grow";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities =>
        new Dictionary<string, string>() { { "grow", "1" }, { "grow-0", "0" }, }.ToImmutableDictionary();
}

/// <summary>
/// The flex-shrink plugin.
/// </summary>
public class FlexShrink : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-shrink";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities =>
        new Dictionary<string, string>() { { "shrink", "1" }, { "shrink-0", "0" }, }.ToImmutableDictionary();
}

/// <summary>
/// The flex-direction plugin.
/// </summary>
public class FlexDirection : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-direction";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities =>
        new Dictionary<string, string>()
        {
            { "flex-row", "row" },
            { "flex-row-reverse", "row-reverse" },
            { "flex-col", "column" },
            { "flex-col-reverse", "column-reverse" },
        }.ToImmutableDictionary();
}

/// <summary>
/// The flex-shrink plugin.
/// </summary>
public class FlexWrap : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-wrap";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities =>
        new Dictionary<string, string>()
        {
            { "flex-wrap", "wrap" },
            { "flex-wrap-reverse", "wrap-reverse" },
            { "flex-nowrap", "nowrap" },
        }.ToImmutableDictionary();
}
