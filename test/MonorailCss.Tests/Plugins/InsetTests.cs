namespace MonorailCss.Tests.Plugins;

public class InsetTests
{
    [Fact]
    public void Inset_All_Sides_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["inset-4"]);
        result.ShouldBeCss("""

                           .inset-4 {
                             bottom:calc(var(--monorail-spacing) * 4);
                             left:calc(var(--monorail-spacing) * 4);
                             right:calc(var(--monorail-spacing) * 4);
                             top:calc(var(--monorail-spacing) * 4);
                           }

                           """);
    }

    [Fact]
    public void Inset_Logical_Properties_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["inset-x-2", "inset-y-3"]);
        result.ShouldBeCss("""

                           .inset-x-2 {
                             inset-inline:calc(var(--monorail-spacing) * 2);
                           }
                           .inset-y-3 {
                             inset-block:calc(var(--monorail-spacing) * 3);
                           }

                           """);
    }

    [Fact]
    public void Start_End_Properties_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["start-4", "end-2"]);
        result.ShouldBeCss("""

                           .end-2 {
                             inset-inline-end:calc(var(--monorail-spacing) * 2);
                           }
                           .start-4 {
                             inset-inline-start:calc(var(--monorail-spacing) * 4);
                           }

                           """);
    }

    [Fact]
    public void Traditional_Properties_Still_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["top-1", "right-2", "bottom-3", "left-4"]);
        result.ShouldBeCss("""

                           .bottom-3 {
                             bottom:calc(var(--monorail-spacing) * 3);
                           }
                           .left-4 {
                             left:calc(var(--monorail-spacing) * 4);
                           }
                           .right-2 {
                             right:calc(var(--monorail-spacing) * 2);
                           }
                           .top-1 {
                             top:calc(var(--monorail-spacing) * 1);
                           }

                           """);
    }

    [Fact]
    public void Auto_Values_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["inset-auto", "start-auto"]);
        result.ShouldBeCss("""

                           .inset-auto {
                             bottom:auto;
                             left:auto;
                             right:auto;
                             top:auto;
                           }
                           .start-auto {
                             inset-inline-start:auto;
                           }

                           """);
    }

    [Fact]
    public void Negative_Values_Work()
    {
        var framework = new CssFramework(new CssFrameworkSettings {CssResetOverride = string.Empty});
        var result = framework.Process(["inset-x-2-", "start-4-"]);
        result.ShouldBeCss("""

                           .inset-x-2- {
                             inset-inline:calc(var(--monorail-spacing) * -2);
                           }
                           .start-4- {
                             inset-inline-start:calc(var(--monorail-spacing) * -4);
                           }

                           """);
    }
}