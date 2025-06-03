namespace MonorailCss.Tests.Plugins.Spacing;

public class SpaceTests
{
    [Fact]
    public void Space_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["space-y-4"]);
        result.ShouldBeCss("""

                           :root {
                             --monorail-spacing:0.25rem;
                           }
                           .space-y-4 > :not([hidden])~:not([hidden]) {
                             --monorail-space-y-reverse:0;
                             margin-bottom:calc(calc(var(spacing) * 4) * var(--monorail-space-y-reverse));
                             margin-top:calc(calc(var(spacing) * 4) * calc(1 - var(--monorail-space-y-reverse)));
                           }

                           """);
    }
}