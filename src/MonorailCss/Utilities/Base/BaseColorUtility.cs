using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.DataTypes;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for utilities that resolve color values from the theme.
/// </summary>
internal abstract class BaseColorUtility : IUtility
{
    public virtual UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    /// <summary>
    /// Gets the utility pattern this handles (e.g., "bg" for background, "text" for text color).
    /// </summary>
    protected abstract string Pattern { get; }

    /// <summary>
    /// Gets the CSS property to set (e.g., "background-color", "color").
    /// </summary>
    protected abstract string CssProperty { get; }

    /// <summary>
    /// Gets the namespace chain for resolving color values.
    /// Override to provide utility-specific namespace chains.
    /// </summary>
    protected abstract string[] ColorNamespaces { get; }

    public virtual string[] GetNamespaces() => ColorNamespaces;

    public virtual bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != Pattern)
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        // For arbitrary values, check if it's actually a color
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var inferredType = DataTypeInference.InferDataType(
                functionalUtility.Value.Value,
                [DataType.Color, DataType.Length, DataType.LineWidth, DataType.Number]);
            if (inferredType != DataType.Color)
            {
                return false;
            }
        }

        if (!TryResolveColor(functionalUtility.Value, theme, out var color))
        {
            return false;
        }

        // Opacity modifiers are now handled by ColorModifierStage in the pipeline
        var declaration = CreateDeclaration(color, candidate.Important);
        results = ImmutableList.Create<AstNode>(declaration);
        return true;
    }

    /// <summary>
    /// Resolves a color value using the utility's namespace chain.
    /// </summary>
    protected virtual bool TryResolveColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        return ValueResolver.TryResolveColor(value, theme, ColorNamespaces, out color);
    }

    /// <summary>
    /// Creates the CSS declaration for this color utility.
    /// Override for utilities that need custom declaration logic.
    /// </summary>
    protected virtual Declaration CreateDeclaration(string color, bool important)
    {
        return new Declaration(CssProperty, color, important);
    }

    /// <summary>
    /// Returns examples of this color utility with theme-aware color values.
    /// Default implementation shows examples with common color shades (50, 500, 900).
    /// Override to provide custom color examples.
    /// </summary>
    public virtual IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>();

        // Show examples with common color shades from each color in theme
        var commonColors = new[] { "red", "blue", "green", "gray" };
        var commonShades = new[] { "50", "500", "900" };

        foreach (var colorName in commonColors)
        {
            foreach (var shade in commonShades)
            {
                var themeKey = $"--color-{colorName}-{shade}";
                if (theme.ContainsKey(themeKey))
                {
                    examples.Add(new Documentation.UtilityExample(
                        $"{Pattern}-{colorName}-{shade}",
                        $"Set {CssProperty} to {colorName} shade {shade}"));

                    // Only show one shade per color to keep examples concise
                    break;
                }
            }
        }

        // Show special colors
        if (theme.ContainsKey("--color-transparent"))
        {
            examples.Add(new Documentation.UtilityExample(
                $"{Pattern}-transparent",
                $"Set {CssProperty} to transparent"));
        }

        // Show arbitrary value support
        examples.Add(new Documentation.UtilityExample(
            $"{Pattern}-[#ff0000]",
            $"Set {CssProperty} with arbitrary color value"));

        // Show opacity modifier support
        examples.Add(new Documentation.UtilityExample(
            $"{Pattern}-red-500/50",
            $"Set {CssProperty} to red-500 with 50% opacity"));

        return examples.Take(10);
    }
}