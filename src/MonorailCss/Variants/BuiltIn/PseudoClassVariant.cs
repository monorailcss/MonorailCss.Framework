using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant that applies pseudo-class selectors like :hover, :focus, etc.
/// </summary>
internal class PseudoClassVariant : IVariant
{
    private readonly string _pseudoClass;

    public PseudoClassVariant(string name, string pseudoClass, int weight)
    {
        Name = name;
        _pseudoClass = pseudoClass;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Apply pseudo-class to the selector
        result = current.TransformSelector(s => s.WithPseudo(_pseudoClass));
        return true;
    }
}

/// <summary>
/// Variant that applies pseudo-element selectors like ::before, ::after, etc.
/// </summary>
internal class PseudoElementVariant : IVariant
{
    private readonly string _pseudoElement;
    private readonly ImmutableList<Declaration> _extraDeclarations;

    public PseudoElementVariant(string name, string pseudoElement, int weight)
        : this(name, pseudoElement, weight, ImmutableList<Declaration>.Empty)
    {
    }

    public PseudoElementVariant(string name, string pseudoElement, int weight, ImmutableList<Declaration> extraDeclarations)
    {
        Name = name;
        _pseudoElement = pseudoElement;
        Weight = weight;
        _extraDeclarations = extraDeclarations;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // Apply pseudo-element to the selector
        result = current.TransformSelector(s => s.WithPseudo(_pseudoElement));
        return true;
    }

    public ImmutableList<AstNode> TransformNodes(ImmutableList<AstNode> nodes)
    {
        if (_extraDeclarations.IsEmpty)
        {
            return nodes;
        }

        // Skip injection for any property already set by the utility (e.g. content-['x']
        // explicitly sets `content`, so we don't want to overwrite it with var(--tw-content)).
        var existingProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in nodes)
        {
            if (node is Declaration declaration)
            {
                existingProperties.Add(declaration.Property);
            }
        }

        var builder = ImmutableList.CreateBuilder<AstNode>();
        foreach (var declaration in _extraDeclarations)
        {
            if (!existingProperties.Contains(declaration.Property))
            {
                builder.Add(declaration);
            }
        }

        if (builder.Count == 0)
        {
            return nodes;
        }

        builder.AddRange(nodes);
        return builder.ToImmutable();
    }
}