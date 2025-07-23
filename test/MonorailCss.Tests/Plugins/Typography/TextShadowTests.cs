using Shouldly;

namespace MonorailCss.Tests.Plugins.Typography;

public class TextShadowTests
{
    [Fact]
    public void Can_generate_text_shadow_sm()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-shadow-sm"]);
        result.ShouldBeCss("""

                           .text-shadow-sm {
                             text-shadow:0 1px 2px rgb(0 0 0 / 0.05);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_shadow_default()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-shadow"]);
        result.ShouldBeCss("""

                           .text-shadow {
                             text-shadow:0 1px 3px rgb(0 0 0 / 0.1), 0 1px 2px rgb(0 0 0 / 0.1);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_shadow_lg()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-shadow-lg"]);
        result.ShouldBeCss("""

                           .text-shadow-lg {
                             text-shadow:0 10px 15px rgb(0 0 0 / 0.1), 0 4px 6px rgb(0 0 0 / 0.1);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_shadow_none()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-shadow-none"]);
        result.ShouldBeCss("""

                           .text-shadow-none {
                             text-shadow:none;
                           }

                           """);
    }

    [Fact]
    public void Can_generate_multiple_text_shadows()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-shadow-sm", "text-shadow-md", "text-shadow-xl", "text-shadow-2xl"]);
        result.ShouldContain("text-shadow-sm");
        result.ShouldContain("text-shadow-md");
        result.ShouldContain("text-shadow-xl");
        result.ShouldContain("text-shadow-2xl");
    }
}