using System.Collections.Immutable;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using MonorailCss.BlazorCssJit;

namespace MonoRailCss.Tests.BlazorCssJit;

public class NaiveGeneratorTests
{
    [Fact]
    public async Task Test1()
    {
        MSBuildLocator.RegisterDefaults();


        using var workspace = MSBuildWorkspace.Create();
        var projectPath = PathTestHelper.GetPath("../BlazorWasmTestApp");
        var projectFile = Path.Combine(projectPath, "BlazorWasmTestApp.csproj");

        var additionalFiles = new DirectoryInfo(projectPath)
            .GetFiles("*.razor", SearchOption.AllDirectories)
            .Select(i => new CustomAdditionalText(i.FullName))
            .Cast<AdditionalText>();

        var project = await workspace.OpenProjectAsync(projectFile);

        var compilation = await project.GetCompilationAsync() ?? throw new InvalidOperationException("Could not compile sample project");
        var generator = new NaiveCssJitGenerator();


        // Create the driver that will control the generation, passing in our generator
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.CreateRange(additionalFiles));

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
    }
}

public class CustomAdditionalText : AdditionalText
{
    private readonly string _text;

    public override string Path { get; }

    public CustomAdditionalText(string path)
    {
        Path = path;
        _text = File.ReadAllText(path);
    }

    public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
    {
        return SourceText.From(_text);
    }
}