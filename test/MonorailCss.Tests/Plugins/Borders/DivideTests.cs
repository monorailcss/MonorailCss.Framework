namespace MonorailCss.Tests.Plugins.Borders;

public class DivideTests
{
    [Fact]
    public void Divide_x_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var result = framework.Process(new[] { "divide-x" });
        result.ShouldBeCss(@"
.divide-x > :not([hidden]) ~ :not([hidden]) {
  --monorail-divide-x-reverse:0;
  border-left-width:calc(1px * var(--monorail-divide-x-reverse));
  border-right-width:calc(1px * calc(1 - var(--monorail-divide-x-reverse)));
}
");
    }
}