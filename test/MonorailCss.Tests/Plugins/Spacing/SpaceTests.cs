namespace MonorailCss.Tests.Plugins.Spacing;

public class SpaceTests
{
    [Fact]
    public void Space_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["space-y-2"]);
        result.ShouldBeCss("""

                           :root {
                             --monorail-spacing:0.25rem;
                           }
                           .space-y-2 > :not(:last-child) {
                             --monorail-space-y-reverse:0;
                             margin-block-end:calc(var(--monorail-spacing) * 2 * (1 - var(--monorail-space-y-reverse)));
                             margin-block-start:calc(var(--monorail-spacing) * 2 * var(--monorail-space-y-reverse));
                           }

                           """);
    }
}