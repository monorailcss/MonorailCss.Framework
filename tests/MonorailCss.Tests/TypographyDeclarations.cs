using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(
    typeof(MonorailCss.Tests.TypographyDeclarationsSerializer),
    typeof(MonorailCss.Tests.TypographyDeclarations))]

namespace MonorailCss.Tests;

/// <summary>
/// Wraps the per-selector property/value pairs from canonical-typography.json so they can be
/// passed through xunit.v3's TheoryDataRow. xunit warns (xUnit1047) when raw
/// <see cref="Dictionary{TKey,TValue}"/> instances are used because they aren't serializable
/// across discovery/execution boundaries.
/// </summary>
public sealed class TypographyDeclarations
{
    public Dictionary<string, string> Values { get; }

    public TypographyDeclarations() => Values = new Dictionary<string, string>();

    public TypographyDeclarations(Dictionary<string, string> values) => Values = values;
}

public sealed class TypographyDeclarationsSerializer : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedValue)
                     ?? new Dictionary<string, string>();
        return new TypographyDeclarations(values);
    }

    public string Serialize(object value)
    {
        var declarations = (TypographyDeclarations)value;
        return JsonSerializer.Serialize(declarations.Values);
    }

    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
    {
        if (type != typeof(TypographyDeclarations))
        {
            failureReason = $"{nameof(TypographyDeclarationsSerializer)} only handles {nameof(TypographyDeclarations)}.";
            return false;
        }

        failureReason = null;
        return true;
    }
}
