// ApiKeyStamperTests.cs — xunit tests for src/ApiKeyStamper.cs.
//
// Vector sources:
//   tests/UpstreamSources/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{private,public}
//   tests/UpstreamSources/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts
//
// P-256 ECDSA signatures are not deterministic across libraries (noble uses
// RFC 6979; BouncyCastle's HMacDsaKCalculator also uses RFC 6979 with the
// same digest, so for identical input + key + nonce-source the signature
// is deterministic). Tests therefore assert:
//   - the header name == "X-Stamp"
//   - the base64url-decoded JSON has keys ["publicKey", "scheme", "signature"]
//   - publicKey matches the configured key
//   - scheme == "SIGNATURE_SCHEME_TK_API_P256"
//   - the signature is a valid DER-encoded ECDSA signature over SHA-256(content)
//     under the configured public key

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Xunit;

namespace Turnkey.Tests
{
    public class ApiKeyStamperTests
    {
        // From upstream __fixtures__/api-key.{private,public}
        private const string FixturePrivateKey =
            "487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c";
        private const string FixturePublicKey =
            "02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316";

        [Fact]
        public void Constructor_DefersValidation()
        {
            // Upstream `new ApiKeyStamper({apiPublicKey, apiPrivateKey})` only
            // assigns fields; validation happens at sign time. C# mirrors
            // that, including null tolerance.
            Action act = () => new ApiKeyStamper(FixturePublicKey, "abcd");
            act.Should().NotThrow();
            Action act2 = () => new ApiKeyStamper(null!, null!);
            act2.Should().NotThrow();
        }

        [Fact]
        public void SignWithApiKey_RejectsBadKeyLength()
        {
            // Validation now occurs in SignWithApiKey -> Crypto.GetPublicKey.
            var stamper = new ApiKeyStamper(FixturePublicKey, "abcd");
            Action act = () => stamper.SignWithApiKey("payload");
            act.Should().Throw<ArgumentException>()
               .WithMessage("invalid P-256 private key: expected 32 bytes, got 2");
        }

        [Fact]
        public void SignWithApiKey_RejectsZeroScalar()
        {
            string zero = new string('0', 64);
            var stamper = new ApiKeyStamper(FixturePublicKey, zero);
            Action act = () => stamper.SignWithApiKey("payload");
            act.Should().Throw<ArgumentException>()
               .WithMessage("invalid P-256 private key: scalar must be in [1, n - 1]");
        }

        /// upstream: tests/UpstreamSources/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:6 "uses provided signature to make stamp"
        [Fact]
        public void Stamp_UpstreamFixture_ProducesValidWireBytes()
        {
            // Matches upstream __tests__/stamp-test.ts:6-37
            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
            const string messageToSign = "hello from TKHQ!";

            var result = stamper.Stamp(messageToSign);

            result.StampHeaderName.Should().Be("X-Stamp");

            // Decode base64url -> JSON -> object.
            string decodedJson = Encoding.DecodeBase64UrlToString(result.StampHeaderValue);

            using var doc = JsonDocument.Parse(decodedJson);
            var root = doc.RootElement;

            // Keys must be exactly publicKey, scheme, signature in this order.
            int i = 0;
            string[] expectedKeys = { "publicKey", "scheme", "signature" };
            foreach (var prop in root.EnumerateObject())
            {
                prop.Name.Should().Be(expectedKeys[i++]);
            }
            i.Should().Be(3);

            root.GetProperty("publicKey").GetString().Should().Be(FixturePublicKey);
            root.GetProperty("scheme").GetString().Should().Be("SIGNATURE_SCHEME_TK_API_P256");

            // Verify the DER ECDSA signature against SHA-256(content) under the
            // public key. This is the wire-format gate: if the signature isn't
            // verifiable against the configured P-256 key, the stamp would be
            // rejected by Turnkey's backend.
            string signatureHex = root.GetProperty("signature").GetString()!;
            byte[] signatureBytes = HexToBytes(signatureHex);
            AssertSignatureVerifies(FixturePublicKey, messageToSign, signatureBytes);
        }

        [Fact]
        public void SignWithApiKey_ProducesDerHexThatVerifies()
        {
            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
            const string content = "any payload we care to sign";
            string sigHex = stamper.SignWithApiKey(content);

            // Output is DER hex; even length, starts with 30 (SEQUENCE tag).
            (sigHex.Length % 2).Should().Be(0);
            sigHex.Substring(0, 2).Should().Be("30");

            AssertSignatureVerifies(FixturePublicKey, content, HexToBytes(sigHex));
        }

