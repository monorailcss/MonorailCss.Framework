using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Parser;
using MonorailCss.Pipeline;
using MonorailCss.Pipeline.Stages;
using MonorailCss.Processing;
using MonorailCss.Sorting;
using MonorailCss.Theme;
using MonorailCss.Utilities;
using MonorailCss.Variants;

namespace MonorailCss;

/// <summary>
/// Main entry point for the MonorailCss framework.
/// Processes Tailwind CSS utility classes and generates optimized CSS output.
/// </summary>
public class CssFramework
{
    private readonly CandidateParser _parser;
    private readonly ApplyProcessor _applyProcessor;
    private readonly CssFrameworkSettings _settings;
    private readonly VariantRegistry _variantRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssFramework"/> class.
    /// Initializes a new instance of the CssFramework with default configuration.
    /// </summary>
    public CssFramework()
        : this(new CssFrameworkSettings())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CssFramework"/> class.
    /// Initializes a new instance of the CssFramework with custom settings.
    /// </summary>
    /// <param name="settings">The framework settings.</param>
    public CssFramework(CssFrameworkSettings settings)
    {
        // Apply ProseCustomization to the theme if needed
        var processedTheme = settings.Theme;
        if (settings.ProseCustomization != null)
        {
            processedTheme = new Theme.Theme(processedTheme.Values)
            {
                Prefix = processedTheme.Prefix,
                ProseCustomization = settings.ProseCustomization,
            };
        }

        _settings = settings with
        {
            Theme = processedTheme,
        };

        UtilityRegistry = new UtilityRegistry(autoRegisterUtilities: true);
        _parser = new CandidateParser(UtilityRegistry);

        _variantRegistry = new VariantRegistry();
        _variantRegistry.RegisterBuiltInVariants(_settings.Theme);

        _applyProcessor = new ApplyProcessor(UtilityRegistry, _variantRegistry, _settings.Theme);
    }

    /// <summary>
    /// Gets the utility registry used by this framework instance.
    /// </summary>
    private UtilityRegistry UtilityRegistry { get; }

    /// <summary>
    /// Adds a custom utility to the framework at runtime.
    /// The utility will be registered with proper priority ordering.
    /// </summary>
    /// <param name="utility">The utility to add.</param>
    public void AddUtility(IUtility utility)
    {
        if (utility == null)
        {
            throw new ArgumentNullException(nameof(utility));
        }

        UtilityRegistry.RegisterUtility(utility);
    }

    /// <summary>
    /// Adds multiple custom utilities to the framework at runtime.
    /// The utilities will be registered with proper priority ordering.
    /// </summary>
    /// <param name="utilities">The utilities to add.</param>
    public void AddUtilities(IEnumerable<IUtility> utilities)
    {
        if (utilities == null)
        {
            throw new ArgumentNullException(nameof(utilities));
        }

        foreach (var utility in utilities)
        {
            AddUtility(utility);
        }
    }

    /// <summary>
    /// Adds a custom variant to the framework at runtime.
    /// </summary>
    /// <param name="variant">The variant to add.</param>
    /// <param name="overwrite">Whether to overwrite an existing variant with the same name.</param>
    public void AddVariant(IVariant variant, bool overwrite = false)
    {
        if (variant == null)
        {
            throw new ArgumentNullException(nameof(variant));
        }

        _variantRegistry.Register(variant, overwrite);
    }

    /// <summary>
    /// Processes a string of CSS classes and returns the generated CSS.
    /// </summary>
    /// <param name="classString">The CSS classes to process (e.g., "bg-red-500 text-white p-4").</param>
    /// <returns>The generated CSS string.</returns>
    public string Process(string classString)
    {
        var result = ProcessWithDetails(classString);
        return result.GeneratedCss;
    }

