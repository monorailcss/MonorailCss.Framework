namespace MonorailCss.Tests.Plugins.Flex;

public class FlexDirectionTests
{
    [Fact]
    public void Can_do_flex_direction()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default).WithCssReset(string.Empty);
        var r =framework.Process(new[] {"flex-col", "md:flex-row"});
        r.ShouldBeCss(@"
.flex-col {
  flex-direction:column;
}
@media (min-width:768px) {
  .md\:flex-row {
    flex-direction:row;
  }
}
");
    }
}