// 1:1 logical port of @turnkey/api-key-stamper@0.5.0
//
// Upstream snapshot:
//   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/
//
// Files covered:
//   ts-source/index.ts                -> ApiKeyStamper class + signWithApiKey
//                                         + stampHeaderName constant
//   ts-source/purejs.ts               -> signWithApiKey "purejs" runtime
//                                         (P-256 ECDSA via @noble/curves,
//                                          SHA-256, DER hex output)
//
// Runtime selection note:
//   The upstream package supports three runtimes — "browser" (WebCrypto),
//   "node" (Node crypto), "purejs" (@noble/curves). This C# port mirrors
//   the **purejs** runtime since BouncyCastle exposes the same primitives
//   in a single backend. There is no runtime detection in C#; behavior is
//   identical to what upstream produces when `runtimeOverride = "purejs"`
//   is passed.
//
// Wire bytes:
//   - signature.toDERHex() output equals upstream's noble signature.toDERHex().
//   - The stamp JSON shape is exactly the upstream's
//     { publicKey, scheme, signature } in this order.
//   - The X-Stamp header value is base64url(JSON), matching upstream's
//     `stringToBase64urlString(JSON.stringify(stamp))`.
//   - SHA-256(content) and deterministic ECDSA (RFC 6979) are used; low-S is
//     enforced (noble defaults to lowS: true).

