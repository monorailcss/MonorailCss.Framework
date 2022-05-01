namespace MonorailCss.Tests.Plugins;

public class FloatTests
{
    [Fact]
    public void Floats_work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(new[] { "float-left" });
        result.ShouldBeCss(@"
.float-left {
  float:left;
}
");
    }
}

public class MaxWidthTests
{
    [Fact]
    public void Max_Width_works()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(new[] { "max-w-prose", "max-w-xl", "max-w-fit	" });
        result.ShouldBeCss(@"
.max-w-prose {
  max-width:65ch;
}
.max-w-xl {
  max-width:36rem;
}
.max-w-fit	 {
  max-width:fit-content;
}

");
    }

    [Fact]
    public void Max_Width_Includes_Screens()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(new[] { "max-w-screen-xl" });
        result.ShouldBeCss(@"
.max-w-screen-xl {
  max-width:1280px;
}
");
    }
}