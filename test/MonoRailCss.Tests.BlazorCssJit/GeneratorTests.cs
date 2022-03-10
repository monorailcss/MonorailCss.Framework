using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;
using MonorailCss.BlazorCssJit;
using MonoRailCss.Tests.BlazorCssJit.Verifiers;

namespace MonoRailCss.Tests.BlazorCssJit;

using VerifyCS = CSharpIncrementalSourceGeneratorVerifier<CssJitGenerator>;

public class GeneratorTests
{
    static readonly ReferenceAssemblies ReferenceAssemblies = ReferenceAssemblies.Net.Net60
        .AddPackages(new[]
            {
                new PackageIdentity("Microsoft.AspNetCore.Components", "6.0.3"),
                new PackageIdentity("Microsoft.AspNetCore.Components.Web", "6.0.3")
            }.ToImmutableArray()
        );

    [Fact]
    public async Task Can_Generate()
    {
        var test = new VerifyCS.Test
        {
            ReferenceAssemblies = ReferenceAssemblies,
            TestState =
            {
                Sources = { input.Trim().ReplaceLineEndings() }, GeneratedSources = { (typeof(CssJitGenerator), "cssjit.g.cs", output.Trim().ReplaceLineEndings()) },
            }
        };
        await test.RunAsync();
    }

    private string output = @"
namespace MonorailCss
{
    public static partial class CssJit
    {
        public static string[] Values() => new string[] {

""alert"",""alert-secondary"",""mt-4"",""btn"",""btn-primary"",""oi"",""oi-pencil"",""me-2"",""text-nowrap"",""font-weight-bold"",""link-dark"",""oi-home"",
        };
    }
}
";

    private string input = @"
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;


namespace BlazorServerTestApp.Pages
{
   public class SurveyPrompt : ComponentBase
  {
    protected override void BuildRenderTree(
    RenderTreeBuilder __builder)
    {
      __builder.OpenElement(0, ""div"");
      __builder.AddAttribute(1, ""class"", ""alert alert-secondary mt-4"");
      __builder.AddMarkupContent(2, ""<span class=\""oi oi-pencil me-2\"" aria-hidden=\""true\""></span>\r\n    "");
      __builder.OpenElement(3, ""strong"");
      __builder.AddContent(4, this.Title);
      __builder.CloseElement();
      __builder.AddMarkupContent(5, ""\r\n\r\n    "");
      __builder.AddMarkupContent(6, ""<span class=\""text-nowrap\"">\r\n        Please take our\r\n        <a target=\""_blank\"" class=\""font-weight-bold link-dark\"" href=\""https://go.microsoft.com/fwlink/?linkid=2149017\"">brief survey</a></span>\r\n    and tell us what you think.\r\n"");
      __builder.AddMarkupContent(7, ""<span class=\""oi oi-home\"" aria-hidden=\""true\"" b-qibanybmmo></span> Home\r\n            "");
      __builder.CloseElement();
    }

    [Parameter]
    public
    #nullable enable
    string? Title { get; set; }
  }

 [Route(""/counter"")]
  public class Counter : ComponentBase
  {
    private int currentCount = 0;

    protected override void BuildRenderTree(
    #nullable disable
    RenderTreeBuilder __builder)
    {
      __builder.OpenComponent<PageTitle>(0);
      __builder.CloseComponent();
      __builder.AddMarkupContent(3, ""\r\n\r\n"");
      __builder.AddMarkupContent(4, ""<h1>Counter</h1>\r\n\r\n"");
      __builder.OpenElement(5, ""p"");
      __builder.AddAttribute(6, ""role"", ""status"");
      __builder.AddContent(7, ""Current count: "");
      __builder.AddContent(8, (object) this.currentCount);
      __builder.CloseElement();
      __builder.AddMarkupContent(9, ""\r\n\r\n"");
      __builder.OpenElement(10, ""button"");
      __builder.AddAttribute(11, ""class"", ""btn btn-primary"");
      __builder.AddAttribute<MouseEventArgs>(12, ""onclick"", EventCallback.Factory.Create<MouseEventArgs>((object) this, new Action(this.IncrementCount)));
      __builder.AddContent(13, ""Click me"");
      __builder.CloseElement();
    }

    private void IncrementCount() => ++this.currentCount;
  }
}

";
}