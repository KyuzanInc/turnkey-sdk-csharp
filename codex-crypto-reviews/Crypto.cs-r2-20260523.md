# Codex review — src/Crypto.cs — round 2 — 20260523

C# SHA: `73e5109466262896c3383d9c00d8788309eacf21`
Upstream snapshot: `turnkey-crypto-2.8.8`

---

2026-05-23T00:06:35.819952Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e5227-bfc8-76a1-8395-eeca3e67dec3
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/Crypto.cs  (git SHA at last commit touching it: 73e5109466262896c3383d9c00d8788309eacf21)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 2 of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA (73e5109466262896c3383d9c00d8788309eacf21).

B. Method coverage table: every public + internal helper method in src/Crypto.cs
   listed in a markdown table:
     | C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
   Status: REVIEWED or NOT-REVIEWED. If NOT-REVIEWED, give a reason. Do not skip rows.

C. Intentional adaptations: list every C#/TS adaptation pattern with a one-line
   justification that it is structural (no wire-byte or observable-behavior change).

D. (Crypto.cs only) D17 enforcement check. Confirm BouncyCastle is used only for
   ECDSA / ECDH / AES-GCM / SHA-256 / HMAC / BigInteger / EC point primitives.
   Banned APIs (must not appear):
     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
     - Org.BouncyCastle.Crypto.Hpke.*
     - Any "high-level" KDF or HPKE wrapper
   If file is not Crypto.cs, write "N/A (not Crypto.cs)".

E. Logic divergence findings: every place C# behavior differs from upstream TS:
   algorithm step order, constants, error handling, byte ordering, leading-zero
   handling, padding, rounding/normalization, signature format (DER vs raw r||s,
   low-S), DTO shape (field names, order, presence, optionality), JSON
   serialization (property order, casing, null handling, escaping).
   For each item: C# file:line, TS upstream path:line, semantic diff,
   suggested fix.

F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
   this file, confirm it was generated from the pinned upstream package and the
   C# test asserts the same bytes Node would produce.

G. Unresolved assumptions you could not verify in this round.

PASS criterion for this round: B has zero NOT-REVIEWED rows (or each is
documented), D is "no banned API present" (Crypto.cs only) or N/A, E has zero
entries, F is "all fixtures match".

DO NOT use "looks good" or "no divergence found" without producing all sections.

2026-05-23T00:06:37.601630Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll use the `codex-review` skill for the review shape, then pin the upstream snapshot and walk `src/Crypto.cs` method-by-method against the TypeScript source. I’m also doing a quick memory lookup because this repo family has prior C# SDK review context.
exec
/bin/zsh -lc "sed -n '1,220p' /Users/takeshi/.agents/skills/codex-review/SKILL.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
---
name: codex-review
version: 2.1.4
author: BenedictKing
description: "Professional code review skill for Claude Code. Automatically collects file changes and task status. Triggers when working directory has uncommitted changes, or reviews latest commit when clean. Triggers: code review, review, 代码审核, 代码审查, 检查代码"
allowed-tools:
  - Task
  - Bash
  - Read
  - Glob
  - Write
  - Edit
user-invocable: true
---

# Codex Code Review Skill

## Trigger Conditions

Triggered when user input contains:

- "代码审核", "代码审查", "审查代码", "审核代码"
- "review", "code review", "review code", "codex 审核"
- "帮我审核", "检查代码", "审一下", "看看代码"

## Core Concept: Intention vs Implementation

Running `codex review --uncommitted` alone only shows AI "what was done (Implementation)".
Recording intention first tells AI "what you wanted to do (Intention)".

**"Code changes + intention description" as combined input is the most effective way to improve AI code review quality.**

## Skill Architecture

This skill operates in two phases:

1. **Preparation Phase** (current context): Check working directory, update CHANGELOG
2. **Review Phase** (isolated context): Invoke Task tool to execute Lint + codex review (using context: fork to reduce context waste)

## Execution Steps

### 0. [First] Check Working Directory Status

```bash
git diff --name-only && git status --short
```

**Decide review mode based on output:**

- **Has uncommitted changes** → Continue with steps 1-4 (normal flow)
- **Clean working directory** → Directly invoke codex-runner: `codex review --commit HEAD`

### 1. [Mandatory] Check if CHANGELOG is Updated

**Before any review, must check if CHANGELOG.md contains description of current changes.**

```bash
# Check if CHANGELOG.md is in uncommitted changes
git diff --name-only | grep -E "(CHANGELOG|changelog)"
```

