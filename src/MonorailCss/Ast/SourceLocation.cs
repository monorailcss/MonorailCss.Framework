namespace MonorailCss.Ast;

internal record SourceLocation(int Line, int Column, int Offset, int Length)
{
    public int EndLine => Line;
    public int EndColumn => Column + Length;
    public int EndOffset => Offset + Length;
}