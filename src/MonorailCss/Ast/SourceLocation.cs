namespace MonorailCss.Ast;

/// <summary>
/// Represents a specific location within a source text, defined by its line, column, character offset, and length.
/// Useful for tracking positions of elements in source code, such as in parsing or syntax tree operations.
/// </summary>
/// <param name="Line">The zero-based line number of the location in the source text.</param>
/// <param name="Column">The zero-based column number of the location in the source text.</param>
/// <param name="Offset">The zero-based character offset in the source text.</param>
/// <param name="Length">The length of the segment or token starting from this location.</param>
public record SourceLocation(int Line, int Column, int Offset, int Length)
{
    /// <summary>
    /// Gets the ending column number for the location in the source text, calculated as the sum of the
    /// starting column number and the length of the segment or token.
    /// </summary>
    /// <remarks>
    /// This property provides the zero-based column number where the segment or token ends
    /// in the source text, facilitating precise source location tracking.
    /// </remarks>
    public int EndColumn => Column + Length;

    /// <summary>
    /// Gets the ending character offset in the source text, calculated as the sum of the
    /// starting offset and the length of the segment or token.
    /// </summary>
    /// <remarks>
    /// This property provides the zero-based character offset immediately after the end
    /// of the segment or token in the source text, enabling accurate tracking of source positions.
    /// </remarks>
    public int EndOffset => Offset + Length;
}