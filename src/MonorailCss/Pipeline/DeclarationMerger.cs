using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Pipeline;

internal enum MergeStrategy
{
    LastWins,
    FirstWins,
    Important,
    Smart,
}

internal class DeclarationMerger
{
    private static readonly Dictionary<string, string[]> _shorthandProperties = new()
    {
        ["margin"] = ["margin-top", "margin-right", "margin-bottom", "margin-left"],
        ["padding"] = ["padding-top", "padding-right", "padding-bottom", "padding-left"],
        ["border"] = ["border-width", "border-style", "border-color", "border-top", "border-right", "border-bottom", "border-left"],
        ["border-width"] = ["border-top-width", "border-right-width", "border-bottom-width", "border-left-width"],
        ["border-style"] = ["border-top-style", "border-right-style", "border-bottom-style", "border-left-style"],
        ["border-color"] = ["border-top-color", "border-right-color", "border-bottom-color", "border-left-color"],
        ["border-radius"] = ["border-top-left-radius", "border-top-right-radius", "border-bottom-right-radius", "border-bottom-left-radius"],
        ["background"] = ["background-color", "background-image", "background-repeat", "background-attachment", "background-position"],
        ["font"] = ["font-style", "font-variant", "font-weight", "font-size", "line-height", "font-family"],
        ["flex"] = ["flex-grow", "flex-shrink", "flex-basis"],
        ["grid-template"] = ["grid-template-rows", "grid-template-columns", "grid-template-areas"],
        ["grid-gap"] = ["grid-row-gap", "grid-column-gap"],
        ["gap"] = ["row-gap", "column-gap"],
        ["place-items"] = ["align-items", "justify-items"],
        ["place-content"] = ["align-content", "justify-content"],
        ["place-self"] = ["align-self", "justify-self"],
        ["overflow"] = ["overflow-x", "overflow-y"],
        ["transition"] = ["transition-property", "transition-duration", "transition-timing-function", "transition-delay"],
    };

    private static readonly Dictionary<string, string> _vendorPrefixMap = new()
    {
        ["-webkit-transform"] = "transform",
        ["-moz-transform"] = "transform",
        ["-ms-transform"] = "transform",
        ["-o-transform"] = "transform",
        ["-webkit-transition"] = "transition",
        ["-moz-transition"] = "transition",
        ["-ms-transition"] = "transition",
        ["-o-transition"] = "transition",
        ["-webkit-box-shadow"] = "box-shadow",
        ["-moz-box-shadow"] = "box-shadow",
        ["-webkit-border-radius"] = "border-radius",
        ["-moz-border-radius"] = "border-radius",
        ["-webkit-animation"] = "animation",
        ["-moz-animation"] = "animation",
        ["-ms-animation"] = "animation",
        ["-o-animation"] = "animation",
        ["-webkit-flex"] = "flex",
        ["-ms-flex"] = "flex",
        ["-webkit-box-sizing"] = "box-sizing",
        ["-moz-box-sizing"] = "box-sizing",
    };

    public ImmutableList<Declaration> MergeDeclarations(
        IEnumerable<Declaration> declarations,
        MergeStrategy strategy = MergeStrategy.LastWins)
    {
        if (strategy == MergeStrategy.Smart)
        {
            return SmartMergeDeclarations(declarations);
        }

        var merged = new Dictionary<string, Declaration>();

        foreach (var declaration in declarations)
        {
            var key = declaration.Property;

            switch (strategy)
            {
                case MergeStrategy.LastWins:
                    merged[key] = declaration;
                    break;

                case MergeStrategy.FirstWins:
                    if (!merged.ContainsKey(key))
                    {
                        merged[key] = declaration;
                    }

                    break;

                case MergeStrategy.Important:
                    if (!merged.ContainsKey(key) || declaration.Important)
                    {
                        merged[key] = declaration;
                    }

                    break;
            }
        }

        return merged.Values.ToImmutableList();
    }

