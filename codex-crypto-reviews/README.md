# Codex multi-round crypto review

This directory holds the evidence that proves the C# implementation is a 1:1
logical port of the Turnkey TypeScript SDK at the **peak-pinned versions**.

## SOP

For each of the 6 production files under `src/`:

1. `Encoding.cs`
2. `CryptoConstants.cs`
3. `Crypto.cs`
4. `ApiKeyStamper.cs`
5. `Http.cs`
6. `TurnkeyJsonContext.cs`

run **three independent rounds** of Codex review using
[`codex-crypto-review.sh`](./codex-crypto-review.sh). Each round writes a
separate evidence file named `{file}.cs-r{N}-{YYYYMMDD}.md`.

A file is considered "review-clean" only when round 3 reports:

- section B has zero `NOT-REVIEWED` rows (or each NOT-REVIEWED row has a
  documented justification),
- section D states "no banned API present" (Crypto.cs only),
- section E has zero entries,
- section F states "all fixtures match".

## Pinned upstream sources

The authoritative upstream sources are the npm tarballs extracted under
[`upstream-snapshots/`](./upstream-snapshots/). The exact tarball sha256 is in
[`upstream-snapshots/tarball-checksums.txt`](./upstream-snapshots/tarball-checksums.txt).

GitHub commit SHAs are recorded in [`turnkey-source-pins.md`](./turnkey-source-pins.md)
as **secondary** metadata only. The npm tarball is the source of truth because
npm publish contents may differ from the git tree (excluded test files,
included build output, etc.).

## Unity port reference

The pre-existing C# Unity port at
[`packages/turnkey-sdk-unity/Runtime/`](../packages/turnkey-sdk-unity/Runtime/)
(peak submodule SHA in [`unity-source-pins.md`](./unity-source-pins.md)) is used
**only as a C# adaptation reference**. Logic divergence from the peak-pinned
TS source must be resolved by re-porting from the TS source, not by trusting
the Unity port.

## Codex prompt template

Codex is invoked with the following prompt (sections A-G required in every
review round; pass criterion in `codex-crypto-review.sh`):

```
SYSTEM:
You are reviewing a C# port of the Turnkey TypeScript SDK file: {cs-file}
The pinned upstream npm tarball is at:
  codex-crypto-reviews/upstream-snapshots/turnkey-{pkg}-{version}/
  (sha256 in tarball-checksums.txt — record the matching hash in your output)
The C# file is at:
  src/{cs-file}

REQUIRED OUTPUTS for this review round (all must appear in your output):

A. Source pin acknowledgement: upstream package name, version, tarball sha256
   (from tarball-checksums.txt), C# file's `git rev-parse HEAD` SHA.

B. Method coverage table: every public + internal helper method in {cs-file}
   listed in a markdown table:
     - C# method (file:line)
     - Upstream TS function (path:line within upstream-snapshots/...)
     - Status: REVIEWED / NOT-REVIEWED
     - Notes (one line)
   Do NOT skip a row. If upstream counterpart is missing, set NOT-REVIEWED.

C. Intentional adaptations: every C#/TS pattern adaptation explicitly listed
   with the reason why it is structural (does not change wire bytes or
   observable behavior). Examples: Task<T>↔Promise<T>, byte[]↔Uint8Array,
   BouncyCastle X↔noble Y, System.Text.Json↔JSON.stringify, BigInteger↔BigInt,
   exceptions↔Error/throw.

D. (Crypto.cs only) D17 enforcement check: confirm BouncyCastle is used only
   for ECDSA / ECDH / AES-GCM / SHA-256 / HMAC / BigInteger / EC point ops.
   The following MUST NOT appear in Crypto.cs:
     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
     - Org.BouncyCastle.Crypto.Hpke.*
     - Any "high-level" KDF or HPKE wrapper
   If any banned API is present, flag it as a divergence.

E. Logic divergence findings: every place where C# behavior differs from
   upstream TS in any of:
     - algorithm step order
     - constant values
     - error handling (which condition throws what)
     - byte ordering
     - leading-zero handling
     - padding
     - rounding / normalization
     - signature format (DER vs raw r||s, low-S)
     - DTO shape (field names, order, presence, optionality)
     - JSON serialization (property order, casing, null handling, escaping)
   For each: C# file:line, TS upstream-snapshots/...:line, semantic difference,
   suggested fix.

F. Fixture comparison gate: for every test fixture that touches this file
   (in tests/Fixtures/), confirm it was generated from the pinned upstream
   package and the C# test asserts the same bytes Node produces.

G. Unresolved assumptions you could not verify.

PASS criterion: B has zero NOT-REVIEWED rows (or documented justifications)
AND E has zero entries AND D is "no banned API present" AND F is
"all fixtures match".

DO NOT use phrases like "looks good" or "no divergence found" without
producing sections A–F.
```

## Review run

```bash
# Round N for file Crypto.cs (replace as needed)
./codex-crypto-review.sh Crypto.cs 1
./codex-crypto-review.sh Crypto.cs 2
./codex-crypto-review.sh Crypto.cs 3
```

Each invocation appends a `.md` file in this directory with the codex output
unchanged. Do not edit the output after the run — it is the evidence.

## When to re-run

- Bump any pinned `@turnkey/*` version → re-run 3 rounds per file.
- Substantive change to a `src/*.cs` file → re-run 3 rounds for that file.
- Bump BouncyCastle major → re-run Crypto.cs + ApiKeyStamper.cs + Http.cs.
