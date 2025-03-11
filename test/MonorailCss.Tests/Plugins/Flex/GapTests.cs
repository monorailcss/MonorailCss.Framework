namespace MonorailCss.Tests.Plugins.Flex;

public class GapTests
{
    [Fact]
    public void Can_handle_gap()
    {
        var framework =  new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["gap-2", "gap-x-4", "gap-y-8"]);
result.ShouldBeCss("""

                   .gap-2 {
                     gap:0.5rem;
                   }
                   .gap-x-4 {
                     column-gap:1rem;
                   }
                   .gap-y-8 {
                     row-gap:2rem;
                   }


                   """);
    }
}