# Codex review — src/Crypto.cs — round 1 — 20260523

C# SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
Upstream snapshot: `turnkey-crypto-2.8.8`

---

2026-05-22T23:55:34.565399Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e521d-a470-7e02-9710-99b8e5bdf20d
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/Crypto.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 1 of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA (388a5fc768fd1215928d29f338ef069acd2eb60c).

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

2026-05-22T23:55:35.245107Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll use the code-review workflow here and do a source-pinned comparison against the local upstream snapshot, then report the required A-G sections with file/line evidence.
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
/bin/zsh -lc 'rg -n "turnkey|csharp|sdk|Crypto.cs|peak" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
273:# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
276:applies_to: cwd=/Users/takeshi/.codex/worktrees/1605/peak; reuse_rule=reuse for future `peak` plan/PR review work when the target is PR #330 or a similar docs-plan review in the Peak repo; treat plan conclusions as PR-specific if the file or branch has materially changed
278:## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted
282:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
283:- extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md, updated_at=2026-05-22T04:47:00+00:00, thread_id=None, chronicle synthetic task for the initial PR-review window)
284:- extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md, updated_at=2026-05-22T06:07:00+00:00, thread_id=None, chronicle synthetic task showing the posted inline review state)
288:- PR #330, plans/plans-peak-sdk-csharp.md, docs(plans): add peak-sdk-csharp migration plan, review the new plan to implement csharp sdk, inline comments, TurnkeyJsonContext, submodule init, csharp-publish.yml
294:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, latest verified head `a1adb9a6ef183151430b7b106e3c6bafce649e5e`)
295:- extensions/chronicle/resources/2026-05-22T06-17-00-qyoC-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-17-00-qyoC-10min-memory-summary.md, updated_at=2026-05-22T06:17:00+00:00, thread_id=None, chronicle synthetic task covering GitHub permission modal and remaining inline comments)
296:- extensions/chronicle/resources/2026-05-22T07-03-00-Iktn-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T07-03-00-Iktn-10min-memory-summary.md, updated_at=2026-05-22T07:03:00+00:00, thread_id=None, chronicle synthetic task showing the draft PR browser state after fixes)
300:- resolve可能であればresolveし、追加の指摘があれば指摘追加してください, verify latest head, resolve thread, locally packed .nupkg, feature/plan-peak-sdk-csharp, reviewer waiting
304:- when reviewing a plan/PR, the user asked: "review the new plan to implement csharp sdk" -> default to a PR-style review with concrete findings, not abstract advice [Task 1]
310:- For `peak` doc-plan reviews, read the plan plus nearby repo guidance before commenting; `docs/development-workflow.md`, `docs/sdk-coding-conventions.md`, `docs/sdk-naming-conventions.md`, `docs/sdk-error-handling.md`, and `docs/peak-sdk-unity-internal/README.md` were the useful mismatch detectors [Task 1]
312:- The stable final plan shape after the verify-resolve loop was PR1 build/test only, PR5 locally packed `.nupkg`, PR7 independent `csharp-publish.yml`, and PR4.5 splitting secure-storage adapters between core and Unity packages [Task 2]

