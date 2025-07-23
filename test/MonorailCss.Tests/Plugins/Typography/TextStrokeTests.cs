using Shouldly;

namespace MonorailCss.Tests.Plugins.Typography;

public class TextStrokeTests
{
    [Fact]
    public void Can_generate_text_stroke_width()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-stroke-1"]);
        result.ShouldBeCss("""

                           .text-stroke-1 {
                             -webkit-text-stroke-width:1px;
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_stroke_zero_width()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-stroke-0"]);
        result.ShouldBeCss("""

                           .text-stroke-0 {
                             -webkit-text-stroke-width:0;
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_stroke_thick()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-stroke-thick"]);
        result.ShouldBeCss("""

                           .text-stroke-thick {
                             -webkit-text-stroke-width:thick;
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_stroke_color()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-stroke-red-500"]);
        result.ShouldBeCss("""

                           .text-stroke-red-500 {
                             -webkit-text-stroke-color:oklch(0.637 0.237 25.331);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_text_stroke_black()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-stroke-black"]);
        result.ShouldBeCss("""

                           .text-stroke-black {
                             -webkit-text-stroke-color:rgb(0, 0, 0);
                           }

                           """);
    }

    [Fact]
    public void Can_generate_multiple_text_stroke_widths()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var result = framework.Process(["text-stroke-1", "text-stroke-2", "text-stroke-4"]);
        result.ShouldContain("text-stroke-1");
        result.ShouldContain("text-stroke-2");
        result.ShouldContain("text-stroke-4");
        result.ShouldContain("-webkit-text-stroke-width:1px");
        result.ShouldContain("-webkit-text-stroke-width:2px");
        result.ShouldContain("-webkit-text-stroke-width:4px");
    }
}