    private ImmutableList<Declaration> SmartMergeDeclarations(IEnumerable<Declaration> declarations)
    {
        var merged = new Dictionary<string, Declaration>();
        var processedShorthands = new HashSet<string>();
        var declarationsList = declarations.ToList();

        foreach (var declaration in declarationsList)
        {
            var property = declaration.Property;

            // Handle vendor prefixes
            var normalizedProperty = NormalizeVendorPrefix(property);

            // Check if this is a shorthand property
            if (_shorthandProperties.ContainsKey(normalizedProperty))
            {
                // Remove any conflicting longhand properties
                foreach (var longhand in _shorthandProperties[normalizedProperty])
                {
                    merged.Remove(longhand);

                    // Also remove vendor-prefixed versions
                    foreach (var prefix in new[] { "-webkit-", "-moz-", "-ms-", "-o-" })
                    {
                        merged.Remove(prefix + longhand);
                    }
                }

                processedShorthands.Add(normalizedProperty);
                merged[property] = declaration;
            }

            // Check if this is a longhand property that conflicts with a shorthand
            else if (IsLonghandProperty(normalizedProperty, out var shorthand))
            {
                // Only add if the shorthand hasn't been processed yet
                if (shorthand == null || !processedShorthands.Contains(shorthand))
                {
                    // If the values are identical to an existing declaration, skip
                    if (merged.TryGetValue(property, out var existing) &&
                        existing.Value == declaration.Value &&
                        existing.Important == declaration.Important)
                    {
                        continue;
                    }

                    merged[property] = declaration;
                }
            }
            else
            {
                // Handle exact duplicates (same property and value)
                if (merged.TryGetValue(property, out var existing))
                {
                    // Keep the important one, or the last one if neither/both are important
                    if (declaration.Important && !existing.Important)
                    {
                        merged[property] = declaration;
                    }
                    else if (!declaration.Important && !existing.Important)
                    {
                        merged[property] = declaration; // LastWins for non-important
                    }

                    // If both are important or existing is important, keep existing
                }
                else
                {
                    merged[property] = declaration;
                }

                // Also check for vendor prefix duplicates
                if (_vendorPrefixMap.TryGetValue(property, out var unprefixed))
                {
                    // If we have the unprefixed version with the same value, we might skip the prefixed
                    if (merged.TryGetValue(unprefixed, out var unprefixedDecl) &&
                        unprefixedDecl.Value == declaration.Value)
                    {
                        // Keep both for maximum compatibility, but ensure consistency
                        merged[property] = declaration;
                    }
                }
            }
        }

        // Order declarations sensibly (vendor prefixes before unprefixed)
        return merged.Values
            .OrderBy(d => GetDeclarationOrder(d.Property))
            .ThenBy(d => d.Property)
            .ToImmutableList();
    }

    private string NormalizeVendorPrefix(string property)
    {
        return _vendorPrefixMap.TryGetValue(property, out var normalized) ? normalized : property;
    }

    private bool IsLonghandProperty(string property, out string? shorthand)
    {
        foreach (var kvp in _shorthandProperties)
        {
            if (kvp.Value.Contains(property))
            {
                shorthand = kvp.Key;
                return true;
            }
        }

        shorthand = null;
        return false;
    }

    private int GetDeclarationOrder(string property)
    {
        // Vendor prefixes come before unprefixed versions
        if (property.StartsWith("-webkit-"))
        {
            return 1;
        }

        if (property.StartsWith("-moz-"))
        {
            return 2;
        }

        if (property.StartsWith("-ms-"))
        {
            return 3;
        }

        if (property.StartsWith("-o-"))
        {
            return 4;
        }

        return 5;
    }

    public ImmutableList<AstNode> ExtractAndMergeDeclarations(
        IEnumerable<AstNode> nodes,
        MergeStrategy strategy = MergeStrategy.LastWins)
    {
        var declarations = new List<Declaration>();
        var otherNodes = new List<AstNode>();

        foreach (var node in nodes)
        {
            if (node is Declaration decl)
            {
                declarations.Add(decl);
            }
            else if (node is StyleRule styleRule)
            {
                foreach (var child in styleRule.Nodes)
                {
                    if (child is Declaration childDecl)
                    {
                        declarations.Add(childDecl);
                    }
                }
            }
            else
            {
                otherNodes.Add(node);
            }
        }

        var mergedDeclarations = MergeDeclarations(declarations, strategy);
        return otherNodes.Concat(mergedDeclarations).ToImmutableList();
    }
}