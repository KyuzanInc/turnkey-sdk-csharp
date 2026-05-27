// HttpTests.cs — xunit tests for src/Http.cs.

using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Turnkey.Tests
{
    public class HttpTests
    {
        // Upstream api-key-stamper@0.5.0 fixture, reused to drive Http here so
        // that the stamped bodies verify under a known public key.
        private const string FixturePrivateKey =
            "487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c";
        private const string FixturePublicKey =
            "02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316";

        private static Http MakeClient() => Http.FromTargetPrivateKey(FixturePrivateKey);

        [Fact]
        public void FromTargetPrivateKey_RejectsEmpty()
        {
            Action act = () => Http.FromTargetPrivateKey("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromTargetPrivateKey_RejectsInvalidHex()
        {
            Action act = () => Http.FromTargetPrivateKey("zz");
            act.Should().Throw<ArgumentException>()
               .WithMessage("cannot create uint8array from invalid hex string*");
        }

        [Fact]
        public void GetHttpClient_RejectsEmptyArgs()
        {
            Action a1 = () => Http.GetHttpClient("", "abc");
            a1.Should().Throw<ArgumentException>();
            Action a2 = () => Http.GetHttpClient("abc", "");
            a2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void StampGetWhoami_ProducesCorrectUrlAndBody()
        {
            var http = MakeClient();
            var req = http.StampGetWhoami("00000000-0000-0000-0000-000000000000");

            req.Url.Should().Be("https://api.turnkey.com/public/v1/query/whoami");
            req.Body.Should().Be("{\"organizationId\":\"00000000-0000-0000-0000-000000000000\"}");
            req.Stamp.StampHeaderName.Should().Be("X-Stamp");
            req.Stamp.StampHeaderValue.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void StampGetWhoami_EmptyOrg_Throws()
        {
            var http = MakeClient();
            Action act = () => http.StampGetWhoami("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void StampInitImportPrivateKey_ProducesCorrectUrlAndJson()
        {
            var http = MakeClient();
            var body = new Http.InitImportPrivateKeyRequestBody
            {
                OrganizationId = "org-1",
                Type = "ACTIVITY_TYPE_INIT_IMPORT_PRIVATE_KEY",
                TimestampMs = "1729000000000",
                Parameters = new Http.InitImportPrivateKeyParameters
                {
                    UserId = "user-1",
                },
            };

            var req = http.StampInitImportPrivateKey(body);

            req.Url.Should().Be("https://api.turnkey.com/public/v1/submit/init_import_private_key");

            // Field order must be organizationId, type, timestampMs, parameters.
            using var doc = JsonDocument.Parse(req.Body);
            var keys = new System.Collections.Generic.List<string>();
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                keys.Add(p.Name);
            }
            keys.Should().ContainInOrder(new[] { "organizationId", "type", "timestampMs", "parameters" });

            doc.RootElement.GetProperty("organizationId").GetString().Should().Be("org-1");
            doc.RootElement.GetProperty("type").GetString().Should().Be("ACTIVITY_TYPE_INIT_IMPORT_PRIVATE_KEY");
            doc.RootElement.GetProperty("timestampMs").GetString().Should().Be("1729000000000");
            doc.RootElement.GetProperty("parameters").GetProperty("userId").GetString().Should().Be("user-1");
        }

        [Fact]
        public void StampImportPrivateKey_ProducesArrayInBody()
        {
            var http = MakeClient();
            var body = new Http.ImportPrivateKeyRequestBody
            {
                OrganizationId = "org-x",
                Type = "ACTIVITY_TYPE_IMPORT_PRIVATE_KEY",
                TimestampMs = "1729000000123",
                Parameters = new Http.ImportPrivateKeyParameters
                {
                    UserId = "user-x",
                    AddressFormats = new[] { "ADDRESS_FORMAT_ETHEREUM", "ADDRESS_FORMAT_SOLANA" },
                    Curve = "CURVE_SECP256K1",
                    EncryptedBundle = "{\"encappedPublic\":\"04...\",\"ciphertext\":\"ab\"}",
                    PrivateKeyName = "my-key",
                },
            };

            var req = http.StampImportPrivateKey(body);
            req.Url.Should().Be("https://api.turnkey.com/public/v1/submit/import_private_key");

            using var doc = JsonDocument.Parse(req.Body);
            var parameters = doc.RootElement.GetProperty("parameters");
            var formats = parameters.GetProperty("addressFormats");
            formats.ValueKind.Should().Be(JsonValueKind.Array);
            formats.GetArrayLength().Should().Be(2);
            formats[0].GetString().Should().Be("ADDRESS_FORMAT_ETHEREUM");
            formats[1].GetString().Should().Be("ADDRESS_FORMAT_SOLANA");
            parameters.GetProperty("curve").GetString().Should().Be("CURVE_SECP256K1");
        }

        [Fact]
        public void StampExportPrivateKey_ProducesCorrectShape()
        {
            var http = MakeClient();
            var body = new Http.ExportPrivateKeyRequestBody
            {
                OrganizationId = "org-y",
                Type = "ACTIVITY_TYPE_EXPORT_PRIVATE_KEY",
                TimestampMs = "1",
                Parameters = new Http.ExportPrivateKeyParameters
                {
                    PrivateKeyId = "pk-1",
                    TargetPublicKey = FixturePublicKey,
                },
            };

            var req = http.StampExportPrivateKey(body);
            req.Url.Should().Be("https://api.turnkey.com/public/v1/submit/export_private_key");

            using var doc = JsonDocument.Parse(req.Body);
            var p = doc.RootElement.GetProperty("parameters");
            p.GetProperty("privateKeyId").GetString().Should().Be("pk-1");
            p.GetProperty("targetPublicKey").GetString().Should().Be(FixturePublicKey);
        }

        [Fact]
        public void StampExportWalletAccount_ProducesCorrectShape()
        {
            var http = MakeClient();
            var body = new Http.ExportWalletAccountRequestBody
            {
                OrganizationId = "org-z",
                Type = "ACTIVITY_TYPE_EXPORT_WALLET_ACCOUNT",
                TimestampMs = "2",
                Parameters = new Http.ExportWalletAccountParameters
                {
                    Address = "0xabcdef",
                    TargetPublicKey = FixturePublicKey,
                },
            };

            var req = http.StampExportWalletAccount(body);
            req.Url.Should().Be("https://api.turnkey.com/public/v1/submit/export_wallet_account");

            using var doc = JsonDocument.Parse(req.Body);
            var p = doc.RootElement.GetProperty("parameters");
            p.GetProperty("address").GetString().Should().Be("0xabcdef");
            p.GetProperty("targetPublicKey").GetString().Should().Be(FixturePublicKey);
        }

        /// upstream: codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__tests__/request-test.ts:8 "requests are stamped after initialization"
        [Fact]
        public void Stamp_HeaderValueDecodesAndVerifies()
        {
            // End-to-end wire-format check: stamp a whoami request, decode the
            // X-Stamp header, parse the JSON, and crypto-verify the DER
            // signature over SHA-256(body) under the API public key.
            var http = MakeClient();
            var req = http.StampGetWhoami("org-id");

            string decodedJson = Encoding.DecodeBase64UrlToString(req.Stamp.StampHeaderValue);
            using var doc = JsonDocument.Parse(decodedJson);
            var root = doc.RootElement;
            root.GetProperty("publicKey").GetString().Should().Be(FixturePublicKey);
            root.GetProperty("scheme").GetString().Should().Be("SIGNATURE_SCHEME_TK_API_P256");
            string signatureHex = root.GetProperty("signature").GetString()!;
            signatureHex.Should().StartWith("30"); // DER SEQUENCE

            // Verify the signature against SHA-256(body) under the API public key.
            byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(req.Body);
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] digest = sha.ComputeHash(bodyBytes);

            var curve = Org.BouncyCastle.Asn1.X9.ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
            var domain = new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            byte[] pubBytes = Encoding.Uint8ArrayFromHexString(FixturePublicKey);
            var publicKey = new Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(
                curve.Curve.DecodePoint(pubBytes), domain);

            var signer = Org.BouncyCastle.Security.SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(false, publicKey);
            signer.BlockUpdate(digest, 0, digest.Length);
            signer.VerifySignature(Encoding.Uint8ArrayFromHexString(signatureHex)).Should().BeTrue();
        }

        [Fact]
        public void StampNullBody_Throws()
        {
            var http = MakeClient();
            Action a1 = () => http.StampInitImportPrivateKey(null!);
            a1.Should().Throw<ArgumentNullException>();
            Action a2 = () => http.StampImportPrivateKey(null!);
            a2.Should().Throw<ArgumentNullException>();
            Action a3 = () => http.StampExportPrivateKey(null!);
            a3.Should().Throw<ArgumentNullException>();
            Action a4 = () => http.StampExportWalletAccount(null!);
            a4.Should().Throw<ArgumentNullException>();
        }
    }
}
