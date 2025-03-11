namespace MonorailCss.Tests.Plugins.Text;

public class TextColorTests
{
    [Fact]
    public void Can_do_white()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["text-white"]);
        result.ShouldBeCss("""

                           .text-white {
                             --monorail-text-opacity:1;
                             color:rgba(255, 255, 255, var(--monorail-text-opacity));
                           }

                           """);

    }

    [Fact]
    public void Can_do_opacities()
    {
        var framework =  new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["text-blue-400/50"]);
        result.ShouldBeCss("""

                           .text-blue-400\/50 {
                             color:oklch(0.707 0.165 254.624 / 0.5);
                           }


                           """);

    }
}