# Codex review — src/CryptoConstants.cs — round 1 — 20260523

C# SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
Upstream snapshot: `turnkey-crypto-2.8.8`

---

2026-05-23T01:23:18.483325Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e526d-f94a-7e90-9120-d36c55ece446
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 1 of 3 for this file.

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

2026-05-23T01:23:20.055210Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll treat this as a source-parity review, so I’m going to pin the exact upstream snapshot, enumerate every C# method, and trace each one to the TypeScript implementation before making any findings. I’m also loading the review skill and checking memory briefly for any prior C# SDK review context.
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
/bin/zsh -lc 'rg -n "turnkey|C# SDK|csharp|CryptoConstants|turnkey-sdk-csharp" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
The prior memory hit is only about review style for a C# SDK plan, not about this source file’s behavior. I’ll keep it in mind for rigor, but I’m going to base the actual parity result on the pinned files in this workspace.
exec
/bin/zsh -lc 'nl -ba src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc 'cat codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz

exec
/bin/zsh -lc 'rg --files codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8 tests/Fixtures src tests' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/PublicApi.expected.txt
tests/ApiKeyStamperTests.cs
tests/CryptoTests.cs
tests/Fixtures/README.md
src/CryptoConstants.cs
tests/Fixtures/README.md
src/Http.cs
src/Crypto.cs
src/turnkey-sdk-csharp.csproj
src/TurnkeyJsonContext.cs
src/Encoding.cs
src/ApiKeyStamper.cs
src/packages.lock.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/crypto/turnkey-crypto-vectors.json
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/LICENSE
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md
tests/PublicApiSnapshotTests.cs
tests/packages.lock.json
tests/HttpTests.cs
tests/EncodingTests.cs
tests/turnkey-sdk-csharp.Tests.csproj
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/index.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts
tests/Fixtures/crypto/turnkey-crypto-vectors.json
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/math.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/math.js
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs.map

