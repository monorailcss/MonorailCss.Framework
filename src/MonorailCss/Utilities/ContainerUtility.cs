using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities;

/// <summary>
/// Utility for CSS container layout with responsive max-widths.
/// Handles: container
/// CSS: width: 100% with progressive max-width constraints at breakpoints.
/// </summary>
internal class ContainerUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // We only define container here so it's recognized by the parser
            // The actual CSS generation is done in TryCompile override
            { "container", ("width", "100%") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility || staticUtility.Root != "container")
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Base width: 100%
        declarations.Add(new Declaration("width", "100%", candidate.Important));

        // Get all breakpoint values from theme and sort them
        var breakpoints = new List<(string Name, string Value)>();

        foreach (var breakpointName in new[] { "sm", "md", "lg", "xl", "2xl" })
        {
            var themeKey = $"--breakpoint-{breakpointName}";
            var breakpointValue = theme.ResolveValue(null, [themeKey]);
            if (breakpointValue != null)
            {
                breakpoints.Add((breakpointName, breakpointValue));
            }
        }

        // Generate media queries for each breakpoint with max-width constraints
        foreach (var (_, value) in breakpoints)
        {
            var mediaQuery = $"(width >= {value})";
            var maxWidthDeclaration = new Declaration("max-width", value, candidate.Important);
            var atRule = new AtRule("media", mediaQuery, ImmutableList.Create<AstNode>(maxWidthDeclaration));
            declarations.Add(atRule);
        }

        results = declarations.ToImmutable();
        return true;
    }
}