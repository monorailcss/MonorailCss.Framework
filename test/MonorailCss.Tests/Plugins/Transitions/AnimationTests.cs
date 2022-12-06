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
}