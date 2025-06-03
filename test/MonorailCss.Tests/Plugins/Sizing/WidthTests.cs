namespace MonorailCss.Tests.Plugins.Sizing;

public class WidthTests
{

    [Fact]
    public void Can_do_dynamic_sizing()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );

        var result = framework.Process(["p-4"]);
        result.ShouldBeCss("""
                           .p-4 {
                             padding:calc(var(--spacing) * 4);
                           }
                           """);
    }

    [Fact]
    public void Container_with_padding()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );

        var result = framework.Process(["w-3/4"]);
        result.ShouldBeCss("""

                           .w-3\/4 {
                             width:75%;
                           }


                           """);
    }
}