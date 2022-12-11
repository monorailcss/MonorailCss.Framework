using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background color plugin.
/// </summary>
public class BackgroundColor : BaseColorNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BackgroundColor(DesignSystem designSystem)
        : base(designSystem)
    {
    }

    /// <inheritdoc />
    protected override string Namespace() => "bg";

    /// <inheritdoc />
    protected override string ColorPropertyName() => "background-color";

    /// <inheritdoc />
    protected override bool ShouldSplitOpacityIntoOwnProperty([NotNullWhen(true)] out string? propertyName)
    {
        propertyName = "bg-opacity";
        return true;
    }
}