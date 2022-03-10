using System.Collections;
using System.Collections.Concurrent;

namespace MonorailCss.Css;

/// <summary>
/// A list of CSS declarations.
/// </summary>
public class CssDeclarationList : IEnumerable<CssDeclaration>
{
    private readonly ConcurrentDictionary<string, CssDeclaration> _declarations = new();

    /// <summary>
    /// Adds a new item to the declaration list.
    /// </summary>
    /// <param name="declaration">The declaration to add.</param>
    public void Add(CssDeclaration declaration)
    {
        _declarations.AddOrUpdate(declaration.Property, _ => declaration, (_, _) => declaration);
    }

    /// <summary>
    /// Adds a list of declarations together.
    /// </summary>
    /// <param name="declarationList">The declarations to add. They will overwrite existing values.</param>
    public void AddRange(CssDeclarationList declarationList)
    {
        foreach (var declaration in declarationList)
        {
            Add(declaration);
        }
    }

    /// <summary>
    /// Gets the number of declarations.
    /// </summary>
    /// <returns>The number of declarations.</returns>
    public int Count
    {
        get
        {
            return _declarations.Count;
        }
    }

    /// <inheritdoc />
    public IEnumerator<CssDeclaration> GetEnumerator()
    {
        return _declarations.Values.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _declarations.Values.GetEnumerator();
    }

    /// <summary>
    /// Adds two CSS declaration lists and returns a new one.
    /// </summary>
    /// <param name="list1">The first list.</param>
    /// <param name="list2">The second list. Values will overwrite the second.</param>
    /// <returns>A new CSS declaration list.</returns>
    public static CssDeclarationList operator +(CssDeclarationList list1, CssDeclarationList list2)
    {
        var newList = new CssDeclarationList();
        newList.AddRange(list1);
        newList.AddRange(list2);
        return newList;
    }
}

/// <summary>
/// A list of CSS rule sets.
/// </summary>
public class CssRuleSetList : IEnumerable<CssRuleSet>
{
    private readonly List<CssRuleSet> _declarations = new();

    /// <summary>
    /// Adds a new rule set to the list.
    /// </summary>
    /// <param name="ruleSet">The rule set to add.</param>
    public void Add(CssRuleSet ruleSet) => _declarations.Add(ruleSet);

    /// <inheritdoc />
    public IEnumerator<CssRuleSet> GetEnumerator()
    {
        return _declarations.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _declarations.GetEnumerator();
    }
}