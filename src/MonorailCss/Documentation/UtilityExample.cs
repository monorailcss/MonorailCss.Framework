namespace MonorailCss.Documentation;

/// <summary>
/// Represents a single example of a utility class usage.
/// </summary>
public class UtilityExample
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityExample"/> class.
    /// </summary>
    /// <param name="className">The utility class name (e.g., "bg-red-500", "flex", "w-full").</param>
    /// <param name="description">Human-readable description of what this utility does.</param>
    /// <param name="generatedCss">Optional preview of the CSS that would be generated (for documentation purposes).</param>
    public UtilityExample(string className, string description, string? generatedCss = null)
    {
        ClassName = className;
        Description = description;
        GeneratedCss = generatedCss;
    }

    /// <summary>
    /// Gets the utility class name.
    /// Example: "bg-red-500", "flex", "w-full".
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Gets a human-readable description of what this utility does.
    /// Example: "Set background color to red shade 500".
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets an optional preview of the generated CSS.
    /// Example: "background-color: oklch(63.7% 0.237 25.331)".
    /// </summary>
    public string? GeneratedCss { get; }

    /// <summary>
    /// Creates a new example with just a class name (description will be auto-generated).
    /// </summary>
    public static UtilityExample FromClassName(string className)
    {
        return new UtilityExample(className, $"Apply {className} utility");
    }
}
