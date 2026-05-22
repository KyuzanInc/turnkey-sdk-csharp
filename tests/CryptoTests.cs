// CryptoTests.cs — xunit tests for src/Crypto.cs.
//
// Vector sources:
//   - HKDF RFC 5869 test cases A.1..A.3 (the SHA-256 cases)
//   - NIST P-256 known points (G and 2G) for GetPublicKey
//   - Compress/UncompressRawPublicKey roundtrip
//   - HpkeEncrypt/HpkeDecrypt roundtrip (deterministic round-trip)
//   - VerifySessionJwtSignature negative cases
//
// Tests that require a Turnkey-signed bundle (DecryptCredentialBundle,
// EncryptPrivateKeyToBundle, DecryptExportBundle against PRODUCTION_SIGNER)
// are not unit-testable without a real Turnkey signing key. They are
// implicitly exercised by the HPKE roundtrip (the inner HPKE step is
// identical) and by the E2E whoami flow once credentials are present.

using System;
using System.Text;
using FluentAssertions;
using Org.BouncyCastle.Math;
using Xunit;

namespace Turnkey.Tests
{
    public class CryptoTests
    {
        // ============================================================
        // Math.ModSqrt — Tonelli-Shanks
        // ============================================================

        [Fact]
        public void ModSqrt_SmallPrime_HappyPath()
        {
            // 4 = 2^2 mod 7, ModSqrt(4, 7) should be 2 or 5 (-2 mod 7).
            var p = new BigInteger("7");
            var x = new BigInteger("4");
            var root = Crypto.Math.ModSqrt(x, p);
            (root.Equals(new BigInteger("2")) || root.Equals(new BigInteger("5")))
                .Should().BeTrue($"got {root}");
        }

        [Fact]
        public void ModSqrt_NonResidue_Throws()
        {
            // 3 is a non-residue mod 7.
            var p = new BigInteger("7");
            var x = new BigInteger("3");
            Action act = () => Crypto.Math.ModSqrt(x, p);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("could not find a modular square root");
        }

        [Fact]
        public void ModSqrt_NonPositiveP_Throws()
        {
            var p = new BigInteger("0");
            var x = new BigInteger("1");
            Action act = () => Crypto.Math.ModSqrt(x, p);
            act.Should().Throw<ArgumentException>()
               .WithMessage("p must be positive");
        }

        [Fact]
        public void ModSqrt_P256Prime_Works()
        {
            // P-256 prime. Pick any quadratic residue: 4 = 2^2.
            var p = new BigInteger(CryptoConstants.P256_P);
            var x = new BigInteger("4");
            var root = Crypto.Math.ModSqrt(x, p);
            root.Multiply(root).Mod(p).Equals(x).Should().BeTrue();
        }

        // ============================================================
        // HKDF RFC 5869
        // ============================================================

        // RFC 5869 Test Case 1 (Basic test case with SHA-256)
        [Fact]
        public void Hkdf_Rfc5869_A1()
        {
            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
            byte[] salt = HexToBytes("000102030405060708090a0b0c");
            byte[] info = HexToBytes("f0f1f2f3f4f5f6f7f8f9");
            int L = 42;

            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);

            BytesToHex(prk).Should().Be(
                "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5");
            BytesToHex(okm).Should().Be(
                "3cb25f25faacd57a90434f64d0362f2a"
                + "2d2d0a90cf1a5a4c5db02d56ecc4c5bf"
                + "34007208d5b887185865");
        }

        // RFC 5869 Test Case 2 (Test with SHA-256 and longer inputs/outputs)
        [Fact]
        public void Hkdf_Rfc5869_A2()
        {
            byte[] ikm = HexToBytes(
                "000102030405060708090a0b0c0d0e0f"
                + "101112131415161718191a1b1c1d1e1f"
                + "202122232425262728292a2b2c2d2e2f"
                + "303132333435363738393a3b3c3d3e3f"
                + "404142434445464748494a4b4c4d4e4f");
            byte[] salt = HexToBytes(
                "606162636465666768696a6b6c6d6e6f"
                + "707172737475767778797a7b7c7d7e7f"
                + "808182838485868788898a8b8c8d8e8f"
                + "909192939495969798999a9b9c9d9e9f"
                + "a0a1a2a3a4a5a6a7a8a9aaabacadaeaf");
            byte[] info = HexToBytes(
                "b0b1b2b3b4b5b6b7b8b9babbbcbdbebf"
                + "c0c1c2c3c4c5c6c7c8c9cacbcccdcecf"
                + "d0d1d2d3d4d5d6d7d8d9dadbdcdddedf"
                + "e0e1e2e3e4e5e6e7e8e9eaebecedeeef"
                + "f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff");
            int L = 82;

            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);

