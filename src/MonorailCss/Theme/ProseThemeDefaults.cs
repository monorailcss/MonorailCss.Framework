using System.Collections.Immutable;
using static MonorailCss.Utilities.Typography.ProseUtilityHelpers;

namespace MonorailCss.Theme;

/// <summary>
/// The <c>ProseThemeDefaults</c> class provides pre-defined typography and theme configurations
/// specifically tailored for handling prose-related styles in CSS generation.
/// This includes settings for various typography sizes and color themes.
/// </summary>
/// <remarks>
/// This class is used to centralize default styling configurations for prose-based elements,
/// ensuring consistency across different implementations while allowing for readability adjustments
/// at multiple levels such as small, large, or extra-large text.
/// </remarks>
/// <seealso cref="MonorailCss.Css.ThemeToCssConverter"/>
public static class ProseThemeDefaults
{
    /// <summary>Retrieves the default typography and color theme settings for prose styling.
    /// Combines base typography values, size modifiers, and color themes to create
    /// a comprehensive set of defaults for managing prose styles within the theme.
    /// </summary>
    /// <returns>
    /// An immutable dictionary containing the default settings for prose,
    /// with keys representing the style names and values representing their respective settings.
    /// </returns>
    public static ImmutableDictionary<string, string> GetProseDefaults()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();

        // Base typography values (DEFAULT size)
        AddBaseTypography(builder);

        // Size modifiers
        AddSmallTypography(builder);
        AddLargeTypography(builder);
        AddXLargeTypography(builder);
        Add2XLargeTypography(builder);

        // Color themes
        AddColorThemes(builder);

