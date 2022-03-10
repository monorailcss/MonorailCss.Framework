using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MonorailCss.BlazorCssJit;

[Generator]
public class CssJitGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<string[]> addClassAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForAttributeClassGeneration(s),
                transform: static (ctx, _) => GetCssClassesFromAddAttributeCall(ctx))
            .Where(static m => m is not null)!;

        IncrementalValuesProvider<string[]> addMarkup = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForAddMarkupGeneration(s),
                transform: static (ctx, _) => GetCssClassesFromAddMarkup(ctx))
            .Where(static m => m is not null)!;

        var allAddClassAttributes = addClassAttribute.Collect().Combine(addMarkup.Collect());

        context.RegisterSourceOutput(allAddClassAttributes, static (spc, source) => Execute(source, spc));
    }

    private static bool IsSyntaxTargetForAttributeClassGeneration(SyntaxNode node)
        => node is InvocationExpressionSyntax m && m.ArgumentList.Arguments.Count == 3 &&
           m.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax &&
           memberAccessExpressionSyntax.Name.ToString() == "AddAttribute" &&
           m.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax literalExpressionSyntax &&
           literalExpressionSyntax.ToString() == "\"class\"";

    private static bool IsSyntaxTargetForAddMarkupGeneration(SyntaxNode node)
        => node is InvocationExpressionSyntax m && m.ArgumentList.Arguments.Count == 2 &&
           m.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax &&
           memberAccessExpressionSyntax.Name.ToString() == "AddMarkupContent" &&
           m.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax;

    private static string[]? GetCssClassesFromAddAttributeCall(GeneratorSyntaxContext context)
    {
        // we know the node is a InvocationExpressionSyntax thanks to IsSyntaxTargetForGeneration
        var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

        if (invocationExpressionSyntax.ArgumentList.Arguments[2].Expression is not LiteralExpressionSyntax
            literalExpressionSyntax)
        {
            return null;
        }

        var value = literalExpressionSyntax.ToString().Replace("\"", string.Empty);
        return value.Split(' ');
    }

    private static string[]? GetCssClassesFromAddMarkup(GeneratorSyntaxContext context)
    {
        // forgive me.
        const string RegExPattern = @"class\s*=\s*[\'\""](?<value>[^<]*?)[\'\""]";
        var invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;

        if (invocationExpressionSyntax.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax
            literalExpressionSyntax)
        {
            return null;
        }

        if (context.SemanticModel.GetConstantValue(literalExpressionSyntax).Value is not string value)
        {
            return null;
        }

        List<string> results = new();
        var matches = Regex.Matches(value, RegExPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        for (var i = 0; i < matches.Count; i++)
        {
             results.AddRange(matches[i].Groups["value"].Captures[0].Value.Split(' '));
        }

        return results.ToArray();

    }

    private static void Execute((ImmutableArray<string[]> FromClassAttribute, ImmutableArray<string[]> FromMarkup) values, SourceProductionContext context)
    {
        var distinctClasses = values.FromClassAttribute.Concat(values.FromMarkup).SelectMany(i => i).Distinct();
        var source = GenerateExtensionClass(distinctClasses);
        context.AddSource("cssjit.g.cs", source);
    }

    private static string GenerateExtensionClass(IEnumerable<string> classesToGenerate)
    {
        var sb = new StringBuilder();
        sb.Append(@"namespace MonorailCss
{
    public static partial class CssJit
    {
        public static string[] Values() => new string[] {

");
        foreach (var className in classesToGenerate)
        {
            sb.Append($"\"{className}\",");
        }

        sb.Append(@"
        };
    }
}");

        return sb.ToString();
    }
}
