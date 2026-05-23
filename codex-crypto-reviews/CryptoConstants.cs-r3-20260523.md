# Codex review — src/CryptoConstants.cs — round 3 — 20260523

C# SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
Upstream snapshot: `turnkey-crypto-2.8.8`

---

2026-05-23T01:28:58.608024Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e5273-25f0-7591-b7c8-745551aabfcd
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 3 of 3 for this file.

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

2026-05-23T01:28:58.938972Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll treat this as a strict parity review against the pinned local npm snapshot. I’m going to read the review skill, then inspect the C# file, upstream TS sources, checksum pin, and relevant fixtures/tests before making any claims.
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
/bin/zsh -lc 'rg -n "turnkey|csharp|sdk|CryptoConstants|crypto" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
278:## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted
282:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
288:- PR #330, plans/plans-peak-sdk-csharp.md, docs(plans): add peak-sdk-csharp migration plan, review the new plan to implement csharp sdk, inline comments, TurnkeyJsonContext, submodule init, csharp-publish.yml
294:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, latest verified head `a1adb9a6ef183151430b7b106e3c6bafce649e5e`)
300:- resolve可能であればresolveし、追加の指摘があれば指摘追加してください, verify latest head, resolve thread, locally packed .nupkg, feature/plan-peak-sdk-csharp, reviewer waiting
304:- when reviewing a plan/PR, the user asked: "review the new plan to implement csharp sdk" -> default to a PR-style review with concrete findings, not abstract advice [Task 1]
310:- For `peak` doc-plan reviews, read the plan plus nearby repo guidance before commenting; `docs/development-workflow.md`, `docs/sdk-coding-conventions.md`, `docs/sdk-naming-conventions.md`, `docs/sdk-error-handling.md`, and `docs/peak-sdk-unity-internal/README.md` were the useful mismatch detectors [Task 1]
312:- The stable final plan shape after the verify-resolve loop was PR1 build/test only, PR5 locally packed `.nupkg`, PR7 independent `csharp-publish.yml`, and PR4.5 splitting secure-storage adapters between core and Unity packages [Task 2]

exec
/bin/zsh -lc 'git status --short && git rev-parse --show-toplevel && git ls-files src/CryptoConstants.cs tests/Fixtures codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex
The review skill is oriented around change reviews, but this task is a source-equivalence audit for one file, so I’m applying its finding-first discipline without staging or modifying anything. Git is emitting macOS/Xcode cache warnings under the read-only sandbox; I’ll avoid depending on writey git operations where possible.
 succeeded in 4542ms:
