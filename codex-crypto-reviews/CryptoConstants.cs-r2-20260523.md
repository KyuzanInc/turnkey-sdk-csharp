# Codex review — src/CryptoConstants.cs — round 2 — 20260523

C# SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
Upstream snapshot: `turnkey-crypto-2.8.8`

---

2026-05-23T01:26:25.381003Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e5270-cf65-7412-bd0f-78e957c94dfc
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 2 of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA (388a5fc768fd1215928d29f338ef069acd2eb60c).

B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
   listed in a markdown table:
     | C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
   Status: REVIEWED or NOT-REVIEWED. If NOT-REVIEWED, give a reason. Do not skip rows.

C. Intentional adaptations: list every C#/TS adaptation pattern with a one-line
   justification that it is structural (no wire-byte or observable-behavior change).

D. (Crypto.cs only) D17 enforcement check. BouncyCastle allow-list:
     - ECDSA P-256 sign/verify, ECDH P-256, AES-GCM (128-bit tag), SHA-256,
       HMAC-SHA256, BigInteger arithmetic, EC point operations.
     - Ed25519PrivateKeyParameters.GeneratePublicKey (seed -> public key)
       only in the SOLANA export branch of DecryptExportBundle.
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

2026-05-23T01:26:25.815049Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll treat this as a review deliverable rather than a patch. I’m going to pin the upstream snapshot and line-number both sides first, then check fixtures and the requested D17 gate.
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
/bin/zsh -lc 'rg -n "turnkey|C# SDK|CryptoConstants|peak-sdk-csharp|csharp" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
273:# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
275:scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.
278:## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted
282:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
288:- PR #330, plans/plans-peak-sdk-csharp.md, docs(plans): add peak-sdk-csharp migration plan, review the new plan to implement csharp sdk, inline comments, TurnkeyJsonContext, submodule init, csharp-publish.yml
294:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, latest verified head `a1adb9a6ef183151430b7b106e3c6bafce649e5e`)
300:- resolve可能であればresolveし、追加の指摘があれば指摘追加してください, verify latest head, resolve thread, locally packed .nupkg, feature/plan-peak-sdk-csharp, reviewer waiting
304:- when reviewing a plan/PR, the user asked: "review the new plan to implement csharp sdk" -> default to a PR-style review with concrete findings, not abstract advice [Task 1]
312:- The stable final plan shape after the verify-resolve loop was PR1 build/test only, PR5 locally packed `.nupkg`, PR7 independent `csharp-publish.yml`, and PR4.5 splitting secure-storage adapters between core and Unity packages [Task 2]

