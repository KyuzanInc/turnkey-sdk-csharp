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
            _baseUrl = baseUrl;
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
            var apiPublicKeyBytes = Crypto.GetPublicKey(apiPrivateKeyBytes, isCompressed: true);

            var normalizedPrivateKey = Encoding.Uint8ArrayToHexString(apiPrivateKeyBytes);
            var apiPublicKey = Encoding.Uint8ArrayToHexString(apiPublicKeyBytes);

            return new Http(new ApiKeyStamper(apiPublicKey, normalizedPrivateKey), baseUrl);
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

            var normalizedPrivateKey = Encoding.Uint8ArrayToHexString(privateKeyBytes);
            var publicKeyBytes = Crypto.GetPublicKey(privateKeyBytes, isCompressed: true);
            var publicKeyHex = Encoding.Uint8ArrayToHexString(publicKeyBytes);

            return new Http(new ApiKeyStamper(publicKeyHex, normalizedPrivateKey), baseUrl);
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
            return CreateSignedRequest(
                _baseUrl + "/public/v1/submit/export_wallet_account",
                JsonSerializer.Serialize(body, typeof(ExportWalletAccountRequestBody), TurnkeyJsonContext.JsCompatibleOptions));
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
