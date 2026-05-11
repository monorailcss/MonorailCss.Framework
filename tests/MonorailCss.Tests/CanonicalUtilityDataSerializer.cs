using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(
    typeof(MonorailCss.Tests.CanonicalUtilityDataSerializer),
    typeof(MonorailCss.Tests.CanonicalUtilityData))]

namespace MonorailCss.Tests;

/// <summary>
/// JSON round-trip serializer for <see cref="CanonicalUtilityData"/> so xunit.v3 can pass
/// it through TheoryDataRow without xUnit1047 warnings. The type carries nested
/// <see cref="Dictionary{TKey,TValue}"/> and <see cref="List{T}"/> members that xunit's
/// built-in serializer can't traverse on its own.
/// </summary>
public sealed class CanonicalUtilityDataSerializer : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue) =>
        JsonSerializer.Deserialize(serializedValue, type)
        ?? throw new InvalidOperationException($"Failed to deserialize {type.Name} from '{serializedValue}'.");

    public string Serialize(object value) => JsonSerializer.Serialize(value);

    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
    {
        if (type != typeof(CanonicalUtilityData))
        {
            failureReason = $"{nameof(CanonicalUtilityDataSerializer)} only handles {nameof(CanonicalUtilityData)}.";
            return false;
        }

        failureReason = null;
        return true;
    }
}
