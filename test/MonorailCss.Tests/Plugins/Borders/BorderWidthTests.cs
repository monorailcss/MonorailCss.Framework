namespace MonorailCss.Tests.Plugins.Borders;

public class BorderWidthTests
{
    [Fact]
    public void Border_width_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(["border", "border-2", "border-b-4", "border-t"]);
        result.ShouldBeCss("""

                           .border-b-4 {
                             border-bottom-width:4px;
                           }
                           .border {
                             border-width: 1px;
                           }
                           .border-t {
                             border-top-width: 1px;
                           }

                           .border-2 {
                             border-width: 2px;
                           }

                           """);
    }
}