// Logical compatibility port of the supported request-signing subset of
// @turnkey/http@3.16.0.
//
// Upstream snapshot:
//   tests/UpstreamSources/turnkey-http-3.16.0/
//
// Scope:
//   The upstream package is a full client (auto-generated activity
//   methods, polling, error handling, WebAuthn). This library exposes the
//   **request-signing** subset: build a request body,
//   stamp it with an ApiKeyStamper, return a { url, body, stamp }
//   bundle for the caller to send over HTTPS.
//
// Activities covered (Turnkey API endpoints):
//   query/whoami
//   submit/init_import_private_key
//   submit/import_private_key
//   submit/export_private_key
//   submit/export_wallet_account
//
// Wire-format choices:
//   - SignedRequest is { url, body, stamp.{stampHeaderName, stampHeaderValue} }
//     matching upstream `TSignedRequest`.
//   - Body JSON serialized via TurnkeyJsonContext (source-gen, no
//     reflection) so that the bytes hashed by ApiKeyStamper.Stamp are
//     deterministic and IL2CPP-safe.
//   - Default base URL = "https://api.turnkey.com". Each factory call
//     accepts an optional `baseUrl` for staging/mock endpoints, matching
//     upstream `THttpConfig.baseUrl`.
//
// JSON property ordering note:
//   Upstream's `stampX(input)` calls JSON.stringify(input) which preserves
//   the caller's object key insertion order — NOT the order declared in
//   public_api.types.ts. Field declaration order in the C# DTOs therefore
//   determines the emitted JSON byte order. The declared order is part of the
//   compatibility contract. The same bytes are both signed (by
//   ApiKeyStamper.Stamp) and returned as the body, so the
//   signature always verifies against the exact body delivered.

using System;
using System.Security.Cryptography;
using System.Text.Json;

namespace Turnkey
{
    /// <summary>
    /// Builds signed Turnkey API requests using an
    /// <see cref="ApiKeyStamper"/>. Logical compatibility port of the
    /// supported subset of <c>@turnkey/http</c> 3.16.0.
    /// </summary>
    public class Http
    {
        /// <summary>
        /// Default Turnkey API base URL. Overridable per factory call for
        /// staging / mock endpoints. Matches upstream <c>THttpConfig.baseUrl</c>.
        /// </summary>
        public const string DefaultBaseUrl = "https://api.turnkey.com";

        private readonly ApiKeyStamper _stamper;
        private readonly string _baseUrl;

        private Http(ApiKeyStamper stamper, string baseUrl)
        {
            _stamper = stamper ?? throw new ArgumentNullException(nameof(stamper));
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("baseUrl is required", nameof(baseUrl));
            }

