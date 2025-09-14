using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities;
using MonorailCss.Variants;

namespace MonorailCss.Sorting;

/// <summary>
/// Manages sorting of CSS classes according to Tailwind's canonical ordering.
/// </summary>
internal sealed class SortingManager
{
    private readonly VariantRegistry _variantRegistry;

    public SortingManager(VariantRegistry variantRegistry)
    {
        _variantRegistry = variantRegistry ?? throw new ArgumentNullException(nameof(variantRegistry));
    }

    /// <summary>
    /// Gets the sort order for a CSS class based on its CSS properties.
    /// </summary>
    public ClassOrder GetClassOrder(ProcessedClass processedClass, int originalIndex)
    {
        // Get variant weights
        var variantWeights = _variantRegistry.GetVariantWeights(processedClass.Candidate.Variants);

        // Get utility order based on CSS properties
        var utilityOrder = GetPropertyOrder(processedClass.CssProperties);

        // Use normalized form for tie-breaking
        var normalizedName = processedClass.Candidate.Normalized ?? processedClass.Candidate.Raw;

        return new ClassOrder(variantWeights, processedClass.Layer, utilityOrder, normalizedName, originalIndex);
    }

    /// <summary>
    /// Gets the sort order for a CSS class using candidate only (fallback).
    /// </summary>
    public ClassOrder GetClassOrder(Candidate candidate, int originalIndex)
    {
        // Get variant weights
        var variantWeights = _variantRegistry.GetVariantWeights(candidate.Variants);

        // Default order for candidates without processed properties
        var utilityOrder = int.MaxValue;

        // Use normalized form for tie-breaking
        var normalizedName = candidate.Normalized ?? candidate.Raw;

        // Default to Utility layer for candidates without ProcessedClass
        return new ClassOrder(variantWeights, UtilityLayer.Utility, utilityOrder, normalizedName, originalIndex);
    }

    /// <summary>
    /// Sorts a collection of processed classes.
    /// </summary>
    public List<ProcessedClassWithOrder> SortClasses(IEnumerable<ProcessedClass> classes)
    {
        var classesWithOrder = new List<ProcessedClassWithOrder>();
        var index = 0;

        foreach (var processedClass in classes)
        {
            var order = GetClassOrder(processedClass, index++);
            classesWithOrder.Add(new ProcessedClassWithOrder(processedClass, order));
        }

        // Sort by the computed order
        classesWithOrder.Sort((a, b) => a.Order.CompareTo(b.Order));

        return classesWithOrder;
    }

    /// <summary>
    /// Sorts AST nodes based on their associated candidates.
    /// </summary>
    public ImmutableList<AstNode> SortNodes(IEnumerable<(AstNode Node, Candidate Candidate)> nodesWithCandidates)
    {
        var sortedItems = nodesWithCandidates
            .Select((item, index) => new
            {
                item.Node,
                item.Candidate,
                Order = GetClassOrder(item.Candidate, index),
            })
            .OrderBy(x => x.Order)
            .Select(x => x.Node);

        return sortedItems.ToImmutableList();
    }

    private int GetPropertyOrder(ImmutableList<string> properties)
    {
        if (properties.IsEmpty)
        {
            return int.MaxValue;
        }

        // Get the minimum order from all properties
        // This ensures utilities are sorted by their primary property
        return PropertyOrder.GetMinOrder(properties);
    }
}