using System.Collections.Immutable;

namespace MonorailCss;

/// <summary>
/// Represents the result of processing CSS classes through the CssFramework.
/// </summary>
internal record CssFrameworkResult
{
    public required string Input { get; init; }

    public required string GeneratedCss { get; init; }

    public required ImmutableArray<ProcessedClass> ProcessedClasses { get; init; }

    public required ImmutableArray<string> InvalidClasses { get; init; }

    public int TotalClasses => ProcessedClasses.Length + InvalidClasses.Length;

    public double SuccessRate => TotalClasses == 0 ? 1.0 : (double)ProcessedClasses.Length / TotalClasses;

    public bool IsFullyProcessed => InvalidClasses.IsEmpty;

    public bool HasProcessedClasses => !ProcessedClasses.IsEmpty;

    public bool HasInvalidClasses => !InvalidClasses.IsEmpty;
}