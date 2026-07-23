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
    // Encoder choice: UnsafeRelaxedJsonEscaping is the closest available match
    // to JS JSON.stringify, and it is byte-for-byte identical for ASCII input
    // (it does not escape <, >, &). It is NOT equivalent in general. Measured
    // differences against JSON.stringify, for a string member reaching the wire:
    //
    //   BMP non-ASCII (e.g. U+00E9, U+3042)
    //       emitted raw. Matches JSON.stringify.
    //   Astral-plane characters (e.g. U+1F600)
    //       escaped as a surrogate pair, "\uD83D\uDE00".
    //       JSON.stringify emits them raw. Different bytes.
    //   U+2028 / U+2029 (LINE / PARAGRAPH SEPARATOR)
    //       escaped as "\u2028" / "\u2029".
    //       JSON.stringify emits them raw. Different bytes.
    //   Unpaired surrogates
    //       replaced with U+FFFD (emitted as the escape "\uFFFD").
    //       JSON.stringify escapes them losslessly as "\uD800".
    //
    // The first three produce different BYTES but parse back to the same
    // string, so a Turnkey backend that parses the body sees what the caller
    // meant; only a byte-level comparison against a TS-produced body would
    // notice. The fourth is LOSSY — the original code point is gone and cannot
    // be recovered by parsing. Because this SDK signs the bytes it emits, a
    // lossy substitution would mean signing content the caller never supplied,
    // so Http rejects unpaired surrogates before serializing rather than
    // letting them reach this encoder (see the ill-formed UTF-16 gate in
    // Http.cs).
    //
    // None of this affects normal Turnkey flows: activity bodies carry UUIDs,
    // enum strings and hex, all ASCII.
    /// <summary>
    /// IL2CPP-safe System.Text.Json source-generated context for every DTO
    /// the SDK serializes. Set <c>Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping</c>
    /// on consumers if they need the closest available approximation of
    /// JS <c>JSON.stringify</c> escaping. The default options here are AOT-safe
    /// for the typical Turnkey wire content (ASCII UUIDs, enum strings, hex).
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
        /// Shared <see cref="JavaScriptEncoder"/>. Byte-identical to JS
        /// <c>JSON.stringify</c> for ASCII content; see the file header comment
        /// for the four measured divergences on non-ASCII content.
        /// </summary>
        public static readonly JavaScriptEncoder JsCompatibleEncoder =
            JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        /// <summary>
        /// Source-generated serializer options that route every supported
        /// type through this context AND apply
        /// <see cref="JsCompatibleEncoder"/>. Internal SDK serialization goes
        /// through this so that HTML-sensitive characters (<c>&lt;</c>,
        /// <c>&gt;</c>, <c>&amp;</c>) in user-controlled strings (organization
        /// IDs, private key names, etc.) are left unescaped, as JS
        /// <c>JSON.stringify</c> leaves them. Non-ASCII content is a closer
        /// approximation than the default encoder rather than an exact match —
        /// see the file header comment.
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
