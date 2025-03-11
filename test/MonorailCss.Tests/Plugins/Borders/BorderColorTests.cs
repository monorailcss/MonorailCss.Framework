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
}