# Codex review — src/Encoding.cs — round 1 — 20260523

C# SHA: `0ffef786d2d8ff119e598316cb5aa93b98562d65`
Upstream snapshot: `turnkey-encoding-0.6.0`

---

2026-05-22T23:22:36.924429Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e51ff-7a43-73f1-801b-9533d30e4b9d
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 1 of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA (0ffef786d2d8ff119e598316cb5aa93b98562d65).

B. Method coverage table: every public + internal helper method in src/Encoding.cs
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

2026-05-22T23:22:38.436323Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll verify the C# file against the pinned local TypeScript snapshot and use the review workflow since this is explicitly a code review. First I’m gathering the file, upstream source, checksums, fixtures, and any relevant prior context.
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
/bin/zsh -lc 'rg -n "turnkey-sdk-csharp|turnkey|Encoding.cs|C# SDK" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
273:# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
275:scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.

exec
/bin/zsh -lc "rg -n \"turnkey-encoding|sha256|encoding\" codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 tests/Fixtures tests src -g '"'!bin'"' -g '"'!obj'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 exited 2 in 0ms:
rg: tests/Fixtures: No such file or directory (os error 2)
codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt:3:2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
src/turnkey-sdk-csharp.csproj:12:    <Description>Unofficial / community-maintained Turnkey SDK for .NET by Kyuzan Inc. Not affiliated with Turnkey, Inc. 1:1 logical port of @turnkey/crypto v2.8.8, @turnkey/http v3.16.0, @turnkey/api-key-stamper v0.5.0, and @turnkey/encoding v0.6.0 (peak-pinned versions). Provides P-256 ECDSA / HPKE / HKDF / API key stamping primitives for .NET. BouncyCastle 2.5.0 is used only for primitives; HPKE / HKDF / bundle parsing are direct ports of the upstream Turnkey TypeScript logic.</Description>
tests/EncodingTests.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
tests/EncodingTests.cs:259:            // From upstream private-key encoding vector
src/Encoding.cs:1:// 1:1 logical port of @turnkey/encoding@0.6.0
src/Encoding.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
src/Encoding.cs:50:    /// 1:1 logical port of <c>@turnkey/encoding</c> v0.6.0.
src/Encoding.cs:55:        /// Internal constants exported by <c>@turnkey/encoding</c>.
src/Encoding.cs:61:            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
src/Encoding.cs:515:        // Unity-port-derived helpers (NOT in @turnkey/encoding)
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:2:  "name": "@turnkey/encoding",
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:24:    "directory": "packages/encoding"
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:1:# @turnkey/encoding
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:67:- 93666ff: turnkey/crypto standard HPKE encryption, first major release. Allows for programmatic importing in environments like node. Moved some encoding helper functions to turnkey/encoding
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/README.md:1:# @turnkey/encoding
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/README.md:3:This is a package containing decoding and encoding functions. This is used by other Turnkey packages. Feel free to import exported functions into your own apps, but be aware that there may be significant interface changes in the future.

exec
/bin/zsh -lc "git status --short && git log --oneline -5 -- src/Encoding.cs && nl -ba src/Encoding.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 3851ms:
2026-05-23 08:22:57.232 xcodebuild[27516:105840250]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:22:57.497 xcodebuild[27516:105840249] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-sC4E9uW2' (errno=Operation not permitted)
2026-05-23 08:22:58.018 xcodebuild[27522:105840311]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:22:58.162 xcodebuild[27522:105840309] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/Encoding.cs-r1-20260523.md
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-yliaSxdi' (errno=Operation not permitted)
2026-05-23 08:22:59.136 xcodebuild[27534:105840431]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:22:59.277 xcodebuild[27534:105840429] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-qKkRPCel' (errno=Operation not permitted)
2026-05-23 08:22:59.683 xcodebuild[27537:105840452]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:22:59.835 xcodebuild[27537:105840451] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0
     1	// 1:1 logical port of @turnkey/encoding@0.6.0
     2	//
     3	// Upstream snapshot:
     4	//   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
     5	//
     6	// Files covered:
     7	//   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
     8	//                                HexToAscii / NormalizePadding
     9	//   ts-source/base64.ts       -> StringToBase64UrlString /
    10	//                                HexStringToBase64Url /
    11	//                                Base64StringToBase64UrlEncodedString /
    12	//                                Base64UrlToBase64 /
    13	//                                DecodeBase64UrlToString
    14	//   ts-source/encode.ts       -> PointEncode
    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
    17	//   ts-source/index.ts        -> DEFAULT_JWK_MEMBER_BYTE_LENGTH constant
    18	//
    19	// Adaptations:
    20	//   Uint8Array          -> byte[]
    21	//   String.fromCharCode(b) reduction -> direct byte buffer + Convert.ToBase64String
    22	//   (the upstream "btoa(s)" implementation produces wire-identical bytes to
    23	//    Convert.ToBase64String when the input is 0-255-bounded, which it always
    24	//    is after hex -> Uint8Array conversion. The custom btoa only exists in
    25	//    upstream to support React Native; it does not affect wire bytes.)
    26	//   throw new Error(msg) -> throw new ArgumentException(msg)
    27	//   regex match         -> System.Text.RegularExpressions.Regex
    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
    29	//                           (same algorithm; produces the same bytes for valid
    30	//                            input as the bs58 npm package)
    31	//
    32	// Unity-specific helpers kept for Crypto.cs consumption (also present in the
    33	// peak Unity port):
    34	//   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
    35	//                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
    36	//   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
    37	//                         array literals; this helper centralizes the pattern)
    38	
    39	using System;
    40	using System.Linq;
    41	using System.Security.Cryptography;
    42	using System.Text;
    43	using System.Text.RegularExpressions;
    44	using Org.BouncyCastle.Math;
    45	
    46	namespace Turnkey
    47	{
    48	    /// <summary>
    49	    /// Encoding utilities for the Turnkey API.
    50	    /// 1:1 logical port of <c>@turnkey/encoding</c> v0.6.0.
    51	    /// </summary>
    52	    public static class Encoding
    53	    {
    54	        /// <summary>
    55	        /// Internal constants exported by <c>@turnkey/encoding</c>.
    56	        /// </summary>
    57	        public static class Constants
    58	        {
    59	            /// <summary>
    60	            /// JWK member byte length used by the upstream <c>index.ts</c>.
    61	            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
    62	            /// </summary>
    63	            public const int DEFAULT_JWK_MEMBER_BYTE_LENGTH = 32;
    64	
    65	            /// <summary>
    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
    67	            /// </summary>
    68	            public const string BASE58_ALPHABET =
    69	                "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    70	
    71	            /// <summary>
    72	            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
    73	            /// </summary>
    74	            internal const string BASE64_KEYSTR =
    75	                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    76	        }
    77	
    78	        private static readonly Regex HexRegex = new Regex(
    79	            "^[0-9A-Fa-f]+$",
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
   134	            if (!length.HasValue)
   135	            {
   136	                return buffer;
   137	            }
   138	
   139	            if (hexString.Length / 2 > length.Value)
   140	            {
   141	                throw new ArgumentException(
   142	                    "hex value cannot fit in a buffer of " + length.Value + " byte(s)");
   143	            }
   144	
   145	            var paddedBuffer = new byte[length.Value];
   146	            // Left-pad: zeros at start, original bytes at the end.
   147	            Array.Copy(buffer, 0, paddedBuffer, length.Value - buffer.Length, buffer.Length);
   148	            return paddedBuffer;
   149	        }
   150	
   151	        /// <summary>
   152	        /// Converts a hex string to an ASCII string.
   153	        /// Upstream: <c>hex.ts hexToAscii</c>.
   154	        /// </summary>
   155	        public static string HexToAscii(string hexString)
   156	        {
   157	            if (hexString == null)
   158	            {
   159	                throw new ArgumentNullException(nameof(hexString));
   160	            }
   161	
   162	            var sb = new StringBuilder(hexString.Length / 2);
   163	            for (int i = 0; i + 1 < hexString.Length; i += 2)
   164	            {
   165	                sb.Append((char)Convert.ToInt32(hexString.Substring(i, 2), 16));
   166	            }
   167	            return sb.ToString();
   168	        }
   169	
   170	        /// <summary>
   171	        /// Normalizes the padding of a byte array to a target length by either
   172	        /// left-padding with zero bytes or stripping leading zero bytes.
   173	        /// Throws if too many non-zero bytes would have to be removed.
   174	        /// Upstream: <c>hex.ts normalizePadding</c>.
   175	        /// </summary>
   176	        public static byte[] NormalizePadding(byte[] byteArray, int targetLength)
   177	        {
   178	            if (byteArray == null)
   179	            {
   180	                throw new ArgumentNullException(nameof(byteArray));
   181	            }
   182	
   183	            int paddingLength = targetLength - byteArray.Length;
   184	
   185	            // Add leading zeros
   186	            if (paddingLength > 0)
   187	            {
   188	                var result = new byte[targetLength];
   189	                Array.Copy(byteArray, 0, result, paddingLength, byteArray.Length);
   190	                return result;
   191	            }
   192	
   193	            // Strip leading zeros
   194	            if (paddingLength < 0)
   195	            {
   196	                int expectedZeroCount = -paddingLength;
   197	                int zeroCount = 0;
   198	                for (int i = 0; i < expectedZeroCount && i < byteArray.Length; i++)
   199	                {
   200	                    if (byteArray[i] == 0)
   201	                    {
   202	                        zeroCount++;
   203	                    }
   204	                }
   205	                if (zeroCount != expectedZeroCount)
   206	                {
   207	                    throw new ArgumentException(
   208	                        "invalid number of starting zeroes. Expected number of zeroes: "
   209	                        + expectedZeroCount + ". Found: " + zeroCount + ".");
   210	                }
   211	
   212	                var result = new byte[targetLength];
   213	                Array.Copy(byteArray, expectedZeroCount, result, 0, targetLength);
   214	                return result;
   215	            }
   216	
   217	            return byteArray;
   218	        }
   219	
   220	        // ============================================================
   221	        // base64.ts
   222	        // ============================================================
   223	
   224	        /// <summary>
   225	        /// Converts a plain string into a base64url-encoded string.
   226	        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
   227	        /// </summary>
   228	        /// <remarks>
   229	        /// Upstream uses a pure-JS <c>btoa</c> for React Native compatibility and
   230	        /// throws on code points greater than 0xFF. This port preserves that
   231	        /// behavior by checking each code point against 0xFF before delegating to
   232	        /// <c>System.Convert.ToBase64String</c>, which is wire-identical for the
   233	        /// 0-255 byte range.
   234	        /// </remarks>
   235	        public static string StringToBase64UrlString(string input)
   236	        {
   237	            if (input == null)
   238	            {
   239	                throw new ArgumentNullException(nameof(input));
   240	            }
   241	
   242	            var bytes = new byte[input.Length];
   243	            for (int i = 0; i < input.Length; i++)
   244	            {
   245	                int cp = input[i];
   246	                if (cp > 0xFF)
   247	                {
   248	                    throw new ArgumentException(
   249	                        "InvalidCharacterError: found code point greater than 255:"
   250	                        + cp + " at position " + i);
   251	                }
   252	                bytes[i] = (byte)cp;
   253	            }
   254	
   255	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(bytes));
   256	        }
   257	
   258	        /// <summary>
   259	        /// Converts a hex string into a base64url-encoded string.
   260	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.

codex
The review skill’s generic changelog/codex-runner flow is not a clean fit here because your requested artifact is a pinned cross-language parity review, and the sandbox is read-only. I’m applying its review stance, but keeping this as source-level verification against the pinned snapshot rather than trying to mutate the repo.
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

exec
/bin/zsh -lc 'find codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 -maxdepth 4 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/LICENSE
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/README.md
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts

exec
/bin/zsh -lc "find tests -maxdepth 4 -type f | sort && find . -maxdepth 4 -type d -name 'Fixtures' -print" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/EncodingTests.cs
tests/bin/Release/net8.0/.msCoverageSourceRootsMapping_turnkey-sdk-csharp.Tests
tests/bin/Release/net8.0/BouncyCastle.Cryptography.dll
tests/bin/Release/net8.0/CoverletSourceRootsMapping_turnkey-sdk-csharp.Tests
tests/bin/Release/net8.0/FluentAssertions.dll
tests/bin/Release/net8.0/KyuzanInc.Turnkey.Sdk.dll
tests/bin/Release/net8.0/KyuzanInc.Turnkey.Sdk.pdb
tests/bin/Release/net8.0/KyuzanInc.Turnkey.Sdk.xml
tests/bin/Release/net8.0/Microsoft.TestPlatform.CommunicationUtilities.dll
tests/bin/Release/net8.0/Microsoft.TestPlatform.CoreUtilities.dll
tests/bin/Release/net8.0/Microsoft.TestPlatform.CrossPlatEngine.dll
tests/bin/Release/net8.0/Microsoft.TestPlatform.PlatformAbstractions.dll
tests/bin/Release/net8.0/Microsoft.TestPlatform.Utilities.dll
tests/bin/Release/net8.0/Microsoft.VisualStudio.CodeCoverage.Shim.dll
tests/bin/Release/net8.0/Microsoft.VisualStudio.TestPlatform.Common.dll
tests/bin/Release/net8.0/Microsoft.VisualStudio.TestPlatform.ObjectModel.dll
tests/bin/Release/net8.0/Mono.Cecil.Mdb.dll
tests/bin/Release/net8.0/Mono.Cecil.Pdb.dll
tests/bin/Release/net8.0/Mono.Cecil.Rocks.dll
tests/bin/Release/net8.0/Mono.Cecil.dll
tests/bin/Release/net8.0/Newtonsoft.Json.dll
tests/bin/Release/net8.0/PublicApiGenerator.dll
tests/bin/Release/net8.0/System.CodeDom.dll
tests/bin/Release/net8.0/System.Configuration.ConfigurationManager.dll
tests/bin/Release/net8.0/System.Security.Cryptography.ProtectedData.dll
tests/bin/Release/net8.0/System.Text.Json.dll
tests/bin/Release/net8.0/testhost.dll
tests/bin/Release/net8.0/turnkey-sdk-csharp.Tests.deps.json
tests/bin/Release/net8.0/turnkey-sdk-csharp.Tests.dll
tests/bin/Release/net8.0/turnkey-sdk-csharp.Tests.pdb
tests/bin/Release/net8.0/turnkey-sdk-csharp.Tests.runtimeconfig.json
tests/bin/Release/net8.0/turnkey-sdk-csharp.Tests.xml
tests/bin/Release/net8.0/xunit.abstractions.dll
tests/bin/Release/net8.0/xunit.assert.dll
tests/bin/Release/net8.0/xunit.core.dll
tests/bin/Release/net8.0/xunit.execution.dotnet.dll
tests/bin/Release/net8.0/xunit.runner.reporters.netcoreapp10.dll
tests/bin/Release/net8.0/xunit.runner.utility.netcoreapp10.dll
tests/bin/Release/net8.0/xunit.runner.visualstudio.testadapter.dll
tests/obj/Release/net8.0/.NETCoreApp,Version=v8.0.AssemblyAttributes.cs
tests/obj/Release/net8.0/turnkey-.10D47728.Up2Date
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.AssemblyInfo.cs
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.AssemblyInfoInputs.cache
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.GeneratedMSBuildEditorConfig.editorconfig
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.assets.cache
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.csproj.AssemblyReference.cache
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.csproj.CoreCompileInputs.cache
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.csproj.FileListAbsolute.txt
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.dll
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.genruntimeconfig.cache
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.pdb
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.sourcelink.json
tests/obj/Release/net8.0/turnkey-sdk-csharp.Tests.xml
tests/obj/project.assets.json
tests/obj/project.nuget.cache
tests/obj/turnkey-sdk-csharp.Tests.csproj.nuget.dgspec.json
tests/obj/turnkey-sdk-csharp.Tests.csproj.nuget.g.props
tests/obj/turnkey-sdk-csharp.Tests.csproj.nuget.g.targets
tests/packages.lock.json
tests/turnkey-sdk-csharp.Tests.csproj

exec
/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '235,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   235	        public static string StringToBase64UrlString(string input)
   236	        {
   237	            if (input == null)
   238	            {
   239	                throw new ArgumentNullException(nameof(input));
   240	            }
   241	
   242	            var bytes = new byte[input.Length];
   243	            for (int i = 0; i < input.Length; i++)
   244	            {
   245	                int cp = input[i];
   246	                if (cp > 0xFF)
   247	                {
   248	                    throw new ArgumentException(
   249	                        "InvalidCharacterError: found code point greater than 255:"
   250	                        + cp + " at position " + i);
   251	                }
   252	                bytes[i] = (byte)cp;
   253	            }
   254	
   255	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(bytes));
   256	        }
   257	
   258	        /// <summary>
   259	        /// Converts a hex string into a base64url-encoded string.
   260	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
   261	        /// </summary>
   262	        public static string HexStringToBase64Url(string input, int? length = null)
   263	        {
   264	            if (input == null)
   265	            {
   266	                throw new ArgumentNullException(nameof(input));
   267	            }
   268	
   269	            // Add a leading 0 if needed to get an even-length hex string.
   270	            // padStart(Math.ceil(input.length / 2) * 2, "0")
   271	            int targetHexLen = ((input.Length + 1) / 2) * 2;
   272	            string hexString = input.Length < targetHexLen
   273	                ? input.PadLeft(targetHexLen, '0')
   274	                : input;
   275	
   276	            var buffer = Uint8ArrayFromHexString(hexString, length);
   277	
   278	            // Upstream then folds the buffer into a per-byte string via
   279	            // String.fromCharCode and calls btoa on that. Convert.ToBase64String
   280	            // on the raw bytes produces the same wire bytes (validated by tests).
   281	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(buffer));
   282	        }
   283	
   284	        /// <summary>
   285	        /// Converts a standard base64 string into a base64url-encoded string by
   286	        /// replacing <c>+</c> with <c>-</c>, <c>/</c> with <c>_</c>, and stripping <c>=</c>.
   287	        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
   288	        /// </summary>
   289	        public static string Base64StringToBase64UrlEncodedString(string input)
   290	        {
   291	            if (input == null)
   292	            {
   293	                throw new ArgumentNullException(nameof(input));
   294	            }
   295	            return input.Replace('+', '-').Replace('/', '_').Replace("=", string.Empty);
   296	        }
   297	
   298	        /// <summary>
   299	        /// Converts a base64url string into a standard base64 string (with
   300	        /// <c>=</c> padding restored).
   301	        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
   302	        /// </summary>
   303	        public static string Base64UrlToBase64(string input)
   304	        {
   305	            if (input == null)
   306	            {
   307	                throw new ArgumentNullException(nameof(input));
   308	            }
   309	            string b64 = input.Replace('-', '+').Replace('_', '/');
   310	            int padLen = (4 - (b64.Length % 4)) % 4;
   311	            return b64 + new string('=', padLen);
   312	        }
   313	
   314	        /// <summary>
   315	        /// Decodes a base64url-encoded string into a plain string by first
   316	        /// restoring base64 padding and then base64-decoding into a byte buffer.
   317	        /// Each output byte is then reinterpreted as a code point.
   318	        /// Upstream: <c>base64.ts decodeBase64urlToString</c>.
   319	        /// </summary>
   320	        public static string DecodeBase64UrlToString(string input)
   321	        {
   322	            string b64 = Base64UrlToBase64(input);
   323	            byte[] bytes = Convert.FromBase64String(b64);
   324	
   325	            // Upstream atob returns a string where each char's code point is the
   326	            // raw byte (0..255). Match that by reinterpreting bytes as chars.
   327	            var sb = new StringBuilder(bytes.Length);
   328	            for (int i = 0; i < bytes.Length; i++)
   329	            {
   330	                sb.Append((char)bytes[i]);
   331	            }
   332	            return sb.ToString();
   333	        }
   334	
   335	        // ============================================================
   336	        // encode.ts
   337	        // ============================================================
   338	
   339	        /// <summary>
   340	        /// Compresses an uncompressed P-256 public key into its 33-byte compressed form.
   341	        /// Upstream: <c>encode.ts pointEncode</c>.
   342	        /// </summary>
   343	        /// <param name="raw">65-byte uncompressed key starting with 0x04.</param>
   344	        /// <returns>33-byte compressed key starting with 0x02 or 0x03.</returns>
   345	        public static byte[] PointEncode(byte[] raw)
   346	        {
   347	            if (raw == null)
   348	            {
   349	                throw new ArgumentNullException(nameof(raw));
   350	            }
   351	            if (raw.Length != 65 || raw[0] != 0x04)
   352	            {
   353	                throw new ArgumentException("Invalid uncompressed P-256 key");
   354	            }
   355	
   356	            // x = raw[1..33], y = raw[33..65]; lengths are 32 and 32 by construction.
   357	            byte yLastByte = raw[64];
   358	            byte prefix = (yLastByte & 1) == 0 ? (byte)0x02 : (byte)0x03;
   359	
   360	            var compressed = new byte[33];
   361	            compressed[0] = prefix;
   362	            Array.Copy(raw, 1, compressed, 1, 32);
   363	            return compressed;
   364	        }
   365	
   366	        // ============================================================
   367	        // bs58.ts (and bs58check.ts)
   368	        //
   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
   370	        // implements the same Bitcoin base58 algorithm in C# using BouncyCastle's
   371	        // BigInteger. The algorithm is standard and produces wire-identical bytes
   372	        // to the upstream packages for valid input.
   373	        // ============================================================
   374	
   375	        /// <summary>
   376	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
   378	        /// </summary>
   379	        public static string Base58Encode(byte[] data)
   380	        {
   381	            if (data == null)
   382	            {
   383	                throw new ArgumentNullException(nameof(data));
   384	            }
   385	            if (data.Length == 0)
   386	            {
   387	                return string.Empty;
   388	            }
   389	
   390	            var intData = new BigInteger(1, data);
   391	            var sb = new StringBuilder();
   392	            var fiftyEight = BigInteger.ValueOf(58);
   393	
   394	            while (intData.CompareTo(BigInteger.Zero) > 0)
   395	            {
   396	                var divmod = intData.DivideAndRemainder(fiftyEight);
   397	                intData = divmod[0];
   398	                sb.Insert(0, Constants.BASE58_ALPHABET[divmod[1].IntValue]);
   399	            }
   400	
   401	            // Preserve leading zero bytes as '1' characters (Base58 convention).
   402	            for (int i = 0; i < data.Length && data[i] == 0; i++)
   403	            {
   404	                sb.Insert(0, '1');
   405	            }
   406	
   407	            return sb.ToString();
   408	        }
   409	
   410	        /// <summary>
   411	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
   413	        /// </summary>
   414	        public static byte[] Base58Decode(string encoded)
   415	        {
   416	            if (encoded == null)
   417	            {
   418	                throw new ArgumentNullException(nameof(encoded));
   419	            }
   420	            if (encoded.Length == 0)
   421	            {
   422	                return new byte[0];
   423	            }
   424	
   425	            var decoded = BigInteger.Zero;
   426	            var multi = BigInteger.One;
   427	            var fiftyEight = BigInteger.ValueOf(58);
   428	
   429	            for (int i = encoded.Length - 1; i >= 0; i--)
   430	            {
   431	                int digit = Constants.BASE58_ALPHABET.IndexOf(encoded[i]);
   432	                if (digit < 0)
   433	                {
   434	                    throw new ArgumentException(
   435	                        "Invalid character '" + encoded[i] + "' in base58 string");
   436	                }
   437	                decoded = decoded.Add(multi.Multiply(BigInteger.ValueOf(digit)));
   438	                multi = multi.Multiply(fiftyEight);
   439	            }
   440	
   441	            byte[] bytes = decoded.Equals(BigInteger.Zero)
   442	                ? new byte[0]
   443	                : decoded.ToByteArrayUnsigned();
   444	
   445	            int leadingZeros = 0;
   446	            for (int i = 0; i < encoded.Length && encoded[i] == '1'; i++)
   447	            {
   448	                leadingZeros++;
   449	            }
   450	            if (leadingZeros > 0)
   451	            {
   452	                var result = new byte[leadingZeros + bytes.Length];
   453	                Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);
   454	                return result;
   455	            }
   456	            return bytes;
   457	        }
   458	
   459	        /// <summary>
   460	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
   462	        /// </summary>
   463	        public static string Base58CheckEncode(byte[] payload)
   464	        {
   465	            if (payload == null)
   466	            {
   467	                throw new ArgumentNullException(nameof(payload));
   468	            }
   469	            using (var sha = SHA256.Create())
   470	            {
   471	                var h1 = sha.ComputeHash(payload);
   472	                var h2 = sha.ComputeHash(h1);
   473	                var checksum = new byte[4];
   474	                Array.Copy(h2, 0, checksum, 0, 4);
   475	                var concat = new byte[payload.Length + 4];
   476	                Array.Copy(payload, 0, concat, 0, payload.Length);
   477	                Array.Copy(checksum, 0, concat, payload.Length, 4);
   478	                return Base58Encode(concat);
   479	            }
   480	        }
   481	
   482	        /// <summary>
   483	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
   484	        /// SHA256(SHA256(payload)) checksum.
   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
   486	        /// </summary>
   487	        public static byte[] Base58CheckDecode(string encoded)
   488	        {
   489	            byte[] decoded = Base58Decode(encoded);
   490	            if (decoded.Length < 4)
   491	            {
   492	                throw new ArgumentException("Invalid Base58Check string - too short");
   493	            }
   494	
   495	            var data = new byte[decoded.Length - 4];
   496	            Array.Copy(decoded, 0, data, 0, data.Length);
   497	            var checksum = new byte[4];
   498	            Array.Copy(decoded, decoded.Length - 4, checksum, 0, 4);
   499	
   500	            using (var sha = SHA256.Create())
   501	            {
   502	                var h1 = sha.ComputeHash(data);
   503	                var h2 = sha.ComputeHash(h1);
   504	                if (h2[0] != checksum[0] || h2[1] != checksum[1]
   505	                    || h2[2] != checksum[2] || h2[3] != checksum[3])
   506	                {
   507	                    throw new ArgumentException("Invalid Base58Check checksum");
   508	                }
   509	            }
   510	
   511	            return data;
   512	        }
   513	
   514	        // ============================================================
   515	        // Unity-port-derived helpers (NOT in @turnkey/encoding)
   516	        //
   517	        // The upstream library reaches for these patterns inline (TextDecoder,
   518	        // spread). The C# port keeps them here as public helpers because
   519	        // Crypto.cs / ApiKeyStamper.cs / Http.cs use them.
   520	        // ============================================================
   521	
   522	        /// <summary>
   523	        /// UTF-8 decodes a byte array into a string.
   524	        /// </summary>
   525	        /// <remarks>
   526	        /// Upstream uses <c>new TextDecoder().decode(bytes)</c> inline. This
   527	        /// helper centralizes the equivalent in the .NET port. It is
   528	        /// wire-irrelevant — it is invoked after wire bytes have already been
   529	        /// decided.
   530	        /// </remarks>
   531	        public static string Uint8ArrayToString(byte[] bytes)
   532	        {
   533	            if (bytes == null)
   534	            {
   535	                throw new ArgumentNullException(nameof(bytes));
   536	            }
   537	            return System.Text.Encoding.UTF8.GetString(bytes);
   538	        }
   539	
   540	        /// <summary>
   541	        /// Concatenates byte arrays.
   542	        /// </summary>
   543	        /// <remarks>
   544	        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
   545	        /// This helper centralizes the equivalent. It is wire-irrelevant — it
   546	        /// just glues bytes that were already computed.
   547	        /// </remarks>
   548	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
   549	        {
   550	            if (arrays == null)
   551	            {
   552	                throw new ArgumentNullException(nameof(arrays));
   553	            }
   554	            int total = arrays.Sum(a => a?.Length ?? 0);
   555	            var result = new byte[total];
   556	            int offset = 0;
   557	            foreach (var a in arrays)
   558	            {
   559	                if (a != null && a.Length > 0)
   560	                {
   561	                    Array.Copy(a, 0, result, offset, a.Length);
   562	                    offset += a.Length;
   563	                }
   564	            }
   565	            return result;
   566	        }
   567	    }
   568	}

