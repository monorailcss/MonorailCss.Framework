using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Theme;

namespace MonorailCss.Pipeline.Stages;

/// <summary>
/// Pipeline stage that automatically tracks theme variable usage by scanning AstNodes
/// for CSS variable references and marking them as used in the theme tracker.
/// </summary>
internal class ThemeVariableTrackingStage : IPipelineStage
{
    private readonly Theme.Theme _theme;
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

        // Find all CSS variable references using a paren-counting parser to handle nested var() calls
        var index = 0;
        while (index < value.Length)
        {
            var varIndex = value.IndexOf("var(", index, StringComparison.Ordinal);
            if (varIndex == -1)
            {
                break;
            }

            // Find the matching closing paren by counting nesting level
            var startIndex = varIndex + 4; // after "var("
            var parenCount = 1;
            var endIndex = startIndex;

            while (endIndex < value.Length && parenCount > 0)
            {
                if (value[endIndex] == '(')
                {
                    parenCount++;
                }
                else if (value[endIndex] == ')')
                {
                    parenCount--;
                }

                endIndex++;
            }

            if (parenCount == 0)
            {
                // Successfully found matching closing paren
                var variableContent = value.Substring(startIndex, endIndex - startIndex - 1).Trim();

                // Handle fallback values: var(--color-red-500, #ef4444)
                // For nested var(), we need to find the first comma that's not inside nested parens
                var commaIndex = FindFirstCommaOutsideParens(variableContent);

                if (commaIndex >= 0)
                {
                    var variableName = variableContent.Substring(0, commaIndex).Trim();
                    var fallbackValue = variableContent.Substring(commaIndex + 1).Trim();

                    // Mark the variable as used in the tracker
                    if (variableName.StartsWith("--") && _tracker != null)
                    {
                        _tracker.MarkUsed(variableName);

                        // Recursively track variables in fallback values
                        TrackVariablesInValue(fallbackValue);
                    }
                }
                else
                {
                    // No fallback, just the variable name
                    var variableName = variableContent.Trim();
                    if (variableName.StartsWith("--") && _tracker != null)
                    {
                        _tracker.MarkUsed(variableName);
                    }
                }
            }

            // Move past this var() call
            index = endIndex;
        }
    }

    private static int FindFirstCommaOutsideParens(string value)
    {
        var parenCount = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '(')
            {
                parenCount++;
            }
            else if (value[i] == ')')
            {
                parenCount--;
            }
            else if (value[i] == ',' && parenCount == 0)
            {
                return i;
            }
        }

        return -1;
    }
}