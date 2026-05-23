// 1:1 logical port of @turnkey/crypto@2.8.8
//
// Upstream snapshot:
//   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
//
// Files covered:
//   ts-source/constants.ts            -> Crypto.Constants nested class
//   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
//   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
//                                        HpkeEncrypt / HpkeDecrypt /
//                                        BuildAdditionalAssociatedData /
//                                        CompressRawPublicKey /
//                                        UncompressRawPublicKey /
//                                        FormatHpkeBuf, plus the internal
//                                        helpers BuildLabeledIkm /
//                                        BuildLabeledInfo / ExtractAndExpand
//   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
//                                        (port of @noble/hashes/hkdf)
//   ts-source/turnkey.ts (subset)     -> Curve enum / DecryptCredentialBundle /
//                                        EncryptPrivateKeyToBundle /
//                                        DecryptExportBundle /
//                                        VerifySessionJwtSignature
//
// Out of scope (matches the peak Unity port):
//   hpkeAuthEncrypt, quorumKeyEncrypt, extractPrivateKeyFromPKCS8Bytes,
//   fromDerSignature, toDerSignature
//   verifyStampSignature, encryptWalletToBundle, encryptToEnclave,
//   encryptOauth2ClientSecret, encryptOnRampSecret
//   proof.ts (AWS Nitro attestation chain)
//
// Adaptations:
//   - System.Text.Json source generation (TurnkeyJsonContext) replaces
//     Newtonsoft.Json's JObject.Parse / JsonConvert.SerializeObject used by
//     the peak Unity port. Wire bytes are unchanged.
//   - BouncyCastle 2.5.0 wraps ECDSA / ECDH / AES-GCM / SHA-256 / HMAC /
//     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
//     Tonelli-Shanks, and bundle parsing logic are direct line-by-line
//     ports of the upstream TypeScript.
//   - Newtonsoft.Json dependency dropped.
//
// 2.8.8 vs 2.8.9 note:
//   The only diff between @turnkey/crypto@2.8.8 and @turnkey/crypto@2.8.9 in
//   the published dist/ is the inlining of QOS_ENCRYPTION_HMAC_MESSAGE
//   (2.8.9 hard-codes the bytes; 2.8.8 uses new TextEncoder().encode("...")).
//   They produce identical wire bytes. This port targets 2.8.8 (peak's pin)
//   but is logically equivalent to 2.8.9 as well.

