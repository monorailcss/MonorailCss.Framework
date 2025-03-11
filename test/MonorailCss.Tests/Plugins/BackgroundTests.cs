namespace MonorailCss.Tests.Plugins;

public class BackgroundTests
{
    [Fact]
    public void Can_do_gradient()
    {
        var framework =  new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var r = framework.Process(["from-green-600", "via-purple-500", "bg-gradient-to-r", "to-blue-900"]);
        r.ShouldBeCss("""

                      .from-green-600 {
                        --monorail-gradient-from:oklch(0.627 0.194 149.214 / 1);
                        --monorail-gradient-stops:var(--monorail-gradient-from), var(--monorail-gradient-to, oklch(0.627 0.194 149.214 / 1));
                      }
                      .via-purple-500 {
                        --monorail-gradient-stops:var(--monorail-gradient-from), oklch(0.627 0.265 303.9 / 1), var(--monorail-gradient-to, oklch(0.627 0.265 303.9 / 1));;
                      }
                      .bg-gradient-to-r {
                        background-image:linear-gradient(to right, var(--monorail-gradient-stops));
                      }
                      .to-blue-900 {
                        --monorail-gradient-to:oklch(0.379 0.146 265.522 / 1);
                      }



                      """);
    }

    [Fact]
    public void Can_do_current_color()
    {
        var framework = new CssFramework(new CssFrameworkSettings() { CssResetOverride = string.Empty });
        var r = framework.Process([
            "bg-current",
        ]);
        r.ShouldBeCss(".bg-current { background-color:currentColor; }");
    }
}