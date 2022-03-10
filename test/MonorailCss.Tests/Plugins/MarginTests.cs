namespace MonorailCss.Tests.Plugins;

public class MarginTests
{
    [Fact]
    public void Margins_work()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        var result = framework.Process(new[] { "my-4-", "mx-4" });
        result.ShouldBeCss(@"
.my-4- {
  margin-bottom:-1rem;
  margin-top:-1rem;
}
.mx-4 {
  margin-left:1rem;
  margin-right:1rem;
}
");
    }

    [Fact]
    public void Supports_Negative_Auto()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        var result = framework.Process(new[] { "mx-auto" });
        result.ShouldBeCss(@"
.mx-auto {
  margin-right:auto;
  margin-left:auto;
}
");
    }
}