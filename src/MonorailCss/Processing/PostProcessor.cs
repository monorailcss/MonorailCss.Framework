using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Variants;
using ArbitraryVariant = MonorailCss.Variants.BuiltIn.ArbitraryVariant;

namespace MonorailCss.Processing;

internal sealed class PostProcessor
{
    private readonly VariantRegistry _variantRegistry;

    public PostProcessor(VariantRegistry variantRegistry)
    {
        _variantRegistry = variantRegistry;
    }

    /// <summary>
    /// Applies post-processing to AST nodes based on candidate properties.
    /// </summary>
    /// <param name="nodes">The compiled AST nodes from utilities.</param>
    /// <param name="candidate">The candidate with variants and important flag.</param>
    /// <returns>The processed AST nodes with variants applied.</returns>
    public ImmutableList<AstNode> ApplyPostProcessing(ImmutableList<AstNode> nodes, Candidate candidate)
    {
        if (nodes.IsEmpty)
        {
            return nodes;
        }

        // Check if nodes contain ComponentRule - handle specially
        if (nodes.Count == 1 && nodes[0] is ComponentRule component)
        {
            return ProcessComponentRule(component, candidate);
        }

        // Start with base selector from escaped class name
        var escapedClassName = CssClassEscaper.Escape(candidate.Raw);
        var baseSelector = Selector.FromClass(escapedClassName);

        // Apply variants in forward order to maintain source order
        // This ensures hover:focus generates :hover:focus, not :focus:hover
        var appliedSelector = new AppliedSelector(baseSelector, ImmutableArray<AtRuleWrapper>.Empty);

        // Process variants from left to right
        foreach (var variantToken in candidate.Variants)
        {
            // Try to find and apply the variant using registry as primary path
            if (_variantRegistry.TryGet(variantToken.Name, out var variant))
            {
                if (variant.TryApply(appliedSelector, variantToken, out var newAppliedSelector))
                {
                    appliedSelector = newAppliedSelector;
                }
            }

            // Fall back to arbitrary variant handling only when registry lookup fails
            else if (variantToken is { IsArbitrary: true, Value: not null })
            {
                // Create an arbitrary variant on-the-fly with default weight
                var arbitraryVariant = new ArbitraryVariant(800);
                if (arbitraryVariant.TryApply(appliedSelector, variantToken, out var newAppliedSelector))
                {
                    appliedSelector = newAppliedSelector;
                }
            }
        }

        // Apply important flag to all declarations
        if (candidate.Important)
        {
            nodes = ApplyImportant(nodes);
        }

        // Wrap the nodes with the final selector
        var result = ImmutableList.CreateBuilder<AstNode>();

        // Create the style rule with the composed selector
        var styleRule = new StyleRule(appliedSelector.Selector.Value, nodes);

        // Wrap with at-rules if any
        // Apply wrappers in reverse order so leftmost variant becomes outermost wrapper
        AstNode currentNode = styleRule;
        foreach (var wrapper in appliedSelector.Wrappers.Reverse())
        {
            currentNode = new AtRule(wrapper.Name, wrapper.Params, ImmutableList.Create(currentNode));
        }

        result.Add(currentNode);
        return result.ToImmutable();
    }

    /// <summary>
    /// Applies the important flag to all declarations in the AST nodes.
    /// </summary>
    private ImmutableList<AstNode> ApplyImportant(ImmutableList<AstNode> nodes)
    {
        var builder = ImmutableList.CreateBuilder<AstNode>();

        foreach (var node in nodes)
        {
            switch (node)
            {
                case Declaration decl:
                    // Create a new declaration with Important = true
                    builder.Add(decl with { Important = true });
                    break;

                case StyleRule rule:
                    {
                        // Recursively apply important to nested declarations
                        var importantNodes = ApplyImportant(rule.Nodes);
                        builder.Add(rule with { Nodes = importantNodes });
                        break;
                    }

                case AtRule atRule:
                    {
                        // Recursively apply important to nested nodes
                        var importantNodes = ApplyImportant(atRule.Nodes);
                        builder.Add(atRule with { Nodes = importantNodes });
                        break;
                    }

                default:
                    // Keep other nodes as-is
                    builder.Add(node);
                    break;
            }
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// Processes a ComponentRule which represents a complex utility with child element rules.
    /// </summary>
    private ImmutableList<AstNode> ProcessComponentRule(ComponentRule component, Candidate candidate)
    {
        var escapedClassName = CssClassEscaper.Escape(candidate.Raw);
        var baseSelector = Selector.FromClass(escapedClassName);
        var appliedSelector = new AppliedSelector(baseSelector, ImmutableArray<AtRuleWrapper>.Empty);

        // Apply variants
        foreach (var variantToken in candidate.Variants)
        {
            if (_variantRegistry.TryGet(variantToken.Name, out var variant))
            {
                if (variant.TryApply(appliedSelector, variantToken, out var newAppliedSelector))
                {
                    appliedSelector = newAppliedSelector;
                }
            }
            else if (variantToken is { IsArbitrary: true, Value: not null })
            {
                var arbitraryVariant = new ArbitraryVariant(800);
                if (arbitraryVariant.TryApply(appliedSelector, variantToken, out var newAppliedSelector))
                {
                    appliedSelector = newAppliedSelector;
                }
            }
        }

        var result = ImmutableList.CreateBuilder<AstNode>();

        // Generate base rule if there are base declarations
        if (component.BaseDeclarations.Any())
        {
            var baseDeclarations = candidate.Important
                ? ApplyImportant(component.BaseDeclarations.Cast<AstNode>().ToImmutableList())
                    .Cast<Declaration>().ToImmutableList()
                : component.BaseDeclarations;

            var baseRule = new StyleRule(appliedSelector.Selector.Value, baseDeclarations.Cast<AstNode>().ToImmutableList());

            // Wrap with at-rules if needed
            AstNode currentNode = baseRule;
            foreach (var wrapper in appliedSelector.Wrappers.Reverse())
            {
                currentNode = new AtRule(wrapper.Name, wrapper.Params, ImmutableList.Create(currentNode));
            }

            result.Add(currentNode);
        }

        // Generate child rules
        foreach (var child in component.ChildRules)
        {
            var childSelector = BuildChildSelector(appliedSelector.Selector.Value, child);
            var childDeclarations = candidate.Important
                ? ApplyImportant(child.Declarations.Cast<AstNode>().ToImmutableList())
                    .Cast<Declaration>().ToImmutableList()
                : child.Declarations;

            var childRule = new StyleRule(childSelector, childDeclarations.Cast<AstNode>().ToImmutableList());

            // Wrap with at-rules if needed
            AstNode currentNode = childRule;
            foreach (var wrapper in appliedSelector.Wrappers.Reverse())
            {
                currentNode = new AtRule(wrapper.Name, wrapper.Params, ImmutableList.Create(currentNode));
            }

            result.Add(currentNode);
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Builds a child selector based on parent selector and child rule configuration.
    /// </summary>
    private string BuildChildSelector(string parentSelector, ChildRule child)
    {
        var selector = $"{parentSelector} ";

        if (child.UseWhereWrapper)
        {
            selector += $":where({child.ChildSelector})";

            if (child.ExcludeClass != null)
            {
                selector += $":not(:where([class~=\"{child.ExcludeClass}\"],[class~=\"{child.ExcludeClass}\"] *))";
            }
        }
        else
        {
            selector += child.ChildSelector;
        }

        return selector;
    }
}