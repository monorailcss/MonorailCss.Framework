using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MonorailCss.SourceGenerator;

/// <summary>
/// Emits, at compile time, two artifacts for every concrete <c>IUtility</c> in the compilation
/// that has a parameterless constructor:
/// <list type="bullet">
/// <item><c>GeneratedUtilityRegistry.CreateAll()</c> — an explicit array of <c>new</c> calls,
/// replacing the reflection (<c>Assembly.GetTypes()</c> + <c>Activator.CreateInstance</c>) that
/// <c>UtilityDiscovery</c> used.</item>
/// <item><c>GeneratedUtilityMetadata</c> — a <c>FrozenDictionary&lt;Type, Entry&gt;</c> of
/// documentation metadata (category, summary, support flags), replacing the runtime XML-doc parse
/// and base-type reflection in <c>UtilityMetadata.FromUtilityType</c>.</item>
/// </list>
/// Both keep the assembly trim-safe and AOT-compatible and move work to build time.
/// </summary>
[Generator]
public sealed class UtilityRegistryGenerator : IIncrementalGenerator
{
    private const string IUtilityMetadataName = "MonorailCss.Utilities.IUtility";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var utilities = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => GetUtilityModel(ctx))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .Collect();

        context.RegisterSourceOutput(utilities, static (spc, models) => Emit(spc, models));
    }

    private static UtilityModel? GetUtilityModel(GeneratorSyntaxContext context)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        // Mirror the old reflection filter: concrete class, not abstract/static, implements
        // IUtility (directly or via a base class), and has an accessible parameterless constructor.
        if (symbol.TypeKind != TypeKind.Class || symbol.IsAbstract || symbol.IsStatic)
        {
            return null;
        }

        if (!ImplementsUtilityInterface(symbol))
        {
            return null;
        }

        if (!HasAccessibleParameterlessConstructor(symbol))
        {
            return null;
        }

        return new UtilityModel(
            symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            InferCategory(symbol),
            GenerateDescription(symbol),
            SupportsModifiers(symbol),
            SupportsArbitraryValues(symbol));
    }

    private static bool ImplementsUtilityInterface(INamedTypeSymbol symbol)
    {
        foreach (var iface in symbol.AllInterfaces)
        {
            if (iface.ToDisplayString() == IUtilityMetadataName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol symbol)
    {
        foreach (var ctor in symbol.InstanceConstructors)
        {
            if (ctor.Parameters.Length != 0)
            {
                continue;
            }

            // Generated code lives in the same assembly, so public/internal/protected-internal
            // parameterless constructors are all reachable (an undeclared ctor is implicitly public).
            if (ctor.DeclaredAccessibility is Accessibility.Public
                or Accessibility.Internal
                or Accessibility.ProtectedOrInternal)
            {
                return true;
            }
        }

        return false;
    }

    // Category is the namespace segment following "Utilities" (e.g. MonorailCss.Utilities.Layout
    // -> "Layout"), matching UtilityMetadata.InferCategory.
    private static string InferCategory(INamedTypeSymbol symbol)
    {
        var ns = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var parts = ns.Split('.');
        var index = Array.IndexOf(parts, "Utilities");
        if (index >= 0 && index < parts.Length - 1)
        {
            return parts[index + 1];
        }

        return "General";
    }

    // Prefer the type's XML <summary>; fall back to the same humanised name heuristic the runtime
    // used, so built-in descriptions are byte-for-byte what reflection produced — just computed once.
    private static string GenerateDescription(INamedTypeSymbol symbol)
    {
        var summary = TryGetXmlSummary(symbol);
        if (!string.IsNullOrEmpty(summary))
        {
            return summary!;
        }

        var name = symbol.Name;
        if (name.EndsWith("Utility", StringComparison.Ordinal))
        {
            name = name.Substring(0, name.Length - "Utility".Length);
        }

        var spaced = new StringBuilder(name.Length * 2);
        foreach (var ch in name)
        {
            if (char.IsUpper(ch) && spaced.Length > 0)
            {
                spaced.Append(' ');
            }

            spaced.Append(ch);
        }

        return $"Handles {spaced.ToString().Trim().ToLowerInvariant()} utilities";
    }

    private static string? TryGetXmlSummary(INamedTypeSymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        try
        {
            var summary = XDocument.Parse(xml).Descendants("summary").FirstOrDefault();
            if (summary == null)
            {
                return null;
            }

            // Mirror UtilityMetadata.CleanXmlDocText: trim each line, drop blanks, join with a space.
            var lines = summary.Value
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0);
            var cleaned = string.Join(" ", lines);
            return cleaned.Length == 0 ? null : cleaned;
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }

    private static bool SupportsModifiers(INamedTypeSymbol symbol) =>
        InheritsFrom(symbol, "BaseColorUtility");

    private static bool SupportsArbitraryValues(INamedTypeSymbol symbol) =>
        InheritsFrom(symbol, "BaseFunctionalUtility")
        || InheritsFrom(symbol, "BaseSpacingUtility")
        || InheritsFrom(symbol, "BaseFilterUtility")
        || InheritsFrom(symbol, "BaseColorUtility");

    private static bool InheritsFrom(INamedTypeSymbol symbol, string baseTypeName)
    {
        for (var current = symbol.BaseType; current is not null; current = current.BaseType)
        {
            if (current.Name == baseTypeName)
            {
                return true;
            }
        }

        return false;
    }

    private static void Emit(SourceProductionContext context, ImmutableArray<UtilityModel> models)
    {
        // Partial classes surface multiple syntax nodes; dedupe by name and sort for deterministic output.
        var distinct = models
            .GroupBy(m => m.FullyQualifiedName, System.StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(m => m.FullyQualifiedName, System.StringComparer.Ordinal)
            .ToList();

        context.AddSource("GeneratedUtilityRegistry.g.cs", EmitRegistry(distinct));
        context.AddSource("GeneratedUtilityMetadata.g.cs", EmitMetadata(distinct));
    }

    private static string EmitRegistry(List<UtilityModel> models)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace MonorailCss.Utilities;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Source-generated registration of every built-in <see cref=\"IUtility\"/>.");
        sb.AppendLine("/// Replaces reflection-based discovery so the assembly is trim- and AOT-safe.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class GeneratedUtilityRegistry");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Creates one instance of every discovered utility.</summary>");
        sb.AppendLine("    internal static global::MonorailCss.Utilities.IUtility[] CreateAll() =>");
        sb.AppendLine("    [");
        foreach (var model in models)
        {
            sb.Append("        new ").Append(model.FullyQualifiedName).AppendLine("(),");
        }

        sb.AppendLine("    ];");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EmitMetadata(List<UtilityModel> models)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Frozen;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("namespace MonorailCss.Utilities;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Source-generated documentation metadata for every built-in utility. Replaces the runtime");
        sb.AppendLine("/// XML-doc parse and base-type reflection in <see cref=\"global::MonorailCss.Documentation.UtilityMetadata\"/>.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class GeneratedUtilityMetadata");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Documentation facts known at compile time for a utility type.</summary>");
        sb.AppendLine("    internal readonly record struct Entry(string Category, string Description, bool SupportsModifiers, bool SupportsArbitraryValues);");
        sb.AppendLine();
        sb.AppendLine("    private static readonly FrozenDictionary<Type, Entry> Map = new Dictionary<Type, Entry>");
        sb.AppendLine("    {");
        foreach (var model in models)
        {
            sb.Append("        [typeof(").Append(model.FullyQualifiedName).Append(")] = new Entry(")
              .Append(SymbolDisplay.FormatLiteral(model.Category, quote: true)).Append(", ")
              .Append(SymbolDisplay.FormatLiteral(model.Description, quote: true)).Append(", ")
              .Append(model.SupportsModifiers ? "true" : "false").Append(", ")
              .Append(model.SupportsArbitraryValues ? "true" : "false").AppendLine("),");
        }

        sb.AppendLine("    }.ToFrozenDictionary();");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Looks up compile-time metadata for a built-in utility type.</summary>");
        sb.AppendLine("    internal static bool TryGet(Type type, out Entry entry) => Map.TryGetValue(type, out entry);");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private sealed record UtilityModel(
        string FullyQualifiedName,
        string Category,
        string Description,
        bool SupportsModifiers,
        bool SupportsArbitraryValues);
}
