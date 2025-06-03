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
                     gap:calc(var(--monorail-spacing) * 2);
                   }
                   .gap-x-4 {
                     column-gap:calc(var(--monorail-spacing) * 4);
                   }
                   .gap-y-8 {
                     row-gap:calc(var(--monorail-spacing) * 8);
                   }

                   """);
    }
}