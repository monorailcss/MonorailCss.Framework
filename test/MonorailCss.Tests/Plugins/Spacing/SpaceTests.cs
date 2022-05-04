namespace MonorailCss.Tests.Plugins;

public class SpaceTests
{
    [Fact]
    public void Space_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(new[] { "space-y-4" });
        result.ShouldBeCss(@"
.space-y-4 > :not([hidden]) ~ :not([hidden]) {
  --monorail-space-y-reverse:0;
  margin-top:calc(1rem * calc(1 - var(--monorail-space-y-reverse)));
  margin-bottom:calc(1rem * var(--monorail-space-y-reverse));
}
");
    }
}