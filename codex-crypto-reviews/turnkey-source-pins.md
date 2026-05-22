# Turnkey upstream source pins

The C# port targets the **peak-pinned** Turnkey npm versions, NOT the Unity
port's newer versions. The npm tarball contents (extracted under
[`upstream-snapshots/`](./upstream-snapshots/)) are the authoritative source.
GitHub SHAs are secondary metadata.

## Pin table

| src C# file                | Upstream snapshot path                  | npm package                | Version | Tarball sha256 |
|---|---|---|---|---|
| src/Encoding.cs            | turnkey-encoding-0.6.0                  | @turnkey/encoding          | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` |
| src/CryptoConstants.cs     | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
| src/Crypto.cs              | turnkey-crypto-2.8.8                    | @turnkey/crypto            | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` |
| src/ApiKeyStamper.cs       | turnkey-api-key-stamper-0.5.0           | @turnkey/api-key-stamper   | 0.5.0   | `962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd` |
| src/Http.cs                | turnkey-http-3.16.0                     | @turnkey/http              | 3.16.0  | `d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf` |
| src/TurnkeyJsonContext.cs  | (no single upstream; covers DTOs from api-key-stamper + http) | n/a | n/a | n/a |

Each upstream snapshot directory contains:

- `package/` — verbatim npm tarball extract (the authoritative wire-format
  source: `dist/*.js` + `dist/*.d.ts` + `package.json` + LICENSE / README).
- `ts-source/` — the original TypeScript source from `github.com/tkhq/sdk` at
  the matching git tag SHA (committed for human readability; the npm tarball
  remains the wire-format source of truth if the two ever disagree).

## Why these versions

These are exactly what the peak monorepo pulls. See
[`peak-lockfile-evidence.md`](./peak-lockfile-evidence.md) for the
`pnpm-lock.yaml` lines.

## GitHub commit SHAs (secondary)

`gitHead` is **NOT present** in the published tarballs for these versions
(`grep gitHead package/package.json` returns nothing for any of the four).
The GitHub commit SHAs below are resolved from the git tag of the form
`@turnkey/<pkg>@<version>` via `gh api /repos/tkhq/sdk/git/ref/tags/...`.
These are recorded for cross-reference only. The npm tarball sha256 above
is the controlling wire-format source.

| npm package                | Version | Git tag SHA                                  | Reference URL |
|---|---|---|---|
| @turnkey/encoding          | 0.6.0   | `60a997f4c52ac5f98bdd285af934f02699b88bff`   | https://github.com/tkhq/sdk/tree/60a997f4c52ac5f98bdd285af934f02699b88bff/packages/encoding |
| @turnkey/crypto            | 2.8.8   | `b35dc642bd7c1728f97acd43d4cba66976b65084`   | https://github.com/tkhq/sdk/tree/b35dc642bd7c1728f97acd43d4cba66976b65084/packages/crypto |
| @turnkey/api-key-stamper   | 0.5.0   | `b711befbb88ec522452dbdac68f0e98762be10dd`   | https://github.com/tkhq/sdk/tree/b711befbb88ec522452dbdac68f0e98762be10dd/packages/api-key-stamper |
| @turnkey/http              | 3.16.0  | `8def9ba521233137437ac7294693a4ae0a0d14da`   | https://github.com/tkhq/sdk/tree/8def9ba521233137437ac7294693a4ae0a0d14da/packages/http |

## Re-pinning procedure

When bumping any pin:

1. Update the table above (npm version, refresh tarball sha256, gitHead).
2. Update `Directory.Packages.props` or any version comment.
3. Re-run `npm pack` and refresh the extracted directories under
   `upstream-snapshots/`.
4. Re-run **3 Codex review rounds per affected file** via
   [`codex-crypto-review.sh`](./codex-crypto-review.sh).
5. Update `CHANGELOG.md`.
