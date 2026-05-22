# Codex review — src/Encoding.cs — round 2 — 20260523

C# SHA: `a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80`
Upstream snapshot: `turnkey-encoding-0.6.0`

---

2026-05-22T23:30:16.096848Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e5206-7914-7e83-beba-d0ff67d74df6
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 2 of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA (a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80).

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

2026-05-22T23:30:16.841755Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll use the review workflow and verify against the pinned upstream snapshot in the workspace, including the checksum and fixture provenance.
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
/bin/zsh -lc 'rg -n "turnkey|C# SDK|csharp|Encoding.cs|round" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
108:- extensions/chronicle/resources/2026-05-22T06-17-00-qyoC-10min-memory-summary.md (cwd=/Users/takeshi/projects/manga-distribution-site, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T06-17-00-qyoC-10min-memory-summary.md, updated_at=2026-05-22T06:17:00+00:00, thread_id=None, chronicle synthetic task for the background visibility of the same project)
134:- The high-signal changed files for PR #61 were `app/cover/[slug]/route.tsx`, `app/search/page.tsx`, `app/updates/page.tsx`, `app/opengraph-image.tsx`, `components/cover-image.tsx`, `components/new-updates-preview.tsx`, `components/popular-works.tsx`, `components/title-header.tsx`, `lib/data/queries.ts`, `scripts/`, and tests around cover overrides/OpenBD fetch/export behavior [Task 3]
217:- extensions/chronicle/resources/2026-05-22T05-07-00-CseJ-10min-memory-summary.md (cwd=/Users/takeshi/Kyuzan/src/eggrypto-x-workspace/eggrypto-x, rollout_path=/Users/takeshi/.codex/memories/extensions/chronicle/resources/2026-05-22T05-07-00-CseJ-10min-memory-summary.md, updated_at=2026-05-22T05:07:00+00:00, thread_id=None, chronicle synthetic task for SSH/public-key setup around the same workspace)
273:# Task Group: /Users/takeshi/.codex/worktrees/1605/peak PR #330 C# SDK plan review
275:scope: Review and iterative verify-resolve workflow for the Peak C# SDK migration-plan PR, including what to read first, how the user expects review comments to be delivered, and the concrete plan-shape decisions that survived the review loop.
278:## Task 1: Initial review of PR #330 `plans/plans-peak-sdk-csharp.md`, inline comments posted
282:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, canonical rollout evidence)
288:- PR #330, plans/plans-peak-sdk-csharp.md, docs(plans): add peak-sdk-csharp migration plan, review the new plan to implement csharp sdk, inline comments, TurnkeyJsonContext, submodule init, csharp-publish.yml
294:- rollout_summaries/2026-05-22T04-48-49-boM1-pr330_csharp_sdk_plan_iterative_review.md (cwd=/Users/takeshi/.codex/worktrees/1605/peak, rollout_path=/Users/takeshi/.codex/sessions/2026/05/22/rollout-2026-05-22T13-48-49-019e4e03-c201-74f2-92c3-2658dfde9489.jsonl, updated_at=2026-05-22T06:45:02+00:00, thread_id=019e4e03-c201-74f2-92c3-2658dfde9489, latest verified head `a1adb9a6ef183151430b7b106e3c6bafce649e5e`)
300:- resolve可能であればresolveし、追加の指摘があれば指摘追加してください, verify latest head, resolve thread, locally packed .nupkg, feature/plan-peak-sdk-csharp, reviewer waiting
304:- when reviewing a plan/PR, the user asked: "review the new plan to implement csharp sdk" -> default to a PR-style review with concrete findings, not abstract advice [Task 1]
312:- The stable final plan shape after the verify-resolve loop was PR1 build/test only, PR5 locally packed `.nupkg`, PR7 independent `csharp-publish.yml`, and PR4.5 splitting secure-storage adapters between core and Unity packages [Task 2]
369:- Detached launches in this environment are tricky: one background run produced no surviving process or log, `nohup` did not survive environment cleanup, and the flow moved to `tmux` for the isolated pessimistic-paper run [Task 3]
392:## Task 2: Review prototype spike surfaces around class switching and motor/controller design
407:- The spike/planning surfaces worth reopening first are `R5ClassSwitchSpike.cs`, `V1MotorSpike.cs`, `SpriteResolver`, `SpriteRenderer`, `SpriteSkin`, and the ADR set around class switching/controller/camera/foundation [Task 2]

codex
I found the local review skill guidance; for this request I’m applying the findings-first review posture, but the user-specified A-G report format takes precedence. Next I’m reading the C# file, upstream package contents, and relevant fixtures/tests.
exec
/bin/zsh -lc 'git status --short' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
   151	        }
   152	
   153	        /// <summary>
   154	        /// Converts a hex string to an ASCII string.
   155	        /// Upstream: <c>hex.ts hexToAscii</c>.
   156	        /// </summary>
   157	        /// <remarks>
   158	        /// Upstream JS uses <c>parseInt(s.substr(i, 2), 16)</c> for each pair.
   159	        /// JS <c>String.prototype.substr(i, 2)</c> can return 1 char on the final
   160	        /// odd index, and <c>parseInt</c> can return <c>NaN</c> for unparseable
   161	        /// pairs; <c>String.fromCharCode(NaN)</c> emits <c>U+0000</c>. This port
   162	        /// preserves both behaviors so wire bytes match upstream for any input.
   163	        /// </remarks>
   164	        public static string HexToAscii(string hexString)
   165	        {
   166	            if (hexString == null)
   167	            {
   168	                throw new ArgumentNullException(nameof(hexString));
   169	            }
   170	
   171	            var sb = new StringBuilder(hexString.Length / 2);
   172	            for (int i = 0; i < hexString.Length; i += 2)
   173	            {
   174	                // substr(i, 2): may yield 1 char on the trailing odd index.
   175	                int take = Math.Min(2, hexString.Length - i);
   176	                string pair = hexString.Substring(i, take);
   177	
   178	                // JS parseInt(s, 16):
   179	                //   - skips leading whitespace
   180	                //   - parses as many leading [0-9A-Fa-f] characters as possible
   181	                //   - returns NaN if no characters parse
   182	                int parsed = JsParseIntBase16(pair);
   183	                sb.Append((char)parsed);
   184	            }
   185	            return sb.ToString();
   186	        }
   187	
   188	        /// <summary>
   189	        /// JS-compatible <c>parseInt(s, 16)</c>: parses the leading hex prefix of
   190	        /// <paramref name="s"/>. Returns 0 for inputs that JS would map to <c>NaN</c>
   191	        /// (no hex digits parseable) so that <c>String.fromCharCode(NaN)</c>
   192	        /// behavior is matched (<c>NaN → U+0000</c>).
   193	        /// </summary>
   194	        private static int JsParseIntBase16(string s)
   195	        {
   196	            if (string.IsNullOrEmpty(s))
   197	            {
   198	                return 0;
   199	            }
   200	            int value = 0;
   201	            int parsedDigits = 0;
   202	            foreach (char c in s)
   203	            {
   204	                int digit;
   205	                if (c >= '0' && c <= '9') digit = c - '0';
   206	                else if (c >= 'a' && c <= 'f') digit = 10 + (c - 'a');
   207	                else if (c >= 'A' && c <= 'F') digit = 10 + (c - 'A');
   208	                else break;
   209	                value = value * 16 + digit;
   210	                parsedDigits++;
   211	            }
   212	            // JS parseInt returns NaN when no digits parsed; String.fromCharCode(NaN) = 0
   213	            return parsedDigits == 0 ? 0 : value;
   214	        }
   215	
   216	        /// <summary>
   217	        /// Normalizes the padding of a byte array to a target length by either
   218	        /// left-padding with zero bytes or stripping leading zero bytes.
   219	        /// Throws if too many non-zero bytes would have to be removed.
   220	        /// Upstream: <c>hex.ts normalizePadding</c>.
   221	        /// </summary>
   222	        public static byte[] NormalizePadding(byte[] byteArray, int targetLength)
   223	        {
   224	            if (byteArray == null)
   225	            {
   226	                throw new ArgumentNullException(nameof(byteArray));
   227	            }
   228	
   229	            int paddingLength = targetLength - byteArray.Length;
   230	
   231	            // Add leading zeros
   232	            if (paddingLength > 0)
   233	            {
   234	                var result = new byte[targetLength];
   235	                Array.Copy(byteArray, 0, result, paddingLength, byteArray.Length);
   236	                return result;
   237	            }
   238	
   239	            // Strip leading zeros
   240	            if (paddingLength < 0)
   241	            {
   242	                int expectedZeroCount = -paddingLength;
   243	                int zeroCount = 0;
   244	                for (int i = 0; i < expectedZeroCount && i < byteArray.Length; i++)
   245	                {
   246	                    if (byteArray[i] == 0)
   247	                    {
   248	                        zeroCount++;
   249	                    }
   250	                }
   251	                if (zeroCount != expectedZeroCount)
   252	                {
   253	                    throw new ArgumentException(
   254	                        "invalid number of starting zeroes. Expected number of zeroes: "
   255	                        + expectedZeroCount + ". Found: " + zeroCount + ".");
   256	                }
   257	
   258	                var result = new byte[targetLength];
   259	                Array.Copy(byteArray, expectedZeroCount, result, 0, targetLength);
   260	                return result;

exec
/bin/zsh -lc "sed -n '1,120p' codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz

exec
/bin/zsh -lc 'git log --oneline --decorate -n 5 -- src/Encoding.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 2232ms:
2026-05-23 08:30:40.186 xcodebuild[45919:105893299]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:30:40.466 xcodebuild[45919:105893296] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-J7K8JqlC' (errno=Operation not permitted)
2026-05-23 08:30:40.979 xcodebuild[45940:105893355]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:30:41.120 xcodebuild[45940:105893354] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
a1cabdf (HEAD -> main) Fix Encoding.cs divergences flagged by Codex r1; add fixtures dir
0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0

 succeeded in 2368ms:
2026-05-23 08:30:40.186 xcodebuild[45918:105893297]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:30:40.466 xcodebuild[45918:105893293] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-MBxWT0Ao' (errno=Operation not permitted)
2026-05-23 08:30:40.979 xcodebuild[45941:105893356]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:30:41.120 xcodebuild[45941:105893353] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/Encoding.cs-r2-20260523.md

exec
/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '261,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   261	            }
   262	
   263	            return byteArray;
   264	        }
   265	
   266	        // ============================================================
   267	        // base64.ts
   268	        // ============================================================
   269	
   270	        /// <summary>
   271	        /// Converts a plain string into a base64url-encoded string.
   272	        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
   273	        /// </summary>
   274	        /// <remarks>
   275	        /// Upstream uses a pure-JS <c>btoa</c> for React Native compatibility and
   276	        /// throws on code points greater than 0xFF. This port preserves that
   277	        /// behavior by checking each code point against 0xFF before delegating to
   278	        /// <c>System.Convert.ToBase64String</c>, which is wire-identical for the
   279	        /// 0-255 byte range.
   280	        /// </remarks>
   281	        public static string StringToBase64UrlString(string input)
   282	        {
   283	            if (input == null)
   284	            {
   285	                throw new ArgumentNullException(nameof(input));
   286	            }
   287	
   288	            var bytes = new byte[input.Length];
   289	            for (int i = 0; i < input.Length; i++)
   290	            {
   291	                int cp = input[i];
   292	                if (cp > 0xFF)
   293	                {
   294	                    throw new ArgumentException(
   295	                        "InvalidCharacterError: found code point greater than 255:"
   296	                        + cp + " at position " + i);
   297	                }
   298	                bytes[i] = (byte)cp;
   299	            }
   300	
   301	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(bytes));
   302	        }
   303	
   304	        /// <summary>
   305	        /// Converts a hex string into a base64url-encoded string.
   306	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
   307	        /// </summary>
   308	        public static string HexStringToBase64Url(string input, int? length = null)
   309	        {
   310	            if (input == null)
   311	            {
   312	                throw new ArgumentNullException(nameof(input));
   313	            }
   314	
   315	            // Add a leading 0 if needed to get an even-length hex string.
   316	            // padStart(Math.ceil(input.length / 2) * 2, "0")
   317	            int targetHexLen = ((input.Length + 1) / 2) * 2;
   318	            string hexString = input.Length < targetHexLen
   319	                ? input.PadLeft(targetHexLen, '0')
   320	                : input;
   321	
   322	            var buffer = Uint8ArrayFromHexString(hexString, length);
   323	
   324	            // Upstream then folds the buffer into a per-byte string via
   325	            // String.fromCharCode and calls btoa on that. Convert.ToBase64String
   326	            // on the raw bytes produces the same wire bytes (validated by tests).
   327	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(buffer));
   328	        }
   329	
   330	        /// <summary>
   331	        /// Converts a standard base64 string into a base64url-encoded string by
   332	        /// replacing <c>+</c> with <c>-</c>, <c>/</c> with <c>_</c>, and stripping <c>=</c>.
   333	        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
   334	        /// </summary>
   335	        public static string Base64StringToBase64UrlEncodedString(string input)
   336	        {
   337	            if (input == null)
   338	            {
   339	                throw new ArgumentNullException(nameof(input));
   340	            }
   341	            return input.Replace('+', '-').Replace('/', '_').Replace("=", string.Empty);
   342	        }
   343	
   344	        /// <summary>
   345	        /// Converts a base64url string into a standard base64 string (with
   346	        /// <c>=</c> padding restored).
   347	        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
   348	        /// </summary>
   349	        public static string Base64UrlToBase64(string input)
   350	        {
   351	            if (input == null)
   352	            {
   353	                throw new ArgumentNullException(nameof(input));
   354	            }
   355	            string b64 = input.Replace('-', '+').Replace('_', '/');
   356	            int padLen = (4 - (b64.Length % 4)) % 4;
   357	            return b64 + new string('=', padLen);
   358	        }
   359	
   360	        /// <summary>
   361	        /// Decodes a base64url-encoded string into a plain string by first
   362	        /// restoring base64 padding and then base64-decoding into a byte buffer.
   363	        /// Each output byte is then reinterpreted as a code point.
   364	        /// Upstream: <c>base64.ts decodeBase64urlToString</c> + <c>atob</c>.
   365	        /// </summary>
   366	        /// <remarks>
   367	        /// Upstream <c>atob</c> silently strips any character not in
   368	        /// <c>[A-Za-z0-9+/=]</c> before decoding, and throws only when the
   369	        /// remaining length mod 4 is 1. This port reproduces that lenient
   370	        /// behavior so callers get the same string upstream JS would return.
   371	        /// </remarks>
   372	        public static string DecodeBase64UrlToString(string input)
   373	        {
   374	            if (input == null)
   375	            {
   376	                throw new ArgumentNullException(nameof(input));
   377	            }
   378	
   379	            string b64 = Base64UrlToBase64(input);
   380	
   381	            // Reproduce upstream atob: strip invalid characters first.
   382	            var filtered = new StringBuilder(b64.Length);
   383	            foreach (char c in b64)
   384	            {
   385	                if ((c >= 'A' && c <= 'Z')
   386	                    || (c >= 'a' && c <= 'z')
   387	                    || (c >= '0' && c <= '9')
   388	                    || c == '+' || c == '/' || c == '=')
   389	                {
   390	                    filtered.Append(c);
   391	                }
   392	            }
   393	            string sanitized = filtered.ToString();
   394	
   395	            // Upstream atob explicitly throws when (length % 4) == 1.
   396	            if (sanitized.Length % 4 == 1)
   397	            {
   398	                throw new ArgumentException(
   399	                    "InvalidCharacterError: The string to be decoded is not correctly encoded.");
   400	            }
   401	
   402	            // Upstream atob then walks the string and shifts in 6-bit groups,
   403	            // emitting full 8-bit bytes. Reproduce that bit-shifting decoder
   404	            // explicitly because it is more permissive than Convert.FromBase64String
   405	            // (it tolerates missing or partial padding the same way upstream does).
   406	            const string keyStr =
   407	                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
   408	            var sb = new StringBuilder();
   409	            int buffer = 0;
   410	            int bits = 0;
   411	            for (int i = 0; i < sanitized.Length; i++)
   412	            {
   413	                char ch = sanitized[i];
   414	                int index = keyStr.IndexOf(ch);
   415	                if (index < 0 || index > 64)
   416	                {
   417	                    continue;
   418	                }
   419	                if (ch == '=')
   420	                {
   421	                    bits = 0;
   422	                }
   423	                else
   424	                {
   425	                    buffer = (buffer << 6) | index;
   426	                    bits += 6;
   427	                }
   428	                while (bits >= 8)
   429	                {
   430	                    bits -= 8;
   431	                    sb.Append((char)((buffer >> bits) & 0xff));
   432	                }
   433	            }
   434	            return sb.ToString();
   435	        }
   436	
   437	        // ============================================================
   438	        // encode.ts
   439	        // ============================================================
   440	
   441	        /// <summary>
   442	        /// Compresses an uncompressed P-256 public key into its 33-byte compressed form.
   443	        /// Upstream: <c>encode.ts pointEncode</c>.
   444	        /// </summary>
   445	        /// <param name="raw">65-byte uncompressed key starting with 0x04.</param>
   446	        /// <returns>33-byte compressed key starting with 0x02 or 0x03.</returns>
   447	        public static byte[] PointEncode(byte[] raw)
   448	        {
   449	            if (raw == null)
   450	            {
   451	                throw new ArgumentNullException(nameof(raw));
   452	            }
   453	            if (raw.Length != 65 || raw[0] != 0x04)
   454	            {
   455	                throw new ArgumentException("Invalid uncompressed P-256 key");
   456	            }
   457	
   458	            // x = raw[1..33], y = raw[33..65]; lengths are 32 and 32 by construction.
   459	            byte yLastByte = raw[64];
   460	            byte prefix = (yLastByte & 1) == 0 ? (byte)0x02 : (byte)0x03;
   461	
   462	            var compressed = new byte[33];
   463	            compressed[0] = prefix;
   464	            Array.Copy(raw, 1, compressed, 1, 32);
   465	            return compressed;
   466	        }
   467	
   468	        // ============================================================
   469	        // bs58.ts (and bs58check.ts)
   470	        //
   471	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
   472	        // implements the same Bitcoin base58 algorithm in C# using BouncyCastle's
   473	        // BigInteger. The algorithm is standard and produces wire-identical bytes
   474	        // to the upstream packages for valid input.
   475	        // ============================================================
   476	
   477	        /// <summary>
   478	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
   479	        /// Upstream: <c>bs58.ts bs58.encode</c>.
   480	        /// </summary>
   481	        public static string Base58Encode(byte[] data)
   482	        {
   483	            if (data == null)
   484	            {
   485	                throw new ArgumentNullException(nameof(data));
   486	            }
   487	            if (data.Length == 0)
   488	            {
   489	                return string.Empty;
   490	            }
   491	
   492	            var intData = new BigInteger(1, data);
   493	            var sb = new StringBuilder();
   494	            var fiftyEight = BigInteger.ValueOf(58);
   495	
   496	            while (intData.CompareTo(BigInteger.Zero) > 0)
   497	            {
   498	                var divmod = intData.DivideAndRemainder(fiftyEight);
   499	                intData = divmod[0];
   500	                sb.Insert(0, Constants.BASE58_ALPHABET[divmod[1].IntValue]);
   501	            }
   502	
   503	            // Preserve leading zero bytes as '1' characters (Base58 convention).
   504	            for (int i = 0; i < data.Length && data[i] == 0; i++)
   505	            {
   506	                sb.Insert(0, '1');
   507	            }
   508	
   509	            return sb.ToString();
   510	        }
   511	
   512	        /// <summary>
   513	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
   514	        /// Upstream: <c>bs58.ts bs58.decode</c>.
   515	        /// </summary>
   516	        public static byte[] Base58Decode(string encoded)
   517	        {
   518	            if (encoded == null)
   519	            {
   520	                throw new ArgumentNullException(nameof(encoded));
   521	            }
   522	            if (encoded.Length == 0)
   523	            {
   524	                return new byte[0];
   525	            }
   526	
   527	            var decoded = BigInteger.Zero;
   528	            var multi = BigInteger.One;
   529	            var fiftyEight = BigInteger.ValueOf(58);
   530	
   531	            for (int i = encoded.Length - 1; i >= 0; i--)
   532	            {
   533	                int digit = Constants.BASE58_ALPHABET.IndexOf(encoded[i]);
   534	                if (digit < 0)
   535	                {
   536	                    throw new ArgumentException(
   537	                        "Invalid character '" + encoded[i] + "' in base58 string");
   538	                }
   539	                decoded = decoded.Add(multi.Multiply(BigInteger.ValueOf(digit)));
   540	                multi = multi.Multiply(fiftyEight);
   541	            }
   542	
   543	            byte[] bytes = decoded.Equals(BigInteger.Zero)
   544	                ? new byte[0]
   545	                : decoded.ToByteArrayUnsigned();
   546	
   547	            int leadingZeros = 0;
   548	            for (int i = 0; i < encoded.Length && encoded[i] == '1'; i++)
   549	            {
   550	                leadingZeros++;
   551	            }
   552	            if (leadingZeros > 0)
   553	            {
   554	                var result = new byte[leadingZeros + bytes.Length];
   555	                Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);
   556	                return result;
   557	            }
   558	            return bytes;
   559	        }
   560	
   561	        /// <summary>
   562	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
   563	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
   564	        /// </summary>
   565	        public static string Base58CheckEncode(byte[] payload)
   566	        {
   567	            if (payload == null)
   568	            {
   569	                throw new ArgumentNullException(nameof(payload));
   570	            }
   571	            using (var sha = SHA256.Create())
   572	            {
   573	                var h1 = sha.ComputeHash(payload);
   574	                var h2 = sha.ComputeHash(h1);
   575	                var checksum = new byte[4];
   576	                Array.Copy(h2, 0, checksum, 0, 4);
   577	                var concat = new byte[payload.Length + 4];
   578	                Array.Copy(payload, 0, concat, 0, payload.Length);
   579	                Array.Copy(checksum, 0, concat, payload.Length, 4);
   580	                return Base58Encode(concat);
   581	            }
   582	        }
   583	
   584	        /// <summary>
   585	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
   586	        /// SHA256(SHA256(payload)) checksum.
   587	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
   588	        /// </summary>
   589	        public static byte[] Base58CheckDecode(string encoded)
   590	        {
   591	            byte[] decoded = Base58Decode(encoded);
   592	            if (decoded.Length < 4)
   593	            {
   594	                throw new ArgumentException("Invalid Base58Check string - too short");
   595	            }
   596	
   597	            var data = new byte[decoded.Length - 4];
   598	            Array.Copy(decoded, 0, data, 0, data.Length);
   599	            var checksum = new byte[4];
   600	            Array.Copy(decoded, decoded.Length - 4, checksum, 0, 4);
   601	
   602	            using (var sha = SHA256.Create())
   603	            {
   604	                var h1 = sha.ComputeHash(data);
   605	                var h2 = sha.ComputeHash(h1);
   606	                if (h2[0] != checksum[0] || h2[1] != checksum[1]
   607	                    || h2[2] != checksum[2] || h2[3] != checksum[3])
   608	                {
   609	                    throw new ArgumentException("Invalid Base58Check checksum");
   610	                }
   611	            }
   612	
   613	            return data;
   614	        }
   615	
   616	        // ============================================================
   617	        // Unity-port-derived helpers (NOT in @turnkey/encoding)
   618	        //
   619	        // The upstream library reaches for these patterns inline (TextDecoder,
   620	        // spread). The C# port keeps them here as public helpers because