            // Require TLS. This class returns a Url paired with a valid X-Stamp,
            // and that stamp is bearer-equivalent for the exact body it covers:
            // anyone who observes a cleartext request can replay it verbatim
            // against the real API. Enforcement has to live here because the SDK
            // hands back the URL rather than performing the request itself.
            //
            // The only exemption is plain http on a loopback host, for local
            // mock/test servers. A loopback host does NOT license an arbitrary
            // scheme: ftp://localhost or file://localhost is not an HTTP endpoint
            // the caller's transport can use, so it is rejected too.
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsedBaseUrl))
            {
                throw new ArgumentException(
                    "baseUrl must be an absolute URL", nameof(baseUrl));
            }
            bool isHttps = string.Equals(
                parsedBaseUrl.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal);
            bool isLoopbackHttp = parsedBaseUrl.IsLoopback
                && string.Equals(parsedBaseUrl.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal);
            if (!isHttps && !isLoopbackHttp)
            {
                throw new ArgumentException(
                    "baseUrl must use https (plain http is allowed only for loopback hosts)",
                    nameof(baseUrl));
            }

            // Trailing slashes would otherwise produce "https://host//public/v1/...".
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// Create an <see cref="Http"/> client from an HPKE-encrypted Turnkey
        /// credential bundle (the "legacy" flow used by export and bundle
        /// auth recovery).
        /// </summary>
        /// <param name="encryptedCredentialBundle">
        /// Base58Check-encoded HPKE bundle from Turnkey
        /// (33-byte compressed sender key followed by the ciphertext).
        /// </param>
        /// <param name="targetPrivateKey">
        /// Hex-encoded target P-256 private key (the receiver of the HPKE
        /// envelope).
        /// </param>
        /// <param name="baseUrl">Optional Turnkey API base URL. Defaults to <see cref="DefaultBaseUrl"/>.</param>
        public static Http GetHttpClient(
            string encryptedCredentialBundle,
            string targetPrivateKey,
            string baseUrl = DefaultBaseUrl)
        {
            if (string.IsNullOrEmpty(encryptedCredentialBundle))
            {
                throw new ArgumentException(
                    "Encrypted credential bundle is required", nameof(encryptedCredentialBundle));
            }
            if (string.IsNullOrEmpty(targetPrivateKey))
            {
                throw new ArgumentException(
                    "Target private key is required", nameof(targetPrivateKey));
            }

            var apiPrivateKey = Crypto.DecryptCredentialBundle(encryptedCredentialBundle, targetPrivateKey);
            var apiPrivateKeyBytes = Encoding.Uint8ArrayFromHexString(apiPrivateKey);
            try
            {
                var apiPublicKeyBytes = Crypto.GetPublicKey(apiPrivateKeyBytes, isCompressed: true);
                var apiPublicKey = Encoding.Uint8ArrayToHexString(apiPublicKeyBytes);

                // `apiPrivateKey` is handed to the stamper as-is. It already came
                // out of Crypto.DecryptCredentialBundle as lower-case hex (that
                // method returns Encoding.Uint8ArrayToHexString(...)), so the
                // round-trip through Uint8ArrayFromHexString/Uint8ArrayToHexString
                // that used to sit here produced a byte-identical string — its
                // only lasting effect was a second immutable, unerasable copy of
                // the private key on the managed heap.
                return new Http(new ApiKeyStamper(apiPublicKey, apiPrivateKey), baseUrl);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(apiPrivateKeyBytes);
            }
        }

        /// <summary>
        /// Create an <see cref="Http"/> client directly from a target private
        /// key (the OTP session flow where the session key is already known).
        /// </summary>
        /// <param name="targetPrivateKey">Hex-encoded target P-256 private key.</param>
        /// <param name="baseUrl">Optional Turnkey API base URL. Defaults to <see cref="DefaultBaseUrl"/>.</param>
        public static Http FromTargetPrivateKey(
            string targetPrivateKey,
            string baseUrl = DefaultBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(targetPrivateKey))
            {
                throw new ArgumentException(
                    "Target private key is required", nameof(targetPrivateKey));
            }

            var privateKeyBytes = Encoding.Uint8ArrayFromHexString(targetPrivateKey);
            if (privateKeyBytes.Length == 0)
            {
                throw new ArgumentException(
                    "Target private key was not valid hex", nameof(targetPrivateKey));
            }

            try
            {
                // Unlike GetHttpClient above, this normalization is NOT
                // redundant: targetPrivateKey is caller-supplied and may be
                // upper-case hex. The stamper's own public key ends up embedded
                // in the stamp JSON, so the spelling that reaches it is part of
                // the wire format and must stay lower-case.
                var normalizedPrivateKey = Encoding.Uint8ArrayToHexString(privateKeyBytes);
                var publicKeyBytes = Crypto.GetPublicKey(privateKeyBytes, isCompressed: true);
                var publicKeyHex = Encoding.Uint8ArrayToHexString(publicKeyBytes);

                return new Http(new ApiKeyStamper(publicKeyHex, normalizedPrivateKey), baseUrl);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }

        // ===== Activity stampers =====

        /// <summary>
        /// Build a signed <c>query/whoami</c> request.
        /// </summary>
        public SignedRequest StampGetWhoami(string organizationId)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                throw new ArgumentException("Organization ID is required", nameof(organizationId));
            }
            RequireWellFormedUtf16(organizationId, "organizationId", nameof(organizationId));
            var body = new WhoamiRequestBody { OrganizationId = organizationId };
            return CreateSignedRequest(
                _baseUrl + "/public/v1/query/whoami",
                JsonSerializer.Serialize(body, typeof(WhoamiRequestBody), TurnkeyJsonContext.JsCompatibleOptions));
        }

        /// <summary>
        /// Build a signed <c>submit/init_import_private_key</c> request.
        /// </summary>
        public SignedRequest StampInitImportPrivateKey(InitImportPrivateKeyRequestBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            ValidateWireStrings(body);
            return CreateSignedRequest(
                _baseUrl + "/public/v1/submit/init_import_private_key",
                JsonSerializer.Serialize(body, typeof(InitImportPrivateKeyRequestBody), TurnkeyJsonContext.JsCompatibleOptions));
        }

        /// <summary>
        /// Build a signed <c>submit/import_private_key</c> request.
        /// </summary>
        public SignedRequest StampImportPrivateKey(ImportPrivateKeyRequestBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            ValidateWireStrings(body);
            return CreateSignedRequest(
                _baseUrl + "/public/v1/submit/import_private_key",
                JsonSerializer.Serialize(body, typeof(ImportPrivateKeyRequestBody), TurnkeyJsonContext.JsCompatibleOptions));
        }

        /// <summary>
        /// Build a signed <c>submit/export_private_key</c> request.
        /// </summary>
        public SignedRequest StampExportPrivateKey(ExportPrivateKeyRequestBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            ValidateWireStrings(body);
            return CreateSignedRequest(
                _baseUrl + "/public/v1/submit/export_private_key",
                JsonSerializer.Serialize(body, typeof(ExportPrivateKeyRequestBody), TurnkeyJsonContext.JsCompatibleOptions));
        }

        /// <summary>
        /// Build a signed <c>submit/export_wallet_account</c> request.
        /// </summary>
        public SignedRequest StampExportWalletAccount(ExportWalletAccountRequestBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            ValidateWireStrings(body);
            return CreateSignedRequest(
                _baseUrl + "/public/v1/submit/export_wallet_account",
                JsonSerializer.Serialize(body, typeof(ExportWalletAccountRequestBody), TurnkeyJsonContext.JsCompatibleOptions));
        }

        // ===== Ill-formed UTF-16 gate ================================
        //
        // System.Text.Json does not reject an unpaired surrogate in a string it
        // is asked to serialize — it substitutes U+FFFD. Left alone, this SDK
        // would therefore sign and hand back a body containing a value the
        // caller never supplied, and the signature would attest to the
        // substituted text. Reject instead of repairing: the caller's intent
        // cannot be recovered once the surrogate is gone, and silently signing
        // altered content is the one outcome that must not happen.
        //
        // The check runs on the INPUT strings, not on the serialized output.
        // Scanning the output for U+FFFD cannot tell an encoder substitution
        // apart from a U+FFFD the caller legitimately supplied without leaning
        // on undocumented encoder details (measured: substitutions come out as
        // a six-character "backslash-u-F-F-F-D" escape while a caller's literal
        // U+FFFD is emitted raw — true today, but nothing contractual keeps it
        // true, and a security check must not rest on that). Unpaired
        // surrogates, by contrast, are decidable from the input alone via
        // documented char.IsHighSurrogate / char.IsLowSurrogate semantics: a
        // literal U+FFFD is not a surrogate and is never rejected, and
        // astral-plane characters are well-formed pairs and are never rejected.
        //
        // MAINTENANCE: every caller-supplied string that reaches the wire needs
        // a check here. When a DTO gains a string member, extend the matching
        // ValidateWireStrings overload below.
        //
        // This is ENFORCED, not left to memory. The shipped library cannot
        // reflect over the DTOs (IL2CPP/AOT), but the test assembly runs on
        // net8.0 with full reflection, so
        //   HttpTests.AllWireDtoStringFields_AreCoveredByIllFormedUtf16Validation
        // walks every string / string[] member of every *RequestBody type,
        // poisons each one in turn with an unpaired surrogate, and asserts the
        // corresponding Stamp* call rejects it. A new field — or a whole new
        // wire DTO — fails that test instead of silently bypassing this gate.

        private static void RequireWellFormedUtf16(string value, string memberName, string parameterName)
        {
            if (value == null)
            {
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsHighSurrogate(c))
                {
                    if (i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                    {
                        i++;
                        continue;
                    }
                }
                else if (!char.IsLowSurrogate(c))
                {
                    continue;
                }

                // The offending value is not echoed: these members are
                // caller-controlled and may carry sensitive material.
                throw new ArgumentException(
                    "cannot sign a request containing ill-formed UTF-16: " + memberName
                    + " has an unpaired surrogate at index " + i
                    + ". System.Text.Json would replace it with U+FFFD, so the "
                    + "signed body would not match the value supplied.",
                    parameterName);
            }
        }

        private static void ValidateWireStrings(InitImportPrivateKeyRequestBody body)
        {
            RequireWellFormedUtf16(body.OrganizationId, "organizationId", nameof(body));
            RequireWellFormedUtf16(body.Type, "type", nameof(body));
            RequireWellFormedUtf16(body.TimestampMs, "timestampMs", nameof(body));
            if (body.Parameters != null)
            {
                RequireWellFormedUtf16(body.Parameters.UserId, "parameters.userId", nameof(body));
            }
        }

        private static void ValidateWireStrings(ImportPrivateKeyRequestBody body)
        {
            RequireWellFormedUtf16(body.OrganizationId, "organizationId", nameof(body));
            RequireWellFormedUtf16(body.Type, "type", nameof(body));
            RequireWellFormedUtf16(body.TimestampMs, "timestampMs", nameof(body));
            if (body.Parameters != null)
            {
                RequireWellFormedUtf16(body.Parameters.UserId, "parameters.userId", nameof(body));
                RequireWellFormedUtf16(body.Parameters.Curve, "parameters.curve", nameof(body));
                RequireWellFormedUtf16(
                    body.Parameters.EncryptedBundle, "parameters.encryptedBundle", nameof(body));
                RequireWellFormedUtf16(
                    body.Parameters.PrivateKeyName, "parameters.privateKeyName", nameof(body));
                if (body.Parameters.AddressFormats != null)
                {
                    for (int i = 0; i < body.Parameters.AddressFormats.Length; i++)
                    {
                        RequireWellFormedUtf16(
                            body.Parameters.AddressFormats[i],
                            "parameters.addressFormats[" + i + "]",
                            nameof(body));
                    }
                }
            }
        }

        private static void ValidateWireStrings(ExportPrivateKeyRequestBody body)
        {
            RequireWellFormedUtf16(body.OrganizationId, "organizationId", nameof(body));
            RequireWellFormedUtf16(body.Type, "type", nameof(body));
            RequireWellFormedUtf16(body.TimestampMs, "timestampMs", nameof(body));
            if (body.Parameters != null)
            {
                RequireWellFormedUtf16(
                    body.Parameters.PrivateKeyId, "parameters.privateKeyId", nameof(body));
                RequireWellFormedUtf16(
                    body.Parameters.TargetPublicKey, "parameters.targetPublicKey", nameof(body));
            }
        }

        private static void ValidateWireStrings(ExportWalletAccountRequestBody body)
        {
            RequireWellFormedUtf16(body.OrganizationId, "organizationId", nameof(body));
            RequireWellFormedUtf16(body.Type, "type", nameof(body));
            RequireWellFormedUtf16(body.TimestampMs, "timestampMs", nameof(body));
            if (body.Parameters != null)
            {
                RequireWellFormedUtf16(body.Parameters.Address, "parameters.address", nameof(body));
                RequireWellFormedUtf16(
                    body.Parameters.TargetPublicKey, "parameters.targetPublicKey", nameof(body));
            }
        }

        private SignedRequest CreateSignedRequest(string url, string bodyJson)
        {
            var stampResult = _stamper.Stamp(bodyJson);
            return new SignedRequest
            {
                Url = url,
                Body = bodyJson,
                Stamp = new Stamp
                {
                    StampHeaderName = stampResult.StampHeaderName,
                    StampHeaderValue = stampResult.StampHeaderValue,
                },
            };
        }

        // ===== DTOs (nested per D16; member order = upstream JSON key order) =====

        /// <summary>
        /// Output of a stamp helper: ready-to-POST URL + canonical JSON body +
        /// stamp header. Matches upstream <c>TSignedRequest</c>.
        /// </summary>
        /// <remarks>
        /// Member declaration order matches upstream's runtime object literal
        /// in <c>public_api.client.ts</c>:
        /// <code>return { body: body, stamp: stamp, url: fullUrl };</code>
        /// so JSON-serializing a <c>SignedRequest</c> produces the same key
        /// order as the upstream TS client.
        /// </remarks>
        public class SignedRequest
        {
            [System.Text.Json.Serialization.JsonPropertyName("body")]
            public string Body { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("stamp")]
            public Stamp Stamp { get; set; } = new Stamp();

            [System.Text.Json.Serialization.JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Stamp header pair. Matches upstream <c>TStamp</c>.
        /// </summary>
        public class Stamp
        {
            [System.Text.Json.Serialization.JsonPropertyName("stampHeaderName")]
            public string StampHeaderName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("stampHeaderValue")]
            public string StampHeaderValue { get; set; } = string.Empty;
        }

        public class WhoamiRequestBody
        {
            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
            public string OrganizationId { get; set; } = string.Empty;
        }

        public class InitImportPrivateKeyRequestBody
        {
            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
            public string OrganizationId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
            public string TimestampMs { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
            public InitImportPrivateKeyParameters Parameters { get; set; } = new InitImportPrivateKeyParameters();

            /// <summary>
            /// Optional upstream <c>generateAppProofs</c> opt-in. Omitted from
            /// JSON when null (matches upstream optional behavior).
            /// </summary>
            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
            public bool? GenerateAppProofs { get; set; }
        }

        public class InitImportPrivateKeyParameters
        {
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public string UserId { get; set; } = string.Empty;
        }

        public class ImportPrivateKeyRequestBody
        {
            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
            public string OrganizationId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
            public string TimestampMs { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
            public ImportPrivateKeyParameters Parameters { get; set; } = new ImportPrivateKeyParameters();

            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
            public bool? GenerateAppProofs { get; set; }
        }

        public class ImportPrivateKeyParameters
        {
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public string UserId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("addressFormats")]
            public string[] AddressFormats { get; set; } = Array.Empty<string>();

            [System.Text.Json.Serialization.JsonPropertyName("curve")]
            public string Curve { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("encryptedBundle")]
            public string EncryptedBundle { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("privateKeyName")]
            public string PrivateKeyName { get; set; } = string.Empty;
        }

        public class ExportPrivateKeyRequestBody
        {
            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
            public string OrganizationId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
            public string TimestampMs { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
            public ExportPrivateKeyParameters Parameters { get; set; } = new ExportPrivateKeyParameters();

            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
            public bool? GenerateAppProofs { get; set; }
        }

        public class ExportPrivateKeyParameters
        {
            [System.Text.Json.Serialization.JsonPropertyName("privateKeyId")]
            public string PrivateKeyId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("targetPublicKey")]
            public string TargetPublicKey { get; set; } = string.Empty;
        }

        public class ExportWalletAccountRequestBody
        {
            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
            public string OrganizationId { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
            public string TimestampMs { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
            public ExportWalletAccountParameters Parameters { get; set; } = new ExportWalletAccountParameters();

            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
            public bool? GenerateAppProofs { get; set; }
        }

        public class ExportWalletAccountParameters
        {
            [System.Text.Json.Serialization.JsonPropertyName("address")]
            public string Address { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("targetPublicKey")]
            public string TargetPublicKey { get; set; } = string.Empty;
        }
    }
}