        [Fact]
        public void SignWithApiKey_DeterministicWithRfc6979()
        {
            // RFC 6979 + same key + same content + same SHA-256(content) +
            // same low-S normalization => signature is bit-identical across
            // invocations (HMacDsaKCalculator is deterministic).
            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
            string a = stamper.SignWithApiKey("the same content");
            string b = stamper.SignWithApiKey("the same content");
            a.Should().Be(b);
        }

        [Fact]
        public void SignWithApiKey_LowSEnforced()
        {
            // For RFC 6979 + low-S: the s value's high bit (top byte) must be
            // strictly less than (n / 2 + 1). Verify by parsing the DER and
            // checking the s integer is in the lower half of [1, n-1].
            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
            for (int i = 0; i < 8; i++)
            {
                string sigHex = stamper.SignWithApiKey("content_" + i);
                var (r, s) = ParseDerEcdsa(HexToBytes(sigHex));

                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
                var halfN = curve.N.ShiftRight(1);
                s.CompareTo(halfN).Should().BeLessOrEqualTo(0,
                    "low-S requires s to be in (0, n/2]");
                r.SignValue.Should().BeGreaterThan(0);
                s.SignValue.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public void SignWithApiKey_WrongPublicKey_Throws()
        {
            // Mismatched public key (any other valid P-256 compressed key).
            var other = Crypto.GenerateP256KeyPair();
            Action act = () =>
            {
                var stamper = new ApiKeyStamper(other.PublicKey, FixturePrivateKey);
                stamper.SignWithApiKey("payload");
            };
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Bad API key. Expected to get public key*");
        }

        [Fact]
        public void SignWithApiKey_UpperCaseConfiguredPublicKey_IsAccepted()
        {
            // The keypair is valid; only the hex spelling of the configured
            // public key differs. An ordinal string comparison rejected it and
            // reported "expected X, got Y" with two values that read as
            // identical. The comparison is over decoded bytes now.
            var stamper = new ApiKeyStamper(FixturePublicKey.ToUpperInvariant(), FixturePrivateKey);

            string sigHex = stamper.SignWithApiKey("payload");

            AssertSignatureVerifies(FixturePublicKey, "payload", HexToBytes(sigHex));
        }

        [Fact]
        public void SignWithApiKey_PublicKeyCasing_DoesNotAffectSignatureBytes()
        {
            string lowerSig = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey)
                .SignWithApiKey("payload");
            string upperSig = new ApiKeyStamper(FixturePublicKey.ToUpperInvariant(), FixturePrivateKey)
                .SignWithApiKey("payload");

            upperSig.Should().Be(lowerSig);
        }

        [Fact]
        public void Stamp_UpperCaseConfiguredPublicKey_IsEmbeddedCanonicalized()
        {
            // Accepting either spelling made this path reachable for the first
            // time (it previously threw), so the stamp must not forward an
            // unusual spelling to the backend. The value embedded is the
            // canonical lower-case key derived from the private key, not the
            // caller's configured spelling.
            var stamper = new ApiKeyStamper(FixturePublicKey.ToUpperInvariant(), FixturePrivateKey);

            var result = stamper.Stamp("hello");

            string decodedJson = Encoding.DecodeBase64UrlToString(result.StampHeaderValue);
            using var doc = JsonDocument.Parse(decodedJson);
            doc.RootElement.GetProperty("publicKey").GetString()
                .Should().Be(FixturePublicKey);
        }

        [Fact]
        public void Stamp_LowerCaseConfiguredPublicKey_WireBytesAreUnchanged()
        {
            // The regression that actually matters for canonicalizing the
            // embedded public key: the normal lower-case path must emit exactly
            // the bytes it emitted before that change. This golden value was
            // captured by running Stamp() against the pre-change implementation,
            // so it is independent of the current code rather than a snapshot of
            // whatever it happens to produce today.
            const string preChangeStampHeaderValue =
                "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2"
                + "YjhjMTYzMDI1MDg4NzAzMTYiLCJzY2hlbWUiOiJTSUdOQVRVUkVfU0NIRU1FX1RLX0FQSV9QMjU2Iiwic2ln"
                + "bmF0dXJlIjoiMzA0NDAyMjA0YWVlN2E4ZTA0NDM2ZWNkMGVhMTZlNzRlYWFiY2MyYjUzYzBjMDdjOTdjMmQx"
                + "NmZiY2RkMTA1NTI2OTFlMjkwMDIyMDBhYTYzMjAyYjhlNTYzNGRhYTk2MTA0Zjk0NzEzNjliNzhhNDAxNzVi"
                + "ZWMzZTAwYzQzMjU5ZGRkMjBlNjY2MGIifQ";

            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);

            stamper.Stamp("hello").StampHeaderValue
                .Should().Be(preChangeStampHeaderValue);
        }