exec
/bin/zsh -lc "rg --files codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 tests/Fixtures tests | sed -n '1,240p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/README.md
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/encoding/turnkey-encoding-vectors.json
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/README.md
tests/EncodingTests.cs
tests/turnkey-sdk-csharp.Tests.csproj
tests/packages.lock.json
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/LICENSE
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.mjs
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.js.map

exec
/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '620,760p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   620	        // spread). The C# port keeps them here as public helpers because
   621	        // Crypto.cs / ApiKeyStamper.cs / Http.cs use them.
   622	        // ============================================================
   623	
   624	        /// <summary>
   625	        /// UTF-8 decodes a byte array into a string.
   626	        /// </summary>
   627	        /// <remarks>
   628	        /// Upstream uses <c>new TextDecoder().decode(bytes)</c> inline. This
   629	        /// helper centralizes the equivalent in the .NET port. It is
   630	        /// wire-irrelevant — it is invoked after wire bytes have already been
   631	        /// decided.
   632	        /// </remarks>
   633	        public static string Uint8ArrayToString(byte[] bytes)
   634	        {
   635	            if (bytes == null)
   636	            {
   637	                throw new ArgumentNullException(nameof(bytes));
   638	            }
   639	            return System.Text.Encoding.UTF8.GetString(bytes);
   640	        }
   641	
   642	        /// <summary>
   643	        /// Concatenates byte arrays.
   644	        /// </summary>
   645	        /// <remarks>
   646	        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
   647	        /// This helper centralizes the equivalent. It is wire-irrelevant — it
   648	        /// just glues bytes that were already computed.
   649	        /// </remarks>
   650	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
   651	        {
   652	            if (arrays == null)
   653	            {
   654	                throw new ArgumentNullException(nameof(arrays));
   655	            }
   656	            int total = arrays.Sum(a => a?.Length ?? 0);
   657	            var result = new byte[total];
   658	            int offset = 0;
   659	            foreach (var a in arrays)
   660	            {
   661	                if (a != null && a.Length > 0)
   662	                {
   663	                    Array.Copy(a, 0, result, offset, a.Length);
   664	                    offset += a.Length;
   665	                }
   666	            }
   667	            return result;
   668	        }
   669	    }
   670	}

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	{
     2	  "name": "@turnkey/encoding",
     3	  "version": "0.6.0",
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
    16	  "description": "Encoding utility functions",
    17	  "author": {
    18	    "name": "Turnkey",
    19	    "url": "https://turnkey.com/"
    20	  },
    21	  "repository": {
    22	    "type": "git",
    23	    "url": "git+https://github.com/tkhq/sdk.git",
    24	    "directory": "packages/encoding"
    25	  },
    26	  "files": [
    27	    "dist/",
    28	    "CHANGELOG.md",
    29	    "README.md"
    30	  ],
    31	  "publishConfig": {
    32	    "access": "public"
    33	  },
    34	  "engines": {
    35	    "node": ">=18.0.0"
    36	  },
    37	  "dependencies": {
    38	    "bs58": "6.0.0",
    39	    "bs58check": "4.0.0"
    40	  },
    41	  "scripts": {
    42	    "build": "rollup -c",
    43	    "clean": "rimraf ./dist ./.cache",
    44	    "test": "jest",
    45	    "typecheck": "tsc -p tsconfig.typecheck.json"
    46	  }
    47	}
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts | sed -n '1,180p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts | sed -n '1,140p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts | sed -n '1,140p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts | sed -n '1,180p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	export const DEFAULT_JWK_MEMBER_BYTE_LENGTH = 32;
     2	
     3	export * from "./base64";
     4	export * from "./encode";
     5	export * from "./hex";
     6	
     7	export { bs58 } from "./bs58";
     8	export { bs58check } from "./bs58check";

