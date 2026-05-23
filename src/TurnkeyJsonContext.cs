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

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Turnkey
{
    // Encoder choice: UnsafeRelaxedJsonEscaping makes System.Text.Json
    // output equivalent to JS JSON.stringify for ASCII-safe inputs and
    // closer to JS behavior for non-ASCII inputs (does NOT escape <, >, &,
    // most non-ASCII Unicode). Turnkey activity bodies contain ASCII-only
    // content (UUIDs, enum strings, hex), so in practice this only affects
    // theoretical wire-format parity for atypical content; it does not
    // affect normal Turnkey flows.
    /// <summary>
    /// IL2CPP-safe System.Text.Json source-generated context for every DTO
    /// the SDK serializes. Set <c>Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping</c>
    /// on consumers if they need maximum JS-stringify parity. The default
    /// options here are AOT-safe for the typical Turnkey wire content
    /// (ASCII UUIDs, enum strings, hex).
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
    [JsonSerializable(typeof(ApiKeyStamper.TurnkeyStamp))]
    [JsonSerializable(typeof(Http.WhoamiRequestBody))]
    [JsonSerializable(typeof(Http.InitImportPrivateKeyRequestBody))]
    [JsonSerializable(typeof(Http.InitImportPrivateKeyParameters))]
    [JsonSerializable(typeof(Http.ImportPrivateKeyRequestBody))]
    [JsonSerializable(typeof(Http.ImportPrivateKeyParameters))]
    [JsonSerializable(typeof(Http.ExportPrivateKeyRequestBody))]
    [JsonSerializable(typeof(Http.ExportPrivateKeyParameters))]
    [JsonSerializable(typeof(Http.ExportWalletAccountRequestBody))]
    [JsonSerializable(typeof(Http.ExportWalletAccountParameters))]
    [JsonSerializable(typeof(Http.SignedRequest))]
    [JsonSerializable(typeof(Http.Stamp))]
    public partial class TurnkeyJsonContext : JsonSerializerContext
    {
        /// <summary>
        /// Shared <see cref="JavaScriptEncoder"/> matching JS
        /// <c>JSON.stringify</c> escaping behavior.
        /// </summary>
        public static readonly JavaScriptEncoder JsCompatibleEncoder =
            JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        /// <summary>
        /// Source-generated serializer options that route every supported
        /// type through this context AND apply
        /// <see cref="JsCompatibleEncoder"/>. Internal SDK serialization
        /// goes through this so that non-ASCII / HTML-sensitive characters
        /// in user-controlled strings (organization IDs, private key names,
        /// etc.) are escaped the same way JS <c>JSON.stringify</c> does.
        /// </summary>
        public static readonly System.Text.Json.JsonSerializerOptions JsCompatibleOptions =
            new System.Text.Json.JsonSerializerOptions
            {
                TypeInfoResolver = Default,
                Encoder = JsCompatibleEncoder,
                WriteIndented = false,
            };
    }
}
