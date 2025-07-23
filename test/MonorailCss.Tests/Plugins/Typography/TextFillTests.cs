using Shouldly;

namespace MonorailCss.Tests.Plugins.Typography;

public class TextFillTests
{
    [Fact]
    public void Can_generate_text_fill_color()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-fill-blue-500"]);
        result.ShouldBeCss("""

                           .text-fill-blue-500 {
                             -webkit-text-fill-color:oklch(0.623 0.214 259.815);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_fill_transparent()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-fill-transparent"]);
        result.ShouldBeCss("""

                           .text-fill-transparent {
                             -webkit-text-fill-color:transparent;
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_fill_white()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-fill-white"]);
        result.ShouldBeCss("""

                           .text-fill-white {
                             -webkit-text-fill-color:rgb(255, 255, 255);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_fill_with_opacity()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-fill-red-600/50"]);
        result.ShouldBeCss("""

                           .text-fill-red-600\/50 {
                             -webkit-text-fill-color:oklch(0.577 0.245 27.325 / 0.5);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_multiple_text_fill_colors()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-fill-red-500", "text-fill-green-500", "text-fill-blue-500"]);
        result.ShouldContain("text-fill-red-500");
        result.ShouldContain("text-fill-green-500");
        result.ShouldContain("text-fill-blue-500");
        result.ShouldContain("-webkit-text-fill-color");
    }
}