codex
The memory hit is only adjacent project context, not evidence for this review. I’m treating the local pinned npm snapshot and the requested C# commit SHA as the review source of truth.
exec
/bin/zsh -lc "git status --short && git rev-parse --show-toplevel && git log -1 --format='%H %ct %s' -- src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc 'cat codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz

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
    19	//   ts-source/turnkey.ts (subset)     -> DecryptCredentialBundle /
    20	//                                        EncryptPrivateKeyToBundle /
    21	//                                        DecryptExportBundle /
    22	//                                        VerifySessionJwtSignature
    23	//
    24	// Out of scope (matches the upstream peak-Unity port):
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
    36	//     BigInteger / EC point primitives only. HPKE, HKDF, Tonelli-Shanks,
    37	//     and bundle parsing logic are direct line-by-line ports of the
    38	//     upstream TypeScript.
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
    49	using System.Linq;
    50	using System.Security.Cryptography;
    51	using System.Text.Json;
    52	using Org.BouncyCastle.Asn1;
    53	using Org.BouncyCastle.Asn1.X9;
    54	using Org.BouncyCastle.Crypto.Agreement;
    55	using Org.BouncyCastle.Crypto.Engines;
    56	using Org.BouncyCastle.Crypto.Generators;
    57	using Org.BouncyCastle.Crypto.Modes;
    58	using Org.BouncyCastle.Crypto.Parameters;
    59	using Org.BouncyCastle.Math;
    60	using Org.BouncyCastle.Security;
    61	
    62	namespace Turnkey
    63	{
    64	    /// <summary>
    65	    /// Cryptographic primitives and Turnkey bundle helpers. 1:1 logical port
    66	    /// of <c>@turnkey/crypto</c> at peak's pinned version 2.8.8.
    67	    /// </summary>
    68	    public static class Crypto
    69	    {
    70	        #region Constants
    71	
    72	        /// <summary>
    73	        /// Constants from upstream <c>constants.ts</c> used by this port.
    74	        /// Bytes match the upstream <c>Uint8Array(...)</c> literals verbatim.
    75	        /// </summary>
    76	        public static class Constants
    77	        {
    78	            // HPKE KEM/HPKE suite IDs.
    79	            public static readonly byte[] SUITE_ID_1 = new byte[] { 75, 69, 77, 0, 16 }; // "KEM\0\x10"
    80	            public static readonly byte[] SUITE_ID_2 = new byte[] { 72, 80, 75, 69, 0, 16, 0, 1, 0, 2 }; // "HPKE\0\x10\0\x01\0\x02"
    81	            public static readonly byte[] HPKE_VERSION = new byte[] { 72, 80, 75, 69, 45, 118, 49 }; // "HPKE-v1"
    82	
    83	            // HPKE labels.
    84	            public static readonly byte[] LABEL_SECRET = new byte[] { 115, 101, 99, 114, 101, 116 }; // "secret"
    85	            public static readonly byte[] LABEL_EAE_PRK = new byte[] { 101, 97, 101, 95, 112, 114, 107 }; // "eae_prk"
    86	            public static readonly byte[] LABEL_SHARED_SECRET = new byte[]
    87	            {
    88	                115, 104, 97, 114, 101, 100, 95, 115, 101, 99, 114, 101, 116, // "shared_secret"
    89	            };
    90	
    91	            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
    92	            public static readonly byte[] AES_KEY_INFO = new byte[]
    93	            {
    94	                0, 32, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 107,
    95	                101, 121, 0, 143, 195, 174, 184, 50, 73, 10, 75, 90, 179, 228, 32, 35, 40,
    96	                125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34, 135, 39, 183, 10, 64, 33,
    97	                18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53, 41, 180, 122, 198, 166, 48,
    98	                185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151, 113, 201, 158, 80,
    99	            };
   100	
   101	            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
   102	            public static readonly byte[] IV_INFO = new byte[]
   103	            {
   104	                0, 12, 72, 80, 75, 69, 45, 118, 49, 72, 80, 75, 69, 0, 16, 0, 1, 0, 2, 98, 97,
   105	                115, 101, 95, 110, 111, 110, 99, 101, 0, 143, 195, 174, 184, 50, 73, 10, 75,
   106	                90, 179, 228, 32, 35, 40, 125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34,
   107	                135, 39, 183, 10, 64, 33, 18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53,
   108	                41, 180, 122, 198, 166, 48, 185, 46, 196, 207, 125, 35, 69, 8, 208, 175, 151,
   109	                113, 201, 158, 80,
   110	            };
   111	
   112	            /// <summary>SEC1 uncompressed P-256 public key length, bytes (0x04 + X + Y).</summary>
   113	            public const int UNCOMPRESSED_PUB_KEY_LENGTH_BYTES = 65;
   114	
   115	            /// <summary>Production signer used by Turnkey to sign export/import bundles.</summary>
   116	            public const string PRODUCTION_SIGNER_SIGN_PUBLIC_KEY =
   117	                "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569";
   118	
   119	            /// <summary>Production notarizer used by Turnkey to sign session JWTs.</summary>
   120	            public const string PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY =
   121	                "04d498aa87ac3bf982ac2b5dd9604d0074905cfbda5d62727c5a237b895e6749205e9f7cd566909c4387f6ca25c308445c60884b788560b785f4a96ac33702a469";
   122	        }
   123	
   124	        #endregion
   125	
   126	        #region Math (math.ts)
   127	
   128	        /// <summary>
   129	        /// Mathematical helpers ported from upstream <c>math.ts</c>.
   130	        /// </summary>
   131	        public static class Math
   132	        {
   133	            /// <summary>
   134	            /// Modular square root via Tonelli-Shanks. Equivalent to
   135	            /// upstream <c>math.ts modSqrt</c>.
   136	            /// </summary>
   137	            /// <param name="x">Value to take the square root of.</param>
   138	            /// <param name="p">Prime modulus.</param>
   139	            /// <returns>One square root of <paramref name="x"/> modulo <paramref name="p"/>.</returns>
   140	            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
   141	            {
   142	                if (p.CompareTo(BigInteger.Zero) <= 0)
   143	                {
   144	                    throw new ArgumentException("p must be positive");
   145	                }
   146	
   147	                // BouncyCastle BigInteger.Mod returns non-negative. Upstream JS
   148	                // BigInt `%` returns a value with the sign of x. For all known
   149	                // call sites in @turnkey/crypto x is already non-negative (x is
   150	                // an EC field-element coordinate), so the two are equivalent.
   151	                var baseVal = x.Mod(p);
   152	
   153	                // p % 4 == 3 fast path (applies to NIST P-256 / P-384 / P-521).
   154	                if (p.TestBit(0) && p.TestBit(1))
   155	                {
   156	                    var q = p.Add(BigInteger.One).ShiftRight(2);
   157	                    var squareRoot = baseVal.ModPow(q, p);
   158	
   159	                    if (!squareRoot.Multiply(squareRoot).Mod(p).Equals(baseVal))
   160	                    {
   161	                        throw new InvalidOperationException("could not find a modular square root");
   162	                    }
   163	                    return squareRoot;
   164	                }
   165	
   166	                throw new InvalidOperationException("unsupported modulus value");
   167	            }
   168	        }
   169	
   170	        #endregion
   171	
   172	        #region HKDF (port of @noble/hashes/hkdf)
   173	
   174	        /// <summary>
   175	        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
   176	        /// which upstream <c>crypto.ts</c> imports.
   177	        /// </summary>
   178	        public static class Hkdf
   179	        {
   180	            private const int HashLen = 32; // SHA-256 output length, bytes.
   181	
   182	            /// <summary>
   183	            /// HKDF Extract: produces a 32-byte pseudorandom key from input
   184	            /// keying material <paramref name="ikm"/> and optional
   185	            /// <paramref name="salt"/>. RFC 5869 §2.2.
   186	            /// </summary>
   187	            public static byte[] Extract(byte[] salt, byte[] ikm)
   188	            {
   189	                if (salt == null || salt.Length == 0)
   190	                {
   191	                    salt = new byte[HashLen]; // RFC 5869: salt defaults to HashLen zero bytes.
   192	                }
   193	                using (var hmac = new HMACSHA256(salt))
   194	                {
   195	                    return hmac.ComputeHash(ikm ?? Array.Empty<byte>());
   196	                }
   197	            }
   198	
   199	            /// <summary>
   200	            /// HKDF Expand: derives <paramref name="length"/> bytes of output
   201	            /// keying material from <paramref name="prk"/> and optional
   202	            /// context <paramref name="info"/>. RFC 5869 §2.3.
   203	            /// </summary>
   204	            public static byte[] Expand(byte[] prk, byte[] info, int length)
   205	            {
   206	                if (prk == null || prk.Length < HashLen)
   207	                {
   208	                    throw new ArgumentException("PRK must be at least HashLen bytes");
   209	                }
   210	                if (length > 255 * HashLen)
   211	                {
   212	                    throw new ArgumentException(
   213	                        "Output length cannot exceed 255 * HashLen (" + (255 * HashLen) + " bytes)");
   214	                }
   215	                if (info == null)
   216	                {
   217	                    info = Array.Empty<byte>();
   218	                }
   219	
   220	                int n = (length + HashLen - 1) / HashLen; // ceil(length / HashLen)
   221	                var okm = new byte[n * HashLen];
   222	                var tPrev = Array.Empty<byte>();
   223	
   224	                using (var hmac = new HMACSHA256(prk))
   225	                {
   226	                    for (int i = 1; i <= n; i++)
   227	                    {
   228	                        var input = new byte[tPrev.Length + info.Length + 1];
   229	                        Array.Copy(tPrev, 0, input, 0, tPrev.Length);
   230	                        Array.Copy(info, 0, input, tPrev.Length, info.Length);
   231	                        input[input.Length - 1] = (byte)i;
   232	
   233	                        var t = hmac.ComputeHash(input);
   234	                        Array.Copy(t, 0, okm, (i - 1) * HashLen, HashLen);
   235	                        tPrev = t;
   236	                    }
   237	                }
   238	
   239	                var result = new byte[length];
   240	                Array.Copy(okm, 0, result, 0, length);
   241	                return result;
   242	            }
   243	        }
   244	
   245	        #endregion
   246	
   247	        #region Nested DTOs
   248	
   249	        /// <summary>HPKE decrypt parameters; equivalent to upstream object literal.</summary>
   250	        public class HpkeDecryptParams
   251	        {
   252	            public byte[]? CiphertextBuf { get; set; }
   253	            public byte[]? EncappedKeyBuf { get; set; }
   254	            public string? ReceiverPriv { get; set; }
   255	        }
   256	
   257	        /// <summary>HPKE encrypt parameters; equivalent to upstream object literal.</summary>
   258	        public class HpkeEncryptParams
   259	        {
   260	            public byte[]? PlainTextBuf { get; set; }

exec
/bin/zsh -lc "find codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8 -maxdepth 3 -type f | sort | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

 succeeded in 4702ms:
2026-05-23 08:56:00.046 xcodebuild[71383:105991345]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:56:00.289 xcodebuild[71383:105991344] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-PWflqXoZ' (errno=Operation not permitted)
2026-05-23 08:56:00.721 xcodebuild[71385:105991370]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:56:00.831 xcodebuild[71385:105991369] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/Crypto.cs-r1-20260523.md
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-xoigQH3q' (errno=Operation not permitted)
2026-05-23 08:56:01.627 xcodebuild[71392:105991409]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:56:01.740 xcodebuild[71392:105991408] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-48oZsYx2' (errno=Operation not permitted)
2026-05-23 08:56:02.078 xcodebuild[71394:105991418]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:56:02.189 xcodebuild[71394:105991417] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-rIz7ox8H' (errno=Operation not permitted)
2026-05-23 08:56:02.949 xcodebuild[71398:105991454]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:56:03.059 xcodebuild[71398:105991453] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-nLl818Nh' (errno=Operation not permitted)
2026-05-23 08:56:03.402 xcodebuild[71400:105991470]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:56:03.513 xcodebuild[71400:105991468] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '261,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   261	            public byte[]? TargetKeyBuf { get; set; }
   262	        }
   263	
   264	        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
   265	        public class KeyPair
   266	        {
   267	            public string PrivateKey { get; set; } = string.Empty;
   268	            public string PublicKey { get; set; } = string.Empty;
   269	            public string PublicKeyUncompressed { get; set; } = string.Empty;
   270	        }
   271	
   272	        /// <summary>
   273	        /// Parameters for <see cref="EncryptPrivateKeyToBundle"/>. Mirrors the
   274	        /// upstream <c>encryptPrivateKeyToBundle</c> options object.
   275	        /// </summary>
   276	        public class EncryptPrivateKeyToBundleParams
   277	        {
   278	            public string? PrivateKey { get; set; }
   279	            public string? ImportBundle { get; set; }
   280	            public string? OrganizationId { get; set; }
   281	            public string? UserId { get; set; }
   282	            public string? KeyFormat { get; set; }
   283	        }
   284	
   285	        /// <summary>
   286	        /// Parameters for <see cref="DecryptExportBundle"/>. Mirrors the
   287	        /// upstream <c>decryptExportBundle</c> options object.
   288	        /// </summary>
   289	        public class DecryptExportBundleParams
   290	        {
   291	            public string? ExportBundle { get; set; }
   292	            public string? EmbeddedKey { get; set; }
   293	            public string? OrganizationId { get; set; }
   294	            public bool ReturnMnemonic { get; set; }
   295	            public string? KeyFormat { get; set; }
   296	        }
   297	
   298	        /// <summary>
   299	        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
   300	        /// match the upstream <c>JSON.stringify({ encappedPublic, ciphertext })</c>
   301	        /// output. Property names use the upstream camelCase names verbatim;
   302	        /// the source-generated context preserves the order.
   303	        /// </summary>
   304	        public class HpkeBundlePayload
   305	        {
   306	            [System.Text.Json.Serialization.JsonPropertyName("encappedPublic")]
   307	            public string EncappedPublic { get; set; } = string.Empty;
   308	
   309	            [System.Text.Json.Serialization.JsonPropertyName("ciphertext")]
   310	            public string Ciphertext { get; set; } = string.Empty;
   311	        }
   312	
   313	        #endregion
   314	
   315	        #region crypto.ts public surface
   316	
   317	        /// <summary>
   318	        /// Derive the SEC1 public key bytes from a private key.
   319	        /// Equivalent to upstream <c>crypto.ts getPublicKey</c>.
   320	        /// </summary>
   321	        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
   322	        {
   323	            if (privateKey == null)
   324	            {
   325	                throw new ArgumentNullException(nameof(privateKey));
   326	            }
   327	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   328	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   329	
   330	            var d = new BigInteger(1, privateKey);
   331	            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
   332	            var publicKeyParams = new ECPublicKeyParameters(
   333	                privateKeyParams.Parameters.G.Multiply(d), domainParams);
   334	
   335	            return publicKeyParams.Q.GetEncoded(isCompressed);
   336	        }
   337	
   338	        /// <summary>
   339	        /// Hex-string overload of <see cref="GetPublicKey(byte[], bool)"/>.
   340	        /// </summary>
   341	        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
   342	        {
   343	            return GetPublicKey(Encoding.Uint8ArrayFromHexString(privateKeyHex), isCompressed);
   344	        }
   345	
   346	        /// <summary>
   347	        /// Generate a random P-256 key pair. Equivalent to upstream
   348	        /// <c>crypto.ts generateP256KeyPair</c>.
   349	        /// </summary>
   350	        public static KeyPair GenerateP256KeyPair()
   351	        {
   352	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   353	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   354	
   355	            var keyGen = new ECKeyPairGenerator();
   356	            var random = new SecureRandom();
   357	            keyGen.Init(new ECKeyGenerationParameters(domainParams, random));
   358	
   359	            var keyPair = keyGen.GenerateKeyPair();
   360	            var privateKey = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
   361	            var publicKeyCompressed = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded(true);
   362	            var publicKeyUncompressed = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded(false);
   363	
   364	            // Pad private key to exactly 32 bytes (upstream returns 32-byte hex).
   365	            if (privateKey.Length < 32)
   366	            {
   367	                var padded = new byte[32];
   368	                Array.Copy(privateKey, 0, padded, 32 - privateKey.Length, privateKey.Length);
   369	                privateKey = padded;
   370	            }
   371	
   372	            return new KeyPair
   373	            {
   374	                PrivateKey = Encoding.Uint8ArrayToHexString(privateKey),
   375	                PublicKey = Encoding.Uint8ArrayToHexString(publicKeyCompressed),
   376	                PublicKeyUncompressed = Encoding.Uint8ArrayToHexString(publicKeyUncompressed),
   377	            };
   378	        }
   379	
   380	        /// <summary>
   381	        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
   382	        /// Equivalent to upstream <c>crypto.ts hpkeDecrypt</c>.
   383	        /// </summary>
   384	        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
   385	        {
   386	            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   387	            var ciphertextBuf = parameters.CiphertextBuf
   388	                                ?? throw new ArgumentNullException(nameof(parameters.CiphertextBuf));
   389	            var encappedKeyBuf = parameters.EncappedKeyBuf
   390	                                 ?? throw new ArgumentNullException(nameof(parameters.EncappedKeyBuf));
   391	            var receiverPriv = parameters.ReceiverPriv
   392	                               ?? throw new ArgumentNullException(nameof(parameters.ReceiverPriv));
   393	
   394	            var receiverPrivBytes = Encoding.Uint8ArrayFromHexString(receiverPriv);
   395	            var receiverPubBuf = GetPublicKey(receiverPrivBytes, false);
   396	
   397	            // Step 1: AAD
   398	            var aad = BuildAdditionalAssociatedData(encappedKeyBuf, receiverPubBuf);
   399	
   400	            // Step 2: ECDH shared secret
   401	            var ss = DeriveSS(encappedKeyBuf, receiverPriv);
   402	
   403	            // Step 3: KEM context
   404	            var kemContext = GetKemContext(encappedKeyBuf, Encoding.Uint8ArrayToHexString(receiverPubBuf));
   405	
   406	            // Step 4: HKDF derive shared secret
   407	            var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
   408	            var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
   409	            var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);
   410	
   411	            // Step 5: derive AES key
   412	            ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
   413	            info = Constants.AES_KEY_INFO;
   414	            var key = ExtractAndExpand(sharedSecret, ikm, info, 32);
   415	
   416	            // Step 6: derive IV
   417	            info = Constants.IV_INFO;
   418	            var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);
   419	
   420	            // Step 7: AES-GCM decrypt
   421	            return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
   422	        }
   423	
   424	        /// <summary>
   425	        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
   426	        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
   427	        /// </summary>
   428	        /// <returns>
   429	        /// The byte concatenation
   430	        /// <c>compressed(senderPub) || ciphertext</c>, ready to feed back into
   431	        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
   432	        /// </returns>
   433	        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
   434	        {
   435	            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   436	            var plainTextBuf = parameters.PlainTextBuf ?? Array.Empty<byte>();
   437	            var targetKeyBuf = parameters.TargetKeyBuf
   438	                               ?? throw new ArgumentNullException(nameof(parameters.TargetKeyBuf));
   439	
   440	            // Ephemeral sender key pair.
   441	            var ephemeralKeyPair = GenerateP256KeyPair();
   442	            var senderPrivBuf = Encoding.Uint8ArrayFromHexString(ephemeralKeyPair.PrivateKey);
   443	            var senderPubBuf = Encoding.Uint8ArrayFromHexString(ephemeralKeyPair.PublicKeyUncompressed);
   444	
   445	            var aad = BuildAdditionalAssociatedData(senderPubBuf, targetKeyBuf);
   446	            var ss = DeriveSS(targetKeyBuf, Encoding.Uint8ArrayToHexString(senderPrivBuf));
   447	            var kemContext = GetKemContext(senderPubBuf, Encoding.Uint8ArrayToHexString(targetKeyBuf));
   448	
   449	            var ikm = BuildLabeledIkm(Constants.LABEL_EAE_PRK, ss, Constants.SUITE_ID_1);
   450	            var info = BuildLabeledInfo(Constants.LABEL_SHARED_SECRET, kemContext, Constants.SUITE_ID_1, 32);
   451	            var sharedSecret = ExtractAndExpand(Array.Empty<byte>(), ikm, info, 32);
   452	
   453	            ikm = BuildLabeledIkm(Constants.LABEL_SECRET, Array.Empty<byte>(), Constants.SUITE_ID_2);
   454	            info = Constants.AES_KEY_INFO;
   455	            var key = ExtractAndExpand(sharedSecret, ikm, info, 32);
   456	
   457	            info = Constants.IV_INFO;
   458	            var iv = ExtractAndExpand(sharedSecret, ikm, info, 12);
   459	
   460	            var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
   461	
   462	            // Upstream returns compressedSender || ciphertext.
   463	            var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
   464	            return Encoding.ConcatUint8Arrays(compressedSenderBuf, encryptedData);
   465	        }
   466	
   467	        /// <summary>
   468	        /// HPKE additional associated data builder.
   469	        /// Equivalent to upstream <c>crypto.ts buildAdditionalAssociatedData</c>.
   470	        /// </summary>
   471	        public static byte[] BuildAdditionalAssociatedData(byte[] senderPubBuf, byte[] receiverPubBuf)
   472	        {
   473	            return Encoding.ConcatUint8Arrays(senderPubBuf, receiverPubBuf);
   474	        }
   475	
   476	        /// <summary>
   477	        /// Compress a 65-byte uncompressed P-256 public key into 33 bytes.
   478	        /// Equivalent to upstream <c>crypto.ts compressRawPublicKey</c>.
   479	        /// </summary>
   480	        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
   481	        {
   482	            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
   483	            if (rawPublicKey.Length != Constants.UNCOMPRESSED_PUB_KEY_LENGTH_BYTES || rawPublicKey[0] != 0x04)
   484	            {
   485	                throw new ArgumentException("Invalid uncompressed public key");
   486	            }
   487	
   488	            var x = new byte[32];
   489	            Array.Copy(rawPublicKey, 1, x, 0, 32);
   490	
   491	            // 0x02 if Y is even, 0x03 if Y is odd. Y's parity is determined by
   492	            // its least-significant bit, which is the last byte's LSB.
   493	            var lastByte = rawPublicKey[64];
   494	            var prefix = (byte)((lastByte & 1) == 0 ? 0x02 : 0x03);
   495	
   496	            var compressed = new byte[33];
   497	            compressed[0] = prefix;
   498	            Array.Copy(x, 0, compressed, 1, 32);
   499	            return compressed;
   500	        }
   501	
   502	        /// <summary>
   503	        /// Uncompress a 33-byte compressed P-256 public key into 65 bytes.
   504	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
   505	        /// </summary>
   506	        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey)
   507	        {
   508	            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
   509	            if (rawPublicKey.Length != CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
   510	            {
   511	                throw new ArgumentException(
   512	                    "Invalid compressed public key size: " + rawPublicKey.Length);
   513	            }
   514	            if (rawPublicKey[0] != 0x02 && rawPublicKey[0] != 0x03)
   515	            {
   516	                throw new ArgumentException("failed to uncompress raw public key: invalid prefix");
   517	            }
   518	
   519	            // 0x03 => odd Y; 0x02 => even Y.
   520	            bool lsb = rawPublicKey[0] == 0x03;
   521	
   522	            // x = BigInt("0x" + hex(rawPublicKey[1..33])); upstream parses via hex string.
   523	            var xBytes = new byte[32];
   524	            Array.Copy(rawPublicKey, 1, xBytes, 0, 32);
   525	            var xHex = Encoding.Uint8ArrayToHexString(xBytes);
   526	            var x = new BigInteger(xHex, 16);
   527	
   528	            // NIST P-256: y^2 = x^3 + a*x + b (mod p), with a = p - 3.
   529	            var p = new BigInteger(CryptoConstants.P256_P);
   530	            var b = new BigInteger(CryptoConstants.P256_B, 16);
   531	            var a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
   532	
   533	            // Upstream computes rhs = ((x*x + a) * x + b) % p in a single chain.
   534	            var x2 = x.Multiply(x).Mod(p);
   535	            var x2PlusA = x2.Add(a).Mod(p);
   536	            var rhs = x2PlusA.Multiply(x).Add(b).Mod(p);
   537	
   538	            var y = Math.ModSqrt(rhs, p);
   539	
   540	            // Pick the root whose parity matches the requested LSB.
   541	            if (lsb != y.TestBit(0))
   542	            {
   543	                y = p.Subtract(y).Mod(p);
   544	            }
   545	
   546	            // Range check (matches upstream's defensive check).
   547	            if (x.SignValue < 0 || x.CompareTo(p) >= 0)
   548	            {
   549	                throw new InvalidOperationException("x is out of range");
   550	            }
   551	            if (y.SignValue < 0 || y.CompareTo(p) >= 0)
   552	            {
   553	                throw new InvalidOperationException("y is out of range");
   554	            }
   555	
   556	            // Assemble 0x04 || X || Y (each 32 bytes, big-endian, zero-padded).
   557	            var uncompressed = new byte[65];
   558	            uncompressed[0] = 0x04;
   559	
   560	            var xHexOut = x.ToString(16).ToLowerInvariant().PadLeft(64, '0');
   561	            var yHexOut = y.ToString(16).ToLowerInvariant().PadLeft(64, '0');
   562	
   563	            var xOutputBytes = Encoding.Uint8ArrayFromHexString(xHexOut);
   564	            var yOutputBytes = Encoding.Uint8ArrayFromHexString(yHexOut);
   565	
   566	            Array.Copy(xOutputBytes, 0, uncompressed, 1, 32);
   567	            Array.Copy(yOutputBytes, 0, uncompressed, 33, 32);
   568	            return uncompressed;
   569	        }
   570	
   571	        /// <summary>
   572	        /// Encode the HPKE result <c>compressed(senderPub) || ciphertext</c>
   573	        /// as the Turnkey import-bundle JSON envelope.
   574	        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
   575	        /// </summary>
   576	        public static string FormatHpkeBuf(byte[] encryptedBuf)
   577	        {
   578	            if (encryptedBuf == null) throw new ArgumentNullException(nameof(encryptedBuf));
   579	            if (encryptedBuf.Length <= CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
   580	            {
   581	                throw new ArgumentException("Encrypted buffer too small");
   582	            }
   583	
   584	            var compressedEncappedPublic = new byte[CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
   585	            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE);
   586	
   587	            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);
   588	
   589	            var ciphertext = new byte[encryptedBuf.Length - CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
   590	            Array.Copy(encryptedBuf, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE, ciphertext, 0, ciphertext.Length);
   591	
   592	            var payload = new HpkeBundlePayload
   593	            {
   594	                EncappedPublic = Encoding.Uint8ArrayToHexString(encappedPublicUncompressed),
   595	                Ciphertext = Encoding.Uint8ArrayToHexString(ciphertext),
   596	            };
   597	
   598	            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
   599	        }
   600	
   601	        #endregion
   602	
   603	        #region turnkey.ts public surface (subset matched to Unity / peak usage)
   604	
   605	        /// <summary>
   606	        /// Decrypt a Turnkey credential bundle.
   607	        /// Equivalent to upstream <c>turnkey.ts decryptCredentialBundle</c>.
   608	        /// </summary>
   609	        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string targetPrivateKey)
   610	        {
   611	            byte[] bundleBytes;
   612	            try
   613	            {
   614	                bundleBytes = Encoding.Base58CheckDecode(encryptedCredentialBundle);
   615	            }
   616	            catch
   617	            {
   618	                bundleBytes = Encoding.Base58Decode(encryptedCredentialBundle);
   619	            }
   620	

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '1,340p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '621,1040p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   621	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
   622	            if (bundleBytes.Length <= compressedSize)
   623	            {
   624	                throw new InvalidOperationException(
   625	                    "Bundle size " + bundleBytes.Length
   626	                    + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
   627	            }
   628	
   629	            var compressedKey = new byte[compressedSize];
   630	            Array.Copy(bundleBytes, 0, compressedKey, 0, compressedSize);
   631	
   632	            var ciphertext = new byte[bundleBytes.Length - compressedSize];
   633	            Array.Copy(bundleBytes, compressedSize, ciphertext, 0, ciphertext.Length);
   634	
   635	            var encappedKey = UncompressRawPublicKey(compressedKey);
   636	
   637	            var decryptedData = HpkeDecrypt(new HpkeDecryptParams
   638	            {
   639	                CiphertextBuf = ciphertext,
   640	                EncappedKeyBuf = encappedKey,
   641	                ReceiverPriv = targetPrivateKey,
   642	            });
   643	
   644	            return Encoding.Uint8ArrayToHexString(decryptedData);
   645	        }
   646	
   647	        /// <summary>
   648	        /// Encrypt a private key into a Turnkey import bundle.
   649	        /// Equivalent to upstream <c>turnkey.ts encryptPrivateKeyToBundle</c>.
   650	        /// </summary>
   651	        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
   652	        {
   653	            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   654	            if (string.IsNullOrWhiteSpace(parameters.PrivateKey))
   655	                throw new ArgumentException("Private key is required", nameof(parameters.PrivateKey));
   656	            if (string.IsNullOrWhiteSpace(parameters.ImportBundle))
   657	                throw new ArgumentException("Import bundle is required", nameof(parameters.ImportBundle));
   658	            if (string.IsNullOrWhiteSpace(parameters.OrganizationId))
   659	                throw new ArgumentException("Organization ID is required", nameof(parameters.OrganizationId));
   660	            if (string.IsNullOrWhiteSpace(parameters.UserId))
   661	                throw new ArgumentException("User ID is required", nameof(parameters.UserId));
   662	
   663	            try
   664	            {
   665	                using var bundleDoc = JsonDocument.Parse(parameters.ImportBundle!);
   666	                var bundle = bundleDoc.RootElement;
   667	
   668	                string? enclaveQuorumPublic = GetStringOrNull(bundle, "enclaveQuorumPublic");
   669	                string? dataSignature = GetStringOrNull(bundle, "dataSignature");
   670	                string? signedDataHex = GetStringOrNull(bundle, "data");
   671	
   672	                if (string.IsNullOrEmpty(enclaveQuorumPublic)
   673	                    || string.IsNullOrEmpty(dataSignature)
   674	                    || string.IsNullOrEmpty(signedDataHex))
   675	                {
   676	                    throw new InvalidOperationException(
   677	                        "Invalid import bundle format - missing required fields");
   678	                }
   679	
   680	                VerifyEnclaveSignature(enclaveQuorumPublic!, dataSignature!, signedDataHex!);
   681	
   682	                var signedDataBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
   683	                using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
   684	                var signed = signedDoc.RootElement;
   685	
   686	                string? orgId = GetStringOrNull(signed, "organizationId");
   687	                if (!string.Equals(orgId, parameters.OrganizationId, StringComparison.Ordinal))
   688	                {
   689	                    throw new InvalidOperationException(
   690	                        "Organization ID mismatch. Expected: " + parameters.OrganizationId
   691	                        + ", got: " + orgId);
   692	                }
   693	
   694	                string? userId = GetStringOrNull(signed, "userId");
   695	                if (!string.Equals(userId, parameters.UserId, StringComparison.Ordinal))
   696	                {
   697	                    throw new InvalidOperationException(
   698	                        "User ID mismatch. Expected: " + parameters.UserId + ", got: " + userId);
   699	                }
   700	
   701	                string? targetPublic = GetStringOrNull(signed, "targetPublic");
   702	                if (string.IsNullOrEmpty(targetPublic))
   703	                {
   704	                    throw new InvalidOperationException("Import bundle missing targetPublic value");
   705	                }
   706	
   707	                var targetKeyBuf = Encoding.Uint8ArrayFromHexString(targetPublic!);
   708	                var plainTextBuf = DecodeKey(parameters.PrivateKey!, parameters.KeyFormat);
   709	
   710	                var encryptedBuf = HpkeEncrypt(new HpkeEncryptParams
   711	                {
   712	                    PlainTextBuf = plainTextBuf,
   713	                    TargetKeyBuf = targetKeyBuf,
   714	                });
   715	
   716	                return FormatHpkeBuf(encryptedBuf);
   717	            }
   718	            catch (Exception err) when (!(err is ArgumentException) && !(err is ArgumentNullException))
   719	            {
   720	                throw new InvalidOperationException(
   721	                    "Error encrypting private key bundle: " + err.Message, err);
   722	            }
   723	        }
   724	
   725	        /// <summary>
   726	        /// Decrypt a Turnkey export bundle (legacy or current envelope).
   727	        /// Equivalent to upstream <c>turnkey.ts decryptExportBundle</c>.
   728	        /// </summary>
   729	        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
   730	        {
   731	            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
   732	            if (string.IsNullOrWhiteSpace(parameters.ExportBundle))
   733	                throw new ArgumentException("Export bundle is required", nameof(parameters.ExportBundle));
   734	            if (string.IsNullOrWhiteSpace(parameters.EmbeddedKey))
   735	                throw new ArgumentException("Embedded key is required", nameof(parameters.EmbeddedKey));
   736	            if (string.IsNullOrWhiteSpace(parameters.OrganizationId))
   737	                throw new ArgumentException("Organization ID is required", nameof(parameters.OrganizationId));
   738	
   739	            try
   740	            {
   741	                using var bundleDoc = JsonDocument.Parse(parameters.ExportBundle!);
   742	                var bundle = bundleDoc.RootElement;
   743	
   744	                string? encappedPublic = GetStringOrNull(bundle, "encappedPublic");
   745	                string? ciphertextStr = GetStringOrNull(bundle, "ciphertext");
   746	                string? signature = GetStringOrNull(bundle, "signature");
   747	                string? signedData = GetStringOrNull(bundle, "signedData");
   748	
   749	                JsonDocument? signedDoc = null;
   750	                JsonElement signedDataObj = default;
   751	                bool hasSignedDataObj = false;
   752	
   753	                try
   754	                {
   755	                    if (!string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(signedData))
   756	                    {
   757	                        // Legacy envelope: ECDSA over the UTF-8 bytes of `signedData`.
   758	                        if (!VerifySignature(Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY, signature!, signedData!))
   759	                        {
   760	                            throw new InvalidOperationException("Invalid signature on export bundle");
   761	                        }
   762	                        signedDoc = JsonDocument.Parse(signedData!);
   763	                        signedDataObj = signedDoc.RootElement;
   764	                        hasSignedDataObj = true;
   765	                        encappedPublic ??= GetStringOrNull(signedDataObj, "encappedPublic");
   766	                        ciphertextStr ??= GetStringOrNull(signedDataObj, "ciphertext");
   767	                    }
   768	                    else
   769	                    {
   770	                        // Current envelope: enclave-quorum-signed hex blob.
   771	                        string? dataHex = GetStringOrNull(bundle, "data");
   772	                        string? dataSignature = GetStringOrNull(bundle, "dataSignature");
   773	                        string? enclaveQuorumPublic = GetStringOrNull(bundle, "enclaveQuorumPublic");
   774	
   775	                        if (string.IsNullOrEmpty(dataHex)
   776	                            || string.IsNullOrEmpty(dataSignature)
   777	                            || string.IsNullOrEmpty(enclaveQuorumPublic))
   778	                        {
   779	                            throw new InvalidOperationException(
   780	                                "Invalid export bundle format - missing required fields");
   781	                        }
   782	
   783	                        VerifyEnclaveSignature(enclaveQuorumPublic!, dataSignature!, dataHex!);
   784	
   785	                        var signedDataBytes = Encoding.Uint8ArrayFromHexString(dataHex!);
   786	                        var signedJson = System.Text.Encoding.UTF8.GetString(signedDataBytes);
   787	                        signedDoc = JsonDocument.Parse(signedJson);
   788	                        signedDataObj = signedDoc.RootElement;
   789	                        hasSignedDataObj = true;
   790	                        encappedPublic = GetStringOrNull(signedDataObj, "encappedPublic");
   791	                        ciphertextStr = GetStringOrNull(signedDataObj, "ciphertext");
   792	                    }
   793	
   794	                    if (hasSignedDataObj)
   795	                    {
   796	                        string? bundleOrgId = GetStringOrNull(signedDataObj, "organizationId");
   797	                        if (!string.IsNullOrEmpty(bundleOrgId)
   798	                            && !string.Equals(bundleOrgId, parameters.OrganizationId, StringComparison.Ordinal))
   799	                        {
   800	                            throw new InvalidOperationException(
   801	                                "Organization ID mismatch. Expected: " + parameters.OrganizationId
   802	                                + ", got: " + bundleOrgId);
   803	                        }
   804	                    }
   805	
   806	                    if (string.IsNullOrEmpty(encappedPublic) || string.IsNullOrEmpty(ciphertextStr))
   807	                    {
   808	                        throw new InvalidOperationException(
   809	                            "Invalid export bundle format - missing HPKE payload");
   810	                    }
   811	
   812	                    var encappedKeyBuf = Encoding.Uint8ArrayFromHexString(encappedPublic!);
   813	                    var ciphertextBuf = Encoding.Uint8ArrayFromHexString(ciphertextStr!);
   814	
   815	                    var decryptedData = HpkeDecrypt(new HpkeDecryptParams
   816	                    {
   817	                        CiphertextBuf = ciphertextBuf,
   818	                        EncappedKeyBuf = encappedKeyBuf,
   819	                        ReceiverPriv = parameters.EmbeddedKey,
   820	                    });
   821	
   822	                    if (parameters.ReturnMnemonic)
   823	                    {
   824	                        return Encoding.Uint8ArrayToString(decryptedData);
   825	                    }
   826	
   827	                    if (string.Equals(parameters.KeyFormat, "SOLANA", StringComparison.OrdinalIgnoreCase))
   828	                    {
   829	                        return Encoding.Base58Encode(decryptedData);
   830	                    }
   831	
   832	                    return Encoding.Uint8ArrayToHexString(decryptedData);
   833	                }
   834	                finally
   835	                {
   836	                    signedDoc?.Dispose();
   837	                }
   838	            }
   839	            catch (Exception err) when (!(err is ArgumentException) && !(err is ArgumentNullException))
   840	            {
   841	                throw new InvalidOperationException(
   842	                    "Error decrypting export bundle: " + err.Message, err);
   843	            }
   844	        }
   845	
   846	        /// <summary>
   847	        /// Verify the ECDSA signature on a Turnkey session JWT against the
   848	        /// production notarizer's P-256 key.
   849	        /// Equivalent to upstream <c>turnkey.ts verifySessionJwtSignature</c>.
   850	        /// </summary>
   851	        public static bool VerifySessionJwtSignature(string jwt)
   852	        {
   853	            try
   854	            {
   855	                if (string.IsNullOrEmpty(jwt))
   856	                {
   857	                    return false;
   858	                }
   859	                var parts = jwt.Split('.');
   860	                if (parts.Length != 3)
   861	                {
   862	                    return false;
   863	                }
   864	
   865	                string headerB64 = parts[0];
   866	                string payloadB64 = parts[1];
   867	                string signatureB64 = parts[2];
   868	                string signingInput = headerB64 + "." + payloadB64;
   869	
   870	                // Upstream: double-SHA-256 of the signing input. The signing
   871	                // algorithm is "NONEwithECDSA" with the pre-hashed digest fed in.
   872	                byte[] msgDigest;
   873	                using (var sha256 = SHA256.Create())
   874	                {
   875	                    var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
   876	                    msgDigest = sha256.ComputeHash(h1);
   877	                }
   878	
   879	                byte[] signature = Base64UrlDecode(signatureB64);
   880	                if (signature.Length != 64)
   881	                {
   882	                    return false;
   883	                }
   884	
   885	                var publicKey = Encoding.Uint8ArrayFromHexString(Constants.PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY);
   886	                return VerifyP256RawSignature(publicKey, signature, msgDigest);
   887	            }
   888	            catch
   889	            {
   890	                return false;
   891	            }
   892	        }
   893	
   894	        #endregion
   895	
   896	        #region Private helpers
   897	
   898	        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
   899	        {
   900	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   901	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   902	
   903	            var privBytes = Encoding.Uint8ArrayFromHexString(privHex);
   904	            var d = new BigInteger(1, privBytes);
   905	            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
   906	
   907	            var point = curve.Curve.DecodePoint(encappedKeyBuf);
   908	            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
   909	
   910	            var agreement = new ECDHBasicAgreement();
   911	            agreement.Init(privateKeyParams);
   912	            var sharedSecretBig = agreement.CalculateAgreement(publicKeyParams);
   913	            var ss = sharedSecretBig.ToByteArrayUnsigned();
   914	
   915	            if (ss.Length < 32)
   916	            {
   917	                var padded = new byte[32];
   918	                Array.Copy(ss, 0, padded, 32 - ss.Length, ss.Length);
   919	                ss = padded;
   920	            }
   921	            return ss;
   922	        }
   923	
   924	        private static byte[] GetKemContext(byte[] encappedKeyBuf, string publicKey)
   925	        {
   926	            return Encoding.ConcatUint8Arrays(
   927	                encappedKeyBuf,
   928	                Encoding.Uint8ArrayFromHexString(publicKey));
   929	        }
   930	
   931	        private static byte[] BuildLabeledIkm(byte[] label, byte[] ikm, byte[] suiteId)
   932	        {
   933	            // Upstream `buildLabeledIkm` returns HPKE_VERSION || suiteId || label || ikm.
   934	            return Encoding.ConcatUint8Arrays(Constants.HPKE_VERSION, suiteId, label, ikm);
   935	        }
   936	
   937	        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
   938	        {
   939	            // Upstream layout: [0, len, ...HPKE_VERSION, ...suiteId, ...label, ...info].
   940	            // suiteIdStartIndex == 2 + HPKE_VERSION.Length == 9.
   941	            const int suiteIdStartIndex = 9;
   942	            var ret = new byte[suiteIdStartIndex + suiteId.Length + label.Length + info.Length];
   943	
   944	            ret[0] = 0;
   945	            ret[1] = (byte)len;
   946	
   947	            Array.Copy(Constants.HPKE_VERSION, 0, ret, 2, Constants.HPKE_VERSION.Length);
   948	            Array.Copy(suiteId, 0, ret, suiteIdStartIndex, suiteId.Length);
   949	            Array.Copy(label, 0, ret, suiteIdStartIndex + suiteId.Length, label.Length);
   950	            Array.Copy(info, 0, ret, suiteIdStartIndex + suiteId.Length + label.Length, info.Length);
   951	
   952	            return ret;
   953	        }
   954	
   955	        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
   956	        {
   957	            var prk = Hkdf.Extract(sharedSecret, ikm);
   958	            return Hkdf.Expand(prk, info, len);
   959	        }
   960	
   961	        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
   962	        {
   963	            var cipher = new GcmBlockCipher(new AesEngine());
   964	            cipher.Init(false, new AeadParameters(new KeyParameter(key), 128, iv, aad));
   965	
   966	            var output = new byte[cipher.GetOutputSize(encryptedData.Length)];
   967	            int len = cipher.ProcessBytes(encryptedData, 0, encryptedData.Length, output, 0);
   968	            cipher.DoFinal(output, len);
   969	            return output;
   970	        }
   971	
   972	        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
   973	        {
   974	            var cipher = new GcmBlockCipher(new AesEngine());
   975	            cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, iv, aad));
   976	
   977	            var output = new byte[cipher.GetOutputSize(plainData.Length)];
   978	            int len = cipher.ProcessBytes(plainData, 0, plainData.Length, output, 0);
   979	            cipher.DoFinal(output, len);
   980	            return output;
   981	        }
   982	
   983	        private static byte[] Base64UrlDecode(string input)
   984	        {
   985	            // Strict base64url-decoder for JWT signatures. Upstream uses
   986	            // Buffer.from(input, "base64url") in Node which is strict on
   987	            // characters; we mirror that here (no lenient atob).
   988	            var output = input.Replace('-', '+').Replace('_', '/');
   989	            switch (output.Length % 4)
   990	            {
   991	                case 2: output += "=="; break;
   992	                case 3: output += "="; break;
   993	            }
   994	            return Convert.FromBase64String(output);
   995	        }
   996	
   997	        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
   998	        {
   999	            try
  1000	            {
  1001	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
  1002	                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
  1003	
  1004	                var point = curve.Curve.DecodePoint(publicKeyBytes);
  1005	                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
  1006	
  1007	                // Convert raw r||s (64 bytes) to DER-encoded sequence.
  1008	                var r = new BigInteger(1, signatureRaw, 0, 32);
  1009	                var s = new BigInteger(1, signatureRaw, 32, 32);
  1010	                byte[] derSignature = new DerSequence(new DerInteger(r), new DerInteger(s)).GetDerEncoded();
  1011	
  1012	                // The message is already a digest; use NONEwithECDSA.
  1013	                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
  1014	                signer.Init(false, publicKeyParams);
  1015	                signer.BlockUpdate(messageDigest, 0, messageDigest.Length);
  1016	                return signer.VerifySignature(derSignature);
  1017	            }
  1018	            catch
  1019	            {
  1020	                return false;
  1021	            }
  1022	        }
  1023	
  1024	        private static void VerifyEnclaveSignature(string enclaveQuorumPublic, string signatureHex, string signedDataHex)
  1025	        {
  1026	            if (!string.Equals(enclaveQuorumPublic, Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY, StringComparison.OrdinalIgnoreCase))
  1027	            {
  1028	                throw new InvalidOperationException(
  1029	                    "Signer key " + enclaveQuorumPublic + " is not recognized. Expected: "
  1030	                    + Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY);
  1031	            }
  1032	
  1033	            var publicKeyBytes = Encoding.Uint8ArrayFromHexString(Constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY);
  1034	            var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex);
  1035	            var messageBytes = Encoding.Uint8ArrayFromHexString(signedDataHex);
  1036	
  1037	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
  1038	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
  1039	            var point = curve.Curve.DecodePoint(publicKeyBytes);
  1040	            var publicKeyParams = new ECPublicKeyParameters(point, domainParams);

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '340,760p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/math.ts | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '1041,1300p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
  1041	
  1042	            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
  1043	            signer.Init(false, publicKeyParams);
  1044	            signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
  1045	
  1046	            if (!signer.VerifySignature(signatureBytes))
  1047	            {
  1048	                throw new InvalidOperationException("Failed to verify enclave signature");
  1049	            }
  1050	        }
  1051	
  1052	        private static bool VerifySignature(string publicKeyHex, string signatureHex, string message)
  1053	        {
  1054	            try
  1055	            {
  1056	                var publicKeyBytes = Encoding.Uint8ArrayFromHexString(publicKeyHex);
  1057	                var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex);
  1058	                var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
  1059	
  1060	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
  1061	                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
  1062	
  1063	                var point = curve.Curve.DecodePoint(publicKeyBytes);
  1064	                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
  1065	
  1066	                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
  1067	                signer.Init(false, publicKeyParams);
  1068	                signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
  1069	                return signer.VerifySignature(signatureBytes);
  1070	            }
  1071	            catch
  1072	            {
  1073	                return false;
  1074	            }
  1075	        }
  1076	
  1077	        private static byte[] DecodeKey(string privateKey, string? keyFormat)
  1078	        {
  1079	            if (string.Equals(keyFormat, "SOLANA", StringComparison.OrdinalIgnoreCase))
  1080	            {
  1081	                var decoded = Encoding.Base58Decode(privateKey);
  1082	                if (decoded.Length != 64)
  1083	                {
  1084	                    throw new InvalidOperationException(
  1085	                        "Invalid Solana private key length. Expected 64 bytes, got " + decoded.Length + ".");
  1086	                }
  1087	                var privateKeyBytes = new byte[32];
  1088	                Array.Copy(decoded, 0, privateKeyBytes, 0, 32);
  1089	                return privateKeyBytes;
  1090	            }
  1091	
  1092	            string normalized = privateKey.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
  1093	                ? privateKey.Substring(2)
  1094	                : privateKey;
  1095	            return Encoding.Uint8ArrayFromHexString(normalized);
  1096	        }
  1097	
  1098	        private static string? GetStringOrNull(JsonElement element, string propertyName)
  1099	        {
  1100	            if (element.ValueKind != JsonValueKind.Object)
  1101	            {
  1102	                return null;
  1103	            }
  1104	            if (!element.TryGetProperty(propertyName, out var value))
  1105	            {
  1106	                return null;
  1107	            }
  1108	            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
  1109	            {
  1110	                return null;
  1111	            }
  1112	            // Upstream uses ?.toString() which for a non-string JToken returns
  1113	            // its string representation. For Turnkey bundles every queried
  1114	            // field is a string; treat non-string values as null so callers
  1115	            // see the same "missing field" path they would in JS.
  1116	            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
  1117	        }
  1118	
  1119	        #endregion
  1120	    }
  1121	}

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '1,360p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc 'rg -n "public static|private static|internal|public class|private const|public const|static readonly" src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
14://                                        FormatHpkeBuf, plus the internal
68:    public static class Crypto
76:        public static class Constants
79:            public static readonly byte[] SUITE_ID_1 = new byte[] { 75, 69, 77, 0, 16 }; // "KEM\0\x10"
80:            public static readonly byte[] SUITE_ID_2 = new byte[] { 72, 80, 75, 69, 0, 16, 0, 1, 0, 2 }; // "HPKE\0\x10\0\x01\0\x02"
81:            public static readonly byte[] HPKE_VERSION = new byte[] { 72, 80, 75, 69, 45, 118, 49 }; // "HPKE-v1"
84:            public static readonly byte[] LABEL_SECRET = new byte[] { 115, 101, 99, 114, 101, 116 }; // "secret"
85:            public static readonly byte[] LABEL_EAE_PRK = new byte[] { 101, 97, 101, 95, 112, 114, 107 }; // "eae_prk"
86:            public static readonly byte[] LABEL_SHARED_SECRET = new byte[]
92:            public static readonly byte[] AES_KEY_INFO = new byte[]
102:            public static readonly byte[] IV_INFO = new byte[]
113:            public const int UNCOMPRESSED_PUB_KEY_LENGTH_BYTES = 65;
116:            public const string PRODUCTION_SIGNER_SIGN_PUBLIC_KEY =
120:            public const string PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY =
131:        public static class Math
140:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
178:        public static class Hkdf
180:            private const int HashLen = 32; // SHA-256 output length, bytes.
187:            public static byte[] Extract(byte[] salt, byte[] ikm)
204:            public static byte[] Expand(byte[] prk, byte[] info, int length)
250:        public class HpkeDecryptParams
258:        public class HpkeEncryptParams
265:        public class KeyPair
276:        public class EncryptPrivateKeyToBundleParams
289:        public class DecryptExportBundleParams
304:        public class HpkeBundlePayload
321:        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
341:        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
350:        public static KeyPair GenerateP256KeyPair()
384:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
433:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
471:        public static byte[] BuildAdditionalAssociatedData(byte[] senderPubBuf, byte[] receiverPubBuf)
480:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
506:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey)
576:        public static string FormatHpkeBuf(byte[] encryptedBuf)
609:        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string targetPrivateKey)
651:        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
729:        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
851:        public static bool VerifySessionJwtSignature(string jwt)
898:        private static byte[] DeriveSS(byte[] encappedKeyBuf, string privHex)
924:        private static byte[] GetKemContext(byte[] encappedKeyBuf, string publicKey)
931:        private static byte[] BuildLabeledIkm(byte[] label, byte[] ikm, byte[] suiteId)
937:        private static byte[] BuildLabeledInfo(byte[] label, byte[] info, byte[] suiteId, int len)
955:        private static byte[] ExtractAndExpand(byte[] sharedSecret, byte[] ikm, byte[] info, int len)
961:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
972:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
983:        private static byte[] Base64UrlDecode(string input)
997:        private static bool VerifyP256RawSignature(byte[] publicKeyBytes, byte[] signatureRaw, byte[] messageDigest)
1024:        private static void VerifyEnclaveSignature(string enclaveQuorumPublic, string signatureHex, string signedDataHex)
1052:        private static bool VerifySignature(string publicKeyHex, string signatureHex, string message)
1077:        private static byte[] DecodeKey(string privateKey, string? keyFormat)
1098:        private static string? GetStringOrNull(JsonElement element, string propertyName)

