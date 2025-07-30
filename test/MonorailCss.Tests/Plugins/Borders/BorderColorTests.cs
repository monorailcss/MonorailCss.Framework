namespace MonorailCss.Tests.Plugins.Borders;

public class BorderColorTests
{
    [Fact]
    public void Can_do_border_color_with_opacity()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-slate-700/20"]);
        r.ShouldBeCss("""

                      .border-slate-700\/20 {
                        border-color:oklch(0.372 0.044 257.287 / 0.2);
                      }

                      """);
    }

    [Fact]
    public void Can_do_directional_border_colors()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-r-transparent", "border-l-red-500", "border-t-blue-300", "border-b-green-700"]);
        r.ShouldBeCss("""

                      .border-r-transparent {
                        border-right-color:transparent;
                      }
                      .border-l-red-500 {
                        border-left-color:oklch(0.637 0.237 25.331);
                      }
                      .border-t-blue-300 {
                        border-top-color:oklch(0.809 0.105 251.813);
                      }
                      .border-b-green-700 {
                        border-bottom-color:oklch(0.527 0.154 150.069);
                      }

                      """);
    }

    [Fact]
    public void Can_do_axis_border_colors()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-x-red-500", "border-y-blue-300"]);
        r.ShouldBeCss("""

                      .border-x-red-500 {
                        border-left-color:oklch(0.637 0.237 25.331);
                        border-right-color:oklch(0.637 0.237 25.331);
                      }
                      .border-y-blue-300 {
                        border-bottom-color:oklch(0.809 0.105 251.813);
                        border-top-color:oklch(0.809 0.105 251.813);
                      }

                      """);
    }

    [Fact]
    public void Can_do_directional_border_colors_with_opacity()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });

        var r = framework.Process(["border-r-red-500/50", "border-l-blue-300/25"]);
        r.ShouldBeCss("""

                      .border-r-red-500\/50 {
                        border-right-color:oklch(0.637 0.237 25.331 / 0.5);
                      }
                      .border-l-blue-300\/25 {
                        border-left-color:oklch(0.809 0.105 251.813 / 0.25);
                      }

                      """);
    }
}