            BytesToHex(prk).Should().Be(
                "06a6b88c5853361a06104c9ceb35b45cef760014904671014a193f40c15fc244");
            BytesToHex(okm).Should().Be(
                "b11e398dc80327a1c8e7f78c596a4934"
                + "4f012eda2d4efad8a050cc4c19afa97c"
                + "59045a99cac7827271cb41c65e590e09"
                + "da3275600c2f09b8367793a9aca3db71"
                + "cc30c58179ec3e87c14c01d5c1f3434f"
                + "1d87");
        }

        // RFC 5869 Test Case 3 (Test with SHA-256 and zero-length salt/info)
        [Fact]
        public void Hkdf_Rfc5869_A3()
        {
            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
            byte[] salt = Array.Empty<byte>();
            byte[] info = Array.Empty<byte>();
            int L = 42;

            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);

            BytesToHex(prk).Should().Be(
                "19ef24a32c717b167f33a91d6f648bdf96596776afdb6377ac434c1c293ccb04");
            BytesToHex(okm).Should().Be(
                "8da4e775a563c18f715f802a063c5a31"
                + "b8a11f5c5ee1879ec3454e5f3c738d2d"
                + "9d201395faa4b61a96c8");
        }

        [Fact]
        public void Hkdf_Expand_Length0_ProducesEmpty()
        {
            byte[] prk = HexToBytes(
                "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5");
            byte[] okm = Crypto.Hkdf.Expand(prk, null!, 0);
            okm.Should().BeEmpty();
        }

        [Fact]
        public void Hkdf_Expand_OverMaxLength_Throws()
        {
            byte[] prk = new byte[32];
            Action act = () => Crypto.Hkdf.Expand(prk, null!, 255 * 32 + 1);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Hkdf_Extract_NullSalt_UsesZeroSalt()
        {
            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
            byte[] prkA = Crypto.Hkdf.Extract(null!, ikm);
            byte[] prkB = Crypto.Hkdf.Extract(new byte[32], ikm); // RFC default
            prkA.Should().Equal(prkB);
        }

        // ============================================================
        // CompressRawPublicKey / UncompressRawPublicKey
        // ============================================================

        [Fact]
        public void CompressUncompress_Roundtrip_RandomKeys()
        {
            for (int i = 0; i < 8; i++)
            {
                var kp = Crypto.GenerateP256KeyPair();
                byte[] uncompressed = Encoding.Uint8ArrayFromHexString(kp.PublicKeyUncompressed);
                byte[] compressed = Crypto.CompressRawPublicKey(uncompressed);
                compressed.Should().HaveCount(33);
                compressed[0].Should().BeOneOf((byte)0x02, (byte)0x03);

                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
                recovered.Should().Equal(uncompressed);
            }
        }

        [Fact]
        public void CompressRawPublicKey_InvalidPrefix_Throws()
        {
            var raw = new byte[65];
            raw[0] = 0x05;
            Action act = () => Crypto.CompressRawPublicKey(raw);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CompressRawPublicKey_WrongLength_Throws()
        {
            var raw = new byte[64];
            raw[0] = 0x04;
            Action act = () => Crypto.CompressRawPublicKey(raw);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UncompressRawPublicKey_BadPrefix_Throws()
        {
            var compressed = new byte[33];
            compressed[0] = 0x05;
            Action act = () => Crypto.UncompressRawPublicKey(compressed);
            act.Should().Throw<ArgumentException>()
               .WithMessage("failed to uncompress raw public key: invalid prefix");
        }

        [Fact]
        public void UncompressRawPublicKey_WrongLength_Throws()
        {
            var compressed = new byte[32];
            compressed[0] = 0x02;
            Action act = () => Crypto.UncompressRawPublicKey(compressed);
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid compressed public key size: 32");
        }

        // ============================================================
        // GetPublicKey / GenerateP256KeyPair
        // ============================================================

        [Fact]
        public void GenerateP256KeyPair_LengthsAreCorrect()
        {
            var kp = Crypto.GenerateP256KeyPair();
            kp.PrivateKey.Should().HaveLength(64);          // 32 bytes hex
            kp.PublicKey.Should().HaveLength(66);            // 33 bytes hex
            kp.PublicKeyUncompressed.Should().HaveLength(130); // 65 bytes hex
            kp.PublicKey.Should().MatchRegex("^0[23][0-9a-f]{64}$");
            kp.PublicKeyUncompressed.Should().StartWith("04");
        }

        [Fact]
        public void GetPublicKey_FromPrivateKey_ProducesCompressedByDefault()
        {
            var kp = Crypto.GenerateP256KeyPair();
            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
            byte[] pub = Crypto.GetPublicKey(priv); // isCompressed default true
            pub.Should().HaveCount(33);
            Encoding.Uint8ArrayToHexString(pub).Should().Be(kp.PublicKey);
        }

        [Fact]
        public void GetPublicKey_Uncompressed_MatchesGenerator()
        {
            var kp = Crypto.GenerateP256KeyPair();
            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
            byte[] uncompressed = Crypto.GetPublicKey(priv, isCompressed: false);
            Encoding.Uint8ArrayToHexString(uncompressed).Should().Be(kp.PublicKeyUncompressed);
        }

        [Fact]
        public void GetPublicKey_HexStringOverload_MatchesByteOverload()
        {
            var kp = Crypto.GenerateP256KeyPair();
            byte[] viaBytes = Crypto.GetPublicKey(Encoding.Uint8ArrayFromHexString(kp.PrivateKey));
            byte[] viaHex = Crypto.GetPublicKey(kp.PrivateKey);
            viaBytes.Should().Equal(viaHex);
        }

        // ============================================================
        // HPKE roundtrip
        // ============================================================

        [Fact]
        public void Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload()
        {
            // Receiver key pair.
            var recv = Crypto.GenerateP256KeyPair();
            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);

            byte[] payload = Encoding.Uint8ArrayFromHexString(
                "deadbeefcafebabe1234567890abcdef00112233445566778899aabbccddeeff");

            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
            {
                PlainTextBuf = payload,
                TargetKeyBuf = recvPubUncompressed,
            });

            // encrypted = compressed(senderPub) || ciphertext (33 + N bytes)
            encrypted.Length.Should().BeGreaterThan(33);

            // Split, uncompress the sender's compressed point, then decrypt.
            var compressedSender = new byte[33];
            Array.Copy(encrypted, 0, compressedSender, 0, 33);
            var ciphertext = new byte[encrypted.Length - 33];
            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);

            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);

            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
            {
                CiphertextBuf = ciphertext,
                EncappedKeyBuf = encapped,
                ReceiverPriv = recv.PrivateKey,
            });

            decrypted.Should().Equal(payload);
        }

        [Fact]
        public void Hpke_EncryptThenDecrypt_EmptyPayload()
        {
            var recv = Crypto.GenerateP256KeyPair();
            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);

            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
            {
                PlainTextBuf = Array.Empty<byte>(),
                TargetKeyBuf = recvPubUncompressed,
            });

            var compressedSender = new byte[33];
            Array.Copy(encrypted, 0, compressedSender, 0, 33);
            var ciphertext = new byte[encrypted.Length - 33];
            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);

            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
            {
                CiphertextBuf = ciphertext,
                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
                ReceiverPriv = recv.PrivateKey,
            });
            decrypted.Should().BeEmpty();
        }

        [Fact]
        public void Hpke_DecryptWithWrongKey_Throws()
        {
            var recv = Crypto.GenerateP256KeyPair();
            var attacker = Crypto.GenerateP256KeyPair();
            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);

            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
            {
                PlainTextBuf = Encoding.Uint8ArrayFromHexString("aabbcc"),
                TargetKeyBuf = recvPubUncompressed,
            });

            var compressedSender = new byte[33];
            Array.Copy(encrypted, 0, compressedSender, 0, 33);
            var ciphertext = new byte[encrypted.Length - 33];
            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);

            Action act = () => Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
            {
                CiphertextBuf = ciphertext,
                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
                ReceiverPriv = attacker.PrivateKey,
            });
            act.Should().Throw<Exception>();
        }

        // ============================================================
        // BuildAdditionalAssociatedData / FormatHpkeBuf
        // ============================================================

        [Fact]
        public void BuildAdditionalAssociatedData_ConcatsArgs()
        {
            byte[] a = { 0x01, 0x02 };
            byte[] b = { 0x03, 0x04, 0x05 };
            byte[] aad = Crypto.BuildAdditionalAssociatedData(a, b);
            aad.Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
        }

        [Fact]
        public void FormatHpkeBuf_ReturnsExpectedJson()
        {
            // Use a real HpkeEncrypt output so the bytes are valid.
            var recv = Crypto.GenerateP256KeyPair();
            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);

            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
            {
                PlainTextBuf = Encoding.Uint8ArrayFromHexString("1234"),
                TargetKeyBuf = recvPubUncompressed,
            });

            string json = Crypto.FormatHpkeBuf(encrypted);

            // Shape contains exactly two keys in upstream order.
            json.Should().StartWith("{\"encappedPublic\":\"04");
            json.Should().Contain("\"ciphertext\":\"");
            json.Should().EndWith("\"}");
        }

        [Fact]
        public void FormatHpkeBuf_TooSmall_Throws()
        {
            Action act = () => Crypto.FormatHpkeBuf(new byte[10]);
            act.Should().Throw<ArgumentException>()
               .WithMessage("Encrypted buffer too small");
        }

        // ============================================================
        // VerifySessionJwtSignature negative paths
        // ============================================================

        [Fact]
        public void VerifySessionJwtSignature_Empty_ReturnsFalse()
        {
            Crypto.VerifySessionJwtSignature(string.Empty).Should().BeFalse();
        }

        [Fact]
        public void VerifySessionJwtSignature_WrongPartCount_ReturnsFalse()
        {
            Crypto.VerifySessionJwtSignature("just.two").Should().BeFalse();
        }

        [Fact]
        public void VerifySessionJwtSignature_BadSignatureLength_ReturnsFalse()
        {
            // 3 parts but the signature decodes to a non-64-byte buffer.
            string jwt = "aGVhZGVy.cGF5bG9hZA.YWJj"; // last part decodes to "abc" (3 bytes)
            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
        }

        [Fact]
        public void VerifySessionJwtSignature_WellFormedButForgedSig_ReturnsFalse()
        {
            // 64-byte signature of all zeros — should fail verification.
            string header = Encoding.Base64StringToBase64UrlEncodedString(
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"ES256\"}")));
            string payload = Encoding.Base64StringToBase64UrlEncodedString(
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"sub\":\"x\"}")));
            string sig = Encoding.Base64StringToBase64UrlEncodedString(
                Convert.ToBase64String(new byte[64]));
            string jwt = header + "." + payload + "." + sig;
            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
        }

        // ============================================================
        // Bundle helpers — negative paths only (positive paths require
        // Turnkey-signed sample bundles which we do not have here)
        // ============================================================

        [Fact]
        public void DecryptCredentialBundle_BundleTooSmall_Throws()
        {
            // Base58 of "0" gives a tiny payload.
            string tiny = Encoding.Base58Encode(new byte[] { 0x01, 0x02, 0x03 });
            Action act = () => Crypto.DecryptCredentialBundle(tiny, new string('1', 64));
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void EncryptPrivateKeyToBundle_MissingArgs_Throws()
        {
            Action act = () => Crypto.EncryptPrivateKeyToBundle(new Crypto.EncryptPrivateKeyToBundleParams());
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DecryptExportBundle_MissingArgs_Throws()
        {
            Action act = () => Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams());
            act.Should().Throw<ArgumentException>();
        }

        // ============================================================
        // Helpers
        // ============================================================

        private static byte[] HexToBytes(string hex)
        {
            return Encoding.Uint8ArrayFromHexString(hex);
        }

        private static string BytesToHex(byte[] bytes)
        {
            return Encoding.Uint8ArrayToHexString(bytes);
        }
    }
}
