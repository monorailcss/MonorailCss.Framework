using MonorailCss;
using MonorailCss.Plugins.Typography;
using Shouldly;
using Xunit;

namespace MonorailCss.Tests.Plugins;

public class FontTests
{
    [Fact]
    public void FontSans_outputs_font_family_only()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty
        });
        var result = framework.Process(["font-sans"]);
        result.ShouldBeCss(@".font-sans {
  font-family:-apple-system, BlinkMacSystemFont, avenir next, avenir, segoe ui, helvetica neue, helvetica, Ubuntu, roboto, noto, arial, sans-serif;
}");
    }

    [Fact]
    public void FontMono_outputs_font_family_only()
    {
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty
        });
        var result = framework.Process(["font-mono"]);
        result.ShouldBeCss(@".font-mono {
  font-family:Cascadia Code, Menlo, Consolas, Monaco, Liberation Mono, Lucida Console, monospace;
}");
    }

    [Fact]
    public void Font_with_feature_and_variation_settings_outputs_all_properties()
    {
        var designSystem = MonorailCss.DesignSystem.Default with
        {
            FontFamilies = MonorailCss.DesignSystem.Default.FontFamilies.Add("display",
                new FontFamilyDefinition(
                    "Oswald, sans-serif",
                    "\"cv02\", \"cv03\", \"cv04\", \"cv11\"",
                    "\"opsz\" 32"
                ))
        };
        var framework = new CssFramework(new CssFrameworkSettings
        {
            CssResetOverride = string.Empty, DesignSystem = designSystem
        });
        var result = framework.Process(["font-display"]);
        result.ShouldBeCss("""
                           :root {
                             --monorail-spacing:0.25rem;
                           }
                           .font-display {
                             font-family:Oswald, sans-serif;
                             font-feature-settings:"cv02", "cv03", "cv04", "cv11";
                             font-variation-settings:"opsz" 32;
                           }
                           """);
    }
}