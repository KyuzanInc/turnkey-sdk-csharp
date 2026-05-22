# Codex review — src/Encoding.cs — round 3 — 20260523

C# SHA: `575885ce76757b984b4b060cd4b43d639b983231`
Upstream snapshot: `turnkey-encoding-0.6.0`

---

2026-05-22T23:41:19.742365Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
OpenAI Codex v0.129.0 (research preview)
--------
workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
model: gpt-5.5
provider: openai
approval: never
sandbox: read-only
reasoning effort: high
reasoning summaries: none
session id: 019e5210-9bfb-7792-a9d9-99801f09e79c
--------
user
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 575885ce76757b984b4b060cd4b43d639b983231)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND 3 of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA (575885ce76757b984b4b060cd4b43d639b983231).

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

2026-05-22T23:41:21.223416Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
codex
I’ll use the review workflow here because this is a line-by-line parity check, then I’ll pin the upstream snapshot and enumerate every helper in `src/Encoding.cs` before comparing fixtures.
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

codex
The installed review skill is oriented toward local diffs and changelog edits; this request is a pinned upstream parity audit, so I’m applying its review stance but keeping the workflow read-only and evidence-based.
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
   189	        /// JS-compatible <c>parseInt(s, 16)</c>: skips leading whitespace, parses
   190	        /// an optional sign, then parses the leading hex digits. Returns 0 for
   191	        /// inputs that JS would map to <c>NaN</c> (no parseable hex digits) so
   192	        /// that <c>String.fromCharCode(NaN)</c> behavior is preserved
   193	        /// (<c>NaN → U+0000</c>).
   194	        /// </summary>
   195	        /// <remarks>
   196	        /// JS whitespace per the spec includes the regular ASCII whitespace
   197	        /// characters and a handful of Unicode whitespace classes. The set
   198	        /// recognized here matches the characters JS treats as
   199	        /// <c>StrWhiteSpaceChar</c> for the purposes of <c>parseInt</c>:
   200	        /// <c>U+0009..U+000D</c>, <c>U+0020</c>, <c>U+00A0</c>, <c>U+1680</c>,
   201	        /// <c>U+2000..U+200A</c>, <c>U+2028</c>, <c>U+2029</c>, <c>U+202F</c>,
   202	        /// <c>U+205F</c>, <c>U+3000</c>, <c>U+FEFF</c>.
   203	        /// The sign is preserved per JS semantics; for HexToAscii output the
   204	        /// downstream <c>(char)</c> cast on a negative value wraps modulo 0x10000,
   205	        /// matching <c>String.fromCharCode</c>.
   206	        /// </remarks>
   207	        private static int JsParseIntBase16(string s)
   208	        {
   209	            if (string.IsNullOrEmpty(s))
   210	            {
   211	                return 0;
   212	            }
   213	            int i = 0;
   214	            // Skip leading whitespace (JS StrWhiteSpaceChar).
   215	            while (i < s.Length && IsJsWhitespace(s[i]))
   216	            {
   217	                i++;
   218	            }
   219	            // Optional sign.
   220	            int sign = 1;
   221	            if (i < s.Length && (s[i] == '+' || s[i] == '-'))
   222	            {
   223	                if (s[i] == '-')
   224	                {
   225	                    sign = -1;
   226	                }
   227	                i++;
   228	            }
   229	            // Parse hex run.
   230	            int value = 0;
   231	            int parsedDigits = 0;
   232	            while (i < s.Length)
   233	            {
   234	                char c = s[i];
   235	                int digit;
   236	                if (c >= '0' && c <= '9') digit = c - '0';
   237	                else if (c >= 'a' && c <= 'f') digit = 10 + (c - 'a');
   238	                else if (c >= 'A' && c <= 'F') digit = 10 + (c - 'A');
   239	                else break;
   240	                value = value * 16 + digit;
   241	                parsedDigits++;
   242	                i++;
   243	            }
   244	            // JS parseInt returns NaN when no digits parsed; String.fromCharCode(NaN) = 0.
   245	            return parsedDigits == 0 ? 0 : sign * value;
   246	        }
   247	
   248	        private static bool IsJsWhitespace(char c)
   249	        {
   250	            // JS StrWhiteSpaceChar per ECMA-262 14.0:
   251	            //   https://262.ecma-international.org/14.0/#prod-StrWhiteSpaceChar
   252	            switch (c)
   253	            {
   254	                case '\u0009': // TAB
   255	                case '\u000A': // LF
   256	                case '\u000B': // VT
   257	                case '\u000C': // FF
   258	                case '\u000D': // CR
   259	                case '\u0020': // SP
   260	                case '\u00A0': // NBSP

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
/bin/zsh -lc "nl -ba codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt | sed -n '1,200p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd  turnkey-api-key-stamper-0.5.0.tgz
     2	75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3  turnkey-crypto-2.8.8.tgz
     3	2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc  turnkey-encoding-0.6.0.tgz
     4	d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf  turnkey-http-3.16.0.tgz

exec
/bin/zsh -lc 'git status --short && git rev-parse --show-toplevel && git log --oneline -- src/Encoding.cs | head -5' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 5546ms:
2026-05-23 08:41:42.039 xcodebuild[56514:105937681]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:41:42.298 xcodebuild[56514:105937680] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-9J2wN8MH' (errno=Operation not permitted)
2026-05-23 08:41:42.819 xcodebuild[56532:105937721]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:41:42.953 xcodebuild[56532:105937720] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
?? codex-crypto-reviews/Encoding.cs-r3-20260523.md
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-n0EwptIW' (errno=Operation not permitted)
2026-05-23 08:41:43.850 xcodebuild[56537:105937750]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:41:43.977 xcodebuild[56537:105937749] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-6VYGtxM5' (errno=Operation not permitted)
2026-05-23 08:41:44.378 xcodebuild[56541:105937801]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:41:44.518 xcodebuild[56541:105937800] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-paAkdBeD' (errno=Operation not permitted)
2026-05-23 08:41:45.461 xcodebuild[56559:105937863]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:41:45.602 xcodebuild[56559:105937862] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-J4JvxZhF' (errno=Operation not permitted)
2026-05-23 08:41:46.013 xcodebuild[56561:105937873]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:41:46.162 xcodebuild[56561:105937872] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
575885c Fix JsParseIntBase16 JS-parity (whitespace + sign) per Codex r2
a1cabdf Fix Encoding.cs divergences flagged by Codex r1; add fixtures dir
0ffef78 Port Encoding.cs from @turnkey/encoding@0.6.0

exec
/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '248,560p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   248	        private static bool IsJsWhitespace(char c)
   249	        {
   250	            // JS StrWhiteSpaceChar per ECMA-262 14.0:
   251	            //   https://262.ecma-international.org/14.0/#prod-StrWhiteSpaceChar
   252	            switch (c)
   253	            {
   254	                case '\u0009': // TAB
   255	                case '\u000A': // LF
   256	                case '\u000B': // VT
   257	                case '\u000C': // FF
   258	                case '\u000D': // CR
   259	                case '\u0020': // SP
   260	                case '\u00A0': // NBSP
   261	                case '\u1680':
   262	                case '\u2000':
   263	                case '\u2001':
   264	                case '\u2002':
   265	                case '\u2003':
   266	                case '\u2004':
   267	                case '\u2005':
   268	                case '\u2006':
   269	                case '\u2007':
   270	                case '\u2008':
   271	                case '\u2009':
   272	                case '\u200A':
   273	                case '\u2028': // LS
   274	                case '\u2029': // PS
   275	                case '\u202F': // NNBSP
   276	                case '\u205F': // MMSP
   277	                case '\u3000': // IDEOGRAPHIC SPACE
   278	                case '\uFEFF': // BOM / ZWNBSP
   279	                    return true;
   280	                default:
   281	                    return false;
   282	            }
   283	        }
   284	
   285	        /// <summary>
   286	        /// Normalizes the padding of a byte array to a target length by either
   287	        /// left-padding with zero bytes or stripping leading zero bytes.
   288	        /// Throws if too many non-zero bytes would have to be removed.
   289	        /// Upstream: <c>hex.ts normalizePadding</c>.
   290	        /// </summary>
   291	        public static byte[] NormalizePadding(byte[] byteArray, int targetLength)
   292	        {
   293	            if (byteArray == null)
   294	            {
   295	                throw new ArgumentNullException(nameof(byteArray));
   296	            }
   297	
   298	            int paddingLength = targetLength - byteArray.Length;
   299	
   300	            // Add leading zeros
   301	            if (paddingLength > 0)
   302	            {
   303	                var result = new byte[targetLength];
   304	                Array.Copy(byteArray, 0, result, paddingLength, byteArray.Length);
   305	                return result;
   306	            }
   307	
   308	            // Strip leading zeros
   309	            if (paddingLength < 0)
   310	            {
   311	                int expectedZeroCount = -paddingLength;
   312	                int zeroCount = 0;
   313	                for (int i = 0; i < expectedZeroCount && i < byteArray.Length; i++)
   314	                {
   315	                    if (byteArray[i] == 0)
   316	                    {
   317	                        zeroCount++;
   318	                    }
   319	                }
   320	                if (zeroCount != expectedZeroCount)
   321	                {
   322	                    throw new ArgumentException(
   323	                        "invalid number of starting zeroes. Expected number of zeroes: "
   324	                        + expectedZeroCount + ". Found: " + zeroCount + ".");
   325	                }
   326	
   327	                var result = new byte[targetLength];
   328	                Array.Copy(byteArray, expectedZeroCount, result, 0, targetLength);
   329	                return result;
   330	            }
   331	
   332	            return byteArray;
   333	        }
   334	
   335	        // ============================================================
   336	        // base64.ts
   337	        // ============================================================
   338	
   339	        /// <summary>
   340	        /// Converts a plain string into a base64url-encoded string.
   341	        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
   342	        /// </summary>
   343	        /// <remarks>
   344	        /// Upstream uses a pure-JS <c>btoa</c> for React Native compatibility and
   345	        /// throws on code points greater than 0xFF. This port preserves that
   346	        /// behavior by checking each code point against 0xFF before delegating to
   347	        /// <c>System.Convert.ToBase64String</c>, which is wire-identical for the
   348	        /// 0-255 byte range.
   349	        /// </remarks>
   350	        public static string StringToBase64UrlString(string input)
   351	        {
   352	            if (input == null)
   353	            {
   354	                throw new ArgumentNullException(nameof(input));
   355	            }
   356	
   357	            var bytes = new byte[input.Length];
   358	            for (int i = 0; i < input.Length; i++)
   359	            {
   360	                int cp = input[i];
   361	                if (cp > 0xFF)
   362	                {
   363	                    throw new ArgumentException(
   364	                        "InvalidCharacterError: found code point greater than 255:"
   365	                        + cp + " at position " + i);
   366	                }
   367	                bytes[i] = (byte)cp;
   368	            }
   369	
   370	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(bytes));
   371	        }
   372	
   373	        /// <summary>
   374	        /// Converts a hex string into a base64url-encoded string.
   375	        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
   376	        /// </summary>
   377	        public static string HexStringToBase64Url(string input, int? length = null)
   378	        {
   379	            if (input == null)
   380	            {
   381	                throw new ArgumentNullException(nameof(input));
   382	            }
   383	
   384	            // Add a leading 0 if needed to get an even-length hex string.
   385	            // padStart(Math.ceil(input.length / 2) * 2, "0")
   386	            int targetHexLen = ((input.Length + 1) / 2) * 2;
   387	            string hexString = input.Length < targetHexLen
   388	                ? input.PadLeft(targetHexLen, '0')
   389	                : input;
   390	
   391	            var buffer = Uint8ArrayFromHexString(hexString, length);
   392	
   393	            // Upstream then folds the buffer into a per-byte string via
   394	            // String.fromCharCode and calls btoa on that. Convert.ToBase64String
   395	            // on the raw bytes produces the same wire bytes (validated by tests).
   396	            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(buffer));
   397	        }
   398	
   399	        /// <summary>
   400	        /// Converts a standard base64 string into a base64url-encoded string by
   401	        /// replacing <c>+</c> with <c>-</c>, <c>/</c> with <c>_</c>, and stripping <c>=</c>.
   402	        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
   403	        /// </summary>
   404	        public static string Base64StringToBase64UrlEncodedString(string input)
   405	        {
   406	            if (input == null)
   407	            {
   408	                throw new ArgumentNullException(nameof(input));
   409	            }
   410	            return input.Replace('+', '-').Replace('/', '_').Replace("=", string.Empty);
   411	        }
   412	
   413	        /// <summary>
   414	        /// Converts a base64url string into a standard base64 string (with
   415	        /// <c>=</c> padding restored).
   416	        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
   417	        /// </summary>
   418	        public static string Base64UrlToBase64(string input)
   419	        {
   420	            if (input == null)
   421	            {
   422	                throw new ArgumentNullException(nameof(input));
   423	            }
   424	            string b64 = input.Replace('-', '+').Replace('_', '/');
   425	            int padLen = (4 - (b64.Length % 4)) % 4;
   426	            return b64 + new string('=', padLen);
   427	        }
   428	
   429	        /// <summary>
   430	        /// Decodes a base64url-encoded string into a plain string by first
   431	        /// restoring base64 padding and then base64-decoding into a byte buffer.
   432	        /// Each output byte is then reinterpreted as a code point.
   433	        /// Upstream: <c>base64.ts decodeBase64urlToString</c> + <c>atob</c>.
   434	        /// </summary>
   435	        /// <remarks>
   436	        /// Upstream <c>atob</c> silently strips any character not in
   437	        /// <c>[A-Za-z0-9+/=]</c> before decoding, and throws only when the
   438	        /// remaining length mod 4 is 1. This port reproduces that lenient
   439	        /// behavior so callers get the same string upstream JS would return.
   440	        /// </remarks>
   441	        public static string DecodeBase64UrlToString(string input)
   442	        {
   443	            if (input == null)
   444	            {
   445	                throw new ArgumentNullException(nameof(input));
   446	            }
   447	
   448	            string b64 = Base64UrlToBase64(input);
   449	
   450	            // Reproduce upstream atob: strip invalid characters first.
   451	            var filtered = new StringBuilder(b64.Length);
   452	            foreach (char c in b64)
   453	            {
   454	                if ((c >= 'A' && c <= 'Z')
   455	                    || (c >= 'a' && c <= 'z')
   456	                    || (c >= '0' && c <= '9')
   457	                    || c == '+' || c == '/' || c == '=')
   458	                {
   459	                    filtered.Append(c);
   460	                }
   461	            }
   462	            string sanitized = filtered.ToString();
   463	
   464	            // Upstream atob explicitly throws when (length % 4) == 1.
   465	            if (sanitized.Length % 4 == 1)
   466	            {
   467	                throw new ArgumentException(
   468	                    "InvalidCharacterError: The string to be decoded is not correctly encoded.");
   469	            }
   470	
   471	            // Upstream atob then walks the string and shifts in 6-bit groups,
   472	            // emitting full 8-bit bytes. Reproduce that bit-shifting decoder
   473	            // explicitly because it is more permissive than Convert.FromBase64String
   474	            // (it tolerates missing or partial padding the same way upstream does).
   475	            const string keyStr =
   476	                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
   477	            var sb = new StringBuilder();
   478	            int buffer = 0;
   479	            int bits = 0;
   480	            for (int i = 0; i < sanitized.Length; i++)
   481	            {
   482	                char ch = sanitized[i];
   483	                int index = keyStr.IndexOf(ch);
   484	                if (index < 0 || index > 64)
   485	                {
   486	                    continue;
   487	                }
   488	                if (ch == '=')
   489	                {
   490	                    bits = 0;
   491	                }
   492	                else
   493	                {
   494	                    buffer = (buffer << 6) | index;
   495	                    bits += 6;
   496	                }
   497	                while (bits >= 8)
   498	                {
   499	                    bits -= 8;
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

exec
/bin/zsh -lc 'for f in codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/ts-source/{hex,base64,encode,bs58,bs58check,index}.ts; do nl -ba "$f" | sed -n '"'1,240p'; done" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
     1	export const DEFAULT_JWK_MEMBER_BYTE_LENGTH = 32;
     2	
     3	export * from "./base64";
     4	export * from "./encode";
     5	export * from "./hex";
     6	
     7	export { bs58 } from "./bs58";
     8	export { bs58check } from "./bs58check";

exec
/bin/zsh -lc 'find tests/Fixtures -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md
tests/Fixtures/encoding/turnkey-encoding-vectors.json

exec
/bin/zsh -lc "rg -n \"Encoding|Uint8Array|HexToAscii|Base58|Base64|PointEncode|Fixtures|fixtures\" tests src -g '*.*'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md:1:# Test fixtures
tests/Fixtures/README.md:3:Each subdirectory groups fixtures for one production file, plus a
tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
tests/Fixtures/README.md:21:Fixtures/
tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
src/Encoding.cs:7://   ts-source/hex.ts          -> Uint8ArrayToHexString / Uint8ArrayFromHexString /
src/Encoding.cs:8://                                HexToAscii / NormalizePadding
src/Encoding.cs:9://   ts-source/base64.ts       -> StringToBase64UrlString /
src/Encoding.cs:10://                                HexStringToBase64Url /
src/Encoding.cs:11://                                Base64StringToBase64UrlEncodedString /
src/Encoding.cs:12://                                Base64UrlToBase64 /
src/Encoding.cs:13://                                DecodeBase64UrlToString
src/Encoding.cs:14://   ts-source/encode.ts       -> PointEncode
src/Encoding.cs:15://   ts-source/bs58.ts         -> Base58Encode / Base58Decode
src/Encoding.cs:16://   ts-source/bs58check.ts    -> Base58CheckEncode / Base58CheckDecode
src/Encoding.cs:20://   Uint8Array          -> byte[]
src/Encoding.cs:21://   String.fromCharCode(b) reduction -> direct byte buffer + Convert.ToBase64String
src/Encoding.cs:23://    Convert.ToBase64String when the input is 0-255-bounded, which it always
src/Encoding.cs:24://    is after hex -> Uint8Array conversion. The custom btoa only exists in
src/Encoding.cs:34://   Uint8ArrayToString -> UTF-8 decode helper (TS uses TextDecoder; in this
src/Encoding.cs:35://                         repo it is a thin wrapper over System.Text.Encoding.UTF8)
src/Encoding.cs:36://   ConcatUint8Arrays  -> array concatenation helper (TS spreads `...` in
src/Encoding.cs:49:    /// Encoding utilities for the Turnkey API.
src/Encoding.cs:52:    public static class Encoding
src/Encoding.cs:66:            /// Bitcoin / Base58 alphabet used by <c>bs58</c> and <c>bs58check</c>.
src/Encoding.cs:72:            /// Standard base64 alphabet used by btoa / Convert.ToBase64String.
src/Encoding.cs:90:        public static string Uint8ArrayToHexString(byte[] input)
src/Encoding.cs:118:        public static byte[] Uint8ArrayFromHexString(string hexString, int? length = null)
src/Encoding.cs:164:        public static string HexToAscii(string hexString)
src/Encoding.cs:203:        /// The sign is preserved per JS semantics; for HexToAscii output the
src/Encoding.cs:341:        /// Upstream: <c>base64.ts stringToBase64urlString</c>.
src/Encoding.cs:347:        /// <c>System.Convert.ToBase64String</c>, which is wire-identical for the
src/Encoding.cs:350:        public static string StringToBase64UrlString(string input)
src/Encoding.cs:370:            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(bytes));
src/Encoding.cs:375:        /// Upstream: <c>base64.ts hexStringToBase64url</c>.
src/Encoding.cs:377:        public static string HexStringToBase64Url(string input, int? length = null)
src/Encoding.cs:391:            var buffer = Uint8ArrayFromHexString(hexString, length);
src/Encoding.cs:394:            // String.fromCharCode and calls btoa on that. Convert.ToBase64String
src/Encoding.cs:396:            return Base64StringToBase64UrlEncodedString(Convert.ToBase64String(buffer));
src/Encoding.cs:402:        /// Upstream: <c>base64.ts base64StringToBase64UrlEncodedString</c>.
src/Encoding.cs:404:        public static string Base64StringToBase64UrlEncodedString(string input)
src/Encoding.cs:416:        /// Upstream: <c>base64.ts base64UrlToBase64</c>.
src/Encoding.cs:418:        public static string Base64UrlToBase64(string input)
src/Encoding.cs:433:        /// Upstream: <c>base64.ts decodeBase64urlToString</c> + <c>atob</c>.
src/Encoding.cs:441:        public static string DecodeBase64UrlToString(string input)
src/Encoding.cs:448:            string b64 = Base64UrlToBase64(input);
src/Encoding.cs:473:            // explicitly because it is more permissive than Convert.FromBase64String
src/Encoding.cs:516:        public static byte[] PointEncode(byte[] raw)
src/Encoding.cs:547:        /// Base58 (Bitcoin alphabet) encode of a byte array, without checksum.
src/Encoding.cs:550:        public static string Base58Encode(byte[] data)
src/Encoding.cs:572:            // Preserve leading zero bytes as '1' characters (Base58 convention).
src/Encoding.cs:582:        /// Base58 (Bitcoin alphabet) decode of a string, without checksum.
src/Encoding.cs:585:        public static byte[] Base58Decode(string encoded)
src/Encoding.cs:631:        /// Base58Check encode (Bitcoin-style: payload || SHA256(SHA256(payload))[0:4]).
src/Encoding.cs:634:        public static string Base58CheckEncode(byte[] payload)
src/Encoding.cs:649:                return Base58Encode(concat);
src/Encoding.cs:654:        /// Base58Check decode (Bitcoin-style). Verifies the trailing 4-byte
src/Encoding.cs:658:        public static byte[] Base58CheckDecode(string encoded)
src/Encoding.cs:660:            byte[] decoded = Base58Decode(encoded);
src/Encoding.cs:663:                throw new ArgumentException("Invalid Base58Check string - too short");
src/Encoding.cs:678:                    throw new ArgumentException("Invalid Base58Check checksum");
src/Encoding.cs:702:        public static string Uint8ArrayToString(byte[] bytes)
src/Encoding.cs:708:            return System.Text.Encoding.UTF8.GetString(bytes);
src/Encoding.cs:715:        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
src/Encoding.cs:719:        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
src/packages.lock.json:21:          "System.Text.Encodings.Web": "8.0.0",
src/packages.lock.json:55:      "System.Text.Encodings.Web": {
tests/Fixtures/encoding/turnkey-encoding-vectors.json:9:  "stringToBase64UrlString": [
tests/Fixtures/encoding/turnkey-encoding-vectors.json:23:  "base64StringToBase64UrlEncodedString": [
tests/Fixtures/encoding/turnkey-encoding-vectors.json:66:  "hexStringToBase64Url": [
tests/turnkey-sdk-csharp.Tests.csproj:24:    <None Include="Fixtures\**\*">

exec
/bin/zsh -lc "nl -ba src/Encoding.cs | sed -n '546,780p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
   691	        // ============================================================
   692	
   693	        /// <summary>
   694	        /// UTF-8 decodes a byte array into a string.
   695	        /// </summary>
   696	        /// <remarks>
   697	        /// Upstream uses <c>new TextDecoder().decode(bytes)</c> inline. This
   698	        /// helper centralizes the equivalent in the .NET port. It is
   699	        /// wire-irrelevant — it is invoked after wire bytes have already been
   700	        /// decided.
   701	        /// </remarks>
   702	        public static string Uint8ArrayToString(byte[] bytes)
   703	        {
   704	            if (bytes == null)
   705	            {
   706	                throw new ArgumentNullException(nameof(bytes));
   707	            }
   708	            return System.Text.Encoding.UTF8.GetString(bytes);
   709	        }
   710	
   711	        /// <summary>
   712	        /// Concatenates byte arrays.
   713	        /// </summary>
   714	        /// <remarks>
   715	        /// Upstream uses <c>new Uint8Array([...a, ...b])</c> spread literals.
   716	        /// This helper centralizes the equivalent. It is wire-irrelevant — it
   717	        /// just glues bytes that were already computed.
   718	        /// </remarks>
   719	        public static byte[] ConcatUint8Arrays(params byte[][] arrays)
   720	        {
   721	            if (arrays == null)
   722	            {
   723	                throw new ArgumentNullException(nameof(arrays));
   724	            }
   725	            int total = arrays.Sum(a => a?.Length ?? 0);
   726	            var result = new byte[total];
   727	            int offset = 0;
   728	            foreach (var a in arrays)
   729	            {
   730	                if (a != null && a.Length > 0)
   731	                {
   732	                    Array.Copy(a, 0, result, offset, a.Length);
   733	                    offset += a.Length;
   734	                }
   735	            }
   736	            return result;
   737	        }
   738	    }
   739	}

codex
The C# file has 14 callable methods to cover, including the two private JS-parity helpers. I’ve also confirmed the upstream source files are present under the pinned snapshot, not just compiled `dist` files.
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
/bin/zsh -lc 'find tests -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
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
/bin/zsh -lc "rg -n \"turnkey-encoding-vectors|Encoding\\.|Uint8ArrayToHexString|DecodeBase64UrlToString|Base58Check|Base58Decode|NormalizePadding|PointEncode|HexToAscii|JsParseInt\" tests -g '*.*'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
tests/Fixtures/README.md:23:│   └── turnkey-encoding-vectors.json       — upstream-test-vectors

exec
/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '1,320p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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
   158	        public void HexToAscii_JsParityWhitespace()
   159	        {
   160	            // JS: parseInt(' f', 16) === 15 -> String.fromCharCode(15) === '\u000F'
   161	            // (leading whitespace skipped per JS spec)
   162	            Encoding.HexToAscii(" f").Should().Be("\u000F");
   163	        }
   164	
   165	        [Fact]
   166	        public void HexToAscii_JsParitySign()
   167	        {
   168	            // JS: parseInt('+f', 16) === 15 -> '\u000F'
   169	            Encoding.HexToAscii("+f").Should().Be("\u000F");
   170	        }
   171	
   172	        [Fact]
   173	        public void HexToAscii_JsParityTabBeforeHex()
   174	        {
   175	            // JS: parseInt('\tA', 16) === 10 -> '\u000A' (LF)
   176	            Encoding.HexToAscii("\tA").Should().Be("\u000A");
   177	        }
   178	
   179	        [Fact]
   180	        public void Uint8ArrayFromHexString_LengthZero_TreatedAsOmitted()
   181	        {
   182	            // JS: `if (!length)` is true for 0; behavior is identical to omitted length.
   183	            Encoding.Uint8ArrayFromHexString("01", 0).Should().Equal(new byte[] { 1 });
   184	        }
   185	
   186	        [Fact]
   187	        public void NormalizePadding_AddsLeadingZeros()
   188	        {
   189	            byte[] input = { 0x01, 0x02 };
   190	            Encoding.NormalizePadding(input, 4).Should().Equal(new byte[] { 0, 0, 1, 2 });
   191	        }
   192	
   193	        [Fact]
   194	        public void NormalizePadding_StripsLeadingZeros()
   195	        {
   196	            byte[] input = { 0x00, 0x00, 0x01, 0x02 };
   197	            Encoding.NormalizePadding(input, 2).Should().Equal(new byte[] { 1, 2 });
   198	        }
   199	
   200	        [Fact]
   201	        public void NormalizePadding_StripWouldRemoveNonZero_Throws()
   202	        {
   203	            byte[] input = { 0x01, 0x02, 0x03, 0x04 };
   204	            Action act = () => Encoding.NormalizePadding(input, 2);
   205	            act.Should().Throw<ArgumentException>()
   206	               .WithMessage("invalid number of starting zeroes*");
   207	        }
   208	
   209	        [Fact]
   210	        public void NormalizePadding_EqualLength_Identity()
   211	        {
   212	            byte[] input = { 0x01, 0x02 };
   213	            Encoding.NormalizePadding(input, 2).Should().Equal(input);
   214	        }
   215	
   216	        // ========================================================
   217	        // base64url
   218	        // ========================================================
   219	
   220	        [Fact]
   221	        public void StringToBase64UrlString_UpstreamHelloVector()
   222	        {
   223	            Encoding.StringToBase64UrlString("hello").Should().Be("aGVsbG8");
   224	        }
   225	
   226	        [Fact]
   227	        public void StringToBase64UrlString_UpstreamPrivateKeyVector()
   228	        {
   229	            // From index-test.ts
   230	            const string input =
   231	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1";
   232	            const string expected =
   233	                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
   234	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
   235	        }
   236	
   237	        [Fact]
   238	        public void StringToBase64UrlString_UpstreamApiKeyStampVector()
   239	        {
   240	            const string input =
   241	                "{\"publicKey\":\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\","
   242	                + "\"signature\":\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\","
   243	                + "\"scheme\":\"SIGNATURE_SCHEME_TK_API_P256\"}";
   244	            const string expected =
   245	                "eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ";
   246	            Encoding.StringToBase64UrlString(input).Should().Be(expected);
   247	        }
   248	
   249	        [Fact]
   250	        public void StringToBase64UrlString_CodePointAbove255_Throws()
   251	        {
   252	            Action act = () => Encoding.StringToBase64UrlString("aĀb");
   253	            act.Should().Throw<ArgumentException>()
   254	               .WithMessage("InvalidCharacterError: found code point greater than 255:256 at position 1");
   255	        }
   256	
   257	        [Fact]
   258	        public void Base64StringToBase64UrlEncodedString_UpstreamVectors()
   259	        {
   260	            Encoding.Base64StringToBase64UrlEncodedString("aGVsbG8gd29ybGQ=")
   261	                .Should().Be("aGVsbG8gd29ybGQ");
   262	            Encoding.Base64StringToBase64UrlEncodedString("U29tZSBzYW1wbGUgdGV4dA==")
   263	                .Should().Be("U29tZSBzYW1wbGUgdGV4dA");
   264	        }
   265	
   266	        [Fact]
   267	        public void HexStringToBase64Url_UpstreamVectors()
   268	        {
   269	            Encoding.HexStringToBase64Url("01").Should().Be("AQ");
   270	            Encoding.HexStringToBase64Url("01", 2).Should().Be("AAE");
   271	            Encoding.HexStringToBase64Url("ff").Should().Be("_w");
   272	            Encoding.HexStringToBase64Url("ff", 2).Should().Be("AP8");
   273	        }
   274	
   275	        [Fact]
   276	        public void HexStringToBase64Url_TooLong_Throws()
   277	        {
   278	            Action act = () => Encoding.HexStringToBase64Url("0100", 1);
   279	            act.Should().Throw<ArgumentException>()
   280	               .WithMessage("hex value cannot fit in a buffer of 1 byte(s)");
   281	        }
   282	
   283	        [Fact]
   284	        public void Base64UrlToBase64_RestoresPadding()
   285	        {
   286	            Encoding.Base64UrlToBase64("aGVsbG8gd29ybGQ").Should().Be("aGVsbG8gd29ybGQ=");
   287	            Encoding.Base64UrlToBase64("U29tZSBzYW1wbGUgdGV4dA").Should().Be("U29tZSBzYW1wbGUgdGV4dA==");
   288	            Encoding.Base64UrlToBase64("AQ").Should().Be("AQ==");
   289	        }
   290	
   291	        [Fact]
   292	        public void Base64UrlToBase64_NoChangeWhenAlreadyPadded()
   293	        {
   294	            // "AAAA" length 4, padding 0
   295	            Encoding.Base64UrlToBase64("AAAA").Should().Be("AAAA");
   296	        }
   297	
   298	        [Fact]
   299	        public void DecodeBase64UrlToString_Roundtrip()
   300	        {
   301	            Encoding.DecodeBase64UrlToString("aGVsbG8").Should().Be("hello");
   302	            // From upstream private-key encoding vector
   303	            const string b64url =
   304	                "NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ";
   305	            Encoding.DecodeBase64UrlToString(b64url).Should().Be(
   306	                "5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1");
   307	        }
   308	
   309	        [Fact]
   310	        public void DecodeBase64UrlToString_StripsInvalidChars()
   311	        {
   312	            // JS atob silently strips chars not in [A-Za-z0-9+/=] before decoding.
   313	            // "aGVsbG8" decodes to "hello"; "a*GVsbG8" must too.
   314	            Encoding.DecodeBase64UrlToString("a*GVsbG8").Should().Be("hello");
   315	        }
   316	
   317	        [Fact]
   318	        public void DecodeBase64UrlToString_LengthMod4Is1_Throws()
   319	        {
   320	            // JS atob: throws when (filtered length) % 4 === 1.

exec
/bin/zsh -lc "nl -ba tests/EncodingTests.cs | sed -n '318,700p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
   318	        public void DecodeBase64UrlToString_LengthMod4Is1_Throws()
   319	        {
   320	            // JS atob: throws when (filtered length) % 4 === 1.
   321	            // 5 chars from [A-Za-z0-9] (no = since base64 padding is +0/2/3),
   322	            // restoring base64 padding adds 3 '='s => total length 8 mod 4 = 0,
   323	            // so we need to confirm the post-padding check triggers.
   324	            // The upstream rule fires on the filtered (post-replace) length;
   325	            // construct an input that yields exactly mod-4==1 after restore.
   326	            // "a" base64url -> "a===" base64 -> filtered length stays 4. mod4=0.
   327	            // We must construct mod-4==1 *after* padding: e.g. "ab" -> "ab==" len 4 mod4=0.
   328	            // The only way to hit mod4==1 is if input strips to something like "a"
   329	            // before padding; Base64UrlToBase64("a") => "a===" mod4=0. So in practice
   330	            // the throw is unreachable via Base64UrlToBase64 input; we exercise the
   331	            // raw path with a manually crafted length-5 string.
   332	            // Use bytes with a leading '=' inside which atob treats as padding reset.
   333	            // Easiest: stamp the helper directly via a length that hits mod-4==1.
   334	            // The Base64UrlToBase64 sanitizer adds '=' until len % 4 == 0, so the
   335	            // only way to trip the throw is when an embedded '=' is followed by
   336	            // material that brings the total to a mod-4==1 residue. We skip the
   337	            // throw test here because the public API path cannot produce it; the
   338	            // throw is kept defensively to match upstream.
   339	            // Sanity check: trivial valid input still decodes.
   340	            Encoding.DecodeBase64UrlToString("AQ").Should().Be("");
   341	        }
   342	
   343	        // ========================================================
   344	        // PointEncode
   345	        // ========================================================
   346	
   347	        [Fact]
   348	        public void PointEncode_EvenY_PrefixIs02()
   349	        {
   350	            var raw = new byte[65];
   351	            raw[0] = 0x04;
   352	            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
   353	            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
   354	            raw[64] = 0xBE; // last byte even
   355	
   356	            var compressed = Encoding.PointEncode(raw);
   357	
   358	            compressed.Should().HaveCount(33);
   359	            compressed[0].Should().Be(0x02);
   360	            for (int i = 1; i < 33; i++) compressed[i].Should().Be(0xAA);
   361	        }
   362	
   363	        [Fact]
   364	        public void PointEncode_OddY_PrefixIs03()
   365	        {
   366	            var raw = new byte[65];
   367	            raw[0] = 0x04;
   368	            for (int i = 1; i < 33; i++) raw[i] = 0xAA;
   369	            for (int i = 33; i < 65; i++) raw[i] = 0xBB;
   370	            raw[64] = 0xBD; // last byte odd
   371	
   372	            var compressed = Encoding.PointEncode(raw);
   373	
   374	            compressed[0].Should().Be(0x03);
   375	        }
   376	
   377	        [Fact]
   378	        public void PointEncode_InvalidPrefix_Throws()
   379	        {
   380	            var raw = new byte[65];
   381	            raw[0] = 0x05;
   382	            Action act = () => Encoding.PointEncode(raw);
   383	            act.Should().Throw<ArgumentException>()
   384	               .WithMessage("Invalid uncompressed P-256 key");
   385	        }
   386	
   387	        [Fact]
   388	        public void PointEncode_WrongLength_Throws()
   389	        {
   390	            var raw = new byte[64];
   391	            raw[0] = 0x04;
   392	            Action act = () => Encoding.PointEncode(raw);
   393	            act.Should().Throw<ArgumentException>()
   394	               .WithMessage("Invalid uncompressed P-256 key");
   395	        }
   396	
   397	        // ========================================================
   398	        // Base58 / Base58Check
   399	        // ========================================================
   400	
   401	        [Fact]
   402	        public void Base58Encode_KnownVector()
   403	        {
   404	            // From "Hello World" -> hex "48656c6c6f20576f726c64" -> base58 "JxF12TrwUP45BMd"
   405	            // (well-known Bitcoin reference vector)
   406	            byte[] data = Encoding.Uint8ArrayFromHexString("48656c6c6f20576f726c64");
   407	            Encoding.Base58Encode(data).Should().Be("JxF12TrwUP45BMd");
   408	        }
   409	
   410	        [Fact]
   411	        public void Base58Decode_KnownVector()
   412	        {
   413	            byte[] decoded = Encoding.Base58Decode("JxF12TrwUP45BMd");
   414	            Encoding.Uint8ArrayToHexString(decoded).Should().Be("48656c6c6f20576f726c64");
   415	        }
   416	
   417	        [Fact]
   418	        public void Base58_Roundtrip_PreservesLeadingZero()
   419	        {
   420	            byte[] data = { 0x00, 0x01, 0x02, 0x03 };
   421	            string encoded = Encoding.Base58Encode(data);
   422	            encoded.Should().StartWith("1"); // leading-zero convention
   423	            byte[] decoded = Encoding.Base58Decode(encoded);
   424	            decoded.Should().Equal(data);
   425	        }
   426	
   427	        [Fact]
   428	        public void Base58_Empty()
   429	        {
   430	            Encoding.Base58Encode(Array.Empty<byte>()).Should().Be(string.Empty);
   431	            Encoding.Base58Decode(string.Empty).Should().BeEmpty();
   432	        }
   433	
   434	        [Fact]
   435	        public void Base58Check_Roundtrip()
   436	        {
   437	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
   438	            string encoded = Encoding.Base58CheckEncode(payload);
   439	            byte[] decoded = Encoding.Base58CheckDecode(encoded);
   440	            decoded.Should().Equal(payload);
   441	        }
   442	
   443	        [Fact]
   444	        public void Base58CheckDecode_TamperedChecksum_Throws()
   445	        {
   446	            byte[] payload = Encoding.Uint8ArrayFromHexString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31");
   447	            string encoded = Encoding.Base58CheckEncode(payload);
   448	            // Flip one character to corrupt the checksum
   449	            char[] chars = encoded.ToCharArray();
   450	            chars[chars.Length - 1] = chars[chars.Length - 1] == 'A' ? 'B' : 'A';
   451	            string tampered = new string(chars);
   452	
   453	            Action act = () => Encoding.Base58CheckDecode(tampered);
   454	            act.Should().Throw<ArgumentException>();
   455	        }
   456	
   457	        [Fact]
   458	        public void Base58CheckDecode_TooShort_Throws()
   459	        {
   460	            // Empty payload encoded would still produce 4-byte checksum + something,
   461	            // but we just feed a 3-byte raw decoded result by abusing a known short string.
   462	            Action act = () => Encoding.Base58CheckDecode("11"); // decodes to 1 byte
   463	            act.Should().Throw<ArgumentException>()
   464	               .WithMessage("Invalid Base58Check string - too short");
   465	        }
   466	
   467	        // ========================================================
   468	        // Helpers
   469	        // ========================================================
   470	
   471	        [Fact]
   472	        public void Uint8ArrayToString_DecodesUtf8()
   473	        {
   474	            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("こんにちは");
   475	            Encoding.Uint8ArrayToString(bytes).Should().Be("こんにちは");
   476	        }
   477	
   478	        [Fact]
   479	        public void ConcatUint8Arrays_ConcatsAndHandlesNull()
   480	        {
   481	            byte[] a = { 1, 2 };
   482	            byte[] b = { 3 };
   483	            byte[] c = { 4, 5, 6 };
   484	            Encoding.ConcatUint8Arrays(a, null!, b, c).Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
   485	        }
   486	    }
   487	}

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
/bin/zsh -lc "nl -ba codex-crypto-reviews/Encoding.cs-r3-20260523.md | sed -n '1,260p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
     1	# Codex review — src/Encoding.cs — round 3 — 20260523
     2	
     3	C# SHA: `575885ce76757b984b4b060cd4b43d639b983231`
     4	Upstream snapshot: `turnkey-encoding-0.6.0`
     5	
     6	---
     7	
     8	2026-05-22T23:41:19.742365Z ERROR codex_core::session: failed to load skill /Users/takeshi/.agents/skills/game-ui-design/SKILL.md: invalid description: exceeds maximum length of 1024 characters
     9	OpenAI Codex v0.129.0 (research preview)
    10	--------
    11	workdir: /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
    12	model: gpt-5.5
    13	provider: openai
    14	approval: never
    15	sandbox: read-only
    16	reasoning effort: high
    17	reasoning summaries: none
    18	session id: 019e5210-9bfb-7792-a9d9-99801f09e79c
    19	--------
    20	user
    21	You are reviewing a C# port of the Turnkey TypeScript SDK.
    22	File under review (C#): src/Encoding.cs  (git SHA at last commit touching it: 575885ce76757b984b4b060cd4b43d639b983231)
    23	Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0
    24	Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt
    25	
    26	This is REVIEW ROUND 3 of 3 for this file.
    27	
    28	REQUIRED OUTPUTS (sections A through G must all appear):
    29	
    30	A. Source pin acknowledgement:
    31	   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
    32	   C# file git SHA (575885ce76757b984b4b060cd4b43d639b983231).
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
    70	2026-05-22T23:41:21.223416Z ERROR rmcp::transport::worker: worker quit with fatal: Transport channel closed, when AuthRequired(AuthRequiredError { www_authenticate_header: "Bearer resource_metadata=\"https://mcp.figma.com/.well-known/oauth-protected-resource\",scope=\"mcp:connect\",authorization_uri=\"https://api.figma.com/.well-known/oauth-authorization-server\"" })
    71	codex
    72	I’ll use the review workflow here because this is a line-by-line parity check, then I’ll pin the upstream snapshot and enumerate every helper in `src/Encoding.cs` before comparing fixtures.
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
/bin/zsh -lc "rg -n \"\\*\\*A\\.|Source pin|Logic divergence|Fixture|PASS|Final\" codex-crypto-reviews/Encoding.cs-r3-20260523.md | head -40" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
30:A. Source pin acknowledgement:
50:E. Logic divergence findings: every place C# behavior differs from upstream TS:
58:F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
64:PASS criterion for this round: B has zero NOT-REVIEWED rows (or each is
1383:/bin/zsh -lc 'find tests/Fixtures -maxdepth 3 -type f | sort' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
1385:tests/Fixtures/README.md
1386:tests/Fixtures/encoding/turnkey-encoding-vectors.json
1389:/bin/zsh -lc "rg -n \"Encoding|Uint8Array|HexToAscii|Base58|Base64|PointEncode|Fixtures|fixtures\" tests src -g '*.*'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
1391:tests/Fixtures/README.md:1:# Test fixtures
1392:tests/Fixtures/README.md:3:Each subdirectory groups fixtures for one production file, plus a
1393:tests/Fixtures/README.md:15:  package inside `tests/Fixtures/Generators/`. The generator script,
1394:tests/Fixtures/README.md:21:Fixtures/
1395:tests/Fixtures/README.md:31:Fixtures sourced from upstream test files are stable as long as
binary file matches (found "\0" byte around offset 92793)

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
/bin/zsh -lc "rg -n \"Base58Encode|Base58Decode|Base58Check|bs58|pointEncode|normalizePadding|hexToAscii|decodeBase64\" codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist -g '*.js' -g '*.mjs'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:1:import * as raw from 'bs58';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:3:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:5:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:7:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:9:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:15:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:26:const bs58 = unwrap(raw);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:28:export { bs58 };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.mjs:29://# sourceMappingURL=bs58.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs:41:function hexToAscii(hexString) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs:55:const normalizePadding = (byteArray, targetLength) => {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.mjs:80:export { hexToAscii, normalizePadding, uint8ArrayFromHexString, uint8ArrayToHexString };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:3:var raw = require('bs58');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:24:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:26:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:28:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:30:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:36:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:47:const bs58 = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:49:exports.bs58 = bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:50://# sourceMappingURL=bs58.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:1:export { atob, base64StringToBase64UrlEncodedString, base64UrlToBase64, decodeBase64urlToString, hexStringToBase64url, stringToBase64urlString } from './base64.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:2:export { pointEncode } from './encode.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:3:export { hexToAscii, normalizePadding, uint8ArrayFromHexString, uint8ArrayToHexString } from './hex.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:4:export { bs58 } from './bs58.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.mjs:5:export { bs58check } from './bs58check.mjs';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.js:10:function pointEncode(raw) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.js:26:exports.pointEncode = pointEncode;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js:43:function hexToAscii(hexString) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js:57:const normalizePadding = (byteArray, targetLength) => {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js:82:exports.hexToAscii = hexToAscii;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/hex.js:83:exports.normalizePadding = normalizePadding;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:68:function decodeBase64urlToString(input) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.js:175:exports.decodeBase64urlToString = decodeBase64urlToString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:6:var bs58 = require('./bs58.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:7:var bs58check = require('./bs58check.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:14:exports.decodeBase64urlToString = base64.decodeBase64urlToString;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:17:exports.pointEncode = encode.pointEncode;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:18:exports.hexToAscii = hex.hexToAscii;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:19:exports.normalizePadding = hex.normalizePadding;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:22:exports.bs58 = bs58.bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:23:exports.bs58check = bs58check.bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:3:var raw = require('bs58check');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:24:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:26:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:28:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:30:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:36:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:47:const bs58check = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:49:exports.bs58check = bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:50://# sourceMappingURL=bs58check.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:1:import * as raw from 'bs58check';
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:3:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:5:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:7:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:9:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:15:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:26:const bs58check = unwrap(raw);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:28:export { bs58check };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.mjs:29://# sourceMappingURL=bs58check.mjs.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.mjs:8:function pointEncode(raw) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/encode.mjs:24:export { pointEncode };
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:66:function decodeBase64urlToString(input) {
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/base64.mjs:170:export { atob, base64StringToBase64UrlEncodedString, base64UrlToBase64, decodeBase64urlToString, hexStringToBase64url, stringToBase64urlString };

exec
/bin/zsh -lc 'rg -n "bs58|bs58check|base-x" codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/*.js' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:6:var bs58 = require('./bs58.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:7:var bs58check = require('./bs58check.js');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:22:exports.bs58 = bs58.bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js:23:exports.bs58check = bs58check.bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:3:var raw = require('bs58check');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:24:// This is a temporary shim for bs58check@4.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:26:// See: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:28:// bs58check v4.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:30:// `import bs58check from 'bs58check'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:36:// We can remove this shim once bs58check publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:47:const bs58check = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:49:exports.bs58check = bs58check;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58check.js:50://# sourceMappingURL=bs58check.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:3:var raw = require('bs58');
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:24:// This is a temporary shim for bs58@6.0.0
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:26:// This issue is similar to the one described here: https://github.com/bitcoinjs/bs58check/issues/47
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:28:// bs58 v6.0.0 uses ESM with only a default export, which causes compatibility
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:30:// `import bs58 from 'bs58'`, Metro applies multiple levels of wrapping,
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:36:// We can remove this shim once bs58 publishes a version that properly re-exports
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:47:const bs58 = unwrap(raw__namespace);
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:49:exports.bs58 = bs58;
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js:50://# sourceMappingURL=bs58.js.map
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:38:    "bs58": "6.0.0",
codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/package.json:39:    "bs58check": "4.0.0"

exec
/bin/zsh -lc "nl -ba src/turnkey-sdk-csharp.csproj | sed -n '1,160p'; nl -ba tests/turnkey-sdk-csharp.Tests.csproj | sed -n '1,120p'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
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

exec
/bin/zsh -lc "node -e \"for (const s of ['0x','0X','0g','-f','-0','-x','+f',' f','\\tA']) console.log(JSON.stringify(s), parseInt(s,16), String.fromCharCode(parseInt(s,16)).charCodeAt(0))\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
"0x" NaN 0
"0X" NaN 0
"0g" 0 0
"-f" -15 65521
"-0" -0 0
"-x" NaN 0
"+f" 15 15
" f" 15 15
"\tA" 10 10

exec
/bin/zsh -lc "find . -path '*node_modules*' -type f \\( -path '*bs58*' -o -path '*base-x*' -o -path '*bs58check*' \\) | head -80" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
exec
/bin/zsh -lc "find ~/.npm -path '*bs58*' -o -path '*base-x*' -o -path '*bs58check*' 2>/dev/null | head -80" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 8065ms:
2026-05-22T23:43:36.893724Z ERROR codex_core::tools::router: error=write_stdin failed: stdin is closed for this session; rerun exec_command with tty=true to keep stdin open
exec
/bin/zsh -lc "node -e \"try{const p=require('./codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js'); console.log(Object.keys(p));}catch(e){console.error(e.message);process.exitCode=1}\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 exited 1 in 0ms:
Cannot find module 'bs58'
Require stack:
- /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/bs58.js
- /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/package/dist/index.js
- /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/[eval]

exec
/bin/zsh -lc "node -e \"const checks=[['hello','aGVsbG8'],['5234d08dfa2c815f3097b8ba848a28172e85bec78886e8e201afccb166fc54c1','NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ']]; for (const [input, expected] of checks) { const actual=Buffer.from(input,'binary').toString('base64url'); console.log(actual===expected, actual); }\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
true aGVsbG8
true NTIzNGQwOGRmYTJjODE1ZjMwOTdiOGJhODQ4YTI4MTcyZTg1YmVjNzg4ODZlOGUyMDFhZmNjYjE2NmZjNTRjMQ

exec
/bin/zsh -lc "node -e 'const input=\"{\\\"publicKey\\\":\\\"02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316\\\",\\\"signature\\\":\\\"304402202a92c24e4b4de3cdb5c05a2b1f42264ba8139cf66b2d1ecf0a09987ab9a2fecb02203bfd91d8c5e87f78da8b5cf5ddb27c96cb00b848797d0fc73bf371892c423f81\\\",\\\"scheme\\\":\\\"SIGNATURE_SCHEME_TK_API_P256\\\"}\"; const expected=\"eyJwdWJsaWNLZXkiOiIwMmY3MzlmOGM3N2IzMmY0ZDVmMTMyNjU4NjFmZWJkNzZlN2E5YzYxYTExNDBkMjk2YjhjMTYzMDI1MDg4NzAzMTYiLCJzaWduYXR1cmUiOiIzMDQ0MDIyMDJhOTJjMjRlNGI0ZGUzY2RiNWMwNWEyYjFmNDIyNjRiYTgxMzljZjY2YjJkMWVjZjBhMDk5ODdhYjlhMmZlY2IwMjIwM2JmZDkxZDhjNWU4N2Y3OGRhOGI1Y2Y1ZGRiMjdjOTZjYjAwYjg0ODc5N2QwZmM3M2JmMzcxODkyYzQyM2Y4MSIsInNjaGVtZSI6IlNJR05BVFVSRV9TQ0hFTUVfVEtfQVBJX1AyNTYifQ\"; const actual=Buffer.from(input,\"binary\").toString(\"base64url\"); console.log(actual===expected);'" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
true

exec
/bin/zsh -lc "node -e \"function f(input,len){const hex=input.padStart(Math.ceil(input.length/2)*2,'0'); let bytes=Buffer.from(hex,'hex'); if(len){ if(bytes.length>len) throw Error('too long'); const out=Buffer.alloc(len); bytes.copy(out,len-bytes.length); bytes=out;} return bytes.toString('base64url')} for(const [i,l,e] of [['01',undefined,'AQ'],['01',2,'AAE'],['ff',undefined,'_w'],['ff',2,'AP8']]) console.log(i,l,f(i,l)===e,f(i,l));\"" in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 0ms:
01 undefined true AQ
01 2 true AAE
ff undefined true _w
ff 2 true AP8

exec
/bin/zsh -lc 'git diff -- src/Encoding.cs && git show --stat --oneline --no-renames 575885ce76757b984b4b060cd4b43d639b983231 -- src/Encoding.cs' in /Users/takeshi/Kyuzan/src/turnkey-sdk-csharp
 succeeded in 3326ms:
2026-05-23 08:44:41.940 xcodebuild[60741:105950523]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:44:42.153 xcodebuild[60741:105950522] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-AiLtNLAm' (errno=Operation not permitted)
2026-05-23 08:44:42.568 xcodebuild[60751:105950554]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:44:42.691 xcodebuild[60751:105950553] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-5XqrW0FJ' (errno=Operation not permitted)
2026-05-23 08:44:43.563 xcodebuild[60772:105950611]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:44:43.686 xcodebuild[60772:105950610] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
git: warning: confstr() failed with code 5: couldn't get path of DARWIN_USER_TEMP_DIR; using /tmp instead
git: error: couldn't create cache file '/tmp/xcrun_db-jjjg4xWr' (errno=Operation not permitted)
2026-05-23 08:44:44.060 xcodebuild[60782:105950635]  DVTFilePathFSEvents: Failed to start fs event stream.
2026-05-23 08:44:44.188 xcodebuild[60782:105950634] [MT] DVTDeveloperPaths: Failed to get length of DARWIN_USER_CACHE_DIR from confstr(3), error = Error Domain=NSPOSIXErrorDomain Code=5 "Input/output error". Using NSCachesDirectory instead.
575885c Fix JsParseIntBase16 JS-parity (whitespace + sign) per Codex r2
 src/Encoding.cs | 83 ++++++++++++++++++++++++++++++++++++++++++++++++++++-----
 1 file changed, 76 insertions(+), 7 deletions(-)

codex
**A. Source Pin**

Upstream package: `@turnkey/encoding`  
Version: `0.6.0`  
Tarball sha256: `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`  
C# file git SHA: `575885ce76757b984b4b060cd4b43d639b983231`

**B. Method Coverage**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---:|---|
| `Uint8ArrayToHexString` (`src/Encoding.cs:90`) | `uint8ArrayToHexString` (`ts-source/hex.ts:7`) | REVIEWED | Lowercase hex and empty-array behavior match. |
| `Uint8ArrayFromHexString` (`src/Encoding.cs:118`) | `uint8ArrayFromHexString` (`ts-source/hex.ts:23`) | REVIEWED | Validation, optional length falsy-`0`, left-padding, overflow errors match. |
| `HexToAscii` (`src/Encoding.cs:164`) | `hexToAscii` (`ts-source/hex.ts:58`) | REVIEWED | Preserves odd trailing nibble and invalid-pair NUL behavior. |
| `JsParseIntBase16` (`src/Encoding.cs:207`) | `parseInt(..., 16)` inside `hexToAscii` (`ts-source/hex.ts:61`) | REVIEWED | JS whitespace, sign, partial-parse, and NaN-to-NUL path reviewed. |
| `IsJsWhitespace` (`src/Encoding.cs:248`) | ECMAScript whitespace used by `parseInt` (`ts-source/hex.ts:61`) | REVIEWED | Helper supports `JsParseIntBase16`; no separate TS function. |
| `NormalizePadding` (`src/Encoding.cs:291`) | `normalizePadding` (`ts-source/hex.ts:73`) | REVIEWED | Add/strip leading zero behavior and error condition match. |
| `StringToBase64UrlString` (`src/Encoding.cs:350`) | `stringToBase64urlString` + `btoa` (`ts-source/base64.ts:13`, `:81`) | REVIEWED | 0-255 code-unit guard plus base64url conversion matches. |
| `HexStringToBase64Url` (`src/Encoding.cs:377`) | `hexStringToBase64url` (`ts-source/base64.ts:28`) | REVIEWED | Odd-length hex normalization, length padding, and base64url output match. |
| `Base64StringToBase64UrlEncodedString` (`src/Encoding.cs:404`) | `base64StringToBase64UrlEncodedString` (`ts-source/base64.ts:45`) | REVIEWED | `+`, `/`, and all `=` replacements match. |
| `Base64UrlToBase64` (`src/Encoding.cs:418`) | `base64UrlToBase64` (`ts-source/base64.ts:58`) | REVIEWED | URL-safe replacement and modulo padding match. |
| `DecodeBase64UrlToString` (`src/Encoding.cs:441`) | `decodeBase64urlToString` + `atob` (`ts-source/base64.ts:74`, `:148`) | REVIEWED | Sanitization, mod-4 error, padding reset, and bit-shift decode match. |
| `PointEncode` (`src/Encoding.cs:516`) | `pointEncode` (`ts-source/encode.ts:8`) | REVIEWED | Prefix and x-coordinate copy match; omitted x/y length check is unreachable after 65-byte guard. |
| `Base58Encode` (`src/Encoding.cs:550`) | `bs58.encode` re-export (`ts-source/bs58.ts:18`, `:36`) | REVIEWED | Bitcoin alphabet, big-endian magnitude, and leading-zero preservation match. |
| `Base58Decode` (`src/Encoding.cs:585`) | `bs58.decode` re-export (`ts-source/bs58.ts:21`, `:36`) | REVIEWED | Alphabet decode and leading `1` to zero-byte restoration match. |
| `Base58CheckEncode` (`src/Encoding.cs:634`) | `bs58check.encode` re-export (`ts-source/bs58check.ts:18`, `:36`) | REVIEWED | Payload + double-SHA256 first-four checksum + base58 match. |
| `Base58CheckDecode` (`src/Encoding.cs:658`) | `bs58check.decode` re-export (`ts-source/bs58check.ts:20`, `:36`) | REVIEWED | Decode, split checksum, double-SHA256 verification, return payload match. |
| `Uint8ArrayToString` (`src/Encoding.cs:702`) | N/A, TS SDK uses `TextDecoder` inline | REVIEWED | Local repo helper; UTF-8 decode equivalent, not an upstream encoding export. |
| `ConcatUint8Arrays` (`src/Encoding.cs:719`) | N/A, TS SDK uses spread literals inline | REVIEWED | Local repo helper; preserves byte order, not an upstream encoding export. |

**C. Intentional Adaptations**

- `Uint8Array` -> `byte[]`: structural type-system mapping; byte values and order unchanged.
- TS `Error` -> .NET exceptions: structural platform mapping; invalid conditions are preserved.
- TS regex literal -> `System.Text.RegularExpressions.Regex`: same hex acceptance set.
- `padStart(Math.ceil(...))` -> integer target length + `PadLeft`: same odd-hex normalization.
- `Uint8Array.set`, `slice`, spread -> `Array.Copy` / new arrays: same byte placement.
- `String.fromCharCode(...)+btoa` over bytes -> `Convert.ToBase64String(byte[])`: same base64 bytes after 0-255 guard.
- Pure JS `atob` -> explicit C# bit-shift decoder: same permissive sanitization and byte emission.
- `bs58` / `bs58check` package calls -> local Bitcoin base58 + double-SHA256 implementation: same wire strings/bytes.
- BouncyCastle `BigInteger` for base58 magnitude math: structural arithmetic primitive; unsigned/big-endian handling preserves leading-zero rules.
- `TextDecoder` inline use -> `UTF8.GetString` helper: same UTF-8 decode role for repo consumers.
- TS spread concatenation -> `ConcatUint8Arrays`: same ordered byte concatenation.

**D. D17 Enforcement Check**

N/A (not Crypto.cs)

**E. Logic Divergence Findings**

Zero entries.

**F. Fixture Comparison Gate**

Fixture reviewed: `tests/Fixtures/encoding/turnkey-encoding-vectors.json`.

The fixture provenance pins `@turnkey/encoding@0.6.0` with the same sha256 above and sources the vectors from `ts-source/__tests__/index-test.ts`. I compared the fixture entries against the pinned upstream test file:

- `stringToBase64UrlString`: fixture lines 9-21 match upstream lines 15-35.
- `base64StringToBase64UrlEncodedString`: fixture lines 23-26 match upstream lines 39-48.
- `uint8ArrayToHexString`: fixture lines 27-31 match upstream lines 52-59.
- `uint8ArrayFromHexString`: fixture lines 33-64 match upstream lines 64-112.
- `hexStringToBase64Url`: fixture lines 66-71 match upstream lines 116-127.

C# tests assert the same upstream bytes/strings inline in `tests/EncodingTests.cs` for those fixture-backed cases. I also spot-checked Node `Buffer(...).toString("base64url")` outputs for the base64url fixture values; they match.

F result: all fixtures match.

**G. Unresolved Assumptions**

I did not run `dotnet test` in this read-only sandbox because test execution/builds write artifacts. Static parity review and Node spot-checks were completed.
tokens used
124,960
**A. Source Pin**

Upstream package: `@turnkey/encoding`  
Version: `0.6.0`  
Tarball sha256: `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`  
C# file git SHA: `575885ce76757b984b4b060cd4b43d639b983231`

**B. Method Coverage**

| C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
|---|---|---:|---|
| `Uint8ArrayToHexString` (`src/Encoding.cs:90`) | `uint8ArrayToHexString` (`ts-source/hex.ts:7`) | REVIEWED | Lowercase hex and empty-array behavior match. |
| `Uint8ArrayFromHexString` (`src/Encoding.cs:118`) | `uint8ArrayFromHexString` (`ts-source/hex.ts:23`) | REVIEWED | Validation, optional length falsy-`0`, left-padding, overflow errors match. |
| `HexToAscii` (`src/Encoding.cs:164`) | `hexToAscii` (`ts-source/hex.ts:58`) | REVIEWED | Preserves odd trailing nibble and invalid-pair NUL behavior. |
| `JsParseIntBase16` (`src/Encoding.cs:207`) | `parseInt(..., 16)` inside `hexToAscii` (`ts-source/hex.ts:61`) | REVIEWED | JS whitespace, sign, partial-parse, and NaN-to-NUL path reviewed. |
| `IsJsWhitespace` (`src/Encoding.cs:248`) | ECMAScript whitespace used by `parseInt` (`ts-source/hex.ts:61`) | REVIEWED | Helper supports `JsParseIntBase16`; no separate TS function. |
| `NormalizePadding` (`src/Encoding.cs:291`) | `normalizePadding` (`ts-source/hex.ts:73`) | REVIEWED | Add/strip leading zero behavior and error condition match. |
| `StringToBase64UrlString` (`src/Encoding.cs:350`) | `stringToBase64urlString` + `btoa` (`ts-source/base64.ts:13`, `:81`) | REVIEWED | 0-255 code-unit guard plus base64url conversion matches. |
| `HexStringToBase64Url` (`src/Encoding.cs:377`) | `hexStringToBase64url` (`ts-source/base64.ts:28`) | REVIEWED | Odd-length hex normalization, length padding, and base64url output match. |
| `Base64StringToBase64UrlEncodedString` (`src/Encoding.cs:404`) | `base64StringToBase64UrlEncodedString` (`ts-source/base64.ts:45`) | REVIEWED | `+`, `/`, and all `=` replacements match. |
| `Base64UrlToBase64` (`src/Encoding.cs:418`) | `base64UrlToBase64` (`ts-source/base64.ts:58`) | REVIEWED | URL-safe replacement and modulo padding match. |
| `DecodeBase64UrlToString` (`src/Encoding.cs:441`) | `decodeBase64urlToString` + `atob` (`ts-source/base64.ts:74`, `:148`) | REVIEWED | Sanitization, mod-4 error, padding reset, and bit-shift decode match. |
| `PointEncode` (`src/Encoding.cs:516`) | `pointEncode` (`ts-source/encode.ts:8`) | REVIEWED | Prefix and x-coordinate copy match; omitted x/y length check is unreachable after 65-byte guard. |
| `Base58Encode` (`src/Encoding.cs:550`) | `bs58.encode` re-export (`ts-source/bs58.ts:18`, `:36`) | REVIEWED | Bitcoin alphabet, big-endian magnitude, and leading-zero preservation match. |
| `Base58Decode` (`src/Encoding.cs:585`) | `bs58.decode` re-export (`ts-source/bs58.ts:21`, `:36`) | REVIEWED | Alphabet decode and leading `1` to zero-byte restoration match. |
| `Base58CheckEncode` (`src/Encoding.cs:634`) | `bs58check.encode` re-export (`ts-source/bs58check.ts:18`, `:36`) | REVIEWED | Payload + double-SHA256 first-four checksum + base58 match. |
| `Base58CheckDecode` (`src/Encoding.cs:658`) | `bs58check.decode` re-export (`ts-source/bs58check.ts:20`, `:36`) | REVIEWED | Decode, split checksum, double-SHA256 verification, return payload match. |
| `Uint8ArrayToString` (`src/Encoding.cs:702`) | N/A, TS SDK uses `TextDecoder` inline | REVIEWED | Local repo helper; UTF-8 decode equivalent, not an upstream encoding export. |
| `ConcatUint8Arrays` (`src/Encoding.cs:719`) | N/A, TS SDK uses spread literals inline | REVIEWED | Local repo helper; preserves byte order, not an upstream encoding export. |

**C. Intentional Adaptations**

- `Uint8Array` -> `byte[]`: structural type-system mapping; byte values and order unchanged.
- TS `Error` -> .NET exceptions: structural platform mapping; invalid conditions are preserved.
- TS regex literal -> `System.Text.RegularExpressions.Regex`: same hex acceptance set.
- `padStart(Math.ceil(...))` -> integer target length + `PadLeft`: same odd-hex normalization.
- `Uint8Array.set`, `slice`, spread -> `Array.Copy` / new arrays: same byte placement.
- `String.fromCharCode(...)+btoa` over bytes -> `Convert.ToBase64String(byte[])`: same base64 bytes after 0-255 guard.
- Pure JS `atob` -> explicit C# bit-shift decoder: same permissive sanitization and byte emission.
- `bs58` / `bs58check` package calls -> local Bitcoin base58 + double-SHA256 implementation: same wire strings/bytes.
- BouncyCastle `BigInteger` for base58 magnitude math: structural arithmetic primitive; unsigned/big-endian handling preserves leading-zero rules.
- `TextDecoder` inline use -> `UTF8.GetString` helper: same UTF-8 decode role for repo consumers.
- TS spread concatenation -> `ConcatUint8Arrays`: same ordered byte concatenation.

**D. D17 Enforcement Check**

N/A (not Crypto.cs)

**E. Logic Divergence Findings**

Zero entries.

**F. Fixture Comparison Gate**

Fixture reviewed: `tests/Fixtures/encoding/turnkey-encoding-vectors.json`.

The fixture provenance pins `@turnkey/encoding@0.6.0` with the same sha256 above and sources the vectors from `ts-source/__tests__/index-test.ts`. I compared the fixture entries against the pinned upstream test file:

- `stringToBase64UrlString`: fixture lines 9-21 match upstream lines 15-35.
- `base64StringToBase64UrlEncodedString`: fixture lines 23-26 match upstream lines 39-48.
- `uint8ArrayToHexString`: fixture lines 27-31 match upstream lines 52-59.
- `uint8ArrayFromHexString`: fixture lines 33-64 match upstream lines 64-112.
- `hexStringToBase64Url`: fixture lines 66-71 match upstream lines 116-127.

C# tests assert the same upstream bytes/strings inline in `tests/EncodingTests.cs` for those fixture-backed cases. I also spot-checked Node `Buffer(...).toString("base64url")` outputs for the base64url fixture values; they match.

F result: all fixtures match.

**G. Unresolved Assumptions**

I did not run `dotnet test` in this read-only sandbox because test execution/builds write artifacts. Static parity review and Node spot-checks were completed.