using System;
using System.Security.Cryptography;
using System.Text.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Turnkey
{
    /// <summary>
    /// Cryptographic primitives and Turnkey bundle helpers. 1:1 logical port
    /// of <c>@turnkey/crypto</c> at peak's pinned version 2.8.8.
    /// </summary>
    public static class Crypto
    {
        #region Constants

        /// <summary>
        /// Constants from upstream <c>constants.ts</c> used by this port.
        /// Bytes match the upstream <c>Uint8Array(...)</c> literals verbatim.
        /// </summary>
        public static class Constants
        {
            // HPKE KEM/HPKE suite IDs.
            public static readonly byte[] SUITE_ID_1 = new byte[] { 75, 69, 77, 0, 16 }; // "KEM\0\x10"
            public static readonly byte[] SUITE_ID_2 = new byte[] { 72, 80, 75, 69, 0, 16, 0, 1, 0, 2 }; // "HPKE\0\x10\0\x01\0\x02"
            public static readonly byte[] HPKE_VERSION = new byte[] { 72, 80, 75, 69, 45, 118, 49 }; // "HPKE-v1"

            // HPKE labels.
            public static readonly byte[] LABEL_SECRET = new byte[] { 115, 101, 99, 114, 101, 116 }; // "secret"
            public static readonly byte[] LABEL_EAE_PRK = new byte[] { 101, 97, 101, 95, 112, 114, 107 }; // "eae_prk"
            public static readonly byte[] LABEL_SHARED_SECRET = new byte[]
            {
                115, 104, 97, 114, 101, 100, 95, 115, 101, 99, 114, 101, 116, // "shared_secret"
            };

            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
            public static readonly byte[] AES_KEY_INFO = new byte[]
            {
                0, 32, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 107,
                101, 121, 0, 143, 195, 174, 184, 50, 73, 10, 75, 90, 179, 228, 32, 35, 40,
                125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34, 135, 39, 183, 10, 64, 33,
                18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53, 41, 180, 122, 198, 166, 48,
                185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151, 113, 201, 158, 80,
            };

            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
            public static readonly byte[] IV_INFO = new byte[]
            {
                0, 12, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 98, 97,
                115, 101, 95, 110, 111, 110, 99, 101, 0, 143, 195, 174, 184, 50, 73, 10, 75,
                90, 179, 228, 32, 35, 40, 125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34,
                135, 39, 183, 10, 64, 33, 18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53,
                41, 180, 122, 198, 166, 48, 185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151,
                113, 201, 158, 80,
            };

            /// <summary>SEC1 uncompressed P-256 public key length, bytes (0x04 + X + Y).</summary>
            public const int UNCOMPRESSED_PUB_KEY_LENGTH_BYTES = 65;

            /// <summary>Production signer used by Turnkey to sign export/import bundles.</summary>
            public const string PRODUCTION_SIGNER_SIGN_PUBLIC_KEY =
                "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569";

            /// <summary>Production notarizer used by Turnkey to sign session JWTs.</summary>
            public const string PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY =
                "04d498aa87ac3bf982ac2b5dd9604d0074905cfbda5d62727c5a237b895e6749205e9f7cd566909c4387f6ca25c308445c60884b788560b785f4a96ac33702a469";
        }

        #endregion

        #region Curve enum (turnkey.ts type)

        /// <summary>
        /// EC curve identifier. Mirrors the upstream union type
        /// <c>"CURVE_P256" | "CURVE_SECP256K1"</c> exposed by
        /// <see cref="UncompressRawPublicKey(byte[], Curve)"/>.
        /// </summary>
        public enum Curve
        {
            P256,
            Secp256k1,
        }

        #endregion

        #region Math (math.ts)

        /// <summary>
        /// Mathematical helpers ported from upstream <c>math.ts</c>.
        /// </summary>
        public static class Math
        {
            /// <summary>
            /// Modular square root via Tonelli-Shanks. Equivalent to
            /// upstream <c>math.ts modSqrt</c>.
            /// </summary>
            /// <param name="x">Value to take the square root of (must be non-negative; EC field-element semantics).</param>
            /// <param name="p">Prime modulus.</param>
            /// <returns>One square root of <paramref name="x"/> modulo <paramref name="p"/>.</returns>
            /// <remarks>
            /// All known call sites in <c>@turnkey/crypto</c> pass a non-negative
            /// <paramref name="x"/> derived from an EC field coordinate, where
            /// BouncyCastle <c>BigInteger.Mod</c> and JS <c>BigInt %</c> agree.
            /// For symmetry with the JS surface, the result for a negative
            /// <paramref name="x"/> may differ; this is not exercised in
            /// production code paths.
            /// </remarks>
            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
            {
                if (p.CompareTo(BigInteger.Zero) <= 0)
                {
                    throw new ArgumentException("p must be positive");
                }
                var baseVal = x.Mod(p);

                // p % 4 == 3 fast path (applies to NIST P-256 / P-384 / P-521).
                if (p.TestBit(0) && p.TestBit(1))
                {
                    var q = p.Add(BigInteger.One).ShiftRight(2);
                    var squareRoot = baseVal.ModPow(q, p);

                    if (!squareRoot.Multiply(squareRoot).Mod(p).Equals(baseVal))
                    {
                        throw new InvalidOperationException("could not find a modular square root");
                    }
                    return squareRoot;
                }
                throw new InvalidOperationException("unsupported modulus value");
            }
        }

        #endregion

        #region HKDF (port of @noble/hashes/hkdf)

        /// <summary>
        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
        /// which upstream <c>crypto.ts</c> imports.
        /// </summary>
        public static class Hkdf
        {
            private const int HashLen = 32; // SHA-256 output length, bytes.

            /// <summary>
            /// HKDF Extract. RFC 5869 §2.2.
            /// </summary>
            public static byte[] Extract(byte[] salt, byte[] ikm)
            {
                if (salt == null || salt.Length == 0)
                {
                    salt = new byte[HashLen]; // RFC 5869: salt defaults to HashLen zero bytes.
                }
                using (var hmac = new HMACSHA256(salt))
                {
                    return hmac.ComputeHash(ikm ?? Array.Empty<byte>());
                }
            }

            /// <summary>
            /// HKDF Expand. RFC 5869 §2.3.
            /// </summary>
            public static byte[] Expand(byte[] prk, byte[] info, int length)
            {
                if (prk == null || prk.Length < HashLen)
                {
                    throw new ArgumentException("PRK must be at least HashLen bytes");
                }
                if (length > 255 * HashLen)
                {
                    throw new ArgumentException(
                        "Output length cannot exceed 255 * HashLen (" + (255 * HashLen) + " bytes)");
                }
                if (info == null)
                {
                    info = Array.Empty<byte>();
                }

                int n = (length + HashLen - 1) / HashLen; // ceil(length / HashLen)
                var okm = new byte[n * HashLen];
                var tPrev = Array.Empty<byte>();

                using (var hmac = new HMACSHA256(prk))
                {
                    for (int i = 1; i <= n; i++)
                    {
                        var input = new byte[tPrev.Length + info.Length + 1];
                        Array.Copy(tPrev, 0, input, 0, tPrev.Length);
                        Array.Copy(info, 0, input, tPrev.Length, info.Length);
                        input[input.Length - 1] = (byte)i;

                        var t = hmac.ComputeHash(input);
                        Array.Copy(t, 0, okm, (i - 1) * HashLen, HashLen);
                        tPrev = t;
                    }
                }

                var result = new byte[length];
                Array.Copy(okm, 0, result, 0, length);
                return result;
            }
        }

        #endregion

        #region Nested DTOs

        /// <summary>HPKE decrypt parameters; equivalent to upstream object literal.</summary>
        public class HpkeDecryptParams
        {
            public byte[]? CiphertextBuf { get; set; }
            public byte[]? EncappedKeyBuf { get; set; }
            public string? ReceiverPriv { get; set; }
        }

        /// <summary>HPKE encrypt parameters; equivalent to upstream object literal.</summary>
        public class HpkeEncryptParams
        {
            public byte[]? PlainTextBuf { get; set; }
            public byte[]? TargetKeyBuf { get; set; }
        }

        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
        public class KeyPair
        {
            public string PrivateKey { get; set; } = string.Empty;
            public string PublicKey { get; set; } = string.Empty;
            public string PublicKeyUncompressed { get; set; } = string.Empty;
        }

        /// <summary>
        /// Parameters for <see cref="EncryptPrivateKeyToBundle"/>. Mirrors the
        /// upstream <c>encryptPrivateKeyToBundle</c> options object.
        /// </summary>
        public class EncryptPrivateKeyToBundleParams
        {
            public string? PrivateKey { get; set; }
            public string? ImportBundle { get; set; }
            public string? OrganizationId { get; set; }
            public string? UserId { get; set; }
            public string? KeyFormat { get; set; }

            /// <summary>
            /// Override the production signer key for testing only. Equivalent
            /// to upstream <c>dangerouslyOverrideSignerPublicKey</c>.
            /// </summary>
            public string? DangerouslyOverrideSignerPublicKey { get; set; }
        }

        /// <summary>
        /// Parameters for <see cref="DecryptExportBundle"/>. Mirrors the
        /// upstream <c>decryptExportBundle</c> options object.
        /// </summary>
        public class DecryptExportBundleParams
        {
            public string? ExportBundle { get; set; }
            public string? EmbeddedKey { get; set; }
            public string? OrganizationId { get; set; }
            public bool ReturnMnemonic { get; set; }
            public string? KeyFormat { get; set; }

            /// <summary>
            /// Override the production signer key for testing only. Equivalent
            /// to upstream <c>dangerouslyOverrideSignerPublicKey</c>.
            /// </summary>
            public string? DangerouslyOverrideSignerPublicKey { get; set; }
        }

        /// <summary>
        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
        /// match the upstream <c>JSON.stringify({ encappedPublic, ciphertext })</c>
        /// output.
        /// </summary>
        public class HpkeBundlePayload
        {
            [System.Text.Json.Serialization.JsonPropertyName("encappedPublic")]
            public string EncappedPublic { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("ciphertext")]
            public string Ciphertext { get; set; } = string.Empty;
        }

        #endregion

        #region crypto.ts public surface

        /// <summary>
        /// Derive the SEC1 public key bytes from a private key.
        /// Equivalent to upstream <c>crypto.ts getPublicKey</c>.
        /// </summary>
        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }
            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var d = new BigInteger(1, privateKey);
            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
            var publicKeyParams = new ECPublicKeyParameters(
                privateKeyParams.Parameters.G.Multiply(d), domainParams);

            return publicKeyParams.Q.GetEncoded(isCompressed);
        }

        /// <summary>
        /// Hex-string overload of <see cref="GetPublicKey(byte[], bool)"/>.
        /// </summary>
        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
        {
            return GetPublicKey(Encoding.Uint8ArrayFromHexString(privateKeyHex), isCompressed);
        }

        /// <summary>
        /// Generate a random P-256 key pair. Equivalent to upstream
        /// <c>crypto.ts generateP256KeyPair</c>. The private key is sampled as
        /// 32 random bytes (then validated by <c>GetPublicKey</c>); identical
        /// to the upstream <c>randomBytes(32)</c> + <c>getPublicKey</c> flow.
        /// </summary>
        public static KeyPair GenerateP256KeyPair()
        {
            // Match upstream: privateKey = randomBytes(32). The valid scalar
            // range check is left to GetPublicKey() / EC math (same as upstream).
            var random = new SecureRandom();
            var privateKey = new byte[32];
            random.NextBytes(privateKey);

            var publicKeyCompressed = GetPublicKey(privateKey, isCompressed: true);
            var publicKeyUncompressed = GetPublicKey(privateKey, isCompressed: false);

            return new KeyPair
            {
                PrivateKey = Encoding.Uint8ArrayToHexString(privateKey),
                PublicKey = Encoding.Uint8ArrayToHexString(publicKeyCompressed),
                PublicKeyUncompressed = Encoding.Uint8ArrayToHexString(publicKeyUncompressed),
            };
        }

        /// <summary>
        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
        /// Equivalent to upstream <c>crypto.ts hpkeDecrypt</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Wraps any underlying decryption failure with the upstream
        /// message <c>"Unable to perform hpkeDecrypt: ..."</c>.
        /// </exception>
        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
        {
            try
            {
                if (parameters == null) throw new ArgumentNullException(nameof(parameters));
                var ciphertextBuf = parameters.CiphertextBuf
                                    ?? throw new ArgumentNullException(nameof(parameters.CiphertextBuf));
                var encappedKeyBuf = parameters.EncappedKeyBuf
                                     ?? throw new ArgumentNullException(nameof(parameters.EncappedKeyBuf));
                var receiverPriv = parameters.ReceiverPriv
                                   ?? throw new ArgumentNullException(nameof(parameters.ReceiverPriv));

                var receiverPrivBytes = Encoding.Uint8ArrayFromHexString(receiverPriv);
                var receiverPubBuf = GetPublicKey(receiverPrivBytes, false);

                var aad = BuildAdditionalAssociatedData(encappedKeyBuf, receiverPubBuf);
                var ss = DeriveSS(encappedKeyBuf, receiverPriv);
                var kemContext = GetKemContext(encappedKeyBuf, Encoding.Uint8ArrayToHexString(receiverPubBuf));

                var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
                var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
                var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);

                ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
                info = Constants.AES_KEY_INFO;
                var key = ExtractAndExpand(sharedSecret, ikm, info, 32);

                info = Constants.IV_INFO;
                var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);

                return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    "Unable to perform hpkeDecrypt: " + error.Message + " ", error);
            }
        }

        /// <summary>
        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
        /// </summary>
        /// <returns>
        /// The byte concatenation
        /// <c>compressed(senderPub) || ciphertext</c>, ready to feed to
        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Wraps any underlying failure with the upstream message
        /// <c>"Unable to perform hpkeEncrypt: ..."</c>.
        /// </exception>
        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
        {
            try
            {
                if (parameters == null) throw new ArgumentNullException(nameof(parameters));
                var plainTextBuf = parameters.PlainTextBuf ?? Array.Empty<byte>();
                var targetKeyBuf = parameters.TargetKeyBuf
                                   ?? throw new ArgumentNullException(nameof(parameters.TargetKeyBuf));

                var ephemeralKeyPair = GenerateP256KeyPair();
                var senderPrivBuf = Encoding.Uint8ArrayFromHexString(ephemeralKeyPair.PrivateKey);
                var senderPubBuf = Encoding.Uint8ArrayFromHexString(ephemeralKeyPair.PublicKeyUncompressed);

                var aad = BuildAdditionalAssociatedData(senderPubBuf, targetKeyBuf);
                var ss = DeriveSS(targetKeyBuf, Encoding.Uint8ArrayToHexString(senderPrivBuf));
                var kemContext = GetKemContext(senderPubBuf, Encoding.Uint8ArrayToHexString(targetKeyBuf));

                var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
                var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
                var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);

                ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
                info = Constants.AES_KEY_INFO;
                var key = ExtractAndExpand(sharedSecret, ikm, info, 32);

                info = Constants.IV_INFO;
                var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);

                var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
                var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
                return Encoding.ConcatUint8Arrays(compressedSenderBuf, encryptedData);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    "Unable to perform hpkeEncrypt: " + error.Message, error);
            }
        }

        /// <summary>
        /// HPKE additional associated data builder.
        /// Equivalent to upstream <c>crypto.ts buildAdditionalAssociatedData</c>.
        /// </summary>
        public static byte[] BuildAdditionalAssociatedData(byte[] senderPubBuf, byte[] receiverPubBuf)
        {
            return Encoding.ConcatUint8Arrays(senderPubBuf, receiverPubBuf);
        }

        /// <summary>
        /// Compress an uncompressed P-256 public key into 33 bytes.
        /// Equivalent to upstream <c>crypto.ts compressRawPublicKey</c>.
        /// </summary>
        /// <remarks>
        /// Upstream uses <c>slice(0, (1 + len) >>> 1)</c> then mutates the
        /// prefix byte based on the last byte's LSB; for a valid 65-byte
        /// SEC1 uncompressed key the result is the same 33-byte
        /// <c>(0x02|0x03) || X</c> output. Defensive validation here only
        /// rejects malformed input that upstream would also subsequently fail
        /// on downstream.
        /// </remarks>
        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
        {
            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
            int len = rawPublicKey.Length;
            // Upstream: var compressedBytes = rawPublicKey.slice(0, (1 + len) >>> 1);
            int half = (1 + len) >> 1;
            var compressedBytes = new byte[half];
            Array.Copy(rawPublicKey, 0, compressedBytes, 0, half);
            // Upstream: compressedBytes[0] = 0x02 | (rawPublicKey[len - 1]! & 0x01);
            compressedBytes[0] = (byte)(0x02 | (rawPublicKey[len - 1] & 0x01));
            return compressedBytes;
        }

        /// <summary>
        /// Uncompress a 33-byte compressed public key into 65 bytes.
        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
        /// </summary>
        /// <param name="rawPublicKey">33-byte compressed key starting with 0x02 or 0x03.</param>
        /// <param name="curve">Curve identifier; defaults to <see cref="Curve.P256"/>.</param>
        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Curve curve = Curve.P256)
        {
            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
            if (rawPublicKey.Length != 33)
            {
                throw new ArgumentException("failed to uncompress raw public key: invalid length");
            }
            if (rawPublicKey[0] != 0x02 && rawPublicKey[0] != 0x03)
            {
                throw new ArgumentException("failed to uncompress raw public key: invalid prefix");
            }

            bool lsb = rawPublicKey[0] == 0x03;

            // x = BigInt("0x" + uint8ArrayToHexString(rawPublicKey.subarray(1)));
            var xBytes = new byte[32];
            Array.Copy(rawPublicKey, 1, xBytes, 0, 32);
            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);

            BigInteger p, a, b;
            if (curve == Curve.P256)
            {
                p = new BigInteger(CryptoConstants.P256_P);
                b = new BigInteger(CryptoConstants.P256_B, 16);
                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
            }
            else // Secp256k1
            {
                p = new BigInteger(
                    "fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f", 16);
                a = BigInteger.ValueOf(0);
                b = BigInteger.ValueOf(7);
            }

            // rhs = ((x * x + a) * x + b) % p
            var x2 = x.Multiply(x).Mod(p);
            var x2PlusA = x2.Add(a).Mod(p);
            var rhs = x2PlusA.Multiply(x).Add(b).Mod(p);

            var y = Math.ModSqrt(rhs, p);
            if (lsb != y.TestBit(0))
            {
                y = p.Subtract(y).Mod(p);
            }

            // Defensive range check matches upstream "throw" intent for out-of-range output.
            if (x.SignValue < 0 || x.CompareTo(p) >= 0)
            {
                throw new InvalidOperationException("x is out of range");
            }
            if (y.SignValue < 0 || y.CompareTo(p) >= 0)
            {
                throw new InvalidOperationException("y is out of range");
            }

            var uncompressed = new byte[65];
            uncompressed[0] = 0x04;
            var xHexOut = x.ToString(16).ToLowerInvariant().PadLeft(64, '0');
            var yHexOut = y.ToString(16).ToLowerInvariant().PadLeft(64, '0');
            Array.Copy(Encoding.Uint8ArrayFromHexString(xHexOut), 0, uncompressed, 1, 32);
            Array.Copy(Encoding.Uint8ArrayFromHexString(yHexOut), 0, uncompressed, 33, 32);
            return uncompressed;
        }

        /// <summary>
        /// Encode the HPKE result <c>compressed(senderPub) || ciphertext</c>
        /// as the Turnkey import-bundle JSON envelope.
        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
        /// </summary>
        public static string FormatHpkeBuf(byte[] encryptedBuf)
        {
            if (encryptedBuf == null) throw new ArgumentNullException(nameof(encryptedBuf));
            // Upstream: encappedPublic := encryptedBuf.slice(0,33);
            //           ciphertext     := encryptedBuf.slice(33);
            // Upstream then uncompressRawPublicKey() the compressed part. If
            // the input is shorter than 33 bytes that uncompress call would
            // throw "invalid length". Mirror the same behavior: pass the
            // raw slice through uncompressRawPublicKey which enforces 33-byte
            // length itself.
            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
            int compressedTake = System.Math.Min(compressedSize, encryptedBuf.Length);
            var compressedEncappedPublic = new byte[compressedTake];
            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, compressedTake);

            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);

            int cipherLen = System.Math.Max(0, encryptedBuf.Length - compressedSize);
            var ciphertext = new byte[cipherLen];
            if (cipherLen > 0)
            {
                Array.Copy(encryptedBuf, compressedSize, ciphertext, 0, cipherLen);
            }

            var payload = new HpkeBundlePayload
            {
                EncappedPublic = Encoding.Uint8ArrayToHexString(encappedPublicUncompressed),
                Ciphertext = Encoding.Uint8ArrayToHexString(ciphertext),
            };
            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
        }

        #endregion

        #region turnkey.ts public surface

        /// <summary>
        /// Decrypt a Turnkey credential bundle.
        /// Equivalent to upstream <c>turnkey.ts decryptCredentialBundle</c>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Wraps any decryption / parse failure with the upstream message
        /// <c>"Error decrypting bundle: ..."</c>.
        /// </exception>
        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string embeddedKey)
        {
            if (encryptedCredentialBundle == null)
                throw new ArgumentNullException(nameof(encryptedCredentialBundle));
            if (embeddedKey == null)
                throw new ArgumentNullException(nameof(embeddedKey));
            try
            {
                // Upstream uses bs58check.decode exclusively (NO raw bs58 fallback).
                var bundleBytes = Encoding.Base58CheckDecode(encryptedCredentialBundle);
                if (bundleBytes.Length <= 33)
                {
                    throw new InvalidOperationException(
                        "Bundle size " + bundleBytes.Length
                        + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
                }

                var compressedEncappedKeyBuf = new byte[33];
                Array.Copy(bundleBytes, 0, compressedEncappedKeyBuf, 0, 33);
                var ciphertextBuf = new byte[bundleBytes.Length - 33];
                Array.Copy(bundleBytes, 33, ciphertextBuf, 0, ciphertextBuf.Length);

                var encappedKeyBuf = UncompressRawPublicKey(compressedEncappedKeyBuf);
                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
                {
                    CiphertextBuf = ciphertextBuf,
                    EncappedKeyBuf = encappedKeyBuf,
                    ReceiverPriv = embeddedKey,
                });
                return Encoding.Uint8ArrayToHexString(decryptedData);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    "\"Error decrypting bundle:\", " + error.Message, error);
            }
        }

        /// <summary>
        /// Encrypt a private key into a Turnkey import bundle.
        /// Equivalent to upstream <c>turnkey.ts encryptPrivateKeyToBundle</c>.
        /// </summary>
        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(parameters.PrivateKey))
                throw new ArgumentException("Private key is required", nameof(parameters.PrivateKey));
            if (string.IsNullOrWhiteSpace(parameters.ImportBundle))
                throw new ArgumentException("Import bundle is required", nameof(parameters.ImportBundle));
            if (string.IsNullOrWhiteSpace(parameters.OrganizationId))
                throw new ArgumentException("Organization ID is required", nameof(parameters.OrganizationId));
            if (string.IsNullOrWhiteSpace(parameters.UserId))
                throw new ArgumentException("User ID is required", nameof(parameters.UserId));

            using var bundleDoc = JsonDocument.Parse(parameters.ImportBundle!);
            var parsedImportBundle = bundleDoc.RootElement;

            // Upstream order: decodeKey FIRST, then verify, then check fields.
            var plainTextBuf = DecodeKey(parameters.PrivateKey!, parameters.KeyFormat);

            string? enclaveQuorumPublic = GetStringOrNull(parsedImportBundle, "enclaveQuorumPublic");
            string? dataSignature = GetStringOrNull(parsedImportBundle, "dataSignature");
            string? signedDataHex = GetStringOrNull(parsedImportBundle, "data");

            VerifyEnclaveSignature(
                enclaveQuorumPublic,
                dataSignature,
                signedDataHex,
                parameters.DangerouslyOverrideSignerPublicKey);

            var signedDataBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
            using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
            var signedData = signedDoc.RootElement;

            string? orgId = GetStringOrNull(signedData, "organizationId");
            if (string.IsNullOrEmpty(orgId) || !string.Equals(orgId, parameters.OrganizationId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "organization id does not match expected value. Expected: " + parameters.OrganizationId
                    + ". Found: " + orgId + ".");
            }

            string? userId = GetStringOrNull(signedData, "userId");
            if (string.IsNullOrEmpty(userId) || !string.Equals(userId, parameters.UserId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "user id does not match expected value. Expected: " + parameters.UserId
                    + ". Found: " + userId + ".");
            }

            string? targetPublic = GetStringOrNull(signedData, "targetPublic");
            if (string.IsNullOrEmpty(targetPublic))
            {
                throw new InvalidOperationException("missing \"targetPublic\" in bundle signed data");
            }

            var targetKeyBuf = Encoding.Uint8ArrayFromHexString(targetPublic!);
            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
            {
                PlainTextBuf = plainTextBuf,
                TargetKeyBuf = targetKeyBuf,
            });
            return FormatHpkeBuf(privateKeyBundle);
        }

        /// <summary>
        /// Decrypt a Turnkey export bundle.
        /// Equivalent to upstream <c>turnkey.ts decryptExportBundle</c>.
        /// </summary>
        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (string.IsNullOrWhiteSpace(parameters.ExportBundle))
                throw new ArgumentException("Export bundle is required", nameof(parameters.ExportBundle));
            if (string.IsNullOrWhiteSpace(parameters.EmbeddedKey))
                throw new ArgumentException("Embedded key is required", nameof(parameters.EmbeddedKey));
            if (string.IsNullOrWhiteSpace(parameters.OrganizationId))
                throw new ArgumentException("Organization ID is required", nameof(parameters.OrganizationId));

            try
            {
                using var bundleDoc = JsonDocument.Parse(parameters.ExportBundle!);
                var parsedExportBundle = bundleDoc.RootElement;

                string? enclaveQuorumPublic = GetStringOrNull(parsedExportBundle, "enclaveQuorumPublic");
                string? dataSignature = GetStringOrNull(parsedExportBundle, "dataSignature");
                string? dataHex = GetStringOrNull(parsedExportBundle, "data");

                VerifyEnclaveSignature(
                    enclaveQuorumPublic,
                    dataSignature,
                    dataHex,
                    parameters.DangerouslyOverrideSignerPublicKey);

                var signedDataBytes = Encoding.Uint8ArrayFromHexString(dataHex!);
                using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
                var signedData = signedDoc.RootElement;

                string? bundleOrgId = GetStringOrNull(signedData, "organizationId");
                if (string.IsNullOrEmpty(bundleOrgId)
                    || !string.Equals(bundleOrgId, parameters.OrganizationId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "organization id does not match expected value. Expected: " + parameters.OrganizationId
                        + ". Found: " + bundleOrgId + ".");
                }

                string? encappedPublic = GetStringOrNull(signedData, "encappedPublic");
                if (string.IsNullOrEmpty(encappedPublic))
                {
                    throw new InvalidOperationException("missing \"encappedPublic\" in bundle signed data");
                }
                string? ciphertextStr = GetStringOrNull(signedData, "ciphertext");

                var encappedKeyBuf = Encoding.Uint8ArrayFromHexString(encappedPublic!);
                var ciphertextBuf = Encoding.Uint8ArrayFromHexString(ciphertextStr!);

                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
                {
                    CiphertextBuf = ciphertextBuf,
                    EncappedKeyBuf = encappedKeyBuf,
                    ReceiverPriv = parameters.EmbeddedKey,
                });

                if (string.Equals(parameters.KeyFormat, "SOLANA", StringComparison.Ordinal)
                    && !parameters.ReturnMnemonic)
                {
                    if (decryptedData.Length != 32)
                    {
                        throw new InvalidOperationException(
                            "invalid private key length. Expected 32 bytes. Got " + decryptedData.Length + ".");
                    }
                    // Derive Ed25519 public key from the 32-byte seed via BouncyCastle.
                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
                    var publicKeyBytes = ed25519PrivKey.GeneratePublicKey().GetEncoded();
                    if (publicKeyBytes.Length != 32)
                    {
                        throw new InvalidOperationException(
                            "invalid public key length. Expected 32 bytes. Got " + publicKeyBytes.Length + ".");
                    }
                    var concatenated = new byte[64];
                    Array.Copy(decryptedData, 0, concatenated, 0, 32);
                    Array.Copy(publicKeyBytes, 0, concatenated, 32, 32);
                    return Encoding.Base58Encode(concatenated);
                }

                var decryptedHex = Encoding.Uint8ArrayToHexString(decryptedData);
                return parameters.ReturnMnemonic ? Encoding.HexToAscii(decryptedHex) : decryptedHex;
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    "Error decrypting bundle: " + error.Message, error);
            }
        }

        /// <summary>
        /// Verify the ECDSA signature on a Turnkey session JWT.
        /// Equivalent to upstream <c>turnkey.ts verifySessionJwtSignature</c>.
        /// </summary>
        /// <param name="jwt">The JWT to verify.</param>
        /// <param name="dangerouslyOverrideNotarizerPublicKey">
        /// Optional hex-encoded uncompressed P-256 public key to verify
        /// against. Use for tests only; defaults to the production
        /// notarizer key.
        /// </param>
        /// <returns><c>true</c> if the signature is valid for the given key.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the JWT is malformed (missing parts).</exception>
        public static bool VerifySessionJwtSignature(
            string jwt,
            string? dangerouslyOverrideNotarizerPublicKey = null)
        {
            if (jwt == null) throw new ArgumentNullException(nameof(jwt));

            string notarizerKeyHex =
                dangerouslyOverrideNotarizerPublicKey ?? Constants.PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY;

            var parts = jwt.Split('.');
            // Upstream: const [a, b, sig] = jwt.split("."); if (!sig) throw new Error(...)
            if (parts.Length < 3 || string.IsNullOrEmpty(parts[2]))
            {
                throw new InvalidOperationException("invalid JWT: need 3 parts");
            }

            string headerB64 = parts[0];
            string payloadB64 = parts[1];
            string signatureB64 = parts[2];
            string signingInput = headerB64 + "." + payloadB64;

            byte[] msgDigest;
            using (var sha256 = SHA256.Create())
            {
                var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
                msgDigest = sha256.ComputeHash(h1);
            }

            byte[] signature = Base64UrlDecode(signatureB64);
            var publicKey = Encoding.Uint8ArrayFromHexString(notarizerKeyHex);
            return VerifyP256RawSignature(publicKey, signature, msgDigest);
        }

        #endregion

        #region Private helpers

        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
        {
            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var privBytes = Encoding.Uint8ArrayFromHexString(privHex);
            var d = new BigInteger(1, privBytes);
            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);

            var point = curve.Curve.DecodePoint(encappedKeyBuf);
            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

            var agreement = new ECDHBasicAgreement();
            agreement.Init(privateKeyParams);
            var sharedSecretBig = agreement.CalculateAgreement(publicKeyParams);
            var ss = sharedSecretBig.ToByteArrayUnsigned();

            if (ss.Length < 32)
            {
                var padded = new byte[32];
                Array.Copy(ss, 0, padded, 32 - ss.Length, ss.Length);
                ss = padded;
            }
            return ss;
        }

        private static byte[] GetKemContext(byte[] encappedKeyBuf, string publicKey)
        {
            return Encoding.ConcatUint8Arrays(
                encappedKeyBuf,
                Encoding.Uint8ArrayFromHexString(publicKey));
        }

        private static byte[] BuildLabeledIkm(byte[] label, byte[] ikm, byte[] suiteId)
        {
            return Encoding.ConcatUint8Arrays(Constants.HPKE_VERSION, suiteId, label, ikm);
        }

        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
        {
            const int suiteIdStartIndex = 9;
            var ret = new byte[suiteIdStartIndex + suiteId.Length + label.Length + info.Length];

            ret[0] = 0;
            ret[1] = (byte)len;

            Array.Copy(Constants.HPKE_VERSION, 0, ret, 2, Constants.HPKE_VERSION.Length);
            Array.Copy(suiteId, 0, ret, suiteIdStartIndex, suiteId.Length);
            Array.Copy(label, 0, ret, suiteIdStartIndex + suiteId.Length, label.Length);
            Array.Copy(info, 0, ret, suiteIdStartIndex + suiteId.Length + label.Length, info.Length);

            return ret;
        }

        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
        {
            var prk = Hkdf.Extract(sharedSecret, ikm);
            return Hkdf.Expand(prk, info, len);
        }

        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(false, new AeadParameters(new KeyParameter(key), 128, iv, aad));

            var output = new byte[cipher.GetOutputSize(encryptedData.Length)];
            int len = cipher.ProcessBytes(encryptedData, 0, encryptedData.Length, output, 0);
            cipher.DoFinal(output, len);
            return output;
        }

        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, iv, aad));

            var output = new byte[cipher.GetOutputSize(plainData.Length)];
            int len = cipher.ProcessBytes(plainData, 0, plainData.Length, output, 0);
            cipher.DoFinal(output, len);
            return output;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4)
            {
                case 2: output += "=="; break;
                case 3: output += "="; break;
            }
            return Convert.FromBase64String(output);
        }

        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
        {
            // Upstream noble p256.verify accepts r||s of length 64. Anything
            // else is an invalid signature; return false.
            if (signatureRaw.Length != 64)
            {
                return false;
            }
            try
            {
                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                var point = curve.Curve.DecodePoint(publicKeyBytes);
                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

                var r = new BigInteger(1, signatureRaw, 0, 32);
                var s = new BigInteger(1, signatureRaw, 32, 32);
                byte[] derSignature = new DerSequence(new DerInteger(r), new DerInteger(s)).GetDerEncoded();

                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
                signer.Init(false, publicKeyParams);
                signer.BlockUpdate(messageDigest, 0, messageDigest.Length);
                return signer.VerifySignature(derSignature);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Equivalent to upstream <c>turnkey.ts verifyEnclaveSignature</c>.
        /// Throws on mismatch / failed verification.
        /// </summary>
        private static void VerifyEnclaveSignature(
            string? enclaveQuorumPublic,
            string? signatureHex,
            string? signedDataHex,
            string? dangerouslyOverrideSignerPublicKey)
        {
            string expectedSignerPublicKey =
                dangerouslyOverrideSignerPublicKey ?? Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY;

            if (!string.Equals(enclaveQuorumPublic, expectedSignerPublicKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "expected signer key " + expectedSignerPublicKey
                    + " does not match signer key from bundle: " + enclaveQuorumPublic);
            }

            if (string.IsNullOrEmpty(signatureHex) || string.IsNullOrEmpty(signedDataHex))
            {
                throw new InvalidOperationException(
                    "failed to verify enclave signature: missing signature or data");
            }

            var publicKeyBytes = Encoding.Uint8ArrayFromHexString(expectedSignerPublicKey);
            var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex!);
            var messageBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);

            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            var point = curve.Curve.DecodePoint(publicKeyBytes);
            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(false, publicKeyParams);
            signer.BlockUpdate(messageBytes, 0, messageBytes.Length);

            if (!signer.VerifySignature(signatureBytes))
            {
                throw new InvalidOperationException("failed to verify enclave signature");
            }
        }

        /// <summary>
        /// Equivalent to upstream <c>turnkey.ts decodeKey</c>. Default and
        /// unknown <paramref name="keyFormat"/> values fall back to hex
        /// parsing exactly like upstream <c>default:</c> branch.
        /// </summary>
        private static byte[] DecodeKey(string privateKey, string? keyFormat)
        {
            if (string.Equals(keyFormat, "SOLANA", StringComparison.Ordinal))
            {
                var decoded = Encoding.Base58Decode(privateKey);
                if (decoded.Length != 64)
                {
                    throw new InvalidOperationException(
                        "invalid key length. Expected 64 bytes. Got " + decoded.Length + ".");
                }
                var first32 = new byte[32];
                Array.Copy(decoded, 0, first32, 0, 32);
                return first32;
            }
            // HEXADECIMAL (and unknown fallback). Upstream "default:" branch
            // also accepts the unknown case by falling through to hex parsing
            // (it logs a warn; this port silently matches the behavior).
            string normalized = privateKey.StartsWith("0x", StringComparison.Ordinal)
                ? privateKey.Substring(2)
                : privateKey;
            return Encoding.Uint8ArrayFromHexString(normalized);
        }

        private static string? GetStringOrNull(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return null;
            }
            if (!element.TryGetProperty(propertyName, out var value))
            {
                return null;
            }
            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }
            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        #endregion
    }
}
