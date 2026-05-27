# Tests/Fixtures/Generators — Node-side fixture generators

The scripts in this directory consume the **pinned upstream npm packages**
and emit byte-snapshot JSON files under `tests/Fixtures/{http,api-key-stamper,crypto}/`.
The C# test suite reads those JSON files and asserts byte-equal outputs
for the deterministic paths (stamper PureJS, credential-bundle decrypt,
export-bundle decrypt, signed-request body).

This directory is **excluded from `dotnet build`** via the
`<Content Remove="Fixtures/Generators/**"/>` lines in
`tests/turnkey-sdk-csharp.Tests.csproj`. Node is therefore **not a
prerequisite for `dotnet test`** in normal CI — the committed JSON
fixtures are the source of truth.

## Pinned versions

| Package | Version | Source |
|---|---|---|
| `@turnkey/crypto` | 2.8.8 | matches `codex-crypto-reviews/upstream-snapshots/turnkey-crypto-2.8.8/` |
| `@turnkey/http` | 3.16.0 | matches `codex-crypto-reviews/upstream-snapshots/turnkey-http-3.16.0/` |
| `@turnkey/api-key-stamper` | 0.5.0 | matches `codex-crypto-reviews/upstream-snapshots/turnkey-api-key-stamper-0.5.0/` |
| `@turnkey/encoding` | 0.6.0 | matches `codex-crypto-reviews/upstream-snapshots/turnkey-encoding-0.6.0/` |

The transitive `@noble/curves` resolution in `package-lock.json` decides
whether `signWithApiKey({ runtimeOverride: "purejs" })` produces RFC 6979
deterministic-k low-S signatures. The generator scripts MUST run a
pre-flight assertion against a known hex vector and exit non-zero if the
resolved noble version diverges from RFC 6979.

## Reproduction

Requires Node ≥ 20.10. The plan target is **Node 20.x**, but anything in
the supported range that ships an unchanged `@noble/curves` is
acceptable — the generator's pre-flight pre-flight assertion is the
final gate.

```bash
# From the repo root
cd tests/Fixtures/Generators
npm ci          # restore exact pinned versions from package-lock.json
npm run gen:all # regenerate all JSON fixtures
```

The resulting JSON files include a `_provenance` object recording:

- `level` (always `"node-generated"`)
- `node_version` (`process.version`)
- `npm_lockfile_sha256` (sha256 of this directory's `package-lock.json`)
- `generator_sha256` (sha256 of the `.mjs` script that produced the file)
- `transitive_lock_sha256` (sha256 of `npm ls --json` output, used to
  detect drift in `@noble/curves` resolution)
- `output_sha256` (self-hash of the data section, excluding the
  provenance object itself)

## CI integration

The committed JSON fixtures are byte-compared in `dotnet test`. Node is
**never invoked** by the main CI workflow (`.github/workflows/ci.yml`)
nor the release workflow. A separate opt-in workflow
`.github/workflows/fixture-regen.yml` (added in PR-13) re-runs these
generators on demand and opens a PR if the output diverges.

## Why pure shell + Node, no TypeScript

These scripts are intentionally `.mjs` (native ESM) with **no
TypeScript build step**. The fewer build tools sit between this directory
and the pinned upstream packages, the fewer chances for transitive
resolution drift. Each script is a single file ≤ ~150 lines.

## Out of scope

- This directory does NOT contain `proof.ts` golden fixtures —
  those live under `tests/Fixtures/proofs/` and were copied directly
  from upstream `__tests__/proof-tests.ts` (no execution path needed,
  the proof verifier itself is unported per plan Section 12).
- This directory does NOT generate QOS encryption vectors —
  `quorumKeyEncrypt` is out of plan scope (see Section 2.1).
