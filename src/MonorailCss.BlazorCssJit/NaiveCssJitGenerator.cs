using System.Collections.Immutable;
using AngleSharp;
using Microsoft.CodeAnalysis;

namespace MonorailCss.BlazorCssJit;

[Generator]
public class NaiveCssJitGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.CSSJIT_Naive", out var naive) ||
            naive != "true")
        {
            return;
        }

        // this is horribly, horribly inefficient. it should be ran by looking at the syntax tree of the generated
        // code, but we need this issue to be resolved - https://github.com/dotnet/roslyn/issues/57239. until then, we'll
        // be idiots and do it as simple as possible.
        var additionalFiles = context.AdditionalFiles.Where(i =>
            i.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
            i.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)).ToImmutableArray();

        var config = Configuration.Default;

        //Create a new context for evaluating webpages with the given config
        var browsing = BrowsingContext.New(config);

        List<string> cssClasses = new();

        foreach (var additionalFile in additionalFiles)
        {
            var sourceText = additionalFile.GetText()?.ToString();
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                continue;
            }

            var document = browsing.OpenAsync(req =>
            {
                req.Content(sourceText);
            }).Result;


            cssClasses.AddRange(document.All
                .Select(i => i.Attributes["class"]?.Value)
                .Where(i => i != default)
                .OfType<string>()
                .SelectMany(i => i.Split(' ')));
        }

        var source = @$"public class CssJitNaive
{{
    public static string AllFoundCssClasses()
    {{
        return @""{string.Join(Environment.NewLine, cssClasses.Distinct())}"";        
    }}
}}";

        context.AddSource("css-jit.g.cs", source);
    }
}