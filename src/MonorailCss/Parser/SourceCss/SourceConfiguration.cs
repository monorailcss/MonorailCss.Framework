using System.Collections.Immutable;

namespace MonorailCss.Parser.SourceCss;

/// <summary>
/// Aggregated configuration extracted from <c>@import</c>, <c>@source</c>, <c>@source not</c>,
/// <c>@source inline()</c>, and <c>@custom-variant</c> directives in a Tailwind v4-style CSS source.
/// </summary>
public record SourceConfiguration
{
    /// <summary>
    /// Gets the base path for automatic source detection from <c>@import "tailwindcss" source(…)</c>.
    /// Null means use the default (the input file's directory).
    /// </summary>
    public string? BasePath { get; init; }

    /// <summary>
    /// Gets a value indicating whether automatic source detection is disabled
    /// (set when <c>@import "tailwindcss" source(none)</c> is specified).
    /// </summary>
    public bool DisableAutoDetection { get; init; }

    /// <summary>
    /// Gets the explicit <c>@source "…"</c> include directives.
    /// </summary>
    public ImmutableList<SourceDirective> IncludeSources { get; init; } = ImmutableList<SourceDirective>.Empty;

    /// <summary>
    /// Gets the <c>@source not "…"</c> exclude directives.
    /// </summary>
    public ImmutableList<SourceDirective> ExcludeSources { get; init; } = ImmutableList<SourceDirective>.Empty;

    /// <summary>
    /// Gets the <c>@source inline("…")</c> safelist directives, with brace-expansion already
    /// resolved into the concrete utility names.
    /// </summary>
    public ImmutableList<InlineSourceDirective> InlineSources { get; init; } = ImmutableList<InlineSourceDirective>.Empty;

    /// <summary>
    /// Gets the <c>@custom-variant</c> definitions parsed from the source.
    /// </summary>
    public ImmutableList<CustomVariantDefinition> CustomVariants { get; init; } = ImmutableList<CustomVariantDefinition>.Empty;

    /// <summary>
    /// Gets every <c>@import</c> directive parsed from the source, including its modifier
    /// (<c>source()</c>, <c>theme()</c>, <c>layer()</c>) when present.
    /// </summary>
    public ImmutableList<ImportDirective> Imports { get; init; } = ImmutableList<ImportDirective>.Empty;

    /// <summary>
    /// Gets a value indicating whether the configuration has any non-default content.
    /// </summary>
    public bool HasConfiguration =>
        BasePath != null
        || DisableAutoDetection
        || IncludeSources.Count > 0
        || ExcludeSources.Count > 0
        || InlineSources.Count > 0
        || CustomVariants.Count > 0
        || Imports.Count > 0;
}

/// <summary>
/// A single <c>@source</c> include or <c>@source not</c> exclude directive.
/// </summary>
public record SourceDirective
{
    /// <summary>
    /// Gets the path or glob pattern.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether <see cref="Path"/> ends in <c>.dll</c>.
    /// </summary>
    public bool IsDll => Path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the absolute path resolved by the consumer (left null until resolution).
    /// </summary>
    public string? ResolvedPath { get; init; }
}

/// <summary>
/// A safelist directive: <c>@source inline("…")</c>. The pattern may use brace expansion
/// (<c>bg-red-{50,100,200}</c>), variant groups (<c>{hover:,focus:,}underline</c>), or numeric
/// ranges (<c>bg-red-{100..900..100}</c>); <see cref="ExpandedUtilities"/> holds the resolved
/// concrete utility names.
/// </summary>
public record InlineSourceDirective
{
    /// <summary>
    /// Gets the raw pattern from the directive.
    /// </summary>
    public string Pattern { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expanded list of concrete utility class names.
    /// </summary>
    public ImmutableList<string> ExpandedUtilities { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Modifier types attached to <c>@import</c> directives.
/// </summary>
public enum ImportModifier
{
    /// <summary>No modifier specified.</summary>
    None,

    /// <summary>The <c>source()</c> modifier (controls auto-detection).</summary>
    Source,

    /// <summary>The <c>theme()</c> modifier (controls how the imported theme integrates).</summary>
    Theme,

    /// <summary>The <c>layer()</c> modifier (assigns the import to a CSS layer).</summary>
    Layer,
}

/// <summary>
/// A parsed <c>@import</c> directive.
/// </summary>
public record ImportDirective
{
    /// <summary>
    /// Gets the import path (e.g. <c>"tailwindcss"</c>, <c>"./fonts.css"</c>).
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the modifier type when present.
    /// </summary>
    public ImportModifier Modifier { get; init; } = ImportModifier.None;

    /// <summary>
    /// Gets the modifier value when present (e.g. <c>"static"</c> for <c>theme(static)</c>,
    /// <c>"utilities"</c> for <c>layer(utilities)</c>, the path for <c>source("…")</c>).
    /// </summary>
    public string? ModifierValue { get; init; }
}
