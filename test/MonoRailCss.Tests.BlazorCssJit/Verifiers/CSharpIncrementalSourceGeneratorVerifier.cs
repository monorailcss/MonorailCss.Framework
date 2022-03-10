using Microsoft.CodeAnalysis;

namespace MonoRailCss.Tests.BlazorCssJit.Verifiers;

public static partial class CSharpIncrementalSourceGeneratorVerifier<TIncrementalGenerator>
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
}