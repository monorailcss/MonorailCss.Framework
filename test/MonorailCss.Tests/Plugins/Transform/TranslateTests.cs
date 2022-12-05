namespace MonorailCss.Tests.Plugins.Transform;

public class TranslateTests
{
    [Fact]
    public void Can_Do_Translate()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "translate-x-2/3",
        });
        r.ShouldBeCss(@"
body, ::before, ::after {
  --monorail-translate-x:0;
  --monorail-translate-y:0;
}
.translate-x-2\/3 {
  --monorail-translate-x:66.666667%;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }

    [Fact]
    public void Can_Do_Negative_Translate()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty,
        });

        var r = framework.Process(new[]
        {
            "-translate-y-2/3",
        });
        r.ShouldBeCss(@"
body, ::before, ::after {
  --monorail-translate-x:0;
  --monorail-translate-y:0;
}
.-translate-y-2\/3 {
  --monorail-translate-y:-66.666667%;
  transform:translate(var(--monorail-translate-x), var(--monorail-translate-y)) rotate(var(--monorail-rotate)) skewX(var(--monorail-skew-x)) skewY(var(--monorail-skew-y)) scaleX(var(--monorail-scale-x)) scaleY(var(--monorail-scale-y));
}
");
    }
}