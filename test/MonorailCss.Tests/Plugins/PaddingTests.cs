namespace MonorailCss.Tests.Plugins;

public class PaddingTests
{
    [Fact]
    public void Supports_Negative_Values()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        var result = framework.Process(new[] { "py-4-", "px-4" });
        result.ShouldBeCss(@"
.py-4- {
  padding-bottom:-1rem;
  padding-top:-1rem;
}
.px-4 {
  padding-left:1rem;
  padding-right:1rem;
}
");
    }
}

public class PositionTests
{
    [Fact]
    public void Position_works()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        var result = framework.Process(new[] { "static", "fixed", "absolute", "relative", "sticky" });
        result.ShouldBeCss(@"
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

");
    }
}