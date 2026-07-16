# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Replaced internal AI-review artifacts with public compatibility
  documentation, ADRs, reproducible source-checksum verification, and a
  machine-enforced upstream-test coverage map.
- Relocated the minimal pinned TypeScript source and test inputs to
  `tests/UpstreamSources/`; removed compiled npm distributions and raw review
  transcripts.
- Documented the supported API boundary and current verification gaps without
  relying on private downstream repositories or unpublished review logs.
- Added Apache-2.0 attribution and license material for retained and adapted
  Turnkey source and fixtures.
- Rewrote the README and contribution guidance for OSS consumers while keeping
  GitHub Packages as the only package registry.
- Hardened the compatibility coverage gate against non-test/skip false
  positives and stale generated evidence, made source-package and drift issue
  handling fail closed, and aligned CI package inspection with the strict
  release checks.

## [0.1.0-alpha.0] — 2026-05-23

### Added

- `Turnkey.Encoding` — logical compatibility port of `@turnkey/encoding@0.6.0`
  (hex / base58 / base58check / base64url / point compression).
- `Turnkey.Crypto` — logical compatibility port of the supported subset of
  `@turnkey/crypto@2.8.8`:
  - P-256 key pair generation (`GenerateP256KeyPair`, `GetPublicKey`).
  - HPKE-Base mode encrypt / decrypt (`HpkeEncrypt`, `HpkeDecrypt`)
    using P-256 / HKDF-SHA256 / AES-128-GCM and the pinned upstream
    `crypto.ts` labelled information construction.
  - Tonelli-Shanks modular square root (`Math.ModSqrt`) for point
    decompression.
  - RFC 5869 HKDF-HMAC-SHA256 (`Hkdf.Extract`, `Hkdf.Expand`) ported
    from `@noble/hashes/hkdf`.
  - SEC1 point compression / decompression
    (`CompressRawPublicKey`, `UncompressRawPublicKey`) supporting
    P-256 and secp256k1.
  - Turnkey credential-bundle decrypt
    (`DecryptCredentialBundle`).
  - Turnkey import-bundle encrypt (`EncryptPrivateKeyToBundle`,
    with `dangerouslyOverrideSignerPublicKey`).
  - Turnkey export-bundle decrypt (`DecryptExportBundle`,
    including Ed25519 derivation for SOLANA-formatted keys).
  - Session JWT signature verification
    (`VerifySessionJwtSignature`, with
    `dangerouslyOverrideNotarizerPublicKey`).
- `Turnkey.ApiKeyStamper` — logical port of
  `@turnkey/api-key-stamper@0.5.0` using deterministic RFC 6979 ECDSA,
  explicit low-S normalization, and DER hex output.
- `Turnkey.Http` — logical compatibility port of the request-signing subset of
  `@turnkey/http@3.16.0`:
  - `query/whoami`
  - `submit/init_import_private_key`
  - `submit/import_private_key`
  - `submit/export_private_key`
  - `submit/export_wallet_account`
- `Turnkey.TurnkeyJsonContext` — System.Text.Json source-generated
  context for IL2CPP-safe AOT serialization, with
  `JsCompatibleOptions` that applies `UnsafeRelaxedJsonEscaping` for
  JS-`JSON.stringify` parity.

### Source pins (Turnkey npm packages)

| Package                       | Version | Tarball sha256                                                       |
|---|---|---|
| `@turnkey/crypto`             | 2.8.8   | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3`   |
| `@turnkey/http`               | 3.16.0  | `d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf`   |
| `@turnkey/api-key-stamper`    | 0.5.0   | `962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd`   |
| `@turnkey/encoding`           | 0.6.0   | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc`   |

### Verification at the initial alpha release

- 113 xunit tests covering hex / base58 / base64url / HKDF RFC 5869
  / NIST P-256 / Curve.Secp256k1 / HPKE round-trip / API key signing
  / DER signature verification / public API surface snapshot.
- Real upstream-signed Turnkey credential bundle test vector
  (`CryptoTests.DecryptCredentialBundle_UpstreamVector`) verifies the
  full HPKE pipeline against an upstream Jest test fixture.

### Notes

- Pre-release. Published to GitHub Packages only, not nuget.org.
- Default base URL is `https://api.turnkey.com`; each
  `Http.GetHttpClient` / `Http.FromTargetPrivateKey` factory accepts an
  optional `baseUrl` argument.
- BouncyCastle 2.5.0 is used only for primitives (ECDSA / ECDH /
  AES-GCM / SHA-256 / HMAC / Ed25519 public-key derivation /
  BigInteger / EC point ops). HPKE / HKDF / Tonelli-Shanks / bundle
  parse are direct line-by-line ports of the upstream TypeScript.
