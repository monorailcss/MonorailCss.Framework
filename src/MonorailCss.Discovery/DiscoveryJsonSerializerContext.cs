using System.Text.Json.Serialization;

namespace MonorailCss.Discovery;

/// <summary>
/// System.Text.Json source-generation context for the discovery diagnostics endpoint. Using a
/// generated context (rather than reflection-based <c>JsonSerializer.Serialize</c>) keeps the
/// diagnostics endpoint trim- and AOT-safe.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DiagnosticsSnapshot))]
internal sealed partial class DiscoveryJsonSerializerContext : JsonSerializerContext;
