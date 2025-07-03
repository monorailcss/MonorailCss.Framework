using Shouldly;

namespace MonorailCss.Tests.Plugins;

public class BackgroundTests
{
    [Fact]
    public void Can_do_gradient()
    {
        var framework =  new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty } );
        var r = framework.Process(["from-green-600", "via-purple-500", "bg-gradient-to-r", "to-blue-900"]);
        r.ShouldBeCss("""

                      .from-green-600 {
                        --monorail-gradient-from:oklch(0.627 0.194 149.214 / 1);
                        --monorail-gradient-stops:var(--monorail-gradient-from), var(--monorail-gradient-to, oklch(0.627 0.194 149.214 / 1));
                      }
                      .via-purple-500 {
                        --monorail-gradient-stops:var(--monorail-gradient-from), oklch(0.627 0.265 303.9 / 1), var(--monorail-gradient-to, oklch(0.627 0.265 303.9 / 1));;
                      }
                      .bg-gradient-to-r {
                        background-image:linear-gradient(to right, var(--monorail-gradient-stops));
                      }
                      .to-blue-900 {
                        --monorail-gradient-to:oklch(0.379 0.146 265.522 / 1);
                      }



                      """);
    }

    [Fact]
    public void Can_do_new_gradient_syntax()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["bg-linear-to-r", "from-cyan-500", "to-blue-500"]);
        r.ShouldContain("--monorail-gradient-position:to right");
        r.ShouldContain("background-image:linear-gradient(var(--monorail-gradient-position), var(--monorail-gradient-stops))");
        r.ShouldContain("--monorail-gradient-from:oklch(0.715 0.143 215.221 / 1)");
        r.ShouldContain("--monorail-gradient-to:oklch(0.623 0.214 259.815 / 1)");
        r.ShouldContain("--monorail-gradient-stops:var(--monorail-gradient-from), var(--monorail-gradient-to, oklch(0.715 0.143 215.221 / 1))");
    }

    [Fact]
    public void Can_do_radial_gradient()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["bg-radial", "from-pink-400", "to-fuchsia-700"]);
        r.ShouldContain("--monorail-gradient-position:ellipse at center");
        r.ShouldContain("background-image:radial-gradient(var(--monorail-gradient-position), var(--monorail-gradient-stops))");
    }

    [Fact]
    public void Can_do_conic_gradient()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["bg-conic", "from-blue-600", "to-sky-400"]);
        r.ShouldContain("--monorail-gradient-position:from 0deg at center");
        r.ShouldContain("background-image:conic-gradient(var(--monorail-gradient-position), var(--monorail-gradient-stops))");
    }

    [Fact]
    public void Can_do_gradient_positions()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process(["from-40%", "to-50%"]);
        r.ShouldContain("--monorail-gradient-from-position:40%");
        r.ShouldContain("--monorail-gradient-to-position:50%");
    }

    [Fact]
    public void Can_do_user_example()
    {
        var framework = new CssFramework(new CssFrameworkSettings { CssResetOverride = string.Empty });
        var r = framework.Process([
            "h-14", "bg-linear-to-r", "from-cyan-500", "to-blue-500",
            "size-18", "rounded-full", "bg-radial", "from-pink-400", "from-40%", "to-fuchsia-700",
            "size-24", "bg-conic", "from-blue-600", "to-sky-400", "to-50%"
        ]);
        
        // Verify key CSS parts
        r.ShouldContain(".bg-linear-to-r");
        r.ShouldContain("--monorail-gradient-position:to right");
        r.ShouldContain("background-image:linear-gradient(var(--monorail-gradient-position), var(--monorail-gradient-stops))");
        r.ShouldContain(".bg-radial");
        r.ShouldContain("--monorail-gradient-position:ellipse at center");
        r.ShouldContain(".bg-conic");
        r.ShouldContain("--monorail-gradient-position:from 0deg at center");
        r.ShouldContain("--monorail-gradient-from-position:40%");
        r.ShouldContain("--monorail-gradient-to-position:50%");
    }

    [Fact]
    public void Can_do_current_color()
    {
        var framework = new CssFramework(new CssFrameworkSettings() { CssResetOverride = string.Empty });
        var r = framework.Process([
            "bg-current",
        ]);
        r.ShouldBeCss(".bg-current { background-color:currentColor; }");
    }
}