exec
/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	/**
     2	 * Converts a Uint8Array into a lowercase hex string.
     3	 *
     4	 * @param {Uint8Array} input - The input byte array.
     5	 * @returns {string} - The resulting hex string.
     6	 */
     7	export function uint8ArrayToHexString(input: Uint8Array): string {
     8	  return input.reduce(
     9	    (result, x) => result + x.toString(16).padStart(2, "0"),
    10	    "",
    11	  );
    12	}
    13	
    14	/**
    15	 * Creates a Uint8Array from a hex string.
    16	 *
    17	 * @param {string} hexString - The input hex string.
    18	 * @param {number} [length] - Optional target length for the output. If specified,
    19	 * the result will be padded with leading 0s or throw if it overflows.
    20	 * @returns {Uint8Array} - The resulting byte array.
    21	 * @throws {Error} - If the hex string is invalid or too long for the specified length.
    22	 */
    23	export const uint8ArrayFromHexString = (
    24	  hexString: string,
    25	  length?: number,
    26	): Uint8Array => {
    27	  const hexRegex = /^[0-9A-Fa-f]+$/;
    28	  if (!hexString || hexString.length % 2 != 0 || !hexRegex.test(hexString)) {
    29	    throw new Error(
    30	      `cannot create uint8array from invalid hex string: "${hexString}"`,
    31	    );
    32	  }
    33	
    34	  const buffer = new Uint8Array(
    35	    hexString!.match(/../g)!.map((h: string) => parseInt(h, 16)),
    36	  );
    37	
    38	  if (!length) {
    39	    return buffer;
    40	  }
    41	  if (hexString.length / 2 > length) {
    42	    throw new Error(
    43	      "hex value cannot fit in a buffer of " + length + " byte(s)",
    44	    );
    45	  }
    46	
    47	  // If a length is specified, ensure we sufficiently pad
    48	  let paddedBuffer = new Uint8Array(length);
    49	  paddedBuffer.set(buffer, length - buffer.length);
    50	  return paddedBuffer;
    51	};
    52	
    53	/**
    54	 * Converts a hex string to an ASCII string.
    55	 * @param {string} hexString - The input hex string to convert.
    56	 * @returns {string} - The converted ASCII string.
    57	 */
    58	export function hexToAscii(hexString: string): string {
    59	  let asciiStr = "";
    60	  for (let i = 0; i < hexString.length; i += 2) {
    61	    asciiStr += String.fromCharCode(parseInt(hexString.substr(i, 2), 16));
    62	  }
    63	  return asciiStr;
    64	}
    65	
    66	/**
    67	 * Function to normalize padding of byte array with 0's to a target length.
    68	 *
    69	 * @param {Uint8Array} byteArray - The byte array to pad or trim.
    70	 * @param {number} targetLength - The target length after padding or trimming.
    71	 * @returns {Uint8Array} - The normalized byte array.
    72	 */
    73	export const normalizePadding = (
    74	  byteArray: Uint8Array,
    75	  targetLength: number,
    76	): Uint8Array => {
    77	  const paddingLength = targetLength - byteArray.length;
    78	
    79	  // Add leading 0's to array
    80	  if (paddingLength > 0) {
    81	    const padding = new Uint8Array(paddingLength).fill(0);
    82	    return new Uint8Array([...padding, ...byteArray]);
    83	  }
    84	
    85	  // Remove leading 0's from array
    86	  if (paddingLength < 0) {
    87	    const expectedZeroCount = paddingLength * -1;
    88	    let zeroCount = 0;
    89	    for (let i = 0; i < expectedZeroCount && i < byteArray.length; i++) {
    90	      if (byteArray[i] === 0) {
    91	        zeroCount++;
    92	      }
    93	    }
    94	    // Check if the number of zeros found equals the number of zeroes expected
    95	    if (zeroCount !== expectedZeroCount) {
    96	      throw new Error(
    97	        `invalid number of starting zeroes. Expected number of zeroes: ${expectedZeroCount}. Found: ${zeroCount}.`,
    98	      );
    99	    }
   100	    return byteArray.slice(expectedZeroCount, expectedZeroCount + targetLength);
   101	  }
   102	  return byteArray;
   103	};
     1	/**
     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
     3	 */
     4	
     5	import { uint8ArrayFromHexString } from "./hex";
     6	
     7	/**
     8	 * Converts a plain string into a base64url-encoded string.
     9	 *
    10	 * @param {string} input - The input string to encode.
    11	 * @returns {string} - The base64url-encoded string.
    12	 */
    13	export function stringToBase64urlString(input: string): string {
    14	  // string to base64
    15	  // we do not rely on the browser's btoa since it's not present in React Native environments
    16	  const base64String = btoa(input);
    17	  return base64StringToBase64UrlEncodedString(base64String);
    18	}
    19	
    20	/**
    21	 * Converts a hex string into a base64url-encoded string.
    22	 *
    23	 * @param {string} input - The input hex string.
    24	 * @param {number} [length] - Optional length for the resulting buffer. Pads with leading 0s if needed.
    25	 * @returns {string} - The base64url-encoded representation of the hex string.
    26	 * @throws {Error} - If the hex string is invalid or too long for the specified length.
    27	 */
    28	export function hexStringToBase64url(input: string, length?: number): string {
    29	  // Add an extra 0 to the start of the string to get a valid hex string (even length)
    30	  // (e.g. 0x0123 instead of 0x123)
    31	  const hexString = input.padStart(Math.ceil(input.length / 2) * 2, "0");
    32	  const buffer = uint8ArrayFromHexString(hexString, length);
    33	
    34	  return stringToBase64urlString(
    35	    buffer.reduce((result, x) => result + String.fromCharCode(x), ""),
    36	  );
    37	}
    38	
    39	/**
    40	 * Converts a base64 string into a base64url-encoded string.
    41	 *
    42	 * @param {string} input - The input base64 string.
    43	 * @returns {string} - The base64url-encoded string.
    44	 */
    45	export function base64StringToBase64UrlEncodedString(input: string): string {
    46	  return input.replace(/\+/g, "-").replace(/\//g, "_").replace(/=/g, "");
    47	}
    48	
    49	/**
    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
    51	 *
    52	 * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
    53	 * - Pads the result with `=` to ensure the length is a multiple of 4.
    54	 *
    55	 * @param {string} input - The base64url-encoded string to convert.
    56	 * @returns {string} - The equivalent base64-encoded string.
    57	 */
    58	export function base64UrlToBase64(input: string): string {
    59	  let b64 = input.replace(/-/g, "+").replace(/_/g, "/");
    60	  const padLen = (4 - (b64.length % 4)) % 4;
    61	  return b64 + "=".repeat(padLen);
    62	}
    63	
    64	/**
    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
    66	 *
    67	 * - Converts the input from base64url to base64.
    68	 * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
    69	 *
    70	 * @param {string} input - The base64url-encoded string to decode.
    71	 * @returns {string} - The decoded plain string.
    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
    73	 */
    74	export function decodeBase64urlToString(input: string): string {
    75	  const b64 = base64UrlToBase64(input);
    76	  return atob(b64);
    77	}
    78	
    79	// Pure JS implementation of btoa. This is adapted from the following:
    80	// https://github.com/jsdom/abab/blob/80874ae1fe1cde2e587bb6e51b6d7c9b42ca1d34/lib/btoa.js
    81	function btoa(s: string): string {
    82	  if (arguments.length === 0) {
    83	    throw new TypeError("1 argument required, but only 0 present.");
    84	  }
    85	
    86	  let i;
    87	  // String conversion as required by Web IDL.
    88	  s = `${s}`;
    89	
    90	  // "The btoa() method must throw an "InvalidCharacterError" DOMException if
    91	  // data contains any character whose code point is greater than U+00FF."
    92	  for (i = 0; i < s.length; i++) {
    93	    if (s.charCodeAt(i) > 255) {
    94	      throw new Error(
    95	        `InvalidCharacterError: found code point greater than 255:${s.charCodeAt(
    96	          i,
    97	        )} at position ${i}`,
    98	      );
    99	    }
   100	  }
   101	
   102	  let out = "";
   103	  for (i = 0; i < s.length; i += 3) {
   104	    const groupsOfSix: (number | undefined)[] = [
   105	      undefined,
   106	      undefined,
   107	      undefined,
   108	      undefined,
   109	    ];
   110	    groupsOfSix[0] = s.charCodeAt(i) >> 2;
   111	    groupsOfSix[1] = (s.charCodeAt(i) & 0x03) << 4;
   112	    if (s.length > i + 1) {
   113	      groupsOfSix[1] |= s.charCodeAt(i + 1) >> 4;
   114	      groupsOfSix[2] = (s.charCodeAt(i + 1) & 0x0f) << 2;
   115	    }
   116	    if (s.length > i + 2) {
   117	      groupsOfSix[2]! |= s.charCodeAt(i + 2) >> 6;
   118	      groupsOfSix[3] = s.charCodeAt(i + 2) & 0x3f;
   119	    }
   120	    for (let j = 0; j < groupsOfSix.length; j++) {
   121	      if (typeof groupsOfSix[j] === "undefined") {
   122	        out += "=";
   123	      } else {
   124	        out += btoaLookup(groupsOfSix[j]!);
   125	      }
   126	    }
   127	  }
   128	  return out;
   129	}
   130	
   131	function btoaLookup(index: number) {
   132	  /**
   133	   * Lookup table for btoa(), which converts a six-bit number into the
   134	   * corresponding ASCII character.
   135	   */
   136	  const keystr =
   137	    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
   138	
   139	  if (index >= 0 && index < 64) {
   140	    return keystr[index];
   141	  }
   142	
   143	  // Throw INVALID_CHARACTER_ERR exception here -- won't be hit in the tests.
   144	  return undefined;
   145	}
   146	
   147	// Pure JS implementation of btoa.
   148	export function atob(input: string): string {
   149	  if (arguments.length === 0) {
   150	    throw new TypeError("1 argument required, but only 0 present.");
   151	  }
   152	
   153	  // convert to string and remove invalid characters upfront
   154	  const str = String(input).replace(/[^A-Za-z0-9+/=]/g, "");
   155	
   156	  // the atob() method must throw an "InvalidCharacterError" if
   157	  // the length of the string is not a multiple of 4
   158	  if (str.length % 4 === 1) {
   159	    throw new Error(
   160	      "InvalidCharacterError: The string to be decoded is not correctly encoded.",
   161	    );
   162	  }
   163	
   164	  const keyStr =
   165	    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
   166	  let output = "";
   167	  let buffer = 0;
   168	  let bits = 0;
   169	  let i = 0;
   170	
   171	  // process each character
   172	  while (i < str.length) {
   173	    const ch = str.charAt(i);
   174	    const index = keyStr.indexOf(ch);
   175	
   176	    if (index < 0 || index > 64) {
   177	      i++;
   178	      continue;
   179	    }
   180	
   181	    if (ch === "=") {
   182	      // we skip padding characters
   183	      bits = 0;
   184	    } else {
   185	      buffer = (buffer << 6) | index;
   186	      bits += 6;
   187	    }
   188	
   189	    // output complete bytes
   190	    while (bits >= 8) {
   191	      bits -= 8;
   192	      output += String.fromCharCode((buffer >> bits) & 0xff);
   193	    }
   194	
   195	    i++;
   196	  }
   197	
   198	  return output;
   199	}

exec
/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// This is a temporary shim for bs58@6.0.0
     2	//
     3	// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
     4	//
     5	// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
     6	// issues with Metro (React Native). When importing the package using
     7	// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
     8	// resulting in a structure like `{ default: { default: { encode, decode, ... } } }`.
     9	//
    10	// This shim unwraps the exports until it reaches the object that contains `.decode`,
    11	// `.encode`, and `.decodeUnsafe`, allowing consistent usage across platforms.
    12	//
    13	// We can remove this shim once bs58 publishes a version that properly re-exports
    14	// named methods from its ESM build.
    15	
    16	import * as raw from "bs58";
    17	
    18	type Bs58 = {
    19	  encode(buffer: Uint8Array | number[]): string;
    20	  decodeUnsafe(str: string): Uint8Array | undefined;
    21	  decode(str: string): Uint8Array;
    22	};
    23	
    24	function unwrap(obj: any): any {
    25	  let cur = obj;
    26	  while (
    27	    cur &&
    28	    !(cur.encode && cur.decode && cur.decodeUnsafe) &&
    29	    cur.default
    30	  ) {
    31	    cur = cur.default;
    32	  }
    33	  return cur;
    34	}
    35	
    36	export const bs58 = unwrap(raw) as Bs58;
     1	// This is a temporary shim for bs58check@4.0.0
     2	//
     3	// See: https://github.com/bitcoinjs/bs58check/issues/47
     4	//
     5	// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
     6	// issues with Metro (React Native). When importing the package using
     7	// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
     8	// resulting in a structure like `{ default: { default: { encode, decode, ... } } }`.
     9	//
    10	// This shim unwraps the exports until it reaches the object that contains `.decode`,
    11	// `.encode`, and `.decodeUnsafe`, allowing consistent usage across platforms.
    12	//
    13	// We can remove this shim once bs58check publishes a version that properly re-exports
    14	// named methods from its ESM build
    15	
    16	import * as raw from "bs58check";
    17	
    18	type Bs58Check = {
    19	  encode(payload: Uint8Array | number[]): string;
    20	  decode(str: string): Uint8Array;
    21	  decodeUnsafe(str: string): Uint8Array | undefined;
    22	};
    23	
    24	function unwrap(obj: any): any {
    25	  let cur = obj;
    26	  while (
    27	    cur &&
    28	    !(cur.encode && cur.decode && cur.decodeUnsafe) &&
    29	    cur.default
    30	  ) {
    31	    cur = cur.default;
    32	  }
    33	  return cur;
    34	}
    35	
    36	export const bs58check = unwrap(raw) as Bs58Check;
     1	/**
     2	 * Compresses an uncompressed P-256 public key into its 33-byte compressed form.
     3	 *
     4	 * @param {Uint8Array} raw - The uncompressed public key (65 bytes, starting with 0x04).
     5	 * @returns {Uint8Array} - The compressed public key (33 bytes, starting with 0x02 or 0x03).
     6	 * @throws {Error} - If the input key is not a valid uncompressed P-256 key.
     7	 */
     8	export function pointEncode(raw: Uint8Array): Uint8Array {
     9	  if (raw.length !== 65 || raw[0] !== 0x04) {
    10	    throw new Error("Invalid uncompressed P-256 key");
    11	  }
    12	
    13	  const x = raw.slice(1, 33);
    14	  const y = raw.slice(33, 65);
    15	
    16	  if (x.length !== 32 || y.length !== 32) {
    17	    throw new Error("Invalid x or y length");
    18	  }
    19	
    20	  const prefix = (y[31]! & 1) === 0 ? 0x02 : 0x03;
    21	
    22	  const compressed = new Uint8Array(33);
    23	  compressed[0] = prefix;
    24	  compressed.set(x, 1);
    25	  return compressed;
    26	}
     1	export const DEFAULT_JWK_MEMBER_BYTE_LENGTH = 32;
     2	
     3	export * from "./base64";
     4	export * from "./encode";
     5	export * from "./hex";
     6	
     7	export { bs58 } from "./bs58";
     8	export { bs58check } from "./bs58check";

exec
/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '1,380p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	// EncodingTests.cs — xunit tests for src/Encoding.cs
     2	//
     3	// Vectors are derived from the upstream test file at:
     4	//   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
     5	//
     6	// Plus additional edge cases for leading-zero handling, base58check
     7	// roundtrip, and PointEncode round-trip.
     8	
     9	using System;
    10	using FluentAssertions;
    11	using Xunit;
    12	
    13	namespace Turnkey.Tests
    14	{
    15	    public class EncodingTests
    16	    {
    17	        // ========================================================
    18	        // hex
    19	        // ========================================================
    20	
    21	        [Fact]
    22	        public void Uint8ArrayToHexString_UpstreamVector()
    23	        {
    24	            // From index-test.ts test("uint8ArrayToHexString")
    25	            byte[] input =
    26	            {
    27	                82, 52, 208, 143, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23,
    28	                46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193,
    29	            };
    30	            const string expected =
    31	                "5234d08ffa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
    32	
    33	            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
    34	        }
    35	
    36	        [Fact]
    37	        public void Uint8ArrayToHexString_EmptyArray_ReturnsEmpty()
    38	        {
    39	            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
    40	        }
    41	
    42	        [Fact]
    43	        public void Uint8ArrayToHexString_Null_Throws()
    44	        {
    45	            Action act = () => Encoding.Uint8ArrayToHexString(null!);
    46	            act.Should().Throw<ArgumentNullException>();
    47	        }
    48	
    49	        [Fact]
    50	        public void Uint8ArrayToHexString_LeadingZeros_Preserved()
    51	        {
    52	            byte[] input = { 0x00, 0x00, 0x01, 0x02 };
    53	            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
    54	        }
    55	
    56	        [Fact]
    57	        public void Uint8ArrayFromHexString_UpstreamVector()
    58	        {
    59	            // From index-test.ts test("uint8ArrayFromHexString")
    60	            const string hex =
    61	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
    62	            byte[] expected =
    63	            {
    64	                82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23,
    65	                46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193,
    66	            };
    67	            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
    68	        }
    69	
    70	        [Fact]
    71	        public void Uint8ArrayFromHexString_ShortAscii()
    72	        {
    73	            // From index-test.ts: "627566666572" -> [98,117,102,102,101,114] (ASCII "buffer")
    74	            byte[] expected = { 98, 117, 102, 102, 101, 114 };
    75	            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
    76	        }
    77	
    78	        [Theory]
    79	        [InlineData("")]    // empty
    80	        [InlineData("123")] // odd length
    81	        [InlineData("oops")] // bad chars
    82	        public void Uint8ArrayFromHexString_Invalid_Throws(string hex)
    83	        {
    84	            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
    85	            act.Should().Throw<ArgumentException>()
    86	               .WithMessage("cannot create uint8array from invalid hex string*");
    87	        }
    88	
    89	        [Fact]
    90	        public void Uint8ArrayFromHexString_WithLength_Pads()
    91	        {
    92	            // "01" with length 2 -> [0, 1]
    93	            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
    94	        }
    95	
    96	        [Fact]
    97	        public void Uint8ArrayFromHexString_WithoutLength_NoPadding()
    98	        {
    99	            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
   100	        }
   101	
   102	        [Fact]
   103	        public void Uint8ArrayFromHexString_TooShortGetsPadded()
   104	        {
   105	            // From upstream "TOO SHORT" case
   106	            const string hex =
   107	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc";
   108	            byte[] expected =
   109	            {
   110	                0, 0, 82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138,
   111	                40, 23, 46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252,
   112	            };
   113	            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
   114	        }
   115	
   116	        [Fact]
   117	        public void Uint8ArrayFromHexString_TooLong_Throws()
   118	        {
   119	            // From upstream "TOO LONG" case: 34 bytes into length=32 buffer
   120	            const string hex =
   121	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fcfafbfcfd";
   122	            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
   123	            act.Should().Throw<ArgumentException>()
   124	               .WithMessage("hex value cannot fit in a buffer of 32 byte(s)");
   125	        }
   126	
   127	        [Fact]
   128	        public void Uint8ArrayFromHexString_OneByte_FitTwoByteRequest_Padding()
   129	        {
   130	            // Upstream: "0100" with length=1 throws "cannot fit in 1 byte(s)"
   131	            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
   132	            act.Should().Throw<ArgumentException>()
   133	               .WithMessage("hex value cannot fit in a buffer of 1 byte(s)");
   134	        }
   135	
   136	        [Fact]
   137	        public void HexToAscii_Roundtrip()
   138	        {
   139	            const string asciiHex = "627566666572";
   140	            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
   141	        }
   142	
   143	        [Fact]
   144	        public void NormalizePadding_AddsLeadingZeros()
   145	        {
   146	            byte[] input = { 0x01, 0x02 };
   147	            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
   148	        }
   149	
   150	        [Fact]
   151	        public void NormalizePadding_StripsLeadingZeros()
   152	        {
   153	            byte[] input = { 0x00, 0x00, 0x01, 0x02 };
   154	            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
   155	        }
   156	
   157	        [Fact]
   158	        public void NormalizePadding_StripWouldRemoveNonZero_Throws()
   159	        {
   160	            byte[] input = { 0x01, 0x02, 0x03, 0x04 };
   161	            Action act = () => Encoding.NormalizePadding(input, 2);
   162	            act.Should().Throw<ArgumentException>()
   163	               .WithMessage("invalid number of starting zeroes*");
   164	        }
   165	
   166	        [Fact]
   167	        public void NormalizePadding_EqualLength_Identity()
   168	        {
   169	            byte[] input = { 0x01, 0x02 };
   170	            Encoding.NormalizePadding(input, 2).Should().Equal(input);
   171	        }
   172	
   173	        // ========================================================
   174	        // base64url
   175	        // ========================================================
   176	
   177	        [Fact]
   178	        public void StringToBase64UrlString_UpstreamHelloVector()
   179	        {
   180	            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
   181	        }
   182	
   183	        [Fact]
   184	        public void StringToBase64UrlString_UpstreamPrivateKeyVector()
   185	        {
   186	            // From index-test.ts
   187	            const string input =
   188	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
   189	            const string expected =
   190	                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
   191	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
   192	        }
   193	
   194	        [Fact]
   195	        public void StringToBase64UrlString_UpstreamApiKeyStampVector()
   196	        {
   197	            const string input =
   198	                "{\"publicKey\":\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\","
   199	                + "\"signature\":\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\","
   200	                + "\"scheme\":\"SIGNATURE_SCHEME_TK_API_P256\"}";
   201	            const string expected =
   202	                "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ";
   203	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
   204	        }
   205	
   206	        [Fact]
   207	        public void StringToBase64UrlString_CodePointAbove255_Throws()
   208	        {
   209	            Action act = () => Encoding.StringToBase64UrlString("aĀb");
   210	            act.Should().Throw<ArgumentException>()
   211	               .WithMessage("InvalidCharacterError: found code point greater than 255:256 at position 1");
   212	        }
   213	
   214	        [Fact]
   215	        public void Base64StringToBase64UrlEncodedString_UpstreamVectors()
   216	        {
   217	            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
   218	                .Should().Be("aGVsbG8gd29ybGQ");
   219	            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
   220	                .Should().Be("U29tZSBzYW1wbGUgdGV4dA");
   221	        }
   222	
   223	        [Fact]
   224	        public void HexStringToBase64Url_UpstreamVectors()
   225	        {
   226	            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
   227	            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
   228	            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
   229	            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
   230	        }
   231	
   232	        [Fact]
   233	        public void HexStringToBase64Url_TooLong_Throws()
   234	        {
   235	            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
   236	            act.Should().Throw<ArgumentException>()
   237	               .WithMessage("hex value cannot fit in a buffer of 1 byte(s)");
   238	        }
   239	
   240	        [Fact]
   241	        public void Base64UrlToBase64_RestoresPadding()
   242	        {
   243	            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
   244	            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
   245	            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
   246	        }
   247	
   248	        [Fact]
   249	        public void Base64UrlToBase64_NoChangeWhenAlreadyPadded()
   250	        {
   251	            // "AAAA" length 4, padding 0
   252	            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
   253	        }
   254	
   255	        [Fact]
   256	        public void DecodeBase64UrlToString_Roundtrip()
   257	        {
   258	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
   259	            // From upstream private-key encoding vector
   260	            const string b64url =
   261	                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
   262	            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
   263	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1");
   264	        }
   265	
   266	        // ========================================================
   267	        // PointEncode
   268	        // ========================================================
   269	
   270	        [Fact]
   271	        public void PointEncode_EvenY_PrefixIs02()
   272	        {
   273	            var raw = new byte[65];
   274	            raw[0] = 0x04;
   275	            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
   276	            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
   277	            raw[64] = 0xBE; // last byte even
   278	
   279	            var compressed = Encoding.PointEncode(raw);
   280	
   281	            compressed.Should().HaveCount(33);
   282	            compressed[0].Should().Be(0x02);
   283	            for (int i = 1; i < 33; i++) compressed[i].Should().Be(0xAA);
   284	        }
   285	
   286	        [Fact]
   287	        public void PointEncode_OddY_PrefixIs03()
   288	        {
   289	            var raw = new byte[65];
   290	            raw[0] = 0x04;
   291	            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
   292	            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
   293	            raw[64] = 0xBD; // last byte odd
   294	
   295	            var compressed = Encoding.PointEncode(raw);
   296	
   297	            compressed[0].Should().Be(0x03);
   298	        }
   299	
   300	        [Fact]
   301	        public void PointEncode_InvalidPrefix_Throws()
   302	        {
   303	            var raw = new byte[65];
   304	            raw[0] = 0x05;
   305	            Action act = () => Encoding.PointEncode(raw);
   306	            act.Should().Throw<ArgumentException>()
   307	               .WithMessage("Invalid uncompressed P-256 key");
   308	        }
   309	
   310	        [Fact]
   311	        public void PointEncode_WrongLength_Throws()
   312	        {
   313	            var raw = new byte[64];
   314	            raw[0] = 0x04;
   315	            Action act = () => Encoding.PointEncode(raw);
   316	            act.Should().Throw<ArgumentException>()
   317	               .WithMessage("Invalid uncompressed P-256 key");
   318	        }
   319	
   320	        // ========================================================
   321	        // Base58 / Base58Check
   322	        // ========================================================
   323	
   324	        [Fact]
   325	        public void Base58Encode_KnownVector()
   326	        {
   327	            // From "Hello World" -> hex "48656c6c6f20576f726c64" -> base58 "JxF12TrwUP45BMd"
   328	            // (well-known Bitcoin reference vector)
   329	            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
   330	            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
   331	        }
   332	
   333	        [Fact]
   334	        public void Base58Decode_KnownVector()
   335	        {
   336	            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
   337	            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
   338	        }
   339	
   340	        [Fact]
   341	        public void Base58_Roundtrip_PreservesLeadingZero()
   342	        {
   343	            byte[] data = { 0x00, 0x01, 0x02, 0x03 };
   344	            string encoded = Encoding.Base58Encode(data);
   345	            encoded.Should().StartWith("1"); // leading-zero convention
   346	            byte[] decoded = Encoding.Base58Decode(encoded);
   347	            decoded.Should().Equal(data);
   348	        }
   349	
   350	        [Fact]
   351	        public void Base58_Empty()
   352	        {
   353	            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
   354	            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
   355	        }
   356	
   357	        [Fact]
   358	        public void Base58Check_Roundtrip()
   359	        {
   360	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
   361	            string encoded = Encoding.Base58CheckEncode(payload);
   362	            byte[] decoded = Encoding.Base58CheckDecode(encoded);
   363	            decoded.Should().Equal(payload);
   364	        }
   365	
   366	        [Fact]
   367	        public void Base58CheckDecode_TamperedChecksum_Throws()
   368	        {
   369	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
   370	            string encoded = Encoding.Base58CheckEncode(payload);
   371	            // Flip one character to corrupt the checksum
   372	            char[] chars = encoded.ToCharArray();
   373	            chars[chars.Length - 1] = chars[chars.Length - 1] == 'A' ? 'B' : 'A';
   374	            string tampered = new string(chars);
   375	
   376	            Action act = () => Encoding.Base58CheckDecode(tampered);
   377	            act.Should().Throw<ArgumentException>();
   378	        }
   379	
   380	        [Fact]

exec
/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '380,520p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   380	        [Fact]
   381	        public void Base58CheckDecode_TooShort_Throws()
   382	        {
   383	            // Empty payload encoded would still produce 4-byte checksum + something,
   384	            // but we just feed a 3-byte raw decoded result by abusing a known short string.
   385	            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
   386	            act.Should().Throw<ArgumentException>()
   387	               .WithMessage("Invalid Base58Check string - too short");
   388	        }
   389	
   390	        // ========================================================
   391	        // Helpers
   392	        // ========================================================
   393	
   394	        [Fact]
   395	        public void Uint8ArrayToString_DecodesUtf8()
   396	        {
   397	            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
   398	            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
   399	        }
   400	
   401	        [Fact]
   402	        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
   403	        {
   404	            byte[] a = { 1, 2 };
   405	            byte[] b = { 3 };
   406	            byte[] c = { 4, 5, 6 };
   407	            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
   408	        }
   409	    }
   410	}

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	import { test, expect } from "@jest/globals";
     2	import {
     3	  stringToBase64urlString,
     4	  uint8ArrayFromHexString,
     5	  uint8ArrayToHexString,
     6	  base64StringToBase64UrlEncodedString,
     7	  hexStringToBase64url,
     8	} from "..";
     9	
    10	// Test for stringToBase64urlString
    11	// These test vectors can be verified with NodeJS:
    12	//   $ node
    13	//   > Buffer.from("<input value>").toString("base64url")
    14	//   > <expected value>
    15	test("stringToBase64urlString", async function () {
    16	  // Trivial test string
    17	  expect(stringToBase64urlString("hello")).toBe("aGVsbG8"); // "hello" => "aGVsbG8"
    18	
    19	  // A private key
    20	  expect(
    21	    stringToBase64urlString(
    22	      "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1",
    23	    ),
    24	  ).toBe(
    25	    "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ", // Base64url encoded
    26	  );
    27	
    28	  // A sample API key stamp
    29	  expect(
    30	    stringToBase64urlString(
    31	      `{"publicKey":"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316","signature":"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81","scheme":"SIGNATURE_SCHEME_TK_API_P256"}`,
    32	    ),
    33	  ).toBe(
    34	    "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ", // Base64url encoded
    35	  );
    36	});
    37	
    38	// Test for base64StringToBase64UrlEncodedString
    39	test("base64StringToBase64UrlEncodedString", async function () {
    40	  // "hello world" => "aGVsbG8gd29ybGQ"
    41	  expect(base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")).toBe(
    42	    "aGVsbG8gd29ybGQ",
    43	  );
    44	
    45	  // "Some sample text" => "U29tZSBzYW1wbGUgdGV4dA"
    46	  expect(base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")).toBe(
    47	    "U29tZSBzYW1wbGUgdGV4dA",
    48	  );
    49	});
    50	
    51	// Test for uint8ArrayToHexString
    52	test("uint8ArrayToHexString", async function () {
    53	  const uint8Array = new Uint8Array([
    54	    82, 52, 208, 143, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23, 46,
    55	    133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193,
    56	  ]);
    57	  const expectedHexString =
    58	    "5234d08ffa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
    59	  expect(uint8ArrayToHexString(uint8Array)).toBe(expectedHexString); // Uint8Array => Hex string
    60	});
    61	
    62	// Test for uint8ArrayFromHexString
    63	// Convert hex string to uint8 array
    64	test("uint8ArrayFromHexString", async function () {
    65	  const hexString =
    66	    "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
    67	  const expectedUint8Array = new Uint8Array([
    68	    82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40, 23, 46,
    69	    133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252, 84, 193,
    70	  ]);
    71	  expect(uint8ArrayFromHexString(hexString)).toEqual(expectedUint8Array); // Hex string => Uint8Array
    72	
    73	  expect(uint8ArrayFromHexString("627566666572").toString()).toEqual(
    74	    "98,117,102,102,101,114",
    75	  );
    76	
    77	  // Error case: empty string
    78	  expect(() => {
    79	    uint8ArrayFromHexString("");
    80	  }).toThrow("cannot create uint8array from invalid hex string");
    81	  // Error case: odd number of characters
    82	  expect(() => {
    83	    uint8ArrayFromHexString("123");
    84	  }).toThrow("cannot create uint8array from invalid hex string");
    85	  // Error case: bad characters outside of hex range
    86	  expect(() => {
    87	    uint8ArrayFromHexString("oops");
    88	  }).toThrow("cannot create uint8array from invalid hex string");
    89	  // Happy path: if length parameter is included, pad the resulting buffer
    90	  expect(uint8ArrayFromHexString("01", 2).toString()).toEqual("0,1");
    91	  // Happy path: if length parameter is omitted, do not pad the resulting buffer
    92	  expect(uint8ArrayFromHexString("01").toString()).toEqual("1");
    93	  // Error case: hex value cannot fit in desired length
    94	  expect(() => {
    95	    uint8ArrayFromHexString("0100", 1).toString(); // the number 256 cannot fit into 1 byte
    96	  }).toThrow("hex value cannot fit in a buffer of 1 byte(s)");
    97	
    98	  // TOO SHORT - test a hex string with less bytes than the "length" parameter provided
    99	  const hexString2 =
   100	    "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc"; // length is 30 bytes, so must be padded with 2 0's at the beginning
   101	  const expectedUint8Array2 = new Uint8Array([
   102	    0, 0, 82, 52, 208, 141, 250, 44, 129, 95, 48, 151, 184, 186, 132, 138, 40,
   103	    23, 46, 133, 190, 199, 136, 134, 232, 226, 1, 175, 204, 177, 102, 252,
   104	  ]);
   105	  expect(uint8ArrayFromHexString(hexString2, 32)).toEqual(expectedUint8Array2); // Hex string => Uint8Array
   106	
   107	  // TOO LONG - test a hex string with less bytes than the "length" parameter provided -- Should error
   108	  const hexString3 =
   109	    "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fcfafbfcfd"; // length is 34 bytes, so no additional padding will be added
   110	  expect(() => uint8ArrayFromHexString(hexString3, 32)).toThrow(
   111	    "hex value cannot fit in a buffer of 32 byte(s)",
   112	  );
   113	});
   114	
   115	// Test for hexStringToBase64url
   116	test("hexStringToBase64url", async function () {
   117	  expect(hexStringToBase64url("01")).toEqual("AQ");
   118	  expect(hexStringToBase64url("01", 2)).toEqual("AAE");
   119	
   120	  // extrapolate to larger numbers
   121	  expect(hexStringToBase64url("ff")).toEqual("_w"); // max 1 byte
   122	  expect(hexStringToBase64url("ff", 2)).toEqual("AP8"); // max 1 byte expressed in 2 bytes
   123	
   124	  // error case
   125	  expect(() => {
   126	    hexStringToBase64url("0100", 1);
   127	  }).toThrow("hex value cannot fit in a buffer of 1 byte(s)");
   128	});

codex
I’ve confirmed the pinned upstream package files are local and the C# file is at the requested commit. One thing to note early: this repo currently has `tests/EncodingTests.cs`, but no `tests/Fixtures/` directory, so the fixture gate will need to distinguish inline upstream vectors from fixture files.
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/Encoding.cs-r1-20260523.md | sed -n '1,240p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	# Codex review — src/Encoding.cs — round 1 — 20260523
     2	
     3	C# SHA: `0ffef786d2d8ff119e598316cb5aa93b98562d65`
     4	Upstream snapshot: `turnkey-encoding-0.6.0`
     5	
     6	---
     7	
     8	2026-05-22T23:22:36.924429Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
     9	OpenAI Codex v0.129.0 (research preview)
    10	--------
    11	workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
    12	model: gpt-5.5
    13	provider: openai
    14	approval: never
    15	sandbox: read-only
    16	reasoning effort: high
    17	reasoning summaries: none
    18	session id: 019e51ff-7a43-73f1-801b-9533d30e4b9d
    19	--------
    20	user
    21	You are reviewing a C# port of the Turnkey TypeScript SDK.
    22	File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
    23	Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0
    24	Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
    25	
    26	This is REVIEW ROUND 1 of 3 for this file.
    27	
    28	REQUIRED OUTPUTS (sections A through G must all appear):
    29	
    30	A. Source pin acknowledgement:
    31	   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
    32	   C# file git SHA (0ffef786d2d8ff119e598316cb5aa93b98562d65).
    33	
    34	B. Method coverage table: every public + internal helper method in src/Encoding.cs
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
    70	2026-05-22T23:22:38.436323Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
    71	codex
    72	I’ll verify the C# file against the pinned local TypeScript snapshot and use the review workflow since this is explicitly a code review. First I’m gathering the file, upstream source, checksums, fixtures, and any relevant prior context.
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

exec
/bin/zsh -lc "find . -path '*/Fixtures/*' -o -path '*/fixtures/*' -o -name '*fixture*' -o -name '*Fixture*' | sort" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__
./codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__fixtures__

exec
/bin/zsh -lc 'find codex-crypto-reviews -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/Encoding.cs-r1-20260523.md
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

exec
/bin/zsh -lc "rg -n \"Base58|bs58|base64|Uint8ArrayToString|ConcatUint8Arrays|Encoding\\.\" src tests codex-crypto-reviews -g '"'!**/bin/**'"' -g '"'!**/obj/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/EncodingTests.cs:1:// EncodingTests.cs — xunit tests for src/Encoding.cs
tests/EncodingTests.cs:33:            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
tests/EncodingTests.cs:39:            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
tests/EncodingTests.cs:45:            Action act = () => Encoding.Uint8ArrayToHexString(null!);
tests/EncodingTests.cs:53:            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
tests/EncodingTests.cs:67:            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
tests/EncodingTests.cs:75:            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
tests/EncodingTests.cs:84:            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
tests/EncodingTests.cs:93:            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
tests/EncodingTests.cs:99:            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
tests/EncodingTests.cs:113:            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
tests/EncodingTests.cs:122:            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
tests/EncodingTests.cs:131:            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
tests/EncodingTests.cs:140:            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
tests/EncodingTests.cs:147:            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
tests/EncodingTests.cs:154:            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
tests/EncodingTests.cs:161:            Action act = () => Encoding.NormalizePadding(input, 2);
tests/EncodingTests.cs:170:            Encoding.NormalizePadding(input, 2).Should().Equal(input);
tests/EncodingTests.cs:174:        // base64url
tests/EncodingTests.cs:180:            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
tests/EncodingTests.cs:191:            Encoding.StringToBase64UrlString(input).Should().Be(expected);
tests/EncodingTests.cs:203:            Encoding.StringToBase64UrlString(input).Should().Be(expected);
tests/EncodingTests.cs:209:            Action act = () => Encoding.StringToBase64UrlString("aĀb");
tests/EncodingTests.cs:217:            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
tests/EncodingTests.cs:219:            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
tests/EncodingTests.cs:226:            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
tests/EncodingTests.cs:227:            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
tests/EncodingTests.cs:228:            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
tests/EncodingTests.cs:229:            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
tests/EncodingTests.cs:235:            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
tests/EncodingTests.cs:243:            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
tests/EncodingTests.cs:244:            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
tests/EncodingTests.cs:245:            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
tests/EncodingTests.cs:252:            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
tests/EncodingTests.cs:258:            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
tests/EncodingTests.cs:262:            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
tests/EncodingTests.cs:279:            var compressed = Encoding.PointEncode(raw);
tests/EncodingTests.cs:295:            var compressed = Encoding.PointEncode(raw);
tests/EncodingTests.cs:305:            Action act = () => Encoding.PointEncode(raw);
tests/EncodingTests.cs:315:            Action act = () => Encoding.PointEncode(raw);
tests/EncodingTests.cs:321:        // Base58 / Base58Check
tests/EncodingTests.cs:325:        public void Base58Encode_KnownVector()
tests/EncodingTests.cs:329:            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
tests/EncodingTests.cs:330:            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
tests/EncodingTests.cs:334:        public void Base58Decode_KnownVector()
tests/EncodingTests.cs:336:            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
tests/EncodingTests.cs:337:            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
tests/EncodingTests.cs:341:        public void Base58_Roundtrip_PreservesLeadingZero()
tests/EncodingTests.cs:344:            string encoded = Encoding.Base58Encode(data);
tests/EncodingTests.cs:346:            byte[] decoded = Encoding.Base58Decode(encoded);
tests/EncodingTests.cs:351:        public void Base58_Empty()
tests/EncodingTests.cs:353:            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
tests/EncodingTests.cs:354:            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
tests/EncodingTests.cs:358:        public void Base58Check_Roundtrip()
tests/EncodingTests.cs:360:            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
tests/EncodingTests.cs:361:            string encoded = Encoding.Base58CheckEncode(payload);
tests/EncodingTests.cs:362:            byte[] decoded = Encoding.Base58CheckDecode(encoded);
tests/EncodingTests.cs:367:        public void Base58CheckDecode_TamperedChecksum_Throws()
tests/EncodingTests.cs:369:            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
tests/EncodingTests.cs:370:            string encoded = Encoding.Base58CheckEncode(payload);
tests/EncodingTests.cs:376:            Action act = () => Encoding.Base58CheckDecode(tampered);
tests/EncodingTests.cs:381:        public void Base58CheckDecode_TooShort_Throws()
tests/EncodingTests.cs:385:            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
tests/EncodingTests.cs:387:               .WithMessage("Invalid Base58Check string - too short");
tests/EncodingTests.cs:395:        public void Uint8ArrayToString_DecodesUtf8()
tests/EncodingTests.cs:397:            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
tests/EncodingTests.cs:398:            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
tests/EncodingTests.cs:402:        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
tests/EncodingTests.cs:407:            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
src/Encoding.cs:9://   ts-source/base64.ts       -> StringToBase64UrlString /
src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
src/Encoding.cs:30://                            input as the bs58 npm package)
src/Encoding.cs:34://   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
src/Encoding.cs:35://                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
src/Encoding.cs:36://   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
src/Encoding.cs:72:            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
src/Encoding.cs:221:        // base64.ts
src/Encoding.cs:225:        /// Converts a plain string into a base64url-encoded string.
src/Encoding.cs:226:        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
src/Encoding.cs:259:        /// Converts a hex string into a base64url-encoded string.
src/Encoding.cs:260:        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
src/Encoding.cs:285:        /// Converts a standard base64 string into a base64url-encoded string by
src/Encoding.cs:287:        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
src/Encoding.cs:299:        /// Converts a base64url string into a standard base64 string (with
src/Encoding.cs:301:        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
src/Encoding.cs:315:        /// Decodes a base64url-encoded string into a plain string by first
src/Encoding.cs:316:        /// restoring base64 padding and then base64-decoding into a byte buffer.
src/Encoding.cs:318:        /// Upstream: <c>base64.ts decodeBase64urlToString</c>.
src/Encoding.cs:367:        // bs58.ts (and bs58check.ts)
src/Encoding.cs:369:        // Upstream imports the `bs58` and `bs58check` npm packages. This port
src/Encoding.cs:376:        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
src/Encoding.cs:377:        /// Upstream: <c>bs58.ts bs58.encode</c>.
src/Encoding.cs:379:        public static string Base58Encode(byte[] data)
src/Encoding.cs:401:            // Preserve leading zero bytes as '1' characters (Base58 convention).
src/Encoding.cs:411:        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
src/Encoding.cs:412:        /// Upstream: <c>bs58.ts bs58.decode</c>.
src/Encoding.cs:414:        public static byte[] Base58Decode(string encoded)
src/Encoding.cs:460:        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
src/Encoding.cs:461:        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
src/Encoding.cs:463:        public static string Base58CheckEncode(byte[] payload)
src/Encoding.cs:478:                return Base58Encode(concat);
src/Encoding.cs:483:        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
src/Encoding.cs:485:        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
src/Encoding.cs:487:        public static byte[] Base58CheckDecode(string encoded)
src/Encoding.cs:489:            byte[] decoded = Base58Decode(encoded);
src/Encoding.cs:492:                throw new ArgumentException("Invalid Base58Check string - too short");
src/Encoding.cs:507:                    throw new ArgumentException("Invalid Base58Check checksum");
src/Encoding.cs:531:        public static string Uint8ArrayToString(byte[] bytes)
src/Encoding.cs:537:            return System.Text.Encoding.UTF8.GetString(bytes);
src/Encoding.cs:548:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/unity-source-pins.md:38:| src/Encoding.cs             | packages/turnkey-sdk-unity/Runtime/Encoding.cs             |
codex-crypto-reviews/turnkey-source-pins.md:12:| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
codex-crypto-reviews/README.md:10:1. `Encoding.cs`
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1:# Codex review — src/Encoding.cs — round 1 — 20260523
codex-crypto-reviews/Encoding.cs-r1-20260523.md:22:File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:34:B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:298:/bin/zsh -lc 'rg -n "turnkey-sdk-csharp|turnkey|Encoding.cs|C# SDK" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:311:src/Encoding.cs:1:// 1:1 logical port of @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:312:src/Encoding.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
codex-crypto-reviews/Encoding.cs-r1-20260523.md:313:src/Encoding.cs:50:    /// 1:1 logical port of <c>@turnkey/encoding</c> v0.6.0.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:314:src/Encoding.cs:55:        /// Internal constants exported by <c>@turnkey/encoding</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:315:src/Encoding.cs:61:            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:316:src/Encoding.cs:515:        // Unity-port-derived helpers (NOT in @turnkey/encoding)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:325:/bin/zsh -lc "git status --short && git log --oneline -5 -- src/Encoding.cs && nl -ba src/Encoding.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:333:?? codex-crypto-reviews/Encoding.cs-r1-20260523.md
codex-crypto-reviews/Encoding.cs-r1-20260523.md:342:0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:351:     9	//   ts-source/base64.ts       -> StringToBase64UrlString /
codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:370:    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:372:    30	//                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:376:    34	//   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
codex-crypto-reviews/Encoding.cs-r1-20260523.md:377:    35	//                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:378:    36	//   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:414:    72	            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:563:   221	        // base64.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:567:   225	        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:568:   226	        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:601:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:602:   260	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:635:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:636:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:637:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:638:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:639:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:640:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:648:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:652:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:673:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:674:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:745:/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '235,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:771:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:772:   260	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:797:   285	        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:799:   287	        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:811:   299	        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:813:   301	        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:827:   315	        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:828:   316	        /// restoring base64 padding and then base64-decoding into a byte buffer.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:830:   318	        /// Upstream: <c>base64.ts decodeBase64urlToString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:879:   367	        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:881:   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:888:   376	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:889:   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:891:   379	        public static string Base58Encode(byte[] data)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:913:   401	            // Preserve leading zero bytes as '1' characters (Base58 convention).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:923:   411	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:924:   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:926:   414	        public static byte[] Base58Decode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:972:   460	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:973:   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:975:   463	        public static string Base58CheckEncode(byte[] payload)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:990:   478	                return Base58Encode(concat);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:995:   483	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:997:   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:999:   487	        public static byte[] Base58CheckDecode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1001:   489	            byte[] decoded = Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1004:   492	                throw new ArgumentException("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1019:   507	                    throw new ArgumentException("Invalid Base58Check checksum");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1043:   531	        public static string Uint8ArrayToString(byte[] bytes)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1049:   537	            return System.Text.Encoding.UTF8.GetString(bytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1060:   548	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1083:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1189:     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1195:     8	 * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1198:    11	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1201:    14	  // string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1203:    16	  const base64String = btoa(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1204:    17	  return base64StringToBase64UrlEncodedString(base64String);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1208:    21	 * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1212:    25	 * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1227:    40	 * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1229:    42	 * @param {string} input - The input base64 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1230:    43	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1232:    45	export function base64StringToBase64UrlEncodedString(input: string): string {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1237:    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1239:    52	 * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1242:    55	 * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1243:    56	 * @returns {string} - The equivalent base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1245:    58	export function base64UrlToBase64(input: string): string {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1252:    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1254:    67	 * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1255:    68	 * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1257:    70	 * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1259:    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1262:    75	  const b64 = base64UrlToBase64(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1389:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1391:     1	// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1393:     3	// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1395:     5	// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1397:     7	// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1403:    13	// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1406:    16	import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1426:    36	export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1427:     1	// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1429:     3	// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1431:     5	// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1433:     7	// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1439:    13	// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1442:    16	import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1462:    36	export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1491:     3	export * from "./base64";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1495:     7	export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1496:     8	export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1501:     1	// EncodingTests.cs — xunit tests for src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1533:    33	            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1539:    39	            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1545:    45	            Action act = () => Encoding.Uint8ArrayToHexString(null!);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1553:    53	            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1567:    67	            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1575:    75	            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1584:    84	            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1593:    93	            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1599:    99	            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1613:   113	            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1622:   122	            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1631:   131	            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1640:   140	            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1647:   147	            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1654:   154	            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1661:   161	            Action act = () => Encoding.NormalizePadding(input, 2);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1670:   170	            Encoding.NormalizePadding(input, 2).Should().Equal(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1674:   174	        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1680:   180	            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1691:   191	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1703:   203	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1709:   209	            Action act = () => Encoding.StringToBase64UrlString("aĀb");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1717:   217	            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1719:   219	            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1726:   226	            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1727:   227	            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1728:   228	            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1729:   229	            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1735:   235	            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1743:   243	            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1744:   244	            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1745:   245	            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1752:   252	            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1758:   258	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1762:   262	            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1779:   279	            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1795:   295	            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1805:   305	            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1815:   315	            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1821:   321	        // Base58 / Base58Check
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1825:   325	        public void Base58Encode_KnownVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1829:   329	            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1830:   330	            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1834:   334	        public void Base58Decode_KnownVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1836:   336	            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1837:   337	            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1841:   341	        public void Base58_Roundtrip_PreservesLeadingZero()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1844:   344	            string encoded = Encoding.Base58Encode(data);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1846:   346	            byte[] decoded = Encoding.Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1851:   351	        public void Base58_Empty()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1853:   353	            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1854:   354	            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1858:   358	        public void Base58Check_Roundtrip()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1860:   360	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1861:   361	            string encoded = Encoding.Base58CheckEncode(payload);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1862:   362	            byte[] decoded = Encoding.Base58CheckDecode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1867:   367	        public void Base58CheckDecode_TamperedChecksum_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1869:   369	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1870:   370	            string encoded = Encoding.Base58CheckEncode(payload);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1876:   376	            Action act = () => Encoding.Base58CheckDecode(tampered);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1886:   381	        public void Base58CheckDecode_TooShort_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1890:   385	            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1892:   387	               .WithMessage("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1900:   395	        public void Uint8ArrayToString_DecodesUtf8()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1902:   397	            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1903:   398	            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1907:   402	        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1912:   407	            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1925:     6	  base64StringToBase64UrlEncodedString,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1932:    13	//   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1957:    38	// Test for base64StringToBase64UrlEncodedString
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1958:    39	test("base64StringToBase64UrlEncodedString", async function () {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1960:    41	  expect(base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")).toBe(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1965:    46	  expect(base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")).toBe(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2052:/bin/zsh -lc "nl -ba codex-crypto-reviews/Encoding.cs-r1-20260523.md | sed -n '1,240p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2054:     1	# Codex review — src/Encoding.cs — round 1 — 20260523
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2075:    22	File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2087:    34	B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:38:    "bs58": "6.0.0",
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:39:    "bs58check": "4.0.0"
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:1:import * as raw from 'bs58';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:3:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:5:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:7:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:9:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:15:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:26:const bs58 = unwrap(raw);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:28:export { bs58 };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:29://# sourceMappingURL=bs58.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:2:export * from "./base64";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:5:export { bs58 } from "./bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:6:export { bs58check } from "./bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map:1:{"version":3,"file":"bs58check.mjs","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs.map:1:{"version":3,"file":"base64.mjs","sources":["../src/base64.ts"],"sourcesContent":[null],"names":[],"mappings":"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"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map:1:{"version":3,"file":"bs58.js","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:7:- [#886](https://github.com/tkhq/sdk/pull/886) [`6bfcbc5`](https://github.com/tkhq/sdk/commit/6bfcbc5c098e64ab1d115518733b87cfc1653e17) Author [@moeodeh3](https://github.com/moeodeh3) - Expose `bs58` and `bs58check` shims for cross-platform usage.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:6:export declare const bs58: Bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:8://# sourceMappingURL=bs58.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:6:export declare const bs58check: Bs58Check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:8://# sourceMappingURL=bs58check.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:159:- [#720](https://github.com/tkhq/sdk/pull/720) [`6cbff7a`](https://github.com/tkhq/sdk/commit/6cbff7a0c0b3a9a05586399e5cef476154d3bdca) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptExportBundle` not working in some environments by adding a shim to handle `bs58`'s ESM-only export.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:167:- [#716](https://github.com/tkhq/sdk/pull/716) [`fa46701`](https://github.com/tkhq/sdk/commit/fa467019eef34b5199372248edff1e7a64934e79) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptCredentialBundle` not working in React Native by adding a shim to handle `bs58check`'s ESM-only export.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:5: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:8: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:12: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:16: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:21: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:23: * @param {string} input - The input base64 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:24: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:26:export declare function base64StringToBase64UrlEncodedString(input: string): string;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:28: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:30: * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:33: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:34: * @returns {string} - The equivalent base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:36:export declare function base64UrlToBase64(input: string): string;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:38: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:40: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:41: * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:43: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:45: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:49://# sourceMappingURL=base64.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:3:var raw = require('bs58check');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:24:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:26:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:28:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:30:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:36:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:47:const bs58check = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:49:exports.bs58check = bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:50://# sourceMappingURL=bs58check.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:1:export { atob, base64StringToBase64UrlEncodedString, base64UrlToBase64, decodeBase64urlToString, hexStringToBase64url, stringToBase64urlString } from './base64.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:4:export { bs58 } from './bs58.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:5:export { bs58check } from './bs58check.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:4: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:7: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:10: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:13:    // string to base64
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:15:    const base64String = btoa(input);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:16:    return base64StringToBase64UrlEncodedString(base64String);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:19: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:23: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:34: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:36: * @param {string} input - The input base64 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:37: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:39:function base64StringToBase64UrlEncodedString(input) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:43: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:45: * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:48: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:49: * @returns {string} - The equivalent base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:51:function base64UrlToBase64(input) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:57: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:59: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:60: * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:62: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:64: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:67:    const b64 = base64UrlToBase64(input);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:170:export { atob, base64StringToBase64UrlEncodedString, base64UrlToBase64, decodeBase64urlToString, hexStringToBase64url, stringToBase64urlString };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:171://# sourceMappingURL=base64.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:8: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:11: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:14:  // string to base64
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:16:  const base64String = btoa(input);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:17:  return base64StringToBase64UrlEncodedString(base64String);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:40: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:42: * @param {string} input - The input base64 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:43: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:45:export function base64StringToBase64UrlEncodedString(input: string): string {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:50: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:52: * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:55: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:56: * @returns {string} - The equivalent base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:58:export function base64UrlToBase64(input: string): string {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:65: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:67: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:68: * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:70: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:72: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:75:  const b64 = base64UrlToBase64(input);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:3:var raw = require('bs58');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:24:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:26:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:28:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:30:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:36:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:47:const bs58 = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:49:exports.bs58 = bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:50://# sourceMappingURL=bs58.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:3:export * from "./base64";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:7:export { bs58 } from "./bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:8:export { bs58check } from "./bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts.map:1:{"version":3,"file":"base64.d.ts","sourceRoot":"","sources":["../src/base64.ts"],"names":[],"mappings":"AAAA;;GAEG;AAIH;;;;;GAKG;AACH,wBAAgB,uBAAuB,CAAC,KAAK,EAAE,MAAM,GAAG,MAAM,CAK7D;AAED;;;;;;;GAOG;AACH,wBAAgB,oBAAoB,CAAC,KAAK,EAAE,MAAM,EAAE,MAAM,CAAC,EAAE,MAAM,GAAG,MAAM,CAS3E;AAED;;;;;GAKG;AACH,wBAAgB,oCAAoC,CAAC,KAAK,EAAE,MAAM,GAAG,MAAM,CAE1E;AAED;;;;;;;;GAQG;AACH,wBAAgB,iBAAiB,CAAC,KAAK,EAAE,MAAM,GAAG,MAAM,CAIvD;AAED;;;;;;;;;GASG;AACH,wBAAgB,uBAAuB,CAAC,KAAK,EAAE,MAAM,GAAG,MAAM,CAG7D;AAuED,wBAAgB,IAAI,CAAC,KAAK,EAAE,MAAM,GAAG,MAAM,CAmD1C"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:3:var base64 = require('./base64.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:6:var bs58 = require('./bs58.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:7:var bs58check = require('./bs58check.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:11:exports.atob = base64.atob;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:12:exports.base64StringToBase64UrlEncodedString = base64.base64StringToBase64UrlEncodedString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:13:exports.base64UrlToBase64 = base64.base64UrlToBase64;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:14:exports.decodeBase64urlToString = base64.decodeBase64urlToString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:15:exports.hexStringToBase64url = base64.hexStringToBase64url;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:16:exports.stringToBase64urlString = base64.stringToBase64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:22:exports.bs58 = bs58.bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:23:exports.bs58check = bs58check.bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:1:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:3:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:5:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:7:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:13:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:16:import * as raw from "bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:36:export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:1:import * as raw from 'bs58check';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:3:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:5:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:7:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:9:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:15:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:26:const bs58check = unwrap(raw);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:28:export { bs58check };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:29://# sourceMappingURL=bs58check.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js.map:1:{"version":3,"file":"base64.js","sources":["../src/base64.ts"],"sourcesContent":[null],"names":["uint8ArrayFromHexString"],"mappings":"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"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:6:  base64StringToBase64UrlEncodedString,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:13://   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:38:// Test for base64StringToBase64UrlEncodedString
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:39:test("base64StringToBase64UrlEncodedString", async function () {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:41:  expect(base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")).toBe(
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:46:  expect(base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")).toBe(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:3:import { bs58check } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:4:import { bs58 } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:77:    const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:179:      return bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:287:      const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:461:  /* 3. base64-url decode signature -------------------------------------- */
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:542:  return bs58check.encode(
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map:1:{"version":3,"file":"bs58.mjs","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map:1:{"version":3,"file":"bs58check.js","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:1:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:3:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:5:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:7:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:13:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:16:import * as raw from "bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:36:export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map:1:{"version":3,"file":"bs58check.d.ts","sourceRoot":"","sources":["../src/bs58check.ts"],"names":[],"mappings":"AAiBA,KAAK,SAAS,GAAG;IACf,MAAM,CAAC,OAAO,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC/C,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;IAChC,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;CACnD,CAAC;AAcF,eAAO,MAAM,SAAS,WAA2B,CAAC"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:6: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:9: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:12: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:15:    // string to base64
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:17:    const base64String = btoa(input);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:18:    return base64StringToBase64UrlEncodedString(base64String);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:36: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:38: * @param {string} input - The input base64 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:39: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:41:function base64StringToBase64UrlEncodedString(input) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:45: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:47: * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:50: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:51: * @returns {string} - The equivalent base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:53:function base64UrlToBase64(input) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:59: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:61: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:62: * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:64: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:66: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:69:    const b64 = base64UrlToBase64(input);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:173:exports.base64StringToBase64UrlEncodedString = base64StringToBase64UrlEncodedString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:174:exports.base64UrlToBase64 = base64UrlToBase64;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:178://# sourceMappingURL=base64.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map:1:{"version":3,"file":"bs58.d.ts","sourceRoot":"","sources":["../src/bs58.ts"],"names":[],"mappings":"AAiBA,KAAK,IAAI,GAAG;IACV,MAAM,CAAC,MAAM,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC9C,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;IAClD,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;CACjC,CAAC;AAcF,eAAO,MAAM,IAAI,MAAsB,CAAC"}
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:30:        const bundleBytes = encoding.bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:98:            return encoding.bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:181:            const decodedKeyBytes = encoding.bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:295:    /* 3. base64-url decode signature -------------------------------------- */
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:348:    return encoding.bs58check.encode(crypto.hpkeEncrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:63:// Convert base64 url encoded string to an array -- used here to test that output pads correctly and doesn't get truncated
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:64:function base64urlToBuffer(baseurl64String: string): ArrayBuffer {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:67:  const base64String =
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:71:  const str = atob(base64String);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:1:import { bs58check, uint8ArrayToHexString, uint8ArrayFromHexString, bs58, hexToAscii } from '@turnkey/encoding';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:28:        const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:96:            return bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:179:            const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:293:    /* 3. base64-url decode signature -------------------------------------- */
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:346:    return bs58check.encode(hpkeEncrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:18:  // We expect the stamp to be base64url encoded
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:20:    Buffer.from(stamp.stampHeaderValue, "base64url").toString(),
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:8:    // Binary string to base64
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:9:    const base64String = btoa(str);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:10:    // Base64 to base64url
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:11:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:12:    const base64urlString = base64String
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:16:    return base64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:20://# sourceMappingURL=base64url.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/tink/bytes.js:49: * @return base64 output
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/bytes.ts:50: * @return base64 output
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:3:var base64url = require('./base64url.js');
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:8:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAttestation, credential);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:11:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAssertion, credential);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js.map:1:{"version":3,"file":"base64url.js","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/tink/bytes.d.ts:31: * @return base64 output
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/tink/bytes.mjs:47: * @return base64 output
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.mjs:1:import { bufferToBase64url } from './base64url.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts.map:1:{"version":3,"file":"base64url.d.ts","sourceRoot":"","sources":["../../src/webauthn-json/base64url.ts"],"names":[],"mappings":"AAAA,MAAM,MAAM,eAAe,GAAG,MAAM,CAAC;AAErC,wBAAgB,iBAAiB,CAC/B,eAAe,EAAE,eAAe,GAC/B,WAAW,CAgBb;AAED,wBAAgB,iBAAiB,CAAC,MAAM,EAAE,WAAW,GAAG,eAAe,CAkBtE"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:2:export declare function base64urlToBuffer(baseurl64String: Base64urlString): ArrayBuffer;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:4://# sourceMappingURL=base64url.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/json.d.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:10:    // Binary string to base64
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:11:    const base64String = btoa(str);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:12:    // Base64 to base64url
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:13:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:14:    const base64urlString = base64String
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:18:    return base64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:22://# sourceMappingURL=base64url.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs.map:1:{"version":3,"file":"base64url.mjs","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":"AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:1:import { base64urlToBuffer, bufferToBase64url } from "./base64url";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:19:  return convert(base64urlToBuffer, credentialCreationOptions, requestJSON);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:44:  return convert(base64urlToBuffer, credentialRequestOptions, requestJSON);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/json.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:3:export function base64urlToBuffer(
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:8:  const base64String =
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:12:  const str = atob(base64String);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:31:  // Binary string to base64
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:32:  const base64String = btoa(str);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:34:  // Base64 to base64url
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:35:  // We assume that the base64url string is well-formed.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:36:  const base64urlString = base64String
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:40:  return base64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:726:        /** @description The cbor encoded then base64 url encoded id of the credential. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:728:        /** @description A base64 url encoded payload containing metadata about the signing context and the challenge. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:730:        /** @description A base64 url encoded payload containing authenticator data and any attestation the webauthn provider chooses. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:785:        /** @description The borsch serialized base64 encoded Manifest. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:787:        /** @description The borsch serialized base64 encoded Manifest Envelope. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:4168:        /** @description A base64 url encoded Unique identifier for a given credential. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:4170:        /** @description A base64 encoded payload containing metadata about the signing context and the challenge. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:4172:        /** @description A base64 encoded payload containing metadata about the authenticator. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:4174:        /** @description The base64 url encoded signature bytes contained within the WebAuthn assertion response. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:884:    /** @description The cbor encoded then base64 url encoded id of the credential. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:886:    /** @description A base64 url encoded payload containing metadata about the signing context and the challenge. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:888:    /** @description A base64 url encoded payload containing authenticator data and any attestation the webauthn provider chooses. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:948:    /** @description The borsch serialized base64 encoded Manifest. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:950:    /** @description The borsch serialized base64 encoded Manifest Envelope. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:4439:    /** @description A base64 url encoded Unique identifier for a given credential. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:4441:    /** @description A base64 encoded payload containing metadata about the signing context and the challenge. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:4443:    /** @description A base64 encoded payload containing metadata about the authenticator. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:4445:    /** @description The base64 url encoded signature bytes contained within the WebAuthn assertion response. */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:4561:          "description": "The cbor encoded then base64 url encoded id of the credential."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:4565:          "description": "A base64 url encoded payload containing metadata about the signing context and the challenge."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:4569:          "description": "A base64 url encoded payload containing authenticator data and any attestation the webauthn provider chooses."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:4728:          "description": "The borsch serialized base64 encoded Manifest."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:4732:          "description": "The borsch serialized base64 encoded Manifest Envelope."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:13412:          "description": "A base64 url encoded Unique identifier for a given credential."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:13416:          "description": "A base64 encoded payload containing metadata about the signing context and the challenge."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:13420:          "description": "A base64 encoded payload containing metadata about the authenticator."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:13424:          "description": "The base64 url encoded signature bytes contained within the WebAuthn assertion response."

exec
/bin/zsh -lc "sed -n '1,80p' codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
{
  "name": "@turnkey/encoding",
  "version": "0.6.0",
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
  "description": "Encoding utility functions",
  "author": {
    "name": "Turnkey",
    "url": "https://turnkey.com/"
  },
  "repository": {
    "type": "git",
    "url": "git+https://github.com/tkhq/sdk.git",
    "directory": "packages/encoding"
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
    "bs58": "6.0.0",
    "bs58check": "4.0.0"
  },
  "scripts": {
    "build": "rollup -c",
    "clean": "rimraf ./dist ./.cache",
    "test": "jest",
    "typecheck": "tsc -p tsconfig.typecheck.json"
  }
}
exec
/bin/zsh -lc 'find codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 -maxdepth 4 -type d -name node_modules -print' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
exec
/bin/zsh -lc "rg -n \"public static|internal static|private static|static .*\\(\" src/Encoding.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
52:    public static class Encoding
57:        public static class Constants
78:        private static readonly Regex HexRegex = new Regex(
90:        public static string Uint8ArrayToHexString(byte[] input)
118:        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
155:        public static string HexToAscii(string hexString)
176:        public static byte[] NormalizePadding(byte[] byteArray, int targetLength)
235:        public static string StringToBase64UrlString(string input)
262:        public static string HexStringToBase64Url(string input, int? length = null)
289:        public static string Base64StringToBase64UrlEncodedString(string input)
303:        public static string Base64UrlToBase64(string input)
320:        public static string DecodeBase64UrlToString(string input)
345:        public static byte[] PointEncode(byte[] raw)
379:        public static string Base58Encode(byte[] data)
414:        public static byte[] Base58Decode(string encoded)
463:        public static string Base58CheckEncode(byte[] payload)
487:        public static byte[] Base58CheckDecode(string encoded)
531:        public static string Uint8ArrayToString(byte[] bytes)
548:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)

exec
/bin/zsh -lc "node -e \"const vals=[['hello',Buffer.from('hello','latin1').toString('base64url')],['private',Buffer.from('5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1','latin1').toString('base64url')],['hex 01',Buffer.from('01','hex').toString('base64url')],['hex 0001',Buffer.from('0001','hex').toString('base64url')],['hex ff',Buffer.from('ff','hex').toString('base64url')],['hex 00ff',Buffer.from('00ff','hex').toString('base64url')],['hello decode',Buffer.from('aGVsbG8','base64url').toString('latin1')]]; for (const [k,v] of vals) console.log(k+'='+v);\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
hello=aGVsbG8
private=NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ
hex 01=AQ
hex 0001=AAE
hex ff=_w
hex 00ff=AP8
hello decode=hello

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt && nl -ba codex-crypto-reviews/turnkey-source-pins.md | sed -n '1,40p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
     2	75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
     3	2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
     4	d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz
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
    17	| src/TurnkeyJsonContext.cs  | (no single upstream; covers DTOs from api-key-stamper + http) | n/a | n/a | n/a |
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

exec
/bin/zsh -lc 'shasum -a 256 codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0.tgz' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0.tgz

exec
/bin/zsh -lc "find . -maxdepth 4 -type f \\( -name '*fixture*' -o -name '*Fixture*' -o -name '*generate*' -o -name '*vector*' \\) | sort" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
exec
/bin/zsh -lc "rg -n \"Fixtures|fixtures|generated from|Node|Buffer\\.from|base64url|bs58check|bs58\" README.md codex-crypto-reviews tests src -g '"'!**/bin/**'"' -g '"'!**/obj/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
README.md:61:tests/      — xunit tests + golden fixtures from pinned Node packages
tests/turnkey-sdk-csharp.Tests.csproj:24:    <None Include="Fixtures\**\*">
src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
src/Encoding.cs:30://                            input as the bs58 npm package)
src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
src/Encoding.cs:225:        /// Converts a plain string into a base64url-encoded string.
src/Encoding.cs:259:        /// Converts a hex string into a base64url-encoded string.
src/Encoding.cs:285:        /// Converts a standard base64 string into a base64url-encoded string by
src/Encoding.cs:299:        /// Converts a base64url string into a standard base64 string (with
src/Encoding.cs:315:        /// Decodes a base64url-encoded string into a plain string by first
src/Encoding.cs:367:        // bs58.ts (and bs58check.ts)
src/Encoding.cs:369:        // Upstream imports the `bs58` and `bs58check` npm packages. This port
src/Encoding.cs:377:        /// Upstream: <c>bs58.ts bs58.encode</c>.
src/Encoding.cs:412:        /// Upstream: <c>bs58.ts bs58.decode</c>.
src/Encoding.cs:461:        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
src/Encoding.cs:485:        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/codex-crypto-review.sh:106:F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
codex-crypto-reviews/codex-crypto-review.sh:107:   this file, confirm it was generated from the pinned upstream package and the
codex-crypto-reviews/codex-crypto-review.sh:108:   C# test asserts the same bytes Node would produce.
codex-crypto-reviews/codex-crypto-review.sh:114:entries, F is "all fixtures match".
tests/EncodingTests.cs:174:        // base64url
codex-crypto-reviews/peak-lockfile-evidence.md:34:peak's Node deployment, so the C# port must match `0.5.0`.
codex-crypto-reviews/README.md:27:- section F states "all fixtures match".
codex-crypto-reviews/README.md:106:   (in tests/Fixtures/), confirm it was generated from the pinned upstream
codex-crypto-reviews/README.md:107:   package and the C# test asserts the same bytes Node produces.
codex-crypto-reviews/README.md:113:"all fixtures match".
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:38:    "bs58": "6.0.0",
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:39:    "bs58check": "4.0.0"
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:3:import { bs58check } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:4:import { bs58 } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:77:    const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:179:      return bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:287:      const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:359:  // Load target public key generated from enclave
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:414:  // Load target public key generated from enclave
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:542:  return bs58check.encode(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:159:- [#720](https://github.com/tkhq/sdk/pull/720) [`6cbff7a`](https://github.com/tkhq/sdk/commit/6cbff7a0c0b3a9a05586399e5cef476154d3bdca) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptExportBundle` not working in some environments by adding a shim to handle `bs58`'s ESM-only export.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:167:- [#716](https://github.com/tkhq/sdk/pull/716) [`fa46701`](https://github.com/tkhq/sdk/commit/fa467019eef34b5199372248edff1e7a64934e79) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptCredentialBundle` not working in React Native by adding a shim to handle `bs58check`'s ESM-only export.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:58:F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
codex-crypto-reviews/Encoding.cs-r1-20260523.md:59:   this file, confirm it was generated from the pinned upstream package and the
codex-crypto-reviews/Encoding.cs-r1-20260523.md:60:   C# test asserts the same bytes Node would produce.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:66:entries, F is "all fixtures match".
codex-crypto-reviews/Encoding.cs-r1-20260523.md:72:I’ll verify the C# file against the pinned local TypeScript snapshot and use the review workflow since this is explicitly a code review. First I’m gathering the file, upstream source, checksums, fixtures, and any relevant prior context.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:277:Node project - Difficult task:
codex-crypto-reviews/Encoding.cs-r1-20260523.md:281:Node project - Normal task:
codex-crypto-reviews/Encoding.cs-r1-20260523.md:304:/bin/zsh -lc "rg -n \"turnkey-encoding|sha256|encoding\" codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 tests/Fixtures tests src -g '"'!bin'"' -g '"'!obj'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:306:rg: tests/Fixtures: No such file or directory (os error 2)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:370:    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:372:    30	//                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:567:   225	        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:601:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:648:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:652:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:674:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:681:/bin/zsh -lc "find tests -maxdepth 4 -type f | sort && find . -maxdepth 4 -type d -name 'Fixtures' -print" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:771:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:797:   285	        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:811:   299	        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:827:   315	        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:879:   367	        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:881:   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:889:   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:924:   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:973:   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:997:   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1189:     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1195:     8	 * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1198:    11	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1208:    21	 * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1212:    25	 * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1227:    40	 * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1230:    43	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1237:    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1242:    55	 * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1252:    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1254:    67	 * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1257:    70	 * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1259:    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1389:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1391:     1	// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1393:     3	// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1395:     5	// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1397:     7	// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1403:    13	// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1406:    16	import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1426:    36	export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1427:     1	// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1429:     3	// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1431:     5	// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1433:     7	// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1439:    13	// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1442:    16	import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1462:    36	export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1495:     7	export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1496:     8	export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1674:   174	        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1930:    11	// These test vectors can be verified with NodeJS:
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1932:    13	//   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2050:I’ve confirmed the pinned upstream package files are local and the C# file is at the requested commit. One thing to note early: this repo currently has `tests/EncodingTests.cs`, but no `tests/Fixtures/` directory, so the fixture gate will need to distinguish inline upstream vectors from fixture files.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2111:    58	F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2112:    59	   this file, confirm it was generated from the pinned upstream package and the
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2113:    60	   C# test asserts the same bytes Node would produce.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2119:    66	entries, F is "all fixtures match".
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2125:    72	I’ll verify the C# file against the pinned local TypeScript snapshot and use the review workflow since this is explicitly a code review. First I’m gathering the file, upstream source, checksums, fixtures, and any relevant prior context.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2296:/bin/zsh -lc "find . -path '*/Fixtures/*' -o -path '*/fixtures/*' -o -name '*fixture*' -o -name '*Fixture*' | sort" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2298:./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2299:./codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__fixtures__
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2317:/bin/zsh -lc "rg -n \"Base58|bs58|base64|Uint8ArrayToString|ConcatUint8Arrays|Encoding\\.\" src tests codex-crypto-reviews -g '"'!**/bin/**'"' -g '"'!**/obj/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2337:tests/EncodingTests.cs:174:        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2389:src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2390:src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2391:src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2392:src/Encoding.cs:30://                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2396:src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2399:src/Encoding.cs:225:        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2401:src/Encoding.cs:259:        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2403:src/Encoding.cs:285:        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2405:src/Encoding.cs:299:        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2407:src/Encoding.cs:315:        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2410:src/Encoding.cs:367:        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2411:src/Encoding.cs:369:        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2413:src/Encoding.cs:377:        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2417:src/Encoding.cs:412:        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2420:src/Encoding.cs:461:        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2424:src/Encoding.cs:485:        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2449:codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2450:codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2451:codex-crypto-reviews/Encoding.cs-r1-20260523.md:370:    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2452:codex-crypto-reviews/Encoding.cs-r1-20260523.md:372:    30	//                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2456:codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2459:codex-crypto-reviews/Encoding.cs-r1-20260523.md:567:   225	        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2461:codex-crypto-reviews/Encoding.cs-r1-20260523.md:601:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2469:codex-crypto-reviews/Encoding.cs-r1-20260523.md:641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2470:codex-crypto-reviews/Encoding.cs-r1-20260523.md:642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2471:codex-crypto-reviews/Encoding.cs-r1-20260523.md:643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2472:codex-crypto-reviews/Encoding.cs-r1-20260523.md:644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2473:codex-crypto-reviews/Encoding.cs-r1-20260523.md:645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2474:codex-crypto-reviews/Encoding.cs-r1-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2475:codex-crypto-reviews/Encoding.cs-r1-20260523.md:647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2476:codex-crypto-reviews/Encoding.cs-r1-20260523.md:648:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2477:codex-crypto-reviews/Encoding.cs-r1-20260523.md:649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2478:codex-crypto-reviews/Encoding.cs-r1-20260523.md:650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2479:codex-crypto-reviews/Encoding.cs-r1-20260523.md:651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2480:codex-crypto-reviews/Encoding.cs-r1-20260523.md:652:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2482:codex-crypto-reviews/Encoding.cs-r1-20260523.md:674:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2483:codex-crypto-reviews/Encoding.cs-r1-20260523.md:675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2485:codex-crypto-reviews/Encoding.cs-r1-20260523.md:771:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2487:codex-crypto-reviews/Encoding.cs-r1-20260523.md:797:   285	        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2489:codex-crypto-reviews/Encoding.cs-r1-20260523.md:811:   299	        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2491:codex-crypto-reviews/Encoding.cs-r1-20260523.md:827:   315	        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2494:codex-crypto-reviews/Encoding.cs-r1-20260523.md:879:   367	        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2495:codex-crypto-reviews/Encoding.cs-r1-20260523.md:881:   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2497:codex-crypto-reviews/Encoding.cs-r1-20260523.md:889:   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2501:codex-crypto-reviews/Encoding.cs-r1-20260523.md:924:   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2504:codex-crypto-reviews/Encoding.cs-r1-20260523.md:973:   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2508:codex-crypto-reviews/Encoding.cs-r1-20260523.md:997:   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2517:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1189:     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2518:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1195:     8	 * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2519:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1198:    11	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2523:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1208:    21	 * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2524:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1212:    25	 * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2525:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1227:    40	 * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2527:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1230:    43	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2529:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1237:    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2531:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1242:    55	 * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2534:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1252:    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2535:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1254:    67	 * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2537:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1257:    70	 * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2538:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1259:    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2540:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1389:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2541:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1391:     1	// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2542:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1393:     3	// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2543:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1395:     5	// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2544:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1397:     7	// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2545:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1403:    13	// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2546:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1406:    16	import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2547:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1426:    36	export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2548:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1427:     1	// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2549:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1429:     3	// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2550:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1431:     5	// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2551:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1433:     7	// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2552:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1439:    13	// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2553:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1442:    16	import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2554:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1462:    36	export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2556:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1495:     7	export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2557:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1496:     8	export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2576:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1674:   174	        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2628:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1932:    13	//   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2637:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:38:    "bs58": "6.0.0",
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2638:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:39:    "bs58check": "4.0.0"
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2639:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:1:import * as raw from 'bs58';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2640:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:3:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:5:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:7:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:9:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:15:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:26:const bs58 = unwrap(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:28:export { bs58 };
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:29://# sourceMappingURL=bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:5:export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:6:export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map:1:{"version":3,"file":"bs58check.mjs","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2653:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map:1:{"version":3,"file":"bs58.js","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2654:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:7:- [#886](https://github.com/tkhq/sdk/pull/886) [`6bfcbc5`](https://github.com/tkhq/sdk/commit/6bfcbc5c098e64ab1d115518733b87cfc1653e17) Author [@moeodeh3](https://github.com/moeodeh3) - Expose `bs58` and `bs58check` shims for cross-platform usage.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2655:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:6:export declare const bs58: Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2656:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:8://# sourceMappingURL=bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2657:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:6:export declare const bs58check: Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2658:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:8://# sourceMappingURL=bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2659:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:159:- [#720](https://github.com/tkhq/sdk/pull/720) [`6cbff7a`](https://github.com/tkhq/sdk/commit/6cbff7a0c0b3a9a05586399e5cef476154d3bdca) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptExportBundle` not working in some environments by adding a shim to handle `bs58`'s ESM-only export.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2660:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:167:- [#716](https://github.com/tkhq/sdk/pull/716) [`fa46701`](https://github.com/tkhq/sdk/commit/fa467019eef34b5199372248edff1e7a64934e79) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptCredentialBundle` not working in React Native by adding a shim to handle `bs58check`'s ESM-only export.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2661:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2662:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:5: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2663:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:8: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2664:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:12: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2665:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:16: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2666:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:21: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2668:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:24: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2670:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:28: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2672:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:33: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:38: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2676:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:40: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2678:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:43: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2679:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:45: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2681:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:3:var raw = require('bs58check');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2682:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:24:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2683:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:26:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2684:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:28:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2685:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:30:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2686:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:36:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2687:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:47:const bs58check = unwrap(raw__namespace);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2688:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:49:exports.bs58check = bs58check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2689:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:50://# sourceMappingURL=bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2691:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:4:export { bs58 } from './bs58.mjs';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2692:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:5:export { bs58check } from './bs58check.mjs';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2693:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:4: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2694:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:7: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2695:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:10: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2699:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:19: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2700:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:23: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2701:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:34: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2703:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:37: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2705:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:43: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2707:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:48: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2710:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:57: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2711:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:59: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2713:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:62: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2714:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:64: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2718:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2719:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:8: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2720:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:11: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2724:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2725:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2726:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:40: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2728:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:43: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2730:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:50: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2732:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:55: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2735:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:65: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2736:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:67: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2738:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:70: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2739:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:72: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2741:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:3:var raw = require('bs58');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2742:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:24:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2743:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:26:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2744:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:28:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2745:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:30:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2746:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:36:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2747:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:47:const bs58 = unwrap(raw__namespace);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2748:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:49:exports.bs58 = bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2749:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:50://# sourceMappingURL=bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2751:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:7:export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2752:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:8:export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2755:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:6:var bs58 = require('./bs58.js');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2756:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:7:var bs58check = require('./bs58check.js');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2763:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:22:exports.bs58 = bs58.bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2764:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:23:exports.bs58check = bs58check.bs58check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2765:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:1:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2766:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:3:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2767:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:5:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2768:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:7:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2769:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:13:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2770:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:16:import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2771:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:36:export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2772:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:1:import * as raw from 'bs58check';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2773:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:3:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2774:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:5:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2775:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:7:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2776:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:9:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2777:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:15:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2778:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:26:const bs58check = unwrap(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2779:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:28:export { bs58check };
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2780:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:29://# sourceMappingURL=bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2783:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:13://   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2788:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:3:import { bs58check } from "@turnkey/encoding";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2789:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:4:import { bs58 } from "@turnkey/encoding";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2790:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:77:    const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2791:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:179:      return bs58.encode(concatenatedBytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2792:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:287:      const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2794:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:542:  return bs58check.encode(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2795:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map:1:{"version":3,"file":"bs58.mjs","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2796:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map:1:{"version":3,"file":"bs58check.js","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2797:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:1:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2798:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:3:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2799:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:5:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2800:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:7:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2801:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:13:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2802:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:16:import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2803:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:36:export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2804:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map:1:{"version":3,"file":"bs58check.d.ts","sourceRoot":"","sources":["../src/bs58check.ts"],"names":[],"mappings":"AAiBA,KAAK,SAAS,GAAG;IACf,MAAM,CAAC,OAAO,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC/C,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;IAChC,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;CACnD,CAAC;AAcF,eAAO,MAAM,SAAS,WAA2B,CAAC"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2805:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:6: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2806:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:9: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2807:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:12: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2811:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2812:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2813:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:36: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2815:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:39: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2817:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:45: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2819:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:50: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2822:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:59: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2823:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:61: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2825:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:64: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2826:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:66: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2831:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map:1:{"version":3,"file":"bs58.d.ts","sourceRoot":"","sources":["../src/bs58.ts"],"names":[],"mappings":"AAiBA,KAAK,IAAI,GAAG;IACV,MAAM,CAAC,MAAM,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC9C,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;IAClD,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;CACjC,CAAC;AAcF,eAAO,MAAM,IAAI,MAAsB,CAAC"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2832:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:30:        const bundleBytes = encoding.bs58check.decode(credentialBundle);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2833:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:98:            return encoding.bs58.encode(concatenatedBytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2834:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:181:            const decodedKeyBytes = encoding.bs58.decode(privateKey);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2836:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:348:    return encoding.bs58check.encode(crypto.hpkeEncrypt({
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2838:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2839:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2840:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2841:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2843:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:64:function base64urlToBuffer(baseurl64String: string): ArrayBuffer {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2846:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:1:import { bs58check, uint8ArrayToHexString, uint8ArrayFromHexString, bs58, hexToAscii } from '@turnkey/encoding';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2847:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:28:        const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2848:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:96:            return bs58.encode(concatenatedBytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2849:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:179:            const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2851:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:346:    return bs58check.encode(hpkeEncrypt({
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2852:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:18:  // We expect the stamp to be base64url encoded
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2853:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:20:    Buffer.from(stamp.stampHeaderValue, "base64url").toString(),
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2854:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:8:    // Binary string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2855:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:9:    const base64String = btoa(str);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2856:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:10:    // Base64 to base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2857:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:11:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2858:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:12:    const base64urlString = base64String
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2859:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:16:    return base64urlString;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2860:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:20://# sourceMappingURL=base64url.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2863:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:3:var base64url = require('./base64url.js');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2864:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:8:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAttestation, credential);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2865:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:11:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAssertion, credential);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2866:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js.map:1:{"version":3,"file":"base64url.js","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2869:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.mjs:1:import { bufferToBase64url } from './base64url.mjs';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2870:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts.map:1:{"version":3,"file":"base64url.d.ts","sourceRoot":"","sources":["../../src/webauthn-json/base64url.ts"],"names":[],"mappings":"AAAA,MAAM,MAAM,eAAe,GAAG,MAAM,CAAC;AAErC,wBAAgB,iBAAiB,CAC/B,eAAe,EAAE,eAAe,GAC/B,WAAW,CAgBb;AAED,wBAAgB,iBAAiB,CAAC,MAAM,EAAE,WAAW,GAAG,eAAe,CAkBtE"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2871:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:2:export declare function base64urlToBuffer(baseurl64String: Base64urlString): ArrayBuffer;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2872:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:4://# sourceMappingURL=base64url.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2873:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/json.d.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2874:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:10:    // Binary string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2875:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:11:    const base64String = btoa(str);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2876:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:12:    // Base64 to base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2877:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:13:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2878:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:14:    const base64urlString = base64String
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2879:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:18:    return base64urlString;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2880:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:22://# sourceMappingURL=base64url.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2881:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs.map:1:{"version":3,"file":"base64url.mjs","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":"AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2882:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:1:import { base64urlToBuffer, bufferToBase64url } from "./base64url";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2883:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:19:  return convert(base64urlToBuffer, credentialCreationOptions, requestJSON);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2884:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:44:  return convert(base64urlToBuffer, credentialRequestOptions, requestJSON);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2885:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/json.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2886:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:3:export function base64urlToBuffer(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2887:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:8:  const base64String =
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2888:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:12:  const str = atob(base64String);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2889:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:31:  // Binary string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2890:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:32:  const base64String = btoa(str);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2891:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:34:  // Base64 to base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2892:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:35:  // We assume that the base64url string is well-formed.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2893:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:36:  const base64urlString = base64String
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2894:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:40:  return base64urlString;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2963:    "bs58": "6.0.0",
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2964:    "bs58check": "4.0.0"
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3000:/bin/zsh -lc "node -e \"const vals=[['hello',Buffer.from('hello','latin1').toString('base64url')],['private',Buffer.from('5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1','latin1').toString('base64url')],['hex 01',Buffer.from('01','hex').toString('base64url')],['hex 0001',Buffer.from('0001','hex').toString('base64url')],['hex ff',Buffer.from('ff','hex').toString('base64url')],['hex 00ff',Buffer.from('00ff','hex').toString('base64url')],['hello decode',Buffer.from('aGVsbG8','base64url').toString('latin1')]]; for (const [k,v] of vals) console.log(k+'='+v);\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:1:import * as raw from 'bs58';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:3:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:5:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:7:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:9:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:15:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:26:const bs58 = unwrap(raw);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:28:export { bs58 };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:29://# sourceMappingURL=bs58.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:504:- Upgrade to Node v18 (#184)
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:804:- Dropped support for Node.js v14; we recommend using Node v18+
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.js:18:const isNode = typeof process !== "undefined" &&
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.js:25:    if (isNode) {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.js:28:    // If we don't have NodeJS or web crypto at our disposal, default to pure JS implementation
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.js:33: * Signature function abstracting the differences between NodeJS and web environments for signing with API keys.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn.js:28:    const hexString = Buffer.from(hashBuffer).toString("hex");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn.js:29:    const hexBuffer = Buffer.from(hexString, "utf8");
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:6:export declare const bs58: Bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:8://# sourceMappingURL=bs58.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:5:export { bs58 } from "./bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:6:export { bs58check } from "./bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/nodecrypto.js:28:        // @ts-expect-error -- the key can be a JWK object since Node v15.12.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/nodecrypto.js:37:    sign.write(Buffer.from(content));
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:5: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:8: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:12: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:16: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:21: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:24: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:28: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:33: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:38: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:40: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:43: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:45: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":"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"}
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/nodecrypto.mjs:7:        // @ts-expect-error -- the key can be a JWK object since Node v15.12.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/nodecrypto.mjs:16:    sign.write(Buffer.from(content));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:122:- Upgrade to Node v18 (#184)
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:140:- Hint for web bundlers not to polyfill Node crypto
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map:1:{"version":3,"file":"bs58check.mjs","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.d.ts:9: * Signature function abstracting the differences between NodeJS and web environments for signing with API keys.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:1:import { bs58check, uint8ArrayToHexString, uint8ArrayFromHexString, bs58, hexToAscii } from '@turnkey/encoding';
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:28:        const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:96:            return bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:179:            const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:222:    // Load target public key generated from enclave
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:253:    // Load target public key generated from enclave
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:346:    return bs58check.encode(hpkeEncrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:30:        const bundleBytes = encoding.bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:98:            return encoding.bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:181:            const decodedKeyBytes = encoding.bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:224:    // Load target public key generated from enclave
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:255:    // Load target public key generated from enclave
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:348:    return encoding.bs58check.encode(crypto.hpkeEncrypt({
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.mjs:16:const isNode = typeof process !== "undefined" &&
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.mjs:23:    if (isNode) {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.mjs:26:    // If we don't have NodeJS or web crypto at our disposal, default to pure JS implementation
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/dist/index.mjs:31: * Signature function abstracting the differences between NodeJS and web environments for signing with API keys.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:4:export { bs58 } from './bs58.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:5:export { bs58check } from './bs58check.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:2654:    /** @description A list of accounts generated from a wallet that share a common seed. */
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:3:var raw = require('bs58');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:24:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:26:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:28:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:30:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:36:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:47:const bs58 = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:49:exports.bs58 = bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:50://# sourceMappingURL=bs58.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/index.ts:26:const isNode: boolean =
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/index.ts:36:  if (isNode) {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/index.ts:40:  // If we don't have NodeJS or web crypto at our disposal, default to pure JS implementation
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/index.ts:46: * Signature function abstracting the differences between NodeJS and web environments for signing with API keys.
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/nodecrypto.ts:12:    // @ts-expect-error -- the key can be a JWK object since Node v15.12.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/nodecrypto.ts:22:  sign.write(Buffer.from(content));
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:1:import * as raw from 'bs58check';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:3:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:5:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:7:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:9:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:15:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:26:const bs58check = unwrap(raw);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:28:export { bs58check };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:29://# sourceMappingURL=bs58check.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__tests__/request-test.ts:4:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map:1:{"version":3,"file":"bs58.mjs","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__tests__/async-test.ts:9:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/base.mjs:50:        // Can't use native `cause` here because it's not well supported on Node v16
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:6: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:9: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:12: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:36: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:39: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:45: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:50: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:59: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:61: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:64: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:66: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:3:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:18:  // We expect the stamp to be base64url encoded
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:20:    Buffer.from(stamp.stampHeaderValue, "base64url").toString(),
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map:1:{"version":3,"file":"bs58.js","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:3:export function base64urlToBuffer(
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:34:  // Base64 to base64url
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:35:  // We assume that the base64url string is well-formed.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:36:  const base64urlString = base64String
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:40:  return base64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:6:export declare const bs58check: Bs58Check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:8://# sourceMappingURL=bs58check.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:5:import { signWithApiKey as signNode } from "../nodecrypto";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:9:import { readFixture } from "../__fixtures__/shared";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:18:  { impl: signNode, name: "sign (node crypto)" },
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/signature-test.ts:53:  { impl: signNode, name: "stamp (node)" },
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:1:import { base64urlToBuffer, bufferToBase64url } from "./base64url";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:19:  return convert(base64urlToBuffer, credentialCreationOptions, requestJSON);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:44:  return convert(base64urlToBuffer, credentialRequestOptions, requestJSON);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:4: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:7: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:10: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:19: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:23: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:34: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:37: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:43: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:48: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:57: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:59: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:62: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:64: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:64:function base64urlToBuffer(baseurl64String: string): ArrayBuffer {
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn.ts:68:  const hexString = Buffer.from(hashBuffer).toString("hex");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn.ts:69:  const hexBuffer = Buffer.from(hexString, "utf8");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:10:    // Base64 to base64url
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:11:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:12:    const base64urlString = base64String
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:16:    return base64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:20://# sourceMappingURL=base64url.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/json.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:6:var bs58 = require('./bs58.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:7:var bs58check = require('./bs58check.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:22:exports.bs58 = bs58.bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:23:exports.bs58check = bs58check.bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs.map:1:{"version":3,"file":"base64url.mjs","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":"AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:3:var raw = require('bs58check');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:24:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:26:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:28:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:30:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:36:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:47:const bs58check = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:49:exports.bs58check = bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:50://# sourceMappingURL=bs58check.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/json.d.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:3:var base64url = require('./base64url.js');
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:8:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAttestation, credential);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:11:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAssertion, credential);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map:1:{"version":3,"file":"bs58check.js","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:7:- [#886](https://github.com/tkhq/sdk/pull/886) [`6bfcbc5`](https://github.com/tkhq/sdk/commit/6bfcbc5c098e64ab1d115518733b87cfc1653e17) Author [@moeodeh3](https://github.com/moeodeh3) - Expose `bs58` and `bs58check` shims for cross-platform usage.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map:1:{"version":3,"file":"bs58check.d.ts","sourceRoot":"","sources":["../src/bs58check.ts"],"names":[],"mappings":"AAiBA,KAAK,SAAS,GAAG;IACf,MAAM,CAAC,OAAO,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC/C,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;IAChC,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;CACnD,CAAC;AAcF,eAAO,MAAM,SAAS,WAA2B,CAAC"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js.map:1:{"version":3,"file":"base64url.js","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__fixtures__/shared.ts:4:const FIXTURES_DIR = path.resolve(__dirname, "..", "__fixtures__");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.mjs:1:import { bufferToBase64url } from './base64url.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn.mjs:26:    const hexString = Buffer.from(hashBuffer).toString("hex");
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn.mjs:27:    const hexBuffer = Buffer.from(hexString, "utf8");
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map:1:{"version":3,"file":"bs58.d.ts","sourceRoot":"","sources":["../src/bs58.ts"],"names":[],"mappings":"AAiBA,KAAK,IAAI,GAAG;IACV,MAAM,CAAC,MAAM,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC9C,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;IAClD,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;CACjC,CAAC;AAcF,eAAO,MAAM,IAAI,MAAsB,CAAC"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts.map:1:{"version":3,"file":"base64url.d.ts","sourceRoot":"","sources":["../../src/webauthn-json/base64url.ts"],"names":[],"mappings":"AAAA,MAAM,MAAM,eAAe,GAAG,MAAM,CAAC;AAErC,wBAAgB,iBAAiB,CAC/B,eAAe,EAAE,eAAe,GAC/B,WAAW,CAgBb;AAED,wBAAgB,iBAAiB,CAAC,MAAM,EAAE,WAAW,GAAG,eAAe,CAkBtE"}
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:2:export declare function base64urlToBuffer(baseurl64String: Base64urlString): ArrayBuffer;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:4://# sourceMappingURL=base64url.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:12:    // Base64 to base64url
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:13:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:14:    const base64urlString = base64String
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:18:    return base64urlString;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:22://# sourceMappingURL=base64url.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:8: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:11: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:40: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:43: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:50: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:55: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:65: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:67: * - Converts the input from base64url to base64.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:70: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:72: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/base.ts:118:    // Can't use native `cause` here because it's not well supported on Node v16
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:8898:          "description": "A list of accounts generated from a wallet that share a common seed."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/base.js:52:        // Can't use native `cause` here because it's not well supported on Node v16
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:7:export { bs58 } from "./bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:8:export { bs58check } from "./bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:1:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:3:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:5:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:7:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:13:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:16:import * as raw from "bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:36:export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:1:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:3:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:5:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:7:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:13:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:16:import * as raw from "bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:36:export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/__generated__/services/coordinator/public/v1/public_api.types.d.ts:2418:        /** @description A list of accounts generated from a wallet that share a common seed. */
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:11:// These test vectors can be verified with NodeJS:
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:13://   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__/shared.ts:4:const FIXTURES_DIR = path.resolve(__dirname, "..", "__fixtures__");

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/README.md | sed -n '96,116p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba README.md | sed -n '52,66p' && nl -ba tests/turnkey-sdk-csharp.Tests.csproj | sed -n '18,28p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
    96	     - leading-zero handling
    97	     - padding
    98	     - rounding / normalization
    99	     - signature format (DER vs raw r||s, low-S)
   100	     - DTO shape (field names, order, presence, optionality)
   101	     - JSON serialization (property order, casing, null handling, escaping)
   102	   For each: C# file:line, TS upstream-snapshots/...:line, semantic difference,
   103	   suggested fix.
   104	
   105	F. Fixture comparison gate: for every test fixture that touches this file
   106	   (in tests/Fixtures/), confirm it was generated from the pinned upstream
   107	   package and the C# test asserts the same bytes Node produces.
   108	
   109	G. Unresolved assumptions you could not verify.
   110	
   111	PASS criterion: B has zero NOT-REVIEWED rows (or documented justifications)
   112	AND E has zero entries AND D is "no banned API present" AND F is
   113	"all fixtures match".
   114	
   115	DO NOT use phrases like "looks good" or "no divergence found" without
   116	producing sections A–F.

 succeeded in 0ms:
    52	  Turnkey TypeScript logic.
    53	- `System.Text.Json 8.0.5` — source-generated context only
    54	  (`TurnkeyJsonContext`). No reflection-based serialization,
    55	  no `System.Reflection.Emit`. Compatible with IL2CPP AOT trim.
    56	
    57	## Project structure
    58	
    59	```
    60	src/        — production code
    61	tests/      — xunit tests + golden fixtures from pinned Node packages
    62	docs/security/threat-model.md — scope: key handling, signing, crypto
    63	                                primitives, serialization, test-secret handling
    64	codex-crypto-reviews/          — pinned upstream snapshots, source-pin docs,
    65	                                 multi-round Codex review evidence
    66	```
    18	    <PackageReference Include="FluentAssertions" />
    19	    <PackageReference Include="coverlet.collector" />
    20	    <PackageReference Include="PublicApiGenerator" />
    21	  </ItemGroup>
    22	
    23	  <ItemGroup>
    24	    <None Include="Fixtures\**\*">
    25	      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    26	    </None>
    27	  </ItemGroup>
    28	</Project>

exec
/bin/zsh -lc "node -e \"function hexToAscii(s){let o=''; for(let i=0;i<s.length;i+=2)o+=String.fromCharCode(parseInt(s.substr(i,2),16)); return Array.from(o).map(c=>c.charCodeAt(0)).join(',');} function b64urlToBase64(input){let b64=input.replace(/-/g,'+').replace(/_/g,'/'); const padLen=(4-(b64.length%4))%4; return b64+'='.repeat(padLen);} function atob2(input){const str=String(input).replace(/["'^A-Za-z0-9+/=]/g,'"''); if(str.length%4===1) throw new Error('InvalidCharacterError'); const keyStr='ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='; let output='',buffer=0,bits=0,i=0; while(i<str.length){const ch=str.charAt(i); const index=keyStr.indexOf(ch); if(index<0||index>64){i++; continue;} if(ch==='='){bits=0;} else {buffer=(buffer<<6)|index; bits+=6;} while(bits>=8){bits-=8; output+=String.fromCharCode((buffer>>bits)&0xff);} i++;} return output;} console.log('hex f ->', hexToAscii('f')); console.log('hex zz ->', hexToAscii('zz')); console.log('decode a*GVsbG8 ->', atob2(b64urlToBase64('a*GVsbG8')));\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
hex f -> 15
hex zz -> 0
decode a*GVsbG8 -> hello

exec
/bin/zsh -lc 'node -e "function u(hexString,length){const hexRegex=/''^[0-9A-Fa-f]+$/; if(!hexString || hexString.length%2!=0 || !hexRegex.test(hexString)) throw new Error('"'invalid'); const buffer=new Uint8Array(hexString.match(/../g).map(h=>parseInt(h,16))); if("'!length) return buffer; if(hexString.length/2>length) throw new Error('"'too long'); const p=new Uint8Array(length); p.set(buffer,length-buffer.length); return p;} console.log(Array.from(u('01',0)).join(','));\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
1

exec
/bin/zsh -lc "nl -ba /Users/takeshi/.codex/memories/MEMORY.md | sed -n '273,280p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   273	# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
   274	
   275	scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.
   276	applies_to: cwd=/Users/takeshi/.codex/worktrees/1605/peak; reuse_rule=reuse for future `peak` plan/PR review work when the target is PR #330 or a similar docs-plan review in the Peak repo; treat plan conclusions as PR-specific if the file or branch has materially changed
   277	
   278	## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted
   279	
   280	### rollout_summary_files

exec
/bin/zsh -lc "nl -ba /Users/takeshi/.codex/memories/MEMORY.md | sed -n '280,285p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   280	### rollout_summary_files
   281	
   282	- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
   283	- extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T04-47-00-pPnT-10min-memory-summary.md, updated_at=2026-05-22T04:47:00+00:00, thread_id=None, chronicle synthetic task for the initial PR-review window)
   284	- extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-07-00-LJjs-10min-memory-summary.md, updated_at=2026-05-22T06:07:00+00:00, thread_id=None, chronicle synthetic task showing the posted inline review state)
   285	

codex
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/encoding`
- Version: `0.6.0`
- Tarball sha256: `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`
- C# file git SHA: `0ffef786d2d8ff119e598316cb5aa93b98562d65`

**B. Method Coverage Table**

No `internal` helper methods are defined in `src/Encoding.cs`; `BASE64_KEYSTR` is an internal constant, not a method.

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| `Uint8ArrayToHexString` (`src/Encoding.cs:90`) | `uint8ArrayToHexString` (`ts-source/hex.ts:7`) | REVIEWED | Success path matches lower-case two-digit hex. |
| `Uint8ArrayFromHexString` (`src/Encoding.cs:118`) | `uint8ArrayFromHexString` (`ts-source/hex.ts:23`) | REVIEWED | Main validation/padding reviewed; `length=0` divergence below. |
| `HexToAscii` (`src/Encoding.cs:155`) | `hexToAscii` (`ts-source/hex.ts:58`) | REVIEWED | Valid even hex matches; malformed/odd behavior diverges below. |
| `NormalizePadding` (`src/Encoding.cs:176`) | `normalizePadding` (`ts-source/hex.ts:73`) | REVIEWED | Leading-zero add/strip logic matches. |
| `StringToBase64UrlString` (`src/Encoding.cs:235`) | `stringToBase64urlString` (`ts-source/base64.ts:13`) | REVIEWED | 0..255 code unit behavior matches custom `btoa`. |
| `HexStringToBase64Url` (`src/Encoding.cs:262`) | `hexStringToBase64url` (`ts-source/base64.ts:28`) | REVIEWED | Main path matches; inherits `length=0` divergence. |
| `Base64StringToBase64UrlEncodedString` (`src/Encoding.cs:289`) | `base64StringToBase64UrlEncodedString` (`ts-source/base64.ts:45`) | REVIEWED | Character replacements and padding removal match. |
| `Base64UrlToBase64` (`src/Encoding.cs:303`) | `base64UrlToBase64` (`ts-source/base64.ts:58`) | REVIEWED | URL-safe replacement and padding restoration match. |
| `DecodeBase64UrlToString` (`src/Encoding.cs:320`) | `decodeBase64urlToString` / `atob` (`ts-source/base64.ts:74`, `:148`) | REVIEWED | Valid inputs match; invalid-character handling diverges below. |
| `PointEncode` (`src/Encoding.cs:345`) | `pointEncode` (`ts-source/encode.ts:8`) | REVIEWED | Prefix and x-coordinate copy match; omitted TS redundant length check is unreachable. |
| `Base58Encode` (`src/Encoding.cs:379`) | `bs58.encode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Standard Bitcoin alphabet and leading-zero handling reviewed. |
| `Base58Decode` (`src/Encoding.cs:414`) | `bs58.decode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Byte math reviewed; invalid error text may differ. |
| `Base58CheckEncode` (`src/Encoding.cs:463`) | `bs58check.encode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Payload plus double-SHA256 first-4-byte checksum matches. |
| `Base58CheckDecode` (`src/Encoding.cs:487`) | `bs58check.decode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Valid checksum logic matches; invalid error text may differ. |
| `Uint8ArrayToString` (`src/Encoding.cs:531`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS `TextDecoder` pattern elsewhere. |
| `ConcatUint8Arrays` (`src/Encoding.cs:548`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS spread/concat pattern elsewhere. |

**C. Intentional Adaptations**

- `Uint8Array` -> `byte[]`: structural runtime type mapping; byte order is unchanged.
- TS `reduce` string building -> `StringBuilder`: same emitted characters/bytes.
- TS `Uint8Array#set`, `slice`, spread -> `Array.Copy`/new arrays: same array contents and order.
- Pure JS `btoa` over 0..255 code units -> code-point guard plus `Convert.ToBase64String`: same base64 bytes for valid upstream inputs.
- Regex and `parseInt` for validated hex -> `Regex` plus `Convert.ToByte`: same bytes for `uint8ArrayFromHexString` valid inputs.
- `Error` -> `ArgumentException` / `ArgumentNullException`: language-native exception mapping; explicit upstream validation failures mostly keep the same message text.
- `bs58` / `bs58check` npm calls -> local BigInteger base58 plus double-SHA256 checksum: same valid wire encoding/decoding.
- TS exported namespace objects `bs58`, `bs58check` -> direct C# methods: structural API flattening; same valid byte/string conversion.
- `TextDecoder` and spread idioms used by other TS packages -> `Uint8ArrayToString` and `ConcatUint8Arrays`: C# helper extraction, not an encoding wire-format change.

**D. D17 Enforcement Check**

N/A (not Crypto.cs).

**E. Logic Divergence Findings**

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Encoding.cs:134`, `:139` | `ts-source/hex.ts:38` | `length=0` differs. TS uses `if (!length)` so `0` is treated as omitted and returns the parsed buffer; C# treats `0` as a requested target length and throws for non-empty input. | Change to `if (!length.HasValue || length.Value == 0) return buffer;` if runtime parity with TS falsy `0` is required. |
| `src/Encoding.cs:276` | `ts-source/base64.ts:31-32`, `ts-source/hex.ts:38` | `HexStringToBase64Url(input, 0)` inherits the same `length=0` divergence from `Uint8ArrayFromHexString`. | Fix the helper above; no separate logic change needed here. |
| `src/Encoding.cs:162-166` | `ts-source/hex.ts:58-63` | `HexToAscii` differs for odd or malformed input. TS loops while `i < length`, parses a trailing single nibble, and JS `parseInt` can yield `NaN` which `String.fromCharCode` turns into NUL. C# loops only while `i + 1 < length`, dropping a trailing nibble, and throws on malformed pairs. | If exact JS runtime behavior is required, loop while `i < hexString.Length` and implement JS-like `parseInt(..., 16)` semantics for partial/invalid pairs. |
| `src/Encoding.cs:322-323` | `ts-source/base64.ts:148-160`, `:171-198` | `decodeBase64urlToString` differs for invalid characters. Upstream `atob` strips non-base64 characters before decoding; `Convert.FromBase64String` rejects many of them. Example: upstream decodes `a*GVsbG8` to `hello`; C# throws. | Port upstream `atob` sanitizer/decoder or sanitize with the same regex before `Convert.FromBase64String`, including the `length % 4 === 1` error rule. |
| `src/Encoding.cs:431-435` | `ts-source/bs58.ts:36` | Invalid base58 inputs throw C# `ArgumentException` with custom text; upstream delegates to `bs58@6.0.0` and likely exposes dependency-specific error text/type. Valid bytes match. | Either match upstream dependency error messages or explicitly document that invalid-input exception text is not a parity surface. |
| `src/Encoding.cs:490-507` | `ts-source/bs58check.ts:36` | Invalid/too-short/checksum-failing Base58Check inputs throw C# custom messages; upstream delegates to `bs58check@4.0.0` dependency behavior. Valid payload/checksum bytes match. | Either match upstream dependency error behavior or document exception text as intentionally .NET-native. |

**F. Fixture Comparison Gate**

`tests/Fixtures/` is absent in this checkout, despite `README.md:61` describing golden fixtures and `tests/turnkey-sdk-csharp.Tests.csproj:24` including `Fixtures\**\*`. Therefore there are no fixture files under `tests/Fixtures/` that exercise `Encoding.cs`, and I cannot confirm fixture provenance from the pinned package. F is not satisfied as “all fixtures match” in this round.

I did verify that `tests/EncodingTests.cs` uses inline vectors copied from `ts-source/__tests__/index-test.ts` for the main hex/base64 cases, and I ran Node `Buffer` checks for the base64url expected values. The Base58/Base58Check tests are local known-vector/roundtrip tests, not generated fixture files from the pinned upstream package.

**G. Unresolved Assumptions**

- I treated typed non-null TS inputs as the main parity surface; C# null guards differ from untyped JS runtime coercion/error behavior.
- Exact `bs58@6.0.0` and `bs58check@4.0.0` dependency error messages were not executable locally because the pinned package has no `node_modules`.
- No `tests/Fixtures/` directory exists, so fixture generation/provenance cannot be verified for this file.
- I did not run `dotnet test`; the sandbox is read-only, and the review above is based on source inspection plus read-only Node checks.


tokens used
111,144
**A. Source Pin Acknowledgement**

- Upstream package: `@turnkey/encoding`
- Version: `0.6.0`
- Tarball sha256: `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`
- C# file git SHA: `0ffef786d2d8ff119e598316cb5aa93b98562d65`

**B. Method Coverage Table**

No `internal` helper methods are defined in `src/Encoding.cs`; `BASE64_KEYSTR` is an internal constant, not a method.

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---|---|
| `Uint8ArrayToHexString` (`src/Encoding.cs:90`) | `uint8ArrayToHexString` (`ts-source/hex.ts:7`) | REVIEWED | Success path matches lower-case two-digit hex. |
| `Uint8ArrayFromHexString` (`src/Encoding.cs:118`) | `uint8ArrayFromHexString` (`ts-source/hex.ts:23`) | REVIEWED | Main validation/padding reviewed; `length=0` divergence below. |
| `HexToAscii` (`src/Encoding.cs:155`) | `hexToAscii` (`ts-source/hex.ts:58`) | REVIEWED | Valid even hex matches; malformed/odd behavior diverges below. |
| `NormalizePadding` (`src/Encoding.cs:176`) | `normalizePadding` (`ts-source/hex.ts:73`) | REVIEWED | Leading-zero add/strip logic matches. |
| `StringToBase64UrlString` (`src/Encoding.cs:235`) | `stringToBase64urlString` (`ts-source/base64.ts:13`) | REVIEWED | 0..255 code unit behavior matches custom `btoa`. |
| `HexStringToBase64Url` (`src/Encoding.cs:262`) | `hexStringToBase64url` (`ts-source/base64.ts:28`) | REVIEWED | Main path matches; inherits `length=0` divergence. |
| `Base64StringToBase64UrlEncodedString` (`src/Encoding.cs:289`) | `base64StringToBase64UrlEncodedString` (`ts-source/base64.ts:45`) | REVIEWED | Character replacements and padding removal match. |
| `Base64UrlToBase64` (`src/Encoding.cs:303`) | `base64UrlToBase64` (`ts-source/base64.ts:58`) | REVIEWED | URL-safe replacement and padding restoration match. |
| `DecodeBase64UrlToString` (`src/Encoding.cs:320`) | `decodeBase64urlToString` / `atob` (`ts-source/base64.ts:74`, `:148`) | REVIEWED | Valid inputs match; invalid-character handling diverges below. |
| `PointEncode` (`src/Encoding.cs:345`) | `pointEncode` (`ts-source/encode.ts:8`) | REVIEWED | Prefix and x-coordinate copy match; omitted TS redundant length check is unreachable. |
| `Base58Encode` (`src/Encoding.cs:379`) | `bs58.encode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Standard Bitcoin alphabet and leading-zero handling reviewed. |
| `Base58Decode` (`src/Encoding.cs:414`) | `bs58.decode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Byte math reviewed; invalid error text may differ. |
| `Base58CheckEncode` (`src/Encoding.cs:463`) | `bs58check.encode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Payload plus double-SHA256 first-4-byte checksum matches. |
| `Base58CheckDecode` (`src/Encoding.cs:487`) | `bs58check.decode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Valid checksum logic matches; invalid error text may differ. |
| `Uint8ArrayToString` (`src/Encoding.cs:531`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS `TextDecoder` pattern elsewhere. |
| `ConcatUint8Arrays` (`src/Encoding.cs:548`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS spread/concat pattern elsewhere. |

**C. Intentional Adaptations**

- `Uint8Array` -> `byte[]`: structural runtime type mapping; byte order is unchanged.
- TS `reduce` string building -> `StringBuilder`: same emitted characters/bytes.
- TS `Uint8Array#set`, `slice`, spread -> `Array.Copy`/new arrays: same array contents and order.
- Pure JS `btoa` over 0..255 code units -> code-point guard plus `Convert.ToBase64String`: same base64 bytes for valid upstream inputs.
- Regex and `parseInt` for validated hex -> `Regex` plus `Convert.ToByte`: same bytes for `uint8ArrayFromHexString` valid inputs.
- `Error` -> `ArgumentException` / `ArgumentNullException`: language-native exception mapping; explicit upstream validation failures mostly keep the same message text.
- `bs58` / `bs58check` npm calls -> local BigInteger base58 plus double-SHA256 checksum: same valid wire encoding/decoding.
- TS exported namespace objects `bs58`, `bs58check` -> direct C# methods: structural API flattening; same valid byte/string conversion.
- `TextDecoder` and spread idioms used by other TS packages -> `Uint8ArrayToString` and `ConcatUint8Arrays`: C# helper extraction, not an encoding wire-format change.

**D. D17 Enforcement Check**

N/A (not Crypto.cs).

**E. Logic Divergence Findings**

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Encoding.cs:134`, `:139` | `ts-source/hex.ts:38` | `length=0` differs. TS uses `if (!length)` so `0` is treated as omitted and returns the parsed buffer; C# treats `0` as a requested target length and throws for non-empty input. | Change to `if (!length.HasValue || length.Value == 0) return buffer;` if runtime parity with TS falsy `0` is required. |
| `src/Encoding.cs:276` | `ts-source/base64.ts:31-32`, `ts-source/hex.ts:38` | `HexStringToBase64Url(input, 0)` inherits the same `length=0` divergence from `Uint8ArrayFromHexString`. | Fix the helper above; no separate logic change needed here. |
| `src/Encoding.cs:162-166` | `ts-source/hex.ts:58-63` | `HexToAscii` differs for odd or malformed input. TS loops while `i < length`, parses a trailing single nibble, and JS `parseInt` can yield `NaN` which `String.fromCharCode` turns into NUL. C# loops only while `i + 1 < length`, dropping a trailing nibble, and throws on malformed pairs. | If exact JS runtime behavior is required, loop while `i < hexString.Length` and implement JS-like `parseInt(..., 16)` semantics for partial/invalid pairs. |
| `src/Encoding.cs:322-323` | `ts-source/base64.ts:148-160`, `:171-198` | `decodeBase64urlToString` differs for invalid characters. Upstream `atob` strips non-base64 characters before decoding; `Convert.FromBase64String` rejects many of them. Example: upstream decodes `a*GVsbG8` to `hello`; C# throws. | Port upstream `atob` sanitizer/decoder or sanitize with the same regex before `Convert.FromBase64String`, including the `length % 4 === 1` error rule. |
| `src/Encoding.cs:431-435` | `ts-source/bs58.ts:36` | Invalid base58 inputs throw C# `ArgumentException` with custom text; upstream delegates to `bs58@6.0.0` and likely exposes dependency-specific error text/type. Valid bytes match. | Either match upstream dependency error messages or explicitly document that invalid-input exception text is not a parity surface. |
| `src/Encoding.cs:490-507` | `ts-source/bs58check.ts:36` | Invalid/too-short/checksum-failing Base58Check inputs throw C# custom messages; upstream delegates to `bs58check@4.0.0` dependency behavior. Valid payload/checksum bytes match. | Either match upstream dependency error behavior or document exception text as intentionally .NET-native. |

**F. Fixture Comparison Gate**

`tests/Fixtures/` is absent in this checkout, despite `README.md:61` describing golden fixtures and `tests/turnkey-sdk-csharp.Tests.csproj:24` including `Fixtures\**\*`. Therefore there are no fixture files under `tests/Fixtures/` that exercise `Encoding.cs`, and I cannot confirm fixture provenance from the pinned package. F is not satisfied as “all fixtures match” in this round.

I did verify that `tests/EncodingTests.cs` uses inline vectors copied from `ts-source/__tests__/index-test.ts` for the main hex/base64 cases, and I ran Node `Buffer` checks for the base64url expected values. The Base58/Base58Check tests are local known-vector/roundtrip tests, not generated fixture files from the pinned upstream package.

**G. Unresolved Assumptions**

- I treated typed non-null TS inputs as the main parity surface; C# null guards differ from untyped JS runtime coercion/error behavior.
- Exact `bs58@6.0.0` and `bs58check@4.0.0` dependency error messages were not executable locally because the pinned package has no `node_modules`.
- No `tests/Fixtures/` directory exists, so fixture generation/provenance cannot be verified for this file.
- I did not run `dotnet test`; the sandbox is read-only, and the review above is based on source inspection plus read-only Node checks.


