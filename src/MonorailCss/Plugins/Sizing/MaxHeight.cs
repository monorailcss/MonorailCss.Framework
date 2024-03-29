﻿using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MaxHeight : BaseUtilityNamespacePlugin
{
    private const string Namespace = "max-h";
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxHeight"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public MaxHeight(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new() { new(Namespace, "max-height"), };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new Dictionary<string, string>
            {
                { "full", "100%" },
                { "screen", "100vh" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
            }.ToImmutableDictionary()
            .AddRange(_designSystem.Spacing);
    }
}