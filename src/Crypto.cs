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
//   ts-source/turnkey.ts (subset)     -> DecryptCredentialBundle /
//                                        EncryptPrivateKeyToBundle /
//                                        DecryptExportBundle /
//                                        VerifySessionJwtSignature
//
// Out of scope (matches the upstream peak-Unity port):
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
//     BigInteger / EC point primitives only. HPKE, HKDF, Tonelli-Shanks,
//     and bundle parsing logic are direct line-by-line ports of the
//     upstream TypeScript.
//   - Newtonsoft.Json dependency dropped.
//
// 2.8.8 vs 2.8.9 note:
//   The only diff between @turnkey/crypto@2.8.8 and @turnkey/crypto@2.8.9 in
//   the published dist/ is the inlining of QOS_ENCRYPTION_HMAC_MESSAGE
//   (2.8.9 hard-codes the bytes; 2.8.8 uses new TextEncoder().encode("...")).
//   They produce identical wire bytes. This port targets 2.8.8 (peak's pin)
//   but is logically equivalent to 2.8.9 as well.

using System;
using System.Linq;
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
            /// <param name="x">Value to take the square root of.</param>
            /// <param name="p">Prime modulus.</param>
            /// <returns>One square root of <paramref name="x"/> modulo <paramref name="p"/>.</returns>
            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
            {
                if (p.CompareTo(BigInteger.Zero) <= 0)
                {
                    throw new ArgumentException("p must be positive");
                }

                // BouncyCastle BigInteger.Mod returns non-negative. Upstream JS
                // BigInt `%` returns a value with the sign of x. For all known
                // call sites in @turnkey/crypto x is already non-negative (x is
                // an EC field-element coordinate), so the two are equivalent.
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
            /// HKDF Extract: produces a 32-byte pseudorandom key from input
            /// keying material <paramref name="ikm"/> and optional
            /// <paramref name="salt"/>. RFC 5869 §2.2.
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
            /// HKDF Expand: derives <paramref name="length"/> bytes of output
            /// keying material from <paramref name="prk"/> and optional
            /// context <paramref name="info"/>. RFC 5869 §2.3.
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
        }

        /// <summary>
        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
        /// match the upstream <c>JSON.stringify({ encappedPublic, ciphertext })</c>
        /// output. Property names use the upstream camelCase names verbatim;
        /// the source-generated context preserves the order.
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
        /// <c>crypto.ts generateP256KeyPair</c>.
        /// </summary>
        public static KeyPair GenerateP256KeyPair()
        {
            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var keyGen = new ECKeyPairGenerator();
            var random = new SecureRandom();
            keyGen.Init(new ECKeyGenerationParameters(domainParams, random));

            var keyPair = keyGen.GenerateKeyPair();
            var privateKey = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
            var publicKeyCompressed = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded(true);
            var publicKeyUncompressed = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded(false);

            // Pad private key to exactly 32 bytes (upstream returns 32-byte hex).
            if (privateKey.Length < 32)
            {
                var padded = new byte[32];
                Array.Copy(privateKey, 0, padded, 32 - privateKey.Length, privateKey.Length);
                privateKey = padded;
            }

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
        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
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

            // Step 1: AAD
            var aad = BuildAdditionalAssociatedData(encappedKeyBuf, receiverPubBuf);

            // Step 2: ECDH shared secret
            var ss = DeriveSS(encappedKeyBuf, receiverPriv);

            // Step 3: KEM context
            var kemContext = GetKemContext(encappedKeyBuf, Encoding.Uint8ArrayToHexString(receiverPubBuf));

            // Step 4: HKDF derive shared secret
            var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
            var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
            var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);

            // Step 5: derive AES key
            ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
            info = Constants.AES_KEY_INFO;
            var key = ExtractAndExpand(sharedSecret, ikm, info, 32);

            // Step 6: derive IV
            info = Constants.IV_INFO;
            var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);

            // Step 7: AES-GCM decrypt
            return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
        }

        /// <summary>
        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
        /// </summary>
        /// <returns>
        /// The byte concatenation
        /// <c>compressed(senderPub) || ciphertext</c>, ready to feed back into
        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
        /// </returns>
        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            var plainTextBuf = parameters.PlainTextBuf ?? Array.Empty<byte>();
            var targetKeyBuf = parameters.TargetKeyBuf
                               ?? throw new ArgumentNullException(nameof(parameters.TargetKeyBuf));

            // Ephemeral sender key pair.
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

            // Upstream returns compressedSender || ciphertext.
            var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
            return Encoding.ConcatUint8Arrays(compressedSenderBuf, encryptedData);
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
        /// Compress a 65-byte uncompressed P-256 public key into 33 bytes.
        /// Equivalent to upstream <c>crypto.ts compressRawPublicKey</c>.
        /// </summary>
        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
        {
            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
            if (rawPublicKey.Length != Constants.UNCOMPRESSED_PUB_KEY_LENGTH_BYTES || rawPublicKey[0] != 0x04)
            {
                throw new ArgumentException("Invalid uncompressed public key");
            }

            var x = new byte[32];
            Array.Copy(rawPublicKey, 1, x, 0, 32);

            // 0x02 if Y is even, 0x03 if Y is odd. Y's parity is determined by
            // its least-significant bit, which is the last byte's LSB.
            var lastByte = rawPublicKey[64];
            var prefix = (byte)((lastByte & 1) == 0 ? 0x02 : 0x03);

            var compressed = new byte[33];
            compressed[0] = prefix;
            Array.Copy(x, 0, compressed, 1, 32);
            return compressed;
        }

        /// <summary>
        /// Uncompress a 33-byte compressed P-256 public key into 65 bytes.
        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
        /// </summary>
        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey)
        {
            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
            if (rawPublicKey.Length != CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
            {
                throw new ArgumentException(
                    "Invalid compressed public key size: " + rawPublicKey.Length);
            }
            if (rawPublicKey[0] != 0x02 && rawPublicKey[0] != 0x03)
            {
                throw new ArgumentException("failed to uncompress raw public key: invalid prefix");
            }

            // 0x03 => odd Y; 0x02 => even Y.
            bool lsb = rawPublicKey[0] == 0x03;

            // x = BigInt("0x" + hex(rawPublicKey[1..33])); upstream parses via hex string.
            var xBytes = new byte[32];
            Array.Copy(rawPublicKey, 1, xBytes, 0, 32);
            var xHex = Encoding.Uint8ArrayToHexString(xBytes);
            var x = new BigInteger(xHex, 16);

            // NIST P-256: y^2 = x^3 + a*x + b (mod p), with a = p - 3.
            var p = new BigInteger(CryptoConstants.P256_P);
            var b = new BigInteger(CryptoConstants.P256_B, 16);
            var a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));

            // Upstream computes rhs = ((x*x + a) * x + b) % p in a single chain.
            var x2 = x.Multiply(x).Mod(p);
            var x2PlusA = x2.Add(a).Mod(p);
            var rhs = x2PlusA.Multiply(x).Add(b).Mod(p);

            var y = Math.ModSqrt(rhs, p);

            // Pick the root whose parity matches the requested LSB.
            if (lsb != y.TestBit(0))
            {
                y = p.Subtract(y).Mod(p);
            }

            // Range check (matches upstream's defensive check).
            if (x.SignValue < 0 || x.CompareTo(p) >= 0)
            {
                throw new InvalidOperationException("x is out of range");
            }
            if (y.SignValue < 0 || y.CompareTo(p) >= 0)
            {
                throw new InvalidOperationException("y is out of range");
            }

            // Assemble 0x04 || X || Y (each 32 bytes, big-endian, zero-padded).
            var uncompressed = new byte[65];
            uncompressed[0] = 0x04;

            var xHexOut = x.ToString(16).ToLowerInvariant().PadLeft(64, '0');
            var yHexOut = y.ToString(16).ToLowerInvariant().PadLeft(64, '0');

            var xOutputBytes = Encoding.Uint8ArrayFromHexString(xHexOut);
            var yOutputBytes = Encoding.Uint8ArrayFromHexString(yHexOut);

            Array.Copy(xOutputBytes, 0, uncompressed, 1, 32);
            Array.Copy(yOutputBytes, 0, uncompressed, 33, 32);
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
            if (encryptedBuf.Length <= CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
            {
                throw new ArgumentException("Encrypted buffer too small");
            }

            var compressedEncappedPublic = new byte[CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE);

            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);

            var ciphertext = new byte[encryptedBuf.Length - CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
            Array.Copy(encryptedBuf, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE, ciphertext, 0, ciphertext.Length);

            var payload = new HpkeBundlePayload
            {
                EncappedPublic = Encoding.Uint8ArrayToHexString(encappedPublicUncompressed),
                Ciphertext = Encoding.Uint8ArrayToHexString(ciphertext),
            };

            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
        }

        #endregion

        #region turnkey.ts public surface (subset matched to Unity / peak usage)

        /// <summary>
        /// Decrypt a Turnkey credential bundle.
        /// Equivalent to upstream <c>turnkey.ts decryptCredentialBundle</c>.
        /// </summary>
        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string targetPrivateKey)
        {
            byte[] bundleBytes;
            try
            {
                bundleBytes = Encoding.Base58CheckDecode(encryptedCredentialBundle);
            }
            catch
            {
                bundleBytes = Encoding.Base58Decode(encryptedCredentialBundle);
            }

            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
            if (bundleBytes.Length <= compressedSize)
            {
                throw new InvalidOperationException(
                    "Bundle size " + bundleBytes.Length
                    + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
            }

            var compressedKey = new byte[compressedSize];
            Array.Copy(bundleBytes, 0, compressedKey, 0, compressedSize);

            var ciphertext = new byte[bundleBytes.Length - compressedSize];
            Array.Copy(bundleBytes, compressedSize, ciphertext, 0, ciphertext.Length);

            var encappedKey = UncompressRawPublicKey(compressedKey);

            var decryptedData = HpkeDecrypt(new HpkeDecryptParams
            {
                CiphertextBuf = ciphertext,
                EncappedKeyBuf = encappedKey,
                ReceiverPriv = targetPrivateKey,
            });

            return Encoding.Uint8ArrayToHexString(decryptedData);
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

            try
            {
                using var bundleDoc = JsonDocument.Parse(parameters.ImportBundle!);
                var bundle = bundleDoc.RootElement;

                string? enclaveQuorumPublic = GetStringOrNull(bundle, "enclaveQuorumPublic");
                string? dataSignature = GetStringOrNull(bundle, "dataSignature");
                string? signedDataHex = GetStringOrNull(bundle, "data");

                if (string.IsNullOrEmpty(enclaveQuorumPublic)
                    || string.IsNullOrEmpty(dataSignature)
                    || string.IsNullOrEmpty(signedDataHex))
                {
                    throw new InvalidOperationException(
                        "Invalid import bundle format - missing required fields");
                }

                VerifyEnclaveSignature(enclaveQuorumPublic!, dataSignature!, signedDataHex!);

                var signedDataBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
                using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
                var signed = signedDoc.RootElement;

                string? orgId = GetStringOrNull(signed, "organizationId");
                if (!string.Equals(orgId, parameters.OrganizationId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Organization ID mismatch. Expected: " + parameters.OrganizationId
                        + ", got: " + orgId);
                }

                string? userId = GetStringOrNull(signed, "userId");
                if (!string.Equals(userId, parameters.UserId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "User ID mismatch. Expected: " + parameters.UserId + ", got: " + userId);
                }

                string? targetPublic = GetStringOrNull(signed, "targetPublic");
                if (string.IsNullOrEmpty(targetPublic))
                {
                    throw new InvalidOperationException("Import bundle missing targetPublic value");
                }

                var targetKeyBuf = Encoding.Uint8ArrayFromHexString(targetPublic!);
                var plainTextBuf = DecodeKey(parameters.PrivateKey!, parameters.KeyFormat);

                var encryptedBuf = HpkeEncrypt(new HpkeEncryptParams
                {
                    PlainTextBuf = plainTextBuf,
                    TargetKeyBuf = targetKeyBuf,
                });

                return FormatHpkeBuf(encryptedBuf);
            }
            catch (Exception err) when (!(err is ArgumentException) && !(err is ArgumentNullException))
            {
                throw new InvalidOperationException(
                    "Error encrypting private key bundle: " + err.Message, err);
            }
        }

        /// <summary>
        /// Decrypt a Turnkey export bundle (legacy or current envelope).
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
                var bundle = bundleDoc.RootElement;

                string? encappedPublic = GetStringOrNull(bundle, "encappedPublic");
                string? ciphertextStr = GetStringOrNull(bundle, "ciphertext");
                string? signature = GetStringOrNull(bundle, "signature");
                string? signedData = GetStringOrNull(bundle, "signedData");

                JsonDocument? signedDoc = null;
                JsonElement signedDataObj = default;
                bool hasSignedDataObj = false;

                try
                {
                    if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(signedData))
                    {
                        // Legacy envelope: ECDSA over the UTF-8 bytes of `signedData`.
                        if (!VerifySignature(Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY, signature!, signedData!))
                        {
                            throw new InvalidOperationException("Invalid signature on export bundle");
                        }
                        signedDoc = JsonDocument.Parse(signedData!);
                        signedDataObj = signedDoc.RootElement;
                        hasSignedDataObj = true;
                        encappedPublic ??= GetStringOrNull(signedDataObj, "encappedPublic");
                        ciphertextStr ??= GetStringOrNull(signedDataObj, "ciphertext");
                    }
                    else
                    {
                        // Current envelope: enclave-quorum-signed hex blob.
                        string? dataHex = GetStringOrNull(bundle, "data");
                        string? dataSignature = GetStringOrNull(bundle, "dataSignature");
                        string? enclaveQuorumPublic = GetStringOrNull(bundle, "enclaveQuorumPublic");

                        if (string.IsNullOrEmpty(dataHex)
                            || string.IsNullOrEmpty(dataSignature)
                            || string.IsNullOrEmpty(enclaveQuorumPublic))
                        {
                            throw new InvalidOperationException(
                                "Invalid export bundle format - missing required fields");
                        }

                        VerifyEnclaveSignature(enclaveQuorumPublic!, dataSignature!, dataHex!);

                        var signedDataBytes = Encoding.Uint8ArrayFromHexString(dataHex!);
                        var signedJson = System.Text.Encoding.UTF8.GetString(signedDataBytes);
                        signedDoc = JsonDocument.Parse(signedJson);
                        signedDataObj = signedDoc.RootElement;
                        hasSignedDataObj = true;
                        encappedPublic = GetStringOrNull(signedDataObj, "encappedPublic");
                        ciphertextStr = GetStringOrNull(signedDataObj, "ciphertext");
                    }

                    if (hasSignedDataObj)
                    {
                        string? bundleOrgId = GetStringOrNull(signedDataObj, "organizationId");
                        if (!string.IsNullOrEmpty(bundleOrgId)
                            && !string.Equals(bundleOrgId, parameters.OrganizationId, StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                "Organization ID mismatch. Expected: " + parameters.OrganizationId
                                + ", got: " + bundleOrgId);
                        }
                    }

                    if (string.IsNullOrEmpty(encappedPublic) || string.IsNullOrEmpty(ciphertextStr))
                    {
                        throw new InvalidOperationException(
                            "Invalid export bundle format - missing HPKE payload");
                    }

                    var encappedKeyBuf = Encoding.Uint8ArrayFromHexString(encappedPublic!);
                    var ciphertextBuf = Encoding.Uint8ArrayFromHexString(ciphertextStr!);

                    var decryptedData = HpkeDecrypt(new HpkeDecryptParams
                    {
                        CiphertextBuf = ciphertextBuf,
                        EncappedKeyBuf = encappedKeyBuf,
                        ReceiverPriv = parameters.EmbeddedKey,
                    });

                    if (parameters.ReturnMnemonic)
                    {
                        return Encoding.Uint8ArrayToString(decryptedData);
                    }

                    if (string.Equals(parameters.KeyFormat, "SOLANA", StringComparison.OrdinalIgnoreCase))
                    {
                        return Encoding.Base58Encode(decryptedData);
                    }

                    return Encoding.Uint8ArrayToHexString(decryptedData);
                }
                finally
                {
                    signedDoc?.Dispose();
                }
            }
            catch (Exception err) when (!(err is ArgumentException) && !(err is ArgumentNullException))
            {
                throw new InvalidOperationException(
                    "Error decrypting export bundle: " + err.Message, err);
            }
        }

        /// <summary>
        /// Verify the ECDSA signature on a Turnkey session JWT against the
        /// production notarizer's P-256 key.
        /// Equivalent to upstream <c>turnkey.ts verifySessionJwtSignature</c>.
        /// </summary>
        public static bool VerifySessionJwtSignature(string jwt)
        {
            try
            {
                if (string.IsNullOrEmpty(jwt))
                {
                    return false;
                }
                var parts = jwt.Split('.');
                if (parts.Length != 3)
                {
                    return false;
                }

                string headerB64 = parts[0];
                string payloadB64 = parts[1];
                string signatureB64 = parts[2];
                string signingInput = headerB64 + "." + payloadB64;

                // Upstream: double-SHA-256 of the signing input. The signing
                // algorithm is "NONEwithECDSA" with the pre-hashed digest fed in.
                byte[] msgDigest;
                using (var sha256 = SHA256.Create())
                {
                    var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
                    msgDigest = sha256.ComputeHash(h1);
                }

                byte[] signature = Base64UrlDecode(signatureB64);
                if (signature.Length != 64)
                {
                    return false;
                }

                var publicKey = Encoding.Uint8ArrayFromHexString(Constants.PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY);
                return VerifyP256RawSignature(publicKey, signature, msgDigest);
            }
            catch
            {
                return false;
            }
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
            // Upstream `buildLabeledIkm` returns HPKE_VERSION || suiteId || label || ikm.
            return Encoding.ConcatUint8Arrays(Constants.HPKE_VERSION, suiteId, label, ikm);
        }

        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
        {
            // Upstream layout: [0, len, ...HPKE_VERSION, ...suiteId, ...label, ...info].
            // suiteIdStartIndex == 2 + HPKE_VERSION.Length == 9.
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
            // Strict base64url-decoder for JWT signatures. Upstream uses
            // Buffer.from(input, "base64url") in Node which is strict on
            // characters; we mirror that here (no lenient atob).
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
            try
            {
                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                var point = curve.Curve.DecodePoint(publicKeyBytes);
                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

                // Convert raw r||s (64 bytes) to DER-encoded sequence.
                var r = new BigInteger(1, signatureRaw, 0, 32);
                var s = new BigInteger(1, signatureRaw, 32, 32);
                byte[] derSignature = new DerSequence(new DerInteger(r), new DerInteger(s)).GetDerEncoded();

                // The message is already a digest; use NONEwithECDSA.
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

        private static void VerifyEnclaveSignature(string enclaveQuorumPublic, string signatureHex, string signedDataHex)
        {
            if (!string.Equals(enclaveQuorumPublic, Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Signer key " + enclaveQuorumPublic + " is not recognized. Expected: "
                    + Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY);
            }

            var publicKeyBytes = Encoding.Uint8ArrayFromHexString(Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY);
            var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex);
            var messageBytes = Encoding.Uint8ArrayFromHexString(signedDataHex);

            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            var point = curve.Curve.DecodePoint(publicKeyBytes);
            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(false, publicKeyParams);
            signer.BlockUpdate(messageBytes, 0, messageBytes.Length);

            if (!signer.VerifySignature(signatureBytes))
            {
                throw new InvalidOperationException("Failed to verify enclave signature");
            }
        }

        private static bool VerifySignature(string publicKeyHex, string signatureHex, string message)
        {
            try
            {
                var publicKeyBytes = Encoding.Uint8ArrayFromHexString(publicKeyHex);
                var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex);
                var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

                var point = curve.Curve.DecodePoint(publicKeyBytes);
                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
                signer.Init(false, publicKeyParams);
                signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
                return signer.VerifySignature(signatureBytes);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] DecodeKey(string privateKey, string? keyFormat)
        {
            if (string.Equals(keyFormat, "SOLANA", StringComparison.OrdinalIgnoreCase))
            {
                var decoded = Encoding.Base58Decode(privateKey);
                if (decoded.Length != 64)
                {
                    throw new InvalidOperationException(
                        "Invalid Solana private key length. Expected 64 bytes, got " + decoded.Length + ".");
                }
                var privateKeyBytes = new byte[32];
                Array.Copy(decoded, 0, privateKeyBytes, 0, 32);
                return privateKeyBytes;
            }

            string normalized = privateKey.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
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
            // Upstream uses ?.toString() which for a non-string JToken returns
            // its string representation. For Turnkey bundles every queried
            // field is a string; treat non-string values as null so callers
            // see the same "missing field" path they would in JS.
            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        #endregion
    }
}
