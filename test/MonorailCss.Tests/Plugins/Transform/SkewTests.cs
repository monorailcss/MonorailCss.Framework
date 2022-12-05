namespace MonorailCss.Tests.Plugins.Transform;

public class SkewTests
{
    [Fact]
    public void Can_Do_Skew()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "skew-x-12",
        });
        r.ShouldBeCss(@"
body, ::before, ::after {
  --monorail-rotate:0;
  --monorail-scale-x:1;
  --monorail-scale-y:1;
  --monorail-skew-x:0;
  --monorail-skew-y:0;
  --monorail-translate-x:0;
  --monorail-translate-y:0;
}
.skew-x-12 {
  --monorail-skew-x:12deg;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }

    [Fact]
    public void Can_Do_Negative()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "-skew-x-12",
        });
        r.ShouldBeCss(@"
body, ::before, ::after {
  --monorail-rotate:0;
  --monorail-scale-x:1;
  --monorail-scale-y:1;
  --monorail-skew-x:0;
  --monorail-skew-y:0;
  --monorail-translate-x:0;
  --monorail-translate-y:0;
}
.-skew-x-12 {
  --monorail-skew-x:-12deg;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }
}