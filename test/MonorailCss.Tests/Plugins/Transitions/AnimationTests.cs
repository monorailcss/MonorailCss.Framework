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
        var r = framework.Process(new[]
        {
            "animate-spin"
        });
        r.ShouldBeCss(@"

@keyframes spin {
  to {
    transform:rotate(360deg);
  }
}
.animate-spin {
  animation:spin 1s linear infinite;
}
");
    }


    [Fact]
    public void Can_Do_Pinging()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process(new[]
        {
            "animate-ping"
        });
        r.ShouldBeCss(@"
@keyframes ping {
  75%, 100% {
    transform:scale(2);
    opacity:0;
  }
}
.animate-ping {
  animation:ping 1s cubic-bezier(0, 0, 0.2, 1) infinite;
}
");
    }

    [Fact]
    public void Keyframe_should_not_have_modifiers()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process(new[]
        {
            "hover:animate-ping"
        });
        r.ShouldBeCss(@"
@keyframes ping {
  75%, 100% {
    transform:scale(2);
    opacity:0;
  }
}
.hover\:animate-ping:hover {
  animation:ping 1s cubic-bezier(0, 0, 0.2, 1) infinite;
}
");
    }


    [Fact]
    public void Keyframe_should_not_have_modifiers_but_media_should_be_respected()
    {
        var framework = new CssFramework(new CssFrameworkSettings()
        {
            CssResetOverride = string.Empty
        });
        var r = framework.Process(new[]
        {
            "md:hover:animate-ping"
        });
        r.ShouldBeCss(@"
@media (min-width:768px) {
  @keyframes ping {
    75%, 100% {
      transform:scale(2);
      opacity:0;
    }
  }
  .md\:hover\:animate-ping:hover {
    animation:ping 1s cubic-bezier(0, 0, 0.2, 1) infinite;
  }
}
");
    }
}