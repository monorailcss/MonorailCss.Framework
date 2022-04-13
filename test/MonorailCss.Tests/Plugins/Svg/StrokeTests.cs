namespace MonorailCss.Tests.Plugins.Svg;

public class StrokeTests
{
    [Fact]
    public void Stroke_works()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default).WithCssReset(string.Empty);
        var r =framework.Process(new[] { "stroke-1" });
        r.ShouldBeCss(@"
.stroke-1 {
  stroke-width:1;
}
");
    }
}