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
    private readonly VariantProcessor _variantProcessor;
    private readonly DeclarationMerger _declarationMerger;
    private readonly Theme.Theme? _theme;
    private readonly ColorModifierStage? _colorModifierStage;
    private readonly NegativeValueNormalizationStage? _negativeValueStage;

    public ApplyProcessor(UtilityRegistry utilityRegistry, VariantRegistry? variantRegistry = null, Theme.Theme? theme = null)
    {
        _parser = new CandidateParser(utilityRegistry);
        _variantProcessor = new VariantProcessor();
        _declarationMerger = new DeclarationMerger();
        _variantRegistry = variantRegistry;
        _theme = theme;

        // Create pipeline stages for processing applies
        if (theme != null)
        {
            _colorModifierStage = new ColorModifierStage(theme);
            _negativeValueStage = new NegativeValueNormalizationStage();
        }
    }

    private readonly VariantRegistry? _variantRegistry;

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
        ThemeUsageTracker? themeTracker)
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

        // Run through pipeline stages if available
        if (_colorModifierStage != null || _negativeValueStage != null)
        {
            var pipelineContext = new PipelineContext();
            pipelineContext.Metadata["processedClasses"] = processedClasses;
            if (themeTracker != null)
            {
                pipelineContext.Metadata["themeTracker"] = themeTracker;
            }

            // Apply pipeline stages
            if (_negativeValueStage != null)
            {
                _negativeValueStage.Process(ImmutableList<AstNode>.Empty, pipelineContext);
            }

            if (_colorModifierStage != null)
            {
                _colorModifierStage.Process(ImmutableList<AstNode>.Empty, pipelineContext);
            }

            // Also run theme tracking stage if we have a tracker
            if (themeTracker != null && _theme != null)
            {
                var trackingStage = new ThemeVariableTrackingStage(_theme);
                trackingStage.Process(ImmutableList<AstNode>.Empty, pipelineContext);
            }
        }

        // Now group the processed classes by variant
        foreach (var processedClass in processedClasses)
        {
            var variantKey = string.Join(":", processedClass.Candidate.Variants.Select(v => v.Name));

            if (!variantGroups.ContainsKey(variantKey))
            {
                variantGroups[variantKey] = new List<(Candidate, ImmutableList<AstNode>)>();
            }

            variantGroups[variantKey].Add((processedClass.Candidate, processedClass.AstNodes));
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
                // Has variants - use variant processor
                var allDeclarations = new List<Declaration>();

                foreach (var (_, astNodes) in group)
                {
                    ExtractDeclarations(astNodes, allDeclarations);
                }

                var mergedDeclarations = _declarationMerger.MergeDeclarations(allDeclarations);

                if (mergedDeclarations.Count > 0)
                {
                    var firstCandidate = group[0].Candidate;
                    var variants = new List<IVariant>();

                    if (_variantRegistry != null)
                    {
                        foreach (var variantToken in firstCandidate.Variants)
                        {
                            if (_variantRegistry.TryGet(variantToken.Name, out var variant))
                            {
                                variants.Add(variant);
                            }
                        }
                    }

                    var variantSelector = _variantProcessor.BuildComponentSelector(selector, variants.ToImmutableList());
                    var styleRule = new StyleRule(variantSelector, mergedDeclarations.Cast<AstNode>().ToImmutableList());
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
            if (node is Declaration directDecl)
            {
                declarations.Add(directDecl);
            }
            else if (node is StyleRule styleRule)
            {
                foreach (var child in styleRule.Nodes)
                {
                    if (child is Declaration decl)
                    {
                        declarations.Add(decl);
                    }
                }
            }
        }
    }
}