namespace MonorailCss.Tests.Plugins;

public class CursorTests
{

    [Fact]
    public void Cursor_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(new[] { "cursor-help" });
        result.ShouldBeCss(@"
.cursor-help {
  cursor:help;
}
");
    }
}