        [Fact]
        public void Stamp_PublicKeyCasing_DoesNotAffectWireBytes()
        {
            // Canonicalization on emit means the whole stamp — not just the
            // signature — is byte-identical regardless of configured spelling.
            var lower = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey).Stamp("hello");
            var upper = new ApiKeyStamper(FixturePublicKey.ToUpperInvariant(), FixturePrivateKey)
                .Stamp("hello");

            upper.StampHeaderValue.Should().Be(lower.StampHeaderValue);
        }

        [Fact]
        public void SignWithApiKey_MalformedConfiguredPublicKey_ThrowsBadApiKey()
        {
            // A public key that cannot be hex-decoded must still surface as the
            // "Bad API key" mismatch, not as an ArgumentException escaping from
            // the hex decoder.
            var stamper = new ApiKeyStamper("not-hex", FixturePrivateKey);

            Action act = () => stamper.SignWithApiKey("payload");

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Bad API key. Expected to get public key*");
        }

        [Fact]
        public void SignWithApiKey_NullConfiguredPublicKey_ThrowsBadApiKey()
        {
            var stamper = new ApiKeyStamper(null!, FixturePrivateKey);

            Action act = () => stamper.SignWithApiKey("payload");

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Bad API key. Expected to get public key*");
        }

        [Fact]
        public void SignWithApiKey_InvalidHexPrivateKey_DoesNotEchoTheKey()
        {
            const string keyShaped =
                "00112233445566778899aabbccddeeff00112233445566778899aabbccddeezz";
            var stamper = new ApiKeyStamper(FixturePublicKey, keyShaped);

            Action act = () => stamper.SignWithApiKey("payload");

            var message = act.Should().Throw<ArgumentException>().Which.Message;
            message.Should().NotContain(keyShaped);
            message.Should().NotContain("00112233");
        }

        [Fact]
        public void SignWithApiKey_RepeatedCalls_StayStableAfterKeyBufferZeroing()
        {
            // The decoded private key is zeroed in a finally on every call. The
            // stamper must therefore re-decode from the configured string each
            // time rather than depend on a buffer it has already wiped.
            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
            string first = stamper.SignWithApiKey("payload");

            for (int i = 0; i < 5; i++)
            {
                stamper.SignWithApiKey("noise " + i);
            }

            stamper.SignWithApiKey("payload").Should().Be(first);
        }

        [Fact]
        public void Stamp_HeaderConstantsAndScheme()
        {
            ApiKeyStamper.StampHeaderName.Should().Be("X-Stamp");
            ApiKeyStamper.SignatureScheme.Should().Be("SIGNATURE_SCHEME_TK_API_P256");
        }

        [Fact]
        public void StampHeaderValue_IsAsciiUrlSafe()
        {
            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
            var result = stamper.Stamp("hello");
            // base64url alphabet: A-Z a-z 0-9 - _
            foreach (char c in result.StampHeaderValue)
            {
                bool ok = (c >= 'A' && c <= 'Z')
                          || (c >= 'a' && c <= 'z')
                          || (c >= '0' && c <= '9')
                          || c == '-' || c == '_';
                ok.Should().BeTrue($"char '{c}' is not in base64url alphabet");
            }
        }

        // ============================================================
        // Node-generated compatibility fixture
        //
        // Loads tests/Fixtures/api-key-stamper/turnkey-stamper-node-vectors.json
        // (produced by tests/Fixtures/Generators/generate-stamper-vectors.mjs)
        // and confirms that for each upstream message:
        //
        //   1. C# stamper produces a stamp with the SAME publicKey,
        //      SAME scheme, and SAME JSON key order as upstream.
        //   2. C# stamper produces the SAME RFC 6979 deterministic r
        //      value as upstream (the s value differs because C# enforces
        //      low-S while upstream noble defaults to high-S; the
        //      cryptographic equivalence is s' = n - s).
        //   3. Both the upstream signature AND the C# signature verify
        //      against the public key over SHA-256(message), proving
        //      cryptographic equivalence.
        //
        // This is the "deterministic byte parity" gate for the stamper,
        // expressed at the level where the two implementations agree.
        // ============================================================

        /// upstream: tests/UpstreamSources/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:17 "sign with Turnkey fixture: sign (PureJS)"
        [Fact]
        public void NodeFixture_StamperByteParity_PureJsRfc6979()
        {
            // Resolve the fixture path. With CopyToOutputDirectory=PreserveNewest,
            // the test runner copies the fixture next to the assembly.
            string fixturePath = Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "api-key-stamper",
                "turnkey-stamper-node-vectors.json");
            File.Exists(fixturePath).Should().BeTrue(
                "fixture must be committed and copied to test output");

            using var doc = JsonDocument.Parse(File.ReadAllText(fixturePath));
            var root = doc.RootElement;