exec
/bin/zsh -lc 'git status --short' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 1475ms:
2026-05-23 10:23:42.920 xcodebuild[35942:106237949]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:23:43.045 xcodebuild[35942:106237948] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-DhBwzcje' (errno=Operation not permitted)
2026-05-23 10:23:43.417 xcodebuild[35944:106237967]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:23:43.541 xcodebuild[35944:106237966] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc 'cat codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
{
  "name": "@turnkey/crypto",
  "version": "2.8.8",
  "main": "./dist/index.js",
  "module": "./dist/index.mjs",
  "exports": {
    ".": {
      "types": "./dist/index.d.ts",
      "import": "./dist/index.mjs",
      "require": "./dist/index.js",
      "default": "./dist/index.mjs"
    }
  },
  "types": "./dist/index.d.ts",
  "license": "Apache-2.0",
  "description": "Encryption, decryption, and key related utility functions",
  "author": {
    "name": "Turnkey",
    "url": "https://turnkey.com/"
  },
  "homepage": "https://github.com/tkhq/sdk",
  "bugs": {
    "url": "https://github.com/tkhq/sdk/issues"
  },
  "repository": {
    "type": "git",
    "url": "git+https://github.com/tkhq/sdk.git",
    "directory": "packages/crypto"
  },
  "files": [
    "dist/",
    "CHANGELOG.md",
    "README.md"
  ],
  "publishConfig": {
    "access": "public"
  },
  "engines": {
    "node": ">=18.0.0"
  },
  "dependencies": {
    "@noble/ciphers": "1.3.0",
    "@noble/curves": "1.9.0",
    "@noble/hashes": "1.8.0",
    "@peculiar/x509": "1.12.3",
    "borsh": "2.0.0",
    "cbor-js": "0.1.0",
    "@turnkey/encoding": "0.6.0",
    "@turnkey/sdk-types": "0.11.0"
  },
  "devDependencies": {
    "@types/cbor-js": "^0.1.1",
    "@types/jest": "^29.5.3",
    "jest": "29.7.0",
    "@turnkey/api-key-stamper": "0.5.0",
    "@turnkey/http": "3.16.0"
  },
  "scripts": {
    "build": "rollup -c",
    "clean": "rimraf ./dist ./.cache",
    "test": "jest",
    "typecheck": "tsc -p tsconfig.typecheck.json"
  }
}
exec
/bin/zsh -lc "rg -n \"uncompressRawPublicKey|compressRawPublicKey|compressedKey.length|BigInt\\(\\\"0xffffffff|BigInt\\(\\\"0x5ac|p - BigInt|33\" codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '520,760p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:23:  uncompressRawPublicKey,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:78:    if (bundleBytes.byteLength <= 33) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:80:        `Bundle size ${bundleBytes.byteLength} is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.`,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:84:    const compressedEncappedKeyBuf = bundleBytes.slice(0, 33);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:85:    const ciphertextBuf = bundleBytes.slice(33);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:86:    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:545:      targetKeyBuf: uncompressRawPublicKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:12:  125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34, 135, 39, 183, 10, 64, 33,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:13:  18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53, 41, 180, 122, 198, 166, 48,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:20:  135, 39, 183, 10, 64, 33, 18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:30:  "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:33:  "04d498aa87ac3bf982ac2b5dd9604d0074905cfbda5d62727c5a237b895e6749205e9f7cd566909c4387f6ca25c308445c60884b788560b785f4a96ac33702a469";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:39:  "02336ebd7e929ef64b87c776b72540255b4c7b41579a24b1e68fb060daa873f9f6";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:8:      "043144c138d522996833f9a5352b98da2ba669f1ab3f712b243a1993c89c7d682f2163c4c63c6625c483cd5e8714e9a2039e1ec05cebadf469a4adb7e721d16e19047b2846a313c6f3c543c23ae9779118c2411b28bd5d76b25572903c360240c841f4c4a3af4a297d770e9d8bceb5e7e5be31a8fe16e571b452e3de99ffcebcb2e1",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:10:      '{"type":"APP_PROOF_TYPE_ADDRESS_DERIVATION","timestampMs":"1758058763571","addressDerivationProof":{"organizationId":"3a2de333-972b-45bf-8a7a-83c4167b81e2","walletId":"ad8a06ec-c2f4-55f2-ac43-65991efbfdd1","derivationPath":"m/44\'/60\'/0\'/0/0","address":"0xb84b4730Cd81Bc82Ee6B1dE6c343Ebd7928138DC"}}',
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:17:      "043144c138d522996833f9a5352b98da2ba669f1ab3f712b243a1993c89c7d682f2163c4c63c6625c483cd5e8714e9a2039e1ec05cebadf469a4adb7e721d16e19047b2846a313c6f3c543c23ae9779118c2411b28bd5d76b25572903c360240c841f4c4a3af4a297d770e9d8bceb5e7e5be31a8fe16e571b452e3de99ffcebcb2e1",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:21:      "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",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:23:      "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",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:37:      "04ab52e052b94bc7f3badb1e0f3500d11062c9f44307bf76091b5c5f48cace9d35caba26028238e391f735c0a74996f98f69c593916aa5fcd6b3c7a56d33b5a31d0435d0354ae36981a010965d13fe6539827be12a17fcbb0fa35586c5b7732c4e8dc6a0efa81e762633f271d05d5440b2d4e97df48912d214c895bc42c7c61d076e",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:46:      "04ab52e052b94bc7f3badb1e0f3500d11062c9f44307bf76091b5c5f48cace9d35caba26028238e391f735c0a74996f98f69c593916aa5fcd6b3c7a56d33b5a31d0435d0354ae36981a010965d13fe6539827be12a17fcbb0fa35586c5b7732c4e8dc6a0efa81e762633f271d05d5440b2d4e97df48912d214c895bc42c7c61d076e",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:50:      "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",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/proof-tests.ts:52:      "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",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:10:  uncompressRawPublicKey,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:11:  compressRawPublicKey,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:26:  "20fa65df11f24833790ae283fc9a0c215eecbbc589549767977994dc69d05a56";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:34:    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:70:    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:77:      "6ab33bd6e4bdc73017233da0554f9616fe10ede5c3ce001e81b321d5a74199b7";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:113:      "data": "7b22656e6361707065645075626c6963223a2230343434313065633837653566653266666461313561313866613337376132316133633431633334373666383631333362343238306164373631303266343064356462326463353362343730303763636139336166666330613535316464353134333937643039373931636664393233306663613330343862313731663364363738222c2263697068657274657874223a22656662303538626633666634626534653232323330326266326636303738363062343237346232623031616339343536643362613638646135613235363236303030613839383262313465306261663061306465323966353434353461333739613362653664633364386339343938376131353638633764393566396663346239316265663232316165356562383432333361323833323131346431373962646664636631643066376164656231353766343131613439383430222c226f7267616e697a6174696f6e4964223a2266396133316336342d643630342d343265342d396265662d613737333039366166616437227d",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:115:      "enclaveQuorumPublic": "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569"
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:160:    expect(publicKey).toHaveLength(33);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:172:  test("compressRawPublicKey - returns a valid value", () => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:175:      compressRawPublicKey(uint8ArrayFromHexString(publicKeyUncompressed)),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:189:      "308187020100301306072a8648ce3d020106082a8648ce3d030107046d306b020101042001d95d256f744b2a855fe2036ec1074c726445f1382f53580a17ce3296cc2deca1440342000440fa0a112351e0f5cdcc3edad914e7e3b911d3e83874d4ef55ff5639f4a3633e65087a8499c46a77f8e68c937203d85e6d38ade95d755a6cf88fa101091d5983";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:232:  describe("uncompressRawPublicKey", () => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:235:      const uncompressedPublicKey = uncompressRawPublicKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:246:      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:255:        uncompressRawPublicKey(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:274:    test("should parse a DER signature with 33-byte integers (with leading zero)", () => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:278:      const totalLength = 2 + 33 + 2 + 33;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:357:        0x22, // length of INTEGER // invalid -- should be 32 or 33
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:377:        0x22, // length of INTEGER // invalid -- should be 32 or 33
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:391:        0x21, // length of INTEGER // 33 -- this is valid
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:392:        ...new Array(33).fill(0x01), // r -- this is invalid, as the first byte in a 33 byte sequence is a non-padding byte
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:411:        0x21, // length of INTEGER // 33 -- this is valid
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:412:        ...new Array(33).fill(0x02), // s -- this is invalid, as the first byte in a 33 byte sequence is a non-padding byte
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:141:    const compressedSenderBuf = compressRawPublicKey(senderPubBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:206:    const compressedSenderBuf = compressRawPublicKey(senderPubBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:287:  const compressedSenderBuf = encryptedBuf.slice(0, 33);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:288:  const encryptedData = encryptedBuf.slice(33);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:291:    uncompressRawPublicKey(compressedSenderBuf),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:362:    uncompressRawPublicKey(publicKey),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:406:export const compressRawPublicKey = (rawPublicKey: Uint8Array): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:426:export const uncompressRawPublicKey = (
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:430:  if (rawPublicKey.length !== 33) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:448:      "115792089210356248762697446949407573530086143415290314195533631308867097853951",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:453:    a = p - BigInt(3);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:534:  ret.set(new Uint8Array([0, len]), 0); // this isn’t an error, we’re starting at index 2 because the first two bytes should be 0. See <https://github.com/dajiaji/hpke-js/blob/1e7fb1372fbcdb6d06bf2f4fa27ff676329d633e/src/kdfs/hkdf.ts#L41> for reference.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:691:  if (rLength > 33) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:712:  if (sLength > 33) {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:814:  const sharedSecret = sharedSecretUncompressed.slice(1, 33);

exec
/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/math.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "rg -n \"CryptoConstants|CURVE_NAME|COMPRESSED_PUBLIC_KEY_SIZE|P256_P|P256_B|P256_A_OFFSET\" src tests codex-crypto-reviews -g '"'!upstream-snapshots/**/package/dist/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
src/CryptoConstants.cs:32:    public static class CryptoConstants
src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
src/CryptoConstants.cs:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
src/CryptoConstants.cs:58:        public const string P256_P =
src/CryptoConstants.cs:66:        public const string P256_B =
src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
tests/PublicApi.expected.txt:121:    public static class CryptoConstants
tests/PublicApi.expected.txt:123:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
tests/PublicApi.expected.txt:124:        public const string CURVE_NAME = "secp256r1";
tests/PublicApi.expected.txt:125:        public const string P256_A_OFFSET = "3";
tests/PublicApi.expected.txt:126:        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
tests/PublicApi.expected.txt:127:        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Http.cs-r3-20260523.md:3341:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/Http.cs-r3-20260523.md:8534:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/unity-source-pins.md:39:| src/CryptoConstants.cs      | packages/turnkey-sdk-unity/Runtime/UnityConstants.cs (rename) |
codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:693:src/CryptoConstants.cs
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:854:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1069:   565	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1070:   566	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1071:   567	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2706:   622	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3009:   925	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3110:  1026	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3167:  1083	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3303:    66	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3681:./src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3682:./src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3683:./src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3767:./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4399:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4417:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4418:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4419:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4424:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4475:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4486:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4501:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4745:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4756:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4757:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4758:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4773:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4779:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4783:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4809:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4820:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4821:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4822:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4837:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4843:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4847:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4873:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5398:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5826:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5940:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6295:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7564:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7572:565:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7573:566:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7574:567:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7582:925:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7592:1026:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7597:1083:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
codex-crypto-reviews/Crypto.cs-r1-20260523.md:739:   327	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:764:   352	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:921:   509	            if (rawPublicKey.Length != CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
codex-crypto-reviews/Crypto.cs-r1-20260523.md:941:   529	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:942:   530	            var b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:943:   531	            var a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r1-20260523.md:991:   579	            if (encryptedBuf.Length <= CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
codex-crypto-reviews/Crypto.cs-r1-20260523.md:996:   584	            var compressedEncappedPublic = new byte[CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
codex-crypto-reviews/Crypto.cs-r1-20260523.md:997:   585	            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:1001:   589	            var ciphertext = new byte[encryptedBuf.Length - CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
codex-crypto-reviews/Crypto.cs-r1-20260523.md:1002:   590	            Array.Copy(encryptedBuf, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE, ciphertext, 0, ciphertext.Length);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:1448:   621	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r1-20260523.md:1727:   900	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:1828:  1001	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:1864:  1037	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:2431:  1060	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/Crypto.cs-r1-20260523.md:3430:    66	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4647:/bin/zsh -lc "nl -ba src/CryptoConstants.cs | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4661:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4680:    32	    public static class CryptoConstants
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4686:    38	        public const string CURVE_NAME = "secp256r1";
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4693:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4706:    58	        public const string P256_P =
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4714:    66	        public const string P256_B =
codex-crypto-reviews/Crypto.cs-r1-20260523.md:4721:    73	        public const string P256_A_OFFSET = "3";
codex-crypto-reviews/Crypto.cs-r2-20260523.md:661:src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/Crypto.cs-r2-20260523.md:663:src/CryptoConstants.cs:32:    public static class CryptoConstants
codex-crypto-reviews/Crypto.cs-r2-20260523.md:664:src/CryptoConstants.cs:35:        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
codex-crypto-reviews/Crypto.cs-r2-20260523.md:665:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
codex-crypto-reviews/Crypto.cs-r2-20260523.md:666:src/CryptoConstants.cs:58:        public const string P256_P =
codex-crypto-reviews/Crypto.cs-r2-20260523.md:667:src/CryptoConstants.cs:66:        public const string P256_B =
codex-crypto-reviews/Crypto.cs-r2-20260523.md:668:src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
codex-crypto-reviews/Crypto.cs-r2-20260523.md:677:tests/CryptoTests.cs:66:            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1023:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1228:   555	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1229:   556	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1230:   557	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1285:   612	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1571:   893	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1672:   994	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:1723:  1045	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3272:    66	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3861:src/CryptoConstants.cs:36:        /// Used with <c>ECNamedCurveTable.GetByName</c>.
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3862:src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:389:    67	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1053:   134	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1114:   195	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1195:src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1196:src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1197:src/CryptoConstants.cs:6:// Upstream snapshot:
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1198:src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1199:src/CryptoConstants.cs:9:// Mapping to upstream:
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1200:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1201:src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1203:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1204:src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2990:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3093:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3094:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3259:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5414:/bin/zsh -lc 'rg -n "const string CURVE_NAME|CURVE_NAME" src/CryptoConstants.cs src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5416:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5417:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5418:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5419:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5420:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
codex-crypto-reviews/Http.cs-r1-20260523.md:728:src/CryptoConstants.cs
codex-crypto-reviews/Http.cs-r1-20260523.md:6957:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Http.cs-r2-20260523.md:315:src/CryptoConstants.cs
codex-crypto-reviews/Http.cs-r2-20260523.md:7369:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/Http.cs-r2-20260523.md:7413:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/Http.cs-r2-20260523.md:7764:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/Http.cs-r2-20260523.md:10436:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Http.cs-r2-20260523.md:12016:codex-crypto-reviews/Http.cs-r2-20260523.md:7764:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1:# Codex review — src/CryptoConstants.cs — round 1 — 20260523
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:22:File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:34:B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:301:/bin/zsh -lc 'rg -n "turnkey|C# SDK|csharp|CryptoConstants|turnkey-sdk-csharp" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:316:/bin/zsh -lc 'nl -ba src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:327:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:328:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:330:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:349:    32	    public static class CryptoConstants
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:355:    38	        public const string CURVE_NAME = "secp256r1";
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:362:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:375:    58	        public const string P256_P =
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:383:    66	        public const string P256_B =
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:390:    73	        public const string P256_A_OFFSET = "3";
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:409:src/CryptoConstants.cs
codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:487:?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:655:   176	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1224:   145	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1285:   206	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1763:   389	    67	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2427:  1053	   134	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2488:  1114	   195	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2569:  1195	src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2570:  1196	src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2571:  1197	src/CryptoConstants.cs:6:// Upstream snapshot:
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2572:  1198	src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2573:  1199	src/CryptoConstants.cs:9:// Mapping to upstream:
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2574:  1200	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2575:  1201	src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2577:  1203	src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2578:  1204	src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4364:  2990	codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4467:  3093	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4468:  3094	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4633:  3259	codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5934:  5414	/bin/zsh -lc 'rg -n "const string CURVE_NAME|CURVE_NAME" src/CryptoConstants.cs src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5936:  5416	src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5937:  5417	src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5938:  5418	src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5939:  5419	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5940:  5420	src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7319:   655	   176	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7888:  1224	   145	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7949:  1285	   206	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8315:/bin/zsh -lc "rg -n \"GetPublicKey\\(|Uint8ArrayFromHexString|StringToBase64UrlString|Uint8ArrayToHexString|DecodeBase64UrlToString|CURVE_NAME|JsonSerializable\\(typeof\\(ApiKeyStamper\" src" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8317:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8318:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8350:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8352:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8356:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8370:src/ApiKeyStamper.cs:176:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8629:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8719:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10199:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8315:/bin/zsh -lc "rg -n \"GetPublicKey\\(|Uint8ArrayFromHexString|StringToBase64UrlString|Uint8ArrayToHexString|DecodeBase64UrlToString|CURVE_NAME|JsonSerializable\\(typeof\\(ApiKeyStamper\" src" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10204:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8370:src/ApiKeyStamper.cs:176:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10383:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17300:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11070:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11336:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2570:  1196	src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11337:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2574:  1200	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11338:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11554:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4467:  3093	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11565:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5939:  5419	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11702:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8317:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12871:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1196:src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12872:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1200:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12873:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13089:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3093:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13103:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5419:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13718:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/README.md:11:2. `CryptoConstants.cs`
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:303:src/CryptoConstants.cs
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:835:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1293:   148	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1354:   209	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1446:/bin/zsh -lc 'rg -n "ApiKeyStamper|TurnkeyStamp|JsonSourceGeneration|DefaultIgnoreCondition|PropertyNamingPolicy|Encoder|WriteIndented" src/TurnkeyJsonContext.cs src/Encoding.cs src/Crypto.cs src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2263:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2870:src/CryptoConstants.cs:19://     readable and so a Codex reviewer can confirm the bytes match.
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2871:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2872:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2873:src/CryptoConstants.cs:41:        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3566:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4361:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8629:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
codex-crypto-reviews/Crypto.cs-r3-20260523.md:97:src/CryptoConstants.cs
codex-crypto-reviews/Crypto.cs-r3-20260523.md:970:   568	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:971:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:972:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:1027:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:1370:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:1459:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:1516:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3396:   568	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3397:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3398:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3453:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3801:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3890:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:3947:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4108:568:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4109:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4110:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4128:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4132:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4136:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:4218:    79	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:5666:src/CryptoConstants.cs
codex-crypto-reviews/Crypto.cs-r3-20260523.md:6539:   568	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:6540:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:6541:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:6596:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:6939:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:7028:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:7085:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:8965:   568	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:8966:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:8967:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9022:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9370:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9459:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9516:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9677:568:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9678:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9679:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9697:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9701:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9705:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:9787:    79	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:11828:src/CryptoConstants.cs
codex-crypto-reviews/Crypto.cs-r3-20260523.md:11989:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:12204:   565	                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:12205:   566	                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:12206:   567	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:13841:   622	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14144:   925	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14245:  1026	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14302:  1083	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14438:    66	            var p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14816:./src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14817:./src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14818:./src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
codex-crypto-reviews/Crypto.cs-r3-20260523.md:14902:./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15534:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15552:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15553:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15554:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15559:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15610:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15621:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15636:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15880:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15891:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15892:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15893:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15908:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15914:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15918:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15944:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15955:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15956:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15957:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15972:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15978:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:15982:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:16008:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
codex-crypto-reviews/Crypto.cs-r3-20260523.md:16533:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
codex-crypto-reviews/Crypto.cs-r3-20260523.md:16961:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523.md:17075:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523.md:17300:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
codex-crypto-reviews/Crypto.cs-r3-20260523.md:17430:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18699:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18707:565:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18708:566:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18709:567:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18717:925:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18727:1026:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:18732:1083:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:20085:568:                p = new BigInteger(CryptoConstants.P256_P);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:20086:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:20087:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
codex-crypto-reviews/Crypto.cs-r3-20260523.md:20095:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:20099:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
codex-crypto-reviews/Crypto.cs-r3-20260523.md:20104:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);

codex
`CryptoConstants.cs` is a constants-only file: no public or internal methods are declared, so the method table will explicitly record that the method surface is empty. I’m checking where each constant is consumed and which fixtures exercise those consuming paths.
exec
/bin/zsh -lc 'nl -ba tests/Fixtures/README.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '220,520p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '520,700p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "nl -ba tests/Fixtures/crypto/turnkey-crypto-vectors.json | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc 'rg -n "turnkey-crypto-vectors|Load|Json|Fixtures/crypto|node-generated|source|sha256|generator" tests src codex-crypto-reviews/README.md codex-crypto-reviews/turnkey-source-pins.md README.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
README.md:53:- `System.Text.Json 8.0.5` — source-generated context only
README.md:54:  (`TurnkeyJsonContext`). No reflection-based serialization,
README.md:64:codex-crypto-reviews/          — pinned upstream snapshots, source-pin docs,
codex-crypto-reviews/README.md:15:6. `TurnkeyJsonContext.cs`
codex-crypto-reviews/README.md:29:## Pinned upstream sources
codex-crypto-reviews/README.md:31:The authoritative upstream sources are the npm tarballs extracted under
codex-crypto-reviews/README.md:32:[`upstream-snapshots/`](./upstream-snapshots/). The exact tarball sha256 is in
codex-crypto-reviews/README.md:35:GitHub commit SHAs are recorded in [`turnkey-source-pins.md`](./turnkey-source-pins.md)
codex-crypto-reviews/README.md:36:as **secondary** metadata only. The npm tarball is the source of truth because
codex-crypto-reviews/README.md:44:(peak submodule SHA in [`unity-source-pins.md`](./unity-source-pins.md)) is used
codex-crypto-reviews/README.md:46:TS source must be resolved by re-porting from the TS source, not by trusting
codex-crypto-reviews/README.md:59:  (sha256 in tarball-checksums.txt — record the matching hash in your output)
codex-crypto-reviews/README.md:65:A. Source pin acknowledgement: upstream package name, version, tarball sha256
codex-crypto-reviews/README.md:79:   BouncyCastle X↔noble Y, System.Text.Json↔JSON.stringify, BigInteger↔BigInt,
codex-crypto-reviews/turnkey-source-pins.md:1:# Turnkey upstream source pins
codex-crypto-reviews/turnkey-source-pins.md:5:[`upstream-snapshots/`](./upstream-snapshots/)) are the authoritative source.
codex-crypto-reviews/turnkey-source-pins.md:10:| src C# file                | Upstream snapshot path                  | npm package                | Version | Tarball sha256 |
codex-crypto-reviews/turnkey-source-pins.md:17:| src/TurnkeyJsonContext.cs  | turnkey-crypto-2.8.8                    | (no single upstream; covers DTOs from crypto + api-key-stamper + http) | n/a | n/a |
codex-crypto-reviews/turnkey-source-pins.md:22:  source: `dist/*.js` + `dist/*.d.ts` + `package.json` + LICENSE / README).
codex-crypto-reviews/turnkey-source-pins.md:23:- `ts-source/` — the original TypeScript source from `github.com/tkhq/sdk` at
codex-crypto-reviews/turnkey-source-pins.md:25:  remains the wire-format source of truth if the two ever disagree).
codex-crypto-reviews/turnkey-source-pins.md:39:These are recorded for cross-reference only. The npm tarball sha256 above
codex-crypto-reviews/turnkey-source-pins.md:40:is the controlling wire-format source.
codex-crypto-reviews/turnkey-source-pins.md:53:1. Update the table above (npm version, refresh tarball sha256, gitHead).
tests/PublicApi.expected.txt:19:            [System.Text.Json.Serialization.JsonPropertyName("publicKey")]
tests/PublicApi.expected.txt:21:            [System.Text.Json.Serialization.JsonPropertyName("scheme")]
tests/PublicApi.expected.txt:23:            [System.Text.Json.Serialization.JsonPropertyName("signature")]
tests/PublicApi.expected.txt:91:            [System.Text.Json.Serialization.JsonPropertyName("ciphertext")]
tests/PublicApi.expected.txt:93:            [System.Text.Json.Serialization.JsonPropertyName("encappedPublic")]
tests/PublicApi.expected.txt:166:            [System.Text.Json.Serialization.JsonPropertyName("privateKeyId")]
tests/PublicApi.expected.txt:168:            [System.Text.Json.Serialization.JsonPropertyName("targetPublicKey")]
tests/PublicApi.expected.txt:174:            [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
tests/PublicApi.expected.txt:175:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
tests/PublicApi.expected.txt:177:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
tests/PublicApi.expected.txt:179:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
tests/PublicApi.expected.txt:181:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
tests/PublicApi.expected.txt:183:            [System.Text.Json.Serialization.JsonPropertyName("type")]
tests/PublicApi.expected.txt:189:            [System.Text.Json.Serialization.JsonPropertyName("address")]
tests/PublicApi.expected.txt:191:            [System.Text.Json.Serialization.JsonPropertyName("targetPublicKey")]
tests/PublicApi.expected.txt:197:            [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
tests/PublicApi.expected.txt:198:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
tests/PublicApi.expected.txt:200:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
tests/PublicApi.expected.txt:202:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
tests/PublicApi.expected.txt:204:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
tests/PublicApi.expected.txt:206:            [System.Text.Json.Serialization.JsonPropertyName("type")]
tests/PublicApi.expected.txt:212:            [System.Text.Json.Serialization.JsonPropertyName("addressFormats")]
tests/PublicApi.expected.txt:214:            [System.Text.Json.Serialization.JsonPropertyName("curve")]
tests/PublicApi.expected.txt:216:            [System.Text.Json.Serialization.JsonPropertyName("encryptedBundle")]
tests/PublicApi.expected.txt:218:            [System.Text.Json.Serialization.JsonPropertyName("privateKeyName")]
tests/PublicApi.expected.txt:220:            [System.Text.Json.Serialization.JsonPropertyName("userId")]
tests/PublicApi.expected.txt:226:            [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
tests/PublicApi.expected.txt:227:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
tests/PublicApi.expected.txt:229:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
tests/PublicApi.expected.txt:231:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
tests/PublicApi.expected.txt:233:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
tests/PublicApi.expected.txt:235:            [System.Text.Json.Serialization.JsonPropertyName("type")]
tests/PublicApi.expected.txt:241:            [System.Text.Json.Serialization.JsonPropertyName("userId")]
tests/PublicApi.expected.txt:247:            [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
tests/PublicApi.expected.txt:248:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
tests/PublicApi.expected.txt:250:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
tests/PublicApi.expected.txt:252:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
tests/PublicApi.expected.txt:254:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
tests/PublicApi.expected.txt:256:            [System.Text.Json.Serialization.JsonPropertyName("type")]
tests/PublicApi.expected.txt:262:            [System.Text.Json.Serialization.JsonPropertyName("body")]
tests/PublicApi.expected.txt:264:            [System.Text.Json.Serialization.JsonPropertyName("stamp")]
tests/PublicApi.expected.txt:266:            [System.Text.Json.Serialization.JsonPropertyName("url")]
tests/PublicApi.expected.txt:272:            [System.Text.Json.Serialization.JsonPropertyName("stampHeaderName")]
tests/PublicApi.expected.txt:274:            [System.Text.Json.Serialization.JsonPropertyName("stampHeaderValue")]
tests/PublicApi.expected.txt:280:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
tests/PublicApi.expected.txt:284:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.ApiKeyStamper.TurnkeyStamp))]
tests/PublicApi.expected.txt:285:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Crypto.HpkeBundlePayload))]
tests/PublicApi.expected.txt:286:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.ExportPrivateKeyParameters))]
tests/PublicApi.expected.txt:287:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.ExportPrivateKeyRequestBody))]
tests/PublicApi.expected.txt:288:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.ExportWalletAccountParameters))]
tests/PublicApi.expected.txt:289:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.ExportWalletAccountRequestBody))]
tests/PublicApi.expected.txt:290:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.ImportPrivateKeyParameters))]
tests/PublicApi.expected.txt:291:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.ImportPrivateKeyRequestBody))]
tests/PublicApi.expected.txt:292:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.InitImportPrivateKeyParameters))]
tests/PublicApi.expected.txt:293:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.InitImportPrivateKeyRequestBody))]
tests/PublicApi.expected.txt:294:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.SignedRequest))]
tests/PublicApi.expected.txt:295:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.Stamp))]
tests/PublicApi.expected.txt:296:    [System.Text.Json.Serialization.JsonSerializable(typeof(Turnkey.Http.WhoamiRequestBody))]
tests/PublicApi.expected.txt:297:    [System.Text.Json.Serialization.JsonSourceGenerationOptions(DefaultIgnoreCondition=System.Text.Json.Serialization.JsonIgnoreCondition.Never, PropertyNameCaseInsensitive=false, WriteIndented=false)]
tests/PublicApi.expected.txt:298:    public class TurnkeyJsonContext : System.Text.Json.Serialization.JsonSerializerContext, System.Text.Json.Serialization.Metadata.IJsonTypeInfoResolver
tests/PublicApi.expected.txt:301:        public TurnkeyJsonContext() { }
tests/PublicApi.expected.txt:302:        public TurnkeyJsonContext(System.Text.Json.JsonSerializerOptions options) { }
tests/PublicApi.expected.txt:303:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<bool> Boolean { get; }
tests/PublicApi.expected.txt:304:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.ExportPrivateKeyParameters> ExportPrivateKeyParameters { get; }
tests/PublicApi.expected.txt:305:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.ExportPrivateKeyRequestBody> ExportPrivateKeyRequestBody { get; }
tests/PublicApi.expected.txt:306:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.ExportWalletAccountParameters> ExportWalletAccountParameters { get; }
tests/PublicApi.expected.txt:307:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.ExportWalletAccountRequestBody> ExportWalletAccountRequestBody { get; }
tests/PublicApi.expected.txt:308:        protected override System.Text.Json.JsonSerializerOptions? GeneratedSerializerOptions { get; }
tests/PublicApi.expected.txt:309:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Crypto.HpkeBundlePayload> HpkeBundlePayload { get; }
tests/PublicApi.expected.txt:310:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.ImportPrivateKeyParameters> ImportPrivateKeyParameters { get; }
tests/PublicApi.expected.txt:311:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.ImportPrivateKeyRequestBody> ImportPrivateKeyRequestBody { get; }
tests/PublicApi.expected.txt:312:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.InitImportPrivateKeyParameters> InitImportPrivateKeyParameters { get; }
tests/PublicApi.expected.txt:313:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.InitImportPrivateKeyRequestBody> InitImportPrivateKeyRequestBody { get; }
tests/PublicApi.expected.txt:314:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<bool?> NullableBoolean { get; }
tests/PublicApi.expected.txt:315:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.SignedRequest> SignedRequest { get; }
tests/PublicApi.expected.txt:316:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.Stamp> Stamp { get; }
tests/PublicApi.expected.txt:317:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<string> String { get; }
tests/PublicApi.expected.txt:318:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<string[]> StringArray { get; }
tests/PublicApi.expected.txt:319:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.ApiKeyStamper.TurnkeyStamp> TurnkeyStamp { get; }
tests/PublicApi.expected.txt:320:        public System.Text.Json.Serialization.Metadata.JsonTypeInfo<Turnkey.Http.WhoamiRequestBody> WhoamiRequestBody { get; }
tests/PublicApi.expected.txt:321:        public static Turnkey.TurnkeyJsonContext Default { get; }
tests/PublicApi.expected.txt:322:        public override System.Text.Json.Serialization.Metadata.JsonTypeInfo? GetTypeInfo(System.Type type) { }
src/Http.cs:25://   - Body JSON serialized via TurnkeyJsonContext (source-gen, no
src/Http.cs:42:using System.Text.Json;
src/Http.cs:156:                JsonSerializer.Serialize(body, TurnkeyJsonContext.Default.WhoamiRequestBody));
src/Http.cs:167:                JsonSerializer.Serialize(body, TurnkeyJsonContext.Default.InitImportPrivateKeyRequestBody));
src/Http.cs:178:                JsonSerializer.Serialize(body, TurnkeyJsonContext.Default.ImportPrivateKeyRequestBody));
src/Http.cs:189:                JsonSerializer.Serialize(body, TurnkeyJsonContext.Default.ExportPrivateKeyRequestBody));
src/Http.cs:200:                JsonSerializer.Serialize(body, TurnkeyJsonContext.Default.ExportWalletAccountRequestBody));
src/Http.cs:203:        private SignedRequest CreateSignedRequest(string url, string bodyJson)
src/Http.cs:205:            var stampResult = _stamper.Stamp(bodyJson);
src/Http.cs:209:                Body = bodyJson,
src/Http.cs:233:            [System.Text.Json.Serialization.JsonPropertyName("body")]
src/Http.cs:236:            [System.Text.Json.Serialization.JsonPropertyName("stamp")]
src/Http.cs:239:            [System.Text.Json.Serialization.JsonPropertyName("url")]
src/Http.cs:248:            [System.Text.Json.Serialization.JsonPropertyName("stampHeaderName")]
src/Http.cs:251:            [System.Text.Json.Serialization.JsonPropertyName("stampHeaderValue")]
src/Http.cs:257:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
src/Http.cs:263:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
src/Http.cs:266:            [System.Text.Json.Serialization.JsonPropertyName("type")]
src/Http.cs:269:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
src/Http.cs:272:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
src/Http.cs:279:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
src/Http.cs:280:            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
src/Http.cs:286:            [System.Text.Json.Serialization.JsonPropertyName("userId")]
src/Http.cs:292:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
src/Http.cs:295:            [System.Text.Json.Serialization.JsonPropertyName("type")]
src/Http.cs:298:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
src/Http.cs:301:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
src/Http.cs:304:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
src/Http.cs:305:            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
src/Http.cs:311:            [System.Text.Json.Serialization.JsonPropertyName("userId")]
src/Http.cs:314:            [System.Text.Json.Serialization.JsonPropertyName("addressFormats")]
src/Http.cs:317:            [System.Text.Json.Serialization.JsonPropertyName("curve")]
src/Http.cs:320:            [System.Text.Json.Serialization.JsonPropertyName("encryptedBundle")]
src/Http.cs:323:            [System.Text.Json.Serialization.JsonPropertyName("privateKeyName")]
src/Http.cs:329:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
src/Http.cs:332:            [System.Text.Json.Serialization.JsonPropertyName("type")]
src/Http.cs:335:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
src/Http.cs:338:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
src/Http.cs:341:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
src/Http.cs:342:            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
src/Http.cs:348:            [System.Text.Json.Serialization.JsonPropertyName("privateKeyId")]
src/Http.cs:351:            [System.Text.Json.Serialization.JsonPropertyName("targetPublicKey")]
src/Http.cs:357:            [System.Text.Json.Serialization.JsonPropertyName("organizationId")]
src/Http.cs:360:            [System.Text.Json.Serialization.JsonPropertyName("type")]
src/Http.cs:363:            [System.Text.Json.Serialization.JsonPropertyName("timestampMs")]
src/Http.cs:366:            [System.Text.Json.Serialization.JsonPropertyName("parameters")]
src/Http.cs:369:            [System.Text.Json.Serialization.JsonPropertyName("generateAppProofs")]
src/Http.cs:370:            [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
src/Http.cs:376:            [System.Text.Json.Serialization.JsonPropertyName("address")]
src/Http.cs:379:            [System.Text.Json.Serialization.JsonPropertyName("targetPublicKey")]
tests/ApiKeyStamperTests.cs:3:// Vector sources:
tests/ApiKeyStamperTests.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{private,public}
tests/ApiKeyStamperTests.cs:5://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts
tests/ApiKeyStamperTests.cs:9:// same digest, so for identical input + key + nonce-source the signature
tests/ApiKeyStamperTests.cs:21:using System.Text.Json;
tests/ApiKeyStamperTests.cs:84:            string decodedJson = Encoding.DecodeBase64UrlToString(result.StampHeaderValue);
tests/ApiKeyStamperTests.cs:86:            using var doc = JsonDocument.Parse(decodedJson);
src/Crypto.cs:7://   ts-source/constants.ts            -> Crypto.Constants nested class
src/Crypto.cs:8://   ts-source/math.ts                 -> Crypto.Math nested class (ModSqrt)
src/Crypto.cs:9://   ts-source/crypto.ts (subset)      -> GetPublicKey / GenerateP256KeyPair /
src/Crypto.cs:17://   ts-source/crypto.ts HKDF helpers  -> Crypto.Hkdf nested class
src/Crypto.cs:19://   ts-source/turnkey.ts (subset)     -> Curve enum / DecryptCredentialBundle /
src/Crypto.cs:32://   - System.Text.Json source generation (TurnkeyJsonContext) replaces
src/Crypto.cs:33://     Newtonsoft.Json's JObject.Parse / JsonConvert.SerializeObject used by
src/Crypto.cs:39://   - Newtonsoft.Json dependency dropped.
src/Crypto.cs:50:using System.Text.Json;
src/Crypto.cs:338:            [System.Text.Json.Serialization.JsonPropertyName("encappedPublic")]
src/Crypto.cs:341:            [System.Text.Json.Serialization.JsonPropertyName("ciphertext")]
src/Crypto.cs:644:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:712:            // (JsonDocument.Parse, DecodeKey hex parse, etc.) instead of
src/Crypto.cs:722:            using var bundleDoc = JsonDocument.Parse(parameters.ImportBundle);
src/Crypto.cs:743:            using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
src/Crypto.cs:797:                using var bundleDoc = JsonDocument.Parse(parameters.ExportBundle);
src/Crypto.cs:816:                using var signedDoc = JsonDocument.Parse(System.Text.Encoding.UTF8.GetString(signedDataBytes));
src/Crypto.cs:911:            using (var sha256 = SHA256.Create())
src/Crypto.cs:913:                var h1 = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signingInput));
src/Crypto.cs:914:                msgDigest = sha256.ComputeHash(h1);
src/Crypto.cs:1159:        private static string? GetStringOrNull(JsonElement element, string propertyName)
src/Crypto.cs:1161:            if (element.ValueKind != JsonValueKind.Object)
src/Crypto.cs:1169:            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
src/Crypto.cs:1173:            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
tests/CryptoTests.cs:3:// Vector sources:
tests/CryptoTests.cs:428:        public void FormatHpkeBuf_ReturnsExpectedJson()
tests/CryptoTests.cs:509:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
tests/CryptoTests.cs:528:            // (codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts)
src/turnkey-sdk-csharp.csproj:24:    <PackageReference Include="System.Text.Json" />
src/TurnkeyJsonContext.cs:1:// IL2CPP-safe System.Text.Json source-generated context.
src/TurnkeyJsonContext.cs:3:// Every JsonSerializer.Serialize / JsonSerializer.Deserialize call in this SDK
src/TurnkeyJsonContext.cs:4:// MUST go through this context's typed overloads (TurnkeyJsonContext.Default.<Type>)
src/TurnkeyJsonContext.cs:9://   - JsonSerializerIsReflectionEnabledByDefault is the user/runtime knob
src/TurnkeyJsonContext.cs:15://   1. Add a [JsonSerializable(typeof(<DTO>))] attribute below.
src/TurnkeyJsonContext.cs:16://   2. Use the resulting TurnkeyJsonContext.Default.<DTO> overload at the
src/TurnkeyJsonContext.cs:20:using System.Text.Json.Serialization;
src/TurnkeyJsonContext.cs:24:    // Encoder choice: UnsafeRelaxedJsonEscaping makes System.Text.Json
src/TurnkeyJsonContext.cs:32:    /// IL2CPP-safe System.Text.Json source-generated context for every DTO
src/TurnkeyJsonContext.cs:33:    /// the SDK serializes. Set <c>Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping</c>
src/TurnkeyJsonContext.cs:38:    [JsonSourceGenerationOptions(
src/TurnkeyJsonContext.cs:41:        DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
src/TurnkeyJsonContext.cs:42:    [JsonSerializable(typeof(Crypto.HpkeBundlePayload))]
src/TurnkeyJsonContext.cs:43:    [JsonSerializable(typeof(ApiKeyStamper.TurnkeyStamp))]
src/TurnkeyJsonContext.cs:44:    [JsonSerializable(typeof(Http.WhoamiRequestBody))]
src/TurnkeyJsonContext.cs:45:    [JsonSerializable(typeof(Http.InitImportPrivateKeyRequestBody))]
src/TurnkeyJsonContext.cs:46:    [JsonSerializable(typeof(Http.InitImportPrivateKeyParameters))]
src/TurnkeyJsonContext.cs:47:    [JsonSerializable(typeof(Http.ImportPrivateKeyRequestBody))]
src/TurnkeyJsonContext.cs:48:    [JsonSerializable(typeof(Http.ImportPrivateKeyParameters))]
src/TurnkeyJsonContext.cs:49:    [JsonSerializable(typeof(Http.ExportPrivateKeyRequestBody))]
src/TurnkeyJsonContext.cs:50:    [JsonSerializable(typeof(Http.ExportPrivateKeyParameters))]
src/TurnkeyJsonContext.cs:51:    [JsonSerializable(typeof(Http.ExportWalletAccountRequestBody))]
src/TurnkeyJsonContext.cs:52:    [JsonSerializable(typeof(Http.ExportWalletAccountParameters))]
src/TurnkeyJsonContext.cs:53:    [JsonSerializable(typeof(Http.SignedRequest))]
src/TurnkeyJsonContext.cs:54:    [JsonSerializable(typeof(Http.Stamp))]
src/TurnkeyJsonContext.cs:55:    public partial class TurnkeyJsonContext : JsonSerializerContext
src/TurnkeyJsonContext.cs:60:        /// <c>JsonSerializerOptions</c> when callers need bit-for-bit
src/TurnkeyJsonContext.cs:65:            JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
tests/Fixtures/README.md:14:- **node-generated** — bytes generated by running the pinned upstream Node
tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
tests/Fixtures/README.md:34:into the matching fixture file and update its sha256 in
src/Encoding.cs:7://   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
src/Encoding.cs:9://   ts-source/base64.ts       -> StringToBase64UrlString /
src/Encoding.cs:14://   ts-source/encode.ts       -> PointEncode
src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
src/Encoding.cs:17://   ts-source/index.ts        -> DEFAULT_JWK_MEMBER_BYTE_LENGTH constant
src/Encoding.cs:61:            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
tests/Fixtures/encoding/turnkey-encoding-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:6:    "tarball_sha256": "2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc",
src/ApiKeyStamper.cs:7://   ts-source/index.ts                -> ApiKeyStamper class + signWithApiKey
src/ApiKeyStamper.cs:9://   ts-source/purejs.ts               -> signWithApiKey "purejs" runtime
src/ApiKeyStamper.cs:109:            [System.Text.Json.Serialization.JsonPropertyName("publicKey")]
src/ApiKeyStamper.cs:112:            [System.Text.Json.Serialization.JsonPropertyName("scheme")]
src/ApiKeyStamper.cs:115:            [System.Text.Json.Serialization.JsonPropertyName("signature")]
src/ApiKeyStamper.cs:136:            string stampJson = System.Text.Json.JsonSerializer.Serialize(
src/ApiKeyStamper.cs:137:                stamp, TurnkeyJsonContext.Default.TurnkeyStamp);
src/ApiKeyStamper.cs:142:                StampHeaderValue = Encoding.StringToBase64UrlString(stampJson),
src/packages.lock.json:11:      "System.Text.Json": {
src/packages.lock.json:81:      "System.Text.Json": {
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:4:    "source_fixture": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{public,private}",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:5:    "source_test": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:7:    "tarball_sha256": "962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:19:    "_source_line": "ts-source/__tests__/stamp-test.ts:14-37",
tests/PublicApiSnapshotTests.cs:33:            // Resolve the baseline relative to the test project source.
tests/packages.lock.json:76:          "Newtonsoft.Json": "13.0.1"
tests/packages.lock.json:84:      "Newtonsoft.Json": {
tests/packages.lock.json:156:          "System.Text.Json": "[8.0.5, )"
tests/packages.lock.json:165:      "System.Text.Json": {
tests/Fixtures/crypto/turnkey-crypto-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:6:    "tarball_sha256": "75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:11:      "_source_line": "ts-source/__tests__/crypto-test.ts:179-184",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:21:      "_source_line": "ts-source/__tests__/crypto-test.ts:243-250",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:26:      "_source_line": "ts-source/__tests__/crypto-test.ts:252-260",
tests/Fixtures/crypto/turnkey-crypto-vectors.json:32:    "_source_line": "ts-source/__tests__/crypto-test.ts:173-178",
tests/HttpTests.cs:4:using System.Text.Json;
tests/HttpTests.cs:66:        public void StampInitImportPrivateKey_ProducesCorrectUrlAndJson()
tests/HttpTests.cs:85:            using var doc = JsonDocument.Parse(req.Body);
tests/HttpTests.cs:121:            using var doc = JsonDocument.Parse(req.Body);
tests/HttpTests.cs:124:            formats.ValueKind.Should().Be(JsonValueKind.Array);
tests/HttpTests.cs:150:            using var doc = JsonDocument.Parse(req.Body);
tests/HttpTests.cs:175:            using var doc = JsonDocument.Parse(req.Body);
tests/HttpTests.cs:189:            string decodedJson = Encoding.DecodeBase64UrlToString(req.Stamp.StampHeaderValue);
tests/HttpTests.cs:190:            using var doc = JsonDocument.Parse(decodedJson);

exec
/bin/zsh -lc 'find tests/Fixtures -maxdepth 3 -type f -print | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
tests/Fixtures/crypto/turnkey-crypto-vectors.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts | sed -n '150,270p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   150	  });
   151	});
   152	
   153	describe("Turnkey Crypto Primitives", () => {
   154	  test("getPublicKey - returns the correct public key", () => {
   155	    const keyPair = generateP256KeyPair();
   156	    const publicKey = getPublicKey(
   157	      uint8ArrayFromHexString(keyPair.privateKey),
   158	      true,
   159	    );
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/turnkey-source-pins.md | sed -n '1,70p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/README.md | sed -n '29,95p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
    29	## Pinned upstream sources
    30	
    31	The authoritative upstream sources are the npm tarballs extracted under
    32	[`upstream-snapshots/`](./upstream-snapshots/). The exact tarball sha256 is in
    33	[`upstream-snapshots/tarball-checksums.txt`](./upstream-snapshots/tarball-checksums.txt).
    34	
    35	GitHub commit SHAs are recorded in [`turnkey-source-pins.md`](./turnkey-source-pins.md)
    36	as **secondary** metadata only. The npm tarball is the source of truth because
    37	npm publish contents may differ from the git tree (excluded test files,
    38	included build output, etc.).
    39	
    40	## Unity port reference
    41	
    42	The pre-existing C# Unity port at
    43	[`packages/turnkey-sdk-unity/Runtime/`](../packages/turnkey-sdk-unity/Runtime/)
    44	(peak submodule SHA in [`unity-source-pins.md`](./unity-source-pins.md)) is used
    45	**only as a C# adaptation reference**. Logic divergence from the peak-pinned
    46	TS source must be resolved by re-porting from the TS source, not by trusting
    47	the Unity port.
    48	
    49	## Codex prompt template
    50	
    51	Codex is invoked with the following prompt (sections A-G required in every
    52	review round; pass criterion in `codex-crypto-review.sh`):
    53	
    54	```
    55	SYSTEM:
    56	You are reviewing a C# port of the Turnkey TypeScript SDK file: {cs-file}
    57	The pinned upstream npm tarball is at:
    58	  codex-crypto-reviews/upstream-snapshots/turnkey-{pkg}-{version}/
    59	  (sha256 in tarball-checksums.txt — record the matching hash in your output)
    60	The C# file is at:
    61	  src/{cs-file}
    62	
    63	REQUIRED OUTPUTS for this review round (all must appear in your output):
    64	
    65	A. Source pin acknowledgement: upstream package name, version, tarball sha256
    66	   (from tarball-checksums.txt), C# file's `git rev-parse HEAD` SHA.
    67	
    68	B. Method coverage table: every public + internal helper method in {cs-file}
    69	   listed in a markdown table:
    70	     - C# method (file:line)
    71	     - Upstream TS function (path:line within upstream-snapshots/...)
    72	     - Status: REVIEWED / NOT-REVIEWED
    73	     - Notes (one line)
    74	   Do NOT skip a row. If upstream counterpart is missing, set NOT-REVIEWED.
    75	
    76	C. Intentional adaptations: every C#/TS pattern adaptation explicitly listed
    77	   with the reason why it is structural (does not change wire bytes or
    78	   observable behavior). Examples: Task<T>↔Promise<T>, byte[]↔Uint8Array,
    79	   BouncyCastle X↔noble Y, System.Text.Json↔JSON.stringify, BigInteger↔BigInt,
    80	   exceptions↔Error/throw.
    81	
    82	D. (Crypto.cs only) D17 enforcement check: confirm BouncyCastle is used only
    83	   for ECDSA / ECDH / AES-GCM / SHA-256 / HMAC / BigInteger / EC point ops.
    84	   The following MUST NOT appear in Crypto.cs:
    85	     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
    86	     - Org.BouncyCastle.Crypto.Hpke.*
    87	     - Any "high-level" KDF or HPKE wrapper
    88	   If any banned API is present, flag it as a divergence.
    89	
    90	E. Logic divergence findings: every place where C# behavior differs from
    91	   upstream TS in any of:
    92	     - algorithm step order
    93	     - constant values
    94	     - error handling (which condition throws what)
    95	     - byte ordering

exec
/bin/zsh -lc 'rg -n "public |internal |private |protected |const |static" src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
32:    public static class CryptoConstants
38:        public const string CURVE_NAME = "secp256r1";
41:        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
58:        public const string P256_P =
66:        public const string P256_B =
71:        /// Upstream inlines <c>const a = p - BigInt(3)</c>.
73:        public const string P256_A_OFFSET = "3";

codex
The constants line up with upstream as extracted values rather than callable logic. I’m doing one last pass on fixture provenance and the package pin docs so section F can distinguish “copied upstream test vectors” from “runtime-generated fixtures.”
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '400,460p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "node -e 'const p=BigInt(\"0xffffffff00000001000000000000000000000000ffffffffffffffffffffffff\"); console.log(p.toString(10)); console.log((p-3n).toString(10)); const b=BigInt(\"0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b\"); console.log(b.toString(16));'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
115792089210356248762697446949407573530086143415290314195533631308867097853951
115792089210356248762697446949407573530086143415290314195533631308867097853948
5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '72,90p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc 'rg -n "secp256r1|p256|P-256|33|P256_P|P256_B|P256_A_OFFSET|COMPRESSED_PUBLIC_KEY_SIZE|CURVE_NAME" src/Crypto.cs src/ApiKeyStamper.cs tests/CryptoTests.cs tests/ApiKeyStamperTests.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/ApiKeyStamper.cs:10://                                         (P-256 ECDSA via @noble/curves,
src/ApiKeyStamper.cs:66:        /// <summary>Turnkey signature scheme identifier for P-256 API keys.</summary>
src/ApiKeyStamper.cs:73:        /// Initialize the stamper with a P-256 API key pair.
src/ApiKeyStamper.cs:75:        /// <param name="apiPublicKey">Compressed P-256 public key as hex (66 chars).</param>
src/ApiKeyStamper.cs:76:        /// <param name="apiPrivateKey">P-256 private key scalar as 64 hex chars.</param>
src/ApiKeyStamper.cs:158:            //   const publicKey = p256.getPublicKey(input.privateKey, true);
src/ApiKeyStamper.cs:177:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/ApiKeyStamper.cs:204:            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
src/ApiKeyStamper.cs:239:                    // Long-form length (1 octet count). For ECDSA P-256
tests/CryptoTests.cs:5://   - NIST P-256 known points (G and 2G) for GetPublicKey
tests/CryptoTests.cs:78:            // P-256 prime. Pick any quadratic residue: 4 = 2^2.
tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
tests/CryptoTests.cs:117:                + "303132333435363738393a3b3c3d3e3f"
tests/CryptoTests.cs:137:                "06a6b88c5853361a06104c9ceb35b45cef760014904671014a193f40c15fc244");
tests/CryptoTests.cs:160:                "19ef24a32c717b167f33a91d6f648bdf96596776afdb6377ac434c1c293ccb04");
tests/CryptoTests.cs:205:                compressed.Should().HaveCount(33);
tests/CryptoTests.cs:217:            // For a 65-byte uncompressed key we produce a 33-byte compressed key.
tests/CryptoTests.cs:231:            var compressed = new byte[33];
tests/CryptoTests.cs:257:            kp.PublicKey.Should().HaveLength(66);            // 33 bytes hex
tests/CryptoTests.cs:269:            pub.Should().HaveCount(33);
tests/CryptoTests.cs:294:            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
tests/CryptoTests.cs:297:               .WithMessage("invalid P-256 private key: expected 32 bytes, got 31");
tests/CryptoTests.cs:306:               .WithMessage("invalid P-256 private key: scalar must be in [1, n - 1]");
tests/CryptoTests.cs:317:               .WithMessage("invalid P-256 private key: scalar must be in [1, n - 1]");
tests/CryptoTests.cs:332:                "deadbeefcafebabe1234567890abcdef00112233445566778899aabbccddeeff");
tests/CryptoTests.cs:340:            // encrypted = compressed(senderPub) || ciphertext (33 + N bytes)
tests/CryptoTests.cs:341:            encrypted.Length.Should().BeGreaterThan(33);
tests/CryptoTests.cs:344:            var compressedSender = new byte[33];
tests/CryptoTests.cs:345:            Array.Copy(encrypted, 0, compressedSender, 0, 33);
tests/CryptoTests.cs:346:            var ciphertext = new byte[encrypted.Length - 33];
tests/CryptoTests.cs:347:            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
tests/CryptoTests.cs:373:            var compressedSender = new byte[33];
tests/CryptoTests.cs:374:            Array.Copy(encrypted, 0, compressedSender, 0, 33);
tests/CryptoTests.cs:375:            var ciphertext = new byte[encrypted.Length - 33];
tests/CryptoTests.cs:376:            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
tests/CryptoTests.cs:400:            var compressedSender = new byte[33];
tests/CryptoTests.cs:401:            Array.Copy(encrypted, 0, compressedSender, 0, 33);
tests/CryptoTests.cs:402:            var ciphertext = new byte[encrypted.Length - 33];
tests/CryptoTests.cs:403:            Array.Copy(encrypted, 33, ciphertext, 0, ciphertext.Length);
tests/CryptoTests.cs:453:            // when the buffer is shorter than 33 bytes.
tests/CryptoTests.cs:503:        // Curve.Secp256k1 — UncompressRawPublicKey
tests/CryptoTests.cs:507:        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
tests/CryptoTests.cs:509:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
tests/CryptoTests.cs:513:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
tests/CryptoTests.cs:532:                "20fa65df11f24833790ae283fc9a0c215eecbbc589549767977994dc69d05a56";
tests/CryptoTests.cs:576:            // Base58Check of a small (<33 byte) payload should still decode (with valid checksum)
tests/ApiKeyStamperTests.cs:7:// P-256 ECDSA signatures are not deterministic across libraries (noble uses
tests/ApiKeyStamperTests.cs:59:               .WithMessage("invalid P-256 private key: expected 32 bytes, got 2");
tests/ApiKeyStamperTests.cs:69:               .WithMessage("invalid P-256 private key: scalar must be in [1, n - 1]");
tests/ApiKeyStamperTests.cs:103:            // verifiable against the configured P-256 key, the stamp would be
tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/ApiKeyStamperTests.cs:160:            // Mismatched public key (any other valid P-256 compressed key).
tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:95:                125, 178, 154, 31, 75, 199, 194, 34, 192, 223, 34, 135, 39, 183, 10, 64, 33,
src/Crypto.cs:96:                18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53, 41, 180, 122, 198, 166, 48,
src/Crypto.cs:106:                135, 39, 183, 10, 64, 33, 18, 47, 63, 4, 233, 32, 108, 209, 36, 19, 80, 53,
src/Crypto.cs:111:            /// <summary>SEC1 uncompressed P-256 public key length, bytes (0x04 + X + Y).</summary>
src/Crypto.cs:116:                "04cf288fe433cc4e1aa0ce1632feac4ea26bf2f5a09dcfe5a42c398e06898710330f0572882f4dbdf0f5304b8fc8703acd69adca9a4bbf7f5d00d20a5e364b2569";
src/Crypto.cs:120:                "04d498aa87ac3bf982ac2b5dd9604d0074905cfbda5d62727c5a237b895e6749205e9f7cd566909c4387f6ca25c308445c60884b788560b785f4a96ac33702a469";
src/Crypto.cs:135:            Secp256k1,
src/Crypto.cs:179:                // p % 4 == 3 fast path (applies to NIST P-256 / P-384 / P-521).
src/Crypto.cs:285:        /// <summary>P-256 key pair (hex-encoded) returned by <see cref="GenerateP256KeyPair"/>.</summary>
src/Crypto.cs:355:            // Upstream uses noble's p256.getPublicKey which validates that
src/Crypto.cs:373:        /// Generate a random P-256 key pair. Equivalent to upstream
src/Crypto.cs:398:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:445:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:507:        /// Compress an uncompressed P-256 public key into 33 bytes.
src/Crypto.cs:513:        /// SEC1 uncompressed key the result is the same 33-byte
src/Crypto.cs:541:        /// Uncompress a 33-byte compressed public key into 65 bytes.
src/Crypto.cs:544:        /// <param name="rawPublicKey">33-byte compressed key starting with 0x02 or 0x03.</param>
src/Crypto.cs:549:            if (rawPublicKey.Length != 33)
src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
src/Crypto.cs:572:            else // Secp256k1
src/Crypto.cs:606:            Array.Copy(Encoding.Uint8ArrayFromHexString(yHexOut), 0, uncompressed, 33, 32);
src/Crypto.cs:618:            // Upstream: encappedPublic := encryptedBuf.slice(0,33);
src/Crypto.cs:619:            //           ciphertext     := encryptedBuf.slice(33);
src/Crypto.cs:621:            // the input is shorter than 33 bytes that uncompress call would
src/Crypto.cs:623:            // raw slice through uncompressRawPublicKey which enforces 33-byte
src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
src/Crypto.cs:669:                if (bundleBytes.Length <= 33)
src/Crypto.cs:673:                        + " is too low. Expecting a compressed public key (33 bytes) and an encrypted credential.");
src/Crypto.cs:676:                var compressedEncappedKeyBuf = new byte[33];
src/Crypto.cs:677:                Array.Copy(bundleBytes, 0, compressedEncappedKeyBuf, 0, 33);
src/Crypto.cs:678:                var ciphertextBuf = new byte[bundleBytes.Length - 33];
src/Crypto.cs:679:                Array.Copy(bundleBytes, 33, ciphertextBuf, 0, ciphertextBuf.Length);
src/Crypto.cs:883:        /// Optional hex-encoded uncompressed P-256 public key to verify
src/Crypto.cs:928:            // Upstream noble p256.getSharedSecret validates the private scalar.
src/Crypto.cs:951:        /// Validates a P-256 private key against noble's invariants: exactly
src/Crypto.cs:965:                    "invalid P-256 private key: expected 32 bytes, got " + privateKey.Length);
src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:975:                    "invalid P-256 private key: scalar must be in [1, n - 1]");
src/Crypto.cs:1049:            // Upstream noble p256.verify accepts r||s of length 64. Anything
src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);

exec
/bin/zsh -lc "sed -n '1,240p' codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
# Codex review — src/CryptoConstants.cs — round 1 — 20260523

C# SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
Upstream snapshot: `turnkey-crypto-2.8.8`

---

2026-05-23T01:23:18.483325Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e526d-f94a-7e90-9120-d36c55ece446
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 1 of 3 for this file.

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

2026-05-23T01:23:20.055210Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll treat this as a source-parity review, so I’m going to pin the exact upstream snapshot, enumerate every C# method, and trace each one to the TypeScript implementation before making any findings. I’m also loading the review skill and checking memory briefly for any prior C# SDK review context.
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

exec
/bin/zsh -lc "nl -ba /Users/takeshi/.codex/memories/MEMORY.md | sed -n '270,306p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   270	
   271	- Symptom: the checklist is already stale when presented. Cause: the user had handled some PRs and the assistant reused the first query result. Fix: refresh the GitHub queries immediately before answering when the user says they already acted on items [Task 1]
   272	
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

codex
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| No public/internal helper methods declared (`src/CryptoConstants.cs:32-74`) | N/A | REVIEWED | File contains only constants: `CURVE_NAME`, `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET`. |

**C. Intentional Adaptations**

- `@noble/curves/p256` implicit curve -> explicit BouncyCastle curve name `secp256r1`; structural registry selection for the same P-256 domain.
- Upstream inline `33` compressed-key length -> `COMPRESSED_PUBLIC_KEY_SIZE`; structural extraction of the same SEC1 compressed P-256 byte length.
- Upstream P-256 prime BigInt literal -> decimal string `P256_P`; same numeric value, chosen for BouncyCastle `BigInteger(string)` parsing.
- Upstream coefficient `b` BigInt hex literal -> hex string `P256_B`; same numeric value, parsed with radix 16.
- Upstream `a = p - BigInt(3)` -> `P256_A_OFFSET = "3"`; preserves the same subtraction in consuming code.
- Upstream inline literals -> named C# constants; changes organization only, not wire bytes produced by consumers.

**D. D17 Enforcement Check**

N/A (not Crypto.cs)

**E. Logic Divergence Findings**

Zero entries for this constants-only file. I checked the constant values against upstream `ts-source/crypto.ts:426-453` and the compressed-key `33` usage against `ts-source/turnkey.ts:78-85` / `ts-source/crypto.ts:430`.

**F. Fixture Comparison Gate**

| Fixture | Exercises this file | Upstream provenance | C# assertion | Gate |
|---|---|---|---|---|
| `tests/Fixtures/crypto/turnkey-crypto-vectors.json` | `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET` through credential-bundle decrypt and public-key uncompress paths | Provenance says `@turnkey/crypto@2.8.8`, sha256 `75115706...cf79d3`, source `ts-source/__tests__/crypto-test.ts` | `tests/CryptoTests.cs:524-537` asserts the same decrypted hex bytes as upstream lines `179-184`; `tests/CryptoTests.cs:540-548` asserts the same invalid-prefix error as upstream lines `241-248`; length/error and compression relation tests cover the same upstream behavior | all fixtures match |

Note: this fixture is marked `upstream-test-vectors`, not `node-generated`; it is copied from the pinned upstream snapshot. For deterministic outputs, the C# assertions compare exact bytes. For randomized upstream keypair cases, the C# tests assert the same byte relation Node asserts rather than fixed random bytes.

**G. Unresolved Assumptions**

- I did not run `dotnet test` in this read-only sandbox because build/test normally writes `bin/` and `obj/`. This round is verified by static source, fixture, and upstream snapshot comparison.
- I treated `ts-source/` in the pinned snapshot as the line-reference source for review readability; the tarball checksum remains the package pin.  


tokens used
230,490
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| No public/internal helper methods declared (`src/CryptoConstants.cs:32-74`) | N/A | REVIEWED | File contains only constants: `CURVE_NAME`, `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET`. |

**C. Intentional Adaptations**

- `@noble/curves/p256` implicit curve -> explicit BouncyCastle curve name `secp256r1`; structural registry selection for the same P-256 domain.
- Upstream inline `33` compressed-key length -> `COMPRESSED_PUBLIC_KEY_SIZE`; structural extraction of the same SEC1 compressed P-256 byte length.
- Upstream P-256 prime BigInt literal -> decimal string `P256_P`; same numeric value, chosen for BouncyCastle `BigInteger(string)` parsing.
- Upstream coefficient `b` BigInt hex literal -> hex string `P256_B`; same numeric value, parsed with radix 16.
- Upstream `a = p - BigInt(3)` -> `P256_A_OFFSET = "3"`; preserves the same subtraction in consuming code.
- Upstream inline literals -> named C# constants; changes organization only, not wire bytes produced by consumers.

**D. D17 Enforcement Check**

N/A (not Crypto.cs)

**E. Logic Divergence Findings**

Zero entries for this constants-only file. I checked the constant values against upstream `ts-source/crypto.ts:426-453` and the compressed-key `33` usage against `ts-source/turnkey.ts:78-85` / `ts-source/crypto.ts:430`.

**F. Fixture Comparison Gate**

| Fixture | Exercises this file | Upstream provenance | C# assertion | Gate |
|---|---|---|---|---|
| `tests/Fixtures/crypto/turnkey-crypto-vectors.json` | `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET` through credential-bundle decrypt and public-key uncompress paths | Provenance says `@turnkey/crypto@2.8.8`, sha256 `75115706...cf79d3`, source `ts-source/__tests__/crypto-test.ts` | `tests/CryptoTests.cs:524-537` asserts the same decrypted hex bytes as upstream lines `179-184`; `tests/CryptoTests.cs:540-548` asserts the same invalid-prefix error as upstream lines `241-248`; length/error and compression relation tests cover the same upstream behavior | all fixtures match |

Note: this fixture is marked `upstream-test-vectors`, not `node-generated`; it is copied from the pinned upstream snapshot. For deterministic outputs, the C# assertions compare exact bytes. For randomized upstream keypair cases, the C# tests assert the same byte relation Node asserts rather than fixed random bytes.

**G. Unresolved Assumptions**

- I did not run `dotnet test` in this read-only sandbox because build/test normally writes `bin/` and `obj/`. This round is verified by static source, fixture, and upstream snapshot comparison.
- I treated `ts-source/` in the pinned snapshot as the line-reference source for review readability; the tarball checksum remains the package pin.  