        return builder.ToImmutable();
    }

    private static void AddBaseTypography(ImmutableDictionary<string, string>.Builder builder)
    {
        // Root styles for base/default size
        builder.Add("--typography-base-font-size", Rem(16));
        builder.Add("--typography-base-line-height", Round(28.0 / 16.0));

        // Paragraph
        builder.Add("--typography-base-p-margin-top", Em(20, 16));
        builder.Add("--typography-base-p-margin-bottom", Em(20, 16));

        // Lead paragraph
        builder.Add("--typography-base-lead-font-size", Em(20, 16));
        builder.Add("--typography-base-lead-line-height", Round(32.0 / 20.0));
        builder.Add("--typography-base-lead-margin-top", Em(24, 20));
        builder.Add("--typography-base-lead-margin-bottom", Em(24, 20));

        // Blockquote
        builder.Add("--typography-base-blockquote-margin-top", Em(32, 20));
        builder.Add("--typography-base-blockquote-margin-bottom", Em(32, 20));
        builder.Add("--typography-base-blockquote-padding-inline-start", Em(20, 20));

        // Headings
        builder.Add("--typography-base-h1-font-size", Em(36, 16));
        builder.Add("--typography-base-h1-margin-top", "0");
        builder.Add("--typography-base-h1-margin-bottom", Em(32, 36));
        builder.Add("--typography-base-h1-line-height", Round(40.0 / 36.0));

        builder.Add("--typography-base-h2-font-size", Em(24, 16));
        builder.Add("--typography-base-h2-margin-top", Em(48, 24));
        builder.Add("--typography-base-h2-margin-bottom", Em(24, 24));
        builder.Add("--typography-base-h2-line-height", Round(32.0 / 24.0));

        builder.Add("--typography-base-h3-font-size", Em(20, 16));
        builder.Add("--typography-base-h3-margin-top", Em(32, 20));
        builder.Add("--typography-base-h3-margin-bottom", Em(12, 20));
        builder.Add("--typography-base-h3-line-height", Round(32.0 / 20.0));

        builder.Add("--typography-base-h4-margin-top", Em(24, 16));
        builder.Add("--typography-base-h4-margin-bottom", Em(8, 16));
        builder.Add("--typography-base-h4-line-height", Round(24.0 / 16.0));

        // Images and media
        builder.Add("--typography-base-img-margin-top", Em(32, 16));
        builder.Add("--typography-base-img-margin-bottom", Em(32, 16));
        builder.Add("--typography-base-video-margin-top", Em(32, 16));
        builder.Add("--typography-base-video-margin-bottom", Em(32, 16));
        builder.Add("--typography-base-figure-margin-top", Em(32, 16));
        builder.Add("--typography-base-figure-margin-bottom", Em(32, 16));
        builder.Add("--typography-base-figcaption-font-size", Em(14, 16));
        builder.Add("--typography-base-figcaption-line-height", Round(20.0 / 14.0));
        builder.Add("--typography-base-figcaption-margin-top", Em(12, 14));

        // Code
        builder.Add("--typography-base-code-font-size", Em(14, 16));
        builder.Add("--typography-base-h2-code-font-size", Em(21, 24));
        builder.Add("--typography-base-h3-code-font-size", Em(18, 20));
        builder.Add("--typography-base-pre-font-size", Em(14, 16));
        builder.Add("--typography-base-pre-line-height", Round(24.0 / 14.0));
        builder.Add("--typography-base-pre-margin-top", Em(24, 14));
        builder.Add("--typography-base-pre-margin-bottom", Em(24, 14));
        builder.Add("--typography-base-pre-border-radius", Rem(6));
        builder.Add("--typography-base-pre-padding-top", Em(12, 14));
        builder.Add("--typography-base-pre-padding-inline-end", Em(16, 14));
        builder.Add("--typography-base-pre-padding-bottom", Em(12, 14));
        builder.Add("--typography-base-pre-padding-inline-start", Em(16, 14));

        // Keyboard
        builder.Add("--typography-base-kbd-font-size", Em(14, 16));
        builder.Add("--typography-base-kbd-border-radius", Rem(5));
        builder.Add("--typography-base-kbd-padding-top", Em(3, 16));
        builder.Add("--typography-base-kbd-padding-inline-end", Em(6, 16));
        builder.Add("--typography-base-kbd-padding-bottom", Em(3, 16));
        builder.Add("--typography-base-kbd-padding-inline-start", Em(6, 16));

        // Lists
        builder.Add("--typography-base-ol-margin-top", Em(20, 16));
        builder.Add("--typography-base-ol-margin-bottom", Em(20, 16));
        builder.Add("--typography-base-ol-padding-inline-start", Em(26, 16));
        builder.Add("--typography-base-ul-margin-top", Em(20, 16));
        builder.Add("--typography-base-ul-margin-bottom", Em(20, 16));
        builder.Add("--typography-base-ul-padding-inline-start", Em(26, 16));
        builder.Add("--typography-base-li-margin-top", Em(8, 16));
        builder.Add("--typography-base-li-margin-bottom", Em(8, 16));
        builder.Add("--typography-base-ol-li-padding-inline-start", Em(6, 16));
        builder.Add("--typography-base-ul-li-padding-inline-start", Em(6, 16));

        // Nested lists
        builder.Add("--typography-base-ul-ul-margin-top", Em(12, 16));
        builder.Add("--typography-base-ul-ul-margin-bottom", Em(12, 16));
        builder.Add("--typography-base-ul-ol-margin-top", Em(12, 16));
        builder.Add("--typography-base-ul-ol-margin-bottom", Em(12, 16));
        builder.Add("--typography-base-ol-ul-margin-top", Em(12, 16));
        builder.Add("--typography-base-ol-ul-margin-bottom", Em(12, 16));
        builder.Add("--typography-base-ol-ol-margin-top", Em(12, 16));
        builder.Add("--typography-base-ol-ol-margin-bottom", Em(12, 16));

        // Definition lists
        builder.Add("--typography-base-dl-margin-top", Em(20, 16));
        builder.Add("--typography-base-dl-margin-bottom", Em(20, 16));
        builder.Add("--typography-base-dt-margin-top", Em(20, 16));
        builder.Add("--typography-base-dd-margin-top", Em(8, 16));
        builder.Add("--typography-base-dd-padding-inline-start", Em(26, 16));

        // Horizontal rule
        builder.Add("--typography-base-hr-margin-top", Em(48, 16));
        builder.Add("--typography-base-hr-margin-bottom", Em(48, 16));

        // Table
        builder.Add("--typography-base-table-font-size", Em(14, 16));
        builder.Add("--typography-base-table-line-height", Round(24.0 / 14.0));
        builder.Add("--typography-base-thead-th-padding-inline-end", Em(8, 14));
        builder.Add("--typography-base-thead-th-padding-bottom", Em(8, 14));
        builder.Add("--typography-base-thead-th-padding-inline-start", Em(8, 14));
        builder.Add("--typography-base-tbody-td-padding-top", Em(8, 14));
        builder.Add("--typography-base-tbody-td-padding-inline-end", Em(8, 14));
        builder.Add("--typography-base-tbody-td-padding-bottom", Em(8, 14));
        builder.Add("--typography-base-tbody-td-padding-inline-start", Em(8, 14));
    }

    private static void AddSmallTypography(ImmutableDictionary<string, string>.Builder builder)
    {
        // sm size modifier values
        builder.Add("--typography-sm-font-size", Rem(14));
        builder.Add("--typography-sm-line-height", Round(24.0 / 14.0));
        builder.Add("--typography-sm-p-margin-top", Em(16, 14));
        builder.Add("--typography-sm-p-margin-bottom", Em(16, 14));
        builder.Add("--typography-sm-lead-font-size", Em(18, 14));
        builder.Add("--typography-sm-lead-line-height", Round(28.0 / 18.0));
        builder.Add("--typography-sm-lead-margin-top", Em(16, 18));
        builder.Add("--typography-sm-lead-margin-bottom", Em(16, 18));
        builder.Add("--typography-sm-blockquote-margin-top", Em(24, 18));
        builder.Add("--typography-sm-blockquote-margin-bottom", Em(24, 18));
        builder.Add("--typography-sm-blockquote-padding-inline-start", Em(20, 18));
        builder.Add("--typography-sm-h1-font-size", Em(30, 14));
        builder.Add("--typography-sm-h1-margin-top", "0");
        builder.Add("--typography-sm-h1-margin-bottom", Em(24, 30));
        builder.Add("--typography-sm-h1-line-height", Round(36.0 / 30.0));
        builder.Add("--typography-sm-h2-font-size", Em(20, 14));
        builder.Add("--typography-sm-h2-margin-top", Em(32, 20));
        builder.Add("--typography-sm-h2-margin-bottom", Em(16, 20));
        builder.Add("--typography-sm-h2-line-height", Round(28.0 / 20.0));
        builder.Add("--typography-sm-h3-font-size", Em(18, 14));
        builder.Add("--typography-sm-h3-margin-top", Em(28, 18));
        builder.Add("--typography-sm-h3-margin-bottom", Em(8, 18));
        builder.Add("--typography-sm-h3-line-height", Round(28.0 / 18.0));
        builder.Add("--typography-sm-h4-margin-top", Em(20, 14));
        builder.Add("--typography-sm-h4-margin-bottom", Em(8, 14));
        builder.Add("--typography-sm-h4-line-height", Round(20.0 / 14.0));
        builder.Add("--typography-sm-img-margin-top", Em(24, 14));
        builder.Add("--typography-sm-img-margin-bottom", Em(24, 14));
        builder.Add("--typography-sm-video-margin-top", Em(24, 14));
        builder.Add("--typography-sm-video-margin-bottom", Em(24, 14));
        builder.Add("--typography-sm-figure-margin-top", Em(24, 14));
        builder.Add("--typography-sm-figure-margin-bottom", Em(24, 14));
        builder.Add("--typography-sm-figcaption-font-size", Em(12, 14));
        builder.Add("--typography-sm-figcaption-line-height", Round(16.0 / 12.0));
        builder.Add("--typography-sm-figcaption-margin-top", Em(8, 12));
        builder.Add("--typography-sm-code-font-size", Em(12, 14));
        builder.Add("--typography-sm-h2-code-font-size", Em(18, 20));
        builder.Add("--typography-sm-h3-code-font-size", Em(16, 18));
        builder.Add("--typography-sm-pre-font-size", Em(12, 14));
        builder.Add("--typography-sm-pre-line-height", Round(20.0 / 12.0));
        builder.Add("--typography-sm-pre-margin-top", Em(20, 12));
        builder.Add("--typography-sm-pre-margin-bottom", Em(20, 12));
        builder.Add("--typography-sm-pre-border-radius", Rem(4));
        builder.Add("--typography-sm-pre-padding-top", Em(8, 12));
        builder.Add("--typography-sm-pre-padding-inline-end", Em(12, 12));
        builder.Add("--typography-sm-pre-padding-bottom", Em(8, 12));
        builder.Add("--typography-sm-pre-padding-inline-start", Em(12, 12));
        builder.Add("--typography-sm-kbd-font-size", Em(12, 14));
        builder.Add("--typography-sm-kbd-border-radius", Rem(5));
        builder.Add("--typography-sm-kbd-padding-top", Em(2, 14));
        builder.Add("--typography-sm-kbd-padding-inline-end", Em(5, 14));
        builder.Add("--typography-sm-kbd-padding-bottom", Em(2, 14));
        builder.Add("--typography-sm-kbd-padding-inline-start", Em(5, 14));
        builder.Add("--typography-sm-table-font-size", Em(12, 14));
        builder.Add("--typography-sm-table-line-height", Round(18.0 / 12.0));
        builder.Add("--typography-sm-thead-th-padding-inline-end", Em(12, 12));
        builder.Add("--typography-sm-thead-th-padding-bottom", Em(8, 12));
        builder.Add("--typography-sm-thead-th-padding-inline-start", Em(12, 12));
        builder.Add("--typography-sm-tbody-td-padding-top", Em(8, 12));
        builder.Add("--typography-sm-tbody-td-padding-inline-end", Em(12, 12));
        builder.Add("--typography-sm-tbody-td-padding-bottom", Em(8, 12));
        builder.Add("--typography-sm-tbody-td-padding-inline-start", Em(12, 12));
        builder.Add("--typography-sm-ol-margin-top", Em(16, 14));
        builder.Add("--typography-sm-ol-margin-bottom", Em(16, 14));
        builder.Add("--typography-sm-ol-padding-inline-start", Em(22, 14));
        builder.Add("--typography-sm-ul-margin-top", Em(16, 14));
        builder.Add("--typography-sm-ul-margin-bottom", Em(16, 14));
        builder.Add("--typography-sm-ul-padding-inline-start", Em(22, 14));
        builder.Add("--typography-sm-li-margin-top", Em(4, 14));
        builder.Add("--typography-sm-li-margin-bottom", Em(4, 14));
        builder.Add("--typography-sm-ol-li-padding-inline-start", Em(6, 14));
        builder.Add("--typography-sm-ul-li-padding-inline-start", Em(6, 14));
        builder.Add("--typography-sm-ul-ul-margin-top", Em(8, 14));
        builder.Add("--typography-sm-ul-ul-margin-bottom", Em(8, 14));
        builder.Add("--typography-sm-ul-ol-margin-top", Em(8, 14));
        builder.Add("--typography-sm-ul-ol-margin-bottom", Em(8, 14));
        builder.Add("--typography-sm-ol-ul-margin-top", Em(8, 14));
        builder.Add("--typography-sm-ol-ul-margin-bottom", Em(8, 14));
        builder.Add("--typography-sm-ol-ol-margin-top", Em(8, 14));
        builder.Add("--typography-sm-ol-ol-margin-bottom", Em(8, 14));
        builder.Add("--typography-sm-dl-margin-top", Em(16, 14));
        builder.Add("--typography-sm-dl-margin-bottom", Em(16, 14));
        builder.Add("--typography-sm-dt-margin-top", Em(16, 14));
        builder.Add("--typography-sm-dd-margin-top", Em(4, 14));
        builder.Add("--typography-sm-dd-padding-inline-start", Em(22, 14));
        builder.Add("--typography-sm-hr-margin-top", Em(40, 14));
        builder.Add("--typography-sm-hr-margin-bottom", Em(40, 14));
    }

    private static void AddLargeTypography(ImmutableDictionary<string, string>.Builder builder)
    {
        // lg size modifier values
        builder.Add("--typography-lg-font-size", Rem(18));
        builder.Add("--typography-lg-line-height", Round(32.0 / 18.0));
        builder.Add("--typography-lg-p-margin-top", Em(24, 18));
        builder.Add("--typography-lg-p-margin-bottom", Em(24, 18));
        builder.Add("--typography-lg-lead-font-size", Em(22, 18));
        builder.Add("--typography-lg-lead-line-height", Round(32.0 / 22.0));
        builder.Add("--typography-lg-lead-margin-top", Em(24, 22));
        builder.Add("--typography-lg-lead-margin-bottom", Em(24, 22));
        builder.Add("--typography-lg-blockquote-margin-top", Em(40, 24));
        builder.Add("--typography-lg-blockquote-margin-bottom", Em(40, 24));
        builder.Add("--typography-lg-blockquote-padding-inline-start", Em(24, 24));
        builder.Add("--typography-lg-h1-font-size", Em(48, 18));
        builder.Add("--typography-lg-h1-margin-top", "0");
        builder.Add("--typography-lg-h1-margin-bottom", Em(40, 48));
        builder.Add("--typography-lg-h1-line-height", Round(48.0 / 48.0));
        builder.Add("--typography-lg-h2-font-size", Em(30, 18));
        builder.Add("--typography-lg-h2-margin-top", Em(56, 30));
        builder.Add("--typography-lg-h2-margin-bottom", Em(32, 30));
        builder.Add("--typography-lg-h2-line-height", Round(40.0 / 30.0));
        builder.Add("--typography-lg-h3-font-size", Em(24, 18));
        builder.Add("--typography-lg-h3-margin-top", Em(40, 24));
        builder.Add("--typography-lg-h3-margin-bottom", Em(16, 24));
        builder.Add("--typography-lg-h3-line-height", Round(36.0 / 24.0));
        builder.Add("--typography-lg-h4-margin-top", Em(32, 18));
        builder.Add("--typography-lg-h4-margin-bottom", Em(8, 18));
        builder.Add("--typography-lg-h4-line-height", Round(28.0 / 18.0));
        builder.Add("--typography-lg-img-margin-top", Em(32, 18));
        builder.Add("--typography-lg-img-margin-bottom", Em(32, 18));
        builder.Add("--typography-lg-video-margin-top", Em(32, 18));
        builder.Add("--typography-lg-video-margin-bottom", Em(32, 18));
        builder.Add("--typography-lg-figure-margin-top", Em(32, 18));
        builder.Add("--typography-lg-figure-margin-bottom", Em(32, 18));
        builder.Add("--typography-lg-figcaption-font-size", Em(16, 18));
        builder.Add("--typography-lg-figcaption-line-height", Round(24.0 / 16.0));
        builder.Add("--typography-lg-figcaption-margin-top", Em(16, 16));
        builder.Add("--typography-lg-code-font-size", Em(16, 18));
        builder.Add("--typography-lg-h2-code-font-size", Em(26, 30));
        builder.Add("--typography-lg-h3-code-font-size", Em(21, 24));
        builder.Add("--typography-lg-pre-font-size", Em(16, 18));
        builder.Add("--typography-lg-pre-line-height", Round(28.0 / 16.0));
        builder.Add("--typography-lg-pre-margin-top", Em(32, 16));
        builder.Add("--typography-lg-pre-margin-bottom", Em(32, 16));
        builder.Add("--typography-lg-pre-border-radius", Rem(6));
        builder.Add("--typography-lg-pre-padding-top", Em(16, 16));
        builder.Add("--typography-lg-pre-padding-inline-end", Em(24, 16));
        builder.Add("--typography-lg-pre-padding-bottom", Em(16, 16));
        builder.Add("--typography-lg-pre-padding-inline-start", Em(24, 16));
        builder.Add("--typography-lg-kbd-font-size", Em(16, 18));
        builder.Add("--typography-lg-kbd-border-radius", Rem(5));
        builder.Add("--typography-lg-kbd-padding-top", Em(4, 18));
        builder.Add("--typography-lg-kbd-padding-inline-end", Em(8, 18));
        builder.Add("--typography-lg-kbd-padding-bottom", Em(4, 18));
        builder.Add("--typography-lg-kbd-padding-inline-start", Em(8, 18));
        builder.Add("--typography-lg-table-font-size", Em(16, 18));
        builder.Add("--typography-lg-table-line-height", Round(24.0 / 16.0));
        builder.Add("--typography-lg-thead-th-padding-inline-end", Em(12, 16));
        builder.Add("--typography-lg-thead-th-padding-bottom", Em(12, 16));
        builder.Add("--typography-lg-thead-th-padding-inline-start", Em(12, 16));
        builder.Add("--typography-lg-tbody-td-padding-top", Em(12, 16));
        builder.Add("--typography-lg-tbody-td-padding-inline-end", Em(12, 16));
        builder.Add("--typography-lg-tbody-td-padding-bottom", Em(12, 16));
        builder.Add("--typography-lg-tbody-td-padding-inline-start", Em(12, 16));
        builder.Add("--typography-lg-ol-margin-top", Em(24, 18));
        builder.Add("--typography-lg-ol-margin-bottom", Em(24, 18));
        builder.Add("--typography-lg-ol-padding-inline-start", Em(28, 18));
        builder.Add("--typography-lg-ul-margin-top", Em(24, 18));
        builder.Add("--typography-lg-ul-margin-bottom", Em(24, 18));
        builder.Add("--typography-lg-ul-padding-inline-start", Em(28, 18));
        builder.Add("--typography-lg-li-margin-top", Em(12, 18));
        builder.Add("--typography-lg-li-margin-bottom", Em(12, 18));
        builder.Add("--typography-lg-ol-li-padding-inline-start", Em(8, 18));
        builder.Add("--typography-lg-ul-li-padding-inline-start", Em(8, 18));
        builder.Add("--typography-lg-ul-ul-margin-top", Em(12, 18));
        builder.Add("--typography-lg-ul-ul-margin-bottom", Em(12, 18));
        builder.Add("--typography-lg-ul-ol-margin-top", Em(12, 18));
        builder.Add("--typography-lg-ul-ol-margin-bottom", Em(12, 18));
        builder.Add("--typography-lg-ol-ul-margin-top", Em(12, 18));
        builder.Add("--typography-lg-ol-ul-margin-bottom", Em(12, 18));
        builder.Add("--typography-lg-ol-ol-margin-top", Em(12, 18));
        builder.Add("--typography-lg-ol-ol-margin-bottom", Em(12, 18));
        builder.Add("--typography-lg-dl-margin-top", Em(24, 18));
        builder.Add("--typography-lg-dl-margin-bottom", Em(24, 18));
        builder.Add("--typography-lg-dt-margin-top", Em(24, 18));
        builder.Add("--typography-lg-dd-margin-top", Em(12, 18));
        builder.Add("--typography-lg-dd-padding-inline-start", Em(28, 18));
        builder.Add("--typography-lg-hr-margin-top", Em(56, 18));
        builder.Add("--typography-lg-hr-margin-bottom", Em(56, 18));
    }

    private static void AddXLargeTypography(ImmutableDictionary<string, string>.Builder builder)
    {
        // xl size modifier values
        builder.Add("--typography-xl-font-size", Rem(20));
        builder.Add("--typography-xl-line-height", Round(36.0 / 20.0));
        builder.Add("--typography-xl-p-margin-top", Em(24, 20));
        builder.Add("--typography-xl-p-margin-bottom", Em(24, 20));
        builder.Add("--typography-xl-lead-font-size", Em(24, 20));
        builder.Add("--typography-xl-lead-line-height", Round(36.0 / 24.0));
        builder.Add("--typography-xl-lead-margin-top", Em(24, 24));
        builder.Add("--typography-xl-lead-margin-bottom", Em(24, 24));
        builder.Add("--typography-xl-blockquote-margin-top", Em(48, 30));
        builder.Add("--typography-xl-blockquote-margin-bottom", Em(48, 30));
        builder.Add("--typography-xl-blockquote-padding-inline-start", Em(32, 30));
        builder.Add("--typography-xl-h1-font-size", Em(56, 20));
        builder.Add("--typography-xl-h1-margin-top", "0");
        builder.Add("--typography-xl-h1-margin-bottom", Em(48, 56));
        builder.Add("--typography-xl-h1-line-height", Round(56.0 / 56.0));
        builder.Add("--typography-xl-h2-font-size", Em(36, 20));
        builder.Add("--typography-xl-h2-margin-top", Em(56, 36));
        builder.Add("--typography-xl-h2-margin-bottom", Em(32, 36));
        builder.Add("--typography-xl-h2-line-height", Round(40.0 / 36.0));
        builder.Add("--typography-xl-h3-font-size", Em(30, 20));
        builder.Add("--typography-xl-h3-margin-top", Em(48, 30));
        builder.Add("--typography-xl-h3-margin-bottom", Em(20, 30));
        builder.Add("--typography-xl-h3-line-height", Round(40.0 / 30.0));
        builder.Add("--typography-xl-h4-margin-top", Em(36, 20));
        builder.Add("--typography-xl-h4-margin-bottom", Em(12, 20));
        builder.Add("--typography-xl-h4-line-height", Round(32.0 / 20.0));
        builder.Add("--typography-xl-img-margin-top", Em(40, 20));
        builder.Add("--typography-xl-img-margin-bottom", Em(40, 20));
        builder.Add("--typography-xl-video-margin-top", Em(40, 20));
        builder.Add("--typography-xl-video-margin-bottom", Em(40, 20));
        builder.Add("--typography-xl-figure-margin-top", Em(40, 20));
        builder.Add("--typography-xl-figure-margin-bottom", Em(40, 20));
        builder.Add("--typography-xl-figcaption-font-size", Em(18, 20));
        builder.Add("--typography-xl-figcaption-line-height", Round(28.0 / 18.0));
        builder.Add("--typography-xl-figcaption-margin-top", Em(18, 18));
        builder.Add("--typography-xl-code-font-size", Em(18, 20));
        builder.Add("--typography-xl-h2-code-font-size", Em(31, 36));
        builder.Add("--typography-xl-h3-code-font-size", Em(27, 30));
        builder.Add("--typography-xl-pre-font-size", Em(18, 20));
        builder.Add("--typography-xl-pre-line-height", Round(32.0 / 18.0));
        builder.Add("--typography-xl-pre-margin-top", Em(36, 18));
        builder.Add("--typography-xl-pre-margin-bottom", Em(36, 18));
        builder.Add("--typography-xl-pre-border-radius", Rem(6));
        builder.Add("--typography-xl-pre-padding-top", Em(20, 18));
        builder.Add("--typography-xl-pre-padding-inline-end", Em(24, 18));
        builder.Add("--typography-xl-pre-padding-bottom", Em(20, 18));
        builder.Add("--typography-xl-pre-padding-inline-start", Em(24, 18));
        builder.Add("--typography-xl-kbd-font-size", Em(18, 20));
        builder.Add("--typography-xl-kbd-border-radius", Rem(5));
        builder.Add("--typography-xl-kbd-padding-top", Em(5, 20));
        builder.Add("--typography-xl-kbd-padding-inline-end", Em(8, 20));
        builder.Add("--typography-xl-kbd-padding-bottom", Em(5, 20));
        builder.Add("--typography-xl-kbd-padding-inline-start", Em(8, 20));
        builder.Add("--typography-xl-table-font-size", Em(18, 20));
        builder.Add("--typography-xl-table-line-height", Round(28.0 / 18.0));
        builder.Add("--typography-xl-thead-th-padding-inline-end", Em(12, 18));
        builder.Add("--typography-xl-thead-th-padding-bottom", Em(16, 18));
        builder.Add("--typography-xl-thead-th-padding-inline-start", Em(12, 18));
        builder.Add("--typography-xl-tbody-td-padding-top", Em(16, 18));
        builder.Add("--typography-xl-tbody-td-padding-inline-end", Em(12, 18));
        builder.Add("--typography-xl-tbody-td-padding-bottom", Em(16, 18));
        builder.Add("--typography-xl-tbody-td-padding-inline-start", Em(12, 18));
        builder.Add("--typography-xl-ol-margin-top", Em(24, 20));
        builder.Add("--typography-xl-ol-margin-bottom", Em(24, 20));
        builder.Add("--typography-xl-ol-padding-inline-start", Em(32, 20));
        builder.Add("--typography-xl-ul-margin-top", Em(24, 20));
        builder.Add("--typography-xl-ul-margin-bottom", Em(24, 20));
        builder.Add("--typography-xl-ul-padding-inline-start", Em(32, 20));
        builder.Add("--typography-xl-li-margin-top", Em(12, 20));
        builder.Add("--typography-xl-li-margin-bottom", Em(12, 20));
        builder.Add("--typography-xl-ol-li-padding-inline-start", Em(8, 20));
        builder.Add("--typography-xl-ul-li-padding-inline-start", Em(8, 20));
        builder.Add("--typography-xl-ul-ul-margin-top", Em(12, 20));
        builder.Add("--typography-xl-ul-ul-margin-bottom", Em(12, 20));
        builder.Add("--typography-xl-ul-ol-margin-top", Em(12, 20));
        builder.Add("--typography-xl-ul-ol-margin-bottom", Em(12, 20));
        builder.Add("--typography-xl-ol-ul-margin-top", Em(12, 20));
        builder.Add("--typography-xl-ol-ul-margin-bottom", Em(12, 20));
        builder.Add("--typography-xl-ol-ol-margin-top", Em(12, 20));
        builder.Add("--typography-xl-ol-ol-margin-bottom", Em(12, 20));
        builder.Add("--typography-xl-dl-margin-top", Em(24, 20));
        builder.Add("--typography-xl-dl-margin-bottom", Em(24, 20));
        builder.Add("--typography-xl-dt-margin-top", Em(24, 20));
        builder.Add("--typography-xl-dd-margin-top", Em(12, 20));
        builder.Add("--typography-xl-dd-padding-inline-start", Em(32, 20));
        builder.Add("--typography-xl-hr-margin-top", Em(56, 20));
        builder.Add("--typography-xl-hr-margin-bottom", Em(56, 20));
    }

    private static void Add2XLargeTypography(ImmutableDictionary<string, string>.Builder builder)
    {
        // 2xl size modifier values
        builder.Add("--typography-2xl-font-size", Rem(24));
        builder.Add("--typography-2xl-line-height", Round(40.0 / 24.0));
        builder.Add("--typography-2xl-p-margin-top", Em(32, 24));
        builder.Add("--typography-2xl-p-margin-bottom", Em(32, 24));
        builder.Add("--typography-2xl-lead-font-size", Em(30, 24));
        builder.Add("--typography-2xl-lead-line-height", Round(44.0 / 30.0));
        builder.Add("--typography-2xl-lead-margin-top", Em(32, 30));
        builder.Add("--typography-2xl-lead-margin-bottom", Em(32, 30));
        builder.Add("--typography-2xl-blockquote-margin-top", Em(64, 36));
        builder.Add("--typography-2xl-blockquote-margin-bottom", Em(64, 36));
        builder.Add("--typography-2xl-blockquote-padding-inline-start", Em(40, 36));
        builder.Add("--typography-2xl-h1-font-size", Em(64, 24));
        builder.Add("--typography-2xl-h1-margin-top", "0");
        builder.Add("--typography-2xl-h1-margin-bottom", Em(56, 64));
        builder.Add("--typography-2xl-h1-line-height", Round(64.0 / 64.0));
        builder.Add("--typography-2xl-h2-font-size", Em(48, 24));
        builder.Add("--typography-2xl-h2-margin-top", Em(72, 48));
        builder.Add("--typography-2xl-h2-margin-bottom", Em(40, 48));
        builder.Add("--typography-2xl-h2-line-height", Round(52.0 / 48.0));
        builder.Add("--typography-2xl-h3-font-size", Em(36, 24));
        builder.Add("--typography-2xl-h3-margin-top", Em(56, 36));
        builder.Add("--typography-2xl-h3-margin-bottom", Em(24, 36));
        builder.Add("--typography-2xl-h3-line-height", Round(44.0 / 36.0));
        builder.Add("--typography-2xl-h4-margin-top", Em(40, 24));
        builder.Add("--typography-2xl-h4-margin-bottom", Em(16, 24));
        builder.Add("--typography-2xl-h4-line-height", Round(36.0 / 24.0));
        builder.Add("--typography-2xl-img-margin-top", Em(48, 24));
        builder.Add("--typography-2xl-img-margin-bottom", Em(48, 24));
        builder.Add("--typography-2xl-video-margin-top", Em(48, 24));
        builder.Add("--typography-2xl-video-margin-bottom", Em(48, 24));
        builder.Add("--typography-2xl-figure-margin-top", Em(48, 24));
        builder.Add("--typography-2xl-figure-margin-bottom", Em(48, 24));
        builder.Add("--typography-2xl-figcaption-font-size", Em(20, 24));
        builder.Add("--typography-2xl-figcaption-line-height", Round(32.0 / 20.0));
        builder.Add("--typography-2xl-figcaption-margin-top", Em(20, 20));
        builder.Add("--typography-2xl-code-font-size", Em(20, 24));
        builder.Add("--typography-2xl-h2-code-font-size", Em(42, 48));
        builder.Add("--typography-2xl-h3-code-font-size", Em(32, 36));
        builder.Add("--typography-2xl-pre-font-size", Em(20, 24));
        builder.Add("--typography-2xl-pre-line-height", Round(36.0 / 20.0));
        builder.Add("--typography-2xl-pre-margin-top", Em(40, 20));
        builder.Add("--typography-2xl-pre-margin-bottom", Em(40, 20));
        builder.Add("--typography-2xl-pre-border-radius", Rem(8));
        builder.Add("--typography-2xl-pre-padding-top", Em(24, 20));
        builder.Add("--typography-2xl-pre-padding-inline-end", Em(32, 20));
        builder.Add("--typography-2xl-pre-padding-bottom", Em(24, 20));
        builder.Add("--typography-2xl-pre-padding-inline-start", Em(32, 20));
        builder.Add("--typography-2xl-kbd-font-size", Em(20, 24));
        builder.Add("--typography-2xl-kbd-border-radius", Rem(6));
        builder.Add("--typography-2xl-kbd-padding-top", Em(6, 24));
        builder.Add("--typography-2xl-kbd-padding-inline-end", Em(8, 24));
        builder.Add("--typography-2xl-kbd-padding-bottom", Em(6, 24));
        builder.Add("--typography-2xl-kbd-padding-inline-start", Em(8, 24));
        builder.Add("--typography-2xl-table-font-size", Em(20, 24));
        builder.Add("--typography-2xl-table-line-height", Round(28.0 / 20.0));
        builder.Add("--typography-2xl-thead-th-padding-inline-end", Em(12, 20));
        builder.Add("--typography-2xl-thead-th-padding-bottom", Em(16, 20));
        builder.Add("--typography-2xl-thead-th-padding-inline-start", Em(12, 20));
        builder.Add("--typography-2xl-tbody-td-padding-top", Em(16, 20));
        builder.Add("--typography-2xl-tbody-td-padding-inline-end", Em(12, 20));
        builder.Add("--typography-2xl-tbody-td-padding-bottom", Em(16, 20));
        builder.Add("--typography-2xl-tbody-td-padding-inline-start", Em(12, 20));
        builder.Add("--typography-2xl-ol-margin-top", Em(32, 24));
        builder.Add("--typography-2xl-ol-margin-bottom", Em(32, 24));
        builder.Add("--typography-2xl-ol-padding-inline-start", Em(38, 24));
        builder.Add("--typography-2xl-ul-margin-top", Em(32, 24));
        builder.Add("--typography-2xl-ul-margin-bottom", Em(32, 24));
        builder.Add("--typography-2xl-ul-padding-inline-start", Em(38, 24));
        builder.Add("--typography-2xl-li-margin-top", Em(12, 24));
        builder.Add("--typography-2xl-li-margin-bottom", Em(12, 24));
        builder.Add("--typography-2xl-ol-li-padding-inline-start", Em(10, 24));
        builder.Add("--typography-2xl-ul-li-padding-inline-start", Em(10, 24));
        builder.Add("--typography-2xl-ul-ul-margin-top", Em(16, 24));
        builder.Add("--typography-2xl-ul-ul-margin-bottom", Em(16, 24));
        builder.Add("--typography-2xl-ul-ol-margin-top", Em(16, 24));
        builder.Add("--typography-2xl-ul-ol-margin-bottom", Em(16, 24));
        builder.Add("--typography-2xl-ol-ul-margin-top", Em(16, 24));
        builder.Add("--typography-2xl-ol-ul-margin-bottom", Em(16, 24));
        builder.Add("--typography-2xl-ol-ol-margin-top", Em(16, 24));
        builder.Add("--typography-2xl-ol-ol-margin-bottom", Em(16, 24));
        builder.Add("--typography-2xl-dl-margin-top", Em(32, 24));
        builder.Add("--typography-2xl-dl-margin-bottom", Em(32, 24));
        builder.Add("--typography-2xl-dt-margin-top", Em(32, 24));
        builder.Add("--typography-2xl-dd-margin-top", Em(12, 24));
        builder.Add("--typography-2xl-dd-padding-inline-start", Em(38, 24));
        builder.Add("--typography-2xl-hr-margin-top", Em(72, 24));
        builder.Add("--typography-2xl-hr-margin-bottom", Em(72, 24));
    }

    private static void AddColorThemes(ImmutableDictionary<string, string>.Builder builder)
    {
        // Default gray theme CSS variables (used when no color modifier specified)
        builder.Add("--typography-color-body", "var(--color-gray-700)");
        builder.Add("--typography-color-headings", "var(--color-gray-900)");
        builder.Add("--typography-color-lead", "var(--color-gray-600)");
        builder.Add("--typography-color-links", "var(--color-gray-900)");
        builder.Add("--typography-color-bold", "var(--color-gray-900)");
        builder.Add("--typography-color-counters", "var(--color-gray-500)");
        builder.Add("--typography-color-bullets", "var(--color-gray-300)");
        builder.Add("--typography-color-hr", "var(--color-gray-200)");
        builder.Add("--typography-color-quotes", "var(--color-gray-900)");
        builder.Add("--typography-color-quote-borders", "var(--color-gray-200)");
        builder.Add("--typography-color-captions", "var(--color-gray-500)");
        builder.Add("--typography-color-kbd", "var(--color-gray-900)");
        builder.Add("--typography-color-kbd-shadows", HexToRgb("#6b7280")); // gray-500
        builder.Add("--typography-color-code", "var(--color-gray-900)");
        builder.Add("--typography-color-pre-code", "var(--color-gray-200)");
        builder.Add("--typography-color-pre-bg", "var(--color-gray-800)");
        builder.Add("--typography-color-th-borders", "var(--color-gray-300)");
        builder.Add("--typography-color-td-borders", "var(--color-gray-200)");

        // Invert colors for dark mode
        builder.Add("--typography-color-invert-body", "var(--color-gray-300)");
        builder.Add("--typography-color-invert-headings", "var(--color-white)");
        builder.Add("--typography-color-invert-lead", "var(--color-gray-400)");
        builder.Add("--typography-color-invert-links", "var(--color-white)");
        builder.Add("--typography-color-invert-bold", "var(--color-white)");
        builder.Add("--typography-color-invert-counters", "var(--color-gray-400)");
        builder.Add("--typography-color-invert-bullets", "var(--color-gray-600)");
        builder.Add("--typography-color-invert-hr", "var(--color-gray-700)");
        builder.Add("--typography-color-invert-quotes", "var(--color-gray-100)");
        builder.Add("--typography-color-invert-quote-borders", "var(--color-gray-700)");
        builder.Add("--typography-color-invert-captions", "var(--color-gray-400)");
        builder.Add("--typography-color-invert-kbd", "var(--color-white)");
        builder.Add("--typography-color-invert-kbd-shadows", HexToRgb("#ffffff"));
        builder.Add("--typography-color-invert-code", "var(--color-white)");
        builder.Add("--typography-color-invert-pre-code", "var(--color-gray-300)");
        builder.Add("--typography-color-invert-pre-bg", "rgb(0 0 0 / 50%)");
        builder.Add("--typography-color-invert-th-borders", "var(--color-gray-600)");
        builder.Add("--typography-color-invert-td-borders", "var(--color-gray-700)");

        // Slate theme
        builder.Add("--typography-color-slate-body", "var(--color-slate-700)");
        builder.Add("--typography-color-slate-headings", "var(--color-slate-900)");
        builder.Add("--typography-color-slate-lead", "var(--color-slate-600)");
        builder.Add("--typography-color-slate-links", "var(--color-slate-900)");
        builder.Add("--typography-color-slate-bold", "var(--color-slate-900)");
        builder.Add("--typography-color-slate-counters", "var(--color-slate-500)");
        builder.Add("--typography-color-slate-bullets", "var(--color-slate-300)");
        builder.Add("--typography-color-slate-hr", "var(--color-slate-200)");
        builder.Add("--typography-color-slate-quotes", "var(--color-slate-900)");
        builder.Add("--typography-color-slate-quote-borders", "var(--color-slate-200)");
        builder.Add("--typography-color-slate-captions", "var(--color-slate-500)");
        builder.Add("--typography-color-slate-kbd", "var(--color-slate-900)");
        builder.Add("--typography-color-slate-kbd-shadows", HexToRgb("#64748b")); // slate-500
        builder.Add("--typography-color-slate-code", "var(--color-slate-900)");
        builder.Add("--typography-color-slate-pre-code", "var(--color-slate-200)");
        builder.Add("--typography-color-slate-pre-bg", "var(--color-slate-800)");
        builder.Add("--typography-color-slate-th-borders", "var(--color-slate-300)");
        builder.Add("--typography-color-slate-td-borders", "var(--color-slate-200)");
        builder.Add("--typography-color-slate-invert-body", "var(--color-slate-300)");
        builder.Add("--typography-color-slate-invert-headings", "var(--color-white)");
        builder.Add("--typography-color-slate-invert-lead", "var(--color-slate-400)");
        builder.Add("--typography-color-slate-invert-links", "var(--color-white)");
        builder.Add("--typography-color-slate-invert-bold", "var(--color-white)");
        builder.Add("--typography-color-slate-invert-counters", "var(--color-slate-400)");
        builder.Add("--typography-color-slate-invert-bullets", "var(--color-slate-600)");
        builder.Add("--typography-color-slate-invert-hr", "var(--color-slate-700)");
        builder.Add("--typography-color-slate-invert-quotes", "var(--color-slate-100)");
        builder.Add("--typography-color-slate-invert-quote-borders", "var(--color-slate-700)");
        builder.Add("--typography-color-slate-invert-captions", "var(--color-slate-400)");
        builder.Add("--typography-color-slate-invert-kbd", "var(--color-white)");
        builder.Add("--typography-color-slate-invert-kbd-shadows", HexToRgb("#ffffff"));
        builder.Add("--typography-color-slate-invert-code", "var(--color-white)");
        builder.Add("--typography-color-slate-invert-pre-code", "var(--color-slate-300)");
        builder.Add("--typography-color-slate-invert-pre-bg", "rgb(0 0 0 / 50%)");
        builder.Add("--typography-color-slate-invert-th-borders", "var(--color-slate-600)");
        builder.Add("--typography-color-slate-invert-td-borders", "var(--color-slate-700)");

        // Similar patterns for zinc, neutral, stone - adding these for completeness
        var fullColorThemes = new[] { "zinc", "neutral", "stone" };
        foreach (var theme in fullColorThemes)
        {
            builder.Add($"--typography-color-{theme}-body", $"var(--color-{theme}-700)");
            builder.Add($"--typography-color-{theme}-headings", $"var(--color-{theme}-900)");
            builder.Add($"--typography-color-{theme}-lead", $"var(--color-{theme}-600)");
            builder.Add($"--typography-color-{theme}-links", $"var(--color-{theme}-900)");
            builder.Add($"--typography-color-{theme}-bold", $"var(--color-{theme}-900)");
            builder.Add($"--typography-color-{theme}-counters", $"var(--color-{theme}-500)");
            builder.Add($"--typography-color-{theme}-bullets", $"var(--color-{theme}-300)");
            builder.Add($"--typography-color-{theme}-hr", $"var(--color-{theme}-200)");
            builder.Add($"--typography-color-{theme}-quotes", $"var(--color-{theme}-900)");
            builder.Add($"--typography-color-{theme}-quote-borders", $"var(--color-{theme}-200)");
            builder.Add($"--typography-color-{theme}-captions", $"var(--color-{theme}-500)");
            builder.Add($"--typography-color-{theme}-kbd", $"var(--color-{theme}-900)");
            builder.Add($"--typography-color-{theme}-kbd-shadows", theme == "zinc" ? HexToRgb("#71717a") : theme == "neutral" ? HexToRgb("#737373") : HexToRgb("#78716c"));
            builder.Add($"--typography-color-{theme}-code", $"var(--color-{theme}-900)");
            builder.Add($"--typography-color-{theme}-pre-code", $"var(--color-{theme}-200)");
            builder.Add($"--typography-color-{theme}-pre-bg", $"var(--color-{theme}-800)");
            builder.Add($"--typography-color-{theme}-th-borders", $"var(--color-{theme}-300)");
            builder.Add($"--typography-color-{theme}-td-borders", $"var(--color-{theme}-200)");
            builder.Add($"--typography-color-{theme}-invert-body", $"var(--color-{theme}-300)");
            builder.Add($"--typography-color-{theme}-invert-headings", "var(--color-white)");
            builder.Add($"--typography-color-{theme}-invert-lead", $"var(--color-{theme}-400)");
            builder.Add($"--typography-color-{theme}-invert-links", "var(--color-white)");
            builder.Add($"--typography-color-{theme}-invert-bold", "var(--color-white)");
            builder.Add($"--typography-color-{theme}-invert-counters", $"var(--color-{theme}-400)");
            builder.Add($"--typography-color-{theme}-invert-bullets", $"var(--color-{theme}-600)");
            builder.Add($"--typography-color-{theme}-invert-hr", $"var(--color-{theme}-700)");
            builder.Add($"--typography-color-{theme}-invert-quotes", $"var(--color-{theme}-100)");
            builder.Add($"--typography-color-{theme}-invert-quote-borders", $"var(--color-{theme}-700)");
            builder.Add($"--typography-color-{theme}-invert-captions", $"var(--color-{theme}-400)");
            builder.Add($"--typography-color-{theme}-invert-kbd", "var(--color-white)");
            builder.Add($"--typography-color-{theme}-invert-kbd-shadows", HexToRgb("#ffffff"));
            builder.Add($"--typography-color-{theme}-invert-code", "var(--color-white)");
            builder.Add($"--typography-color-{theme}-invert-pre-code", $"var(--color-{theme}-300)");
            builder.Add($"--typography-color-{theme}-invert-pre-bg", "rgb(0 0 0 / 50%)");
            builder.Add($"--typography-color-{theme}-invert-th-borders", $"var(--color-{theme}-600)");
            builder.Add($"--typography-color-{theme}-invert-td-borders", $"var(--color-{theme}-700)");
        }

        // Link-only color themes (red, orange, amber, yellow, lime, green, emerald, teal, cyan, sky, blue, indigo, violet, purple, fuchsia, pink, rose)
        var linkOnlyColors = new[]
        {
            "red", "orange", "amber", "yellow", "lime", "green", "emerald",
            "teal", "cyan", "sky", "blue", "indigo", "violet", "purple",
            "fuchsia", "pink", "rose",
        };

        foreach (var color in linkOnlyColors)
        {
            builder.Add($"--typography-color-{color}-links", $"var(--color-{color}-600)");
            builder.Add($"--typography-color-{color}-invert-links", $"var(--color-{color}-500)");
        }
    }
}