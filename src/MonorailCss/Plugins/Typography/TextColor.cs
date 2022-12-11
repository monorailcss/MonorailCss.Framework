using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-color plugin.
/// </summary>
public class TextColor : BaseColorNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public TextColor(DesignSystem designSystem)
        : base(designSystem)
    {
    }

    /// <inheritdoc />
    protected override string Namespace() => "text";

    /// <inheritdoc />
    protected override string ColorPropertyName() => CssProperties.Color;

    /// <inheritdoc />
    protected override bool ShouldSplitOpacityIntoOwnProperty(out string propertyName)
    {
        propertyName = "text-opacity";
        return true;
    }
}