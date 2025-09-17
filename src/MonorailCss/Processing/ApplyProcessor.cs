using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Parser;
using MonorailCss.Pipeline;
using MonorailCss.Pipeline.Stages;
using MonorailCss.Theme;
using MonorailCss.Variants;

namespace MonorailCss.Processing;

/// <summary>
/// Processes apply definitions (component classes) by expanding utility classes into CSS declarations.
/// </summary>
internal class ApplyProcessor
{
    private readonly CandidateParser _parser;
    private readonly DeclarationMerger _declarationMerger;
    private readonly VariantRegistry _variantRegistry;
    private readonly Pipeline.Pipeline _pipeline;

    public ApplyProcessor(UtilityRegistry utilityRegistry, VariantRegistry variantRegistry, Theme.Theme theme)
    {
        _parser = new CandidateParser(utilityRegistry);
        _declarationMerger = new DeclarationMerger();
        _variantRegistry = variantRegistry;

        // Create a pipeline with the necessary stages for processing applies
        // Note: We don't need ProcessingAndSortingStage here as applies handle their own variant application
        _pipeline = new Pipeline.Pipeline(
            new ThemeVariableTrackingStage(theme),
            new ArbitraryValueValidationStage(),
            new NegativeValueNormalizationStage(),
            new ColorModifierStage(theme),
            new ImportantFlagStage(),
            new VariableFallbackStage(),
            new PropertyRegistrationStage());
    }

    /// <summary>
    /// Converts an AppliedSelector (which uses class selector format) to component selector format.
    /// </summary>
    private static string ConvertAppliedSelectorToComponentSelector(AppliedSelector appliedSelector, string componentSelector)
    {
        var selectorString = appliedSelector.Selector.Value;

        // For descendant selectors, the appliedSelector already has the correct format
        // We should just use it as-is since the variant was applied to the full selector
        if (componentSelector.Contains(' ') && selectorString.StartsWith(componentSelector))
        {
            // The selector was correctly transformed by the variant system
            return selectorString;
        }

        // Handle special cases for component selectors
        if (selectorString.StartsWith("."))
        {
            // Regular class selector - replace with component selector
            var className = selectorString[1..];
            var parts = className.Split([':', '[', ' ', '>'], 2);
            if (parts.Length > 1)
            {
                // Has modifiers after the class name
                return string.Concat(componentSelector, selectorString.AsSpan(parts[0].Length + 1));
            }

            return componentSelector;
        }

        if (selectorString.Contains('&'))
        {
            // Contains & placeholder - replace with component selector
            return selectorString.Replace("&", componentSelector);
        }

        if (selectorString.StartsWith(":where") || selectorString.StartsWith(":is") ||
            selectorString.StartsWith(":not") || selectorString.StartsWith(":has"))
        {
            // Starts with a pseudo-function - needs special handling
            return componentSelector + selectorString;
        }

        if (selectorString.Contains(string.Concat(" ", componentSelector.AsSpan(1))))
        {
            // Already contains the component selector (e.g., for dark mode)
            return selectorString.Replace(string.Concat(".", componentSelector.AsSpan(1)), componentSelector);
        }

        // Default: use the selector as-is
        return selectorString;
    }

    /// <summary>
    /// Processes apply definitions into CSS component rules.
    /// </summary>
    /// <param name="applies">Dictionary of component selectors and their utility classes.</param>
    /// <param name="utilityRegistry">The utility registry containing registered utilities.</param>
    /// <param name="theme">The theme for resolving values.</param>
    /// <param name="propertyRegistry">Registry for CSS custom properties.</param>
    /// <param name="themeTracker">Theme tracker.
    /// </param>
    /// <returns>List of AST nodes representing component style rules.</returns>
    public List<AstNode> ProcessApplies(
        ImmutableDictionary<string, string> applies,
        UtilityRegistry utilityRegistry,
        Theme.Theme theme,
        CssPropertyRegistry propertyRegistry,
        ThemeUsageTracker themeTracker)
    {
        var componentNodes = new List<AstNode>();

        if (applies.Count == 0)
        {
            return componentNodes;
        }

        foreach (var (selector, utilityClasses) in applies)
        {
            var nodes = ProcessSingleApply(selector, utilityClasses, utilityRegistry, theme, propertyRegistry, themeTracker);
            componentNodes.AddRange(nodes);
        }

        return componentNodes;
    }

