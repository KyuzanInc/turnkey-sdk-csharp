// HttpTests.cs — xunit tests for src/Http.cs.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// upstream: tests/UpstreamSources/turnkey-http-3.16.0/ts-source/__tests__/request-test.ts:8 "requests are stamped after initialization"
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

        // ========================================================
        // Key-material handling
        // ========================================================

        [Fact]
        public void FromTargetPrivateKey_InvalidHex_DoesNotEchoTheKey()
        {
            const string keyShaped =
                "00112233445566778899aabbccddeeff00112233445566778899aabbccddeezz";

            Action act = () => Http.FromTargetPrivateKey(keyShaped);

            var message = act.Should().Throw<ArgumentException>().Which.Message;
            message.Should().NotContain(keyShaped);
            message.Should().NotContain("00112233");
        }

        [Fact]
        public void FromTargetPrivateKey_UpperCaseHex_ProducesIdenticalWireBytes()
        {
            // Pins the normalization in FromTargetPrivateKey: the stamp JSON
            // embeds the public key, so a caller-supplied upper-case private key
            // must still yield lower-case wire bytes identical to the
            // lower-case-input case.
            var fromLower = Http.FromTargetPrivateKey(FixturePrivateKey);
            var fromUpper = Http.FromTargetPrivateKey(FixturePrivateKey.ToUpperInvariant());

            var a = fromLower.StampGetWhoami("org-id");
            var b = fromUpper.StampGetWhoami("org-id");

            b.Body.Should().Be(a.Body);
            b.Stamp.StampHeaderValue.Should().Be(a.Stamp.StampHeaderValue);

            string decodedJson = Encoding.DecodeBase64UrlToString(b.Stamp.StampHeaderValue);
            using var doc = JsonDocument.Parse(decodedJson);
            doc.RootElement.GetProperty("publicKey").GetString().Should().Be(FixturePublicKey);
        }

        // ========================================================
        // Ill-formed UTF-16
        // ========================================================

        [Fact]
        public void SystemTextJson_SilentlyReplacesLoneSurrogates()
        {
            // Characterization of the behavior the gate exists for. If a future
            // runtime stops substituting, this test says so.
            var body = new Http.WhoamiRequestBody { OrganizationId = "org-\uD800-id" };

            string json = JsonSerializer.Serialize(
                body, typeof(Http.WhoamiRequestBody), TurnkeyJsonContext.JsCompatibleOptions);

            json.Should().NotContain("\uD800");
            json.Should().Contain("uFFFD");
        }

        [Fact]
        public void StampGetWhoami_UnpairedHighSurrogate_IsRejected()
        {
            var http = MakeClient();

            Action act = () => http.StampGetWhoami("org-\uD800-id");

            act.Should().Throw<ArgumentException>()
               .WithMessage("cannot sign a request containing ill-formed UTF-16*");
        }

        [Fact]
        public void StampGetWhoami_UnpairedLowSurrogate_IsRejected()
        {
            var http = MakeClient();

            Action act = () => http.StampGetWhoami("org-\uDC00-id");

            act.Should().Throw<ArgumentException>()
               .WithMessage("cannot sign a request containing ill-formed UTF-16*");
        }

        [Fact]
        public void StampGetWhoami_LiteralReplacementCharacter_IsAccepted()
        {
            // U+FFFD is a perfectly good character. Rejecting it would mean the
            // gate is scanning output for U+FFFD rather than checking the input
            // for unpaired surrogates.
            var http = MakeClient();

            var req = http.StampGetWhoami("org-�-id");

            using var doc = JsonDocument.Parse(req.Body);
            doc.RootElement.GetProperty("organizationId").GetString()
                .Should().Be("org-�-id");
        }

        [Fact]
        public void StampGetWhoami_AstralCharacter_IsAccepted()
        {
            // A well-formed surrogate pair must survive: the encoder escapes it
            // rather than substituting, so the body still parses back to the
            // exact value supplied.
            var http = MakeClient();

            var req = http.StampGetWhoami("org-😀-id");

            using var doc = JsonDocument.Parse(req.Body);
            doc.RootElement.GetProperty("organizationId").GetString()
                .Should().Be("org-😀-id");
        }

        [Fact]
        public void StampImportPrivateKey_UnpairedSurrogateInNestedParameter_IsRejected()
        {
            var http = MakeClient();
            var body = new Http.ImportPrivateKeyRequestBody
            {
                OrganizationId = "org-x",
                Type = "ACTIVITY_TYPE_IMPORT_PRIVATE_KEY",
                TimestampMs = "1",
                Parameters = new Http.ImportPrivateKeyParameters
                {
                    UserId = "user-x",
                    AddressFormats = new[] { "ADDRESS_FORMAT_ETHEREUM" },
                    Curve = "CURVE_SECP256K1",
                    EncryptedBundle = "{}",
                    PrivateKeyName = "key-\uD800",
                },
            };

            Action act = () => http.StampImportPrivateKey(body);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*parameters.privateKeyName has an unpaired surrogate*");
        }

        [Fact]
        public void StampImportPrivateKey_UnpairedSurrogateInArrayElement_IsRejected()
        {
            var http = MakeClient();
            var body = new Http.ImportPrivateKeyRequestBody
            {
                OrganizationId = "org-x",
                Type = "ACTIVITY_TYPE_IMPORT_PRIVATE_KEY",
                TimestampMs = "1",
                Parameters = new Http.ImportPrivateKeyParameters
                {
                    UserId = "user-x",
                    AddressFormats = new[] { "ADDRESS_FORMAT_ETHEREUM", "BAD-\uDFFF" },
                    Curve = "CURVE_SECP256K1",
                    EncryptedBundle = "{}",
                    PrivateKeyName = "key",
                },
            };

            Action act = () => http.StampImportPrivateKey(body);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*parameters.addressFormats[1] has an unpaired surrogate*");
        }

        [Fact]
        public void StampExportWalletAccount_UnpairedSurrogateInAddress_IsRejected()
        {
            var http = MakeClient();
            var body = new Http.ExportWalletAccountRequestBody
            {
                OrganizationId = "org-z",
                Type = "ACTIVITY_TYPE_EXPORT_WALLET_ACCOUNT",
                TimestampMs = "2",
                Parameters = new Http.ExportWalletAccountParameters
                {
                    Address = "0xabc\uD800",
                    TargetPublicKey = FixturePublicKey,
                },
            };

            Action act = () => http.StampExportWalletAccount(body);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*parameters.address has an unpaired surrogate*");
        }

        [Fact]
        public void RejectedRequest_MessageDoesNotEchoTheOffendingValue()
        {
            var http = MakeClient();

            Action act = () => http.StampGetWhoami("super-secret-org-\uD800");

            act.Should().Throw<ArgumentException>()
               .Which.Message.Should().NotContain("super-secret-org");
        }

        // --------------------------------------------------------
        // Coverage guard for the ill-formed UTF-16 gate.
        //
        // src/Http.cs enumerates the validated members by hand because the
        // shipped library cannot reflect over its DTOs under IL2CPP/AOT. That
        // is a maintenance hazard: add a DTO field, forget the check, and the
        // gate is silently bypassed for it. The test assembly runs on net8.0
        // with full reflection, so the enumeration can be verified here even
        // though it cannot be generated there.
        // --------------------------------------------------------

        private const string IllFormedUtf16 = "x\uD800y";

        [Fact]
        public void AllWireDtoStringFields_AreCoveredByIllFormedUtf16Validation()
        {
            var http = MakeClient();

            var bodyTypes = typeof(Http)
                .GetNestedTypes(BindingFlags.Public)
                .Where(t => t.Name.EndsWith("RequestBody", StringComparison.Ordinal))
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();

            bodyTypes.Should().NotBeEmpty("the reflection walk must discover the wire DTOs");

            int fieldsChecked = 0;
            foreach (var bodyType in bodyTypes)
            {
                var paths = new List<List<PropertyInfo>>();
                CollectWireStringPaths(bodyType, new List<PropertyInfo>(), paths, new HashSet<Type>());

                paths.Should().NotBeEmpty(
                    "{0} must expose at least one string reaching the wire", bodyType.Name);

                foreach (var path in paths)
                {
                    object body = BuildPopulatedInstance(bodyType);
                    PoisonLeaf(body, path);

                    Action act = InvokeStampFor(http, body);

                    act.Should().Throw<ArgumentException>(
                            "{0}.{1} reaches the wire, so it must be covered by a "
                            + "ValidateWireStrings overload in src/Http.cs",
                            bodyType.Name, DescribePath(path))
                       .WithMessage("cannot sign a request containing ill-formed UTF-16*");

                    fieldsChecked++;
                }
            }

            // Vacuity guard: if the reflection walk silently stopped finding
            // members, the loop above would pass by doing nothing.
            fieldsChecked.Should().BeGreaterOrEqualTo(20,
                "the wire DTOs currently expose ~23 string members between them");
        }

        private static void CollectWireStringPaths(
            Type type,
            List<PropertyInfo> prefix,
            List<List<PropertyInfo>> sink,
            HashSet<Type> visiting)
        {
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite)
                {
                    continue;
                }

                var path = new List<PropertyInfo>(prefix) { prop };
                Type pt = prop.PropertyType;

                if (pt == typeof(string) || pt == typeof(string[]))
                {
                    sink.Add(path);
                }
                else if (pt.IsClass && pt.DeclaringType == typeof(Http))
                {
                    // Nested DTO (the *Parameters types). Guard against cycles
                    // even though the current shapes are acyclic.
                    if (!visiting.Add(pt))
                    {
                        continue;
                    }
                    CollectWireStringPaths(pt, path, sink, visiting);
                    visiting.Remove(pt);
                }
            }
        }

        private static object BuildPopulatedInstance(Type type)
        {
            object instance = Activator.CreateInstance(type)!;
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite)
                {
                    continue;
                }

                Type pt = prop.PropertyType;
                if (pt == typeof(string))
                {
                    prop.SetValue(instance, "x");
                }
                else if (pt == typeof(string[]))
                {
                    // One element so an array member has something to poison.
                    prop.SetValue(instance, new[] { "x" });
                }
                else if (pt.IsClass && pt.DeclaringType == typeof(Http))
                {
                    prop.SetValue(instance, BuildPopulatedInstance(pt));
                }
            }
            return instance;
        }

        private static void PoisonLeaf(object root, List<PropertyInfo> path)
        {
            object current = root;
            for (int i = 0; i < path.Count - 1; i++)
            {
                current = path[i].GetValue(current)!;
            }

            var leaf = path[path.Count - 1];
            if (leaf.PropertyType == typeof(string))
            {
                leaf.SetValue(current, IllFormedUtf16);
            }
            else
            {
                leaf.SetValue(current, new[] { IllFormedUtf16 });
            }
        }

        private static string DescribePath(List<PropertyInfo> path) =>
            string.Join(".", path.Select(p => p.Name));

        private static Action InvokeStampFor(Http http, object body) => body switch
        {
            Http.WhoamiRequestBody b => () => http.StampGetWhoami(b.OrganizationId),
            Http.InitImportPrivateKeyRequestBody b => () => http.StampInitImportPrivateKey(b),
            Http.ImportPrivateKeyRequestBody b => () => http.StampImportPrivateKey(b),
            Http.ExportPrivateKeyRequestBody b => () => http.StampExportPrivateKey(b),
            Http.ExportWalletAccountRequestBody b => () => http.StampExportWalletAccount(b),
            _ => throw new InvalidOperationException(
                "Wire DTO '" + body.GetType().Name + "' is not mapped here. Add its Stamp* "
                + "method to InvokeStampFor and a matching ValidateWireStrings overload "
                + "in src/Http.cs."),
        };

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