    /// <summary>
    /// Processes a string of CSS classes and returns detailed results including
    /// generated CSS, processed classes, and invalid classes.
    /// </summary>
    /// <param name="classString">The CSS classes to process.</param>
    /// <returns>A detailed result object containing CSS and processing information.</returns>
    internal CssFrameworkResult ProcessWithDetails(string classString)
    {
        // Don't return early if we have applies to process
        if (string.IsNullOrWhiteSpace(classString) && _settings.Applies.Count == 0)
        {
            return new CssFrameworkResult
            {
                Input = classString,
                GeneratedCss = string.Empty,
                ProcessedClasses = ImmutableArray<ProcessedClass>.Empty,
                InvalidClasses = ImmutableArray<string>.Empty,
            };
        }

        var processedClasses = new List<ProcessedClass>();
        var invalidClasses = new List<string>();
        var propertyRegistry = new CssPropertyRegistry();
        var themeTracker = new ThemeUsageTracker(_settings.Theme);

        // Parse the input into candidates
        var candidates = string.IsNullOrWhiteSpace(classString)
            ? []
            : _parser.ParseCandidates(classString).ToList();

        // Process each candidate through the utilities
        foreach (var candidate in candidates)
        {
            if (candidate is StaticUtility staticUtility && UtilityRegistry.StaticUtilitiesLookup.TryGetValue(staticUtility.Root, out var value))
            {
                if (value.TryCompile(candidate, _settings.Theme, propertyRegistry, out var astNodes))
                {
                    processedClasses.Add(new ProcessedClass(
                        ClassName: candidate.Raw,
                        UtilityName: value.GetType().Name,
                        AstNodes: astNodes!,
                        Candidate: candidate,
                        Layer: value.Layer));

                    continue;
                }
            }

            var processed = false;

            // Try each registered utility
            foreach (var utility in UtilityRegistry.RegisteredUtilities)
            {
                if (!utility.TryCompile(candidate, _settings.Theme, propertyRegistry, out var astNodes))
                {
                    continue;
                }

                processedClasses.Add(new ProcessedClass(
                    ClassName: candidate.Raw,
                    UtilityName: utility.GetType().Name,
                    AstNodes: astNodes!,
                    Candidate: candidate,
                    Layer: utility.Layer));
                processed = true;
                break;
            }

            if (!processed)
            {
                invalidClasses.Add(candidate.Raw);
            }
        }

        // Process classes through the pipeline
        var postProcessor = new PostProcessor(_variantRegistry);
        var sortingManager = new SortingManager(_variantRegistry);

        // Initialize pipeline with stages
        var pipeline = new Pipeline.Pipeline(
            new ThemeVariableTrackingStage(_settings.Theme),
            new ArbitraryValueValidationStage(),
            new NegativeValueNormalizationStage(),
            new ColorModifierStage(_settings.Theme),
            new ImportantFlagStage(),
            new VariableFallbackStage(),
            new PropertyRegistrationStage(),
            new ProcessingAndSortingStage(postProcessor, sortingManager),
            new DeclarationMergingStage(new DeclarationMerger()),
            new MediaQueryConsolidationStage(),
            new LayerAssignmentStage());

        var pipelineContext = new PipelineContext();
        pipelineContext.Metadata["processedClasses"] = processedClasses;
        pipelineContext.Metadata["propertyRegistry"] = propertyRegistry;
        pipelineContext.Metadata["themeTracker"] = themeTracker;

        // Process through the pipeline - handles post-processing, sorting, merging, and layering
        var cssRules = pipeline.Process(ImmutableList<AstNode>.Empty, pipelineContext).ToList();

        // Collect used theme variables (optimized single-pass method)
        var usedVariablesImmutable = themeTracker.GetUsedValuesWithValues();
        var usedVariables = new Dictionary<string, string>(usedVariablesImmutable);

        // Always include font variables in theme (Tailwind v4 compatibility)
        EnsureFontVariables(usedVariables, _settings.Theme);

        // Generate preflight CSS if enabled
        string? preflightCss = null;
        if (_settings.IncludePreflight)
        {
            preflightCss = PreflightCss.Process(_settings.Theme);
        }

        // Process applies (component classes) if configured
        var componentNodes = new List<AstNode>();
        if (_settings.Applies.Count > 0)
        {
            componentNodes = _applyProcessor.ProcessApplies(_settings.Applies, UtilityRegistry, _settings.Theme, propertyRegistry, themeTracker);

            // Add theme variables used by applies
            foreach (var (key, value) in themeTracker.GetUsedValuesWithValues())
            {
                if (!usedVariables.ContainsKey(key))
                {
                    usedVariables[key] = value;
                }
            }
        }

        // Generate the final CSS
        var generator = new CssGenerator();
        var generatedCss = generator.GenerateCss(cssRules.ToImmutableList(), usedVariables, propertyRegistry, false, preflightCss, componentNodes);

        return new CssFrameworkResult
        {
            Input = classString,
            GeneratedCss = generatedCss,
            ProcessedClasses = [.. processedClasses],
            InvalidClasses = [.. invalidClasses],
        };
    }

    /// <summary>
    /// Processes a collection of CSS class names and returns the generated CSS.
    /// </summary>
    /// <param name="candidates">The CSS class names to process.</param>
    /// <returns>The generated CSS string.</returns>
    public string Process(IEnumerable<string> candidates)
    {
        var combined = string.Join(" ", candidates.Where(s => !string.IsNullOrWhiteSpace(s)));
        return Process(combined);
    }

    /// <summary>
    /// Processes a collection of CSS class names and returns detailed results.
    /// </summary>
    /// <param name="candidates">The CSS class names to process.</param>
    /// <returns>A detailed result object containing CSS and processing information.</returns>
    internal CssFrameworkResult ProcessWithDetails(IEnumerable<string> candidates)
    {
        var combined = string.Join(" ", candidates.Where(s => !string.IsNullOrWhiteSpace(s)));
        return ProcessWithDetails(combined);
    }

    /// <summary>
    /// Processes multiple CSS class strings and returns the combined generated CSS.
    /// </summary>
    /// <param name="classStrings">The CSS class strings to process.</param>
    /// <returns>The combined generated CSS string.</returns>
    public string ProcessMultiple(params string[] classStrings)
    {
        if (classStrings.Length == 0)
        {
            return string.Empty;
        }

        var combined = string.Join(" ", classStrings.Where(s => !string.IsNullOrWhiteSpace(s)));
        return Process(combined);
    }

    private static void EnsureFontVariables(Dictionary<string, string> variables, Theme.Theme theme)
    {
        // Always include font-sans and font-mono
        if (!variables.ContainsKey("--font-sans"))
        {
            var fontSans = theme.ResolveValue("--font-sans", []) ??
                           "ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'";
            variables["--font-sans"] = fontSans;
        }

        if (!variables.ContainsKey("--font-mono"))
        {
            var fontMono = theme.ResolveValue("--font-mono", []) ??
                           "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace";
            variables["--font-mono"] = fontMono;
        }

        // Add default font family variables that reference the theme fonts
        variables["--default-font-family"] = "var(--font-sans)";
        variables["--default-mono-font-family"] = "var(--font-mono)";
    }
}