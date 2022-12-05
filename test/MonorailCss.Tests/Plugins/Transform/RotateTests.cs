namespace MonorailCss.Tests.Plugins.Transform;

public class RotateTests
{
    [Fact]
    public void Can_Do_Rotate()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "rotate-180",
        });
        r.ShouldBeCss(@"
body, ::before, ::after {
  --monorail-rotate:0;
  --monorail-scale-x:0;
  --monorail-scale-y:0;
  --monorail-skew-x:0;
  --monorail-skew-y:0;
  --monorail-translate-x:0;
  --monorail-translate-y:0;
}
.rotate-180 {
  --monorail-rotate:180deg;
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
            "-rotate-180",
        });
        r.ShouldBeCss(@"
body, ::before, ::after {
  --monorail-rotate:0;
  --monorail-scale-x:0;
  --monorail-scale-y:0;
  --monorail-skew-x:0;
  --monorail-skew-y:0;
  --monorail-translate-x:0;
  --monorail-translate-y:0;
}
.-rotate-180 {
  --monorail-rotate:-180deg;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }
}