            // Provenance sanity: must be node-generated with the pinned
            // upstream package + a noble-curves 1.3.0 override.
            var prov = root.GetProperty("_provenance");
            prov.GetProperty("level").GetString().Should().Be("node-generated");
            prov.GetProperty("runtime_override").GetString().Should().Be("purejs");

            var preflight = root.GetProperty("preflight");
            preflight.GetProperty("deterministic").GetBoolean().Should().BeTrue();
            preflight.GetProperty("schemeOk").GetBoolean().Should().BeTrue();
            preflight.GetProperty("keyOrderOk").GetBoolean().Should().BeTrue();

            var stamps = root.GetProperty("stamps");
            stamps.GetArrayLength().Should().BeGreaterThan(0);

            foreach (var entry in stamps.EnumerateArray())
            {
                var input = entry.GetProperty("input");
                string message = input.GetProperty("message").GetString()!;
                string privHex = input.GetProperty("privateKey").GetString()!;
                string pubHex = input.GetProperty("publicKey").GetString()!;

                var expected = entry.GetProperty("expected");
                string upstreamScheme = expected.GetProperty("decoded")
                    .GetProperty("scheme").GetString()!;
                string upstreamPublicKey = expected.GetProperty("decoded")
                    .GetProperty("publicKey").GetString()!;
                string upstreamSignatureDerHex = expected.GetProperty("decoded")
                    .GetProperty("signature").GetString()!;

                var derived = expected.GetProperty("derived");
                string expectedRHex = derived.GetProperty("r_hex").GetString()!;
                string expectedLowSHex =
                    derived.GetProperty("low_s_equivalent_s_hex").GetString()!;

                // C# stamper.
                var stamper = new ApiKeyStamper(pubHex, privHex);
                var stamp = stamper.Stamp(message);

                stamp.StampHeaderName.Should().Be("X-Stamp");
                string decodedJson = Encoding.DecodeBase64UrlToString(stamp.StampHeaderValue);
                using var stampDoc = JsonDocument.Parse(decodedJson);
                var stampRoot = stampDoc.RootElement;

                // JSON key order: ["publicKey", "scheme", "signature"]
                var propNames = new System.Collections.Generic.List<string>();
                foreach (var p in stampRoot.EnumerateObject()) propNames.Add(p.Name);
                propNames.Should().Equal("publicKey", "scheme", "signature");

                stampRoot.GetProperty("publicKey").GetString().Should().Be(pubHex);
                stampRoot.GetProperty("scheme").GetString().Should().Be(upstreamScheme);

                string csSigHex = stampRoot.GetProperty("signature").GetString()!;
                var (csR, csS) = ParseDerEcdsa(HexToBytes(csSigHex));
                csR.ToString(16).TrimStart('0').Should().BeEquivalentTo(
                    expectedRHex.TrimStart('0'),
                    "C# RFC 6979 must derive the same k => same r as upstream");
                csS.ToString(16).TrimStart('0').Should().BeEquivalentTo(
                    expectedLowSHex.TrimStart('0'),
                    "C# must emit the low-S form of upstream's high-S signature");

                // Upstream's high-S sig and C# low-S sig both verify against
                // the same public key over the same SHA-256(message).
                AssertSignatureVerifies(pubHex, message, HexToBytes(upstreamSignatureDerHex));
                AssertSignatureVerifies(pubHex, message, HexToBytes(csSigHex));

                // The publicKey in the upstream and C# stamps must match
                // exactly (independent of the s-form difference).
                upstreamPublicKey.Should().Be(pubHex);
            }
        }

        // ============================================================
        // Helpers
        // ============================================================

        private static byte[] HexToBytes(string hex) => Encoding.Uint8ArrayFromHexString(hex);

        private static void AssertSignatureVerifies(string compressedPubHex, string content, byte[] derSignature)
        {
            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(content);

            var digest = DigestUtilities.GetDigest("SHA-256");
            digest.BlockUpdate(payloadBytes, 0, payloadBytes.Length);
            var hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            byte[] compressedPub = Encoding.Uint8ArrayFromHexString(compressedPubHex);
            var point = curve.Curve.DecodePoint(compressedPub);
            var publicKey = new ECPublicKeyParameters(point, domainParams);

            var signer = SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(false, publicKey);
            signer.BlockUpdate(hash, 0, hash.Length);
            signer.VerifySignature(derSignature).Should().BeTrue();
        }

        private static (BigInteger R, BigInteger S) ParseDerEcdsa(byte[] der)
        {
            using var ms = new MemoryStream(der);
            using var reader = new Asn1InputStream(ms);
            var seq = (DerSequence)reader.ReadObject();
            var r = ((DerInteger)seq[0]).Value;
            var s = ((DerInteger)seq[1]).Value;
            return (r, s);
        }
    }
}
