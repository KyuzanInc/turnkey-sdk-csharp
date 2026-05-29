# KyuzanInc.Turnkey.Sdk

> **Unofficial / community-maintained Turnkey SDK for .NET by Kyuzan Inc.
> Not affiliated with Turnkey, Inc.**

A NuGet package that lets .NET applications produce signed Turnkey API
requests and decrypt Turnkey credential / export bundles.

The crypto logic is a 1:1 logical port of the Turnkey TypeScript SDK packages
at the exact versions consumed by the Kyuzan **peak** monorepo:

| Source npm package         | Version |
|----------------------------|---------|
| `@turnkey/crypto`          | 2.8.8   |
| `@turnkey/http`            | 3.16.0  |
| `@turnkey/api-key-stamper` | 0.5.0   |
| `@turnkey/encoding`        | 0.6.0   |

The 1:1 logical equivalence is confirmed by multi-round Codex review.
Evidence is committed to [`codex-crypto-reviews/`](./codex-crypto-reviews/).

For the authoritative Turnkey SDK in TypeScript, see
[`github.com/tkhq/sdk`](https://github.com/tkhq/sdk).

## Status

**v0.1.0-alpha** — internal use within Kyuzan. The crypto and signing path
has been through three rounds of independent Codex review (evidence under
[`codex-crypto-reviews/`](./codex-crypto-reviews/)) and is byte-compared
against fixtures generated from pinned `@turnkey/*` npm packages. Public
nuget.org distribution and contribution policy are pending; trademark and
distribution review with Turnkey, Inc. is in progress.

## Features (planned for v0.1.0)

- `Turnkey.Encoding` — hex / base58 / base58check / UTF-8 utilities.
- `Turnkey.Crypto` — P-256 key pair generation, HPKE encrypt / decrypt,
  HKDF, Tonelli-Shanks modular square root, Turnkey credential-bundle
  decrypt, Turnkey export-bundle decrypt, Turnkey session JWT signature
  verification.
- `Turnkey.ApiKeyStamper` — ECDSA P-256 stamp for the Turnkey API
  `X-Stamp` header.
- `Turnkey.Http` — typed signed-request builders for the Turnkey API
  activity endpoints used by the peak wallet flow.

## Installation

This package is distributed via **GitHub Packages** during v0.1.0-alpha
(public nuget.org distribution is pending).

Create a **project-local** `nuget.config` next to your `.sln` (do NOT
edit your global / user config; this avoids polluting other projects and
sidesteps `NU1507` errors when consumers use Central Package Management):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org"     value="https://api.nuget.org/v3/index.json" />
    <add key="kyuzan-github" value="https://nuget.pkg.github.com/KyuzanInc/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <kyuzan-github>
      <add key="Username" value="%KYUZAN_GH_USER%" />
      <add key="ClearTextPassword" value="%KYUZAN_GH_TOKEN%" />
    </kyuzan-github>
  </packageSourceCredentials>
  <!-- Optional but recommended for CPM users: keep our package isolated
       to the Kyuzan feed and everything else on nuget.org. -->
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="kyuzan-github">
      <package pattern="KyuzanInc.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Export the credentials in your shell (GitHub token needs `read:packages` scope):

```bash
export KYUZAN_GH_USER="your-github-username"
export KYUZAN_GH_TOKEN="ghp_..."
```

Then add the package:

```bash
dotnet add package KyuzanInc.Turnkey.Sdk --version 0.1.0-alpha.0
```

> **Why project-local, not `dotnet nuget add source`**: the CLI command writes
> to your user-global config, which leaks into every other .NET project on the
> machine and breaks projects that use Central Package Management
> (`ManagePackageVersionsCentrally`) with `NU1507`. A project-local
> `nuget.config` keeps the feed scoped to this project.

## Quick start

Six common flows, each independently usable. An end-to-end example follows.

```csharp
using Turnkey;

// 1. Generate an ephemeral P-256 key pair (HPKE recipient / session key).
var keyPair = Crypto.GenerateP256KeyPair();
// keyPair.PrivateKey            -> hex string
// keyPair.PublicKey             -> hex string (33 bytes compressed)
// keyPair.PublicKeyUncompressed -> hex string (65 bytes uncompressed)

// 2. Build a signed Turnkey API request.
//    FromTargetPrivateKey derives the public key for you.
var http = Http.FromTargetPrivateKey(targetPrivateKey: "<your-api-private-key-hex>");
SignedRequest signed = http.StampGetWhoami(organizationId: "<your-org-id>");
// signed.Url, signed.Body, signed.Stamp.{StampHeaderName, StampHeaderValue}

// 3. Stamp arbitrary JSON with an ApiKeyStamper.
var stamper = new ApiKeyStamper(
    apiPublicKey:  "<your-api-public-key-hex>",
    apiPrivateKey: "<your-api-private-key-hex>");
ApiKeyStamper.StampResult stamp = stamper.Stamp("{\"foo\":\"bar\"}");
// stamp.StampHeaderName == "X-Stamp"
// stamp.StampHeaderValue == base64url(JSON({publicKey, scheme, signature}))

// 4. Decrypt a Turnkey credential bundle.
string apiPrivateKey = Crypto.DecryptCredentialBundle(
    encryptedCredentialBundle: "<bundle-from-turnkey>",
    embeddedKey:               keyPair.PrivateKey);

// 5. Decrypt a Turnkey export bundle (returns mnemonic or hex private key).
string exported = Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams
{
    ExportBundle    = "<export-bundle-json>",
    EmbeddedKey     = keyPair.PrivateKey,
    OrganizationId  = "<your-org-id>",
    ReturnMnemonic  = false,
    KeyFormat       = "HEXADECIMAL",
});

// 6. Encoding helpers (hex, base58, base58check).
string hex      = Encoding.Uint8ArrayToHexString(bytes);
byte[] decoded  = Encoding.Uint8ArrayFromHexString(hex);
string base58   = Encoding.Base58Encode(bytes);
```

### End-to-end: `query/whoami` against the Turnkey API

```csharp
using System.Net.Http;
using System.Text;
using Turnkey;

// 1. Build the signed request (no network call yet — the SDK does not own transport).
var http = Http.FromTargetPrivateKey(targetPrivateKey: "<your-api-private-key-hex>");
SignedRequest signed = http.StampGetWhoami(organizationId: "<your-org-id>");

// 2. POST it. The caller owns HTTPS, retries, and error handling.
using var client = new HttpClient();
var req = new HttpRequestMessage(HttpMethod.Post, signed.Url)
{
    Content = new StringContent(signed.Body, Encoding.UTF8, "application/json"),
};
req.Headers.Add(signed.Stamp.StampHeaderName, signed.Stamp.StampHeaderValue);

HttpResponseMessage response = await client.SendAsync(req);
string json = await response.Content.ReadAsStringAsync();
Console.WriteLine(json);
```

> **Caller responsibilities** (the SDK does not own these): supplying secret
> material, calling HTTPS, retry / backoff, persistent key storage, WebAuthn,
> OAuth / OTP. See [`docs/security/threat-model.md`](./docs/security/threat-model.md)
> for the full boundary.

## Target frameworks

`netstandard2.1` and `net8.0`. Tested on .NET 8.

## Dependencies

- `BouncyCastle.Cryptography 2.5.0` — for ECDSA / ECDH / AES-GCM /
  SHA-256 / HMAC / BigInteger / EC point primitives only. **No
  BouncyCastle HPKE / HKDF / KDF wrappers are used.** HPKE, HKDF,
  Tonelli-Shanks, and bundle parsing are direct ports of the upstream
  Turnkey TypeScript logic.
- `System.Text.Json 8.0.5` — source-generated context only
  (`TurnkeyJsonContext`). No reflection-based serialization,
  no `System.Reflection.Emit`. Compatible with IL2CPP AOT trim.

## Project structure

```
src/        — production code
tests/      — xunit tests + golden fixtures from pinned Node packages
docs/security/threat-model.md — scope: key handling, signing, crypto
                                primitives, serialization, test-secret handling
codex-crypto-reviews/          — pinned upstream snapshots, source-pin docs,
                                 multi-round Codex review evidence
```

## Source mapping (Turnkey TS → this port)

This package is a 1:1 logical port of the upstream Turnkey TypeScript
packages at the versions consumed by peak. Each `.cs` file's header repeats
this mapping for the file's own scope; the table below is the index.

| npm package                | Version | TS source                                 | C# file                                 | Description                                                      |
|----------------------------|---------|-------------------------------------------|-----------------------------------------|------------------------------------------------------------------|
| `@turnkey/encoding`        | 0.6.0   | `hex.ts`                                  | `Encoding.cs`                           | `Uint8ArrayToHexString`, `Uint8ArrayFromHexString`, `HexToAscii`, `NormalizePadding` |
| `@turnkey/encoding`        | 0.6.0   | `base64.ts`                               | `Encoding.cs`                           | `StringToBase64UrlString`, `HexStringToBase64Url`, `Base64UrlToBase64`, `DecodeBase64UrlToString` |
| `@turnkey/encoding`        | 0.6.0   | `bs58.ts`, `bs58check.ts`                 | `Encoding.cs`                           | `Base58Encode/Decode`, `Base58CheckEncode/Decode`                |
| `@turnkey/encoding`        | 0.6.0   | `encode.ts`                               | `Encoding.cs`                           | `PointEncode`                                                    |
| `@turnkey/crypto`          | 2.8.8   | `crypto.ts` (subset)                      | `Crypto.cs`                             | `GenerateP256KeyPair`, `HpkeEncrypt/Decrypt`, `CompressRawPublicKey`, `UncompressRawPublicKey`, `BuildAdditionalAssociatedData`, `FormatHpkeBuf` |
| `@turnkey/crypto`          | 2.8.8   | `constants.ts`                            | `Crypto.cs` (`Crypto.Constants` nested) | HPKE suite IDs, HKDF labels, signer public keys                  |
| `@turnkey/crypto`          | 2.8.8   | `math.ts`                                 | `Crypto.cs` (`Crypto.Math` nested)      | Tonelli-Shanks `ModSqrt`                                         |
| `@turnkey/crypto`          | 2.8.8   | HKDF helpers in `crypto.ts` (`@noble/hashes/hkdf` upstream) | `Crypto.cs` (`Crypto.Hkdf` nested) | HKDF `Extract` / `Expand`                                        |
| `@turnkey/crypto`          | 2.8.8   | `turnkey.ts` (subset)                     | `Crypto.cs`                             | `DecryptCredentialBundle`, `EncryptPrivateKeyToBundle`, `DecryptExportBundle`, `VerifySessionJwtSignature` |
| `@turnkey/api-key-stamper` | 0.5.0   | `index.ts`, `purejs.ts`                   | `ApiKeyStamper.cs`                      | ECDSA P-256 signing, DER-hex output, X-Stamp header construction |
| `@turnkey/http`            | 3.16.0  | `index.ts` (signing subset)               | `Http.cs`                               | Signed activity-request builder for the 5 endpoints peak uses    |
| (C#-specific)              | —       | —                                         | `CryptoConstants.cs`                    | BouncyCastle curve / parameter constants (not in upstream)       |
| (C#-specific)              | —       | —                                         | `TurnkeyJsonContext.cs`                 | `System.Text.Json` source-generated context (AOT/IL2CPP-safe)    |

Upstream snapshots used for the port are committed under
[`codex-crypto-reviews/upstream-snapshots/`](./codex-crypto-reviews/) so future
reviewers can diff against them without re-downloading from npm.

## Intentionally unported

The peak wallet flow does not need the following upstream surfaces, so they
are **not** ported in v0.1.0. Each is verified absent from `src/`.

**From `@turnkey/crypto` 2.8.8:**

- `hpkeAuthEncrypt`
- `quorumKeyEncrypt`
- `extractPrivateKeyFromPKCS8Bytes`
- `fromDerSignature`, `toDerSignature`
- `verifyStampSignature`
- `encryptWalletToBundle`
- `encryptToEnclave`
- `encryptOauth2ClientSecret`
- `encryptOnRampSecret`
- `proof.ts` (AWS Nitro attestation chain verification)

**From `@turnkey/http` 3.16.0:**

- Auto-generated activity methods beyond the 5 needed by peak
  (`whoami`, `init_import_private_key`, `import_private_key`,
   `export_private_key`, `export_wallet_account`).
- Polling, error handling, retry, WebAuthn stamping.
- The full typed client surface (only the request-signing subset is ported).

**From `@turnkey/api-key-stamper` 0.5.0:**

- `"browser"` (WebCrypto) and `"node"` (Node crypto) runtimes — only the
  `"purejs"` (noble) runtime equivalent is ported, since BouncyCastle covers
  both in a single backend.

**From `@turnkey/encoding` 0.6.0:**

- `base64.ts`'s React-Native `btoa` implementation — the C# port uses
  `System.Convert.ToBase64String` directly, producing identical bytes.

If any of these become required by peak, they will be added in a future
minor — they were omitted for v0.1.0 scope, not for technical reasons.

> **How correctness is verified** is documented in detail in the
> [Verification posture](#verification-posture) section below: the 4-tier
> equivalence strategy, multi-round Codex review trail, pinned upstream
> snapshots, lockfile-pinned dependencies, and an uncertainty checklist.

## Build and test

```bash
dotnet restore --use-lock-file
dotnet build turnkey-sdk-csharp.sln -c Release
dotnet test  turnkey-sdk-csharp.sln -c Release \
  --collect:"XPlat Code Coverage"
```

## Where to read next

- [Verification posture](#verification-posture) — the 4-tier equivalence
  strategy, coverage matrix, and uncertainty checklist. **Read this before
  depending on this SDK in production.**
- [`docs/security/threat-model.md`](./docs/security/threat-model.md) — scope, assets, threats, mitigations.
- [`codex-crypto-reviews/`](./codex-crypto-reviews/) — multi-round independent review evidence.
- [`CHANGELOG.md`](./CHANGELOG.md) — release notes.
- [`NOTICE`](./NOTICE) — upstream Turnkey TypeScript attribution.
- Per-file `src/*.cs` header comments — exact TS → C# function mapping for each file.
- Dependency pins: `src/packages.lock.json`, `tests/packages.lock.json`; BouncyCastle pinned `[2.5.0]` exact.

## Contributing

This repository is private. Contribution policy will be set after v0.1.0.

## Verification posture

The equivalence between this C# port and the pinned upstream TypeScript
packages is enforced by four cooperating mechanisms ([plan](./plans/PLAN-EQUIVALENCE-VERIFICATION.md)
Section 5, "Verification strategy — 4 Tier"):

| Tier | Source of truth | Asserted in C# by | Coverage today |
|---|---|---|---|
| 1 | Hand-transcribed upstream `__tests__/` values + cross-language hex constants borrowed from `tkhq/sdk` / `swift-sdk` / `dart-sdk` | `tests/EncodingTests.cs`, `tests/CryptoTests.cs`, `tests/ApiKeyStamperTests.cs`, `tests/Fixtures/{encoding,crypto,api-key-stamper}/*.json`, `tests/ProofFixtureProvenanceTests.cs` | **Fully enforced**; the `coverage-map.sh --check` CI step is the gate. |
| 2 | Bytes produced by running the **pinned** upstream npm packages under `tests/Fixtures/Generators/` (Node 20.x, npm override `@noble/curves@1.3.0`) | `tests/ApiKeyStamperTests.NodeFixture_StamperByteParity_PureJsRfc6979` | **Partial.** Only the api-key-stamper deterministic path (PR-6) is committed. HTTP byte vectors (plan PR-5), HPKE round-trip + structural fixtures (plan PR-7), and the SDK HPKE ↔ RFC 9180 byte-equality evaluation (plan PR-7a) are NOT enforced by code yet. They are tracked as deferred follow-up PRs in [`plans/PLAN-EQUIVALENCE-VERIFICATION.md`](./plans/PLAN-EQUIVALENCE-VERIFICATION.md) Section 7. |
| 3 | RFC / NIST / SEC2 / BIP standard vectors for each cryptographic primitive (HKDF RFC 5869, etc.) — primitive-level independent oracle of correctness | `tests/CryptoTests.cs` HKDF tests (RFC 5869 A.1–A.3 only) | **Partial.** Only HKDF is committed. ECDSA P-256 RFC 6979 vectors, AES-GCM-256 NIST CAVP, SHA-256 NIST SHS, HMAC-SHA256 RFC 4231, Ed25519 RFC 8032, secp256k1 SEC2, and Base58Check Bitcoin BIP test vectors are NOT enforced by code yet (plan PR-8 / PR-9 / PR-9b). |
| 4 | Live Turnkey backend — opt-in only via `TURNKEY_TEST_ORG_API_KEY` env var | (out of CI; manual run only) | **Opt-in only.** |

### Coverage matrix

[`codex-crypto-reviews/coverage-map.sh --check`](./codex-crypto-reviews/coverage-map.sh)
walks every `test()` block in the pinned upstream `__tests__/*.ts` files
and confirms each one either maps to a C# `[Fact]`/`[Theory]` via a
`/// upstream: <relpath>:<line>` annotation or is recorded in
[`coverage-map.na.tsv`](./codex-crypto-reviews/coverage-map.na.tsv) with a
non-empty `N/A` reason. The gate runs in CI and fails the build if a
new upstream test is added without either a C# counterpart or an
explicit N/A reason. See
[`codex-crypto-reviews/coverage-map.md`](./codex-crypto-reviews/coverage-map.md)
for the current matrix.

### Upstream drift

[`/.github/workflows/upstream-drift.yml`](./.github/workflows/upstream-drift.yml)
re-hashes every upstream `__tests__/*.ts` and `__fixtures__/*` file at
the pinned git tag SHA once a month. Drift surfaces as a single open
issue per package labelled `upstream-drift`; the workflow updates an
existing issue rather than opening a new one (idempotent). It is NOT a
main-branch CI gate, so it can never block a PR.

### Deferred enforcement (NOT enforced by this version of the SDK)

The plan defines additional verification mechanisms that this version
of the SDK does NOT yet enforce. Until the corresponding PR lands they
are documentation, not a code-checked guarantee:

- **HTTP signed-request body byte equality** (plan PR-5) — Node-generated
  `tests/Fixtures/http/turnkey-http-vectors.json` is not committed.
  HTTP-side tests verify shape and crypto-verify the stamp header but
  do not byte-compare body or stamp header against an upstream-produced
  reference.
- **HPKE round-trip + structural Node fixture** (plan PR-7) —
  `tests/Fixtures/crypto/turnkey-crypto-node-vectors.json` is not
  committed. HPKE encrypt/decrypt round-trip tests use C#-only key
  material, not pinned upstream bytes.
- **SDK HPKE ↔ RFC 9180 byte-equality evaluation** (plan PR-7a) — open.
  Whether C# `HpkeEncrypt` is byte-identical to RFC 9180 base mode with
  a fixed ephemeral key is undetermined; PR-7a is the deliverable.
- **Primitive standard vectors except HKDF** (plan PR-8 / PR-9 / PR-9b) —
  ECDSA P-256 RFC 6979, AES-GCM-256 NIST CAVP, SHA-256 NIST SHS,
  HMAC-SHA256 RFC 4231, Ed25519 RFC 8032, secp256k1 SEC2, Base58Check
  Bitcoin BIP, and any HPKE RFC 9180 vectors are not committed.
- **Cross-language hex constant adoption** (plan PR-10) — C# tests use
  Crypto.GenerateP256KeyPair-generated keys in some HPKE round-trip
  paths, not the `mockSenderPrivateKey` / `mockCredentialBundle` hex
  values shared by the upstream multi-language SDKs.
- **Fixture regeneration CI workflow** (plan PR-13) —
  `tests/Fixtures/Generators/` regeneration is a manual `npm ci &&
  npm run gen:all` from a developer machine. No automated workflow
  diffs the regeneration result against the committed bytes.

### Uncertainty checklist (read before depending on this SDK in production)

- [ ] Upstream P-256 ECDSA on `nodecrypto.ts` and `webcrypto.ts`
      runtimes is non-deterministic. The C# port covers **only** the
      `purejs` runtime path (RFC 6979 deterministic-k, low-S), as
      documented in
      [`codex-crypto-reviews/ApiKeyStamper.cs-codex-findings-reconciliation.md`](./codex-crypto-reviews/ApiKeyStamper.cs-codex-findings-reconciliation.md)
      E1/E2.
- [ ] Upstream `@noble/curves` does NOT enforce low-S by default in
      any 1.x version (verified in PR-4 preflight). The C# port DOES
      enforce low-S, which is the standard wire format. Both signatures
      verify against the same public key. The fixture at
      `tests/Fixtures/api-key-stamper/turnkey-stamper-node-vectors.json`
      records the upstream high-S DER bytes AND the C# low-S
      equivalent; both are confirmed to be cryptographic equivalents
      (same r, s_lowS = n - s_highS).
- [ ] Whether SDK HPKE (`@turnkey/crypto.hpkeEncrypt`) is byte-equal
      to RFC 9180 base mode is an open evaluation deferred to plan
      PR-7a. The C# port is wire-compatible with the SDK's bytes for
      decrypt round-trip; full RFC 9180 byte vectors are NOT imported
      until PR-7a confirms equality.
- [ ] APIs deliberately **not ported**: `hpkeAuthEncrypt`,
      `quorumKeyEncrypt`, `extractPrivateKeyFromPKCS8Bytes`,
      `fromDerSignature`, `toDerSignature`, `verifyStampSignature`,
      `verifyRequestStamp`, `encryptOauth2ClientSecret`,
      `encryptOnRampSecret`, `proof.ts`. Justification + peak monorepo
      grep evidence in
      [`codex-crypto-reviews/peak-usage-grep-evidence.md`](./codex-crypto-reviews/peak-usage-grep-evidence.md)
      and the per-file Codex review under
      [`codex-crypto-reviews/`](./codex-crypto-reviews/).
- [ ] Live Turnkey backend e2e is opt-in only (`TURNKEY_TEST_ORG_API_KEY`).
      The CI does not run it, and the SDK is NOT a substitute for an
      end-to-end test against the backend.
- [ ] Cross-language hex constants (`mockSenderPrivateKey`,
      `mockCredentialBundle`, the BIP-39 mnemonic, etc.) used in
      `CryptoTests.cs` are copied verbatim from `tkhq/sdk` at the
      pinned git tag and from `tkhq/swift-sdk` / `tkhq/dart-sdk` for
      cross-implementation triangulation. Their source URLs are in the
      Codex review files.

## License

MIT — see [LICENSE](./LICENSE). Upstream Turnkey TypeScript attribution
is recorded in [NOTICE](./NOTICE).
