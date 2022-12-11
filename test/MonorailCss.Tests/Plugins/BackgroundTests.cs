namespace MonorailCss.Tests.Plugins;

public class BackgroundTests
{
    [Fact]
    public void Can_do_gradient()
    {
        var framework =  new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var r = framework.Process(new[] {"from-green-600", "via-purple-500", "bg-gradient-to-r", "to-blue-900"});
        r.ShouldBeCss(@"
.from-green-600 {
  --monorail-gradient-from:rgba(22, 163, 74, 1);
  --monorail-gradient-stops:var(--monorail-gradient-from), var(--monorail-gradient-to, rgba(22, 163, 74, 1));
}
.via-purple-500 {
  --monorail-gradient-stops:var(--monorail-gradient-from), rgba(168, 85, 247, 1), var(--monorail-gradient-to, rgba(168, 85, 247, 1));;
}
.bg-gradient-to-r {
  background-image:linear-gradient(to right, var(--monorail-gradient-stops));
}
.to-blue-900 {
  --monorail-gradient-to:rgba(30, 58, 138, 1);
}

");
    }

    [Fact]
    public void Can_do_current_color()
    {
        var framework = new CssFramework(new CssFrameworkSettings() { CssResetOverride = string.Empty });
        var r = framework.Process(new[]
        {
            "bg-current",
        });
        r.ShouldBeCss(".bg-current { background-color:currentColor; }");
    }
}