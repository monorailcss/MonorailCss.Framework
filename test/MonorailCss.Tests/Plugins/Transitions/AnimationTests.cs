using Shouldly;

namespace MonorailCss.Tests.Plugins.Transitions;

public class AnimationTests
{
    [Fact]
    public void Can_Do_Spinning()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r= framework.Process(new[] { "animate-spin" });
        r.Trim().ShouldBe(@"

@keyframes spin {
  to {
    transform:rotate(360deg);
  }
}
.animate-spin {
  animation:spin 1s linear infinite;
}
".Trim());
    }


    [Fact]
    public void Can_Do_Pinging()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r= framework.Process(new[] { "animate-ping" });
        r.Trim().ShouldBe(@"
@keyframes ping {
  75%, 100% {
    transform:scale(2);
    opacity:0;
  }
}
.animate-ping {
  animation:ping 1s cubic-bezier(0, 0, 0.2, 1) infinite;
}
".Trim());
    }
}