**If CHANGELOG is not updated, you must automatically perform the following (don't ask user to do it manually):**

1. **Analyze changes**: Run `git diff --stat` and `git diff` to get complete changes
2. **Auto-generate CHANGELOG entry**: Generate compliant entry based on code changes
3. **Write to CHANGELOG.md**: Use Edit tool to insert entry at top of `[Unreleased]` section
4. **Continue review flow**: Immediately proceed to next steps after CHANGELOG update

**Auto-generated CHANGELOG entry format:**

```markdown
## [Unreleased]

### Added / Changed / Fixed

- Feature description: what problem was solved or what functionality was implemented
- Affected files: main modified files/modules
```

**Example - Auto-generation Flow:**

```
1. Detected CHANGELOG not updated
2. Run git diff --stat, found handlers/responses.go modified (+88 lines)
3. Run git diff to analyze details: added CompactHandler function
4. Auto-generate entry:
   ### Added
   - Added `/v1/responses/compact` endpoint for conversation context compression
   - Supports multi-channel failover and request body size limits
5. Use Edit tool to write to CHANGELOG.md
6. Continue with lint and codex review
```

### 2. [Critical] Stage All New Files

**Before invoking codex review, must add all new files (untracked files) to git staging area, otherwise codex will report P1 error.**

```bash
# Check for new files
git status --short | grep "^??"
```

**If there are new files, automatically execute:**

```bash
# Safely stage all new files (handles empty list and special filenames)
git ls-files --others --exclude-standard -z | while IFS= read -r -d '' f; do git add -- "$f"; done
```

**Explanation:**

- `-z` uses null character to separate filenames, correctly handles filenames with spaces/newlines
- `while IFS= read -r -d ''` reads filenames one by one
- `git add -- "$f"` uses `--` separator, correctly handles filenames starting with `-`
- When no new files exist, loop body doesn't execute, safely skipped
- This won't stage modified files, only handles new files
- codex needs files to be tracked by git for proper review

### 3. Evaluate Task Difficulty and Invoke codex-runner

**Count change scale:**

```bash
# Count number of changed files and lines of code
git diff --stat | tail -1
```

**Difficulty Assessment Criteria:**

**Difficult Tasks** (meets any condition):

- Modified files ≥ 10
- Total code changes (insertions + deletions) ≥ 500 lines
- Single metric: insertions ≥ 300 lines OR deletions ≥ 300 lines
- Involves core architecture/algorithm changes
- Cross-module refactoring
- Config: `model_reasoning_effort=xhigh`, timeout 30 minutes

**Normal Tasks** (other cases):

- Config: `model_reasoning_effort=high`, timeout 10 minutes

**Evaluation Method:**

You MUST parse the `git diff --stat` output correctly to determine difficulty:

```bash
# Get the summary line (last line of git diff --stat)
git diff --stat | tail -1
# Example outputs:
# "20 files changed, 342 insertions(+), 985 deletions(-)"
# "1 file changed, 50 insertions(+)"  # No deletions
# "3 files changed, 120 deletions(-)"  # No insertions
```

**Parsing Rules:**
1. Extract file count from "X file(s) changed" (handle both "1 file" and "N files")
2. Extract insertions from "Y insertion(s)(+)" if present (handle both "1 insertion" and "N insertions"), otherwise 0
3. Extract deletions from "Z deletion(s)(-)" if present (handle both "1 deletion" and "N deletions"), otherwise 0
4. Calculate total changes = insertions + deletions

**Important Edge Cases:**
- Single file: `"1 file changed"` (singular form)
- No insertions: Git omits `"insertions(+)"` entirely → treat as 0
- No deletions: Git omits `"deletions(-)"` entirely → treat as 0
- Pure rename: May show `"0 insertions(+), 0 deletions(-)"` or omit both

**Decision Logic (ANY condition triggers xhigh):**
- IF file_count >= 10 → xhigh
- IF total_changes >= 500 → xhigh
- IF insertions >= 300 → xhigh
- IF deletions >= 300 → xhigh
- ELSE → high

**Example Cases:**
- ✅ "20 files changed, 342 insertions(+), 985 deletions(-)" → xhigh (files=20≥10, total=1327≥500, deletions=985≥300)
- ✅ "5 files changed, 600 insertions(+), 50 deletions(-)" → xhigh (total=650≥500, insertions=600≥300)
- ✅ "12 files changed, 100 insertions(+), 50 deletions(-)" → xhigh (files=12≥10)
- ✅ "1 file changed, 400 deletions(-)" → xhigh (deletions=400≥300)
- ❌ "3 files changed, 150 insertions(+), 80 deletions(-)" → high (all conditions fail)
- ❌ "1 file changed, 50 insertions(+)" → high (no deletions, total=50<500)

**Invoke codex-runner Subtask:**

Use Task tool to invoke codex-runner, passing complete command (including Lint + codex review):

```
Task parameters:
- subagent_type: Bash
- description: "Execute Lint and codex review"
- timeout: 1800000 (30 minutes for difficult tasks) or 600000 (10 minutes for normal tasks)
- prompt: Choose corresponding command based on project type and difficulty

Go project - Difficult task:
  go fmt ./... && go vet ./... && codex review --uncommitted --config model_reasoning_effort=xhigh
  (timeout: 1800000)

Go project - Normal task:
  go fmt ./... && go vet ./... && codex review --uncommitted --config model_reasoning_effort=high
  (timeout: 600000)

Node project - Difficult task:
  npm run lint:fix && codex review --uncommitted --config model_reasoning_effort=xhigh
  (timeout: 1800000)

Node project - Normal task:
  npm run lint:fix && codex review --uncommitted --config model_reasoning_effort=high
  (timeout: 600000)

Python project - Difficult task:
  black . && ruff check --fix . && codex review --uncommitted --config model_reasoning_effort=xhigh
  (timeout: 1800000)

Python project - Normal task:
  black . && ruff check --fix . && codex review --uncommitted --config model_reasoning_effort=high
  (timeout: 600000)

Clean working directory:
  codex review --commit HEAD --config model_reasoning_effort=high
  (timeout: 600000)

exec
/bin/zsh -lc 'rg -n "turnkey|C# SDK|Crypto.cs|D17|73e510|SDK" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
273:# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
275:scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.

exec
/bin/zsh -lc "sed -n '268,286p' /Users/takeshi/.codex/memories/MEMORY.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:

## Failures and how to do differently

- Symptom: the checklist is already stale when presented. Cause: the user had handled some PRs and the assistant reused the first query result. Fix: refresh the GitHub queries immediately before answering when the user says they already acted on items [Task 1]

# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review

scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.
applies_to: cwd=/Users/takeshi/.codex/worktrees/1605/peak; reuse_rule=reuse for future `peak` plan/PR review work when the target is PR #330 or a similar docs-plan review in the Peak repo; treat plan conclusions as PR-specific if the file or branch has materially changed

## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted

### rollout_summary_files

- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
- extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md, updated_at=2026-05-22T04:47:00+00:00, thread_id=None, chronicle synthetic task for the initial PR-review window)
- extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md, updated_at=2026-05-22T06:07:00+00:00, thread_id=None, chronicle synthetic task showing the posted inline review state)

### keywords

codex
The prior memory only reinforces that this should be treated as a strict review artifact, not a general summary. I’m now locating the C# methods, upstream TypeScript functions, tarball hash, and all fixture tests that touch `Crypto.cs`.
exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// 1:1 logical port of @turnkey/crypto@2.8.8
     2	//
     3	// Upstream snapshot:
     4	//   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
     5	//
     6	// Files covered:
     7	//   ts-source/constants.ts            -> Crypto.Constants nested class
     8	//   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
     9	//   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
    10	//                                        HpkeEncrypt / HpkeDecrypt /
    11	//                                        BuildAdditionalAssociatedData /
    12	//                                        CompressRawPublicKey /
    13	//                                        UncompressRawPublicKey /
    14	//                                        FormatHpkeBuf, plus the internal
    15	//                                        helpers BuildLabeledIkm /
    16	//                                        BuildLabeledInfo / ExtractAndExpand
    17	//   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
    18	//                                        (port of @noble/hashes/hkdf)
    19	//   ts-source/turnkey.ts (subset)     -> Curve enum / DecryptCredentialBundle /
    20	//                                        EncryptPrivateKeyToBundle /
    21	//                                        DecryptExportBundle /
    22	//                                        VerifySessionJwtSignature
    23	//
    24	// Out of scope (matches the peak Unity port):
    25	//   hpkeAuthEncrypt, quorumKeyEncrypt, extractPrivateKeyFromPKCS8Bytes,
    26	//   fromDerSignature, toDerSignature
    27	//   verifyStampSignature, encryptWalletToBundle, encryptToEnclave,
    28	//   encryptOauth2ClientSecret, encryptOnRampSecret
    29	//   proof.ts (AWS Nitro attestation chain)
    30	//
    31	// Adaptations:
    32	//   - System.Text.Json source generation (TurnkeyJsonContext) replaces
    33	//     Newtonsoft.Json's JObject.Parse / JsonConvert.SerializeObject used by
    34	//     the peak Unity port. Wire bytes are unchanged.
    35	//   - BouncyCastle 2.5.0 wraps ECDSA / ECDH / AES-GCM / SHA-256 / HMAC /
    36	//     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
    37	//     Tonelli-Shanks, and bundle parsing logic are direct line-by-line
    38	//     ports of the upstream TypeScript.
    39	//   - Newtonsoft.Json dependency dropped.
    40	//
    41	// 2.8.8 vs 2.8.9 note:
    42	//   The only diff between @turnkey/crypto@2.8.8 and @turnkey/crypto@2.8.9 in
    43	//   the published dist/ is the inlining of QOS_ENCRYPTION_HMAC_MESSAGE
    44	//   (2.8.9 hard-codes the bytes; 2.8.8 uses new TextEncoder().encode("...")).
    45	//   They produce identical wire bytes. This port targets 2.8.8 (peak's pin)
    46	//   but is logically equivalent to 2.8.9 as well.
    47	
    48	using System;
    49	using System.Security.Cryptography;
    50	using System.Text.Json;
    51	using Org.BouncyCastle.Asn1;
    52	using Org.BouncyCastle.Asn1.X9;
    53	using Org.BouncyCastle.Crypto.Agreement;
    54	using Org.BouncyCastle.Crypto.Engines;
    55	using Org.BouncyCastle.Crypto.Generators;
    56	using Org.BouncyCastle.Crypto.Modes;
    57	using Org.BouncyCastle.Crypto.Parameters;
    58	using Org.BouncyCastle.Math;
    59	using Org.BouncyCastle.Security;
    60	
    61	namespace Turnkey
    62	{
    63	    /// <summary>
    64	    /// Cryptographic primitives and Turnkey bundle helpers. 1:1 logical port
    65	    /// of <c>@turnkey/crypto</c> at peak's pinned version 2.8.8.
    66	    /// </summary>
    67	    public static class Crypto
    68	    {
    69	        #region Constants
    70	
    71	        /// <summary>
    72	        /// Constants from upstream <c>constants.ts</c> used by this port.
    73	        /// Bytes match the upstream <c>Uint8Array(...)</c> literals verbatim.
    74	        /// </summary>
    75	        public static class Constants
    76	        {
    77	            // HPKE KEM/HPKE suite IDs.
    78	            public static readonly byte[] SUITE_ID_1 = new byte[] { 75, 69, 77, 0, 16 }; // "KEM\0\x10"
    79	            public static readonly byte[] SUITE_ID_2 = new byte[] { 72, 80, 75, 69, 0, 16, 0, 1, 0, 2 }; // "HPKE\0\x10\0\x01\0\x02"
    80	            public static readonly byte[] HPKE_VERSION = new byte[] { 72, 80, 75, 69, 45, 118, 49 }; // "HPKE-v1"
    81	
    82	            // HPKE labels.
    83	            public static readonly byte[] LABEL_SECRET = new byte[] { 115, 101, 99, 114, 101, 116 }; // "secret"
    84	            public static readonly byte[] LABEL_EAE_PRK = new byte[] { 101, 97, 101, 95, 112, 114, 107 }; // "eae_prk"
    85	            public static readonly byte[] LABEL_SHARED_SECRET = new byte[]
    86	            {
    87	                115, 104, 97, 114, 101, 100, 95, 115, 101, 99, 114, 101, 116, // "shared_secret"
    88	            };
    89	
    90	            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
    91	            public static readonly byte[] AES_KEY_INFO = new byte[]
    92	            {
    93	                0, 32, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 107,
    94	                101, 121, 0, 143, 195, 174, 184, 50, 73, 10, 75, 90, 179, 228, 32, 35, 40,
    95	                125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34, 135, 39, 183, 10, 64, 33,
    96	                18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53, 41, 180, 122, 198, 166, 48,
    97	                185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151, 113, 201, 158, 80,
    98	            };
    99	
   100	            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
   101	            public static readonly byte[] IV_INFO = new byte[]
   102	            {
   103	                0, 12, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 98, 97,
   104	                115, 101, 95, 110, 111, 110, 99, 101, 0, 143, 195, 174, 184, 50, 73, 10, 75,
   105	                90, 179, 228, 32, 35, 40, 125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34,
   106	                135, 39, 183, 10, 64, 33, 18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53,
   107	                41, 180, 122, 198, 166, 48, 185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151,
   108	                113, 201, 158, 80,
   109	            };
   110	
   111	            /// <summary>SEC1 uncompressed P-256 public key length, bytes (0x04 + X + Y).</summary>
   112	            public const int UNCOMPRESSED_PUB_KEY_LENGTH_BYTES = 65;
   113	
   114	            /// <summary>Production signer used by Turnkey to sign export/import bundles.</summary>
   115	            public const string PRODUCTION_SIGNER_SIGN_PUBLIC_KEY =
   116	                "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569";
   117	
   118	            /// <summary>Production notarizer used by Turnkey to sign session JWTs.</summary>
   119	            public const string PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY =
   120	                "04d498aa87ac3bf982ac2b5dd9604d0074905cfbda5d62727c5a237b895e6749205e9f7cd566909c4387f6ca25c308445c60884b788560b785f4a96ac33702a469";
   121	        }
   122	
   123	        #endregion
   124	
   125	        #region Curve enum (turnkey.ts type)
   126	
   127	        /// <summary>
   128	        /// EC curve identifier. Mirrors the upstream union type
   129	        /// <c>"CURVE_P256" | "CURVE_SECP256K1"</c> exposed by
   130	        /// <see cref="UncompressRawPublicKey(byte[], Curve)"/>.
   131	        /// </summary>
   132	        public enum Curve
   133	        {
   134	            P256,
   135	            Secp256k1,
   136	        }
   137	
   138	        #endregion
   139	
   140	        #region Math (math.ts)
   141	
   142	        /// <summary>
   143	        /// Mathematical helpers ported from upstream <c>math.ts</c>.
   144	        /// </summary>
   145	        public static class Math
   146	        {
   147	            /// <summary>
   148	            /// Modular square root via Tonelli-Shanks. Equivalent to
   149	            /// upstream <c>math.ts modSqrt</c>.
   150	            /// </summary>
   151	            /// <param name="x">Value to take the square root of (must be non-negative; EC field-element semantics).</param>
   152	            /// <param name="p">Prime modulus.</param>
   153	            /// <returns>One square root of <paramref name="x"/> modulo <paramref name="p"/>.</returns>
   154	            /// <remarks>
   155	            /// All known call sites in <c>@turnkey/crypto</c> pass a non-negative
   156	            /// <paramref name="x"/> derived from an EC field coordinate, where
   157	            /// BouncyCastle <c>BigInteger.Mod</c> and JS <c>BigInt %</c> agree.
   158	            /// For symmetry with the JS surface, the result for a negative
   159	            /// <paramref name="x"/> may differ; this is not exercised in
   160	            /// production code paths.
   161	            /// </remarks>
   162	            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
   163	            {
   164	                if (p.CompareTo(BigInteger.Zero) <= 0)
   165	                {
   166	                    throw new ArgumentException("p must be positive");
   167	                }
   168	                var baseVal = x.Mod(p);
   169	
   170	                // p % 4 == 3 fast path (applies to NIST P-256 / P-384 / P-521).
   171	                if (p.TestBit(0) && p.TestBit(1))
   172	                {
   173	                    var q = p.Add(BigInteger.One).ShiftRight(2);
   174	                    var squareRoot = baseVal.ModPow(q, p);
   175	
   176	                    if (!squareRoot.Multiply(squareRoot).Mod(p).Equals(baseVal))
   177	                    {
   178	                        throw new InvalidOperationException("could not find a modular square root");
   179	                    }
   180	                    return squareRoot;
   181	                }
   182	                throw new InvalidOperationException("unsupported modulus value");
   183	            }
   184	        }
   185	
   186	        #endregion
   187	
   188	        #region HKDF (port of @noble/hashes/hkdf)
   189	
   190	        /// <summary>
   191	        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
   192	        /// which upstream <c>crypto.ts</c> imports.
   193	        /// </summary>
   194	        public static class Hkdf
   195	        {
   196	            private const int HashLen = 32; // SHA-256 output length, bytes.
   197	
   198	            /// <summary>
   199	            /// HKDF Extract. RFC 5869 §2.2.
   200	            /// </summary>
   201	            public static byte[] Extract(byte[] salt, byte[] ikm)
   202	            {
   203	                if (salt == null || salt.Length == 0)
   204	                {
   205	                    salt = new byte[HashLen]; // RFC 5869: salt defaults to HashLen zero bytes.
   206	                }
   207	                using (var hmac = new HMACSHA256(salt))
   208	                {
   209	                    return hmac.ComputeHash(ikm ?? Array.Empty<byte>());
   210	                }
   211	            }
   212	
   213	            /// <summary>
   214	            /// HKDF Expand. RFC 5869 §2.3.
   215	            /// </summary>
   216	            public static byte[] Expand(byte[] prk, byte[] info, int length)
   217	            {
   218	                if (prk == null || prk.Length < HashLen)
   219	                {
   220	                    throw new ArgumentException("PRK must be at least HashLen bytes");
   221	                }
   222	                if (length > 255 * HashLen)
   223	                {
   224	                    throw new ArgumentException(
   225	                        "Output length cannot exceed 255 * HashLen (" + (255 * HashLen) + " bytes)");
   226	                }
   227	                if (info == null)
   228	                {
   229	                    info = Array.Empty<byte>();
   230	                }
   231	
   232	                int n = (length + HashLen - 1) / HashLen; // ceil(length / HashLen)
   233	                var okm = new byte[n * HashLen];
   234	                var tPrev = Array.Empty<byte>();
   235	
   236	                using (var hmac = new HMACSHA256(prk))
   237	                {
   238	                    for (int i = 1; i <= n; i++)
   239	                    {
   240	                        var input = new byte[tPrev.Length + info.Length + 1];
   241	                        Array.Copy(tPrev, 0, input, 0, tPrev.Length);
   242	                        Array.Copy(info, 0, input, tPrev.Length, info.Length);
   243	                        input[input.Length - 1] = (byte)i;
   244	
   245	                        var t = hmac.ComputeHash(input);
   246	                        Array.Copy(t, 0, okm, (i - 1) * HashLen, HashLen);
   247	                        tPrev = t;
   248	                    }
   249	                }
   250	
   251	                var result = new byte[length];
   252	                Array.Copy(okm, 0, result, 0, length);
   253	                return result;
   254	            }
   255	        }
   256	
   257	        #endregion
   258	
   259	        #region Nested DTOs
   260	

exec
/bin/zsh -lc 'find codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8 -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/LICENSE
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/index.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/math.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt | sed -n '1,160p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc 'find tests/Fixtures -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/encoding/turnkey-encoding-vectors.json

 succeeded in 0ms:
     1	962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
     2	75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
     3	2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
     4	d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz

exec
/bin/zsh -lc "rg -n \"Crypto|P256|secp|Turnkey\\.Crypto|encrypt|decrypt|sign|hmac|sha|fixture|Fixtures\" tests src -S" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
src/CryptoConstants.cs:32:    public static class CryptoConstants
src/CryptoConstants.cs:35:        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
src/CryptoConstants.cs:58:        public const string P256_P =
src/CryptoConstants.cs:66:        public const string P256_B =
src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
tests/CryptoTests.cs:1:// CryptoTests.cs — xunit tests for src/Crypto.cs.
tests/CryptoTests.cs:10:// Tests that require a Turnkey-signed bundle (DecryptCredentialBundle,
tests/CryptoTests.cs:12:// are not unit-testable without a real Turnkey signing key. They are
tests/CryptoTests.cs:24:    public class CryptoTests
tests/CryptoTests.cs:36:            var root = Crypto.Math.ModSqrt(x, p);
tests/CryptoTests.cs:47:            Action act = () => Crypto.Math.ModSqrt(x, p);
tests/CryptoTests.cs:57:            Action act = () => Crypto.Math.ModSqrt(x, p);
tests/CryptoTests.cs:63:        public void ModSqrt_P256Prime_Works()
tests/CryptoTests.cs:66:            var p = new BigInteger(CryptoConstants.P256_P);
tests/CryptoTests.cs:68:            var root = Crypto.Math.ModSqrt(x, p);
tests/CryptoTests.cs:85:            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
tests/CryptoTests.cs:86:            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
tests/CryptoTests.cs:120:            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
tests/CryptoTests.cs:121:            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
tests/CryptoTests.cs:143:            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
tests/CryptoTests.cs:144:            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
tests/CryptoTests.cs:159:            byte[] okm = Crypto.Hkdf.Expand(prk, null!, 0);
tests/CryptoTests.cs:167:            Action act = () => Crypto.Hkdf.Expand(prk, null!, 255 * 32 + 1);
tests/CryptoTests.cs:175:            byte[] prkA = Crypto.Hkdf.Extract(null!, ikm);
tests/CryptoTests.cs:176:            byte[] prkB = Crypto.Hkdf.Extract(new byte[32], ikm); // RFC default
tests/CryptoTests.cs:189:                var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:191:                byte[] compressed = Crypto.CompressRawPublicKey(uncompressed);
tests/CryptoTests.cs:195:                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:203:            // Upstream behavior: slice + LSB-flip regardless of input shape.
tests/CryptoTests.cs:211:            Action act = () => Crypto.CompressRawPublicKey(raw);
tests/CryptoTests.cs:220:            Action act = () => Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:230:            Action act = () => Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:236:        // GetPublicKey / GenerateP256KeyPair
tests/CryptoTests.cs:240:        public void GenerateP256KeyPair_LengthsAreCorrect()
tests/CryptoTests.cs:242:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:253:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:255:            byte[] pub = Crypto.GetPublicKey(priv); // isCompressed default true
tests/CryptoTests.cs:263:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:265:            byte[] uncompressed = Crypto.GetPublicKey(priv, isCompressed: false);
tests/CryptoTests.cs:272:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:273:            byte[] viaBytes = Crypto.GetPublicKey(Encoding.Uint8ArrayFromHexString(kp.PrivateKey));
tests/CryptoTests.cs:274:            byte[] viaHex = Crypto.GetPublicKey(kp.PrivateKey);
tests/CryptoTests.cs:286:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:292:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:298:            // encrypted = compressed(senderPub) || ciphertext (33 + N bytes)
tests/CryptoTests.cs:299:            encrypted.Length.Should().BeGreaterThan(33);
tests/CryptoTests.cs:301:            // Split, uncompress the sender's compressed point, then decrypt.
tests/CryptoTests.cs:303:            Array.Copy(encrypted, 0, compressedSender, 0, 33);
tests/CryptoTests.cs:304:            var ciphertext = new byte[encrypted.Length - 33];
tests/CryptoTests.cs:305:            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
tests/CryptoTests.cs:307:            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);
tests/CryptoTests.cs:309:            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
tests/CryptoTests.cs:316:            decrypted.Should().Equal(payload);
tests/CryptoTests.cs:322:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:325:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:332:            Array.Copy(encrypted, 0, compressedSender, 0, 33);
tests/CryptoTests.cs:333:            var ciphertext = new byte[encrypted.Length - 33];
tests/CryptoTests.cs:334:            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
tests/CryptoTests.cs:336:            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
tests/CryptoTests.cs:339:                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
tests/CryptoTests.cs:342:            decrypted.Should().BeEmpty();
tests/CryptoTests.cs:348:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:349:            var attacker = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:352:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:359:            Array.Copy(encrypted, 0, compressedSender, 0, 33);
tests/CryptoTests.cs:360:            var ciphertext = new byte[encrypted.Length - 33];
tests/CryptoTests.cs:361:            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
tests/CryptoTests.cs:363:            Action act = () => Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
tests/CryptoTests.cs:366:                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
tests/CryptoTests.cs:381:            byte[] aad = Crypto.BuildAdditionalAssociatedData(a, b);
tests/CryptoTests.cs:389:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:392:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:398:            string json = Crypto.FormatHpkeBuf(encrypted);
tests/CryptoTests.cs:412:            Action act = () => Crypto.FormatHpkeBuf(new byte[10]);
tests/CryptoTests.cs:424:            // Upstream: throws "invalid JWT: need 3 parts" when signature part is missing.
tests/CryptoTests.cs:425:            Action act = () => Crypto.VerifySessionJwtSignature(string.Empty);
tests/CryptoTests.cs:433:            Action act = () => Crypto.VerifySessionJwtSignature("just.two");
tests/CryptoTests.cs:441:            // 3 parts but the signature decodes to a non-64-byte buffer.
tests/CryptoTests.cs:443:            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
tests/CryptoTests.cs:449:            // 64-byte signature of all zeros — should fail verification.
tests/CryptoTests.cs:457:            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
tests/CryptoTests.cs:467:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
tests/CryptoTests.cs:480:        // Turnkey-signed sample bundles which we do not have here)
tests/CryptoTests.cs:490:            Action act = () => Crypto.DecryptCredentialBundle(tiny, new string('1', 64));
tests/CryptoTests.cs:492:               .WithMessage("*Error decrypting bundle*");
tests/CryptoTests.cs:501:            Action act = () => Crypto.DecryptCredentialBundle(rawBs58, new string('1', 64));
tests/CryptoTests.cs:508:            Action act = () => Crypto.EncryptPrivateKeyToBundle(new Crypto.EncryptPrivateKeyToBundleParams());
tests/CryptoTests.cs:515:            Action act = () => Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams());
src/Crypto.cs:7://   ts-source/constants.ts            -> Crypto.Constants nested class
src/Crypto.cs:8://   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
src/Crypto.cs:9://   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:27://   verifyStampSignature, encryptWalletToBundle, encryptToEnclave,
src/Crypto.cs:28://   encryptOauth2ClientSecret, encryptOnRampSecret
src/Crypto.cs:49:using System.Security.Cryptography;
src/Crypto.cs:53:using Org.BouncyCastle.Crypto.Agreement;
src/Crypto.cs:54:using Org.BouncyCastle.Crypto.Engines;
src/Crypto.cs:55:using Org.BouncyCastle.Crypto.Generators;
src/Crypto.cs:56:using Org.BouncyCastle.Crypto.Modes;
src/Crypto.cs:57:using Org.BouncyCastle.Crypto.Parameters;
src/Crypto.cs:64:    /// Cryptographic primitives and Turnkey bundle helpers. 1:1 logical port
src/Crypto.cs:67:    public static class Crypto
src/Crypto.cs:87:                115, 104, 97, 114, 101, 100, 95, 115, 101, 99, 114, 101, 116, // "shared_secret"
src/Crypto.cs:114:            /// <summary>Production signer used by Turnkey to sign export/import bundles.</summary>
src/Crypto.cs:118:            /// <summary>Production notarizer used by Turnkey to sign session JWTs.</summary>
src/Crypto.cs:129:        /// <c>"CURVE_P256" | "CURVE_SECP256K1"</c> exposed by
src/Crypto.cs:134:            P256,
src/Crypto.cs:207:                using (var hmac = new HMACSHA256(salt))
src/Crypto.cs:209:                    return hmac.ComputeHash(ikm ?? Array.Empty<byte>());
src/Crypto.cs:236:                using (var hmac = new HMACSHA256(prk))
src/Crypto.cs:245:                        var t = hmac.ComputeHash(input);
src/Crypto.cs:261:        /// <summary>HPKE decrypt parameters; equivalent to upstream object literal.</summary>
src/Crypto.cs:269:        /// <summary>HPKE encrypt parameters; equivalent to upstream object literal.</summary>
src/Crypto.cs:276:        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
src/Crypto.cs:286:        /// upstream <c>encryptPrivateKeyToBundle</c> options object.
src/Crypto.cs:297:            /// Override the production signer key for testing only. Equivalent
src/Crypto.cs:305:        /// upstream <c>decryptExportBundle</c> options object.
src/Crypto.cs:316:            /// Override the production signer key for testing only. Equivalent
src/Crypto.cs:323:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:371:        /// <c>crypto.ts generateP256KeyPair</c>. The private key is sampled as
src/Crypto.cs:375:        public static KeyPair GenerateP256KeyPair()
src/Crypto.cs:395:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:399:        /// Wraps any underlying decryption failure with the upstream
src/Crypto.cs:423:                var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);
src/Crypto.cs:427:                var key = ExtractAndExpand(sharedSecret, ikm, info, 32);
src/Crypto.cs:430:                var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);
src/Crypto.cs:442:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:463:                var ephemeralKeyPair = GenerateP256KeyPair();
src/Crypto.cs:473:                var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);
src/Crypto.cs:477:                var key = ExtractAndExpand(sharedSecret, ikm, info, 32);
src/Crypto.cs:480:                var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);
src/Crypto.cs:482:                var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
src/Crypto.cs:484:                return Encoding.ConcatUint8Arrays(compressedSenderBuf, encryptedData);
src/Crypto.cs:532:        /// <param name="curve">Curve identifier; defaults to <see cref="Curve.P256"/>.</param>
src/Crypto.cs:533:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Curve curve = Curve.P256)
src/Crypto.cs:553:            if (curve == Curve.P256)
src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:602:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:604:            if (encryptedBuf == null) throw new ArgumentNullException(nameof(encryptedBuf));
src/Crypto.cs:605:            // Upstream: encappedPublic := encryptedBuf.slice(0,33);
src/Crypto.cs:606:            //           ciphertext     := encryptedBuf.slice(33);
src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
src/Crypto.cs:613:            int compressedTake = System.Math.Min(compressedSize, encryptedBuf.Length);
src/Crypto.cs:615:            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, compressedTake);
src/Crypto.cs:619:            int cipherLen = System.Math.Max(0, encryptedBuf.Length - compressedSize);
src/Crypto.cs:623:                Array.Copy(encryptedBuf, compressedSize, ciphertext, 0, cipherLen);
src/Crypto.cs:640:        /// Equivalent to upstream <c>turnkey.ts decryptCredentialBundle</c>.
src/Crypto.cs:643:        /// Wraps any decryption / parse failure with the upstream message
src/Crypto.cs:644:        /// <c>"Error decrypting bundle: ..."</c>.
src/Crypto.cs:646:        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string embeddedKey)
src/Crypto.cs:648:            if (encryptedCredentialBundle == null)
src/Crypto.cs:649:                throw new ArgumentNullException(nameof(encryptedCredentialBundle));
src/Crypto.cs:655:                var bundleBytes = Encoding.Base58CheckDecode(encryptedCredentialBundle);
src/Crypto.cs:660:                        + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
src/Crypto.cs:669:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:675:                return Encoding.Uint8ArrayToHexString(decryptedData);
src/Crypto.cs:680:                    "\"Error decrypting bundle:\", " + error.Message, error);
src/Crypto.cs:686:        /// Equivalent to upstream <c>turnkey.ts encryptPrivateKeyToBundle</c>.
src/Crypto.cs:708:            string? signedDataHex = GetStringOrNull(parsedImportBundle, "data");
src/Crypto.cs:713:                signedDataHex,
src/Crypto.cs:716:            var signedDataBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
src/Crypto.cs:717:            using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
src/Crypto.cs:718:            var signedData = signedDoc.RootElement;
src/Crypto.cs:720:            string? orgId = GetStringOrNull(signedData, "organizationId");
src/Crypto.cs:728:            string? userId = GetStringOrNull(signedData, "userId");
src/Crypto.cs:736:            string? targetPublic = GetStringOrNull(signedData, "targetPublic");
src/Crypto.cs:739:                throw new InvalidOperationException("missing \"targetPublic\" in bundle signed data");
src/Crypto.cs:753:        /// Equivalent to upstream <c>turnkey.ts decryptExportBundle</c>.
src/Crypto.cs:780:                var signedDataBytes = Encoding.Uint8ArrayFromHexString(dataHex!);
src/Crypto.cs:781:                using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
src/Crypto.cs:782:                var signedData = signedDoc.RootElement;
src/Crypto.cs:784:                string? bundleOrgId = GetStringOrNull(signedData, "organizationId");
src/Crypto.cs:793:                string? encappedPublic = GetStringOrNull(signedData, "encappedPublic");
src/Crypto.cs:796:                    throw new InvalidOperationException("missing \"encappedPublic\" in bundle signed data");
src/Crypto.cs:798:                string? ciphertextStr = GetStringOrNull(signedData, "ciphertext");
src/Crypto.cs:803:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:813:                    if (decryptedData.Length != 32)
src/Crypto.cs:816:                            "invalid private key length. Expected 32 bytes. Got " + decryptedData.Length + ".");
src/Crypto.cs:819:                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
src/Crypto.cs:827:                    Array.Copy(decryptedData, 0, concatenated, 0, 32);
src/Crypto.cs:832:                var decryptedHex = Encoding.Uint8ArrayToHexString(decryptedData);
src/Crypto.cs:833:                return parameters.ReturnMnemonic ? Encoding.HexToAscii(decryptedHex) : decryptedHex;
src/Crypto.cs:838:                    "Error decrypting bundle: " + error.Message, error);
src/Crypto.cs:843:        /// Verify the ECDSA signature on a Turnkey session JWT.
src/Crypto.cs:852:        /// <returns><c>true</c> if the signature is valid for the given key.</returns>
src/Crypto.cs:872:            string signatureB64 = parts[2];
src/Crypto.cs:873:            string signingInput = headerB64 + "." + payloadB64;
src/Crypto.cs:876:            using (var sha256 = SHA256.Create())
src/Crypto.cs:878:                var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
src/Crypto.cs:879:                msgDigest = sha256.ComputeHash(h1);
src/Crypto.cs:882:            byte[] signature = Base64UrlDecode(signatureB64);
src/Crypto.cs:884:            return VerifyP256RawSignature(publicKey, signature, msgDigest);
src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:905:            var sharedSecretBig = agreement.CalculateAgreement(publicKeyParams);
src/Crypto.cs:906:            var ss = sharedSecretBig.ToByteArrayUnsigned();
src/Crypto.cs:945:        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
src/Crypto.cs:947:            var prk = Hkdf.Extract(sharedSecret, ikm);
src/Crypto.cs:951:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:956:            var output = new byte[cipher.GetOutputSize(encryptedData.Length)];
src/Crypto.cs:957:            int len = cipher.ProcessBytes(encryptedData, 0, encryptedData.Length, output, 0);
src/Crypto.cs:984:        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
src/Crypto.cs:987:            // else is an invalid signature; return false.
src/Crypto.cs:988:            if (signatureRaw.Length != 64)
src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1000:                var r = new BigInteger(1, signatureRaw, 0, 32);
src/Crypto.cs:1001:                var s = new BigInteger(1, signatureRaw, 32, 32);
src/Crypto.cs:1004:                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
src/Crypto.cs:1005:                signer.Init(false, publicKeyParams);
src/Crypto.cs:1006:                signer.BlockUpdate(messageDigest, 0, messageDigest.Length);
src/Crypto.cs:1007:                return signer.VerifySignature(derSignature);
src/Crypto.cs:1021:            string? signatureHex,
src/Crypto.cs:1022:            string? signedDataHex,
src/Crypto.cs:1031:                    "expected signer key " + expectedSignerPublicKey
src/Crypto.cs:1032:                    + " does not match signer key from bundle: " + enclaveQuorumPublic);
src/Crypto.cs:1035:            if (string.IsNullOrEmpty(signatureHex) || string.IsNullOrEmpty(signedDataHex))
src/Crypto.cs:1038:                    "failed to verify enclave signature: missing signature or data");
src/Crypto.cs:1042:            var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex!);
src/Crypto.cs:1043:            var messageBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1050:            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
src/Crypto.cs:1051:            signer.Init(false, publicKeyParams);
src/Crypto.cs:1052:            signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
src/Crypto.cs:1054:            if (!signer.VerifySignature(signatureBytes))
src/Crypto.cs:1056:                throw new InvalidOperationException("failed to verify enclave signature");
tests/Fixtures/README.md:1:# Test fixtures
tests/Fixtures/README.md:3:Each subdirectory groups fixtures for one production file, plus a
tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
tests/Fixtures/README.md:21:Fixtures/
tests/Fixtures/README.md:24:├── crypto/                                  — (added with Crypto.cs port)
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
tests/Fixtures/README.md:34:into the matching fixture file and update its sha256 in
src/turnkey-sdk-csharp.csproj:13:    <PackageProjectUrl>https://github.com/KyuzanInc/turnkey-sdk-csharp</PackageProjectUrl>
src/turnkey-sdk-csharp.csproj:16:    <RepositoryUrl>https://github.com/KyuzanInc/turnkey-sdk-csharp</RepositoryUrl>
src/turnkey-sdk-csharp.csproj:23:    <PackageReference Include="BouncyCastle.Cryptography" />
src/TurnkeyJsonContext.cs:27:    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
tests/Fixtures/encoding/turnkey-encoding-vectors.json:6:    "tarball_sha256": "2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:19:      "input": "{\"publicKey\":\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\",\"signature\":\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\",\"scheme\":\"SIGNATURE_SCHEME_TK_API_P256\"}",
src/Encoding.cs:32:// Unity-specific helpers kept for Crypto.cs consumption (also present in the
src/Encoding.cs:41:using System.Security.Cryptography;
src/Encoding.cs:190:        /// an optional sign, then parses the leading hex digits. Returns 0 for
src/Encoding.cs:203:        /// The sign is preserved per JS semantics; for HexToAscii output the
src/Encoding.cs:219:            // Optional sign.
src/Encoding.cs:220:            int sign = 1;
src/Encoding.cs:225:                    sign = -1;
src/Encoding.cs:245:            return parsedDigits == 0 ? 0 : sign * value;
src/Encoding.cs:614:                : decoded.ToByteArrayUnsigned();
src/Encoding.cs:640:            using (var sha = SHA256.Create())
src/Encoding.cs:642:                var h1 = sha.ComputeHash(payload);
src/Encoding.cs:643:                var h2 = sha.ComputeHash(h1);
src/Encoding.cs:671:            using (var sha = SHA256.Create())
src/Encoding.cs:673:                var h1 = sha.ComputeHash(data);
src/Encoding.cs:674:                var h2 = sha.ComputeHash(h1);
src/Encoding.cs:690:        // Crypto.cs / ApiKeyStamper.cs / Http.cs use them.
tests/turnkey-sdk-csharp.Tests.csproj:11:    <ProjectReference Include="..\src\turnkey-sdk-csharp.csproj" />
tests/turnkey-sdk-csharp.Tests.csproj:24:    <None Include="Fixtures\**\*">
src/packages.lock.json:5:      "BouncyCastle.Cryptography": {
src/packages.lock.json:75:      "BouncyCastle.Cryptography": {
tests/packages.lock.json:99:          "System.Security.Cryptography.ProtectedData": "4.4.0"
tests/packages.lock.json:107:      "System.Security.Cryptography.ProtectedData": {
tests/packages.lock.json:155:          "BouncyCastle.Cryptography": "[2.5.0, 2.5.0]",
tests/packages.lock.json:159:      "BouncyCastle.Cryptography": {

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '260,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   260	
   261	        /// <summary>HPKE decrypt parameters; equivalent to upstream object literal.</summary>
   262	        public class HpkeDecryptParams
   263	        {
   264	            public byte[]? CiphertextBuf { get; set; }
   265	            public byte[]? EncappedKeyBuf { get; set; }
   266	            public string? ReceiverPriv { get; set; }
   267	        }
   268	
   269	        /// <summary>HPKE encrypt parameters; equivalent to upstream object literal.</summary>
   270	        public class HpkeEncryptParams
   271	        {
   272	            public byte[]? PlainTextBuf { get; set; }
   273	            public byte[]? TargetKeyBuf { get; set; }
   274	        }
   275	
   276	        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
   277	        public class KeyPair
   278	        {
   279	            public string PrivateKey { get; set; } = string.Empty;
   280	            public string PublicKey { get; set; } = string.Empty;
   281	            public string PublicKeyUncompressed { get; set; } = string.Empty;
   282	        }
   283	
   284	        /// <summary>
   285	        /// Parameters for <see cref="EncryptPrivateKeyToBundle"/>. Mirrors the
   286	        /// upstream <c>encryptPrivateKeyToBundle</c> options object.
   287	        /// </summary>
   288	        public class EncryptPrivateKeyToBundleParams
   289	        {
   290	            public string? PrivateKey { get; set; }
   291	            public string? ImportBundle { get; set; }
   292	            public string? OrganizationId { get; set; }
   293	            public string? UserId { get; set; }
   294	            public string? KeyFormat { get; set; }
   295	
   296	            /// <summary>
   297	            /// Override the production signer key for testing only. Equivalent
   298	            /// to upstream <c>dangerouslyOverrideSignerPublicKey</c>.
   299	            /// </summary>
   300	            public string? DangerouslyOverrideSignerPublicKey { get; set; }
   301	        }
   302	
   303	        /// <summary>
   304	        /// Parameters for <see cref="DecryptExportBundle"/>. Mirrors the
   305	        /// upstream <c>decryptExportBundle</c> options object.
   306	        /// </summary>
   307	        public class DecryptExportBundleParams
   308	        {
   309	            public string? ExportBundle { get; set; }
   310	            public string? EmbeddedKey { get; set; }
   311	            public string? OrganizationId { get; set; }
   312	            public bool ReturnMnemonic { get; set; }
   313	            public string? KeyFormat { get; set; }
   314	
   315	            /// <summary>
   316	            /// Override the production signer key for testing only. Equivalent
   317	            /// to upstream <c>dangerouslyOverrideSignerPublicKey</c>.
   318	            /// </summary>
   319	            public string? DangerouslyOverrideSignerPublicKey { get; set; }
   320	        }
   321	
   322	        /// <summary>
   323	        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
   324	        /// match the upstream <c>JSON.stringify({ encappedPublic, ciphertext })</c>
   325	        /// output.
   326	        /// </summary>
   327	        public class HpkeBundlePayload
   328	        {
   329	            [System.Text.Json.Serialization.JsonPropertyName("encappedPublic")]
   330	            public string EncappedPublic { get; set; } = string.Empty;
   331	
   332	            [System.Text.Json.Serialization.JsonPropertyName("ciphertext")]
   333	            public string Ciphertext { get; set; } = string.Empty;
   334	        }
   335	
   336	        #endregion
   337	
   338	        #region crypto.ts public surface
   339	
   340	        /// <summary>
   341	        /// Derive the SEC1 public key bytes from a private key.
   342	        /// Equivalent to upstream <c>crypto.ts getPublicKey</c>.
   343	        /// </summary>
   344	        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
   345	        {
   346	            if (privateKey == null)
   347	            {
   348	                throw new ArgumentNullException(nameof(privateKey));
   349	            }
   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   351	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   352	
   353	            var d = new BigInteger(1, privateKey);
   354	            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
   355	            var publicKeyParams = new ECPublicKeyParameters(
   356	                privateKeyParams.Parameters.G.Multiply(d), domainParams);
   357	
   358	            return publicKeyParams.Q.GetEncoded(isCompressed);
   359	        }
   360	
   361	        /// <summary>
   362	        /// Hex-string overload of <see cref="GetPublicKey(byte[], bool)"/>.
   363	        /// </summary>
   364	        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
   365	        {
   366	            return GetPublicKey(Encoding.Uint8ArrayFromHexString(privateKeyHex), isCompressed);
   367	        }
   368	
   369	        /// <summary>
   370	        /// Generate a random P-256 key pair. Equivalent to upstream
   371	        /// <c>crypto.ts generateP256KeyPair</c>. The private key is sampled as
   372	        /// 32 random bytes (then validated by <c>GetPublicKey</c>); identical
   373	        /// to the upstream <c>randomBytes(32)</c> + <c>getPublicKey</c> flow.
   374	        /// </summary>
   375	        public static KeyPair GenerateP256KeyPair()
   376	        {
   377	            // Match upstream: privateKey = randomBytes(32). The valid scalar
   378	            // range check is left to GetPublicKey() / EC math (same as upstream).
   379	            var random = new SecureRandom();
   380	            var privateKey = new byte[32];
   381	            random.NextBytes(privateKey);
   382	
   383	            var publicKeyCompressed = GetPublicKey(privateKey, isCompressed: true);
   384	            var publicKeyUncompressed = GetPublicKey(privateKey, isCompressed: false);
   385	
   386	            return new KeyPair
   387	            {
   388	                PrivateKey = Encoding.Uint8ArrayToHexString(privateKey),
   389	                PublicKey = Encoding.Uint8ArrayToHexString(publicKeyCompressed),
   390	                PublicKeyUncompressed = Encoding.Uint8ArrayToHexString(publicKeyUncompressed),
   391	            };
   392	        }
   393	
   394	        /// <summary>
   395	        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
   396	        /// Equivalent to upstream <c>crypto.ts hpkeDecrypt</c>.
   397	        /// </summary>
   398	        /// <exception cref="InvalidOperationException">
   399	        /// Wraps any underlying decryption failure with the upstream
   400	        /// message <c>"Unable to perform hpkeDecrypt: ..."</c>.
   401	        /// </exception>
   402	        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
   403	        {
   404	            try
   405	            {
   406	                if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   407	                var ciphertextBuf = parameters.CiphertextBuf
   408	                                    ?? throw new ArgumentNullException(nameof(parameters.CiphertextBuf));
   409	                var encappedKeyBuf = parameters.EncappedKeyBuf
   410	                                     ?? throw new ArgumentNullException(nameof(parameters.EncappedKeyBuf));
   411	                var receiverPriv = parameters.ReceiverPriv
   412	                                   ?? throw new ArgumentNullException(nameof(parameters.ReceiverPriv));
   413	
   414	                var receiverPrivBytes = Encoding.Uint8ArrayFromHexString(receiverPriv);
   415	                var receiverPubBuf = GetPublicKey(receiverPrivBytes, false);
   416	
   417	                var aad = BuildAdditionalAssociatedData(encappedKeyBuf, receiverPubBuf);
   418	                var ss = DeriveSS(encappedKeyBuf, receiverPriv);
   419	                var kemContext = GetKemContext(encappedKeyBuf, Encoding.Uint8ArrayToHexString(receiverPubBuf));
   420	
   421	                var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
   422	                var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
   423	                var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);
   424	
   425	                ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
   426	                info = Constants.AES_KEY_INFO;
   427	                var key = ExtractAndExpand(sharedSecret, ikm, info, 32);
   428	
   429	                info = Constants.IV_INFO;
   430	                var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);
   431	
   432	                return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
   433	            }
   434	            catch (Exception error)
   435	            {
   436	                throw new InvalidOperationException(
   437	                    "Unable to perform hpkeDecrypt: " + error.Message + " ", error);
   438	            }
   439	        }
   440	
   441	        /// <summary>
   442	        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
   443	        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
   444	        /// </summary>
   445	        /// <returns>
   446	        /// The byte concatenation
   447	        /// <c>compressed(senderPub) || ciphertext</c>, ready to feed to
   448	        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
   449	        /// </returns>
   450	        /// <exception cref="InvalidOperationException">
   451	        /// Wraps any underlying failure with the upstream message
   452	        /// <c>"Unable to perform hpkeEncrypt: ..."</c>.
   453	        /// </exception>
   454	        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
   455	        {
   456	            try
   457	            {
   458	                if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   459	                var plainTextBuf = parameters.PlainTextBuf ?? Array.Empty<byte>();
   460	                var targetKeyBuf = parameters.TargetKeyBuf
   461	                                   ?? throw new ArgumentNullException(nameof(parameters.TargetKeyBuf));
   462	
   463	                var ephemeralKeyPair = GenerateP256KeyPair();
   464	                var senderPrivBuf = Encoding.Uint8ArrayFromHexString(ephemeralKeyPair.PrivateKey);
   465	                var senderPubBuf = Encoding.Uint8ArrayFromHexString(ephemeralKeyPair.PublicKeyUncompressed);
   466	
   467	                var aad = BuildAdditionalAssociatedData(senderPubBuf, targetKeyBuf);
   468	                var ss = DeriveSS(targetKeyBuf, Encoding.Uint8ArrayToHexString(senderPrivBuf));
   469	                var kemContext = GetKemContext(senderPubBuf, Encoding.Uint8ArrayToHexString(targetKeyBuf));
   470	
   471	                var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
   472	                var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
   473	                var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);
   474	
   475	                ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
   476	                info = Constants.AES_KEY_INFO;
   477	                var key = ExtractAndExpand(sharedSecret, ikm, info, 32);
   478	
   479	                info = Constants.IV_INFO;
   480	                var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);
   481	
   482	                var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
   483	                var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
   484	                return Encoding.ConcatUint8Arrays(compressedSenderBuf, encryptedData);
   485	            }
   486	            catch (Exception error)
   487	            {
   488	                throw new InvalidOperationException(
   489	                    "Unable to perform hpkeEncrypt: " + error.Message, error);
   490	            }
   491	        }
   492	
   493	        /// <summary>
   494	        /// HPKE additional associated data builder.
   495	        /// Equivalent to upstream <c>crypto.ts buildAdditionalAssociatedData</c>.
   496	        /// </summary>
   497	        public static byte[] BuildAdditionalAssociatedData(byte[] senderPubBuf, byte[] receiverPubBuf)
   498	        {
   499	            return Encoding.ConcatUint8Arrays(senderPubBuf, receiverPubBuf);
   500	        }
   501	
   502	        /// <summary>
   503	        /// Compress an uncompressed P-256 public key into 33 bytes.
   504	        /// Equivalent to upstream <c>crypto.ts compressRawPublicKey</c>.
   505	        /// </summary>
   506	        /// <remarks>
   507	        /// Upstream uses <c>slice(0, (1 + len) >>> 1)</c> then mutates the
   508	        /// prefix byte based on the last byte's LSB; for a valid 65-byte
   509	        /// SEC1 uncompressed key the result is the same 33-byte
   510	        /// <c>(0x02|0x03) || X</c> output. Defensive validation here only
   511	        /// rejects malformed input that upstream would also subsequently fail
   512	        /// on downstream.
   513	        /// </remarks>
   514	        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
   515	        {
   516	            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
   517	            int len = rawPublicKey.Length;
   518	            // Upstream: var compressedBytes = rawPublicKey.slice(0, (1 + len) >>> 1);
   519	            int half = (1 + len) >> 1;
   520	            var compressedBytes = new byte[half];
   521	            Array.Copy(rawPublicKey, 0, compressedBytes, 0, half);
   522	            // Upstream: compressedBytes[0] = 0x02 | (rawPublicKey[len - 1]! & 0x01);
   523	            compressedBytes[0] = (byte)(0x02 | (rawPublicKey[len - 1] & 0x01));
   524	            return compressedBytes;
   525	        }
   526	
   527	        /// <summary>
   528	        /// Uncompress a 33-byte compressed public key into 65 bytes.
   529	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
   530	        /// </summary>
   531	        /// <param name="rawPublicKey">33-byte compressed key starting with 0x02 or 0x03.</param>
   532	        /// <param name="curve">Curve identifier; defaults to <see cref="Curve.P256"/>.</param>
   533	        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Curve curve = Curve.P256)
   534	        {
   535	            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
   536	            if (rawPublicKey.Length != 33)
   537	            {
   538	                throw new ArgumentException("failed to uncompress raw public key: invalid length");
   539	            }
   540	            if (rawPublicKey[0] != 0x02 && rawPublicKey[0] != 0x03)
   541	            {
   542	                throw new ArgumentException("failed to uncompress raw public key: invalid prefix");
   543	            }
   544	
   545	            bool lsb = rawPublicKey[0] == 0x03;
   546	
   547	            // x = BigInt("0x" + uint8ArrayToHexString(rawPublicKey.subarray(1)));
   548	            var xBytes = new byte[32];
   549	            Array.Copy(rawPublicKey, 1, xBytes, 0, 32);
   550	            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);
   551	
   552	            BigInteger p, a, b;
   553	            if (curve == Curve.P256)
   554	            {
   555	                p = new BigInteger(CryptoConstants.P256_P);
   556	                b = new BigInteger(CryptoConstants.P256_B, 16);
   557	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
   558	            }
   559	            else // Secp256k1
   560	            {
   561	                p = new BigInteger(
   562	                    "fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f", 16);
   563	                a = BigInteger.ValueOf(0);
   564	                b = BigInteger.ValueOf(7);
   565	            }
   566	
   567	            // rhs = ((x * x + a) * x + b) % p
   568	            var x2 = x.Multiply(x).Mod(p);
   569	            var x2PlusA = x2.Add(a).Mod(p);
   570	            var rhs = x2PlusA.Multiply(x).Add(b).Mod(p);
   571	
   572	            var y = Math.ModSqrt(rhs, p);
   573	            if (lsb != y.TestBit(0))
   574	            {
   575	                y = p.Subtract(y).Mod(p);
   576	            }
   577	
   578	            // Defensive range check matches upstream "throw" intent for out-of-range output.
   579	            if (x.SignValue < 0 || x.CompareTo(p) >= 0)
   580	            {
   581	                throw new InvalidOperationException("x is out of range");
   582	            }
   583	            if (y.SignValue < 0 || y.CompareTo(p) >= 0)
   584	            {
   585	                throw new InvalidOperationException("y is out of range");
   586	            }
   587	
   588	            var uncompressed = new byte[65];
   589	            uncompressed[0] = 0x04;
   590	            var xHexOut = x.ToString(16).ToLowerInvariant().PadLeft(64, '0');
   591	            var yHexOut = y.ToString(16).ToLowerInvariant().PadLeft(64, '0');
   592	            Array.Copy(Encoding.Uint8ArrayFromHexString(xHexOut), 0, uncompressed, 1, 32);
   593	            Array.Copy(Encoding.Uint8ArrayFromHexString(yHexOut), 0, uncompressed, 33, 32);
   594	            return uncompressed;
   595	        }
   596	
   597	        /// <summary>
   598	        /// Encode the HPKE result <c>compressed(senderPub) || ciphertext</c>
   599	        /// as the Turnkey import-bundle JSON envelope.
   600	        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
   601	        /// </summary>
   602	        public static string FormatHpkeBuf(byte[] encryptedBuf)
   603	        {
   604	            if (encryptedBuf == null) throw new ArgumentNullException(nameof(encryptedBuf));
   605	            // Upstream: encappedPublic := encryptedBuf.slice(0,33);
   606	            //           ciphertext     := encryptedBuf.slice(33);
   607	            // Upstream then uncompressRawPublicKey() the compressed part. If
   608	            // the input is shorter than 33 bytes that uncompress call would
   609	            // throw "invalid length". Mirror the same behavior: pass the
   610	            // raw slice through uncompressRawPublicKey which enforces 33-byte
   611	            // length itself.
   612	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
   613	            int compressedTake = System.Math.Min(compressedSize, encryptedBuf.Length);
   614	            var compressedEncappedPublic = new byte[compressedTake];
   615	            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, compressedTake);
   616	
   617	            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);
   618	
   619	            int cipherLen = System.Math.Max(0, encryptedBuf.Length - compressedSize);
   620	            var ciphertext = new byte[cipherLen];

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '620,1085p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   620	            var ciphertext = new byte[cipherLen];
   621	            if (cipherLen > 0)
   622	            {
   623	                Array.Copy(encryptedBuf, compressedSize, ciphertext, 0, cipherLen);
   624	            }
   625	
   626	            var payload = new HpkeBundlePayload
   627	            {
   628	                EncappedPublic = Encoding.Uint8ArrayToHexString(encappedPublicUncompressed),
   629	                Ciphertext = Encoding.Uint8ArrayToHexString(ciphertext),
   630	            };
   631	            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
   632	        }
   633	
   634	        #endregion
   635	
   636	        #region turnkey.ts public surface
   637	
   638	        /// <summary>
   639	        /// Decrypt a Turnkey credential bundle.
   640	        /// Equivalent to upstream <c>turnkey.ts decryptCredentialBundle</c>.
   641	        /// </summary>
   642	        /// <exception cref="InvalidOperationException">
   643	        /// Wraps any decryption / parse failure with the upstream message
   644	        /// <c>"Error decrypting bundle: ..."</c>.
   645	        /// </exception>
   646	        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string embeddedKey)
   647	        {
   648	            if (encryptedCredentialBundle == null)
   649	                throw new ArgumentNullException(nameof(encryptedCredentialBundle));
   650	            if (embeddedKey == null)
   651	                throw new ArgumentNullException(nameof(embeddedKey));
   652	            try
   653	            {
   654	                // Upstream uses bs58check.decode exclusively (NO raw bs58 fallback).
   655	                var bundleBytes = Encoding.Base58CheckDecode(encryptedCredentialBundle);
   656	                if (bundleBytes.Length <= 33)
   657	                {
   658	                    throw new InvalidOperationException(
   659	                        "Bundle size " + bundleBytes.Length
   660	                        + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
   661	                }
   662	
   663	                var compressedEncappedKeyBuf = new byte[33];
   664	                Array.Copy(bundleBytes, 0, compressedEncappedKeyBuf, 0, 33);
   665	                var ciphertextBuf = new byte[bundleBytes.Length - 33];
   666	                Array.Copy(bundleBytes, 33, ciphertextBuf, 0, ciphertextBuf.Length);
   667	
   668	                var encappedKeyBuf = UncompressRawPublicKey(compressedEncappedKeyBuf);
   669	                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
   670	                {
   671	                    CiphertextBuf = ciphertextBuf,
   672	                    EncappedKeyBuf = encappedKeyBuf,
   673	                    ReceiverPriv = embeddedKey,
   674	                });
   675	                return Encoding.Uint8ArrayToHexString(decryptedData);
   676	            }
   677	            catch (Exception error)
   678	            {
   679	                throw new InvalidOperationException(
   680	                    "\"Error decrypting bundle:\", " + error.Message, error);
   681	            }
   682	        }
   683	
   684	        /// <summary>
   685	        /// Encrypt a private key into a Turnkey import bundle.
   686	        /// Equivalent to upstream <c>turnkey.ts encryptPrivateKeyToBundle</c>.
   687	        /// </summary>
   688	        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
   689	        {
   690	            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   691	            if (string.IsNullOrWhiteSpace(parameters.PrivateKey))
   692	                throw new ArgumentException("Private key is required", nameof(parameters.PrivateKey));
   693	            if (string.IsNullOrWhiteSpace(parameters.ImportBundle))
   694	                throw new ArgumentException("Import bundle is required", nameof(parameters.ImportBundle));
   695	            if (string.IsNullOrWhiteSpace(parameters.OrganizationId))
   696	                throw new ArgumentException("Organization ID is required", nameof(parameters.OrganizationId));
   697	            if (string.IsNullOrWhiteSpace(parameters.UserId))
   698	                throw new ArgumentException("User ID is required", nameof(parameters.UserId));
   699	
   700	            using var bundleDoc = JsonDocument.Parse(parameters.ImportBundle!);
   701	            var parsedImportBundle = bundleDoc.RootElement;
   702	
   703	            // Upstream order: decodeKey FIRST, then verify, then check fields.
   704	            var plainTextBuf = DecodeKey(parameters.PrivateKey!, parameters.KeyFormat);
   705	
   706	            string? enclaveQuorumPublic = GetStringOrNull(parsedImportBundle, "enclaveQuorumPublic");
   707	            string? dataSignature = GetStringOrNull(parsedImportBundle, "dataSignature");
   708	            string? signedDataHex = GetStringOrNull(parsedImportBundle, "data");
   709	
   710	            VerifyEnclaveSignature(
   711	                enclaveQuorumPublic,
   712	                dataSignature,
   713	                signedDataHex,
   714	                parameters.DangerouslyOverrideSignerPublicKey);
   715	
   716	            var signedDataBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
   717	            using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
   718	            var signedData = signedDoc.RootElement;
   719	
   720	            string? orgId = GetStringOrNull(signedData, "organizationId");
   721	            if (string.IsNullOrEmpty(orgId) || !string.Equals(orgId, parameters.OrganizationId, StringComparison.Ordinal))
   722	            {
   723	                throw new InvalidOperationException(
   724	                    "organization id does not match expected value. Expected: " + parameters.OrganizationId
   725	                    + ". Found: " + orgId + ".");
   726	            }
   727	
   728	            string? userId = GetStringOrNull(signedData, "userId");
   729	            if (string.IsNullOrEmpty(userId) || !string.Equals(userId, parameters.UserId, StringComparison.Ordinal))
   730	            {
   731	                throw new InvalidOperationException(
   732	                    "user id does not match expected value. Expected: " + parameters.UserId
   733	                    + ". Found: " + userId + ".");
   734	            }
   735	
   736	            string? targetPublic = GetStringOrNull(signedData, "targetPublic");
   737	            if (string.IsNullOrEmpty(targetPublic))
   738	            {
   739	                throw new InvalidOperationException("missing \"targetPublic\" in bundle signed data");
   740	            }
   741	
   742	            var targetKeyBuf = Encoding.Uint8ArrayFromHexString(targetPublic!);
   743	            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
   744	            {
   745	                PlainTextBuf = plainTextBuf,
   746	                TargetKeyBuf = targetKeyBuf,
   747	            });
   748	            return FormatHpkeBuf(privateKeyBundle);
   749	        }
   750	
   751	        /// <summary>
   752	        /// Decrypt a Turnkey export bundle.
   753	        /// Equivalent to upstream <c>turnkey.ts decryptExportBundle</c>.
   754	        /// </summary>
   755	        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
   756	        {
   757	            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   758	            if (string.IsNullOrWhiteSpace(parameters.ExportBundle))
   759	                throw new ArgumentException("Export bundle is required", nameof(parameters.ExportBundle));
   760	            if (string.IsNullOrWhiteSpace(parameters.EmbeddedKey))
   761	                throw new ArgumentException("Embedded key is required", nameof(parameters.EmbeddedKey));
   762	            if (string.IsNullOrWhiteSpace(parameters.OrganizationId))
   763	                throw new ArgumentException("Organization ID is required", nameof(parameters.OrganizationId));
   764	
   765	            try
   766	            {
   767	                using var bundleDoc = JsonDocument.Parse(parameters.ExportBundle!);
   768	                var parsedExportBundle = bundleDoc.RootElement;
   769	
   770	                string? enclaveQuorumPublic = GetStringOrNull(parsedExportBundle, "enclaveQuorumPublic");
   771	                string? dataSignature = GetStringOrNull(parsedExportBundle, "dataSignature");
   772	                string? dataHex = GetStringOrNull(parsedExportBundle, "data");
   773	
   774	                VerifyEnclaveSignature(
   775	                    enclaveQuorumPublic,
   776	                    dataSignature,
   777	                    dataHex,
   778	                    parameters.DangerouslyOverrideSignerPublicKey);
   779	
   780	                var signedDataBytes = Encoding.Uint8ArrayFromHexString(dataHex!);
   781	                using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
   782	                var signedData = signedDoc.RootElement;
   783	
   784	                string? bundleOrgId = GetStringOrNull(signedData, "organizationId");
   785	                if (string.IsNullOrEmpty(bundleOrgId)
   786	                    || !string.Equals(bundleOrgId, parameters.OrganizationId, StringComparison.Ordinal))
   787	                {
   788	                    throw new InvalidOperationException(
   789	                        "organization id does not match expected value. Expected: " + parameters.OrganizationId
   790	                        + ". Found: " + bundleOrgId + ".");
   791	                }
   792	
   793	                string? encappedPublic = GetStringOrNull(signedData, "encappedPublic");
   794	                if (string.IsNullOrEmpty(encappedPublic))
   795	                {
   796	                    throw new InvalidOperationException("missing \"encappedPublic\" in bundle signed data");
   797	                }
   798	                string? ciphertextStr = GetStringOrNull(signedData, "ciphertext");
   799	
   800	                var encappedKeyBuf = Encoding.Uint8ArrayFromHexString(encappedPublic!);
   801	                var ciphertextBuf = Encoding.Uint8ArrayFromHexString(ciphertextStr!);
   802	
   803	                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
   804	                {
   805	                    CiphertextBuf = ciphertextBuf,
   806	                    EncappedKeyBuf = encappedKeyBuf,
   807	                    ReceiverPriv = parameters.EmbeddedKey,
   808	                });
   809	
   810	                if (string.Equals(parameters.KeyFormat, "SOLANA", StringComparison.Ordinal)
   811	                    && !parameters.ReturnMnemonic)
   812	                {
   813	                    if (decryptedData.Length != 32)
   814	                    {
   815	                        throw new InvalidOperationException(
   816	                            "invalid private key length. Expected 32 bytes. Got " + decryptedData.Length + ".");
   817	                    }
   818	                    // Derive Ed25519 public key from the 32-byte seed via BouncyCastle.
   819	                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
   820	                    var publicKeyBytes = ed25519PrivKey.GeneratePublicKey().GetEncoded();
   821	                    if (publicKeyBytes.Length != 32)
   822	                    {
   823	                        throw new InvalidOperationException(
   824	                            "invalid public key length. Expected 32 bytes. Got " + publicKeyBytes.Length + ".");
   825	                    }
   826	                    var concatenated = new byte[64];
   827	                    Array.Copy(decryptedData, 0, concatenated, 0, 32);
   828	                    Array.Copy(publicKeyBytes, 0, concatenated, 32, 32);
   829	                    return Encoding.Base58Encode(concatenated);
   830	                }
   831	
   832	                var decryptedHex = Encoding.Uint8ArrayToHexString(decryptedData);
   833	                return parameters.ReturnMnemonic ? Encoding.HexToAscii(decryptedHex) : decryptedHex;
   834	            }
   835	            catch (Exception error)
   836	            {
   837	                throw new InvalidOperationException(
   838	                    "Error decrypting bundle: " + error.Message, error);
   839	            }
   840	        }
   841	
   842	        /// <summary>
   843	        /// Verify the ECDSA signature on a Turnkey session JWT.
   844	        /// Equivalent to upstream <c>turnkey.ts verifySessionJwtSignature</c>.
   845	        /// </summary>
   846	        /// <param name="jwt">The JWT to verify.</param>
   847	        /// <param name="dangerouslyOverrideNotarizerPublicKey">
   848	        /// Optional hex-encoded uncompressed P-256 public key to verify
   849	        /// against. Use for tests only; defaults to the production
   850	        /// notarizer key.
   851	        /// </param>
   852	        /// <returns><c>true</c> if the signature is valid for the given key.</returns>
   853	        /// <exception cref="InvalidOperationException">Thrown when the JWT is malformed (missing parts).</exception>
   854	        public static bool VerifySessionJwtSignature(
   855	            string jwt,
   856	            string? dangerouslyOverrideNotarizerPublicKey = null)
   857	        {
   858	            if (jwt == null) throw new ArgumentNullException(nameof(jwt));
   859	
   860	            string notarizerKeyHex =
   861	                dangerouslyOverrideNotarizerPublicKey ?? Constants.PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY;
   862	
   863	            var parts = jwt.Split('.');
   864	            // Upstream: const [a, b, sig] = jwt.split("."); if (!sig) throw new Error(...)
   865	            if (parts.Length < 3 || string.IsNullOrEmpty(parts[2]))
   866	            {
   867	                throw new InvalidOperationException("invalid JWT: need 3 parts");
   868	            }
   869	
   870	            string headerB64 = parts[0];
   871	            string payloadB64 = parts[1];
   872	            string signatureB64 = parts[2];
   873	            string signingInput = headerB64 + "." + payloadB64;
   874	
   875	            byte[] msgDigest;
   876	            using (var sha256 = SHA256.Create())
   877	            {
   878	                var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
   879	                msgDigest = sha256.ComputeHash(h1);
   880	            }
   881	
   882	            byte[] signature = Base64UrlDecode(signatureB64);
   883	            var publicKey = Encoding.Uint8ArrayFromHexString(notarizerKeyHex);
   884	            return VerifyP256RawSignature(publicKey, signature, msgDigest);
   885	        }
   886	
   887	        #endregion
   888	
   889	        #region Private helpers
   890	
   891	        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
   892	        {
   893	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   894	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   895	
   896	            var privBytes = Encoding.Uint8ArrayFromHexString(privHex);
   897	            var d = new BigInteger(1, privBytes);
   898	            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
   899	
   900	            var point = curve.Curve.DecodePoint(encappedKeyBuf);
   901	            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
   902	
   903	            var agreement = new ECDHBasicAgreement();
   904	            agreement.Init(privateKeyParams);
   905	            var sharedSecretBig = agreement.CalculateAgreement(publicKeyParams);
   906	            var ss = sharedSecretBig.ToByteArrayUnsigned();
   907	
   908	            if (ss.Length < 32)
   909	            {
   910	                var padded = new byte[32];
   911	                Array.Copy(ss, 0, padded, 32 - ss.Length, ss.Length);
   912	                ss = padded;
   913	            }
   914	            return ss;
   915	        }
   916	
   917	        private static byte[] GetKemContext(byte[] encappedKeyBuf, string publicKey)
   918	        {
   919	            return Encoding.ConcatUint8Arrays(
   920	                encappedKeyBuf,
   921	                Encoding.Uint8ArrayFromHexString(publicKey));
   922	        }
   923	
   924	        private static byte[] BuildLabeledIkm(byte[] label, byte[] ikm, byte[] suiteId)
   925	        {
   926	            return Encoding.ConcatUint8Arrays(Constants.HPKE_VERSION, suiteId, label, ikm);
   927	        }
   928	
   929	        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
   930	        {
   931	            const int suiteIdStartIndex = 9;
   932	            var ret = new byte[suiteIdStartIndex + suiteId.Length + label.Length + info.Length];
   933	
   934	            ret[0] = 0;
   935	            ret[1] = (byte)len;
   936	
   937	            Array.Copy(Constants.HPKE_VERSION, 0, ret, 2, Constants.HPKE_VERSION.Length);
   938	            Array.Copy(suiteId, 0, ret, suiteIdStartIndex, suiteId.Length);
   939	            Array.Copy(label, 0, ret, suiteIdStartIndex + suiteId.Length, label.Length);
   940	            Array.Copy(info, 0, ret, suiteIdStartIndex + suiteId.Length + label.Length, info.Length);
   941	
   942	            return ret;
   943	        }
   944	
   945	        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
   946	        {
   947	            var prk = Hkdf.Extract(sharedSecret, ikm);
   948	            return Hkdf.Expand(prk, info, len);
   949	        }
   950	
   951	        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
   952	        {
   953	            var cipher = new GcmBlockCipher(new AesEngine());
   954	            cipher.Init(false, new AeadParameters(new KeyParameter(key), 128, iv, aad));
   955	
   956	            var output = new byte[cipher.GetOutputSize(encryptedData.Length)];
   957	            int len = cipher.ProcessBytes(encryptedData, 0, encryptedData.Length, output, 0);
   958	            cipher.DoFinal(output, len);
   959	            return output;
   960	        }
   961	
   962	        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
   963	        {
   964	            var cipher = new GcmBlockCipher(new AesEngine());
   965	            cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, iv, aad));
   966	
   967	            var output = new byte[cipher.GetOutputSize(plainData.Length)];
   968	            int len = cipher.ProcessBytes(plainData, 0, plainData.Length, output, 0);
   969	            cipher.DoFinal(output, len);
   970	            return output;
   971	        }
   972	
   973	        private static byte[] Base64UrlDecode(string input)
   974	        {
   975	            var output = input.Replace('-', '+').Replace('_', '/');
   976	            switch (output.Length % 4)
   977	            {
   978	                case 2: output += "=="; break;
   979	                case 3: output += "="; break;
   980	            }
   981	            return Convert.FromBase64String(output);
   982	        }
   983	
   984	        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
   985	        {
   986	            // Upstream noble p256.verify accepts r||s of length 64. Anything
   987	            // else is an invalid signature; return false.
   988	            if (signatureRaw.Length != 64)
   989	            {
   990	                return false;
   991	            }
   992	            try
   993	            {
   994	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   995	                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   996	
   997	                var point = curve.Curve.DecodePoint(publicKeyBytes);
   998	                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
   999	
  1000	                var r = new BigInteger(1, signatureRaw, 0, 32);
  1001	                var s = new BigInteger(1, signatureRaw, 32, 32);
  1002	                byte[] derSignature = new DerSequence(new DerInteger(r), new DerInteger(s)).GetDerEncoded();
  1003	
  1004	                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
  1005	                signer.Init(false, publicKeyParams);
  1006	                signer.BlockUpdate(messageDigest, 0, messageDigest.Length);
  1007	                return signer.VerifySignature(derSignature);
  1008	            }
  1009	            catch
  1010	            {
  1011	                return false;
  1012	            }
  1013	        }
  1014	
  1015	        /// <summary>
  1016	        /// Equivalent to upstream <c>turnkey.ts verifyEnclaveSignature</c>.
  1017	        /// Throws on mismatch / failed verification.
  1018	        /// </summary>
  1019	        private static void VerifyEnclaveSignature(
  1020	            string? enclaveQuorumPublic,
  1021	            string? signatureHex,
  1022	            string? signedDataHex,
  1023	            string? dangerouslyOverrideSignerPublicKey)
  1024	        {
  1025	            string expectedSignerPublicKey =
  1026	                dangerouslyOverrideSignerPublicKey ?? Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY;
  1027	
  1028	            if (!string.Equals(enclaveQuorumPublic, expectedSignerPublicKey, StringComparison.Ordinal))
  1029	            {
  1030	                throw new InvalidOperationException(
  1031	                    "expected signer key " + expectedSignerPublicKey
  1032	                    + " does not match signer key from bundle: " + enclaveQuorumPublic);
  1033	            }
  1034	
  1035	            if (string.IsNullOrEmpty(signatureHex) || string.IsNullOrEmpty(signedDataHex))
  1036	            {
  1037	                throw new InvalidOperationException(
  1038	                    "failed to verify enclave signature: missing signature or data");
  1039	            }
  1040	
  1041	            var publicKeyBytes = Encoding.Uint8ArrayFromHexString(expectedSignerPublicKey);
  1042	            var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex!);
  1043	            var messageBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
  1044	
  1045	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
  1046	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
  1047	            var point = curve.Curve.DecodePoint(publicKeyBytes);
  1048	            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
  1049	
  1050	            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
  1051	            signer.Init(false, publicKeyParams);
  1052	            signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
  1053	
  1054	            if (!signer.VerifySignature(signatureBytes))
  1055	            {
  1056	                throw new InvalidOperationException("failed to verify enclave signature");
  1057	            }
  1058	        }
  1059	
  1060	        /// <summary>
  1061	        /// Equivalent to upstream <c>turnkey.ts decodeKey</c>. Default and
  1062	        /// unknown <paramref name="keyFormat"/> values fall back to hex
  1063	        /// parsing exactly like upstream <c>default:</c> branch.
  1064	        /// </summary>
  1065	        private static byte[] DecodeKey(string privateKey, string? keyFormat)
  1066	        {
  1067	            if (string.Equals(keyFormat, "SOLANA", StringComparison.Ordinal))
  1068	            {
  1069	                var decoded = Encoding.Base58Decode(privateKey);
  1070	                if (decoded.Length != 64)
  1071	                {
  1072	                    throw new InvalidOperationException(
  1073	                        "invalid key length. Expected 64 bytes. Got " + decoded.Length + ".");
  1074	                }
  1075	                var first32 = new byte[32];
  1076	                Array.Copy(decoded, 0, first32, 0, 32);
  1077	                return first32;
  1078	            }
  1079	            // HEXADECIMAL (and unknown fallback). Upstream "default:" branch
  1080	            // also accepts the unknown case by falling through to hex parsing
  1081	            // (it logs a warn; this port silently matches the behavior).
  1082	            string normalized = privateKey.StartsWith("0x", StringComparison.Ordinal)
  1083	                ? privateKey.Substring(2)
  1084	                : privateKey;
  1085	            return Encoding.Uint8ArrayFromHexString(normalized);

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '1,280p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '1,340p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts | sed -n '1,220p' && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/math.ts | sed -n '1,180p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	/// <reference lib="dom" />
     2	import { p256 } from "@noble/curves/p256";
     3	import * as hkdf from "@noble/hashes/hkdf";
     4	import { sha256 } from "@noble/hashes/sha256";
     5	import { gcm } from "@noble/ciphers/aes";
     6	import { randomBytes } from "@noble/hashes/utils";
     7	import * as borsh from "borsh";
     8	
     9	import {
    10	  uint8ArrayToHexString,
    11	  uint8ArrayFromHexString,
    12	  normalizePadding,
    13	} from "@turnkey/encoding";
    14	
    15	import { modSqrt, testBit } from "./math";
    16	import {
    17	  AES_KEY_INFO,
    18	  HPKE_VERSION,
    19	  IV_INFO,
    20	  LABEL_EAE_PRK,
    21	  LABEL_SECRET,
    22	  LABEL_SHARED_SECRET,
    23	  SUITE_ID_1,
    24	  SUITE_ID_2,
    25	  QOS_ENCRYPTION_HMAC_MESSAGE,
    26	  QUORUM_ENCRYPT_NONCE_LENGTH_BYTES,
    27	  UNCOMPRESSED_PUB_KEY_LENGTH_BYTES,
    28	} from "./constants";
    29	
    30	interface HpkeDecryptParams {
    31	  ciphertextBuf: Uint8Array;
    32	  encappedKeyBuf: Uint8Array;
    33	  receiverPriv: string;
    34	}
    35	
    36	interface HpkeEncryptParams {
    37	  plainTextBuf: Uint8Array;
    38	  targetKeyBuf: Uint8Array;
    39	}
    40	interface HpkeAuthEncryptParams {
    41	  plainTextBuf: Uint8Array;
    42	  targetKeyBuf: Uint8Array;
    43	  senderPriv: string;
    44	}
    45	
    46	interface KeyPair {
    47	  privateKey: string;
    48	  publicKey: string;
    49	  publicKeyUncompressed: string;
    50	}
    51	
    52	// Envelope for serializing an encrypted message with its context.
    53	type Envelope = {
    54	  nonce: Uint8Array;
    55	  ephemeralSenderPublic: Uint8Array;
    56	  encryptedMessage: Uint8Array;
    57	};
    58	
    59	// schema for borsh serialization
    60	const EnvelopeSchema = {
    61	  struct: {
    62	    nonce: { array: { type: "u8", len: QUORUM_ENCRYPT_NONCE_LENGTH_BYTES } },
    63	    ephemeralSenderPublic: {
    64	      array: { type: "u8", len: UNCOMPRESSED_PUB_KEY_LENGTH_BYTES },
    65	    },
    66	    encryptedMessage: { array: { type: "u8" } },
    67	  },
    68	};
    69	
    70	type Curve = "CURVE_SECP256K1" | "CURVE_P256";
    71	
    72	/**
    73	 * Get PublicKey function
    74	 * Derives public key from Uint8Array or hexstring private key
    75	 *
    76	 * @param {Uint8Array | string} privateKey - The Uint8Array or hexstring representation of a compressed private key.
    77	 * @param {boolean} isCompressed - Specifies whether to return a compressed or uncompressed public key. Defaults to true.
    78	 * @returns {Uint8Array} - The public key in Uin8Array representation.
    79	 */
    80	export const getPublicKey = (
    81	  privateKey: Uint8Array | string,
    82	  isCompressed: boolean = true,
    83	): Uint8Array => {
    84	  return p256.getPublicKey(privateKey, isCompressed);
    85	};
    86	
    87	/**
    88	 * HPKE Encrypt Function
    89	 * Encrypts data using Hybrid Public Key Encryption (HPKE) standard https://datatracker.ietf.org/doc/rfc9180/.
    90	 *
    91	 * @param {HpkeEncryptParams} params - The encryption parameters including plain text, encapsulated key, and sender private key.
    92	 * @returns {Uint8Array} - The encrypted data.
    93	 */
    94	
    95	export const hpkeEncrypt = ({
    96	  plainTextBuf,
    97	  targetKeyBuf,
    98	}: HpkeEncryptParams): Uint8Array => {
    99	  try {
   100	    // Standard HPKE Mode (Ephemeral Key Pair)
   101	    const ephemeralKeyPair = generateP256KeyPair();
   102	    const senderPrivBuf = uint8ArrayFromHexString(ephemeralKeyPair.privateKey);
   103	    const senderPubBuf = uint8ArrayFromHexString(
   104	      ephemeralKeyPair.publicKeyUncompressed,
   105	    );
   106	
   107	    const aad = buildAdditionalAssociatedData(senderPubBuf, targetKeyBuf);
   108	
   109	    // Step 1: Generate Shared Secret
   110	    const ss = deriveSS(targetKeyBuf, uint8ArrayToHexString(senderPrivBuf!));
   111	
   112	    // Step 2: Generate the KEM context
   113	    const kemContext = getKemContext(
   114	      senderPubBuf,
   115	      uint8ArrayToHexString(targetKeyBuf),
   116	    );
   117	
   118	    // Step 3: Build the HKDF inputs for key derivation
   119	    let ikm = buildLabeledIkm(LABEL_EAE_PRK, ss, SUITE_ID_1);
   120	    let info = buildLabeledInfo(
   121	      LABEL_SHARED_SECRET,
   122	      kemContext,
   123	      SUITE_ID_1,
   124	      32,
   125	    );
   126	    const sharedSecret = extractAndExpand(new Uint8Array([]), ikm, info, 32);
   127	
   128	    // Step 4: Derive the AES key
   129	    ikm = buildLabeledIkm(LABEL_SECRET, new Uint8Array([]), SUITE_ID_2);
   130	    info = AES_KEY_INFO;
   131	    const key = extractAndExpand(sharedSecret, ikm, info, 32);
   132	
   133	    // Step 5: Derive the initialization vector
   134	    info = IV_INFO;
   135	    const iv = extractAndExpand(sharedSecret, ikm, info, 12);
   136	
   137	    // Step 6: Encrypt the data using AES-GCM
   138	    const encryptedData = aesGcmEncrypt(plainTextBuf, key, iv, aad);
   139	
   140	    // Step 7: Concatenate the encapsulated key and the encrypted data for output
   141	    const compressedSenderBuf = compressRawPublicKey(senderPubBuf);
   142	    const result = new Uint8Array(
   143	      compressedSenderBuf.length + encryptedData.length,
   144	    );
   145	    result.set(compressedSenderBuf, 0);
   146	    result.set(encryptedData, compressedSenderBuf.length);
   147	
   148	    return result;
   149	  } catch (error) {
   150	    throw new Error(`Unable to perform hpkeEncrypt: ${error}`);
   151	  }
   152	};
   153	
   154	/**
   155	 * HPKE Encrypt Function
   156	 * Encrypts data using Authenticated ,Hybrid Public Key Encryption (HPKE) standard https://datatracker.ietf.org/doc/rfc9180/.
   157	 *
   158	 * @param {HpkeAuthEncryptParams} params - The encryption parameters including plain text, encapsulated key, and sender private key.
   159	 * @returns {Uint8Array} - The encrypted data.
   160	 */
   161	
   162	export const hpkeAuthEncrypt = ({
   163	  plainTextBuf,
   164	  targetKeyBuf,
   165	  senderPriv,
   166	}: HpkeAuthEncryptParams): Uint8Array => {
   167	  try {
   168	    // Authenticated HPKE Mode
   169	    const senderPrivBuf = uint8ArrayFromHexString(senderPriv);
   170	    const senderPubBuf = getPublicKey(senderPriv, false);
   171	
   172	    const aad = buildAdditionalAssociatedData(senderPubBuf, targetKeyBuf);
   173	
   174	    // Step 1: Generate Shared Secret
   175	    const ss = deriveSS(targetKeyBuf, uint8ArrayToHexString(senderPrivBuf!));
   176	
   177	    // Step 2: Generate the KEM context
   178	    const kemContext = getKemContext(
   179	      senderPubBuf,
   180	      uint8ArrayToHexString(targetKeyBuf),
   181	    );
   182	
   183	    // Step 3: Build the HKDF inputs for key derivation
   184	    let ikm = buildLabeledIkm(LABEL_EAE_PRK, ss, SUITE_ID_1);
   185	    let info = buildLabeledInfo(
   186	      LABEL_SHARED_SECRET,
   187	      kemContext,
   188	      SUITE_ID_1,
   189	      32,
   190	    );
   191	    const sharedSecret = extractAndExpand(new Uint8Array([]), ikm, info, 32);
   192	
   193	    // Step 4: Derive the AES key
   194	    ikm = buildLabeledIkm(LABEL_SECRET, new Uint8Array([]), SUITE_ID_2);
   195	    info = AES_KEY_INFO;
   196	    const key = extractAndExpand(sharedSecret, ikm, info, 32);
   197	
   198	    // Step 5: Derive the initialization vector
   199	    info = IV_INFO;
   200	    const iv = extractAndExpand(sharedSecret, ikm, info, 12);
   201	
   202	    // Step 6: Encrypt the data using AES-GCM
   203	    const encryptedData = aesGcmEncrypt(plainTextBuf, key, iv, aad);
   204	
   205	    // Step 7: Concatenate the encapsulated key and the encrypted data for output
   206	    const compressedSenderBuf = compressRawPublicKey(senderPubBuf);
   207	    const result = new Uint8Array(
   208	      compressedSenderBuf.length + encryptedData.length,
   209	    );
   210	    result.set(compressedSenderBuf, 0);
   211	    result.set(encryptedData, compressedSenderBuf.length);
   212	    return result;
   213	  } catch (error) {
   214	    throw new Error(`Unable to perform hpkeEncrypt: ${error}`);
   215	  }
   216	};
   217	
   218	/**
   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
   220	 * Returns a borsh serialized encrypted Envelope which is the nonce + ephemeralSenderPublicKey + encryptedMessage
   221	 * This function creates an ephemeral key, creates a shared secret with the recipient targetPublicKeyUncompressed
   222	 * creates additional associated data which follows the form: sender_public||sender_public_len||receiver_public||receiver_public_len
   223	 * encrypts using aes-gcm-256 with a SHA-512 HMAC over the QOS_ENCRYPTION_HMAC_MESSAGE literally: "qos_encryption_hmac_message"
   224	 * inserts and returns the necessary information in a borsh serialized envelope as described above
   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
   226	 *
   227	 * @param {Uint8Array} targetPublicKeyUncompressed - The P256 uncompressed public key to encrypt the message to
   228	 * @param {Uint8Array} message - The message to encrypt to targetPublicKeyUncompressed
   229	 * @returns {Uint8Array} - A borsh serialized envelope containing the nonce + ephemeralSenderPublicKey + encrypted message
   230	 */
   231	export const quorumKeyEncrypt = async (
   232	  targetPublicKeyUncompressed: Uint8Array,
   233	  message: Uint8Array,
   234	): Promise<Uint8Array> => {
   235	  // generate an ephemeral keypair for this encryption operation
   236	  const ephemeralKeyPair = generateP256KeyPair();
   237	  const ephemeralSenderPublic = ephemeralKeyPair.publicKeyUncompressed;
   238	
   239	  // create a shared secret AES-GCM key with the SHA-512 HMAC
   240	  let cipher = await createQuorumKeyEncryptCipher(
   241	    uint8ArrayFromHexString(ephemeralSenderPublic),
   242	    uint8ArrayFromHexString(ephemeralKeyPair.privateKey),
   243	    targetPublicKeyUncompressed,
   244	  );
   245	
   246	  // generate a nonce
   247	  const nonce = new Uint8Array(QUORUM_ENCRYPT_NONCE_LENGTH_BYTES);
   248	  crypto.getRandomValues(nonce);
   249	
   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
   251	  const aad = createAdditionalAssociatedData(
   252	    uint8ArrayFromHexString(ephemeralSenderPublic),
   253	    targetPublicKeyUncompressed,
   254	  );
   255	
   256	  // algorithm specifications for AES-GCM
   257	  const alg: AesGcmParams = {
   258	    name: "AES-GCM",
   259	    iv: nonce,
   260	    tagLength: 128,
   261	    additionalData: aad,
   262	  };
   263	
   264	  // encrypt the message with the shared secret
   265	  const encryptedMessageBuf = await crypto.subtle.encrypt(alg, cipher, message);
   266	
   267	  // create the envelope
   268	  let envelope: Envelope = {
   269	    nonce: nonce,
   270	    ephemeralSenderPublic: uint8ArrayFromHexString(ephemeralSenderPublic),
   271	    encryptedMessage: new Uint8Array(encryptedMessageBuf),
   272	  };
   273	
   274	  // borsh serialize the envelope
   275	  return borsh.serialize(EnvelopeSchema, envelope);
   276	};
   277	
   278	/**
   279	 * Format HPKE Buffer Function
   280	 * Returns a JSON string of an encrypted bundle, separating out the cipher text and the sender public key

 succeeded in 0ms:
     1	/// <reference lib="dom" />
     2	// Turnkey-specific cryptographic utilities
     3	import { bs58check } from "@turnkey/encoding";
     4	import { bs58 } from "@turnkey/encoding";
     5	import {
     6	  uint8ArrayToHexString,
     7	  uint8ArrayFromHexString,
     8	  hexToAscii,
     9	} from "@turnkey/encoding";
    10	
    11	import {
    12	  PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY,
    13	  PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY,
    14	  PRODUCTION_SIGNER_SIGN_PUBLIC_KEY,
    15	  PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY,
    16	} from "./constants";
    17	import {
    18	  formatHpkeBuf,
    19	  fromDerSignature,
    20	  hpkeDecrypt,
    21	  hpkeEncrypt,
    22	  quorumKeyEncrypt,
    23	  uncompressRawPublicKey,
    24	} from "./crypto";
    25	
    26	import { p256 } from "@noble/curves/p256";
    27	import { ed25519 } from "@noble/curves/ed25519";
    28	import type { ProjPointType } from "@noble/curves/abstract/weierstrass";
    29	import { sha256 } from "@noble/hashes/sha256";
    30	
    31	interface DecryptExportBundleParams {
    32	  exportBundle: string;
    33	  organizationId: string;
    34	  embeddedKey: string;
    35	  dangerouslyOverrideSignerPublicKey?: string; // Optional override for signer key
    36	  keyFormat?: "SOLANA" | "HEXADECIMAL";
    37	  returnMnemonic: boolean;
    38	}
    39	interface EncryptPrivateKeyToBundleParams {
    40	  privateKey: string;
    41	  keyFormat: string;
    42	  importBundle: string;
    43	  userId: string;
    44	  organizationId: string;
    45	  dangerouslyOverrideSignerPublicKey?: string; // Optional override for signer key
    46	}
    47	
    48	interface EncryptWalletToBundleParams {
    49	  mnemonic: string;
    50	  importBundle: string;
    51	  userId: string;
    52	  organizationId: string;
    53	  dangerouslyOverrideSignerPublicKey?: string; // Optional override for signer key
    54	}
    55	
    56	export enum Enclave {
    57	  NOTARIZER = "notarizer",
    58	  SIGNER = "signer",
    59	  EVM_PARSER = "evm-parser",
    60	  TLS_FETCHER = "tls-fetcher",
    61	  UMP = "ump",
    62	}
    63	
    64	/**
    65	 * Decrypt an encrypted email auth/recovery or oauth credential bundle.
    66	 *
    67	 * @param {string} credentialBundle - The encrypted credential bundle.
    68	 * @param {string} embeddedKey - The private key for decryption.
    69	 * @returns {string} - The decrypted data or null if decryption fails.
    70	 * @throws {Error} - If unable to decrypt the credential bundle
    71	 */
    72	export const decryptCredentialBundle = (
    73	  credentialBundle: string,
    74	  embeddedKey: string,
    75	): string => {
    76	  try {
    77	    const bundleBytes = bs58check.decode(credentialBundle);
    78	    if (bundleBytes.byteLength <= 33) {
    79	      throw new Error(
    80	        `Bundle size ${bundleBytes.byteLength} is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.`,
    81	      );
    82	    }
    83	
    84	    const compressedEncappedKeyBuf = bundleBytes.slice(0, 33);
    85	    const ciphertextBuf = bundleBytes.slice(33);
    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
    87	    const decryptedData = hpkeDecrypt({
    88	      ciphertextBuf,
    89	      encappedKeyBuf,
    90	      receiverPriv: embeddedKey,
    91	    });
    92	
    93	    return uint8ArrayToHexString(decryptedData);
    94	  } catch (error) {
    95	    throw new Error(`"Error decrypting bundle:", ${error}`);
    96	  }
    97	};
    98	
    99	/**
   100	 * Decrypt an encrypted export bundle (such as a private key or wallet account bundle).
   101	 *
   102	 * This function verifies the enclave signature to ensure the authenticity of the encrypted data.
   103	 * It uses HPKE (Hybrid Public Key Encryption) to decrypt the contents of the bundle and returns
   104	 * either the decrypted mnemonic or the decrypted data in hexadecimal format, based on the
   105	 * `returnMnemonic` flag.
   106	 *
   107	 * @param {DecryptExportBundleParams} params - An object containing the following properties:
   108	 *   - exportBundle {string}: The encrypted export bundle in JSON format.
   109	 *   - organizationId {string}: The expected organization ID to verify against the signed data.
   110	 *   - embeddedKey {string}: The private key used for decrypting the data.
   111	 *   - dangerouslyOverrideSignerPublicKey {string} [Optional]: Optionally override the default signer public key used for verifying the signature. This should only be done for testing
   112	 *   - returnMnemonic {boolean}: If true, returns the decrypted data as a mnemonic string; otherwise, returns it in hexadecimal format.
   113	 * @returns {Promise<string>} - A promise that resolves to the decrypted mnemonic or decrypted hexadecimal data.
   114	 * @throws {Error} - If decryption or signature verification fails, throws an error with details.
   115	 */
   116	export const decryptExportBundle = async ({
   117	  exportBundle,
   118	  embeddedKey,
   119	  organizationId,
   120	  dangerouslyOverrideSignerPublicKey,
   121	  keyFormat,
   122	  returnMnemonic,
   123	}: DecryptExportBundleParams): Promise<string> => {
   124	  try {
   125	    const parsedExportBundle = JSON.parse(exportBundle);
   126	    const verified = await verifyEnclaveSignature(
   127	      parsedExportBundle.enclaveQuorumPublic,
   128	      parsedExportBundle.dataSignature,
   129	      parsedExportBundle.data,
   130	      dangerouslyOverrideSignerPublicKey,
   131	    );
   132	    if (!verified) {
   133	      throw new Error(
   134	        `failed to verify enclave signature: ${parsedExportBundle}`,
   135	      );
   136	    }
   137	
   138	    const signedData = JSON.parse(
   139	      new TextDecoder().decode(
   140	        uint8ArrayFromHexString(parsedExportBundle.data),
   141	      ),
   142	    );
   143	
   144	    if (
   145	      !signedData.organizationId ||
   146	      signedData.organizationId !== organizationId
   147	    ) {
   148	      throw new Error(
   149	        `organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`,
   150	      );
   151	    }
   152	    if (!signedData.encappedPublic) {
   153	      throw new Error('missing "encappedPublic" in bundle signed data');
   154	    }
   155	
   156	    const encappedKeyBuf = uint8ArrayFromHexString(signedData.encappedPublic);
   157	    const ciphertextBuf = uint8ArrayFromHexString(signedData.ciphertext);
   158	    const decryptedData = hpkeDecrypt({
   159	      ciphertextBuf,
   160	      encappedKeyBuf,
   161	      receiverPriv: embeddedKey,
   162	    });
   163	
   164	    if (keyFormat === "SOLANA" && !returnMnemonic) {
   165	      if (decryptedData.length !== 32) {
   166	        throw new Error(
   167	          `invalid private key length. Expected 32 bytes. Got ${decryptedData.length}.`,
   168	        );
   169	      }
   170	      const publicKeyBytes = ed25519.getPublicKey(decryptedData);
   171	      if (publicKeyBytes.length !== 32) {
   172	        throw new Error(
   173	          `invalid public key length. Expected 32 bytes. Got ${publicKeyBytes.length}.`,
   174	        );
   175	      }
   176	      const concatenatedBytes = new Uint8Array(64);
   177	      concatenatedBytes.set(decryptedData, 0);
   178	      concatenatedBytes.set(publicKeyBytes, 32);
   179	      return bs58.encode(concatenatedBytes);
   180	    }
   181	
   182	    const decryptedDataHex = uint8ArrayToHexString(decryptedData);
   183	    return returnMnemonic ? hexToAscii(decryptedDataHex) : decryptedDataHex;
   184	  } catch (error) {
   185	    throw new Error(`Error decrypting bundle: ${error}`);
   186	  }
   187	};
   188	
   189	/**
   190	 * Verifies a signature from a Turnkey stamp using ECDSA and SHA-256.
   191	 *
   192	 * @param {string} publicKey - The public key of the authenticator (e.g. WebAuthn or P256 API key).
   193	 * @param {string} signature - The ECDSA signature in DER format.
   194	 * @param {string} signedData - The data that was signed (e.g. JSON-stringified Turnkey request body).
   195	 * @returns {Promise<boolean>} - Returns true if the signature is valid, otherwise throws an error.
   196	 *
   197	 * @example
   198	 *
   199	 * const stampedRequest = await turnkeyClient.stampGetWhoami(...);
   200	 * const decodedStampContents = atob(stampedRequest.stamp.stampHeaderValue);
   201	 * const parsedStampContents = JSON.parse(decodedStampContents);
   202	 * const signature = parsedStampContents.signature;
   203	 *
   204	 * await verifyStampSignature(publicKey, signature, stampedRequest.body)
   205	 */
   206	export const verifyStampSignature = async (
   207	  publicKey: string,
   208	  signature: string,
   209	  signedData: string,
   210	): Promise<boolean> => {
   211	  const publicKeyBuffer = uint8ArrayFromHexString(publicKey);
   212	  const loadedPublicKey = loadPublicKey(publicKeyBuffer);
   213	  if (!loadedPublicKey) {
   214	    throw new Error("failed to load public key");
   215	  }
   216	
   217	  // Convert the ASN.1 DER-encoded signature for verification
   218	  const publicSignatureBuf = fromDerSignature(signature);
   219	  const signedDataBuf = new TextEncoder().encode(signedData);
   220	  const hashedData = sha256(signedDataBuf);
   221	
   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
   223	};
   224	
   225	/**
   226	 * Verifies a signature from a Turnkey enclave using ECDSA and SHA-256.
   227	 *
   228	 * @param {string} enclaveQuorumPublic - The public key of the enclave signer.
   229	 * @param {string} publicSignature - The ECDSA signature in DER format.
   230	 * @param {string} signedData - The data that was signed.
   231	 * @param {Environment} dangerouslyOverrideSignerPublicKey - (optional) an enum (PROD or PREPROD) to verify against the correct signer enclave key.
   232	 * @returns {Promise<boolean>} - Returns true if the signature is valid, otherwise throws an error.
   233	 */
   234	const verifyEnclaveSignature = async (
   235	  enclaveQuorumPublic: string,
   236	  publicSignature: string,
   237	  signedData: string,
   238	  dangerouslyOverrideSignerPublicKey?: string,
   239	): Promise<boolean> => {
   240	  const expectedSignerPublicKey =
   241	    dangerouslyOverrideSignerPublicKey || PRODUCTION_SIGNER_SIGN_PUBLIC_KEY;
   242	  if (enclaveQuorumPublic != expectedSignerPublicKey) {
   243	    throw new Error(
   244	      `expected signer key ${
   245	        dangerouslyOverrideSignerPublicKey ?? PRODUCTION_SIGNER_SIGN_PUBLIC_KEY
   246	      } does not match signer key from bundle: ${enclaveQuorumPublic}`,
   247	    );
   248	  }
   249	
   250	  const encryptionQuorumPublicBuf = new Uint8Array(
   251	    uint8ArrayFromHexString(enclaveQuorumPublic),
   252	  );
   253	  const quorumKey = loadPublicKey(encryptionQuorumPublicBuf);
   254	  if (!quorumKey) {
   255	    throw new Error("failed to load quorum key");
   256	  }
   257	
   258	  // Convert the ASN.1 DER-encoded signature for verification
   259	  const publicSignatureBuf = fromDerSignature(publicSignature);
   260	  const signedDataBuf = uint8ArrayFromHexString(signedData);
   261	  const hashedData = sha256(signedDataBuf);
   262	
   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
   264	};
   265	
   266	/**
   267	 * Loads an ECDSA public key from a raw format for signature verification.
   268	 *
   269	 * @param {Uint8Array} publicKey - The raw P-256 public key bytes.
   270	 * @returns {ProjPointType<bigint>} - The parsed ECDSA public key.
   271	 * @throws {Error} - If the public key is invalid.
   272	 */
   273	const loadPublicKey = (publicKey: Uint8Array): ProjPointType<bigint> => {
   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
   275	};
   276	
   277	/**
   278	 * Decodes a private key based on the specified format.
   279	 *
   280	 * @param {string} privateKey - The private key to decode.
   281	 * @param {string} keyFormat - The format of the private key (e.g., "SOLANA", "HEXADECIMAL").
   282	 * @returns {Uint8Array} - The decoded private key.
   283	 */
   284	const decodeKey = (privateKey: string, keyFormat: any): Uint8Array => {
   285	  switch (keyFormat) {
   286	    case "SOLANA":
   287	      const decodedKeyBytes = bs58.decode(privateKey);
   288	      if (decodedKeyBytes.length !== 64) {
   289	        throw new Error(
   290	          `invalid key length. Expected 64 bytes. Got ${decodedKeyBytes.length}.`,
   291	        );
   292	      }
   293	      return decodedKeyBytes.subarray(0, 32);
   294	    case "HEXADECIMAL":
   295	      if (privateKey.startsWith("0x")) {
   296	        return uint8ArrayFromHexString(privateKey.slice(2));
   297	      }
   298	      return uint8ArrayFromHexString(privateKey);
   299	    default:
   300	      console.warn(
   301	        `invalid key format: ${keyFormat}. Defaulting to HEXADECIMAL.`,
   302	      );
   303	      if (privateKey.startsWith("0x")) {
   304	        return uint8ArrayFromHexString(privateKey.slice(2));
   305	      }
   306	      return uint8ArrayFromHexString(privateKey);
   307	  }
   308	};
   309	
   310	/**
   311	 * Encrypts a private key bundle using HPKE and verifies the enclave signature.
   312	 *
   313	 * @param {EncryptPrivateKeyToBundleParams} params - An object containing the private key, key format, bundle, user, and organization details. Optionally, you can override the default signer key (for testing purposes)
   314	 * @returns {Promise<string>} - A promise that resolves to a JSON string representing the encrypted bundle.
   315	 * @throws {Error} - If enclave signature verification or any other validation fails.
   316	 */
   317	export const encryptPrivateKeyToBundle = async ({
   318	  privateKey,
   319	  keyFormat,
   320	  importBundle,
   321	  userId,
   322	  organizationId,
   323	  dangerouslyOverrideSignerPublicKey,
   324	}: EncryptPrivateKeyToBundleParams): Promise<string> => {
   325	  const parsedImportBundle = JSON.parse(importBundle);
   326	  const plainTextBuf = decodeKey(privateKey, keyFormat);
   327	  const verified = await verifyEnclaveSignature(
   328	    parsedImportBundle.enclaveQuorumPublic,
   329	    parsedImportBundle.dataSignature,
   330	    parsedImportBundle.data,
   331	    dangerouslyOverrideSignerPublicKey,
   332	  );
   333	  if (!verified) {
   334	    throw new Error(`failed to verify enclave signature: ${importBundle}`);
   335	  }
   336	
   337	  const signedData = JSON.parse(
   338	    new TextDecoder().decode(uint8ArrayFromHexString(parsedImportBundle.data)),
   339	  );
   340	

 succeeded in 0ms:
     1	export const SUITE_ID_1 = new Uint8Array([75, 69, 77, 0, 16]); //KEM suite ID
     2	export const SUITE_ID_2 = new Uint8Array([72, 80, 75, 69, 0, 16, 0, 1, 0, 2]); //HPKE suite ID
     3	export const HPKE_VERSION = new Uint8Array([72, 80, 75, 69, 45, 118, 49]); //HPKE-v1
     4	export const LABEL_SECRET = new Uint8Array([115, 101, 99, 114, 101, 116]); //secret
     5	export const LABEL_EAE_PRK = new Uint8Array([101, 97, 101, 95, 112, 114, 107]); //eae_prk
     6	export const LABEL_SHARED_SECRET = new Uint8Array([
     7	  115, 104, 97, 114, 101, 100, 95, 115, 101, 99, 114, 101, 116,
     8	]); //shared_secret
     9	export const AES_KEY_INFO = new Uint8Array([
    10	  0, 32, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 107,
    11	  101, 121, 0, 143, 195, 174, 184, 50, 73, 10, 75, 90, 179, 228, 32, 35, 40,
    12	  125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34, 135, 39, 183, 10, 64, 33,
    13	  18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53, 41, 180, 122, 198, 166, 48,
    14	  185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151, 113, 201, 158, 80,
    15	]); //key
    16	export const IV_INFO = new Uint8Array([
    17	  0, 12, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 98, 97,
    18	  115, 101, 95, 110, 111, 110, 99, 101, 0, 143, 195, 174, 184, 50, 73, 10, 75,
    19	  90, 179, 228, 32, 35, 40, 125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34,
    20	  135, 39, 183, 10, 64, 33, 18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53,
    21	  41, 180, 122, 198, 166, 48, 185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151,
    22	  113, 201, 158, 80,
    23	]); //base_nonce
    24	export const QUORUM_ENCRYPT_NONCE_LENGTH_BYTES = 12;
    25	export const UNCOMPRESSED_PUB_KEY_LENGTH_BYTES = 65;
    26	export const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode(
    27	  "qos_encryption_hmac_message",
    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
    29	export const PRODUCTION_SIGNER_SIGN_PUBLIC_KEY =
    30	  "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569";
    31	
    32	export const PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY =
    33	  "04d498aa87ac3bf982ac2b5dd9604d0074905cfbda5d62727c5a237b895e6749205e9f7cd566909c4387f6ca25c308445c60884b788560b785f4a96ac33702a469";
    34	
    35	export const PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY =
    36	  "045e899f1fcf7d12b3c8fd997a7a43bb853dd4e8d63419a8f867c70aacc1c4cf9d04848baca41f0c85ffbbd23cbf78967501cd8eca9e4a6369370a9a38f70d13c0";
    37	
    38	export const PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY =
    39	  "02336ebd7e929ef64b87c776b72540255b4c7b41579a24b1e68fb060daa873f9f6";
    40	
    41	// Pinned AWS Nitro Enclaves Root
    42	export const AWS_ROOT_CERT_PEM = `-----BEGIN CERTIFICATE-----
    43	MIICETCCAZagAwIBAgIRAPkxdWgbkK/hHUbMtOTn+FYwCgYIKoZIzj0EAwMwSTEL
    44	MAkGA1UEBhMCVVMxDzANBgNVBAoMBkFtYXpvbjEMMAoGA1UECwwDQVdTMRswGQYD
    45	VQQDDBJhd3Mubml0cm8tZW5jbGF2ZXMwHhcNMTkxMDI4MTMyODA1WhcNNDkxMDI4
    46	MTQyODA1WjBJMQswCQYDVQQGEwJVUzEPMA0GA1UECgwGQW1hem9uMQwwCgYDVQQL
    47	DANBV1MxGzAZBgNVBAMMEmF3cy5uaXRyby1lbmNsYXZlczB2MBAGByqGSM49AgEG
    48	BSuBBAAiA2IABPwCVOumCMHzaHDimtqQvkY4MpJzbolL//Zy2YlES1BR5TSksfbb
    49	48C8WBoyt7F2Bw7eEtaaP+ohG2bnUs990d0JX28TcPQXCEPZ3BABIeTPYwEoCWZE
    50	h8l5YoQwTcU/9KNCMEAwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUkCW1DdkF
    51	R+eWw5b6cp3PmanfS5YwDgYDVR0PAQH/BAQDAgGGMAoGCCqGSM49BAMDA2kAMGYC
    52	MQCjfy+Rocm9Xue4YnwWmNJVA44fA0P5W2OpYow9OYCVRaEevL8uO1XYru5xtMPW
    53	rfMCMQCi85sWBbJwKKXdS6BptQFuZbT73o/gBh1qUxl/nNr12UO8Yfwr6wPLb+6N
    54	IwLz3/Y=
    55	-----END CERTIFICATE-----`;
    56	
    57	// Official SHA-256 fingerprint
    58	export const AWS_ROOT_CERT_SHA256 =
    59	  "641A0321A3E244EFE456463195D606317ED7CDCC3C1756E09893F3C68F79BB5B";
     1	/**
     2	 * Compute the modular square root using the Tonelli-Shanks algorithm.
     3	 */
     4	export const modSqrt = (x: bigint, p: bigint): bigint => {
     5	  if (p <= BigInt(0)) {
     6	    throw new Error("p must be positive");
     7	  }
     8	  const base = x % p;
     9	
    10	  // Check if p % 4 == 3 (applies to NIST curves P-256, P-384, and P-521)
    11	  if (testBit(p, 0) && testBit(p, 1)) {
    12	    const q = (p + BigInt(1)) >> BigInt(2);
    13	    const squareRoot = modPow(base, q, p);
    14	    if ((squareRoot * squareRoot) % p !== base) {
    15	      throw new Error("could not find a modular square root");
    16	    }
    17	    return squareRoot;
    18	  }
    19	
    20	  // Other elliptic curve types not supported
    21	  throw new Error("unsupported modulus value");
    22	};
    23	
    24	/**
    25	 * Test if a specific bit is set.
    26	 */
    27	export const testBit = (n: bigint, i: number): boolean => {
    28	  const m = BigInt(1) << BigInt(i);
    29	  return (n & m) !== BigInt(0);
    30	};
    31	
    32	/**
    33	 * Compute the modular exponentiation.
    34	 */
    35	const modPow = (b: bigint, exp: bigint, p: bigint): bigint => {
    36	  if (exp === BigInt(0)) {
    37	    return BigInt(1);
    38	  }
    39	  let result = b % p;
    40	  const exponentBitString = exp.toString(2);
    41	  for (let i = 1; i < exponentBitString.length; ++i) {
    42	    result = (result * result) % p;
    43	    if (exponentBitString[i] === "1") {
    44	      result = (result * b) % p;
    45	    }
    46	  }
    47	  return result;
    48	};

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '278,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   278	/**
   279	 * Format HPKE Buffer Function
   280	 * Returns a JSON string of an encrypted bundle, separating out the cipher text and the sender public key
   281	 *
   282	 * @param {Uint8Array} encryptedBuf - The result of hpkeAuthEncrypt or hpkeEncrypt
   283	 * @returns {string} - A JSON string with "encappedPublic" and "ciphertext"
   284	 */
   285	
   286	export const formatHpkeBuf = (encryptedBuf: Uint8Array): string => {
   287	  const compressedSenderBuf = encryptedBuf.slice(0, 33);
   288	  const encryptedData = encryptedBuf.slice(33);
   289	
   290	  const encappedKeyBufHex = uint8ArrayToHexString(
   291	    uncompressRawPublicKey(compressedSenderBuf),
   292	  );
   293	  const ciphertextHex = uint8ArrayToHexString(encryptedData);
   294	
   295	  return JSON.stringify({
   296	    encappedPublic: encappedKeyBufHex,
   297	    ciphertext: ciphertextHex,
   298	  });
   299	};
   300	
   301	/**
   302	 * HPKE Decrypt Function
   303	 * Decrypts data using Hybrid Public Key Encryption (HPKE) standard https://datatracker.ietf.org/doc/rfc9180/.
   304	 *
   305	 * @param {HpkeDecryptParams} params - The decryption parameters including ciphertext, encapsulated key, and receiver private key.
   306	 * @returns {Uint8Array} - The decrypted data.
   307	 */
   308	export const hpkeDecrypt = ({
   309	  ciphertextBuf,
   310	  encappedKeyBuf,
   311	  receiverPriv,
   312	}: HpkeDecryptParams): Uint8Array => {
   313	  try {
   314	    let ikm: Uint8Array;
   315	    let info: Uint8Array;
   316	    const receiverPubBuf = getPublicKey(
   317	      uint8ArrayFromHexString(receiverPriv),
   318	      false,
   319	    );
   320	    const aad = buildAdditionalAssociatedData(encappedKeyBuf, receiverPubBuf); // Eventually we want users to be able to pass in aad as optional
   321	
   322	    // Step 1: Generate Shared Secret
   323	    const ss = deriveSS(encappedKeyBuf, receiverPriv);
   324	
   325	    // Step 2: Generate the KEM context
   326	    const kemContext = getKemContext(
   327	      encappedKeyBuf,
   328	      uint8ArrayToHexString(receiverPubBuf),
   329	    );
   330	
   331	    // Step 3: Build the HKDF inputs for key derivation
   332	    ikm = buildLabeledIkm(LABEL_EAE_PRK, ss, SUITE_ID_1);
   333	    info = buildLabeledInfo(LABEL_SHARED_SECRET, kemContext, SUITE_ID_1, 32);
   334	    const sharedSecret = extractAndExpand(new Uint8Array([]), ikm, info, 32);
   335	
   336	    // Step 4: Derive the AES key
   337	    ikm = buildLabeledIkm(LABEL_SECRET, new Uint8Array([]), SUITE_ID_2);
   338	    info = AES_KEY_INFO;
   339	    const key = extractAndExpand(sharedSecret, ikm, info, 32);
   340	
   341	    // Step 5: Derive the initialization vector
   342	    info = IV_INFO;
   343	    const iv = extractAndExpand(sharedSecret, ikm, info, 12);
   344	
   345	    // Step 6: Decrypt the data using AES-GCM
   346	    const decryptedData = aesGcmDecrypt(ciphertextBuf, key, iv, aad);
   347	    return decryptedData;
   348	  } catch (error) {
   349	    throw new Error(`Unable to perform hpkeDecrypt: ${error} `);
   350	  }
   351	};
   352	
   353	/**
   354	 * Generate a P-256 key pair. Contains the hexed privateKey, publicKey, and Uncompressed publicKey
   355	 *
   356	 * @returns {KeyPair} - The generated key pair.
   357	 */
   358	export const generateP256KeyPair = (): KeyPair => {
   359	  const privateKey = randomBytes(32);
   360	  const publicKey = getPublicKey(privateKey, true);
   361	  const publicKeyUncompressed = uint8ArrayToHexString(
   362	    uncompressRawPublicKey(publicKey),
   363	  );
   364	  return {
   365	    privateKey: uint8ArrayToHexString(privateKey),
   366	    publicKey: uint8ArrayToHexString(publicKey),
   367	    publicKeyUncompressed,
   368	  };
   369	};
   370	
   371	/**
   372	 * Create additional associated data (AAD) for AES-GCM decryption.
   373	 *
   374	 * @param {Uint8Array} senderPubBuf
   375	 * @param {Uint8Array} receiverPubBuf
   376	 * @return {Uint8Array} - The resulting concatenation of sender and receiver pubkeys.
   377	 */
   378	export const buildAdditionalAssociatedData = (
   379	  senderPubBuf: Uint8Array,
   380	  receiverPubBuf: Uint8Array,
   381	): Uint8Array => {
   382	  return new Uint8Array([
   383	    ...Array.from(senderPubBuf),
   384	    ...Array.from(receiverPubBuf),
   385	  ]);
   386	};
   387	
   388	/**
   389	 * Accepts a private key Uint8Array in the PKCS8 format, and returns the encapsulated private key.
   390	 *
   391	 * @param {Uint8Array} privateKey - A PKCS#8 private key structured with the key data at a specific position. The actual key starts at byte 36 and is 32 bytes long.
   392	 * @return {Uint8Array} - The private key.
   393	 */
   394	export const extractPrivateKeyFromPKCS8Bytes = (
   395	  privateKey: Uint8Array,
   396	): Uint8Array => {
   397	  return privateKey.slice(36, 36 + 32);
   398	};
   399	
   400	/**
   401	 * Accepts a public key Uint8Array, and returns a Uint8Array with the compressed version of the public key.
   402	 *
   403	 * @param {Uint8Array} rawPublicKey - The raw public key.
   404	 * @return {Uint8Array} – The compressed public key.
   405	 */
   406	export const compressRawPublicKey = (rawPublicKey: Uint8Array): Uint8Array => {
   407	  const len = rawPublicKey.byteLength;
   408	
   409	  // Drop the y coordinate
   410	  // Uncompressed key is in the form 0x04||x||y
   411	  // `len >>> 1` is a more concise way to write `floor(len/2)`
   412	  var compressedBytes = rawPublicKey.slice(0, (1 + len) >>> 1);
   413	
   414	  // Encode the parity of `y` in first bit
   415	  // `BYTE & 0x01` tests for parity and returns 0x00 when even, or 0x01 when odd
   416	  // Then `0x02 | <parity test result>` yields either 0x02 (even case) or 0x03 (odd).
   417	  compressedBytes[0] = 0x02 | (rawPublicKey[len - 1]! & 0x01);
   418	  return compressedBytes;
   419	};
   420	
   421	/**
   422	 * Accepts a public key array buffer, and returns a buffer with the uncompressed version of the public key
   423	 * @param {Uint8Array} rawPublicKey - The public key.
   424	 * @return {Uint8Array} - The uncompressed public key.
   425	 */
   426	export const uncompressRawPublicKey = (
   427	  rawPublicKey: Uint8Array,
   428	  curve: Curve = "CURVE_P256",
   429	): Uint8Array => {
   430	  if (rawPublicKey.length !== 33) {
   431	    throw new Error("failed to uncompress raw public key: invalid length");
   432	  }
   433	
   434	  if (!(rawPublicKey[0] === 2 || rawPublicKey[0] === 3)) {
   435	    throw new Error("failed to uncompress raw public key: invalid prefix");
   436	  }
   437	
   438	  // point[0] must be 2 (false) or 3 (true).
   439	  // this maps to the initial "02" or "03" prefix
   440	  const lsb = rawPublicKey[0] === 3;
   441	  const x = BigInt("0x" + uint8ArrayToHexString(rawPublicKey.subarray(1)));
   442	
   443	  let p: bigint, a: bigint, b: bigint;
   444	  if (curve === "CURVE_P256") {
   445	    // p-256 domain parameters
   446	    // https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.186-4.pdf (Appendix D).
   447	    p = BigInt(
   448	      "115792089210356248762697446949407573530086143415290314195533631308867097853951",
   449	    );
   450	    b = BigInt(
   451	      "0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b",
   452	    );
   453	    a = p - BigInt(3);
   454	  } else {
   455	    // secp256k1 domain parameters
   456	    // https://www.secg.org/sec2-v2.pdf (Section 2.4.1).
   457	    p = BigInt(
   458	      "0xfffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f",
   459	    );
   460	    a = BigInt(0);
   461	    b = BigInt(7);
   462	  }
   463	
   464	  // Now compute y based on x
   465	  const rhs = ((x * x + a) * x + b) % p;
   466	  let y = modSqrt(rhs, p);
   467	  if (lsb !== testBit(y, 0)) {
   468	    y = (p - y) % p;
   469	  }
   470	
   471	  if (x < BigInt(0) || x >= p) {
   472	    throw new Error("x is out of range");
   473	  }
   474	
   475	  if (y < BigInt(0) || y >= p) {
   476	    throw new Error("y is out of range");
   477	  }
   478	
   479	  var uncompressedHexString = "04" + bigIntToHex(x, 64) + bigIntToHex(y, 64);
   480	  return uint8ArrayFromHexString(uncompressedHexString);
   481	};
   482	
   483	/**
   484	 * Build labeled Initial Key Material (IKM).
   485	 *
   486	 * @param {Uint8Array} label - The label to use.
   487	 * @param {Uint8Array} ikm - The input key material.
   488	 * @param {Uint8Array} suiteId - The suite identifier.
   489	 * @returns {Uint8Array} - The labeled IKM.
   490	 */
   491	const buildLabeledIkm = (
   492	  label: Uint8Array,
   493	  ikm: Uint8Array,
   494	  suiteId: Uint8Array,
   495	): Uint8Array => {
   496	  const combinedLength =
   497	    HPKE_VERSION.length + suiteId.length + label.length + ikm.length;
   498	  const ret = new Uint8Array(combinedLength);
   499	  let offset = 0;
   500	
   501	  ret.set(HPKE_VERSION, offset);
   502	  offset += HPKE_VERSION.length;
   503	
   504	  ret.set(suiteId, offset);
   505	  offset += suiteId.length;
   506	
   507	  ret.set(label, offset);
   508	  offset += label.length;
   509	
   510	  ret.set(ikm, offset);
   511	
   512	  return ret;
   513	};
   514	
   515	/**
   516	 * Build labeled info for HKDF operations.
   517	 *
   518	 * @param {Uint8Array} label - The label to use.
   519	 * @param {Uint8Array} info - Additional information.
   520	 * @param {Uint8Array} suiteId - The suite identifier.
   521	 * @param {number} len - The output length.
   522	 * @returns {Uint8Array} - The labeled info.
   523	 */
   524	const buildLabeledInfo = (
   525	  label: Uint8Array,
   526	  info: Uint8Array,
   527	  suiteId: Uint8Array,
   528	  len: number,
   529	): Uint8Array => {
   530	  const suiteIdStartIndex = 9; // first two are reserved for length bytes (unused in this case), the next 7 are for the HPKE_VERSION, then the suiteId starts at 9
   531	  const ret = new Uint8Array(
   532	    suiteIdStartIndex + suiteId.byteLength + label.byteLength + info.byteLength,
   533	  );
   534	  ret.set(new Uint8Array([0, len]), 0); // this isn’t an error, we’re starting at index 2 because the first two bytes should be 0. See <https://github.com/dajiaji/hpke-js/blob/1e7fb1372fbcdb6d06bf2f4fa27ff676329d633e/src/kdfs/hkdf.ts#L41> for reference.
   535	  ret.set(HPKE_VERSION, 2);
   536	  ret.set(suiteId, suiteIdStartIndex);
   537	  ret.set(label, suiteIdStartIndex + suiteId.byteLength);
   538	  ret.set(info, suiteIdStartIndex + suiteId.byteLength + label.byteLength);
   539	  return ret;
   540	};
   541	
   542	/**
   543	 * Perform HKDF extract and expand operations.
   544	 */
   545	const extractAndExpand = (
   546	  sharedSecret: Uint8Array,
   547	  ikm: Uint8Array,
   548	  info: Uint8Array,
   549	  len: number,
   550	): Uint8Array => {
   551	  const prk = hkdf.extract(sha256, ikm, sharedSecret);
   552	  const resp = hkdf.expand(sha256, prk, info, len);
   553	  return new Uint8Array(resp);
   554	};
   555	
   556	/**
   557	 * Derive the Diffie-Hellman shared secret using ECDH.
   558	 */
   559	const deriveSS = (encappedKeyBuf: Uint8Array, priv: string): Uint8Array => {
   560	  const ss = p256.getSharedSecret(
   561	    uint8ArrayFromHexString(priv),
   562	    encappedKeyBuf,
   563	  );
   564	  return ss.slice(1);
   565	};
   566	
   567	/**
   568	 * Encrypt data using AES-GCM.
   569	 */
   570	const aesGcmEncrypt = (
   571	  plainTextData: Uint8Array,
   572	  key: Uint8Array,
   573	  iv: Uint8Array,
   574	  aad?: Uint8Array,
   575	): Uint8Array => {
   576	  const aes = gcm(key, iv, aad);
   577	  const data = aes.encrypt(plainTextData);
   578	  return data;
   579	};
   580	
   581	/**
   582	 * Decrypt data using AES-GCM.
   583	 */
   584	const aesGcmDecrypt = (
   585	  encryptedData: Uint8Array,
   586	  key: Uint8Array,
   587	  iv: Uint8Array,
   588	  aad?: Uint8Array,
   589	): Uint8Array => {
   590	  const aes = gcm(key, iv, aad);
   591	  const data = aes.decrypt(encryptedData);
   592	  return data;
   593	};
   594	
   595	/**
   596	 * Generate a Key Encapsulation Mechanism (KEM) context.
   597	 */
   598	const getKemContext = (
   599	  encappedKeyBuf: Uint8Array,
   600	  publicKey: string,
   601	): Uint8Array => {
   602	  const encappedKeyArray = new Uint8Array(encappedKeyBuf);
   603	  const publicKeyArray = uint8ArrayFromHexString(publicKey);
   604	
   605	  const kemContext = new Uint8Array(
   606	    encappedKeyArray.length + publicKeyArray.length,
   607	  );
   608	  kemContext.set(encappedKeyArray);
   609	  kemContext.set(publicKeyArray, encappedKeyArray.length);
   610	
   611	  return kemContext;
   612	};
   613	
   614	/**
   615	 * Convert a BigInt to a hexadecimal string of a specific length.
   616	 */
   617	const bigIntToHex = (num: bigint, length: number): string => {
   618	  const hexString = num.toString(16);
   619	  if (hexString.length > length) {
   620	    throw new Error(

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '620,760p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   620	    throw new Error(
   621	      `number cannot fit in a hex string of ${length} characters`,
   622	    );
   623	  }
   624	  return hexString.padStart(length, "0");
   625	};
   626	
   627	/**
   628	 * Converts an ASN.1 DER-encoded ECDSA signature to the raw format used for verification.
   629	 *
   630	 * @param {string} derSignature - The DER-encoded signature.
   631	 * @returns {Uint8Array} - The raw signature.
   632	 */
   633	export const fromDerSignature = (derSignature: string): Uint8Array => {
   634	  const derSignatureBuf = uint8ArrayFromHexString(derSignature);
   635	
   636	  // Check minimum length
   637	  if (derSignatureBuf.length < 2) {
   638	    throw new Error(
   639	      "failed to convert DER-encoded signature: insufficient length",
   640	    );
   641	  }
   642	
   643	  // Check SEQUENCE tag (0x30 at first byte)
   644	  if (derSignatureBuf[0] !== 0x30) {
   645	    throw new Error(
   646	      "failed to convert DER-encoded signature: invalid format (missing SEQUENCE tag)",
   647	    );
   648	  }
   649	
   650	  // Check second byte, start of length field
   651	  let index = 1;
   652	  const lengthByte = derSignatureBuf[index]!;
   653	  if (lengthByte <= 0x7f) {
   654	    // Short form: single byte length
   655	
   656	    // directly take the consumed value as length and check against buffer
   657	    // buffer length: initial header bytes + claimed remaining length
   658	    if (derSignatureBuf.length < 1 + 1 + lengthByte) {
   659	      throw new Error(
   660	        "failed to convert DER-encoded signature: inconsistent message length header",
   661	      );
   662	    }
   663	
   664	    // continue parsing
   665	    index += 1;
   666	  } else {
   667	    // Multi-byte DER length header
   668	    // Invalid DER values: lengthByte 0x80 and 0xff
   669	    // Valid DER values: lengthByte > 0x80, < 0xff
   670	    //
   671	    // We do not expect signature data in the Long form notation
   672	    // -> reject all such inputs
   673	    //
   674	    // More complex parsing for longer signature sequences can be implemented once needed
   675	    throw new Error(
   676	      "failed to convert DER-encoded signature: unexpectedly large or invalid signature length",
   677	    );
   678	  }
   679	
   680	  // Parse 'r' and check for integer tag (0x02)
   681	  if (derSignatureBuf[index] !== 0x02) {
   682	    throw new Error(
   683	      "failed to convert DER-encoded signature: invalid tag for r",
   684	    );
   685	  }
   686	
   687	  index++; // Move past the INTEGER tag
   688	  const rLength = derSignatureBuf[index]!;
   689	
   690	  // Allow up to 32 data bytes + 1 byte 0-padding prefix
   691	  if (rLength > 33) {
   692	    throw new Error(
   693	      "failed to convert DER-encoded signature: unexpected length for r",
   694	    );
   695	  }
   696	
   697	  index++; // Move past the length byte
   698	  const r = derSignatureBuf.slice(index, index + rLength);
   699	  index += rLength; // Move to the start of s
   700	
   701	  // Parse 's' and check for integer tag (0x02)
   702	  if (derSignatureBuf[index] !== 0x02) {
   703	    throw new Error(
   704	      "failed to convert DER-encoded signature: invalid tag for s",
   705	    );
   706	  }
   707	
   708	  index++; // Move past the INTEGER tag
   709	  const sLength = derSignatureBuf[index]!;
   710	
   711	  // Allow up to 32 data bytes + 1 byte 0-padding prefix
   712	  if (sLength > 33) {
   713	    throw new Error(
   714	      "failed to convert DER-encoded signature: unexpected length for s",
   715	    );
   716	  }
   717	
   718	  index++; // Move past the length byte
   719	  const s = derSignatureBuf.slice(index, index + sLength);
   720	
   721	  // Normalize 'r' and 's' to 32 bytes each
   722	  const rPadded = normalizePadding(r, 32);
   723	  const sPadded = normalizePadding(s, 32);
   724	
   725	  // Concatenate and return the raw signature
   726	  return new Uint8Array([...rPadded, ...sPadded]);
   727	};
   728	
   729	/**
   730	 * Converts a raw ECDSA signature to DER-encoded format.
   731	 *
   732	 * This function takes a raw ECDSA signature, which is a concatenation of two 32-byte integers (r and s),
   733	 * and converts it into the DER-encoded format. DER (Distinguished Encoding Rules) is a binary encoding
   734	 * for data structures described by ASN.1.
   735	 *
   736	 * @param {string} rawSignature - The raw signature in hexadecimal string format.
   737	 * @returns {string} - The DER-encoded signature in hexadecimal string format.
   738	 *
   739	 * @throws {Error} - Throws an error if the input signature is invalid or if the encoding process fails.
   740	 *
   741	 * @example
   742	 * // Example usage:
   743	 * const rawSignature = "0x487cdb8a88f2f4044b701cbb116075c4cabe5fe4657a6358b395c0aab70694db3453a8057e442bd1aff0ecabe8a82c831f0edd7f2158b7c1feb3de9b1f20309b1c";
   744	 * const derSignature = toDerSignature(rawSignature);
   745	 * console.log(derSignature); // Outputs the DER-encoded signature as a hex string
   746	 * // "30440220487cdb8a88f2f4044b701cbb116075c4cabe5fe4657a6358b395c0aab70694db02203453a8057e442bd1aff0ecabe8a82c831f0edd7f2158b7c1feb3de9b1f20309b"
   747	 */
   748	export const toDerSignature = (rawSignature: string) => {
   749	  const rawSignatureBuf = uint8ArrayFromHexString(rawSignature);
   750	
   751	  // Split raw signature into r and s, each 32 bytes
   752	  const r = rawSignatureBuf.slice(0, 32);
   753	  const s = rawSignatureBuf.slice(32, 64);
   754	
   755	  // Helper function to encode an integer with DER structure
   756	  const encodeDerInteger = (integer?: Uint8Array): Uint8Array => {
   757	    // Check if integer is defined and has at least one byte
   758	    if (
   759	      integer === undefined ||
   760	      integer.length === 0 ||

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '317,520p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   317	export const encryptPrivateKeyToBundle = async ({
   318	  privateKey,
   319	  keyFormat,
   320	  importBundle,
   321	  userId,
   322	  organizationId,
   323	  dangerouslyOverrideSignerPublicKey,
   324	}: EncryptPrivateKeyToBundleParams): Promise<string> => {
   325	  const parsedImportBundle = JSON.parse(importBundle);
   326	  const plainTextBuf = decodeKey(privateKey, keyFormat);
   327	  const verified = await verifyEnclaveSignature(
   328	    parsedImportBundle.enclaveQuorumPublic,
   329	    parsedImportBundle.dataSignature,
   330	    parsedImportBundle.data,
   331	    dangerouslyOverrideSignerPublicKey,
   332	  );
   333	  if (!verified) {
   334	    throw new Error(`failed to verify enclave signature: ${importBundle}`);
   335	  }
   336	
   337	  const signedData = JSON.parse(
   338	    new TextDecoder().decode(uint8ArrayFromHexString(parsedImportBundle.data)),
   339	  );
   340	
   341	  if (
   342	    !signedData.organizationId ||
   343	    signedData.organizationId !== organizationId
   344	  ) {
   345	    throw new Error(
   346	      `organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`,
   347	    );
   348	  }
   349	  if (!signedData.userId || signedData.userId !== userId) {
   350	    throw new Error(
   351	      `user id does not match expected value. Expected: ${userId}. Found: ${signedData.userId}.`,
   352	    );
   353	  }
   354	
   355	  if (!signedData.targetPublic) {
   356	    throw new Error('missing "targetPublic" in bundle signed data');
   357	  }
   358	
   359	  // Load target public key generated from enclave
   360	  const targetKeyBuf = uint8ArrayFromHexString(signedData.targetPublic);
   361	  const privateKeyBundle = hpkeEncrypt({ plainTextBuf, targetKeyBuf });
   362	  return formatHpkeBuf(privateKeyBundle);
   363	};
   364	
   365	/**
   366	  /**
   367	   * Encrypts a mnemonic wallet bundle using HPKE and verifies the enclave signature.
   368	   *
   369	   * @param {EncryptWalletToBundleParams} params - An object containing the mnemonic, bundle, user, and organization details. Optionally, you can override the default signer key (for testing purposes).
   370	   * @returns {Promise<string>} - A promise that resolves to a JSON string representing the encrypted wallet bundle.
   371	   * @throws {Error} - If enclave signature verification or any other validation fails.
   372	   */
   373	export const encryptWalletToBundle = async ({
   374	  mnemonic,
   375	  importBundle,
   376	  userId,
   377	  organizationId,
   378	  dangerouslyOverrideSignerPublicKey,
   379	}: EncryptWalletToBundleParams): Promise<string> => {
   380	  const parsedImportBundle = JSON.parse(importBundle);
   381	  const plainTextBuf = new TextEncoder().encode(mnemonic);
   382	  const verified = await verifyEnclaveSignature(
   383	    parsedImportBundle.enclaveQuorumPublic,
   384	    parsedImportBundle.dataSignature,
   385	    parsedImportBundle.data,
   386	    dangerouslyOverrideSignerPublicKey,
   387	  );
   388	  if (!verified) {
   389	    throw new Error(`failed to verify enclave signature: ${importBundle}`);
   390	  }
   391	
   392	  const signedData = JSON.parse(
   393	    new TextDecoder().decode(uint8ArrayFromHexString(parsedImportBundle.data)),
   394	  );
   395	
   396	  if (
   397	    !signedData.organizationId ||
   398	    signedData.organizationId !== organizationId
   399	  ) {
   400	    throw new Error(
   401	      `organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`,
   402	    );
   403	  }
   404	  if (!signedData.userId || signedData.userId !== userId) {
   405	    throw new Error(
   406	      `user id does not match expected value. Expected: ${userId}. Found: ${signedData.userId}.`,
   407	    );
   408	  }
   409	
   410	  if (!signedData.targetPublic) {
   411	    throw new Error('missing "targetPublic" in bundle signed data');
   412	  }
   413	
   414	  // Load target public key generated from enclave
   415	  const targetKeyBuf = uint8ArrayFromHexString(signedData.targetPublic);
   416	  const privateKeyBundle = hpkeEncrypt({ plainTextBuf, targetKeyBuf });
   417	  return formatHpkeBuf(privateKeyBundle);
   418	};
   419	
   420	/**
   421	 * Verifies that a **session JWT** was signed by Turnkey’s
   422	 * notarizer key (P-256 / ES256, compact 64-byte r‖s signature).
   423	 *
   424	 * How it works
   425	 * ------------
   426	 * 1.  Split the JWT into `header.payload.signature`.
   427	 * 2.  **Double-hash** the string `"header.payload"`:
   428	 *        `h1 = sha256(header.payload)`
   429	 *        `msg = sha256(h1)`
   430	 *     (The Rust signer feeds `h1` into `SigningKey::sign`, which hashes once
   431	 *     more internally, yielding `msg`.)
   432	 * 3.  Base64-URL-decode the signature (`r||s`, 64 bytes).
   433	 * 4.  Import the notarizer public key (hex `04‖X‖Y` → `Uint8Array`).
   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
   435	 *     `msg` as a pre-hashed digest and performs ECDSA verification.
   436	 *
   437	 * @param jwt   The session JWT to validate.
   438	 * @param dangerouslyOverrideNotarizerPublicKey *(optional)* Hex-encoded
   439	 *              uncompressed P-256 public key to verify against (use only in
   440	 *              tests).  Defaults to the production notarizer key.
   441	 * @returns `true` if the signature is valid for the given key, else `false`.
   442	 * @throws  If the JWT is malformed.
   443	 */
   444	export const verifySessionJwtSignature = async (
   445	  jwt: string,
   446	  dangerouslyOverrideNotarizerPublicKey?: string,
   447	): Promise<boolean> => {
   448	  const notarizerKeyHex =
   449	    dangerouslyOverrideNotarizerPublicKey ??
   450	    PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY;
   451	
   452	  /* 1. split JWT -------------------------------------------------------- */
   453	  const [headerB64, payloadB64, signatureB64] = jwt.split(".");
   454	  if (!signatureB64) throw new Error("invalid JWT: need 3 parts");
   455	  const signingInput = `${headerB64}.${payloadB64}`;
   456	
   457	  /* 2. sha256(sha256(header.payload)) ----------------------------------- */
   458	  const h1 = sha256(new TextEncoder().encode(signingInput));
   459	  const msgDigest = sha256(h1); // 32-byte Uint8Array
   460	
   461	  /* 3. base64-url decode signature -------------------------------------- */
   462	  const toB64 = (u: string) =>
   463	    (u = u.replace(/-/g, "+").replace(/_/g, "/")).padEnd(
   464	      u.length + ((4 - (u.length % 4)) % 4),
   465	      "=",
   466	    );
   467	  const signature = Uint8Array.from(
   468	    atob(toB64(signatureB64))
   469	      .split("")
   470	      .map((c) => c.charCodeAt(0)),
   471	  ); // 64 bytes
   472	
   473	  /* 4. load public key -------------------------------------------------- */
   474	  const publicKey = uint8ArrayFromHexString(notarizerKeyHex);
   475	
   476	  /* 5. verify ----------------------------------------------------------- */
   477	  return p256.verify(signature, msgDigest, publicKey);
   478	};
   479	
   480	/**
   481	 * Encrypts a message to an uncompressed P256 public key
   482	 * The function takes in standard strings and converts them
   483	 * to Uint8Arrays to be used by the lower level quorumKeyEncrypt
   484	 * function. More details about how the encryption works is described
   485	 * in that function's documentation.
   486	 *
   487	 * @param targetPublicKeyUncompressed A hex string uncompressed public key to encrypt a message to
   488	 * @param message A standard string message to encrypt, does not have to be hex encoded
   489	 * @returns {Promise<Uint8Array>} A borsh serialized envelope with the encrypted message (more details found in quorumKeyEncrypt)
   490	 */
   491	export const encryptToEnclave = async (
   492	  targetPublicKeyUncompressed: string,
   493	  message: string,
   494	): Promise<Uint8Array> => {
   495	  return await quorumKeyEncrypt(
   496	    uint8ArrayFromHexString(targetPublicKeyUncompressed),
   497	    new TextEncoder().encode(message),
   498	  );
   499	};
   500	
   501	/**
   502	 * Helper function used specifically to encrypt a client secret to
   503	 * TLS Fetchers quorum key. This is used for client_secret upload
   504	 * when enabling authentication with an OAuth 2.0 provider
   505	 *
   506	 * @param client_secret The client secret issued by the OAuth 2.0 provider
   507	 * @param dangerouslyOverrideTlsFetcherPublicKey *(optional)* Hex-encoded
   508	 *              uncompressed P-256 public key to encrypt to (use only in
   509	 *              tests/dev environment).  Defaults to the production TLS Fetcher key.
   510	 * @returns {Promise<string>} A hex encoded borsh serialized envelope with the encrypted client
   511	 *                            secret meant to be passed to the CreateOauth2Credential Activity
   512	 */
   513	export const encryptOauth2ClientSecret = async (
   514	  client_secret: string,
   515	  dangerouslyOverrideTlsFetcherPublicKey?: string,
   516	): Promise<string> => {
   517	  return uint8ArrayToHexString(
   518	    await encryptToEnclave(
   519	      dangerouslyOverrideTlsFetcherPublicKey ??
   520	        PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY,

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '1,540p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// CryptoTests.cs — xunit tests for src/Crypto.cs.
     2	//
     3	// Vector sources:
     4	//   - HKDF RFC 5869 test cases A.1..A.3 (the SHA-256 cases)
     5	//   - NIST P-256 known points (G and 2G) for GetPublicKey
     6	//   - Compress/UncompressRawPublicKey roundtrip
     7	//   - HpkeEncrypt/HpkeDecrypt roundtrip (deterministic round-trip)
     8	//   - VerifySessionJwtSignature negative cases
     9	//
    10	// Tests that require a Turnkey-signed bundle (DecryptCredentialBundle,
    11	// EncryptPrivateKeyToBundle, DecryptExportBundle against PRODUCTION_SIGNER)
    12	// are not unit-testable without a real Turnkey signing key. They are
    13	// implicitly exercised by the HPKE roundtrip (the inner HPKE step is
    14	// identical) and by the E2E whoami flow once credentials are present.
    15	
    16	using System;
    17	using System.Text;
    18	using FluentAssertions;
    19	using Org.BouncyCastle.Math;
    20	using Xunit;
    21	
    22	namespace Turnkey.Tests
    23	{
    24	    public class CryptoTests
    25	    {
    26	        // ============================================================
    27	        // Math.ModSqrt — Tonelli-Shanks
    28	        // ============================================================
    29	
    30	        [Fact]
    31	        public void ModSqrt_SmallPrime_HappyPath()
    32	        {
    33	            // 4 = 2^2 mod 7, ModSqrt(4, 7) should be 2 or 5 (-2 mod 7).
    34	            var p = new BigInteger("7");
    35	            var x = new BigInteger("4");
    36	            var root = Crypto.Math.ModSqrt(x, p);
    37	            (root.Equals(new BigInteger("2")) || root.Equals(new BigInteger("5")))
    38	                .Should().BeTrue($"got {root}");
    39	        }
    40	
    41	        [Fact]
    42	        public void ModSqrt_NonResidue_Throws()
    43	        {
    44	            // 3 is a non-residue mod 7.
    45	            var p = new BigInteger("7");
    46	            var x = new BigInteger("3");
    47	            Action act = () => Crypto.Math.ModSqrt(x, p);
    48	            act.Should().Throw<InvalidOperationException>()
    49	               .WithMessage("could not find a modular square root");
    50	        }
    51	
    52	        [Fact]
    53	        public void ModSqrt_NonPositiveP_Throws()
    54	        {
    55	            var p = new BigInteger("0");
    56	            var x = new BigInteger("1");
    57	            Action act = () => Crypto.Math.ModSqrt(x, p);
    58	            act.Should().Throw<ArgumentException>()
    59	               .WithMessage("p must be positive");
    60	        }
    61	
    62	        [Fact]
    63	        public void ModSqrt_P256Prime_Works()
    64	        {
    65	            // P-256 prime. Pick any quadratic residue: 4 = 2^2.
    66	            var p = new BigInteger(CryptoConstants.P256_P);
    67	            var x = new BigInteger("4");
    68	            var root = Crypto.Math.ModSqrt(x, p);
    69	            root.Multiply(root).Mod(p).Equals(x).Should().BeTrue();
    70	        }
    71	
    72	        // ============================================================
    73	        // HKDF RFC 5869
    74	        // ============================================================
    75	
    76	        // RFC 5869 Test Case 1 (Basic test case with SHA-256)
    77	        [Fact]
    78	        public void Hkdf_Rfc5869_A1()
    79	        {
    80	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
    81	            byte[] salt = HexToBytes("000102030405060708090a0b0c");
    82	            byte[] info = HexToBytes("f0f1f2f3f4f5f6f7f8f9");
    83	            int L = 42;
    84	
    85	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
    86	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
    87	
    88	            BytesToHex(prk).Should().Be(
    89	                "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5");
    90	            BytesToHex(okm).Should().Be(
    91	                "3cb25f25faacd57a90434f64d0362f2a"
    92	                + "2d2d0a90cf1a5a4c5db02d56ecc4c5bf"
    93	                + "34007208d5b887185865");
    94	        }
    95	
    96	        // RFC 5869 Test Case 2 (Test with SHA-256 and longer inputs/outputs)
    97	        [Fact]
    98	        public void Hkdf_Rfc5869_A2()
    99	        {
   100	            byte[] ikm = HexToBytes(
   101	                "000102030405060708090a0b0c0d0e0f"
   102	                + "101112131415161718191a1b1c1d1e1f"
   103	                + "202122232425262728292a2b2c2d2e2f"
   104	                + "303132333435363738393a3b3c3d3e3f"
   105	                + "404142434445464748494a4b4c4d4e4f");
   106	            byte[] salt = HexToBytes(
   107	                "606162636465666768696a6b6c6d6e6f"
   108	                + "707172737475767778797a7b7c7d7e7f"
   109	                + "808182838485868788898a8b8c8d8e8f"
   110	                + "909192939495969798999a9b9c9d9e9f"
   111	                + "a0a1a2a3a4a5a6a7a8a9aaabacadaeaf");
   112	            byte[] info = HexToBytes(
   113	                "b0b1b2b3b4b5b6b7b8b9babbbcbdbebf"
   114	                + "c0c1c2c3c4c5c6c7c8c9cacbcccdcecf"
   115	                + "d0d1d2d3d4d5d6d7d8d9dadbdcdddedf"
   116	                + "e0e1e2e3e4e5e6e7e8e9eaebecedeeef"
   117	                + "f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff");
   118	            int L = 82;
   119	
   120	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
   121	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
   122	
   123	            BytesToHex(prk).Should().Be(
   124	                "06a6b88c5853361a06104c9ceb35b45cef760014904671014a193f40c15fc244");
   125	            BytesToHex(okm).Should().Be(
   126	                "b11e398dc80327a1c8e7f78c596a4934"
   127	                + "4f012eda2d4efad8a050cc4c19afa97c"
   128	                + "59045a99cac7827271cb41c65e590e09"
   129	                + "da3275600c2f09b8367793a9aca3db71"
   130	                + "cc30c58179ec3e87c14c01d5c1f3434f"
   131	                + "1d87");
   132	        }
   133	
   134	        // RFC 5869 Test Case 3 (Test with SHA-256 and zero-length salt/info)
   135	        [Fact]
   136	        public void Hkdf_Rfc5869_A3()
   137	        {
   138	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
   139	            byte[] salt = Array.Empty<byte>();
   140	            byte[] info = Array.Empty<byte>();
   141	            int L = 42;
   142	
   143	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
   144	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
   145	
   146	            BytesToHex(prk).Should().Be(
   147	                "19ef24a32c717b167f33a91d6f648bdf96596776afdb6377ac434c1c293ccb04");
   148	            BytesToHex(okm).Should().Be(
   149	                "8da4e775a563c18f715f802a063c5a31"
   150	                + "b8a11f5c5ee1879ec3454e5f3c738d2d"
   151	                + "9d201395faa4b61a96c8");
   152	        }
   153	
   154	        [Fact]
   155	        public void Hkdf_Expand_Length0_ProducesEmpty()
   156	        {
   157	            byte[] prk = HexToBytes(
   158	                "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5");
   159	            byte[] okm = Crypto.Hkdf.Expand(prk, null!, 0);
   160	            okm.Should().BeEmpty();
   161	        }
   162	
   163	        [Fact]
   164	        public void Hkdf_Expand_OverMaxLength_Throws()
   165	        {
   166	            byte[] prk = new byte[32];
   167	            Action act = () => Crypto.Hkdf.Expand(prk, null!, 255 * 32 + 1);
   168	            act.Should().Throw<ArgumentException>();
   169	        }
   170	
   171	        [Fact]
   172	        public void Hkdf_Extract_NullSalt_UsesZeroSalt()
   173	        {
   174	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
   175	            byte[] prkA = Crypto.Hkdf.Extract(null!, ikm);
   176	            byte[] prkB = Crypto.Hkdf.Extract(new byte[32], ikm); // RFC default
   177	            prkA.Should().Equal(prkB);
   178	        }
   179	
   180	        // ============================================================
   181	        // CompressRawPublicKey / UncompressRawPublicKey
   182	        // ============================================================
   183	
   184	        [Fact]
   185	        public void CompressUncompress_Roundtrip_RandomKeys()
   186	        {
   187	            for (int i = 0; i < 8; i++)
   188	            {
   189	                var kp = Crypto.GenerateP256KeyPair();
   190	                byte[] uncompressed = Encoding.Uint8ArrayFromHexString(kp.PublicKeyUncompressed);
   191	                byte[] compressed = Crypto.CompressRawPublicKey(uncompressed);
   192	                compressed.Should().HaveCount(33);
   193	                compressed[0].Should().BeOneOf((byte)0x02, (byte)0x03);
   194	
   195	                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
   196	                recovered.Should().Equal(uncompressed);
   197	            }
   198	        }
   199	
   200	        [Fact]
   201	        public void CompressRawPublicKey_Permissive_OnAnyLength()
   202	        {
   203	            // Upstream behavior: slice + LSB-flip regardless of input shape.
   204	            // For a 65-byte uncompressed key we produce a 33-byte compressed key.
   205	            // For other lengths we produce a different-length output that
   206	            // upstream would also produce. Confirm we no longer throw on
   207	            // malformed input.
   208	            var raw = new byte[64];
   209	            raw[0] = 0x04;
   210	            raw[63] = 0x01;
   211	            Action act = () => Crypto.CompressRawPublicKey(raw);
   212	            act.Should().NotThrow();
   213	        }
   214	
   215	        [Fact]
   216	        public void UncompressRawPublicKey_BadPrefix_Throws()
   217	        {
   218	            var compressed = new byte[33];
   219	            compressed[0] = 0x05;
   220	            Action act = () => Crypto.UncompressRawPublicKey(compressed);
   221	            act.Should().Throw<ArgumentException>()
   222	               .WithMessage("failed to uncompress raw public key: invalid prefix");
   223	        }
   224	
   225	        [Fact]
   226	        public void UncompressRawPublicKey_WrongLength_Throws()
   227	        {
   228	            var compressed = new byte[32];
   229	            compressed[0] = 0x02;
   230	            Action act = () => Crypto.UncompressRawPublicKey(compressed);
   231	            act.Should().Throw<ArgumentException>()
   232	               .WithMessage("failed to uncompress raw public key: invalid length");
   233	        }
   234	
   235	        // ============================================================
   236	        // GetPublicKey / GenerateP256KeyPair
   237	        // ============================================================
   238	
   239	        [Fact]
   240	        public void GenerateP256KeyPair_LengthsAreCorrect()
   241	        {
   242	            var kp = Crypto.GenerateP256KeyPair();
   243	            kp.PrivateKey.Should().HaveLength(64);          // 32 bytes hex
   244	            kp.PublicKey.Should().HaveLength(66);            // 33 bytes hex
   245	            kp.PublicKeyUncompressed.Should().HaveLength(130); // 65 bytes hex
   246	            kp.PublicKey.Should().MatchRegex("^0[23][0-9a-f]{64}$");
   247	            kp.PublicKeyUncompressed.Should().StartWith("04");
   248	        }
   249	
   250	        [Fact]
   251	        public void GetPublicKey_FromPrivateKey_ProducesCompressedByDefault()
   252	        {
   253	            var kp = Crypto.GenerateP256KeyPair();
   254	            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
   255	            byte[] pub = Crypto.GetPublicKey(priv); // isCompressed default true
   256	            pub.Should().HaveCount(33);
   257	            Encoding.Uint8ArrayToHexString(pub).Should().Be(kp.PublicKey);
   258	        }
   259	
   260	        [Fact]
   261	        public void GetPublicKey_Uncompressed_MatchesGenerator()
   262	        {
   263	            var kp = Crypto.GenerateP256KeyPair();
   264	            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
   265	            byte[] uncompressed = Crypto.GetPublicKey(priv, isCompressed: false);
   266	            Encoding.Uint8ArrayToHexString(uncompressed).Should().Be(kp.PublicKeyUncompressed);
   267	        }
   268	
   269	        [Fact]
   270	        public void GetPublicKey_HexStringOverload_MatchesByteOverload()
   271	        {
   272	            var kp = Crypto.GenerateP256KeyPair();
   273	            byte[] viaBytes = Crypto.GetPublicKey(Encoding.Uint8ArrayFromHexString(kp.PrivateKey));
   274	            byte[] viaHex = Crypto.GetPublicKey(kp.PrivateKey);
   275	            viaBytes.Should().Equal(viaHex);
   276	        }
   277	
   278	        // ============================================================
   279	        // HPKE roundtrip
   280	        // ============================================================
   281	
   282	        [Fact]
   283	        public void Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload()
   284	        {
   285	            // Receiver key pair.
   286	            var recv = Crypto.GenerateP256KeyPair();
   287	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   288	
   289	            byte[] payload = Encoding.Uint8ArrayFromHexString(
   290	                "deadbeefcafebabe1234567890abcdef00112233445566778899aabbccddeeff");
   291	
   292	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   293	            {
   294	                PlainTextBuf = payload,
   295	                TargetKeyBuf = recvPubUncompressed,
   296	            });
   297	
   298	            // encrypted = compressed(senderPub) || ciphertext (33 + N bytes)
   299	            encrypted.Length.Should().BeGreaterThan(33);
   300	
   301	            // Split, uncompress the sender's compressed point, then decrypt.
   302	            var compressedSender = new byte[33];
   303	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   304	            var ciphertext = new byte[encrypted.Length - 33];
   305	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   306	
   307	            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);
   308	
   309	            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   310	            {
   311	                CiphertextBuf = ciphertext,
   312	                EncappedKeyBuf = encapped,
   313	                ReceiverPriv = recv.PrivateKey,
   314	            });
   315	
   316	            decrypted.Should().Equal(payload);
   317	        }
   318	
   319	        [Fact]
   320	        public void Hpke_EncryptThenDecrypt_EmptyPayload()
   321	        {
   322	            var recv = Crypto.GenerateP256KeyPair();
   323	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   324	
   325	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   326	            {
   327	                PlainTextBuf = Array.Empty<byte>(),
   328	                TargetKeyBuf = recvPubUncompressed,
   329	            });
   330	
   331	            var compressedSender = new byte[33];
   332	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   333	            var ciphertext = new byte[encrypted.Length - 33];
   334	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   335	
   336	            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   337	            {
   338	                CiphertextBuf = ciphertext,
   339	                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
   340	                ReceiverPriv = recv.PrivateKey,
   341	            });
   342	            decrypted.Should().BeEmpty();
   343	        }
   344	
   345	        [Fact]
   346	        public void Hpke_DecryptWithWrongKey_Throws()
   347	        {
   348	            var recv = Crypto.GenerateP256KeyPair();
   349	            var attacker = Crypto.GenerateP256KeyPair();
   350	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   351	
   352	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   353	            {
   354	                PlainTextBuf = Encoding.Uint8ArrayFromHexString("aabbcc"),
   355	                TargetKeyBuf = recvPubUncompressed,
   356	            });
   357	
   358	            var compressedSender = new byte[33];
   359	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   360	            var ciphertext = new byte[encrypted.Length - 33];
   361	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   362	
   363	            Action act = () => Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   364	            {
   365	                CiphertextBuf = ciphertext,
   366	                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
   367	                ReceiverPriv = attacker.PrivateKey,
   368	            });
   369	            act.Should().Throw<Exception>();
   370	        }
   371	
   372	        // ============================================================
   373	        // BuildAdditionalAssociatedData / FormatHpkeBuf
   374	        // ============================================================
   375	
   376	        [Fact]
   377	        public void BuildAdditionalAssociatedData_ConcatsArgs()
   378	        {
   379	            byte[] a = { 0x01, 0x02 };
   380	            byte[] b = { 0x03, 0x04, 0x05 };
   381	            byte[] aad = Crypto.BuildAdditionalAssociatedData(a, b);
   382	            aad.Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
   383	        }
   384	
   385	        [Fact]
   386	        public void FormatHpkeBuf_ReturnsExpectedJson()
   387	        {
   388	            // Use a real HpkeEncrypt output so the bytes are valid.
   389	            var recv = Crypto.GenerateP256KeyPair();
   390	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   391	
   392	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   393	            {
   394	                PlainTextBuf = Encoding.Uint8ArrayFromHexString("1234"),
   395	                TargetKeyBuf = recvPubUncompressed,
   396	            });
   397	
   398	            string json = Crypto.FormatHpkeBuf(encrypted);
   399	
   400	            // Shape contains exactly two keys in upstream order.
   401	            json.Should().StartWith("{\"encappedPublic\":\"04");
   402	            json.Should().Contain("\"ciphertext\":\"");
   403	            json.Should().EndWith("\"}");
   404	        }
   405	
   406	        [Fact]
   407	        public void FormatHpkeBuf_TooSmall_Throws()
   408	        {
   409	            // Upstream passes the slice to uncompressRawPublicKey which
   410	            // throws "failed to uncompress raw public key: invalid length"
   411	            // when the buffer is shorter than 33 bytes.
   412	            Action act = () => Crypto.FormatHpkeBuf(new byte[10]);
   413	            act.Should().Throw<ArgumentException>()
   414	               .WithMessage("failed to uncompress raw public key: invalid length");
   415	        }
   416	
   417	        // ============================================================
   418	        // VerifySessionJwtSignature negative paths
   419	        // ============================================================
   420	
   421	        [Fact]
   422	        public void VerifySessionJwtSignature_Empty_Throws()
   423	        {
   424	            // Upstream: throws "invalid JWT: need 3 parts" when signature part is missing.
   425	            Action act = () => Crypto.VerifySessionJwtSignature(string.Empty);
   426	            act.Should().Throw<InvalidOperationException>()
   427	               .WithMessage("invalid JWT: need 3 parts");
   428	        }
   429	
   430	        [Fact]
   431	        public void VerifySessionJwtSignature_WrongPartCount_Throws()
   432	        {
   433	            Action act = () => Crypto.VerifySessionJwtSignature("just.two");
   434	            act.Should().Throw<InvalidOperationException>()
   435	               .WithMessage("invalid JWT: need 3 parts");
   436	        }
   437	
   438	        [Fact]
   439	        public void VerifySessionJwtSignature_BadSignatureLength_ReturnsFalse()
   440	        {
   441	            // 3 parts but the signature decodes to a non-64-byte buffer.
   442	            string jwt = "aGVhZGVy.cGF5bG9hZA.YWJj"; // last part decodes to "abc" (3 bytes)
   443	            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
   444	        }
   445	
   446	        [Fact]
   447	        public void VerifySessionJwtSignature_WellFormedButForgedSig_ReturnsFalse()
   448	        {
   449	            // 64-byte signature of all zeros — should fail verification.
   450	            string header = Encoding.Base64StringToBase64UrlEncodedString(
   451	                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"ES256\"}")));
   452	            string payload = Encoding.Base64StringToBase64UrlEncodedString(
   453	                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"sub\":\"x\"}")));
   454	            string sig = Encoding.Base64StringToBase64UrlEncodedString(
   455	                Convert.ToBase64String(new byte[64]));
   456	            string jwt = header + "." + payload + "." + sig;
   457	            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
   458	        }
   459	
   460	        // ============================================================
   461	        // Curve.Secp256k1 — UncompressRawPublicKey
   462	        // ============================================================
   463	
   464	        [Fact]
   465	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
   466	        {
   467	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
   468	            //                                       029bfcdb 2dce28d9 59f2815b 16f81798
   469	            byte[] compressed = Encoding.Uint8ArrayFromHexString(
   470	                "0279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798");
   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
   472	            uncompressed.Should().HaveCount(65);
   473	            uncompressed[0].Should().Be(0x04);
   474	            // Y coordinate of G must be even (prefix was 0x02).
   475	            (uncompressed[64] & 1).Should().Be(0);
   476	        }
   477	
   478	        // ============================================================
   479	        // Bundle helpers — negative paths only (positive paths require
   480	        // Turnkey-signed sample bundles which we do not have here)
   481	        // ============================================================
   482	
   483	        [Fact]
   484	        public void DecryptCredentialBundle_BundleTooSmall_Throws()
   485	        {
   486	            // Base58Check of a small (<33 byte) payload should still decode (with valid checksum)
   487	            // but trigger the size guard. Use Base58Check encode so DecryptCredentialBundle's
   488	            // bs58check decode succeeds, then it should fail on the size check.
   489	            string tiny = Encoding.Base58CheckEncode(new byte[] { 0x01, 0x02, 0x03 });
   490	            Action act = () => Crypto.DecryptCredentialBundle(tiny, new string('1', 64));
   491	            act.Should().Throw<InvalidOperationException>()
   492	               .WithMessage("*Error decrypting bundle*");
   493	        }
   494	
   495	        [Fact]
   496	        public void DecryptCredentialBundle_RawBase58_Rejected()
   497	        {
   498	            // Upstream uses bs58check.decode exclusively. A plain base58 input
   499	            // (no checksum) must NOT be accepted.
   500	            string rawBs58 = Encoding.Base58Encode(new byte[40]);
   501	            Action act = () => Crypto.DecryptCredentialBundle(rawBs58, new string('1', 64));
   502	            act.Should().Throw<InvalidOperationException>();
   503	        }
   504	
   505	        [Fact]
   506	        public void EncryptPrivateKeyToBundle_MissingArgs_Throws()
   507	        {
   508	            Action act = () => Crypto.EncryptPrivateKeyToBundle(new Crypto.EncryptPrivateKeyToBundleParams());
   509	            act.Should().Throw<ArgumentException>();
   510	        }
   511	
   512	        [Fact]
   513	        public void DecryptExportBundle_MissingArgs_Throws()
   514	        {
   515	            Action act = () => Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams());
   516	            act.Should().Throw<ArgumentException>();
   517	        }
   518	
   519	        // ============================================================
   520	        // Helpers
   521	        // ============================================================
   522	
   523	        private static byte[] HexToBytes(string hex)
   524	        {
   525	            return Encoding.Uint8ArrayFromHexString(hex);
   526	        }
   527	
   528	        private static string BytesToHex(byte[] bytes)
   529	        {
   530	            return Encoding.Uint8ArrayToHexString(bytes);
   531	        }
   532	    }
   533	}

exec
/bin/zsh -lc "nl -ba tests/Fixtures/README.md | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	# Test fixtures
     2	
     3	Each subdirectory groups fixtures for one production file, plus a
     4	provenance note explaining where the bytes came from.
     5	
     6	## Provenance levels
     7	
     8	- **upstream-test-vectors** — directly copied from the upstream
     9	  `__tests__/` directory inside `codex-crypto-reviews/upstream-snapshots/`.
    10	- **rfc** — standardized test vectors (RFC 5869, RFC 9180, etc.).
    11	- **nist** — NIST CAVP P-256 vectors.
    12	- **turnkey-sample** — sample bundles published by Turnkey in their
    13	  developer docs (de-identified).
    14	- **node-generated** — bytes generated by running the pinned upstream Node
    15	  package inside `tests/Fixtures/Generators/`. The generator script,
    16	  Node version, and output hash are recorded so reviewers can re-run.
    17	
    18	## Layout
    19	
    20	```
    21	Fixtures/
    22	├── encoding/
    23	│   └── turnkey-encoding-vectors.json       — upstream-test-vectors
    24	├── crypto/                                  — (added with Crypto.cs port)
    25	├── api-key-stamper/                         — (added with ApiKeyStamper.cs port)
    26	└── http/                                    — (added with Http.cs port)
    27	```
    28	
    29	## Re-generating
    30	
    31	Fixtures sourced from upstream test files are stable as long as
    32	[`codex-crypto-reviews/upstream-snapshots/`](../../codex-crypto-reviews/upstream-snapshots/)
    33	is not bumped. If a pin is bumped, re-extract the upstream `__tests__/`
    34	into the matching fixture file and update its sha256 in
    35	`tarball-checksums.txt` next to the new tarball.

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '1080,1125p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
  1080	            // also accepts the unknown case by falling through to hex parsing
  1081	            // (it logs a warn; this port silently matches the behavior).
  1082	            string normalized = privateKey.StartsWith("0x", StringComparison.Ordinal)
  1083	                ? privateKey.Substring(2)
  1084	                : privateKey;
  1085	            return Encoding.Uint8ArrayFromHexString(normalized);
  1086	        }
  1087	
  1088	        private static string? GetStringOrNull(JsonElement element, string propertyName)
  1089	        {
  1090	            if (element.ValueKind != JsonValueKind.Object)
  1091	            {
  1092	                return null;
  1093	            }
  1094	            if (!element.TryGetProperty(propertyName, out var value))
  1095	            {
  1096	                return null;
  1097	            }
  1098	            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
  1099	            {
  1100	                return null;
  1101	            }
  1102	            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
  1103	        }
  1104	
  1105	        #endregion
  1106	    }
  1107	}

exec
/bin/zsh -lc "nl -ba src/TurnkeyJsonContext.cs | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// IL2CPP-safe System.Text.Json source-generated context.
     2	//
     3	// Every JsonSerializer.Serialize / JsonSerializer.Deserialize call in this SDK
     4	// MUST go through this context's typed overloads (TurnkeyJsonContext.Default.<Type>)
     5	// so that no reflection-based fallback is reached at runtime. The context
     6	// covers every DTO that crosses a wire boundary or that we serialize to JSON.
     7	//
     8	// IL2CPP / AOT safety:
     9	//   - JsonSerializerIsReflectionEnabledByDefault is the user/runtime knob
    10	//     that controls whether fallback to reflection happens. We do not set it
    11	//     here, but every call site we own goes via this context and so never
    12	//     needs reflection.
    13	//
    14	// When adding a new DTO that this SDK serializes:
    15	//   1. Add a [JsonSerializable(typeof(<DTO>))] attribute below.
    16	//   2. Use the resulting TurnkeyJsonContext.Default.<DTO> overload at the
    17	//      call site.
    18	
    19	using System.Text.Json.Serialization;
    20	
    21	namespace Turnkey
    22	{
    23	    [JsonSourceGenerationOptions(
    24	        WriteIndented = false,
    25	        PropertyNameCaseInsensitive = false,
    26	        DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
    27	    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
    28	    public partial class TurnkeyJsonContext : JsonSerializerContext
    29	    {
    30	    }
    31	}

exec
/bin/zsh -lc "rg -n \"Org\\.BouncyCastle\\.Crypto\\.Generators\\.HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|HkdfBytesGenerator|Hpke|Kdf|KDF|Derive|Agreement|GcmBlockCipher|AesEngine|Sha256|HMAC|HMACSHA256|BigInteger|ECNamedCurveTable|SignerUtilities|Ed25519|ECDHBasicAgreement\" src/Crypto.cs src/*.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/TurnkeyJsonContext.cs:27:    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
src/Encoding.cs:542:        // BigInteger. The algorithm is standard and produces wire-identical bytes
src/Encoding.cs:561:            var intData = new BigInteger(1, data);
src/Encoding.cs:563:            var fiftyEight = BigInteger.ValueOf(58);
src/Encoding.cs:565:            while (intData.CompareTo(BigInteger.Zero) > 0)
src/Encoding.cs:596:            var decoded = BigInteger.Zero;
src/Encoding.cs:597:            var multi = BigInteger.One;
src/Encoding.cs:598:            var fiftyEight = BigInteger.ValueOf(58);
src/Encoding.cs:608:                decoded = decoded.Add(multi.Multiply(BigInteger.ValueOf(digit)));
src/Encoding.cs:612:            byte[] bytes = decoded.Equals(BigInteger.Zero)
src/CryptoConstants.cs:36:        /// Used with <c>ECNamedCurveTable.GetByName</c>.
src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:35://   - BouncyCastle 2.5.0 wraps ECDSA / ECDH / AES-GCM / SHA-256 / HMAC /
src/Crypto.cs:36://     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
src/Crypto.cs:43://   the published dist/ is the inlining of QOS_ENCRYPTION_HMAC_MESSAGE
src/Crypto.cs:53:using Org.BouncyCastle.Crypto.Agreement;
src/Crypto.cs:90:            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
src/Crypto.cs:100:            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
src/Crypto.cs:157:            /// BouncyCastle <c>BigInteger.Mod</c> and JS <c>BigInt %</c> agree.
src/Crypto.cs:162:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
src/Crypto.cs:164:                if (p.CompareTo(BigInteger.Zero) <= 0)
src/Crypto.cs:173:                    var q = p.Add(BigInteger.One).ShiftRight(2);
src/Crypto.cs:188:        #region HKDF (port of @noble/hashes/hkdf)
src/Crypto.cs:191:        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
src/Crypto.cs:199:            /// HKDF Extract. RFC 5869 §2.2.
src/Crypto.cs:207:                using (var hmac = new HMACSHA256(salt))
src/Crypto.cs:214:            /// HKDF Expand. RFC 5869 §2.3.
src/Crypto.cs:236:                using (var hmac = new HMACSHA256(prk))
src/Crypto.cs:262:        public class HpkeDecryptParams
src/Crypto.cs:270:        public class HpkeEncryptParams
src/Crypto.cs:323:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:327:        public class HpkeBundlePayload
src/Crypto.cs:341:        /// Derive the SEC1 public key bytes from a private key.
src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:353:            var d = new BigInteger(1, privateKey);
src/Crypto.cs:395:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:402:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
src/Crypto.cs:418:                var ss = DeriveSS(encappedKeyBuf, receiverPriv);
src/Crypto.cs:442:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:448:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
src/Crypto.cs:454:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
src/Crypto.cs:468:                var ss = DeriveSS(targetKeyBuf, Encoding.Uint8ArrayToHexString(senderPrivBuf));
src/Crypto.cs:550:            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);
src/Crypto.cs:552:            BigInteger p, a, b;
src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:561:                p = new BigInteger(
src/Crypto.cs:563:                a = BigInteger.ValueOf(0);
src/Crypto.cs:564:                b = BigInteger.ValueOf(7);
src/Crypto.cs:600:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:602:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:626:            var payload = new HpkeBundlePayload
src/Crypto.cs:631:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:669:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:743:            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
src/Crypto.cs:748:            return FormatHpkeBuf(privateKeyBundle);
src/Crypto.cs:803:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:818:                    // Derive Ed25519 public key from the 32-byte seed via BouncyCastle.
src/Crypto.cs:819:                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
src/Crypto.cs:891:        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:897:            var d = new BigInteger(1, privBytes);
src/Crypto.cs:903:            var agreement = new ECDHBasicAgreement();
src/Crypto.cs:905:            var sharedSecretBig = agreement.CalculateAgreement(publicKeyParams);
src/Crypto.cs:953:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:964:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1000:                var r = new BigInteger(1, signatureRaw, 0, 32);
src/Crypto.cs:1001:                var s = new BigInteger(1, signatureRaw, 32, 32);
src/Crypto.cs:1004:                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1050:            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:35://   - BouncyCastle 2.5.0 wraps ECDSA / ECDH / AES-GCM / SHA-256 / HMAC /
src/Crypto.cs:36://     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
src/Crypto.cs:43://   the published dist/ is the inlining of QOS_ENCRYPTION_HMAC_MESSAGE
src/Crypto.cs:53:using Org.BouncyCastle.Crypto.Agreement;
src/Crypto.cs:90:            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
src/Crypto.cs:100:            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
src/Crypto.cs:157:            /// BouncyCastle <c>BigInteger.Mod</c> and JS <c>BigInt %</c> agree.
src/Crypto.cs:162:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
src/Crypto.cs:164:                if (p.CompareTo(BigInteger.Zero) <= 0)
src/Crypto.cs:173:                    var q = p.Add(BigInteger.One).ShiftRight(2);
src/Crypto.cs:188:        #region HKDF (port of @noble/hashes/hkdf)
src/Crypto.cs:191:        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
src/Crypto.cs:199:            /// HKDF Extract. RFC 5869 §2.2.
src/Crypto.cs:207:                using (var hmac = new HMACSHA256(salt))
src/Crypto.cs:214:            /// HKDF Expand. RFC 5869 §2.3.
src/Crypto.cs:236:                using (var hmac = new HMACSHA256(prk))
src/Crypto.cs:262:        public class HpkeDecryptParams
src/Crypto.cs:270:        public class HpkeEncryptParams
src/Crypto.cs:323:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:327:        public class HpkeBundlePayload
src/Crypto.cs:341:        /// Derive the SEC1 public key bytes from a private key.
src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:353:            var d = new BigInteger(1, privateKey);
src/Crypto.cs:395:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:402:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
src/Crypto.cs:418:                var ss = DeriveSS(encappedKeyBuf, receiverPriv);
src/Crypto.cs:442:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:448:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
src/Crypto.cs:454:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
src/Crypto.cs:468:                var ss = DeriveSS(targetKeyBuf, Encoding.Uint8ArrayToHexString(senderPrivBuf));
src/Crypto.cs:550:            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);
src/Crypto.cs:552:            BigInteger p, a, b;
src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:561:                p = new BigInteger(
src/Crypto.cs:563:                a = BigInteger.ValueOf(0);
src/Crypto.cs:564:                b = BigInteger.ValueOf(7);
src/Crypto.cs:600:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:602:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:626:            var payload = new HpkeBundlePayload
src/Crypto.cs:631:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:669:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:743:            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
src/Crypto.cs:748:            return FormatHpkeBuf(privateKeyBundle);
src/Crypto.cs:803:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:818:                    // Derive Ed25519 public key from the 32-byte seed via BouncyCastle.
src/Crypto.cs:819:                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
src/Crypto.cs:891:        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:897:            var d = new BigInteger(1, privBytes);
src/Crypto.cs:903:            var agreement = new ECDHBasicAgreement();
src/Crypto.cs:905:            var sharedSecretBig = agreement.CalculateAgreement(publicKeyParams);
src/Crypto.cs:953:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:964:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1000:                var r = new BigInteger(1, signatureRaw, 0, 32);
src/Crypto.cs:1001:                var s = new BigInteger(1, signatureRaw, 32, 32);
src/Crypto.cs:1004:                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1050:            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");

exec
/bin/zsh -lc "rg -n \"static .*\\(|public .*\\(|private .*\\(|internal .*\\(\" src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
111:            /// <summary>SEC1 uncompressed P-256 public key length, bytes (0x04 + X + Y).</summary>
162:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
201:            public static byte[] Extract(byte[] salt, byte[] ikm)
216:            public static byte[] Expand(byte[] prk, byte[] info, int length)
344:        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
364:        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
375:        public static KeyPair GenerateP256KeyPair()
402:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
454:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
497:        public static byte[] BuildAdditionalAssociatedData(byte[] senderPubBuf, byte[] receiverPubBuf)
514:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
533:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Curve curve = Curve.P256)
602:        public static string FormatHpkeBuf(byte[] encryptedBuf)
646:        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string embeddedKey)
660:                        + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
688:        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
755:        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
854:        public static bool VerifySessionJwtSignature(
891:        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
917:        private static byte[] GetKemContext(byte[] encappedKeyBuf, string publicKey)
924:        private static byte[] BuildLabeledIkm(byte[] label, byte[] ikm, byte[] suiteId)
929:        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
945:        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
951:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
962:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
973:        private static byte[] Base64UrlDecode(string input)
984:        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
1019:        private static void VerifyEnclaveSignature(
1065:        private static byte[] DecodeKey(string privateKey, string? keyFormat)
1088:        private static string? GetStringOrNull(JsonElement element, string propertyName)

exec
/bin/zsh -lc "find tests/Fixtures -maxdepth 2 -type f -print -exec shasum -a 256 {} \\;" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/encoding/turnkey-encoding-vectors.json
a7b70e45538a66fc2953204ff2c0cc8b32c8d7941560183889f44707f238cc3c  tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/README.md
174ddea6f3982136b7c7ea7c6553a1b125288958f2bb08a98ebfbcc1aa5dde3c  tests/Fixtures/README.md

codex
I’ve got the method surface and the upstream line anchors. Two areas need extra care before I can claim the gate: exact signature verification semantics and whether any fixture files under `tests/Fixtures` actually exercise `Crypto.cs`.
exec
/bin/zsh -lc "rg -n \"Uint8ArrayFromHexString|Base64UrlDecode|HexToAscii|Base58Decode|Base58CheckDecode\" src/Encoding.cs && nl -ba src/Encoding.cs | sed -n '80,150p' && nl -ba src/Encoding.cs | sed -n '500,690p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
7://   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
8://                                HexToAscii / NormalizePadding
15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
118:        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
164:        public static string HexToAscii(string hexString)
203:        /// The sign is preserved per JS semantics; for HexToAscii output the
391:            var buffer = Uint8ArrayFromHexString(hexString, length);
585:        public static byte[] Base58Decode(string encoded)
658:        public static byte[] Base58CheckDecode(string encoded)
660:            byte[] decoded = Base58Decode(encoded);
    80	            RegexOptions.Compiled);
    81	
    82	        // ============================================================
    83	        // hex.ts
    84	        // ============================================================
    85	
    86	        /// <summary>
    87	        /// Converts a byte array into a lower-case hex string.
    88	        /// Upstream: <c>hex.ts uint8ArrayToHexString</c>.
    89	        /// </summary>
    90	        public static string Uint8ArrayToHexString(byte[] input)
    91	        {
    92	            if (input == null)
    93	            {
    94	                throw new ArgumentNullException(nameof(input));
    95	            }
    96	
    97	            // Upstream uses Array.prototype.reduce starting at "". For an empty
    98	            // array the result is "". Match that explicitly.
    99	            if (input.Length == 0)
   100	            {
   101	                return string.Empty;
   102	            }
   103	
   104	            var sb = new StringBuilder(input.Length * 2);
   105	            for (int i = 0; i < input.Length; i++)
   106	            {
   107	                sb.Append(input[i].ToString("x2"));
   108	            }
   109	            return sb.ToString();
   110	        }
   111	
   112	        /// <summary>
   113	        /// Creates a byte array from a hex string.
   114	        /// Upstream: <c>hex.ts uint8ArrayFromHexString</c>.
   115	        /// </summary>
   116	        /// <param name="hexString">Hex string. Must be even-length and contain only [0-9A-Fa-f].</param>
   117	        /// <param name="length">Optional target length. When given, the result is left-padded with leading zero bytes or an exception is thrown if the value does not fit.</param>
   118	        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
   119	        {
   120	            if (string.IsNullOrEmpty(hexString)
   121	                || hexString.Length % 2 != 0
   122	                || !HexRegex.IsMatch(hexString))
   123	            {
   124	                throw new ArgumentException(
   125	                    "cannot create uint8array from invalid hex string: \"" + hexString + "\"");
   126	            }
   127	
   128	            var buffer = new byte[hexString.Length / 2];
   129	            for (int i = 0; i < buffer.Length; i++)
   130	            {
   131	                buffer[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
   132	            }
   133	
   134	            // Upstream uses `if (!length)`, which treats `0` as falsy/omitted.
   135	            // Match that exactly so the wire-level behavior is identical.
   136	            if (!length.HasValue || length.Value == 0)
   137	            {
   138	                return buffer;
   139	            }
   140	
   141	            if (hexString.Length / 2 > length.Value)
   142	            {
   143	                throw new ArgumentException(
   144	                    "hex value cannot fit in a buffer of " + length.Value + " byte(s)");
   145	            }
   146	
   147	            var paddedBuffer = new byte[length.Value];
   148	            // Left-pad: zeros at start, original bytes at the end.
   149	            Array.Copy(buffer, 0, paddedBuffer, length.Value - buffer.Length, buffer.Length);
   150	            return paddedBuffer;
   500	                    sb.Append((char)((buffer >> bits) & 0xff));
   501	                }
   502	            }
   503	            return sb.ToString();
   504	        }
   505	
   506	        // ============================================================
   507	        // encode.ts
   508	        // ============================================================
   509	
   510	        /// <summary>
   511	        /// Compresses an uncompressed P-256 public key into its 33-byte compressed form.
   512	        /// Upstream: <c>encode.ts pointEncode</c>.
   513	        /// </summary>
   514	        /// <param name="raw">65-byte uncompressed key starting with 0x04.</param>
   515	        /// <returns>33-byte compressed key starting with 0x02 or 0x03.</returns>
   516	        public static byte[] PointEncode(byte[] raw)
   517	        {
   518	            if (raw == null)
   519	            {
   520	                throw new ArgumentNullException(nameof(raw));
   521	            }
   522	            if (raw.Length != 65 || raw[0] != 0x04)
   523	            {
   524	                throw new ArgumentException("Invalid uncompressed P-256 key");
   525	            }
   526	
   527	            // x = raw[1..33], y = raw[33..65]; lengths are 32 and 32 by construction.
   528	            byte yLastByte = raw[64];
   529	            byte prefix = (yLastByte & 1) == 0 ? (byte)0x02 : (byte)0x03;
   530	
   531	            var compressed = new byte[33];
   532	            compressed[0] = prefix;
   533	            Array.Copy(raw, 1, compressed, 1, 32);
   534	            return compressed;
   535	        }
   536	
   537	        // ============================================================
   538	        // bs58.ts (and bs58check.ts)
   539	        //
   540	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
   541	        // implements the same Bitcoin base58 algorithm in C# using BouncyCastle's
   542	        // BigInteger. The algorithm is standard and produces wire-identical bytes
   543	        // to the upstream packages for valid input.
   544	        // ============================================================
   545	
   546	        /// <summary>
   547	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
   548	        /// Upstream: <c>bs58.ts bs58.encode</c>.
   549	        /// </summary>
   550	        public static string Base58Encode(byte[] data)
   551	        {
   552	            if (data == null)
   553	            {
   554	                throw new ArgumentNullException(nameof(data));
   555	            }
   556	            if (data.Length == 0)
   557	            {
   558	                return string.Empty;
   559	            }
   560	
   561	            var intData = new BigInteger(1, data);
   562	            var sb = new StringBuilder();
   563	            var fiftyEight = BigInteger.ValueOf(58);
   564	
   565	            while (intData.CompareTo(BigInteger.Zero) > 0)
   566	            {
   567	                var divmod = intData.DivideAndRemainder(fiftyEight);
   568	                intData = divmod[0];
   569	                sb.Insert(0, Constants.BASE58_ALPHABET[divmod[1].IntValue]);
   570	            }
   571	
   572	            // Preserve leading zero bytes as '1' characters (Base58 convention).
   573	            for (int i = 0; i < data.Length && data[i] == 0; i++)
   574	            {
   575	                sb.Insert(0, '1');
   576	            }
   577	
   578	            return sb.ToString();
   579	        }
   580	
   581	        /// <summary>
   582	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
   583	        /// Upstream: <c>bs58.ts bs58.decode</c>.
   584	        /// </summary>
   585	        public static byte[] Base58Decode(string encoded)
   586	        {
   587	            if (encoded == null)
   588	            {
   589	                throw new ArgumentNullException(nameof(encoded));
   590	            }
   591	            if (encoded.Length == 0)
   592	            {
   593	                return new byte[0];
   594	            }
   595	
   596	            var decoded = BigInteger.Zero;
   597	            var multi = BigInteger.One;
   598	            var fiftyEight = BigInteger.ValueOf(58);
   599	
   600	            for (int i = encoded.Length - 1; i >= 0; i--)
   601	            {
   602	                int digit = Constants.BASE58_ALPHABET.IndexOf(encoded[i]);
   603	                if (digit < 0)
   604	                {
   605	                    throw new ArgumentException(
   606	                        "Invalid character '" + encoded[i] + "' in base58 string");
   607	                }
   608	                decoded = decoded.Add(multi.Multiply(BigInteger.ValueOf(digit)));
   609	                multi = multi.Multiply(fiftyEight);
   610	            }
   611	
   612	            byte[] bytes = decoded.Equals(BigInteger.Zero)
   613	                ? new byte[0]
   614	                : decoded.ToByteArrayUnsigned();
   615	
   616	            int leadingZeros = 0;
   617	            for (int i = 0; i < encoded.Length && encoded[i] == '1'; i++)
   618	            {
   619	                leadingZeros++;
   620	            }
   621	            if (leadingZeros > 0)
   622	            {
   623	                var result = new byte[leadingZeros + bytes.Length];
   624	                Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);
   625	                return result;
   626	            }
   627	            return bytes;
   628	        }
   629	
   630	        /// <summary>
   631	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
   632	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
   633	        /// </summary>
   634	        public static string Base58CheckEncode(byte[] payload)
   635	        {
   636	            if (payload == null)
   637	            {
   638	                throw new ArgumentNullException(nameof(payload));
   639	            }
   640	            using (var sha = SHA256.Create())
   641	            {
   642	                var h1 = sha.ComputeHash(payload);
   643	                var h2 = sha.ComputeHash(h1);
   644	                var checksum = new byte[4];
   645	                Array.Copy(h2, 0, checksum, 0, 4);
   646	                var concat = new byte[payload.Length + 4];
   647	                Array.Copy(payload, 0, concat, 0, payload.Length);
   648	                Array.Copy(checksum, 0, concat, payload.Length, 4);
   649	                return Base58Encode(concat);
   650	            }
   651	        }
   652	
   653	        /// <summary>
   654	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
   655	        /// SHA256(SHA256(payload)) checksum.
   656	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
   657	        /// </summary>
   658	        public static byte[] Base58CheckDecode(string encoded)
   659	        {
   660	            byte[] decoded = Base58Decode(encoded);
   661	            if (decoded.Length < 4)
   662	            {
   663	                throw new ArgumentException("Invalid Base58Check string - too short");
   664	            }
   665	
   666	            var data = new byte[decoded.Length - 4];
   667	            Array.Copy(decoded, 0, data, 0, data.Length);
   668	            var checksum = new byte[4];
   669	            Array.Copy(decoded, decoded.Length - 4, checksum, 0, 4);
   670	
   671	            using (var sha = SHA256.Create())
   672	            {
   673	                var h1 = sha.ComputeHash(data);
   674	                var h2 = sha.ComputeHash(h1);
   675	                if (h2[0] != checksum[0] || h2[1] != checksum[1]
   676	                    || h2[2] != checksum[2] || h2[3] != checksum[3])
   677	                {
   678	                    throw new ArgumentException("Invalid Base58Check checksum");
   679	                }
   680	            }
   681	
   682	            return data;
   683	        }
   684	
   685	        // ============================================================
   686	        // Unity-port-derived helpers (NOT in @turnkey/encoding)
   687	        //
   688	        // The upstream library reaches for these patterns inline (TextDecoder,
   689	        // spread). The C# port keeps them here as public helpers because
   690	        // Crypto.cs / ApiKeyStamper.cs / Http.cs use them.

exec
/bin/zsh -lc 'rg -n "HpkeBundlePayload|DefaultIgnoreCondition|JsonSerializerOptions|PropertyNamingPolicy|JsonPropertyOrder" -S src tests' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/Crypto.cs:327:        public class HpkeBundlePayload
src/Crypto.cs:626:            var payload = new HpkeBundlePayload
src/Crypto.cs:631:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/TurnkeyJsonContext.cs:26:        DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
src/TurnkeyJsonContext.cs:27:    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]

exec
/bin/zsh -lc "rg -n \"CryptoTests|Crypto\\.\" tests/Fixtures tests -g '"'!CryptoTests.cs'"' -S" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md:24:├── crypto/                                  — (added with Crypto.cs port)
tests/Fixtures/README.md:24:├── crypto/                                  — (added with Crypto.cs port)

exec
/bin/zsh -lc 'find tests -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/CryptoTests.cs
tests/EncodingTests.cs
tests/Fixtures/README.md
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/obj/project.assets.json
tests/obj/project.nuget.cache
tests/obj/turnkey-sdk-csharp.Tests.csproj.nuget.dgspec.json
tests/obj/turnkey-sdk-csharp.Tests.csproj.nuget.g.props
tests/obj/turnkey-sdk-csharp.Tests.csproj.nuget.g.targets
tests/packages.lock.json
tests/turnkey-sdk-csharp.Tests.csproj

exec
/bin/zsh -lc "git status --short && git log -n 5 --format='%H %ad %s' -- src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 2974ms:
2026-05-23 09:07:59.601 xcodebuild[78079:106020648]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:07:59.821 xcodebuild[78079:106020647] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-C8F48QNt' (errno=Operation not permitted)
2026-05-23 09:08:00.227 xcodebuild[78081:106020663]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:00.338 xcodebuild[78081:106020662] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/Crypto.cs-r2-20260523.md
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-YbILycd4' (errno=Operation not permitted)
2026-05-23 09:08:01.124 xcodebuild[78085:106020691]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:01.235 xcodebuild[78085:106020690] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-2rS1wSfw' (errno=Operation not permitted)
2026-05-23 09:08:01.568 xcodebuild[78087:106020702]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:01.678 xcodebuild[78087:106020701] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
73e5109466262896c3383d9c00d8788309eacf21 Sat May 23 09:06:33 2026 +0900 Fix Crypto.cs divergences flagged by Codex r1
388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/Crypto.cs-r2-20260523.md | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	# Codex review — src/Crypto.cs — round 2 — 20260523
     2	
     3	C# SHA: `73e5109466262896c3383d9c00d8788309eacf21`
     4	Upstream snapshot: `turnkey-crypto-2.8.8`
     5	
     6	---
     7	
     8	2026-05-23T00:06:35.819952Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
     9	OpenAI Codex v0.129.0 (research preview)
    10	--------
    11	workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
    12	model: gpt-5.5
    13	provider: openai
    14	approval: never
    15	sandbox: read-only
    16	reasoning effort: high
    17	reasoning summaries: none
    18	session id: 019e5227-bfc8-76a1-8395-eeca3e67dec3
    19	--------
    20	user
    21	You are reviewing a C# port of the Turnkey TypeScript SDK.
    22	File under review (C#): src/Crypto.cs  (git SHA at last commit touching it: 73e5109466262896c3383d9c00d8788309eacf21)
    23	Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
    24	Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
    25	
    26	This is REVIEW ROUND 2 of 3 for this file.
    27	
    28	REQUIRED OUTPUTS (sections A through G must all appear):
    29	
    30	A. Source pin acknowledgement:
    31	   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
    32	   C# file git SHA (73e5109466262896c3383d9c00d8788309eacf21).
    33	
    34	B. Method coverage table: every public + internal helper method in src/Crypto.cs
    35	   listed in a markdown table:
    36	     | C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
    37	   Status: REVIEWED or NOT-REVIEWED. If NOT-REVIEWED, give a reason. Do not skip rows.
    38	
    39	C. Intentional adaptations: list every C#/TS adaptation pattern with a one-line
    40	   justification that it is structural (no wire-byte or observable-behavior change).
    41	
    42	D. (Crypto.cs only) D17 enforcement check. Confirm BouncyCastle is used only for
    43	   ECDSA / ECDH / AES-GCM / SHA-256 / HMAC / BigInteger / EC point primitives.
    44	   Banned APIs (must not appear):
    45	     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
    46	     - Org.BouncyCastle.Crypto.Hpke.*
    47	     - Any "high-level" KDF or HPKE wrapper
    48	   If file is not Crypto.cs, write "N/A (not Crypto.cs)".
    49	
    50	E. Logic divergence findings: every place C# behavior differs from upstream TS:
    51	   algorithm step order, constants, error handling, byte ordering, leading-zero
    52	   handling, padding, rounding/normalization, signature format (DER vs raw r||s,
    53	   low-S), DTO shape (field names, order, presence, optionality), JSON
    54	   serialization (property order, casing, null handling, escaping).
    55	   For each item: C# file:line, TS upstream path:line, semantic diff,
    56	   suggested fix.
    57	
    58	F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
    59	   this file, confirm it was generated from the pinned upstream package and the
    60	   C# test asserts the same bytes Node would produce.
    61	
    62	G. Unresolved assumptions you could not verify in this round.
    63	
    64	PASS criterion for this round: B has zero NOT-REVIEWED rows (or each is
    65	documented), D is "no banned API present" (Crypto.cs only) or N/A, E has zero
    66	entries, F is "all fixtures match".
    67	
    68	DO NOT use "looks good" or "no divergence found" without producing all sections.
    69	
    70	2026-05-23T00:06:37.601630Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
    71	codex
    72	I’ll use the `codex-review` skill for the review shape, then pin the upstream snapshot and walk `src/Crypto.cs` method-by-method against the TypeScript source. I’m also doing a quick memory lookup because this repo family has prior C# SDK review context.
    73	exec
    74	/bin/zsh -lc "sed -n '1,220p' /Users/takeshi/.agents/skills/codex-review/SKILL.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
    75	 succeeded in 0ms:
    76	---
    77	name: codex-review
    78	version: 2.1.4
    79	author: BenedictKing
    80	description: "Professional code review skill for Claude Code. Automatically collects file changes and task status. Triggers when working directory has uncommitted changes, or reviews latest commit when clean. Triggers: code review, review, 代码审核, 代码审查, 检查代码"
    81	allowed-tools:
    82	  - Task
    83	  - Bash
    84	  - Read
    85	  - Glob
    86	  - Write
    87	  - Edit
    88	user-invocable: true
    89	---
    90	
    91	# Codex Code Review Skill
    92	
    93	## Trigger Conditions
    94	
    95	Triggered when user input contains:
    96	
    97	- "代码审核", "代码审查", "审查代码", "审核代码"
    98	- "review", "code review", "review code", "codex 审核"
    99	- "帮我审核", "检查代码", "审一下", "看看代码"
   100	
   101	## Core Concept: Intention vs Implementation
   102	
   103	Running `codex review --uncommitted` alone only shows AI "what was done (Implementation)".
   104	Recording intention first tells AI "what you wanted to do (Intention)".
   105	
   106	**"Code changes + intention description" as combined input is the most effective way to improve AI code review quality.**
   107	
   108	## Skill Architecture
   109	
   110	This skill operates in two phases:
   111	
   112	1. **Preparation Phase** (current context): Check working directory, update CHANGELOG
   113	2. **Review Phase** (isolated context): Invoke Task tool to execute Lint + codex review (using context: fork to reduce context waste)
   114	
   115	## Execution Steps
   116	
   117	### 0. [First] Check Working Directory Status
   118	
   119	```bash
   120	git diff --name-only && git status --short
   121	```
   122	
   123	**Decide review mode based on output:**
   124	
   125	- **Has uncommitted changes** → Continue with steps 1-4 (normal flow)
   126	- **Clean working directory** → Directly invoke codex-runner: `codex review --commit HEAD`
   127	
   128	### 1. [Mandatory] Check if CHANGELOG is Updated
   129	
   130	**Before any review, must check if CHANGELOG.md contains description of current changes.**
   131	
   132	```bash
   133	# Check if CHANGELOG.md is in uncommitted changes
   134	git diff --name-only | grep -E "(CHANGELOG|changelog)"
   135	```
   136	
   137	**If CHANGELOG is not updated, you must automatically perform the following (don't ask user to do it manually):**
   138	
   139	1. **Analyze changes**: Run `git diff --stat` and `git diff` to get complete changes
   140	2. **Auto-generate CHANGELOG entry**: Generate compliant entry based on code changes
   141	3. **Write to CHANGELOG.md**: Use Edit tool to insert entry at top of `[Unreleased]` section
   142	4. **Continue review flow**: Immediately proceed to next steps after CHANGELOG update
   143	
   144	**Auto-generated CHANGELOG entry format:**
   145	
   146	```markdown
   147	## [Unreleased]
   148	
   149	### Added / Changed / Fixed
   150	
   151	- Feature description: what problem was solved or what functionality was implemented
   152	- Affected files: main modified files/modules
   153	```
   154	
   155	**Example - Auto-generation Flow:**
   156	
   157	```
   158	1. Detected CHANGELOG not updated
   159	2. Run git diff --stat, found handlers/responses.go modified (+88 lines)
   160	3. Run git diff to analyze details: added CompactHandler function
   161	4. Auto-generate entry:
   162	   ### Added
   163	   - Added `/v1/responses/compact` endpoint for conversation context compression
   164	   - Supports multi-channel failover and request body size limits
   165	5. Use Edit tool to write to CHANGELOG.md
   166	6. Continue with lint and codex review
   167	```
   168	
   169	### 2. [Critical] Stage All New Files
   170	
   171	**Before invoking codex review, must add all new files (untracked files) to git staging area, otherwise codex will report P1 error.**
   172	
   173	```bash
   174	# Check for new files
   175	git status --short | grep "^??"
   176	```
   177	
   178	**If there are new files, automatically execute:**
   179	
   180	```bash
   181	# Safely stage all new files (handles empty list and special filenames)
   182	git ls-files --others --exclude-standard -z | while IFS= read -r -d '' f; do git add -- "$f"; done
   183	```
   184	
   185	**Explanation:**
   186	
   187	- `-z` uses null character to separate filenames, correctly handles filenames with spaces/newlines
   188	- `while IFS= read -r -d ''` reads filenames one by one
   189	- `git add -- "$f"` uses `--` separator, correctly handles filenames starting with `-`
   190	- When no new files exist, loop body doesn't execute, safely skipped
   191	- This won't stage modified files, only handles new files
   192	- codex needs files to be tracked by git for proper review
   193	
   194	### 3. Evaluate Task Difficulty and Invoke codex-runner
   195	
   196	**Count change scale:**
   197	
   198	```bash
   199	# Count number of changed files and lines of code
   200	git diff --stat | tail -1
   201	```
   202	
   203	**Difficulty Assessment Criteria:**
   204	
   205	**Difficult Tasks** (meets any condition):
   206	
   207	- Modified files ≥ 10
   208	- Total code changes (insertions + deletions) ≥ 500 lines
   209	- Single metric: insertions ≥ 300 lines OR deletions ≥ 300 lines
   210	- Involves core architecture/algorithm changes
   211	- Cross-module refactoring
   212	- Config: `model_reasoning_effort=xhigh`, timeout 30 minutes
   213	
   214	**Normal Tasks** (other cases):
   215	
   216	- Config: `model_reasoning_effort=high`, timeout 10 minutes
   217	
   218	**Evaluation Method:**
   219	
   220	You MUST parse the `git diff --stat` output correctly to determine difficulty:
   221	
   222	```bash
   223	# Get the summary line (last line of git diff --stat)
   224	git diff --stat | tail -1
   225	# Example outputs:
   226	# "20 files changed, 342 insertions(+), 985 deletions(-)"
   227	# "1 file changed, 50 insertions(+)"  # No deletions
   228	# "3 files changed, 120 deletions(-)"  # No insertions
   229	```
   230	
   231	**Parsing Rules:**
   232	1. Extract file count from "X file(s) changed" (handle both "1 file" and "N files")
   233	2. Extract insertions from "Y insertion(s)(+)" if present (handle both "1 insertion" and "N insertions"), otherwise 0
   234	3. Extract deletions from "Z deletion(s)(-)" if present (handle both "1 deletion" and "N deletions"), otherwise 0
   235	4. Calculate total changes = insertions + deletions
   236	
   237	**Important Edge Cases:**
   238	- Single file: `"1 file changed"` (singular form)
   239	- No insertions: Git omits `"insertions(+)"` entirely → treat as 0
   240	- No deletions: Git omits `"deletions(-)"` entirely → treat as 0
   241	- Pure rename: May show `"0 insertions(+), 0 deletions(-)"` or omit both
   242	
   243	**Decision Logic (ANY condition triggers xhigh):**
   244	- IF file_count >= 10 → xhigh
   245	- IF total_changes >= 500 → xhigh
   246	- IF insertions >= 300 → xhigh
   247	- IF deletions >= 300 → xhigh
   248	- ELSE → high
   249	
   250	**Example Cases:**
   251	- ✅ "20 files changed, 342 insertions(+), 985 deletions(-)" → xhigh (files=20≥10, total=1327≥500, deletions=985≥300)
   252	- ✅ "5 files changed, 600 insertions(+), 50 deletions(-)" → xhigh (total=650≥500, insertions=600≥300)
   253	- ✅ "12 files changed, 100 insertions(+), 50 deletions(-)" → xhigh (files=12≥10)
   254	- ✅ "1 file changed, 400 deletions(-)" → xhigh (deletions=400≥300)
   255	- ❌ "3 files changed, 150 insertions(+), 80 deletions(-)" → high (all conditions fail)
   256	- ❌ "1 file changed, 50 insertions(+)" → high (no deletions, total=50<500)
   257	
   258	**Invoke codex-runner Subtask:**
   259	
   260	Use Task tool to invoke codex-runner, passing complete command (including Lint + codex review):

exec
/bin/zsh -lc "find codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package -path '*node_modules*' -prune -o -type f -name '*.js' -print | head -20 && rg -n \"lowS|verify\\(|Signature\" codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist -S" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts:13:export declare function verify(appProof: v1AppProof, bootProof: v1BootProof): Promise<void>;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts:17:export declare function verifyAppProofSignature(appProof: v1AppProof): void;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:74: * await verifyStampSignature(publicKey, signature, stampedRequest.body)
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:76:export declare const verifyStampSignature: (publicKey: string, signature: string, signedData: string) => Promise<boolean>;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:118:export declare const verifySessionJwtSignature: (jwt: string, dangerouslyOverrideNotarizerPublicKey?: string) => Promise<boolean>;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:2:export { Enclave, decryptCredentialBundle, decryptExportBundle, encryptOauth2ClientSecret, encryptOnRampSecret, encryptPrivateKeyToBundle, encryptToEnclave, encryptWalletToBundle, verifySessionJwtSignature, verifyStampSignature } from './turnkey.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:3:export { getCryptoInstance, verify, verifyAppProofSignature, verifyCertificateChain, verifyCoseSign1Sig } from './proof.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:68:async function verify(appProof, bootProof) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:70:    verifyAppProofSignature(appProof);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:103:function verifyAppProofSignature(appProof) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:147:        throw new Error("Signature verification failed");
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:183:                const ok = await cert.verify({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:199:                const ok = await cert.verify({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:205:                    throw new Error(`Signature check failed: ${cert.subject} not signed by ${issuer?.subject}`);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:217:        "Signature1",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:225:    const ok = await cryptoInstance.subtle.verify({ name: "ECDSA", hash: { name: "SHA-384" } }, pubKey, new Uint8Array(signature), tbs);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:241:exports.verifyAppProofSignature = verifyAppProofSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:66:        const verified = await verifyEnclaveSignature(parsedExportBundle.enclaveQuorumPublic, parsedExportBundle.dataSignature, parsedExportBundle.data, dangerouslyOverrideSignerPublicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:120: * await verifyStampSignature(publicKey, signature, stampedRequest.body)
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:122:const verifyStampSignature = async (publicKey, signature, signedData) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:129:    const publicSignatureBuf = fromDerSignature(signature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:138: * @param {string} publicSignature - The ECDSA signature in DER format.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:143:const verifyEnclaveSignature = async (enclaveQuorumPublic, publicSignature, signedData, dangerouslyOverrideSignerPublicKey) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:154:    const publicSignatureBuf = fromDerSignature(publicSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:207:    const verified = await verifyEnclaveSignature(parsedImportBundle.enclaveQuorumPublic, parsedImportBundle.dataSignature, parsedImportBundle.data, dangerouslyOverrideSignerPublicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:238:    const verified = await verifyEnclaveSignature(parsedImportBundle.enclaveQuorumPublic, parsedImportBundle.dataSignature, parsedImportBundle.data, dangerouslyOverrideSignerPublicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:282:const verifySessionJwtSignature = async (jwt, dangerouslyOverrideNotarizerPublicKey) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:353:export { Enclave, decryptCredentialBundle, decryptExportBundle, encryptOauth2ClientSecret, encryptOnRampSecret, encryptPrivateKeyToBundle, encryptToEnclave, encryptWalletToBundle, verifySessionJwtSignature, verifyStampSignature };
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":";;;;;;;;;AAAA;AACA;AAsDYA;AAAZ,CAAA,UAAY,OAAO,EAAA;AACjB,IAAA,OAAA,CAAA,WAAA,CAAA,GAAA,WAAuB;AACvB,IAAA,OAAA,CAAA,QAAA,CAAA,GAAA,QAAiB;AACjB,IAAA,OAAA,CAAA,YAAA,CAAA,GAAA,YAAyB;AACzB,IAAA,OAAA,CAAA,aAAA,CAAA,GAAA,aAA2B;AAC3B,IAAA,OAAA,CAAA,KAAA,CAAA,GAAA,KAAW;AACb,CAAC,EANWA,eAAO,KAAPA,eAAO,GAAA,EAAA,CAAA,CAAA;AAQnB;;;;;;;AAOG;MACU,uBAAuB,GAAG,CACrC,gBAAwB,EACxB,WAAmB,KACT;AACV,IAAA,IAAI;QACF,MAAM,WAAW,GAAGC,kBAAS,CAAC,MAAM,CAAC,gBAAgB,CAAC;AACtD,QAAA,IAAI,WAAW,CAAC,UAAU,IAAI,EAAE,EAAE;YAChC,MAAM,IAAI,KAAK,CACb,CAAA,YAAA,EAAe,WAAW,CAAC,UAAU,CAAA,sFAAA,CAAwF,CAC9H;QACH;QAEA,MAAM,wBAAwB,GAAG,WAAW,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;QACzD,MAAM,aAAa,GAAG,WAAW,CAAC,KAAK,CAAC,EAAE,CAAC;AAC3C,QAAA,MAAM,cAAc,GAAGC,6BAAsB,CAAC,wBAAwB,CAAC;QACvE,MAAM,aAAa,GAAGC,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,OAAOC,8BAAqB,CAAC,aAAa,CAAC;IAC7C;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,+BAA+B,KAAK,CAAA,CAAE,CAAC;IACzD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;MACU,mBAAmB,GAAG,OAAO,EACxC,YAAY,EACZ,WAAW,EACX,cAAc,EACd,kCAAkC,EAClC,SAAS,EACT,cAAc,GACY,KAAqB;AAC/C,IAAA,IAAI;QACF,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;AACnD,QAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;QACD,IAAI,CAAC,QAAQ,EAAE;AACb,YAAA,MAAM,IAAI,KAAK,CACb,uCAAuC,kBAAkB,CAAA,CAAE,CAC5D;QACH;QAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CACtBC,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CACjD,CACF;QAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,YAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;YACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;QACH;AACA,QAAA,IAAI,CAAC,UAAU,CAAC,cAAc,EAAE;AAC9B,YAAA,MAAM,IAAI,KAAK,CAAC,gDAAgD,CAAC;QACnE;QAEA,MAAM,cAAc,GAAGA,gCAAuB,CAAC,UAAU,CAAC,cAAc,CAAC;QACzE,MAAM,aAAa,GAAGA,gCAAuB,CAAC,UAAU,CAAC,UAAU,CAAC;QACpE,MAAM,aAAa,GAAGF,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,IAAI,SAAS,KAAK,QAAQ,IAAI,CAAC,cAAc,EAAE;AAC7C,YAAA,IAAI,aAAa,CAAC,MAAM,KAAK,EAAE,EAAE;gBAC/B,MAAM,IAAI,KAAK,CACb,CAAA,mDAAA,EAAsD,aAAa,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;YACA,MAAM,cAAc,GAAGG,eAAO,CAAC,YAAY,CAAC,aAAa,CAAC;AAC1D,YAAA,IAAI,cAAc,CAAC,MAAM,KAAK,EAAE,EAAE;gBAChC,MAAM,IAAI,KAAK,CACb,CAAA,kDAAA,EAAqD,cAAc,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;AACA,YAAA,MAAM,iBAAiB,GAAG,IAAI,UAAU,CAAC,EAAE,CAAC;AAC5C,YAAA,iBAAiB,CAAC,GAAG,CAAC,aAAa,EAAE,CAAC,CAAC;AACvC,YAAA,iBAAiB,CAAC,GAAG,CAAC,cAAc,EAAE,EAAE,CAAC;AACzC,YAAA,OAAOC,aAAI,CAAC,MAAM,CAAC,iBAAiB,CAAC;QACvC;AAEA,QAAA,MAAM,gBAAgB,GAAGH,8BAAqB,CAAC,aAAa,CAAC;AAC7D,QAAA,OAAO,cAAc,GAAGI,mBAAU,CAAC,gBAAgB,CAAC,GAAG,gBAAgB;IACzE;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,4BAA4B,KAAK,CAAA,CAAE,CAAC;IACtD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;AACI,MAAM,oBAAoB,GAAG,OAClC,SAAiB,EACjB,SAAiB,EACjB,UAAkB,KACE;AACpB,IAAA,MAAM,eAAe,GAAGH,gCAAuB,CAAC,SAAS,CAAC;AAC1D,IAAA,MAAM,eAAe,GAAG,aAAa,CAAC,eAAe,CAAC;IACtD,IAAI,CAAC,eAAe,EAAE;AACpB,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,SAAS,CAAC;IACtD,MAAM,aAAa,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,UAAU,CAAC;AAC1D,IAAA,MAAM,UAAU,GAAGC,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,eAAe,CAAC,KAAK,EAAE,CAAC;AAC7E;AAEA;;;;;;;;AAQG;AACH,MAAM,sBAAsB,GAAG,OAC7B,mBAA2B,EAC3B,eAAuB,EACvB,UAAkB,EAClB,kCAA2C,KACvB;AACpB,IAAA,MAAM,uBAAuB,GAC3B,kCAAkC,IAAIC,2CAAiC;AACzE,IAAA,IAAI,mBAAmB,IAAI,uBAAuB,EAAE;QAClD,MAAM,IAAI,KAAK,CACb,CAAA,oBAAA,EACE,kCAAkC,IAAIA,2CACxC,CAAA,wCAAA,EAA2C,mBAAmB,CAAA,CAAE,CACjE;IACH;IAEA,MAAM,yBAAyB,GAAG,IAAI,UAAU,CAC9CP,gCAAuB,CAAC,mBAAmB,CAAC,CAC7C;AACD,IAAA,MAAM,SAAS,GAAG,aAAa,CAAC,yBAAyB,CAAC;IAC1D,IAAI,CAAC,SAAS,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,eAAe,CAAC;AAC5D,IAAA,MAAM,aAAa,GAAGJ,gCAAuB,CAAC,UAAU,CAAC;AACzD,IAAA,MAAM,UAAU,GAAGK,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,SAAS,CAAC,KAAK,EAAE,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,aAAa,GAAG,CAAC,SAAqB,KAA2B;IACrE,OAAOA,SAAI,CAAC,eAAe,CAAC,OAAO,CAACP,8BAAqB,CAAC,SAAS,CAAC,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,SAAS,GAAG,CAAC,UAAkB,EAAE,SAAc,KAAgB;IACnE,QAAQ,SAAS;AACf,QAAA,KAAK,QAAQ;YACX,MAAM,eAAe,GAAGG,aAAI,CAAC,MAAM,CAAC,UAAU,CAAC;AAC/C,YAAA,IAAI,eAAe,CAAC,MAAM,KAAK,EAAE,EAAE;gBACjC,MAAM,IAAI,KAAK,CACb,CAAA,2CAAA,EAA8C,eAAe,CAAC,MAAM,CAAA,CAAA,CAAG,CACxE;YACH;YACA,OAAO,eAAe,CAAC,QAAQ,CAAC,CAAC,EAAE,EAAE,CAAC;AACxC,QAAA,KAAK,aAAa;AAChB,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOF,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;AAC5C,QAAA;AACE,YAAA,OAAO,CAAC,IAAI,CACV,uBAAuB,SAAS,CAAA,4BAAA,CAA8B,CAC/D;AACD,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOA,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;;AAEhD,CAAC;AAED;;;;;;AAMG;MACU,yBAAyB,GAAG,OAAO,EAC9C,UAAU,EACV,SAAS,EACT,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACF,KAAqB;IACrD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,SAAS,CAAC,UAAU,EAAE,SAAS,CAAC;AACrD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACA,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;AAOK;AACE,MAAM,qBAAqB,GAAG,OAAO,EAC1C,QAAQ,EACR,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACN,KAAqB;IACjD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,QAAQ,CAAC;AACvD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACT,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;;;;;;;;;;;;;;;;;AAuBG;AACI,MAAM,yBAAyB,GAAG,OACvC,GAAW,EACX,qCAA8C,KAC1B;IACpB,MAAM,eAAe,GACnB,qCAAqC;AACrC,QAAAC,8CAAoC;;AAGtC,IAAA,MAAM,CAAC,SAAS,EAAE,UAAU,EAAE,YAAY,CAAC,GAAG,GAAG,CAAC,KAAK,CAAC,GAAG,CAAC;AAC5D,IAAA,IAAI,CAAC,YAAY;AAAE,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;AAC/D,IAAA,MAAM,YAAY,GAAG,CAAA,EAAG,SAAS,CAAA,CAAA,EAAI,UAAU,EAAE;;AAGjD,IAAA,MAAM,EAAE,GAAGL,aAAM,CAAC,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,YAAY,CAAC,CAAC;IACzD,MAAM,SAAS,GAAGA,aAAM,CAAC,EAAE,CAAC,CAAC;;IAG7B,MAAM,KAAK,GAAG,CAAC,CAAS,KACtB,CAAC,CAAC,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,EAAE,MAAM,CAClD,CAAC,CAAC,MAAM,IAAI,CAAC,CAAC,IAAI,CAAC,CAAC,MAAM,GAAG,CAAC,CAAC,IAAI,CAAC,CAAC,EACrC,GAAG,CACJ;AACH,IAAA,MAAM,SAAS,GAAG,UAAU,CAAC,IAAI,CAC/B,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;SACrB,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B,CAAC;;AAGF,IAAA,MAAM,SAAS,GAAGL,gCAAuB,CAAC,eAAe,CAAC;;IAG1D,OAAOM,SAAI,CAAC,MAAM,CAAC,SAAS,EAAE,SAAS,EAAE,SAAS,CAAC;AACrD;AAEA;;;;;;;;;;AAUG;AACI,MAAM,gBAAgB,GAAG,OAC9B,2BAAmC,EACnC,OAAe,KACQ;AACvB,IAAA,OAAO,MAAMK,uBAAgB,CAC3BX,gCAAuB,CAAC,2BAA2B,CAAC,EACpD,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,OAAO,CAAC,CAClC;AACH;AAEA;;;;;;;;;;;AAWG;AACI,MAAM,yBAAyB,GAAG,OACvC,aAAqB,EACrB,sCAA+C,KAC5B;AACnB,IAAA,OAAOD,8BAAqB,CAC1B,MAAM,gBAAgB,CACpB,sCAAsC;AACpC,QAAAa,mDAAyC,EAC3C,aAAa,CACd,CACF;AACH;AAEA;;;;;;;;;;;AAWG;MACU,mBAAmB,GAAG,CACjC,MAAc,EACd,4CAAqD,KAC3C;AACV,IAAA,OAAOhB,kBAAS,CAAC,MAAM,CACrBY,kBAAW,CAAC;QACV,YAAY,EAAE,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,MAAM,CAAC;AAC9C,QAAA,YAAY,EAAEX,6BAAsB,CAClCG,gCAAuB,CACrB,4CAA4C;AAC1C,YAAAa,8DAAoD,CACvD,CACF;AACF,KAAA,CAAC,CACH;AACH;;;;;;;;;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:429: * @param {string} derSignature - The DER-encoded signature.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:432:const fromDerSignature = (derSignature) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:433:    const derSignatureBuf = encoding.uint8ArrayFromHexString(derSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:435:    if (derSignatureBuf.length < 2) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:439:    if (derSignatureBuf[0] !== 0x30) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:444:    const lengthByte = derSignatureBuf[index];
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:449:        if (derSignatureBuf.length < 1 + 1 + lengthByte) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:467:    if (derSignatureBuf[index] !== 0x02) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:471:    const rLength = derSignatureBuf[index];
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:477:    const r = derSignatureBuf.slice(index, index + rLength);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:480:    if (derSignatureBuf[index] !== 0x02) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:484:    const sLength = derSignatureBuf[index];
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:490:    const s = derSignatureBuf.slice(index, index + sLength);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:504: * @param {string} rawSignature - The raw signature in hexadecimal string format.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:511: * const rawSignature = "0x487cdb8a88f2f4044b701cbb116075c4cabe5fe4657a6358b395c0aab70694db3453a8057e442bd1aff0ecabe8a82c831f0edd7f2158b7c1feb3de9b1f20309b1c";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:512: * const derSignature = toDerSignature(rawSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:513: * console.log(derSignature); // Outputs the DER-encoded signature as a hex string
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:516:const toDerSignature = (rawSignature) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:517:    const rawSignatureBuf = encoding.uint8ArrayFromHexString(rawSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:519:    const r = rawSignatureBuf.slice(0, 32);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:520:    const s = rawSignatureBuf.slice(32, 64);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:541:    const derSignature = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:547:    return encoding.uint8ArrayToHexString(derSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:615:exports.fromDerSignature = fromDerSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:622:exports.toDerSignature = toDerSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:114: * @param {string} derSignature - The DER-encoded signature.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:117:export declare const fromDerSignature: (derSignature: string) => Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:125: * @param {string} rawSignature - The raw signature in hexadecimal string format.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:132: * const rawSignature = "0x487cdb8a88f2f4044b701cbb116075c4cabe5fe4657a6358b395c0aab70694db3453a8057e442bd1aff0ecabe8a82c831f0edd7f2158b7c1feb3de9b1f20309b1c";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:133: * const derSignature = toDerSignature(rawSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:134: * console.log(derSignature); // Outputs the DER-encoded signature as a hex string
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:137:export declare const toDerSignature: (rawSignature: string) => string;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:13:exports.fromDerSignature = crypto.fromDerSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:20:exports.toDerSignature = crypto.toDerSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:33:exports.verifySessionJwtSignature = turnkey.verifySessionJwtSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:34:exports.verifyStampSignature = turnkey.verifyStampSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:37:exports.verifyAppProofSignature = proof.verifyAppProofSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:46:async function verify(appProof, bootProof) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:48:    verifyAppProofSignature(appProof);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:81:function verifyAppProofSignature(appProof) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:125:        throw new Error("Signature verification failed");
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:161:                const ok = await cert.verify({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:177:                const ok = await cert.verify({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:183:                    throw new Error(`Signature check failed: ${cert.subject} not signed by ${issuer?.subject}`);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:195:        "Signature1",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:203:    const ok = await cryptoInstance.subtle.verify({ name: "ECDSA", hash: { name: "SHA-384" } }, pubKey, new Uint8Array(signature), tbs);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:217:export { getCryptoInstance, verify, verifyAppProofSignature, verifyCertificateChain, verifyCoseSign1Sig };
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:68:        const verified = await verifyEnclaveSignature(parsedExportBundle.enclaveQuorumPublic, parsedExportBundle.dataSignature, parsedExportBundle.data, dangerouslyOverrideSignerPublicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:122: * await verifyStampSignature(publicKey, signature, stampedRequest.body)
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:124:const verifyStampSignature = async (publicKey, signature, signedData) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:131:    const publicSignatureBuf = crypto.fromDerSignature(signature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:140: * @param {string} publicSignature - The ECDSA signature in DER format.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:145:const verifyEnclaveSignature = async (enclaveQuorumPublic, publicSignature, signedData, dangerouslyOverrideSignerPublicKey) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:156:    const publicSignatureBuf = crypto.fromDerSignature(publicSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:209:    const verified = await verifyEnclaveSignature(parsedImportBundle.enclaveQuorumPublic, parsedImportBundle.dataSignature, parsedImportBundle.data, dangerouslyOverrideSignerPublicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:240:    const verified = await verifyEnclaveSignature(parsedImportBundle.enclaveQuorumPublic, parsedImportBundle.dataSignature, parsedImportBundle.data, dangerouslyOverrideSignerPublicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:284:const verifySessionJwtSignature = async (jwt, dangerouslyOverrideNotarizerPublicKey) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:362:exports.verifySessionJwtSignature = verifySessionJwtSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:363:exports.verifyStampSignature = verifyStampSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:407: * @param {string} derSignature - The DER-encoded signature.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:410:const fromDerSignature = (derSignature) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:411:    const derSignatureBuf = uint8ArrayFromHexString(derSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:413:    if (derSignatureBuf.length < 2) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:417:    if (derSignatureBuf[0] !== 0x30) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:422:    const lengthByte = derSignatureBuf[index];
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:427:        if (derSignatureBuf.length < 1 + 1 + lengthByte) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:445:    if (derSignatureBuf[index] !== 0x02) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:449:    const rLength = derSignatureBuf[index];
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:455:    const r = derSignatureBuf.slice(index, index + rLength);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:458:    if (derSignatureBuf[index] !== 0x02) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:462:    const sLength = derSignatureBuf[index];
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:468:    const s = derSignatureBuf.slice(index, index + sLength);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:482: * @param {string} rawSignature - The raw signature in hexadecimal string format.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:489: * const rawSignature = "0x487cdb8a88f2f4044b701cbb116075c4cabe5fe4657a6358b395c0aab70694db3453a8057e442bd1aff0ecabe8a82c831f0edd7f2158b7c1feb3de9b1f20309b1c";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:490: * const derSignature = toDerSignature(rawSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:491: * console.log(derSignature); // Outputs the DER-encoded signature as a hex string
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:494:const toDerSignature = (rawSignature) => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:495:    const rawSignatureBuf = uint8ArrayFromHexString(rawSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:497:    const r = rawSignatureBuf.slice(0, 32);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:498:    const s = rawSignatureBuf.slice(32, 64);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:519:    const derSignature = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:525:    return uint8ArrayToHexString(derSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json | sed -n '1,140p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	{
     2	  "name": "@turnkey/crypto",
     3	  "version": "2.8.8",
     4	  "main": "./dist/index.js",
     5	  "module": "./dist/index.mjs",
     6	  "exports": {
     7	    ".": {
     8	      "types": "./dist/index.d.ts",
     9	      "import": "./dist/index.mjs",
    10	      "require": "./dist/index.js",
    11	      "default": "./dist/index.mjs"
    12	    }
    13	  },
    14	  "types": "./dist/index.d.ts",
    15	  "license": "Apache-2.0",
    16	  "description": "Encryption, decryption, and key related utility functions",
    17	  "author": {
    18	    "name": "Turnkey",
    19	    "url": "https://turnkey.com/"
    20	  },
    21	  "homepage": "https://github.com/tkhq/sdk",
    22	  "bugs": {
    23	    "url": "https://github.com/tkhq/sdk/issues"
    24	  },
    25	  "repository": {
    26	    "type": "git",
    27	    "url": "git+https://github.com/tkhq/sdk.git",
    28	    "directory": "packages/crypto"
    29	  },
    30	  "files": [
    31	    "dist/",
    32	    "CHANGELOG.md",
    33	    "README.md"
    34	  ],
    35	  "publishConfig": {
    36	    "access": "public"
    37	  },
    38	  "engines": {
    39	    "node": ">=18.0.0"
    40	  },
    41	  "dependencies": {
    42	    "@noble/ciphers": "1.3.0",
    43	    "@noble/curves": "1.9.0",
    44	    "@noble/hashes": "1.8.0",
    45	    "@peculiar/x509": "1.12.3",
    46	    "borsh": "2.0.0",
    47	    "cbor-js": "0.1.0",
    48	    "@turnkey/encoding": "0.6.0",
    49	    "@turnkey/sdk-types": "0.11.0"
    50	  },
    51	  "devDependencies": {
    52	    "@types/cbor-js": "^0.1.1",
    53	    "@types/jest": "^29.5.3",
    54	    "jest": "29.7.0",
    55	    "@turnkey/api-key-stamper": "0.5.0",
    56	    "@turnkey/http": "3.16.0"
    57	  },
    58	  "scripts": {
    59	    "build": "rollup -c",
    60	    "clean": "rimraf ./dist ./.cache",
    61	    "test": "jest",
    62	    "typecheck": "tsc -p tsconfig.typecheck.json"
    63	  }
    64	}
exec
/bin/zsh -lc "find . -path '*@noble*' -maxdepth 6 -type f | head -50" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
exec
/bin/zsh -lc "git show --stat --oneline -- src/Crypto.cs && git show --unified=80 -- src/Crypto.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 2861ms:
2026-05-23 09:08:57.176 xcodebuild[78856:106024258]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:57.326 xcodebuild[78856:106024257] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-kXKY1U4C' (errno=Operation not permitted)
2026-05-23 09:08:57.702 xcodebuild[78869:106024284]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:57.815 xcodebuild[78869:106024283] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
73e5109 Fix Crypto.cs divergences flagged by Codex r1
 src/Crypto.cs | 790 ++++++++++++++++++++++++++++------------------------------
 1 file changed, 388 insertions(+), 402 deletions(-)
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-1j1Bs3OD' (errno=Operation not permitted)
2026-05-23 09:08:58.611 xcodebuild[78891:106024343]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:58.720 xcodebuild[78891:106024342] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-dXHSQyvG' (errno=Operation not permitted)
2026-05-23 09:08:59.077 xcodebuild[78915:106024381]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 09:08:59.192 xcodebuild[78915:106024380] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
commit 73e5109466262896c3383d9c00d8788309eacf21
Author: Takeshi Juan <nienwu@kyuzan.com>
Date:   Sat May 23 09:06:33 2026 +0900

    Fix Crypto.cs divergences flagged by Codex r1
    
    Codex r1 (codex-crypto-reviews/Crypto.cs-r1-20260523.md) found 15
    divergences. Wire-affecting + API-completeness items fixed:
    
    1. SOLANA export (WIRE-CRITICAL): derive Ed25519 public key from the
       32-byte decrypted seed and base58-encode the 64-byte
       private||public concatenation. Was: base58-encode only the 32-byte
       private key — wire-incompatible. Now uses
       Ed25519PrivateKeyParameters.GeneratePublicKey().
    
    2. DecryptCredentialBundle Base58 fallback (BEHAVIOR-CRITICAL):
       upstream 2.8.8 uses bs58check.decode exclusively. The Unity port
       fell back to raw Base58 which would accept bundles upstream rejects.
       Removed; only Base58Check is accepted. Error path wraps as
       "Error decrypting bundle: ..." per upstream.
    
    3. DecryptExportBundle legacy envelope (BEHAVIOR-CRITICAL): the
       Unity port supported a legacy signature/signedData envelope not
       present in upstream 2.8.8. Removed; only the current
       {enclaveQuorumPublic, dataSignature, data} envelope is parsed.
    
    4. dangerouslyOverrideSignerPublicKey: added to
       EncryptPrivateKeyToBundleParams + DecryptExportBundleParams. Wired
       into VerifyEnclaveSignature.
    
    5. dangerouslyOverrideNotarizerPublicKey: added as optional second
       parameter to VerifySessionJwtSignature.
    
    6. Curve enum + UncompressRawPublicKey(curve): added Curve.P256 /
       Curve.Secp256k1; second curve uses upstream's hardcoded
       secp256k1 p / a / b. Default is P256.
    
    7. HpkeEncrypt / HpkeDecrypt error wrapping: both now wrap
       exceptions as "Unable to perform hpkeEncrypt: ..." /
       "Unable to perform hpkeDecrypt: ..." per upstream try/catch.
    
    8. VerifySessionJwtSignature: upstream throws
       "invalid JWT: need 3 parts" when the signature segment is missing;
       bad-length signature returns false; verification result bubbles
       up. Refactored away from blanket catch-all.
    
    9. EncryptPrivateKeyToBundle ordering: DecodeKey now runs BEFORE
       signature verification, matching upstream error precedence.
    
    10. Required non-empty organizationId: DecryptExportBundle now
        rejects missing bundleOrgId per upstream's
        "if (!signedData.organizationId || ...)" check.
    
    11. CompressRawPublicKey: replaced strict validation with upstream's
        permissive slice-and-LSB-flip implementation (slice(0, (1+len)>>>1)
        then compressedBytes[0] = 0x02 | (rawPublicKey[len-1] & 0x01)).
    
    12. FormatHpkeBuf: delegate length validation to UncompressRawPublicKey
        instead of an explicit "too small" check, matching upstream slice
        semantics. Now throws "failed to uncompress raw public key:
        invalid length" for sub-33-byte input.
    
    13. GenerateP256KeyPair: switched from BouncyCastle ECKeyPairGenerator
        (rejection sampling) to randomBytes(32) + GetPublicKey, matching
        upstream randomBytes(32) + getPublicKey flow exactly.
    
    14. UncompressRawPublicKey error message: changed to upstream's
        "failed to uncompress raw public key: invalid length" (was:
        "Invalid compressed public key size: {n}").
    
    15. DecodeKey: now matches upstream's exact text
        "invalid key length. Expected 64 bytes. Got X." (was:
        "Invalid Solana private key length...").
    
    ModSqrt negative-input behavior is documented (xml-doc) as not
    exercised by production call sites; left as-is. CompressRawPublicKey
    test that asserted strict-validation throws was repurposed to assert
    permissive behavior.
    
    CryptoTests updated:
    - VerifySessionJwtSignature_Empty / WrongPartCount now expect throws
      (was: returns false).
    - New UncompressRawPublicKey_Secp256k1_Roundtrip test.
    - New DecryptCredentialBundle_RawBase58_Rejected test.
    - CompressRawPublicKey_InvalidPrefix / WrongLength removed (no
      longer applicable); replaced with permissive-behavior assertion.
    - UncompressRawPublicKey_WrongLength error message updated.
    - FormatHpkeBuf_TooSmall error message updated.
    
    All 83 tests pass on net8.0 (50 Encoding + 33 Crypto).
    src builds clean on netstandard2.1 + net8.0.
    
    Codex r1 evidence committed as codex-crypto-reviews/Crypto.cs-r1-20260523.md.
    r2 and r3 to follow.

diff --git a/src/Crypto.cs b/src/Crypto.cs
index 4cd205c..1435fc3 100644
--- a/src/Crypto.cs
+++ b/src/Crypto.cs
@@ -1,1121 +1,1107 @@
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
-//   ts-source/turnkey.ts (subset)     -> DecryptCredentialBundle /
+//   ts-source/turnkey.ts (subset)     -> Curve enum / DecryptCredentialBundle /
 //                                        EncryptPrivateKeyToBundle /
 //                                        DecryptExportBundle /
 //                                        VerifySessionJwtSignature
 //
-// Out of scope (matches the upstream peak-Unity port):
+// Out of scope (matches the peak Unity port):
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
-//     BigInteger / EC point primitives only. HPKE, HKDF, Tonelli-Shanks,
-//     and bundle parsing logic are direct line-by-line ports of the
-//     upstream TypeScript.
+//     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
+//     Tonelli-Shanks, and bundle parsing logic are direct line-by-line
+//     ports of the upstream TypeScript.
 //   - Newtonsoft.Json dependency dropped.
 //
 // 2.8.8 vs 2.8.9 note:
 //   The only diff between @turnkey/crypto@2.8.8 and @turnkey/crypto@2.8.9 in
 //   the published dist/ is the inlining of QOS_ENCRYPTION_HMAC_MESSAGE
 //   (2.8.9 hard-codes the bytes; 2.8.8 uses new TextEncoder().encode("...")).
 //   They produce identical wire bytes. This port targets 2.8.8 (peak's pin)
 //   but is logically equivalent to 2.8.9 as well.
 
 using System;
-using System.Linq;
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
 
+        #region Curve enum (turnkey.ts type)
+
+        /// <summary>
+        /// EC curve identifier. Mirrors the upstream union type
+        /// <c>"CURVE_P256" | "CURVE_SECP256K1"</c> exposed by
+        /// <see cref="UncompressRawPublicKey(byte[], Curve)"/>.
+        /// </summary>
+        public enum Curve
+        {
+            P256,
+            Secp256k1,
+        }
+
+        #endregion
+
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
-            /// <param name="x">Value to take the square root of.</param>
+            /// <param name="x">Value to take the square root of (must be non-negative; EC field-element semantics).</param>
             /// <param name="p">Prime modulus.</param>
             /// <returns>One square root of <paramref name="x"/> modulo <paramref name="p"/>.</returns>

exec
/bin/zsh -lc "rg -n \"Logic divergence|Divergence|E\\.|ModSqrt|low|CompressRawPublicKey|VerifySession|Error\" codex-crypto-reviews/Crypto.cs-r1-20260523.md | head -80" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
50:E. Logic divergence findings: every place C# behavior differs from upstream TS:
53:   low-S), DTO shape (field names, order, presence, optionality), JSON
70:2026-05-22T23:55:35.245107Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
72:I’ll use the code-review workflow here and do a source-pinned comparison against the local upstream snapshot, then report the required A-G sections with file/line evidence.
81:allowed-tools:
125:- **Has uncommitted changes** → Continue with steps 1-4 (normal flow)
137:**If CHANGELOG is not updated, you must automatically perform the following (don't ask user to do it manually):**
142:4. **Continue review flow**: Immediately proceed to next steps after CHANGELOG update
155:**Example - Auto-generation Flow:**
312:310:- For `peak` doc-plan reviews, read the plan plus nearby repo guidance before commenting; `docs/development-workflow.md`, `docs/sdk-coding-conventions.md`, `docs/sdk-naming-conventions.md`, `docs/sdk-error-handling.md`, and `docs/peak-sdk-unity-internal/README.md` were the useful mismatch detectors [Task 1]
337:     8	//   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
341:    12	//                                        CompressRawPublicKey /
351:    22	//                                        VerifySessionJwtSignature
469:   140	            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
596:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md
645:2026-05-23 08:56:00.289 xcodebuild[71383:105991344] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
649:2026-05-23 08:56:00.831 xcodebuild[71385:105991369] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
654:2026-05-23 08:56:01.740 xcodebuild[71392:105991408] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
658:2026-05-23 08:56:02.189 xcodebuild[71394:105991417] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
663:2026-05-23 08:56:03.059 xcodebuild[71398:105991453] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
667:2026-05-23 08:56:03.513 xcodebuild[71400:105991468] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
875:   463	            var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
892:   480	        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
950:   538	            var y = Math.ModSqrt(rhs, p);
1069:    33	    "README.md"
1253:   150	    throw new Error(`Unable to perform hpkeEncrypt: ${error}`);
1317:   214	    throw new Error(`Unable to perform hpkeEncrypt: ${error}`);
1325:   222	 * creates additional associated data which follows the form: sender_public||sender_public_len||receiver_public||receiver_public_len
1453:   626	                    + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
1548:   721	                    "Error encrypting private key bundle: " + err.Message, err);
1669:   842	                    "Error decrypting export bundle: " + err.Message, err);
1678:   851	        public static bool VerifySessionJwtSignature(string jwt)
1881:   349	    throw new Error(`Unable to perform hpkeDecrypt: ${error} `);
1963:   431	    throw new Error("failed to uncompress raw public key: invalid length");
1967:   435	    throw new Error("failed to uncompress raw public key: invalid prefix");
2004:   472	    throw new Error("x is out of range");
2008:   476	    throw new Error("y is out of range");
2152:   620	    throw new Error(
2170:   638	    throw new Error(
2177:   645	    throw new Error(
2191:   659	      throw new Error(
2207:   675	    throw new Error(
2214:   682	    throw new Error(
2222:   690	  // Allow up to 32 data bytes + 1 byte 0-padding prefix
2224:   692	    throw new Error(
2235:   703	    throw new Error(
2243:   711	  // Allow up to 32 data bytes + 1 byte 0-padding prefix
2245:   713	    throw new Error(
2271:   739	 * @throws {Error} - Throws an error if the input signature is invalid or if the encoding process fails.
2302:     6	    throw new Error("p must be positive");
2311:    15	      throw new Error("could not find a modular square root");
2317:    21	  throw new Error("unsupported modulus value");
2566:    70	 * @throws {Error} - If unable to decrypt the credential bundle
2575:    79	      throw new Error(
2576:    80	        `Bundle size ${bundleBytes.byteLength} is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.`,
2591:    95	    throw new Error(`"Error decrypting bundle:", ${error}`);
2603:   107	 * @param {DecryptExportBundleParams} params - An object containing the following properties:
2610:   114	 * @throws {Error} - If decryption or signature verification fails, throws an error with details.
2629:   133	      throw new Error(
2644:   148	      throw new Error(
2649:   153	      throw new Error('missing "encappedPublic" in bundle signed data');
2662:   166	        throw new Error(
2668:   172	        throw new Error(
2681:   185	    throw new Error(`Error decrypting bundle: ${error}`);
2710:   214	    throw new Error("failed to load public key");
2739:   243	    throw new Error(
2751:   255	    throw new Error("failed to load quorum key");
2767:   271	 * @throws {Error} - If the public key is invalid.
2785:   289	        throw new Error(
2811:   315	 * @throws {Error} - If enclave signature verification or any other validation fails.
2830:   334	    throw new Error(`failed to verify enclave signature: ${importBundle}`);
2841:   345	    throw new Error(
2846:   350	    throw new Error(
2852:   356	    throw new Error('missing "targetPublic" in bundle signed data');
2876:140:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
2893:480:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
2899:851:        public static bool VerifySessionJwtSignature(string jwt)
2962:   371	   * @throws {Error} - If enclave signature verification or any other validation fails.
2980:   389	    throw new Error(`failed to verify enclave signature: ${importBundle}`);
2991:   400	    throw new Error(

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/Crypto.cs-r1-20260523.md | sed -n '2860,3045p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
  2860	 succeeded in 0ms:
  2861	14://                                        FormatHpkeBuf, plus the internal
  2862	68:    public static class Crypto
  2863	76:        public static class Constants
  2864	79:            public static readonly byte[] SUITE_ID_1 = new byte[] { 75, 69, 77, 0, 16 }; // "KEM\0\x10"
  2865	80:            public static readonly byte[] SUITE_ID_2 = new byte[] { 72, 80, 75, 69, 0, 16, 0, 1, 0, 2 }; // "HPKE\0\x10\0\x01\0\x02"
  2866	81:            public static readonly byte[] HPKE_VERSION = new byte[] { 72, 80, 75, 69, 45, 118, 49 }; // "HPKE-v1"
  2867	84:            public static readonly byte[] LABEL_SECRET = new byte[] { 115, 101, 99, 114, 101, 116 }; // "secret"
  2868	85:            public static readonly byte[] LABEL_EAE_PRK = new byte[] { 101, 97, 101, 95, 112, 114, 107 }; // "eae_prk"
  2869	86:            public static readonly byte[] LABEL_SHARED_SECRET = new byte[]
  2870	92:            public static readonly byte[] AES_KEY_INFO = new byte[]
  2871	102:            public static readonly byte[] IV_INFO = new byte[]
  2872	113:            public const int UNCOMPRESSED_PUB_KEY_LENGTH_BYTES = 65;
  2873	116:            public const string PRODUCTION_SIGNER_SIGN_PUBLIC_KEY =
  2874	120:            public const string PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY =
  2875	131:        public static class Math
  2876	140:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
  2877	178:        public static class Hkdf
  2878	180:            private const int HashLen = 32; // SHA-256 output length, bytes.
  2879	187:            public static byte[] Extract(byte[] salt, byte[] ikm)
  2880	204:            public static byte[] Expand(byte[] prk, byte[] info, int length)
  2881	250:        public class HpkeDecryptParams
  2882	258:        public class HpkeEncryptParams
  2883	265:        public class KeyPair
  2884	276:        public class EncryptPrivateKeyToBundleParams
  2885	289:        public class DecryptExportBundleParams
  2886	304:        public class HpkeBundlePayload
  2887	321:        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
  2888	341:        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
  2889	350:        public static KeyPair GenerateP256KeyPair()
  2890	384:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
  2891	433:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
  2892	471:        public static byte[] BuildAdditionalAssociatedData(byte[] senderPubBuf, byte[] receiverPubBuf)
  2893	480:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
  2894	506:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey)
  2895	576:        public static string FormatHpkeBuf(byte[] encryptedBuf)
  2896	609:        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string targetPrivateKey)
  2897	651:        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
  2898	729:        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
  2899	851:        public static bool VerifySessionJwtSignature(string jwt)
  2900	898:        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
  2901	924:        private static byte[] GetKemContext(byte[] encappedKeyBuf, string publicKey)
  2902	931:        private static byte[] BuildLabeledIkm(byte[] label, byte[] ikm, byte[] suiteId)
  2903	937:        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
  2904	955:        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
  2905	961:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
  2906	972:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
  2907	983:        private static byte[] Base64UrlDecode(string input)
  2908	997:        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
  2909	1024:        private static void VerifyEnclaveSignature(string enclaveQuorumPublic, string signatureHex, string signedDataHex)
  2910	1052:        private static bool VerifySignature(string publicKeyHex, string signatureHex, string message)
  2911	1077:        private static byte[] DecodeKey(string privateKey, string? keyFormat)
  2912	1098:        private static string? GetStringOrNull(JsonElement element, string propertyName)
  2913	
  2914	exec
  2915	/bin/zsh -lc "rg -n \"Org\\.BouncyCastle\\.Crypto\\.Generators\\.HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|HkdfBytesGenerator|Hpke|HKDF|Kdf|KDF|Generator\" src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
  2916	 succeeded in 0ms:
  2917	10://                                        HpkeEncrypt / HpkeDecrypt /
  2918	14://                                        FormatHpkeBuf, plus the internal
  2919	17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
  2920	36://     BigInteger / EC point primitives only. HPKE, HKDF, Tonelli-Shanks,
  2921	56:using Org.BouncyCastle.Crypto.Generators;
  2922	91:            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
  2923	101:            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
  2924	172:        #region HKDF (port of @noble/hashes/hkdf)
  2925	175:        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
  2926	183:            /// HKDF Extract: produces a 32-byte pseudorandom key from input
  2927	200:            /// HKDF Expand: derives <paramref name="length"/> bytes of output
  2928	250:        public class HpkeDecryptParams
  2929	258:        public class HpkeEncryptParams
  2930	299:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
  2931	304:        public class HpkeBundlePayload
  2932	355:            var keyGen = new ECKeyPairGenerator();
  2933	381:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
  2934	384:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
  2935	406:            // Step 4: HKDF derive shared secret
  2936	425:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
  2937	431:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
  2938	433:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
  2939	574:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
  2940	576:        public static string FormatHpkeBuf(byte[] encryptedBuf)
  2941	592:            var payload = new HpkeBundlePayload
  2942	598:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
  2943	637:            var decryptedData = HpkeDecrypt(new HpkeDecryptParams
  2944	710:                var encryptedBuf = HpkeEncrypt(new HpkeEncryptParams
  2945	716:                return FormatHpkeBuf(encryptedBuf);
  2946	815:                    var decryptedData = HpkeDecrypt(new HpkeDecryptParams
  2947	
  2948	exec
  2949	/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '360,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
  2950	 succeeded in 0ms:
  2951	   360	  const targetKeyBuf = uint8ArrayFromHexString(signedData.targetPublic);
  2952	   361	  const privateKeyBundle = hpkeEncrypt({ plainTextBuf, targetKeyBuf });
  2953	   362	  return formatHpkeBuf(privateKeyBundle);
  2954	   363	};
  2955	   364	
  2956	   365	/**
  2957	   366	  /**
  2958	   367	   * Encrypts a mnemonic wallet bundle using HPKE and verifies the enclave signature.
  2959	   368	   *
  2960	   369	   * @param {EncryptWalletToBundleParams} params - An object containing the mnemonic, bundle, user, and organization details. Optionally, you can override the default signer key (for testing purposes).
  2961	   370	   * @returns {Promise<string>} - A promise that resolves to a JSON string representing the encrypted wallet bundle.
  2962	   371	   * @throws {Error} - If enclave signature verification or any other validation fails.
  2963	   372	   */
  2964	   373	export const encryptWalletToBundle = async ({
  2965	   374	  mnemonic,
  2966	   375	  importBundle,
  2967	   376	  userId,
  2968	   377	  organizationId,
  2969	   378	  dangerouslyOverrideSignerPublicKey,
  2970	   379	}: EncryptWalletToBundleParams): Promise<string> => {
  2971	   380	  const parsedImportBundle = JSON.parse(importBundle);
  2972	   381	  const plainTextBuf = new TextEncoder().encode(mnemonic);
  2973	   382	  const verified = await verifyEnclaveSignature(
  2974	   383	    parsedImportBundle.enclaveQuorumPublic,
  2975	   384	    parsedImportBundle.dataSignature,
  2976	   385	    parsedImportBundle.data,
  2977	   386	    dangerouslyOverrideSignerPublicKey,
  2978	   387	  );
  2979	   388	  if (!verified) {
  2980	   389	    throw new Error(`failed to verify enclave signature: ${importBundle}`);
  2981	   390	  }
  2982	   391	
  2983	   392	  const signedData = JSON.parse(
  2984	   393	    new TextDecoder().decode(uint8ArrayFromHexString(parsedImportBundle.data)),
  2985	   394	  );
  2986	   395	
  2987	   396	  if (
  2988	   397	    !signedData.organizationId ||
  2989	   398	    signedData.organizationId !== organizationId
  2990	   399	  ) {
  2991	   400	    throw new Error(
  2992	   401	      `organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`,
  2993	   402	    );
  2994	   403	  }
  2995	   404	  if (!signedData.userId || signedData.userId !== userId) {
  2996	   405	    throw new Error(
  2997	   406	      `user id does not match expected value. Expected: ${userId}. Found: ${signedData.userId}.`,
  2998	   407	    );
  2999	   408	  }
  3000	   409	
  3001	   410	  if (!signedData.targetPublic) {
  3002	   411	    throw new Error('missing "targetPublic" in bundle signed data');
  3003	   412	  }
  3004	   413	
  3005	   414	  // Load target public key generated from enclave
  3006	   415	  const targetKeyBuf = uint8ArrayFromHexString(signedData.targetPublic);
  3007	   416	  const privateKeyBundle = hpkeEncrypt({ plainTextBuf, targetKeyBuf });
  3008	   417	  return formatHpkeBuf(privateKeyBundle);
  3009	   418	};
  3010	   419	
  3011	   420	/**
  3012	   421	 * Verifies that a **session JWT** was signed by Turnkey’s
  3013	   422	 * notarizer key (P-256 / ES256, compact 64-byte r‖s signature).
  3014	   423	 *
  3015	   424	 * How it works
  3016	   425	 * ------------
  3017	   426	 * 1.  Split the JWT into `header.payload.signature`.
  3018	   427	 * 2.  **Double-hash** the string `"header.payload"`:
  3019	   428	 *        `h1 = sha256(header.payload)`
  3020	   429	 *        `msg = sha256(h1)`
  3021	   430	 *     (The Rust signer feeds `h1` into `SigningKey::sign`, which hashes once
  3022	   431	 *     more internally, yielding `msg`.)
  3023	   432	 * 3.  Base64-URL-decode the signature (`r||s`, 64 bytes).
  3024	   433	 * 4.  Import the notarizer public key (hex `04‖X‖Y` → `Uint8Array`).
  3025	   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
  3026	   435	 *     `msg` as a pre-hashed digest and performs ECDSA verification.
  3027	   436	 *
  3028	   437	 * @param jwt   The session JWT to validate.
  3029	   438	 * @param dangerouslyOverrideNotarizerPublicKey *(optional)* Hex-encoded
  3030	   439	 *              uncompressed P-256 public key to verify against (use only in
  3031	   440	 *              tests).  Defaults to the production notarizer key.
  3032	   441	 * @returns `true` if the signature is valid for the given key, else `false`.
  3033	   442	 * @throws  If the JWT is malformed.
  3034	   443	 */
  3035	   444	export const verifySessionJwtSignature = async (
  3036	   445	  jwt: string,
  3037	   446	  dangerouslyOverrideNotarizerPublicKey?: string,
  3038	   447	): Promise<boolean> => {
  3039	   448	  const notarizerKeyHex =
  3040	   449	    dangerouslyOverrideNotarizerPublicKey ??
  3041	   450	    PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY;
  3042	   451	
  3043	   452	  /* 1. split JWT -------------------------------------------------------- */
  3044	   453	  const [headerB64, payloadB64, signatureB64] = jwt.split(".");
  3045	   454	  if (!signatureB64) throw new Error("invalid JWT: need 3 parts");

exec
/bin/zsh -lc "rg -n \"\\| .*src/Crypto.cs|Logic divergence findings|WIRE|BEHAVIOR|SEMANTIC|divergence\" codex-crypto-reviews/Crypto.cs-r1-20260523.md | tail -120" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
50:E. Logic divergence findings: every place C# behavior differs from upstream TS:
68:DO NOT use "looks good" or "no divergence found" without producing all sections.
3954:    50	E. Logic divergence findings: every place C# behavior differs from upstream TS:
3972:    68	DO NOT use "looks good" or "no divergence found" without producing all sections.
4182:    14	| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
4230:/bin/zsh -lc 'rg -n "''^A'"\\.|"'^B'"\\.|"'^C'"\\.|"'^D'"\\.|"'^E'"\\.|"'^F'"\\.|"'^G'"\\.|Logic divergence|NOT-REVIEWED|PASS|FAIL|src/Crypto.cs\" codex-crypto-reviews/Crypto.cs-r1-20260523.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
4241:50:E. Logic divergence findings: every place C# behavior differs from upstream TS:
4343:3954:    50	E. Logic divergence findings: every place C# behavior differs from upstream TS:
4345:4182:    14	| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
4521:   174	    14	| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
5032:| `Crypto.Math.ModSqrt` (`src/Crypto.cs:140`) | `modSqrt` (`ts-source/math.ts:4`) | REVIEWED | Same P-256 call-path semantics; negative public input differs, listed in E. |
5033:| `Crypto.Hkdf.Extract` (`src/Crypto.cs:187`) | `hkdf.extract` call (`ts-source/crypto.ts:551`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
5034:| `Crypto.Hkdf.Expand` (`src/Crypto.cs:204`) | `hkdf.expand` call (`ts-source/crypto.ts:552`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
5035:| `GetPublicKey(byte[])` (`src/Crypto.cs:321`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | P-256 SEC1 output matches compressed flag. |
5036:| `GetPublicKey(string)` (`src/Crypto.cs:341`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | C# overload for TS union input. |
5037:| `GenerateP256KeyPair` (`src/Crypto.cs:350`) | `generateP256KeyPair` (`ts-source/crypto.ts:358`) | REVIEWED | Key generation distribution/error behavior differs, listed in E. |
5038:| `HpkeDecrypt` (`src/Crypto.cs:384`) | `hpkeDecrypt` (`ts-source/crypto.ts:308`) | REVIEWED | HPKE steps match; error wrapping differs, listed in E. |
5039:| `HpkeEncrypt` (`src/Crypto.cs:433`) | `hpkeEncrypt` (`ts-source/crypto.ts:95`) | REVIEWED | HPKE steps match; null/error behavior differs, listed in E. |
5040:| `BuildAdditionalAssociatedData` (`src/Crypto.cs:471`) | `buildAdditionalAssociatedData` (`ts-source/crypto.ts:378`) | REVIEWED | Concatenation matches. |
5041:| `CompressRawPublicKey` (`src/Crypto.cs:480`) | `compressRawPublicKey` (`ts-source/crypto.ts:406`) | REVIEWED | Valid-key output matches; validation differs, listed in E. |
5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
5043:| `FormatHpkeBuf` (`src/Crypto.cs:576`) | `formatHpkeBuf` (`ts-source/crypto.ts:286`) | REVIEWED | JSON field order/name matches; short-buffer behavior differs, listed in E. |
5044:| `DecryptCredentialBundle` (`src/Crypto.cs:609`) | `decryptCredentialBundle` (`ts-source/turnkey.ts:72`) | REVIEWED | HPKE path matches; Base58 fallback differs, listed in E. |
5045:| `EncryptPrivateKeyToBundle` (`src/Crypto.cs:651`) | `encryptPrivateKeyToBundle` (`ts-source/turnkey.ts:317`) | REVIEWED | Main happy path matches; DTO/validation/order differ, listed in E. |
5046:| `DecryptExportBundle` (`src/Crypto.cs:729`) | `decryptExportBundle` (`ts-source/turnkey.ts:116`) | REVIEWED | Multiple semantic differences, listed in E. |
5047:| `VerifySessionJwtSignature` (`src/Crypto.cs:851`) | `verifySessionJwtSignature` (`ts-source/turnkey.ts:444`) | REVIEWED | Double-SHA256 and raw signature path reviewed; malformed/error behavior differs, listed in E. |
5048:| `DeriveSS` (`src/Crypto.cs:898`) | `deriveSS` (`ts-source/crypto.ts:559`) | REVIEWED | Returns x-coordinate shared secret, padded to 32 bytes. |
5049:| `GetKemContext` (`src/Crypto.cs:924`) | `getKemContext` (`ts-source/crypto.ts:598`) | REVIEWED | Concatenation matches. |
5050:| `BuildLabeledIkm` (`src/Crypto.cs:931`) | `buildLabeledIkm` (`ts-source/crypto.ts:491`) | REVIEWED | Label layout matches. |
5051:| `BuildLabeledInfo` (`src/Crypto.cs:937`) | `buildLabeledInfo` (`ts-source/crypto.ts:524`) | REVIEWED | HPKE info layout matches for reviewed lengths. |
5052:| `ExtractAndExpand` (`src/Crypto.cs:955`) | `extractAndExpand` (`ts-source/crypto.ts:545`) | REVIEWED | HKDF extract/expand order matches. |
5053:| `AesGcmDecrypt` (`src/Crypto.cs:961`) | `aesGcmDecrypt` (`ts-source/crypto.ts:584`) | REVIEWED | AES-GCM decrypt/tag handling matches. |
5054:| `AesGcmEncrypt` (`src/Crypto.cs:972`) | `aesGcmEncrypt` (`ts-source/crypto.ts:570`) | REVIEWED | AES-GCM ciphertext||tag output matches. |
5055:| `Base64UrlDecode` (`src/Crypto.cs:983`) | inline `toB64` decode (`ts-source/turnkey.ts:461`) | REVIEWED | Padding/base64url transform reviewed. |
5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
5057:| `VerifyEnclaveSignature` (`src/Crypto.cs:1024`) | `verifyEnclaveSignature` (`ts-source/turnkey.ts:234`) | REVIEWED | DER signature verification reviewed; override missing in E. |
5058:| `VerifySignature` (`src/Crypto.cs:1052`) | no current upstream equivalent | REVIEWED | C# legacy-envelope extension; listed in E. |
5059:| `DecodeKey` (`src/Crypto.cs:1077`) | `decodeKey` (`ts-source/turnkey.ts:284`) | REVIEWED | SOLANA/hex decode matches for import; error text differs. |
5060:| `GetStringOrNull` (`src/Crypto.cs:1098`) | JSON property access inline | REVIEWED | Structural parser helper; non-string handling differs only on malformed JSON. |
5085:| `src/Crypto.cs:151` | `ts-source/math.ts:8` | Public negative `x` differs: C# `BigInteger.Mod` normalizes non-negative; JS `%` keeps negative sign. Crypto call sites use non-negative EC coordinates, but the public helper is observably different. | Mirror JS remainder for public parity or make helper private/document non-negative input. |
5086:| `src/Crypto.cs:355` | `ts-source/crypto.ts:358-363` | C# uses `ECKeyPairGenerator`, which samples valid scalars; TS samples 32 random bytes then derives public key. Rare invalid scalar/failure/distribution behavior differs. | If strict parity is required, generate 32 random bytes and derive with the same validation behavior; otherwise document as deliberate hardening. |
5087:| `src/Crypto.cs:435-438` | `ts-source/crypto.ts:95-151` | `HpkeEncrypt` treats null `PlainTextBuf` as empty and does not wrap errors as `Unable to perform hpkeEncrypt`. TS catches and wraps all errors. | Require non-null plaintext and wrap exceptions with the upstream message. |
5088:| `src/Crypto.cs:386-421` | `ts-source/crypto.ts:308-350` | `HpkeDecrypt` throws C# argument/crypto exceptions directly; TS wraps all errors as `Unable to perform hpkeDecrypt: ...`. | Wrap non-fatal exceptions with upstream-equivalent message. |
5089:| `src/Crypto.cs:482-486` | `ts-source/crypto.ts:406-418` | C# rejects non-65-byte or non-`0x04` input; TS slices any input and mutates prefix based on last byte. Valid public keys match, malformed input behavior differs. | Remove extra validation for parity or keep and document stricter validation. |
5090:| `src/Crypto.cs:506` | `ts-source/crypto.ts:426-462` | C# only supports P-256; upstream exposes optional `curve` and supports `CURVE_SECP256K1`. | Add curve parameter/support or document intentional P-256-only subset. |
5091:| `src/Crypto.cs:509-512` | `ts-source/crypto.ts:430-432` | Invalid compressed length error message differs. | Use upstream message if exact error parity matters. |
5092:| `src/Crypto.cs:579-582` | `ts-source/crypto.ts:286-299` | C# rejects buffers `<= 33`; TS slices first 33 bytes and allows downstream behavior. A valid compressed key plus empty ciphertext would stringify upstream but throw in C#. | Match TS slicing behavior, or only reject `< 33` if defensive validation is desired. |
5093:| `src/Crypto.cs:614-619` | `ts-source/turnkey.ts:76-77` | C# falls back from Base58Check to raw Base58; upstream only accepts Base58Check. This can accept bundles TS rejects. | Remove raw Base58 fallback. |
5094:| `src/Crypto.cs:276-283`, `289-296`, `851` | `ts-source/turnkey.ts:31-45`, `116-123`, `444-450` | DTO/API shape omits `dangerouslyOverrideSignerPublicKey` and `dangerouslyOverrideNotarizerPublicKey`. | Add optional override fields/parameters and pass them through to verification helpers. |
5095:| `src/Crypto.cs:665-708` | `ts-source/turnkey.ts:325-360` | `EncryptPrivateKeyToBundle` verifies/parses bundle before `DecodeKey`; upstream decodes private key before signature verification. Error precedence differs when both inputs are bad. | Move `DecodeKey` immediately after JSON parse to match upstream. |
5096:| `src/Crypto.cs:753-767`, `1052` | `ts-source/turnkey.ts:124-162` | C# accepts a legacy `signature`/`signedData` envelope path not present in upstream 2.8.8. | Remove from this port or isolate as a separately named compatibility helper. |
5097:| `src/Crypto.cs:796-803` | `ts-source/turnkey.ts:144-151` | C# allows missing `organizationId`; upstream rejects missing or mismatched `organizationId`. | Require `bundleOrgId` to be non-empty and equal. |
5098:| `src/Crypto.cs:827-830` | `ts-source/turnkey.ts:164-180` | SOLANA export output is wrong: C# Base58-encodes only 32 decrypted private-key bytes; upstream derives Ed25519 public key, concatenates 32 private + 32 public bytes, then Base58-encodes 64 bytes. | Derive Ed25519 public key and encode the 64-byte concatenation. |
5099:| `src/Crypto.cs:853-890` | `ts-source/turnkey.ts:452-477` | Malformed JWT behavior differs: C# returns `false` for wrong part count and catches all errors; upstream throws for missing signature part and returns verifier result otherwise. | Throw on malformed JWT and avoid catch-all if exact upstream behavior is required. |
5114:- Pass criterion is not met this round: section B has zero `NOT-REVIEWED` rows and D passes, but E has divergences and F is not satisfied.
5130:| `Crypto.Math.ModSqrt` (`src/Crypto.cs:140`) | `modSqrt` (`ts-source/math.ts:4`) | REVIEWED | Same P-256 call-path semantics; negative public input differs, listed in E. |
5131:| `Crypto.Hkdf.Extract` (`src/Crypto.cs:187`) | `hkdf.extract` call (`ts-source/crypto.ts:551`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
5132:| `Crypto.Hkdf.Expand` (`src/Crypto.cs:204`) | `hkdf.expand` call (`ts-source/crypto.ts:552`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
5133:| `GetPublicKey(byte[])` (`src/Crypto.cs:321`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | P-256 SEC1 output matches compressed flag. |
5134:| `GetPublicKey(string)` (`src/Crypto.cs:341`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | C# overload for TS union input. |
5135:| `GenerateP256KeyPair` (`src/Crypto.cs:350`) | `generateP256KeyPair` (`ts-source/crypto.ts:358`) | REVIEWED | Key generation distribution/error behavior differs, listed in E. |
5136:| `HpkeDecrypt` (`src/Crypto.cs:384`) | `hpkeDecrypt` (`ts-source/crypto.ts:308`) | REVIEWED | HPKE steps match; error wrapping differs, listed in E. |
5137:| `HpkeEncrypt` (`src/Crypto.cs:433`) | `hpkeEncrypt` (`ts-source/crypto.ts:95`) | REVIEWED | HPKE steps match; null/error behavior differs, listed in E. |
5138:| `BuildAdditionalAssociatedData` (`src/Crypto.cs:471`) | `buildAdditionalAssociatedData` (`ts-source/crypto.ts:378`) | REVIEWED | Concatenation matches. |
5139:| `CompressRawPublicKey` (`src/Crypto.cs:480`) | `compressRawPublicKey` (`ts-source/crypto.ts:406`) | REVIEWED | Valid-key output matches; validation differs, listed in E. |
5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
5141:| `FormatHpkeBuf` (`src/Crypto.cs:576`) | `formatHpkeBuf` (`ts-source/crypto.ts:286`) | REVIEWED | JSON field order/name matches; short-buffer behavior differs, listed in E. |
5142:| `DecryptCredentialBundle` (`src/Crypto.cs:609`) | `decryptCredentialBundle` (`ts-source/turnkey.ts:72`) | REVIEWED | HPKE path matches; Base58 fallback differs, listed in E. |
5143:| `EncryptPrivateKeyToBundle` (`src/Crypto.cs:651`) | `encryptPrivateKeyToBundle` (`ts-source/turnkey.ts:317`) | REVIEWED | Main happy path matches; DTO/validation/order differ, listed in E. |
5144:| `DecryptExportBundle` (`src/Crypto.cs:729`) | `decryptExportBundle` (`ts-source/turnkey.ts:116`) | REVIEWED | Multiple semantic differences, listed in E. |
5145:| `VerifySessionJwtSignature` (`src/Crypto.cs:851`) | `verifySessionJwtSignature` (`ts-source/turnkey.ts:444`) | REVIEWED | Double-SHA256 and raw signature path reviewed; malformed/error behavior differs, listed in E. |
5146:| `DeriveSS` (`src/Crypto.cs:898`) | `deriveSS` (`ts-source/crypto.ts:559`) | REVIEWED | Returns x-coordinate shared secret, padded to 32 bytes. |
5147:| `GetKemContext` (`src/Crypto.cs:924`) | `getKemContext` (`ts-source/crypto.ts:598`) | REVIEWED | Concatenation matches. |
5148:| `BuildLabeledIkm` (`src/Crypto.cs:931`) | `buildLabeledIkm` (`ts-source/crypto.ts:491`) | REVIEWED | Label layout matches. |
5149:| `BuildLabeledInfo` (`src/Crypto.cs:937`) | `buildLabeledInfo` (`ts-source/crypto.ts:524`) | REVIEWED | HPKE info layout matches for reviewed lengths. |
5150:| `ExtractAndExpand` (`src/Crypto.cs:955`) | `extractAndExpand` (`ts-source/crypto.ts:545`) | REVIEWED | HKDF extract/expand order matches. |
5151:| `AesGcmDecrypt` (`src/Crypto.cs:961`) | `aesGcmDecrypt` (`ts-source/crypto.ts:584`) | REVIEWED | AES-GCM decrypt/tag handling matches. |
5152:| `AesGcmEncrypt` (`src/Crypto.cs:972`) | `aesGcmEncrypt` (`ts-source/crypto.ts:570`) | REVIEWED | AES-GCM ciphertext||tag output matches. |
5153:| `Base64UrlDecode` (`src/Crypto.cs:983`) | inline `toB64` decode (`ts-source/turnkey.ts:461`) | REVIEWED | Padding/base64url transform reviewed. |
5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
5155:| `VerifyEnclaveSignature` (`src/Crypto.cs:1024`) | `verifyEnclaveSignature` (`ts-source/turnkey.ts:234`) | REVIEWED | DER signature verification reviewed; override missing in E. |
5156:| `VerifySignature` (`src/Crypto.cs:1052`) | no current upstream equivalent | REVIEWED | C# legacy-envelope extension; listed in E. |
5157:| `DecodeKey` (`src/Crypto.cs:1077`) | `decodeKey` (`ts-source/turnkey.ts:284`) | REVIEWED | SOLANA/hex decode matches for import; error text differs. |
5158:| `GetStringOrNull` (`src/Crypto.cs:1098`) | JSON property access inline | REVIEWED | Structural parser helper; non-string handling differs only on malformed JSON. |
5183:| `src/Crypto.cs:151` | `ts-source/math.ts:8` | Public negative `x` differs: C# `BigInteger.Mod` normalizes non-negative; JS `%` keeps negative sign. Crypto call sites use non-negative EC coordinates, but the public helper is observably different. | Mirror JS remainder for public parity or make helper private/document non-negative input. |
5184:| `src/Crypto.cs:355` | `ts-source/crypto.ts:358-363` | C# uses `ECKeyPairGenerator`, which samples valid scalars; TS samples 32 random bytes then derives public key. Rare invalid scalar/failure/distribution behavior differs. | If strict parity is required, generate 32 random bytes and derive with the same validation behavior; otherwise document as deliberate hardening. |
5185:| `src/Crypto.cs:435-438` | `ts-source/crypto.ts:95-151` | `HpkeEncrypt` treats null `PlainTextBuf` as empty and does not wrap errors as `Unable to perform hpkeEncrypt`. TS catches and wraps all errors. | Require non-null plaintext and wrap exceptions with the upstream message. |
5186:| `src/Crypto.cs:386-421` | `ts-source/crypto.ts:308-350` | `HpkeDecrypt` throws C# argument/crypto exceptions directly; TS wraps all errors as `Unable to perform hpkeDecrypt: ...`. | Wrap non-fatal exceptions with upstream-equivalent message. |
5187:| `src/Crypto.cs:482-486` | `ts-source/crypto.ts:406-418` | C# rejects non-65-byte or non-`0x04` input; TS slices any input and mutates prefix based on last byte. Valid public keys match, malformed input behavior differs. | Remove extra validation for parity or keep and document stricter validation. |
5188:| `src/Crypto.cs:506` | `ts-source/crypto.ts:426-462` | C# only supports P-256; upstream exposes optional `curve` and supports `CURVE_SECP256K1`. | Add curve parameter/support or document intentional P-256-only subset. |
5189:| `src/Crypto.cs:509-512` | `ts-source/crypto.ts:430-432` | Invalid compressed length error message differs. | Use upstream message if exact error parity matters. |
5190:| `src/Crypto.cs:579-582` | `ts-source/crypto.ts:286-299` | C# rejects buffers `<= 33`; TS slices first 33 bytes and allows downstream behavior. A valid compressed key plus empty ciphertext would stringify upstream but throw in C#. | Match TS slicing behavior, or only reject `< 33` if defensive validation is desired. |
5191:| `src/Crypto.cs:614-619` | `ts-source/turnkey.ts:76-77` | C# falls back from Base58Check to raw Base58; upstream only accepts Base58Check. This can accept bundles TS rejects. | Remove raw Base58 fallback. |
5192:| `src/Crypto.cs:276-283`, `289-296`, `851` | `ts-source/turnkey.ts:31-45`, `116-123`, `444-450` | DTO/API shape omits `dangerouslyOverrideSignerPublicKey` and `dangerouslyOverrideNotarizerPublicKey`. | Add optional override fields/parameters and pass them through to verification helpers. |
5193:| `src/Crypto.cs:665-708` | `ts-source/turnkey.ts:325-360` | `EncryptPrivateKeyToBundle` verifies/parses bundle before `DecodeKey`; upstream decodes private key before signature verification. Error precedence differs when both inputs are bad. | Move `DecodeKey` immediately after JSON parse to match upstream. |
5194:| `src/Crypto.cs:753-767`, `1052` | `ts-source/turnkey.ts:124-162` | C# accepts a legacy `signature`/`signedData` envelope path not present in upstream 2.8.8. | Remove from this port or isolate as a separately named compatibility helper. |
5195:| `src/Crypto.cs:796-803` | `ts-source/turnkey.ts:144-151` | C# allows missing `organizationId`; upstream rejects missing or mismatched `organizationId`. | Require `bundleOrgId` to be non-empty and equal. |
5196:| `src/Crypto.cs:827-830` | `ts-source/turnkey.ts:164-180` | SOLANA export output is wrong: C# Base58-encodes only 32 decrypted private-key bytes; upstream derives Ed25519 public key, concatenates 32 private + 32 public bytes, then Base58-encodes 64 bytes. | Derive Ed25519 public key and encode the 64-byte concatenation. |
5197:| `src/Crypto.cs:853-890` | `ts-source/turnkey.ts:452-477` | Malformed JWT behavior differs: C# returns `false` for wrong part count and catches all errors; upstream throws for missing signature part and returns verifier result otherwise. | Throw on malformed JWT and avoid catch-all if exact upstream behavior is required. |
5212:- Pass criterion is not met this round: section B has zero `NOT-REVIEWED` rows and D passes, but E has divergences and F is not satisfied.

codex
A. Source Pin Acknowledgement

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `73e5109466262896c3383d9c00d8788309eacf21`

B. Method Coverage Table

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---:|---|
| `Crypto.Math.ModSqrt` (`src/Crypto.cs:162`) | `modSqrt` (`ts-source/math.ts:4`) | REVIEWED | Same production EC-coordinate path; negative input divergence in E. |
| `Crypto.Hkdf.Extract` (`src/Crypto.cs:201`) | `hkdf.extract` call (`ts-source/crypto.ts:551`) | REVIEWED | Manual RFC5869 SHA-256/HMAC port. |
| `Crypto.Hkdf.Expand` (`src/Crypto.cs:216`) | `hkdf.expand` call (`ts-source/crypto.ts:552`) | REVIEWED | Manual RFC5869 SHA-256/HMAC port. |
| `GetPublicKey(byte[])` (`src/Crypto.cs:344`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | P-256 SEC1 output reviewed. |
| `GetPublicKey(string)` (`src/Crypto.cs:364`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | C# overload for TS union input. |
| `GenerateP256KeyPair` (`src/Crypto.cs:375`) | `generateP256KeyPair` (`ts-source/crypto.ts:358`) | REVIEWED | Random 32-byte private key flow now matches. |
| `HpkeDecrypt` (`src/Crypto.cs:402`) | `hpkeDecrypt` (`ts-source/crypto.ts:308`) | REVIEWED | HPKE step order reviewed. |
| `HpkeEncrypt` (`src/Crypto.cs:454`) | `hpkeEncrypt` (`ts-source/crypto.ts:95`) | REVIEWED | Null plaintext divergence in E. |
| `BuildAdditionalAssociatedData` (`src/Crypto.cs:497`) | `buildAdditionalAssociatedData` (`ts-source/crypto.ts:378`) | REVIEWED | Byte concatenation matches. |
| `CompressRawPublicKey` (`src/Crypto.cs:514`) | `compressRawPublicKey` (`ts-source/crypto.ts:406`) | REVIEWED | Valid-key output matches; empty input divergence in E. |
| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
| `FormatHpkeBuf` (`src/Crypto.cs:602`) | `formatHpkeBuf` (`ts-source/crypto.ts:286`) | REVIEWED | JSON shape/order reviewed. |
| `DecryptCredentialBundle` (`src/Crypto.cs:646`) | `decryptCredentialBundle` (`ts-source/turnkey.ts:72`) | REVIEWED | Base58Check-only path now matches. |
| `EncryptPrivateKeyToBundle` (`src/Crypto.cs:688`) | `encryptPrivateKeyToBundle` (`ts-source/turnkey.ts:317`) | REVIEWED | Main path reviewed; validation/error divergence in E. |
| `DecryptExportBundle` (`src/Crypto.cs:755`) | `decryptExportBundle` (`ts-source/turnkey.ts:116`) | REVIEWED | Main path reviewed; validation/error divergence in E. |
| `VerifySessionJwtSignature` (`src/Crypto.cs:854`) | `verifySessionJwtSignature` (`ts-source/turnkey.ts:444`) | REVIEWED | Double SHA-256/raw signature path reviewed; low-S assumption in G. |
| `DeriveSS` (`src/Crypto.cs:891`) | `deriveSS` (`ts-source/crypto.ts:559`) | REVIEWED | Returns 32-byte x-coordinate shared secret. |
| `GetKemContext` (`src/Crypto.cs:917`) | `getKemContext` (`ts-source/crypto.ts:598`) | REVIEWED | Concatenation matches. |
| `BuildLabeledIkm` (`src/Crypto.cs:924`) | `buildLabeledIkm` (`ts-source/crypto.ts:491`) | REVIEWED | HPKE label layout matches. |
| `BuildLabeledInfo` (`src/Crypto.cs:929`) | `buildLabeledInfo` (`ts-source/crypto.ts:524`) | REVIEWED | Length/version/suite/label/info layout matches. |
| `ExtractAndExpand` (`src/Crypto.cs:945`) | `extractAndExpand` (`ts-source/crypto.ts:545`) | REVIEWED | HKDF argument order matches. |
| `AesGcmDecrypt` (`src/Crypto.cs:951`) | `aesGcmDecrypt` (`ts-source/crypto.ts:584`) | REVIEWED | AES-GCM ciphertext/tag handling reviewed. |
| `AesGcmEncrypt` (`src/Crypto.cs:962`) | `aesGcmEncrypt` (`ts-source/crypto.ts:570`) | REVIEWED | AES-GCM ciphertext||tag output reviewed. |
| `Base64UrlDecode` (`src/Crypto.cs:973`) | inline `toB64` decode (`ts-source/turnkey.ts:461`) | REVIEWED | Base64url transform reviewed. |
| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
| `VerifyEnclaveSignature` (`src/Crypto.cs:1019`) | `verifyEnclaveSignature` (`ts-source/turnkey.ts:234`) | REVIEWED | DER verification reviewed; invalid-signature error divergence in E. |
| `DecodeKey` (`src/Crypto.cs:1065`) | `decodeKey` (`ts-source/turnkey.ts:284`) | REVIEWED | SOLANA/hex decode matches; TS warning omitted. |
| `GetStringOrNull` (`src/Crypto.cs:1088`) | inline JSON property access | REVIEWED | Structural parser helper. |

C. Intentional Adaptations

- C# DTO classes replace TS object/interface params: structural API adaptation; serialized wire fields are controlled explicitly.
- `System.Text.Json` source generation replaces `JSON.stringify` for `FormatHpkeBuf`: property names/order match `encappedPublic`, then `ciphertext`.
- BouncyCastle primitives replace Noble/WebCrypto primitives: intended as primitive substitution, not HPKE/KDF wrapper substitution.
- `Hkdf.Extract/Expand` manually ports RFC5869 instead of calling Noble HKDF: same PRK/OKM bytes.
- `BigInteger` replaces JS `BigInt` for EC math: same non-negative field-coordinate paths.
- C# synchronous methods replace TS `async` wrappers where no actual async work remains: return/error timing differs by language only.
- `DecodeKey` omits TS `console.warn` for unknown key format: return bytes still follow upstream hex fallback.

D. D17 Enforcement Check

Crypto.cs D17 result: no banned HPKE/KDF API string is present.

- No `Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator`.
- No `Org.BouncyCastle.Crypto.Hpke.*`.
- No high-level HPKE wrapper found.
- BouncyCastle is used for ECDH, AES-GCM, ECDSA, BigInteger, EC point operations, and also `Ed25519PrivateKeyParameters` at `src/Crypto.cs:819`. That Ed25519 use is a primitive for Solana export parity, but it is outside the exact D17 allowed list as written.

E. Logic Divergence Findings

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Crypto.cs:168` | `ts-source/math.ts:8` | Negative public `x` differs: C# `BigInteger.Mod` normalizes to non-negative; JS `%` keeps the negative sign. Production EC call sites pass non-negative values. | Use JS-style remainder for exact public helper parity, or make the helper private/non-negative-only. |
| `src/Crypto.cs:459` | `ts-source/crypto.ts:95-138` | `HpkeEncrypt` treats `PlainTextBuf = null` as empty plaintext; TS would fail when `plainTextBuf` is absent/invalid. | Require non-null plaintext and let the upstream-style catch wrap the error. |
| `src/Crypto.cs:520-523` | `ts-source/crypto.ts:412-418` | Empty input to `CompressRawPublicKey` throws in C#; TS returns an empty `Uint8Array` because out-of-bounds typed-array assignment is ignored. | Special-case `rawPublicKey.Length == 0` to return `Array.Empty<byte>()`. |
| `src/Crypto.cs:690-698` | `ts-source/turnkey.ts:325-327` | `EncryptPrivateKeyToBundle` pre-validates required strings before JSON parse/decode; TS parses `importBundle` then decodes `privateKey`, so malformed multi-error inputs have different error precedence. | Move/remove pre-validation to mirror TS operation order if exact error parity is required. |
| `src/Crypto.cs:758-763` | `ts-source/turnkey.ts:124-126` | `DecryptExportBundle` pre-validates params before `JSON.parse`; TS enters the try and parses first. Error precedence/message differ for malformed inputs. | Parse first inside the try, then follow upstream field checks. |
| `src/Crypto.cs:1054-1056` | `ts-source/turnkey.ts:126-135`, `ts-source/turnkey.ts:327-335` | Invalid enclave signature throws `"failed to verify enclave signature"` inside the helper; TS helper returns `false`, and callers throw messages containing the bundle context. | Return `bool` from `VerifyEnclaveSignature` and let callers throw the upstream contextual errors. |

F. Fixture Comparison Gate

No fixture under `tests/Fixtures/` exercises `src/Crypto.cs`. The only fixture present is `tests/Fixtures/encoding/turnkey-encoding-vectors.json`, which is for `Encoding.cs`. `tests/Fixtures/README.md` mentions a future `crypto/` fixture directory, but it is not present.

Result: fixture gate is not satisfiable as “all fixtures match” for `Crypto.cs`; there are no pinned Node-generated Crypto fixtures to compare. Existing `tests/CryptoTests.cs` uses RFC literals, NIST-style known values, negative cases, and randomized C# roundtrips, not pinned upstream fixture files.

G. Unresolved Assumptions

- I could not verify Noble `@noble/curves@1.9.0` low-S verification defaults from source in this checkout. If Noble rejects high-S signatures by default while BouncyCastle accepts them, `VerifyP256RawSignature` and `VerifyEnclaveSignature` need an explicit `s <= n/2` check.
- I did not run `dotnet test`; this environment is read-only and test execution may write `bin/obj` or cache files.
- Positive Turnkey-signed bundle parity remains unverified because no `tests/Fixtures/crypto/` vectors are present.

Pass criterion: not met. B has zero `NOT-REVIEWED` rows, and banned HPKE/KDF APIs are absent, but E has remaining divergence entries and F cannot be confirmed as “all fixtures match” for `Crypto.cs`.


tokens used
138,053
A. Source Pin Acknowledgement

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `73e5109466262896c3383d9c00d8788309eacf21`

B. Method Coverage Table

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---:|---|
| `Crypto.Math.ModSqrt` (`src/Crypto.cs:162`) | `modSqrt` (`ts-source/math.ts:4`) | REVIEWED | Same production EC-coordinate path; negative input divergence in E. |
| `Crypto.Hkdf.Extract` (`src/Crypto.cs:201`) | `hkdf.extract` call (`ts-source/crypto.ts:551`) | REVIEWED | Manual RFC5869 SHA-256/HMAC port. |
| `Crypto.Hkdf.Expand` (`src/Crypto.cs:216`) | `hkdf.expand` call (`ts-source/crypto.ts:552`) | REVIEWED | Manual RFC5869 SHA-256/HMAC port. |
| `GetPublicKey(byte[])` (`src/Crypto.cs:344`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | P-256 SEC1 output reviewed. |
| `GetPublicKey(string)` (`src/Crypto.cs:364`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | C# overload for TS union input. |
| `GenerateP256KeyPair` (`src/Crypto.cs:375`) | `generateP256KeyPair` (`ts-source/crypto.ts:358`) | REVIEWED | Random 32-byte private key flow now matches. |
| `HpkeDecrypt` (`src/Crypto.cs:402`) | `hpkeDecrypt` (`ts-source/crypto.ts:308`) | REVIEWED | HPKE step order reviewed. |
| `HpkeEncrypt` (`src/Crypto.cs:454`) | `hpkeEncrypt` (`ts-source/crypto.ts:95`) | REVIEWED | Null plaintext divergence in E. |
| `BuildAdditionalAssociatedData` (`src/Crypto.cs:497`) | `buildAdditionalAssociatedData` (`ts-source/crypto.ts:378`) | REVIEWED | Byte concatenation matches. |
| `CompressRawPublicKey` (`src/Crypto.cs:514`) | `compressRawPublicKey` (`ts-source/crypto.ts:406`) | REVIEWED | Valid-key output matches; empty input divergence in E. |
| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
| `FormatHpkeBuf` (`src/Crypto.cs:602`) | `formatHpkeBuf` (`ts-source/crypto.ts:286`) | REVIEWED | JSON shape/order reviewed. |
| `DecryptCredentialBundle` (`src/Crypto.cs:646`) | `decryptCredentialBundle` (`ts-source/turnkey.ts:72`) | REVIEWED | Base58Check-only path now matches. |
| `EncryptPrivateKeyToBundle` (`src/Crypto.cs:688`) | `encryptPrivateKeyToBundle` (`ts-source/turnkey.ts:317`) | REVIEWED | Main path reviewed; validation/error divergence in E. |
| `DecryptExportBundle` (`src/Crypto.cs:755`) | `decryptExportBundle` (`ts-source/turnkey.ts:116`) | REVIEWED | Main path reviewed; validation/error divergence in E. |
| `VerifySessionJwtSignature` (`src/Crypto.cs:854`) | `verifySessionJwtSignature` (`ts-source/turnkey.ts:444`) | REVIEWED | Double SHA-256/raw signature path reviewed; low-S assumption in G. |
| `DeriveSS` (`src/Crypto.cs:891`) | `deriveSS` (`ts-source/crypto.ts:559`) | REVIEWED | Returns 32-byte x-coordinate shared secret. |
| `GetKemContext` (`src/Crypto.cs:917`) | `getKemContext` (`ts-source/crypto.ts:598`) | REVIEWED | Concatenation matches. |
| `BuildLabeledIkm` (`src/Crypto.cs:924`) | `buildLabeledIkm` (`ts-source/crypto.ts:491`) | REVIEWED | HPKE label layout matches. |
| `BuildLabeledInfo` (`src/Crypto.cs:929`) | `buildLabeledInfo` (`ts-source/crypto.ts:524`) | REVIEWED | Length/version/suite/label/info layout matches. |
| `ExtractAndExpand` (`src/Crypto.cs:945`) | `extractAndExpand` (`ts-source/crypto.ts:545`) | REVIEWED | HKDF argument order matches. |
| `AesGcmDecrypt` (`src/Crypto.cs:951`) | `aesGcmDecrypt` (`ts-source/crypto.ts:584`) | REVIEWED | AES-GCM ciphertext/tag handling reviewed. |
| `AesGcmEncrypt` (`src/Crypto.cs:962`) | `aesGcmEncrypt` (`ts-source/crypto.ts:570`) | REVIEWED | AES-GCM ciphertext||tag output reviewed. |
| `Base64UrlDecode` (`src/Crypto.cs:973`) | inline `toB64` decode (`ts-source/turnkey.ts:461`) | REVIEWED | Base64url transform reviewed. |
| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
| `VerifyEnclaveSignature` (`src/Crypto.cs:1019`) | `verifyEnclaveSignature` (`ts-source/turnkey.ts:234`) | REVIEWED | DER verification reviewed; invalid-signature error divergence in E. |
| `DecodeKey` (`src/Crypto.cs:1065`) | `decodeKey` (`ts-source/turnkey.ts:284`) | REVIEWED | SOLANA/hex decode matches; TS warning omitted. |
| `GetStringOrNull` (`src/Crypto.cs:1088`) | inline JSON property access | REVIEWED | Structural parser helper. |

C. Intentional Adaptations

- C# DTO classes replace TS object/interface params: structural API adaptation; serialized wire fields are controlled explicitly.
- `System.Text.Json` source generation replaces `JSON.stringify` for `FormatHpkeBuf`: property names/order match `encappedPublic`, then `ciphertext`.
- BouncyCastle primitives replace Noble/WebCrypto primitives: intended as primitive substitution, not HPKE/KDF wrapper substitution.
- `Hkdf.Extract/Expand` manually ports RFC5869 instead of calling Noble HKDF: same PRK/OKM bytes.
- `BigInteger` replaces JS `BigInt` for EC math: same non-negative field-coordinate paths.
- C# synchronous methods replace TS `async` wrappers where no actual async work remains: return/error timing differs by language only.
- `DecodeKey` omits TS `console.warn` for unknown key format: return bytes still follow upstream hex fallback.

D. D17 Enforcement Check

Crypto.cs D17 result: no banned HPKE/KDF API string is present.

- No `Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator`.
- No `Org.BouncyCastle.Crypto.Hpke.*`.
- No high-level HPKE wrapper found.
- BouncyCastle is used for ECDH, AES-GCM, ECDSA, BigInteger, EC point operations, and also `Ed25519PrivateKeyParameters` at `src/Crypto.cs:819`. That Ed25519 use is a primitive for Solana export parity, but it is outside the exact D17 allowed list as written.

E. Logic Divergence Findings

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Crypto.cs:168` | `ts-source/math.ts:8` | Negative public `x` differs: C# `BigInteger.Mod` normalizes to non-negative; JS `%` keeps the negative sign. Production EC call sites pass non-negative values. | Use JS-style remainder for exact public helper parity, or make the helper private/non-negative-only. |
| `src/Crypto.cs:459` | `ts-source/crypto.ts:95-138` | `HpkeEncrypt` treats `PlainTextBuf = null` as empty plaintext; TS would fail when `plainTextBuf` is absent/invalid. | Require non-null plaintext and let the upstream-style catch wrap the error. |
| `src/Crypto.cs:520-523` | `ts-source/crypto.ts:412-418` | Empty input to `CompressRawPublicKey` throws in C#; TS returns an empty `Uint8Array` because out-of-bounds typed-array assignment is ignored. | Special-case `rawPublicKey.Length == 0` to return `Array.Empty<byte>()`. |
| `src/Crypto.cs:690-698` | `ts-source/turnkey.ts:325-327` | `EncryptPrivateKeyToBundle` pre-validates required strings before JSON parse/decode; TS parses `importBundle` then decodes `privateKey`, so malformed multi-error inputs have different error precedence. | Move/remove pre-validation to mirror TS operation order if exact error parity is required. |
| `src/Crypto.cs:758-763` | `ts-source/turnkey.ts:124-126` | `DecryptExportBundle` pre-validates params before `JSON.parse`; TS enters the try and parses first. Error precedence/message differ for malformed inputs. | Parse first inside the try, then follow upstream field checks. |
| `src/Crypto.cs:1054-1056` | `ts-source/turnkey.ts:126-135`, `ts-source/turnkey.ts:327-335` | Invalid enclave signature throws `"failed to verify enclave signature"` inside the helper; TS helper returns `false`, and callers throw messages containing the bundle context. | Return `bool` from `VerifyEnclaveSignature` and let callers throw the upstream contextual errors. |

F. Fixture Comparison Gate

No fixture under `tests/Fixtures/` exercises `src/Crypto.cs`. The only fixture present is `tests/Fixtures/encoding/turnkey-encoding-vectors.json`, which is for `Encoding.cs`. `tests/Fixtures/README.md` mentions a future `crypto/` fixture directory, but it is not present.

Result: fixture gate is not satisfiable as “all fixtures match” for `Crypto.cs`; there are no pinned Node-generated Crypto fixtures to compare. Existing `tests/CryptoTests.cs` uses RFC literals, NIST-style known values, negative cases, and randomized C# roundtrips, not pinned upstream fixture files.

G. Unresolved Assumptions

- I could not verify Noble `@noble/curves@1.9.0` low-S verification defaults from source in this checkout. If Noble rejects high-S signatures by default while BouncyCastle accepts them, `VerifyP256RawSignature` and `VerifyEnclaveSignature` need an explicit `s <= n/2` check.
- I did not run `dotnet test`; this environment is read-only and test execution may write `bin/obj` or cache files.
- Positive Turnkey-signed bundle parity remains unverified because no `tests/Fixtures/crypto/` vectors are present.

Pass criterion: not met. B has zero `NOT-REVIEWED` rows, and banned HPKE/KDF APIs are absent, but E has remaining divergence entries and F cannot be confirmed as “all fixtures match” for `Crypto.cs`.


