using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-fill plugin for -webkit-text-fill-color.
/// </summary>
public class TextFill : BaseColorNamespacePlugin
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextFill"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public TextFill(DesignSystem designSystem)
        : base(designSystem)
    {
    }

    /// <inheritdoc />
    protected override string Namespace() => "text-fill";

    /// <inheritdoc />
    protected override string ColorPropertyName() => CssProperties.WebkitTextFillColor;
}