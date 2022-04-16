using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace MonorailCss.Css;

/// <summary>
/// A list of CSS declarations.
/// </summary>
public class CssDeclarationList : IEnumerable<CssDeclaration>
{
    private ImmutableDictionary<string, CssDeclaration> _declarations = ImmutableDictionary<string, CssDeclaration>.Empty;

    /// <summary>
    /// Adds a new item to the declaration list.
    /// </summary>
    /// <param name="declaration">The declaration to add.</param>
    public void Add(CssDeclaration declaration)
    {
        _declarations = _declarations.SetItem(declaration.Property, declaration);
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
    private ImmutableDictionary<CssSelector, CssRuleSet> _declarations = ImmutableDictionary<CssSelector, CssRuleSet>.Empty;

    /// <summary>
    /// Adds a new rule set to the list.
    /// </summary>
    /// <param name="ruleSet">The rule set to add.</param>
    public void Add(CssRuleSet ruleSet)
    {
        if (_declarations.TryGetValue(ruleSet.Selector, out var existingRuleSet))
        {
            _declarations = _declarations.SetItem(ruleSet.Selector, existingRuleSet + ruleSet);
        }
        else
        {
            _declarations = _declarations.Add(ruleSet.Selector, ruleSet);
        }
    }

    /// <inheritdoc />
    public IEnumerator<CssRuleSet> GetEnumerator()
    {
        return _declarations.Values.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _declarations.Values.GetEnumerator();
    }

    /// <summary>
    /// Adds two rule set lists together.
    /// </summary>
    /// <param name="ruleSet1">The first rule set.</param>
    /// <param name="ruleSet2">The second rule set.</param>
    /// <returns>A new rule set with the existing rules.</returns>
    public static CssRuleSetList operator +(CssRuleSetList ruleSet1, CssRuleSetList ruleSet2)
    {
        var newRuleSetList = new CssRuleSetList();
        foreach (var ruleSet in ruleSet1)
        {
            newRuleSetList.Add(ruleSet);
        }

        foreach (var ruleSet in ruleSet2)
        {
            newRuleSetList.Add(ruleSet);
        }

        return newRuleSetList;
    }
}