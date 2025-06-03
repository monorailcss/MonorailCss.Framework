namespace MonorailCss.Tests.Plugins;

public class PaddingTests
{
    [Fact]
    public void Supports_Negative_Values()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["py-4-", "px-4"]);
        result.ShouldBeCss("""

                           .py-4- {
                             padding-bottom:calc(var(--monorail-spacing) * -4);
                             padding-top:calc(var(--monorail-spacing) * -4);
                           }
                           .px-4 {
                             padding-left:calc(var(--monorail-spacing) * 4);
                             padding-right:calc(var(--monorail-spacing) * 4);
                           }

                           """);
    }
}

public class PositionTests
{
    [Fact]
    public void Position_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["static", "fixed", "absolute", "relative", "sticky"]);
        result.ShouldBeCss("""

                           .static {
                             position:static;
                           }
                           .fixed {
                             position:fixed;
                           }
                           .absolute {
                             position:absolute;
                           }
                           .relative {
                             position:relative;
                           }
                           .sticky {
                             position:sticky;
                           }


                           """);
    }
}