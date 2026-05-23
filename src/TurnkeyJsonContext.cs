// IL2CPP-safe System.Text.Json source-generated context.
//
// Every JsonSerializer.Serialize / JsonSerializer.Deserialize call in this SDK
// MUST go through this context's typed overloads (TurnkeyJsonContext.Default.<Type>)
// so that no reflection-based fallback is reached at runtime. The context
// covers every DTO that crosses a wire boundary or that we serialize to JSON.
//
// IL2CPP / AOT safety:
//   - JsonSerializerIsReflectionEnabledByDefault is the user/runtime knob
//     that controls whether fallback to reflection happens. We do not set it
//     here, but every call site we own goes via this context and so never
//     needs reflection.
//
// When adding a new DTO that this SDK serializes:
//   1. Add a [JsonSerializable(typeof(<DTO>))] attribute below.
//   2. Use the resulting TurnkeyJsonContext.Default.<DTO> overload at the
//      call site.

using System.Text.Json.Serialization;

namespace Turnkey
{
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
    [JsonSerializable(typeof(ApiKeyStamper.TurnkeyStamp))]
    public partial class TurnkeyJsonContext : JsonSerializerContext
    {
    }
}