2026-05-23 10:29:14.176 xcodebuild[40109:106255370]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:29:14.385 xcodebuild[40109:106255369] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-GzkXZLYi' (errno=Operation not permitted)
2026-05-23 10:29:14.796 xcodebuild[40112:106255390]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:29:14.913 xcodebuild[40112:106255389] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
?? codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md
?? codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-LUbm5Qqz' (errno=Operation not permitted)
2026-05-23 10:29:15.731 xcodebuild[40119:106255441]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:29:15.848 xcodebuild[40119:106255440] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-azKsYc3l' (errno=Operation not permitted)
2026-05-23 10:29:16.214 xcodebuild[40126:106255498]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:29:16.334 xcodebuild[40126:106255493] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-YpIfauDt' (errno=Operation not permitted)
2026-05-23 10:29:17.146 xcodebuild[40134:106255616]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:29:17.268 xcodebuild[40134:106255615] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-22cu3CLE' (errno=Operation not permitted)
2026-05-23 10:29:17.615 xcodebuild[40136:106255638]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:29:17.739 xcodebuild[40136:106255637] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
src/CryptoConstants.cs
tests/Fixtures/README.md
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
tests/Fixtures/crypto/turnkey-crypto-vectors.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json

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
/bin/zsh -lc "find codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8 -maxdepth 3 -type f | sort | sed -n '1,200p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc 'rg -n "CryptoConstants|CURVE_NAME|COMPRESSED_PUBLIC_KEY_SIZE|P256_P|P256_B|P256_A_OFFSET|uncompressRawPublicKey|compressedKey.length|secp256r1|p256" -S .' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
./src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./src/CryptoConstants.cs:14://     `crypto.ts uncompressRawPublicKey`:
./src/CryptoConstants.cs:32:    public static class CryptoConstants
./src/CryptoConstants.cs:35:        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./src/CryptoConstants.cs:43:        /// <c>compressedKey.length === 33</c> checks.
./src/CryptoConstants.cs:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./src/CryptoConstants.cs:51:        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
./src/CryptoConstants.cs:58:        public const string P256_P =
./src/CryptoConstants.cs:66:        public const string P256_B =
./src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
./src/Crypto.cs:135:            Secp256k1,
./src/Crypto.cs:355:            // Upstream uses noble's p256.getPublicKey which validates that
./src/Crypto.cs:542:        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./src/Crypto.cs:572:            else // Secp256k1
./src/Crypto.cs:620:            // Upstream then uncompressRawPublicKey() the compressed part. If
./src/Crypto.cs:623:            // raw slice through uncompressRawPublicKey which enforces 33-byte
./src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./src/Crypto.cs:928:            // Upstream noble p256.getSharedSecret validates the private scalar.
./src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./src/Crypto.cs:1049:            // Upstream noble p256.verify accepts r||s of length 64. Anything
./src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./src/turnkey-sdk-csharp.csproj:18:    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
./src/ApiKeyStamper.cs:158:            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./src/ApiKeyStamper.cs:177:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./src/ApiKeyStamper.cs:204:            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
./tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
./tests/CryptoTests.cs:294:            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./tests/CryptoTests.cs:451:            // Upstream passes the slice to uncompressRawPublicKey which
./tests/CryptoTests.cs:503:        // Curve.Secp256k1 — UncompressRawPublicKey
./tests/CryptoTests.cs:507:        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./tests/CryptoTests.cs:509:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./tests/CryptoTests.cs:513:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./tests/PublicApi.expected.txt:61:            Secp256k1 = 1,
./tests/PublicApi.expected.txt:121:    public static class CryptoConstants
./tests/PublicApi.expected.txt:123:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./tests/PublicApi.expected.txt:124:        public const string CURVE_NAME = "secp256r1";
./tests/PublicApi.expected.txt:125:        public const string P256_A_OFFSET = "3";
./tests/PublicApi.expected.txt:126:        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
./tests/PublicApi.expected.txt:127:        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./plans/plan-v2-codex-reviewed.md:24:| Q6 | **Unity SDK class-name and method-name parity** preserved. `Turnkey.Encoding` / `Turnkey.Crypto` / `Turnkey.ApiKeyStamper` / `Turnkey.Http` remain the public surface. `CryptoConstants` (was `UnityConstants`) renamed but stays a **public static class** with same constants. | Existing peak-sdk-unity / turnkey-sdk-unity downstream code can later be re-targeted via `using` alias if needed. |
./plans/plan-v2-codex-reviewed.md:121:│   ├── CryptoConstants.cs                          RENAME — Unity UnityConstants.cs renamed.
./plans/plan-v2-codex-reviewed.md:122:│   │                                                 Same public surface (CURVE_NAME, COMPRESSED_PUBLIC_KEY_SIZE,
./plans/plan-v2-codex-reviewed.md:123:│   │                                                 P256_P, P256_B, P256_A_OFFSET). Same namespace Turnkey.
./plans/plan-v2-codex-reviewed.md:164:│   │   ├── p256-nist-cavp.json                     — NIST CAVP P-256 vectors
./plans/plan-v2-codex-reviewed.md:203:│   ├── CryptoConstants.cs-r{1,2,3}-YYYYMMDD.md
./plans/plan-v2-codex-reviewed.md:344:6. Port `CryptoConstants.cs`. Codex r1 → r2 → r3.
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:1:# Codex review — src/CryptoConstants.cs — round 3 — 20260523
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:22:File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:34:B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:301:/bin/zsh -lc 'rg -n "turnkey|csharp|sdk|CryptoConstants|crypto" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:313:/bin/zsh -lc 'git status --short && git rev-parse --show-toplevel && git ls-files src/CryptoConstants.cs tests/Fixtures codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:323:?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:324:?? codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:325:?? codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:344:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:351:/bin/zsh -lc 'nl -ba src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:362:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:363:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:365:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:366:    14	//     `crypto.ts uncompressRawPublicKey`:
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:384:    32	    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:387:    35	        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:390:    38	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:395:    43	        /// <c>compressedKey.length === 33</c> checks.
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:397:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:403:    51	        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:410:    58	        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:418:    66	        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:425:    73	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:560:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:642:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:777:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:783:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:808:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:845:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:848:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:908:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/CryptoConstants.cs-r3-20260523.md:1044:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-codex-findings-reconciliation.md:32:upstream `purejs.ts` call `p256.sign(hash, privateKey)` would emit high-S
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1:# Codex review — src/CryptoConstants.cs — round 1 — 20260523
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:22:File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:34:B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:301:/bin/zsh -lc 'rg -n "turnkey|C# SDK|csharp|CryptoConstants|turnkey-sdk-csharp" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:316:/bin/zsh -lc 'nl -ba src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:327:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:328:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:330:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:331:    14	//     `crypto.ts uncompressRawPublicKey`:
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:349:    32	    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:352:    35	        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:355:    38	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:360:    43	        /// <c>compressedKey.length === 33</c> checks.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:362:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:368:    51	        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:375:    58	        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:383:    66	        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:390:    73	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:409:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:487:?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:516:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:519:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:579:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:715:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:757:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:839:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:974:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:980:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1005:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1047:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1148:/bin/zsh -lc "rg -n \"uncompressRawPublicKey|compressRawPublicKey|compressedKey.length|BigInt\\(\\\"0xffffffff|BigInt\\(\\\"0x5ac|p - BigInt|33\" codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1192:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1395:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:23:  uncompressRawPublicKey,
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1400:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:86:    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1401:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:545:      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1417:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:10:  uncompressRawPublicKey,
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1420:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:34:    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1421:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:70:    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1429:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:232:  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1430:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:235:      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1431:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:246:      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1432:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:255:        uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1445:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:291:    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1446:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:362:    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1448:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:426:export const uncompressRawPublicKey = (
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1510:/bin/zsh -lc "rg -n \"CryptoConstants|CURVE_NAME|COMPRESSED_PUBLIC_KEY_SIZE|P256_P|P256_B|P256_A_OFFSET\" src tests codex-crypto-reviews -g '"'!upstream-snapshots/**/package/dist/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1512:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1513:src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1514:src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1515:src/CryptoConstants.cs:32:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1516:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1517:src/CryptoConstants.cs:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1518:src/CryptoConstants.cs:58:        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1519:src/CryptoConstants.cs:66:        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1520:src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1521:tests/PublicApi.expected.txt:121:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1522:tests/PublicApi.expected.txt:123:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1523:tests/PublicApi.expected.txt:124:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1524:tests/PublicApi.expected.txt:125:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1525:tests/PublicApi.expected.txt:126:        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1526:tests/PublicApi.expected.txt:127:        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1527:tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1528:tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1529:src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1530:src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1531:src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1532:src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1533:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1534:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1535:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1536:tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1537:src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1538:codex-crypto-reviews/Http.cs-r3-20260523.md:3341:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1539:codex-crypto-reviews/Http.cs-r3-20260523.md:8534:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1540:codex-crypto-reviews/unity-source-pins.md:39:| src/CryptoConstants.cs      | packages/turnkey-sdk-unity/Runtime/UnityConstants.cs (rename) |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1541:codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1542:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:693:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1543:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:854:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1544:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1069:   565	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1545:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1070:   566	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1546:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1071:   567	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1547:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2706:   622	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1548:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3009:   925	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1549:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3110:  1026	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1550:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3167:  1083	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1551:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3303:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1552:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3681:./src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1553:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3682:./src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1554:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3683:./src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1555:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3767:./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1556:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4399:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1557:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4417:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1558:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4418:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1559:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4419:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1560:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4424:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1561:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4475:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1562:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4486:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1563:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4501:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1564:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4745:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1565:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4756:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1566:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4757:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1567:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4758:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1568:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4773:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1569:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4779:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1570:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4783:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1571:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4809:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1572:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4820:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1573:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4821:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1574:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4822:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1575:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4837:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1576:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4843:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1577:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4847:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1578:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4873:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1579:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5398:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1580:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5826:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1581:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5940:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1582:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1583:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6295:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1584:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7564:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1585:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7572:565:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1586:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7573:566:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1587:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7574:567:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1588:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7582:925:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1589:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7592:1026:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1590:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7597:1083:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1591:codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1592:codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1593:codex-crypto-reviews/Crypto.cs-r1-20260523.md:739:   327	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1594:codex-crypto-reviews/Crypto.cs-r1-20260523.md:764:   352	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1595:codex-crypto-reviews/Crypto.cs-r1-20260523.md:921:   509	            if (rawPublicKey.Length != CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1596:codex-crypto-reviews/Crypto.cs-r1-20260523.md:941:   529	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1597:codex-crypto-reviews/Crypto.cs-r1-20260523.md:942:   530	            var b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1598:codex-crypto-reviews/Crypto.cs-r1-20260523.md:943:   531	            var a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1599:codex-crypto-reviews/Crypto.cs-r1-20260523.md:991:   579	            if (encryptedBuf.Length <= CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1600:codex-crypto-reviews/Crypto.cs-r1-20260523.md:996:   584	            var compressedEncappedPublic = new byte[CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1601:codex-crypto-reviews/Crypto.cs-r1-20260523.md:997:   585	            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1602:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1001:   589	            var ciphertext = new byte[encryptedBuf.Length - CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1603:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1002:   590	            Array.Copy(encryptedBuf, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE, ciphertext, 0, ciphertext.Length);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1604:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1448:   621	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1605:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1727:   900	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1606:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1828:  1001	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1607:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1864:  1037	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1608:codex-crypto-reviews/Crypto.cs-r1-20260523.md:2431:  1060	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1609:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1610:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3430:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1611:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1612:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1613:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4647:/bin/zsh -lc "nl -ba src/CryptoConstants.cs | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1614:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1615:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1616:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4661:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1617:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4680:    32	    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1618:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4686:    38	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1619:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4693:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1620:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4706:    58	        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1621:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4714:    66	        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1622:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4721:    73	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1623:codex-crypto-reviews/Crypto.cs-r2-20260523.md:661:src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1624:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1625:codex-crypto-reviews/Crypto.cs-r2-20260523.md:663:src/CryptoConstants.cs:32:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1626:codex-crypto-reviews/Crypto.cs-r2-20260523.md:664:src/CryptoConstants.cs:35:        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1627:codex-crypto-reviews/Crypto.cs-r2-20260523.md:665:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1628:codex-crypto-reviews/Crypto.cs-r2-20260523.md:666:src/CryptoConstants.cs:58:        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1629:codex-crypto-reviews/Crypto.cs-r2-20260523.md:667:src/CryptoConstants.cs:66:        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1630:codex-crypto-reviews/Crypto.cs-r2-20260523.md:668:src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1631:codex-crypto-reviews/Crypto.cs-r2-20260523.md:677:tests/CryptoTests.cs:66:            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1632:codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1633:codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1634:codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1635:codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1636:codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1637:codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1638:codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1639:codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1640:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1023:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1641:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1228:   555	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1642:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1229:   556	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1643:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1230:   557	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1644:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1285:   612	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1645:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1571:   893	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1646:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1672:   994	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1647:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1723:  1045	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1648:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3272:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1649:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3861:src/CryptoConstants.cs:36:        /// Used with <c>ECNamedCurveTable.GetByName</c>.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1650:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3862:src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1651:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1652:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1653:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1654:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1655:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1656:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1657:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1658:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1659:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1660:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1661:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1662:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1663:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1664:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1665:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1666:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:389:    67	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1667:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1053:   134	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1668:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1114:   195	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1669:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1195:src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1670:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1196:src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1671:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1197:src/CryptoConstants.cs:6:// Upstream snapshot:
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1672:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1198:src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1673:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1199:src/CryptoConstants.cs:9:// Mapping to upstream:
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1674:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1200:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1675:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1201:src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1676:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1677:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1203:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1678:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1204:src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1679:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2990:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1680:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3093:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1681:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3094:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1682:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3259:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1683:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5414:/bin/zsh -lc 'rg -n "const string CURVE_NAME|CURVE_NAME" src/CryptoConstants.cs src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1684:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5416:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1685:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5417:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1686:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5418:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1687:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5419:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1688:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5420:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1689:codex-crypto-reviews/Http.cs-r1-20260523.md:728:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1690:codex-crypto-reviews/Http.cs-r1-20260523.md:6957:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1691:codex-crypto-reviews/Http.cs-r2-20260523.md:315:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1692:codex-crypto-reviews/Http.cs-r2-20260523.md:7369:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1693:codex-crypto-reviews/Http.cs-r2-20260523.md:7413:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1694:codex-crypto-reviews/Http.cs-r2-20260523.md:7764:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1695:codex-crypto-reviews/Http.cs-r2-20260523.md:10436:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1696:codex-crypto-reviews/Http.cs-r2-20260523.md:12016:codex-crypto-reviews/Http.cs-r2-20260523.md:7764:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1697:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1:# Codex review — src/CryptoConstants.cs — round 1 — 20260523
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1698:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:22:File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1699:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:34:B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1700:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:301:/bin/zsh -lc 'rg -n "turnkey|C# SDK|csharp|CryptoConstants|turnkey-sdk-csharp" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1701:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:316:/bin/zsh -lc 'nl -ba src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1702:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:327:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1703:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:328:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1704:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:330:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1705:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:349:    32	    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1706:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:355:    38	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1707:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:362:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1708:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:375:    58	        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1709:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:383:    66	        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1710:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:390:    73	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1711:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:409:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1712:codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:487:?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1713:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:655:   176	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1714:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1224:   145	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1715:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1285:   206	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1716:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1763:   389	    67	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1717:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2427:  1053	   134	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1718:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2488:  1114	   195	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1719:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2569:  1195	src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1720:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2570:  1196	src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1721:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2571:  1197	src/CryptoConstants.cs:6:// Upstream snapshot:
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1722:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2572:  1198	src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1723:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2573:  1199	src/CryptoConstants.cs:9:// Mapping to upstream:
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1724:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2574:  1200	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1725:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2575:  1201	src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1726:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1727:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2577:  1203	src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1728:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2578:  1204	src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1729:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4364:  2990	codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1730:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4467:  3093	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1731:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4468:  3094	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1732:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4633:  3259	codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1733:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5934:  5414	/bin/zsh -lc 'rg -n "const string CURVE_NAME|CURVE_NAME" src/CryptoConstants.cs src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1734:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5936:  5416	src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1735:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5937:  5417	src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1736:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5938:  5418	src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1737:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5939:  5419	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1738:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5940:  5420	src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1739:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7319:   655	   176	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1740:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7888:  1224	   145	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1741:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7949:  1285	   206	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1742:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8315:/bin/zsh -lc "rg -n \"GetPublicKey\\(|Uint8ArrayFromHexString|StringToBase64UrlString|Uint8ArrayToHexString|DecodeBase64UrlToString|CURVE_NAME|JsonSerializable\\(typeof\\(ApiKeyStamper\" src" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1743:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8317:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1744:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8318:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1745:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8350:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1746:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8352:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1747:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8356:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1748:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8370:src/ApiKeyStamper.cs:176:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1749:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8629:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1750:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8719:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1751:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10199:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8315:/bin/zsh -lc "rg -n \"GetPublicKey\\(|Uint8ArrayFromHexString|StringToBase64UrlString|Uint8ArrayToHexString|DecodeBase64UrlToString|CURVE_NAME|JsonSerializable\\(typeof\\(ApiKeyStamper\" src" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1752:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10204:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8370:src/ApiKeyStamper.cs:176:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1753:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10383:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17300:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1754:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11070:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1755:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11336:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2570:  1196	src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1756:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11337:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2574:  1200	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1757:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11338:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1758:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11554:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4467:  3093	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1759:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11565:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5939:  5419	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1760:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11702:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8317:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1761:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12871:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1196:src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1762:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12872:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1200:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1763:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12873:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1764:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13089:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3093:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1765:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13103:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5419:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1766:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13718:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1767:codex-crypto-reviews/README.md:11:2. `CryptoConstants.cs`
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1768:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:303:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1769:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:835:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1770:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1293:   148	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1771:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1354:   209	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1772:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1446:/bin/zsh -lc 'rg -n "ApiKeyStamper|TurnkeyStamp|JsonSourceGeneration|DefaultIgnoreCondition|PropertyNamingPolicy|Encoder|WriteIndented" src/TurnkeyJsonContext.cs src/Encoding.cs src/Crypto.cs src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1773:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2263:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1774:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2870:src/CryptoConstants.cs:19://     readable and so a Codex reviewer can confirm the bytes match.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1775:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2871:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1776:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2872:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1777:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2873:src/CryptoConstants.cs:41:        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1778:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3566:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1779:codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4361:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8629:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1780:codex-crypto-reviews/Crypto.cs-r3-20260523.md:97:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1781:codex-crypto-reviews/Crypto.cs-r3-20260523.md:970:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1782:codex-crypto-reviews/Crypto.cs-r3-20260523.md:971:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1783:codex-crypto-reviews/Crypto.cs-r3-20260523.md:972:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1784:codex-crypto-reviews/Crypto.cs-r3-20260523.md:1027:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1785:codex-crypto-reviews/Crypto.cs-r3-20260523.md:1370:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1786:codex-crypto-reviews/Crypto.cs-r3-20260523.md:1459:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1787:codex-crypto-reviews/Crypto.cs-r3-20260523.md:1516:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1788:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3396:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1789:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3397:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1790:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3398:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1791:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3453:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1792:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3801:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1793:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3890:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1794:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3947:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1795:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4108:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1796:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4109:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1797:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4110:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1798:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4128:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1799:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4132:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1800:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4136:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1801:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4218:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1802:codex-crypto-reviews/Crypto.cs-r3-20260523.md:5666:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1803:codex-crypto-reviews/Crypto.cs-r3-20260523.md:6539:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1804:codex-crypto-reviews/Crypto.cs-r3-20260523.md:6540:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1805:codex-crypto-reviews/Crypto.cs-r3-20260523.md:6541:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1806:codex-crypto-reviews/Crypto.cs-r3-20260523.md:6596:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1807:codex-crypto-reviews/Crypto.cs-r3-20260523.md:6939:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1808:codex-crypto-reviews/Crypto.cs-r3-20260523.md:7028:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1809:codex-crypto-reviews/Crypto.cs-r3-20260523.md:7085:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1810:codex-crypto-reviews/Crypto.cs-r3-20260523.md:8965:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1811:codex-crypto-reviews/Crypto.cs-r3-20260523.md:8966:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1812:codex-crypto-reviews/Crypto.cs-r3-20260523.md:8967:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1813:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9022:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1814:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9370:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1815:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9459:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1816:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9516:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1817:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9677:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1818:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9678:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1819:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9679:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1820:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9697:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1821:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9701:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1822:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9705:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1823:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9787:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1824:codex-crypto-reviews/Crypto.cs-r3-20260523.md:11828:src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1825:codex-crypto-reviews/Crypto.cs-r3-20260523.md:11989:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1826:codex-crypto-reviews/Crypto.cs-r3-20260523.md:12204:   565	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1827:codex-crypto-reviews/Crypto.cs-r3-20260523.md:12205:   566	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1828:codex-crypto-reviews/Crypto.cs-r3-20260523.md:12206:   567	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1829:codex-crypto-reviews/Crypto.cs-r3-20260523.md:13841:   622	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1830:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14144:   925	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1831:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14245:  1026	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1832:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14302:  1083	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1833:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14438:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1834:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14816:./src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1835:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14817:./src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1836:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14818:./src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1837:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14902:./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1838:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15534:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1839:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15552:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1840:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15553:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1841:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15554:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1842:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15559:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1843:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15610:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1844:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15621:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1845:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15636:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1846:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15880:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1847:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15891:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1848:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15892:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1849:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15893:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1850:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15908:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1851:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15914:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1852:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15918:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1853:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15944:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1854:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15955:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1855:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15956:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1856:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15957:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1857:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15972:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1858:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15978:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1859:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15982:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1860:codex-crypto-reviews/Crypto.cs-r3-20260523.md:16008:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1861:codex-crypto-reviews/Crypto.cs-r3-20260523.md:16533:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1862:codex-crypto-reviews/Crypto.cs-r3-20260523.md:16961:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1863:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17075:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1864:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17300:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1865:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17430:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1866:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18699:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1867:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18707:565:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1868:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18708:566:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1869:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18709:567:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1870:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18717:925:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1871:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18727:1026:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1872:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18732:1083:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1873:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20085:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1874:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20086:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1875:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20087:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1876:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20095:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1877:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20099:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1878:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20104:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:1881:`CryptoConstants.cs` is a constants-only file: no public or internal methods are declared, so the method table will explicitly record that the method surface is empty. I’m checking where each constant is consumed and which fixtures exercise those consuming paths.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2002:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2222:   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2379:   451	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2431:   503	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2435:   507	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2437:   509	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2441:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2579:    19	  "uncompressRawPublicKey": {
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2789:tests/CryptoTests.cs:509:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2961:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2964:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2975:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:2984:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3016:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3136:/bin/zsh -lc 'rg -n "public |internal |private |protected |const |static" src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3138:32:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3139:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3141:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3142:58:        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3143:66:        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3145:73:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3178:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3207:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3238:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3245:/bin/zsh -lc 'rg -n "secp256r1|p256|P-256|33|P256_P|P256_B|P256_A_OFFSET|COMPRESSED_PUBLIC_KEY_SIZE|CURVE_NAME" src/Crypto.cs src/ApiKeyStamper.cs tests/CryptoTests.cs tests/ApiKeyStamperTests.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3252:src/ApiKeyStamper.cs:158:            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3253:src/ApiKeyStamper.cs:177:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3254:src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3255:src/ApiKeyStamper.cs:204:            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3259:tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3268:tests/CryptoTests.cs:294:            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3288:tests/CryptoTests.cs:503:        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3289:tests/CryptoTests.cs:507:        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3290:tests/CryptoTests.cs:509:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3291:tests/CryptoTests.cs:513:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3298:tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3300:tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3307:src/Crypto.cs:135:            Secp256k1,
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3310:src/Crypto.cs:355:            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3319:src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3320:src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3321:src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3322:src/Crypto.cs:572:            else // Secp256k1
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3327:src/Crypto.cs:623:            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3328:src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3336:src/Crypto.cs:928:            // Upstream noble p256.getSharedSecret validates the private scalar.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3339:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3341:src/Crypto.cs:1049:            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3342:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3343:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3346:/bin/zsh -lc "sed -n '1,240p' codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3348:# Codex review — src/CryptoConstants.cs — round 1 — 20260523
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3369:File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3381:B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3642:| No public/internal helper methods declared (`src/CryptoConstants.cs:32-74`) | N/A | REVIEWED | File contains only constants: `CURVE_NAME`, `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET`. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3646:- `@noble/curves/p256` implicit curve -> explicit BouncyCastle curve name `secp256r1`; structural registry selection for the same P-256 domain.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3647:- Upstream inline `33` compressed-key length -> `COMPRESSED_PUBLIC_KEY_SIZE`; structural extraction of the same SEC1 compressed P-256 byte length.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3648:- Upstream P-256 prime BigInt literal -> decimal string `P256_P`; same numeric value, chosen for BouncyCastle `BigInteger(string)` parsing.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3649:- Upstream coefficient `b` BigInt hex literal -> hex string `P256_B`; same numeric value, parsed with radix 16.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3650:- Upstream `a = p - BigInt(3)` -> `P256_A_OFFSET = "3"`; preserves the same subtraction in consuming code.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3665:| `tests/Fixtures/crypto/turnkey-crypto-vectors.json` | `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET` through credential-bundle decrypt and public-key uncompress paths | Provenance says `@turnkey/crypto@2.8.8`, sha256 `75115706...cf79d3`, source `ts-source/__tests__/crypto-test.ts` | `tests/CryptoTests.cs:524-537` asserts the same decrypted hex bytes as upstream lines `179-184`; `tests/CryptoTests.cs:540-548` asserts the same invalid-prefix error as upstream lines `241-248`; length/error and compression relation tests cover the same upstream behavior | all fixtures match |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3688:| No public/internal helper methods declared (`src/CryptoConstants.cs:32-74`) | N/A | REVIEWED | File contains only constants: `CURVE_NAME`, `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET`. |
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3692:- `@noble/curves/p256` implicit curve -> explicit BouncyCastle curve name `secp256r1`; structural registry selection for the same P-256 domain.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3693:- Upstream inline `33` compressed-key length -> `COMPRESSED_PUBLIC_KEY_SIZE`; structural extraction of the same SEC1 compressed P-256 byte length.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3694:- Upstream P-256 prime BigInt literal -> decimal string `P256_P`; same numeric value, chosen for BouncyCastle `BigInteger(string)` parsing.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3695:- Upstream coefficient `b` BigInt hex literal -> hex string `P256_B`; same numeric value, parsed with radix 16.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3696:- Upstream `a = p - BigInt(3)` -> `P256_A_OFFSET = "3"`; preserves the same subtraction in consuming code.
./codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md:3711:| `tests/Fixtures/crypto/turnkey-crypto-vectors.json` | `COMPRESSED_PUBLIC_KEY_SIZE`, `P256_P`, `P256_B`, `P256_A_OFFSET` through credential-bundle decrypt and public-key uncompress paths | Provenance says `@turnkey/crypto@2.8.8`, sha256 `75115706...cf79d3`, source `ts-source/__tests__/crypto-test.ts` | `tests/CryptoTests.cs:524-537` asserts the same decrypted hex bytes as upstream lines `179-184`; `tests/CryptoTests.cs:540-548` asserts the same invalid-prefix error as upstream lines `241-248`; length/error and compression relation tests cover the same upstream behavior | all fixtures match |
./codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3411:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":";;;;;;;;;AAAA;AACA;AAsDYA;AAAZ,CAAA,UAAY,OAAO,EAAA;AACjB,IAAA,OAAA,CAAA,WAAA,CAAA,GAAA,WAAuB;AACvB,IAAA,OAAA,CAAA,QAAA,CAAA,GAAA,QAAiB;AACjB,IAAA,OAAA,CAAA,YAAA,CAAA,GAAA,YAAyB;AACzB,IAAA,OAAA,CAAA,aAAA,CAAA,GAAA,aAA2B;AAC3B,IAAA,OAAA,CAAA,KAAA,CAAA,GAAA,KAAW;AACb,CAAC,EANWA,eAAO,KAAPA,eAAO,GAAA,EAAA,CAAA,CAAA;AAQnB;;;;;;;AAOG;MACU,uBAAuB,GAAG,CACrC,gBAAwB,EACxB,WAAmB,KACT;AACV,IAAA,IAAI;QACF,MAAM,WAAW,GAAGC,kBAAS,CAAC,MAAM,CAAC,gBAAgB,CAAC;AACtD,QAAA,IAAI,WAAW,CAAC,UAAU,IAAI,EAAE,EAAE;YAChC,MAAM,IAAI,KAAK,CACb,CAAA,YAAA,EAAe,WAAW,CAAC,UAAU,CAAA,sFAAA,CAAwF,CAC9H;QACH;QAEA,MAAM,wBAAwB,GAAG,WAAW,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;QACzD,MAAM,aAAa,GAAG,WAAW,CAAC,KAAK,CAAC,EAAE,CAAC;AAC3C,QAAA,MAAM,cAAc,GAAGC,6BAAsB,CAAC,wBAAwB,CAAC;QACvE,MAAM,aAAa,GAAGC,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,OAAOC,8BAAqB,CAAC,aAAa,CAAC;IAC7C;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,+BAA+B,KAAK,CAAA,CAAE,CAAC;IACzD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;MACU,mBAAmB,GAAG,OAAO,EACxC,YAAY,EACZ,WAAW,EACX,cAAc,EACd,kCAAkC,EAClC,SAAS,EACT,cAAc,GACY,KAAqB;AAC/C,IAAA,IAAI;QACF,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;AACnD,QAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;QACD,IAAI,CAAC,QAAQ,EAAE;AACb,YAAA,MAAM,IAAI,KAAK,CACb,uCAAuC,kBAAkB,CAAA,CAAE,CAC5D;QACH;QAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CACtBC,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CACjD,CACF;QAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,YAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;YACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;QACH;AACA,QAAA,IAAI,CAAC,UAAU,CAAC,cAAc,EAAE;AAC9B,YAAA,MAAM,IAAI,KAAK,CAAC,gDAAgD,CAAC;QACnE;QAEA,MAAM,cAAc,GAAGA,gCAAuB,CAAC,UAAU,CAAC,cAAc,CAAC;QACzE,MAAM,aAAa,GAAGA,gCAAuB,CAAC,UAAU,CAAC,UAAU,CAAC;QACpE,MAAM,aAAa,GAAGF,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,IAAI,SAAS,KAAK,QAAQ,IAAI,CAAC,cAAc,EAAE;AAC7C,YAAA,IAAI,aAAa,CAAC,MAAM,KAAK,EAAE,EAAE;gBAC/B,MAAM,IAAI,KAAK,CACb,CAAA,mDAAA,EAAsD,aAAa,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;YACA,MAAM,cAAc,GAAGG,eAAO,CAAC,YAAY,CAAC,aAAa,CAAC;AAC1D,YAAA,IAAI,cAAc,CAAC,MAAM,KAAK,EAAE,EAAE;gBAChC,MAAM,IAAI,KAAK,CACb,CAAA,kDAAA,EAAqD,cAAc,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;AACA,YAAA,MAAM,iBAAiB,GAAG,IAAI,UAAU,CAAC,EAAE,CAAC;AAC5C,YAAA,iBAAiB,CAAC,GAAG,CAAC,aAAa,EAAE,CAAC,CAAC;AACvC,YAAA,iBAAiB,CAAC,GAAG,CAAC,cAAc,EAAE,EAAE,CAAC;AACzC,YAAA,OAAOC,aAAI,CAAC,MAAM,CAAC,iBAAiB,CAAC;QACvC;AAEA,QAAA,MAAM,gBAAgB,GAAGH,8BAAqB,CAAC,aAAa,CAAC;AAC7D,QAAA,OAAO,cAAc,GAAGI,mBAAU,CAAC,gBAAgB,CAAC,GAAG,gBAAgB;IACzE;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,4BAA4B,KAAK,CAAA,CAAE,CAAC;IACtD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;AACI,MAAM,oBAAoB,GAAG,OAClC,SAAiB,EACjB,SAAiB,EACjB,UAAkB,KACE;AACpB,IAAA,MAAM,eAAe,GAAGH,gCAAuB,CAAC,SAAS,CAAC;AAC1D,IAAA,MAAM,eAAe,GAAG,aAAa,CAAC,eAAe,CAAC;IACtD,IAAI,CAAC,eAAe,EAAE;AACpB,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,SAAS,CAAC;IACtD,MAAM,aAAa,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,UAAU,CAAC;AAC1D,IAAA,MAAM,UAAU,GAAGC,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,eAAe,CAAC,KAAK,EAAE,CAAC;AAC7E;AAEA;;;;;;;;AAQG;AACH,MAAM,sBAAsB,GAAG,OAC7B,mBAA2B,EAC3B,eAAuB,EACvB,UAAkB,EAClB,kCAA2C,KACvB;AACpB,IAAA,MAAM,uBAAuB,GAC3B,kCAAkC,IAAIC,2CAAiC;AACzE,IAAA,IAAI,mBAAmB,IAAI,uBAAuB,EAAE;QAClD,MAAM,IAAI,KAAK,CACb,CAAA,oBAAA,EACE,kCAAkC,IAAIA,2CACxC,CAAA,wCAAA,EAA2C,mBAAmB,CAAA,CAAE,CACjE;IACH;IAEA,MAAM,yBAAyB,GAAG,IAAI,UAAU,CAC9CP,gCAAuB,CAAC,mBAAmB,CAAC,CAC7C;AACD,IAAA,MAAM,SAAS,GAAG,aAAa,CAAC,yBAAyB,CAAC;IAC1D,IAAI,CAAC,SAAS,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,eAAe,CAAC;AAC5D,IAAA,MAAM,aAAa,GAAGJ,gCAAuB,CAAC,UAAU,CAAC;AACzD,IAAA,MAAM,UAAU,GAAGK,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,SAAS,CAAC,KAAK,EAAE,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,aAAa,GAAG,CAAC,SAAqB,KAA2B;IACrE,OAAOA,SAAI,CAAC,eAAe,CAAC,OAAO,CAACP,8BAAqB,CAAC,SAAS,CAAC,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,SAAS,GAAG,CAAC,UAAkB,EAAE,SAAc,KAAgB;IACnE,QAAQ,SAAS;AACf,QAAA,KAAK,QAAQ;YACX,MAAM,eAAe,GAAGG,aAAI,CAAC,MAAM,CAAC,UAAU,CAAC;AAC/C,YAAA,IAAI,eAAe,CAAC,MAAM,KAAK,EAAE,EAAE;gBACjC,MAAM,IAAI,KAAK,CACb,CAAA,2CAAA,EAA8C,eAAe,CAAC,MAAM,CAAA,CAAA,CAAG,CACxE;YACH;YACA,OAAO,eAAe,CAAC,QAAQ,CAAC,CAAC,EAAE,EAAE,CAAC;AACxC,QAAA,KAAK,aAAa;AAChB,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOF,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;AAC5C,QAAA;AACE,YAAA,OAAO,CAAC,IAAI,CACV,uBAAuB,SAAS,CAAA,4BAAA,CAA8B,CAC/D;AACD,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOA,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;;AAEhD,CAAC;AAED;;;;;;AAMG;MACU,yBAAyB,GAAG,OAAO,EAC9C,UAAU,EACV,SAAS,EACT,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACF,KAAqB;IACrD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,SAAS,CAAC,UAAU,EAAE,SAAS,CAAC;AACrD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACA,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;AAOK;AACE,MAAM,qBAAqB,GAAG,OAAO,EAC1C,QAAQ,EACR,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACN,KAAqB;IACjD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,QAAQ,CAAC;AACvD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACT,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;;;;;;;;;;;;;;;;;AAuBG;AACI,MAAM,yBAAyB,GAAG,OACvC,GAAW,EACX,qCAA8C,KAC1B;IACpB,MAAM,eAAe,GACnB,qCAAqC;AACrC,QAAAC,8CAAoC;;AAGtC,IAAA,MAAM,CAAC,SAAS,EAAE,UAAU,EAAE,YAAY,CAAC,GAAG,GAAG,CAAC,KAAK,CAAC,GAAG,CAAC;AAC5D,IAAA,IAAI,CAAC,YAAY;AAAE,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;AAC/D,IAAA,MAAM,YAAY,GAAG,CAAA,EAAG,SAAS,CAAA,CAAA,EAAI,UAAU,EAAE;;AAGjD,IAAA,MAAM,EAAE,GAAGL,aAAM,CAAC,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,YAAY,CAAC,CAAC;IACzD,MAAM,SAAS,GAAGA,aAAM,CAAC,EAAE,CAAC,CAAC;;IAG7B,MAAM,KAAK,GAAG,CAAC,CAAS,KACtB,CAAC,CAAC,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,EAAE,MAAM,CAClD,CAAC,CAAC,MAAM,IAAI,CAAC,CAAC,IAAI,CAAC,CAAC,MAAM,GAAG,CAAC,CAAC,IAAI,CAAC,CAAC,EACrC,GAAG,CACJ;AACH,IAAA,MAAM,SAAS,GAAG,UAAU,CAAC,IAAI,CAC/B,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;SACrB,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B,CAAC;;AAGF,IAAA,MAAM,SAAS,GAAGL,gCAAuB,CAAC,eAAe,CAAC;;IAG1D,OAAOM,SAAI,CAAC,MAAM,CAAC,SAAS,EAAE,SAAS,EAAE,SAAS,CAAC;AACrD;AAEA;;;;;;;;;;AAUG;AACI,MAAM,gBAAgB,GAAG,OAC9B,2BAAmC,EACnC,OAAe,KACQ;AACvB,IAAA,OAAO,MAAMK,uBAAgB,CAC3BX,gCAAuB,CAAC,2BAA2B,CAAC,EACpD,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,OAAO,CAAC,CAClC;AACH;AAEA;;;;;;;;;;;AAWG;AACI,MAAM,yBAAyB,GAAG,OACvC,aAAqB,EACrB,sCAA+C,KAC5B;AACnB,IAAA,OAAOD,8BAAqB,CAC1B,MAAM,gBAAgB,CACpB,sCAAsC;AACpC,QAAAa,mDAAyC,EAC3C,aAAa,CACd,CACF;AACH;AAEA;;;;;;;;;;;AAWG;MACU,mBAAmB,GAAG,CACjC,MAAc,EACd,4CAAqD,KAC3C;AACV,IAAA,OAAOhB,kBAAS,CAAC,MAAM,CACrBY,kBAAW,CAAC;QACV,YAAY,EAAE,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,MAAM,CAAC;AAC9C,QAAA,YAAY,EAAEX,6BAAsB,CAClCG,gCAAuB,CACrB,4CAA4C;AAC1C,YAAAa,8DAAoD,CACvD,CACF;AACF,KAAA,CAAC,CACH;AACH;;;;;;;;;;;;"}
./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3500:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/README.md:11:2. `CryptoConstants.cs`
./codex-crypto-reviews/unity-source-pins.md:39:| src/CryptoConstants.cs      | packages/turnkey-sdk-unity/Runtime/UnityConstants.cs (rename) |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:739:   327	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:764:   352	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:916:   504	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:921:   509	            if (rawPublicKey.Length != CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:941:   529	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:942:   530	            var b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:943:   531	            var a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:991:   579	            if (encryptedBuf.Length <= CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE)
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:996:   584	            var compressedEncappedPublic = new byte[CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:997:   585	            Array.Copy(encryptedBuf, 0, compressedEncappedPublic, 0, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1001:   589	            var ciphertext = new byte[encryptedBuf.Length - CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE];
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1002:   590	            Array.Copy(encryptedBuf, CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE, ciphertext, 0, ciphertext.Length);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1105:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1187:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1322:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1328:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1353:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1394:   291	    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1448:   621	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1727:   900	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1828:  1001	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1864:  1037	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1894:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1958:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1987:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2092:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2376:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2431:  1060	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2519:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2522:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2582:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2718:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2759:   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:2770:   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:3025:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:3068:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:3136:   545	      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:3331:src/turnkey-sdk-csharp.csproj:18:    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:3430:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4572:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4573:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4575:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4584:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4593:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4594:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4596:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4599:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4603:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4604:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4606:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4647:/bin/zsh -lc "nl -ba src/CryptoConstants.cs | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4661:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4662:    14	//     `crypto.ts uncompressRawPublicKey`:
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4680:    32	    public static class CryptoConstants
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4683:    35	        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4686:    38	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4691:    43	        /// <c>compressedKey.length === 33</c> checks.
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4693:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4699:    51	        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4706:    58	        public const string P256_P =
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4714:    66	        public const string P256_B =
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4721:    73	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4822:   134	    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4847:   159	    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4857:   169	    return p256.p256.ProjectivePoint.fromHex(encoding.uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4962:   274	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4991:   303	    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:454:   135	            Secp256k1,
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:602:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:610:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:612:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:621:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:632:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:693:src/CryptoConstants.cs
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:854:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1043:   539	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1069:   565	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1070:   566	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1071:   567	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1073:   569	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1121:   617	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1124:   620	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1130:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1212:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1347:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1353:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1378:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1419:   291	    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1490:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1554:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1583:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1688:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1937:   809	  const sharedSecretUncompressed = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1944:   816	  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1988:   860	/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2049:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2052:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2112:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2248:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2294:   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2305:   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2465:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2508:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2576:   545	      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2668:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2704:   620	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2706:   622	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3009:   925	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3102:  1018	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3110:  1026	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3167:  1083	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3303:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3659:  "uncompressRawPublicKey": {
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3681:./src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3682:./src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3683:./src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3767:./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3848:./tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3923:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:602:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3931:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:610:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3933:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:612:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3942:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:621:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3953:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:632:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:3967:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4077:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1347:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4363:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:746:tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4399:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4417:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4418:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4419:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4424:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4475:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4486:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4501:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4582:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1990:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4704:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3677:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4745:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4756:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4757:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4758:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4773:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4779:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4783:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4809:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4820:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4821:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4822:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4837:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4843:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4847:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4873:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4899:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4901:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4643:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4907:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4649:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4915:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4657:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4920:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4662:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4924:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4666:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4927:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4929:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4671:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4931:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4673:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4972:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4714:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4984:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4726:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4988:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4730:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4991:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4993:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4735:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5019:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4761:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5141:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5411:5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5155:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5425:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5185:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5456:5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5199:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5470:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5230:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5514:| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5244:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5528:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5274:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5601:| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5288:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5615:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5398:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5449:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1322:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5826:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5940:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5949:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4572:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5950:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4573:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5951:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5952:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4575:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5955:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5960:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4584:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5969:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4593:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5970:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4594:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5971:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5972:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4596:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5973:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5975:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4599:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5979:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4603:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5980:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4604:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5981:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5982:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4606:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6017:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6031:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6059:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6071:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6085:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6113:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6218:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":";;;;;;;;;AAAA;AACA;AAsDYA;AAAZ,CAAA,UAAY,OAAO,EAAA;AACjB,IAAA,OAAA,CAAA,WAAA,CAAA,GAAA,WAAuB;AACvB,IAAA,OAAA,CAAA,QAAA,CAAA,GAAA,QAAiB;AACjB,IAAA,OAAA,CAAA,YAAA,CAAA,GAAA,YAAyB;AACzB,IAAA,OAAA,CAAA,aAAA,CAAA,GAAA,aAA2B;AAC3B,IAAA,OAAA,CAAA,KAAA,CAAA,GAAA,KAAW;AACb,CAAC,EANWA,eAAO,KAAPA,eAAO,GAAA,EAAA,CAAA,CAAA;AAQnB;;;;;;;AAOG;MACU,uBAAuB,GAAG,CACrC,gBAAwB,EACxB,WAAmB,KACT;AACV,IAAA,IAAI;QACF,MAAM,WAAW,GAAGC,kBAAS,CAAC,MAAM,CAAC,gBAAgB,CAAC;AACtD,QAAA,IAAI,WAAW,CAAC,UAAU,IAAI,EAAE,EAAE;YAChC,MAAM,IAAI,KAAK,CACb,CAAA,YAAA,EAAe,WAAW,CAAC,UAAU,CAAA,sFAAA,CAAwF,CAC9H;QACH;QAEA,MAAM,wBAAwB,GAAG,WAAW,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;QACzD,MAAM,aAAa,GAAG,WAAW,CAAC,KAAK,CAAC,EAAE,CAAC;AAC3C,QAAA,MAAM,cAAc,GAAGC,6BAAsB,CAAC,wBAAwB,CAAC;QACvE,MAAM,aAAa,GAAGC,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,OAAOC,8BAAqB,CAAC,aAAa,CAAC;IAC7C;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,+BAA+B,KAAK,CAAA,CAAE,CAAC;IACzD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;MACU,mBAAmB,GAAG,OAAO,EACxC,YAAY,EACZ,WAAW,EACX,cAAc,EACd,kCAAkC,EAClC,SAAS,EACT,cAAc,GACY,KAAqB;AAC/C,IAAA,IAAI;QACF,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;AACnD,QAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;QACD,IAAI,CAAC,QAAQ,EAAE;AACb,YAAA,MAAM,IAAI,KAAK,CACb,uCAAuC,kBAAkB,CAAA,CAAE,CAC5D;QACH;QAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CACtBC,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CACjD,CACF;QAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,YAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;YACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;QACH;AACA,QAAA,IAAI,CAAC,UAAU,CAAC,cAAc,EAAE;AAC9B,YAAA,MAAM,IAAI,KAAK,CAAC,gDAAgD,CAAC;QACnE;QAEA,MAAM,cAAc,GAAGA,gCAAuB,CAAC,UAAU,CAAC,cAAc,CAAC;QACzE,MAAM,aAAa,GAAGA,gCAAuB,CAAC,UAAU,CAAC,UAAU,CAAC;QACpE,MAAM,aAAa,GAAGF,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,IAAI,SAAS,KAAK,QAAQ,IAAI,CAAC,cAAc,EAAE;AAC7C,YAAA,IAAI,aAAa,CAAC,MAAM,KAAK,EAAE,EAAE;gBAC/B,MAAM,IAAI,KAAK,CACb,CAAA,mDAAA,EAAsD,aAAa,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;YACA,MAAM,cAAc,GAAGG,eAAO,CAAC,YAAY,CAAC,aAAa,CAAC;AAC1D,YAAA,IAAI,cAAc,CAAC,MAAM,KAAK,EAAE,EAAE;gBAChC,MAAM,IAAI,KAAK,CACb,CAAA,kDAAA,EAAqD,cAAc,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;AACA,YAAA,MAAM,iBAAiB,GAAG,IAAI,UAAU,CAAC,EAAE,CAAC;AAC5C,YAAA,iBAAiB,CAAC,GAAG,CAAC,aAAa,EAAE,CAAC,CAAC;AACvC,YAAA,iBAAiB,CAAC,GAAG,CAAC,cAAc,EAAE,EAAE,CAAC;AACzC,YAAA,OAAOC,aAAI,CAAC,MAAM,CAAC,iBAAiB,CAAC;QACvC;AAEA,QAAA,MAAM,gBAAgB,GAAGH,8BAAqB,CAAC,aAAa,CAAC;AAC7D,QAAA,OAAO,cAAc,GAAGI,mBAAU,CAAC,gBAAgB,CAAC,GAAG,gBAAgB;IACzE;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,4BAA4B,KAAK,CAAA,CAAE,CAAC;IACtD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;AACI,MAAM,oBAAoB,GAAG,OAClC,SAAiB,EACjB,SAAiB,EACjB,UAAkB,KACE;AACpB,IAAA,MAAM,eAAe,GAAGH,gCAAuB,CAAC,SAAS,CAAC;AAC1D,IAAA,MAAM,eAAe,GAAG,aAAa,CAAC,eAAe,CAAC;IACtD,IAAI,CAAC,eAAe,EAAE;AACpB,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,SAAS,CAAC;IACtD,MAAM,aAAa,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,UAAU,CAAC;AAC1D,IAAA,MAAM,UAAU,GAAGC,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,eAAe,CAAC,KAAK,EAAE,CAAC;AAC7E;AAEA;;;;;;;;AAQG;AACH,MAAM,sBAAsB,GAAG,OAC7B,mBAA2B,EAC3B,eAAuB,EACvB,UAAkB,EAClB,kCAA2C,KACvB;AACpB,IAAA,MAAM,uBAAuB,GAC3B,kCAAkC,IAAIC,2CAAiC;AACzE,IAAA,IAAI,mBAAmB,IAAI,uBAAuB,EAAE;QAClD,MAAM,IAAI,KAAK,CACb,CAAA,oBAAA,EACE,kCAAkC,IAAIA,2CACxC,CAAA,wCAAA,EAA2C,mBAAmB,CAAA,CAAE,CACjE;IACH;IAEA,MAAM,yBAAyB,GAAG,IAAI,UAAU,CAC9CP,gCAAuB,CAAC,mBAAmB,CAAC,CAC7C;AACD,IAAA,MAAM,SAAS,GAAG,aAAa,CAAC,yBAAyB,CAAC;IAC1D,IAAI,CAAC,SAAS,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,eAAe,CAAC;AAC5D,IAAA,MAAM,aAAa,GAAGJ,gCAAuB,CAAC,UAAU,CAAC;AACzD,IAAA,MAAM,UAAU,GAAGK,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,SAAS,CAAC,KAAK,EAAE,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,aAAa,GAAG,CAAC,SAAqB,KAA2B;IACrE,OAAOA,SAAI,CAAC,eAAe,CAAC,OAAO,CAACP,8BAAqB,CAAC,SAAS,CAAC,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,SAAS,GAAG,CAAC,UAAkB,EAAE,SAAc,KAAgB;IACnE,QAAQ,SAAS;AACf,QAAA,KAAK,QAAQ;YACX,MAAM,eAAe,GAAGG,aAAI,CAAC,MAAM,CAAC,UAAU,CAAC;AAC/C,YAAA,IAAI,eAAe,CAAC,MAAM,KAAK,EAAE,EAAE;gBACjC,MAAM,IAAI,KAAK,CACb,CAAA,2CAAA,EAA8C,eAAe,CAAC,MAAM,CAAA,CAAA,CAAG,CACxE;YACH;YACA,OAAO,eAAe,CAAC,QAAQ,CAAC,CAAC,EAAE,EAAE,CAAC;AACxC,QAAA,KAAK,aAAa;AAChB,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOF,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;AAC5C,QAAA;AACE,YAAA,OAAO,CAAC,IAAI,CACV,uBAAuB,SAAS,CAAA,4BAAA,CAA8B,CAC/D;AACD,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOA,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;;AAEhD,CAAC;AAED;;;;;;AAMG;MACU,yBAAyB,GAAG,OAAO,EAC9C,UAAU,EACV,SAAS,EACT,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACF,KAAqB;IACrD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,SAAS,CAAC,UAAU,EAAE,SAAS,CAAC;AACrD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACA,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;AAOK;AACE,MAAM,qBAAqB,GAAG,OAAO,EAC1C,QAAQ,EACR,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACN,KAAqB;IACjD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,QAAQ,CAAC;AACvD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACT,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;;;;;;;;;;;;;;;;;AAuBG;AACI,MAAM,yBAAyB,GAAG,OACvC,GAAW,EACX,qCAA8C,KAC1B;IACpB,MAAM,eAAe,GACnB,qCAAqC;AACrC,QAAAC,8CAAoC;;AAGtC,IAAA,MAAM,CAAC,SAAS,EAAE,UAAU,EAAE,YAAY,CAAC,GAAG,GAAG,CAAC,KAAK,CAAC,GAAG,CAAC;AAC5D,IAAA,IAAI,CAAC,YAAY;AAAE,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;AAC/D,IAAA,MAAM,YAAY,GAAG,CAAA,EAAG,SAAS,CAAA,CAAA,EAAI,UAAU,EAAE;;AAGjD,IAAA,MAAM,EAAE,GAAGL,aAAM,CAAC,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,YAAY,CAAC,CAAC;IACzD,MAAM,SAAS,GAAGA,aAAM,CAAC,EAAE,CAAC,CAAC;;IAG7B,MAAM,KAAK,GAAG,CAAC,CAAS,KACtB,CAAC,CAAC,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,EAAE,MAAM,CAClD,CAAC,CAAC,MAAM,IAAI,CAAC,CAAC,IAAI,CAAC,CAAC,MAAM,GAAG,CAAC,CAAC,IAAI,CAAC,CAAC,EACrC,GAAG,CACJ;AACH,IAAA,MAAM,SAAS,GAAG,UAAU,CAAC,IAAI,CAC/B,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;SACrB,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B,CAAC;;AAGF,IAAA,MAAM,SAAS,GAAGL,gCAAuB,CAAC,eAAe,CAAC;;IAG1D,OAAOM,SAAI,CAAC,MAAM,CAAC,SAAS,EAAE,SAAS,EAAE,SAAS,CAAC;AACrD;AAEA;;;;;;;;;;AAUG;AACI,MAAM,gBAAgB,GAAG,OAC9B,2BAAmC,EACnC,OAAe,KACQ;AACvB,IAAA,OAAO,MAAMK,uBAAgB,CAC3BX,gCAAuB,CAAC,2BAA2B,CAAC,EACpD,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,OAAO,CAAC,CAClC;AACH;AAEA;;;;;;;;;;;AAWG;AACI,MAAM,yBAAyB,GAAG,OACvC,aAAqB,EACrB,sCAA+C,KAC5B;AACnB,IAAA,OAAOD,8BAAqB,CAC1B,MAAM,gBAAgB,CACpB,sCAAsC;AACpC,QAAAa,mDAAyC,EAC3C,aAAa,CACd,CACF;AACH;AAEA;;;;;;;;;;;AAWG;MACU,mBAAmB,GAAG,CACjC,MAAc,EACd,4CAAqD,KAC3C;AACV,IAAA,OAAOhB,kBAAS,CAAC,MAAM,CACrBY,kBAAW,CAAC;QACV,YAAY,EAAE,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,MAAM,CAAC;AAC9C,QAAA,YAAY,EAAEX,6BAAsB,CAClCG,gCAAuB,CACrB,4CAA4C;AAC1C,YAAAa,8DAAoD,CACvD,CACF;AACF,KAAA,CAAC,CACH;AACH;;;;;;;;;;;;"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6219:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6242:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:348:        targetKeyBuf: uncompressRawPublicKey(uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6282:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6295:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6324:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3411:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6333:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3500:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6374:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:139: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6427:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:48: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6435:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6451:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:117: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6468:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6495:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:219: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7368:   409	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7420:   461	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7424:   465	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7426:   467	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7430:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7564:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7572:565:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7573:566:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7574:567:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7582:925:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7592:1026:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7597:1083:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7624:    10	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7648:    34	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7684:    70	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7846:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7849:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7860:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7869:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7939:Error: Cannot find module '@noble/curves/p256'
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8156:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8161:     2	 * NIST secp256r1 aka p256.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8172:    13	const Fp256 = Field(BigInt('0xffffffff00000001000000000000000000000000ffffffffffffffffffffffff'));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8173:    14	const CURVE_A = Fp256.create(BigInt('-3'));
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8177:    18	 * secp256r1 curve, ECDSA and ECDH methods.
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8181:    22	export const p256: CurveFnWithCreate = createCurve({
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8184:    25	  Fp: Fp256,
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8191:    32	/** Alias to p256. */
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8192:    33	export const secp256r1: CurveFnWithCreate = p256;
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8195:    36	  mapToCurveSimpleSWU(Fp256, {
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8198:    39	    Z: Fp256.create(BigInt('-10')),
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8202:    43	  createHasher(secp256r1.ProjectivePoint, (scalars: bigint[]) => mapSWU(scalars[0]), {
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8205:    46	    p: Fp256.ORDER,
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8211:    52	/** secp256r1 hash-to-curve from RFC 9380. */
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8213:    54	/** secp256r1 encode-to-curve from RFC 9380. */
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8780:| `UncompressRawPublicKey` (`src/Crypto.cs:543`) | `ts-source/crypto.ts:426` | REVIEWED | P-256/secp256k1 math reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8819:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8820:| `src/Crypto.cs:923` | `ts-source/crypto.ts:559` | Same private-key validation gap exists in ECDH shared-secret derivation; upstream delegates to `p256.getSharedSecret`, which validates the private scalar. | Reuse the same validator before constructing `ECPrivateKeyParameters`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8858:| `UncompressRawPublicKey` (`src/Crypto.cs:543`) | `ts-source/crypto.ts:426` | REVIEWED | P-256/secp256k1 math reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8897:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8898:| `src/Crypto.cs:923` | `ts-source/crypto.ts:559` | Same private-key validation gap exists in ECDH shared-secret derivation; upstream delegates to `p256.getSharedSecret`, which validates the private scalar. | Reuse the same validator before constructing `ECPrivateKeyParameters`. |
./codex-crypto-reviews/Http.cs-r3-20260523.md:2956:tests/CryptoTests.cs:509:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Http.cs-r3-20260523.md:3288:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1347:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3289:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1353:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:3290:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1378:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:3295:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1944:   816	  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r3-20260523.md:3296:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1988:   860	/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:3298:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2668:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r3-20260523.md:3313:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4077:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1347:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3320:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4582:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1990:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3328:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5449:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1322:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3341:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/Http.cs-r3-20260523.md:3365:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6374:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:139: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3369:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6427:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:48: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3373:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6451:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:117: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:3382:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:219: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:4080:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7426:   467	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Http.cs-r3-20260523.md:4127:codex-crypto-reviews/Crypto.cs-r2-20260523.md:745:tests/CryptoTests.cs:467:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Http.cs-r3-20260523.md:4134:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1990:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:4135:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1996:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:4136:codex-crypto-reviews/Crypto.cs-r2-20260523.md:2021:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:4137:codex-crypto-reviews/Crypto.cs-r2-20260523.md:2423:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r3-20260523.md:4142:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3673:   467	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Http.cs-r3-20260523.md:4179:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1322:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:4180:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1328:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:4181:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1353:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:4187:codex-crypto-reviews/Crypto.cs-r1-20260523.md:2376:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r3-20260523.md:6302:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:139: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:6303:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:145: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:6304:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:160:    // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:6309:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:565:    // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r3-20260523.md:6310:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:586:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:6319:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:219: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:6320:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:225: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:6321:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:250:  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:6326:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:816:  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r3-20260523.md:6327:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:860:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:6335:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:48: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:6336:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:54: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:6348:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js:28:const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode("qos_encryption_hmac_message"); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r3-20260523.md:6359:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs:26:const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode("qos_encryption_hmac_message"); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r3-20260523.md:6364:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:117: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r3-20260523.md:6365:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:123: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r3-20260523.md:6366:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:138:    // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:6371:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:543:    // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r3-20260523.md:6372:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:564:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r3-20260523.md:8513:   158	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/Http.cs-r3-20260523.md:8532:   177	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/Http.cs-r3-20260523.md:8534:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Http.cs-r3-20260523.md:8559:   204	            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
./codex-crypto-reviews/Http.cs-r3-20260523.md:8869:    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
./codex-crypto-reviews/Http.cs-r3-20260523.md:9109:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Http.cs-r3-20260523.md:9186:  "uncompressRawPublicKey": {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md:11:const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/README.md:29:  encappedKeyBuf: uncompressRawPublicKey(encappedKeyBuf),
./tests/Fixtures/crypto/turnkey-crypto-vectors.json:19:  "uncompressRawPublicKey": {
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:389:    67	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:475:   153	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:489:   167	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:849:     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:858:    10	  const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:870:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1053:   134	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1114:   195	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1195:src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1196:src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1197:src/CryptoConstants.cs:6:// Upstream snapshot:
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1198:src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1199:src/CryptoConstants.cs:9:// Mapping to upstream:
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1200:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1201:src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1203:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1204:src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1349:src/Crypto.cs:542:        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1463:src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1881:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2188:codex-crypto-reviews/Encoding.cs-r1-20260523.md:3411:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2274:codex-crypto-reviews/Encoding.cs-r1-20260523.md:3500:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2620:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:489:   167	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2763:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:870:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2878:codex-crypto-reviews/Crypto.cs-r1-20260523.md:916:   504	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2981:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3025:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2988:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3068:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2990:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3053:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4572:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3054:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4573:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3055:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3056:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4575:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3060:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3065:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4584:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3074:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4593:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3075:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4594:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3076:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3077:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4596:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3078:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3080:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4599:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3084:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4603:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3085:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4604:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3086:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3087:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4606:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3093:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3094:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3134:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4962:   274	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3140:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4991:   303	    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3142:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3160:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3163:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3181:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3259:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3313:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1202:   529	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3415:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3116:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3422:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3159:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3473:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3475:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4643:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3481:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4649:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3489:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4657:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3494:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4662:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3498:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4666:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3501:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3503:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4671:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3505:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4673:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3546:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4714:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3558:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4726:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3562:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4730:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3565:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3567:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4735:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3593:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4761:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3601:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4887:       Curve.Secp256k1; second curve uses upstream's hardcoded
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3645:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5366:  3025	   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3654:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5425:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3669:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5470:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3684:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5528:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3696:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5615:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5170:src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5391:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5414:/bin/zsh -lc 'rg -n "const string CURVE_NAME|CURVE_NAME" src/CryptoConstants.cs src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5416:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5417:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5418:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5419:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5420:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1:# Codex review — src/CryptoConstants.cs — round 2 — 20260523
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:22:File under review (C#): src/CryptoConstants.cs  (git SHA at last commit touching it: 388a5fc768fd1215928d29f338ef069acd2eb60c)
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:34:B. Method coverage table: every public + internal helper method in src/CryptoConstants.cs
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:301:/bin/zsh -lc 'rg -n "turnkey|C# SDK|CryptoConstants|peak-sdk-csharp|csharp" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:316:/bin/zsh -lc "nl -ba src/CryptoConstants.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:327:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:328:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:330:    13	//   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:331:    14	//     `crypto.ts uncompressRawPublicKey`:
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:349:    32	    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:352:    35	        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:355:    38	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:360:    43	        /// <c>compressedKey.length === 33</c> checks.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:362:    45	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:368:    51	        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:375:    58	        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:383:    66	        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:390:    73	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:397:/bin/zsh -lc 'rg -n "turnkey-crypto|crypto|sha256|CryptoConstants" codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8 src tests' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:400:src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:401:src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:402:src/CryptoConstants.cs:14://     `crypto.ts uncompressRawPublicKey`:
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:403:src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:404:src/CryptoConstants.cs:32:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:405:src/CryptoConstants.cs:42:        /// Equivalent to <c>turnkey/crypto</c> hard-coded
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:406:src/CryptoConstants.cs:51:        /// Upstream <c>crypto.ts uncompressRawPublicKey</c> writes this as the
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:407:tests/PublicApi.expected.txt:121:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:411:tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:412:tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:415:tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:433:src/Crypto.cs:542:        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:434:src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:435:src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:436:src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:438:src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:442:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:443:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:444:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:448:src/turnkey-sdk-csharp.csproj:18:    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:457:src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:463:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:481:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:517:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:518:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:528:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:565:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":"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"}
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:577:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:598:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js.map:1:{"version":3,"file":"proof.js","sources":["../src/proof.ts"],"sourcesContent":[null],"names":["x509","uint8ArrayToHexString","CBOR","AWS_ROOT_CERT_PEM","sha256","uint8ArrayFromHexString","p256","AWS_ROOT_CERT_SHA256"],"mappings":"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"}
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:632:?? codex-crypto-reviews/CryptoConstants.cs-r1-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:633:?? codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:639:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:721:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:734:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:798:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:827:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:939:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:961:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:986:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1036:CryptoConstants.cs is unusual: it has no methods, only extracted P-256 constants used by Crypto.cs. I’m checking whether those constants are exercised indirectly by the crypto fixtures and whether any fixture claims an exact pinned-upstream byte source.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1093:/bin/zsh -lc 'rg -n "CryptoConstants|P256_P|P256_B|P256_A_OFFSET|CURVE_NAME|COMPRESSED_PUBLIC_KEY_SIZE|UncompressRawPublicKey|CompressRawPublicKey|turnkey-crypto|tarball_sha256|source" tests/Fixtures tests src' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1096:src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1097:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1098:src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1099:src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1100:src/CryptoConstants.cs:32:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1101:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1102:src/CryptoConstants.cs:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1103:src/CryptoConstants.cs:58:        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1104:src/CryptoConstants.cs:66:        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1105:src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1108:tests/PublicApi.expected.txt:121:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1109:tests/PublicApi.expected.txt:123:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1110:tests/PublicApi.expected.txt:124:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1111:tests/PublicApi.expected.txt:125:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1112:tests/PublicApi.expected.txt:126:        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1113:tests/PublicApi.expected.txt:127:        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1119:tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1120:tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1136:src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1137:src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1138:src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1139:src/Crypto.cs:625:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1142:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1143:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1144:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1147:tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1160:tests/CryptoTests.cs:503:        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1161:tests/CryptoTests.cs:507:        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1162:tests/CryptoTests.cs:513:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1192:src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1226:    19	  "uncompressRawPublicKey": {
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1325:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1545:   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1702:   451	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1754:   503	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1758:   507	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1760:   509	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1764:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1956:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1959:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1970:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:1979:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2005:/bin/zsh -lc "rg -n \"HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|Hpke|HKDF|Hkdf|GeneratePublicKey|Ed25519PrivateKeyParameters|ECDH|ECDSA|AesGcm|GcmBlockCipher|BigInteger|ECPoint\" src/CryptoConstants.cs src/Crypto.cs src/*.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2018:src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2030:src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2059:src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2060:src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2061:src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2119:src/Crypto.cs:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2120:src/Crypto.cs:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2121:src/Crypto.cs:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2153:/bin/zsh -lc "git log --format='%H %ad %s' --date=iso -- src/CryptoConstants.cs | sed -n '1,8p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2161:388a5fc768fd1215928d29f338ef069acd2eb60c 2026-05-23 08:55:27 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2240:   148	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2301:   209	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2350:   121	    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2352:   123	        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2353:   124	        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2354:   125	        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2355:   126	        public const string P256_B = "5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2356:   127	        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2406:/bin/zsh -lc "rg -n \"\\b(public|internal)\\b|\\(\" src/CryptoConstants.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2411:32:    public static class CryptoConstants
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2412:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2414:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2417:58:        public const string P256_P =
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2419:66:        public const string P256_B =
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2421:73:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2461:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2499:   542	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2525:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2526:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2527:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2529:   572	            else // Secp256k1
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2577:   620	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2580:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2582:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2615:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2624:  1049	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2632:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2642:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2664:| N/A - no public or internal methods declared (`src/CryptoConstants.cs:32`) | N/A - constants map to inline upstream values in `ts-source/crypto.ts:2`, `ts-source/crypto.ts:430`, `ts-source/crypto.ts:447-453` | REVIEWED | File contains only `public const` fields. Field parity is reviewed below. |
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2668:- `CURVE_NAME = "secp256r1"` replaces upstream implicit `p256` noble import: structural BouncyCastle registry name for the same NIST P-256 curve.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2669:- `COMPRESSED_PUBLIC_KEY_SIZE = 33` hoists upstream literal `rawPublicKey.length !== 33`: same SEC1 compressed P-256 byte length.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2670:- `P256_P` hoists upstream inline decimal BigInt: same prime field modulus, no byte change.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2671:- `P256_B` stores upstream hex BigInt without `0x`: consumer parses radix 16, so same coefficient bytes.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2672:- `P256_A_OFFSET = "3"` hoists upstream `p - BigInt(3)`: same derived `a` coefficient.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2683:Reviewed categories requested: algorithm step order, constants, error handling, byte ordering, leading-zero handling, padding, rounding/normalization, signature format, DTO shape, and JSON serialization. `CryptoConstants.cs` itself has no executable logic, DTOs, serialization, or signature handling. The constants match upstream P-256 length and domain parameters.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2687:| Fixture | Exercises `CryptoConstants.cs` how | Pinned upstream provenance | C# assertion parity |
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2690:| `tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json` | Indirectly uses `CURVE_NAME` for P-256 verification helpers | Different pinned package: `@turnkey/api-key-stamper@0.5.0`, not the crypto tarball | Relevant only to `CURVE_NAME`; tests verify BouncyCastle `secp256r1` accepts the fixture public key and DER ECDSA signatures. |
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2716:| N/A - no public or internal methods declared (`src/CryptoConstants.cs:32`) | N/A - constants map to inline upstream values in `ts-source/crypto.ts:2`, `ts-source/crypto.ts:430`, `ts-source/crypto.ts:447-453` | REVIEWED | File contains only `public const` fields. Field parity is reviewed below. |
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2720:- `CURVE_NAME = "secp256r1"` replaces upstream implicit `p256` noble import: structural BouncyCastle registry name for the same NIST P-256 curve.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2721:- `COMPRESSED_PUBLIC_KEY_SIZE = 33` hoists upstream literal `rawPublicKey.length !== 33`: same SEC1 compressed P-256 byte length.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2722:- `P256_P` hoists upstream inline decimal BigInt: same prime field modulus, no byte change.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2723:- `P256_B` stores upstream hex BigInt without `0x`: consumer parses radix 16, so same coefficient bytes.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2724:- `P256_A_OFFSET = "3"` hoists upstream `p - BigInt(3)`: same derived `a` coefficient.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2735:Reviewed categories requested: algorithm step order, constants, error handling, byte ordering, leading-zero handling, padding, rounding/normalization, signature format, DTO shape, and JSON serialization. `CryptoConstants.cs` itself has no executable logic, DTOs, serialization, or signature handling. The constants match upstream P-256 length and domain parameters.
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2739:| Fixture | Exercises `CryptoConstants.cs` how | Pinned upstream provenance | C# assertion parity |
./codex-crypto-reviews/CryptoConstants.cs-r2-20260523.md:2742:| `tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json` | Indirectly uses `CURVE_NAME` for P-256 verification helpers | Different pinned package: `@turnkey/api-key-stamper@0.5.0`, not the crypto tarball | Relevant only to `CURVE_NAME`; tests verify BouncyCastle `secp256r1` accepts the fixture public key and DER ECDSA signatures. |
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:303:src/CryptoConstants.cs
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:814:   158	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:833:   177	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:835:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:860:   204	            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1075:     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1084:    10	  const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1096:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1293:   148	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1354:   209	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:1446:/bin/zsh -lc 'rg -n "ApiKeyStamper|TurnkeyStamp|JsonSourceGeneration|DefaultIgnoreCondition|PropertyNamingPolicy|Encoder|WriteIndented" src/TurnkeyJsonContext.cs src/Encoding.cs src/Crypto.cs src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2263:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2656:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2981:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3025:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2679:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3055:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2681:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3076:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2682:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3078:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2683:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3086:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2687:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3134:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4962:   274	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2692:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3160:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2698:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3181:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2722:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3415:codex-crypto-reviews/Crypto.cs-r2-20260523.md:3116:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2734:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3473:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2735:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3501:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2736:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3565:codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2741:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3645:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5366:  3025	   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2870:src/CryptoConstants.cs:19://     readable and so a Codex reviewer can confirm the bytes match.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2871:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2872:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2873:src/CryptoConstants.cs:41:        /// Length in bytes of a SEC1 compressed P-256 public key (0x02|0x03 + X).
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:2946:src/Crypto.cs:623:            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3017:tests/CryptoTests.cs:294:            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3054:tests/CryptoTests.cs:513:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3566:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3959:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4355:  2981	codex-crypto-reviews/Crypto.cs-r1-20260523.md:3025:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3982:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4429:  3055	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3984:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4450:  3076	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3985:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4452:  3078	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3986:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4460:  3086	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3990:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4508:  3134	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4962:   274	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:3995:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4534:  3160	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4001:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4555:  3181	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4361:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8629:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4420:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8711:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6059:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4422:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8713:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6113:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4533:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8828:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4534:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8829:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4620:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4363:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:746:tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4669:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4704:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3677:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4698:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4899:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4699:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4927:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4700:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:4991:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4838:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5951:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4839:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5971:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4840:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5973:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4841:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:5981:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4851:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6059:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4860:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6113:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:4961:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7430:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5020:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5021:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5059:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8156:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5082:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8819:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5090:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8897:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5223:codex-crypto-reviews/Crypto.cs-r3-20260523.md:1025:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5294:codex-crypto-reviews/Crypto.cs-r3-20260523.md:2940:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5354:codex-crypto-reviews/Crypto.cs-r3-20260523.md:3451:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5430:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4433:   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5467:codex-crypto-reviews/Crypto.cs-r3-20260523.md:4652:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5812:codex-crypto-reviews/Crypto.cs-r3-20260523.md:6594:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5883:codex-crypto-reviews/Crypto.cs-r3-20260523.md:8509:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:5943:codex-crypto-reviews/Crypto.cs-r3-20260523.md:9020:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6019:codex-crypto-reviews/Crypto.cs-r3-20260523.md:10002:   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6056:codex-crypto-reviews/Crypto.cs-r3-20260523.md:10221:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6368:codex-crypto-reviews/Crypto.cs-r3-20260523.md:12259:   620	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6404:codex-crypto-reviews/Crypto.cs-r3-20260523.md:13600:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6407:codex-crypto-reviews/Crypto.cs-r3-20260523.md:13839:   620	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6540:codex-crypto-reviews/Crypto.cs-r3-20260523.md:14983:./tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6612:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15498:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:746:tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6661:codex-crypto-reviews/Crypto.cs-r3-20260523.md:15839:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3677:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6690:codex-crypto-reviews/Crypto.cs-r3-20260523.md:16034:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6691:codex-crypto-reviews/Crypto.cs-r3-20260523.md:16062:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6692:codex-crypto-reviews/Crypto.cs-r3-20260523.md:16126:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6830:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17086:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6831:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17106:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6832:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17108:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6833:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17116:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6843:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17194:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6852:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17248:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:6953:codex-crypto-reviews/Crypto.cs-r3-20260523.md:18565:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7012:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7013:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7051:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19291:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7074:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19954:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7082:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20032:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7104:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20430:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7105:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20433:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7106:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20437:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7107:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20458:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7116:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20480:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7117:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20481:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7139:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20514:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7140:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20515:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7167:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20559:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7168:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20560:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/p256.js:31:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7169:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20561:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:78: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7170:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20562:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:90:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7171:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20563:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:253: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7183:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20575:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:19: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7184:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20576:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:67: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7186:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20578:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:70: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7187:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20579:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:81:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7188:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20580:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:224: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7210:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20603:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:31:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7211:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20604:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:82:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7212:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20605:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.d.ts:35:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7213:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20606:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.js:55:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7217:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20610:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7218:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20611:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/esm/secp256k1.js:52:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7223:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20616:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.js:26:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7250:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20644:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/p256.ts:29:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7256:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20650:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/p256.js:30:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7262:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20656:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7263:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20657:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7264:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20658:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:76: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7265:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20659:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:88:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7266:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20660:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:250: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7269:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20663:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7270:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20664:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7276:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20670:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:68: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7277:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20671:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:79:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7278:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20672:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:221: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7285:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20679:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:65: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7286:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20680:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:76:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7287:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20681:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:218: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7468:/bin/zsh -lc 'rg -n "lowS|p256" /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/abstract/weierstrass.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7470:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts:8:// NIST secp256r1 aka p256
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7471:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts:16:export const p256 = createCurve({
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7472:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7473:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts:28:export const secp256r1 = p256;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7474:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts:38:  createHasher(secp256r1.ProjectivePoint, (scalars: bigint[]) => mapSWU(scalars[0]), {
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7477:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/abstract/weierstrass.ts:38: *   `k256.Point.BASE.add(p256.Point.BASE)`
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7495:/bin/zsh -lc 'rg -n "lowS|p256" /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/p256.ts /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/abstract/weierstrass.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7497:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/p256.ts:8:// NIST secp256r1 aka p256
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7498:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/p256.ts:16:export const p256 = createCurve({
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7499:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7500:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/p256.ts:28:export const secp256r1 = p256;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7501:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/p256.ts:38:  createHasher(secp256r1.ProjectivePoint, (scalars: bigint[]) => mapSWU(scalars[0]), {
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7504:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.2/node_modules/@noble/curves/src/abstract/weierstrass.ts:38: *   `k256.Point.BASE.add(p256.Point.BASE)`
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7534:     3	var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7539:     8	    const publicKey = p256.p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7546:    15	    const signature = p256.p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7554:/bin/zsh -lc "node -e \"const {p256}=require('/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/p256'); const {createHash}=require('crypto'); const priv='487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c'; for (const msg of ['hello from TKHQ"'!'"','the same content','content_0','content_1','any payload we care to sign']) { const hash=createHash('sha256').update(msg).digest(); const sig=p256.sign(hash, priv); console.log(msg, sig.toDERHex(), 'hasHighS', sig.hasHighS && sig.hasHighS()); }\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7734:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7757:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/p256.ts | sed -n '16,30p'; nl -ba /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/src/abstract/weierstrass.ts | sed -n '1007,1014p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7759:    16	export const p256 = createCurve({
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7771:    28	export const secp256r1 = p256;
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7873:/bin/zsh -lc "node -e \"const {p256}=require('/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.4.0/node_modules/@noble/curves/p256'); const {createHash}=require('crypto'); const priv='487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c'; const hash=createHash('sha256').update('hello from TKHQ"'!'"').digest(); const sig=p256.sign(hash, priv); console.log(Object.getOwnPropertyNames(Object.getPrototypeOf(sig))); console.log(sig.normalizeS ? sig.normalizeS().toDERHex() : 'no normalizeS');\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7924:| `src/ApiKeyStamper.cs:199-213` | `ts-source/purejs.ts:22-23`, `package/package.json:51` | C# forcibly normalizes ECDSA `s` to low-S. Upstream `purejs.ts` calls `p256.sign(hash, privateKey)` with no options. The pinned package depends on `@noble/curves` `^1.3.0`; inspected compatible Noble P-256 code defaults `lowS: false`, so high-S DER signatures can be emitted. Example for `"hello from TKHQ!"`: Noble-compatible purejs emitted high-S DER `304602...b03f...`, while C# would normalize to low-S DER `304502...4fc0...`. | Remove the low-S clamp for upstream purejs byte parity, or generate/compare fixtures against an explicitly modified upstream call using `{ lowS: true }`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r3-20260523.md:7978:| `src/ApiKeyStamper.cs:199-213` | `ts-source/purejs.ts:22-23`, `package/package.json:51` | C# forcibly normalizes ECDSA `s` to low-S. Upstream `purejs.ts` calls `p256.sign(hash, privateKey)` with no options. The pinned package depends on `@noble/curves` `^1.3.0`; inspected compatible Noble P-256 code defaults `lowS: false`, so high-S DER signatures can be emitted. Example for `"hello from TKHQ!"`: Noble-compatible purejs emitted high-S DER `304602...b03f...`, while C# would normalize to low-S DER `304502...4fc0...`. | Remove the low-S clamp for upstream purejs byte parity, or generate/compare fixtures against an explicitly modified upstream call using `{ lowS: true }`. |
./codex-crypto-reviews/Http.cs-r2-20260523.md:315:src/CryptoConstants.cs
./codex-crypto-reviews/Http.cs-r2-20260523.md:7369:codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/Http.cs-r2-20260523.md:7413:codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/Http.cs-r2-20260523.md:7442:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Http.cs-r2-20260523.md:7446:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Http.cs-r2-20260523.md:7755:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6059:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Http.cs-r2-20260523.md:7758:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6113:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Http.cs-r2-20260523.md:7764:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/Http.cs-r2-20260523.md:8567:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Http.cs-r2-20260523.md:8568:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/Http.cs-r2-20260523.md:8606:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8156:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Http.cs-r2-20260523.md:9107:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17194:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Http.cs-r2-20260523.md:9110:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17248:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Http.cs-r2-20260523.md:10415:   158	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/Http.cs-r2-20260523.md:10434:   177	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/Http.cs-r2-20260523.md:10436:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Http.cs-r2-20260523.md:12016:codex-crypto-reviews/Http.cs-r2-20260523.md:7764:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js:3:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js:8:    const publicKey = p256.p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js:15:    const signature = p256.p256.sign(hash, input.privateKey);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:465:   135	            Secp256k1,
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:661:src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:663:src/CryptoConstants.cs:32:    public static class CryptoConstants
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:664:src/CryptoConstants.cs:35:        /// BouncyCastle curve registry name for NIST P-256 / secp256r1.
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:665:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:666:src/CryptoConstants.cs:58:        public const string P256_P =
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:667:src/CryptoConstants.cs:66:        public const string P256_B =
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:668:src/CryptoConstants.cs:73:        public const string P256_A_OFFSET = "3";
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:677:tests/CryptoTests.cs:66:            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:745:tests/CryptoTests.cs:467:            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:746:tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1023:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1202:   529	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1228:   555	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1229:   556	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1230:   557	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1232:   559	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1280:   607	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1283:   610	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1285:   612	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1571:   893	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1664:   986	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1672:   994	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1723:  1045	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1773:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1855:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1990:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1996:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2021:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2076:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2079:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2139:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2275:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2316:   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2327:   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2423:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2520:   291	    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2591:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2655:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2684:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:2789:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3116:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3159:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3272:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3615:   409	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3667:   461	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3671:   465	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3673:   467	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3677:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3861:src/CryptoConstants.cs:36:        /// Used with <c>ECNamedCurveTable.GetByName</c>.
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3862:src/CryptoConstants.cs:55:        /// <see cref="Org.BouncyCastle.Math.BigInteger(string)"/> radix-10
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4643:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4649:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4657:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4662:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4666:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4671:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4673:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4714:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4726:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4730:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4735:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4761:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4887:       Curve.Secp256k1; second curve uses upstream's hardcoded
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4888:       secp256k1 p / a / b. Default is P256.
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4935:    - New UncompressRawPublicKey_Secp256k1_Roundtrip test.
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5093:+            Secp256k1,
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5366:  3025	   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5411:5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5425:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5456:5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5470:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5514:| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5528:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5601:| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5615:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:6:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:169:    return p256.p256.ProjectivePoint.fromHex(encoding.uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:97:src/CryptoConstants.cs
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:537:   135	            Secp256k1,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:757:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:944:   542	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:970:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:971:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:972:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:974:   572	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1022:   620	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1025:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1027:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1330:   928	            // Upstream noble p256.getSharedSecret validates the private scalar.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1370:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1451:  1049	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1459:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1516:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1610:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1692:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1827:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1833:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1858:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1899:   291	    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1970:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2034:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2063:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2168:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2417:   809	  const sharedSecretUncompressed = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2424:   816	  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2468:   860	/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2529:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2532:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2592:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2728:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2769:   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2780:   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2940:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:2983:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3051:   545	      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3183:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3370:   542	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3396:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3397:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3398:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3400:   572	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3448:   620	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3451:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3453:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3761:   928	            // Upstream noble p256.getSharedSecret validates the private scalar.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3801:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3882:  1049	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3890:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:3947:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4043:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4108:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4109:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4110:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4128:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4132:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4136:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4218:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4433:   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4590:   451	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4642:   503	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4646:   507	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4648:   509	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4652:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4824:  "uncompressRawPublicKey": {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4854:    10	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4878:    34	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:4914:    70	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:5076:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:5079:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:5090:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:5099:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:5666:src/CryptoConstants.cs
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6106:   135	            Secp256k1,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6326:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6513:   542	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6539:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6540:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6541:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6543:   572	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6591:   620	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6594:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6596:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6899:   928	            // Upstream noble p256.getSharedSecret validates the private scalar.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:6939:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7020:  1049	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7028:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7085:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7179:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7261:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7396:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7402:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7427:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7468:   291	    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7539:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7603:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7632:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7737:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7986:   809	  const sharedSecretUncompressed = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:7993:   816	  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8037:   860	/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8098:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8101:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8161:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8297:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8338:   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8349:   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8509:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8552:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8620:   545	      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8752:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8939:   542	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8965:   568	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8966:   569	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8967:   570	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:8969:   572	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9017:   620	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9020:   623	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9022:   625	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9330:   928	            // Upstream noble p256.getSharedSecret validates the private scalar.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9370:   968	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9451:  1049	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9459:  1057	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9516:  1114	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9612:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9677:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9678:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9679:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9697:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9701:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9705:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:9787:    79	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10002:   294	            // Upstream noble p256.getPublicKey requires exactly 32 bytes.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10159:   451	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10211:   503	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10215:   507	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10217:   509	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10221:   513	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10393:  "uncompressRawPublicKey": {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10423:    10	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10447:    34	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10483:    70	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10645:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10648:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10659:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:10668:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11589:   135	            Secp256k1,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11737:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11745:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11747:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11756:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11767:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11781:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11828:src/CryptoConstants.cs
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:11989:   350	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12178:   539	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12204:   565	                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12205:   566	                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12206:   567	                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12208:   569	            else // Secp256k1
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12256:   617	            // Upstream then uncompressRawPublicKey() the compressed part. If
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12259:   620	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12265:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12347:    84	  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12482:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12488:   225	 * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12513:   250	  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12554:   291	    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12625:   362	    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12689:   426	export const uncompressRawPublicKey = (
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12718:   455	    // secp256k1 domain parameters
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:12823:   560	  const ss = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13072:   809	  const sharedSecretUncompressed = p256.getSharedSecret(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13079:   816	  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13123:   860	/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13184:    23	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13187:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13247:    86	    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13383:   222	  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13429:   263	  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13440:   274	  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13600:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13643:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13711:   545	      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13803:    28	); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13839:   620	            // raw slice through uncompressRawPublicKey which enforces 33-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:13841:   622	            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14144:   925	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14237:  1018	            // Upstream noble p256.verify accepts r||s of length 64. Anything
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14245:  1026	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14302:  1083	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14438:    66	            var p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14794:  "uncompressRawPublicKey": {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14816:./src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14817:./src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14818:./src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14902:./codex-crypto-reviews/turnkey-source-pins.md:13:| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:14983:./tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15058:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:602:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15066:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:610:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15068:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:612:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15077:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:621:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:36:        const encappedKeyBuf = crypto.uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15088:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:632:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15102:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15212:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:1347:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15498:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:746:tests/CryptoTests.cs:471:            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15534:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:784:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15552:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:802:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15553:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:803:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15554:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:804:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15559:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:809:src/Crypto.cs:612:            int compressedSize = CryptoConstants.COMPRESSED_PUBLIC_KEY_SIZE;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15610:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:860:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15621:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:871:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15636:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:886:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15717:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:1990:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15839:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3677:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15880:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3887:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15891:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3898:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15892:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3899:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15893:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3900:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15908:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3915:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15914:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3921:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15918:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3925:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15944:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3951:src/Crypto.cs:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15955:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3962:src/Crypto.cs:555:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15956:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3963:src/Crypto.cs:556:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15957:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3964:src/Crypto.cs:557:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15972:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3979:src/Crypto.cs:893:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15978:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3985:src/Crypto.cs:994:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:15982:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:3989:src/Crypto.cs:1045:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16008:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4362:388a5fc768fd1215928d29f338ef069acd2eb60c Sat May 23 08:55:27 2026 +0900 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16034:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4641:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16036:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4643:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16042:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4649:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16050:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4657:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16055:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4662:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16059:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4666:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16062:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4669:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16064:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4671:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16066:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4673:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16107:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4714:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16119:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4726:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16123:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4730:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16126:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4733:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16128:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4735:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16154:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:4761:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16276:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5411:5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16290:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5425:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16320:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5456:5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16334:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5470:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16365:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5514:| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16379:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5528:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16409:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5601:| `UncompressRawPublicKey` (`src/Crypto.cs:533`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 and secp256k1 branches reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16423:./codex-crypto-reviews/Crypto.cs-r2-20260523.md:5615:| `VerifyP256RawSignature` (`src/Crypto.cs:984`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Raw `r||s` converted to DER for BouncyCastle. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16533:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:668:388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16584:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:1322:   219	 * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:16961:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4181:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17075:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4520:   173	    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17084:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4572:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17085:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4573:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17086:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17087:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4575:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17090:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17095:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4584:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17104:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4593:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17105:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4594:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17106:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17107:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4596:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17108:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17110:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4599:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17114:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4603:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17115:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4604:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17116:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17117:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:4606:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17152:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5042:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17166:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17194:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17206:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5140:| `UncompressRawPublicKey` (`src/Crypto.cs:506`) | `uncompressRawPublicKey` (`ts-source/crypto.ts:426`) | REVIEWED | P-256 default matches; missing curve parameter/error text differ, listed in E. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17220:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17248:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17300:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17353:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17354:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17377:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:348:        targetKeyBuf: uncompressRawPublicKey(uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17417:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17430:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3029:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17459:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3411:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17468:./codex-crypto-reviews/Encoding.cs-r1-20260523.md:3500:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17509:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:139: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17562:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:48: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17570:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17586:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:117: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17603:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17630:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:350:        targetKeyBuf: crypto.uncompressRawPublicKey(encoding.uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:17714:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:219: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18503:   409	            // Upstream passes the slice to uncompressRawPublicKey which
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18555:   461	        // Curve.Secp256k1 — UncompressRawPublicKey
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18559:   465	        public void UncompressRawPublicKey_Secp256k1_Roundtrip()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18561:   467	            // secp256k1 generator point compressed = 02 79be667e f9dcbbac 55a06295 ce870b07
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18565:   471	            byte[] uncompressed = Crypto.UncompressRawPublicKey(compressed, Crypto.Curve.Secp256k1);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18699:350:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18707:565:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18708:566:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18709:567:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18717:925:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18727:1026:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18732:1083:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18759:    10	  uncompressRawPublicKey,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18783:    34	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18819:    70	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18981:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18984:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:18995:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19004:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19074:Error: Cannot find module '@noble/curves/p256'
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19291:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19296:     2	 * NIST secp256r1 aka p256.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19307:    13	const Fp256 = Field(BigInt('0xffffffff00000001000000000000000000000000ffffffffffffffffffffffff'));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19308:    14	const CURVE_A = Fp256.create(BigInt('-3'));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19312:    18	 * secp256r1 curve, ECDSA and ECDH methods.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19316:    22	export const p256: CurveFnWithCreate = createCurve({
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19319:    25	  Fp: Fp256,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19326:    32	/** Alias to p256. */
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19327:    33	export const secp256r1: CurveFnWithCreate = p256;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19330:    36	  mapToCurveSimpleSWU(Fp256, {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19333:    39	    Z: Fp256.create(BigInt('-10')),
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19337:    43	  createHasher(secp256r1.ProjectivePoint, (scalars: bigint[]) => mapSWU(scalars[0]), {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19340:    46	    p: Fp256.ORDER,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19346:    52	/** secp256r1 hash-to-curve from RFC 9380. */
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19348:    54	/** secp256r1 encode-to-curve from RFC 9380. */
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19915:| `UncompressRawPublicKey` (`src/Crypto.cs:543`) | `ts-source/crypto.ts:426` | REVIEWED | P-256/secp256k1 math reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19954:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19955:| `src/Crypto.cs:923` | `ts-source/crypto.ts:559` | Same private-key validation gap exists in ECDH shared-secret derivation; upstream delegates to `p256.getSharedSecret`, which validates the private scalar. | Reuse the same validator before constructing `ECPrivateKeyParameters`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19993:| `UncompressRawPublicKey` (`src/Crypto.cs:543`) | `ts-source/crypto.ts:426` | REVIEWED | P-256/secp256k1 math reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20032:| `src/Crypto.cs:344`, `src/Crypto.cs:364` | `ts-source/crypto.ts:80` | C# accepts non-32-byte private key inputs and does not explicitly reject scalar `0` / `>= n`; noble `p256.getPublicKey` validates exact key size and scalar range. | Add a shared P-256 private-key validator: exactly 32 bytes and scalar in `[1, n - 1]`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20033:| `src/Crypto.cs:923` | `ts-source/crypto.ts:559` | Same private-key validation gap exists in ECDH shared-secret derivation; upstream delegates to `p256.getSharedSecret`, which validates the private scalar. | Reuse the same validator before constructing `ECPrivateKeyParameters`. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20085:568:                p = new BigInteger(CryptoConstants.P256_P);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20086:569:                b = new BigInteger(CryptoConstants.P256_B, 16);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20087:570:                a = p.Subtract(new BigInteger(CryptoConstants.P256_A_OFFSET));
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20095:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20099:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20104:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20211:    19	  "uncompressRawPublicKey": {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20387:   232	  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20390:   235	      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20401:   246	      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20410:   255	        uncompressRawPublicKey(
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20418:The only potentially subtle equivalence point left is ECDSA verification policy: Turnkey’s TS path delegates to noble `p256.verify`, while C# feeds DER into BouncyCastle. I’m checking whether there is a low-S or signature-format policy mismatch before I mark E empty.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20426:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20430:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20431:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20432:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20433:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20434:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20435:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20436:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20437:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20438:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20440:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20446:/bin/zsh -lc "rg -n \"verifySessionJwtSignature|verifyEnclaveSignature|p256\\.verify|fromDerSignature\" codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20452:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20455:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20458:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20460:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20480:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20481:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20514:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20515:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20559:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20560:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/p256.js:31:    lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20561:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:78: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20562:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:90:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20563:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:253: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20575:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:19: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20576:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:67: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20578:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:70: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20579:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:81:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20580:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:224: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20603:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:31:        lowS: boolean;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20604:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:82:        lowS: boolean;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20605:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.d.ts:35:        lowS: boolean;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20606:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.js:55:    lowS: true,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20610:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20611:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/esm/secp256k1.js:52:    lowS: true,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20616:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.js:26:    lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20644:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/p256.ts:29:  lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20650:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/p256.js:30:    lowS: false,
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20656:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20657:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20658:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:76: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20659:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:88:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20660:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:250: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20663:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20664:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20670:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:68: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20671:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:79:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20672:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:221: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20679:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:65: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20680:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:76:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20681:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:218: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20705:| `UncompressRawPublicKey` (`src/Crypto.cs:546`) | `ts-source/crypto.ts:426` | REVIEWED | P-256/secp256k1 math, parity, padding reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20733:- DER ECDSA verify is performed directly by BouncyCastle instead of TS `fromDerSignature` plus `p256.verify`; digest/signature semantics are preserved.
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20788:| `UncompressRawPublicKey` (`src/Crypto.cs:546`) | `ts-source/crypto.ts:426` | REVIEWED | P-256/secp256k1 math, parity, padding reviewed. |
./codex-crypto-reviews/Crypto.cs-r3-20260523.md:20816:- DER ECDSA verify is performed directly by BouncyCastle instead of TS `fromDerSignature` plus `p256.verify`; digest/signature semantics are preserved.
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs:26:const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode("qos_encryption_hmac_message"); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:2:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:103:        p256.ProjectivePoint.fromHex(signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.js:21:exports.uncompressRawPublicKey = crypto.uncompressRawPublicKey;
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.mjs:1:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.mjs:6:    const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.mjs:13:    const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:48: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:54: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:110:export declare const uncompressRawPublicKey: (rawPublicKey: Uint8Array, curve?: Curve) => Uint8Array;
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:4:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:125:        p256.p256.ProjectivePoint.fromHex(signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js:28:const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode("qos_encryption_hmac_message"); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/index.mjs:1:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey } from './crypto.mjs';
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:173:- [#700](https://github.com/tkhq/sdk/pull/700) [`878e039`](https://github.com/tkhq/sdk/commit/878e03973856cfec83e6e3fda5b76d1b64943628) Author [@andrewkmin](https://github.com/andrewkmin) - Add validations to uncompressRawPublicKey method
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:3:import { uncompressRawPublicKey, hpkeDecrypt, fromDerSignature, hpkeEncrypt, formatHpkeBuf, quorumKeyEncrypt } from './crypto.mjs';
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:4:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:34:        const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:167:    return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:348:        targetKeyBuf: uncompressRawPublicKey(uint8ArrayFromHexString(dangerouslyOverrideOnRampEncryptionPublicKey ??
./codex-crypto-reviews/Http.cs-r1-20260523.md:728:src/CryptoConstants.cs
./codex-crypto-reviews/Http.cs-r1-20260523.md:6936:   158	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/Http.cs-r1-20260523.md:6955:   177	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/Http.cs-r1-20260523.md:6957:   179	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/Http.cs-r1-20260523.md:6982:   204	            // The upstream purejs.ts call `p256.sign(hash, privateKey)` passes
./codex-crypto-reviews/Http.cs-r1-20260523.md:9786:./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14199:| `src/ApiKeyStamper.cs:196` | `ts-source/purejs.ts:22` | C# unconditionally normalizes `s` to low-S. Upstream purejs calls `p256.sign(hash, privateKey)` with no options. The package depends on `@noble/curves` v1 range (`^1.3.0`), and noble’s release notes say low-S became default for ECDSA in v2, which implies v1 P-256 default behavior is not the same low-S policy. Source: https://github.com/paulmillr/noble-curves/releases | Remove C# low-S normalization for v1 purejs parity, or generate upstream vectors with an explicit low-S policy and document that this intentionally differs from `@turnkey/api-key-stamper@0.5.0`. |
./codex-crypto-reviews/Http.cs-r1-20260523.md:9791:./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14246:| `src/ApiKeyStamper.cs:196` | `ts-source/purejs.ts:22` | C# unconditionally normalizes `s` to low-S. Upstream purejs calls `p256.sign(hash, privateKey)` with no options. The package depends on `@noble/curves` v1 range (`^1.3.0`), and noble’s release notes say low-S became default for ECDSA in v2, which implies v1 P-256 default behavior is not the same low-S policy. Source: https://github.com/paulmillr/noble-curves/releases | Remove C# low-S normalization for v1 purejs parity, or generate upstream vectors with an explicit low-S policy and document that this intentionally differs from `@turnkey/api-key-stamper@0.5.0`. |
./codex-crypto-reviews/Http.cs-r1-20260523.md:9850:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:173:- [#700](https://github.com/tkhq/sdk/pull/700) [`878e039`](https://github.com/tkhq/sdk/commit/878e03973856cfec83e6e3fda5b76d1b64943628) Author [@andrewkmin](https://github.com/andrewkmin) - Add validations to uncompressRawPublicKey method
./codex-crypto-reviews/Http.cs-r1-20260523.md:9857:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.mjs:26:const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode("qos_encryption_hmac_message"); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r1-20260523.md:9867:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/constants.js:28:const QOS_ENCRYPTION_HMAC_MESSAGE = new TextEncoder().encode("qos_encryption_hmac_message"); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/Http.cs-r1-20260523.md:9870:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:219: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r1-20260523.md:9871:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:225: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r1-20260523.md:9872:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:250:  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r1-20260523.md:9879:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:816:  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r1-20260523.md:9880:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:860:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r1-20260523.md:9884:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:48: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r1-20260523.md:9885:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.d.ts:54: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r1-20260523.md:11745:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:139: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r1-20260523.md:11746:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:145: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r1-20260523.md:11747:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:160:    // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r1-20260523.md:11754:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:565:    // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r1-20260523.md:11755:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:586:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r1-20260523.md:11759:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:117: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/Http.cs-r1-20260523.md:11760:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:123: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/Http.cs-r1-20260523.md:11761:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:138:    // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r1-20260523.md:11768:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:543:    // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/Http.cs-r1-20260523.md:11769:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:564:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/Http.cs-r1-20260523.md:11771:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:28:); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:23:  uncompressRawPublicKey,
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:26:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:86:    const encappedKeyBuf = uncompressRawPublicKey(compressedEncappedKeyBuf);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:274:  return p256.ProjectivePoint.fromHex(uint8ArrayToHexString(publicKey));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:545:      targetKeyBuf: uncompressRawPublicKey(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:1:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:31:    return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:117: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:123: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:138:    // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:168:    const encappedKeyBufHex = uint8ArrayToHexString(uncompressRawPublicKey(compressedSenderBuf));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:219:    const publicKeyUncompressed = uint8ArrayToHexString(uncompressRawPublicKey(publicKey));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:271:const uncompressRawPublicKey = (rawPublicKey, curve = "CURVE_P256") => {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:291:        // secp256k1 domain parameters
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:364:    const ss = p256.getSharedSecret(uint8ArrayFromHexString(priv), encappedKeyBuf);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:541:    const sharedSecretUncompressed = p256.getSharedSecret(ephemeralSenderPrivate, targetPublicKeyUncompressed, false);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:543:    // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:564:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:589:export { buildAdditionalAssociatedData, compressRawPublicKey, extractPrivateKeyFromPKCS8Bytes, formatHpkeBuf, fromDerSignature, generateP256KeyPair, getPublicKey, hpkeAuthEncrypt, hpkeDecrypt, hpkeEncrypt, quorumKeyEncrypt, toDerSignature, uncompressRawPublicKey };
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js.map:1:{"version":3,"file":"proof.js","sources":["../src/proof.ts"],"sourcesContent":[null],"names":["x509","uint8ArrayToHexString","CBOR","AWS_ROOT_CERT_PEM","sha256","uint8ArrayFromHexString","p256","AWS_ROOT_CERT_SHA256"],"mappings":"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"}
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:3:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:53:    return p256.p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:139: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:145: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:160:    // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:190:    const encappedKeyBufHex = encoding.uint8ArrayToHexString(uncompressRawPublicKey(compressedSenderBuf));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:241:    const publicKeyUncompressed = encoding.uint8ArrayToHexString(uncompressRawPublicKey(publicKey));
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:293:const uncompressRawPublicKey = (rawPublicKey, curve = "CURVE_P256") => {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:313:        // secp256k1 domain parameters
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:386:    const ss = p256.p256.getSharedSecret(encoding.uint8ArrayFromHexString(priv), encappedKeyBuf);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:563:    const sharedSecretUncompressed = p256.p256.getSharedSecret(ephemeralSenderPrivate, targetPublicKeyUncompressed, false);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:565:    // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:586:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:623:exports.uncompressRawPublicKey = uncompressRawPublicKey;
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js.map:1:{"version":3,"file":"purejs.js","sources":["../src/purejs.ts"],"sourcesContent":[null],"names":["p256","uint8ArrayToHexString","createHash"],"mappings":";;;;;;MAIa,cAAc,GAAG,OAAO,KAIpC,KAAqB;AACpB,IAAA,MAAM,SAAS,GAAGA,SAAI,CAAC,YAAY,CAAC,KAAK,CAAC,UAAU,EAAE,IAAI,CAAC;;AAG3D,IAAA,MAAM,eAAe,GAAGC,8BAAqB,CAAC,SAAS,CAAC;AAExD,IAAA,IAAI,eAAe,IAAI,KAAK,CAAC,SAAS,EAAE;QACtC,MAAM,IAAI,KAAK,CACb,CAA2C,wCAAA,EAAA,KAAK,CAAC,SAAS,CAAS,MAAA,EAAA,eAAe,CAAE,CAAA,CACrF;;AAGH,IAAA,MAAM,IAAI,GAAGC,2BAAU,EAAE,CAAC,MAAM,CAAC,KAAK,CAAC,OAAO,CAAC,CAAC,MAAM,EAAE;AACxD,IAAA,MAAM,SAAS,GAAGF,SAAI,CAAC,IAAI,CAAC,IAAI,EAAE,KAAK,CAAC,UAAU,CAAC;AACnD,IAAA,OAAO,SAAS,CAAC,QAAQ,EAAE;AAC7B;;;;"}
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js.map:1:{"version":3,"file":"crypto.js","sources":["../src/crypto.ts"],"sourcesContent":[null],"names":["QUORUM_ENCRYPT_NONCE_LENGTH_BYTES","UNCOMPRESSED_PUB_KEY_LENGTH_BYTES","p256","uint8ArrayFromHexString","uint8ArrayToHexString","LABEL_EAE_PRK","SUITE_ID_1","LABEL_SHARED_SECRET","LABEL_SECRET","SUITE_ID_2","AES_KEY_INFO","IV_INFO","borsh","randomBytes","modSqrt","testBit","HPKE_VERSION","hkdf","sha256","aes","gcm","normalizePadding","QOS_ENCRYPTION_HMAC_MESSAGE"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:634:   155	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:653:   174	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:655:   176	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:845:     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:854:    10	  const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:866:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1224:   145	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1285:   206	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1763:   389	    67	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1849:   475	   153	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:1863:   489	   167	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2223:   849	     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2232:   858	    10	  const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2244:   870	    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2427:  1053	   134	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2488:  1114	   195	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2569:  1195	src/CryptoConstants.cs:2:// @turnkey/crypto. These are NOT in the upstream @turnkey/crypto npm package;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2570:  1196	src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2571:  1197	src/CryptoConstants.cs:6:// Upstream snapshot:
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2572:  1198	src/CryptoConstants.cs:7://   codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2573:  1199	src/CryptoConstants.cs:9:// Mapping to upstream:
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2574:  1200	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2575:  1201	src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2577:  1203	src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2578:  1204	src/CryptoConstants.cs:30:    /// <c>@turnkey/crypto</c>. Not present in the upstream npm package.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2723:  1349	src/Crypto.cs:542:        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2837:  1463	src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:3255:  1881	codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":";;;;;;;;;AAAA;AACA;AAsDYA;AAAZ,CAAA,UAAY,OAAO,EAAA;AACjB,IAAA,OAAA,CAAA,WAAA,CAAA,GAAA,WAAuB;AACvB,IAAA,OAAA,CAAA,QAAA,CAAA,GAAA,QAAiB;AACjB,IAAA,OAAA,CAAA,YAAA,CAAA,GAAA,YAAyB;AACzB,IAAA,OAAA,CAAA,aAAA,CAAA,GAAA,aAA2B;AAC3B,IAAA,OAAA,CAAA,KAAA,CAAA,GAAA,KAAW;AACb,CAAC,EANWA,eAAO,KAAPA,eAAO,GAAA,EAAA,CAAA,CAAA;AAQnB;;;;;;;AAOG;MACU,uBAAuB,GAAG,CACrC,gBAAwB,EACxB,WAAmB,KACT;AACV,IAAA,IAAI;QACF,MAAM,WAAW,GAAGC,kBAAS,CAAC,MAAM,CAAC,gBAAgB,CAAC;AACtD,QAAA,IAAI,WAAW,CAAC,UAAU,IAAI,EAAE,EAAE;YAChC,MAAM,IAAI,KAAK,CACb,CAAA,YAAA,EAAe,WAAW,CAAC,UAAU,CAAA,sFAAA,CAAwF,CAC9H;QACH;QAEA,MAAM,wBAAwB,GAAG,WAAW,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;QACzD,MAAM,aAAa,GAAG,WAAW,CAAC,KAAK,CAAC,EAAE,CAAC;AAC3C,QAAA,MAAM,cAAc,GAAGC,6BAAsB,CAAC,wBAAwB,CAAC;QACvE,MAAM,aAAa,GAAGC,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,OAAOC,8BAAqB,CAAC,aAAa,CAAC;IAC7C;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,+BAA+B,KAAK,CAAA,CAAE,CAAC;IACzD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;MACU,mBAAmB,GAAG,OAAO,EACxC,YAAY,EACZ,WAAW,EACX,cAAc,EACd,kCAAkC,EAClC,SAAS,EACT,cAAc,GACY,KAAqB;AAC/C,IAAA,IAAI;QACF,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;AACnD,QAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;QACD,IAAI,CAAC,QAAQ,EAAE;AACb,YAAA,MAAM,IAAI,KAAK,CACb,uCAAuC,kBAAkB,CAAA,CAAE,CAC5D;QACH;QAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CACtBC,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CACjD,CACF;QAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,YAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;YACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;QACH;AACA,QAAA,IAAI,CAAC,UAAU,CAAC,cAAc,EAAE;AAC9B,YAAA,MAAM,IAAI,KAAK,CAAC,gDAAgD,CAAC;QACnE;QAEA,MAAM,cAAc,GAAGA,gCAAuB,CAAC,UAAU,CAAC,cAAc,CAAC;QACzE,MAAM,aAAa,GAAGA,gCAAuB,CAAC,UAAU,CAAC,UAAU,CAAC;QACpE,MAAM,aAAa,GAAGF,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,IAAI,SAAS,KAAK,QAAQ,IAAI,CAAC,cAAc,EAAE;AAC7C,YAAA,IAAI,aAAa,CAAC,MAAM,KAAK,EAAE,EAAE;gBAC/B,MAAM,IAAI,KAAK,CACb,CAAA,mDAAA,EAAsD,aAAa,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;YACA,MAAM,cAAc,GAAGG,eAAO,CAAC,YAAY,CAAC,aAAa,CAAC;AAC1D,YAAA,IAAI,cAAc,CAAC,MAAM,KAAK,EAAE,EAAE;gBAChC,MAAM,IAAI,KAAK,CACb,CAAA,kDAAA,EAAqD,cAAc,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;AACA,YAAA,MAAM,iBAAiB,GAAG,IAAI,UAAU,CAAC,EAAE,CAAC;AAC5C,YAAA,iBAAiB,CAAC,GAAG,CAAC,aAAa,EAAE,CAAC,CAAC;AACvC,YAAA,iBAAiB,CAAC,GAAG,CAAC,cAAc,EAAE,EAAE,CAAC;AACzC,YAAA,OAAOC,aAAI,CAAC,MAAM,CAAC,iBAAiB,CAAC;QACvC;AAEA,QAAA,MAAM,gBAAgB,GAAGH,8BAAqB,CAAC,aAAa,CAAC;AAC7D,QAAA,OAAO,cAAc,GAAGI,mBAAU,CAAC,gBAAgB,CAAC,GAAG,gBAAgB;IACzE;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,4BAA4B,KAAK,CAAA,CAAE,CAAC;IACtD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;AACI,MAAM,oBAAoB,GAAG,OAClC,SAAiB,EACjB,SAAiB,EACjB,UAAkB,KACE;AACpB,IAAA,MAAM,eAAe,GAAGH,gCAAuB,CAAC,SAAS,CAAC;AAC1D,IAAA,MAAM,eAAe,GAAG,aAAa,CAAC,eAAe,CAAC;IACtD,IAAI,CAAC,eAAe,EAAE;AACpB,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,SAAS,CAAC;IACtD,MAAM,aAAa,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,UAAU,CAAC;AAC1D,IAAA,MAAM,UAAU,GAAGC,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,eAAe,CAAC,KAAK,EAAE,CAAC;AAC7E;AAEA;;;;;;;;AAQG;AACH,MAAM,sBAAsB,GAAG,OAC7B,mBAA2B,EAC3B,eAAuB,EACvB,UAAkB,EAClB,kCAA2C,KACvB;AACpB,IAAA,MAAM,uBAAuB,GAC3B,kCAAkC,IAAIC,2CAAiC;AACzE,IAAA,IAAI,mBAAmB,IAAI,uBAAuB,EAAE;QAClD,MAAM,IAAI,KAAK,CACb,CAAA,oBAAA,EACE,kCAAkC,IAAIA,2CACxC,CAAA,wCAAA,EAA2C,mBAAmB,CAAA,CAAE,CACjE;IACH;IAEA,MAAM,yBAAyB,GAAG,IAAI,UAAU,CAC9CP,gCAAuB,CAAC,mBAAmB,CAAC,CAC7C;AACD,IAAA,MAAM,SAAS,GAAG,aAAa,CAAC,yBAAyB,CAAC;IAC1D,IAAI,CAAC,SAAS,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,eAAe,CAAC;AAC5D,IAAA,MAAM,aAAa,GAAGJ,gCAAuB,CAAC,UAAU,CAAC;AACzD,IAAA,MAAM,UAAU,GAAGK,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,SAAS,CAAC,KAAK,EAAE,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,aAAa,GAAG,CAAC,SAAqB,KAA2B;IACrE,OAAOA,SAAI,CAAC,eAAe,CAAC,OAAO,CAACP,8BAAqB,CAAC,SAAS,CAAC,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,SAAS,GAAG,CAAC,UAAkB,EAAE,SAAc,KAAgB;IACnE,QAAQ,SAAS;AACf,QAAA,KAAK,QAAQ;YACX,MAAM,eAAe,GAAGG,aAAI,CAAC,MAAM,CAAC,UAAU,CAAC;AAC/C,YAAA,IAAI,eAAe,CAAC,MAAM,KAAK,EAAE,EAAE;gBACjC,MAAM,IAAI,KAAK,CACb,CAAA,2CAAA,EAA8C,eAAe,CAAC,MAAM,CAAA,CAAA,CAAG,CACxE;YACH;YACA,OAAO,eAAe,CAAC,QAAQ,CAAC,CAAC,EAAE,EAAE,CAAC;AACxC,QAAA,KAAK,aAAa;AAChB,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOF,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;AAC5C,QAAA;AACE,YAAA,OAAO,CAAC,IAAI,CACV,uBAAuB,SAAS,CAAA,4BAAA,CAA8B,CAC/D;AACD,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOA,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;;AAEhD,CAAC;AAED;;;;;;AAMG;MACU,yBAAyB,GAAG,OAAO,EAC9C,UAAU,EACV,SAAS,EACT,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACF,KAAqB;IACrD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,SAAS,CAAC,UAAU,EAAE,SAAS,CAAC;AACrD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACA,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;AAOK;AACE,MAAM,qBAAqB,GAAG,OAAO,EAC1C,QAAQ,EACR,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACN,KAAqB;IACjD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,QAAQ,CAAC;AACvD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACT,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;;;;;;;;;;;;;;;;;AAuBG;AACI,MAAM,yBAAyB,GAAG,OACvC,GAAW,EACX,qCAA8C,KAC1B;IACpB,MAAM,eAAe,GACnB,qCAAqC;AACrC,QAAAC,8CAAoC;;AAGtC,IAAA,MAAM,CAAC,SAAS,EAAE,UAAU,EAAE,YAAY,CAAC,GAAG,GAAG,CAAC,KAAK,CAAC,GAAG,CAAC;AAC5D,IAAA,IAAI,CAAC,YAAY;AAAE,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;AAC/D,IAAA,MAAM,YAAY,GAAG,CAAA,EAAG,SAAS,CAAA,CAAA,EAAI,UAAU,EAAE;;AAGjD,IAAA,MAAM,EAAE,GAAGL,aAAM,CAAC,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,YAAY,CAAC,CAAC;IACzD,MAAM,SAAS,GAAGA,aAAM,CAAC,EAAE,CAAC,CAAC;;IAG7B,MAAM,KAAK,GAAG,CAAC,CAAS,KACtB,CAAC,CAAC,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,EAAE,MAAM,CAClD,CAAC,CAAC,MAAM,IAAI,CAAC,CAAC,IAAI,CAAC,CAAC,MAAM,GAAG,CAAC,CAAC,IAAI,CAAC,CAAC,EACrC,GAAG,CACJ;AACH,IAAA,MAAM,SAAS,GAAG,UAAU,CAAC,IAAI,CAC/B,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;SACrB,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B,CAAC;;AAGF,IAAA,MAAM,SAAS,GAAGL,gCAAuB,CAAC,eAAe,CAAC;;IAG1D,OAAOM,SAAI,CAAC,MAAM,CAAC,SAAS,EAAE,SAAS,EAAE,SAAS,CAAC;AACrD;AAEA;;;;;;;;;;AAUG;AACI,MAAM,gBAAgB,GAAG,OAC9B,2BAAmC,EACnC,OAAe,KACQ;AACvB,IAAA,OAAO,MAAMK,uBAAgB,CAC3BX,gCAAuB,CAAC,2BAA2B,CAAC,EACpD,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,OAAO,CAAC,CAClC;AACH;AAEA;;;;;;;;;;;AAWG;AACI,MAAM,yBAAyB,GAAG,OACvC,aAAqB,EACrB,sCAA+C,KAC5B;AACnB,IAAA,OAAOD,8BAAqB,CAC1B,MAAM,gBAAgB,CACpB,sCAAsC;AACpC,QAAAa,mDAAyC,EAC3C,aAAa,CACd,CACF;AACH;AAEA;;;;;;;;;;;AAWG;MACU,mBAAmB,GAAG,CACjC,MAAc,EACd,4CAAqD,KAC3C;AACV,IAAA,OAAOhB,kBAAS,CAAC,MAAM,CACrBY,kBAAW,CAAC;QACV,YAAY,EAAE,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,MAAM,CAAC;AAC9C,QAAA,YAAY,EAAEX,6BAAsB,CAClCG,gCAAuB,CACrB,4CAA4C;AAC1C,YAAAa,8DAAoD,CACvD,CACF;AACF,KAAA,CAAC,CACH;AACH;;;;;;;;;;;;"}
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:3562:  2188	codex-crypto-reviews/Encoding.cs-r1-20260523.md:3411:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:3648:  2274	codex-crypto-reviews/Encoding.cs-r1-20260523.md:3500:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:3994:  2620	codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:489:   167	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4137:  2763	codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:870:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4252:  2878	codex-crypto-reviews/Crypto.cs-r1-20260523.md:916:   504	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4355:  2981	codex-crypto-reviews/Crypto.cs-r1-20260523.md:3025:   434	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4362:  2988	codex-crypto-reviews/Crypto.cs-r1-20260523.md:3068:   477	  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4364:  2990	codex-crypto-reviews/Crypto.cs-r1-20260523.md:3157:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4427:  3053	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4572:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:222:  return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4428:  3054	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4573:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:263:  return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4429:  3055	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4574:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:434: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4430:  3056	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4575:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:477:  return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4434:  3060	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4579:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:145:    const isValid = p256.p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4439:  3065	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4584:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4448:  3074	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4593:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:132:    return p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4449:  3075	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4594:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:157:    return p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4450:  3076	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4595:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:272: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4451:  3077	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4596:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:301:    return p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4452:  3078	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4597:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.d.ts:108: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4454:  3080	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4599:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:123:    const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4458:  3084	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4603:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:134:    return p256.p256.verify(publicSignatureBuf, hashedData, loadedPublicKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4459:  3085	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4604:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:159:    return p256.p256.verify(publicSignatureBuf, hashedData, quorumKey.toHex());
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4460:  3086	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4605:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:274: * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4461:  3087	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4606:./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:303:    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4467:  3093	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4468:  3094	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4659:    11	//   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4508:  3134	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4962:   274	 * 5.  Call `p256.verify(signature, msg, publicKey)`; noble treats the 32-byte
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4514:  3140	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4991:   303	    return p256.p256.verify(signature, msgDigest, publicKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4516:  3142	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4534:  3160	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4537:  3163	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4555:  3181	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4633:  3259	codex-crypto-reviews/Crypto.cs-r2-20260523.md:662:src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4687:  3313	codex-crypto-reviews/Crypto.cs-r2-20260523.md:1202:   529	        /// Equivalent to upstream <c>crypto.ts uncompressRawPublicKey</c>.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5690:  5170	src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5911:  5391	   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5934:  5414	/bin/zsh -lc 'rg -n "const string CURVE_NAME|CURVE_NAME" src/CryptoConstants.cs src/Crypto.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5936:  5416	src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5937:  5417	src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5938:  5418	src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5939:  5419	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5940:  5420	src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7298:   634	   155	            //   const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7317:   653	   174	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7319:   655	   176	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7509:   845	     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7518:   854	    10	  const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7530:   866	    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7888:  1224	   145	                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7949:  1285	   206	            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8315:/bin/zsh -lc "rg -n \"GetPublicKey\\(|Uint8ArrayFromHexString|StringToBase64UrlString|Uint8ArrayToHexString|DecodeBase64UrlToString|CURVE_NAME|JsonSerializable\\(typeof\\(ApiKeyStamper\" src" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8317:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8318:src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8350:src/Crypto.cs:968:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8352:src/Crypto.cs:1057:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8356:src/Crypto.cs:1114:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8370:src/ApiKeyStamper.cs:176:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8402:   355	            // Upstream uses noble's p256.getPublicKey which validates that
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8629:src/CryptoConstants.cs:22:// the BouncyCastle code paths that consume them produce the same bytes as
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8711:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6059:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8713:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6113:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8719:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:6165:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8828:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8829:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8867:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8156:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9058:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1463:src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9127:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2620:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:489:   167	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9270:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:2763:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:870:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9304:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3160:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9306:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3181:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9333:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5170:src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9420:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9422:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9721:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2837:  1463	src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9790:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:3994:  2620	codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:489:   167	            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9933:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4137:  2763	codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:870:    22	  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9967:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4534:  3160	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9969:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4555:  3181	codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:9989:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5690:  5170	src/ApiKeyStamper.cs:167:            //   const signature = p256.sign(hash, input.privateKey);        // RFC 6979 + lowS
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10199:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8315:/bin/zsh -lc "rg -n \"GetPublicKey\\(|Uint8ArrayFromHexString|StringToBase64UrlString|Uint8ArrayToHexString|DecodeBase64UrlToString|CURVE_NAME|JsonSerializable\\(typeof\\(ApiKeyStamper\" src" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10204:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8370:src/ApiKeyStamper.cs:176:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10287:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js.map:1:{"version":3,"file":"purejs.js","sources":["../src/purejs.ts"],"sourcesContent":[null],"names":["p256","uint8ArrayToHexString","createHash"],"mappings":";;;;;;MAIa,cAAc,GAAG,OAAO,KAIpC,KAAqB;AACpB,IAAA,MAAM,SAAS,GAAGA,SAAI,CAAC,YAAY,CAAC,KAAK,CAAC,UAAU,EAAE,IAAI,CAAC;;AAG3D,IAAA,MAAM,eAAe,GAAGC,8BAAqB,CAAC,SAAS,CAAC;AAExD,IAAA,IAAI,eAAe,IAAI,KAAK,CAAC,SAAS,EAAE;QACtC,MAAM,IAAI,KAAK,CACb,CAA2C,wCAAA,EAAA,KAAK,CAAC,SAAS,CAAS,MAAA,EAAA,eAAe,CAAE,CAAA,CACrF;;AAGH,IAAA,MAAM,IAAI,GAAGC,2BAAU,EAAE,CAAC,MAAM,CAAC,KAAK,CAAC,OAAO,CAAC,CAAC,MAAM,EAAE;AACxD,IAAA,MAAM,SAAS,GAAGF,SAAI,CAAC,IAAI,CAAC,IAAI,EAAE,KAAK,CAAC,UAAU,CAAC;AACnD,IAAA,OAAO,SAAS,CAAC,QAAQ,EAAE;AAC7B;;;;"}
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10375:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17194:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10377:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17248:./codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10383:codex-crypto-reviews/Crypto.cs-r3-20260523.md:17300:./plans/plan-v2-codex-reviewed.md:22:| Q4 | **All implementation files get 3 Codex rounds**: `Encoding.cs`, `Crypto.cs`, `CryptoConstants.cs`, `ApiKeyStamper.cs`, `Http.cs`, `TurnkeyJsonContext.cs`. 6 files × 3 rounds = 18 evidence files. | User condition: "Require codex review for all implementation phase". No file is exempt. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10492:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10493:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10531:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19291:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10550:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20480:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10551:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20481:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10572:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20514:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10573:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20515:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10600:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20559:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10601:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20560:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/p256.js:31:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10602:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20561:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:78: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10603:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20562:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:90:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10604:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20563:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:253: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10616:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20575:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:19: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10617:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20576:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:67: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10619:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20578:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:70: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10620:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20579:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:81:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10621:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20580:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:224: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10643:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20603:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:31:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10644:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20604:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:82:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10645:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20605:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.d.ts:35:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10646:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20606:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.js:55:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10650:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20610:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10651:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20611:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/esm/secp256k1.js:52:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10656:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20616:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.js:26:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10683:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20644:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/p256.ts:29:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10689:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20650:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/p256.js:30:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10695:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20656:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10696:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20657:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10697:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20658:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:76: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10698:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20659:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:88:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10699:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20660:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:250: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10702:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20663:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10703:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20664:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10709:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20670:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:68: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10710:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20671:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:79:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10711:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20672:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:221: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10718:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20679:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:65: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10719:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20680:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:76:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10720:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20681:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:218: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10911:codex-crypto-reviews/Crypto.cs-r2-20260523.md:1773:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10912:codex-crypto-reviews/Crypto.cs-r2-20260523.md:2079:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10920:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:1130:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10921:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:2052:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10977:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:7939:Error: Cannot find module '@noble/curves/p256'
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11005:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11006:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11044:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8156:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11059:codex-crypto-reviews/Crypto.cs-r1-20260523.md:1105:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11060:codex-crypto-reviews/Crypto.cs-r1-20260523.md:2522:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11070:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11075:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js:3:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11193:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:845:     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11331:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2223:   849	     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11336:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2570:  1196	src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11337:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2574:  1200	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11338:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:2576:  1202	src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11554:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:4467:  3093	codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11565:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:5939:  5419	src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11692:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:7509:   845	     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11702:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8317:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11799:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8828:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11800:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8829:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:11838:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:8867:codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8156:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12397:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10287:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.js.map:1:{"version":3,"file":"purejs.js","sources":["../src/purejs.ts"],"sourcesContent":[null],"names":["p256","uint8ArrayToHexString","createHash"],"mappings":";;;;;;MAIa,cAAc,GAAG,OAAO,KAIpC,KAAqB;AACpB,IAAA,MAAM,SAAS,GAAGA,SAAI,CAAC,YAAY,CAAC,KAAK,CAAC,UAAU,EAAE,IAAI,CAAC;;AAG3D,IAAA,MAAM,eAAe,GAAGC,8BAAqB,CAAC,SAAS,CAAC;AAExD,IAAA,IAAI,eAAe,IAAI,KAAK,CAAC,SAAS,EAAE;QACtC,MAAM,IAAI,KAAK,CACb,CAA2C,wCAAA,EAAA,KAAK,CAAC,SAAS,CAAS,MAAA,EAAA,eAAe,CAAE,CAAA,CACrF;;AAGH,IAAA,MAAM,IAAI,GAAGC,2BAAU,EAAE,CAAC,MAAM,CAAC,KAAK,CAAC,OAAO,CAAC,CAAC,MAAM,EAAE;AACxD,IAAA,MAAM,SAAS,GAAGF,SAAI,CAAC,IAAI,CAAC,IAAI,EAAE,KAAK,CAAC,UAAU,CAAC;AACnD,IAAA,OAAO,SAAS,CAAC,QAAQ,EAAE;AAC7B;;;;"}
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12505:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10492:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12506:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10493:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12544:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10531:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19291:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12558:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10550:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20480:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12559:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10551:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20481:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12579:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10572:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20514:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12580:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10573:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20515:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12607:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10600:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20559:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12608:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10601:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20560:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/p256.js:31:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12609:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10602:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20561:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:78: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12610:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10603:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20562:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:90:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12611:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10604:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20563:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:253: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12623:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10616:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20575:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:19: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12624:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10617:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20576:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:67: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12626:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10619:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20578:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:70: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12627:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10620:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20579:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:81:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12628:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10621:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20580:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:224: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12650:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10643:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20603:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:31:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12651:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10644:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20604:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:82:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12652:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10645:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20605:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.d.ts:35:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12653:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10646:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20606:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.js:55:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12657:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10650:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20610:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12658:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10651:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20611:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/esm/secp256k1.js:52:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12663:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10656:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20616:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.js:26:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12690:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10683:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20644:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/p256.ts:29:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12696:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10689:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20650:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/p256.js:30:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12702:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10695:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20656:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12703:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10696:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20657:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12704:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10697:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20658:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:76: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12705:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10698:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20659:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:88:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12706:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10699:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20660:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:250: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12709:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10702:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20663:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12710:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10703:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20664:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12716:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10709:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20670:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:68: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12717:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10710:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20671:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:79:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12718:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10711:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20672:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:221: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12725:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10718:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20679:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:65: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12726:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10719:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20680:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:76:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12727:codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:10720:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20681:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:218: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12866:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:849:     1	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12871:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1196:src/CryptoConstants.cs:4:// where the upstream uses @noble/curves which hides them.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12872:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1200:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:12873:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:1202:src/CryptoConstants.cs:23:// the upstream @noble/curves paths. Wire-byte parity is verified by the
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13089:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:3093:codex-crypto-reviews/Crypto.cs-r1-20260523.md:4658:    10	//   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13103:codex-crypto-reviews/ApiKeyStamper.cs-r1-20260523.md:5419:src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13117:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/purejs.mjs:1:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13181:codex-crypto-reviews/Crypto.cs-r3-20260523.md:1610:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13182:codex-crypto-reviews/Crypto.cs-r3-20260523.md:2532:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13248:codex-crypto-reviews/Crypto.cs-r3-20260523.md:7179:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13249:codex-crypto-reviews/Crypto.cs-r3-20260523.md:8101:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13253:codex-crypto-reviews/Crypto.cs-r3-20260523.md:12265:     2	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13254:codex-crypto-reviews/Crypto.cs-r3-20260523.md:13187:    26	import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13310:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19074:Error: Cannot find module '@noble/curves/p256'
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13338:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13339:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13377:codex-crypto-reviews/Crypto.cs-r3-20260523.md:19291:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts | sed -n '1,60p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13397:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20480:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8110:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13398:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20481:./codex-crypto-reviews/Crypto.cs-r3-20260523-prefix-evidence.md:8112:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13420:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20514:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19245:/bin/zsh -lc "rg -n \"lowS|verify\\(|getPublicKey|normPrivateKeyToScalar|isValidPrivateKey|validatePrivateKey\" /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/abstract/weierstrass.ts /Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13421:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20515:./codex-crypto-reviews/Crypto.cs-r3-20260523.md:19247:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13450:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20559:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/p256.ts:30:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13451:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20560:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/p256.js:31:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13452:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20561:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:78: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13453:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20562:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:90:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13454:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20563:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/src/secp256k1.ts:253: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13466:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20575:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:19: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13467:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20576:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.d.ts:67: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13469:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20578:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:70: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13470:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20579:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:81:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13471:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20580:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-adminjs/node_modules/@noble/curves/secp256k1.js:224: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13493:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20603:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:31:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13494:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20604:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.d.ts:82:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13495:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20605:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.d.ts:35:        lowS: boolean;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13496:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20606:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/secp256k1.js:55:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13500:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20610:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/src/p256.ts:26:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13501:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20611:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/esm/secp256k1.js:52:    lowS: true,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13506:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20616:/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x/eggrypto-x-server/node_modules/@noble/curves/p256.js:26:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13533:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20644:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/p256.ts:29:  lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13539:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20650:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/p256.js:30:    lowS: false,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13545:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20656:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13546:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20657:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13547:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20658:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:76: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13548:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20659:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:88:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13549:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20660:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/secp256k1.ts:250: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13552:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20663:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:17: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13553:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20664:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.d.ts:63: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13559:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20670:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:68: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13560:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20671:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:79:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13561:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20672:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/secp256k1.js:221: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13568:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20679:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:65: * const isValid = secp256k1.verify(sig, msg, pub) === true;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13569:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20680:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:76:    lowS: true, // Allow only low-S signatures by default in sign() and verify()
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13570:codex-crypto-reviews/Crypto.cs-r3-20260523.md:20681:/Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/esm/secp256k1.js:218: * const isValid = schnorr.verify(sig, msg, pub);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13571:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/purejs.ts:1:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13603:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:26:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13606:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:2:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13607:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:6:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13611:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:6:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13613:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.js:4:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13615:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:4:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13617:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:3:var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13618:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:1:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13619:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/proof.mjs:2:import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13718:    13	| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13762:/bin/zsh -lc "nl -ba /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/p256.ts 2>/dev/null | sed -n '1,60p' && nl -ba /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm/@noble+curves@1.8.1/node_modules/@noble/curves/src/abstract/weierstrass.ts 2>/dev/null | sed -n '945,995p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13765:     2	 * NIST secp256r1 aka p256.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13776:    13	const Fp256 = Field(BigInt('0xffffffff00000001000000000000000000000000ffffffffffffffffffffffff'));
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13777:    14	const CURVE_A = Fp256.create(BigInt('-3'));
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13780:    17	/** secp256r1 curve, ECDSA and ECDH methods. */
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13782:    19	export const p256: CurveFnWithCreate = createCurve({
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13785:    22	  Fp: Fp256, // Field: 2n**224n * (2n**32n-1n) + 2n**192n + 2n**96n-1n
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13794:    31	/** Alias to p256. */
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13795:    32	export const secp256r1: CurveFnWithCreate = p256;
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13798:    35	  mapToCurveSimpleSWU(Fp256, {
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13801:    38	    Z: Fp256.create(BigInt('-10')),
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13805:    42	  createHasher(secp256r1.ProjectivePoint, (scalars: bigint[]) => mapSWU(scalars[0]), {
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13808:    45	    p: Fp256.ORDER,
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13814:    51	/** secp256r1 hash-to-curve from [RFC 9380](https://www.rfc-editor.org/rfc/rfc9380). */
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13816:    53	/** secp256r1 encode-to-curve from [RFC 9380](https://www.rfc-editor.org/rfc/rfc9380). */
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:13956:/bin/zsh -lc "find /Users/takeshi -path '*node_modules/@noble/curves/src/p256.ts' -o -path '*node_modules/@noble/curves/p256.js' 2>/dev/null | head -30" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14032:3142:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14034:3160:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5112:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14035:3163:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14037:3181:codex-crypto-reviews/Crypto.cs-r1-20260523.md:5210:- Noble `p256.verify` low-S policy could not be verified from vendored dependency source because `node_modules` is not present in the snapshot.
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14047:3654:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5425:5056:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14048:3669:codex-crypto-reviews/Crypto.cs-r2-20260523.md:5470:5154:| `VerifyP256RawSignature` (`src/Crypto.cs:997`) | `p256.verify` call (`ts-source/turnkey.ts:477`) | REVIEWED | Converts raw `r||s` to DER for BouncyCastle. Low-S default unresolved in G. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14092:/bin/zsh -lc "find /Users/takeshi/Kyuzan/src/peak/node_modules/.pnpm -path '*@noble+curves@1.9.7*/node_modules/@noble/curves*' -maxdepth 8 -type f | rg '(p256|nist|weierstrass)\\.(ts|js)"'$'"' | sed -n '1,40p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14105:     3	var p256 = require('@noble/curves/p256');
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14110:     8	    const publicKey = p256.p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14117:    15	    const signature = p256.p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14123:     1	import { p256 } from '@noble/curves/p256';
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14128:     6	    const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14135:    13	    const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14161:web search: github noble curves v1.9.7 p256 lowS false
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14199:| `src/ApiKeyStamper.cs:196` | `ts-source/purejs.ts:22` | C# unconditionally normalizes `s` to low-S. Upstream purejs calls `p256.sign(hash, privateKey)` with no options. The package depends on `@noble/curves` v1 range (`^1.3.0`), and noble’s release notes say low-S became default for ECDSA in v2, which implies v1 P-256 default behavior is not the same low-S policy. Source: https://github.com/paulmillr/noble-curves/releases | Remove C# low-S normalization for v1 purejs parity, or generate upstream vectors with an explicit low-S policy and document that this intentionally differs from `@turnkey/api-key-stamper@0.5.0`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14202:| `src/ApiKeyStamper.cs:163` | `ts-source/purejs.ts:10` | Invalid private-key errors come from C# `Crypto.GetPublicKey` custom validation/messages; upstream errors come from noble `p256.getPublicKey`. | If error-message parity is required, add tests against upstream invalid vectors and normalize C# exception messages. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14246:| `src/ApiKeyStamper.cs:196` | `ts-source/purejs.ts:22` | C# unconditionally normalizes `s` to low-S. Upstream purejs calls `p256.sign(hash, privateKey)` with no options. The package depends on `@noble/curves` v1 range (`^1.3.0`), and noble’s release notes say low-S became default for ECDSA in v2, which implies v1 P-256 default behavior is not the same low-S policy. Source: https://github.com/paulmillr/noble-curves/releases | Remove C# low-S normalization for v1 purejs parity, or generate upstream vectors with an explicit low-S policy and document that this intentionally differs from `@turnkey/api-key-stamper@0.5.0`. |
./codex-crypto-reviews/ApiKeyStamper.cs-r2-20260523.md:14249:| `src/ApiKeyStamper.cs:163` | `ts-source/purejs.ts:10` | Invalid private-key errors come from C# `Crypto.GetPublicKey` custom validation/messages; upstream errors come from noble `p256.getPublicKey`. | If error-message parity is required, add tests against upstream invalid vectors and normalize C# exception messages. |
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:28:); // used for encrypting messages to quorum keys matched whats found here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L22
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:2:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:84:  return p256.getPublicKey(privateKey, isCompressed);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:219: * Encrypt a message to a quorum key. Algorithm originally implemented in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L123
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:225: * This encryption function is meant to be used with this decryption function in QOS: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L52
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:250:  // create the additional data in the form of sender_public||sender_public_len||receiver_public||receiver_public_len taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:291:    uncompressRawPublicKey(compressedSenderBuf),
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:362:    uncompressRawPublicKey(publicKey),
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:426:export const uncompressRawPublicKey = (
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:455:    // secp256k1 domain parameters
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:560:  const ss = p256.getSharedSecret(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:809:  const sharedSecretUncompressed = p256.getSharedSecret(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:816:  // create the preImage as defined in qos here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L273-L282
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:860:/// This is taken from QOS here: https://github.com/tkhq/qos/blob/ae01904c756107f850aea42000137ef124df3fe4/src/qos_p256/src/encrypt.rs#L298
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/purejs.ts:1:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/purejs.ts:10:  const publicKey = p256.getPublicKey(input.privateKey, true);
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/purejs.ts:22:  const signature = p256.sign(hash, input.privateKey);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:6:import { p256 } from "@noble/curves/p256";
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:150:    p256.ProjectivePoint.fromHex(signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:173:  const isValid = p256.verify(signatureBytes, payloadDigest, signingKeyBytes);
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:10:  uncompressRawPublicKey,
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:34:    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:70:    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:232:  describe("uncompressRawPublicKey", () => {
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:235:      const uncompressedPublicKey = uncompressRawPublicKey(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:246:      expect(() => uncompressRawPublicKey(invalidPrefix)).toThrow(
./codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:255:        uncompressRawPublicKey(

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '400,490p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts | sed -n '540,575p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "nl -ba tests/Fixtures/README.md && nl -ba tests/Fixtures/crypto/turnkey-crypto-vectors.json | sed -n '1,240p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "rg -n \"Fixtures|turnkey-crypto-vectors|CryptoConstants|P256_P|COMPRESSED_PUBLIC_KEY_SIZE|CURVE_NAME|UncompressRawPublicKey|p256-nist|CAVP|fixture\" tests src codex-crypto-reviews -g '"'!codex-crypto-reviews/**/*.md'"' -g '"'!codex-crypto-reviews/upstream-snapshots/**/package/dist/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/CryptoConstants.cs:10://   - CURVE_NAME: not present upstream; @noble/curves uses p256 implicitly.
src/CryptoConstants.cs:11://   - COMPRESSED_PUBLIC_KEY_SIZE: implied upstream by compressedKey.length === 33
src/CryptoConstants.cs:13://   - P256_P / P256_B / P256_A_OFFSET: hardcoded inline in
src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
src/CryptoConstants.cs:32:    public static class CryptoConstants
src/CryptoConstants.cs:38:        public const string CURVE_NAME = "secp256r1";
src/CryptoConstants.cs:45:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
src/CryptoConstants.cs:58:        public const string P256_P =
src/Crypto.cs:13://                                        UncompressRawPublicKey /
src/Crypto.cs:130:        /// <see cref="UncompressRawPublicKey(byte[], Curve)"/>.
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
tests/PublicApi.expected.txt:40:        public static byte[] UncompressRawPublicKey(byte[] rawPublicKey, Turnkey.Crypto.Curve curve = 0) { }
tests/PublicApi.expected.txt:121:    public static class CryptoConstants
tests/PublicApi.expected.txt:123:        public const int COMPRESSED_PUBLIC_KEY_SIZE = 33;
tests/PublicApi.expected.txt:124:        public const string CURVE_NAME = "secp256r1";
tests/PublicApi.expected.txt:127:        public const string P256_P = "115792089210356248762697446949407573530086143415290314195533631308867097853951";
tests/ApiKeyStamperTests.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{private,public}
tests/ApiKeyStamperTests.cs:34:        // From upstream __fixtures__/api-key.{private,public}
tests/ApiKeyStamperTests.cs:148:                var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/ApiKeyStamperTests.cs:209:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/CryptoTests.cs:6://   - Compress/UncompressRawPublicKey roundtrip
tests/CryptoTests.cs:79:            var p = new BigInteger(CryptoConstants.P256_P);
tests/CryptoTests.cs:194:        // CompressRawPublicKey / UncompressRawPublicKey
tests/CryptoTests.cs:208:                byte[] recovered = Crypto.UncompressRawPublicKey(compressed);
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
tests/CryptoTests.cs:541:        public void UncompressRawPublicKey_UpstreamInvalidPrefixVector()
tests/CryptoTests.cs:546:            Action act = () => Crypto.UncompressRawPublicKey(invalidPrefix);
src/ApiKeyStamper.cs:179:            var curve = ECNamedCurveTable.GetByName(CryptoConstants.CURVE_NAME);
tests/HttpTests.cs:12:        // Upstream api-key-stamper@0.5.0 fixture, reused to drive Http here so
tests/turnkey-sdk-csharp.Tests.csproj:24:    <None Include="Fixtures\**\*">
tests/Fixtures/README.md:1:# Test fixtures
tests/Fixtures/README.md:3:Each subdirectory groups fixtures for one production file, plus a
tests/Fixtures/README.md:11:- **nist** — NIST CAVP P-256 vectors.
tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
tests/Fixtures/README.md:21:Fixtures/
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
tests/Fixtures/README.md:34:into the matching fixture file and update its sha256 in
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:4:    "source_fixture": "codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{public,private}",
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:12:    "byte_level_assertion_strategy": "Exact Node-generated byte snapshots would require running the upstream npm package with its full node_modules; the tarball ships only built JS + declarations. Instead, ApiKeyStamperTests asserts wire-format equivalence by: (1) reproducing the upstream stamp-test.ts shape (decoded JSON key order = ['publicKey', 'scheme', 'signature']; scheme constant; publicKey equality); (2) crypto-verifying the DER ECDSA signature against SHA-256(content) under the upstream fixture public key; (3) asserting RFC 6979 determinism (same content -> same signature); (4) asserting low-S (s in (0, n/2]). This combination is sufficient to guarantee the stamp is accepted by Turnkey's backend, which is the wire-format goal."
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:24:    "stamp_test_assertion": "Decode base64url -> JSON.parse; keys equal ['publicKey', 'scheme', 'signature']; publicKey == fixture.publicKey; scheme == SIGNATURE_SCHEME_TK_API_P256; signature verifies as DER ECDSA against SHA-256(message) under the fixture public key."
codex-crypto-reviews/codex-crypto-review.sh:109:F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
codex-crypto-reviews/codex-crypto-review.sh:117:entries, F is "all fixtures match".
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:3:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:9:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:22:])("sign with Turnkey fixture: $name", async ({ impl: sign }) => {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/shared.ts:4:const FIXTURES_DIR = path.resolve(__dirname, "..", "__fixtures__");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__fixtures__/shared.ts:4:const FIXTURES_DIR = path.resolve(__dirname, "..", "__fixtures__");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__tests__/request-test.ts:4:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__tests__/async-test.ts:9:import { readFixture } from "../__fixtures__/shared";

codex
I found `CryptoConstants.cs` has constants only, no public or internal methods. I’m checking the fixture provenance and the tests that indirectly exercise those constants through compression, decompression, and BouncyCastle P-256 curve usage.
exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '1,130p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '180,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "nl -ba tests/CryptoTests.cs | sed -n '520,560p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba tests/ApiKeyStamperTests.cs | sed -n '1,60p' && nl -ba tests/ApiKeyStamperTests.cs | sed -n '130,225p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// ApiKeyStamperTests.cs — xunit tests for src/ApiKeyStamper.cs.
     2	//
     3	// Vector sources:
     4	//   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/api-key.{private,public}
     5	//   codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts
     6	//
     7	// P-256 ECDSA signatures are not deterministic across libraries (noble uses
     8	// RFC 6979; BouncyCastle's HMacDsaKCalculator also uses RFC 6979 with the
     9	// same digest, so for identical input + key + nonce-source the signature
    10	// is deterministic). Tests therefore assert:
    11	//   - the header name == "X-Stamp"
    12	//   - the base64url-decoded JSON has keys ["publicKey", "scheme", "signature"]
    13	//   - publicKey matches the configured key
    14	//   - scheme == "SIGNATURE_SCHEME_TK_API_P256"
    15	//   - the signature is a valid DER-encoded ECDSA signature over SHA-256(content)
    16	//     under the configured public key
    17	
    18	using System;
    19	using System.IO;
    20	using System.Text;
    21	using System.Text.Json;
    22	using FluentAssertions;
    23	using Org.BouncyCastle.Asn1;
    24	using Org.BouncyCastle.Asn1.X9;
    25	using Org.BouncyCastle.Crypto.Parameters;
    26	using Org.BouncyCastle.Math;
    27	using Org.BouncyCastle.Security;
    28	using Xunit;
    29	
    30	namespace Turnkey.Tests
    31	{
    32	    public class ApiKeyStamperTests
    33	    {
    34	        // From upstream __fixtures__/api-key.{private,public}
    35	        private const string FixturePrivateKey =
    36	            "487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c";
    37	        private const string FixturePublicKey =
    38	            "02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316";
    39	
    40	        [Fact]
    41	        public void Constructor_DefersValidation()
    42	        {
    43	            // Upstream `new ApiKeyStamper({apiPublicKey, apiPrivateKey})` only
    44	            // assigns fields; validation happens at sign time. C# mirrors
    45	            // that, including null tolerance.
    46	            Action act = () => new ApiKeyStamper(FixturePublicKey, "abcd");
    47	            act.Should().NotThrow();
    48	            Action act2 = () => new ApiKeyStamper(null!, null!);
    49	            act2.Should().NotThrow();
    50	        }
    51	
    52	        [Fact]
    53	        public void SignWithApiKey_RejectsBadKeyLength()
    54	        {
    55	            // Validation now occurs in SignWithApiKey -> Crypto.GetPublicKey.
    56	            var stamper = new ApiKeyStamper(FixturePublicKey, "abcd");
    57	            Action act = () => stamper.SignWithApiKey("payload");
    58	            act.Should().Throw<ArgumentException>()
    59	               .WithMessage("invalid P-256 private key: expected 32 bytes, got 2");
    60	        }
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
   221	
   222	        private static (BigInteger R, BigInteger S) ParseDerEcdsa(byte[] der)
   223	        {
   224	            using var ms = new MemoryStream(der);
   225	            using var reader = new Asn1InputStream(ms);

exec
/bin/zsh -lc 'rg -n "turnkey-crypto-vectors|Fixtures/crypto|ReadAllText|JsonSerializer|expectedHex|_provenance" tests' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/PublicApi.expected.txt:298:    public class TurnkeyJsonContext : System.Text.Json.Serialization.JsonSerializerContext, System.Text.Json.Serialization.Metadata.IJsonTypeInfoResolver
tests/PublicApi.expected.txt:302:        public TurnkeyJsonContext(System.Text.Json.JsonSerializerOptions options) { }
tests/PublicApi.expected.txt:308:        protected override System.Text.Json.JsonSerializerOptions? GeneratedSerializerOptions { get; }
tests/Fixtures/encoding/turnkey-encoding-vectors.json:2:  "_provenance": {
tests/PublicApiSnapshotTests.cs:42:            string expected = File.ReadAllText(baselinePath);
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:2:  "_provenance": {
tests/Fixtures/crypto/turnkey-crypto-vectors.json:2:  "_provenance": {
tests/Fixtures/crypto/turnkey-crypto-vectors.json:16:      "expectedHex": "67ee05fc3bdf4161bc70701c221d8d77180294cefcfcea64ba83c4d4c732fcb9"

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts | sed -n '160,265p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "node -e 'const p=BigInt(\"115792089210356248762697446949407573530086143415290314195533631308867097853951\"); console.log(p.toString(16)); console.log((p-3n).toString(16)); console.log(BigInt(\"0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b\").toString(10));'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
ffffffff00000001000000000000000000000000ffffffffffffffffffffffff
ffffffff00000001000000000000000000000000fffffffffffffffffffffffc
41058363725152142129326129780047268409114441015993725554835256314039467401291

exec
/bin/zsh -lc "nl -ba tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json | sed -n '1,80p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "git show --no-ext-diff --format='%H %ct %s' --no-patch 388a5fc768fd1215928d29f338ef069acd2eb60c" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc 'git show --no-ext-diff 388a5fc768fd1215928d29f338ef069acd2eb60c:src/CryptoConstants.cs | diff -u src/CryptoConstants.cs -' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba tests/Fixtures/encoding/turnkey-encoding-vectors.json | sed -n '1,80p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	{
     2	  "_provenance": {
     3	    "level": "upstream-test-vectors",
     4	    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts",
     5	    "package": "@turnkey/encoding@0.6.0",
     6	    "tarball_sha256": "2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc",
     7	    "note": "Bytes / strings reproduced verbatim from the upstream Jest test file. Reviewers can byte-compare each entry against the linked TS file."
     8	  },
     9	  "stringToBase64UrlString": [
    10	    {
    11	      "input": "hello",
    12	      "expected": "aGVsbG8"
    13	    },
    14	    {
    15	      "input": "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1",
    16	      "expected": "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ"
    17	    },
    18	    {
    19	      "input": "{\"publicKey\":\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\",\"signature\":\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\",\"scheme\":\"SIGNATURE_SCHEME_TK_API_P256\"}",
    20	      "expected": "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ"
    21	    }
    22	  ],
    23	  "base64StringToBase64UrlEncodedString": [
    24	    { "input": "aGVsbG8gd29ybGQ=", "expected": "aGVsbG8gd29ybGQ" },
    25	    { "input": "U29tZSBzYW1wbGUgdGV4dA==", "expected": "U29tZSBzYW1wbGUgdGV4dA" }
    26	  ],
    27	  "uint8ArrayToHexString": [
    28	    {
    29	      "input": [82, 52, 208, 143, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23, 46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193],
    30	      "expected": "5234d08ffa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1"
    31	    }
    32	  ],
    33	  "uint8ArrayFromHexString": {
    34	    "happy": [
    35	      {
    36	        "input": "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1",
    37	        "expected": [82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23, 46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193]
    38	      },
    39	      {
    40	        "input": "627566666572",
    41	        "expected": [98, 117, 102, 102, 101, 114]
    42	      },
    43	      {
    44	        "input": "01",
    45	        "length": 2,
    46	        "expected": [0, 1]
    47	      },
    48	      {
    49	        "input": "01",
    50	        "expected": [1]
    51	      },
    52	      {
    53	        "input": "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc",
    54	        "length": 32,
    55	        "expected": [0, 0, 82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23, 46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252]
    56	      }
    57	    ],
    58	    "errors": [
    59	      { "input": "",     "expected_message_prefix": "cannot create uint8array from invalid hex string" },
    60	      { "input": "123",  "expected_message_prefix": "cannot create uint8array from invalid hex string" },
    61	      { "input": "oops", "expected_message_prefix": "cannot create uint8array from invalid hex string" },
    62	      { "input": "0100", "length": 1, "expected_message_prefix": "hex value cannot fit in a buffer of 1 byte(s)" },
    63	      { "input": "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fcfafbfcfd", "length": 32, "expected_message_prefix": "hex value cannot fit in a buffer of 32 byte(s)" }
    64	    ]
    65	  },
    66	  "hexStringToBase64Url": [
    67	    { "input": "01",            "expected": "AQ" },
    68	    { "input": "01", "length": 2, "expected": "AAE" },
    69	    { "input": "ff",            "expected": "_w" },
    70	    { "input": "ff", "length": 2, "expected": "AP8" }
    71	  ]
    72	}

 succeeded in 1698ms:
2026-05-23 10:30:29.143 xcodebuild[40457:106257865]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:30:29.352 xcodebuild[40457:106257863] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-vUCzUk3e' (errno=Operation not permitted)
2026-05-23 10:30:29.766 xcodebuild[40461:106257890]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:30:29.898 xcodebuild[40461:106257888] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
388a5fc768fd1215928d29f338ef069acd2eb60c 1779494127 Port Crypto.cs + CryptoConstants.cs + TurnkeyJsonContext.cs from @turnkey/crypto@2.8.8

 exited 2 in 1692ms:
2026-05-23 10:30:29.143 xcodebuild[40458:106257866]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:30:29.352 xcodebuild[40458:106257864] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-Gube4Y0Y' (errno=Operation not permitted)
2026-05-23 10:30:29.766 xcodebuild[40462:106257889]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:30:29.898 xcodebuild[40462:106257887] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.

exec
/bin/zsh -lc 'shasum -a 256 src/CryptoConstants.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
8b25ada0410bd081c8b28ea9fe002098f78a5f3dc73865e0725025a4e76f287b  src/CryptoConstants.cs

exec
/bin/zsh -lc 'git show --no-ext-diff 388a5fc768fd1215928d29f338ef069acd2eb60c:src/CryptoConstants.cs | shasum -a 256' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "rg -n \"Org\\.BouncyCastle\\.Crypto\\.Generators\\.HkdfBytesGenerator|Org\\.BouncyCastle\\.Crypto\\.Hpke|HkdfBytesGenerator|HPKE|Hpke\" src/CryptoConstants.cs src/Crypto.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
src/CryptoConstants.cs:24:// HPKE / bundle-decrypt / signature-verify test fixtures.
src/Crypto.cs:10://                                        HpkeEncrypt / HpkeDecrypt /
src/Crypto.cs:14://                                        FormatHpkeBuf, plus the internal
src/Crypto.cs:36://     BigInteger / EC point / Ed25519 primitives only. HPKE, HKDF,
src/Crypto.cs:77:            // HPKE KEM/HPKE suite IDs.
src/Crypto.cs:79:            public static readonly byte[] SUITE_ID_2 = new byte[] { 72, 80, 75, 69, 0, 16, 0, 1, 0, 2 }; // "HPKE\0\x10\0\x01\0\x02"
src/Crypto.cs:80:            public static readonly byte[] HPKE_VERSION = new byte[] { 72, 80, 75, 69, 45, 118, 49 }; // "HPKE-v1"
src/Crypto.cs:82:            // HPKE labels.
src/Crypto.cs:270:        /// <summary>HPKE decrypt parameters; equivalent to upstream object literal.</summary>
src/Crypto.cs:271:        public class HpkeDecryptParams
src/Crypto.cs:278:        /// <summary>HPKE encrypt parameters; equivalent to upstream object literal.</summary>
src/Crypto.cs:279:        public class HpkeEncryptParams
src/Crypto.cs:332:        /// JSON shape returned by <see cref="FormatHpkeBuf"/>. The two fields
src/Crypto.cs:336:        public class HpkeBundlePayload
src/Crypto.cs:398:        /// HPKE-Base mode decrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:405:        public static byte[] HpkeDecrypt(HpkeDecryptParams parameters)
src/Crypto.cs:445:        /// HPKE-Base mode encrypt (P-256 / HKDF-SHA256 / AES-128-GCM).
src/Crypto.cs:451:        /// <see cref="FormatHpkeBuf"/> or <see cref="DecryptCredentialBundle"/>.
src/Crypto.cs:457:        public static byte[] HpkeEncrypt(HpkeEncryptParams parameters)
src/Crypto.cs:498:        /// HPKE additional associated data builder.
src/Crypto.cs:611:        /// Encode the HPKE result <c>compressed(senderPub) || ciphertext</c>
src/Crypto.cs:613:        /// Equivalent to upstream <c>crypto.ts formatHpkeBuf</c>.
src/Crypto.cs:615:        public static string FormatHpkeBuf(byte[] encryptedBuf)
src/Crypto.cs:639:            var payload = new HpkeBundlePayload
src/Crypto.cs:644:            return JsonSerializer.Serialize(payload, TurnkeyJsonContext.Default.HpkeBundlePayload);
src/Crypto.cs:682:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:710:            //   5. hpkeEncrypt + formatHpkeBuf
src/Crypto.cs:769:            var privateKeyBundle = HpkeEncrypt(new HpkeEncryptParams
src/Crypto.cs:774:            return FormatHpkeBuf(privateKeyBundle);
src/Crypto.cs:838:                var decryptedData = HpkeDecrypt(new HpkeDecryptParams
src/Crypto.cs:989:            return Encoding.ConcatUint8Arrays(Constants.HPKE_VERSION, suiteId, label, ikm);
src/Crypto.cs:1000:            Array.Copy(Constants.HPKE_VERSION, 0, ret, 2, Constants.HPKE_VERSION.Length);

 succeeded in 1614ms:
2026-05-23 10:30:43.700 xcodebuild[40581:106258527]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:30:43.891 xcodebuild[40581:106258526] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-MEoUCe2b' (errno=Operation not permitted)
2026-05-23 10:30:44.270 xcodebuild[40583:106258542]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 10:30:44.387 xcodebuild[40583:106258541] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
8b25ada0410bd081c8b28ea9fe002098f78a5f3dc73865e0725025a4e76f287b  -

exec
/bin/zsh -lc "rg -n \"115792089210356|0xffffffff00000001000000000000000000000000ffffffff|5ac635d8|p - BigInt\\(3\\)\" codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:448:      "115792089210356248762697446949407573530086143415290314195533631308867097853951",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:451:      "0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:453:    a = p - BigInt(3);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:286:        p = BigInt("115792089210356248762697446949407573530086143415290314195533631308867097853951");
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:287:        b = BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b");
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.mjs:288:        a = p - BigInt(3);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:308:        p = BigInt("115792089210356248762697446949407573530086143415290314195533631308867097853951");
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:309:        b = BigInt("0x5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b");
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/crypto.js:310:        a = p - BigInt(3);

exec
/bin/zsh -lc 'find tests/Fixtures -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json
tests/Fixtures/crypto/turnkey-crypto-vectors.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json

exec
/bin/zsh -lc 'find tests/Fixtures/Generators -maxdepth 3 -type f -print 2>/dev/null | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
exec
/bin/zsh -lc 'rg -n "mockCredentialBundle|mockPrivateKey|mockSenderPrivateKey" codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
23:const mockSenderPrivateKey =
25:const mockPrivateKey =
27:const mockCredentialBundle =
181:      mockCredentialBundle,
182:      mockPrivateKey,
184:    expect(decryptedData).toBe(mockSenderPrivateKey);

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts | sed -n '1,80p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	import { test, expect, describe } from "@jest/globals";
     2	import { ApiKeyStamper } from "@turnkey/api-key-stamper";
     3	import { TurnkeyClient } from "@turnkey/http";
     4	import { uint8ArrayFromHexString } from "@turnkey/encoding";
     5	import {
     6	  getPublicKey,
     7	  generateP256KeyPair,
     8	  decryptCredentialBundle,
     9	  extractPrivateKeyFromPKCS8Bytes,
    10	  uncompressRawPublicKey,
    11	  compressRawPublicKey,
    12	  hpkeDecrypt,
    13	  hpkeEncrypt,
    14	  decryptExportBundle,
    15	  hpkeAuthEncrypt,
    16	  formatHpkeBuf,
    17	  verifyStampSignature,
    18	  verifySessionJwtSignature,
    19	  fromDerSignature,
    20	} from "../";
    21	
    22	// Mock data for testing
    23	const mockSenderPrivateKey =
    24	  "67ee05fc3bdf4161bc70701c221d8d77180294cefcfcea64ba83c4d4c732fcb9";
    25	const mockPrivateKey =
    26	  "20fa65df11f24833790ae283fc9a0c215eecbbc589549767977994dc69d05a56";
    27	const mockCredentialBundle =
    28	  "w99a5xV6A75TfoAUkZn869fVyDYvgVsKrawMALZXmrauZd8hEv66EkPU1Z42CUaHESQjcA5bqd8dynTGBMLWB9ewtXWPEVbZvocB4Tw2K1vQVp7uwjf";
    29	
    30	describe("HPKE Encryption and Decryption", () => {
    31	  test("hpkeAuthEncrypt and hpkeDecrypt - end-to-end encryption and decryption", () => {
    32	    const senderKeyPair = generateP256KeyPair();
    33	    const receiverKeyPair = generateP256KeyPair();
    34	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
    35	      uint8ArrayFromHexString(receiverKeyPair.publicKey),
    36	    );
    37	
    38	    const textEncoder = new TextEncoder();
    39	    // Mock plaintext
    40	    const plainText = "Hello, this is a secure message!";
    41	    const plainTextBuf = textEncoder.encode(plainText);
    42	    // Encrypt
    43	    const encryptedDataBuf = hpkeAuthEncrypt({
    44	      plainTextBuf: plainTextBuf,
    45	      targetKeyBuf: receiverPublicKeyUncompressed,
    46	      senderPriv: senderKeyPair.privateKey,
    47	    });
    48	    const encryptedData = formatHpkeBuf(encryptedDataBuf);
    49	    // Extract the encapsulated key buffer and the ciphertext
    50	    const data = JSON.parse(encryptedData);
    51	    // Decrypt
    52	    const decryptedData = hpkeDecrypt({
    53	      ciphertextBuf: uint8ArrayFromHexString(data.ciphertext),
    54	      encappedKeyBuf: uint8ArrayFromHexString(data.encappedPublic),
    55	      receiverPriv: receiverKeyPair.privateKey,
    56	    });
    57	
    58	    // Convert decrypted data back to string
    59	    const decryptedText = new TextDecoder().decode(decryptedData);
    60	
    61	    // Expect the decrypted text to equal the original plaintext
    62	    expect(decryptedText).toEqual(plainText);
    63	  });
    64	});
    65	
    66	describe("HPKE Standard Encryption and Decryption", () => {
    67	  test("hpkeEncrypt and hpkeDecrypt - standard mode (ephemeral sender key)", async () => {
    68	    // Generate a receiver key pair
    69	    const receiverKeyPair = generateP256KeyPair();
    70	    const receiverPublicKeyUncompressed = uncompressRawPublicKey(
    71	      uint8ArrayFromHexString(receiverKeyPair.publicKey),
    72	    );
    73	
    74	    // Prepare the plaintext
    75	    const textEncoder = new TextEncoder();
    76	    const plainText =
    77	      "6ab33bd6e4bdc73017233da0554f9616fe10ede5c3ce001e81b321d5a74199b7";
    78	    const plainTextBuf = textEncoder.encode(plainText);
    79	
    80	    // Encrypt using standard mode (no sender private key provided)

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/turnkey-source-pins.md | sed -n '1,30p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba tests/PublicApi.expected.txt | sed -n '118,130p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

codex
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
- Current `src/CryptoConstants.cs` content hash matches the blob at that SHA.

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| N/A - no public or internal helper methods in `src/CryptoConstants.cs:32-74` | N/A | REVIEWED | The file declares only a public static constants class. Public constants were reviewed against `ts-source/crypto.ts:430-453`, `ts-source/crypto.ts:559-564`, and `ts-source/turnkey.ts:84-86`. |

**C. Intentional Adaptations**

- `CryptoConstants` class extracts upstream inline literals into named C# constants so BouncyCastle call sites can share one source of truth; this changes organization only, not derived bytes.
- `CURVE_NAME = "secp256r1"` names the same NIST P-256 curve that upstream imports as noble `p256`; structural provider selection only.
- `COMPRESSED_PUBLIC_KEY_SIZE = 33` names upstream’s hard-coded compressed P-256 public-key length check; same byte boundary.
- `P256_P` is stored as a decimal string for `BigInteger(string)` parsing; same numeric field modulus as upstream.
- `P256_B` omits the upstream `0x` prefix because C# parses it with radix 16 at use sites; same coefficient bytes.
- `P256_A_OFFSET = "3"` names upstream `BigInt(3)` so C# computes `p - 3`; same curve coefficient derivation.

**D. D17 Enforcement Check**

N/A (not Crypto.cs)

**E. Logic Divergence Findings**

Zero entries.

I checked the constants against upstream P-256 decompression and compressed-key handling: `COMPRESSED_PUBLIC_KEY_SIZE` matches upstream length `33`, `P256_P` matches upstream modulus, `P256_B` matches upstream coefficient `b`, and `P256_A_OFFSET` matches upstream `a = p - BigInt(3)`. No algorithm step order, byte ordering, leading-zero, padding, signature, DTO, or JSON behavior exists in this file.

**F. Fixture Comparison Gate**

All applicable fixtures match.

- `tests/Fixtures/crypto/turnkey-crypto-vectors.json` is pinned to `@turnkey/crypto@2.8.8` with sha256 `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`.
- Its credential-bundle vector matches upstream `ts-source/__tests__/crypto-test.ts:179-184`; C# asserts the same decrypted hex in `tests/CryptoTests.cs:524-537`.
- Its invalid compressed-key prefix vector matches upstream `ts-source/__tests__/crypto-test.ts:241-248`; C# asserts the same error in `tests/CryptoTests.cs:541-548`.
- Its compressed/uncompressed public-key coverage follows upstream `ts-source/__tests__/crypto-test.ts:172-177`; C# asserts compression emits 33 bytes and round-trips to the original uncompressed key in `tests/CryptoTests.cs:197-209`.

`encoding` fixtures do not exercise this file. `api-key-stamper` fixtures use `CryptoConstants.CURVE_NAME` only in test-side verification helpers, not as fixture coverage for this file’s `@turnkey/crypto` parity.

**G. Unresolved Assumptions**

- I did not run `dotnet test`; the workspace is read-only, and test execution would normally write `bin/obj` artifacts.
- There is no `tests/Fixtures/Generators/` content to rerun fixture generation in this round, so fixture provenance was verified statically against the pinned upstream snapshot.


tokens used
115,471
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/crypto`
- Version: `2.8.8`
- Tarball sha256: `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`
- C# file git SHA: `388a5fc768fd1215928d29f338ef069acd2eb60c`
- Current `src/CryptoConstants.cs` content hash matches the blob at that SHA.

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| N/A - no public or internal helper methods in `src/CryptoConstants.cs:32-74` | N/A | REVIEWED | The file declares only a public static constants class. Public constants were reviewed against `ts-source/crypto.ts:430-453`, `ts-source/crypto.ts:559-564`, and `ts-source/turnkey.ts:84-86`. |

**C. Intentional Adaptations**

- `CryptoConstants` class extracts upstream inline literals into named C# constants so BouncyCastle call sites can share one source of truth; this changes organization only, not derived bytes.
- `CURVE_NAME = "secp256r1"` names the same NIST P-256 curve that upstream imports as noble `p256`; structural provider selection only.
- `COMPRESSED_PUBLIC_KEY_SIZE = 33` names upstream’s hard-coded compressed P-256 public-key length check; same byte boundary.
- `P256_P` is stored as a decimal string for `BigInteger(string)` parsing; same numeric field modulus as upstream.
- `P256_B` omits the upstream `0x` prefix because C# parses it with radix 16 at use sites; same coefficient bytes.
- `P256_A_OFFSET = "3"` names upstream `BigInt(3)` so C# computes `p - 3`; same curve coefficient derivation.

**D. D17 Enforcement Check**

N/A (not Crypto.cs)

**E. Logic Divergence Findings**

Zero entries.

I checked the constants against upstream P-256 decompression and compressed-key handling: `COMPRESSED_PUBLIC_KEY_SIZE` matches upstream length `33`, `P256_P` matches upstream modulus, `P256_B` matches upstream coefficient `b`, and `P256_A_OFFSET` matches upstream `a = p - BigInt(3)`. No algorithm step order, byte ordering, leading-zero, padding, signature, DTO, or JSON behavior exists in this file.

**F. Fixture Comparison Gate**

All applicable fixtures match.

- `tests/Fixtures/crypto/turnkey-crypto-vectors.json` is pinned to `@turnkey/crypto@2.8.8` with sha256 `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`.
- Its credential-bundle vector matches upstream `ts-source/__tests__/crypto-test.ts:179-184`; C# asserts the same decrypted hex in `tests/CryptoTests.cs:524-537`.
- Its invalid compressed-key prefix vector matches upstream `ts-source/__tests__/crypto-test.ts:241-248`; C# asserts the same error in `tests/CryptoTests.cs:541-548`.
- Its compressed/uncompressed public-key coverage follows upstream `ts-source/__tests__/crypto-test.ts:172-177`; C# asserts compression emits 33 bytes and round-trips to the original uncompressed key in `tests/CryptoTests.cs:197-209`.

`encoding` fixtures do not exercise this file. `api-key-stamper` fixtures use `CryptoConstants.CURVE_NAME` only in test-side verification helpers, not as fixture coverage for this file’s `@turnkey/crypto` parity.

**G. Unresolved Assumptions**

- I did not run `dotnet test`; the workspace is read-only, and test execution would normally write `bin/obj` artifacts.
- There is no `tests/Fixtures/Generators/` content to rerun fixture generation in this round, so fixture provenance was verified statically against the pinned upstream snapshot.


