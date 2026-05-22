# Peak monorepo lockfile evidence for the Turnkey version pins

The pinned `@turnkey/*` npm versions in `turnkey-source-pins.md` come from
peak's actual `pnpm-lock.yaml` and `package.json` files. This file documents
the exact source.

## package.json declarations in peak

| File in peak                                      | Line / declaration |
|---|---|
| `packages/peak-sdk-core/package.json`             | `"@turnkey/core": "1.10.0"`, `"@turnkey/crypto": "2.8.8"`, `"@turnkey/encoding": "0.6.0"`, `"@turnkey/http": "3.16.0"` |
| `packages/peak-sdk-browser/package.json`          | `"@turnkey/core": "1.10.0"`, `"@turnkey/crypto": "2.8.8"`, `"@turnkey/encoding": "0.6.0"`, `"@turnkey/iframe-stamper": "2.10.0"` |
| `packages/peak-sdk-node/package.json`             | `"@turnkey/crypto": "2.8.8"`, `"@turnkey/encoding": "0.6.0"`, `"@turnkey/sdk-server": "5.0.0"` |

## pnpm-lock.yaml resolutions

```
'@turnkey/crypto':
  specifier: 2.8.8       # peak-sdk-core, peak-sdk-browser, peak-sdk-node
'@turnkey/http':
  specifier: 3.16.0      # peak-sdk-core
'@turnkey/encoding':
  specifier: 0.6.0       # peak-sdk-core, peak-sdk-browser, peak-sdk-node
'@turnkey/api-key-stamper': 0.5.0        # transitive: resolved by @turnkey/sdk-server@5.0.0
'@turnkey/encoding': 0.6.0               # transitive: resolved by @turnkey/sdk-server@5.0.0
'@turnkey/crypto': 2.8.8                 # transitive: resolved by @turnkey/sdk-server@5.0.0
```

## Why @turnkey/api-key-stamper is at 0.5.0

`@turnkey/api-key-stamper` is not declared as a direct dependency of any peak
package. It is pulled transitively by `@turnkey/sdk-server@5.0.0`, which
pins it at `0.5.0` in its own dependency tree. That is what actually runs in
peak's Node deployment, so the C# port must match `0.5.0`.

## Why the Unity port version is NOT the source of truth

The Unity submodule (`packages/turnkey-sdk-unity`, pinned at
`039d8e4...`) was originally ported from slightly newer npm versions:
crypto 2.8.9, http 3.16.1, api-key-stamper 0.6.0. Those versions may carry
small wire-protocol-relevant changes (constant tweaks, additional fields,
renamed bundle envelope keys) compared to what peak actually ships.

To guarantee wire compatibility with peak's TypeScript code paths, this C#
port targets **peak's** versions and uses the Unity port only as a C#
adaptation reference (BouncyCastle wiring, exception types, byte[] vs
Uint8Array adaptations).