codex
The C# file is at the requested `a1cabdf` HEAD and the upstream snapshot declares `@turnkey/encoding` 0.6.0. I’m now checking whether fixtures were generated from that exact snapshot and whether the tests assert Node-equivalent byte/string outputs.
exec
/bin/zsh -lc "nl -ba tests/Fixtures/README.md | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '1,280p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
   144	        public void HexToAscii_OddLength_JsParity()
   145	        {
   146	            // JS: parseInt('f', 16) === 15, String.fromCharCode(15) === ''
   147	            Encoding.HexToAscii("f").Should().Be("");
   148	        }
   149	
   150	        [Fact]
   151	        public void HexToAscii_InvalidPair_EmitsNul()
   152	        {
   153	            // JS: parseInt('zz', 16) === NaN, String.fromCharCode(NaN) === ' '
   154	            Encoding.HexToAscii("zz").Should().Be(" ");
   155	        }
   156	
   157	        [Fact]
   158	        public void Uint8ArrayFromHexString_LengthZero_TreatedAsOmitted()
   159	        {
   160	            // JS: `if (!length)` is true for 0; behavior is identical to omitted length.
   161	            Encoding.Uint8ArrayFromHexString("01", 0).Should().Equal(new byte[] { 1 });
   162	        }
   163	
   164	        [Fact]
   165	        public void NormalizePadding_AddsLeadingZeros()
   166	        {
   167	            byte[] input = { 0x01, 0x02 };
   168	            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
   169	        }
   170	
   171	        [Fact]
   172	        public void NormalizePadding_StripsLeadingZeros()
   173	        {
   174	            byte[] input = { 0x00, 0x00, 0x01, 0x02 };
   175	            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
   176	        }
   177	
   178	        [Fact]
   179	        public void NormalizePadding_StripWouldRemoveNonZero_Throws()
   180	        {
   181	            byte[] input = { 0x01, 0x02, 0x03, 0x04 };
   182	            Action act = () => Encoding.NormalizePadding(input, 2);
   183	            act.Should().Throw<ArgumentException>()
   184	               .WithMessage("invalid number of starting zeroes*");
   185	        }
   186	
   187	        [Fact]
   188	        public void NormalizePadding_EqualLength_Identity()
   189	        {
   190	            byte[] input = { 0x01, 0x02 };
   191	            Encoding.NormalizePadding(input, 2).Should().Equal(input);
   192	        }
   193	
   194	        // ========================================================
   195	        // base64url
   196	        // ========================================================
   197	
   198	        [Fact]
   199	        public void StringToBase64UrlString_UpstreamHelloVector()
   200	        {
   201	            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
   202	        }
   203	
   204	        [Fact]
   205	        public void StringToBase64UrlString_UpstreamPrivateKeyVector()
   206	        {
   207	            // From index-test.ts
   208	            const string input =
   209	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
   210	            const string expected =
   211	                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
   212	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
   213	        }
   214	
   215	        [Fact]
   216	        public void StringToBase64UrlString_UpstreamApiKeyStampVector()
   217	        {
   218	            const string input =
   219	                "{\"publicKey\":\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\","
   220	                + "\"signature\":\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\","
   221	                + "\"scheme\":\"SIGNATURE_SCHEME_TK_API_P256\"}";
   222	            const string expected =
   223	                "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ";
   224	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
   225	        }
   226	
   227	        [Fact]
   228	        public void StringToBase64UrlString_CodePointAbove255_Throws()
   229	        {
   230	            Action act = () => Encoding.StringToBase64UrlString("aĀb");
   231	            act.Should().Throw<ArgumentException>()
   232	               .WithMessage("InvalidCharacterError: found code point greater than 255:256 at position 1");
   233	        }
   234	
   235	        [Fact]
   236	        public void Base64StringToBase64UrlEncodedString_UpstreamVectors()
   237	        {
   238	            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
   239	                .Should().Be("aGVsbG8gd29ybGQ");
   240	            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
   241	                .Should().Be("U29tZSBzYW1wbGUgdGV4dA");
   242	        }
   243	
   244	        [Fact]
   245	        public void HexStringToBase64Url_UpstreamVectors()
   246	        {
   247	            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
   248	            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
   249	            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
   250	            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
   251	        }
   252	
   253	        [Fact]
   254	        public void HexStringToBase64Url_TooLong_Throws()
   255	        {
   256	            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
   257	            act.Should().Throw<ArgumentException>()
   258	               .WithMessage("hex value cannot fit in a buffer of 1 byte(s)");
   259	        }
   260	
   261	        [Fact]
   262	        public void Base64UrlToBase64_RestoresPadding()
   263	        {
   264	            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
   265	            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
   266	            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
   267	        }
   268	
   269	        [Fact]
   270	        public void Base64UrlToBase64_NoChangeWhenAlreadyPadded()
   271	        {
   272	            // "AAAA" length 4, padding 0
   273	            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
   274	        }
   275	
   276	        [Fact]
   277	        public void DecodeBase64UrlToString_Roundtrip()
   278	        {
   279	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
   280	            // From upstream private-key encoding vector

exec
/bin/zsh -lc "nl -ba tests/Fixtures/encoding/turnkey-encoding-vectors.json | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "rg -n \"turnkey-encoding-vectors|Encoding|Base58|HexStringToBase64|PointEncode|Uint8Array\" tests src codex-crypto-reviews -g '"'!codex-crypto-reviews/upstream-snapshots/**/package/dist/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md:23:│   └── turnkey-encoding-vectors.json       — upstream-test-vectors
codex-crypto-reviews/README.md:10:1. `Encoding.cs`
codex-crypto-reviews/README.md:78:   observable behavior). Examples: Task<T>↔Promise<T>, byte[]↔Uint8Array,
codex-crypto-reviews/unity-source-pins.md:4:`packages/turnkey-sdk-unity/Runtime/{Encoding,Crypto,ApiKeyStamper,Http,UnityConstants}.cs`
codex-crypto-reviews/unity-source-pins.md:38:| src/Encoding.cs             | packages/turnkey-sdk-unity/Runtime/Encoding.cs             |
codex-crypto-reviews/turnkey-source-pins.md:12:| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
src/packages.lock.json:21:          "System.Text.Encodings.Web": "8.0.0",
src/packages.lock.json:55:      "System.Text.Encodings.Web": {
codex-crypto-reviews/peak-lockfile-evidence.md:47:Uint8Array adaptations).
src/Encoding.cs:7://   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
src/Encoding.cs:10://                                HexStringToBase64Url /
src/Encoding.cs:14://   ts-source/encode.ts       -> PointEncode
src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
src/Encoding.cs:20://   Uint8Array          -> byte[]
src/Encoding.cs:24://    is after hex -> Uint8Array conversion. The custom btoa only exists in
src/Encoding.cs:34://   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
src/Encoding.cs:35://                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
src/Encoding.cs:36://   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
src/Encoding.cs:49:    /// Encoding utilities for the Turnkey API.
src/Encoding.cs:52:    public static class Encoding
src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
src/Encoding.cs:90:        public static string Uint8ArrayToHexString(byte[] input)
src/Encoding.cs:118:        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
src/Encoding.cs:308:        public static string HexStringToBase64Url(string input, int? length = null)
src/Encoding.cs:322:            var buffer = Uint8ArrayFromHexString(hexString, length);
src/Encoding.cs:447:        public static byte[] PointEncode(byte[] raw)
src/Encoding.cs:478:        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
src/Encoding.cs:481:        public static string Base58Encode(byte[] data)
src/Encoding.cs:503:            // Preserve leading zero bytes as '1' characters (Base58 convention).
src/Encoding.cs:513:        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
src/Encoding.cs:516:        public static byte[] Base58Decode(string encoded)
src/Encoding.cs:562:        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
src/Encoding.cs:565:        public static string Base58CheckEncode(byte[] payload)
src/Encoding.cs:580:                return Base58Encode(concat);
src/Encoding.cs:585:        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
src/Encoding.cs:589:        public static byte[] Base58CheckDecode(string encoded)
src/Encoding.cs:591:            byte[] decoded = Base58Decode(encoded);
src/Encoding.cs:594:                throw new ArgumentException("Invalid Base58Check string - too short");
src/Encoding.cs:609:                    throw new ArgumentException("Invalid Base58Check checksum");
src/Encoding.cs:633:        public static string Uint8ArrayToString(byte[] bytes)
src/Encoding.cs:639:            return System.Text.Encoding.UTF8.GetString(bytes);
src/Encoding.cs:646:        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
src/Encoding.cs:650:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1:# Codex review — src/Encoding.cs — round 2 — 20260523
codex-crypto-reviews/Encoding.cs-r2-20260523.md:22:File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:34:B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r2-20260523.md:298:/bin/zsh -lc 'rg -n "turnkey|C# SDK|csharp|Encoding.cs|round" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r2-20260523.md:321:/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r2-20260523.md:329:     7	//   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
codex-crypto-reviews/Encoding.cs-r2-20260523.md:332:    10	//                                HexStringToBase64Url /
codex-crypto-reviews/Encoding.cs-r2-20260523.md:336:    14	//   ts-source/encode.ts       -> PointEncode
codex-crypto-reviews/Encoding.cs-r2-20260523.md:337:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r2-20260523.md:338:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r2-20260523.md:342:    20	//   Uint8Array          -> byte[]
codex-crypto-reviews/Encoding.cs-r2-20260523.md:346:    24	//    is after hex -> Uint8Array conversion. The custom btoa only exists in
codex-crypto-reviews/Encoding.cs-r2-20260523.md:356:    34	//   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
codex-crypto-reviews/Encoding.cs-r2-20260523.md:357:    35	//                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:358:    36	//   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
codex-crypto-reviews/Encoding.cs-r2-20260523.md:371:    49	    /// Encoding utilities for the Turnkey API.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:374:    52	    public static class Encoding
codex-crypto-reviews/Encoding.cs-r2-20260523.md:388:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:412:    90	        public static string Uint8ArrayToHexString(byte[] input)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:440:   118	        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:593:/bin/zsh -lc 'git log --oneline --decorate -n 5 -- src/Encoding.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r2-20260523.md:601:a1cabdf (HEAD -> main) Fix Encoding.cs divergences flagged by Codex r1; add fixtures dir
codex-crypto-reviews/Encoding.cs-r2-20260523.md:602:0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r2-20260523.md:611:?? codex-crypto-reviews/Encoding.cs-r2-20260523.md
codex-crypto-reviews/Encoding.cs-r2-20260523.md:614:/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '261,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r2-20260523.md:663:   308	        public static string HexStringToBase64Url(string input, int? length = null)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:677:   322	            var buffer = Uint8ArrayFromHexString(hexString, length);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:802:   447	        public static byte[] PointEncode(byte[] raw)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:833:   478	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:836:   481	        public static string Base58Encode(byte[] data)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:858:   503	            // Preserve leading zero bytes as '1' characters (Base58 convention).
codex-crypto-reviews/Encoding.cs-r2-20260523.md:868:   513	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:871:   516	        public static byte[] Base58Decode(string encoded)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:917:   562	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
codex-crypto-reviews/Encoding.cs-r2-20260523.md:920:   565	        public static string Base58CheckEncode(byte[] payload)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:935:   580	                return Base58Encode(concat);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:940:   585	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
codex-crypto-reviews/Encoding.cs-r2-20260523.md:944:   589	        public static byte[] Base58CheckDecode(string encoded)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:946:   591	            byte[] decoded = Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:949:   594	                throw new ArgumentException("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r2-20260523.md:964:   609	                    throw new ArgumentException("Invalid Base58Check checksum");
codex-crypto-reviews/Encoding.cs-r2-20260523.md:982:tests/Fixtures/encoding/turnkey-encoding-vectors.json
codex-crypto-reviews/Encoding.cs-r2-20260523.md:983:tests/Fixtures/encoding/turnkey-encoding-vectors.json
codex-crypto-reviews/Encoding.cs-r2-20260523.md:986:tests/EncodingTests.cs
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1036:/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '620,760p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1051:   633	        public static string Uint8ArrayToString(byte[] bytes)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1057:   639	            return System.Text.Encoding.UTF8.GetString(bytes);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1064:   646	        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1068:   650	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1094:     2	 * Converts a Uint8Array into a lowercase hex string.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1096:     4	 * @param {Uint8Array} input - The input byte array.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1099:     7	export function uint8ArrayToHexString(input: Uint8Array): string {
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1107:    15	 * Creates a Uint8Array from a hex string.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1112:    20	 * @returns {Uint8Array} - The resulting byte array.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1118:    26	): Uint8Array => {
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1126:    34	  const buffer = new Uint8Array(
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1140:    48	  let paddedBuffer = new Uint8Array(length);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1161:    69	 * @param {Uint8Array} byteArray - The byte array to pad or trim.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1163:    71	 * @returns {Uint8Array} - The normalized byte array.
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1166:    74	  byteArray: Uint8Array,
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1168:    76	): Uint8Array => {
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1173:    81	    const padding = new Uint8Array(paddingLength).fill(0);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1174:    82	    return new Uint8Array([...padding, ...byteArray]);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1418:    16	  "description": "Encoding utility functions",
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1456:     4	 * @param {Uint8Array} raw - The uncompressed public key (65 bytes, starting with 0x04).
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1457:     5	 * @returns {Uint8Array} - The compressed public key (33 bytes, starting with 0x02 or 0x03).
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1460:     8	export function pointEncode(raw: Uint8Array): Uint8Array {
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1474:    22	  const compressed = new Uint8Array(33);
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1501:    19	  encode(buffer: Uint8Array | number[]): string;
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1502:    20	  decodeUnsafe(str: string): Uint8Array | undefined;
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1503:    21	  decode(str: string): Uint8Array;
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1541:    19	  encode(payload: Uint8Array | number[]): string;
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1542:    20	  decode(str: string): Uint8Array;
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1543:    21	  decodeUnsafe(str: string): Uint8Array | undefined;
codex-crypto-reviews/Encoding.cs-r2-20260523.md:1599:    23	│   └── turnkey-encoding-vectors.json       — upstream-test-vectors
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:16:  "description": "Encoding utility functions",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:176:      const concatenatedBytes = new Uint8Array(64);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:250:  const encryptionQuorumPublicBuf = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:269: * @param {Uint8Array} publicKey - The raw P-256 public key bytes.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:273:const loadPublicKey = (publicKey: Uint8Array): ProjPointType<bigint> => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:282: * @returns {Uint8Array} - The decoded private key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:284:const decodeKey = (privateKey: string, keyFormat: any): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:433: * 4.  Import the notarizer public key (hex `04‖X‖Y` → `Uint8Array`).
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:459:  const msgDigest = sha256(h1); // 32-byte Uint8Array
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:467:  const signature = Uint8Array.from(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:483: * to Uint8Arrays to be used by the lower level quorumKeyEncrypt
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:489: * @returns {Promise<Uint8Array>} A borsh serialized envelope with the encrypted message (more details found in quorumKeyEncrypt)
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:494:): Promise<Uint8Array> => {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts:4: * @param {Uint8Array} raw - The uncompressed public key (65 bytes, starting with 0x04).
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts:5: * @returns {Uint8Array} - The compressed public key (33 bytes, starting with 0x02 or 0x03).
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts:8:export function pointEncode(raw: Uint8Array): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts:22:  const compressed = new Uint8Array(33);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:53:  const uint8Array = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:59:  expect(uint8ArrayToHexString(uint8Array)).toBe(expectedHexString); // Uint8Array => Hex string
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:67:  const expectedUint8Array = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:71:  expect(uint8ArrayFromHexString(hexString)).toEqual(expectedUint8Array); // Hex string => Uint8Array
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:101:  const expectedUint8Array2 = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:105:  expect(uint8ArrayFromHexString(hexString2, 32)).toEqual(expectedUint8Array2); // Hex string => Uint8Array
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:16:  const byteView = new Uint8Array(buffer);
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:25:  const byteView = new Uint8Array(buffer);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1:# Codex review — src/Encoding.cs — round 1 — 20260523
codex-crypto-reviews/Encoding.cs-r1-20260523.md:22:File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:34:B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:298:/bin/zsh -lc 'rg -n "turnkey-sdk-csharp|turnkey|Encoding.cs|C# SDK" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:309:tests/EncodingTests.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:310:tests/EncodingTests.cs:259:            // From upstream private-key encoding vector
codex-crypto-reviews/Encoding.cs-r1-20260523.md:311:src/Encoding.cs:1:// 1:1 logical port of @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:312:src/Encoding.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
codex-crypto-reviews/Encoding.cs-r1-20260523.md:313:src/Encoding.cs:50:    /// 1:1 logical port of <c>@turnkey/encoding</c> v0.6.0.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:314:src/Encoding.cs:55:        /// Internal constants exported by <c>@turnkey/encoding</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:315:src/Encoding.cs:61:            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:316:src/Encoding.cs:515:        // Unity-port-derived helpers (NOT in @turnkey/encoding)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:325:/bin/zsh -lc "git status --short && git log --oneline -5 -- src/Encoding.cs && nl -ba src/Encoding.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:333:?? codex-crypto-reviews/Encoding.cs-r1-20260523.md
codex-crypto-reviews/Encoding.cs-r1-20260523.md:342:0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:349:     7	//   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
codex-crypto-reviews/Encoding.cs-r1-20260523.md:352:    10	//                                HexStringToBase64Url /
codex-crypto-reviews/Encoding.cs-r1-20260523.md:356:    14	//   ts-source/encode.ts       -> PointEncode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:362:    20	//   Uint8Array          -> byte[]
codex-crypto-reviews/Encoding.cs-r1-20260523.md:366:    24	//    is after hex -> Uint8Array conversion. The custom btoa only exists in
codex-crypto-reviews/Encoding.cs-r1-20260523.md:376:    34	//   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
codex-crypto-reviews/Encoding.cs-r1-20260523.md:377:    35	//                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:378:    36	//   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
codex-crypto-reviews/Encoding.cs-r1-20260523.md:391:    49	    /// Encoding utilities for the Turnkey API.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:394:    52	    public static class Encoding
codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:432:    90	        public static string Uint8ArrayToHexString(byte[] input)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:460:   118	        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:683:tests/EncodingTests.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:745:/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '235,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:774:   262	        public static string HexStringToBase64Url(string input, int? length = null)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:788:   276	            var buffer = Uint8ArrayFromHexString(hexString, length);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:857:   345	        public static byte[] PointEncode(byte[] raw)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:888:   376	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:891:   379	        public static string Base58Encode(byte[] data)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:913:   401	            // Preserve leading zero bytes as '1' characters (Base58 convention).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:923:   411	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:926:   414	        public static byte[] Base58Decode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:972:   460	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:975:   463	        public static string Base58CheckEncode(byte[] payload)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:990:   478	                return Base58Encode(concat);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:995:   483	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:999:   487	        public static byte[] Base58CheckDecode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1001:   489	            byte[] decoded = Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1004:   492	                throw new ArgumentException("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1019:   507	                    throw new ArgumentException("Invalid Base58Check checksum");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1043:   531	        public static string Uint8ArrayToString(byte[] bytes)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1049:   537	            return System.Text.Encoding.UTF8.GetString(bytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1056:   544	        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1060:   548	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1086:     2	 * Converts a Uint8Array into a lowercase hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1088:     4	 * @param {Uint8Array} input - The input byte array.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1091:     7	export function uint8ArrayToHexString(input: Uint8Array): string {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1099:    15	 * Creates a Uint8Array from a hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1104:    20	 * @returns {Uint8Array} - The resulting byte array.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1110:    26	): Uint8Array => {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1118:    34	  const buffer = new Uint8Array(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1132:    48	  let paddedBuffer = new Uint8Array(length);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1153:    69	 * @param {Uint8Array} byteArray - The byte array to pad or trim.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1155:    71	 * @returns {Uint8Array} - The normalized byte array.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1158:    74	  byteArray: Uint8Array,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1160:    76	): Uint8Array => {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1165:    81	    const padding = new Uint8Array(paddingLength).fill(0);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1166:    82	    return new Uint8Array([...padding, ...byteArray]);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1409:    19	  encode(buffer: Uint8Array | number[]): string;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1410:    20	  decodeUnsafe(str: string): Uint8Array | undefined;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1411:    21	  decode(str: string): Uint8Array;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1445:    19	  encode(payload: Uint8Array | number[]): string;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1446:    20	  decode(str: string): Uint8Array;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1447:    21	  decodeUnsafe(str: string): Uint8Array | undefined;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1466:     4	 * @param {Uint8Array} raw - The uncompressed public key (65 bytes, starting with 0x04).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1467:     5	 * @returns {Uint8Array} - The compressed public key (33 bytes, starting with 0x02 or 0x03).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1470:     8	export function pointEncode(raw: Uint8Array): Uint8Array {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1484:    22	  const compressed = new Uint8Array(33);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1499:/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '1,380p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1501:     1	// EncodingTests.cs — xunit tests for src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1507:     7	// roundtrip, and PointEncode round-trip.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1515:    15	    public class EncodingTests
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1522:    22	        public void Uint8ArrayToHexString_UpstreamVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1533:    33	            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1537:    37	        public void Uint8ArrayToHexString_EmptyArray_ReturnsEmpty()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1539:    39	            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1543:    43	        public void Uint8ArrayToHexString_Null_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1545:    45	            Action act = () => Encoding.Uint8ArrayToHexString(null!);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1550:    50	        public void Uint8ArrayToHexString_LeadingZeros_Preserved()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1553:    53	            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1557:    57	        public void Uint8ArrayFromHexString_UpstreamVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1567:    67	            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1571:    71	        public void Uint8ArrayFromHexString_ShortAscii()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1575:    75	            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1582:    82	        public void Uint8ArrayFromHexString_Invalid_Throws(string hex)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1584:    84	            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1590:    90	        public void Uint8ArrayFromHexString_WithLength_Pads()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1593:    93	            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1597:    97	        public void Uint8ArrayFromHexString_WithoutLength_NoPadding()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1599:    99	            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1603:   103	        public void Uint8ArrayFromHexString_TooShortGetsPadded()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1613:   113	            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1617:   117	        public void Uint8ArrayFromHexString_TooLong_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1622:   122	            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1628:   128	        public void Uint8ArrayFromHexString_OneByte_FitTwoByteRequest_Padding()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1631:   131	            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1640:   140	            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1647:   147	            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1654:   154	            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1661:   161	            Action act = () => Encoding.NormalizePadding(input, 2);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1670:   170	            Encoding.NormalizePadding(input, 2).Should().Equal(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1680:   180	            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1691:   191	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1703:   203	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1709:   209	            Action act = () => Encoding.StringToBase64UrlString("aĀb");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1717:   217	            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1719:   219	            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1724:   224	        public void HexStringToBase64Url_UpstreamVectors()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1726:   226	            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1727:   227	            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1728:   228	            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1729:   229	            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1733:   233	        public void HexStringToBase64Url_TooLong_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1735:   235	            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1743:   243	            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1744:   244	            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1745:   245	            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1752:   252	            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1758:   258	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1762:   262	            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1767:   267	        // PointEncode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1771:   271	        public void PointEncode_EvenY_PrefixIs02()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1779:   279	            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1787:   287	        public void PointEncode_OddY_PrefixIs03()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1795:   295	            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1801:   301	        public void PointEncode_InvalidPrefix_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1805:   305	            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1811:   311	        public void PointEncode_WrongLength_Throws()
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
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1883:/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '380,520p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1886:   381	        public void Base58CheckDecode_TooShort_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1890:   385	            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1892:   387	               .WithMessage("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1900:   395	        public void Uint8ArrayToString_DecodesUtf8()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1902:   397	            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1903:   398	            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1907:   402	        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1912:   407	            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1972:    53	  const uint8Array = new Uint8Array([
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1978:    59	  expect(uint8ArrayToHexString(uint8Array)).toBe(expectedHexString); // Uint8Array => Hex string
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1986:    67	  const expectedUint8Array = new Uint8Array([
codex-crypto-reviews/Encoding.cs-r1-20260523.md:1990:    71	  expect(uint8ArrayFromHexString(hexString)).toEqual(expectedUint8Array); // Hex string => Uint8Array
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2020:   101	  const expectedUint8Array2 = new Uint8Array([
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2024:   105	  expect(uint8ArrayFromHexString(hexString2, 32)).toEqual(expectedUint8Array2); // Hex string => Uint8Array
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2050:I’ve confirmed the pinned upstream package files are local and the C# file is at the requested commit. One thing to note early: this repo currently has `tests/EncodingTests.cs`, but no `tests/Fixtures/` directory, so the fixture gate will need to distinguish inline upstream vectors from fixture files.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2052:/bin/zsh -lc "nl -ba codex-crypto-reviews/Encoding.cs-r1-20260523.md | sed -n '1,240p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2054:     1	# Codex review — src/Encoding.cs — round 1 — 20260523
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2075:    22	File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2087:    34	B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2304:codex-crypto-reviews/Encoding.cs-r1-20260523.md
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2317:/bin/zsh -lc "rg -n \"Base58|bs58|base64|Uint8ArrayToString|ConcatUint8Arrays|Encoding\\.\" src tests codex-crypto-reviews -g '"'!**/bin/**'"' -g '"'!**/obj/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2319:tests/EncodingTests.cs:1:// EncodingTests.cs — xunit tests for src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2320:tests/EncodingTests.cs:33:            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2321:tests/EncodingTests.cs:39:            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2322:tests/EncodingTests.cs:45:            Action act = () => Encoding.Uint8ArrayToHexString(null!);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2323:tests/EncodingTests.cs:53:            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2324:tests/EncodingTests.cs:67:            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2325:tests/EncodingTests.cs:75:            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2326:tests/EncodingTests.cs:84:            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2327:tests/EncodingTests.cs:93:            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2328:tests/EncodingTests.cs:99:            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2329:tests/EncodingTests.cs:113:            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2330:tests/EncodingTests.cs:122:            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2331:tests/EncodingTests.cs:131:            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2332:tests/EncodingTests.cs:140:            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2333:tests/EncodingTests.cs:147:            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2334:tests/EncodingTests.cs:154:            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2335:tests/EncodingTests.cs:161:            Action act = () => Encoding.NormalizePadding(input, 2);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2336:tests/EncodingTests.cs:170:            Encoding.NormalizePadding(input, 2).Should().Equal(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2337:tests/EncodingTests.cs:174:        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2338:tests/EncodingTests.cs:180:            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2339:tests/EncodingTests.cs:191:            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2340:tests/EncodingTests.cs:203:            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2341:tests/EncodingTests.cs:209:            Action act = () => Encoding.StringToBase64UrlString("aĀb");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2342:tests/EncodingTests.cs:217:            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2343:tests/EncodingTests.cs:219:            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2344:tests/EncodingTests.cs:226:            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2345:tests/EncodingTests.cs:227:            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2346:tests/EncodingTests.cs:228:            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2347:tests/EncodingTests.cs:229:            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2348:tests/EncodingTests.cs:235:            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2349:tests/EncodingTests.cs:243:            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2350:tests/EncodingTests.cs:244:            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2351:tests/EncodingTests.cs:245:            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2352:tests/EncodingTests.cs:252:            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2353:tests/EncodingTests.cs:258:            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2354:tests/EncodingTests.cs:262:            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2355:tests/EncodingTests.cs:279:            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2356:tests/EncodingTests.cs:295:            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2357:tests/EncodingTests.cs:305:            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2358:tests/EncodingTests.cs:315:            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2359:tests/EncodingTests.cs:321:        // Base58 / Base58Check
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2360:tests/EncodingTests.cs:325:        public void Base58Encode_KnownVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2361:tests/EncodingTests.cs:329:            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2362:tests/EncodingTests.cs:330:            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2363:tests/EncodingTests.cs:334:        public void Base58Decode_KnownVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2364:tests/EncodingTests.cs:336:            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2365:tests/EncodingTests.cs:337:            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2366:tests/EncodingTests.cs:341:        public void Base58_Roundtrip_PreservesLeadingZero()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2367:tests/EncodingTests.cs:344:            string encoded = Encoding.Base58Encode(data);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2368:tests/EncodingTests.cs:346:            byte[] decoded = Encoding.Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2369:tests/EncodingTests.cs:351:        public void Base58_Empty()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2370:tests/EncodingTests.cs:353:            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2371:tests/EncodingTests.cs:354:            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2372:tests/EncodingTests.cs:358:        public void Base58Check_Roundtrip()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2373:tests/EncodingTests.cs:360:            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2374:tests/EncodingTests.cs:361:            string encoded = Encoding.Base58CheckEncode(payload);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2375:tests/EncodingTests.cs:362:            byte[] decoded = Encoding.Base58CheckDecode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2376:tests/EncodingTests.cs:367:        public void Base58CheckDecode_TamperedChecksum_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2377:tests/EncodingTests.cs:369:            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2378:tests/EncodingTests.cs:370:            string encoded = Encoding.Base58CheckEncode(payload);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2379:tests/EncodingTests.cs:376:            Action act = () => Encoding.Base58CheckDecode(tampered);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2380:tests/EncodingTests.cs:381:        public void Base58CheckDecode_TooShort_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2381:tests/EncodingTests.cs:385:            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2382:tests/EncodingTests.cs:387:               .WithMessage("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2383:tests/EncodingTests.cs:395:        public void Uint8ArrayToString_DecodesUtf8()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2384:tests/EncodingTests.cs:397:            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2385:tests/EncodingTests.cs:398:            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2386:tests/EncodingTests.cs:402:        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2387:tests/EncodingTests.cs:407:            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2388:src/Encoding.cs:9://   ts-source/base64.ts       -> StringToBase64UrlString /
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2389:src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2390:src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2391:src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2392:src/Encoding.cs:30://                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2393:src/Encoding.cs:34://   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2394:src/Encoding.cs:35://                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2395:src/Encoding.cs:36://   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2396:src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2397:src/Encoding.cs:72:            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2398:src/Encoding.cs:221:        // base64.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2399:src/Encoding.cs:225:        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2400:src/Encoding.cs:226:        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2401:src/Encoding.cs:259:        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2402:src/Encoding.cs:260:        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2403:src/Encoding.cs:285:        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2404:src/Encoding.cs:287:        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2405:src/Encoding.cs:299:        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2406:src/Encoding.cs:301:        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2407:src/Encoding.cs:315:        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2408:src/Encoding.cs:316:        /// restoring base64 padding and then base64-decoding into a byte buffer.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2409:src/Encoding.cs:318:        /// Upstream: <c>base64.ts decodeBase64urlToString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2410:src/Encoding.cs:367:        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2411:src/Encoding.cs:369:        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2412:src/Encoding.cs:376:        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2413:src/Encoding.cs:377:        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2414:src/Encoding.cs:379:        public static string Base58Encode(byte[] data)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2415:src/Encoding.cs:401:            // Preserve leading zero bytes as '1' characters (Base58 convention).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2416:src/Encoding.cs:411:        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2417:src/Encoding.cs:412:        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2418:src/Encoding.cs:414:        public static byte[] Base58Decode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2419:src/Encoding.cs:460:        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2420:src/Encoding.cs:461:        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2421:src/Encoding.cs:463:        public static string Base58CheckEncode(byte[] payload)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2422:src/Encoding.cs:478:                return Base58Encode(concat);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2423:src/Encoding.cs:483:        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2424:src/Encoding.cs:485:        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2425:src/Encoding.cs:487:        public static byte[] Base58CheckDecode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2426:src/Encoding.cs:489:            byte[] decoded = Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2427:src/Encoding.cs:492:                throw new ArgumentException("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2428:src/Encoding.cs:507:                    throw new ArgumentException("Invalid Base58Check checksum");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2429:src/Encoding.cs:531:        public static string Uint8ArrayToString(byte[] bytes)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2430:src/Encoding.cs:537:            return System.Text.Encoding.UTF8.GetString(bytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2431:src/Encoding.cs:548:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2432:codex-crypto-reviews/unity-source-pins.md:38:| src/Encoding.cs             | packages/turnkey-sdk-unity/Runtime/Encoding.cs             |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2433:codex-crypto-reviews/turnkey-source-pins.md:12:| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2434:codex-crypto-reviews/README.md:10:1. `Encoding.cs`
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2435:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1:# Codex review — src/Encoding.cs — round 1 — 20260523
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2436:codex-crypto-reviews/Encoding.cs-r1-20260523.md:22:File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2437:codex-crypto-reviews/Encoding.cs-r1-20260523.md:34:B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2438:codex-crypto-reviews/Encoding.cs-r1-20260523.md:298:/bin/zsh -lc 'rg -n "turnkey-sdk-csharp|turnkey|Encoding.cs|C# SDK" /Users/takeshi/.codex/memories/MEMORY.md' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2439:codex-crypto-reviews/Encoding.cs-r1-20260523.md:311:src/Encoding.cs:1:// 1:1 logical port of @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2440:codex-crypto-reviews/Encoding.cs-r1-20260523.md:312:src/Encoding.cs:4://   codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2441:codex-crypto-reviews/Encoding.cs-r1-20260523.md:313:src/Encoding.cs:50:    /// 1:1 logical port of <c>@turnkey/encoding</c> v0.6.0.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2442:codex-crypto-reviews/Encoding.cs-r1-20260523.md:314:src/Encoding.cs:55:        /// Internal constants exported by <c>@turnkey/encoding</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2443:codex-crypto-reviews/Encoding.cs-r1-20260523.md:315:src/Encoding.cs:61:            /// Source: <c>turnkey-encoding-0.6.0/ts-source/index.ts</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2444:codex-crypto-reviews/Encoding.cs-r1-20260523.md:316:src/Encoding.cs:515:        // Unity-port-derived helpers (NOT in @turnkey/encoding)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2445:codex-crypto-reviews/Encoding.cs-r1-20260523.md:325:/bin/zsh -lc "git status --short && git log --oneline -5 -- src/Encoding.cs && nl -ba src/Encoding.cs | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2446:codex-crypto-reviews/Encoding.cs-r1-20260523.md:333:?? codex-crypto-reviews/Encoding.cs-r1-20260523.md
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2447:codex-crypto-reviews/Encoding.cs-r1-20260523.md:342:0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2448:codex-crypto-reviews/Encoding.cs-r1-20260523.md:351:     9	//   ts-source/base64.ts       -> StringToBase64UrlString /
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2449:codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2450:codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2451:codex-crypto-reviews/Encoding.cs-r1-20260523.md:370:    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2452:codex-crypto-reviews/Encoding.cs-r1-20260523.md:372:    30	//                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2453:codex-crypto-reviews/Encoding.cs-r1-20260523.md:376:    34	//   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2454:codex-crypto-reviews/Encoding.cs-r1-20260523.md:377:    35	//                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2455:codex-crypto-reviews/Encoding.cs-r1-20260523.md:378:    36	//   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2456:codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2457:codex-crypto-reviews/Encoding.cs-r1-20260523.md:414:    72	            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2458:codex-crypto-reviews/Encoding.cs-r1-20260523.md:563:   221	        // base64.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2459:codex-crypto-reviews/Encoding.cs-r1-20260523.md:567:   225	        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2460:codex-crypto-reviews/Encoding.cs-r1-20260523.md:568:   226	        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2461:codex-crypto-reviews/Encoding.cs-r1-20260523.md:601:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2462:codex-crypto-reviews/Encoding.cs-r1-20260523.md:602:   260	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2463:codex-crypto-reviews/Encoding.cs-r1-20260523.md:635:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2464:codex-crypto-reviews/Encoding.cs-r1-20260523.md:636:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2465:codex-crypto-reviews/Encoding.cs-r1-20260523.md:637:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2466:codex-crypto-reviews/Encoding.cs-r1-20260523.md:638:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2467:codex-crypto-reviews/Encoding.cs-r1-20260523.md:639:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2468:codex-crypto-reviews/Encoding.cs-r1-20260523.md:640:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs.map
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
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2481:codex-crypto-reviews/Encoding.cs-r1-20260523.md:673:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2482:codex-crypto-reviews/Encoding.cs-r1-20260523.md:674:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2483:codex-crypto-reviews/Encoding.cs-r1-20260523.md:675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2484:codex-crypto-reviews/Encoding.cs-r1-20260523.md:745:/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '235,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2485:codex-crypto-reviews/Encoding.cs-r1-20260523.md:771:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2486:codex-crypto-reviews/Encoding.cs-r1-20260523.md:772:   260	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2487:codex-crypto-reviews/Encoding.cs-r1-20260523.md:797:   285	        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2488:codex-crypto-reviews/Encoding.cs-r1-20260523.md:799:   287	        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2489:codex-crypto-reviews/Encoding.cs-r1-20260523.md:811:   299	        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2490:codex-crypto-reviews/Encoding.cs-r1-20260523.md:813:   301	        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2491:codex-crypto-reviews/Encoding.cs-r1-20260523.md:827:   315	        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2492:codex-crypto-reviews/Encoding.cs-r1-20260523.md:828:   316	        /// restoring base64 padding and then base64-decoding into a byte buffer.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2493:codex-crypto-reviews/Encoding.cs-r1-20260523.md:830:   318	        /// Upstream: <c>base64.ts decodeBase64urlToString</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2494:codex-crypto-reviews/Encoding.cs-r1-20260523.md:879:   367	        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2495:codex-crypto-reviews/Encoding.cs-r1-20260523.md:881:   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2496:codex-crypto-reviews/Encoding.cs-r1-20260523.md:888:   376	        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2497:codex-crypto-reviews/Encoding.cs-r1-20260523.md:889:   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2498:codex-crypto-reviews/Encoding.cs-r1-20260523.md:891:   379	        public static string Base58Encode(byte[] data)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2499:codex-crypto-reviews/Encoding.cs-r1-20260523.md:913:   401	            // Preserve leading zero bytes as '1' characters (Base58 convention).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2500:codex-crypto-reviews/Encoding.cs-r1-20260523.md:923:   411	        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2501:codex-crypto-reviews/Encoding.cs-r1-20260523.md:924:   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2502:codex-crypto-reviews/Encoding.cs-r1-20260523.md:926:   414	        public static byte[] Base58Decode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2503:codex-crypto-reviews/Encoding.cs-r1-20260523.md:972:   460	        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2504:codex-crypto-reviews/Encoding.cs-r1-20260523.md:973:   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2505:codex-crypto-reviews/Encoding.cs-r1-20260523.md:975:   463	        public static string Base58CheckEncode(byte[] payload)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2506:codex-crypto-reviews/Encoding.cs-r1-20260523.md:990:   478	                return Base58Encode(concat);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2507:codex-crypto-reviews/Encoding.cs-r1-20260523.md:995:   483	        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2508:codex-crypto-reviews/Encoding.cs-r1-20260523.md:997:   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2509:codex-crypto-reviews/Encoding.cs-r1-20260523.md:999:   487	        public static byte[] Base58CheckDecode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2510:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1001:   489	            byte[] decoded = Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2511:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1004:   492	                throw new ArgumentException("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2512:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1019:   507	                    throw new ArgumentException("Invalid Base58Check checksum");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2513:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1043:   531	        public static string Uint8ArrayToString(byte[] bytes)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2514:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1049:   537	            return System.Text.Encoding.UTF8.GetString(bytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2515:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1060:   548	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2516:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1083:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2517:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1189:     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2518:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1195:     8	 * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2519:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1198:    11	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2520:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1201:    14	  // string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2521:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1203:    16	  const base64String = btoa(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2522:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1204:    17	  return base64StringToBase64UrlEncodedString(base64String);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2523:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1208:    21	 * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2524:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1212:    25	 * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2525:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1227:    40	 * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2526:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1229:    42	 * @param {string} input - The input base64 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2527:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1230:    43	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2528:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1232:    45	export function base64StringToBase64UrlEncodedString(input: string): string {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2529:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1237:    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2530:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1239:    52	 * - Replaces URL-safe characters (`-` and `_`) back to standard base64 characters (`+` and `/`).
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2531:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1242:    55	 * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2532:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1243:    56	 * @returns {string} - The equivalent base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2533:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1245:    58	export function base64UrlToBase64(input: string): string {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2534:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1252:    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2535:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1254:    67	 * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2536:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1255:    68	 * - Decodes the base64 string into a plain string using a pure JS `atob` implementation.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2537:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1257:    70	 * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2538:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1259:    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2539:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1262:    75	  const b64 = base64UrlToBase64(input);
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
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2555:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1491:     3	export * from "./base64";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2556:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1495:     7	export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2557:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1496:     8	export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2558:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1501:     1	// EncodingTests.cs — xunit tests for src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2559:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1533:    33	            Encoding.Uint8ArrayToHexString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2560:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1539:    39	            Encoding.Uint8ArrayToHexString(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2561:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1545:    45	            Action act = () => Encoding.Uint8ArrayToHexString(null!);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2562:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1553:    53	            Encoding.Uint8ArrayToHexString(input).Should().Be("00000102");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2563:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1567:    67	            Encoding.Uint8ArrayFromHexString(hex).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2564:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1575:    75	            Encoding.Uint8ArrayFromHexString("627566666572").Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2565:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1584:    84	            Action act = () => Encoding.Uint8ArrayFromHexString(hex);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2566:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1593:    93	            Encoding.Uint8ArrayFromHexString("01", 2).Should().Equal(new byte[] { 0, 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2567:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1599:    99	            Encoding.Uint8ArrayFromHexString("01").Should().Equal(new byte[] { 1 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2568:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1613:   113	            Encoding.Uint8ArrayFromHexString(hex, 32).Should().Equal(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2569:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1622:   122	            Action act = () => Encoding.Uint8ArrayFromHexString(hex, 32);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2570:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1631:   131	            Action act = () => Encoding.Uint8ArrayFromHexString("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2571:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1640:   140	            Encoding.HexToAscii(asciiHex).Should().Be("buffer");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2572:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1647:   147	            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2573:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1654:   154	            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2574:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1661:   161	            Action act = () => Encoding.NormalizePadding(input, 2);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2575:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1670:   170	            Encoding.NormalizePadding(input, 2).Should().Equal(input);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2576:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1674:   174	        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2577:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1680:   180	            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2578:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1691:   191	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2579:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1703:   203	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2580:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1709:   209	            Action act = () => Encoding.StringToBase64UrlString("aĀb");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2581:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1717:   217	            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2582:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1719:   219	            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2583:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1726:   226	            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2584:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1727:   227	            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2585:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1728:   228	            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2586:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1729:   229	            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2587:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1735:   235	            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2588:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1743:   243	            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2589:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1744:   244	            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2590:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1745:   245	            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2591:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1752:   252	            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2592:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1758:   258	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2593:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1762:   262	            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2594:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1779:   279	            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2595:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1795:   295	            var compressed = Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2596:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1805:   305	            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2597:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1815:   315	            Action act = () => Encoding.PointEncode(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2598:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1821:   321	        // Base58 / Base58Check
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2599:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1825:   325	        public void Base58Encode_KnownVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2600:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1829:   329	            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2601:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1830:   330	            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2602:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1834:   334	        public void Base58Decode_KnownVector()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2603:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1836:   336	            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2604:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1837:   337	            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2605:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1841:   341	        public void Base58_Roundtrip_PreservesLeadingZero()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2606:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1844:   344	            string encoded = Encoding.Base58Encode(data);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2607:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1846:   346	            byte[] decoded = Encoding.Base58Decode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2608:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1851:   351	        public void Base58_Empty()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2609:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1853:   353	            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2610:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1854:   354	            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2611:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1858:   358	        public void Base58Check_Roundtrip()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2612:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1860:   360	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2613:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1861:   361	            string encoded = Encoding.Base58CheckEncode(payload);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2614:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1862:   362	            byte[] decoded = Encoding.Base58CheckDecode(encoded);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2615:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1867:   367	        public void Base58CheckDecode_TamperedChecksum_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2616:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1869:   369	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2617:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1870:   370	            string encoded = Encoding.Base58CheckEncode(payload);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2618:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1876:   376	            Action act = () => Encoding.Base58CheckDecode(tampered);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2619:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1886:   381	        public void Base58CheckDecode_TooShort_Throws()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2620:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1890:   385	            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2621:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1892:   387	               .WithMessage("Invalid Base58Check string - too short");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2622:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1900:   395	        public void Uint8ArrayToString_DecodesUtf8()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2623:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1902:   397	            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2624:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1903:   398	            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2625:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1907:   402	        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2626:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1912:   407	            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2627:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1925:     6	  base64StringToBase64UrlEncodedString,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2628:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1932:    13	//   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2629:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1957:    38	// Test for base64StringToBase64UrlEncodedString
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2630:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1958:    39	test("base64StringToBase64UrlEncodedString", async function () {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2631:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1960:    41	  expect(base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")).toBe(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2632:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1965:    46	  expect(base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")).toBe(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2633:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2052:/bin/zsh -lc "nl -ba codex-crypto-reviews/Encoding.cs-r1-20260523.md | sed -n '1,240p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2634:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2054:     1	# Codex review — src/Encoding.cs — round 1 — 20260523
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2635:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2075:    22	File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 0ffef786d2d8ff119e598316cb5aa93b98562d65)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2636:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2087:    34	B. Method coverage table: every public + internal helper method in src/Encoding.cs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2838:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2839:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2840:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2841:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2941:  "description": "Encoding utility functions",
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2977:/bin/zsh -lc "rg -n \"public static|internal static|private static|static .*\\(\" src/Encoding.cs" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2979:52:    public static class Encoding
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2982:90:        public static string Uint8ArrayToHexString(byte[] input)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2983:118:        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2987:262:        public static string HexStringToBase64Url(string input, int? length = null)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2991:345:        public static byte[] PointEncode(byte[] raw)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2992:379:        public static string Base58Encode(byte[] data)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2993:414:        public static byte[] Base58Decode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2994:463:        public static string Base58CheckEncode(byte[] payload)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2995:487:        public static byte[] Base58CheckDecode(string encoded)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2996:531:        public static string Uint8ArrayToString(byte[] bytes)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:2997:548:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3028:    12	| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3071:src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3072:src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3073:src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3074:src/Encoding.cs:30://                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3075:src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3076:src/Encoding.cs:225:        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3077:src/Encoding.cs:259:        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3078:src/Encoding.cs:285:        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3079:src/Encoding.cs:299:        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3080:src/Encoding.cs:315:        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3081:src/Encoding.cs:367:        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3082:src/Encoding.cs:369:        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3083:src/Encoding.cs:377:        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3084:src/Encoding.cs:412:        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3085:src/Encoding.cs:461:        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3086:src/Encoding.cs:485:        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3091:tests/EncodingTests.cs:174:        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3109:codex-crypto-reviews/Encoding.cs-r1-20260523.md:58:F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3110:codex-crypto-reviews/Encoding.cs-r1-20260523.md:59:   this file, confirm it was generated from the pinned upstream package and the
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3111:codex-crypto-reviews/Encoding.cs-r1-20260523.md:60:   C# test asserts the same bytes Node would produce.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3112:codex-crypto-reviews/Encoding.cs-r1-20260523.md:66:entries, F is "all fixtures match".
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3113:codex-crypto-reviews/Encoding.cs-r1-20260523.md:72:I’ll verify the C# file against the pinned local TypeScript snapshot and use the review workflow since this is explicitly a code review. First I’m gathering the file, upstream source, checksums, fixtures, and any relevant prior context.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3114:codex-crypto-reviews/Encoding.cs-r1-20260523.md:277:Node project - Difficult task:
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3115:codex-crypto-reviews/Encoding.cs-r1-20260523.md:281:Node project - Normal task:
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3116:codex-crypto-reviews/Encoding.cs-r1-20260523.md:304:/bin/zsh -lc "rg -n \"turnkey-encoding|sha256|encoding\" codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 tests/Fixtures tests src -g '"'!bin'"' -g '"'!obj'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3117:codex-crypto-reviews/Encoding.cs-r1-20260523.md:306:rg: tests/Fixtures: No such file or directory (os error 2)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3118:codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3119:codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3120:codex-crypto-reviews/Encoding.cs-r1-20260523.md:370:    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3121:codex-crypto-reviews/Encoding.cs-r1-20260523.md:372:    30	//                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3122:codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3123:codex-crypto-reviews/Encoding.cs-r1-20260523.md:567:   225	        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3124:codex-crypto-reviews/Encoding.cs-r1-20260523.md:601:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3125:codex-crypto-reviews/Encoding.cs-r1-20260523.md:641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3126:codex-crypto-reviews/Encoding.cs-r1-20260523.md:642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3127:codex-crypto-reviews/Encoding.cs-r1-20260523.md:643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3128:codex-crypto-reviews/Encoding.cs-r1-20260523.md:644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3129:codex-crypto-reviews/Encoding.cs-r1-20260523.md:645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3130:codex-crypto-reviews/Encoding.cs-r1-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3131:codex-crypto-reviews/Encoding.cs-r1-20260523.md:647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3132:codex-crypto-reviews/Encoding.cs-r1-20260523.md:648:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3133:codex-crypto-reviews/Encoding.cs-r1-20260523.md:649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3134:codex-crypto-reviews/Encoding.cs-r1-20260523.md:650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3135:codex-crypto-reviews/Encoding.cs-r1-20260523.md:651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3136:codex-crypto-reviews/Encoding.cs-r1-20260523.md:652:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3137:codex-crypto-reviews/Encoding.cs-r1-20260523.md:674:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3138:codex-crypto-reviews/Encoding.cs-r1-20260523.md:675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3139:codex-crypto-reviews/Encoding.cs-r1-20260523.md:681:/bin/zsh -lc "find tests -maxdepth 4 -type f | sort && find . -maxdepth 4 -type d -name 'Fixtures' -print" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3140:codex-crypto-reviews/Encoding.cs-r1-20260523.md:771:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3141:codex-crypto-reviews/Encoding.cs-r1-20260523.md:797:   285	        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3142:codex-crypto-reviews/Encoding.cs-r1-20260523.md:811:   299	        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3143:codex-crypto-reviews/Encoding.cs-r1-20260523.md:827:   315	        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3144:codex-crypto-reviews/Encoding.cs-r1-20260523.md:879:   367	        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3145:codex-crypto-reviews/Encoding.cs-r1-20260523.md:881:   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3146:codex-crypto-reviews/Encoding.cs-r1-20260523.md:889:   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3147:codex-crypto-reviews/Encoding.cs-r1-20260523.md:924:   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3148:codex-crypto-reviews/Encoding.cs-r1-20260523.md:973:   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3149:codex-crypto-reviews/Encoding.cs-r1-20260523.md:997:   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3150:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1189:     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3151:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1195:     8	 * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3152:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1198:    11	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3153:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1208:    21	 * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3154:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1212:    25	 * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3155:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1227:    40	 * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3156:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1230:    43	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3157:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1237:    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3158:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1242:    55	 * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3159:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1252:    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3160:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1254:    67	 * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3161:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1257:    70	 * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3162:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1259:    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3163:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1389:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3164:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1391:     1	// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3165:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1393:     3	// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3166:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1395:     5	// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3167:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1397:     7	// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3168:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1403:    13	// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3169:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1406:    16	import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3170:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1426:    36	export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3171:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1427:     1	// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3172:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1429:     3	// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3173:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1431:     5	// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3174:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1433:     7	// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3175:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1439:    13	// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3176:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1442:    16	import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3177:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1462:    36	export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3178:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1495:     7	export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3179:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1496:     8	export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3180:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1674:   174	        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3181:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1930:    11	// These test vectors can be verified with NodeJS:
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3182:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1932:    13	//   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3183:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2050:I’ve confirmed the pinned upstream package files are local and the C# file is at the requested commit. One thing to note early: this repo currently has `tests/EncodingTests.cs`, but no `tests/Fixtures/` directory, so the fixture gate will need to distinguish inline upstream vectors from fixture files.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3184:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2111:    58	F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3185:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2112:    59	   this file, confirm it was generated from the pinned upstream package and the
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3186:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2113:    60	   C# test asserts the same bytes Node would produce.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3187:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2119:    66	entries, F is "all fixtures match".
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3188:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2125:    72	I’ll verify the C# file against the pinned local TypeScript snapshot and use the review workflow since this is explicitly a code review. First I’m gathering the file, upstream source, checksums, fixtures, and any relevant prior context.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3189:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2296:/bin/zsh -lc "find . -path '*/Fixtures/*' -o -path '*/fixtures/*' -o -name '*fixture*' -o -name '*Fixture*' | sort" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3190:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2298:./codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__fixtures__
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3191:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2299:./codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__fixtures__
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3192:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2317:/bin/zsh -lc "rg -n \"Base58|bs58|base64|Uint8ArrayToString|ConcatUint8Arrays|Encoding\\.\" src tests codex-crypto-reviews -g '"'!**/bin/**'"' -g '"'!**/obj/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3193:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2337:tests/EncodingTests.cs:174:        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3194:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2389:src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3195:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2390:src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3196:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2391:src/Encoding.cs:28://   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3197:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2392:src/Encoding.cs:30://                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3198:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2396:src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3199:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2399:src/Encoding.cs:225:        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3200:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2401:src/Encoding.cs:259:        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3201:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2403:src/Encoding.cs:285:        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3202:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2405:src/Encoding.cs:299:        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3203:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2407:src/Encoding.cs:315:        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3204:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2410:src/Encoding.cs:367:        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3205:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2411:src/Encoding.cs:369:        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3206:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2413:src/Encoding.cs:377:        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3207:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2417:src/Encoding.cs:412:        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3208:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2420:src/Encoding.cs:461:        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3209:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2424:src/Encoding.cs:485:        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3210:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2449:codex-crypto-reviews/Encoding.cs-r1-20260523.md:357:    15	//   ts-source/bs58.ts         -> Base58Encode / Base58Decode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3211:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2450:codex-crypto-reviews/Encoding.cs-r1-20260523.md:358:    16	//   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3212:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2451:codex-crypto-reviews/Encoding.cs-r1-20260523.md:370:    28	//   bs58 npm package    -> BouncyCastle BigInteger-based base58 implementation
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3213:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2452:codex-crypto-reviews/Encoding.cs-r1-20260523.md:372:    30	//                            input as the bs58 npm package)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3214:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2456:codex-crypto-reviews/Encoding.cs-r1-20260523.md:408:    66	            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3215:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2459:codex-crypto-reviews/Encoding.cs-r1-20260523.md:567:   225	        /// Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3216:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2461:codex-crypto-reviews/Encoding.cs-r1-20260523.md:601:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3217:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2469:codex-crypto-reviews/Encoding.cs-r1-20260523.md:641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3218:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2470:codex-crypto-reviews/Encoding.cs-r1-20260523.md:642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3219:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2471:codex-crypto-reviews/Encoding.cs-r1-20260523.md:643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3220:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2472:codex-crypto-reviews/Encoding.cs-r1-20260523.md:644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3221:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2473:codex-crypto-reviews/Encoding.cs-r1-20260523.md:645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3222:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2474:codex-crypto-reviews/Encoding.cs-r1-20260523.md:646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3223:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2475:codex-crypto-reviews/Encoding.cs-r1-20260523.md:647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3224:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2476:codex-crypto-reviews/Encoding.cs-r1-20260523.md:648:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3225:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2477:codex-crypto-reviews/Encoding.cs-r1-20260523.md:649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3226:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2478:codex-crypto-reviews/Encoding.cs-r1-20260523.md:650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3227:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2479:codex-crypto-reviews/Encoding.cs-r1-20260523.md:651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3228:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2480:codex-crypto-reviews/Encoding.cs-r1-20260523.md:652:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3229:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2482:codex-crypto-reviews/Encoding.cs-r1-20260523.md:674:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3230:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2483:codex-crypto-reviews/Encoding.cs-r1-20260523.md:675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3231:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2485:codex-crypto-reviews/Encoding.cs-r1-20260523.md:771:   259	        /// Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3232:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2487:codex-crypto-reviews/Encoding.cs-r1-20260523.md:797:   285	        /// Converts a standard base64 string into a base64url-encoded string by
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3233:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2489:codex-crypto-reviews/Encoding.cs-r1-20260523.md:811:   299	        /// Converts a base64url string into a standard base64 string (with
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3234:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2491:codex-crypto-reviews/Encoding.cs-r1-20260523.md:827:   315	        /// Decodes a base64url-encoded string into a plain string by first
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3235:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2494:codex-crypto-reviews/Encoding.cs-r1-20260523.md:879:   367	        // bs58.ts (and bs58check.ts)
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3236:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2495:codex-crypto-reviews/Encoding.cs-r1-20260523.md:881:   369	        // Upstream imports the `bs58` and `bs58check` npm packages. This port
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3237:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2497:codex-crypto-reviews/Encoding.cs-r1-20260523.md:889:   377	        /// Upstream: <c>bs58.ts bs58.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3238:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2501:codex-crypto-reviews/Encoding.cs-r1-20260523.md:924:   412	        /// Upstream: <c>bs58.ts bs58.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3239:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2504:codex-crypto-reviews/Encoding.cs-r1-20260523.md:973:   461	        /// Upstream: <c>bs58check.ts bs58check.encode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3240:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2508:codex-crypto-reviews/Encoding.cs-r1-20260523.md:997:   485	        /// Upstream: <c>bs58check.ts bs58check.decode</c>.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3241:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2517:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1189:     2	 * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3242:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2518:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1195:     8	 * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3243:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2519:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1198:    11	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3244:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2523:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1208:    21	 * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3245:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2524:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1212:    25	 * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3246:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2525:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1227:    40	 * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3247:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2527:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1230:    43	 * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3248:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2529:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1237:    50	 * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3249:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2531:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1242:    55	 * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3250:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2534:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1252:    65	 * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3251:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2535:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1254:    67	 * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3252:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2537:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1257:    70	 * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3253:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2538:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1259:    72	 * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3254:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2540:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1389:/bin/zsh -lc 'nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts && nl -ba codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3255:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2541:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1391:     1	// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3256:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2542:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1393:     3	// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3257:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2543:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1395:     5	// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3258:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2544:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1397:     7	// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3259:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2545:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1403:    13	// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3260:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2546:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1406:    16	import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3261:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2547:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1426:    36	export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3262:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2548:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1427:     1	// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3263:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2549:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1429:     3	// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3264:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2550:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1431:     5	// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3265:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2551:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1433:     7	// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3266:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2552:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1439:    13	// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3267:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2553:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1442:    16	import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3268:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2554:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1462:    36	export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3269:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2556:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1495:     7	export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3270:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2557:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1496:     8	export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3271:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2576:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1674:   174	        // base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3272:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2628:codex-crypto-reviews/Encoding.cs-r1-20260523.md:1932:    13	//   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3273:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2637:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:38:    "bs58": "6.0.0",
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3274:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2638:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:39:    "bs58check": "4.0.0"
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3275:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2639:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:1:import * as raw from 'bs58';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3276:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2640:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:3:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3277:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2641:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:5:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3278:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2642:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:7:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3279:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2643:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:9:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3280:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2644:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:15:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3281:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2645:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:26:const bs58 = unwrap(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3282:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2646:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:28:export { bs58 };
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3283:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2647:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:29://# sourceMappingURL=bs58.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3284:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2649:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:5:export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3285:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2650:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.d.ts:6:export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3286:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2651:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs.map:1:{"version":3,"file":"bs58check.mjs","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3287:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2653:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js.map:1:{"version":3,"file":"bs58.js","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3288:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2654:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:7:- [#886](https://github.com/tkhq/sdk/pull/886) [`6bfcbc5`](https://github.com/tkhq/sdk/commit/6bfcbc5c098e64ab1d115518733b87cfc1653e17) Author [@moeodeh3](https://github.com/moeodeh3) - Expose `bs58` and `bs58check` shims for cross-platform usage.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3289:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2655:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:6:export declare const bs58: Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3290:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2656:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts:8://# sourceMappingURL=bs58.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3291:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2657:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:6:export declare const bs58check: Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3292:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2658:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts:8://# sourceMappingURL=bs58check.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3293:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2659:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:159:- [#720](https://github.com/tkhq/sdk/pull/720) [`6cbff7a`](https://github.com/tkhq/sdk/commit/6cbff7a0c0b3a9a05586399e5cef476154d3bdca) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptExportBundle` not working in some environments by adding a shim to handle `bs58`'s ESM-only export.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3294:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2660:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:167:- [#716](https://github.com/tkhq/sdk/pull/716) [`fa46701`](https://github.com/tkhq/sdk/commit/fa467019eef34b5199372248edff1e7a64934e79) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptCredentialBundle` not working in React Native by adding a shim to handle `bs58check`'s ESM-only export.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3295:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2661:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3296:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2662:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:5: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3297:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2663:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:8: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3298:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2664:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:12: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3299:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2665:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:16: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3300:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2666:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:21: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3301:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2668:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:24: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3302:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2670:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:28: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3303:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2672:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:33: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3304:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2675:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:38: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3305:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2676:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:40: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3306:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2678:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:43: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3307:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2679:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.d.ts:45: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3308:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2681:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:3:var raw = require('bs58check');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3309:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2682:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:24:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3310:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2683:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:26:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3311:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2684:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:28:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3312:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2685:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:30:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3313:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2686:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:36:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3314:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2687:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:47:const bs58check = unwrap(raw__namespace);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3315:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2688:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:49:exports.bs58check = bs58check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3316:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2689:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:50://# sourceMappingURL=bs58check.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3317:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2691:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:4:export { bs58 } from './bs58.mjs';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3318:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2692:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:5:export { bs58check } from './bs58check.mjs';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3319:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2693:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:4: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3320:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2694:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:7: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3321:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2695:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:10: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3322:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2699:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:19: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3323:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2700:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:23: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3324:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2701:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:34: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3325:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2703:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:37: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3326:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2705:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:43: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3327:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2707:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:48: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3328:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2710:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:57: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3329:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2711:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:59: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3330:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2713:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:62: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3331:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2714:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:64: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3332:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2718:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:2: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3333:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2719:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:8: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3334:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2720:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:11: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3335:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2724:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3336:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2725:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3337:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2726:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:40: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3338:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2728:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:43: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3339:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2730:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:50: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3340:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2732:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:55: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3341:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2735:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:65: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3342:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2736:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:67: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3343:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2738:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:70: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3344:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2739:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/base64.ts:72: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3345:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2741:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:3:var raw = require('bs58');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3346:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2742:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:24:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3347:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2743:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:26:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3348:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2744:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:28:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3349:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2745:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:30:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3350:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2746:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:36:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3351:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2747:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:47:const bs58 = unwrap(raw__namespace);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3352:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2748:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:49:exports.bs58 = bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3353:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2749:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:50://# sourceMappingURL=bs58.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3354:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2751:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:7:export { bs58 } from "./bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3355:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2752:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:8:export { bs58check } from "./bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3356:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2755:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:6:var bs58 = require('./bs58.js');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3357:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2756:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:7:var bs58check = require('./bs58check.js');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3358:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2763:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:22:exports.bs58 = bs58.bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3359:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2764:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:23:exports.bs58check = bs58check.bs58check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3360:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2765:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:1:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3361:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2766:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:3:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3362:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2767:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:5:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3363:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2768:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:7:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3364:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2769:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:13:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3365:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2770:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:16:import * as raw from "bs58check";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3366:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2771:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:36:export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3367:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2772:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:1:import * as raw from 'bs58check';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3368:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2773:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:3:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3369:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2774:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:5:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3370:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2775:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:7:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3371:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2776:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:9:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3372:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2777:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:15:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3373:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2778:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:26:const bs58check = unwrap(raw);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3374:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2779:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:28:export { bs58check };
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3375:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2780:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:29://# sourceMappingURL=bs58check.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3376:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2783:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts:13://   > Buffer.from("<input value>").toString("base64url")
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3377:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2788:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:3:import { bs58check } from "@turnkey/encoding";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3378:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2789:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:4:import { bs58 } from "@turnkey/encoding";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3379:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2790:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:77:    const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3380:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2791:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:179:      return bs58.encode(concatenatedBytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3381:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2792:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:287:      const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3382:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2794:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:542:  return bs58check.encode(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3383:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2795:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs.map:1:{"version":3,"file":"bs58.mjs","sources":["../src/bs58.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,IAAI,GAAG,MAAM,CAAC,GAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3384:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2796:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js.map:1:{"version":3,"file":"bs58check.js","sources":["../src/bs58check.ts"],"sourcesContent":[null],"names":["raw"],"mappings":";;;;;;;;;;;;;;;;;;;;;;;AAAA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AACA;AAUA,SAAS,MAAM,CAAC,GAAQ,EAAA;IACtB,IAAI,GAAG,GAAG,GAAG;AACb,IAAA,OACE,GAAG;AACH,QAAA,EAAE,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,MAAM,IAAI,GAAG,CAAC,YAAY,CAAC;QAC/C,GAAG,CAAC,OAAO,EACX;AACA,QAAA,GAAG,GAAG,GAAG,CAAC,OAAO;;AAEnB,IAAA,OAAO,GAAG;AACZ;MAEa,SAAS,GAAG,MAAM,CAACA,cAAG;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3385:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2797:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:1:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3386:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2798:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:3:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3387:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2799:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:5:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3388:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2800:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:7:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3389:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2801:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:13:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3390:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2802:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:16:import * as raw from "bs58";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3391:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2803:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:36:export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3392:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2804:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.d.ts.map:1:{"version":3,"file":"bs58check.d.ts","sourceRoot":"","sources":["../src/bs58check.ts"],"names":[],"mappings":"AAiBA,KAAK,SAAS,GAAG;IACf,MAAM,CAAC,OAAO,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC/C,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;IAChC,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;CACnD,CAAC;AAcF,eAAO,MAAM,SAAS,WAA2B,CAAC"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3393:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2805:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:6: * Code modified from https://github.com/github/webauthn-json/blob/e932b3585fa70b0bd5b5a4012ba7dbad7b0a0d0f/src/webauthn-json/base64url.ts#L23
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3394:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2806:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:9: * Converts a plain string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3395:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2807:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:12: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3396:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2811:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:21: * Converts a hex string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3397:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2812:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:25: * @returns {string} - The base64url-encoded representation of the hex string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3398:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2813:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:36: * Converts a base64 string into a base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3399:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2815:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:39: * @returns {string} - The base64url-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3400:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2817:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:45: * Converts a base64url-encoded string into a standard base64-encoded string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3401:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2819:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:50: * @param {string} input - The base64url-encoded string to convert.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3402:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2822:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:59: * Decodes a base64url-encoded string into a plain UTF-8 string.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3403:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2823:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:61: * - Converts the input from base64url to base64.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3404:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2825:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:64: * @param {string} input - The base64url-encoded string to decode.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3405:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2826:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:66: * @throws {Error} If the input is not correctly base64url/base64 encoded.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3406:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2831:codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.d.ts.map:1:{"version":3,"file":"bs58.d.ts","sourceRoot":"","sources":["../src/bs58.ts"],"names":[],"mappings":"AAiBA,KAAK,IAAI,GAAG;IACV,MAAM,CAAC,MAAM,EAAE,UAAU,GAAG,MAAM,EAAE,GAAG,MAAM,CAAC;IAC9C,YAAY,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,GAAG,SAAS,CAAC;IAClD,MAAM,CAAC,GAAG,EAAE,MAAM,GAAG,UAAU,CAAC;CACjC,CAAC;AAcF,eAAO,MAAM,IAAI,MAAsB,CAAC"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3407:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2832:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:30:        const bundleBytes = encoding.bs58check.decode(credentialBundle);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3408:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2833:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:98:            return encoding.bs58.encode(concatenatedBytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3409:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2834:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:181:            const decodedKeyBytes = encoding.bs58.decode(privateKey);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3410:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2836:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js:348:    return encoding.bs58check.encode(crypto.hpkeEncrypt({
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3411:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2837:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.js.map:1:{"version":3,"file":"turnkey.js","sources":["../src/turnkey.ts"],"sourcesContent":[null],"names":["Enclave","bs58check","uncompressRawPublicKey","hpkeDecrypt","uint8ArrayToHexString","uint8ArrayFromHexString","ed25519","bs58","hexToAscii","fromDerSignature","sha256","p256","PRODUCTION_SIGNER_SIGN_PUBLIC_KEY","hpkeEncrypt","formatHpkeBuf","PRODUCTION_NOTARIZER_SIGN_PUBLIC_KEY","quorumKeyEncrypt","PRODUCTION_TLS_FETCHER_ENCRYPT_PUBLIC_KEY","PRODUCTION_ON_RAMP_CREDENTIALS_ENCRYPTION_PUBLIC_KEY"],"mappings":";;;;;;;;;AAAA;AACA;AAsDYA;AAAZ,CAAA,UAAY,OAAO,EAAA;AACjB,IAAA,OAAA,CAAA,WAAA,CAAA,GAAA,WAAuB;AACvB,IAAA,OAAA,CAAA,QAAA,CAAA,GAAA,QAAiB;AACjB,IAAA,OAAA,CAAA,YAAA,CAAA,GAAA,YAAyB;AACzB,IAAA,OAAA,CAAA,aAAA,CAAA,GAAA,aAA2B;AAC3B,IAAA,OAAA,CAAA,KAAA,CAAA,GAAA,KAAW;AACb,CAAC,EANWA,eAAO,KAAPA,eAAO,GAAA,EAAA,CAAA,CAAA;AAQnB;;;;;;;AAOG;MACU,uBAAuB,GAAG,CACrC,gBAAwB,EACxB,WAAmB,KACT;AACV,IAAA,IAAI;QACF,MAAM,WAAW,GAAGC,kBAAS,CAAC,MAAM,CAAC,gBAAgB,CAAC;AACtD,QAAA,IAAI,WAAW,CAAC,UAAU,IAAI,EAAE,EAAE;YAChC,MAAM,IAAI,KAAK,CACb,CAAA,YAAA,EAAe,WAAW,CAAC,UAAU,CAAA,sFAAA,CAAwF,CAC9H;QACH;QAEA,MAAM,wBAAwB,GAAG,WAAW,CAAC,KAAK,CAAC,CAAC,EAAE,EAAE,CAAC;QACzD,MAAM,aAAa,GAAG,WAAW,CAAC,KAAK,CAAC,EAAE,CAAC;AAC3C,QAAA,MAAM,cAAc,GAAGC,6BAAsB,CAAC,wBAAwB,CAAC;QACvE,MAAM,aAAa,GAAGC,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,OAAOC,8BAAqB,CAAC,aAAa,CAAC;IAC7C;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,+BAA+B,KAAK,CAAA,CAAE,CAAC;IACzD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;MACU,mBAAmB,GAAG,OAAO,EACxC,YAAY,EACZ,WAAW,EACX,cAAc,EACd,kCAAkC,EAClC,SAAS,EACT,cAAc,GACY,KAAqB;AAC/C,IAAA,IAAI;QACF,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;AACnD,QAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;QACD,IAAI,CAAC,QAAQ,EAAE;AACb,YAAA,MAAM,IAAI,KAAK,CACb,uCAAuC,kBAAkB,CAAA,CAAE,CAC5D;QACH;QAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CACtBC,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CACjD,CACF;QAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,YAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;YACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;QACH;AACA,QAAA,IAAI,CAAC,UAAU,CAAC,cAAc,EAAE;AAC9B,YAAA,MAAM,IAAI,KAAK,CAAC,gDAAgD,CAAC;QACnE;QAEA,MAAM,cAAc,GAAGA,gCAAuB,CAAC,UAAU,CAAC,cAAc,CAAC;QACzE,MAAM,aAAa,GAAGA,gCAAuB,CAAC,UAAU,CAAC,UAAU,CAAC;QACpE,MAAM,aAAa,GAAGF,kBAAW,CAAC;YAChC,aAAa;YACb,cAAc;AACd,YAAA,YAAY,EAAE,WAAW;AAC1B,SAAA,CAAC;AAEF,QAAA,IAAI,SAAS,KAAK,QAAQ,IAAI,CAAC,cAAc,EAAE;AAC7C,YAAA,IAAI,aAAa,CAAC,MAAM,KAAK,EAAE,EAAE;gBAC/B,MAAM,IAAI,KAAK,CACb,CAAA,mDAAA,EAAsD,aAAa,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;YACA,MAAM,cAAc,GAAGG,eAAO,CAAC,YAAY,CAAC,aAAa,CAAC;AAC1D,YAAA,IAAI,cAAc,CAAC,MAAM,KAAK,EAAE,EAAE;gBAChC,MAAM,IAAI,KAAK,CACb,CAAA,kDAAA,EAAqD,cAAc,CAAC,MAAM,CAAA,CAAA,CAAG,CAC9E;YACH;AACA,YAAA,MAAM,iBAAiB,GAAG,IAAI,UAAU,CAAC,EAAE,CAAC;AAC5C,YAAA,iBAAiB,CAAC,GAAG,CAAC,aAAa,EAAE,CAAC,CAAC;AACvC,YAAA,iBAAiB,CAAC,GAAG,CAAC,cAAc,EAAE,EAAE,CAAC;AACzC,YAAA,OAAOC,aAAI,CAAC,MAAM,CAAC,iBAAiB,CAAC;QACvC;AAEA,QAAA,MAAM,gBAAgB,GAAGH,8BAAqB,CAAC,aAAa,CAAC;AAC7D,QAAA,OAAO,cAAc,GAAGI,mBAAU,CAAC,gBAAgB,CAAC,GAAG,gBAAgB;IACzE;IAAE,OAAO,KAAK,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,4BAA4B,KAAK,CAAA,CAAE,CAAC;IACtD;AACF;AAEA;;;;;;;;;;;;;;;;AAgBG;AACI,MAAM,oBAAoB,GAAG,OAClC,SAAiB,EACjB,SAAiB,EACjB,UAAkB,KACE;AACpB,IAAA,MAAM,eAAe,GAAGH,gCAAuB,CAAC,SAAS,CAAC;AAC1D,IAAA,MAAM,eAAe,GAAG,aAAa,CAAC,eAAe,CAAC;IACtD,IAAI,CAAC,eAAe,EAAE;AACpB,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,SAAS,CAAC;IACtD,MAAM,aAAa,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,UAAU,CAAC;AAC1D,IAAA,MAAM,UAAU,GAAGC,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,eAAe,CAAC,KAAK,EAAE,CAAC;AAC7E;AAEA;;;;;;;;AAQG;AACH,MAAM,sBAAsB,GAAG,OAC7B,mBAA2B,EAC3B,eAAuB,EACvB,UAAkB,EAClB,kCAA2C,KACvB;AACpB,IAAA,MAAM,uBAAuB,GAC3B,kCAAkC,IAAIC,2CAAiC;AACzE,IAAA,IAAI,mBAAmB,IAAI,uBAAuB,EAAE;QAClD,MAAM,IAAI,KAAK,CACb,CAAA,oBAAA,EACE,kCAAkC,IAAIA,2CACxC,CAAA,wCAAA,EAA2C,mBAAmB,CAAA,CAAE,CACjE;IACH;IAEA,MAAM,yBAAyB,GAAG,IAAI,UAAU,CAC9CP,gCAAuB,CAAC,mBAAmB,CAAC,CAC7C;AACD,IAAA,MAAM,SAAS,GAAG,aAAa,CAAC,yBAAyB,CAAC;IAC1D,IAAI,CAAC,SAAS,EAAE;AACd,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;IAC9C;;AAGA,IAAA,MAAM,kBAAkB,GAAGI,uBAAgB,CAAC,eAAe,CAAC;AAC5D,IAAA,MAAM,aAAa,GAAGJ,gCAAuB,CAAC,UAAU,CAAC;AACzD,IAAA,MAAM,UAAU,GAAGK,aAAM,CAAC,aAAa,CAAC;AAExC,IAAA,OAAOC,SAAI,CAAC,MAAM,CAAC,kBAAkB,EAAE,UAAU,EAAE,SAAS,CAAC,KAAK,EAAE,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,aAAa,GAAG,CAAC,SAAqB,KAA2B;IACrE,OAAOA,SAAI,CAAC,eAAe,CAAC,OAAO,CAACP,8BAAqB,CAAC,SAAS,CAAC,CAAC;AACvE,CAAC;AAED;;;;;;AAMG;AACH,MAAM,SAAS,GAAG,CAAC,UAAkB,EAAE,SAAc,KAAgB;IACnE,QAAQ,SAAS;AACf,QAAA,KAAK,QAAQ;YACX,MAAM,eAAe,GAAGG,aAAI,CAAC,MAAM,CAAC,UAAU,CAAC;AAC/C,YAAA,IAAI,eAAe,CAAC,MAAM,KAAK,EAAE,EAAE;gBACjC,MAAM,IAAI,KAAK,CACb,CAAA,2CAAA,EAA8C,eAAe,CAAC,MAAM,CAAA,CAAA,CAAG,CACxE;YACH;YACA,OAAO,eAAe,CAAC,QAAQ,CAAC,CAAC,EAAE,EAAE,CAAC;AACxC,QAAA,KAAK,aAAa;AAChB,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOF,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;AAC5C,QAAA;AACE,YAAA,OAAO,CAAC,IAAI,CACV,uBAAuB,SAAS,CAAA,4BAAA,CAA8B,CAC/D;AACD,YAAA,IAAI,UAAU,CAAC,UAAU,CAAC,IAAI,CAAC,EAAE;gBAC/B,OAAOA,gCAAuB,CAAC,UAAU,CAAC,KAAK,CAAC,CAAC,CAAC,CAAC;YACrD;AACA,YAAA,OAAOA,gCAAuB,CAAC,UAAU,CAAC;;AAEhD,CAAC;AAED;;;;;;AAMG;MACU,yBAAyB,GAAG,OAAO,EAC9C,UAAU,EACV,SAAS,EACT,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACF,KAAqB;IACrD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,SAAS,CAAC,UAAU,EAAE,SAAS,CAAC;AACrD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACA,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;AAOK;AACE,MAAM,qBAAqB,GAAG,OAAO,EAC1C,QAAQ,EACR,YAAY,EACZ,MAAM,EACN,cAAc,EACd,kCAAkC,GACN,KAAqB;IACjD,MAAM,kBAAkB,GAAG,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;IACnD,MAAM,YAAY,GAAG,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,QAAQ,CAAC;AACvD,IAAA,MAAM,QAAQ,GAAG,MAAM,sBAAsB,CAC3C,kBAAkB,CAAC,mBAAmB,EACtC,kBAAkB,CAAC,aAAa,EAChC,kBAAkB,CAAC,IAAI,EACvB,kCAAkC,CACnC;IACD,IAAI,CAAC,QAAQ,EAAE;AACb,QAAA,MAAM,IAAI,KAAK,CAAC,uCAAuC,YAAY,CAAA,CAAE,CAAC;IACxE;IAEA,MAAM,UAAU,GAAG,IAAI,CAAC,KAAK,CAC3B,IAAI,WAAW,EAAE,CAAC,MAAM,CAACT,gCAAuB,CAAC,kBAAkB,CAAC,IAAI,CAAC,CAAC,CAC3E;IAED,IACE,CAAC,UAAU,CAAC,cAAc;AAC1B,QAAA,UAAU,CAAC,cAAc,KAAK,cAAc,EAC5C;QACA,MAAM,IAAI,KAAK,CACb,CAAA,yDAAA,EAA4D,cAAc,CAAA,SAAA,EAAY,UAAU,CAAC,cAAc,CAAA,CAAA,CAAG,CACnH;IACH;IACA,IAAI,CAAC,UAAU,CAAC,MAAM,IAAI,UAAU,CAAC,MAAM,KAAK,MAAM,EAAE;QACtD,MAAM,IAAI,KAAK,CACb,CAAA,iDAAA,EAAoD,MAAM,CAAA,SAAA,EAAY,UAAU,CAAC,MAAM,CAAA,CAAA,CAAG,CAC3F;IACH;AAEA,IAAA,IAAI,CAAC,UAAU,CAAC,YAAY,EAAE;AAC5B,QAAA,MAAM,IAAI,KAAK,CAAC,8CAA8C,CAAC;IACjE;;IAGA,MAAM,YAAY,GAAGA,gCAAuB,CAAC,UAAU,CAAC,YAAY,CAAC;IACrE,MAAM,gBAAgB,GAAGQ,kBAAW,CAAC,EAAE,YAAY,EAAE,YAAY,EAAE,CAAC;AACpE,IAAA,OAAOC,oBAAa,CAAC,gBAAgB,CAAC;AACxC;AAEA;;;;;;;;;;;;;;;;;;;;;;;AAuBG;AACI,MAAM,yBAAyB,GAAG,OACvC,GAAW,EACX,qCAA8C,KAC1B;IACpB,MAAM,eAAe,GACnB,qCAAqC;AACrC,QAAAC,8CAAoC;;AAGtC,IAAA,MAAM,CAAC,SAAS,EAAE,UAAU,EAAE,YAAY,CAAC,GAAG,GAAG,CAAC,KAAK,CAAC,GAAG,CAAC;AAC5D,IAAA,IAAI,CAAC,YAAY;AAAE,QAAA,MAAM,IAAI,KAAK,CAAC,2BAA2B,CAAC;AAC/D,IAAA,MAAM,YAAY,GAAG,CAAA,EAAG,SAAS,CAAA,CAAA,EAAI,UAAU,EAAE;;AAGjD,IAAA,MAAM,EAAE,GAAGL,aAAM,CAAC,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,YAAY,CAAC,CAAC;IACzD,MAAM,SAAS,GAAGA,aAAM,CAAC,EAAE,CAAC,CAAC;;IAG7B,MAAM,KAAK,GAAG,CAAC,CAAS,KACtB,CAAC,CAAC,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,CAAC,OAAO,CAAC,IAAI,EAAE,GAAG,CAAC,EAAE,MAAM,CAClD,CAAC,CAAC,MAAM,IAAI,CAAC,CAAC,IAAI,CAAC,CAAC,MAAM,GAAG,CAAC,CAAC,IAAI,CAAC,CAAC,EACrC,GAAG,CACJ;AACH,IAAA,MAAM,SAAS,GAAG,UAAU,CAAC,IAAI,CAC/B,IAAI,CAAC,KAAK,CAAC,YAAY,CAAC;SACrB,KAAK,CAAC,EAAE;AACR,SAAA,GAAG,CAAC,CAAC,CAAC,KAAK,CAAC,CAAC,UAAU,CAAC,CAAC,CAAC,CAAC,CAC/B,CAAC;;AAGF,IAAA,MAAM,SAAS,GAAGL,gCAAuB,CAAC,eAAe,CAAC;;IAG1D,OAAOM,SAAI,CAAC,MAAM,CAAC,SAAS,EAAE,SAAS,EAAE,SAAS,CAAC;AACrD;AAEA;;;;;;;;;;AAUG;AACI,MAAM,gBAAgB,GAAG,OAC9B,2BAAmC,EACnC,OAAe,KACQ;AACvB,IAAA,OAAO,MAAMK,uBAAgB,CAC3BX,gCAAuB,CAAC,2BAA2B,CAAC,EACpD,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,OAAO,CAAC,CAClC;AACH;AAEA;;;;;;;;;;;AAWG;AACI,MAAM,yBAAyB,GAAG,OACvC,aAAqB,EACrB,sCAA+C,KAC5B;AACnB,IAAA,OAAOD,8BAAqB,CAC1B,MAAM,gBAAgB,CACpB,sCAAsC;AACpC,QAAAa,mDAAyC,EAC3C,aAAa,CACd,CACF;AACH;AAEA;;;;;;;;;;;AAWG;MACU,mBAAmB,GAAG,CACjC,MAAc,EACd,4CAAqD,KAC3C;AACV,IAAA,OAAOhB,kBAAS,CAAC,MAAM,CACrBY,kBAAW,CAAC;QACV,YAAY,EAAE,IAAI,WAAW,EAAE,CAAC,MAAM,CAAC,MAAM,CAAC;AAC9C,QAAA,YAAY,EAAEX,6BAAsB,CAClCG,gCAAuB,CACrB,4CAA4C;AAC1C,YAAAa,8DAAoD,CACvD,CACF;AACF,KAAA,CAAC,CACH;AACH;;;;;;;;;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3412:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2838:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3413:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2839:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3414:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2840:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3415:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2841:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3416:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2843:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:64:function base64urlToBuffer(baseurl64String: string): ArrayBuffer {
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3417:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2846:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:1:import { bs58check, uint8ArrayToHexString, uint8ArrayFromHexString, bs58, hexToAscii } from '@turnkey/encoding';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3418:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2847:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:28:        const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3419:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2848:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:96:            return bs58.encode(concatenatedBytes);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3420:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2849:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:179:            const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3421:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2851:codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/dist/turnkey.mjs:346:    return bs58check.encode(hpkeEncrypt({
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3422:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2852:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:18:  // We expect the stamp to be base64url encoded
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3423:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2853:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/stamp-test.ts:20:    Buffer.from(stamp.stampHeaderValue, "base64url").toString(),
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3424:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2854:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:8:    // Binary string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3425:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2855:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:9:    const base64String = btoa(str);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3426:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2856:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:10:    // Base64 to base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3427:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2857:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:11:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3428:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2858:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:12:    const base64urlString = base64String
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3429:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2859:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:16:    return base64urlString;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3430:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2860:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs:20://# sourceMappingURL=base64url.mjs.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3431:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2863:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:3:var base64url = require('./base64url.js');
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3432:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2864:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:8:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAttestation, credential);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3433:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2865:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.js:11:    return convert.convert(base64url.bufferToBase64url, schema.publicKeyCredentialWithAssertion, credential);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3434:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2866:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js.map:1:{"version":3,"file":"base64url.js","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":";;AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3435:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2869:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/api.mjs:1:import { bufferToBase64url } from './base64url.mjs';
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3436:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2870:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts.map:1:{"version":3,"file":"base64url.d.ts","sourceRoot":"","sources":["../../src/webauthn-json/base64url.ts"],"names":[],"mappings":"AAAA,MAAM,MAAM,eAAe,GAAG,MAAM,CAAC;AAErC,wBAAgB,iBAAiB,CAC/B,eAAe,EAAE,eAAe,GAC/B,WAAW,CAgBb;AAED,wBAAgB,iBAAiB,CAAC,MAAM,EAAE,WAAW,GAAG,eAAe,CAkBtE"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3437:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2871:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:2:export declare function base64urlToBuffer(baseurl64String: Base64urlString): ArrayBuffer;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3438:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2872:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.d.ts:4://# sourceMappingURL=base64url.d.ts.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3439:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2873:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/json.d.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3440:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2874:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:10:    // Binary string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3441:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2875:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:11:    const base64String = btoa(str);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3442:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2876:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:12:    // Base64 to base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3443:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2877:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:13:    // We assume that the base64url string is well-formed.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3444:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2878:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:14:    const base64urlString = base64String
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3445:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2879:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:18:    return base64urlString;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3446:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2880:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.js:22://# sourceMappingURL=base64url.js.map
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3447:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2881:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/dist/webauthn-json/base64url.mjs.map:1:{"version":3,"file":"base64url.mjs","sources":["../../src/webauthn-json/base64url.ts"],"sourcesContent":[null],"names":[],"mappings":"AAsBM,SAAU,iBAAiB,CAAC,MAAmB,EAAA;;AAEnD,IAAA,MAAM,QAAQ,GAAG,IAAI,UAAU,CAAC,MAAM,CAAC;IACvC,IAAI,GAAG,GAAG,EAAE;AACZ,IAAA,KAAK,MAAM,QAAQ,IAAI,QAAQ,EAAE;AAC/B,QAAA,GAAG,IAAI,MAAM,CAAC,YAAY,CAAC,QAAQ,CAAC;IACtC;;AAGA,IAAA,MAAM,YAAY,GAAG,IAAI,CAAC,GAAG,CAAC;;;IAI9B,MAAM,eAAe,GAAG;AACrB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,KAAK,EAAE,GAAG;AAClB,SAAA,OAAO,CAAC,IAAI,EAAE,EAAE,CAAC;AACpB,IAAA,OAAO,eAAe;AACxB;;;;"}
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3448:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2882:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:1:import { base64urlToBuffer, bufferToBase64url } from "./base64url";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3449:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2883:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:19:  return convert(base64urlToBuffer, credentialCreationOptions, requestJSON);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3450:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2884:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/api.ts:44:  return convert(base64urlToBuffer, credentialRequestOptions, requestJSON);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3451:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2885:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/json.ts:1:import type { Base64urlString } from "./base64url";
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3452:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2886:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:3:export function base64urlToBuffer(
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3453:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2887:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:8:  const base64String =
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3454:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2888:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:12:  const str = atob(base64String);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3455:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2889:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:31:  // Binary string to base64
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3456:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2890:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:32:  const base64String = btoa(str);
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3457:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2891:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:34:  // Base64 to base64url
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3458:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2892:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:35:  // We assume that the base64url string is well-formed.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3459:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2893:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:36:  const base64urlString = base64String
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3460:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2894:codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn-json/base64url.ts:40:  return base64urlString;
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3461:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2963:    "bs58": "6.0.0",
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3462:codex-crypto-reviews/Encoding.cs-r1-20260523.md:2964:    "bs58check": "4.0.0"
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3463:codex-crypto-reviews/Encoding.cs-r1-20260523.md:3000:/bin/zsh -lc "node -e \"const vals=[['hello',Buffer.from('hello','latin1').toString('base64url')],['private',Buffer.from('5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1','latin1').toString('base64url')],['hex 01',Buffer.from('01','hex').toString('base64url')],['hex 0001',Buffer.from('0001','hex').toString('base64url')],['hex ff',Buffer.from('ff','hex').toString('base64url')],['hex 00ff',Buffer.from('00ff','hex').toString('base64url')],['hello decode',Buffer.from('aGVsbG8','base64url').toString('latin1')]]; for (const [k,v] of vals) console.log(k+'='+v);\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3599:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3600:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3601:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3602:codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3747:/bin/zsh -lc 'node -e "function u(hexString,length){const hexRegex=/''^[0-9A-Fa-f]+$/; if(!hexString || hexString.length%2!=0 || !hexRegex.test(hexString)) throw new Error('"'invalid'); const buffer=new Uint8Array(hexString.match(/../g).map(h=>parseInt(h,16))); if("'!length) return buffer; if(hexString.length/2>length) throw new Error('"'too long'); const p=new Uint8Array(length); p.set(buffer,length-buffer.length); return p;} console.log(Array.from(u('01',0)).join(','));\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3783:No `internal` helper methods are defined in `src/Encoding.cs`; `BASE64_KEYSTR` is an internal constant, not a method.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3787:| `Uint8ArrayToHexString` (`src/Encoding.cs:90`) | `uint8ArrayToHexString` (`ts-source/hex.ts:7`) | REVIEWED | Success path matches lower-case two-digit hex. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3788:| `Uint8ArrayFromHexString` (`src/Encoding.cs:118`) | `uint8ArrayFromHexString` (`ts-source/hex.ts:23`) | REVIEWED | Main validation/padding reviewed; `length=0` divergence below. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3789:| `HexToAscii` (`src/Encoding.cs:155`) | `hexToAscii` (`ts-source/hex.ts:58`) | REVIEWED | Valid even hex matches; malformed/odd behavior diverges below. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3790:| `NormalizePadding` (`src/Encoding.cs:176`) | `normalizePadding` (`ts-source/hex.ts:73`) | REVIEWED | Leading-zero add/strip logic matches. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3791:| `StringToBase64UrlString` (`src/Encoding.cs:235`) | `stringToBase64urlString` (`ts-source/base64.ts:13`) | REVIEWED | 0..255 code unit behavior matches custom `btoa`. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3792:| `HexStringToBase64Url` (`src/Encoding.cs:262`) | `hexStringToBase64url` (`ts-source/base64.ts:28`) | REVIEWED | Main path matches; inherits `length=0` divergence. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3793:| `Base64StringToBase64UrlEncodedString` (`src/Encoding.cs:289`) | `base64StringToBase64UrlEncodedString` (`ts-source/base64.ts:45`) | REVIEWED | Character replacements and padding removal match. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3794:| `Base64UrlToBase64` (`src/Encoding.cs:303`) | `base64UrlToBase64` (`ts-source/base64.ts:58`) | REVIEWED | URL-safe replacement and padding restoration match. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3795:| `DecodeBase64UrlToString` (`src/Encoding.cs:320`) | `decodeBase64urlToString` / `atob` (`ts-source/base64.ts:74`, `:148`) | REVIEWED | Valid inputs match; invalid-character handling diverges below. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3796:| `PointEncode` (`src/Encoding.cs:345`) | `pointEncode` (`ts-source/encode.ts:8`) | REVIEWED | Prefix and x-coordinate copy match; omitted TS redundant length check is unreachable. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3797:| `Base58Encode` (`src/Encoding.cs:379`) | `bs58.encode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Standard Bitcoin alphabet and leading-zero handling reviewed. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3798:| `Base58Decode` (`src/Encoding.cs:414`) | `bs58.decode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Byte math reviewed; invalid error text may differ. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3799:| `Base58CheckEncode` (`src/Encoding.cs:463`) | `bs58check.encode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Payload plus double-SHA256 first-4-byte checksum matches. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3800:| `Base58CheckDecode` (`src/Encoding.cs:487`) | `bs58check.decode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Valid checksum logic matches; invalid error text may differ. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3801:| `Uint8ArrayToString` (`src/Encoding.cs:531`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS `TextDecoder` pattern elsewhere. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3802:| `ConcatUint8Arrays` (`src/Encoding.cs:548`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS spread/concat pattern elsewhere. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3806:- `Uint8Array` -> `byte[]`: structural runtime type mapping; byte order is unchanged.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3808:- TS `Uint8Array#set`, `slice`, spread -> `Array.Copy`/new arrays: same array contents and order.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3814:- `TextDecoder` and spread idioms used by other TS packages -> `Uint8ArrayToString` and `ConcatUint8Arrays`: C# helper extraction, not an encoding wire-format change.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3824:| `src/Encoding.cs:134`, `:139` | `ts-source/hex.ts:38` | `length=0` differs. TS uses `if (!length)` so `0` is treated as omitted and returns the parsed buffer; C# treats `0` as a requested target length and throws for non-empty input. | Change to `if (!length.HasValue || length.Value == 0) return buffer;` if runtime parity with TS falsy `0` is required. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3825:| `src/Encoding.cs:276` | `ts-source/base64.ts:31-32`, `ts-source/hex.ts:38` | `HexStringToBase64Url(input, 0)` inherits the same `length=0` divergence from `Uint8ArrayFromHexString`. | Fix the helper above; no separate logic change needed here. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3826:| `src/Encoding.cs:162-166` | `ts-source/hex.ts:58-63` | `HexToAscii` differs for odd or malformed input. TS loops while `i < length`, parses a trailing single nibble, and JS `parseInt` can yield `NaN` which `String.fromCharCode` turns into NUL. C# loops only while `i + 1 < length`, dropping a trailing nibble, and throws on malformed pairs. | If exact JS runtime behavior is required, loop while `i < hexString.Length` and implement JS-like `parseInt(..., 16)` semantics for partial/invalid pairs. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3827:| `src/Encoding.cs:322-323` | `ts-source/base64.ts:148-160`, `:171-198` | `decodeBase64urlToString` differs for invalid characters. Upstream `atob` strips non-base64 characters before decoding; `Convert.FromBase64String` rejects many of them. Example: upstream decodes `a*GVsbG8` to `hello`; C# throws. | Port upstream `atob` sanitizer/decoder or sanitize with the same regex before `Convert.FromBase64String`, including the `length % 4 === 1` error rule. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3828:| `src/Encoding.cs:431-435` | `ts-source/bs58.ts:36` | Invalid base58 inputs throw C# `ArgumentException` with custom text; upstream delegates to `bs58@6.0.0` and likely exposes dependency-specific error text/type. Valid bytes match. | Either match upstream dependency error messages or explicitly document that invalid-input exception text is not a parity surface. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3829:| `src/Encoding.cs:490-507` | `ts-source/bs58check.ts:36` | Invalid/too-short/checksum-failing Base58Check inputs throw C# custom messages; upstream delegates to `bs58check@4.0.0` dependency behavior. Valid payload/checksum bytes match. | Either match upstream dependency error behavior or document exception text as intentionally .NET-native. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3833:`tests/Fixtures/` is absent in this checkout, despite `README.md:61` describing golden fixtures and `tests/turnkey-sdk-csharp.Tests.csproj:24` including `Fixtures\**\*`. Therefore there are no fixture files under `tests/Fixtures/` that exercise `Encoding.cs`, and I cannot confirm fixture provenance from the pinned package. F is not satisfied as “all fixtures match” in this round.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3835:I did verify that `tests/EncodingTests.cs` uses inline vectors copied from `ts-source/__tests__/index-test.ts` for the main hex/base64 cases, and I ran Node `Buffer` checks for the base64url expected values. The Base58/Base58Check tests are local known-vector/roundtrip tests, not generated fixture files from the pinned upstream package.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3856:No `internal` helper methods are defined in `src/Encoding.cs`; `BASE64_KEYSTR` is an internal constant, not a method.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3860:| `Uint8ArrayToHexString` (`src/Encoding.cs:90`) | `uint8ArrayToHexString` (`ts-source/hex.ts:7`) | REVIEWED | Success path matches lower-case two-digit hex. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3861:| `Uint8ArrayFromHexString` (`src/Encoding.cs:118`) | `uint8ArrayFromHexString` (`ts-source/hex.ts:23`) | REVIEWED | Main validation/padding reviewed; `length=0` divergence below. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3862:| `HexToAscii` (`src/Encoding.cs:155`) | `hexToAscii` (`ts-source/hex.ts:58`) | REVIEWED | Valid even hex matches; malformed/odd behavior diverges below. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3863:| `NormalizePadding` (`src/Encoding.cs:176`) | `normalizePadding` (`ts-source/hex.ts:73`) | REVIEWED | Leading-zero add/strip logic matches. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3864:| `StringToBase64UrlString` (`src/Encoding.cs:235`) | `stringToBase64urlString` (`ts-source/base64.ts:13`) | REVIEWED | 0..255 code unit behavior matches custom `btoa`. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3865:| `HexStringToBase64Url` (`src/Encoding.cs:262`) | `hexStringToBase64url` (`ts-source/base64.ts:28`) | REVIEWED | Main path matches; inherits `length=0` divergence. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3866:| `Base64StringToBase64UrlEncodedString` (`src/Encoding.cs:289`) | `base64StringToBase64UrlEncodedString` (`ts-source/base64.ts:45`) | REVIEWED | Character replacements and padding removal match. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3867:| `Base64UrlToBase64` (`src/Encoding.cs:303`) | `base64UrlToBase64` (`ts-source/base64.ts:58`) | REVIEWED | URL-safe replacement and padding restoration match. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3868:| `DecodeBase64UrlToString` (`src/Encoding.cs:320`) | `decodeBase64urlToString` / `atob` (`ts-source/base64.ts:74`, `:148`) | REVIEWED | Valid inputs match; invalid-character handling diverges below. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3869:| `PointEncode` (`src/Encoding.cs:345`) | `pointEncode` (`ts-source/encode.ts:8`) | REVIEWED | Prefix and x-coordinate copy match; omitted TS redundant length check is unreachable. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3870:| `Base58Encode` (`src/Encoding.cs:379`) | `bs58.encode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Standard Bitcoin alphabet and leading-zero handling reviewed. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3871:| `Base58Decode` (`src/Encoding.cs:414`) | `bs58.decode` via shim (`ts-source/bs58.ts:36`) | REVIEWED | Byte math reviewed; invalid error text may differ. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3872:| `Base58CheckEncode` (`src/Encoding.cs:463`) | `bs58check.encode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Payload plus double-SHA256 first-4-byte checksum matches. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3873:| `Base58CheckDecode` (`src/Encoding.cs:487`) | `bs58check.decode` via shim (`ts-source/bs58check.ts:36`) | REVIEWED | Valid checksum logic matches; invalid error text may differ. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3874:| `Uint8ArrayToString` (`src/Encoding.cs:531`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS `TextDecoder` pattern elsewhere. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3875:| `ConcatUint8Arrays` (`src/Encoding.cs:548`) | N/A in `@turnkey/encoding` | REVIEWED | C# consumer helper for TS spread/concat pattern elsewhere. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3879:- `Uint8Array` -> `byte[]`: structural runtime type mapping; byte order is unchanged.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3881:- TS `Uint8Array#set`, `slice`, spread -> `Array.Copy`/new arrays: same array contents and order.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3887:- `TextDecoder` and spread idioms used by other TS packages -> `Uint8ArrayToString` and `ConcatUint8Arrays`: C# helper extraction, not an encoding wire-format change.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3897:| `src/Encoding.cs:134`, `:139` | `ts-source/hex.ts:38` | `length=0` differs. TS uses `if (!length)` so `0` is treated as omitted and returns the parsed buffer; C# treats `0` as a requested target length and throws for non-empty input. | Change to `if (!length.HasValue || length.Value == 0) return buffer;` if runtime parity with TS falsy `0` is required. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3898:| `src/Encoding.cs:276` | `ts-source/base64.ts:31-32`, `ts-source/hex.ts:38` | `HexStringToBase64Url(input, 0)` inherits the same `length=0` divergence from `Uint8ArrayFromHexString`. | Fix the helper above; no separate logic change needed here. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3899:| `src/Encoding.cs:162-166` | `ts-source/hex.ts:58-63` | `HexToAscii` differs for odd or malformed input. TS loops while `i < length`, parses a trailing single nibble, and JS `parseInt` can yield `NaN` which `String.fromCharCode` turns into NUL. C# loops only while `i + 1 < length`, dropping a trailing nibble, and throws on malformed pairs. | If exact JS runtime behavior is required, loop while `i < hexString.Length` and implement JS-like `parseInt(..., 16)` semantics for partial/invalid pairs. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3900:| `src/Encoding.cs:322-323` | `ts-source/base64.ts:148-160`, `:171-198` | `decodeBase64urlToString` differs for invalid characters. Upstream `atob` strips non-base64 characters before decoding; `Convert.FromBase64String` rejects many of them. Example: upstream decodes `a*GVsbG8` to `hello`; C# throws. | Port upstream `atob` sanitizer/decoder or sanitize with the same regex before `Convert.FromBase64String`, including the `length % 4 === 1` error rule. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3901:| `src/Encoding.cs:431-435` | `ts-source/bs58.ts:36` | Invalid base58 inputs throw C# `ArgumentException` with custom text; upstream delegates to `bs58@6.0.0` and likely exposes dependency-specific error text/type. Valid bytes match. | Either match upstream dependency error messages or explicitly document that invalid-input exception text is not a parity surface. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3902:| `src/Encoding.cs:490-507` | `ts-source/bs58check.ts:36` | Invalid/too-short/checksum-failing Base58Check inputs throw C# custom messages; upstream delegates to `bs58check@4.0.0` dependency behavior. Valid payload/checksum bytes match. | Either match upstream dependency error behavior or document exception text as intentionally .NET-native. |
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3906:`tests/Fixtures/` is absent in this checkout, despite `README.md:61` describing golden fixtures and `tests/turnkey-sdk-csharp.Tests.csproj:24` including `Fixtures\**\*`. Therefore there are no fixture files under `tests/Fixtures/` that exercise `Encoding.cs`, and I cannot confirm fixture provenance from the pinned package. F is not satisfied as “all fixtures match” in this round.
codex-crypto-reviews/Encoding.cs-r1-20260523.md:3908:I did verify that `tests/EncodingTests.cs` uses inline vectors copied from `ts-source/__tests__/index-test.ts` for the main hex/base64 cases, and I ran Node `Buffer` checks for the base64url expected values. The Base58/Base58Check tests are local known-vector/roundtrip tests, not generated fixture files from the pinned upstream package.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:1:export const SUITE_ID_1 = new Uint8Array([75, 69, 77, 0, 16]); //KEM suite ID
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:2:export const SUITE_ID_2 = new Uint8Array([72, 80, 75, 69, 0, 16, 0, 1, 0, 2]); //HPKE suite ID
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:3:export const HPKE_VERSION = new Uint8Array([72, 80, 75, 69, 45, 118, 49]); //HPKE-v1
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:4:export const LABEL_SECRET = new Uint8Array([115, 101, 99, 114, 101, 116]); //secret
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:5:export const LABEL_EAE_PRK = new Uint8Array([101, 97, 101, 95, 112, 114, 107]); //eae_prk
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:6:export const LABEL_SHARED_SECRET = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:9:export const AES_KEY_INFO = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/constants.ts:16:export const IV_INFO = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn.ts:65:async function getChallengeFromPayload(payload: string): Promise<Uint8Array> {
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/webauthn.ts:70:  return new Uint8Array(hexBuffer);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:19:  encode(payload: Uint8Array | number[]): string;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:20:  decode(str: string): Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:21:  decodeUnsafe(str: string): Uint8Array | undefined;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:2: * Converts a Uint8Array into a lowercase hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:4: * @param {Uint8Array} input - The input byte array.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:7:export function uint8ArrayToHexString(input: Uint8Array): string {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:15: * Creates a Uint8Array from a hex string.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:20: * @returns {Uint8Array} - The resulting byte array.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:26:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:34:  const buffer = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:48:  let paddedBuffer = new Uint8Array(length);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:69: * @param {Uint8Array} byteArray - The byte array to pad or trim.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:71: * @returns {Uint8Array} - The normalized byte array.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:74:  byteArray: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:76:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:81:    const padding = new Uint8Array(paddingLength).fill(0);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:82:    return new Uint8Array([...padding, ...byteArray]);
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:66:    new Uint8Array(signatureIeee1363),
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:86:function convertEcdsaIeee1363ToDer(ieee: Uint8Array): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:98:    der = new Uint8Array(length + 3);
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:103:    der = new Uint8Array(length + 2);
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:123:function toUnsignedBigNum(bytes: Uint8Array): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:139:  const res = new Uint8Array(bytes.length - start + extraZero);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:30:async function sha256Hex(data: Uint8Array): Promise<string> {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:33:  return uint8ArrayToHexString(new Uint8Array(digest)).toUpperCase();
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:69:  const coseSign1Der = Uint8Array.from(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:76:  const attestationDoc = CBOR.decode(new Uint8Array(payload).buffer);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:93:  const decodedBootProofManifest = Uint8Array.from(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:106:  const publicKeyBytes = new Uint8Array(attestationDoc.public_key);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:127:  let publicKeyBytes: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:156:  let signatureBytes: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:180:  cabundle: Uint8Array[],
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:182:  leafCert: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:188:    const rootDer = new Uint8Array(rootX509.rawData);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:259:  leaf: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:262:  const tbs = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:265:      new Uint8Array(protectedHeaders),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:266:      new Uint8Array(0),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:267:      new Uint8Array(payload),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:278:    new Uint8Array(signature),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:285:  const A = new Uint8Array(a),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:286:    B = new Uint8Array(b);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:19:  encode(buffer: Uint8Array | number[]): string;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:20:  decodeUnsafe(str: string): Uint8Array | undefined;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:21:  decode(str: string): Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/elliptic_curves.ts:36:function byteArrayToInteger(bytes: Uint8Array): bigint {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/elliptic_curves.ts:41:function integerToByteArray(i: bigint, length: number): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/elliptic_curves.ts:166:export function pointDecode(point: Uint8Array): JsonWebKey {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:31:  ciphertextBuf: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:32:  encappedKeyBuf: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:37:  plainTextBuf: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:38:  targetKeyBuf: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:41:  plainTextBuf: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:42:  targetKeyBuf: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:54:  nonce: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:55:  ephemeralSenderPublic: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:56:  encryptedMessage: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:74: * Derives public key from Uint8Array or hexstring private key
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:76: * @param {Uint8Array | string} privateKey - The Uint8Array or hexstring representation of a compressed private key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:78: * @returns {Uint8Array} - The public key in Uin8Array representation.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:81:  privateKey: Uint8Array | string,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:83:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:92: * @returns {Uint8Array} - The encrypted data.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:98:}: HpkeEncryptParams): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:126:    const sharedSecret = extractAndExpand(new Uint8Array([]), ikm, info, 32);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:129:    ikm = buildLabeledIkm(LABEL_SECRET, new Uint8Array([]), SUITE_ID_2);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:142:    const result = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:159: * @returns {Uint8Array} - The encrypted data.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:166:}: HpkeAuthEncryptParams): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:191:    const sharedSecret = extractAndExpand(new Uint8Array([]), ikm, info, 32);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:194:    ikm = buildLabeledIkm(LABEL_SECRET, new Uint8Array([]), SUITE_ID_2);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:207:    const result = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:227: * @param {Uint8Array} targetPublicKeyUncompressed - The P256 uncompressed public key to encrypt the message to
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:228: * @param {Uint8Array} message - The message to encrypt to targetPublicKeyUncompressed
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:229: * @returns {Uint8Array} - A borsh serialized envelope containing the nonce + ephemeralSenderPublicKey + encrypted message
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:232:  targetPublicKeyUncompressed: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:233:  message: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:234:): Promise<Uint8Array> => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:247:  const nonce = new Uint8Array(QUORUM_ENCRYPT_NONCE_LENGTH_BYTES);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:271:    encryptedMessage: new Uint8Array(encryptedMessageBuf),
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:282: * @param {Uint8Array} encryptedBuf - The result of hpkeAuthEncrypt or hpkeEncrypt
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:286:export const formatHpkeBuf = (encryptedBuf: Uint8Array): string => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:306: * @returns {Uint8Array} - The decrypted data.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:312:}: HpkeDecryptParams): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:314:    let ikm: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:315:    let info: Uint8Array;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:334:    const sharedSecret = extractAndExpand(new Uint8Array([]), ikm, info, 32);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:337:    ikm = buildLabeledIkm(LABEL_SECRET, new Uint8Array([]), SUITE_ID_2);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:374: * @param {Uint8Array} senderPubBuf
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:375: * @param {Uint8Array} receiverPubBuf
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:376: * @return {Uint8Array} - The resulting concatenation of sender and receiver pubkeys.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:379:  senderPubBuf: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:380:  receiverPubBuf: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:381:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:382:  return new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:389: * Accepts a private key Uint8Array in the PKCS8 format, and returns the encapsulated private key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:391: * @param {Uint8Array} privateKey - A PKCS#8 private key structured with the key data at a specific position. The actual key starts at byte 36 and is 32 bytes long.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:392: * @return {Uint8Array} - The private key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:395:  privateKey: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:396:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:401: * Accepts a public key Uint8Array, and returns a Uint8Array with the compressed version of the public key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:403: * @param {Uint8Array} rawPublicKey - The raw public key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:404: * @return {Uint8Array} – The compressed public key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:406:export const compressRawPublicKey = (rawPublicKey: Uint8Array): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:423: * @param {Uint8Array} rawPublicKey - The public key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:424: * @return {Uint8Array} - The uncompressed public key.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:427:  rawPublicKey: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:429:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:486: * @param {Uint8Array} label - The label to use.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:487: * @param {Uint8Array} ikm - The input key material.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:488: * @param {Uint8Array} suiteId - The suite identifier.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:489: * @returns {Uint8Array} - The labeled IKM.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:492:  label: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:493:  ikm: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:494:  suiteId: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:495:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:498:  const ret = new Uint8Array(combinedLength);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:518: * @param {Uint8Array} label - The label to use.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:519: * @param {Uint8Array} info - Additional information.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:520: * @param {Uint8Array} suiteId - The suite identifier.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:522: * @returns {Uint8Array} - The labeled info.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:525:  label: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:526:  info: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:527:  suiteId: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:529:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:531:  const ret = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:534:  ret.set(new Uint8Array([0, len]), 0); // this isn’t an error, we’re starting at index 2 because the first two bytes should be 0. See <https://github.com/dajiaji/hpke-js/blob/1e7fb1372fbcdb6d06bf2f4fa27ff676329d633e/src/kdfs/hkdf.ts#L41> for reference.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:546:  sharedSecret: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:547:  ikm: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:548:  info: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:550:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:553:  return new Uint8Array(resp);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:559:const deriveSS = (encappedKeyBuf: Uint8Array, priv: string): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:571:  plainTextData: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:572:  key: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:573:  iv: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:574:  aad?: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:575:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:585:  encryptedData: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:586:  key: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:587:  iv: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:588:  aad?: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:589:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:599:  encappedKeyBuf: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:601:): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:602:  const encappedKeyArray = new Uint8Array(encappedKeyBuf);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:605:  const kemContext = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:631: * @returns {Uint8Array} - The raw signature.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:633:export const fromDerSignature = (derSignature: string): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:726:  return new Uint8Array([...rPadded, ...sPadded]);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:733: * and converts it into the DER-encoded format. DER (Distinguished Encoding Rules) is a binary encoding
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:756:  const encodeDerInteger = (integer?: Uint8Array): Uint8Array => {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:769:      ? new Uint8Array([0x00, ...integer])
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:773:    return new Uint8Array([0x02, paddedInteger.length, ...paddedInteger]);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:781:  const derSignature = new Uint8Array([
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:798: * @param {Uint8Array} ephemeralSenderPublic - The ephemeral public key used to create the preImage
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:799: * @param {Uint8Array} ephemeralSenderPrivate - The ephemeral private key to create the shared secret with
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:800: * @param {Uint8Array} targetPublicKeyUncompressed - The public key to create the shared secret with and encrypt the message to
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:804:  ephemeralSenderPublic: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:805:  ephemeralSenderPrivate: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:806:  targetPublicKeyUncompressed: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:817:  let preImage = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:840:  const mac = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:864:  ephemeralSenderPublic: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:865:  receiverPublic: Uint8Array,
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:866:): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:876:  const aad = new Uint8Array(
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:27:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:33:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:52:  let xBytes = new Uint8Array(base64urlToBuffer(xString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:58:  let yBytes = new Uint8Array(base64urlToBuffer(yString));
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:75:  const byteView = new Uint8Array(buffer);
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/bytes.ts:17:export function fromHex(hex: string): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/bytes.ts:21:  const arr = new Uint8Array(hex.length / 2);
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/bytes.ts:35:export function toHex(bytes: Uint8Array): string {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/bytes.ts:53:export function toBase64(bytes: Uint8Array, opt_webSafe?: boolean): string {
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/tink/bytes.ts:73:export function toByteString(bytes: Uint8Array): string {
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3453:  v1PayloadEncoding:
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3749:    /** @description Encoding of the `payload` string. Turnkey uses this information to convert `payload` into bytes with the correct decoder (e.g. hex, utf8). */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3750:    encoding: definitions["v1PayloadEncoding"];
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3759:    /** @description Encoding of the `payload` string. Turnkey uses this information to convert `payload` into bytes with the correct decoder (e.g. hex, utf8). */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3760:    encoding: definitions["v1PayloadEncoding"];
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3787:    /** @description Encoding of the `payload` string. Turnkey uses this information to convert `payload` into bytes with the correct decoder (e.g. hex, utf8). */
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.types.ts:3788:    encoding: definitions["v1PayloadEncoding"];
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:10858:    "v1PayloadEncoding": {
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:11712:          "$ref": "#/definitions/v1PayloadEncoding",
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:11713:          "description": "Encoding of the `payload` string. Turnkey uses this information to convert `payload` into bytes with the correct decoder (e.g. hex, utf8)."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:11734:          "$ref": "#/definitions/v1PayloadEncoding",
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:11735:          "description": "Encoding of the `payload` string. Turnkey uses this information to convert `payload` into bytes with the correct decoder (e.g. hex, utf8)."
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:11801:          "$ref": "#/definitions/v1PayloadEncoding",
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/__generated__/services/coordinator/public/v1/public_api.swagger.json:11802:          "description": "Encoding of the `payload` string. Turnkey uses this information to convert `payload` into bytes with the correct decoder (e.g. hex, utf8)."

exec
/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '276,620p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   276	        [Fact]
   277	        public void DecodeBase64UrlToString_Roundtrip()
   278	        {
   279	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
   280	            // From upstream private-key encoding vector
   281	            const string b64url =
   282	                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
   283	            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
   284	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1");
   285	        }
   286	
   287	        [Fact]
   288	        public void DecodeBase64UrlToString_StripsInvalidChars()
   289	        {
   290	            // JS atob silently strips chars not in [A-Za-z0-9+/=] before decoding.
   291	            // "aGVsbG8" decodes to "hello"; "a*GVsbG8" must too.
   292	            Encoding.DecodeBase64UrlToString("a*GVsbG8").Should().Be("hello");
   293	        }
   294	
   295	        [Fact]
   296	        public void DecodeBase64UrlToString_LengthMod4Is1_Throws()
   297	        {
   298	            // JS atob: throws when (filtered length) % 4 === 1.
   299	            // 5 chars from [A-Za-z0-9] (no = since base64 padding is +0/2/3),
   300	            // restoring base64 padding adds 3 '='s => total length 8 mod 4 = 0,
   301	            // so we need to confirm the post-padding check triggers.
   302	            // The upstream rule fires on the filtered (post-replace) length;
   303	            // construct an input that yields exactly mod-4==1 after restore.
   304	            // "a" base64url -> "a===" base64 -> filtered length stays 4. mod4=0.
   305	            // We must construct mod-4==1 *after* padding: e.g. "ab" -> "ab==" len 4 mod4=0.
   306	            // The only way to hit mod4==1 is if input strips to something like "a"
   307	            // before padding; Base64UrlToBase64("a") => "a===" mod4=0. So in practice
   308	            // the throw is unreachable via Base64UrlToBase64 input; we exercise the
   309	            // raw path with a manually crafted length-5 string.
   310	            // Use bytes with a leading '=' inside which atob treats as padding reset.
   311	            // Easiest: stamp the helper directly via a length that hits mod-4==1.
   312	            // The Base64UrlToBase64 sanitizer adds '=' until len % 4 == 0, so the
   313	            // only way to trip the throw is when an embedded '=' is followed by
   314	            // material that brings the total to a mod-4==1 residue. We skip the
   315	            // throw test here because the public API path cannot produce it; the
   316	            // throw is kept defensively to match upstream.
   317	            // Sanity check: trivial valid input still decodes.
   318	            Encoding.DecodeBase64UrlToString("AQ").Should().Be("");
   319	        }
   320	
   321	        // ========================================================
   322	        // PointEncode
   323	        // ========================================================
   324	
   325	        [Fact]
   326	        public void PointEncode_EvenY_PrefixIs02()
   327	        {
   328	            var raw = new byte[65];
   329	            raw[0] = 0x04;
   330	            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
   331	            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
   332	            raw[64] = 0xBE; // last byte even
   333	
   334	            var compressed = Encoding.PointEncode(raw);
   335	
   336	            compressed.Should().HaveCount(33);
   337	            compressed[0].Should().Be(0x02);
   338	            for (int i = 1; i < 33; i++) compressed[i].Should().Be(0xAA);
   339	        }
   340	
   341	        [Fact]
   342	        public void PointEncode_OddY_PrefixIs03()
   343	        {
   344	            var raw = new byte[65];
   345	            raw[0] = 0x04;
   346	            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
   347	            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
   348	            raw[64] = 0xBD; // last byte odd
   349	
   350	            var compressed = Encoding.PointEncode(raw);
   351	
   352	            compressed[0].Should().Be(0x03);
   353	        }
   354	
   355	        [Fact]
   356	        public void PointEncode_InvalidPrefix_Throws()
   357	        {
   358	            var raw = new byte[65];
   359	            raw[0] = 0x05;
   360	            Action act = () => Encoding.PointEncode(raw);
   361	            act.Should().Throw<ArgumentException>()
   362	               .WithMessage("Invalid uncompressed P-256 key");
   363	        }
   364	
   365	        [Fact]
   366	        public void PointEncode_WrongLength_Throws()
   367	        {
   368	            var raw = new byte[64];
   369	            raw[0] = 0x04;
   370	            Action act = () => Encoding.PointEncode(raw);
   371	            act.Should().Throw<ArgumentException>()
   372	               .WithMessage("Invalid uncompressed P-256 key");
   373	        }
   374	
   375	        // ========================================================
   376	        // Base58 / Base58Check
   377	        // ========================================================
   378	
   379	        [Fact]
   380	        public void Base58Encode_KnownVector()
   381	        {
   382	            // From "Hello World" -> hex "48656c6c6f20576f726c64" -> base58 "JxF12TrwUP45BMd"
   383	            // (well-known Bitcoin reference vector)
   384	            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
   385	            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
   386	        }
   387	
   388	        [Fact]
   389	        public void Base58Decode_KnownVector()
   390	        {
   391	            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
   392	            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
   393	        }
   394	
   395	        [Fact]
   396	        public void Base58_Roundtrip_PreservesLeadingZero()
   397	        {
   398	            byte[] data = { 0x00, 0x01, 0x02, 0x03 };
   399	            string encoded = Encoding.Base58Encode(data);
   400	            encoded.Should().StartWith("1"); // leading-zero convention
   401	            byte[] decoded = Encoding.Base58Decode(encoded);
   402	            decoded.Should().Equal(data);
   403	        }
   404	
   405	        [Fact]
   406	        public void Base58_Empty()
   407	        {
   408	            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
   409	            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
   410	        }
   411	
   412	        [Fact]
   413	        public void Base58Check_Roundtrip()
   414	        {
   415	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
   416	            string encoded = Encoding.Base58CheckEncode(payload);
   417	            byte[] decoded = Encoding.Base58CheckDecode(encoded);
   418	            decoded.Should().Equal(payload);
   419	        }
   420	
   421	        [Fact]
   422	        public void Base58CheckDecode_TamperedChecksum_Throws()
   423	        {
   424	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
   425	            string encoded = Encoding.Base58CheckEncode(payload);
   426	            // Flip one character to corrupt the checksum
   427	            char[] chars = encoded.ToCharArray();
   428	            chars[chars.Length - 1] = chars[chars.Length - 1] == 'A' ? 'B' : 'A';
   429	            string tampered = new string(chars);
   430	
   431	            Action act = () => Encoding.Base58CheckDecode(tampered);
   432	            act.Should().Throw<ArgumentException>();
   433	        }
   434	
   435	        [Fact]
   436	        public void Base58CheckDecode_TooShort_Throws()
   437	        {
   438	            // Empty payload encoded would still produce 4-byte checksum + something,
   439	            // but we just feed a 3-byte raw decoded result by abusing a known short string.
   440	            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
   441	            act.Should().Throw<ArgumentException>()
   442	               .WithMessage("Invalid Base58Check string - too short");
   443	        }
   444	
   445	        // ========================================================
   446	        // Helpers
   447	        // ========================================================
   448	
   449	        [Fact]
   450	        public void Uint8ArrayToString_DecodesUtf8()
   451	        {
   452	            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
   453	            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
   454	        }
   455	
   456	        [Fact]
   457	        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
   458	        {
   459	            byte[] a = { 1, 2 };
   460	            byte[] b = { 3 };
   461	            byte[] c = { 4, 5, 6 };
   462	            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
   463	        }
   464	    }
   465	}

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

exec
/bin/zsh -lc 'rg -n "Base58Encode|Base58Decode|Base58Check|PointEncode|DecodeBase64UrlToString|ConcatUint8Arrays|Uint8ArrayToString" tests/EncodingTests.cs tests/Fixtures/encoding/turnkey-encoding-vectors.json' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/EncodingTests.cs: binary file matches (found "\0" byte around offset 5671)

exec
/bin/zsh -lc "rg -n \"node-generated|Generators|turnkey-encoding|@turnkey/encoding|bs58|bs58check|pointEncode\" tests codex-crypto-reviews -g '"'!codex-crypto-reviews/Encoding.cs-r*.md'"' -g '"'!codex-crypto-reviews/upstream-snapshots/**/package/dist/**'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/unity-source-pins.md:28:| @turnkey/encoding          | 0.6.0              | 0.6.0               |
tests/Fixtures/README.md:14:- **node-generated** — bytes generated by running the pinned upstream Node
tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
tests/Fixtures/README.md:23:│   └── turnkey-encoding-vectors.json       — upstream-test-vectors
codex-crypto-reviews/turnkey-source-pins.md:12:| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
codex-crypto-reviews/turnkey-source-pins.md:44:| @turnkey/encoding          | 0.6.0   | `60a997f4c52ac5f98bdd285af934f02699b88bff`   | https://github.com/tkhq/sdk/tree/60a997f4c52ac5f98bdd285af934f02699b88bff/packages/encoding |
tests/Fixtures/encoding/turnkey-encoding-vectors.json:4:    "source": "codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts",
tests/Fixtures/encoding/turnkey-encoding-vectors.json:5:    "package": "@turnkey/encoding@0.6.0",
codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt:3:2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
codex-crypto-reviews/peak-lockfile-evidence.md:11:| `packages/peak-sdk-core/package.json`             | `"@turnkey/core": "1.10.0"`, `"@turnkey/crypto": "2.8.8"`, `"@turnkey/encoding": "0.6.0"`, `"@turnkey/http": "3.16.0"` |
codex-crypto-reviews/peak-lockfile-evidence.md:12:| `packages/peak-sdk-browser/package.json`          | `"@turnkey/core": "1.10.0"`, `"@turnkey/crypto": "2.8.8"`, `"@turnkey/encoding": "0.6.0"`, `"@turnkey/iframe-stamper": "2.10.0"` |
codex-crypto-reviews/peak-lockfile-evidence.md:13:| `packages/peak-sdk-node/package.json`             | `"@turnkey/crypto": "2.8.8"`, `"@turnkey/encoding": "0.6.0"`, `"@turnkey/sdk-server": "5.0.0"` |
codex-crypto-reviews/peak-lockfile-evidence.md:22:'@turnkey/encoding':
codex-crypto-reviews/peak-lockfile-evidence.md:25:'@turnkey/encoding': 0.6.0               # transitive: resolved by @turnkey/sdk-server@5.0.0
codex-crypto-reviews/README.md:85:     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:2:  "name": "@turnkey/encoding",
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:38:    "bs58": "6.0.0",
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:39:    "bs58check": "4.0.0"
codex-crypto-reviews/codex-crypto-review.sh:93:     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/README.md:1:# @turnkey/encoding
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:1:# @turnkey/encoding
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:7:- [#886](https://github.com/tkhq/sdk/pull/886) [`6bfcbc5`](https://github.com/tkhq/sdk/commit/6bfcbc5c098e64ab1d115518733b87cfc1653e17) Author [@moeodeh3](https://github.com/moeodeh3) - Expose `bs58` and `bs58check` shims for cross-platform usage.
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/CHANGELOG.md:55:- [#653](https://github.com/tkhq/sdk/pull/653) [`40c4035`](https://github.com/tkhq/sdk/commit/40c40359ec7096d0bca39ffc93e89361b3b11a1a) Thanks [@moe-dev](https://github.com/moe-dev)! - Add pointEncode function
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/encode.ts:8:export function pointEncode(raw: Uint8Array): Uint8Array {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:7:export { bs58 } from "./bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/index.ts:8:export { bs58check } from "./bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:1:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:3:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:5:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:7:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:13:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:16:import * as raw from "bs58";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58.ts:36:export const bs58 = unwrap(raw) as Bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:1:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:3:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:5:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:7:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:13:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:16:import * as raw from "bs58check";
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/bs58check.ts:36:export const bs58check = unwrap(raw) as Bs58Check;
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/package.json:48:    "@turnkey/encoding": "0.6.0",
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/package.json:44:    "@turnkey/encoding": "0.6.0",
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:84:  - @turnkey/encoding@0.6.0
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:95:  - @turnkey/encoding@0.6.0-beta.6
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:106:  - @turnkey/encoding@0.6.0-beta.5
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:119:  - @turnkey/encoding@0.6.0-beta.4
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:126:  - @turnkey/encoding@0.6.0-beta.3
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:133:  - @turnkey/encoding@0.6.0-beta.2
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:140:  - @turnkey/encoding@0.6.0-beta.1
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:147:  - @turnkey/encoding@0.6.0-beta.0
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:153:- [#812](https://github.com/tkhq/sdk/pull/812) [`6cde41c`](https://github.com/tkhq/sdk/commit/6cde41cfecdfb7d54abf52cc65e28ef0e2ad6ba3) Author [@turnekybc](https://github.com/turnekybc) - Add `@turnkey/encoding` as a package dependency instead of a devDependency to `@turnkey/crypto`. This resolves an issue with transitive dependencies when devDependencies are not included in the artifact.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:159:- [#720](https://github.com/tkhq/sdk/pull/720) [`6cbff7a`](https://github.com/tkhq/sdk/commit/6cbff7a0c0b3a9a05586399e5cef476154d3bdca) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptExportBundle` not working in some environments by adding a shim to handle `bs58`'s ESM-only export.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:167:- [#716](https://github.com/tkhq/sdk/pull/716) [`fa46701`](https://github.com/tkhq/sdk/commit/fa467019eef34b5199372248edff1e7a64934e79) Author [@moeodeh3](https://github.com/moeodeh3) - Fixed `decryptCredentialBundle` not working in React Native by adding a shim to handle `bs58check`'s ESM-only export.
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:224:  - @turnkey/encoding@0.4.0
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:235:  - @turnkey/encoding@0.3.0
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/package/CHANGELOG.md:242:  - @turnkey/encoding@0.2.1
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:58:  - @turnkey/encoding@0.6.0
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:67:  - @turnkey/encoding@0.6.0-beta.6
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:87:  - @turnkey/encoding@0.6.0-beta.5
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:98:  - @turnkey/encoding@0.6.0-beta.4
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:110:  - @turnkey/encoding@0.6.0-beta.3
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:130:  - @turnkey/encoding@0.6.0-beta.2
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:138:  - @turnkey/encoding@0.6.0-beta.1
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:146:  - @turnkey/encoding@1.0.0-beta.0
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:207:  - @turnkey/encoding@0.5.0
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:372:  - @turnkey/encoding@0.4.0
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:381:  - @turnkey/encoding@0.3.0
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:416:  - @turnkey/encoding@0.2.1
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:470:- Introduce and reference `@turnkey/encoding` to consolidate utility functions
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/package/CHANGELOG.md:474:  - @turnkey/encoding@0.1.0
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:3:import { bs58check } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:4:import { bs58 } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:9:} from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:77:    const bundleBytes = bs58check.decode(credentialBundle);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:179:      return bs58.encode(concatenatedBytes);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:287:      const decodedKeyBytes = bs58.decode(privateKey);
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/turnkey.ts:542:  return bs58check.encode(
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/package.json:53:    "@turnkey/encoding": "0.6.0"
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/index.ts:2:import { stringToBase64urlString } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/crypto.ts:13:} from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/utils.ts:6:} from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/proof.ts:4:} from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/purejs.ts:3:import { uint8ArrayToHexString } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:8:  - @turnkey/encoding@0.6.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:15:  - @turnkey/encoding@0.6.0-beta.6
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:26:  - @turnkey/encoding@0.6.0-beta.5
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:33:  - @turnkey/encoding@0.6.0-beta.4
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:40:  - @turnkey/encoding@0.6.0-beta.3
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:47:  - @turnkey/encoding@0.6.0-beta.2
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:54:  - @turnkey/encoding@0.6.0-beta.1
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:61:  - @turnkey/encoding@0.6.0-beta.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:74:  - @turnkey/encoding@0.5.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:93:  - @turnkey/encoding@0.4.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:100:  - @turnkey/encoding@0.3.0
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:109:  - @turnkey/encoding@0.2.1
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/package/CHANGELOG.md:116:- Introduce a dependency on `@turnkey/encoding` to consolidate utility functions
codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/ts-source/__tests__/crypto-test.ts:4:import { uint8ArrayFromHexString } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/webcrypto.ts:3:import { uint8ArrayToHexString } from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/ts-source/__tests__/elliptic-curves-test.ts:6:} from "@turnkey/encoding";
codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/ts-source/base.ts:4:import { stringToBase64urlString } from "@turnkey/encoding";

codex
A notable edge-case difference is emerging in `HexToAscii`: the C# helper comments claim JS `parseInt` whitespace behavior, but the implementation does not actually skip leading whitespace/sign characters. I’m checking the rest of the method surface for similar invalid-input or shim behavior differences before writing the A-G report.
exec
/bin/zsh -lc "nl -ba codex-crypto-reviews/README.md | sed -n '60,110p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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

exec
/bin/zsh -lc "find codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0 -maxdepth 3 -type d -name 'node_modules' -o -name 'package-lock.json' -o -name 'pnpm-lock.yaml' -o -name 'yarn.lock'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
exec
/bin/zsh -lc 'find tests/Fixtures -maxdepth 3 -type f -print' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/encoding/turnkey-encoding-vectors.json
tests/Fixtures/README.md

exec
/bin/zsh -lc 'rg -n "public static|private static|internal static| const |readonly" src/Encoding.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
52:    public static class Encoding
57:        public static class Constants
63:            public const int DEFAULT_JWK_MEMBER_BYTE_LENGTH = 32;
68:            public const string BASE58_ALPHABET =
74:            internal const string BASE64_KEYSTR =
78:        private static readonly Regex HexRegex = new Regex(
90:        public static string Uint8ArrayToHexString(byte[] input)
118:        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
164:        public static string HexToAscii(string hexString)
194:        private static int JsParseIntBase16(string s)
222:        public static byte[] NormalizePadding(byte[] byteArray, int targetLength)
281:        public static string StringToBase64UrlString(string input)
308:        public static string HexStringToBase64Url(string input, int? length = null)
335:        public static string Base64StringToBase64UrlEncodedString(string input)
349:        public static string Base64UrlToBase64(string input)
372:        public static string DecodeBase64UrlToString(string input)
406:            const string keyStr =
447:        public static byte[] PointEncode(byte[] raw)
481:        public static string Base58Encode(byte[] data)
516:        public static byte[] Base58Decode(string encoded)
565:        public static string Base58CheckEncode(byte[] payload)
589:        public static byte[] Base58CheckDecode(string encoded)
633:        public static string Uint8ArrayToString(byte[] bytes)
650:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)

exec
/bin/zsh -lc "node -e \"for (const s of [' f','+f','-f','g1','0x10','\\tA']) { const n=parseInt(s,16); const c=String.fromCharCode(n); console.log(JSON.stringify(s), Number.isNaN(n)?'NaN':n, c.charCodeAt(0)); }\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
" f" 15 15
"+f" 15 15
"-f" -15 65521
"g1" NaN 0
"0x10" 16 16
"\tA" 10 10

exec
/bin/zsh -lc 'git rev-parse HEAD' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "git show --no-patch --format='%H %s' a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "git diff -- src/Encoding.cs tests/EncodingTests.cs tests/Fixtures/encoding/turnkey-encoding-vectors.json | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 1779ms:
2026-05-23 08:32:28.911 xcodebuild[48364:105901834]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:29.111 xcodebuild[48364:105901829] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-GOlZALr8' (errno=Operation not permitted)
2026-05-23 08:32:29.587 xcodebuild[48373:105901881]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:29.733 xcodebuild[48373:105901878] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80

 succeeded in 1772ms:
2026-05-23 08:32:28.911 xcodebuild[48366:105901832]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:29.111 xcodebuild[48366:105901831] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-Z7BieJc5' (errno=Operation not permitted)
2026-05-23 08:32:29.587 xcodebuild[48372:105901882]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:29.733 xcodebuild[48372:105901880] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.

 succeeded in 1782ms:
2026-05-23 08:32:28.911 xcodebuild[48365:105901833]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:29.111 xcodebuild[48365:105901830] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-83NMqYaQ' (errno=Operation not permitted)
2026-05-23 08:32:29.587 xcodebuild[48371:105901883]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:29.733 xcodebuild[48371:105901879] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80 Fix Encoding.cs divergences flagged by Codex r1; add fixtures dir

exec
/bin/zsh -lc "nl -ba tests/turnkey-sdk-csharp.Tests.csproj | sed -n '1,220p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
exec
/bin/zsh -lc "nl -ba src/turnkey-sdk-csharp.csproj | sed -n '1,200p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	<Project Sdk="Microsoft.NET.Sdk">
     2	  <PropertyGroup>
     3	    <TargetFramework>net8.0</TargetFramework>
     4	    <RootNamespace>Turnkey.Tests</RootNamespace>
     5	    <IsPackable>false</IsPackable>
     6	    <!-- Tests legitimately use APIs that production code does not, soften 1591 only -->
     7	    <NoWarn>$(NoWarn);CS1591;CA1707</NoWarn>
     8	  </PropertyGroup>
     9	
    10	  <ItemGroup>
    11	    <ProjectReference Include="..\src\turnkey-sdk-csharp.csproj" />
    12	  </ItemGroup>
    13	
    14	  <ItemGroup>
    15	    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    16	    <PackageReference Include="xunit" />
    17	    <PackageReference Include="xunit.runner.visualstudio" />
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

 succeeded in 0ms:
     1	<Project Sdk="Microsoft.NET.Sdk">
     2	  <PropertyGroup>
     3	    <TargetFrameworks>netstandard2.1;net8.0</TargetFrameworks>
     4	    <RootNamespace>Turnkey</RootNamespace>
     5	    <AssemblyName>KyuzanInc.Turnkey.Sdk</AssemblyName>
     6	
     7	    <!-- NuGet packaging -->
     8	    <PackageId>KyuzanInc.Turnkey.Sdk</PackageId>
     9	    <Version>0.1.0-alpha.0</Version>
    10	    <Authors>Kyuzan Inc.</Authors>
    11	    <Company>Kyuzan Inc.</Company>
    12	    <Description>Unofficial / community-maintained Turnkey SDK for .NET by Kyuzan Inc. Not affiliated with Turnkey, Inc. 1:1 logical port of @turnkey/crypto v2.8.8, @turnkey/http v3.16.0, @turnkey/api-key-stamper v0.5.0, and @turnkey/encoding v0.6.0 (peak-pinned versions). Provides P-256 ECDSA / HPKE / HKDF / API key stamping primitives for .NET. BouncyCastle 2.5.0 is used only for primitives; HPKE / HKDF / bundle parsing are direct ports of the upstream Turnkey TypeScript logic.</Description>
    13	    <PackageProjectUrl>https://github.com/KyuzanInc/turnkey-sdk-csharp</PackageProjectUrl>
    14	    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    15	    <PackageReadmeFile>README.md</PackageReadmeFile>
    16	    <RepositoryUrl>https://github.com/KyuzanInc/turnkey-sdk-csharp</RepositoryUrl>
    17	    <RepositoryType>git</RepositoryType>
    18	    <PackageTags>turnkey;wallet;crypto;p256;hpke;ecdsa</PackageTags>
    19	    <PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
    20	  </PropertyGroup>
    21	
    22	  <ItemGroup>
    23	    <PackageReference Include="BouncyCastle.Cryptography" />
    24	    <PackageReference Include="System.Text.Json" />
    25	  </ItemGroup>
    26	
    27	  <ItemGroup>
    28	    <None Include="..\README.md" Pack="true" PackagePath="\" />
    29	    <None Include="..\LICENSE"   Pack="true" PackagePath="\" />
    30	    <None Include="..\NOTICE"    Pack="true" PackagePath="\" />
    31	  </ItemGroup>
    32	</Project>

exec
/bin/zsh -lc "rg -n \"CheckForOverflowUnderflow|checked|TreatWarnings|Nullable|LangVersion\" -S . -g '*.csproj' -g 'Directory.*'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
./Directory.Build.props:3:    <LangVersion>10</LangVersion>
./Directory.Build.props:4:    <Nullable>enable</Nullable>
./Directory.Build.props:5:    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

exec
/bin/zsh -lc 'git ls-files tests/Fixtures/encoding/turnkey-encoding-vectors.json tests/EncodingTests.cs src/Encoding.cs codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 1680ms:
2026-05-23 08:32:43.020 xcodebuild[48886:105903225]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:43.191 xcodebuild[48886:105903224] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-vEkGpPaa' (errno=Operation not permitted)
2026-05-23 08:32:43.624 xcodebuild[48891:105903253]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:32:43.769 xcodebuild[48891:105903252] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/__tests__/index-test.ts
src/Encoding.cs
tests/EncodingTests.cs
tests/Fixtures/encoding/turnkey-encoding-vectors.json

exec
/bin/zsh -lc "node -e \"for (const s of ['0x','0X','0f','fZ',' f','+f','-f','\\tA']) { const n=parseInt(s,16); console.log(JSON.stringify(s), Number.isNaN(n)?'NaN':n, Number.isNaN(n)?0:String.fromCharCode(n).charCodeAt(0)); }\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
"0x" NaN 0
"0X" NaN 0
"0f" 15 15
"fZ" 15 15
" f" 15 15
"+f" 15 15
"-f" -15 65521
"\tA" 10 10

exec
/bin/zsh -lc "find . -maxdepth 3 \\( -type d -name bin -o -type d -name obj \\) -print" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
./tests/obj
./tests/bin
./src/obj
./src/bin

codex
The existing test project has built `bin/obj` directories, so I’m trying a no-build test run. If the read-only sandbox blocks test host temp writes, I’ll report that separately from the static fixture comparison.
exec
/bin/zsh -lc 'dotnet test --no-restore --no-build' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
2026-05-22T23:33:59.242129Z ERROR codex_core::tools::router: error=write_stdin failed: stdin is closed for this session; rerun exec_command with tty=true to keep stdin open
exec
/bin/zsh -lc 'pkill -f "dotnet test --no-restore --no-build" || true' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
sysmon request failed with error: sysmond service not found
pkill: Cannot get process list

exec
/bin/zsh -lc 'killall dotnet || true' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
killall: could not sysctl(KERN_PROC): Operation not permitted

exec
/bin/zsh -lc "node -e 'const fs=require(\"fs\");const f=JSON.parse(fs.readFileSync(\"tests/Fixtures/encoding/turnkey-encoding-vectors.json\",\"utf8\"));let ok=true;for(const v of f.stringToBase64UrlString){const got=Buffer.from(v.input,\"binary\").toString(\"base64url\");if(got"'!==v.expected){console.log("string mismatch",got,v.expected);ok=false;}}for(const v of f.uint8ArrayToHexString){const got=Buffer.from(v.input).toString("hex");if(got!==v.expected){console.log("hex mismatch",got,v.expected);ok=false;}}for(const v of f.uint8ArrayFromHexString.happy){let buf=Array.from(Buffer.from(v.input,"hex"));if(v.length&&buf.length<v.length)buf=Array(v.length-buf.length).fill(0).concat(buf);if(JSON.stringify(buf)!==JSON.stringify(v.expected)){console.log("fromhex mismatch",v.input,JSON.stringify(buf),JSON.stringify(v.expected));ok=false;}}for(const v of f.base64StringToBase64UrlEncodedString){const got=v.input.replace(/'"\\+/g,\"-\").replace(/\\//g,\"_\").replace(/=/g,\"\");if(got"'!==v.expected){console.log("b64url mismatch",got,v.expected);ok=false;}}for(const v of f.hexStringToBase64Url){let s=v.input.padStart(Math.ceil(v.input.length/2)*2,"0");let buf=Array.from(Buffer.from(s,"hex"));if(v.length&&buf.length<v.length)buf=Array(v.length-buf.length).fill(0).concat(buf);const got=Buffer.from(buf).toString("base64url");if(got!==v.expected){console.log("hextob64 mismatch",v.input,got,v.expected);ok=false;}}console.log(ok?"fixture-node-check: ok":"fixture-node-check: fail");'"'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
fixture-node-check: ok

 exited 1 in 300750ms:
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)
MSBUILD : error MSB1025: MSBuild を実行中に内部エラーが発生しました。
System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
Unhandled exception. System.Net.Sockets.SocketException (13): Permission denied
   at System.Net.Sockets.Socket.DoBind(EndPoint endPointSnapshot, SocketAddress socketAddress)
   at System.Net.Sockets.Socket.Bind(EndPoint localEP)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer..ctor(String path, Int32 maxCount, Boolean isFirstPipeInstance)
   at System.IO.Pipes.NamedPipeServerStream.SharedServer.Get(String path, Int32 maxCount, PipeOptions pipeOptions)
   at System.IO.Pipes.NamedPipeServerStream.Create(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize, HandleInheritability inheritability)
   at System.IO.Pipes.NamedPipeServerStream..ctor(String pipeName, PipeDirection direction, Int32 maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, Int32 inBufferSize, Int32 outBufferSize)
   at Microsoft.Build.BackEnd.NodeEndpointOutOfProcBase.InternalConstruct(String pipeName)
   at Microsoft.Build.Execution.OutOfProcNode.Run(Boolean enableReuse, Boolean lowPriority, Exception& shutdownException)
   at Microsoft.Build.CommandLine.MSBuildApp.StartLocalNode(CommandLineSwitches commandLineSwitches, Boolean lowpriority)
   at Microsoft.Build.CommandLine.MSBuildApp.ProcessCommandLineSwitches(CommandLineSwitches switchesFromAutoResponseFile, CommandLineSwitches switchesNotFromAutoResponseFile, String& projectFile, String[]& targets, String& toolsVersion, Dictionary`2& globalProperties, ILogger[]& loggers, LoggerVerbosity& verbosity, LoggerVerbosity& originalVerbosity, List`1& distributedLoggerRecords, Int32& cpuCount, Boolean& enableNodeReuse, TextWriter& preprocessWriter, TextWriter& targetsWriter, Boolean& detailedSummary, ISet`1& warningsAsErrors, ISet`1& warningsNotAsErrors, ISet`1& warningsAsMessages, Boolean& enableRestore, Boolean& interactive, ProfilerLogger& profilerLogger, Boolean& enableProfiler, Dictionary`2& restoreProperties, ProjectIsolationMode& isolateProjects, GraphBuildOptions& graphBuild, String[]& inputResultsCaches, String& outputResultsCache, Boolean& lowPriority, Boolean& question, String[]& getProperty, String[]& getItem, String[]& getTargetResult, String& getResultOutputFile, Boolean recursing, String commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Execute(String[] commandLine)
   at Microsoft.Build.CommandLine.MSBuildApp.Main(String[] args)

codex
**A. Source Pin Acknowledgement**
- Upstream package: `@turnkey/encoding`
- Version: `0.6.0`
- Tarball sha256: `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`
- C# file git SHA: `a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80`

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---:|---|
| `Uint8ArrayToHexString` `src/Encoding.cs:90` | `ts-source/hex.ts:7` | REVIEWED | Lowercase hex and empty array behavior match. |
| `Uint8ArrayFromHexString` `src/Encoding.cs:118` | `ts-source/hex.ts:23` | REVIEWED | Validation, even length, left padding, and length `0` falsy behavior match. |
| `HexToAscii` `src/Encoding.cs:164` | `ts-source/hex.ts:58` | REVIEWED | Main pair parsing reviewed; divergence below for JS `parseInt` edge cases. |
| `JsParseIntBase16` `src/Encoding.cs:194` | inline `parseInt(..., 16)` at `ts-source/hex.ts:61` | REVIEWED | Helper is intended to emulate JS parsing; incomplete for whitespace/sign. |
| `NormalizePadding` `src/Encoding.cs:222` | `ts-source/hex.ts:73` | REVIEWED | Leading-zero add/strip and throw condition match. |
| `StringToBase64UrlString` `src/Encoding.cs:281` | `ts-source/base64.ts:13` | REVIEWED | 0-255 code-unit validation and base64url output match. |
| `HexStringToBase64Url` `src/Encoding.cs:308` | `ts-source/base64.ts:28` | REVIEWED | Odd hex padding, length handling, and base64url conversion match. |
| `Base64StringToBase64UrlEncodedString` `src/Encoding.cs:335` | `ts-source/base64.ts:45` | REVIEWED | `+`, `/`, and `=` normalization matches. |
| `Base64UrlToBase64` `src/Encoding.cs:349` | `ts-source/base64.ts:58` | REVIEWED | URL-safe replacement and padding restoration match. |
| `DecodeBase64UrlToString` `src/Encoding.cs:372` | `ts-source/base64.ts:74`, `ts-source/base64.ts:148` | REVIEWED | Lenient `atob` sanitizer and bit decoder match reviewed source. |
| `PointEncode` `src/Encoding.cs:447` | `ts-source/encode.ts:8` | REVIEWED | 65-byte uncompressed key, parity prefix, and x-copy match. |
| `Base58Encode` `src/Encoding.cs:481` | `ts-source/bs58.ts:18` / export `:36` | REVIEWED | Reviewed against exported `bs58.encode` contract and Bitcoin alphabet behavior. |
| `Base58Decode` `src/Encoding.cs:516` | `ts-source/bs58.ts:21` / export `:36` | REVIEWED | Reviewed against exported `bs58.decode` contract and leading-zero behavior. |
| `Base58CheckEncode` `src/Encoding.cs:565` | `ts-source/bs58check.ts:18` / export `:36` | REVIEWED | Payload + double-SHA256 checksum + base58 encode matches expected contract. |
| `Base58CheckDecode` `src/Encoding.cs:589` | `ts-source/bs58check.ts:20` / export `:36` | REVIEWED | Decode, split checksum, verify first 4 double-SHA256 bytes. |
| `Uint8ArrayToString` `src/Encoding.cs:633` | N/A | NOT-REVIEWED | Not in `@turnkey/encoding`; Unity/downstream helper only. |
| `ConcatUint8Arrays` `src/Encoding.cs:650` | N/A | NOT-REVIEWED | Not in `@turnkey/encoding`; structural spread helper only. |

**C. Intentional Adaptations**
- `Uint8Array` -> `byte[]`: same byte sequence representation.
- TS `reduce`/spread/slice -> C# loops, `Array.Copy`, `StringBuilder`: structural implementation change only.
- Pure JS `btoa` -> `Convert.ToBase64String` after 0-255 validation: same base64 bytes for valid inputs.
- Pure JS `atob` -> explicit C# sanitizer/bit decoder: preserves upstream lenient decoding behavior.
- `Error`/runtime throws -> `ArgumentException`/`ArgumentNullException`: C# exception type adaptation; no wire-byte change for valid typed inputs.
- TS regex -> `System.Text.RegularExpressions.Regex`: same hex validation pattern.
- `bs58`/`bs58check` npm packages -> local BigInteger/SHA256 implementation: same Bitcoin alphabet, leading-zero, and checksum wire format.
- `DEFAULT_JWK_MEMBER_BYTE_LENGTH` named export -> nested C# constant: same value, different namespace shape.
- Inline `TextDecoder`/spread patterns from downstream TS -> `Uint8ArrayToString`/`ConcatUint8Arrays`: helper extraction only.

**D. D17 Enforcement Check**
N/A (not Crypto.cs).

**E. Logic Divergence Findings**

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Encoding.cs:194` | `codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:61` | `HexToAscii` uses JS `parseInt(pair, 16)`. JS skips leading whitespace and accepts optional `+`/`-`; current C# helper stops at the first non-hex char, so inputs like `" f"`, `"+f"`, `"-f"`, or `"\tA"` decode differently. | Update `JsParseIntBase16` to emulate JS `parseInt` more completely: skip JS leading whitespace, parse optional sign, then parse the leading hex run; keep mapping no parsed digits to `0` for `String.fromCharCode(NaN)`. |

**F. Fixture Comparison Gate**
- Fixture exercising this file: `tests/Fixtures/encoding/turnkey-encoding-vectors.json`.
- Provenance: fixture declares `@turnkey/encoding@0.6.0`, tarball sha256 `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`, source `ts-source/__tests__/index-test.ts`.
- Static comparison: fixture values match the pinned upstream Jest vectors, and `tests/EncodingTests.cs` mirrors/asserts those bytes/strings inline.
- Node comparison: a local Node check over the fixture returned `fixture-node-check: ok`.
- `dotnet test --no-restore --no-build` could not complete in this read-only sandbox: MSBuild failed creating its named-pipe server with `SocketException (13): Permission denied`.

Fixture gate result: all fixture values match the pinned upstream/Node outputs; C# test execution was sandbox-blocked.

**G. Unresolved Assumptions**
- Exact invalid-input error messages from external `bs58@6.0.0` and `bs58check@4.0.0` were not fully re-verified because the snapshot has only Turnkey shims, not vendored dependency source or `node_modules`.
- The two Unity/downstream helpers have no `@turnkey/encoding` upstream counterpart, so they are documented as NOT-REVIEWED in B.

Round 2 pass criterion is not met because section E has one real divergence.


tokens used
114,667
**A. Source Pin Acknowledgement**
- Upstream package: `@turnkey/encoding`
- Version: `0.6.0`
- Tarball sha256: `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`
- C# file git SHA: `a1cabdff9c1b0dbd82e4c4fb5e0107a60c994c80`

**B. Method Coverage Table**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---:|---|
| `Uint8ArrayToHexString` `src/Encoding.cs:90` | `ts-source/hex.ts:7` | REVIEWED | Lowercase hex and empty array behavior match. |
| `Uint8ArrayFromHexString` `src/Encoding.cs:118` | `ts-source/hex.ts:23` | REVIEWED | Validation, even length, left padding, and length `0` falsy behavior match. |
| `HexToAscii` `src/Encoding.cs:164` | `ts-source/hex.ts:58` | REVIEWED | Main pair parsing reviewed; divergence below for JS `parseInt` edge cases. |
| `JsParseIntBase16` `src/Encoding.cs:194` | inline `parseInt(..., 16)` at `ts-source/hex.ts:61` | REVIEWED | Helper is intended to emulate JS parsing; incomplete for whitespace/sign. |
| `NormalizePadding` `src/Encoding.cs:222` | `ts-source/hex.ts:73` | REVIEWED | Leading-zero add/strip and throw condition match. |
| `StringToBase64UrlString` `src/Encoding.cs:281` | `ts-source/base64.ts:13` | REVIEWED | 0-255 code-unit validation and base64url output match. |
| `HexStringToBase64Url` `src/Encoding.cs:308` | `ts-source/base64.ts:28` | REVIEWED | Odd hex padding, length handling, and base64url conversion match. |
| `Base64StringToBase64UrlEncodedString` `src/Encoding.cs:335` | `ts-source/base64.ts:45` | REVIEWED | `+`, `/`, and `=` normalization matches. |
| `Base64UrlToBase64` `src/Encoding.cs:349` | `ts-source/base64.ts:58` | REVIEWED | URL-safe replacement and padding restoration match. |
| `DecodeBase64UrlToString` `src/Encoding.cs:372` | `ts-source/base64.ts:74`, `ts-source/base64.ts:148` | REVIEWED | Lenient `atob` sanitizer and bit decoder match reviewed source. |
| `PointEncode` `src/Encoding.cs:447` | `ts-source/encode.ts:8` | REVIEWED | 65-byte uncompressed key, parity prefix, and x-copy match. |
| `Base58Encode` `src/Encoding.cs:481` | `ts-source/bs58.ts:18` / export `:36` | REVIEWED | Reviewed against exported `bs58.encode` contract and Bitcoin alphabet behavior. |
| `Base58Decode` `src/Encoding.cs:516` | `ts-source/bs58.ts:21` / export `:36` | REVIEWED | Reviewed against exported `bs58.decode` contract and leading-zero behavior. |
| `Base58CheckEncode` `src/Encoding.cs:565` | `ts-source/bs58check.ts:18` / export `:36` | REVIEWED | Payload + double-SHA256 checksum + base58 encode matches expected contract. |
| `Base58CheckDecode` `src/Encoding.cs:589` | `ts-source/bs58check.ts:20` / export `:36` | REVIEWED | Decode, split checksum, verify first 4 double-SHA256 bytes. |
| `Uint8ArrayToString` `src/Encoding.cs:633` | N/A | NOT-REVIEWED | Not in `@turnkey/encoding`; Unity/downstream helper only. |
| `ConcatUint8Arrays` `src/Encoding.cs:650` | N/A | NOT-REVIEWED | Not in `@turnkey/encoding`; structural spread helper only. |

**C. Intentional Adaptations**
- `Uint8Array` -> `byte[]`: same byte sequence representation.
- TS `reduce`/spread/slice -> C# loops, `Array.Copy`, `StringBuilder`: structural implementation change only.
- Pure JS `btoa` -> `Convert.ToBase64String` after 0-255 validation: same base64 bytes for valid inputs.
- Pure JS `atob` -> explicit C# sanitizer/bit decoder: preserves upstream lenient decoding behavior.
- `Error`/runtime throws -> `ArgumentException`/`ArgumentNullException`: C# exception type adaptation; no wire-byte change for valid typed inputs.
- TS regex -> `System.Text.RegularExpressions.Regex`: same hex validation pattern.
- `bs58`/`bs58check` npm packages -> local BigInteger/SHA256 implementation: same Bitcoin alphabet, leading-zero, and checksum wire format.
- `DEFAULT_JWK_MEMBER_BYTE_LENGTH` named export -> nested C# constant: same value, different namespace shape.
- Inline `TextDecoder`/spread patterns from downstream TS -> `Uint8ArrayToString`/`ConcatUint8Arrays`: helper extraction only.

**D. D17 Enforcement Check**
N/A (not Crypto.cs).

**E. Logic Divergence Findings**

| C# file:line | TS upstream path:line | Semantic diff | Suggested fix |
|---|---|---|---|
| `src/Encoding.cs:194` | `codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/hex.ts:61` | `HexToAscii` uses JS `parseInt(pair, 16)`. JS skips leading whitespace and accepts optional `+`/`-`; current C# helper stops at the first non-hex char, so inputs like `" f"`, `"+f"`, `"-f"`, or `"\tA"` decode differently. | Update `JsParseIntBase16` to emulate JS `parseInt` more completely: skip JS leading whitespace, parse optional sign, then parse the leading hex run; keep mapping no parsed digits to `0` for `String.fromCharCode(NaN)`. |

**F. Fixture Comparison Gate**
- Fixture exercising this file: `tests/Fixtures/encoding/turnkey-encoding-vectors.json`.
- Provenance: fixture declares `@turnkey/encoding@0.6.0`, tarball sha256 `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`, source `ts-source/__tests__/index-test.ts`.
- Static comparison: fixture values match the pinned upstream Jest vectors, and `tests/EncodingTests.cs` mirrors/asserts those bytes/strings inline.
- Node comparison: a local Node check over the fixture returned `fixture-node-check: ok`.
- `dotnet test --no-restore --no-build` could not complete in this read-only sandbox: MSBuild failed creating its named-pipe server with `SocketException (13): Permission denied`.

Fixture gate result: all fixture values match the pinned upstream/Node outputs; C# test execution was sandbox-blocked.

**G. Unresolved Assumptions**
- Exact invalid-input error messages from external `bs58@6.0.0` and `bs58check@4.0.0` were not fully re-verified because the snapshot has only Turnkey shims, not vendored dependency source or `node_modules`.
- The two Unity/downstream helpers have no `@turnkey/encoding` upstream counterpart, so they are documented as NOT-REVIEWED in B.

Round 2 pass criterion is not met because section E has one real divergence.


