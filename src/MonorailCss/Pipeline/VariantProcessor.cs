using System.Collections.Immutable;
using MonorailCss.Variants;

namespace MonorailCss.Pipeline;

internal class VariantProcessor
{
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