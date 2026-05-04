using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MonorailCss.Docs.Services;

/// <summary>
/// Parses the customer-facing public surface of MonorailCss directly from source
/// using Roslyn syntax trees, so descriptions track the xmldoc that ships with the
/// library. Cached at construction; the underlying source files don't move at runtime.
/// </summary>
public sealed partial class ApiReferenceService
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private readonly Lazy<ImmutableList<ApiTypeDoc>> _types;

    public ApiReferenceService(IHostEnvironment env)
    {
        _types = new Lazy<ImmutableList<ApiTypeDoc>>(() => LoadTypes(env.ContentRootPath));
    }

    public ImmutableList<ApiTypeDoc> Types => _types.Value;

    private static ImmutableList<ApiTypeDoc> LoadTypes(string contentRoot)
    {
        // docs/MonorailCss.Docs -> ../../src/MonorailCss
        var srcRoot = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "src", "MonorailCss"));

        var sources = new[]
        {
            Path.Combine(srcRoot, "CssFramework.cs"),
            Path.Combine(srcRoot, "CssFrameworkSettings.cs"),
        };

        // Type names we expose, in the order we want them on the page.
        var wantedOrder = new[]
        {
            "CssFramework",
            "CssFrameworkSettings",
            "ColorEmissionMode",
            "CustomVariantDefinition",
        };

        var byName = new Dictionary<string, ApiTypeDoc>(StringComparer.Ordinal);

        foreach (var path in sources)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path));
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var fileNamespace = root.DescendantNodes()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .FirstOrDefault()?.Name.ToString() ?? "MonorailCss";

            foreach (var member in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                if (member is not (ClassDeclarationSyntax or RecordDeclarationSyntax or EnumDeclarationSyntax))
                {
                    continue;
                }

                if (!IsPublic(member.Modifiers))
                {
                    continue;
                }

                var name = member.Identifier.Text;
                if (!wantedOrder.Contains(name))
                {
                    continue;
                }

                byName[name] = ExtractType(member, fileNamespace, path);
            }
        }

        return wantedOrder
            .Where(n => byName.ContainsKey(n))
            .Select(n => byName[n])
            .ToImmutableList();
    }

    private static ApiTypeDoc ExtractType(BaseTypeDeclarationSyntax decl, string ns, string sourcePath)
    {
        var name = decl.Identifier.Text;
        var (kindCode, kindLabel) = decl switch
        {
            EnumDeclarationSyntax => ("E", "Enum"),
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => ("S", "Record struct"),
            RecordDeclarationSyntax => ("C", "Record"),
            ClassDeclarationSyntax c when c.Modifiers.Any(SyntaxKind.StaticKeyword) => ("C", "Static class"),
            ClassDeclarationSyntax => ("C", "Class"),
            _ => ("S", "Struct"),
        };

        var summary = ExtractSummary(decl);
        var properties = ImmutableList.CreateBuilder<ApiPropertyDoc>();
        var methods = ImmutableList.CreateBuilder<ApiMethodDoc>();
        var values = ImmutableList.CreateBuilder<ApiEnumValueDoc>();

        if (decl is TypeDeclarationSyntax type)
        {
            // Constructors (only public ones — these are what consumers call).
            foreach (var ctor in type.Members.OfType<ConstructorDeclarationSyntax>())
            {
                if (!IsPublic(ctor.Modifiers))
                {
                    continue;
                }

                methods.Add(new ApiMethodDoc(
                    Name: ctor.Identifier.Text,
                    Params: ExtractParams(ctor.ParameterList),
                    ReturnType: string.Empty,
                    IsConstructor: true,
                    Tags: ExtractMethodTags(ctor.Modifiers),
                    Summary: ExtractSummary(ctor)));
            }

            foreach (var prop in type.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (!IsPublic(prop.Modifiers))
                {
                    continue;
                }

                properties.Add(new ApiPropertyDoc(
                    Name: prop.Identifier.Text,
                    Type: prop.Type.ToString(),
                    Tags: ExtractPropertyTags(prop),
                    Summary: ExtractSummary(prop)));
            }

            foreach (var method in type.Members.OfType<MethodDeclarationSyntax>())
            {
                if (!IsPublic(method.Modifiers))
                {
                    continue;
                }

                methods.Add(new ApiMethodDoc(
                    Name: method.Identifier.Text,
                    Params: ExtractParams(method.ParameterList),
                    ReturnType: method.ReturnType.ToString(),
                    IsConstructor: false,
                    Tags: ExtractMethodTags(method.Modifiers),
                    Summary: ExtractSummary(method)));
            }
        }
        else if (decl is EnumDeclarationSyntax enumDecl)
        {
            foreach (var v in enumDecl.Members)
            {
                values.Add(new ApiEnumValueDoc(
                    Name: v.Identifier.Text,
                    Value: v.EqualsValue?.Value.ToString(),
                    Summary: ExtractSummary(v)));
            }
        }

        // Source path relative to repo root (the docs project lives 2 dirs deep).
        var relSource = sourcePath.Replace('\\', '/');
        var srcIdx = relSource.IndexOf("/src/", StringComparison.Ordinal);
        if (srcIdx >= 0)
        {
            relSource = relSource[(srcIdx + 1)..];
        }

        return new ApiTypeDoc(
            Id: name,
            Name: name,
            Namespace: ns,
            Kind: kindCode,
            KindLabel: kindLabel,
            Summary: summary,
            Properties: properties.ToImmutable(),
            Methods: methods.ToImmutable(),
            EnumValues: values.ToImmutable(),
            SourcePath: relSource);
    }

    private static ImmutableList<ApiParamDoc> ExtractParams(ParameterListSyntax list) =>
        list.Parameters
            .Select(p => new ApiParamDoc(
                Name: p.Identifier.Text,
                Type: p.Type?.ToString() ?? string.Empty))
            .ToImmutableList();

    private static ImmutableList<string> ExtractPropertyTags(PropertyDeclarationSyntax prop)
    {
        var tags = ImmutableList.CreateBuilder<string>();
        if (prop.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            tags.Add("static");
        }

        var accessors = prop.AccessorList?.Accessors;
        if (accessors is not null)
        {
            var hasInit = accessors.Value.Any(a => a.IsKind(SyntaxKind.InitAccessorDeclaration));
            var hasSet = accessors.Value.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
            if (hasInit)
            {
                tags.Add("init");
            }
            else if (!hasSet)
            {
                tags.Add("readonly");
            }
        }

        return tags.ToImmutable();
    }

    private static ImmutableList<string> ExtractMethodTags(SyntaxTokenList modifiers)
    {
        var tags = ImmutableList.CreateBuilder<string>();
        if (modifiers.Any(SyntaxKind.StaticKeyword))
        {
            tags.Add("static");
        }

        if (modifiers.Any(SyntaxKind.AsyncKeyword))
        {
            tags.Add("async");
        }

        if (modifiers.Any(SyntaxKind.VirtualKeyword))
        {
            tags.Add("virtual");
        }

        return tags.ToImmutable();
    }

    private static bool IsPublic(SyntaxTokenList modifiers) =>
        modifiers.Any(SyntaxKind.PublicKeyword);

    private static string ExtractSummary(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        if (trivia is null)
        {
            return string.Empty;
        }

        // Build the raw inner XML by stripping the leading "/// " from each line, then
        // wrap in a synthetic root so XDocument can parse the fragments (<summary>, <param>, …).
        var sb = new StringBuilder();
        foreach (var line in trivia.ToFullString().Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("///", StringComparison.Ordinal))
            {
                trimmed = trimmed[3..];
                if (trimmed.StartsWith(' '))
                {
                    trimmed = trimmed[1..];
                }
            }

            sb.AppendLine(trimmed);
        }

        var wrapped = "<doc>" + sb + "</doc>";
        XDocument xd;
        try
        {
            xd = XDocument.Parse(wrapped);
        }
        catch
        {
            return string.Empty;
        }

        var summary = xd.Root?.Element("summary");
        if (summary is null)
        {
            return string.Empty;
        }

        // Flatten to plain text. Preserve <c>code</c> but drop the rest.
        var text = string.Concat(summary.Nodes().Select(NodeToText));
        return CollapseWhitespace(text);
    }

    private static string NodeToText(XNode node) => node switch
    {
        XText t => t.Value,
        XElement el when el.Name.LocalName == "c" => "`" + el.Value + "`",
        XElement el when el.Name.LocalName == "see" =>
            el.Attribute("cref")?.Value is { } cref ? "`" + StripCref(cref) + "`" : string.Empty,
        XElement el => string.Concat(el.Nodes().Select(NodeToText)),
        _ => string.Empty,
    };

    private static string StripCref(string cref)
    {
        // T:MonorailCss.CssFramework -> CssFramework
        var colon = cref.IndexOf(':');
        if (colon >= 0)
        {
            cref = cref[(colon + 1)..];
        }

        var lastDot = cref.LastIndexOf('.');
        return lastDot >= 0 ? cref[(lastDot + 1)..] : cref;
    }

    private static string CollapseWhitespace(string s) =>
        WhitespaceRegex().Replace(s.Trim(), " ");
}

public record ApiTypeDoc(
    string Id,
    string Name,
    string Namespace,
    string Kind,
    string KindLabel,
    string Summary,
    ImmutableList<ApiPropertyDoc> Properties,
    ImmutableList<ApiMethodDoc> Methods,
    ImmutableList<ApiEnumValueDoc> EnumValues,
    string SourcePath);

public record ApiPropertyDoc(
    string Name,
    string Type,
    ImmutableList<string> Tags,
    string Summary);

public record ApiMethodDoc(
    string Name,
    ImmutableList<ApiParamDoc> Params,
    string ReturnType,
    bool IsConstructor,
    ImmutableList<string> Tags,
    string Summary);

public record ApiParamDoc(string Name, string Type);

public record ApiEnumValueDoc(string Name, string? Value, string Summary);
