# turnkey-sdk-csharp standalone repo plan (v2, post-Codex-review-r1)

## Intent

Create **`KyuzanInc/turnkey-sdk-csharp`** as a standalone **private** GitHub repository.
Port `packages/turnkey-sdk-unity/Runtime/{Encoding,Crypto,ApiKeyStamper,Http,UnityConstants}.cs`
(1740 LOC) into a NuGet-publishable .NET package, with logic 1:1 to the Turnkey npm
packages **at the exact versions consumed by the peak monorepo**.

This plan is adapted from `plans/plans-peak-sdk-csharp.md` (PR 1 scope) into a
standalone repo. The peak monorepo MUST NOT be modified.

## Decisions resolved (v2; post-Codex-review-r1)

These were Open Questions in v1; resolved by plan author for v2:

| # | Decision | Rationale |
|---|---|---|
| Q1 | **Namespace = `Turnkey`** (Unity parity). PackageId / AssemblyName = `KyuzanInc.Turnkey.Sdk`. | Unity port uses `namespace Turnkey`; matches Unity's `com.kyuzan.turnkey-sdk-unity` package id + `Turnkey` namespace pattern. NuGet-side brand stays clear via PackageId. |
| Q2 | **Source-of-truth = npm tarball contents** (primary) + GitHub source at commit (secondary, for line links only). | npm publish artifacts may differ from git tree (e.g. test files excluded, `dist/` included). The wire-format risk is decided by what's on npm, not git. |
| Q3 | **Upstream TS/JS snapshots committed** into `codex-crypto-reviews/upstream-snapshots/{package}@{version}/`. | Reproducibility of Codex review across model versions and future maintainers. |
| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
| Q5 | **IL2CPP-safety is a v0.1.0 goal** (System.Text.Json source-gen via TurnkeyJsonContext, no reflection-based serialization, no `System.Reflection.Emit`). Mentioned in README.md as a compatibility target but not a Unity SDK claim. | Future Unity consumer (`peak-sdk-csharp-unity`) needs this; cost is low if set up from day 1. |
| Q6 | **Unity SDK class-name and method-name parity** preserved. `Turnkey.Encoding` / `Turnkey.Crypto` / `Turnkey.ApiKeyStamper` / `Turnkey.Http` remain the public surface. `CryptoConstants` (was `UnityConstants`) renamed but stays a **public static class** with same constants. | Existing peak-sdk-unity / turnkey-sdk-unity downstream code can later be re-targeted via `using` alias if needed. |
| Q7 | **E2E TurnkeyWhoamiTests is NOT a CI gate**. Optional, runs only when `TURNKEY_TEST_ORG_API_KEY` + `TURNKEY_TEST_ORG_ID` env vars are present, otherwise skipped. | Avoids gating on test-org credentials that the user (or this AI) cannot provision. |
| Q8 | **Initial distribution = GitHub Packages** (this private repo's package feed). nuget.org is post-v0.1.0. | Matches "set it to private" directive. No risk of accidental public upload during dev. |
| Q9 | **MIT license + NOTICE file** acknowledging the Turnkey TS source attribution (per upstream Apache-2.0-style NOTICE conventions even though Turnkey is MIT). Single README via `<PackageReadmeFile>`. | Codex r1 critique: README.public.md prepack swap is over-designed for standalone repo. |

## Source-of-truth pins (CRITICAL)

Per user directive: "you should carefully check the version that we use in peak".

| Turnkey npm package | Peak version | Unity port version | Pin choice |
|---|---|---|---|
| @turnkey/crypto          | **2.8.8**  | 2.8.9 | **2.8.8** (peak) |
| @turnkey/http            | **3.16.0** | 3.16.1 | **3.16.0** (peak) |
| @turnkey/api-key-stamper | **0.5.0** (transitive) | 0.6.0 | **0.5.0** (peak) |
| @turnkey/encoding        | **0.6.0**  | 0.6.0 | **0.6.0** (match) |

Peak lockfile evidence (`pnpm-lock.yaml`):
- `'@turnkey/crypto': specifier: 2.8.8` (in peak-sdk-core, peak-sdk-browser, peak-sdk-node)
- `'@turnkey/http': specifier: 3.16.0` (in peak-sdk-core)
- `'@turnkey/api-key-stamper': 0.5.0` (transitive resolved by @turnkey/sdk-server@5.0.0)
- `'@turnkey/encoding': 0.6.0` (in peak-sdk-core, peak-sdk-browser, peak-sdk-node)

### Source-provenance workflow

**Step A (PR 0 prereq, before any C# code)**:
```bash
mkdir -p codex-crypto-reviews/upstream-snapshots
cd codex-crypto-reviews/upstream-snapshots
for pkg in '@turnkey/crypto@2.8.8' '@turnkey/http@3.16.0' \
           '@turnkey/api-key-stamper@0.5.0' '@turnkey/encoding@0.6.0'; do
  npm pack "$pkg" --silent
done
# Record sha256 of each tarball.
shasum -a 256 *.tgz > tarball-checksums.txt
# Extract each tarball into a separate directory.
for f in *.tgz; do
  name="${f%.tgz}"
  mkdir -p "$name"
  tar -xzf "$f" -C "$name"
done
```

The resulting `tarball-checksums.txt` + extracted `package/` trees are the
**authoritative** source for Codex reviews. GitHub commit SHAs are recorded
**only** as secondary metadata (file: `turnkey-source-pins.md`).

### Diff strategy

Per Codex r1 recommendation: **"fresh verification/port from peak-pinned npm
source, with Unity used only as C# adaptation reference."**

For each public method in each file:
1. Open the pinned npm-tarball TS source (authoritative).
2. Open the Unity port C# source (scaffold + adaptation reference).
3. Map TS logic → C# line-by-line. Where Unity port differs from peak-pinned
   TS, **re-port from TS, not from Unity**.
4. Document any intentional adaptation (Task<T> / byte[] / etc.) in code comment
   `// adapt: <TS-pattern> → <C#-pattern>` so Codex can verify the adaptation
   is structural and not logical.

## Repository layout (v2, post-Codex-review-r1 + simplifications)

```
turnkey-sdk-csharp/                                (GitHub: KyuzanInc/turnkey-sdk-csharp, PRIVATE)
├── README.md                                       — single README, packed into NuGet via <PackageReadmeFile>
│                                                     D15 disclaimer in body. (NO prepack swap, NO README.public.md.)
├── NOTICE                                          — upstream Turnkey TS SDK attribution
├── LICENSE                                         — MIT (Kyuzan Inc.)
├── CHANGELOG.md                                    — initial empty (v0.1.0-alpha.0)
├── .editorconfig                                   — C# style (4-space indent, csharp_new_line_before_open_brace=all)
├── .gitignore                                      — .NET / VS / Rider / npm tarball artifacts
├── turnkey-sdk-csharp.sln                          — root sln: src + tests
├── Directory.Build.props                           — LangVersion=10, Nullable=enable, TreatWarningsAsErrors=true,
│                                                     GenerateDocumentationFile=true, ImplicitUsings=disable
├── Directory.Packages.props                        — CPM: BouncyCastle.Cryptography 2.5.0, System.Text.Json 8.0.5,
│                                                     xunit 2.9.2, FluentAssertions 6.12.2, coverlet.collector 6.0.4,
│                                                     PublicApiGenerator (test-only, latest stable)
├── packages.lock.json                              — generated by `dotnet restore --use-lock-file`, committed
│                                                     to lock transitive deps for both TFMs
├── src/
│   ├── turnkey-sdk-csharp.csproj                   — <TargetFrameworks>netstandard2.1;net8.0</TargetFrameworks>
│   │                                                 PackageId=KyuzanInc.Turnkey.Sdk, AssemblyName=KyuzanInc.Turnkey.Sdk,
│   │                                                 RootNamespace=Turnkey, Version=0.1.0-alpha.0
│   │                                                 PackageReadmeFile=README.md, PackageLicenseExpression=MIT
│   │                                                 deps: BouncyCastle.Cryptography, System.Text.Json
│   │                                                 NO Microsoft.Extensions.Logging.Abstractions (D16)
│   ├── Encoding.cs                                 PORT — `namespace Turnkey; public static class Encoding`
│   │                                                 fresh port from @turnkey/encoding@0.6.0 (npm tarball)
│   │                                                 Unity Encoding.cs used as C# adaptation reference only
│   ├── Crypto.cs                                   PORT — `namespace Turnkey; public static class Crypto`
│   │                                                 fresh port from @turnkey/crypto@2.8.8 (npm tarball)
│   │                                                 D17: BouncyCastle wraps primitives only.
│   │                                                       BANNED: BouncyCastle HPKE, BouncyCastle HKDF helpers,
│   │                                                       BouncyCastle KDF wrappers, Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
│   │                                                       (use System.Security.Cryptography.HKDF on net8.0 only if
│   │                                                       it matches @noble/hashes/hkdf bit-for-bit; else self-port)
│   │                                                 JObject → JsonDocument (System.Text.Json)
│   ├── CryptoConstants.cs                          RENAME — Unity UnityConstants.cs renamed.
│   │                                                 Same public surface (CURVE_NAME, COMPRESSED_PUBLIC_KEY_SIZE,
│   │                                                 P256_P, P256_B, P256_A_OFFSET). Same namespace Turnkey.
│   ├── ApiKeyStamper.cs                            PORT — `namespace Turnkey; public class ApiKeyStamper`
│   │                                                 fresh port from @turnkey/api-key-stamper@0.5.0 (npm tarball)
│   │                                                 JsonUtility.ToJson → JsonSerializer.Serialize via TurnkeyJsonContext
│   ├── Http.cs                                     PORT — `namespace Turnkey; public class Http`
│   │                                                 fresh port from @turnkey/http@3.16.0 (npm tarball)
│   │                                                 nested DTOs kept (SignedRequest, Stamp, *RequestBody, *Parameters)
│   └── TurnkeyJsonContext.cs                       NEW — `[JsonSourceGenerationOptions]` + `[JsonSerializable]`
│                                                     `partial class TurnkeyJsonContext : JsonSerializerContext`
│                                                     covers all nested DTOs in ApiKeyStamper.cs + Http.cs
│                                                     all JsonSerializer.Serialize/Deserialize go via
│                                                     TurnkeyJsonContext.Default.<Type> (no reflection path)
├── tests/
│   ├── turnkey-sdk-csharp.Tests.csproj             — <TargetFramework>net8.0</TargetFramework>
│   │                                                 ProjectReference: ../src/turnkey-sdk-csharp.csproj
│   │                                                 deps: xunit, xunit.runner.visualstudio, FluentAssertions,
│   │                                                 coverlet.collector, Microsoft.NET.Test.Sdk
│   │                                                 NO NSubstitute (Codex r1 over-scope finding)
│   ├── EncodingTests.cs                            NEW — hex (incl. leading zero, odd length, invalid chars),
│   │                                                 base58 (empty, 1-char, multi-char), base58check (bad checksum),
│   │                                                 ConcatUint8Arrays. Vectors derived from pinned @turnkey/encoding source.
│   ├── CryptoTests.cs                              NEW —
│   │                                                 • HKDF: RFC 5869 A.1-A.7 + golden fixtures from @noble/hashes
│   │                                                 • HPKE: RFC 9180 §A.3 + Turnkey sample bundles
│   │                                                 • Tonelli-Shanks ModSqrt: NIST P-256 + edge
│   │                                                 • GetPublicKey: NIST CAVP P-256 vectors
│   │                                                 • CompressRawPublicKey: even/odd Y + invalid prefix
│   │                                                 • UncompressRawPublicKey: even/odd Y + bad prefix + out-of-range
│   │                                                 • Decrypt/EncryptCredentialBundle: Turnkey-pinned sample
│   │                                                 • DecryptExportBundle: legacy + current envelope
│   │                                                 • VerifySessionJwtSignature: positive + negative + malformed
│   ├── ApiKeyStamperTests.cs                       PORT — from Unity ApiKeyStamperTests, [Test]→[Fact]
│   │                                                 + golden fixture: signed request bytes from pinned Node source
│   ├── HttpTests.cs                                NEW — Stamp* method outputs verified against golden Node fixtures
│   │                                                 (canonical JSON body bytes, SignedRequest shape, header values)
│   ├── PublicApiSnapshotTests.cs                   NEW — PublicApiGenerator snapshot test
│   │                                                 catches accidental namespace/type/member-signature drift
│   ├── Fixtures/
│   │   ├── README.md                               — provenance: which fixture from which source / RFC / Node script
│   │   ├── hkdf-rfc5869.json                       — RFC 5869 test cases
│   │   ├── hpke-rfc9180-a3.json                    — RFC 9180 §A.3 vectors
│   │   ├── p256-nist-cavp.json                     — NIST CAVP P-256 vectors
│   │   ├── turnkey-export-bundle-sample.json       — generated from @turnkey/crypto@2.8.8 sample
│   │   ├── turnkey-credential-bundle-sample.json   — generated from @turnkey/crypto@2.8.8 sample
│   │   ├── turnkey-issued-jwt-sample.txt           — Turnkey-issued JWT sample (de-identified)
│   │   ├── api-key-stamper-golden.json             — generated from @turnkey/api-key-stamper@0.5.0
│   │   │                                              (input bytes → expected stamp header bytes)
│   │   └── http-signed-request-golden.json         — generated from @turnkey/http@3.16.0
│   │                                                  (request body + key → expected signed request shape)
│   ├── Generators/
│   │   └── README.md                               — instructions for re-generating golden fixtures
│   │                                                 via `node scripts/gen-fixtures.mjs` in a pinned env
│   └── E2E/
│       └── TurnkeyWhoamiTests.cs                   NEW — skipped if env vars absent
│                                                       Trait("Category", "E2E") for filter
├── docs/
│   └── security/
│       └── threat-model.md                         NEW — scoped per Codex r1:
│                                                     • API-key in-memory handling
│                                                     • Signing path
│                                                     • Crypto primitives surface
│                                                     • Serialization wire safety
│                                                     • Test-secret handling
│                                                     (storage / persistence is OUT OF SCOPE — that belongs
│                                                      to peak-sdk-csharp, not turnkey-sdk-csharp)
├── codex-crypto-reviews/
│   ├── README.md                                   — SOP, prompt template (v2 per Codex r1 critique),
│   │                                                 pass criteria, evidence file naming
│   ├── turnkey-source-pins.md                      — npm version pin table + tarball sha256 + GitHub SHA (secondary)
│   ├── unity-source-pins.md                        — Unity port submodule SHA from peak (039d8e4...)
│   ├── peak-lockfile-evidence.md                   — pnpm-lock.yaml excerpts proving the pins
│   ├── codex-crypto-review.sh                      — review automation: feeds upstream snapshot + .cs file
│   │                                                 + prompt template to `codex exec` with high reasoning
│   ├── upstream-snapshots/                         — `npm pack` extracted trees (committed)
│   │   ├── tarball-checksums.txt                   — sha256 of each .tgz
│   │   ├── turnkey-crypto-2.8.8/                   — extracted package/ tree
│   │   ├── turnkey-http-3.16.0/
│   │   ├── turnkey-api-key-stamper-0.5.0/
│   │   └── turnkey-encoding-0.6.0/
│   ├── Encoding.cs-r{1,2,3}-YYYYMMDD.md            — 3 evidence files
│   ├── CryptoConstants.cs-r{1,2,3}-YYYYMMDD.md
│   ├── Crypto.cs-r{1,2,3}-YYYYMMDD.md
│   ├── ApiKeyStamper.cs-r{1,2,3}-YYYYMMDD.md
│   ├── Http.cs-r{1,2,3}-YYYYMMDD.md
│   └── TurnkeyJsonContext.cs-r{1,2,3}-YYYYMMDD.md
└── .github/
    └── workflows/
        ├── ci.yml                                  — dual-target build/test, coverage gate ≥80%,
        │                                              `dotnet pack` validation, packages.lock.json gate
        └── repo-privacy-check.yml                  — verify `gh repo view --json visibility` == PRIVATE,
                                                       run on push to main
```

## Codex review prompt template (v2, post-Codex-review-r1 critique)

```
SYSTEM:
You are reviewing a C# port of the Turnkey TypeScript SDK file: **{cs-file}**
The pinned upstream npm tarball is at:
  codex-crypto-reviews/upstream-snapshots/turnkey-{pkg}-{version}/
  (sha256 in tarball-checksums.txt — record the matching hash in your output)
The C# file is at:
  src/{cs-file}

REQUIRED OUTPUTS for this review round (all must appear in your output):

A. **Source pin acknowledgement**: state the upstream package name, version,
   tarball sha256 (from tarball-checksums.txt), and the C# file's
   `git rev-parse HEAD` SHA.

B. **Method coverage table**: for every public + internal helper method in
   {cs-file}, list it in a markdown table with columns:
     - C# method (file:line)
     - Upstream TS function (path:line within upstream-snapshots/...)
     - Status: REVIEWED / NOT-REVIEWED (no upstream / no fixture)
     - Notes (one line)
   If you cannot find the upstream counterpart, set Status = NOT-REVIEWED with
   the reason — do NOT skip the row.

C. **Intentional adaptations**: list every C#/TS pattern adaptation:
     - Task<T> ↔ Promise<T>
     - byte[] ↔ Uint8Array
     - BouncyCastle <X> ↔ noble <Y>
     - System.Text.Json ↔ JSON.stringify
     - BigInteger ↔ BigInt
     - exceptions ↔ Error/throw
   For each, state why it is structural (does not change wire bytes or
   observable behavior).

D. **D17 enforcement check**: confirm that for Crypto.cs only,
   BouncyCastle is used **only** for primitives (ECDSA / ECDH / AES-GCM /
   SHA-256 / HMAC / BigInteger / EC point ops). The following MUST NOT
   be present in C# Crypto.cs:
     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
     - Org.BouncyCastle.Crypto.Hpke.*
     - Any "high-level KDF" or "high-level HPKE" wrapper
   If any banned API is present, flag it as a divergence.

E. **Logic divergence findings**: every place where C# behavior differs from
   the upstream TS in any of the following dimensions:
     - algorithm step order
     - constant values
     - error handling (which condition throws what)
     - byte ordering (big-endian / little-endian)
     - leading-zero handling (hex / base64 / EC coordinates)
     - padding (PKCS7, base64url, none)
     - rounding / normalization
     - signature format (DER vs raw r||s, low-S)
     - DTO shape (field names, order, presence, optionality)
     - JSON serialization (property order, casing, null handling, escaping)
   For each, include:
     - C#: file:line
     - TS: upstream-snapshots/{...}:line
     - Semantic difference
     - Suggested fix (revert C# / re-port from TS)

F. **Fixture comparison gate**: for every test fixture that touches this
   file (in tests/Fixtures/), confirm the fixture was generated from the
   pinned upstream package (Generators/ provenance) and the C# test asserts
   the same bytes the upstream Node produces.

G. **Unresolved assumptions**: list assumptions you could not verify.

PASS criterion for this round: B has zero NOT-REVIEWED rows OR each
NOT-REVIEWED row has a documented justification AND E has zero entries
AND D is "no banned API present" AND F is "all fixtures match".

DO NOT use phrases like "looks good" or "no divergence found" without
producing sections A–F. If you cannot produce them, return FAIL with the
reason.
```

## Pass criteria for the 3-round Codex review (per file)

- All 3 rounds executed with `codex exec -s read-only -c model_reasoning_effort=high`.
- Each round produces sections A–G.
- Round 3 must report: B has zero NOT-REVIEWED rows, E has zero entries,
  D is "no banned API present", F is "all fixtures match".
- Each round is a **fresh codex invocation** (no `--resume`).
- Evidence saved to `codex-crypto-reviews/{cs-file}-r{N}-{YYYYMMDD}.md`
  with the exact codex stdout pasted in.

## Acceptance criteria

- [ ] Repo `KyuzanInc/turnkey-sdk-csharp` exists and is **PRIVATE**
  (verified via `gh repo view KyuzanInc/turnkey-sdk-csharp --json visibility`).
- [ ] `dotnet restore --use-lock-file` produces a committed `packages.lock.json`
  for both TFMs.
- [ ] `dotnet build turnkey-sdk-csharp.sln -c Release` passes on both
  `netstandard2.1` and `net8.0`.
- [ ] `dotnet test turnkey-sdk-csharp.sln -c Release` passes, **line coverage ≥80%**.
- [ ] `dotnet pack src/turnkey-sdk-csharp.csproj -c Release` produces a valid
  NuGet package with README.md and LICENSE inside.
- [ ] CI workflow green on push and PR.
- [ ] Repo-privacy CI check passes (asserts visibility == PRIVATE).
- [ ] PublicApiSnapshotTests.cs is committed with baseline.
- [ ] `codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt` exists.
- [ ] **6 implementation files × 3 Codex rounds = 18 evidence files**, round 3
  per file reports clean per the pass criteria above.
- [ ] `turnkey-source-pins.md` records each tarball sha256.
- [ ] `unity-source-pins.md` records port-base submodule SHA
  (`039d8e4801095e46cbadca188702535a0e76e5dd`).
- [ ] `peak-lockfile-evidence.md` cites peak pnpm-lock.yaml lines for each pin.
- [ ] Namespace is `Turnkey` (Unity parity). PackageId is `KyuzanInc.Turnkey.Sdk`.
- [ ] README.md contains the D15 disclaimer + D17 1:1-port disclosure + peak versions.
- [ ] `NOTICE` file attributes the Turnkey TS source.
- [ ] `docs/security/threat-model.md` initial draft committed; scope = key handling
  / signing / crypto primitives / serialization (storage is OUT OF SCOPE).

## Execution order

1. **(this work)** Plan v2 → Codex review r2 → user-visible iteration if needed.
2. Create GitHub repo `KyuzanInc/turnkey-sdk-csharp` (private).
3. Local clone outside peak. Initial scaffold commit:
   sln, csproj, props, packages.lock.json (initial), editorconfig, gitignore,
   README, LICENSE, NOTICE, CHANGELOG, CI workflows (ci.yml + repo-privacy-check.yml),
   threat-model.md, codex-crypto-reviews/ skeleton (README + SOP).
4. Run `npm pack` for the 4 Turnkey packages. Commit `upstream-snapshots/` +
   `tarball-checksums.txt` + `turnkey-source-pins.md` + `unity-source-pins.md` +
   `peak-lockfile-evidence.md`.
5. Port `Encoding.cs` + `EncodingTests.cs`. Codex r1 → r2 → r3.
6. Port `CryptoConstants.cs`. Codex r1 → r2 → r3.
7. Port `TurnkeyJsonContext.cs` skeleton (will grow with each new DTO).
   Codex r1 → r2 → r3 after final DTO set is fixed.
8. Port `ApiKeyStamper.cs` + `ApiKeyStamperTests.cs` + golden fixture from
   pinned Node source. Codex r1 → r2 → r3.
9. Port `Crypto.cs` + `CryptoTests.cs` + golden fixtures from pinned Node source.
   Codex r1 → r2 → r3. **Most rigorous round.**
10. Port `Http.cs` + `HttpTests.cs` + golden fixtures. Codex r1 → r2 → r3.
11. PublicApiSnapshotTests.cs + run to create baseline.
12. CI green on push + PR.
13. Final integrated Codex review covering all 6 files at once + the test suite.

## Risk register (v2)

| ID | Risk | Severity | Mitigation |
|---|---|---|---|
| R-NPM | npm package contents differ from GitHub tags | **High** | Q2: npm tarball is authoritative; SHA committed. |
| R-JSON | JSON serialization drift invalidates signatures | **High** | golden Node fixtures byte-compared in tests. |
| R-BIG | C# BigInteger signedness/endian corrupts P-256 scalars | **High** | targeted unit tests with NIST CAVP vectors + leading-zero edge cases. |
| R-SIG | BouncyCastle signature format diverges (DER vs r||s, low-S) | **High** | DER + low-S explicit in port; round-trip via PEM and against Node fixture. |
| R-LEAD | Leading-zero loss in hex/base64url/EC coordinates | **High** | encoding edge tests + EC coordinate round-trip tests. |
| R-AEAD | AES-GCM nonce/tag/AAD layout subtle divergence | **High** | HPKE §A.3 fixture passes round-trip + Turnkey sample bundle round-trip. |
| R-HPKE | HPKE randomness blocks deterministic fixtures | Medium | HpkeEncrypt round-trips into HpkeDecrypt; HpkeDecrypt against deterministic fixtures. |
| R-AOT | System.Text.Json source-gen misses nested DTOs / AOT trim | Medium | TurnkeyJsonContext covers every DTO; analyzer verifies. |
| R-LEAK | CI logs leak keys/payloads/bundles | Medium | secrets via env, never echo, fixtures de-identified. |
| R-CVE | Pinned deps may have CVEs (System.Text.Json 8.0.5 / BouncyCastle 2.5.0) | Medium | dependabot alerts (later), document in threat-model. |
| R-BRAND | Package name / disclaimer trademark concern | Low | D15 disclaimer + NOTICE + private feed mitigates. |
| R-CODEX-BLIND | Codex misses runtime mutation / timing / side-channel | Medium | post-v0.1.0 paid audit (out of scope here, in plan). |
| R-DRIFT | Turnkey upstream future bumps | Low | re-run Codex review on bump; pin SHAs. |

## Deferred non-blocking items (from Codex r2 review)

Per Codex r2 GO verdict; address during implementation iteration:

1. **PR0 fixture script**: commit a real `tests/Fixtures/Generators/gen-fixtures.mjs`
   that exports the exact npm-version pinned Node code that produced each golden
   fixture; record output hashes in `tarball-checksums.txt` next to each `.json`.
2. **Authoritative file recording**: PR0 step that records, per pinned npm
   package, which files inside `package/` (TS or built JS) are the authoritative
   source for Codex review. Some packages may ship only built JS; document this.
3. **HKDF TFM unification**: pick ONE HKDF implementation path for both
   `netstandard2.1` and `net8.0` (default: self-ported from `@noble/hashes/hkdf`).
   Do NOT branch on TFM. If branching is unavoidable, fixtures must cover both
   paths.
4. **Codex prompt tightening**: after the first file's r1 round produces output,
   refine the prompt template to require explicit checks of
   `JsonSourceGenerationOptions` (naming policy, null handling, escaping,
   property order) against Node-fixture bytes.

## What this plan deliberately does NOT do

- Does not create `peak-sdk-csharp` (separate work).
- Does not create `peak-sdk-csharp-unity` (separate work).
- Does not publish to nuget.org.
- Does not modify any file in the peak monorepo.
- Does not add a pnpm wrapper (standalone repo).
- Does not include storage / persistence concerns (peak-sdk-csharp scope).
- Does not include IStorage / PeakClient / AuthenticatedPeakClient (peak-sdk-csharp scope).
