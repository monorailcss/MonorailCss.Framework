using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for utilities that have static predefined values.
/// </summary>
internal abstract class BaseStaticUtility : IUtility
{
    public virtual UtilityPriority Priority => UtilityPriority.ExactStatic;

    /// <summary>
    /// Gets map of utility names to their CSS property and value.
    /// </summary>
    protected abstract ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; }

    /// <summary>
    /// Static utilities typically don't use theme namespaces.
    /// </summary>
    public virtual string[] GetNamespaces() => [];

    public virtual bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (!StaticValues.TryGetValue(staticUtility.Root, out var cssDeclaration))
        {
            return false;
        }

        var declaration = new Declaration(
            cssDeclaration.Property,
            cssDeclaration.Value,
            candidate.Important);

        results = ImmutableList.Create<AstNode>(declaration);
        return true;
    }

    /// <summary>
    /// Gets all utility names this static utility handles.
    /// </summary>
    public virtual IEnumerable<string> GetUtilityNames()
    {
        return StaticValues.Keys;
    }
}