// ProofFixtureProvenanceTests.cs
//
// Asserts the structural integrity and provenance of the upstream proof
// fixture JSON committed at tests/Fixtures/proofs/. The C# proof
// verifier is outside the supported surface; this test class
// guarantees only:
//
//   1. The fixture JSON parses.
//   2. _provenance carries the expected schema (level, upstream_file,
//      upstream_file_sha256, upstream_package, upstream_package_version).
//   3. The sha256 recorded in _provenance matches the actual sha256 of
//      the cited upstream snapshot file (detects manual edits to either
//      the fixture or the upstream snapshot tree).
//   4. Each appProofs entry has the v1AppProof fields
//      (scheme, publicKey, proofPayload, signature) populated with
//      plausible types (P-256 ephemeral key hex length 264, DER-ish
//      hex strings, SIGNATURE_SCHEME_EPHEMERAL_KEY_P256).
//   5. Each bootProofs entry has the v1BootProof fields populated:
//      ephemeralPublicKeyHex, awsAttestationDocB64, qosManifestB64,
//      qosManifestEnvelopeB64, deploymentLabel, enclaveApp, owner,
//      createdAt {seconds, nanos}.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Turnkey.Tests
{
    public class ProofFixtureProvenanceTests
    {
        private static readonly string FixturePath = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "proofs",
            "upstream-proof-tests-fixture.json");

        // The fixture is shipped under tests/Fixtures/proofs/ at repo root.
        // AppContext.BaseDirectory points at .../tests/bin/Release/net8.0/.
        // The upstream snapshot it cites lives outside the test bin dir;
        // we resolve the snapshot path by walking up to the repo root.
        private static string ResolveRepoRoot()
        {
            // tests/bin/Release/net8.0/ -> walk up to the repository root
            // containing turnkey-sdk-csharp.sln.
            string dir = AppContext.BaseDirectory;
            for (int i = 0; i < 8; i++)
            {
                if (File.Exists(Path.Combine(dir, "turnkey-sdk-csharp.sln")))
                {
                    return dir;
                }
                dir = Path.GetDirectoryName(dir)!;
            }
            throw new InvalidOperationException(
                "could not locate repo root from " + AppContext.BaseDirectory);
        }

        [Fact]
        public void FixtureFile_Exists_AndParsesAsJson()
        {
            File.Exists(FixturePath).Should().BeTrue(
                $"fixture must be committed at {FixturePath}");

            using var doc = JsonDocument.Parse(File.ReadAllText(FixturePath));
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        }

        [Fact]
        public void Provenance_SchemaIsComplete()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(FixturePath));
            var prov = doc.RootElement.GetProperty("_provenance");

            prov.GetProperty("level").GetString().Should().Be("upstream-test-vectors");
            prov.GetProperty("upstream_file").GetString().Should().StartWith(
                "tests/UpstreamSources/turnkey-crypto-");
            prov.GetProperty("upstream_file_sha256").GetString().Should().MatchRegex("^[0-9a-f]{64}$");
            prov.GetProperty("upstream_package").GetString().Should().Be("@turnkey/crypto");
            prov.GetProperty("upstream_package_version").GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Provenance_UpstreamFileSha256_MatchesActualSnapshotBytes()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(FixturePath));
            var prov = doc.RootElement.GetProperty("_provenance");
            string upstreamRel = prov.GetProperty("upstream_file").GetString()!;
            string recordedSha = prov.GetProperty("upstream_file_sha256").GetString()!;

            string repoRoot = ResolveRepoRoot();
            string upstreamAbs = Path.Combine(repoRoot, upstreamRel);

            File.Exists(upstreamAbs).Should().BeTrue(
                $"upstream snapshot file cited in provenance must exist: {upstreamAbs}");

            byte[] bytes = File.ReadAllBytes(upstreamAbs);
            string actualSha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            actualSha.Should().Be(
                recordedSha,
                "fixture provenance sha256 must match the actual bytes of the cited upstream snapshot file. " +
                "If this fails, either the fixture is stale or the upstream snapshot was edited locally.");
        }

        [Fact]
        public void AppProofs_StructuralSchema_IsCorrect()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(FixturePath));
            var arr = doc.RootElement.GetProperty("appProofs");
            arr.GetArrayLength().Should().BeGreaterThan(0);

            foreach (var entry in arr.EnumerateArray())
            {
                entry.GetProperty("scheme").GetString().Should().Be(
                    "SIGNATURE_SCHEME_EPHEMERAL_KEY_P256");

                string publicKey = entry.GetProperty("publicKey").GetString()!;
                publicKey.Should().MatchRegex("^[0-9a-fA-F]+$");
                // The upstream ephemeralPublicKey form is 2 P-256 uncompressed
                // points concatenated (65 + 65 = 130 bytes => 260 hex chars,
                // or with prefix 04 already present => same).
                publicKey.Length.Should().BeInRange(130, 264,
                    "expected P-256 uncompressed-pair hex (130 chars) or longer");

                string proofPayload = entry.GetProperty("proofPayload").GetString()!;
                proofPayload.Should().StartWith("{").And.EndWith("}");
                // Parse the nested JSON.
                using var payloadDoc = JsonDocument.Parse(proofPayload);
                payloadDoc.RootElement.GetProperty("type").GetString()
                    .Should().Be("APP_PROOF_TYPE_ADDRESS_DERIVATION");

                string signature = entry.GetProperty("signature").GetString()!;
                signature.Should().MatchRegex("^[0-9a-fA-F]+$");
                signature.Length.Should().BeInRange(64, 256,
                    "P-256 signature in compact-bytes hex (64 chars min) or DER hex");
            }
        }

        [Fact]
        public void BootProofs_StructuralSchema_IsCorrect()
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(FixturePath));
            var arr = doc.RootElement.GetProperty("bootProofs");
            arr.GetArrayLength().Should().BeGreaterThan(0);

            foreach (var entry in arr.EnumerateArray())
            {
                string ephHex = entry.GetProperty("ephemeralPublicKeyHex").GetString()!;
                ephHex.Should().MatchRegex("^[0-9a-fA-F]+$");

                // AWS attestation + QOS manifests are base64.
                string awsB64 = entry.GetProperty("awsAttestationDocB64").GetString()!;
                Action act = () => Convert.FromBase64String(awsB64);
                act.Should().NotThrow(
                    "awsAttestationDocB64 must be valid base64");

                string qmB64 = entry.GetProperty("qosManifestB64").GetString()!;
                Action act2 = () => Convert.FromBase64String(qmB64);
                act2.Should().NotThrow("qosManifestB64 must be valid base64");

                string qmEnvB64 = entry.GetProperty("qosManifestEnvelopeB64").GetString()!;
                Action act3 = () => Convert.FromBase64String(qmEnvB64);
                act3.Should().NotThrow("qosManifestEnvelopeB64 must be valid base64");

                entry.GetProperty("deploymentLabel").GetString()
                    .Should().NotBeNullOrWhiteSpace();
                entry.GetProperty("enclaveApp").GetString()
                    .Should().Be("signer");
                entry.GetProperty("owner").GetString()
                    .Should().Be("tkhq");

                var createdAt = entry.GetProperty("createdAt");
                createdAt.GetProperty("seconds").GetString()
                    .Should().MatchRegex("^[0-9]+$");
                createdAt.GetProperty("nanos").GetString()
                    .Should().MatchRegex("^[0-9]+$");
            }
        }

        [Fact]
        public void AppProofs_And_BootProofs_HaveMatchingPublicKeys()
        {
            // The upstream test pairs (appProof_i, bootProof_i) so that
            // appProof.publicKey == bootProof.ephemeralPublicKeyHex.
            using var doc = JsonDocument.Parse(File.ReadAllText(FixturePath));
            var apps = doc.RootElement.GetProperty("appProofs").EnumerateArray().ToList();
            var boots = doc.RootElement.GetProperty("bootProofs").EnumerateArray().ToList();

            apps.Count.Should().Be(boots.Count,
                "appProofs and bootProofs arrays must be paired (same length)");

            for (int i = 0; i < apps.Count; i++)
            {
                apps[i].GetProperty("publicKey").GetString()
                    .Should().Be(
                        boots[i].GetProperty("ephemeralPublicKeyHex").GetString(),
                        $"appProof[{i}].publicKey must equal bootProof[{i}].ephemeralPublicKeyHex (upstream pairing rule)");
            }
        }
    }
}