    /// <summary>
    /// Processes a single apply definition into style rules.
    /// </summary>
    private List<AstNode> ProcessSingleApply(
        string selector,
        string utilityClasses,
        UtilityRegistry utilityRegistry,
        Theme.Theme theme,
        CssPropertyRegistry propertyRegistry,
        ThemeUsageTracker themeTracker)
    {
        var nodes = new List<AstNode>();

        if (string.IsNullOrWhiteSpace(utilityClasses))
        {
            return nodes;
        }

        // Parse the utility classes
        var candidates = _parser.ParseCandidates(utilityClasses).ToList();

        // Group candidates by their variant combination for proper merging
        var variantGroups = new Dictionary<string, List<(Candidate Candidate, ImmutableList<AstNode> Nodes)>>();

        // First compile all utilities and create ProcessedClass objects
        var processedClasses = new List<ProcessedClass>();

        foreach (var candidate in candidates)
        {
            // Try to compile the utility
            foreach (var utility in utilityRegistry.RegisteredUtilities)
            {
                if (utility.TryCompile(candidate, theme, propertyRegistry, out var astNodes))
                {
                    // Create ProcessedClass for pipeline processing
                    var processedClass = new ProcessedClass(
                        ClassName: candidate.Raw,
                        UtilityName: utility.GetType().Name,
                        AstNodes: astNodes!,
                        Candidate: candidate,
                        Layer: utility.Layer);
                    processedClasses.Add(processedClass);
                    break;
                }
            }
        }

        // Process through the pipeline
        var pipelineContext = new PipelineContext
        {
            Metadata =
            {
                ["processedClasses"] = processedClasses,
                ["propertyRegistry"] = propertyRegistry,
                ["themeTracker"] = themeTracker,
            },
        };

        // Run the processed classes through the pipeline to apply all transformations
        _pipeline.Process(ImmutableList<AstNode>.Empty, pipelineContext);

        // Now group the processed classes by variant
        foreach (var processedClass in processedClasses)
        {
            var variantKey = string.Join(":", processedClass.Candidate.Variants.Select(v => v.Name));

            if (!variantGroups.TryGetValue(variantKey, out var value))
            {
                value = [];
                variantGroups[variantKey] = value;
            }

            value.Add((processedClass.Candidate, processedClass.AstNodes));
        }

        // Always ensure we have a base component rule, even if empty
        var hasBaseRule = false;

        // Process each variant group
        foreach (var (variantKey, group) in variantGroups)
        {
            if (string.IsNullOrEmpty(variantKey))
            {
                // No variants - create base style rule with merged declarations
                var allDeclarations = new List<Declaration>();

                foreach (var (_, astNodes) in group)
                {
                    ExtractDeclarations(astNodes, allDeclarations);
                }

                var mergedDeclarations = _declarationMerger.MergeDeclarations(allDeclarations);
                nodes.Add(new StyleRule(selector, mergedDeclarations.Cast<AstNode>().ToImmutableList()));
                hasBaseRule = true;
            }
            else
            {
                // Has variants - use the proper variant system
                var allDeclarations = new List<Declaration>();

                foreach (var (_, astNodes) in group)
                {
                    ExtractDeclarations(astNodes, allDeclarations);
                }

                var mergedDeclarations = _declarationMerger.MergeDeclarations(allDeclarations);

                if (mergedDeclarations.Count > 0)
                {
                    var firstCandidate = group[0].Candidate;

                    // Use the VariantRegistry to apply variants properly
                    // For descendant selectors, we need to pass them properly to the variant system
                    AppliedSelector appliedSelector;
                    if (selector.Contains(' '))
                    {
                        // For descendant selectors, create an AppliedSelector directly with the full selector
                        // Don't use FromClass since it's not a simple class name
                        appliedSelector = AppliedSelector.FromSelector(new Selector(selector));

                        // Now apply the variants to this selector
                        foreach (var variant in firstCandidate.Variants)
                        {
                            foreach (var registeredVariant in _variantRegistry.GetAll())
                            {
                                if (registeredVariant.CanHandle(variant))
                                {
                                    if (registeredVariant.TryApply(appliedSelector, variant, out var newResult))
                                    {
                                        appliedSelector = newResult;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Simple class selector - use the existing logic
                        var baseSelector = selector.StartsWith(".") ? selector.Substring(1) : selector;
                        appliedSelector = _variantRegistry.ApplyVariants(baseSelector, firstCandidate.Variants);
                    }

                    // Extract the final selector and handle any at-rule wrappers
                    var finalSelector = ConvertAppliedSelectorToComponentSelector(appliedSelector, selector);
                    var styleRule = new StyleRule(finalSelector, mergedDeclarations.Cast<AstNode>().ToImmutableList());

                    // Wrap in any at-rules (media queries, supports, etc.)
                    AstNode ruleToAdd = styleRule;
                    foreach (var wrapper in appliedSelector.Wrappers)
                    {
                        ruleToAdd = new AtRule(wrapper.Name, wrapper.Params, ImmutableList.Create(ruleToAdd));
                    }

                    nodes.Add(ruleToAdd);
                }
                else if (mergedDeclarations.Count > 0)
                {
                    // No variant registry, fallback to simple selector
                    var styleRule = new StyleRule(selector, mergedDeclarations.Cast<AstNode>().ToImmutableList());
                    nodes.Add(styleRule);
                }
            }
        }

        // If no base rule was created (all utilities have variants), add an empty one
        if (!hasBaseRule && variantGroups.Count > 0)
        {
            nodes.Insert(0, new StyleRule(selector, ImmutableList<AstNode>.Empty));
        }

        return nodes;
    }

    /// <summary>
    /// Extracts declarations from AST nodes into a list.
    /// </summary>
    private void ExtractDeclarations(ImmutableList<AstNode> astNodes, List<Declaration> declarations)
    {
        foreach (var node in astNodes)
        {
            switch (node)
            {
                case Declaration directDecl:
                    declarations.Add(directDecl);
                    break;

                case StyleRule styleRule:
                    {
                        foreach (var child in styleRule.Nodes)
                        {
                            if (child is Declaration decl)
                            {
                                declarations.Add(decl);
                            }
                        }

                        break;
                    }
            }
        }
    }
}