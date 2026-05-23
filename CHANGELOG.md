# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0-alpha.0] — 2026-05-23

### Added

- `Turnkey.Encoding` — 1:1 logical port of `@turnkey/encoding@0.6.0`
  (hex / base58 / base58check / base64url / point compression).
- `Turnkey.Crypto` — 1:1 logical port of the subset of
  `@turnkey/crypto@2.8.8` consumed by the peak Unity SDK:
  - P-256 key pair generation (`GenerateP256KeyPair`, `GetPublicKey`).
  - HPKE-Base mode encrypt / decrypt (`HpkeEncrypt`, `HpkeDecrypt`)
    using P-256 / HKDF-SHA256 / AES-128-GCM, with the labelled
    information construction copied byte-for-byte from upstream
    `crypto.ts`.
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
- `Turnkey.ApiKeyStamper` — 1:1 logical port of
  `@turnkey/api-key-stamper@0.5.0` mirroring the upstream "purejs"
  runtime (RFC 6979 deterministic ECDSA + low-S, DER hex output).
- `Turnkey.Http` — 1:1 logical port of the request-signing subset of
  `@turnkey/http@3.16.0` consumed by the peak Unity SDK:
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

### Reviewed by

- Per-file Codex multi-round review (≥3 rounds each, evidence in
  [`codex-crypto-reviews/`](./codex-crypto-reviews/)).
- Integrated cross-file review with **GO** verdict
  (`codex-crypto-reviews/FINAL-INTEGRATED-REVIEW-20260523.md`).
- 113 xunit tests covering hex / base58 / base64url / HKDF RFC 5869
  / NIST P-256 / Curve.Secp256k1 / HPKE round-trip / API key signing
  / DER signature verification / public API surface snapshot.
- Real upstream-signed Turnkey credential bundle test vector
  (`CryptoTests.DecryptCredentialBundle_UpstreamVector`) verifies the
  full HPKE pipeline against an upstream Jest test fixture.

### Notes

- Pre-release, internal-only. Not published to nuget.org.
- Default base URL is `https://api.turnkey.com`; each
  `Http.GetHttpClient` / `Http.FromTargetPrivateKey` factory accepts an
  optional `baseUrl` argument.
- BouncyCastle 2.5.0 is used only for primitives (ECDSA / ECDH /
  AES-GCM / SHA-256 / HMAC / Ed25519 public-key derivation /
  BigInteger / EC point ops). HPKE / HKDF / Tonelli-Shanks / bundle
  parse are direct line-by-line ports of the upstream TypeScript.