exec
/bin/zsh -lc "rg -n \"Org\\.BouncyCastle\\.Crypto\\.Generators\\.HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|HkdfBytesGenerator|Hpke|HKDF|Kdf|KDF|Generator\" src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
10://                                        HpkeEncrypt / HpkeDecrypt /
14://                                        FormatHpkeBuf, plus the internal
17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
36://     BigInteger / EC point primitives only. HPKE, HKDF, Tonelli-Shanks,
56:using Org.BouncyCastle.Crypto.Generators;
91:            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
101:            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
172:        #region HKDF (port of @noble/hashes/hkdf)
175:        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
183:            /// HKDF Extract: produces a 32-byte pseudorandom key from input
200:            /// HKDF Expand: derives <paramref name="length"/> bytes of output
250:        public class HpkeDecryptParams
258:        public class HpkeEncryptParams
299:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
304:        public class HpkeBundlePayload
355:            var keyGen = new ECKeyPairGenerator();
381:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
384:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
406:            // Step 4: HKDF derive shared secret
425:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
431:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
433:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
574:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
576:        public static string FormatHpkeBuf(byte[] encryptedBuf)
592:            var payload = new HpkeBundlePayload
598:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
637:            var decryptedData = HpkeDecrypt(new HpkeDecryptParams
710:                var encryptedBuf = HpkeEncrypt(new HpkeEncryptParams
716:                return FormatHpkeBuf(encryptedBuf);
815:                    var decryptedData = HpkeDecrypt(new HpkeDecryptParams

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '360,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
   521	      client_secret,
   522	    ),
   523	  );
   524	};
   525	
   526	/**
   527	 * Helper function used specifically to encrypt your on ramp private/secret api keys
   528	 * to the on ramp encryption public key. This is used before uploading your on ramp
   529	 * credentials to Turnkey via the CreateFiatOnRampCredential activity
   530	 *
   531	 * @param secret The private/secret api key issued by the on ramp provider
   532	 * @param dangerouslyOverrideOnRampEncryptionPublicKey *(optional)* Hex-encoded
   533	 *              uncompressed P-256 public key to encrypt to (use only in
   534	 *              tests/dev environment).  Defaults to the production on ramp encryption public key.
   535	 * @returns {Promise<string>} A base58check encoded borsh serialized envelope with the encrypted secret
   536	 *                            meant to be passed to the CreateFiatOnRampCredential activity
   537	 */
   538	export const encryptOnRampSecret = (
   539	  secret: string,
   540	  dangerouslyOverrideOnRampEncryptionPublicKey?: string,
   541	): string => {
   542	  return bs58check.encode(
   543	    hpkeEncrypt({
   544	      plainTextBuf: new TextEncoder().encode(secret),
   545	      targetKeyBuf: uncompressRawPublicKey(
   546	        uint8ArrayFromHexString(
   547	          dangerouslyOverrideOnRampEncryptionPublicKey ??
   548	            PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY,
   549	        ),
   550	      ),
   551	    }),
   552	  );
   553	};

exec
/bin/zsh -lc 'find tests/Fixtures -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/encoding/turnkey-encoding-vectors.json

