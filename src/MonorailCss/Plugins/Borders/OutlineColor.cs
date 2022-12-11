using MonorailCss.Css;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The outline-color plugin.
/// </summary>
public class OutlineColor : BaseColorNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutlineColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public OutlineColor(DesignSystem designSystem)
        : base(designSystem)
    {
    }

    /// <inheritdoc />
    protected override string Namespace() => "outline";

    protected override string ColorPropertyName() => CssProperties.OutlineColor;
}
