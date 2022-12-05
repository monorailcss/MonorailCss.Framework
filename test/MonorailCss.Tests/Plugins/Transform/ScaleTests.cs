namespace MonorailCss.Tests.Plugins.Transform;

public class ScaleTests
{
    [Fact]
    public void Can_Do_Scale()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "scale-x-50",
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
.scale-x-50 {
  --monorail-scale-x:.5;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }

    [Fact]
    public void Can_Do_Negative_Scale()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "-scale-x-50", "scale-y-50-", // both formats with prefix and postfix dash
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
.-scale-x-50 {
  --monorail-scale-x:-.5;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
.scale-y-50- {
  --monorail-scale-y:-.5;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }
}