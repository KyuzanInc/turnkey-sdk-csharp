# Peak monorepo usage grep evidence

This document is the deliverable of **PR-1b** in
[`plans/PLAN-EQUIVALENCE-VERIFICATION.md`](../plans/PLAN-EQUIVALENCE-VERIFICATION.md).

It records `grep` results against the [`peak`](https://github.com/Kyuzan-inc/peak)
monorepo to establish whether the upstream Turnkey TypeScript API surface that the
C# SDK has **deliberately not ported** is used anywhere in Peak's production code.

The grep result confirms the **N/A justifications** that PR-3 will write into
`codex-crypto-reviews/coverage-map.tsv` for those API names. If Peak called any of
these symbols, the corresponding `coverage-map.tsv` row would have to be `MISSING`
(must port to C#) instead of `N/A` (out of scope by usage).

## Provenance

| Field | Value |
|---|---|
| Peak monorepo root | `/Users/takeshi/Kyuzan/src/peak` |
| Peak HEAD SHA at evidence time | `d08872d14faf13a399bbacbaf927b735de5a189b` |
| Peak HEAD subject | `fix(peak-web-wallet): smooth Google login redirect-mode UX (#324)` |
| C# SDK worktree HEAD | `a0fd24d1ba166652abfa7c4eedf6bd2a3368ba32` |
| Evidence date | 2026-05-27 |
| Grep scope | `$PEAK/packages` + `$PEAK/apps`, excluding `**/dist/**` and `**/node_modules/**` |
| Grep extensions | `*.ts`, `*.tsx`, `*.cs` |

Peak pins `@turnkey/crypto@2.8.8` across 4 workspace packages — matches the C#
SDK's pinned upstream version exactly:

```
packages/peak-sdk-core/package.json:    "@turnkey/crypto": "2.8.8",
packages/peak-sdk-node/package.json:    "@turnkey/crypto": "2.8.8",
packages/peak-sdk-browser/package.json: "@turnkey/crypto": "2.8.8",
apps/peak-server/package.json:          "@turnkey/crypto": "2.8.8",
```

## Symbols probed (= out-of-scope candidates per `src/Crypto.cs:22-29`)

For each symbol, the table shows the number of files in the main Peak tree that
reference it.

| Symbol | Files in Peak main tree | Conclusion |
|---|---|---|
| `verifyAppProof` | 0 | **Not used by Peak.** Justifies `N/A: not-ported: proof verifier` in coverage-map. |
| `AppProof` | 0 | **Not used by Peak.** Confirms proof verifier scope-out for `proof-tests.ts`. |
| `BootProof` | 0 | **Not used by Peak.** Confirms proof verifier scope-out. |
| `withAsyncPolling` | 0 | **Not used by Peak.** Justifies `N/A: not-ported: withAsyncPolling` for all 6 `__tests__/async-test.ts` cases. |
| `quorumKeyEncrypt` | 1 | Only inside `packages/turnkey-sdk-unity/Runtime/Crypto.cs:22` — a **doc comment listing API NOT implemented in Unity port**. Zero call sites. Justifies QOS scope-out. |
| `hpkeAuthEncrypt` | 1 | Same single file, same doc comment, zero call sites. Justifies `N/A: not-ported: hpkeAuthEncrypt` for the upstream `hpkeAuthEncrypt and hpkeDecrypt - end-to-end` test. |
| `extractPrivateKeyFromPKCS8Bytes` | 1 | Same single file, same doc comment, zero call sites. Justifies `N/A: not-ported: extractPrivateKeyFromPKCS8Bytes`. |
| `fromDerSignature` | 1 | Same single file, same doc comment, zero call sites. Justifies `N/A: not-ported: fromDerSignature` for the 14 DER parser cases at `crypto-test.ts:263-454`. |
| `toDerSignature` | 1 | Same single file, same doc comment, zero call sites. |
| `verifyStampSignature` | 0 | Not referenced anywhere in Peak. Justifies `N/A: not-ported: verifyStampSignature`. |
| `verifyRequestStamp` | 0 | Not referenced anywhere in Peak. Justifies `N/A: not-ported: verifyRequestStamp`. |

**Net result**: the C# SDK's out-of-scope policy at
[`src/Crypto.cs:22-29`](../src/Crypto.cs) is consistent with Peak's actual usage.
No Peak production call site exists that would require porting any of these
symbols to C#.

## Symbols Peak **does** use from `@turnkey/crypto`

For completeness, the inverse list — what Peak imports — is recorded below so
PR-3 can confirm that the C# SDK already covers them:

| Peak source path:line | Imported symbol | C# port status |
|---|---|---|
| `packages/peak-sdk-core/src/utils/session-jwt.ts:1` | `verifySessionJwtSignature` | **Ported** as `Turnkey.Crypto.VerifySessionJwtSignature` (see `src/Crypto.cs`). |
| `packages/peak-sdk-node/src/utils/turnkey.ts:1` | `getPublicKey` | **Ported** as `Turnkey.Crypto.GetPublicKey`. |
| `packages/peak-sdk-node/src/utils/turnkey.ts:34-39` | `decryptExportBundle`, `encryptPrivateKeyToBundle`, `encryptWalletToBundle`, `generateP256KeyPair` | **All ported**: `Turnkey.Crypto.{DecryptExportBundle, EncryptPrivateKeyToBundle, EncryptWalletToBundle, GenerateP256KeyPair}`. |
| `packages/peak-sdk-browser/src/utils/turnkey.ts:104-112` | `decryptExportBundle`, `encryptPrivateKeyToBundle`, `encryptWalletToBundle`, `generateP256KeyPair` | Same set as Node side — already ported. |
| `apps/peak-server/src/routes/v1/public-api/strategies/turnkey-session-jwt.strategy.ts:3` | `verifySessionJwtSignature` | Already ported. |

## Symbols Peak uses from `@turnkey/encoding`

| Peak source path:line | Imported symbol | C# port status |
|---|---|---|
| `packages/peak-sdk-core/src/utils/session-jwt.ts:2` | `decodeBase64urlToString` | **Ported** as `Turnkey.Encoding.DecodeBase64UrlToString`. |
| `packages/peak-sdk-node/src/utils/turnkey.ts:3-5` | `uint8ArrayFromHexString`, `uint8ArrayToHexString` | **Ported** as `Turnkey.Encoding.HexToBytes` / `Turnkey.Encoding.BytesToHex`. |

## Other Peak Turnkey ecosystem imports (informational only)

These imports are **non-crypto** packages and are out of scope of this verification
plan (they are HTTP / SDK / iframe-stamper layers, not the crypto/encoding/stamper
that this C# SDK is verifying equivalence for):

- `@turnkey/core` — `TurnkeyClient`, `TurnkeySDKClientBase` (peak-sdk-browser).
- `@turnkey/sdk-server` — `Turnkey`, `TurnkeyRequestError` (peak-sdk-node, peak-server).
- `@turnkey/iframe-stamper` — `IframeStamper`, `KeyFormat`, `TIframeStyles` (peak-sdk-browser only).
- `@turnkey/http` — only `TSignedRequest` type re-export from peak-sdk-core/index.

## Reproduction

To re-run this evidence (e.g., after Peak HEAD advances):

```bash
PEAK=/Users/takeshi/Kyuzan/src/peak
SYMS="verifyAppProof AppProof BootProof withAsyncPolling quorumKeyEncrypt \
      hpkeAuthEncrypt extractPrivateKeyFromPKCS8Bytes fromDerSignature \
      toDerSignature verifyStampSignature verifyRequestStamp"

for sym in $SYMS; do
  count=$(grep -rln --include="*.ts" --include="*.tsx" --include="*.cs" "$sym" \
            "$PEAK/packages" "$PEAK/apps" 2>/dev/null \
            | grep -v "/dist/" | grep -v "node_modules" | wc -l | tr -d ' ')
  echo "$sym: $count file(s)"
done
```

A symbol whose count rises above the value recorded in this document means Peak
has started using a previously out-of-scope API, and the corresponding
`coverage-map.tsv` row must be re-evaluated.
