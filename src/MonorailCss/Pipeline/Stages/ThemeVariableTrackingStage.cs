using System.Collections.Immutable;
using System.Text.RegularExpressions;
using MonorailCss.Ast;
using MonorailCss.Theme;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically tracks theme variable usage by scanning AstNodes
/// for CSS variable references and marking them as used in the theme tracker.
/// </summary>
internal partial class ThemeVariableTrackingStage : IPipelineStage
{
    private readonly Theme.Theme _theme;
    private static readonly Regex _cssVariablePattern = CssVariablePatternDefinition();
    private ThemeUsageTracker? _tracker;

    public string Name => "Theme Variable Tracking";

    public ThemeVariableTrackingStage(Theme.Theme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public ImmutableList<AstNode> Process(ImmutableList<AstNode> nodes, PipelineContext context)
    {
        // Get the tracker from the context
        if (context.Metadata.TryGetValue("themeTracker", out var trackerObj) &&
            trackerObj is ThemeUsageTracker tracker)
        {
            _tracker = tracker;
        }
        else
        {
            // If no tracker in context, create a temporary one (for backward compatibility)
            _tracker = new ThemeUsageTracker(_theme);
        }

        // Track variables from all nodes
        foreach (var node in nodes)
        {
            TrackVariablesInNode(node);
        }

        // Also track from processed classes if available
        if (context.Metadata.TryGetValue("processedClasses", out var classesObj) &&
            classesObj is List<ProcessedClass> processedClasses)
        {
            foreach (var processedClass in processedClasses)
            {
                foreach (var astNode in processedClass.AstNodes)
                {
                    TrackVariablesInNode(astNode);
                }
            }
        }

        // Return nodes unchanged - this stage only tracks variables
        return nodes;
    }

    private void TrackVariablesInNode(AstNode node)
    {
        switch (node)
        {
            case Declaration declaration:
                TrackVariablesInValue(declaration.Value);
                break;

            case StyleRule styleRule:
                foreach (var child in styleRule.Nodes)
                {
                    TrackVariablesInNode(child);
                }

                break;

            case NestedRule nestedRule:
                foreach (var child in nestedRule.Nodes)
                {
                    TrackVariablesInNode(child);
                }

                break;

            case AtRule atRule:
                // Track variables in at-rule parameters (e.g., media queries)
                TrackVariablesInValue(atRule.Params);
                foreach (var child in atRule.Nodes)
                {
                    TrackVariablesInNode(child);
                }

                break;

            case Context contextNode:
                foreach (var child in contextNode.Nodes)
                {
                    TrackVariablesInNode(child);
                }

                break;

            case RawCss rawCss:
                TrackVariablesInValue(rawCss.Content);
                break;
        }
    }

    private void TrackVariablesInValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        // Find all CSS variable references
        var matches = _cssVariablePattern.Matches(value);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var variableContent = match.Groups[1].Value.Trim();

                // Handle fallback values: var(--color-red-500, #ef4444)
                var parts = variableContent.Split(',');
                if (parts.Length > 0)
                {
                    var variableName = parts[0].Trim();

                    // Mark the variable as used in the tracker
                    if (variableName.StartsWith("--") && _tracker != null)
                    {
                        _tracker.MarkUsed(variableName);

                        // Recursively track variables in fallback values
                        if (parts.Length > 1)
                        {
                            var fallbackValue = string.Join(",", parts.Skip(1)).Trim();
                            TrackVariablesInValue(fallbackValue);
                        }
                    }
                }
            }
        }
    }

    [GeneratedRegex(@"var\(([^)]+)\)", RegexOptions.Compiled)]
    private static partial Regex CssVariablePatternDefinition();
}