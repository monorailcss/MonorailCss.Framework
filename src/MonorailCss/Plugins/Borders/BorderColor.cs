using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-color plugin.
/// </summary>
public class BorderColor : BaseColorNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BorderColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BorderColor(DesignSystem designSystem) 
        : base(designSystem)
    {
    }

    /// <inheritdoc />
    protected override string Namespace() => "border";

    /// <inheritdoc />
    protected override string ColorPropertyName() => CssProperties.BorderColor;
}