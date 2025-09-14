using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;
using MonorailCss.Variants;

namespace MonorailCss.Pipeline;

internal class VariantProcessor
{
    public AstNode ApplyVariants(AstNode node, AppliedSelector appliedSelector)
    {
        if (node is StyleRule styleRule)
        {
            if (!appliedSelector.HasWrappers && appliedSelector.Selector.Value == styleRule.Selector)
            {
                return node;
            }

            var updatedRule = styleRule with { Selector = appliedSelector.Selector.Value };

            if (appliedSelector.HasWrappers)
            {
                AstNode wrappedNode = updatedRule;
                foreach (var wrapper in appliedSelector.Wrappers.Reverse())
                {
                    wrappedNode = new AtRule(wrapper.Name, wrapper.Params, ImmutableList.Create(wrappedNode));
                }

                return wrappedNode;
            }

            return updatedRule;
        }

        return node;
    }

    public string BuildComponentSelector(string baseSelector, ImmutableList<IVariant> variants)
    {
        if (variants.IsEmpty)
        {
            return baseSelector;
        }

        var selector = baseSelector;
        var pseudoClasses = new List<string>();
        var pseudoElements = new List<string>();

        foreach (var variant in variants)
        {
            switch (variant.Name)
            {
                case "dark":
                    return $":where(.dark, .dark *) {baseSelector}";

                case "before":
                case "after":
                case "first-line":
                case "first-letter":
                case "marker":
                case "selection":
                case "backdrop":
                case "placeholder":
                case "file":
                    pseudoElements.Add(variant.Name);
                    break;

                case "hover":
                case "focus":
                case "active":
                case "visited":
                case "disabled":
                case "checked":
                case "required":
                case "valid":
                case "invalid":
                case "in-range":
                case "out-of-range":
                case "focus-within":
                case "focus-visible":
                    pseudoClasses.Add(variant.Name);
                    break;

                default:
                    // For other static variants, assume pseudo-class
                    if (variant.Kind == VariantKind.Static)
                    {
                        pseudoClasses.Add(variant.Name);
                    }

                    break;
            }
        }

        foreach (var pc in pseudoClasses)
        {
            selector += $":{pc}";
        }

        if (pseudoElements.Count > 0)
        {
            selector += $"::{pseudoElements[0]}";
        }

        return selector;
    }
}