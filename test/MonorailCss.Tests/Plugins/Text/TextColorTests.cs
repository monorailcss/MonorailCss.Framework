using NuGet.Frameworks;

namespace MonorailCss.Tests.Plugins.Text;

public class TextColorTests
{
    [Fact]
    public void Can_do_white()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default).WithCssReset(string.Empty);
        var result = framework.Process(new[] {"text-white"});
        result.ShouldBeCss(@"
.text-white {
  --monorail-text-opacity:1;
  color:rgba(255, 255, 255, var(--monorail-text-opacity));
}
");

    }

    [Fact]
    public void Can_do_opacities()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default).WithCssReset(string.Empty);
        var result = framework.Process(new[] {"text-blue-400/50"});
        result.ShouldBeCss(@"
.text-blue-400\/50 {
  color:rgba(96, 165, 250, 0.5);
}

");

    }
}