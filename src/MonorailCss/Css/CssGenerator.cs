using System.Collections.Immutable;
using System.Text;
using MonorailCss.Ast;

namespace MonorailCss.Css;

internal sealed class CssGenerator
{
    // The @supports rule detects browsers that don't support @property declarations
    // - Safari < 16.4: has -webkit-hyphens but not margin-trim:inline
    // - Firefox < 128: has -moz-orient but not color:rgb(from red r g b)
    private const string PropertyFallbackSupportsCondition =
        "((-webkit-hyphens: none) and (not (margin-trim: inline))) or " +
        "((-moz-orient: inline) and (not (color:rgb(from red r g b))))";

    public string GenerateCss(
        ImmutableList<AstNode> nodes,
        Dictionary<string, string>? themeVariables = null,
        CssPropertyRegistry? propertyRegistry = null,
        bool includeComments = false,
        string? preflightCss = null,
        List<AstNode>? componentNodes = null)
    {
        if (nodes.IsEmpty && (themeVariables == null || themeVariables.Count == 0) && (propertyRegistry == null || propertyRegistry.Count == 0) && string.IsNullOrWhiteSpace(preflightCss))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        // Add layer declarations at the top (Tailwind v4 style)
        sb.AppendLine("@layer theme, base, components, utilities;");

        // Collect nodes by layer
        var (themeNodes, baseNodes, utilityNodes) = CollectLayerNodes(nodes, includeComments);

        // Add theme variables to theme layer
        if (themeVariables != null && themeVariables.Count > 0)
        {
            var rootDeclarations = ImmutableList.CreateBuilder<AstNode>();
            foreach (var (key, value) in themeVariables.OrderBy(kvp => kvp.Key))
            {
                rootDeclarations.Add(new Declaration(key, value));
            }

            if (rootDeclarations.Count > 0)
            {
                themeNodes.Insert(0, new StyleRule(":root, :host", rootDeclarations.ToImmutable()));
            }
        }

        // Add preflight CSS to base layer if provided
        if (!string.IsNullOrWhiteSpace(preflightCss))
        {
            // Parse the preflight CSS and add to base nodes at the beginning
            var preflightNode = new RawCss(preflightCss);
            baseNodes.Insert(0, preflightNode);
        }

        // Emit layers
        EmitLayer(sb, "theme", themeNodes);
        EmitLayer(sb, "base", baseNodes);
        EmitLayer(sb, "components", componentNodes ?? new List<AstNode>());
        EmitLayer(sb, "utilities", utilityNodes);

        // Generate @property declarations if property registry is provided
        if (propertyRegistry != null && propertyRegistry.Count > 0)
        {
            EmitProperties(sb, propertyRegistry);
        }

        return sb.ToString().TrimEnd();
    }

    public string GenerateCss(AstNode node, bool includeComments = false)
    {
        if (!includeComments && node is Comment)
        {
            return string.Empty;
        }

        return node.ToCss();
    }

    private static (List<AstNode> Theme, List<AstNode> Base, List<AstNode> Utilities) CollectLayerNodes(
        ImmutableList<AstNode> nodes,
        bool includeComments)
    {
        var themeNodes = new List<AstNode>();
        var baseNodes = new List<AstNode>();
        var utilityNodes = new List<AstNode>();

        if (nodes.IsEmpty)
        {
            return (themeNodes, baseNodes, utilityNodes);
        }

        foreach (var node in nodes)
        {
            if (!includeComments && node is Comment)
            {
                continue;
            }

            if (node is AtRule atRule && atRule.Name == "layer")
            {
                switch (atRule.Params.Trim())
                {
                    case "theme":
                        themeNodes.AddRange(atRule.Nodes);
                        break;
                    case "base":
                        baseNodes.AddRange(atRule.Nodes);
                        break;
                    case "utilities":
                        utilityNodes.AddRange(atRule.Nodes);
                        break;
                    default:
                        utilityNodes.Add(node);
                        break;
                }
            }
            else
            {
                utilityNodes.Add(node);
            }
        }

        return (themeNodes, baseNodes, utilityNodes);
    }

    private static void EmitLayer(StringBuilder sb, string layerName, List<AstNode> nodes)
    {
        if (nodes.Count == 0)
        {
            return;
        }

        if (sb.Length > 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine($"@layer {layerName} {{");
        foreach (var node in nodes)
        {
            sb.AppendLine(node.ToCss(1));
        }

        sb.AppendLine("}");
    }

    private static void EmitProperties(StringBuilder sb, CssPropertyRegistry propertyRegistry)
    {
        if (sb.Length > 0)
        {
            sb.AppendLine();
        }

        var properties = propertyRegistry.GetAll().OrderBy(p => p.Name).ToList();
        foreach (var property in properties)
        {
            sb.AppendLine($"@property {property.Name} {{");
            sb.AppendLine($"  syntax: \"{property.Syntax}\";");
            sb.AppendLine($"  inherits: {(property.Inherits ? "true" : "false")};");

            // Only output initial-value if it's not null
            if (property.InitialValue != null)
            {
                sb.AppendLine($"  initial-value: {property.InitialValue};");
            }

            sb.AppendLine("}");
        }

        // Generate properties layer with fallback initialization
        var fallbackProperties = propertyRegistry.GetPropertiesNeedingFallback().ToList();
        if (fallbackProperties.Count > 0)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine("@layer properties {");
            sb.AppendLine($"  @supports {PropertyFallbackSupportsCondition} {{");
            sb.AppendLine("    *, ::before, ::after, ::backdrop {");

            foreach (var property in fallbackProperties.OrderBy(p => p.Name))
            {
                // Only output if InitialValue is not null (should be guaranteed by GetPropertiesNeedingFallback)
                if (property.InitialValue != null)
                {
                    sb.AppendLine($"      {property.Name}: {property.InitialValue};");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }
    }
}