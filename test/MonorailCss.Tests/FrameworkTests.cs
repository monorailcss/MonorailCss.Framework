using MonorailCss.Tests.Plugins;

namespace MonorailCss.Tests;

public class FrameworkTests
{
    [Fact]
    public void Smoke_Test()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
            .WithCssReset(string.Empty);
        var r = framework.Process(new[] { "bg-blue-100", "dark:bg-blue-50", "hover:bg-blue-200", "hover:sm:bg-blue-300", "sm:bg-blue-400", "dark:sm:bg-blue-500", "prose-h1:bg-blue-200" });
        r.ShouldBeCss(@"
.bg-blue-100 {
  --monorail-bg-opacity:1;
  background-color:rgb(219 234 254 / var(--monorail-bg-opacity));
}
.dark .dark\:bg-blue-50 {
  --monorail-bg-opacity:1;
  background-color:rgb(239 246 255 / var(--monorail-bg-opacity));
}
.hover\:bg-blue-200:hover {
  --monorail-bg-opacity:1;
  background-color:rgb(191 219 254 / var(--monorail-bg-opacity));
}
.prose h1 .prose-h1\:bg-blue-200 {
  --monorail-bg-opacity:1;
  background-color:rgb(191 219 254 / var(--monorail-bg-opacity));
}
@media (min-width:640px) {
  .hover\:sm\:bg-blue-300:hover {
    --monorail-bg-opacity:1;
    background-color:rgb(147 197 253 / var(--monorail-bg-opacity));
  }
  .sm\:bg-blue-400 {
    --monorail-bg-opacity:1;
    background-color:rgb(96 165 250 / var(--monorail-bg-opacity));
  }
  .dark .dark\:sm\:bg-blue-500 {
    --monorail-bg-opacity:1;
    background-color:rgb(59 130 246 / var(--monorail-bg-opacity));
  }
}
");
    }

    [Fact]
    public void Can_Do_Apply()
    {
        var framework = new CssFramework(MonorailCss.DesignSystem.Default)
                .WithCssReset(string.Empty)
                .Apply("body", "font-sans mb-2");
        var r =  framework.Process(Array.Empty<string>());
        r.ShouldBeCss(@"
body {
  font-family:-apple-system, BlinkMacSystemFont, avenir next, avenir, segoe ui, helvetica neue, helvetica, Ubuntu, roboto, noto, arial, sans-serif;
  margin-bottom:0.5rem;
}
");

    }
}