using System;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Turnkey
{
    /// <summary>
    /// Stamper for Turnkey API requests. 1:1 logical port of upstream
    /// <c>@turnkey/api-key-stamper</c> at peak's pinned version 0.5.0.
    /// </summary>
    /// <remarks>
    /// <para><b>Runtime parity target:</b> this port mirrors upstream's
    /// <c>purejs</c> runtime (deterministic ECDSA via RFC 6979 + low-S).
    /// Upstream's default <c>detectRuntime()</c> picks <c>"node"</c> in
    /// Node which dispatches to <c>nodecrypto.ts</c> and produces
    /// non-deterministic signatures (random k); behavior parity with
    /// that runtime is intentionally NOT a goal — Turnkey's backend
    /// accepts either, but the C# wire bytes will only match
    /// deterministically against upstream <c>runtimeOverride: "purejs"</c>
    /// callers. All three runtimes produce signatures that verify against
    /// the same public key, so end-to-end Turnkey API calls work
    /// regardless.</para>
    /// <para><b>Validation timing:</b> the constructor parses but does NOT
    /// validate the private key. Validation happens in
    /// <see cref="SignWithApiKey(string)"/>, matching upstream which only
    /// constructs noble's key object at sign time.</para>
    /// </remarks>
    public class ApiKeyStamper
    {
        /// <summary>Header name used by Turnkey API requests.</summary>
        public const string StampHeaderName = "X-Stamp";

        /// <summary>Turnkey signature scheme identifier for P-256 API keys.</summary>
        public const string SignatureScheme = "SIGNATURE_SCHEME_TK_API_P256";

        private readonly string _apiPublicKey;
        private readonly string _apiPrivateKey;

        /// <summary>
        /// Initialize the stamper with a P-256 API key pair.
        /// </summary>
        /// <param name="apiPublicKey">Compressed P-256 public key as hex (66 chars).</param>
        /// <param name="apiPrivateKey">P-256 private key scalar as 64 hex chars.</param>
        /// <remarks>
        /// The constructor only stores the strings. Key parsing and range
        /// validation happen lazily in <see cref="SignWithApiKey(string)"/>,
        /// matching upstream which builds noble's key object on each sign.
        /// </remarks>
        public ApiKeyStamper(string apiPublicKey, string apiPrivateKey)
        {
            // Upstream constructor only assigns config fields without
            // validating; mirror that exactly. We do allow null here so a
            // subsequent SignWithApiKey call surfaces the failure naturally.
            _apiPublicKey = apiPublicKey!;
            _apiPrivateKey = apiPrivateKey!;
        }

        /// <summary>
        /// Output of <see cref="Stamp(string)"/>: the header name and the
        /// base64url-encoded stamp JSON to use as the header value.
        /// </summary>
        public class StampResult
        {
            public string StampHeaderName { get; set; } = string.Empty;
            public string StampHeaderValue { get; set; } = string.Empty;
        }

        /// <summary>
        /// Internal JSON shape of the stamp. Field order matches upstream
        /// (<c>publicKey, scheme, signature</c>) so the base64url-encoded
        /// header value is wire-identical to what
        /// <c>JSON.stringify({ publicKey, scheme, signature })</c> produces.
        /// </summary>
        public class TurnkeyStamp
        {
            [System.Text.Json.Serialization.JsonPropertyName("publicKey")]
            public string PublicKey { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("scheme")]
            public string Scheme { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("signature")]
            public string Signature { get; set; } = string.Empty;
        }

        /// <summary>
        /// Produce a Turnkey API stamp for <paramref name="payload"/>.
        /// Equivalent to upstream <c>ApiKeyStamper.stamp(payload)</c>.
        /// </summary>
        public StampResult Stamp(string payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            string signatureDerHex = SignWithApiKey(payload);

            var stamp = new TurnkeyStamp
            {
                PublicKey = _apiPublicKey,
                Scheme = SignatureScheme,
                Signature = signatureDerHex,
            };

            string stampJson = System.Text.Json.JsonSerializer.Serialize(
                stamp, TurnkeyJsonContext.Default.TurnkeyStamp);

            return new StampResult
            {
                StampHeaderName = StampHeaderName,
                StampHeaderValue = Encoding.StringToBase64UrlString(stampJson),
            };
        }

        /// <summary>
        /// Mirror of upstream <c>purejs.ts signWithApiKey</c>. Verifies that
        /// the configured public key matches the one derived from the
        /// configured private key, then SHA-256-hashes the payload and signs
        /// it with deterministic ECDSA (RFC 6979 + low-S) returning the
        /// DER-encoded signature as a hex string.
        /// </summary>
        public string SignWithApiKey(string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            // Upstream `purejs.ts`:
            //   const publicKey = p256.getPublicKey(input.privateKey, true);
            //   const publicKeyString = uint8ArrayToHexString(publicKey);
            //   if (publicKeyString != input.publicKey) throw new Error(...);
            //
            // Crypto.GetPublicKey enforces the noble invariants (exact 32-byte
            // private key, scalar in [1, n - 1]) before deriving the public
            // key. Per the upstream pattern this validation happens at sign
            // time, not at constructor time.
            byte[] derivedPublicKeyBytes = Crypto.GetPublicKey(_apiPrivateKey, isCompressed: true);
            string derivedPublicKey = Encoding.Uint8ArrayToHexString(derivedPublicKeyBytes);
            if (!string.Equals(derivedPublicKey, _apiPublicKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Bad API key. Expected to get public key " + _apiPublicKey
                    + ", got " + derivedPublicKey);
            }

            // Upstream:
            //   const hash = createHash().update(input.content).digest();   // SHA-256
            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
            //   return signature.toDERHex();
            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            var privateKeyBytes = Encoding.Uint8ArrayFromHexString(_apiPrivateKey);
            var privateKeyScalar = new BigInteger(1, privateKeyBytes);

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(content);

            var digest = DigestUtilities.GetDigest("SHA-256");
            digest.BlockUpdate(payloadBytes, 0, payloadBytes.Length);
            var hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

            var hmacCalculator = new HMacDsaKCalculator(DigestUtilities.GetDigest("SHA-256"));
            var signer = new ECDsaSigner(hmacCalculator);
            signer.Init(true, new ECPrivateKeyParameters(privateKeyScalar, domainParams));

            BigInteger[] signature = signer.GenerateSignature(hash);
            BigInteger r = signature[0];
            BigInteger s = signature[1];

            // Low-S normalization. @turnkey/api-key-stamper@0.5.0 depends on
            // @noble/curves@^1.3.0, and noble v1.3.0
            // (src/abstract/weierstrass.ts) defaults to lowS=true:
            //   let { lowS, prehash, extraEntropy: ent } = opts;
            //   if (lowS == null) lowS = true; // RFC6979 3.2
            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
            // no opts, so the runtime emits low-S signatures. We reproduce
            // that here by clamping s into (0, n/2].
            BigInteger halfN = domainParams.N.ShiftRight(1);
            if (s.CompareTo(halfN) > 0)
            {
                s = domainParams.N.Subtract(s);
            }

            return EncodeDerSignatureHex(r, s);
        }

        /// <summary>
        /// Encode the <c>(r, s)</c> pair as a DER ASN.1 SEQUENCE of two
        /// INTEGERs, then hex-encode the result.
        /// Equivalent to noble's <c>signature.toDERHex()</c>.
        /// </summary>
        private static string EncodeDerSignatureHex(BigInteger r, BigInteger s)
        {
            byte[] rBytes = r.ToByteArrayUnsigned();
            byte[] sBytes = s.ToByteArrayUnsigned();

            bool rNeedsPadding = rBytes.Length > 0 && (rBytes[0] & 0x80) != 0;
            bool sNeedsPadding = sBytes.Length > 0 && (sBytes[0] & 0x80) != 0;
            int rLen = rBytes.Length + (rNeedsPadding ? 1 : 0);
            int sLen = sBytes.Length + (sNeedsPadding ? 1 : 0);

            int contentLen = 2 + rLen + 2 + sLen; // (INTEGER tag + length) * 2 + r + s

            using (var ms = new MemoryStream())
            {
                ms.WriteByte(0x30); // SEQUENCE

                if (contentLen >= 128)
                {
                    // Long-form length (1 octet count). For ECDSA P-256
                    // signatures the total length never exceeds 0x7f in
                    // practice; this branch is defensive.
                    ms.WriteByte(0x81);
                    ms.WriteByte((byte)contentLen);
                }
                else
                {
                    ms.WriteByte((byte)contentLen);
                }

                ms.WriteByte(0x02); // INTEGER tag (r)
                ms.WriteByte((byte)rLen);
                if (rNeedsPadding) ms.WriteByte(0x00);
                ms.Write(rBytes, 0, rBytes.Length);

                ms.WriteByte(0x02); // INTEGER tag (s)
                ms.WriteByte((byte)sLen);
                if (sNeedsPadding) ms.WriteByte(0x00);
                ms.Write(sBytes, 0, sBytes.Length);

                return Encoding.Uint8ArrayToHexString(ms.ToArray());
            }
        }
    }
}