exec
/bin/zsh -lc "rg -n \"Crypto\\.|Hpke|hpke|credential|exportBundle|importBundle|privateKey|fixture|Fixtures|VerifySession|Decrypt|Encrypt|GenerateP256|GetPublicKey|CompressRaw|UncompressRaw|FormatHpke\" tests src -g '*.*'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "find codex-crypto-reviews -maxdepth 3 -type f | sort | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
tests/CryptoTests.cs:1:// CryptoTests.cs — xunit tests for src/Crypto.cs.
tests/CryptoTests.cs:5://   - NIST P-256 known points (G and 2G) for GetPublicKey
tests/CryptoTests.cs:6://   - Compress/UncompressRawPublicKey roundtrip
tests/CryptoTests.cs:7://   - HpkeEncrypt/HpkeDecrypt roundtrip (deterministic round-trip)
tests/CryptoTests.cs:8://   - VerifySessionJwtSignature negative cases
tests/CryptoTests.cs:10:// Tests that require a Turnkey-signed bundle (DecryptCredentialBundle,
tests/CryptoTests.cs:11:// EncryptPrivateKeyToBundle, DecryptExportBundle against PRODUCTION_SIGNER)
tests/CryptoTests.cs:14:// identical) and by the E2E whoami flow once credentials are present.
tests/CryptoTests.cs:36:            var root = Crypto.Math.ModSqrt(x, p);
tests/CryptoTests.cs:47:            Action act = () => Crypto.Math.ModSqrt(x, p);
tests/CryptoTests.cs:57:            Action act = () => Crypto.Math.ModSqrt(x, p);
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
tests/CryptoTests.cs:181:        // CompressRawPublicKey / UncompressRawPublicKey
tests/CryptoTests.cs:189:                var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:191:                byte[] compressed = Crypto.CompressRawPublicKey(uncompressed);
tests/CryptoTests.cs:195:                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:201:        public void CompressRawPublicKey_InvalidPrefix_Throws()
tests/CryptoTests.cs:205:            Action act = () => Crypto.CompressRawPublicKey(raw);
tests/CryptoTests.cs:210:        public void CompressRawPublicKey_WrongLength_Throws()
tests/CryptoTests.cs:214:            Action act = () => Crypto.CompressRawPublicKey(raw);
tests/CryptoTests.cs:219:        public void UncompressRawPublicKey_BadPrefix_Throws()
tests/CryptoTests.cs:223:            Action act = () => Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:229:        public void UncompressRawPublicKey_WrongLength_Throws()
tests/CryptoTests.cs:233:            Action act = () => Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:239:        // GetPublicKey / GenerateP256KeyPair
tests/CryptoTests.cs:243:        public void GenerateP256KeyPair_LengthsAreCorrect()
tests/CryptoTests.cs:245:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:254:        public void GetPublicKey_FromPrivateKey_ProducesCompressedByDefault()
tests/CryptoTests.cs:256:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:258:            byte[] pub = Crypto.GetPublicKey(priv); // isCompressed default true
tests/CryptoTests.cs:264:        public void GetPublicKey_Uncompressed_MatchesGenerator()
tests/CryptoTests.cs:266:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:268:            byte[] uncompressed = Crypto.GetPublicKey(priv, isCompressed: false);
tests/CryptoTests.cs:273:        public void GetPublicKey_HexStringOverload_MatchesByteOverload()
tests/CryptoTests.cs:275:            var kp = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:276:            byte[] viaBytes = Crypto.GetPublicKey(Encoding.Uint8ArrayFromHexString(kp.PrivateKey));
tests/CryptoTests.cs:277:            byte[] viaHex = Crypto.GetPublicKey(kp.PrivateKey);
tests/CryptoTests.cs:286:        public void Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload()
tests/CryptoTests.cs:289:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:295:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:310:            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);
tests/CryptoTests.cs:312:            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
tests/CryptoTests.cs:323:        public void Hpke_EncryptThenDecrypt_EmptyPayload()
tests/CryptoTests.cs:325:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:328:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:339:            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
tests/CryptoTests.cs:342:                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
tests/CryptoTests.cs:349:        public void Hpke_DecryptWithWrongKey_Throws()
tests/CryptoTests.cs:351:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:352:            var attacker = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:355:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:366:            Action act = () => Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
tests/CryptoTests.cs:369:                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
tests/CryptoTests.cs:376:        // BuildAdditionalAssociatedData / FormatHpkeBuf
tests/CryptoTests.cs:384:            byte[] aad = Crypto.BuildAdditionalAssociatedData(a, b);
tests/CryptoTests.cs:389:        public void FormatHpkeBuf_ReturnsExpectedJson()
tests/CryptoTests.cs:391:            // Use a real HpkeEncrypt output so the bytes are valid.
tests/CryptoTests.cs:392:            var recv = Crypto.GenerateP256KeyPair();
tests/CryptoTests.cs:395:            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
tests/CryptoTests.cs:401:            string json = Crypto.FormatHpkeBuf(encrypted);
tests/CryptoTests.cs:410:        public void FormatHpkeBuf_TooSmall_Throws()
tests/CryptoTests.cs:412:            Action act = () => Crypto.FormatHpkeBuf(new byte[10]);
tests/CryptoTests.cs:414:               .WithMessage("Encrypted buffer too small");
tests/CryptoTests.cs:418:        // VerifySessionJwtSignature negative paths
tests/CryptoTests.cs:422:        public void VerifySessionJwtSignature_Empty_ReturnsFalse()
tests/CryptoTests.cs:424:            Crypto.VerifySessionJwtSignature(string.Empty).Should().BeFalse();
tests/CryptoTests.cs:428:        public void VerifySessionJwtSignature_WrongPartCount_ReturnsFalse()
tests/CryptoTests.cs:430:            Crypto.VerifySessionJwtSignature("just.two").Should().BeFalse();
tests/CryptoTests.cs:434:        public void VerifySessionJwtSignature_BadSignatureLength_ReturnsFalse()
tests/CryptoTests.cs:438:            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
tests/CryptoTests.cs:442:        public void VerifySessionJwtSignature_WellFormedButForgedSig_ReturnsFalse()
tests/CryptoTests.cs:452:            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
tests/CryptoTests.cs:461:        public void DecryptCredentialBundle_BundleTooSmall_Throws()
tests/CryptoTests.cs:465:            Action act = () => Crypto.DecryptCredentialBundle(tiny, new string('1', 64));
tests/CryptoTests.cs:470:        public void EncryptPrivateKeyToBundle_MissingArgs_Throws()
tests/CryptoTests.cs:472:            Action act = () => Crypto.EncryptPrivateKeyToBundle(new Crypto.EncryptPrivateKeyToBundleParams());
tests/CryptoTests.cs:477:        public void DecryptExportBundle_MissingArgs_Throws()
tests/CryptoTests.cs:479:            Action act = () => Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams());
src/Crypto.cs:7://   ts-source/constants.ts            -> Crypto.Constants nested class
src/Crypto.cs:8://   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
src/Crypto.cs:9://   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
src/Crypto.cs:12://                                        CompressRawPublicKey /
src/Crypto.cs:13://                                        UncompressRawPublicKey /
src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:19://   ts-source/turnkey.ts (subset)     -> DecryptCredentialBundle /
src/Crypto.cs:20://                                        EncryptPrivateKeyToBundle /
src/Crypto.cs:21://                                        DecryptExportBundle /
src/Crypto.cs:22://                                        VerifySessionJwtSignature
src/Crypto.cs:25://   hpkeAuthEncrypt, quorumKeyEncrypt, extractPrivateKeyFromPKCS8Bytes,
src/Crypto.cs:54:using Org.BouncyCastle.Crypto.Agreement;
src/Crypto.cs:55:using Org.BouncyCastle.Crypto.Engines;
src/Crypto.cs:56:using Org.BouncyCastle.Crypto.Generators;
src/Crypto.cs:57:using Org.BouncyCastle.Crypto.Modes;
src/Crypto.cs:58:using Org.BouncyCastle.Crypto.Parameters;
src/Crypto.cs:250:        public class HpkeDecryptParams
src/Crypto.cs:258:        public class HpkeEncryptParams
src/Crypto.cs:264:        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
src/Crypto.cs:273:        /// Parameters for <see cref="EncryptPrivateKeyToBundle"/>. Mirrors the
src/Crypto.cs:276:        public class EncryptPrivateKeyToBundleParams
src/Crypto.cs:286:        /// Parameters for <see cref="DecryptExportBundle"/>. Mirrors the
src/Crypto.cs:289:        public class DecryptExportBundleParams
src/Crypto.cs:299:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:304:        public class HpkeBundlePayload
src/Crypto.cs:321:        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
src/Crypto.cs:323:            if (privateKey == null)
src/Crypto.cs:325:                throw new ArgumentNullException(nameof(privateKey));
src/Crypto.cs:330:            var d = new BigInteger(1, privateKey);
src/Crypto.cs:331:            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
src/Crypto.cs:333:                privateKeyParams.Parameters.G.Multiply(d), domainParams);
src/Crypto.cs:339:        /// Hex-string overload of <see cref="GetPublicKey(byte[], bool)"/>.
src/Crypto.cs:341:        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
src/Crypto.cs:343:            return GetPublicKey(Encoding.Uint8ArrayFromHexString(privateKeyHex), isCompressed);
src/Crypto.cs:350:        public static KeyPair GenerateP256KeyPair()
src/Crypto.cs:360:            var privateKey = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
src/Crypto.cs:365:            if (privateKey.Length < 32)
src/Crypto.cs:368:                Array.Copy(privateKey, 0, padded, 32 - privateKey.Length, privateKey.Length);
src/Crypto.cs:369:                privateKey = padded;
src/Crypto.cs:374:                PrivateKey = Encoding.Uint8ArrayToHexString(privateKey),
src/Crypto.cs:382:        /// Equivalent to upstream <c>crypto.ts hpkeDecrypt</c>.
src/Crypto.cs:384:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
src/Crypto.cs:395:            var receiverPubBuf = GetPublicKey(receiverPrivBytes, false);
src/Crypto.cs:421:            return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
src/Crypto.cs:426:        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
src/Crypto.cs:431:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
src/Crypto.cs:433:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
src/Crypto.cs:441:            var ephemeralKeyPair = GenerateP256KeyPair();
src/Crypto.cs:460:            var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
src/Crypto.cs:463:            var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
src/Crypto.cs:480:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
src/Crypto.cs:506:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey)
src/Crypto.cs:574:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:576:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:581:                throw new ArgumentException("Encrypted buffer too small");
src/Crypto.cs:587:            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);
src/Crypto.cs:592:            var payload = new HpkeBundlePayload
src/Crypto.cs:598:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:606:        /// Decrypt a Turnkey credential bundle.
src/Crypto.cs:609:        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string targetPrivateKey)
src/Crypto.cs:626:                    + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
src/Crypto.cs:635:            var encappedKey = UncompressRawPublicKey(compressedKey);
src/Crypto.cs:637:            var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:648:        /// Encrypt a private key into a Turnkey import bundle.
src/Crypto.cs:651:        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
src/Crypto.cs:710:                var encryptedBuf = HpkeEncrypt(new HpkeEncryptParams
src/Crypto.cs:716:                return FormatHpkeBuf(encryptedBuf);
src/Crypto.cs:726:        /// Decrypt a Turnkey export bundle (legacy or current envelope).
src/Crypto.cs:729:        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
src/Crypto.cs:815:                    var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:851:        public static bool VerifySessionJwtSignature(string jwt)
src/Crypto.cs:905:            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
src/Crypto.cs:911:            agreement.Init(privateKeyParams);
src/Crypto.cs:961:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:972:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:1077:        private static byte[] DecodeKey(string privateKey, string? keyFormat)
src/Crypto.cs:1081:                var decoded = Encoding.Base58Decode(privateKey);
src/Crypto.cs:1087:                var privateKeyBytes = new byte[32];
src/Crypto.cs:1088:                Array.Copy(decoded, 0, privateKeyBytes, 0, 32);
src/Crypto.cs:1089:                return privateKeyBytes;
src/Crypto.cs:1092:            string normalized = privateKey.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
src/Crypto.cs:1093:                ? privateKey.Substring(2)
src/Crypto.cs:1094:                : privateKey;
src/turnkey-sdk-csharp.csproj:18:    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
tests/Fixtures/README.md:1:# Test fixtures
tests/Fixtures/README.md:3:Each subdirectory groups fixtures for one production file, plus a
tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
tests/Fixtures/README.md:21:Fixtures/
tests/Fixtures/README.md:24:├── crypto/                                  — (added with Crypto.cs port)
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
tests/Fixtures/README.md:34:into the matching fixture file and update its sha256 in
src/TurnkeyJsonContext.cs:27:    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
src/Encoding.cs:32:// Unity-specific helpers kept for Crypto.cs consumption (also present in the
src/Encoding.cs:690:        // Crypto.cs / ApiKeyStamper.cs / Http.cs use them.
tests/turnkey-sdk-csharp.Tests.csproj:24:    <None Include="Fixtures\**\*">

 succeeded in 0ms:
codex-crypto-reviews/Crypto.cs-r1-20260523.md
codex-crypto-reviews/Encoding.cs-r1-20260523.md
codex-crypto-reviews/Encoding.cs-r2-20260523.md
codex-crypto-reviews/Encoding.cs-r3-20260523.md
codex-crypto-reviews/README.md
codex-crypto-reviews/codex-crypto-review.sh
codex-crypto-reviews/peak-lockfile-evidence.md
codex-crypto-reviews/turnkey-source-pins.md
codex-crypto-reviews/unity-source-pins.md
codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0.tgz
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8.tgz
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0.tgz
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0.tgz

codex
I’ve found that `tests/Fixtures/` currently only contains encoding fixtures, not a crypto fixture directory. I’m still checking the C# tests and review notes so the fixture gate is explicit rather than assumed.
exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '1,560p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
   201	        public void CompressRawPublicKey_InvalidPrefix_Throws()
   202	        {
   203	            var raw = new byte[65];
   204	            raw[0] = 0x05;
   205	            Action act = () => Crypto.CompressRawPublicKey(raw);
   206	            act.Should().Throw<ArgumentException>();
   207	        }
   208	
   209	        [Fact]
   210	        public void CompressRawPublicKey_WrongLength_Throws()
   211	        {
   212	            var raw = new byte[64];
   213	            raw[0] = 0x04;
   214	            Action act = () => Crypto.CompressRawPublicKey(raw);
   215	            act.Should().Throw<ArgumentException>();
   216	        }
   217	
   218	        [Fact]
   219	        public void UncompressRawPublicKey_BadPrefix_Throws()
   220	        {
   221	            var compressed = new byte[33];
   222	            compressed[0] = 0x05;
   223	            Action act = () => Crypto.UncompressRawPublicKey(compressed);
   224	            act.Should().Throw<ArgumentException>()
   225	               .WithMessage("failed to uncompress raw public key: invalid prefix");
   226	        }
   227	
   228	        [Fact]
   229	        public void UncompressRawPublicKey_WrongLength_Throws()
   230	        {
   231	            var compressed = new byte[32];
   232	            compressed[0] = 0x02;
   233	            Action act = () => Crypto.UncompressRawPublicKey(compressed);
   234	            act.Should().Throw<ArgumentException>()
   235	               .WithMessage("Invalid compressed public key size: 32");
   236	        }
   237	
   238	        // ============================================================
   239	        // GetPublicKey / GenerateP256KeyPair
   240	        // ============================================================
   241	
   242	        [Fact]
   243	        public void GenerateP256KeyPair_LengthsAreCorrect()
   244	        {
   245	            var kp = Crypto.GenerateP256KeyPair();
   246	            kp.PrivateKey.Should().HaveLength(64);          // 32 bytes hex
   247	            kp.PublicKey.Should().HaveLength(66);            // 33 bytes hex
   248	            kp.PublicKeyUncompressed.Should().HaveLength(130); // 65 bytes hex
   249	            kp.PublicKey.Should().MatchRegex("^0[23][0-9a-f]{64}$");
   250	            kp.PublicKeyUncompressed.Should().StartWith("04");
   251	        }
   252	
   253	        [Fact]
   254	        public void GetPublicKey_FromPrivateKey_ProducesCompressedByDefault()
   255	        {
   256	            var kp = Crypto.GenerateP256KeyPair();
   257	            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
   258	            byte[] pub = Crypto.GetPublicKey(priv); // isCompressed default true
   259	            pub.Should().HaveCount(33);
   260	            Encoding.Uint8ArrayToHexString(pub).Should().Be(kp.PublicKey);
   261	        }
   262	
   263	        [Fact]
   264	        public void GetPublicKey_Uncompressed_MatchesGenerator()
   265	        {
   266	            var kp = Crypto.GenerateP256KeyPair();
   267	            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
   268	            byte[] uncompressed = Crypto.GetPublicKey(priv, isCompressed: false);
   269	            Encoding.Uint8ArrayToHexString(uncompressed).Should().Be(kp.PublicKeyUncompressed);
   270	        }
   271	
   272	        [Fact]
   273	        public void GetPublicKey_HexStringOverload_MatchesByteOverload()
   274	        {
   275	            var kp = Crypto.GenerateP256KeyPair();
   276	            byte[] viaBytes = Crypto.GetPublicKey(Encoding.Uint8ArrayFromHexString(kp.PrivateKey));
   277	            byte[] viaHex = Crypto.GetPublicKey(kp.PrivateKey);
   278	            viaBytes.Should().Equal(viaHex);
   279	        }
   280	
   281	        // ============================================================
   282	        // HPKE roundtrip
   283	        // ============================================================
   284	
   285	        [Fact]
   286	        public void Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload()
   287	        {
   288	            // Receiver key pair.
   289	            var recv = Crypto.GenerateP256KeyPair();
   290	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   291	
   292	            byte[] payload = Encoding.Uint8ArrayFromHexString(
   293	                "deadbeefcafebabe1234567890abcdef00112233445566778899aabbccddeeff");
   294	
   295	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   296	            {
   297	                PlainTextBuf = payload,
   298	                TargetKeyBuf = recvPubUncompressed,
   299	            });
   300	
   301	            // encrypted = compressed(senderPub) || ciphertext (33 + N bytes)
   302	            encrypted.Length.Should().BeGreaterThan(33);
   303	
   304	            // Split, uncompress the sender's compressed point, then decrypt.
   305	            var compressedSender = new byte[33];
   306	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   307	            var ciphertext = new byte[encrypted.Length - 33];
   308	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   309	
   310	            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);
   311	
   312	            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   313	            {
   314	                CiphertextBuf = ciphertext,
   315	                EncappedKeyBuf = encapped,
   316	                ReceiverPriv = recv.PrivateKey,
   317	            });
   318	
   319	            decrypted.Should().Equal(payload);
   320	        }
   321	
   322	        [Fact]
   323	        public void Hpke_EncryptThenDecrypt_EmptyPayload()
   324	        {
   325	            var recv = Crypto.GenerateP256KeyPair();
   326	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   327	
   328	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   329	            {
   330	                PlainTextBuf = Array.Empty<byte>(),
   331	                TargetKeyBuf = recvPubUncompressed,
   332	            });
   333	
   334	            var compressedSender = new byte[33];
   335	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   336	            var ciphertext = new byte[encrypted.Length - 33];
   337	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   338	
   339	            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   340	            {
   341	                CiphertextBuf = ciphertext,
   342	                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
   343	                ReceiverPriv = recv.PrivateKey,
   344	            });
   345	            decrypted.Should().BeEmpty();
   346	        }
   347	
   348	        [Fact]
   349	        public void Hpke_DecryptWithWrongKey_Throws()
   350	        {
   351	            var recv = Crypto.GenerateP256KeyPair();
   352	            var attacker = Crypto.GenerateP256KeyPair();
   353	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   354	
   355	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   356	            {
   357	                PlainTextBuf = Encoding.Uint8ArrayFromHexString("aabbcc"),
   358	                TargetKeyBuf = recvPubUncompressed,
   359	            });
   360	
   361	            var compressedSender = new byte[33];
   362	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   363	            var ciphertext = new byte[encrypted.Length - 33];
   364	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   365	
   366	            Action act = () => Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   367	            {
   368	                CiphertextBuf = ciphertext,
   369	                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
   370	                ReceiverPriv = attacker.PrivateKey,
   371	            });
   372	            act.Should().Throw<Exception>();
   373	        }
   374	
   375	        // ============================================================
   376	        // BuildAdditionalAssociatedData / FormatHpkeBuf
   377	        // ============================================================
   378	
   379	        [Fact]
   380	        public void BuildAdditionalAssociatedData_ConcatsArgs()
   381	        {
   382	            byte[] a = { 0x01, 0x02 };
   383	            byte[] b = { 0x03, 0x04, 0x05 };
   384	            byte[] aad = Crypto.BuildAdditionalAssociatedData(a, b);
   385	            aad.Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
   386	        }
   387	
   388	        [Fact]
   389	        public void FormatHpkeBuf_ReturnsExpectedJson()
   390	        {
   391	            // Use a real HpkeEncrypt output so the bytes are valid.
   392	            var recv = Crypto.GenerateP256KeyPair();
   393	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   394	
   395	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   396	            {
   397	                PlainTextBuf = Encoding.Uint8ArrayFromHexString("1234"),
   398	                TargetKeyBuf = recvPubUncompressed,
   399	            });
   400	
   401	            string json = Crypto.FormatHpkeBuf(encrypted);
   402	
   403	            // Shape contains exactly two keys in upstream order.
   404	            json.Should().StartWith("{\"encappedPublic\":\"04");
   405	            json.Should().Contain("\"ciphertext\":\"");
   406	            json.Should().EndWith("\"}");
   407	        }
   408	
   409	        [Fact]
   410	        public void FormatHpkeBuf_TooSmall_Throws()
   411	        {
   412	            Action act = () => Crypto.FormatHpkeBuf(new byte[10]);
   413	            act.Should().Throw<ArgumentException>()
   414	               .WithMessage("Encrypted buffer too small");
   415	        }
   416	
   417	        // ============================================================
   418	        // VerifySessionJwtSignature negative paths
   419	        // ============================================================
   420	
   421	        [Fact]
   422	        public void VerifySessionJwtSignature_Empty_ReturnsFalse()
   423	        {
   424	            Crypto.VerifySessionJwtSignature(string.Empty).Should().BeFalse();
   425	        }
   426	
   427	        [Fact]
   428	        public void VerifySessionJwtSignature_WrongPartCount_ReturnsFalse()
   429	        {
   430	            Crypto.VerifySessionJwtSignature("just.two").Should().BeFalse();
   431	        }
   432	
   433	        [Fact]
   434	        public void VerifySessionJwtSignature_BadSignatureLength_ReturnsFalse()
   435	        {
   436	            // 3 parts but the signature decodes to a non-64-byte buffer.
   437	            string jwt = "aGVhZGVy.cGF5bG9hZA.YWJj"; // last part decodes to "abc" (3 bytes)
   438	            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
   439	        }
   440	
   441	        [Fact]
   442	        public void VerifySessionJwtSignature_WellFormedButForgedSig_ReturnsFalse()
   443	        {
   444	            // 64-byte signature of all zeros — should fail verification.
   445	            string header = Encoding.Base64StringToBase64UrlEncodedString(
   446	                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"ES256\"}")));
   447	            string payload = Encoding.Base64StringToBase64UrlEncodedString(
   448	                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"sub\":\"x\"}")));
   449	            string sig = Encoding.Base64StringToBase64UrlEncodedString(
   450	                Convert.ToBase64String(new byte[64]));
   451	            string jwt = header + "." + payload + "." + sig;
   452	            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
   453	        }
   454	
   455	        // ============================================================
   456	        // Bundle helpers — negative paths only (positive paths require
   457	        // Turnkey-signed sample bundles which we do not have here)
   458	        // ============================================================
   459	
   460	        [Fact]
   461	        public void DecryptCredentialBundle_BundleTooSmall_Throws()
   462	        {
   463	            // Base58 of "0" gives a tiny payload.
   464	            string tiny = Encoding.Base58Encode(new byte[] { 0x01, 0x02, 0x03 });
   465	            Action act = () => Crypto.DecryptCredentialBundle(tiny, new string('1', 64));
   466	            act.Should().Throw<Exception>();
   467	        }
   468	
   469	        [Fact]
   470	        public void EncryptPrivateKeyToBundle_MissingArgs_Throws()
   471	        {
   472	            Action act = () => Crypto.EncryptPrivateKeyToBundle(new Crypto.EncryptPrivateKeyToBundleParams());
   473	            act.Should().Throw<ArgumentException>();
   474	        }
   475	
   476	        [Fact]
   477	        public void DecryptExportBundle_MissingArgs_Throws()
   478	        {
   479	            Action act = () => Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams());
   480	            act.Should().Throw<ArgumentException>();
   481	        }
   482	
   483	        // ============================================================
   484	        // Helpers
   485	        // ============================================================
   486	
   487	        private static byte[] HexToBytes(string hex)
   488	        {
   489	            return Encoding.Uint8ArrayFromHexString(hex);
   490	        }
   491	
   492	        private static string BytesToHex(byte[] bytes)
   493	        {
   494	            return Encoding.Uint8ArrayToHexString(bytes);
   495	        }
   496	    }
   497	}

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
/bin/zsh -lc "nl -ba codex-crypto-reviews/Crypto.cs-r1-20260523.md | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	# Codex review — src/Crypto.cs — round 1 — 20260523
     2	
     3	C# SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
     4	Upstream snapshot: `turnkey-crypto-2.8.8`
     5	
     6	---
     7	
     8	2026-05-22T23:55:34.565399Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
     9	OpenAI Codex v0.129.0 (research preview)
    10	--------
    11	workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
    12	model: gpt-5.5
    13	provider: openai
    14	approval: never
    15	sandbox: read-only
    16	reasoning effort: high
    17	reasoning summaries: none
    18	session id: 019e521d-a470-7e02-9710-99b8e5bdf20d
    19	--------
    20	user
    21	You are reviewing a C# port of the Turnkey TypeScript SDK.
    22	File under review (C#): src/Crypto.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
    23	Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
    24	Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
    25	
    26	This is REVIEW ROUND 1 of 3 for this file.
    27	
    28	REQUIRED OUTPUTS (sections A through G must all appear):
    29	
    30	A. Source pin acknowledgement:
    31	   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
    32	   C# file git SHA (388a5fc768fd1215928d29f338ef069acd2eb60c).
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
    70	2026-05-22T23:55:35.245107Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
    71	codex
    72	I’ll use the code-review workflow here and do a source-pinned comparison against the local upstream snapshot, then report the required A-G sections with file/line evidence.
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/turnkey-source-pins.md | sed -n '1,180p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	# Turnkey upstream source pins
     2	
     3	The C# port targets the **peak-pinned** Turnkey npm versions, NOT the Unity
     4	port's newer versions. The npm tarball contents (extracted under
     5	[`upstream-snapshots/`](./upstream-snapshots/)) are the authoritative source.
     6	GitHub SHAs are secondary metadata.
     7	
     8	## Pin table
     9	
    10	| src C# file                | Upstream snapshot path                  | npm package                | Version | Tarball sha256 |
    11	|---|---|---|---|---|
    12	| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
    14	| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
    15	| src/ApiKeyStamper.cs       | turnkey-api-key-stamper-0.5.0           | @turnkey/api-key-stamper   | 0.5.0   | `962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd` |
    16	| src/Http.cs                | turnkey-http-3.16.0                     | @turnkey/http              | 3.16.0  | `d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf` |
    17	| src/TurnkeyJsonContext.cs  | turnkey-crypto-2.8.8                    | (no single upstream; covers DTOs from crypto + api-key-stamper + http) | n/a | n/a |
    18	
    19	Each upstream snapshot directory contains:
    20	
    21	- `package/` — verbatim npm tarball extract (the authoritative wire-format
    22	  source: `dist/*.js` + `dist/*.d.ts` + `package.json` + LICENSE / README).
    23	- `ts-source/` — the original TypeScript source from `github.com/tkhq/sdk` at
    24	  the matching git tag SHA (committed for human readability; the npm tarball
    25	  remains the wire-format source of truth if the two ever disagree).
    26	
    27	## Why these versions
    28	
    29	These are exactly what the peak monorepo pulls. See
    30	[`peak-lockfile-evidence.md`](./peak-lockfile-evidence.md) for the
    31	`pnpm-lock.yaml` lines.
    32	
    33	## GitHub commit SHAs (secondary)
    34	
    35	`gitHead` is **NOT present** in the published tarballs for these versions
    36	(`grep gitHead package/package.json` returns nothing for any of the four).
    37	The GitHub commit SHAs below are resolved from the git tag of the form
    38	`@turnkey/<pkg>@<version>` via `gh api /repos/tkhq/sdk/git/ref/tags/...`.
    39	These are recorded for cross-reference only. The npm tarball sha256 above
    40	is the controlling wire-format source.
    41	
    42	| npm package                | Version | Git tag SHA                                  | Reference URL |
    43	|---|---|---|---|
    44	| @turnkey/encoding          | 0.6.0   | `60a997f4c52ac5f98bdd285af934f02699b88bff`   | https://github.com/tkhq/sdk/tree/60a997f4c52ac5f98bdd285af934f02699b88bff/packages/encoding |
    45	| @turnkey/crypto            | 2.8.8   | `b35dc642bd7c1728f97acd43d4cba66976b65084`   | https://github.com/tkhq/sdk/tree/b35dc642bd7c1728f97acd43d4cba66976b65084/packages/crypto |
    46	| @turnkey/api-key-stamper   | 0.5.0   | `b711befbb88ec522452dbdac68f0e98762be10dd`   | https://github.com/tkhq/sdk/tree/b711befbb88ec522452dbdac68f0e98762be10dd/packages/api-key-stamper |
    47	| @turnkey/http              | 3.16.0  | `8def9ba521233137437ac7294693a4ae0a0d14da`   | https://github.com/tkhq/sdk/tree/8def9ba521233137437ac7294693a4ae0a0d14da/packages/http |
    48	
    49	## Re-pinning procedure
    50	
    51	When bumping any pin:
    52	
    53	1. Update the table above (npm version, refresh tarball sha256, gitHead).
    54	2. Update `Directory.Packages.props` or any version comment.
    55	3. Re-run `npm pack` and refresh the extracted directories under
    56	   `upstream-snapshots/`.
    57	4. Re-run **3 Codex review rounds per affected file** via
    58	   [`codex-crypto-review.sh`](./codex-crypto-review.sh).
    59	5. Update `CHANGELOG.md`.

