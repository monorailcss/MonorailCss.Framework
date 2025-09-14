using MonorailCss.Sorting;

namespace MonorailCss;

/// <summary>
/// A processed class with its computed sort order.
/// </summary>
internal sealed record ProcessedClassWithOrder(ProcessedClass ProcessedClass, ClassOrder Order);