codex
I’m using the review skill for the review posture, but not its changelog/codex-runner flow because this is a pinned source-parity review with required sections, not a local diff review. The only relevant memory hit is the user’s prior preference for concrete C# SDK review findings; I’ll keep this output source-pinned and line-specific.
exec
/bin/zsh -lc "nl -ba src/CryptoConstants.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc 'git status --short' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc 'rg -n "turnkey-crypto|crypto|sha256|CryptoConstants" codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8 src tests' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt:2:75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
src/CryptoConstants.cs:14://     `crypto.ts uncompressRawPublicKey`:
src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
src/CryptoConstants.cs:32:    public static class CryptoConstants
src/CryptoConstants.cs:42:        /// Equivalent to <c>turnkey/crypto</c> hard-coded
src/CryptoConstants.cs:51:        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
tests/PublicApi.expected.txt:121:    public static class CryptoConstants
src/Http.cs:5://   codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/
tests/ApiKeyStamperTests.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{private,public}
tests/ApiKeyStamperTests.cs:5://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts
tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json:2:  "name": "@turnkey/crypto",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json:28:    "directory": "packages/crypto"
tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
tests/CryptoTests.cs:527:            // From upstream tests/__tests__/crypto-test.ts:179-184
tests/CryptoTests.cs:528:            // (codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts)
tests/CryptoTests.cs:543:            // From upstream tests/__tests__/crypto-test.ts:243-250
src/Crypto.cs:1:// 1:1 logical port of @turnkey/crypto@2.8.8
src/Crypto.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
src/Crypto.cs:9://   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:42://   The only diff between @turnkey/crypto@2.8.8 and @turnkey/crypto@2.8.9 in
src/Crypto.cs:65:    /// of <c>@turnkey/crypto</c> at peak's pinned version 2.8.8.
src/Crypto.cs:201:        /// which upstream <c>crypto.ts</c> imports.
src/Crypto.cs:347:        #region crypto.ts public surface
src/Crypto.cs:351:        /// Equivalent to upstream <c>crypto.ts getPublicKey</c>.
src/Crypto.cs:374:        /// <c>crypto.ts generateP256KeyPair</c>. The private key is sampled as
src/Crypto.cs:399:        /// Equivalent to upstream <c>crypto.ts hpkeDecrypt</c>.
src/Crypto.cs:446:        /// Equivalent to upstream <c>crypto.ts hpkeEncrypt</c>.
src/Crypto.cs:499:        /// Equivalent to upstream <c>crypto.ts buildAdditionalAssociatedData</c>.
src/Crypto.cs:508:        /// Equivalent to upstream <c>crypto.ts compressRawPublicKey</c>.
src/Crypto.cs:542:        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:613:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
src/Crypto.cs:911:            using (var sha256 = SHA256.Create())
src/Crypto.cs:913:                var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
src/Crypto.cs:914:                msgDigest = sha256.ComputeHash(h1);
src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md:1:# @turnkey/crypto
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md:3:This package consolidates some common cryptographic utilities used across our applications, particularly primitives related to keys, encryption, and decryption in a pure JS implementation. For react-native you will need to polyfill our random byte generation by importing [react-native-get-random-values](https://www.npmjs.com/package/react-native-get-random-values)
src/turnkey-sdk-csharp.csproj:12:    <Description>Unofficial / community-maintained Turnkey SDK for .NET by Kyuzan Inc. Not affiliated with Turnkey, Inc. 1:1 logical port of @turnkey/crypto v2.8.8, @turnkey/http v3.16.0, @turnkey/api-key-stamper v0.5.0, and @turnkey/encoding v0.6.0 (peak-pinned versions). Provides P-256 ECDSA / HPKE / HKDF / API key stamping primitives for .NET. BouncyCastle 2.5.0 is used only for primitives; HPKE / HKDF / bundle parsing are direct ports of the upstream Turnkey TypeScript logic.</Description>
src/turnkey-sdk-csharp.csproj:18:    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
tests/Fixtures/README.md:9:  `__tests__/` directory inside `codex-crypto-reviews/upstream-snapshots/`.
tests/Fixtures/README.md:24:├── crypto/                                  — (added with Crypto.cs port)
tests/Fixtures/README.md:32:[`codex-crypto-reviews/upstream-snapshots/`](../../codex-crypto-reviews/upstream-snapshots/)
tests/Fixtures/README.md:34:into the matching fixture file and update its sha256 in
src/Encoding.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
src/ApiKeyStamper.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/
src/ApiKeyStamper.cs:15://   "node" (Node crypto), "purejs" (@noble/curves). This C# port mirrors
src/ApiKeyStamper.cs:48:    /// Node which dispatches to <c>nodecrypto.ts</c> and produces
src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/Fixtures/encoding/turnkey-encoding-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:6:    "tarball_sha256": "2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:5:var crypto = require('./crypto.js');
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:8:var sha256 = require('@noble/hashes/sha256');
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:11:// Turnkey-specific cryptographic utilities
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:37:        const decryptedData = crypto.hpkeDecrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:82:        const decryptedData = crypto.hpkeDecrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:131:    const publicSignatureBuf = crypto.fromDerSignature(signature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:133:    const hashedData = sha256.sha256(signedDataBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:156:    const publicSignatureBuf = crypto.fromDerSignature(publicSignature);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:158:    const hashedData = sha256.sha256(signedDataBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:226:    const privateKeyBundle = crypto.hpkeEncrypt({ plainTextBuf, targetKeyBuf });
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:227:    return crypto.formatHpkeBuf(privateKeyBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:257:    const privateKeyBundle = crypto.hpkeEncrypt({ plainTextBuf, targetKeyBuf });
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:258:    return crypto.formatHpkeBuf(privateKeyBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:268: *        `h1 = sha256(header.payload)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:269: *        `msg = sha256(h1)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:292:    /* 2. sha256(sha256(header.payload)) ----------------------------------- */
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:293:    const h1 = sha256.sha256(new TextEncoder().encode(signingInput));
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:294:    const msgDigest = sha256.sha256(h1); // 32-byte Uint8Array
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:317:    return await crypto.quorumKeyEncrypt(encoding.uint8ArrayFromHexString(targetPublicKeyUncompressed), new TextEncoder().encode(message));
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:348:    return encoding.bs58check.encode(crypto.hpkeEncrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:31:    let cryptoInstance;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:32:    // Use globalThis.crypto.subtle if available
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:33:    if (typeof globalThis !== "undefined" && globalThis.crypto?.subtle) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:34:        cryptoInstance = globalThis.crypto;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:35:        x509__namespace.cryptoProvider.set(cryptoInstance);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:36:        return cryptoInstance;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:45:async function sha256Hex(data) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:46:    const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:47:    const digest = await cryptoInstance.subtle.digest("SHA-256", data);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:54:    const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:55:    return cryptoInstance.subtle.importKey("spki", spki, { name: "ECDSA", namedCurve: "P-384" }, // AWS Nitro uses ES384
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:88:    const manifestDigest = sha2.sha256(decodedBootProofManifest);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:143:    const payloadDigest = sha2.sha256(payloadBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:155:        const rootSha = await sha256Hex(rootDer);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:224:    const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:225:    const ok = await cryptoInstance.subtle.verify({ name: "ECDSA", hash: { name: "SHA-384" } }, pubKey, new Uint8Array(signature), tbs);
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:4:    "source_fixture": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{public,private}",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:5:    "source_test": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:7:    "tarball_sha256": "962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:11:    "note": "P-256 ECDSA signatures are NOT byte-identical across upstream runtimes when the impl uses random-k (nodecrypto.ts, webcrypto.ts). This C# port mirrors upstream's purejs runtime which uses RFC 6979 deterministic-k AND low-S (noble v1.3.0 default), so the C# signature bytes ARE reproducible against an upstream purejs runtime. All three upstream runtimes produce signatures that verify under the same public key + SHA-256(content), so end-to-end Turnkey API calls work regardless of runtime.",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:12:    "byte_level_assertion_strategy": "Exact Node-generated byte snapshots would require running the upstream npm package with its full node_modules; the tarball ships only built JS + declarations. Instead, ApiKeyStamperTests asserts wire-format equivalence by: (1) reproducing the upstream stamp-test.ts shape (decoded JSON key order = ['publicKey', 'scheme', 'signature']; scheme constant; publicKey equality); (2) crypto-verifying the DER ECDSA signature against SHA-256(content) under the upstream fixture public key; (3) asserting RFC 6979 determinism (same content -> same signature); (4) asserting low-S (s in (0, n/2]). This combination is sufficient to guarantee the stamp is accepted by Turnkey's backend, which is the wire-format goal."
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.d.ts:1:export * from "./crypto";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:3:var crypto = require('./crypto.js');
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:9:exports.buildAdditionalAssociatedData = crypto.buildAdditionalAssociatedData;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:10:exports.compressRawPublicKey = crypto.compressRawPublicKey;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:11:exports.extractPrivateKeyFromPKCS8Bytes = crypto.extractPrivateKeyFromPKCS8Bytes;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:12:exports.formatHpkeBuf = crypto.formatHpkeBuf;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:13:exports.fromDerSignature = crypto.fromDerSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:14:exports.generateP256KeyPair = crypto.generateP256KeyPair;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:15:exports.getPublicKey = crypto.getPublicKey;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:16:exports.hpkeAuthEncrypt = crypto.hpkeAuthEncrypt;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:17:exports.hpkeDecrypt = crypto.hpkeDecrypt;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:18:exports.hpkeEncrypt = crypto.hpkeEncrypt;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:19:exports.quorumKeyEncrypt = crypto.quorumKeyEncrypt;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:20:exports.toDerSignature = crypto.toDerSignature;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
tests/Fixtures/crypto/turnkey-crypto-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:5:    "package": "@turnkey/crypto@2.8.8",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:6:    "tarball_sha256": "75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:7:    "note": "Real Turnkey-pinned credential bundle test vector. Verified by Codex r2 against the upstream Jest test (__tests__/crypto-test.ts line 179-184)."
tests/Fixtures/crypto/turnkey-crypto-vectors.json:11:      "_source_line": "ts-source/__tests__/crypto-test.ts:179-184",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:21:      "_source_line": "ts-source/__tests__/crypto-test.ts:243-250",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:26:      "_source_line": "ts-source/__tests__/crypto-test.ts:252-260",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:32:    "_source_line": "ts-source/__tests__/crypto-test.ts:173-178",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts.map:1:{"version":3,"file":"crypto.d.ts","sourceRoot":"","sources":["../src/crypto.ts"],"names":[],"mappings":";AA6BA,UAAU,iBAAiB;IACzB,aAAa,EAAE,UAAU,CAAC;IAC1B,cAAc,EAAE,UAAU,CAAC;IAC3B,YAAY,EAAE,MAAM,CAAC;CACtB;AAED,UAAU,iBAAiB;IACzB,YAAY,EAAE,UAAU,CAAC;IACzB,YAAY,EAAE,UAAU,CAAC;CAC1B;AACD,UAAU,qBAAqB;IAC7B,YAAY,EAAE,UAAU,CAAC;IACzB,YAAY,EAAE,UAAU,CAAC;IACzB,UAAU,EAAE,MAAM,CAAC;CACpB;AAED,UAAU,OAAO;IACf,UAAU,EAAE,MAAM,CAAC;IACnB,SAAS,EAAE,MAAM,CAAC;IAClB,qBAAqB,EAAE,MAAM,CAAC;CAC/B;AAoBD,KAAK,KAAK,GAAG,iBAAiB,GAAG,YAAY,CAAC;AAE9C;;;;;;;GAOG;AACH,eAAO,MAAM,YAAY,eACX,UAAU,GAAG,MAAM,iBACjB,OAAO,KACpB,UAEF,CAAC;AAEF;;;;;;GAMG;AAEH,eAAO,MAAM,WAAW,oCAGrB,iBAAiB,KAAG,UAsDtB,CAAC;AAEF;;;;;;GAMG;AAEH,eAAO,MAAM,eAAe,gDAIzB,qBAAqB,KAAG,UAkD1B,CAAC;AAEF;;;;;;;;;;;;GAYG;AACH,eAAO,MAAM,gBAAgB,gCACE,UAAU,WAC9B,UAAU,KAClB,QAAQ,UAAU,CA0CpB,CAAC;AAEF;;;;;;GAMG;AAEH,eAAO,MAAM,aAAa,iBAAkB,UAAU,KAAG,MAaxD,CAAC;AAEF;;;;;;GAMG;AACH,eAAO,MAAM,WAAW,qDAIrB,iBAAiB,KAAG,UAuCtB,CAAC;AAEF;;;;GAIG;AACH,eAAO,MAAM,mBAAmB,QAAO,OAWtC,CAAC;AAEF;;;;;;GAMG;AACH,eAAO,MAAM,6BAA6B,iBAC1B,UAAU,kBACR,UAAU,KACzB,UAKF,CAAC;AAEF;;;;;GAKG;AACH,eAAO,MAAM,+BAA+B,eAC9B,UAAU,KACrB,UAEF,CAAC;AAEF;;;;;GAKG;AACH,eAAO,MAAM,oBAAoB,iBAAkB,UAAU,KAAG,UAa/D,CAAC;AAEF;;;;GAIG;AACH,eAAO,MAAM,sBAAsB,iBACnB,UAAU,UACjB,KAAK,KACX,UAoDF,CAAC;AAkJF;;;;;GAKG;AACH,eAAO,MAAM,gBAAgB,iBAAkB,MAAM,KAAG,UA8FvD,CAAC;AAEF;;;;;;;;;;;;;;;;;;GAkBG;AACH,eAAO,MAAM,cAAc,iBAAkB,MAAM,WAyClD,CAAC"}
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:6:import { sha256 } from '@noble/hashes/sha256';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:9:// Turnkey-specific cryptographic utilities
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:131:    const hashedData = sha256(signedDataBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:156:    const hashedData = sha256(signedDataBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:266: *        `h1 = sha256(header.payload)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:267: *        `msg = sha256(h1)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:290:    /* 2. sha256(sha256(header.payload)) ----------------------------------- */
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:291:    const h1 = sha256(new TextEncoder().encode(signingInput));
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:292:    const msgDigest = sha256(h1); // 32-byte Uint8Array
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:3:import { sha256 } from '@noble/hashes/sha2';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:9:    let cryptoInstance;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:10:    // Use globalThis.crypto.subtle if available
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:11:    if (typeof globalThis !== "undefined" && globalThis.crypto?.subtle) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:12:        cryptoInstance = globalThis.crypto;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:13:        x509.cryptoProvider.set(cryptoInstance);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:14:        return cryptoInstance;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:23:async function sha256Hex(data) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:24:    const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:25:    const digest = await cryptoInstance.subtle.digest("SHA-256", data);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:32:    const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:33:    return cryptoInstance.subtle.importKey("spki", spki, { name: "ECDSA", namedCurve: "P-384" }, // AWS Nitro uses ES384
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:66:    const manifestDigest = sha256(decodedBootProofManifest);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:121:    const payloadDigest = sha256(payloadBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:133:        const rootSha = await sha256Hex(rootDer);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:202:    const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:203:    const ok = await cryptoInstance.subtle.verify({ name: "ECDSA", hash: { name: "SHA-384" } }, pubKey, new Uint8Array(signature), tbs);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:5:var sha256 = require('@noble/hashes/sha256');
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:159:    crypto.getRandomValues(nonce);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:170:    const encryptedMessageBuf = await crypto.subtle.encrypt(alg, cipher, message);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:378:    const prk = hkdf__namespace.extract(sha256.sha256, ikm, sharedSecret);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:379:    const resp = hkdf__namespace.expand(sha256.sha256, prk, info, len);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:573:    const hmacKey = await crypto.subtle.importKey("raw", preImage, {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:577:    const mac = new Uint8Array(await crypto.subtle.sign("HMAC", hmacKey, constants.QOS_ENCRYPTION_HMAC_MESSAGE));
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:580:    return crypto.subtle.importKey("raw", aesKeyRaw, {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:624://# sourceMappingURL=crypto.js.map
tests/HttpTests.cs:185:            // decodes to a stamp object that crypto-verifies over the body.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;AAAA;AA0DA;AACA,MAAM,cAAc,GAAG;AACrB,IAAA,MAAM,EAAE;AACN,QAAA,KAAK,EAAE,EAAE,KAAK,EAAE,EAAE,IAAI,EAAE,IAAI,EAAE,GAAG,EAAEA,2CAAiC,EAAE,EAAE;AACxE,QAAA,qBAAqB,EAAE;YACrB,KAAK,EAAE,EAAE,IAAI,EAAE,IAAI,EAAE,GAAG,EAAEC,2CAAiC,EAAE;AAC9D,SAAA;QACD,gBAAgB,EAAE,EAAE,KAAK,EAAE,EAAE,IAAI,EAAE,IAAI,EAAE,EAAE;AAC5C,KAAA;CACF;AAID;;;;;;;AAOG;AACI,MAAM,YAAY,GAAG,CAC1B,UAA+B,EAC/B,YAAA,GAAwB,IAAI,KACd;IACd,OAAOC,SAAI,CAAC,YAAY,CAAC,UAAU,EAAE,YAAY,CAAC;AACpD;AAEA;;;;;;AAMG;AAEI,MAAM,WAAW,GAAG,CAAC,EAC1B,YAAY,EACZ,YAAY,GACM,KAAgB;AAClC,IAAA,IAAI;;AAEF,QAAA,MAAM,gBAAgB,GAAG,mBAAmB,EAAE;QAC9C,MAAM,aAAa,GAAGC,gCAAuB,CAAC,gBAAgB,CAAC,UAAU,CAAC;QAC1E,MAAM,YAAY,GAAGA,gCAAuB,CAC1C,gBAAgB,CAAC,qBAAqB,CACvC;QAED,MAAM,GAAG,GAAG,6BAA6B,CAAC,YAAY,EAAE,YAAY,CAAC;;QAGrE,MAAM,EAAE,GAAG,QAAQ,CAAC,YAAY,EAAEC,8BAAqB,CAAC,aAAc,CAAC,CAAC;;QAGxE,MAAM,UAAU,GAAG,aAAa,CAC9B,YAAY,EACZA,8BAAqB,CAAC,YAAY,CAAC,CACpC;;QAGD,IAAI,GAAG,GAAG,eAAe,CAACC,uBAAa,EAAE,EAAE,EAAEC,oBAAU,CAAC;AACxD,QAAA,IAAI,IAAI,GAAG,gBAAgB,CACzBC,6BAAmB,EACnB,UAAU,EACVD,oBAAU,EACV,EAAE,CACH;AACD,QAAA,MAAM,YAAY,GAAG,gBAAgB,CAAC,IAAI,UAAU,CAAC,EAAE,CAAC,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;AAGxE,QAAA,GAAG,GAAG,eAAe,CAACE,sBAAY,EAAE,IAAI,UAAU,CAAC,EAAE,CAAC,EAAEC,oBAAU,CAAC;QACnE,IAAI,GAAGC,sBAAY;AACnB,QAAA,MAAM,GAAG,GAAG,gBAAgB,CAAC,YAAY,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;QAGzD,IAAI,GAAGC,iBAAO;AACd,QAAA,MAAM,EAAE,GAAG,gBAAgB,CAAC,YAAY,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;AAGxD,QAAA,MAAM,aAAa,GAAG,aAAa,CAAC,YAAY,EAAE,GAAG,EAAE,EAAE,EAAE,GAAG,CAAC;;AAG/D,QAAA,MAAM,mBAAmB,GAAG,oBAAoB,CAAC,YAAY,CAAC;AAC9D,QAAA,MAAM,MAAM,GAAG,IAAI,UAAU,CAC3B,mBAAmB,CAAC,MAAM,GAAG,aAAa,CAAC,MAAM,CAClD;AACD,QAAA,MAAM,CAAC,GAAG,CAAC,mBAAmB,EAAE,CAAC,CAAC;QAClC,MAAM,CAAC,GAAG,CAAC,aAAa,EAAE,mBAAmB,CAAC,MAAM,CAAC;AAErD,QAAA,OAAO,MAAM;IACf;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,kCAAkC,KAAK,CAAA,CAAE,CAAC;IAC5D;AACF;AAEA;;;;;;AAMG;AAEI,MAAM,eAAe,GAAG,CAAC,EAC9B,YAAY,EACZ,YAAY,EACZ,UAAU,GACY,KAAgB;AACtC,IAAA,IAAI;;AAEF,QAAA,MAAM,aAAa,GAAGR,gCAAuB,CAAC,UAAU,CAAC;QACzD,MAAM,YAAY,GAAG,YAAY,CAAC,UAAU,EAAE,KAAK,CAAC;QAEpD,MAAM,GAAG,GAAG,6BAA6B,CAAC,YAAY,EAAE,YAAY,CAAC;;QAGrE,MAAM,EAAE,GAAG,QAAQ,CAAC,YAAY,EAAEC,8BAAqB,CAAC,aAAc,CAAC,CAAC;;QAGxE,MAAM,UAAU,GAAG,aAAa,CAC9B,YAAY,EACZA,8BAAqB,CAAC,YAAY,CAAC,CACpC;;QAGD,IAAI,GAAG,GAAG,eAAe,CAACC,uBAAa,EAAE,EAAE,EAAEC,oBAAU,CAAC;AACxD,QAAA,IAAI,IAAI,GAAG,gBAAgB,CACzBC,6BAAmB,EACnB,UAAU,EACVD,oBAAU,EACV,EAAE,CACH;AACD,QAAA,MAAM,YAAY,GAAG,gBAAgB,CAAC,IAAI,UAAU,CAAC,EAAE,CAAC,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;AAGxE,QAAA,GAAG,GAAG,eAAe,CAACE,sBAAY,EAAE,IAAI,UAAU,CAAC,EAAE,CAAC,EAAEC,oBAAU,CAAC;QACnE,IAAI,GAAGC,sBAAY;AACnB,QAAA,MAAM,GAAG,GAAG,gBAAgB,CAAC,YAAY,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;QAGzD,IAAI,GAAGC,iBAAO;AACd,QAAA,MAAM,EAAE,GAAG,gBAAgB,CAAC,YAAY,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;AAGxD,QAAA,MAAM,aAAa,GAAG,aAAa,CAAC,YAAY,EAAE,GAAG,EAAE,EAAE,EAAE,GAAG,CAAC;;AAG/D,QAAA,MAAM,mBAAmB,GAAG,oBAAoB,CAAC,YAAY,CAAC;AAC9D,QAAA,MAAM,MAAM,GAAG,IAAI,UAAU,CAC3B,mBAAmB,CAAC,MAAM,GAAG,aAAa,CAAC,MAAM,CAClD;AACD,QAAA,MAAM,CAAC,GAAG,CAAC,mBAAmB,EAAE,CAAC,CAAC;QAClC,MAAM,CAAC,GAAG,CAAC,aAAa,EAAE,mBAAmB,CAAC,MAAM,CAAC;AACrD,QAAA,OAAO,MAAM;IACf;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,kCAAkC,KAAK,CAAA,CAAE,CAAC;IAC5D;AACF;AAEA;;;;;;;;;;;;AAYG;AACI,MAAM,gBAAgB,GAAG,OAC9B,2BAAuC,EACvC,OAAmB,KACI;;AAEvB,IAAA,MAAM,gBAAgB,GAAG,mBAAmB,EAAE;AAC9C,IAAA,MAAM,qBAAqB,GAAG,gBAAgB,CAAC,qBAAqB;;AAGpE,IAAA,IAAI,MAAM,GAAG,MAAM,4BAA4B,CAC7CR,gCAAuB,CAAC,qBAAqB,CAAC,EAC9CA,gCAAuB,CAAC,gBAAgB,CAAC,UAAU,CAAC,EACpD,2BAA2B,CAC5B;;AAGD,IAAA,MAAM,KAAK,GAAG,IAAI,UAAU,CAACH,2CAAiC,CAAC;AAC/D,IAAA,MAAM,CAAC,eAAe,CAAC,KAAK,CAAC;;IAG7B,MAAM,GAAG,GAAG,8BAA8B,CACxCG,gCAAuB,CAAC,qBAAqB,CAAC,EAC9C,2BAA2B,CAC5B;;AAGD,IAAA,MAAM,GAAG,GAAiB;AACxB,QAAA,IAAI,EAAE,SAAS;AACf,QAAA,EAAE,EAAE,KAAK;AACT,QAAA,SAAS,EAAE,GAAG;AACd,QAAA,cAAc,EAAE,GAAG;KACpB;;AAGD,IAAA,MAAM,mBAAmB,GAAG,MAAM,MAAM,CAAC,MAAM,CAAC,OAAO,CAAC,GAAG,EAAE,MAAM,EAAE,OAAO,CAAC;;AAG7E,IAAA,IAAI,QAAQ,GAAa;AACvB,QAAA,KAAK,EAAE,KAAK;AACZ,QAAA,qBAAqB,EAAEA,gCAAuB,CAAC,qBAAqB,CAAC;AACrE,QAAA,gBAAgB,EAAE,IAAI,UAAU,CAAC,mBAAmB,CAAC;KACtD;;IAGD,OAAOS,gBAAK,CAAC,SAAS,CAAC,cAAc,EAAE,QAAQ,CAAC;AAClD;AAEA;;;;;;AAMG;AAEI,MAAM,aAAa,GAAG,CAAC,YAAwB,KAAY;IAChE,MAAM,mBAAmB,GAAG,YAAY,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;IACrD,MAAM,aAAa,GAAG,YAAY,CAAC,KAAK,CAAC,EAAE,CAAC;IAE5C,MAAM,iBAAiB,GAAGR,8BAAqB,CAC7C,sBAAsB,CAAC,mBAAmB,CAAC,CAC5C;AACD,IAAA,MAAM,aAAa,GAAGA,8BAAqB,CAAC,aAAa,CAAC;IAE1D,OAAO,IAAI,CAAC,SAAS,CAAC;AACpB,QAAA,cAAc,EAAE,iBAAiB;AACjC,QAAA,UAAU,EAAE,aAAa;AAC1B,KAAA,CAAC;AACJ;AAEA;;;;;;AAMG;AACI,MAAM,WAAW,GAAG,CAAC,EAC1B,aAAa,EACb,cAAc,EACd,YAAY,GACM,KAAgB;AAClC,IAAA,IAAI;AACF,QAAA,IAAI,GAAe;AACnB,QAAA,IAAI,IAAgB;QACpB,MAAM,cAAc,GAAG,YAAY,CACjCD,gCAAuB,CAAC,YAAY,CAAC,EACrC,KAAK,CACN;QACD,MAAM,GAAG,GAAG,6BAA6B,CAAC,cAAc,EAAE,cAAc,CAAC,CAAC;;QAG1E,MAAM,EAAE,GAAG,QAAQ,CAAC,cAAc,EAAE,YAAY,CAAC;;QAGjD,MAAM,UAAU,GAAG,aAAa,CAC9B,cAAc,EACdC,8BAAqB,CAAC,cAAc,CAAC,CACtC;;QAGD,GAAG,GAAG,eAAe,CAACC,uBAAa,EAAE,EAAE,EAAEC,oBAAU,CAAC;QACpD,IAAI,GAAG,gBAAgB,CAACC,6BAAmB,EAAE,UAAU,EAAED,oBAAU,EAAE,EAAE,CAAC;AACxE,QAAA,MAAM,YAAY,GAAG,gBAAgB,CAAC,IAAI,UAAU,CAAC,EAAE,CAAC,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;AAGxE,QAAA,GAAG,GAAG,eAAe,CAACE,sBAAY,EAAE,IAAI,UAAU,CAAC,EAAE,CAAC,EAAEC,oBAAU,CAAC;QACnE,IAAI,GAAGC,sBAAY;AACnB,QAAA,MAAM,GAAG,GAAG,gBAAgB,CAAC,YAAY,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;QAGzD,IAAI,GAAGC,iBAAO;AACd,QAAA,MAAM,EAAE,GAAG,gBAAgB,CAAC,YAAY,EAAE,GAAG,EAAE,IAAI,EAAE,EAAE,CAAC;;AAGxD,QAAA,MAAM,aAAa,GAAG,aAAa,CAAC,aAAa,EAAE,GAAG,EAAE,EAAE,EAAE,GAAG,CAAC;AAChE,QAAA,OAAO,aAAa;IACtB;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,kCAAkC,KAAK,CAAA,CAAA,CAAG,CAAC;IAC7D;AACF;AAEA;;;;AAIG;AACI,MAAM,mBAAmB,GAAG,MAAc;AAC/C,IAAA,MAAM,UAAU,GAAGE,iBAAW,CAAC,EAAE,CAAC;IAClC,MAAM,SAAS,GAAG,YAAY,CAAC,UAAU,EAAE,IAAI,CAAC;IAChD,MAAM,qBAAqB,GAAGT,8BAAqB,CACjD,sBAAsB,CAAC,SAAS,CAAC,CAClC;IACD,OAAO;AACL,QAAA,UAAU,EAAEA,8BAAqB,CAAC,UAAU,CAAC;AAC7C,QAAA,SAAS,EAAEA,8BAAqB,CAAC,SAAS,CAAC;QAC3C,qBAAqB;KACtB;AACH;AAEA;;;;;;AAMG;MACU,6BAA6B,GAAG,CAC3C,YAAwB,EACxB,cAA0B,KACZ;IACd,OAAO,IAAI,UAAU,CAAC;AACpB,QAAA,GAAG,KAAK,CAAC,IAAI,CAAC,YAAY,CAAC;AAC3B,QAAA,GAAG,KAAK,CAAC,IAAI,CAAC,cAAc,CAAC;AAC9B,KAAA,CAAC;AACJ;AAEA;;;;;AAKG;AACI,MAAM,+BAA+B,GAAG,CAC7C,UAAsB,KACR;IACd,OAAO,UAAU,CAAC,KAAK,CAAC,EAAE,EAAE,EAAE,GAAG,EAAE,CAAC;AACtC;AAEA;;;;;AAKG;AACI,MAAM,oBAAoB,GAAG,CAAC,YAAwB,KAAgB;AAC3E,IAAA,MAAM,GAAG,GAAG,YAAY,CAAC,UAAU;;;;AAKnC,IAAA,IAAI,eAAe,GAAG,YAAY,CAAC,KAAK,CAAC,CAAC,EAAE,CAAC,CAAC,GAAG,GAAG,MAAM,CAAC,CAAC;;;;AAK5D,IAAA,eAAe,CAAC,CAAC,CAAC,GAAG,IAAI,IAAI,YAAY,CAAC,GAAG,GAAG,CAAC,CAAE,GAAG,IAAI,CAAC;AAC3D,IAAA,OAAO,eAAe;AACxB;AAEA;;;;AAIG;AACI,MAAM,sBAAsB,GAAG,CACpC,YAAwB,EACxB,KAAA,GAAe,YAAY,KACb;AACd,IAAA,IAAI,YAAY,CAAC,MAAM,KAAK,EAAE,EAAE;AAC9B,QAAA,MAAM,IAAI,KAAK,CAAC,qDAAqD,CAAC;IACxE;AAEA,IAAA,IAAI,EAAE,YAAY,CAAC,CAAC,CAAC,KAAK,CAAC,IAAI,YAAY,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,EAAE;AACrD,QAAA,MAAM,IAAI,KAAK,CAAC,qDAAqD,CAAC;IACxE;;;IAIA,MAAM,GAAG,GAAG,YAAY,CAAC,CAAC,CAAC,KAAK,CAAC;AACjC,IAAA,MAAM,CAAC,GAAG,MAAM,CAAC,IAAI,GAAGA,8BAAqB,CAAC,YAAY,CAAC,QAAQ,CAAC,CAAC,CAAC,CAAC,CAAC;AAExE,IAAA,IAAI,CAAS,EAAE,CAAS,EAAE,CAAS;AACnC,IAAA,IAAI,KAAK,KAAK,YAAY,EAAE;;;AAG1B,QAAA,CAAC,GAAG,MAAM,CACR,gFAAgF,CACjF;AACD,QAAA,CAAC,GAAG,MAAM,CACR,oEAAoE,CACrE;AACD,QAAA,CAAC,GAAG,CAAC,GAAG,MAAM,CAAC,CAAC,CAAC;IACnB;SAAO;;;AAGL,QAAA,CAAC,GAAG,MAAM,CACR,oEAAoE,CACrE;AACD,QAAA,CAAC,GAAG,MAAM,CAAC,CAAC,CAAC;AACb,QAAA,CAAC,GAAG,MAAM,CAAC,CAAC,CAAC;IACf;;AAGA,IAAA,MAAM,GAAG,GAAG,CAAC,CAAC,CAAC,GAAG,CAAC,GAAG,CAAC,IAAI,CAAC,GAAG,CAAC,IAAI,CAAC;IACrC,IAAI,CAAC,GAAGU,YAAO,CAAC,GAAG,EAAE,CAAC,CAAC;IACvB,IAAI,GAAG,KAAKC,YAAO,CAAC,CAAC,EAAE,CAAC,CAAC,EAAE;QACzB,CAAC,GAAG,CAAC,CAAC,GAAG,CAAC,IAAI,CAAC;IACjB;IAEA,IAAI,CAAC,GAAG,MAAM,CAAC,CAAC,CAAC,IAAI,CAAC,IAAI,CAAC,EAAE;AAC3B,QAAA,MAAM,IAAI,KAAK,CAAC,mBAAmB,CAAC;IACtC;IAEA,IAAI,CAAC,GAAG,MAAM,CAAC,CAAC,CAAC,IAAI,CAAC,IAAI,CAAC,EAAE;AAC3B,QAAA,MAAM,IAAI,KAAK,CAAC,mBAAmB,CAAC;IACtC;AAEA,IAAA,IAAI,qBAAqB,GAAG,IAAI,GAAG,WAAW,CAAC,CAAC,EAAE,EAAE,CAAC,GAAG,WAAW,CAAC,CAAC,EAAE,EAAE,CAAC;AAC1E,IAAA,OAAOZ,gCAAuB,CAAC,qBAAqB,CAAC;AACvD;AAEA;;;;;;;AAOG;AACH,MAAM,eAAe,GAAG,CACtB,KAAiB,EACjB,GAAe,EACf,OAAmB,KACL;AACd,IAAA,MAAM,cAAc,GAClBa,sBAAY,CAAC,MAAM,GAAG,OAAO,CAAC,MAAM,GAAG,KAAK,CAAC,MAAM,GAAG,GAAG,CAAC,MAAM;AAClE,IAAA,MAAM,GAAG,GAAG,IAAI,UAAU,CAAC,cAAc,CAAC;IAC1C,IAAI,MAAM,GAAG,CAAC;AAEd,IAAA,GAAG,CAAC,GAAG,CAACA,sBAAY,EAAE,MAAM,CAAC;AAC7B,IAAA,MAAM,IAAIA,sBAAY,CAAC,MAAM;AAE7B,IAAA,GAAG,CAAC,GAAG,CAAC,OAAO,EAAE,MAAM,CAAC;AACxB,IAAA,MAAM,IAAI,OAAO,CAAC,MAAM;AAExB,IAAA,GAAG,CAAC,GAAG,CAAC,KAAK,EAAE,MAAM,CAAC;AACtB,IAAA,MAAM,IAAI,KAAK,CAAC,MAAM;AAEtB,IAAA,GAAG,CAAC,GAAG,CAAC,GAAG,EAAE,MAAM,CAAC;AAEpB,IAAA,OAAO,GAAG;AACZ,CAAC;AAED;;;;;;;;AAQG;AACH,MAAM,gBAAgB,GAAG,CACvB,KAAiB,EACjB,IAAgB,EAChB,OAAmB,EACnB,GAAW,KACG;AACd,IAAA,MAAM,iBAAiB,GAAG,CAAC,CAAC;AAC5B,IAAA,MAAM,GAAG,GAAG,IAAI,UAAU,CACxB,iBAAiB,GAAG,OAAO,CAAC,UAAU,GAAG,KAAK,CAAC,UAAU,GAAG,IAAI,CAAC,UAAU,CAC5E;AACD,IAAA,GAAG,CAAC,GAAG,CAAC,IAAI,UAAU,CAAC,CAAC,CAAC,EAAE,GAAG,CAAC,CAAC,EAAE,CAAC,CAAC,CAAC;AACrC,IAAA,GAAG,CAAC,GAAG,CAACA,sBAAY,EAAE,CAAC,CAAC;AACxB,IAAA,GAAG,CAAC,GAAG,CAAC,OAAO,EAAE,iBAAiB,CAAC;IACnC,GAAG,CAAC,GAAG,CAAC,KAAK,EAAE,iBAAiB,GAAG,OAAO,CAAC,UAAU,CAAC;AACtD,IAAA,GAAG,CAAC,GAAG,CAAC,IAAI,EAAE,iBAAiB,GAAG,OAAO,CAAC,UAAU,GAAG,KAAK,CAAC,UAAU,CAAC;AACxE,IAAA,OAAO,GAAG;AACZ,CAAC;AAED;;AAEG;AACH,MAAM,gBAAgB,GAAG,CACvB,YAAwB,EACxB,GAAe,EACf,IAAgB,EAChB,GAAW,KACG;AACd,IAAA,MAAM,GAAG,GAAGC,eAAI,CAAC,OAAO,CAACC,aAAM,EAAE,GAAG,EAAE,YAAY,CAAC;AACnD,IAAA,MAAM,IAAI,GAAGD,eAAI,CAAC,MAAM,CAACC,aAAM,EAAE,GAAG,EAAE,IAAI,EAAE,GAAG,CAAC;AAChD,IAAA,OAAO,IAAI,UAAU,CAAC,IAAI,CAAC;AAC7B,CAAC;AAED;;AAEG;AACH,MAAM,QAAQ,GAAG,CAAC,cAA0B,EAAE,IAAY,KAAgB;AACxE,IAAA,MAAM,EAAE,GAAGhB,SAAI,CAAC,eAAe,CAC7BC,gCAAuB,CAAC,IAAI,CAAC,EAC7B,cAAc,CACf;AACD,IAAA,OAAO,EAAE,CAAC,KAAK,CAAC,CAAC,CAAC;AACpB,CAAC;AAED;;AAEG;AACH,MAAM,aAAa,GAAG,CACpB,aAAyB,EACzB,GAAe,EACf,EAAc,EACd,GAAgB,KACF;IACd,MAAMgB,KAAG,GAAGC,OAAG,CAAC,GAAG,EAAE,EAAE,EAAE,GAAG,CAAC;IAC7B,MAAM,IAAI,GAAGD,KAAG,CAAC,OAAO,CAAC,aAAa,CAAC;AACvC,IAAA,OAAO,IAAI;AACb,CAAC;AAED;;AAEG;AACH,MAAM,aAAa,GAAG,CACpB,aAAyB,EACzB,GAAe,EACf,EAAc,EACd,GAAgB,KACF;IACd,MAAMA,KAAG,GAAGC,OAAG,CAAC,GAAG,EAAE,EAAE,EAAE,GAAG,CAAC;IAC7B,MAAM,IAAI,GAAGD,KAAG,CAAC,OAAO,CAAC,aAAa,CAAC;AACvC,IAAA,OAAO,IAAI;AACb,CAAC;AAED;;AAEG;AACH,MAAM,aAAa,GAAG,CACpB,cAA0B,EAC1B,SAAiB,KACH;AACd,IAAA,MAAM,gBAAgB,GAAG,IAAI,UAAU,CAAC,cAAc,CAAC;AACvD,IAAA,MAAM,cAAc,GAAGhB,gCAAuB,CAAC,SAAS,CAAC;AAEzD,IAAA,MAAM,UAAU,GAAG,IAAI,UAAU,CAC/B,gBAAgB,CAAC,MAAM,GAAG,cAAc,CAAC,MAAM,CAChD;AACD,IAAA,UAAU,CAAC,GAAG,CAAC,gBAAgB,CAAC;IAChC,UAAU,CAAC,GAAG,CAAC,cAAc,EAAE,gBAAgB,CAAC,MAAM,CAAC;AAEvD,IAAA,OAAO,UAAU;AACnB,CAAC;AAED;;AAEG;AACH,MAAM,WAAW,GAAG,CAAC,GAAW,EAAE,MAAc,KAAY;IAC1D,MAAM,SAAS,GAAG,GAAG,CAAC,QAAQ,CAAC,EAAE,CAAC;AAClC,IAAA,IAAI,SAAS,CAAC,MAAM,GAAG,MAAM,EAAE;AAC7B,QAAA,MAAM,IAAI,KAAK,CACb,wCAAwC,MAAM,CAAA,WAAA,CAAa,CAC5D;IACH;IACA,OAAO,SAAS,CAAC,QAAQ,CAAC,MAAM,EAAE,GAAG,CAAC;AACxC,CAAC;AAED;;;;;AAKG;AACI,MAAM,gBAAgB,GAAG,CAAC,YAAoB,KAAgB;AACnE,IAAA,MAAM,eAAe,GAAGA,gCAAuB,CAAC,YAAY,CAAC;;AAG7D,IAAA,IAAI,eAAe,CAAC,MAAM,GAAG,CAAC,EAAE;AAC9B,QAAA,MAAM,IAAI,KAAK,CACb,8DAA8D,CAC/D;IACH;;AAGA,IAAA,IAAI,eAAe,CAAC,CAAC,CAAC,KAAK,IAAI,EAAE;AAC/B,QAAA,MAAM,IAAI,KAAK,CACb,gFAAgF,CACjF;IACH;;IAGA,IAAI,KAAK,GAAG,CAAC;AACb,IAAA,MAAM,UAAU,GAAG,eAAe,CAAC,KAAK,CAAE;AAC1C,IAAA,IAAI,UAAU,IAAI,IAAI,EAAE;;;;QAKtB,IAAI,eAAe,CAAC,MAAM,GAAG,CAAC,GAAG,CAAC,GAAG,UAAU,EAAE;AAC/C,YAAA,MAAM,IAAI,KAAK,CACb,6EAA6E,CAC9E;QACH;;QAGA,KAAK,IAAI,CAAC;IACZ;SAAO;;;;;;;;;AASL,QAAA,MAAM,IAAI,KAAK,CACb,yFAAyF,CAC1F;IACH;;AAGA,IAAA,IAAI,eAAe,CAAC,KAAK,CAAC,KAAK,IAAI,EAAE;AACnC,QAAA,MAAM,IAAI,KAAK,CACb,4DAA4D,CAC7D;IACH;IAEA,KAAK,EAAE,CAAC;AACR,IAAA,MAAM,OAAO,GAAG,eAAe,CAAC,KAAK,CAAE;;AAGvC,IAAA,IAAI,OAAO,GAAG,EAAE,EAAE;AAChB,QAAA,MAAM,IAAI,KAAK,CACb,kEAAkE,CACnE;IACH;IAEA,KAAK,EAAE,CAAC;AACR,IAAA,MAAM,CAAC,GAAG,eAAe,CAAC,KAAK,CAAC,KAAK,EAAE,KAAK,GAAG,OAAO,CAAC;AACvD,IAAA,KAAK,IAAI,OAAO,CAAC;;AAGjB,IAAA,IAAI,eAAe,CAAC,KAAK,CAAC,KAAK,IAAI,EAAE;AACnC,QAAA,MAAM,IAAI,KAAK,CACb,4DAA4D,CAC7D;IACH;IAEA,KAAK,EAAE,CAAC;AACR,IAAA,MAAM,OAAO,GAAG,eAAe,CAAC,KAAK,CAAE;;AAGvC,IAAA,IAAI,OAAO,GAAG,EAAE,EAAE;AAChB,QAAA,MAAM,IAAI,KAAK,CACb,kEAAkE,CACnE;IACH;IAEA,KAAK,EAAE,CAAC;AACR,IAAA,MAAM,CAAC,GAAG,eAAe,CAAC,KAAK,CAAC,KAAK,EAAE,KAAK,GAAG,OAAO,CAAC;;IAGvD,MAAM,OAAO,GAAGkB,yBAAgB,CAAC,CAAC,EAAE,EAAE,CAAC;IACvC,MAAM,OAAO,GAAGA,yBAAgB,CAAC,CAAC,EAAE,EAAE,CAAC;;IAGvC,OAAO,IAAI,UAAU,CAAC,CAAC,GAAG,OAAO,EAAE,GAAG,OAAO,CAAC,CAAC;AACjD;AAEA;;;;;;;;;;;;;;;;;;AAkBG;AACI,MAAM,cAAc,GAAG,CAAC,YAAoB,KAAI;AACrD,IAAA,MAAM,eAAe,GAAGlB,gCAAuB,CAAC,YAAY,CAAC;;IAG7D,MAAM,CAAC,GAAG,eAAe,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;IACtC,MAAM,CAAC,GAAG,eAAe,CAAC,KAAK,CAAC,EAAE,EAAE,EAAE,CAAC;;AAGvC,IAAA,MAAM,gBAAgB,GAAG,CAAC,OAAoB,KAAgB;;QAE5D,IACE,OAAO,KAAK,SAAS;YACrB,OAAO,CAAC,MAAM,KAAK,CAAC;AACpB,YAAA,OAAO,CAAC,CAAC,CAAC,KAAK,SAAS,EACxB;AACA,YAAA,MAAM,IAAI,KAAK,CAAC,+CAA+C,CAAC;QAClE;;QAGA,MAAM,YAAY,GAAG,OAAO,CAAC,CAAC,CAAC,GAAG,IAAI;QACtC,MAAM,aAAa,GAAG;cAClB,IAAI,UAAU,CAAC,CAAC,IAAI,EAAE,GAAG,OAAO,CAAC;cACjC,OAAO;;AAGX,QAAA,OAAO,IAAI,UAAU,CAAC,CAAC,IAAI,EAAE,aAAa,CAAC,MAAM,EAAE,GAAG,aAAa,CAAC,CAAC;AACvE,IAAA,CAAC;;AAGD,IAAA,MAAM,QAAQ,GAAG,gBAAgB,CAAC,CAAC,CAAC;AACpC,IAAA,MAAM,QAAQ,GAAG,gBAAgB,CAAC,CAAC,CAAC;;AAGpC,IAAA,MAAM,YAAY,GAAG,IAAI,UAAU,CAAC;QAClC,IAAI;AACJ,QAAA,QAAQ,CAAC,MAAM,GAAG,QAAQ,CAAC,MAAM;AACjC,QAAA,GAAG,QAAQ;AACX,QAAA,GAAG,QAAQ;AACZ,KAAA,CAAC;AAEF,IAAA,OAAOC,8BAAqB,CAAC,YAAY,CAAC;AAC5C;AAEA;;;;;;;;;;;AAWG;AACH,eAAe,4BAA4B,CACzC,qBAAiC,EACjC,sBAAkC,EAClC,2BAAuC,EAAA;;AAGvC,IAAA,MAAM,wBAAwB,GAAGF,SAAI,CAAC,eAAe,CACnD,sBAAsB,EACtB,2BAA2B,EAC3B,KAAK,CACN;IACD,MAAM,YAAY,GAAG,wBAAwB,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;;AAG1D,IAAA,IAAI,QAAQ,GAAG,IAAI,UAAU,CAC3B,qBAAqB,CAAC,MAAM;AAC1B,QAAA,2BAA2B,CAAC,MAAM;QAClC,YAAY,CAAC,MAAM,CACtB;AACD,IAAA,QAAQ,CAAC,GAAG,CAAC,qBAAqB,EAAE,CAAC,CAAC;IACtC,QAAQ,CAAC,GAAG,CAAC,2BAA2B,EAAE,qBAAqB,CAAC,MAAM,CAAC;AACvE,IAAA,QAAQ,CAAC,GAAG,CACV,YAAY,EACZ,qBAAqB,CAAC,MAAM,GAAG,2BAA2B,CAAC,MAAM,CAClE;;AAGD,IAAA,MAAM,OAAO,GAAG,MAAM,MAAM,CAAC,MAAM,CAAC,SAAS,CAC3C,KAAK,EACL,QAAQ,EACR;AACE,QAAA,IAAI,EAAE,MAAM;AACZ,QAAA,IAAI,EAAE,SAAS;AAChB,KAAA,EACD,KAAK,EACL,CAAC,MAAM,CAAC,CACT;AACD,IAAA,MAAM,GAAG,GAAG,IAAI,UAAU,CACxB,MAAM,MAAM,CAAC,MAAM,CAAC,IAAI,CAAC,MAAM,EAAE,OAAO,EAAEoB,qCAA2B,CAAC,CACvE;;IAGD,MAAM,SAAS,GAAG,GAAG,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;IAElC,OAAO,MAAM,CAAC,MAAM,CAAC,SAAS,CAC5B,KAAK,EACL,SAAS,EACT;AACE,QAAA,IAAI,EAAE,SAAS;AAChB,KAAA,EACD,KAAK,EACL,CAAC,SAAS,CAAC,CACZ;AACH;AAEA;AACA;AACA;AACA;AACA;AACA,SAAS,8BAA8B,CACrC,qBAAiC,EACjC,cAA0B,EAAA;;AAG1B,IAAA,MAAM,qBAAqB,GAAG,qBAAqB,CAAC,MAAM;AAC1D,IAAA,MAAM,oBAAoB,GAAG,cAAc,CAAC,MAAM;;AAGlD,IAAA,IAAI,qBAAqB,GAAG,GAAG,IAAI,oBAAoB,GAAG,GAAG;AAC3D,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;;AAG9C,IAAA,MAAM,GAAG,GAAG,IAAI,UAAU,CACxB,qBAAqB,GAAG,CAAC,GAAG,oBAAoB,GAAG,CAAC,CACrD;;IAGD,IAAI,MAAM,GAAG,CAAC;;AAGd,IAAA,GAAG,CAAC,GAAG,CAAC,qBAAqB,EAAE,MAAM,CAAC;IACtC,MAAM,IAAI,qBAAqB;AAC/B,IAAA,GAAG,CAAC,MAAM,EAAE,CAAC,GAAG,qBAAqB;;AAGrC,IAAA,GAAG,CAAC,GAAG,CAAC,cAAc,EAAE,MAAM,CAAC;IAC/B,MAAM,IAAI,oBAAoB;AAC9B,IAAA,GAAG,CAAC,MAAM,EAAE,CAAC,GAAG,oBAAoB;AAEpC,IAAA,OAAO,GAAG;AACZ;;;;;;;;;;;;;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:139://# sourceMappingURL=crypto.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:102: *        `h1 = sha256(header.payload)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:103: *        `msg = sha256(h1)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:1:# @turnkey/crypto
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:37:- [#1050](https://github.com/tkhq/sdk/pull/1050) [`c745646`](https://github.com/tkhq/sdk/commit/c745646ae4b2a275e116abca07c6e108f89beb04) Author [@amircheikh](https://github.com/amircheikh) - - Removed `@peculiar/webcrypto` dependancy. This will fix build errors in environments where `webcrypto` is not defined but will still require a polyfill if you use a function where `webcrypto` is required.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:153:- [#812](https://github.com/tkhq/sdk/pull/812) [`6cde41c`](https://github.com/tkhq/sdk/commit/6cde41cfecdfb7d54abf52cc65e28ef0e2ad6ba3) Author [@turnekybc](https://github.com/turnekybc) - Add `@turnkey/encoding` as a package dependency instead of a devDependency to `@turnkey/crypto`. This resolves an issue with transitive dependencies when devDependencies are not included in the artifact.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:185:- 2bc0046: Migrated from WebCrypto (crypto.subtle.verify) to Noble for ECDSA signature verification
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:204:  - `crypto.ts`: core cryptography utilities
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:205:  - `turnkey.ts`: Turnkey-specific cryptography utilities
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:230:- 93666ff: turnkey/crypto standard HPKE encryption, first major release. Allows for programmatic importing in environments like node. Moved some encoding helper functions to turnkey/encoding
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs.map:1:{"version":3,"file":"crypto.mjs","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":[],"mappings":"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"}
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:2:// Turnkey-specific cryptographic utilities
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:24:} from "./crypto";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:29:import { sha256 } from "@noble/hashes/sha256";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:220:  const hashedData = sha256(signedDataBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:261:  const hashedData = sha256(signedDataBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:428: *        `h1 = sha256(header.payload)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:429: *        `msg = sha256(h1)`
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:457:  /* 2. sha256(sha256(header.payload)) ----------------------------------- */
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:458:  const h1 = sha256(new TextEncoder().encode(signingInput));
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:459:  const msgDigest = sha256(h1); // 32-byte Uint8Array
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:3:import { sha256 } from '@noble/hashes/sha256';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:137:    crypto.getRandomValues(nonce);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:148:    const encryptedMessageBuf = await crypto.subtle.encrypt(alg, cipher, message);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:356:    const prk = hkdf.extract(sha256, ikm, sharedSecret);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:357:    const resp = hkdf.expand(sha256, prk, info, len);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:551:    const hmacKey = await crypto.subtle.importKey("raw", preImage, {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:555:    const mac = new Uint8Array(await crypto.subtle.sign("HMAC", hmacKey, QOS_ENCRYPTION_HMAC_MESSAGE));
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:558:    return crypto.subtle.importKey("raw", aesKeyRaw, {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:590://# sourceMappingURL=crypto.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/index.ts:1:export * from "./crypto";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js.map:1:{"version":3,"file":"proof.js","sources":["../src/proof.ts"],"sourcesContent":[null],"names":["x509","uint8ArrayToHexString","CBOR","AWS_ROOT_CERT_PEM","sha256","uint8ArrayFromHexString","p256","AWS_ROOT_CERT_SHA256"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;;;;;;;AAWO,MAAM,iBAAiB,GAAG,YAAW;AAC1C,IAAA,IAAI,cAAsB;;IAE1B,IAAI,OAAO,UAAU,KAAK,WAAW,IAAI,UAAU,CAAC,MAAM,EAAE,MAAM,EAAE;AAClE,QAAA,cAAc,GAAG,UAAU,CAAC,MAAgB;AAC5C,QAAAA,eAAI,CAAC,cAAc,CAAC,GAAG,CAAC,cAAc,CAAC;AAEvC,QAAA,OAAO,cAAc;IACvB;SAAO;AACL,QAAA,MAAM,IAAI,KAAK,CACb,mFAAmF,CACpF;IACH;AACF;AAEA;;AAEG;AACH,eAAe,SAAS,CAAC,IAAgB,EAAA;AACvC,IAAA,MAAM,cAAc,GAAG,MAAM,iBAAiB,EAAE;AAChD,IAAA,MAAM,MAAM,GAAG,MAAM,cAAc,CAAC,MAAM,CAAC,MAAM,CAAC,SAAS,EAAE,IAAI,CAAC;IAClE,OAAOC,8BAAqB,CAAC,IAAI,UAAU,CAAC,MAAM,CAAC,CAAC,CAAC,WAAW,EAAE;AACpE;AAEA;;AAEG;AACH,eAAe,oBAAoB,CAAC,IAAiB,EAAA;AACnD,IAAA,MAAM,cAAc,GAAG,MAAM,iBAAiB,EAAE;IAChD,OAAO,cAAc,CAAC,MAAM,CAAC,SAAS,CACpC,MAAM,EACN,IAAI,EACJ,EAAE,IAAI,EAAE,OAAO,EAAE,UAAU,EAAE,OAAO,EAAE;AACtC,IAAA,KAAK,EACL,CAAC,QAAQ,CAAC,CACX;AACH;AAEA;;;;;;;;;AASG;AACI,eAAe,MAAM,CAC1B,QAAoB,EACpB,SAAsB,EAAA;;IAGtB,uBAAuB,CAAC,QAAQ,CAAC;;;IAIjC,MAAM,YAAY,GAAG,UAAU,CAAC,IAAI,CAClC,IAAI,CAAC,SAAS,CAAC,oBAAoB;SAChC,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B;IACD,MAAM,SAAS,GAAGC,eAAI,CAAC,MAAM,CAAC,YAAY,CAAC,MAAM,CAAC;IAClD,MAAM,KAAK,OAAO,CAAC,GAAG,SAAS;AAC/B,IAAA,MAAM,cAAc,GAAGA,eAAI,CAAC,MAAM,CAAC,IAAI,UAAU,CAAC,OAAO,CAAC,CAAC,MAAM,CAAC;;IAGlE,MAAM,kBAAkB,CAAC,SAAS,EAAE,cAAc,CAAC,WAAW,CAAC;;AAG/D,IAAA,MAAM,mBAAmB,GAAG,QAAQ,CAClC,IAAI,CAAC,KAAK,CAAC,QAAQ,CAAC,YAAY,CAAC,CAAC,WAAW,CAC9C;AACD,IAAA,MAAM,sBAAsB,CAC1B,cAAc,CAAC,QAAQ,EACvBC,2BAAiB,EACjB,cAAc,CAAC,WAAW,EAC1B,mBAAmB,CACpB;;IAGD,MAAM,wBAAwB,GAAG,UAAU,CAAC,IAAI,CAC9C,IAAI,CAAC,SAAS,CAAC,cAAc;SAC1B,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B;AACD,IAAA,MAAM,cAAc,GAAGC,WAAM,CAAC,wBAAwB,CAAC;IACvD,IAAI,CAAC,OAAO,CAAC,cAAc,EAAE,cAAc,CAAC,SAAS,CAAC,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,6FAAA,EAAgG,cAAc,CAAC,SAAS,CAAA,oBAAA,EAAuB,cAAc,CAAA,CAAE,CAChK;IACH;;IAGA,MAAM,cAAc,GAAG,IAAI,UAAU,CAAC,cAAc,CAAC,UAAU,CAAC;AAChE,IAAA,MAAM,iBAAiB,GAAGH,8BAAqB,CAAC,cAAc,CAAC;AAC/D,IAAA,IACE,QAAQ,CAAC,SAAS,KAAK,iBAAiB;AACxC,QAAA,iBAAiB,KAAK,SAAS,CAAC,qBAAqB,EACrD;AACA,QAAA,MAAM,IAAI,KAAK,CACb,CAAA,mCAAA,EAAsC,QAAQ,CAAC,SAAS,CAAA,uBAAA,EAA0B,SAAS,CAAC,qBAAqB,CAAA,sBAAA,EAAyB,iBAAiB,CAAA,iBAAA,CAAmB,CAC/K;IACH;AACF;AAEA;;AAEG;AACG,SAAU,uBAAuB,CAAC,QAAoB,EAAA;AAC1D,IAAA,IAAI,QAAQ,CAAC,MAAM,KAAK,qCAAqC,EAAE;AAC7D,QAAA,MAAM,IAAI,KAAK,CAAC,8BAA8B,CAAC;IACjD;;AAGA,IAAA,IAAI,cAA0B;AAC9B,IAAA,IAAI;AACF,QAAA,cAAc,GAAGI,gCAAuB,CAAC,QAAQ,CAAC,SAAS,CAAC;IAC9D;AAAE,IAAA,MAAM;AACN,QAAA,MAAM,IAAI,KAAK,CAAC,6BAA6B,CAAC;IAChD;AAEA,IAAA,IAAI,cAAc,CAAC,MAAM,KAAK,GAAG,EAAE;QACjC,MAAM,IAAI,KAAK,CACb,CAAA,wDAAA,EAA2D,cAAc,CAAC,MAAM,CAAA,MAAA,CAAQ,CACzF;IACH;;IAGA,MAAM,eAAe,GAAG,cAAc,CAAC,KAAK,CAAC,EAAE,CAAC;AAChD,IAAA,IAAI,eAAe,CAAC,MAAM,KAAK,EAAE,IAAI,eAAe,CAAC,CAAC,CAAC,KAAK,IAAI,EAAE;AAChE,QAAA,MAAM,IAAI,KAAK,CACb,oFAAoF,CACrF;IACH;;AAGA,IAAA,IAAI;AACF,QAAAC,SAAI,CAAC,eAAe,CAAC,OAAO,CAAC,eAAe,CAAC;IAC/C;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,6BAA6B,KAAK,CAAA,CAAE,CAAC;IACvD;;AAGA,IAAA,IAAI,cAA0B;AAC9B,IAAA,IAAI;AACF,QAAA,cAAc,GAAGD,gCAAuB,CAAC,QAAQ,CAAC,SAAS,CAAC;IAC9D;AAAE,IAAA,MAAM;AACN,QAAA,MAAM,IAAI,KAAK,CAAC,4BAA4B,CAAC;IAC/C;AACA,IAAA,IAAI,cAAc,CAAC,MAAM,KAAK,EAAE,EAAE;QAChC,MAAM,IAAI,KAAK,CACb,CAAA,wCAAA,EAA2C,cAAc,CAAC,MAAM,CAAA,MAAA,CAAQ,CACzE;IACH;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,QAAQ,CAAC,YAAY,CAAC;AACpE,IAAA,MAAM,aAAa,GAAGD,WAAM,CAAC,YAAY,CAAC;;AAG1C,IAAA,MAAM,OAAO,GAAGE,SAAI,CAAC,MAAM,CAAC,cAAc,EAAE,aAAa,EAAE,eAAe,CAAC;IAC3E,IAAI,CAAC,OAAO,EAAE;AACZ,QAAA,MAAM,IAAI,KAAK,CAAC,+BAA+B,CAAC;IAClD;AACF;AAEO,eAAe,sBAAsB,CAC1C,QAAsB,EACtB,WAAmB,EACnB,QAAoB,EACpB,WAAmB,EAAA;AAEnB,IAAA,IAAI;;QAEF,MAAM,QAAQ,GAAG,IAAIN,eAAI,CAAC,eAAe,CAAC,WAAW,CAAC;QACtD,MAAM,OAAO,GAAG,IAAI,UAAU,CAAC,QAAQ,CAAC,OAAO,CAAC;AAChD,QAAA,MAAM,OAAO,GAAG,MAAM,SAAS,CAAC,OAAO,CAAC;AACxC,QAAA,IAAI,OAAO,KAAKO,8BAAoB,EAAE;YACpC,MAAM,IAAI,KAAK,CACb,CAAA,+CAAA,EAAkDA,8BAAoB,CAAA,QAAA,EAAW,OAAO,CAAA,CAAE,CAC3F;QACH;;QAGA,MAAM,iBAAiB,GAAG,QAAQ,CAAC,KAAK,CAAC,CAAC,CAAC;QAC3C,MAAM,iBAAiB,GAAG,iBAAiB,CAAC,GAAG,CAAC,CAAC,CAAC,KAAI;AACpD,YAAA,IAAI,CAAC,CAAC;AAAE,gBAAA,MAAM,IAAI,KAAK,CAAC,sCAAsC,CAAC;AAC/D,YAAA,OAAO,IAAIP,eAAI,CAAC,eAAe,CAAC,CAAC,CAAC;AACpC,QAAA,CAAC,CAAC;QACF,MAAM,IAAI,GAAG,IAAIA,eAAI,CAAC,eAAe,CAAC,QAAQ,CAAC;;AAG/C,QAAA,MAAM,OAAO,GAAG,IAAIA,eAAI,CAAC,gBAAgB,CAAC;AACxC,YAAA,YAAY,EAAE,CAAC,QAAQ,EAAE,GAAG,iBAAiB,CAAC;AAC/C,SAAA,CAAC;QACF,MAAM,KAAK,GAAG,MAAM,OAAO,CAAC,KAAK,CAAC,IAAI,CAAC;QACvC,IAAI,KAAK,CAAC,MAAM,KAAK,iBAAiB,CAAC,MAAM,GAAG,CAAC,EAAE;AACjD,YAAA,MAAM,IAAI,KAAK,CACb,CAAA,kDAAA,EAAqD,iBAAiB,CAAC,MAAM,GAAG,CAAC,SAAS,KAAK,CAAC,MAAM,CAAA,CAAE,CACzG;QACH;AAEA,QAAA,MAAM,YAAY,GAAG,IAAI,IAAI,CAAC,WAAW,CAAC;AAC1C,QAAA,KAAK,IAAI,CAAC,GAAG,CAAC,EAAE,CAAC,GAAG,KAAK,CAAC,MAAM,EAAE,CAAC,EAAE,EAAE;AACrC,YAAA,MAAM,IAAI,GAAG,KAAK,CAAC,CAAC,CAAC;AACrB,YAAA,IAAI,CAAC,IAAI;AAAE,gBAAA,MAAM,IAAI,KAAK,CAAC,8BAA8B,CAAC;YAE1D,IAAI,CAAC,KAAK,KAAK,CAAC,MAAM,GAAG,CAAC,EAAE;;;AAG1B,gBAAA,MAAM,EAAE,GAAG,MAAM,IAAI,CAAC,MAAM,CAAC;oBAC3B,SAAS,EAAE,IAAI,CAAC,SAAS;AACzB,oBAAA,IAAI,EAAE,YAAY;AACnB,iBAAA,CAAC;AACF,gBAAA,IAAI,CAAC,EAAE;AACL,oBAAA,MAAM,IAAI,KAAK,CAAC,gDAAgD,CAAC;YACrE;iBAAO;;gBAEL,MAAM,MAAM,GAAG,KAAK,CAAC,CAAC,GAAG,CAAC,CAAC;AAC3B,gBAAA,IAAI,CAAC,MAAM;AAAE,oBAAA,MAAM,IAAI,KAAK,CAAC,sBAAsB,CAAC;;;;;AAMpD,gBAAA,MAAM,EAAE,GAAG,MAAM,IAAI,CAAC,MAAM,CAAC;oBAC3B,SAAS,EAAE,MAAM,CAAC,SAAS;AAC3B,oBAAA,aAAa,EAAE,IAAI;AACnB,oBAAA,IAAI,EAAE,YAAY;AACnB,iBAAA,CAAC;gBACF,IAAI,CAAC,EAAE,EAAE;AACP,oBAAA,MAAM,IAAI,KAAK,CACb,CAAA,wBAAA,EAA2B,IAAI,CAAC,OAAO,CAAA,eAAA,EAAkB,MAAM,EAAE,OAAO,CAAA,CAAE,CAC3E;gBACH;YACF;QACF;IACF;IAAE,OAAO,KAAK,EAAE;QACd,MAAM,IAAI,KAAK,CACb,CAAA,uCAAA,EAA0C,KAAK,YAAY,KAAK,GAAG,KAAK,CAAC,OAAO,GAAG,MAAM,CAAC,KAAK,CAAC,CAAA,CAAE,CACnG;IACH;AACF;AAEO,eAAe,kBAAkB,CACtC,SAAc,EACd,IAAgB,EAAA;IAEhB,MAAM,CAAC,gBAAgB,IAAI,OAAO,EAAE,SAAS,CAAC,GAAG,SAAS;IAC1D,MAAM,GAAG,GAAG,IAAI,UAAU,CACxBE,eAAI,CAAC,MAAM,CAAC;QACV,YAAY;QACZ,IAAI,UAAU,CAAC,gBAAgB,CAAC;QAChC,IAAI,UAAU,CAAC,CAAC,CAAC;QACjB,IAAI,UAAU,CAAC,OAAO,CAAC;AACxB,KAAA,CAAC,CACH;IAED,MAAM,QAAQ,GAAG,IAAIF,eAAI,CAAC,eAAe,CAAC,IAAI,CAAC;IAC/C,MAAM,MAAM,GAAG,MAAM,oBAAoB,CAAC,QAAQ,CAAC,SAAS,CAAC,OAAO,CAAC;AAErE,IAAA,MAAM,cAAc,GAAG,MAAM,iBAAiB,EAAE;AAChD,IAAA,MAAM,EAAE,GAAG,MAAM,cAAc,CAAC,MAAM,CAAC,MAAM,CAC3C,EAAE,IAAI,EAAE,OAAO,EAAE,IAAI,EAAE,EAAE,IAAI,EAAE,SAAS,EAAE,EAAE,EAC5C,MAAM,EACN,IAAI,UAAU,CAAC,SAAS,CAAC,EACzB,GAAG,CACJ;AACD,IAAA,IAAI,CAAC,EAAE;AAAE,QAAA,MAAM,IAAI,KAAK,CAAC,sCAAsC,CAAC;AAClE;AAEA,SAAS,OAAO,CAAC,CAAc,EAAE,CAAc,EAAA;AAC7C,IAAA,MAAM,CAAC,GAAG,IAAI,UAAU,CAAC,CAAC,CAAC,EACzB,CAAC,GAAG,IAAI,UAAU,CAAC,CAAC,CAAC;AACvB,IAAA,IAAI,CAAC,CAAC,MAAM,KAAK,CAAC,CAAC,MAAM;AAAE,QAAA,OAAO,KAAK;AACvC,IAAA,KAAK,IAAI,CAAC,GAAG,CAAC,EAAE,CAAC,GAAG,CAAC,CAAC,MAAM,EAAE,CAAC,EAAE;QAAE,IAAI,CAAC,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;AAAE,YAAA,OAAO,KAAK;AAClE,IAAA,OAAO,IAAI;AACb;;;;;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:4:import { sha256 } from "@noble/hashes/sha256";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:248:  crypto.getRandomValues(nonce);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:265:  const encryptedMessageBuf = await crypto.subtle.encrypt(alg, cipher, message);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:551:  const prk = hkdf.extract(sha256, ikm, sharedSecret);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:552:  const resp = hkdf.expand(sha256, prk, info, len);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:830:  const hmacKey = await crypto.subtle.importKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:841:    await crypto.subtle.sign("HMAC", hmacKey, QOS_ENCRYPTION_HMAC_MESSAGE),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:847:  return crypto.subtle.importKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:7:import { sha256 } from "@noble/hashes/sha2";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:13:  let cryptoInstance: Crypto;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:14:  // Use globalThis.crypto.subtle if available
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:15:  if (typeof globalThis !== "undefined" && globalThis.crypto?.subtle) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:16:    cryptoInstance = globalThis.crypto as Crypto;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:17:    x509.cryptoProvider.set(cryptoInstance);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:19:    return cryptoInstance;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:30:async function sha256Hex(data: Uint8Array): Promise<string> {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:31:  const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:32:  const digest = await cryptoInstance.subtle.digest("SHA-256", data);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:40:  const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:41:  return cryptoInstance.subtle.importKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:98:  const manifestDigest = sha256(decodedBootProofManifest);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:170:  const payloadDigest = sha256(payloadBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:189:    const rootSha = await sha256Hex(rootDer);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:274:  const cryptoInstance = await getCryptoInstance();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:275:  const ok = await cryptoInstance.subtle.verify(

 succeeded in 1529ms:
2026-05-23 10:26:52.296 xcodebuild[38118:106247609]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:26:52.421 xcodebuild[38118:106247608] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-SwTNeFLz' (errno=Operation not permitted)
2026-05-23 10:26:52.803 xcodebuild[38120:106247618]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:26:52.926 xcodebuild[38120:106247617] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
?? codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '1,90p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '360,470p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '470,560p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '60,100p;510,560p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

 succeeded in 0ms:
    60	        }
    61	
    62	        [Fact]
    63	        public void ModSqrt_NegativeX_Throws()
    64	        {
    65	            // Upstream JS BigInt % keeps the sign of the dividend, so a
    66	            // negative x stays negative through "base = x % p" and then
    67	            // fails the squareRoot check. Mirror that.
    68	            var p = new BigInteger("7");
    69	            var x = new BigInteger("-4");
    70	            Action act = () => Crypto.Math.ModSqrt(x, p);
    71	            act.Should().Throw<InvalidOperationException>()
    72	               .WithMessage("could not find a modular square root");
    73	        }
    74	
    75	        [Fact]
    76	        public void ModSqrt_P256Prime_Works()
    77	        {
    78	            // P-256 prime. Pick any quadratic residue: 4 = 2^2.
    79	            var p = new BigInteger(CryptoConstants.P256_P);
    80	            var x = new BigInteger("4");
    81	            var root = Crypto.Math.ModSqrt(x, p);
    82	            root.Multiply(root).Mod(p).Equals(x).Should().BeTrue();
    83	        }
    84	
    85	        // ============================================================
    86	        // HKDF RFC 5869
    87	        // ============================================================
    88	
    89	        // RFC 5869 Test Case 1 (Basic test case with SHA-256)
    90	        [Fact]
    91	        public void Hkdf_Rfc5869_A1()
    92	        {
    93	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
    94	            byte[] salt = HexToBytes("000102030405060708090a0b0c");
    95	            byte[] info = HexToBytes("f0f1f2f3f4f5f6f7f8f9");
    96	            int L = 42;
    97	
    98	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
    99	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
   100	
   510	            //                                       029bfcdb 2dce28d9 59f2815b 16f81798
   511	            byte[] compressed = Encoding.Uint8ArrayFromHexString(
   512	                "0279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798");
   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
   514	            uncompressed.Should().HaveCount(65);
   515	            uncompressed[0].Should().Be(0x04);
   516	            // Y coordinate of G must be even (prefix was 0x02).
   517	            (uncompressed[64] & 1).Should().Be(0);
   518	        }
   519	
   520	        // ============================================================
   521	        // Bundle helpers — including a real Turnkey-pinned vector
   522	        // ============================================================
   523	
   524	        [Fact]
   525	        public void DecryptCredentialBundle_UpstreamVector()
   526	        {
   527	            // From upstream tests/__tests__/crypto-test.ts:179-184
   528	            // (codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts)
   529	            const string credentialBundle =
   530	                "w99a5xV6A75TfoAUkZn869fVyDYvgVsKrawMALZXmrauZd8hEv66EkPU1Z42CUaHESQjcA5bqd8dynTGBMLWB9ewtXWPEVbZvocB4Tw2K1vQVp7uwjf";
   531	            const string embeddedKey =
   532	                "20fa65df11f24833790ae283fc9a0c215eecbbc589549767977994dc69d05a56";
   533	            const string expectedSenderPrivateKey =
   534	                "67ee05fc3bdf4161bc70701c221d8d77180294cefcfcea64ba83c4d4c732fcb9";
   535	
   536	            string decrypted = Crypto.DecryptCredentialBundle(credentialBundle, embeddedKey);
   537	            decrypted.Should().Be(expectedSenderPrivateKey);
   538	        }
   539	
   540	        [Fact]
   541	        public void UncompressRawPublicKey_UpstreamInvalidPrefixVector()
   542	        {
   543	            // From upstream tests/__tests__/crypto-test.ts:243-250
   544	            byte[] invalidPrefix = Encoding.Uint8ArrayFromHexString(
   545	                "77c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5");
   546	            Action act = () => Crypto.UncompressRawPublicKey(invalidPrefix);
   547	            act.Should().Throw<ArgumentException>()
   548	               .WithMessage("failed to uncompress raw public key: invalid prefix");
   549	        }
   550	
   551	        [Fact]
   552	        public void CompressRawPublicKey_EmptyInput_ReturnsEmpty()
   553	        {
   554	            // Upstream behavior: empty Uint8Array slice produces empty result.
   555	            Crypto.CompressRawPublicKey(Array.Empty<byte>()).Should().BeEmpty();
   556	        }
   557	
   558	        [Fact]
   559	        public void HpkeEncrypt_NullPlainTextBuf_Throws()
   560	        {

codex
CryptoConstants.cs is unusual: it has no methods, only extracted P-256 constants used by Crypto.cs. I’m checking whether those constants are exercised indirectly by the crypto fixtures and whether any fixture claims an exact pinned-upstream byte source.
exec
/bin/zsh -lc 'rg --files tests/Fixtures' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
tests/Fixtures/crypto/turnkey-crypto-vectors.json

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
/bin/zsh -lc 'find tests/Fixtures -type f -maxdepth 3 -print' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/crypto/turnkey-crypto-vectors.json
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/README.md

exec
/bin/zsh -lc 'rg -n "CryptoConstants|P256_P|P256_B|P256_A_OFFSET|CURVE_NAME|COMPRESSED_PUBLIC_KEY_SIZE|UncompressRawPublicKey|CompressRawPublicKey|turnkey-crypto|tarball_sha256|source" tests/Fixtures tests src' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
src/CryptoConstants.cs:32:    public static class CryptoConstants
src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
src/CryptoConstants.cs:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
src/CryptoConstants.cs:58:        public const string P256_P =
src/CryptoConstants.cs:66:        public const string P256_B =
src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
tests/PublicApi.expected.txt:30:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey) { }
tests/PublicApi.expected.txt:40:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Turnkey.Crypto.Curve curve = 0) { }
tests/PublicApi.expected.txt:121:    public static class CryptoConstants
tests/PublicApi.expected.txt:123:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
tests/PublicApi.expected.txt:124:        public const string CURVE_NAME = "secp256r1";
tests/PublicApi.expected.txt:125:        public const string P256_A_OFFSET = "3";
tests/PublicApi.expected.txt:126:        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
tests/PublicApi.expected.txt:127:        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
src/Http.cs:25://   - Body JSON serialized via TurnkeyJsonContext (source-gen, no
tests/ApiKeyStamperTests.cs:3:// Vector sources:
tests/ApiKeyStamperTests.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{private,public}
tests/ApiKeyStamperTests.cs:5://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts
tests/ApiKeyStamperTests.cs:9:// same digest, so for identical input + key + nonce-source the signature
tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/Fixtures/encoding/turnkey-encoding-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:6:    "tarball_sha256": "2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc",
src/Crypto.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
src/Crypto.cs:7://   ts-source/constants.ts            -> Crypto.Constants nested class
src/Crypto.cs:8://   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
src/Crypto.cs:9://   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
src/Crypto.cs:12://                                        CompressRawPublicKey /
src/Crypto.cs:13://                                        UncompressRawPublicKey /
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:19://   ts-source/turnkey.ts (subset)     -> Curve enum / DecryptCredentialBundle /
src/Crypto.cs:32://   - System.Text.Json source generation (TurnkeyJsonContext) replaces
src/Crypto.cs:130:        /// <see cref="UncompressRawPublicKey(byte[], Curve)"/>.
src/Crypto.cs:487:                var compressedSenderBuf = CompressRawPublicKey(senderPubBuf);
src/Crypto.cs:518:        public static byte[] CompressRawPublicKey(byte[] rawPublicKey)
src/Crypto.cs:546:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Curve curve = Curve.P256)
src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
src/Crypto.cs:630:            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);
src/Crypto.cs:681:                var encappedKeyBuf = UncompressRawPublicKey(compressedEncappedKeyBuf);
src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/CryptoTests.cs:3:// Vector sources:
tests/CryptoTests.cs:6://   - Compress/UncompressRawPublicKey roundtrip
tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
tests/CryptoTests.cs:194:        // CompressRawPublicKey / UncompressRawPublicKey
tests/CryptoTests.cs:204:                byte[] compressed = Crypto.CompressRawPublicKey(uncompressed);
tests/CryptoTests.cs:208:                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:214:        public void CompressRawPublicKey_Permissive_OnAnyLength()
tests/CryptoTests.cs:224:            Action act = () => Crypto.CompressRawPublicKey(raw);
tests/CryptoTests.cs:229:        public void UncompressRawPublicKey_BadPrefix_Throws()
tests/CryptoTests.cs:233:            Action act = () => Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:239:        public void UncompressRawPublicKey_WrongLength_Throws()
tests/CryptoTests.cs:243:            Action act = () => Crypto.UncompressRawPublicKey(compressed);
tests/CryptoTests.cs:349:            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);
tests/CryptoTests.cs:381:                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
tests/CryptoTests.cs:408:                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
tests/CryptoTests.cs:503:        // Curve.Secp256k1 — UncompressRawPublicKey
tests/CryptoTests.cs:507:        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
tests/CryptoTests.cs:513:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
tests/CryptoTests.cs:528:            // (codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts)
tests/CryptoTests.cs:541:        public void UncompressRawPublicKey_UpstreamInvalidPrefixVector()
tests/CryptoTests.cs:546:            Action act = () => Crypto.UncompressRawPublicKey(invalidPrefix);
tests/CryptoTests.cs:552:        public void CompressRawPublicKey_EmptyInput_ReturnsEmpty()
tests/CryptoTests.cs:555:            Crypto.CompressRawPublicKey(Array.Empty<byte>()).Should().BeEmpty();
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:4:    "source_fixture": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{public,private}",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:5:    "source_test": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:7:    "tarball_sha256": "962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:19:    "_source_line": "ts-source/__tests__/stamp-test.ts:14-37",
src/TurnkeyJsonContext.cs:1:// IL2CPP-safe System.Text.Json source-generated context.
src/TurnkeyJsonContext.cs:32:    /// IL2CPP-safe System.Text.Json source-generated context for every DTO
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
src/Encoding.cs:7://   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
src/Encoding.cs:9://   ts-source/base64.ts       -> StringToBase64UrlString /
src/Encoding.cs:14://   ts-source/encode.ts       -> PointEncode
src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
src/Encoding.cs:17://   ts-source/index.ts        -> DEFAULT_JWK_MEMBER_BYTE_LENGTH constant
src/Encoding.cs:61:            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
tests/Fixtures/crypto/turnkey-crypto-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:6:    "tarball_sha256": "75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:11:      "_source_line": "ts-source/__tests__/crypto-test.ts:179-184",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:21:      "_source_line": "ts-source/__tests__/crypto-test.ts:243-250",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:26:      "_source_line": "ts-source/__tests__/crypto-test.ts:252-260",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:32:    "_source_line": "ts-source/__tests__/crypto-test.ts:173-178",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:6:    "tarball_sha256": "2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc",
src/ApiKeyStamper.cs:7://   ts-source/index.ts                -> ApiKeyStamper class + signWithApiKey
src/ApiKeyStamper.cs:9://   ts-source/purejs.ts               -> signWithApiKey "purejs" runtime
src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/PublicApiSnapshotTests.cs:33:            // Resolve the baseline relative to the test project source.
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:4:    "source_fixture": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{public,private}",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:5:    "source_test": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:7:    "tarball_sha256": "962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:19:    "_source_line": "ts-source/__tests__/stamp-test.ts:14-37",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:6:    "tarball_sha256": "75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:11:      "_source_line": "ts-source/__tests__/crypto-test.ts:179-184",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:21:      "_source_line": "ts-source/__tests__/crypto-test.ts:243-250",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:26:      "_source_line": "ts-source/__tests__/crypto-test.ts:252-260",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:32:    "_source_line": "ts-source/__tests__/crypto-test.ts:173-178",

exec
/bin/zsh -lc "nl -ba tests/Fixtures/crypto/turnkey-crypto-vectors.json | sed -n '1,90p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	{
     2	  "_provenance": {
     3	    "level": "upstream-test-vectors",
     4	    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts",
     5	    "package": "@turnkey/crypto@2.8.8",
     6	    "tarball_sha256": "75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3",
     7	    "note": "Real Turnkey-pinned credential bundle test vector. Verified by Codex r2 against the upstream Jest test (__tests__/crypto-test.ts line 179-184)."
     8	  },
     9	  "decryptCredentialBundle": [
    10	    {
    11	      "_source_line": "ts-source/__tests__/crypto-test.ts:179-184",
    12	      "input": {
    13	        "credentialBundle": "w99a5xV6A75TfoAUkZn869fVyDYvgVsKrawMALZXmrauZd8hEv66EkPU1Z42CUaHESQjcA5bqd8dynTGBMLWB9ewtXWPEVbZvocB4Tw2K1vQVp7uwjf",
    14	        "embeddedKey": "20fa65df11f24833790ae283fc9a0c215eecbbc589549767977994dc69d05a56"
    15	      },
    16	      "expectedHex": "67ee05fc3bdf4161bc70701c221d8d77180294cefcfcea64ba83c4d4c732fcb9"
    17	    }
    18	  ],
    19	  "uncompressRawPublicKey": {
    20	    "invalid_prefix": {
    21	      "_source_line": "ts-source/__tests__/crypto-test.ts:243-250",
    22	      "input_hex": "77c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5",
    23	      "expected_error_message": "failed to uncompress raw public key: invalid prefix"
    24	    },
    25	    "invalid_length": {
    26	      "_source_line": "ts-source/__tests__/crypto-test.ts:252-260",
    27	      "note": "Upstream feeds keypair.publicKey + keypair.publicKey (66 bytes); we use any non-33-byte buffer.",
    28	      "expected_error_message": "failed to uncompress raw public key: invalid length"
    29	    }
    30	  },
    31	  "compressRawPublicKey": {
    32	    "_source_line": "ts-source/__tests__/crypto-test.ts:173-178",
    33	    "roundtrip_note": "compress(publicKeyUncompressed) === publicKey for any P-256 key pair."
    34	  }
    35	}

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
    63	        public void ModSqrt_NegativeX_Throws()
    64	        {
    65	            // Upstream JS BigInt % keeps the sign of the dividend, so a
    66	            // negative x stays negative through "base = x % p" and then
    67	            // fails the squareRoot check. Mirror that.
    68	            var p = new BigInteger("7");
    69	            var x = new BigInteger("-4");
    70	            Action act = () => Crypto.Math.ModSqrt(x, p);
    71	            act.Should().Throw<InvalidOperationException>()
    72	               .WithMessage("could not find a modular square root");
    73	        }
    74	
    75	        [Fact]
    76	        public void ModSqrt_P256Prime_Works()
    77	        {
    78	            // P-256 prime. Pick any quadratic residue: 4 = 2^2.
    79	            var p = new BigInteger(CryptoConstants.P256_P);
    80	            var x = new BigInteger("4");
    81	            var root = Crypto.Math.ModSqrt(x, p);
    82	            root.Multiply(root).Mod(p).Equals(x).Should().BeTrue();
    83	        }
    84	
    85	        // ============================================================
    86	        // HKDF RFC 5869
    87	        // ============================================================
    88	
    89	        // RFC 5869 Test Case 1 (Basic test case with SHA-256)
    90	        [Fact]
    91	        public void Hkdf_Rfc5869_A1()
    92	        {
    93	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
    94	            byte[] salt = HexToBytes("000102030405060708090a0b0c");
    95	            byte[] info = HexToBytes("f0f1f2f3f4f5f6f7f8f9");
    96	            int L = 42;
    97	
    98	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
    99	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
   100	
   101	            BytesToHex(prk).Should().Be(
   102	                "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5");
   103	            BytesToHex(okm).Should().Be(
   104	                "3cb25f25faacd57a90434f64d0362f2a"
   105	                + "2d2d0a90cf1a5a4c5db02d56ecc4c5bf"
   106	                + "34007208d5b887185865");
   107	        }
   108	
   109	        // RFC 5869 Test Case 2 (Test with SHA-256 and longer inputs/outputs)
   110	        [Fact]
   111	        public void Hkdf_Rfc5869_A2()
   112	        {
   113	            byte[] ikm = HexToBytes(
   114	                "000102030405060708090a0b0c0d0e0f"
   115	                + "101112131415161718191a1b1c1d1e1f"
   116	                + "202122232425262728292a2b2c2d2e2f"
   117	                + "303132333435363738393a3b3c3d3e3f"
   118	                + "404142434445464748494a4b4c4d4e4f");
   119	            byte[] salt = HexToBytes(
   120	                "606162636465666768696a6b6c6d6e6f"
   121	                + "707172737475767778797a7b7c7d7e7f"
   122	                + "808182838485868788898a8b8c8d8e8f"
   123	                + "909192939495969798999a9b9c9d9e9f"
   124	                + "a0a1a2a3a4a5a6a7a8a9aaabacadaeaf");
   125	            byte[] info = HexToBytes(
   126	                "b0b1b2b3b4b5b6b7b8b9babbbcbdbebf"
   127	                + "c0c1c2c3c4c5c6c7c8c9cacbcccdcecf"
   128	                + "d0d1d2d3d4d5d6d7d8d9dadbdcdddedf"
   129	                + "e0e1e2e3e4e5e6e7e8e9eaebecedeeef"
   130	                + "f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff");
   131	            int L = 82;
   132	
   133	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
   134	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
   135	
   136	            BytesToHex(prk).Should().Be(
   137	                "06a6b88c5853361a06104c9ceb35b45cef760014904671014a193f40c15fc244");
   138	            BytesToHex(okm).Should().Be(
   139	                "b11e398dc80327a1c8e7f78c596a4934"
   140	                + "4f012eda2d4efad8a050cc4c19afa97c"
   141	                + "59045a99cac7827271cb41c65e590e09"
   142	                + "da3275600c2f09b8367793a9aca3db71"
   143	                + "cc30c58179ec3e87c14c01d5c1f3434f"
   144	                + "1d87");
   145	        }
   146	
   147	        // RFC 5869 Test Case 3 (Test with SHA-256 and zero-length salt/info)
   148	        [Fact]
   149	        public void Hkdf_Rfc5869_A3()
   150	        {
   151	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
   152	            byte[] salt = Array.Empty<byte>();
   153	            byte[] info = Array.Empty<byte>();
   154	            int L = 42;
   155	
   156	            byte[] prk = Crypto.Hkdf.Extract(salt, ikm);
   157	            byte[] okm = Crypto.Hkdf.Expand(prk, info, L);
   158	
   159	            BytesToHex(prk).Should().Be(
   160	                "19ef24a32c717b167f33a91d6f648bdf96596776afdb6377ac434c1c293ccb04");
   161	            BytesToHex(okm).Should().Be(
   162	                "8da4e775a563c18f715f802a063c5a31"
   163	                + "b8a11f5c5ee1879ec3454e5f3c738d2d"
   164	                + "9d201395faa4b61a96c8");
   165	        }
   166	
   167	        [Fact]
   168	        public void Hkdf_Expand_Length0_ProducesEmpty()
   169	        {
   170	            byte[] prk = HexToBytes(
   171	                "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5");
   172	            byte[] okm = Crypto.Hkdf.Expand(prk, null!, 0);
   173	            okm.Should().BeEmpty();
   174	        }
   175	
   176	        [Fact]
   177	        public void Hkdf_Expand_OverMaxLength_Throws()
   178	        {
   179	            byte[] prk = new byte[32];
   180	            Action act = () => Crypto.Hkdf.Expand(prk, null!, 255 * 32 + 1);
   181	            act.Should().Throw<ArgumentException>();
   182	        }
   183	
   184	        [Fact]
   185	        public void Hkdf_Extract_NullSalt_UsesZeroSalt()
   186	        {
   187	            byte[] ikm = HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
   188	            byte[] prkA = Crypto.Hkdf.Extract(null!, ikm);
   189	            byte[] prkB = Crypto.Hkdf.Extract(new byte[32], ikm); // RFC default
   190	            prkA.Should().Equal(prkB);
   191	        }
   192	
   193	        // ============================================================
   194	        // CompressRawPublicKey / UncompressRawPublicKey
   195	        // ============================================================
   196	
   197	        [Fact]
   198	        public void CompressUncompress_Roundtrip_RandomKeys()
   199	        {
   200	            for (int i = 0; i < 8; i++)
   201	            {
   202	                var kp = Crypto.GenerateP256KeyPair();
   203	                byte[] uncompressed = Encoding.Uint8ArrayFromHexString(kp.PublicKeyUncompressed);
   204	                byte[] compressed = Crypto.CompressRawPublicKey(uncompressed);
   205	                compressed.Should().HaveCount(33);
   206	                compressed[0].Should().BeOneOf((byte)0x02, (byte)0x03);
   207	
   208	                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
   209	                recovered.Should().Equal(uncompressed);
   210	            }
   211	        }
   212	
   213	        [Fact]
   214	        public void CompressRawPublicKey_Permissive_OnAnyLength()
   215	        {
   216	            // Upstream behavior: slice + LSB-flip regardless of input shape.
   217	            // For a 65-byte uncompressed key we produce a 33-byte compressed key.
   218	            // For other lengths we produce a different-length output that
   219	            // upstream would also produce. Confirm we no longer throw on
   220	            // malformed input.
   221	            var raw = new byte[64];
   222	            raw[0] = 0x04;
   223	            raw[63] = 0x01;
   224	            Action act = () => Crypto.CompressRawPublicKey(raw);
   225	            act.Should().NotThrow();
   226	        }
   227	
   228	        [Fact]
   229	        public void UncompressRawPublicKey_BadPrefix_Throws()
   230	        {
   231	            var compressed = new byte[33];
   232	            compressed[0] = 0x05;
   233	            Action act = () => Crypto.UncompressRawPublicKey(compressed);
   234	            act.Should().Throw<ArgumentException>()
   235	               .WithMessage("failed to uncompress raw public key: invalid prefix");
   236	        }
   237	
   238	        [Fact]
   239	        public void UncompressRawPublicKey_WrongLength_Throws()
   240	        {
   241	            var compressed = new byte[32];
   242	            compressed[0] = 0x02;
   243	            Action act = () => Crypto.UncompressRawPublicKey(compressed);
   244	            act.Should().Throw<ArgumentException>()
   245	               .WithMessage("failed to uncompress raw public key: invalid length");
   246	        }
   247	
   248	        // ============================================================
   249	        // GetPublicKey / GenerateP256KeyPair
   250	        // ============================================================
   251	
   252	        [Fact]
   253	        public void GenerateP256KeyPair_LengthsAreCorrect()
   254	        {
   255	            var kp = Crypto.GenerateP256KeyPair();
   256	            kp.PrivateKey.Should().HaveLength(64);          // 32 bytes hex
   257	            kp.PublicKey.Should().HaveLength(66);            // 33 bytes hex
   258	            kp.PublicKeyUncompressed.Should().HaveLength(130); // 65 bytes hex
   259	            kp.PublicKey.Should().MatchRegex("^0[23][0-9a-f]{64}$");
   260	            kp.PublicKeyUncompressed.Should().StartWith("04");

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '260,560p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   260	            kp.PublicKeyUncompressed.Should().StartWith("04");
   261	        }
   262	
   263	        [Fact]
   264	        public void GetPublicKey_FromPrivateKey_ProducesCompressedByDefault()
   265	        {
   266	            var kp = Crypto.GenerateP256KeyPair();
   267	            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
   268	            byte[] pub = Crypto.GetPublicKey(priv); // isCompressed default true
   269	            pub.Should().HaveCount(33);
   270	            Encoding.Uint8ArrayToHexString(pub).Should().Be(kp.PublicKey);
   271	        }
   272	
   273	        [Fact]
   274	        public void GetPublicKey_Uncompressed_MatchesGenerator()
   275	        {
   276	            var kp = Crypto.GenerateP256KeyPair();
   277	            byte[] priv = Encoding.Uint8ArrayFromHexString(kp.PrivateKey);
   278	            byte[] uncompressed = Crypto.GetPublicKey(priv, isCompressed: false);
   279	            Encoding.Uint8ArrayToHexString(uncompressed).Should().Be(kp.PublicKeyUncompressed);
   280	        }
   281	
   282	        [Fact]
   283	        public void GetPublicKey_HexStringOverload_MatchesByteOverload()
   284	        {
   285	            var kp = Crypto.GenerateP256KeyPair();
   286	            byte[] viaBytes = Crypto.GetPublicKey(Encoding.Uint8ArrayFromHexString(kp.PrivateKey));
   287	            byte[] viaHex = Crypto.GetPublicKey(kp.PrivateKey);
   288	            viaBytes.Should().Equal(viaHex);
   289	        }
   290	
   291	        [Fact]
   292	        public void GetPublicKey_InvalidKeyLength_Throws()
   293	        {
   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
   295	            Action act = () => Crypto.GetPublicKey(new byte[31]);
   296	            act.Should().Throw<ArgumentException>()
   297	               .WithMessage("invalid P-256 private key: expected 32 bytes, got 31");
   298	        }
   299	
   300	        [Fact]
   301	        public void GetPublicKey_ScalarZero_Throws()
   302	        {
   303	            // Upstream noble rejects scalar 0 (outside [1, n-1]).
   304	            Action act = () => Crypto.GetPublicKey(new byte[32]);
   305	            act.Should().Throw<ArgumentException>()
   306	               .WithMessage("invalid P-256 private key: scalar must be in [1, n - 1]");
   307	        }
   308	
   309	        [Fact]
   310	        public void GetPublicKey_ScalarEqualsN_Throws()
   311	        {
   312	            // Use the curve order N (= 0xffffff...bce6faada7179e84f3b9cac2fc632551)
   313	            byte[] nBytes = Encoding.Uint8ArrayFromHexString(
   314	                "ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551");
   315	            Action act = () => Crypto.GetPublicKey(nBytes);
   316	            act.Should().Throw<ArgumentException>()
   317	               .WithMessage("invalid P-256 private key: scalar must be in [1, n - 1]");
   318	        }
   319	
   320	        // ============================================================
   321	        // HPKE roundtrip
   322	        // ============================================================
   323	
   324	        [Fact]
   325	        public void Hpke_EncryptThenDecrypt_RoundTripsArbitraryPayload()
   326	        {
   327	            // Receiver key pair.
   328	            var recv = Crypto.GenerateP256KeyPair();
   329	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   330	
   331	            byte[] payload = Encoding.Uint8ArrayFromHexString(
   332	                "deadbeefcafebabe1234567890abcdef00112233445566778899aabbccddeeff");
   333	
   334	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   335	            {
   336	                PlainTextBuf = payload,
   337	                TargetKeyBuf = recvPubUncompressed,
   338	            });
   339	
   340	            // encrypted = compressed(senderPub) || ciphertext (33 + N bytes)
   341	            encrypted.Length.Should().BeGreaterThan(33);
   342	
   343	            // Split, uncompress the sender's compressed point, then decrypt.
   344	            var compressedSender = new byte[33];
   345	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   346	            var ciphertext = new byte[encrypted.Length - 33];
   347	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   348	
   349	            byte[] encapped = Crypto.UncompressRawPublicKey(compressedSender);
   350	
   351	            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   352	            {
   353	                CiphertextBuf = ciphertext,
   354	                EncappedKeyBuf = encapped,
   355	                ReceiverPriv = recv.PrivateKey,
   356	            });
   357	
   358	            decrypted.Should().Equal(payload);
   359	        }
   360	
   361	        [Fact]
   362	        public void Hpke_EncryptThenDecrypt_EmptyPayload()
   363	        {
   364	            var recv = Crypto.GenerateP256KeyPair();
   365	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   366	
   367	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   368	            {
   369	                PlainTextBuf = Array.Empty<byte>(),
   370	                TargetKeyBuf = recvPubUncompressed,
   371	            });
   372	
   373	            var compressedSender = new byte[33];
   374	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   375	            var ciphertext = new byte[encrypted.Length - 33];
   376	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   377	
   378	            byte[] decrypted = Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   379	            {
   380	                CiphertextBuf = ciphertext,
   381	                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
   382	                ReceiverPriv = recv.PrivateKey,
   383	            });
   384	            decrypted.Should().BeEmpty();
   385	        }
   386	
   387	        [Fact]
   388	        public void Hpke_DecryptWithWrongKey_Throws()
   389	        {
   390	            var recv = Crypto.GenerateP256KeyPair();
   391	            var attacker = Crypto.GenerateP256KeyPair();
   392	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   393	
   394	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   395	            {
   396	                PlainTextBuf = Encoding.Uint8ArrayFromHexString("aabbcc"),
   397	                TargetKeyBuf = recvPubUncompressed,
   398	            });
   399	
   400	            var compressedSender = new byte[33];
   401	            Array.Copy(encrypted, 0, compressedSender, 0, 33);
   402	            var ciphertext = new byte[encrypted.Length - 33];
   403	            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
   404	
   405	            Action act = () => Crypto.HpkeDecrypt(new Crypto.HpkeDecryptParams
   406	            {
   407	                CiphertextBuf = ciphertext,
   408	                EncappedKeyBuf = Crypto.UncompressRawPublicKey(compressedSender),
   409	                ReceiverPriv = attacker.PrivateKey,
   410	            });
   411	            act.Should().Throw<Exception>();
   412	        }
   413	
   414	        // ============================================================
   415	        // BuildAdditionalAssociatedData / FormatHpkeBuf
   416	        // ============================================================
   417	
   418	        [Fact]
   419	        public void BuildAdditionalAssociatedData_ConcatsArgs()
   420	        {
   421	            byte[] a = { 0x01, 0x02 };
   422	            byte[] b = { 0x03, 0x04, 0x05 };
   423	            byte[] aad = Crypto.BuildAdditionalAssociatedData(a, b);
   424	            aad.Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
   425	        }
   426	
   427	        [Fact]
   428	        public void FormatHpkeBuf_ReturnsExpectedJson()
   429	        {
   430	            // Use a real HpkeEncrypt output so the bytes are valid.
   431	            var recv = Crypto.GenerateP256KeyPair();
   432	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   433	
   434	            byte[] encrypted = Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   435	            {
   436	                PlainTextBuf = Encoding.Uint8ArrayFromHexString("1234"),
   437	                TargetKeyBuf = recvPubUncompressed,
   438	            });
   439	
   440	            string json = Crypto.FormatHpkeBuf(encrypted);
   441	
   442	            // Shape contains exactly two keys in upstream order.
   443	            json.Should().StartWith("{\"encappedPublic\":\"04");
   444	            json.Should().Contain("\"ciphertext\":\"");
   445	            json.Should().EndWith("\"}");
   446	        }
   447	
   448	        [Fact]
   449	        public void FormatHpkeBuf_TooSmall_Throws()
   450	        {
   451	            // Upstream passes the slice to uncompressRawPublicKey which
   452	            // throws "failed to uncompress raw public key: invalid length"
   453	            // when the buffer is shorter than 33 bytes.
   454	            Action act = () => Crypto.FormatHpkeBuf(new byte[10]);
   455	            act.Should().Throw<ArgumentException>()
   456	               .WithMessage("failed to uncompress raw public key: invalid length");
   457	        }
   458	
   459	        // ============================================================
   460	        // VerifySessionJwtSignature negative paths
   461	        // ============================================================
   462	
   463	        [Fact]
   464	        public void VerifySessionJwtSignature_Empty_Throws()
   465	        {
   466	            // Upstream: throws "invalid JWT: need 3 parts" when signature part is missing.
   467	            Action act = () => Crypto.VerifySessionJwtSignature(string.Empty);
   468	            act.Should().Throw<InvalidOperationException>()
   469	               .WithMessage("invalid JWT: need 3 parts");
   470	        }
   471	
   472	        [Fact]
   473	        public void VerifySessionJwtSignature_WrongPartCount_Throws()
   474	        {
   475	            Action act = () => Crypto.VerifySessionJwtSignature("just.two");
   476	            act.Should().Throw<InvalidOperationException>()
   477	               .WithMessage("invalid JWT: need 3 parts");
   478	        }
   479	
   480	        [Fact]
   481	        public void VerifySessionJwtSignature_BadSignatureLength_ReturnsFalse()
   482	        {
   483	            // 3 parts but the signature decodes to a non-64-byte buffer.
   484	            string jwt = "aGVhZGVy.cGF5bG9hZA.YWJj"; // last part decodes to "abc" (3 bytes)
   485	            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
   486	        }
   487	
   488	        [Fact]
   489	        public void VerifySessionJwtSignature_WellFormedButForgedSig_ReturnsFalse()
   490	        {
   491	            // 64-byte signature of all zeros — should fail verification.
   492	            string header = Encoding.Base64StringToBase64UrlEncodedString(
   493	                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"ES256\"}")));
   494	            string payload = Encoding.Base64StringToBase64UrlEncodedString(
   495	                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"sub\":\"x\"}")));
   496	            string sig = Encoding.Base64StringToBase64UrlEncodedString(
   497	                Convert.ToBase64String(new byte[64]));
   498	            string jwt = header + "." + payload + "." + sig;
   499	            Crypto.VerifySessionJwtSignature(jwt).Should().BeFalse();
   500	        }
   501	
   502	        // ============================================================
   503	        // Curve.Secp256k1 — UncompressRawPublicKey
   504	        // ============================================================
   505	
   506	        [Fact]
   507	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
   508	        {
   509	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
   510	            //                                       029bfcdb 2dce28d9 59f2815b 16f81798
   511	            byte[] compressed = Encoding.Uint8ArrayFromHexString(
   512	                "0279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798");
   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
   514	            uncompressed.Should().HaveCount(65);
   515	            uncompressed[0].Should().Be(0x04);
   516	            // Y coordinate of G must be even (prefix was 0x02).
   517	            (uncompressed[64] & 1).Should().Be(0);
   518	        }
   519	
   520	        // ============================================================
   521	        // Bundle helpers — including a real Turnkey-pinned vector
   522	        // ============================================================
   523	
   524	        [Fact]
   525	        public void DecryptCredentialBundle_UpstreamVector()
   526	        {
   527	            // From upstream tests/__tests__/crypto-test.ts:179-184
   528	            // (codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts)
   529	            const string credentialBundle =
   530	                "w99a5xV6A75TfoAUkZn869fVyDYvgVsKrawMALZXmrauZd8hEv66EkPU1Z42CUaHESQjcA5bqd8dynTGBMLWB9ewtXWPEVbZvocB4Tw2K1vQVp7uwjf";
   531	            const string embeddedKey =
   532	                "20fa65df11f24833790ae283fc9a0c215eecbbc589549767977994dc69d05a56";
   533	            const string expectedSenderPrivateKey =
   534	                "67ee05fc3bdf4161bc70701c221d8d77180294cefcfcea64ba83c4d4c732fcb9";
   535	
   536	            string decrypted = Crypto.DecryptCredentialBundle(credentialBundle, embeddedKey);
   537	            decrypted.Should().Be(expectedSenderPrivateKey);
   538	        }
   539	
   540	        [Fact]
   541	        public void UncompressRawPublicKey_UpstreamInvalidPrefixVector()
   542	        {
   543	            // From upstream tests/__tests__/crypto-test.ts:243-250
   544	            byte[] invalidPrefix = Encoding.Uint8ArrayFromHexString(
   545	                "77c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5");
   546	            Action act = () => Crypto.UncompressRawPublicKey(invalidPrefix);
   547	            act.Should().Throw<ArgumentException>()
   548	               .WithMessage("failed to uncompress raw public key: invalid prefix");
   549	        }
   550	
   551	        [Fact]
   552	        public void CompressRawPublicKey_EmptyInput_ReturnsEmpty()
   553	        {
   554	            // Upstream behavior: empty Uint8Array slice produces empty result.
   555	            Crypto.CompressRawPublicKey(Array.Empty<byte>()).Should().BeEmpty();
   556	        }
   557	
   558	        [Fact]
   559	        public void HpkeEncrypt_NullPlainTextBuf_Throws()
   560	        {

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '560,760p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   560	        {
   561	            // Upstream throws when plainTextBuf is absent (wrapped via try/catch).
   562	            var recv = Crypto.GenerateP256KeyPair();
   563	            byte[] recvPubUncompressed = Encoding.Uint8ArrayFromHexString(recv.PublicKeyUncompressed);
   564	            Action act = () => Crypto.HpkeEncrypt(new Crypto.HpkeEncryptParams
   565	            {
   566	                PlainTextBuf = null,
   567	                TargetKeyBuf = recvPubUncompressed,
   568	            });
   569	            act.Should().Throw<InvalidOperationException>()
   570	               .WithMessage("Unable to perform hpkeEncrypt:*");
   571	        }
   572	
   573	        [Fact]
   574	        public void DecryptCredentialBundle_BundleTooSmall_Throws()
   575	        {
   576	            // Base58Check of a small (<33 byte) payload should still decode (with valid checksum)
   577	            // but trigger the size guard. Use Base58Check encode so DecryptCredentialBundle's
   578	            // bs58check decode succeeds, then it should fail on the size check.
   579	            string tiny = Encoding.Base58CheckEncode(new byte[] { 0x01, 0x02, 0x03 });
   580	            Action act = () => Crypto.DecryptCredentialBundle(tiny, new string('1', 64));
   581	            act.Should().Throw<InvalidOperationException>()
   582	               .WithMessage("*Error decrypting bundle*");
   583	        }
   584	
   585	        [Fact]
   586	        public void DecryptCredentialBundle_RawBase58_Rejected()
   587	        {
   588	            // Upstream uses bs58check.decode exclusively. A plain base58 input
   589	            // (no checksum) must NOT be accepted.
   590	            string rawBs58 = Encoding.Base58Encode(new byte[40]);
   591	            Action act = () => Crypto.DecryptCredentialBundle(rawBs58, new string('1', 64));
   592	            act.Should().Throw<InvalidOperationException>();
   593	        }
   594	
   595	        [Fact]
   596	        public void EncryptPrivateKeyToBundle_MissingArgs_Throws()
   597	        {
   598	            Action act = () => Crypto.EncryptPrivateKeyToBundle(new Crypto.EncryptPrivateKeyToBundleParams());
   599	            act.Should().Throw<ArgumentException>();
   600	        }
   601	
   602	        [Fact]
   603	        public void DecryptExportBundle_MissingArgs_Throws()
   604	        {
   605	            Action act = () => Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams());
   606	            act.Should().Throw<ArgumentException>();
   607	        }
   608	
   609	        // ============================================================
   610	        // Helpers
   611	        // ============================================================
   612	
   613	        private static byte[] HexToBytes(string hex)
   614	        {
   615	            return Encoding.Uint8ArrayFromHexString(hex);
   616	        }
   617	
   618	        private static string BytesToHex(byte[] bytes)
   619	        {
   620	            return Encoding.Uint8ArrayToHexString(bytes);
   621	        }
   622	    }
   623	}

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts | sed -n '160,270p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   160	    expect(publicKey).toHaveLength(33);
   161	  });
   162	
   163	  test("generateP256KeyPair - generates a valid key pair", () => {
   164	    const keyPair = generateP256KeyPair();
   165	    expect(keyPair.privateKey).toBeTruthy();
   166	    expect(keyPair.publicKey).toBeTruthy();
   167	    expect(keyPair.publicKeyUncompressed).toBeTruthy();
   168	    expect(keyPair.privateKey).not.toEqual(keyPair.publicKey);
   169	    expect(keyPair.publicKey).not.toEqual(keyPair.publicKeyUncompressed);
   170	  });
   171	
   172	  test("compressRawPublicKey - returns a valid value", () => {
   173	    const { publicKey, publicKeyUncompressed } = generateP256KeyPair();
   174	    expect(
   175	      compressRawPublicKey(uint8ArrayFromHexString(publicKeyUncompressed)),
   176	    ).toEqual(uint8ArrayFromHexString(publicKey));
   177	  });
   178	
   179	  test("decryptCredentialBundle - successfully decrypts a credential bundle", () => {
   180	    const decryptedData = decryptCredentialBundle(
   181	      mockCredentialBundle,
   182	      mockPrivateKey,
   183	    );
   184	    expect(decryptedData).toBe(mockSenderPrivateKey);
   185	  });
   186	
   187	  test("extractPrivateKeyFromPKCS8Bytes", () => {
   188	    const pkcs8PrivateKeyHex =
   189	      "308187020100301306072a8648ce3d020106082a8648ce3d030107046d306b020101042001d95d256f744b2a855fe2036ec1074c726445f1382f53580a17ce3296cc2deca1440342000440fa0a112351e0f5cdcc3edad914e7e3b911d3e83874d4ef55ff5639f4a3633e65087a8499c46a77f8e68c937203d85e6d38ade95d755a6cf88fa101091d5983";
   190	    const expectedRawPrivateKeyHex =
   191	      "01d95d256f744b2a855fe2036ec1074c726445f1382f53580a17ce3296cc2dec";
   192	    expect(
   193	      extractPrivateKeyFromPKCS8Bytes(
   194	        uint8ArrayFromHexString(pkcs8PrivateKeyHex),
   195	      ),
   196	    ).toEqual(uint8ArrayFromHexString(expectedRawPrivateKeyHex));
   197	  });
   198	
   199	  test("verifyRequestStamp", async () => {
   200	    const { publicKey: apiPublicKey, privateKey: apiPrivateKey } =
   201	      generateP256KeyPair();
   202	    const apiKeyStamper = new ApiKeyStamper({
   203	      apiPublicKey,
   204	      apiPrivateKey,
   205	    });
   206	
   207	    const turnkeyClient = new TurnkeyClient(
   208	      {
   209	        baseUrl: "https://api.turnkey.com",
   210	      },
   211	      apiKeyStamper,
   212	    );
   213	
   214	    const stampedRequest = await turnkeyClient.stampGetWhoami({
   215	      organizationId: "00000000-00000000-00000000-00000000",
   216	    });
   217	
   218	    const stampContents = stampedRequest.stamp.stampHeaderValue;
   219	    const decodedStampContents = atob(stampContents);
   220	    const parsedStampContents = JSON.parse(decodedStampContents);
   221	    const signature = parsedStampContents.signature;
   222	
   223	    const verified = await verifyStampSignature(
   224	      apiPublicKey,
   225	      signature,
   226	      stampedRequest.body,
   227	    );
   228	
   229	    expect(verified).toEqual(true);
   230	  });
   231	
   232	  describe("uncompressRawPublicKey", () => {
   233	    test("happy path", async () => {
   234	      const keypair = generateP256KeyPair();
   235	      const uncompressedPublicKey = uncompressRawPublicKey(
   236	        uint8ArrayFromHexString(keypair.publicKey),
   237	      );
   238	      expect(uncompressedPublicKey.length).toEqual(65);
   239	    });
   240	
   241	    test("invalid prefix", async () => {
   242	      const invalidPrefix = uint8ArrayFromHexString(
   243	        "77c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5",
   244	      );
   245	
   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
   247	        "failed to uncompress raw public key: invalid prefix",
   248	      );
   249	    });
   250	
   251	    test("invalid length", async () => {
   252	      const keypair = generateP256KeyPair();
   253	
   254	      expect(() =>
   255	        uncompressRawPublicKey(
   256	          uint8ArrayFromHexString(keypair.publicKey + keypair.publicKey),
   257	        ),
   258	      ).toThrow("failed to uncompress raw public key: invalid length");
   259	    });
   260	  });
   261	
   262	  describe("Valid DER signatures", () => {
   263	    test("should parse a simple DER signature with short-form length", () => {
   264	      // Create a signature with 32-byte r and s values
   265	      const rValue = new Array(32).fill(0x01);
   266	      const sValue = new Array(32).fill(0x02);
   267	      const totalLength = 2 + 32 + 2 + 32; // 2 bytes for each INTEGER header + values
   268	
   269	      const derHex = createDerSignature([totalLength], rValue, sValue);
   270	

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt | sed -n '1,20p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
     2	75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
     3	2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
     4	d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz

exec
/bin/zsh -lc "rg -n \"HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|Hpke|HKDF|Hkdf|GeneratePublicKey|Ed25519PrivateKeyParameters|ECDH|ECDSA|AesGcm|GcmBlockCipher|BigInteger|ECPoint\" src/CryptoConstants.cs src/Crypto.cs src/*.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/TurnkeyJsonContext.cs:42:    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
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
src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
src/ApiKeyStamper.cs:10://                                         (P-256 ECDSA via @noble/curves,
src/ApiKeyStamper.cs:27://   - SHA-256(content) and deterministic ECDSA (RFC 6979) are used; low-S is
src/ApiKeyStamper.cs:46:    /// <c>purejs</c> runtime (deterministic ECDSA via RFC 6979 + low-S).
src/ApiKeyStamper.cs:150:        /// it with deterministic ECDSA (RFC 6979 + low-S) returning the
src/ApiKeyStamper.cs:182:            var privateKeyScalar = new BigInteger(1, privateKeyBytes);
src/ApiKeyStamper.cs:195:            BigInteger[] signature = signer.GenerateSignature(hash);
src/ApiKeyStamper.cs:196:            BigInteger r = signature[0];
src/ApiKeyStamper.cs:197:            BigInteger s = signature[1];
src/ApiKeyStamper.cs:207:            BigInteger halfN = domainParams.N.ShiftRight(1);
src/ApiKeyStamper.cs:221:        private static string EncodeDerSignatureHex(BigInteger r, BigInteger s)
src/ApiKeyStamper.cs:239:                    // Long-form length (1 octet count). For ECDSA P-256
src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:35://   - BouncyCastle 2.5.0 wraps ECDSA / ECDH / AES-GCM / SHA-256 / HMAC /
src/Crypto.cs:36://     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
src/Crypto.cs:90:            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
src/Crypto.cs:100:            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
src/Crypto.cs:162:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
src/Crypto.cs:164:                if (p.CompareTo(BigInteger.Zero) <= 0)
src/Crypto.cs:182:                    var q = p.Add(BigInteger.One).ShiftRight(2);
src/Crypto.cs:197:        #region HKDF (port of @noble/hashes/hkdf)
src/Crypto.cs:200:        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
src/Crypto.cs:203:        public static class Hkdf
src/Crypto.cs:208:            /// HKDF Extract. RFC 5869 §2.2.
src/Crypto.cs:223:            /// HKDF Expand. RFC 5869 §2.3.
src/Crypto.cs:271:        public class HpkeDecryptParams
src/Crypto.cs:279:        public class HpkeEncryptParams
src/Crypto.cs:332:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:336:        public class HpkeBundlePayload
src/Crypto.cs:398:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:405:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
src/Crypto.cs:435:                return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
src/Crypto.cs:445:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:451:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
src/Crypto.cs:457:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
src/Crypto.cs:486:                var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
src/Crypto.cs:563:            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);
src/Crypto.cs:565:            BigInteger p, a, b;
src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:574:                p = new BigInteger(
src/Crypto.cs:576:                a = BigInteger.ValueOf(0);
src/Crypto.cs:577:                b = BigInteger.ValueOf(7);
src/Crypto.cs:613:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:615:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:639:            var payload = new HpkeBundlePayload
src/Crypto.cs:644:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:682:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:710:            //   5. hpkeEncrypt + formatHpkeBuf
src/Crypto.cs:769:            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
src/Crypto.cs:774:            return FormatHpkeBuf(privateKeyBundle);
src/Crypto.cs:838:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:854:                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
src/Crypto.cs:855:                    var publicKeyBytes = ed25519PrivKey.GeneratePublicKey().GetEncoded();
src/Crypto.cs:878:        /// Verify the ECDSA signature on a Turnkey session JWT.
src/Crypto.cs:936:            var agreement = new ECDHBasicAgreement();
src/Crypto.cs:955:        private static (BigInteger Scalar, ECDomainParameters DomainParams)
src/Crypto.cs:971:            var d = new BigInteger(1, privateKey);
src/Crypto.cs:1010:            var prk = Hkdf.Extract(sharedSecret, ikm);
src/Crypto.cs:1011:            return Hkdf.Expand(prk, info, len);
src/Crypto.cs:1014:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:1016:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:1025:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:1027:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:1063:                var r = new BigInteger(1, signatureRaw, 0, 32);
src/Crypto.cs:1064:                var s = new BigInteger(1, signatureRaw, 32, 32);
src/Crypto.cs:1067:                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
src/Crypto.cs:1081:        /// when the underlying ECDSA verify fails. Throws on mismatched
src/Crypto.cs:1119:                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:35://   - BouncyCastle 2.5.0 wraps ECDSA / ECDH / AES-GCM / SHA-256 / HMAC /
src/Crypto.cs:36://     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
src/Crypto.cs:90:            /// <summary>Pre-computed HKDF expand "info" for the 32-byte AES key.</summary>
src/Crypto.cs:100:            /// <summary>Pre-computed HKDF expand "info" for the 12-byte AES-GCM IV.</summary>
src/Crypto.cs:162:            public static BigInteger ModSqrt(BigInteger x, BigInteger p)
src/Crypto.cs:164:                if (p.CompareTo(BigInteger.Zero) <= 0)
src/Crypto.cs:182:                    var q = p.Add(BigInteger.One).ShiftRight(2);
src/Crypto.cs:197:        #region HKDF (port of @noble/hashes/hkdf)
src/Crypto.cs:200:        /// RFC 5869 HKDF-HMAC-SHA256. Equivalent to <c>@noble/hashes/hkdf</c>
src/Crypto.cs:203:        public static class Hkdf
src/Crypto.cs:208:            /// HKDF Extract. RFC 5869 §2.2.
src/Crypto.cs:223:            /// HKDF Expand. RFC 5869 §2.3.
src/Crypto.cs:271:        public class HpkeDecryptParams
src/Crypto.cs:279:        public class HpkeEncryptParams
src/Crypto.cs:332:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:336:        public class HpkeBundlePayload
src/Crypto.cs:398:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:405:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
src/Crypto.cs:435:                return AesGcmDecrypt(ciphertextBuf, key, iv, aad);
src/Crypto.cs:445:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:451:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
src/Crypto.cs:457:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
src/Crypto.cs:486:                var encryptedData = AesGcmEncrypt(plainTextBuf, key, iv, aad);
src/Crypto.cs:563:            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);
src/Crypto.cs:565:            BigInteger p, a, b;
src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:574:                p = new BigInteger(
src/Crypto.cs:576:                a = BigInteger.ValueOf(0);
src/Crypto.cs:577:                b = BigInteger.ValueOf(7);
src/Crypto.cs:613:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:615:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:639:            var payload = new HpkeBundlePayload
src/Crypto.cs:644:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:682:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:710:            //   5. hpkeEncrypt + formatHpkeBuf
src/Crypto.cs:769:            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
src/Crypto.cs:774:            return FormatHpkeBuf(privateKeyBundle);
src/Crypto.cs:838:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:854:                    var ed25519PrivKey = new Ed25519PrivateKeyParameters(decryptedData, 0);
src/Crypto.cs:855:                    var publicKeyBytes = ed25519PrivKey.GeneratePublicKey().GetEncoded();
src/Crypto.cs:878:        /// Verify the ECDSA signature on a Turnkey session JWT.
src/Crypto.cs:936:            var agreement = new ECDHBasicAgreement();
src/Crypto.cs:955:        private static (BigInteger Scalar, ECDomainParameters DomainParams)
src/Crypto.cs:971:            var d = new BigInteger(1, privateKey);
src/Crypto.cs:1010:            var prk = Hkdf.Extract(sharedSecret, ikm);
src/Crypto.cs:1011:            return Hkdf.Expand(prk, info, len);
src/Crypto.cs:1014:        private static byte[] AesGcmDecrypt(byte[] encryptedData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:1016:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:1025:        private static byte[] AesGcmEncrypt(byte[] plainData, byte[] key, byte[] iv, byte[] aad)
src/Crypto.cs:1027:            var cipher = new GcmBlockCipher(new AesEngine());
src/Crypto.cs:1063:                var r = new BigInteger(1, signatureRaw, 0, 32);
src/Crypto.cs:1064:                var s = new BigInteger(1, signatureRaw, 32, 32);
src/Crypto.cs:1067:                var signer = SignerUtilities.GetSigner("NONEwithECDSA");
src/Crypto.cs:1081:        /// when the underlying ECDSA verify fails. Throws on mismatched
src/Crypto.cs:1119:                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");

exec
/bin/zsh -lc "git log --format='%H %ad %s' --date=iso -- src/CryptoConstants.cs | sed -n '1,8p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 1466ms:
2026-05-23 10:27:31.990 xcodebuild[38351:106248935]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:27:32.113 xcodebuild[38351:106248933] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-3qWxYmPW' (errno=Operation not permitted)
2026-05-23 10:27:32.488 xcodebuild[38362:106248964]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:27:32.617 xcodebuild[38362:106248963] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
388a5fc768fd1215928d29f338ef069acd2eb60c 2026-05-23 08:55:27 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json | sed -n '1,50p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba tests/ApiKeyStamperTests.cs | sed -n '130,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   130	            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
   131	            string a = stamper.SignWithApiKey("the same content");
   132	            string b = stamper.SignWithApiKey("the same content");
   133	            a.Should().Be(b);
   134	        }
   135	
   136	        [Fact]
   137	        public void SignWithApiKey_LowSEnforced()
   138	        {
   139	            // For RFC 6979 + low-S: the s value's high bit (top byte) must be
   140	            // strictly less than (n / 2 + 1). Verify by parsing the DER and
   141	            // checking the s integer is in the lower half of [1, n-1].
   142	            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
   143	            for (int i = 0; i < 8; i++)
   144	            {
   145	                string sigHex = stamper.SignWithApiKey("content_" + i);
   146	                var (r, s) = ParseDerEcdsa(HexToBytes(sigHex));
   147	
   148	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   149	                var halfN = curve.N.ShiftRight(1);
   150	                s.CompareTo(halfN).Should().BeLessOrEqualTo(0,
   151	                    "low-S requires s to be in (0, n/2]");
   152	                r.SignValue.Should().BeGreaterThan(0);
   153	                s.SignValue.Should().BeGreaterThan(0);
   154	            }
   155	        }
   156	
   157	        [Fact]
   158	        public void SignWithApiKey_WrongPublicKey_Throws()
   159	        {
   160	            // Mismatched public key (any other valid P-256 compressed key).
   161	            var other = Crypto.GenerateP256KeyPair();
   162	            Action act = () =>
   163	            {
   164	                var stamper = new ApiKeyStamper(other.PublicKey, FixturePrivateKey);
   165	                stamper.SignWithApiKey("payload");
   166	            };
   167	            act.Should().Throw<InvalidOperationException>()
   168	               .WithMessage("Bad API key. Expected to get public key*");
   169	        }
   170	
   171	        [Fact]
   172	        public void Stamp_HeaderConstantsAndScheme()
   173	        {
   174	            ApiKeyStamper.StampHeaderName.Should().Be("X-Stamp");
   175	            ApiKeyStamper.SignatureScheme.Should().Be("SIGNATURE_SCHEME_TK_API_P256");
   176	        }
   177	
   178	        [Fact]
   179	        public void StampHeaderValue_IsAsciiUrlSafe()
   180	        {
   181	            var stamper = new ApiKeyStamper(FixturePublicKey, FixturePrivateKey);
   182	            var result = stamper.Stamp("hello");
   183	            // base64url alphabet: A-Z a-z 0-9 - _
   184	            foreach (char c in result.StampHeaderValue)
   185	            {
   186	                bool ok = (c >= 'A' && c <= 'Z')
   187	                          || (c >= 'a' && c <= 'z')
   188	                          || (c >= '0' && c <= '9')
   189	                          || c == '-' || c == '_';
   190	                ok.Should().BeTrue($"char '{c}' is not in base64url alphabet");
   191	            }
   192	        }
   193	
   194	        // ============================================================
   195	        // Helpers
   196	        // ============================================================
   197	
   198	        private static byte[] HexToBytes(string hex) => Encoding.Uint8ArrayFromHexString(hex);
   199	
   200	        private static void AssertSignatureVerifies(string compressedPubHex, string content, byte[] derSignature)
   201	        {
   202	            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(content);
   203	
   204	            var digest = DigestUtilities.GetDigest("SHA-256");
   205	            digest.BlockUpdate(payloadBytes, 0, payloadBytes.Length);
   206	            var hash = new byte[digest.GetDigestSize()];
   207	            digest.DoFinal(hash, 0);
   208	
   209	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   210	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   211	
   212	            byte[] compressedPub = Encoding.Uint8ArrayFromHexString(compressedPubHex);
   213	            var point = curve.Curve.DecodePoint(compressedPub);
   214	            var publicKey = new ECPublicKeyParameters(point, domainParams);
   215	
   216	            var signer = SignerUtilities.GetSigner("NONEwithECDSA");
   217	            signer.Init(false, publicKey);
   218	            signer.BlockUpdate(hash, 0, hash.Length);
   219	            signer.VerifySignature(derSignature).Should().BeTrue();
   220	        }

 succeeded in 0ms:
     1	{
     2	  "_provenance": {
     3	    "level": "upstream-test-vectors",
     4	    "source_fixture": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{public,private}",
     5	    "source_test": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts",
     6	    "package": "@turnkey/api-key-stamper@0.5.0",
     7	    "tarball_sha256": "962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd",
     8	    "runtime_parity_target": "purejs",
     9	    "noble_curves_version": "^1.3.0",
    10	    "noble_curves_lowS_default": "lowS=true in v1.3.0 src/abstract/weierstrass.ts",
    11	    "note": "P-256 ECDSA signatures are NOT byte-identical across upstream runtimes when the impl uses random-k (nodecrypto.ts, webcrypto.ts). This C# port mirrors upstream's purejs runtime which uses RFC 6979 deterministic-k AND low-S (noble v1.3.0 default), so the C# signature bytes ARE reproducible against an upstream purejs runtime. All three upstream runtimes produce signatures that verify under the same public key + SHA-256(content), so end-to-end Turnkey API calls work regardless of runtime.",
    12	    "byte_level_assertion_strategy": "Exact Node-generated byte snapshots would require running the upstream npm package with its full node_modules; the tarball ships only built JS + declarations. Instead, ApiKeyStamperTests asserts wire-format equivalence by: (1) reproducing the upstream stamp-test.ts shape (decoded JSON key order = ['publicKey', 'scheme', 'signature']; scheme constant; publicKey equality); (2) crypto-verifying the DER ECDSA signature against SHA-256(content) under the upstream fixture public key; (3) asserting RFC 6979 determinism (same content -> same signature); (4) asserting low-S (s in (0, n/2]). This combination is sufficient to guarantee the stamp is accepted by Turnkey's backend, which is the wire-format goal."
    13	  },
    14	  "apiKeyFixture": {
    15	    "privateKeyHex": "487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c",
    16	    "publicKeyHex":  "02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316"
    17	  },
    18	  "stampShape": {
    19	    "_source_line": "ts-source/__tests__/stamp-test.ts:14-37",
    20	    "stampHeaderName": "X-Stamp",
    21	    "decodedKeysInOrder": ["publicKey", "scheme", "signature"],
    22	    "scheme": "SIGNATURE_SCHEME_TK_API_P256",
    23	    "stamp_test_message": "hello from TKHQ!",
    24	    "stamp_test_assertion": "Decode base64url -> JSON.parse; keys equal ['publicKey', 'scheme', 'signature']; publicKey == fixture.publicKey; scheme == SIGNATURE_SCHEME_TK_API_P256; signature verifies as DER ECDSA against SHA-256(message) under the fixture public key."
    25	  }
    26	}

exec
/bin/zsh -lc "nl -ba tests/PublicApi.expected.txt | sed -n '116,130p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   116	        public static class Math
   117	        {
   118	            public static Org.BouncyCastle.Math.BigInteger ModSqrt(Org.BouncyCastle.Math.BigInteger x, Org.BouncyCastle.Math.BigInteger p) { }
   119	        }
   120	    }
   121	    public static class CryptoConstants
   122	    {
   123	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
   124	        public const string CURVE_NAME = "secp256r1";
   125	        public const string P256_A_OFFSET = "3";
   126	        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
   127	        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
   128	    }
   129	    public static class Encoding
   130	    {

exec
/bin/zsh -lc "nl -ba /Users/takeshi/.codex/memories/MEMORY.md | sed -n '273,312p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   273	# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
   274	
   275	scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.
   276	applies_to: cwd=/Users/takeshi/.codex/worktrees/1605/peak; reuse_rule=reuse for future `peak` plan/PR review work when the target is PR #330 or a similar docs-plan review in the Peak repo; treat plan conclusions as PR-specific if the file or branch has materially changed
   277	
   278	## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted
   279	
   280	### rollout_summary_files
   281	
   282	- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
   283	- extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md, updated_at=2026-05-22T04:47:00+00:00, thread_id=None, chronicle synthetic task for the initial PR-review window)
   284	- extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md, updated_at=2026-05-22T06:07:00+00:00, thread_id=None, chronicle synthetic task showing the posted inline review state)
   285	
   286	### keywords
   287	
   288	- PR #330, plans/plans-peak-sdk-csharp.md, docs(plans): add peak-sdk-csharp migration plan, review the new plan to implement csharp sdk, inline comments, TurnkeyJsonContext, submodule init, csharp-publish.yml
   289	
   290	## Task 2: Re-check fixes on PR #330, resolve fixed threads, add only remaining issues
   291	
   292	### rollout_summary_files
   293	
   294	- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, latest verified head `a1adb9a6ef183151430b7b106e3c6bafce649e5e`)
   295	- extensions/chronicle/resources/2026-05-22T06-17-00-qyoC-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-17-00-qyoC-10min-memory-summary.md, updated_at=2026-05-22T06:17:00+00:00, thread_id=None, chronicle synthetic task covering GitHub permission modal and remaining inline comments)
   296	- extensions/chronicle/resources/2026-05-22T07-03-00-Iktn-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T07-03-00-Iktn-10min-memory-summary.md, updated_at=2026-05-22T07:03:00+00:00, thread_id=None, chronicle synthetic task showing the draft PR browser state after fixes)
   297	
   298	### keywords
   299	
   300	- resolve可能であればresolveし、追加の指摘があれば指摘追加してください, verify latest head, resolve thread, locally packed .nupkg, feature/plan-peak-sdk-csharp, reviewer waiting
   301	
   302	## User preferences
   303	
   304	- when reviewing a plan/PR, the user asked: "review the new plan to implement csharp sdk" -> default to a PR-style review with concrete findings, not abstract advice [Task 1]
   305	- when the user followed up with: "post inline comments to the PR" -> prefer file/line-anchored review feedback on the actual changed file, especially for doc-only plan PRs [Task 1]
   306	- when fixes arrive, the user asked twice: "修正が届いているので、修正確認して、resolve可能であればresolveし、追加の指摘があれば指摘追加してください" -> verify the latest head, resolve what is fixed, and add comments only for genuinely remaining issues instead of rehashing resolved points [Task 2]
   307	
   308	## Reusable knowledge
   309	
   310	- For `peak` doc-plan reviews, read the plan plus nearby repo guidance before commenting; `docs/development-workflow.md`, `docs/sdk-coding-conventions.md`, `docs/sdk-naming-conventions.md`, `docs/sdk-error-handling.md`, and `docs/peak-sdk-unity-internal/README.md` were the useful mismatch detectors [Task 1]
   311	- The high-signal issues in PR #330 were internal-consistency problems, not prose quality: storage placement, PR1 CI scope, IL2CPP source-generation concretization, submodule prerequisites, publish workflow separation, and audit-trail placement [Task 1]
   312	- The stable final plan shape after the verify-resolve loop was PR1 build/test only, PR5 locally packed `.nupkg`, PR7 independent `csharp-publish.yml`, and PR4.5 splitting secure-storage adapters between core and Unity packages [Task 2]

exec
/bin/zsh -lc "rg -n \"\\b(public|internal)\\b|\\(\" src/CryptoConstants.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
15://       p = BigInt("0xffff...ffffffff")   (P-256 prime)
16://       b = BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")
17://       a = p - BigInt(3)
32:    public static class CryptoConstants
38:        public const string CURVE_NAME = "secp256r1";
41:        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
48:        /// NIST P-256 prime field modulus (p), decimal form.
55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
58:        public const string P256_P =
64:        /// <c>BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b")</c>.
66:        public const string P256_B =
71:        /// Upstream inlines <c>const a = p - BigInt(3)</c>.
73:        public const string P256_A_OFFSET = "3";

exec
/bin/zsh -lc 'rg -n "turnkey-crypto-vectors|Fixtures/crypto|CryptoTests|turnkey-api-key-stamper-vectors" tests/*.cs tests/**/*.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/CryptoTests.cs:1:// CryptoTests.cs — xunit tests for src/Crypto.cs.
tests/CryptoTests.cs:24:    public class CryptoTests
tests/CryptoTests.cs:1:// CryptoTests.cs — xunit tests for src/Crypto.cs.
tests/CryptoTests.cs:24:    public class CryptoTests

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts | sed -n '1,160p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '540,635p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   540	        /// <summary>
   541	        /// Uncompress a 33-byte compressed public key into 65 bytes.
   542	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
   543	        /// </summary>
   544	        /// <param name="rawPublicKey">33-byte compressed key starting with 0x02 or 0x03.</param>
   545	        /// <param name="curve">Curve identifier; defaults to <see cref="Curve.P256"/>.</param>
   546	        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Curve curve = Curve.P256)
   547	        {
   548	            if (rawPublicKey == null) throw new ArgumentNullException(nameof(rawPublicKey));
   549	            if (rawPublicKey.Length != 33)
   550	            {
   551	                throw new ArgumentException("failed to uncompress raw public key: invalid length");
   552	            }
   553	            if (rawPublicKey[0] != 0x02 && rawPublicKey[0] != 0x03)
   554	            {
   555	                throw new ArgumentException("failed to uncompress raw public key: invalid prefix");
   556	            }
   557	
   558	            bool lsb = rawPublicKey[0] == 0x03;
   559	
   560	            // x = BigInt("0x" + uint8ArrayToHexString(rawPublicKey.subarray(1)));
   561	            var xBytes = new byte[32];
   562	            Array.Copy(rawPublicKey, 1, xBytes, 0, 32);
   563	            var x = new BigInteger(Encoding.Uint8ArrayToHexString(xBytes), 16);
   564	
   565	            BigInteger p, a, b;
   566	            if (curve == Curve.P256)
   567	            {
   568	                p = new BigInteger(CryptoConstants.P256_P);
   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
   571	            }
   572	            else // Secp256k1
   573	            {
   574	                p = new BigInteger(
   575	                    "fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f", 16);
   576	                a = BigInteger.ValueOf(0);
   577	                b = BigInteger.ValueOf(7);
   578	            }
   579	
   580	            // rhs = ((x * x + a) * x + b) % p
   581	            var x2 = x.Multiply(x).Mod(p);
   582	            var x2PlusA = x2.Add(a).Mod(p);
   583	            var rhs = x2PlusA.Multiply(x).Add(b).Mod(p);
   584	
   585	            var y = Math.ModSqrt(rhs, p);
   586	            if (lsb != y.TestBit(0))
   587	            {
   588	                y = p.Subtract(y).Mod(p);
   589	            }
   590	
   591	            // Defensive range check matches upstream "throw" intent for out-of-range output.
   592	            if (x.SignValue < 0 || x.CompareTo(p) >= 0)
   593	            {
   594	                throw new InvalidOperationException("x is out of range");
   595	            }
   596	            if (y.SignValue < 0 || y.CompareTo(p) >= 0)
   597	            {
   598	                throw new InvalidOperationException("y is out of range");
   599	            }
   600	
   601	            var uncompressed = new byte[65];
   602	            uncompressed[0] = 0x04;
   603	            var xHexOut = x.ToString(16).ToLowerInvariant().PadLeft(64, '0');
   604	            var yHexOut = y.ToString(16).ToLowerInvariant().PadLeft(64, '0');
   605	            Array.Copy(Encoding.Uint8ArrayFromHexString(xHexOut), 0, uncompressed, 1, 32);
   606	            Array.Copy(Encoding.Uint8ArrayFromHexString(yHexOut), 0, uncompressed, 33, 32);
   607	            return uncompressed;
   608	        }
   609	
   610	        /// <summary>
   611	        /// Encode the HPKE result <c>compressed(senderPub) || ciphertext</c>
   612	        /// as the Turnkey import-bundle JSON envelope.
   613	        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
   614	        /// </summary>
   615	        public static string FormatHpkeBuf(byte[] encryptedBuf)
   616	        {
   617	            if (encryptedBuf == null) throw new ArgumentNullException(nameof(encryptedBuf));
   618	            // Upstream: encappedPublic := encryptedBuf.slice(0,33);
   619	            //           ciphertext     := encryptedBuf.slice(33);
   620	            // Upstream then uncompressRawPublicKey() the compressed part. If
   621	            // the input is shorter than 33 bytes that uncompress call would
   622	            // throw "invalid length". Mirror the same behavior: pass the
   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
   624	            // length itself.
   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
   626	            int compressedTake = System.Math.Min(compressedSize, encryptedBuf.Length);
   627	            var compressedEncappedPublic = new byte[compressedTake];
   628	            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, compressedTake);
   629	
   630	            var encappedPublicUncompressed = UncompressRawPublicKey(compressedEncappedPublic);
   631	
   632	            int cipherLen = System.Math.Max(0, encryptedBuf.Length - compressedSize);
   633	            var ciphertext = new byte[cipherLen];
   634	            if (cipherLen > 0)
   635	            {

exec
/bin/zsh -lc "nl -ba src/Crypto.cs | sed -n '950,975p;1048,1060p;1108,1122p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   950	        /// <summary>
   951	        /// Validates a P-256 private key against noble's invariants: exactly
   952	        /// 32 bytes and scalar in [1, n - 1]. Returns the parsed scalar and
   953	        /// curve domain parameters for reuse.
   954	        /// </summary>
   955	        private static (BigInteger Scalar, ECDomainParameters DomainParams)
   956	            ValidateAndDeserializeP256PrivateKey(byte[] privateKey)
   957	        {
   958	            if (privateKey == null)
   959	            {
   960	                throw new ArgumentNullException(nameof(privateKey));
   961	            }
   962	            if (privateKey.Length != 32)
   963	            {
   964	                throw new ArgumentException(
   965	                    "invalid P-256 private key: expected 32 bytes, got " + privateKey.Length);
   966	            }
   967	
   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
   969	            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
   970	
   971	            var d = new BigInteger(1, privateKey);
   972	            if (d.SignValue == 0 || d.CompareTo(domainParams.N) >= 0)
   973	            {
   974	                throw new ArgumentException(
   975	                    "invalid P-256 private key: scalar must be in [1, n - 1]");
  1048	        {
  1049	            // Upstream noble p256.verify accepts r||s of length 64. Anything
  1050	            // else is an invalid signature; return false.
  1051	            if (signatureRaw.Length != 64)
  1052	            {
  1053	                return false;
  1054	            }
  1055	            try
  1056	            {
  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
  1058	                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
  1059	
  1060	                var point = curve.Curve.DecodePoint(publicKeyBytes);
  1108	            try
  1109	            {
  1110	                var publicKeyBytes = Encoding.Uint8ArrayFromHexString(expectedSignerPublicKey);
  1111	                var signatureBytes = Encoding.Uint8ArrayFromHexString(signatureHex!);
  1112	                var messageBytes = Encoding.Uint8ArrayFromHexString(signedDataHex!);
  1113	
  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
  1115	                var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
  1116	                var point = curve.Curve.DecodePoint(publicKeyBytes);
  1117	                var publicKeyParams = new ECPublicKeyParameters(point, domainParams);
  1118	
  1119	                var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
  1120	                signer.Init(false, publicKeyParams);
  1121	                signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
  1122	

codex
**A. Source Pin**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`

**B. Method Coverage**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| N/A - no public or internal methods declared (`src/CryptoConstants.cs:32`) | N/A - constants map to inline upstream values in `ts-source/crypto.ts:2`, `ts-source/crypto.ts:430`, `ts-source/crypto.ts:447-453` | REVIEWED | File contains only `public const` fields. Field parity is reviewed below. |

**C. Intentional Adaptations**

- `CURVE_NAME = "secp256r1"` replaces upstream implicit `p256` noble import: structural BouncyCastle registry name for the same NIST P-256 curve.
- `COMPRESSED_PUBLIC_KEY_SIZE = 33` hoists upstream literal `rawPublicKey.length !== 33`: same SEC1 compressed P-256 byte length.
- `P256_P` hoists upstream inline decimal BigInt: same prime field modulus, no byte change.
- `P256_B` stores upstream hex BigInt without `0x`: consumer parses radix 16, so same coefficient bytes.
- `P256_A_OFFSET = "3"` hoists upstream `p - BigInt(3)`: same derived `a` coefficient.
- Constants are public C# fields rather than TS inline locals: structural reuse only; behavior comes from consumers in `Crypto.cs`.

**D. D17 Enforcement Check**

N/A (not Crypto.cs).

**E. Logic Divergence Findings**

Zero entries.

Reviewed categories requested: algorithm step order, constants, error handling, byte ordering, leading-zero handling, padding, rounding/normalization, signature format, DTO shape, and JSON serialization. `CryptoConstants.cs` itself has no executable logic, DTOs, serialization, or signature handling. The constants match upstream P-256 length and domain parameters.

**F. Fixture Comparison Gate**

| Fixture | Exercises `CryptoConstants.cs` how | Pinned upstream provenance | C# assertion parity |
|---|---|---|---|
| `tests/Fixtures/crypto/turnkey-crypto-vectors.json` | Indirectly through `UncompressRawPublicKey`, HPKE bundle split size, and P-256 domain params | Provenance says `@turnkey/crypto@2.8.8`, same tarball sha256, source `ts-source/__tests__/crypto-test.ts` | `CryptoTests.cs` asserts the upstream credential bundle decrypts to `67ee...fcb9`, invalid prefix throws the same upstream message, and compressed/uncompressed key size/parity behavior matches Node-side tests. |
| `tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json` | Indirectly uses `CURVE_NAME` for P-256 verification helpers | Different pinned package: `@turnkey/api-key-stamper@0.5.0`, not the crypto tarball | Relevant only to `CURVE_NAME`; tests verify BouncyCastle `secp256r1` accepts the fixture public key and DER ECDSA signatures. |
| `tests/Fixtures/encoding/turnkey-encoding-vectors.json` | Does not exercise this file | N/A | N/A |

Gate result for fixtures that exercise this file: all checked fixture values match their pinned provenance. I did not re-run Node generation or `dotnet test` in this read-only sandbox; this is a static source/fixture/test assertion review.

**G. Unresolved Assumptions**

- I could not execute the test suite or regenerate Node outputs because the environment is read-only.
- `api-key-stamper` fixture coverage is indirect and belongs to a different pinned upstream package, so I did not treat it as part of the `@turnkey/crypto@2.8.8` byte-for-byte gate.

PASS criterion for this round: B has zero `NOT-REVIEWED` rows, D is N/A, E has zero entries, and F is satisfied by static fixture/source parity.


tokens used
113,180
**A. Source Pin**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`

**B. Method Coverage**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| N/A - no public or internal methods declared (`src/CryptoConstants.cs:32`) | N/A - constants map to inline upstream values in `ts-source/crypto.ts:2`, `ts-source/crypto.ts:430`, `ts-source/crypto.ts:447-453` | REVIEWED | File contains only `public const` fields. Field parity is reviewed below. |

**C. Intentional Adaptations**

- `CURVE_NAME = "secp256r1"` replaces upstream implicit `p256` noble import: structural BouncyCastle registry name for the same NIST P-256 curve.
- `COMPRESSED_PUBLIC_KEY_SIZE = 33` hoists upstream literal `rawPublicKey.length !== 33`: same SEC1 compressed P-256 byte length.
- `P256_P` hoists upstream inline decimal BigInt: same prime field modulus, no byte change.
- `P256_B` stores upstream hex BigInt without `0x`: consumer parses radix 16, so same coefficient bytes.
- `P256_A_OFFSET = "3"` hoists upstream `p - BigInt(3)`: same derived `a` coefficient.
- Constants are public C# fields rather than TS inline locals: structural reuse only; behavior comes from consumers in `Crypto.cs`.

**D. D17 Enforcement Check**

N/A (not Crypto.cs).

**E. Logic Divergence Findings**

Zero entries.

Reviewed categories requested: algorithm step order, constants, error handling, byte ordering, leading-zero handling, padding, rounding/normalization, signature format, DTO shape, and JSON serialization. `CryptoConstants.cs` itself has no executable logic, DTOs, serialization, or signature handling. The constants match upstream P-256 length and domain parameters.

**F. Fixture Comparison Gate**

| Fixture | Exercises `CryptoConstants.cs` how | Pinned upstream provenance | C# assertion parity |
|---|---|---|---|
| `tests/Fixtures/crypto/turnkey-crypto-vectors.json` | Indirectly through `UncompressRawPublicKey`, HPKE bundle split size, and P-256 domain params | Provenance says `@turnkey/crypto@2.8.8`, same tarball sha256, source `ts-source/__tests__/crypto-test.ts` | `CryptoTests.cs` asserts the upstream credential bundle decrypts to `67ee...fcb9`, invalid prefix throws the same upstream message, and compressed/uncompressed key size/parity behavior matches Node-side tests. |
| `tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json` | Indirectly uses `CURVE_NAME` for P-256 verification helpers | Different pinned package: `@turnkey/api-key-stamper@0.5.0`, not the crypto tarball | Relevant only to `CURVE_NAME`; tests verify BouncyCastle `secp256r1` accepts the fixture public key and DER ECDSA signatures. |
| `tests/Fixtures/encoding/turnkey-encoding-vectors.json` | Does not exercise this file | N/A | N/A |

Gate result for fixtures that exercise this file: all checked fixture values match their pinned provenance. I did not re-run Node generation or `dotnet test` in this read-only sandbox; this is a static source/fixture/test assertion review.

**G. Unresolved Assumptions**

- I could not execute the test suite or regenerate Node outputs because the environment is read-only.
- `api-key-stamper` fixture coverage is indirect and belongs to a different pinned upstream package, so I did not treat it as part of the `@turnkey/crypto@2.8.8` byte-for-byte gate.

PASS criterion for this round: B has zero `NOT-REVIEWED` rows, D is N/A, E has zero entries, and F is satisfied by static fixture/source parity.