exec
/bin/zsh -lc 'rg -n "''^A'"\\.|"'^B'"\\.|"'^C'"\\.|"'^D'"\\.|"'^E'"\\.|"'^F'"\\.|"'^G'"\\.|Logic divergence|NOT-REVIEWED|PASS|FAIL|src/Crypto.cs\" codex-crypto-reviews/Crypto.cs-r1-20260523.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc 'tail -n 220 codex-crypto-reviews/Crypto.cs-r1-20260523.md | nl -ba' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
1:# Codex review — src/Crypto.cs — round 1 — 20260523
22:File under review (C#): src/Crypto.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
30:A. Source pin acknowledgement:
34:B. Method coverage table: every public + internal helper method in src/Crypto.cs
37:   Status: REVIEWED or NOT-REVIEWED. If NOT-REVIEWED, give a reason. Do not skip rows.
39:C. Intentional adaptations: list every C#/TS adaptation pattern with a one-line
42:D. (Crypto.cs only) D17 enforcement check. Confirm BouncyCastle is used only for
50:E. Logic divergence findings: every place C# behavior differs from upstream TS:
58:F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
62:G. Unresolved assumptions you could not verify in this round.
64:PASS criterion for this round: B has zero NOT-REVIEWED rows (or each is
318:/bin/zsh -lc "git status --short && git rev-parse --show-toplevel && git log -1 --format='%H %ct %s' -- src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
322:/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
671:/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '261,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
1446:/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '621,1040p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
2410:/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '1041,1300p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
2859:/bin/zsh -lc 'rg -n "public static|private static|internal|public class|private const|public const|static readonly" src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
2915:/bin/zsh -lc "rg -n \"Org\\.BouncyCastle\\.Crypto\\.Generators\\.HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|HkdfBytesGenerator|Hpke|HKDF|Kdf|KDF|Generator\" src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
3158:tests/CryptoTests.cs:1:// CryptoTests.cs — xunit tests for src/Crypto.cs.
3246:src/Crypto.cs:7://   ts-source/constants.ts            -> Crypto.Constants nested class
3247:src/Crypto.cs:8://   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
3248:src/Crypto.cs:9://   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
3249:src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
3250:src/Crypto.cs:12://                                        CompressRawPublicKey /
3251:src/Crypto.cs:13://                                        UncompressRawPublicKey /
3252:src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
3253:src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
3254:src/Crypto.cs:19://   ts-source/turnkey.ts (subset)     -> DecryptCredentialBundle /
3255:src/Crypto.cs:20://                                        EncryptPrivateKeyToBundle /
3256:src/Crypto.cs:21://                                        DecryptExportBundle /
3257:src/Crypto.cs:22://                                        VerifySessionJwtSignature
3258:src/Crypto.cs:25://   hpkeAuthEncrypt, quorumKeyEncrypt, extractPrivateKeyFromPKCS8Bytes,
3259:src/Crypto.cs:54:using Org.BouncyCastle.Crypto.Agreement;
3260:src/Crypto.cs:55:using Org.BouncyCastle.Crypto.Engines;
3261:src/Crypto.cs:56:using Org.BouncyCastle.Crypto.Generators;
3262:src/Crypto.cs:57:using Org.BouncyCastle.Crypto.Modes;
3263:src/Crypto.cs:58:using Org.BouncyCastle.Crypto.Parameters;
3264:src/Crypto.cs:250:        public class HpkeDecryptParams
3265:src/Crypto.cs:258:        public class HpkeEncryptParams
3266:src/Crypto.cs:264:        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
3267:src/Crypto.cs:273:        /// Parameters for <see cref="EncryptPrivateKeyToBundle"/>. Mirrors the
3268:src/Crypto.cs:276:        public class EncryptPrivateKeyToBundleParams
3269:src/Crypto.cs:286:        /// Parameters for <see cref="DecryptExportBundle"/>. Mirrors the
3270:src/Crypto.cs:289:        public class DecryptExportBundleParams
3271:src/Crypto.cs:299:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
3272:src/Crypto.cs:304:        public class HpkeBundlePayload
3273:src/Crypto.cs:321:        public static byte[] GetPublicKey(byte[] privateKey, bool isCompressed = true)
3274:src/Crypto.cs:323:            if (privateKey == null)
3275:src/Crypto.cs:325:                throw new ArgumentNullException(nameof(privateKey));
3276:src/Crypto.cs:330:            var d = new BigInteger(1, privateKey);
3277:src/Crypto.cs:331:            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
3278:src/Crypto.cs:333:                privateKeyParams.Parameters.G.Multiply(d), domainParams);
3279:src/Crypto.cs:339:        /// Hex-string overload of <see cref="GetPublicKey(byte[], bool)"/>.
3280:src/Crypto.cs:341:        public static byte[] GetPublicKey(string privateKeyHex, bool isCompressed = true)
3281:src/Crypto.cs:343:            return GetPublicKey(Encoding.Uint8ArrayFromHexString(privateKeyHex), isCompressed);
3282:src/Crypto.cs:350:        public static KeyPair GenerateP256KeyPair()
3283:src/Crypto.cs:360:            var privateKey = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArrayUnsigned();
3284:src/Crypto.cs:365:            if (privateKey.Length < 32)
3285:src/Crypto.cs:368:                Array.Copy(privateKey, 0, padded, 32 - privateKey.Length, privateKey.Length);
3286:src/Crypto.cs:369:                privateKey = padded;
3287:src/Crypto.cs:374:                PrivateKey = Encoding.Uint8ArrayToHexString(privateKey),
3288:src/Crypto.cs:382:        /// Equivalent to upstream <c>crypto.ts hpkeDecrypt</c>.
3289:src/Crypto.cs:384:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
3290:src/Crypto.cs:395:            var receiverPubBuf = GetPublicKey(receiverPrivBytes, false);
3291:src/Crypto.cs:421:            return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
3292:src/Crypto.cs:426:        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
3293:src/Crypto.cs:431:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
3294:src/Crypto.cs:433:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
3295:src/Crypto.cs:441:            var ephemeralKeyPair = GenerateP256KeyPair();
3296:src/Crypto.cs:460:            var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
3297:src/Crypto.cs:463:            var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
3298:src/Crypto.cs:480:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
3299:src/Crypto.cs:506:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey)
3300:src/Crypto.cs:574:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
3301:src/Crypto.cs:576:        public static string FormatHpkeBuf(byte[] encryptedBuf)
3302:src/Crypto.cs:581:                throw new ArgumentException("Encrypted buffer too small");
3303:src/Crypto.cs:587:            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);
3304:src/Crypto.cs:592:            var payload = new HpkeBundlePayload
3305:src/Crypto.cs:598:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
3306:src/Crypto.cs:606:        /// Decrypt a Turnkey credential bundle.
3307:src/Crypto.cs:609:        public static string DecryptCredentialBundle(string encryptedCredentialBundle, string targetPrivateKey)
3308:src/Crypto.cs:626:                    + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
3309:src/Crypto.cs:635:            var encappedKey = UncompressRawPublicKey(compressedKey);
3310:src/Crypto.cs:637:            var decryptedData = HpkeDecrypt(new HpkeDecryptParams
3311:src/Crypto.cs:648:        /// Encrypt a private key into a Turnkey import bundle.
3312:src/Crypto.cs:651:        public static string EncryptPrivateKeyToBundle(EncryptPrivateKeyToBundleParams parameters)
3313:src/Crypto.cs:710:                var encryptedBuf = HpkeEncrypt(new HpkeEncryptParams
3314:src/Crypto.cs:716:                return FormatHpkeBuf(encryptedBuf);
3315:src/Crypto.cs:726:        /// Decrypt a Turnkey export bundle (legacy or current envelope).
3316:src/Crypto.cs:729:        public static string DecryptExportBundle(DecryptExportBundleParams parameters)
3317:src/Crypto.cs:815:                    var decryptedData = HpkeDecrypt(new HpkeDecryptParams
3318:src/Crypto.cs:851:        public static bool VerifySessionJwtSignature(string jwt)
3319:src/Crypto.cs:905:            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);
3320:src/Crypto.cs:911:            agreement.Init(privateKeyParams);
3321:src/Crypto.cs:961:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
3322:src/Crypto.cs:972:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
3323:src/Crypto.cs:1077:        private static byte[] DecodeKey(string privateKey, string? keyFormat)
3324:src/Crypto.cs:1081:                var decoded = Encoding.Base58Decode(privateKey);
3325:src/Crypto.cs:1087:                var privateKeyBytes = new byte[32];
3326:src/Crypto.cs:1088:                Array.Copy(decoded, 0, privateKeyBytes, 0, 32);
3327:src/Crypto.cs:1089:                return privateKeyBytes;
3328:src/Crypto.cs:1092:            string normalized = privateKey.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
3329:src/Crypto.cs:1093:                ? privateKey.Substring(2)
3330:src/Crypto.cs:1094:                : privateKey;
3365:     1	// CryptoTests.cs — xunit tests for src/Crypto.cs.
3905:     1	# Codex review — src/Crypto.cs — round 1 — 20260523
3926:    22	File under review (C#): src/Crypto.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
3938:    34	B. Method coverage table: every public + internal helper method in src/Crypto.cs
3941:    37	   Status: REVIEWED or NOT-REVIEWED. If NOT-REVIEWED, give a reason. Do not skip rows.
3954:    50	E. Logic divergence findings: every place C# behavior differs from upstream TS:
3968:    64	PASS criterion for this round: B has zero NOT-REVIEWED rows (or each is
4182:    14	| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |

 succeeded in 0ms:
     1	   105	
     2	   106	**"Code changes + intention description" as combined input is the most effective way to improve AI code review quality.**
     3	   107	
     4	   108	## Skill Architecture
     5	   109	
     6	   110	This skill operates in two phases:
     7	   111	
     8	   112	1. **Preparation Phase** (current context): Check working directory, update CHANGELOG
     9	   113	2. **Review Phase** (isolated context): Invoke Task tool to execute Lint + codex review (using context: fork to reduce context waste)
    10	   114	
    11	   115	## Execution Steps
    12	   116	
    13	   117	### 0. [First] Check Working Directory Status
    14	   118	
    15	   119	```bash
    16	   120	git diff --name-only && git status --short
    17	   121	```
    18	   122	
    19	   123	**Decide review mode based on output:**
    20	   124	
    21	   125	- **Has uncommitted changes** → Continue with steps 1-4 (normal flow)
    22	   126	- **Clean working directory** → Directly invoke codex-runner: `codex review --commit HEAD`
    23	   127	
    24	   128	### 1. [Mandatory] Check if CHANGELOG is Updated
    25	   129	
    26	   130	**Before any review, must check if CHANGELOG.md contains description of current changes.**
    27	   131	
    28	   132	```bash
    29	   133	# Check if CHANGELOG.md is in uncommitted changes
    30	   134	git diff --name-only | grep -E "(CHANGELOG|changelog)"
    31	   135	```
    32	   136	
    33	   137	**If CHANGELOG is not updated, you must automatically perform the following (don't ask user to do it manually):**
    34	   138	
    35	   139	1. **Analyze changes**: Run `git diff --stat` and `git diff` to get complete changes
    36	   140	2. **Auto-generate CHANGELOG entry**: Generate compliant entry based on code changes
    37	   141	3. **Write to CHANGELOG.md**: Use Edit tool to insert entry at top of `[Unreleased]` section
    38	   142	4. **Continue review flow**: Immediately proceed to next steps after CHANGELOG update
    39	   143	
    40	   144	**Auto-generated CHANGELOG entry format:**
    41	   145	
    42	   146	```markdown
    43	   147	## [Unreleased]
    44	   148	
    45	   149	### Added / Changed / Fixed
    46	   150	
    47	   151	- Feature description: what problem was solved or what functionality was implemented
    48	   152	- Affected files: main modified files/modules
    49	   153	```
    50	   154	
    51	   155	**Example - Auto-generation Flow:**
    52	   156	
    53	   157	```
    54	   158	1. Detected CHANGELOG not updated
    55	   159	2. Run git diff --stat, found handlers/responses.go modified (+88 lines)
    56	   160	3. Run git diff to analyze details: added CompactHandler function
    57	   161	4. Auto-generate entry:
    58	   162	   ### Added
    59	   163	   - Added `/v1/responses/compact` endpoint for conversation context compression
    60	   164	   - Supports multi-channel failover and request body size limits
    61	   165	5. Use Edit tool to write to CHANGELOG.md
    62	   166	6. Continue with lint and codex review
    63	   167	```
    64	   168	
    65	   169	### 2. [Critical] Stage All New Files
    66	   170	
    67	   171	**Before invoking codex review, must add all new files (untracked files) to git staging area, otherwise codex will report P1 error.**
    68	   172	
    69	   173	```bash
    70	   174	# Check for new files
    71	   175	git status --short | grep "^??"
    72	   176	```
    73	   177	
    74	   178	**If there are new files, automatically execute:**
    75	   179	
    76	   180	```bash
    77	   181	# Safely stage all new files (handles empty list and special filenames)
    78	   182	git ls-files --others --exclude-standard -z | while IFS= read -r -d '' f; do git add -- "$f"; done
    79	   183	```
    80	   184	
    81	   185	**Explanation:**
    82	   186	
    83	   187	- `-z` uses null character to separate filenames, correctly handles filenames with spaces/newlines
    84	   188	- `while IFS= read -r -d ''` reads filenames one by one
    85	   189	- `git add -- "$f"` uses `--` separator, correctly handles filenames starting with `-`
    86	   190	- When no new files exist, loop body doesn't execute, safely skipped
    87	   191	- This won't stage modified files, only handles new files
    88	   192	- codex needs files to be tracked by git for proper review
    89	   193	
    90	   194	### 3. Evaluate Task Difficulty and Invoke codex-runner
    91	   195	
    92	   196	**Count change scale:**
    93	   197	
    94	   198	```bash
    95	   199	# Count number of changed files and lines of code
    96	   200	git diff --stat | tail -1
    97	   201	```
    98	   202	
    99	   203	**Difficulty Assessment Criteria:**
   100	   204	
   101	   205	**Difficult Tasks** (meets any condition):
   102	   206	
   103	   207	- Modified files ≥ 10
   104	   208	- Total code changes (insertions + deletions) ≥ 500 lines
   105	   209	- Single metric: insertions ≥ 300 lines OR deletions ≥ 300 lines
   106	   210	- Involves core architecture/algorithm changes
   107	   211	- Cross-module refactoring
   108	   212	- Config: `model_reasoning_effort=xhigh`, timeout 30 minutes
   109	   213	
   110	   214	**Normal Tasks** (other cases):
   111	   215	
   112	   216	- Config: `model_reasoning_effort=high`, timeout 10 minutes
   113	   217	
   114	   218	**Evaluation Method:**
   115	   219	
   116	   220	You MUST parse the `git diff --stat` output correctly to determine difficulty:
   117	   221	
   118	   222	```bash
   119	   223	# Get the summary line (last line of git diff --stat)
   120	   224	git diff --stat | tail -1
   121	   225	# Example outputs:
   122	   226	# "20 files changed, 342 insertions(+), 985 deletions(-)"
   123	   227	# "1 file changed, 50 insertions(+)"  # No deletions
   124	   228	# "3 files changed, 120 deletions(-)"  # No insertions
   125	   229	```
   126	   230	
   127	   231	**Parsing Rules:**
   128	   232	1. Extract file count from "X file(s) changed" (handle both "1 file" and "N files")
   129	   233	2. Extract insertions from "Y insertion(s)(+)" if present (handle both "1 insertion" and "N insertions"), otherwise 0
   130	   234	3. Extract deletions from "Z deletion(s)(-)" if present (handle both "1 deletion" and "N deletions"), otherwise 0
   131	   235	4. Calculate total changes = insertions + deletions
   132	   236	
   133	   237	**Important Edge Cases:**
   134	   238	- Single file: `"1 file changed"` (singular form)
   135	   239	- No insertions: Git omits `"insertions(+)"` entirely → treat as 0
   136	   240	- No deletions: Git omits `"deletions(-)"` entirely → treat as 0
   137	   241	- Pure rename: May show `"0 insertions(+), 0 deletions(-)"` or omit both
   138	   242	
   139	   243	**Decision Logic (ANY condition triggers xhigh):**
   140	   244	- IF file_count >= 10 → xhigh
   141	   245	- IF total_changes >= 500 → xhigh
   142	   246	- IF insertions >= 300 → xhigh
   143	   247	- IF deletions >= 300 → xhigh
   144	   248	- ELSE → high
   145	   249	
   146	   250	**Example Cases:**
   147	   251	- ✅ "20 files changed, 342 insertions(+), 985 deletions(-)" → xhigh (files=20≥10, total=1327≥500, deletions=985≥300)
   148	   252	- ✅ "5 files changed, 600 insertions(+), 50 deletions(-)" → xhigh (total=650≥500, insertions=600≥300)
   149	   253	- ✅ "12 files changed, 100 insertions(+), 50 deletions(-)" → xhigh (files=12≥10)
   150	   254	- ✅ "1 file changed, 400 deletions(-)" → xhigh (deletions=400≥300)
   151	   255	- ❌ "3 files changed, 150 insertions(+), 80 deletions(-)" → high (all conditions fail)
   152	   256	- ❌ "1 file changed, 50 insertions(+)" → high (no deletions, total=50<500)
   153	   257	
   154	   258	**Invoke codex-runner Subtask:**
   155	   259	
   156	   260	Use Task tool to invoke codex-runner, passing complete command (including Lint + codex review):
   157	
   158	exec
   159	/bin/zsh -lc "nl -ba codex-crypto-reviews/turnkey-source-pins.md | sed -n '1,180p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
   160	 succeeded in 0ms:
   161	     1	# Turnkey upstream source pins
   162	     2	
   163	     3	The C# port targets the **peak-pinned** Turnkey npm versions, NOT the Unity
   164	     4	port's newer versions. The npm tarball contents (extracted under
   165	     5	[`upstream-snapshots/`](./upstream-snapshots/)) are the authoritative source.
   166	     6	GitHub SHAs are secondary metadata.
   167	     7	
   168	     8	## Pin table
   169	     9	
   170	    10	| src C# file                | Upstream snapshot path                  | npm package                | Version | Tarball sha256 |
   171	    11	|---|---|---|---|---|
   172	    12	| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
   174	    14	| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
   175	    15	| src/ApiKeyStamper.cs       | turnkey-api-key-stamper-0.5.0           | @turnkey/api-key-stamper   | 0.5.0   | `962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd` |
   176	    16	| src/Http.cs                | turnkey-http-3.16.0                     | @turnkey/http              | 3.16.0  | `d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf` |
   177	    17	| src/TurnkeyJsonContext.cs  | turnkey-crypto-2.8.8                    | (no single upstream; covers DTOs from crypto + api-key-stamper + http) | n/a | n/a |
   178	    18	
   179	    19	Each upstream snapshot directory contains:
   180	    20	
   181	    21	- `package/` — verbatim npm tarball extract (the authoritative wire-format
   182	    22	  source: `dist/*.js` + `dist/*.d.ts` + `package.json` + LICENSE / README).
   183	    23	- `ts-source/` — the original TypeScript source from `github.com/tkhq/sdk` at
   184	    24	  the matching git tag SHA (committed for human readability; the npm tarball
   185	    25	  remains the wire-format source of truth if the two ever disagree).
   186	    26	
   187	    27	## Why these versions
   188	    28	
   189	    29	These are exactly what the peak monorepo pulls. See
   190	    30	[`peak-lockfile-evidence.md`](./peak-lockfile-evidence.md) for the
   191	    31	`pnpm-lock.yaml` lines.
   192	    32	
   193	    33	## GitHub commit SHAs (secondary)
   194	    34	
   195	    35	`gitHead` is **NOT present** in the published tarballs for these versions
   196	    36	(`grep gitHead package/package.json` returns nothing for any of the four).
   197	    37	The GitHub commit SHAs below are resolved from the git tag of the form
   198	    38	`@turnkey/<pkg>@<version>` via `gh api /repos/tkhq/sdk/git/ref/tags/...`.
   199	    39	These are recorded for cross-reference only. The npm tarball sha256 above
   200	    40	is the controlling wire-format source.
   201	    41	
   202	    42	| npm package                | Version | Git tag SHA                                  | Reference URL |
   203	    43	|---|---|---|---|
   204	    44	| @turnkey/encoding          | 0.6.0   | `60a997f4c52ac5f98bdd285af934f02699b88bff`   | https://github.com/tkhq/sdk/tree/60a997f4c52ac5f98bdd285af934f02699b88bff/packages/encoding |
   205	    45	| @turnkey/crypto            | 2.8.8   | `b35dc642bd7c1728f97acd43d4cba66976b65084`   | https://github.com/tkhq/sdk/tree/b35dc642bd7c1728f97acd43d4cba66976b65084/packages/crypto |
   206	    46	| @turnkey/api-key-stamper   | 0.5.0   | `b711befbb88ec522452dbdac68f0e98762be10dd`   | https://github.com/tkhq/sdk/tree/b711befbb88ec522452dbdac68f0e98762be10dd/packages/api-key-stamper |
   207	    47	| @turnkey/http              | 3.16.0  | `8def9ba521233137437ac7294693a4ae0a0d14da`   | https://github.com/tkhq/sdk/tree/8def9ba521233137437ac7294693a4ae0a0d14da/packages/http |
   208	    48	
   209	    49	## Re-pinning procedure
   210	    50	
   211	    51	When bumping any pin:
   212	    52	
   213	    53	1. Update the table above (npm version, refresh tarball sha256, gitHead).
   214	    54	2. Update `Directory.Packages.props` or any version comment.
   215	    55	3. Re-run `npm pack` and refresh the extracted directories under
   216	    56	   `upstream-snapshots/`.
   217	    57	4. Re-run **3 Codex review rounds per affected file** via
   218	    58	   [`codex-crypto-review.sh`](./codex-crypto-review.sh).
   219	    59	5. Update `CHANGELOG.md`.
   220	

exec
/bin/zsh -lc "rg -n \"lowS|low-s|canonical|verify\\(\" . -g '*.js' -g '*.mjs' -g '*.ts'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts:13:export declare function verify(appProof: v1AppProof, bootProof: v1BootProof): Promise<void>;
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/shared.ts:78:  if (verifier.verify(pemPublicKey, signature, "hex")) {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:68:async function verify(appProof, bootProof) {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:183:                const ok = await cert.verify({
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:199:                const ok = await cert.verify({
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:225:    const ok = await cryptoInstance.subtle.verify({ name: "ECDSA", hash: { name: "SHA-384" } }, pubKey, new Uint8Array(signature), tbs);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:60:export async function verify(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:223:        const ok = await cert.verify({
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:238:        const ok = await cert.verify({
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:275:  const ok = await cryptoInstance.subtle.verify(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:70:      verify(testAppProof1, testBootProof1),
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:73:      verify(testAppProof2, testBootProof2),
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:107:    await expect(verify(testAppProof2, malformedBootProof2)).rejects.toThrow(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:116:    await expect(verify(testAppProof2, malformedBootProof2)).rejects.toThrow(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:125:    await expect(verify(testAppProof2, malformedBootProof2)).rejects.toThrow(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:46:async function verify(appProof, bootProof) {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:161:                const ok = await cert.verify({
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:177:                const ok = await cert.verify({
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:203:    const ok = await cryptoInstance.subtle.verify({ name: "ECDSA", hash: { name: "SHA-384" } }, pubKey, new Uint8Array(signature), tbs);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);

exec
/bin/zsh -lc "find . -path '*node_modules*' -type f -path '*@noble*' | sed -n '1,40p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "nl -ba src/CryptoConstants.cs | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// Implementation-specific constants for the BouncyCastle-backed C# port of
     2	// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
     3	// they exist because BouncyCastle requires explicit curve / parameter inputs
     4	// where the upstream uses @noble/curves which hides them.
     5	//
     6	// Upstream snapshot:
     7	//   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
     8	//
     9	// Mapping to upstream:
    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
    12	//     checks in turnkey.ts.
    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
    14	//     `crypto.ts uncompressRawPublicKey`:
    15	//       p = BigInt("0xffff...ffffffff")   (P-256 prime)
    16	//       b = BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")
    17	//       a = p - BigInt(3)
    18	//     Pulled out into named constants here so the BouncyCastle wiring stays
    19	//     readable and so a Codex reviewer can confirm the bytes match.
    20	//
    21	// These constants are wire-irrelevant by themselves; what matters is that
    22	// the BouncyCastle code paths that consume them produce the same bytes as
    23	// the upstream @noble/curves paths. Wire-byte parity is verified by the
    24	// HPKE / bundle-decrypt / signature-verify test fixtures.
    25	
    26	namespace Turnkey
    27	{
    28	    /// <summary>
    29	    /// Constants required by the BouncyCastle-backed C# port of
    30	    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
    31	    /// </summary>
    32	    public static class CryptoConstants
    33	    {
    34	        /// <summary>
    35	        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
    36	        /// Used with <c>ECNamedCurveTable.GetByName</c>.
    37	        /// </summary>
    38	        public const string CURVE_NAME = "secp256r1";
    39	
    40	        /// <summary>
    41	        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
    42	        /// Equivalent to <c>turnkey/crypto</c> hard-coded
    43	        /// <c>compressedKey.length === 33</c> checks.
    44	        /// </summary>
    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
    46	
    47	        /// <summary>
    48	        /// NIST P-256 prime field modulus (p), decimal form.
    49	        /// </summary>
    50	        /// <remarks>
    51	        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
    52	        /// hex literal
    53	        /// <c>0xffffffff00000001000000000000000000000000ffffffffffffffffffffffff</c>.
    54	        /// The decimal form is given here so BouncyCastle's
    55	        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
    56	        /// constructor parses it directly.
    57	        /// </remarks>
    58	        public const string P256_P =
    59	            "115792089210356248762697446949407573530086143415290314195533631308867097853951";
    60	
    61	        /// <summary>
    62	        /// NIST P-256 curve coefficient b, hex form. See FIPS 186-4 Appendix D.
    63	        /// Upstream inlines this as
    64	        /// <c>BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")</c>.
    65	        /// </summary>
    66	        public const string P256_B =
    67	            "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
    68	
    69	        /// <summary>
    70	        /// Offset used to derive the P-256 coefficient a as <c>p - 3</c>.
    71	        /// Upstream inlines <c>const a = p - BigInt(3)</c>.
    72	        /// </summary>
    73	        public const string P256_A_OFFSET = "3";
    74	    }
    75	}

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js | sed -n '40,330p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
    40	            receiverPriv: embeddedKey,
    41	        });
    42	        return encoding.uint8ArrayToHexString(decryptedData);
    43	    }
    44	    catch (error) {
    45	        throw new Error(`"Error decrypting bundle:", ${error}`);
    46	    }
    47	};
    48	/**
    49	 * Decrypt an encrypted export bundle (such as a private key or wallet account bundle).
    50	 *
    51	 * This function verifies the enclave signature to ensure the authenticity of the encrypted data.
    52	 * It uses HPKE (Hybrid Public Key Encryption) to decrypt the contents of the bundle and returns
    53	 * either the decrypted mnemonic or the decrypted data in hexadecimal format, based on the
    54	 * `returnMnemonic` flag.
    55	 *
    56	 * @param {DecryptExportBundleParams} params - An object containing the following properties:
    57	 *   - exportBundle {string}: The encrypted export bundle in JSON format.
    58	 *   - organizationId {string}: The expected organization ID to verify against the signed data.
    59	 *   - embeddedKey {string}: The private key used for decrypting the data.
    60	 *   - dangerouslyOverrideSignerPublicKey {string} [Optional]: Optionally override the default signer public key used for verifying the signature. This should only be done for testing
    61	 *   - returnMnemonic {boolean}: If true, returns the decrypted data as a mnemonic string; otherwise, returns it in hexadecimal format.
    62	 * @returns {Promise<string>} - A promise that resolves to the decrypted mnemonic or decrypted hexadecimal data.
    63	 * @throws {Error} - If decryption or signature verification fails, throws an error with details.
    64	 */
    65	const decryptExportBundle = async ({ exportBundle, embeddedKey, organizationId, dangerouslyOverrideSignerPublicKey, keyFormat, returnMnemonic, }) => {
    66	    try {
    67	        const parsedExportBundle = JSON.parse(exportBundle);
    68	        const verified = await verifyEnclaveSignature(parsedExportBundle.enclaveQuorumPublic, parsedExportBundle.dataSignature, parsedExportBundle.data, dangerouslyOverrideSignerPublicKey);
    69	        if (!verified) {
    70	            throw new Error(`failed to verify enclave signature: ${parsedExportBundle}`);
    71	        }
    72	        const signedData = JSON.parse(new TextDecoder().decode(encoding.uint8ArrayFromHexString(parsedExportBundle.data)));
    73	        if (!signedData.organizationId ||
    74	            signedData.organizationId !== organizationId) {
    75	            throw new Error(`organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`);
    76	        }
    77	        if (!signedData.encappedPublic) {
    78	            throw new Error('missing "encappedPublic" in bundle signed data');
    79	        }
    80	        const encappedKeyBuf = encoding.uint8ArrayFromHexString(signedData.encappedPublic);
    81	        const ciphertextBuf = encoding.uint8ArrayFromHexString(signedData.ciphertext);
    82	        const decryptedData = crypto.hpkeDecrypt({
    83	            ciphertextBuf,
    84	            encappedKeyBuf,
    85	            receiverPriv: embeddedKey,
    86	        });
    87	        if (keyFormat === "SOLANA" && !returnMnemonic) {
    88	            if (decryptedData.length !== 32) {
    89	                throw new Error(`invalid private key length. Expected 32 bytes. Got ${decryptedData.length}.`);
    90	            }
    91	            const publicKeyBytes = ed25519.ed25519.getPublicKey(decryptedData);
    92	            if (publicKeyBytes.length !== 32) {
    93	                throw new Error(`invalid public key length. Expected 32 bytes. Got ${publicKeyBytes.length}.`);
    94	            }
    95	            const concatenatedBytes = new Uint8Array(64);
    96	            concatenatedBytes.set(decryptedData, 0);
    97	            concatenatedBytes.set(publicKeyBytes, 32);
    98	            return encoding.bs58.encode(concatenatedBytes);
    99	        }
   100	        const decryptedDataHex = encoding.uint8ArrayToHexString(decryptedData);
   101	        return returnMnemonic ? encoding.hexToAscii(decryptedDataHex) : decryptedDataHex;
   102	    }
   103	    catch (error) {
   104	        throw new Error(`Error decrypting bundle: ${error}`);
   105	    }
   106	};
   107	/**
   108	 * Verifies a signature from a Turnkey stamp using ECDSA and SHA-256.
   109	 *
   110	 * @param {string} publicKey - The public key of the authenticator (e.g. WebAuthn or P256 API key).
   111	 * @param {string} signature - The ECDSA signature in DER format.
   112	 * @param {string} signedData - The data that was signed (e.g. JSON-stringified Turnkey request body).
   113	 * @returns {Promise<boolean>} - Returns true if the signature is valid, otherwise throws an error.
   114	 *
   115	 * @example
   116	 *
   117	 * const stampedRequest = await turnkeyClient.stampGetWhoami(...);
   118	 * const decodedStampContents = atob(stampedRequest.stamp.stampHeaderValue);
   119	 * const parsedStampContents = JSON.parse(decodedStampContents);
   120	 * const signature = parsedStampContents.signature;
   121	 *
   122	 * await verifyStampSignature(publicKey, signature, stampedRequest.body)
   123	 */
   124	const verifyStampSignature = async (publicKey, signature, signedData) => {
   125	    const publicKeyBuffer = encoding.uint8ArrayFromHexString(publicKey);
   126	    const loadedPublicKey = loadPublicKey(publicKeyBuffer);
   127	    if (!loadedPublicKey) {
   128	        throw new Error("failed to load public key");
   129	    }
   130	    // Convert the ASN.1 DER-encoded signature for verification
   131	    const publicSignatureBuf = crypto.fromDerSignature(signature);
   132	    const signedDataBuf = new TextEncoder().encode(signedData);
   133	    const hashedData = sha256.sha256(signedDataBuf);
   134	    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
   135	};
   136	/**
   137	 * Verifies a signature from a Turnkey enclave using ECDSA and SHA-256.
   138	 *
   139	 * @param {string} enclaveQuorumPublic - The public key of the enclave signer.
   140	 * @param {string} publicSignature - The ECDSA signature in DER format.
   141	 * @param {string} signedData - The data that was signed.
   142	 * @param {Environment} dangerouslyOverrideSignerPublicKey - (optional) an enum (PROD or PREPROD) to verify against the correct signer enclave key.
   143	 * @returns {Promise<boolean>} - Returns true if the signature is valid, otherwise throws an error.
   144	 */
   145	const verifyEnclaveSignature = async (enclaveQuorumPublic, publicSignature, signedData, dangerouslyOverrideSignerPublicKey) => {
   146	    const expectedSignerPublicKey = dangerouslyOverrideSignerPublicKey || constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY;
   147	    if (enclaveQuorumPublic != expectedSignerPublicKey) {
   148	        throw new Error(`expected signer key ${dangerouslyOverrideSignerPublicKey ?? constants.PRODUCTION_SIGNER_SIGN_PUBLIC_KEY} does not match signer key from bundle: ${enclaveQuorumPublic}`);
   149	    }
   150	    const encryptionQuorumPublicBuf = new Uint8Array(encoding.uint8ArrayFromHexString(enclaveQuorumPublic));
   151	    const quorumKey = loadPublicKey(encryptionQuorumPublicBuf);
   152	    if (!quorumKey) {
   153	        throw new Error("failed to load quorum key");
   154	    }
   155	    // Convert the ASN.1 DER-encoded signature for verification
   156	    const publicSignatureBuf = crypto.fromDerSignature(publicSignature);
   157	    const signedDataBuf = encoding.uint8ArrayFromHexString(signedData);
   158	    const hashedData = sha256.sha256(signedDataBuf);
   159	    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
   160	};
   161	/**
   162	 * Loads an ECDSA public key from a raw format for signature verification.
   163	 *
   164	 * @param {Uint8Array} publicKey - The raw P-256 public key bytes.
   165	 * @returns {ProjPointType<bigint>} - The parsed ECDSA public key.
   166	 * @throws {Error} - If the public key is invalid.
   167	 */
   168	const loadPublicKey = (publicKey) => {
   169	    return p256.p256.ProjectivePoint.fromHex(encoding.uint8ArrayToHexString(publicKey));
   170	};
   171	/**
   172	 * Decodes a private key based on the specified format.
   173	 *
   174	 * @param {string} privateKey - The private key to decode.
   175	 * @param {string} keyFormat - The format of the private key (e.g., "SOLANA", "HEXADECIMAL").
   176	 * @returns {Uint8Array} - The decoded private key.
   177	 */
   178	const decodeKey = (privateKey, keyFormat) => {
   179	    switch (keyFormat) {
   180	        case "SOLANA":
   181	            const decodedKeyBytes = encoding.bs58.decode(privateKey);
   182	            if (decodedKeyBytes.length !== 64) {
   183	                throw new Error(`invalid key length. Expected 64 bytes. Got ${decodedKeyBytes.length}.`);
   184	            }
   185	            return decodedKeyBytes.subarray(0, 32);
   186	        case "HEXADECIMAL":
   187	            if (privateKey.startsWith("0x")) {
   188	                return encoding.uint8ArrayFromHexString(privateKey.slice(2));
   189	            }
   190	            return encoding.uint8ArrayFromHexString(privateKey);
   191	        default:
   192	            console.warn(`invalid key format: ${keyFormat}. Defaulting to HEXADECIMAL.`);
   193	            if (privateKey.startsWith("0x")) {
   194	                return encoding.uint8ArrayFromHexString(privateKey.slice(2));
   195	            }
   196	            return encoding.uint8ArrayFromHexString(privateKey);
   197	    }
   198	};
   199	/**
   200	 * Encrypts a private key bundle using HPKE and verifies the enclave signature.
   201	 *
   202	 * @param {EncryptPrivateKeyToBundleParams} params - An object containing the private key, key format, bundle, user, and organization details. Optionally, you can override the default signer key (for testing purposes)
   203	 * @returns {Promise<string>} - A promise that resolves to a JSON string representing the encrypted bundle.
   204	 * @throws {Error} - If enclave signature verification or any other validation fails.
   205	 */
   206	const encryptPrivateKeyToBundle = async ({ privateKey, keyFormat, importBundle, userId, organizationId, dangerouslyOverrideSignerPublicKey, }) => {
   207	    const parsedImportBundle = JSON.parse(importBundle);
   208	    const plainTextBuf = decodeKey(privateKey, keyFormat);
   209	    const verified = await verifyEnclaveSignature(parsedImportBundle.enclaveQuorumPublic, parsedImportBundle.dataSignature, parsedImportBundle.data, dangerouslyOverrideSignerPublicKey);
   210	    if (!verified) {
   211	        throw new Error(`failed to verify enclave signature: ${importBundle}`);
   212	    }
   213	    const signedData = JSON.parse(new TextDecoder().decode(encoding.uint8ArrayFromHexString(parsedImportBundle.data)));
   214	    if (!signedData.organizationId ||
   215	        signedData.organizationId !== organizationId) {
   216	        throw new Error(`organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`);
   217	    }
   218	    if (!signedData.userId || signedData.userId !== userId) {
   219	        throw new Error(`user id does not match expected value. Expected: ${userId}. Found: ${signedData.userId}.`);
   220	    }
   221	    if (!signedData.targetPublic) {
   222	        throw new Error('missing "targetPublic" in bundle signed data');
   223	    }
   224	    // Load target public key generated from enclave
   225	    const targetKeyBuf = encoding.uint8ArrayFromHexString(signedData.targetPublic);
   226	    const privateKeyBundle = crypto.hpkeEncrypt({ plainTextBuf, targetKeyBuf });
   227	    return crypto.formatHpkeBuf(privateKeyBundle);
   228	};
   229	/**
   230	  /**
   231	   * Encrypts a mnemonic wallet bundle using HPKE and verifies the enclave signature.
   232	   *
   233	   * @param {EncryptWalletToBundleParams} params - An object containing the mnemonic, bundle, user, and organization details. Optionally, you can override the default signer key (for testing purposes).
   234	   * @returns {Promise<string>} - A promise that resolves to a JSON string representing the encrypted wallet bundle.
   235	   * @throws {Error} - If enclave signature verification or any other validation fails.
   236	   */
   237	const encryptWalletToBundle = async ({ mnemonic, importBundle, userId, organizationId, dangerouslyOverrideSignerPublicKey, }) => {
   238	    const parsedImportBundle = JSON.parse(importBundle);
   239	    const plainTextBuf = new TextEncoder().encode(mnemonic);
   240	    const verified = await verifyEnclaveSignature(parsedImportBundle.enclaveQuorumPublic, parsedImportBundle.dataSignature, parsedImportBundle.data, dangerouslyOverrideSignerPublicKey);
   241	    if (!verified) {
   242	        throw new Error(`failed to verify enclave signature: ${importBundle}`);
   243	    }
   244	    const signedData = JSON.parse(new TextDecoder().decode(encoding.uint8ArrayFromHexString(parsedImportBundle.data)));
   245	    if (!signedData.organizationId ||
   246	        signedData.organizationId !== organizationId) {
   247	        throw new Error(`organization id does not match expected value. Expected: ${organizationId}. Found: ${signedData.organizationId}.`);
   248	    }
   249	    if (!signedData.userId || signedData.userId !== userId) {
   250	        throw new Error(`user id does not match expected value. Expected: ${userId}. Found: ${signedData.userId}.`);
   251	    }
   252	    if (!signedData.targetPublic) {
   253	        throw new Error('missing "targetPublic" in bundle signed data');
   254	    }
   255	    // Load target public key generated from enclave
   256	    const targetKeyBuf = encoding.uint8ArrayFromHexString(signedData.targetPublic);
   257	    const privateKeyBundle = crypto.hpkeEncrypt({ plainTextBuf, targetKeyBuf });
   258	    return crypto.formatHpkeBuf(privateKeyBundle);
   259	};
   260	/**
   261	 * Verifies that a **session JWT** was signed by Turnkey’s
   262	 * notarizer key (P-256 / ES256, compact 64-byte r‖s signature).
   263	 *
   264	 * How it works
   265	 * ------------
   266	 * 1.  Split the JWT into `header.payload.signature`.
   267	 * 2.  **Double-hash** the string `"header.payload"`:
   268	 *        `h1 = sha256(header.payload)`
   269	 *        `msg = sha256(h1)`
   270	 *     (The Rust signer feeds `h1` into `SigningKey::sign`, which hashes once
   271	 *     more internally, yielding `msg`.)
   272	 * 3.  Base64-URL-decode the signature (`r||s`, 64 bytes).
   273	 * 4.  Import the notarizer public key (hex `04‖X‖Y` → `Uint8Array`).
   274	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
   275	 *     `msg` as a pre-hashed digest and performs ECDSA verification.
   276	 *
   277	 * @param jwt   The session JWT to validate.
   278	 * @param dangerouslyOverrideNotarizerPublicKey *(optional)* Hex-encoded
   279	 *              uncompressed P-256 public key to verify against (use only in
   280	 *              tests).  Defaults to the production notarizer key.
   281	 * @returns `true` if the signature is valid for the given key, else `false`.
   282	 * @throws  If the JWT is malformed.
   283	 */
   284	const verifySessionJwtSignature = async (jwt, dangerouslyOverrideNotarizerPublicKey) => {
   285	    const notarizerKeyHex = dangerouslyOverrideNotarizerPublicKey ??
   286	        constants.PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY;
   287	    /* 1. split JWT -------------------------------------------------------- */
   288	    const [headerB64, payloadB64, signatureB64] = jwt.split(".");
   289	    if (!signatureB64)
   290	        throw new Error("invalid JWT: need 3 parts");
   291	    const signingInput = `${headerB64}.${payloadB64}`;
   292	    /* 2. sha256(sha256(header.payload)) ----------------------------------- */
   293	    const h1 = sha256.sha256(new TextEncoder().encode(signingInput));
   294	    const msgDigest = sha256.sha256(h1); // 32-byte Uint8Array
   295	    /* 3. base64-url decode signature -------------------------------------- */
   296	    const toB64 = (u) => (u = u.replace(/-/g, "+").replace(/_/g, "/")).padEnd(u.length + ((4 - (u.length % 4)) % 4), "=");
   297	    const signature = Uint8Array.from(atob(toB64(signatureB64))
   298	        .split("")
   299	        .map((c) => c.charCodeAt(0))); // 64 bytes
   300	    /* 4. load public key -------------------------------------------------- */
   301	    const publicKey = encoding.uint8ArrayFromHexString(notarizerKeyHex);
   302	    /* 5. verify ----------------------------------------------------------- */
   303	    return p256.p256.verify(signature, msgDigest, publicKey);
   304	};
   305	/**
   306	 * Encrypts a message to an uncompressed P256 public key
   307	 * The function takes in standard strings and converts them
   308	 * to Uint8Arrays to be used by the lower level quorumKeyEncrypt
   309	 * function. More details about how the encryption works is described
   310	 * in that function's documentation.
   311	 *
   312	 * @param targetPublicKeyUncompressed A hex string uncompressed public key to encrypt a message to
   313	 * @param message A standard string message to encrypt, does not have to be hex encoded
   314	 * @returns {Promise<Uint8Array>} A borsh serialized envelope with the encrypted message (more details found in quorumKeyEncrypt)
   315	 */
   316	const encryptToEnclave = async (targetPublicKeyUncompressed, message) => {
   317	    return await crypto.quorumKeyEncrypt(encoding.uint8ArrayFromHexString(targetPublicKeyUncompressed), new TextEncoder().encode(message));
   318	};
   319	/**
   320	 * Helper function used specifically to encrypt a client secret to
   321	 * TLS Fetchers quorum key. This is used for client_secret upload
   322	 * when enabling authentication with an OAuth 2.0 provider
   323	 *
   324	 * @param client_secret The client secret issued by the OAuth 2.0 provider
   325	 * @param dangerouslyOverrideTlsFetcherPublicKey *(optional)* Hex-encoded
   326	 *              uncompressed P-256 public key to encrypt to (use only in
   327	 *              tests/dev environment).  Defaults to the production TLS Fetcher key.
   328	 * @returns {Promise<string>} A hex encoded borsh serialized envelope with the encrypted client
   329	 *                            secret meant to be passed to the CreateOauth2Credential Activity
   330	 */

codex
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| `Crypto.Math.ModSqrt` (`src/Crypto.cs:140`) | `modSqrt` (`ts-source/math.ts:4`) | REVIEWED | Same P-256 call-path semantics; negative public input differs, listed in E. |
| `Crypto.Hkdf.Extract` (`src/Crypto.cs:187`) | `hkdf.extract` call (`ts-source/crypto.ts:551`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
| `Crypto.Hkdf.Expand` (`src/Crypto.cs:204`) | `hkdf.expand` call (`ts-source/crypto.ts:552`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
| `GetPublicKey(byte[])` (`src/Crypto.cs:321`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | P-256 SEC1 output matches compressed flag. |
| `GetPublicKey(string)` (`src/Crypto.cs:341`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | C# overload for TS union input. |
| `GenerateP256KeyPair` (`src/Crypto.cs:350`) | `generateP256KeyPair` (`ts-source/crypto.ts:358`) | REVIEWED | Key generation distribution/error behavior differs, listed in E. |
| `HpkeDecrypt` (`src/Crypto.cs:384`) | `hpkeDecrypt` (`ts-source/crypto.ts:308`) | REVIEWED | HPKE steps match; error wrapping differs, listed in E. |
| `HpkeEncrypt` (`src/Crypto.cs:433`) | `hpkeEncrypt` (`ts-source/crypto.ts:95`) | REVIEWED | HPKE steps match; null/error behavior differs, listed in E. |
| `BuildAdditionalAssociatedData` (`src/Crypto.cs:471`) | `buildAdditionalAssociatedData` (`ts-source/crypto.ts:378`) | REVIEWED | Concatenation matches. |
| `CompressRawPublicKey` (`src/Crypto.cs:480`) | `compressRawPublicKey` (`ts-source/crypto.ts:406`) | REVIEWED | Valid-key output matches; validation differs, listed in E. |
| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
| `FormatHpkeBuf` (`src/Crypto.cs:576`) | `formatHpkeBuf` (`ts-source/crypto.ts:286`) | REVIEWED | JSON field order/name matches; short-buffer behavior differs, listed in E. |
| `DecryptCredentialBundle` (`src/Crypto.cs:609`) | `decryptCredentialBundle` (`ts-source/turnkey.ts:72`) | REVIEWED | HPKE path matches; Base58 fallback differs, listed in E. |
| `EncryptPrivateKeyToBundle` (`src/Crypto.cs:651`) | `encryptPrivateKeyToBundle` (`ts-source/turnkey.ts:317`) | REVIEWED | Main happy path matches; DTO/validation/order differ, listed in E. |
| `DecryptExportBundle` (`src/Crypto.cs:729`) | `decryptExportBundle` (`ts-source/turnkey.ts:116`) | REVIEWED | Multiple semantic differences, listed in E. |
| `VerifySessionJwtSignature` (`src/Crypto.cs:851`) | `verifySessionJwtSignature` (`ts-source/turnkey.ts:444`) | REVIEWED | Double-SHA256 and raw signature path reviewed; malformed/error behavior differs, listed in E. |
| `DeriveSS` (`src/Crypto.cs:898`) | `deriveSS` (`ts-source/crypto.ts:559`) | REVIEWED | Returns x-coordinate shared secret, padded to 32 bytes. |
| `GetKemContext` (`src/Crypto.cs:924`) | `getKemContext` (`ts-source/crypto.ts:598`) | REVIEWED | Concatenation matches. |
| `BuildLabeledIkm` (`src/Crypto.cs:931`) | `buildLabeledIkm` (`ts-source/crypto.ts:491`) | REVIEWED | Label layout matches. |
| `BuildLabeledInfo` (`src/Crypto.cs:937`) | `buildLabeledInfo` (`ts-source/crypto.ts:524`) | REVIEWED | HPKE info layout matches for reviewed lengths. |
| `ExtractAndExpand` (`src/Crypto.cs:955`) | `extractAndExpand` (`ts-source/crypto.ts:545`) | REVIEWED | HKDF extract/expand order matches. |
| `AesGcmDecrypt` (`src/Crypto.cs:961`) | `aesGcmDecrypt` (`ts-source/crypto.ts:584`) | REVIEWED | AES-GCM decrypt/tag handling matches. |
| `AesGcmEncrypt` (`src/Crypto.cs:972`) | `aesGcmEncrypt` (`ts-source/crypto.ts:570`) | REVIEWED | AES-GCM ciphertext||tag output matches. |
| `Base64UrlDecode` (`src/Crypto.cs:983`) | inline `toB64` decode (`ts-source/turnkey.ts:461`) | REVIEWED | Padding/base64url transform reviewed. |
| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
| `VerifyEnclaveSignature` (`src/Crypto.cs:1024`) | `verifyEnclaveSignature` (`ts-source/turnkey.ts:234`) | REVIEWED | DER signature verification reviewed; override missing in E. |
| `VerifySignature` (`src/Crypto.cs:1052`) | no current upstream equivalent | REVIEWED | C# legacy-envelope extension; listed in E. |
| `DecodeKey` (`src/Crypto.cs:1077`) | `decodeKey` (`ts-source/turnkey.ts:284`) | REVIEWED | SOLANA/hex decode matches for import; error text differs. |
| `GetStringOrNull` (`src/Crypto.cs:1098`) | JSON property access inline | REVIEWED | Structural parser helper; non-string handling differs only on malformed JSON. |

**C. Intentional Adaptations**

- `Uint8Array` is represented as `byte[]`; this is structural and preserves byte order.
- TS union inputs are represented as C# overloads/DTO classes; this preserves wire values.
- `System.Text.Json` source generation replaces `JSON.stringify` for `HpkeBundlePayload`; property names/order are explicit and unindented.
- BouncyCastle EC point/ECDH/ECDSA/AES-GCM primitives replace noble/WebCrypto primitives; same curves, encodings, digests, and tag layout.
- Manual RFC5869 HKDF with `HMACSHA256` replaces `@noble/hashes/hkdf`; same extract/expand inputs and output lengths.
- Raw JWT `r||s` signatures are converted to DER only because BouncyCastle verifies DER; the verified mathematical signature is the same.
- BigInteger construction uses explicit positive sign and fixed-width padding; this preserves SEC1 big-endian leading-zero behavior.

**D. D17 Enforcement Check**

Crypto.cs check: no banned API present.

- No `Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator`.
- No `Org.BouncyCastle.Crypto.Hpke.*`.
- No high-level KDF or HPKE wrapper.
- BouncyCastle usage is limited to EC key/point/ECDH/ECDSA, AES-GCM, SHA-256 signing mode, BigInteger, and related primitive parameter types. The broad `Org.BouncyCastle.Crypto.Generators` using is used for `ECKeyPairGenerator`, not HKDF.

**E. Logic Divergence Findings**

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Crypto.cs:151` | `ts-source/math.ts:8` | Public negative `x` differs: C# `BigInteger.Mod` normalizes non-negative; JS `%` keeps negative sign. Crypto call sites use non-negative EC coordinates, but the public helper is observably different. | Mirror JS remainder for public parity or make helper private/document non-negative input. |
| `src/Crypto.cs:355` | `ts-source/crypto.ts:358-363` | C# uses `ECKeyPairGenerator`, which samples valid scalars; TS samples 32 random bytes then derives public key. Rare invalid scalar/failure/distribution behavior differs. | If strict parity is required, generate 32 random bytes and derive with the same validation behavior; otherwise document as deliberate hardening. |
| `src/Crypto.cs:435-438` | `ts-source/crypto.ts:95-151` | `HpkeEncrypt` treats null `PlainTextBuf` as empty and does not wrap errors as `Unable to perform hpkeEncrypt`. TS catches and wraps all errors. | Require non-null plaintext and wrap exceptions with the upstream message. |
| `src/Crypto.cs:386-421` | `ts-source/crypto.ts:308-350` | `HpkeDecrypt` throws C# argument/crypto exceptions directly; TS wraps all errors as `Unable to perform hpkeDecrypt: ...`. | Wrap non-fatal exceptions with upstream-equivalent message. |
| `src/Crypto.cs:482-486` | `ts-source/crypto.ts:406-418` | C# rejects non-65-byte or non-`0x04` input; TS slices any input and mutates prefix based on last byte. Valid public keys match, malformed input behavior differs. | Remove extra validation for parity or keep and document stricter validation. |
| `src/Crypto.cs:506` | `ts-source/crypto.ts:426-462` | C# only supports P-256; upstream exposes optional `curve` and supports `CURVE_SECP256K1`. | Add curve parameter/support or document intentional P-256-only subset. |
| `src/Crypto.cs:509-512` | `ts-source/crypto.ts:430-432` | Invalid compressed length error message differs. | Use upstream message if exact error parity matters. |
| `src/Crypto.cs:579-582` | `ts-source/crypto.ts:286-299` | C# rejects buffers `<= 33`; TS slices first 33 bytes and allows downstream behavior. A valid compressed key plus empty ciphertext would stringify upstream but throw in C#. | Match TS slicing behavior, or only reject `< 33` if defensive validation is desired. |
| `src/Crypto.cs:614-619` | `ts-source/turnkey.ts:76-77` | C# falls back from Base58Check to raw Base58; upstream only accepts Base58Check. This can accept bundles TS rejects. | Remove raw Base58 fallback. |
| `src/Crypto.cs:276-283`, `289-296`, `851` | `ts-source/turnkey.ts:31-45`, `116-123`, `444-450` | DTO/API shape omits `dangerouslyOverrideSignerPublicKey` and `dangerouslyOverrideNotarizerPublicKey`. | Add optional override fields/parameters and pass them through to verification helpers. |
| `src/Crypto.cs:665-708` | `ts-source/turnkey.ts:325-360` | `EncryptPrivateKeyToBundle` verifies/parses bundle before `DecodeKey`; upstream decodes private key before signature verification. Error precedence differs when both inputs are bad. | Move `DecodeKey` immediately after JSON parse to match upstream. |
| `src/Crypto.cs:753-767`, `1052` | `ts-source/turnkey.ts:124-162` | C# accepts a legacy `signature`/`signedData` envelope path not present in upstream 2.8.8. | Remove from this port or isolate as a separately named compatibility helper. |
| `src/Crypto.cs:796-803` | `ts-source/turnkey.ts:144-151` | C# allows missing `organizationId`; upstream rejects missing or mismatched `organizationId`. | Require `bundleOrgId` to be non-empty and equal. |
| `src/Crypto.cs:827-830` | `ts-source/turnkey.ts:164-180` | SOLANA export output is wrong: C# Base58-encodes only 32 decrypted private-key bytes; upstream derives Ed25519 public key, concatenates 32 private + 32 public bytes, then Base58-encodes 64 bytes. | Derive Ed25519 public key and encode the 64-byte concatenation. |
| `src/Crypto.cs:853-890` | `ts-source/turnkey.ts:452-477` | Malformed JWT behavior differs: C# returns `false` for wrong part count and catches all errors; upstream throws for missing signature part and returns verifier result otherwise. | Throw on malformed JWT and avoid catch-all if exact upstream behavior is required. |

**F. Fixture Comparison Gate**

Not satisfied for `Crypto.cs`.

`tests/Fixtures/` currently contains only `tests/Fixtures/encoding/turnkey-encoding-vectors.json`; no fixture under `tests/Fixtures/` exercises `src/Crypto.cs`. `tests/Fixtures/README.md` lists `crypto/` as planned, but the directory is absent. `tests/CryptoTests.cs` uses inline RFC/NIST/random roundtrip tests and explicitly states positive Turnkey-signed bundle tests are not present.

So I cannot confirm that every Crypto.cs fixture was generated from the pinned upstream package or that C# asserts the same bytes Node would produce. There are no applicable Crypto fixtures to compare.

**G. Unresolved Assumptions**

- I did not run `dotnet test`; the workspace is read-only and test/build output would require writes.
- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
- Positive Turnkey-signed import/export/credential bundle parity could not be verified because no pinned Node-generated Crypto fixtures are present.
- Pass criterion is not met this round: section B has zero `NOT-REVIEWED` rows and D passes, but E has divergences and F is not satisfied.


tokens used
97,359
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| `Crypto.Math.ModSqrt` (`src/Crypto.cs:140`) | `modSqrt` (`ts-source/math.ts:4`) | REVIEWED | Same P-256 call-path semantics; negative public input differs, listed in E. |
| `Crypto.Hkdf.Extract` (`src/Crypto.cs:187`) | `hkdf.extract` call (`ts-source/crypto.ts:551`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
| `Crypto.Hkdf.Expand` (`src/Crypto.cs:204`) | `hkdf.expand` call (`ts-source/crypto.ts:552`) | REVIEWED | Manual RFC5869 HMAC-SHA256 implementation. |
| `GetPublicKey(byte[])` (`src/Crypto.cs:321`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | P-256 SEC1 output matches compressed flag. |
| `GetPublicKey(string)` (`src/Crypto.cs:341`) | `getPublicKey` (`ts-source/crypto.ts:80`) | REVIEWED | C# overload for TS union input. |
| `GenerateP256KeyPair` (`src/Crypto.cs:350`) | `generateP256KeyPair` (`ts-source/crypto.ts:358`) | REVIEWED | Key generation distribution/error behavior differs, listed in E. |
| `HpkeDecrypt` (`src/Crypto.cs:384`) | `hpkeDecrypt` (`ts-source/crypto.ts:308`) | REVIEWED | HPKE steps match; error wrapping differs, listed in E. |
| `HpkeEncrypt` (`src/Crypto.cs:433`) | `hpkeEncrypt` (`ts-source/crypto.ts:95`) | REVIEWED | HPKE steps match; null/error behavior differs, listed in E. |
| `BuildAdditionalAssociatedData` (`src/Crypto.cs:471`) | `buildAdditionalAssociatedData` (`ts-source/crypto.ts:378`) | REVIEWED | Concatenation matches. |
| `CompressRawPublicKey` (`src/Crypto.cs:480`) | `compressRawPublicKey` (`ts-source/crypto.ts:406`) | REVIEWED | Valid-key output matches; validation differs, listed in E. |
| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
| `FormatHpkeBuf` (`src/Crypto.cs:576`) | `formatHpkeBuf` (`ts-source/crypto.ts:286`) | REVIEWED | JSON field order/name matches; short-buffer behavior differs, listed in E. |
| `DecryptCredentialBundle` (`src/Crypto.cs:609`) | `decryptCredentialBundle` (`ts-source/turnkey.ts:72`) | REVIEWED | HPKE path matches; Base58 fallback differs, listed in E. |
| `EncryptPrivateKeyToBundle` (`src/Crypto.cs:651`) | `encryptPrivateKeyToBundle` (`ts-source/turnkey.ts:317`) | REVIEWED | Main happy path matches; DTO/validation/order differ, listed in E. |
| `DecryptExportBundle` (`src/Crypto.cs:729`) | `decryptExportBundle` (`ts-source/turnkey.ts:116`) | REVIEWED | Multiple semantic differences, listed in E. |
| `VerifySessionJwtSignature` (`src/Crypto.cs:851`) | `verifySessionJwtSignature` (`ts-source/turnkey.ts:444`) | REVIEWED | Double-SHA256 and raw signature path reviewed; malformed/error behavior differs, listed in E. |
| `DeriveSS` (`src/Crypto.cs:898`) | `deriveSS` (`ts-source/crypto.ts:559`) | REVIEWED | Returns x-coordinate shared secret, padded to 32 bytes. |
| `GetKemContext` (`src/Crypto.cs:924`) | `getKemContext` (`ts-source/crypto.ts:598`) | REVIEWED | Concatenation matches. |
| `BuildLabeledIkm` (`src/Crypto.cs:931`) | `buildLabeledIkm` (`ts-source/crypto.ts:491`) | REVIEWED | Label layout matches. |
| `BuildLabeledInfo` (`src/Crypto.cs:937`) | `buildLabeledInfo` (`ts-source/crypto.ts:524`) | REVIEWED | HPKE info layout matches for reviewed lengths. |
| `ExtractAndExpand` (`src/Crypto.cs:955`) | `extractAndExpand` (`ts-source/crypto.ts:545`) | REVIEWED | HKDF extract/expand order matches. |
| `AesGcmDecrypt` (`src/Crypto.cs:961`) | `aesGcmDecrypt` (`ts-source/crypto.ts:584`) | REVIEWED | AES-GCM decrypt/tag handling matches. |
| `AesGcmEncrypt` (`src/Crypto.cs:972`) | `aesGcmEncrypt` (`ts-source/crypto.ts:570`) | REVIEWED | AES-GCM ciphertext||tag output matches. |
| `Base64UrlDecode` (`src/Crypto.cs:983`) | inline `toB64` decode (`ts-source/turnkey.ts:461`) | REVIEWED | Padding/base64url transform reviewed. |
| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
| `VerifyEnclaveSignature` (`src/Crypto.cs:1024`) | `verifyEnclaveSignature` (`ts-source/turnkey.ts:234`) | REVIEWED | DER signature verification reviewed; override missing in E. |
| `VerifySignature` (`src/Crypto.cs:1052`) | no current upstream equivalent | REVIEWED | C# legacy-envelope extension; listed in E. |
| `DecodeKey` (`src/Crypto.cs:1077`) | `decodeKey` (`ts-source/turnkey.ts:284`) | REVIEWED | SOLANA/hex decode matches for import; error text differs. |
| `GetStringOrNull` (`src/Crypto.cs:1098`) | JSON property access inline | REVIEWED | Structural parser helper; non-string handling differs only on malformed JSON. |

**C. Intentional Adaptations**

- `Uint8Array` is represented as `byte[]`; this is structural and preserves byte order.
- TS union inputs are represented as C# overloads/DTO classes; this preserves wire values.
- `System.Text.Json` source generation replaces `JSON.stringify` for `HpkeBundlePayload`; property names/order are explicit and unindented.
- BouncyCastle EC point/ECDH/ECDSA/AES-GCM primitives replace noble/WebCrypto primitives; same curves, encodings, digests, and tag layout.
- Manual RFC5869 HKDF with `HMACSHA256` replaces `@noble/hashes/hkdf`; same extract/expand inputs and output lengths.
- Raw JWT `r||s` signatures are converted to DER only because BouncyCastle verifies DER; the verified mathematical signature is the same.
- BigInteger construction uses explicit positive sign and fixed-width padding; this preserves SEC1 big-endian leading-zero behavior.

**D. D17 Enforcement Check**

Crypto.cs check: no banned API present.

- No `Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator`.
- No `Org.BouncyCastle.Crypto.Hpke.*`.
- No high-level KDF or HPKE wrapper.
- BouncyCastle usage is limited to EC key/point/ECDH/ECDSA, AES-GCM, SHA-256 signing mode, BigInteger, and related primitive parameter types. The broad `Org.BouncyCastle.Crypto.Generators` using is used for `ECKeyPairGenerator`, not HKDF.

**E. Logic Divergence Findings**

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Crypto.cs:151` | `ts-source/math.ts:8` | Public negative `x` differs: C# `BigInteger.Mod` normalizes non-negative; JS `%` keeps negative sign. Crypto call sites use non-negative EC coordinates, but the public helper is observably different. | Mirror JS remainder for public parity or make helper private/document non-negative input. |
| `src/Crypto.cs:355` | `ts-source/crypto.ts:358-363` | C# uses `ECKeyPairGenerator`, which samples valid scalars; TS samples 32 random bytes then derives public key. Rare invalid scalar/failure/distribution behavior differs. | If strict parity is required, generate 32 random bytes and derive with the same validation behavior; otherwise document as deliberate hardening. |
| `src/Crypto.cs:435-438` | `ts-source/crypto.ts:95-151` | `HpkeEncrypt` treats null `PlainTextBuf` as empty and does not wrap errors as `Unable to perform hpkeEncrypt`. TS catches and wraps all errors. | Require non-null plaintext and wrap exceptions with the upstream message. |
| `src/Crypto.cs:386-421` | `ts-source/crypto.ts:308-350` | `HpkeDecrypt` throws C# argument/crypto exceptions directly; TS wraps all errors as `Unable to perform hpkeDecrypt: ...`. | Wrap non-fatal exceptions with upstream-equivalent message. |
| `src/Crypto.cs:482-486` | `ts-source/crypto.ts:406-418` | C# rejects non-65-byte or non-`0x04` input; TS slices any input and mutates prefix based on last byte. Valid public keys match, malformed input behavior differs. | Remove extra validation for parity or keep and document stricter validation. |
| `src/Crypto.cs:506` | `ts-source/crypto.ts:426-462` | C# only supports P-256; upstream exposes optional `curve` and supports `CURVE_SECP256K1`. | Add curve parameter/support or document intentional P-256-only subset. |
| `src/Crypto.cs:509-512` | `ts-source/crypto.ts:430-432` | Invalid compressed length error message differs. | Use upstream message if exact error parity matters. |
| `src/Crypto.cs:579-582` | `ts-source/crypto.ts:286-299` | C# rejects buffers `<= 33`; TS slices first 33 bytes and allows downstream behavior. A valid compressed key plus empty ciphertext would stringify upstream but throw in C#. | Match TS slicing behavior, or only reject `< 33` if defensive validation is desired. |
| `src/Crypto.cs:614-619` | `ts-source/turnkey.ts:76-77` | C# falls back from Base58Check to raw Base58; upstream only accepts Base58Check. This can accept bundles TS rejects. | Remove raw Base58 fallback. |
| `src/Crypto.cs:276-283`, `289-296`, `851` | `ts-source/turnkey.ts:31-45`, `116-123`, `444-450` | DTO/API shape omits `dangerouslyOverrideSignerPublicKey` and `dangerouslyOverrideNotarizerPublicKey`. | Add optional override fields/parameters and pass them through to verification helpers. |
| `src/Crypto.cs:665-708` | `ts-source/turnkey.ts:325-360` | `EncryptPrivateKeyToBundle` verifies/parses bundle before `DecodeKey`; upstream decodes private key before signature verification. Error precedence differs when both inputs are bad. | Move `DecodeKey` immediately after JSON parse to match upstream. |
| `src/Crypto.cs:753-767`, `1052` | `ts-source/turnkey.ts:124-162` | C# accepts a legacy `signature`/`signedData` envelope path not present in upstream 2.8.8. | Remove from this port or isolate as a separately named compatibility helper. |
| `src/Crypto.cs:796-803` | `ts-source/turnkey.ts:144-151` | C# allows missing `organizationId`; upstream rejects missing or mismatched `organizationId`. | Require `bundleOrgId` to be non-empty and equal. |
| `src/Crypto.cs:827-830` | `ts-source/turnkey.ts:164-180` | SOLANA export output is wrong: C# Base58-encodes only 32 decrypted private-key bytes; upstream derives Ed25519 public key, concatenates 32 private + 32 public bytes, then Base58-encodes 64 bytes. | Derive Ed25519 public key and encode the 64-byte concatenation. |
| `src/Crypto.cs:853-890` | `ts-source/turnkey.ts:452-477` | Malformed JWT behavior differs: C# returns `false` for wrong part count and catches all errors; upstream throws for missing signature part and returns verifier result otherwise. | Throw on malformed JWT and avoid catch-all if exact upstream behavior is required. |

**F. Fixture Comparison Gate**

Not satisfied for `Crypto.cs`.

`tests/Fixtures/` currently contains only `tests/Fixtures/encoding/turnkey-encoding-vectors.json`; no fixture under `tests/Fixtures/` exercises `src/Crypto.cs`. `tests/Fixtures/README.md` lists `crypto/` as planned, but the directory is absent. `tests/CryptoTests.cs` uses inline RFC/NIST/random roundtrip tests and explicitly states positive Turnkey-signed bundle tests are not present.

So I cannot confirm that every Crypto.cs fixture was generated from the pinned upstream package or that C# asserts the same bytes Node would produce. There are no applicable Crypto fixtures to compare.

**G. Unresolved Assumptions**

- I did not run `dotnet test`; the workspace is read-only and test/build output would require writes.
- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
- Positive Turnkey-signed import/export/credential bundle parity could not be verified because no pinned Node-generated Crypto fixtures are present.
- Pass criterion is not met this round: section B has zero `NOT-REVIEWED` rows and D passes, but E has divergences and F is not satisfied.


