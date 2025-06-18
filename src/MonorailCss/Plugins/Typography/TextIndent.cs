namespace MonorailCss.Plugins.Typography;

/// <summary>
/// Text Indent plugin.
/// </summary>
public class TextIndent : BaseUtilityNamespacePlugin
{
    private const string Namespace = "indent";
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextIndent"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public TextIndent(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => [new(Namespace, "text-indent")];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return SizeHelpers.Percentages
            .AddRange(new Dictionary<string, string>
            {
                { "px", "1px" },
            });
    }

    /// <inheritdoc />
    protected override bool SupportsDynamicValues(out string cssVariableName, out string calculationPattern)
    {
        cssVariableName = "spacing";
        calculationPattern = "calc(var({0}) * {1})";
        return true;
    }
}