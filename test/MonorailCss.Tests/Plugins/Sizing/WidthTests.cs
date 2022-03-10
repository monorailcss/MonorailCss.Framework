namespace MonorailCss.Tests.Plugins.Sizing;

public class WidthTests
{

    [Fact]
    public void Container_with_padding()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);

        var result = framework.Process(new[] { "w-3/4" });
        result.ShouldBeCss(@"
.w-3\/4 {
  width:75%;
}

");
    }
}