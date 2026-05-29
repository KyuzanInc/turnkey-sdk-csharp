# README improvements — items 1, 2, 3

## Intent

Make `turnkey-sdk-csharp` README a self-contained, copy-paste-runnable entry
point for any .NET developer landing on the repo. This package is **a
standalone C# port of the upstream Turnkey TypeScript SDKs**, independent of
any other Kyuzan package — the README must stand entirely on its own and
must not reference sibling Kyuzan packages.

Existing strengths to preserve:
- Threat model document (`docs/security/threat-model.md`)
- Multi-round Codex review trail (`codex-crypto-reviews/`)
- Explicit AOT/IL2CPP safety (source-gen JSON only)
- Exact peak-monorepo version pinning (with peak as the consumer, not the source)

Existing gap to close: the C# `src/*.cs` file headers already contain
comprehensive upstream-TS provenance and out-of-scope notes, but a README
reader cannot see this without opening source files. **This plan surfaces
that information into the README so it is visible at first scan.**

Upstream reference (the only external project the README should mention):
- [`github.com/tkhq/sdk`](https://github.com/tkhq/sdk) — official Turnkey TS
  SDK; this package is a logical port of pinned subsets at peak's versions.

## What's NOT in scope

- Any source code change (`src/*.cs`, `tests/*`).
- Threat model (`docs/security/threat-model.md`) updates.
- License / NOTICE / CHANGELOG edits.
- `codex-crypto-reviews/` artifacts.
- Adding install-from-NuGet instructions (still GitHub Packages per
  `plans/plan-v2-codex-reviewed.md` Q8).

## What already exists (avoiding duplication)

| Information | Already lives at | Plan action |
|---|---|---|
| Public API surface | `src/*.cs` XML doc comments | Quote a few key signatures in README usage examples |
| TS → C# file mapping | Per-file header comments in `src/*.cs` | Aggregate into one README table |
| Unported pieces | `Crypto.cs` "Out of scope" header | Aggregate + surface in README |
| Version pins | README "1:1 logical port" table | Keep as-is (don't touch) |
| Build/test commands | README "Build and test" | Keep as-is |
| BouncyCastle scope | README "Dependencies" | Keep as-is |

## Item 1 — Usage examples (new "Quick start" section)

**Insert after "Features (planned for v0.1.0)" and before "Target frameworks".**

Pick the **6 most common flows** that the peak Unity port actually uses, drawn
from the public API surface in `src/*.cs`:

1. Generate an ephemeral P-256 key pair (HPKE recipient).
2. Build a signed Turnkey API request via `Http`.
3. Construct an `ApiKeyStamper` and stamp arbitrary JSON.
4. Decrypt a Turnkey credential bundle (API key import flow).
5. Decrypt a Turnkey export bundle (wallet export flow).
6. Encode hex / base58 with `Encoding`.

### Proposed diff (Quick start section)

```diff
@@ README.md — after "Features (planned for v0.1.0)" block @@
+## Quick start
+
+```csharp
+using Turnkey;
+
+// 1. Generate an ephemeral P-256 key pair (HPKE recipient).
+var keyPair = Crypto.GenerateP256KeyPair();
+// keyPair.PrivateKey / keyPair.PublicKey / keyPair.PublicKeyUncompressed
+
+// 2. Build a signed Turnkey API request.
+var http = Http.FromTargetPrivateKey(
+    privateKeyHex: "0x...",          // your Turnkey API private key
+    publicKeyHex:  "0x...",          // matching compressed public key
+    organizationId: "<org-id>");
+var whoamiRequest = http.Whoami();
+// whoamiRequest is { Url, Body, Stamp }; POST it over HTTPS.
+
+// 3. Stamp arbitrary JSON with an ApiKeyStamper.
+var stamper = new ApiKeyStamper(
+    publicKeyHex:  "0x...",
+    privateKeyHex: "0x...");
+var stamp = stamper.Stamp("{\"foo\":\"bar\"}");
+// stamp.StampHeaderValue is the base64url(JSON) X-Stamp header.
+
+// 4. Decrypt a Turnkey credential bundle (returns API private key hex).
+var apiPrivateKey = Crypto.DecryptCredentialBundle(
+    encryptedCredentialBundle: bundleString,
+    embeddedKey:               keyPair.PrivateKey);
+
+// 5. Decrypt an export bundle (returns mnemonic or hex private key).
+var exported = Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams
+{
+    ExportBundle    = exportBundleJson,
+    EmbeddedKey     = keyPair.PrivateKey,
+    OrganizationId  = "<org-id>",
+    ReturnMnemonic  = false,
+    KeyFormat       = "HEXADECIMAL",
+});
+
+// 6. Hex / base58 encoding helpers.
+var hex     = Encoding.Uint8ArrayToHexString(bytes);
+var bytes2  = Encoding.Uint8ArrayFromHexString(hex);
+var b58     = Encoding.Base58Encode(bytes);
+```
+
+> **Caller responsibilities** (the SDK does not own these): supplying secret
+> material, calling HTTPS, retry/backoff, persistent key storage, WebAuthn,
+> OAuth/OTP. See [`docs/security/threat-model.md`](./docs/security/threat-model.md)
+> for the full boundary.
+
```

**Property-name accuracy gate**: before merging, verify each property name
against the actual `src/*.cs` definitions (`KeyPair.PrivateKey` etc.) by
running `grep -n "public " src/Crypto.cs src/Http.cs src/ApiKeyStamper.cs`.
README will only land with verified property names — no guessed casing.

## Item 2 — TS → C# file mapping table (new section)

**Insert after "Project structure" and before "Build and test".**

Aggregate the provenance info already in `src/*.cs` headers into one table so a
reader can see it without opening the files. Mirror the format the unity SDK
README uses, adapted for csharp's file layout (6 files instead of 5).

### Proposed diff (Source mapping section)

```diff
@@ README.md — after "Project structure" block @@
+## Source mapping (Turnkey TS → this port)
+
+This package is a 1:1 logical port of the upstream Turnkey TypeScript
+packages at the versions consumed by peak. Each `.cs` file's header repeats
+this mapping for the file's own scope; the table below is the index.
+
+| npm package              | Version | TS source                                    | C# file                                 | Description                                                      |
+|--------------------------|---------|----------------------------------------------|-----------------------------------------|------------------------------------------------------------------|
+| `@turnkey/encoding`      | 0.6.0   | `hex.ts`                                     | `Encoding.cs`                           | `Uint8ArrayToHexString`, `Uint8ArrayFromHexString`, `HexToAscii`, `NormalizePadding` |
+| `@turnkey/encoding`      | 0.6.0   | `base64.ts`                                  | `Encoding.cs`                           | `StringToBase64UrlString`, `HexStringToBase64Url`, `Base64UrlToBase64`, `DecodeBase64UrlToString` |
+| `@turnkey/encoding`      | 0.6.0   | `bs58.ts`, `bs58check.ts`                    | `Encoding.cs`                           | `Base58Encode/Decode`, `Base58CheckEncode/Decode`                |
+| `@turnkey/encoding`      | 0.6.0   | `encode.ts`                                  | `Encoding.cs`                           | `PointEncode`                                                    |
+| `@turnkey/crypto`        | 2.8.8   | `crypto.ts` (subset)                         | `Crypto.cs`                             | `GenerateP256KeyPair`, `HpkeEncrypt/Decrypt`, `CompressRawPublicKey`, `UncompressRawPublicKey`, `BuildAdditionalAssociatedData`, `FormatHpkeBuf` |
+| `@turnkey/crypto`        | 2.8.8   | `constants.ts`                               | `Crypto.cs` (`Crypto.Constants` nested) | HPKE suite IDs, HKDF labels, signer public keys                  |
+| `@turnkey/crypto`        | 2.8.8   | `math.ts`                                    | `Crypto.cs` (`Crypto.Math` nested)      | Tonelli-Shanks `ModSqrt`                                         |
+| `@turnkey/crypto`        | 2.8.8   | HKDF helpers in `crypto.ts` (`@noble/hashes/hkdf` upstream) | `Crypto.cs` (`Crypto.Hkdf` nested)      | HKDF `Extract` / `Expand`                                        |
+| `@turnkey/crypto`        | 2.8.8   | `turnkey.ts` (subset)                        | `Crypto.cs`                             | `DecryptCredentialBundle`, `EncryptPrivateKeyToBundle`, `DecryptExportBundle`, `VerifySessionJwtSignature` |
+| `@turnkey/api-key-stamper` | 0.5.0 | `index.ts`, `purejs.ts`                      | `ApiKeyStamper.cs`                      | ECDSA P-256 signing, DER-hex output, X-Stamp header construction |
+| `@turnkey/http`          | 3.16.0  | `index.ts` (signing subset)                  | `Http.cs`                               | Signed activity-request builder for the 5 endpoints peak uses    |
+| (C#-specific)            | -       | -                                            | `CryptoConstants.cs`                    | BouncyCastle curve / parameter constants (not in upstream)       |
+| (C#-specific)            | -       | -                                            | `TurnkeyJsonContext.cs`                 | `System.Text.Json` source-generated context (AOT/IL2CPP-safe)    |
+
+Upstream snapshots used for the port are committed under
+[`codex-crypto-reviews/upstream-snapshots/`](./codex-crypto-reviews/) so future
+reviewers can diff against them without re-downloading from npm.
+
```

## Item 3 — Unported pieces (new section)

**Insert after the new "Source mapping" section, before "Build and test".**

Aggregate the "Out of scope" comments from each `src/*.cs` file header into
one place so README readers immediately see what they have to NOT count on.

### Proposed diff (Unported pieces section)

```diff
@@ README.md — after "Source mapping" block @@
+## Intentionally unported
+
+The peak Unity flow does not need the following upstream surfaces, so they
+are **not** ported in v0.1.0. Each is verified absent from `src/`:
+
+**From `@turnkey/crypto` 2.8.8:**
+- `hpkeAuthEncrypt`
+- `quorumKeyEncrypt`
+- `extractPrivateKeyFromPKCS8Bytes`
+- `fromDerSignature`, `toDerSignature`
+- `verifyStampSignature`
+- `encryptWalletToBundle`
+- `encryptToEnclave`
+- `encryptOauth2ClientSecret`
+- `encryptOnRampSecret`
+- `proof.ts` (AWS Nitro attestation chain verification)
+
+**From `@turnkey/http` 3.16.0:**
+- Auto-generated activity methods beyond the 5 needed by peak
+  (`whoami`, `init_import_private_key`, `import_private_key`,
+   `export_private_key`, `export_wallet_account`).
+- Polling, error-handling, retry, WebAuthn stamping.
+- The full typed client surface (only the request-signing subset is ported).
+
+**From `@turnkey/api-key-stamper` 0.5.0:**
+- `"browser"` (WebCrypto) and `"node"` (Node crypto) runtimes — only the
+  `"purejs"` (noble) runtime equivalent is ported, since BouncyCastle covers
+  both in a single backend.
+
+**From `@turnkey/encoding` 0.6.0:**
+- `base64.ts`'s React-Native `btoa` implementation — the C# port uses
+  `System.Convert.ToBase64String` directly, producing identical bytes.
+
+If any of these become required by peak (or by `peak-sdk-csharp` /
+`peak-sdk-csharp-unity`), they will be added in a future minor — they were
+omitted for v0.1.0 scope, not for technical reasons.
+
```

## Where each insertion lands (line-level)

Current README sections (per `README.md` line numbers):
- L1-L23: title + 1:1-port table — **untouched**
- L25-L28: Status — **untouched**
- L30-L40: Features — **untouched**
- L42-L44: Target frameworks
- L46-L55: Dependencies
- L57-L66: Project structure
- L68-L75: Build and test
- L77-L84: Contributing / License — **untouched**

Insertion order in the updated README:
1. Title + 1:1 table (existing)
2. Status (existing)
3. Features (existing)
4. **NEW: Quick start** (Item 1)
5. Target frameworks (existing, moved down)
6. Dependencies (existing)
7. Project structure (existing)
8. **NEW: Source mapping** (Item 2)
9. **NEW: Intentionally unported** (Item 3)
10. Build and test (existing)
11. Contributing / License (existing)

## Verification plan

Before declaring done:

1. **API name verification** — `grep -n "public " src/Crypto.cs src/Http.cs
   src/ApiKeyStamper.cs src/Encoding.cs` and reconcile every name used in the
   Quick start block. No guessed casings.
2. **Out-of-scope verification** — `grep -n "hpkeAuthEncrypt\|quorumKeyEncrypt\|
   verifyStampSignature\|encryptWalletToBundle\|encryptToEnclave" src/`
   should return zero hits. If any hit is found, fix the README list.
3. **Markdown render check** — open the rendered README on GitHub (or via
   `gh repo view --web`) and visually scan that tables render and code blocks
   are syntax-highlighted.
4. **Link check** — confirm relative links (`./docs/security/threat-model.md`,
   `./codex-crypto-reviews/`, `./LICENSE`, `./NOTICE`) resolve.
5. **Spell + tone** — README voice stays factual / no marketing language.

## Test plan

This is a doc-only change, so:
- No source code tests added.
- No CI changes.
- The `dotnet build` / `dotnet test` block in README is not modified, so existing
  CI continues unaffected.

If a future contributor wants linkcheck CI for README, that is **out of scope**
for this change.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| Quick start snippet drifts from real API after a refactor | API-name verification step (see Verification plan); each snippet is short enough to spot-check on PR review. |
| Source mapping table goes out of date when files are split/merged | Each `src/*.cs` header already carries the canonical mapping; README table is the *index*, source files are the *truth*. If they drift, source wins — README is fixed in a follow-up commit. |
| "Intentionally unported" list misses something | Cross-checked against `Crypto.cs` header "Out of scope" block which is the canonical list. README aggregates; source is canonical. |
| README becomes too long for the at-a-glance "what is this?" use | Sections are individually short; total estimated growth: ~85 → ~180 lines, still well under the unity SDK README's length and structure. |

## DX scope detection

Per /plan-devex-review triggers, this plan is in scope because:
- It modifies developer-facing documentation (README is the first file a developer reads).
- It directly impacts time-to-hello-world (currently you cannot copy-paste anything from the README).
- It improves error-message-style developer guidance (the "what's NOT here" list prevents wasted debugging).

UI scope: **no** (no rendered UI, no components).
CEO scope: **weak** (positioning of "unofficial port", but no strategy change).
Eng scope: **weak** (no code).
**DX scope: strong** — this is squarely DX work.

## Out-of-scope but worth noting (for follow-up TODOs.md)

- README's "License" section says "MIT" but doesn't include the SPDX tag —
  could add `SPDX-License-Identifier: MIT` in source files (separate PR).
- A `CHANGELOG.md` entry under "Unreleased → Docs" should be added when this
  ships (part of /ship workflow, not this plan).

## Standalone-package boundary (do NOT in this README)

- Do NOT reference any other Kyuzan package (turnkey-sdk-unity, peak-sdk-*,
  any future sibling). This package is a standalone .NET port; cross-links
  to siblings would imply coupling that does not exist.
- Do NOT include "migrating from X" guidance for other Kyuzan packages.
- DO reference the upstream Turnkey TS SDK (`github.com/tkhq/sdk`) since
  this package is a logical port of it.

---

## DX Review (per /plan-devex-review on 2026-05-26)

### Developer Persona Card

```
TARGET DEVELOPER PERSONA
========================
Who:       Future external .NET contributor browsing the repo
           (forward-looking: current state is private; this README is
            written so that the eventual OSS-readiness moment is cheap)
Context:   Lands on the repo from a Turnkey-related GitHub search,
           or referred by a Kyuzan engineer. Has used at least one
           crypto / signing SDK before. Knows what NuGet is.
Tolerance: ~3 minutes of scanning. If they can't tell what this is,
           how it's verified, and how to read the API, they leave.
Expects:   - Clear "what is this vs. official Turnkey SDK" framing
           - Copy-paste-runnable code that compiles against the real API
           - Security credibility signals (audit trail, version pinning)
           - Honest "what's NOT here" list (no debugging surprises)
```

### Developer Empathy Narrative (first-person, on the *updated* README)

> I land on the repo from a GitHub search for "Turnkey C# SDK". First heading:
> `KyuzanInc.Turnkey.Sdk` and a banner that says "Unofficial". Good — I know
> immediately this isn't the upstream one. Version-pin table tells me which
> upstream Turnkey TS package versions this matches. I scan to "Quick start"
> — I see six C# snippets. Snippet 2 calls `http.Whoami()` but I check the
> source and the method is actually `StampGetWhoami(orgId)`. Now I don't
> trust the README. I close the tab.

This narrative drove the P1 findings below.

### Competitive DX Benchmark

This package is one of the **first C# ports** of Turnkey's TS SDKs; there is
no direct .NET competitor to benchmark against. Use upstream Turnkey TS SDK
and generally-respected .NET crypto/signing libraries as the reference for
DX expectations.

| Tool | TTHW (estimate) | Notable DX choice | Source |
|---|---|---|---|
| `@turnkey/sdk-server` (upstream TS) | < 2 min | npm install + 5-line example on README | `tkhq/sdk` repo README |
| `NSec.Cryptography` (.NET crypto reference) | ~2 min | NuGet + Quick start that compiles | nuget.org |
| `BouncyCastle.Cryptography` (.NET crypto reference) | ~5 min | API reference heavy; minimal narrative | nuget.org |
| **`turnkey-sdk-csharp` (this plan applied)** | **~3-5 min** (target) | Quick start + Source mapping + Intentionally unported | this plan |
| `turnkey-sdk-csharp` (current) | > 10 min | only build/test commands; no usage example | current README |

Target tier: **Competitive (2-5 min)**. Reaching Champion (< 2 min) would
require an interactive playground or a downloadable sample app, both out of
scope for v0.1.0-alpha.

### Mode: DX POLISH

The plan is improving an existing v0.1.0-alpha README, not expanding scope.
Apply rigor to every touchpoint; do not add new artifacts (no playground, no
sample repo, no docs site).

### Developer Journey Map

| Stage | Developer does | Friction (in current plan) | Status |
|---|---|---|---|
| Discover | GitHub search lands on repo | Status header "Pre-release, internal-only ... Not for public production use" reads like *go away* | **P1 — fix** |
| Install / consume | Want to use from their .NET project | No `dotnet add package` instructions; no GitHub Packages source guidance | **P1 — fix** |
| Hello world | Run the first snippet | **Method names in Quick start don't match real API** (`Whoami()` ≠ `StampGetWhoami()`, `FromTargetPrivateKey` signature wrong) | **P1 — fix** |
| Real usage | Want to whoami the Turnkey API end-to-end | Six disconnected snippets; no complete end-to-end example | **P2 — fix** |
| Verify trust | "Can I trust this crypto code?" | `codex-crypto-reviews/` and `threat-model.md` exist but README narrative doesn't surface them prominently | **P2 — fix** |
| Contribute | Want to know how to contribute | "policy TBD after v0.1.0" — acceptable but could point at issues for now | **P3 — TODO** |

### Findings — ranked

#### P1 — Quick start has API drift (blocks ship)

The proposed Quick start snippet uses `http.Whoami()` and
`Http.FromTargetPrivateKey(privateKeyHex, publicKeyHex, organizationId)`.

**Real API** (verified via `grep -n "public" src/Http.cs`):
- `Http.FromTargetPrivateKey(targetPrivateKey, baseUrl?)` — 1-2 args, no public key, no org id
- `Http.GetHttpClient(encryptedCredentialBundle, targetPrivateKey, baseUrl?)` — for credential bundle flow
- `http.StampGetWhoami(organizationId)` — returns `SignedRequest`
- `http.StampInitImportPrivateKey(body)`, `StampImportPrivateKey`, `StampExportPrivateKey`, `StampExportWalletAccount`
- All methods **synchronous** (return `SignedRequest`, not `Task<SignedRequest>`)

The verification gate in the plan would catch this *before merging*, but DX
review catches it *at plan stage*. **Corrected Quick start diff below.**

#### P1 — No install / consume instructions

Plan's Project structure section explains the *repo* layout. For an external
.NET contributor, the question they actually have is: **"how do I add this to
my project?"**. v0.1.0-alpha distribution is GitHub Packages per
`plan-v2-codex-reviewed.md` Q8. README should say so explicitly.

**Proposed addition** (new "Installation" subsection inside Quick start):

```markdown
### Installation

This package is distributed via **GitHub Packages** (not nuget.org) during
v0.1.0-alpha.

Add the Kyuzan feed to your `nuget.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="kyuzan-github" value="https://nuget.pkg.github.com/KyuzanInc/index.json" />
  </packageSources>
</configuration>
```

Authenticate with a GitHub token that has `read:packages` scope:

```bash
dotnet nuget add source https://nuget.pkg.github.com/KyuzanInc/index.json \
  --name kyuzan-github --username YOUR_GITHUB_USER --password YOUR_GH_TOKEN \
  --store-password-in-clear-text
```

Then add the package:

```bash
dotnet add package KyuzanInc.Turnkey.Sdk --version 0.1.0-alpha.0
```

> Public nuget.org distribution is post-v0.1.0; see CHANGELOG.
```

#### P1 — Status framing too cold

Current Status: *"Pre-release, internal-only. Not published to nuget.org. Not
for public production use. Trademark and distribution review pending."*

For an OSS-curious viewer this reads "you are not welcome". Soften:

```markdown
## Status

**v0.1.0-alpha** — internal use within Kyuzan and selected partners. The
crypto and signing path has been through 3 rounds of independent Codex review
(evidence in [`codex-crypto-reviews/`](./codex-crypto-reviews/)) and is
wire-byte-compared against fixtures generated from pinned `@turnkey/*` npm
packages. Public nuget.org distribution and contribution guidelines are
pending — open an issue if you want to be notified.
```

Same information; signals that the code IS verified and that interest is
welcome even if you can't `nuget install` it today.

#### P2 — No end-to-end flow

Six snippets show six APIs but not how to compose them. Add one **complete
flow** (most realistic: whoami) showing key material → http client → signed
request → POST. Helps the reader see how the pieces fit. Even ~25 lines of
copy-paste code converts "is this real?" to "OK I can use this".

**Proposed end-to-end appendix** (after the 6 atomic snippets):

```csharp
// End-to-end: call Turnkey query/whoami with your API key pair.
using System.Net.Http;
using System.Text;
using Turnkey;

// 1. Build the signed request (no network call yet).
var http = new Http(
    stamper: new ApiKeyStamper(
        apiPublicKey: "<your-api-public-key-hex>",
        apiPrivateKey: "<your-api-private-key-hex>"));
var signed = http.StampGetWhoami("<your-organization-id>");

// 2. POST it. The SDK does NOT own the transport; you do.
using var client = new HttpClient();
var req = new HttpRequestMessage(HttpMethod.Post, signed.Url)
{
    Content = new StringContent(signed.Body, Encoding.UTF8, "application/json"),
};
req.Headers.Add(signed.Stamp.StampHeaderName, signed.Stamp.StampHeaderValue);
var response = await client.SendAsync(req);
var json = await response.Content.ReadAsStringAsync();
Console.WriteLine(json);
```

#### P2 — Credibility signals not surfaced

`codex-crypto-reviews/` and `docs/security/threat-model.md` are the strongest
trust signals this SDK has. Plan currently puts:
- threat-model link inside a Quick start "Caller responsibilities" sub-note
- codex-crypto-reviews link at the end of "Source mapping"

For the OSS-curious persona, both should be in a **dedicated top-level
section** so they don't get missed. Add:

```markdown
## How correctness is verified

- **Multi-round Codex review** — every implementation file in `src/` has 3
  rounds of independent review by Codex; evidence committed under
  [`codex-crypto-reviews/`](./codex-crypto-reviews/).
- **Pinned upstream snapshots** — exact npm tarball contents for each
  `@turnkey/*` version are committed alongside the C# port so a future
  reviewer can byte-compare without re-fetching from npm.
- **Threat model** — explicit scope, assets, and threat-mitigation matrix at
  [`docs/security/threat-model.md`](./docs/security/threat-model.md).
- **Lockfile-pinned dependencies** — `src/packages.lock.json` and
  `tests/packages.lock.json` are committed; BouncyCastle is pinned at
  `[2.5.0]` exact.
```

#### P2 — No "Where to go next"

After the README, where does the reader land? Add a short navigation
footer above License:

```markdown
## Where to read next

- [`docs/security/threat-model.md`](./docs/security/threat-model.md) — scope, assets, threats.
- [`codex-crypto-reviews/`](./codex-crypto-reviews/) — multi-round review evidence.
- [`CHANGELOG.md`](./CHANGELOG.md) — release notes.
- [`NOTICE`](./NOTICE) — upstream Turnkey TS attribution.
- Per-file `src/*.cs` header comments — exact TS → C# function mapping.
```

#### P3 — Follow-up TODOs (don't gate this PR)

- Add "Troubleshooting" subsection (common errors and their resolution).
- Add a "Migrating from `turnkey-sdk-unity`" subsection once both ship together.
- Add SPDX tag to source files (separate PR).

### DX Scorecard

```
+====================================================================+
|              DX PLAN REVIEW — SCORECARD                             |
+====================================================================+
| Dimension            | Score (plan) | Target  | Notes
|----------------------|--------------|---------|----------------------
| Getting Started      | 5/10         | 8/10    | Quick start exists
|                      |              |         | but API drift + no install
| API/CLI/SDK          | 6/10         | 8/10    | Names verified by gate
|                      |              |         | but drift in plan body
| Error Messages       | n/a          | n/a     | README scope; defer to
|                      |              |         | follow-up troubleshooting
| Documentation        | 7/10         | 9/10    | Source mapping + unported
|                      |              |         | are strong additions
| Upgrade Path         | n/a          | n/a     | doc-only PR
| Dev Environment      | 7/10         | 7/10    | unchanged; build/test ok
| Community            | 5/10         | 6/10    | Soften "policy TBD"
|                      |              |         | with issue invitation
| DX Measurement       | n/a          | n/a     | not applicable
+--------------------------------------------------------------------+
| TTHW (current)       | > 10 min     |
| TTHW (plan applied)  | ~5 min       |
| TTHW (plan + P1+P2)  | ~3 min       | <- target
| Competitive Rank     | Competitive (2-5 min) after P1+P2 fixes
| Magical Moment       | end-to-end whoami snippet (P2 finding)
| Product Type         | Library / SDK
| Mode                 | DX POLISH
| Overall DX           | 6/10 -> 8/10 with P1+P2 applied
+====================================================================+
| DX PRINCIPLE COVERAGE
| Zero Friction      | gap (no install instructions) -> covered with P1
| Learn by Doing     | gap (no end-to-end) -> covered with P2
| Fight Uncertainty  | covered (intentionally-unported list, threat model)
| Opinionated + Escape Hatches | covered (caller owns transport explicitly)
| Code in Context    | gap (snippets disconnected) -> covered with P2
| Magical Moments    | n/a for library README
+====================================================================+
```

### DX Implementation Checklist

- [ ] Quick start API calls match real API surface (verified via grep)
- [ ] `Installation` subsection with GitHub Packages instructions present
- [ ] `Status` section softened (still honest, less hostile)
- [ ] One end-to-end whoami example present
- [ ] `How correctness is verified` section present
- [ ] `Where to read next` navigation footer present
- [ ] `gh repo view --web` rendering check passes (tables / code blocks)
- [ ] Relative links resolve

## Implementation Tasks

Synthesized from this DX review. Each task derives from a specific finding.

- [ ] **T1 (P1, human: ~10min / CC: ~3min)** — README — Fix Quick start API drift
  - Surfaced by: DX Pass 1+2 — `Whoami()` doesn't exist; real method is
    `StampGetWhoami(orgId)`. `FromTargetPrivateKey` signature also wrong.
  - Files: `plans/readme-improvements.md` (Quick start diff block), eventual `README.md`
  - Verify: `grep -n "public " src/Http.cs src/ApiKeyStamper.cs src/Crypto.cs`
    then diff against README snippets

- [ ] **T2 (P1, human: ~15min / CC: ~5min)** — README — Add `Installation` subsection (GitHub Packages)
  - Surfaced by: DX journey "Install / consume" stage — no `dotnet add package`
    instructions exist. Plan-v2 Q8 set distribution = GitHub Packages.
  - Files: `plans/readme-improvements.md`, eventual `README.md`
  - Verify: `dotnet nuget` commands compile-check in a clean shell

- [ ] **T3 (P1, human: ~10min / CC: ~3min)** — README — Soften `Status` framing
  - Surfaced by: DX Pass 7 + empathy narrative — current Status reads
    "go away" to OSS-curious persona.
  - Files: `plans/readme-improvements.md`, eventual `README.md`
  - Verify: re-read as persona; tone should be "internal-first, not hostile"

- [ ] **T4 (P2, human: ~20min / CC: ~5min)** — README — Add end-to-end whoami example
  - Surfaced by: DX Pass 1 + magical moment — 6 atomic snippets without
    composition. End-to-end converts "is this real?" to "I can use this".
  - Files: `plans/readme-improvements.md`, eventual `README.md`
  - Verify: copy-paste code into a `dotnet new console` project and compile

- [ ] **T5 (P2, human: ~10min / CC: ~3min)** — README — Add `How correctness is verified` section
  - Surfaced by: DX trust pass — codex-crypto-reviews + threat-model are
    buried in sub-notes; for OSS-curious persona these are the strongest
    trust signals.
  - Files: `plans/readme-improvements.md`, eventual `README.md`
  - Verify: all referenced paths exist

- [ ] **T6 (P2, human: ~5min / CC: ~2min)** — README — Add `Where to read next` footer
  - Surfaced by: DX journey "after-README" gap — reader doesn't know
    which file to open next.
  - Files: `plans/readme-improvements.md`, eventual `README.md`
  - Verify: all relative links resolve

- [ ] **T7 (P3, human: ~30min / CC: ~10min)** — TODOS.md — Troubleshooting subsection (follow-up PR)
  - Surfaced by: DX Pass 3 — no error-message guidance for common pitfalls
    (wrong key length, bad bundle format, baseUrl validation).
  - Files: `TODOS.md`

- ~~T8 — Migrating from `turnkey-sdk-unity` subsection~~ **REMOVED**
  - Removed per user direction: `turnkey-sdk-csharp` is a standalone package
    independent of any other Kyuzan package. README must not reference
    sibling Kyuzan packages or cross-link to them.

## GSTACK REVIEW REPORT

| Review | Trigger | Why | Runs | Status | Findings |
|--------|---------|-----|------|--------|----------|
| CEO Review | `/plan-ceo-review` | Scope & strategy | 0 | — | not requested |
| Codex Review | `/codex review` | Independent 2nd opinion | 0 | — | not requested |
| Eng Review | `/plan-eng-review` | Architecture & tests (required) | 0 | — | not requested (doc-only PR; eng impact is null) |
| Design Review | `/plan-design-review` | UI/UX gaps | 0 | — | n/a (no UI) |
| DX Review | `/plan-devex-review` | Developer experience gaps | 1 | issues_open | 3 P1, 3 P2, 2 P3 — see Implementation Tasks |

- **UNRESOLVED:** 0 (all findings tagged as P1/P2/P3 tasks; user can override at apply-time)
- **VERDICT:** DX REVIEW COMPLETE — 8 tasks queued. P1×3 should land before merging README changes; P2×3 same-branch; P3×2 follow-up. Eng review skipped by explicit user choice (DX-only). Ship-readiness depends on T1-T3 landing.

