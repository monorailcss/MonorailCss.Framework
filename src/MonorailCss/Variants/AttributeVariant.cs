namespace MonorailCss.Variants;

/// <summary>
/// Represents a variant that applies an attribute selector after the element e.g. data-[state=open]:bg-red becomes .data-\[state\=open\]\:bg-red [data-state="open"].
/// </summary>
/// <param name="AttributeSelector">The attribute selector to apply.</param>
public record AttributeVariant(string AttributeSelector) : IVariant;