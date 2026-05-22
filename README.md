# KyuzanInc.Turnkey.Sdk

> **Unofficial / community-maintained Turnkey SDK for .NET by Kyuzan Inc.
> Not affiliated with Turnkey, Inc.**

A NuGet package that lets .NET applications produce signed Turnkey API
requests and decrypt Turnkey credential / export bundles.

The crypto logic is a 1:1 logical port of the Turnkey TypeScript SDK packages
at the exact versions consumed by the Kyuzan **peak** monorepo:

| Source npm package         | Version |
|----------------------------|---------|
| `@turnkey/crypto`          | 2.8.8   |
| `@turnkey/http`            | 3.16.0  |
| `@turnkey/api-key-stamper` | 0.5.0   |
| `@turnkey/encoding`        | 0.6.0   |

The 1:1 logical equivalence is confirmed by multi-round Codex review.
Evidence is committed to [`codex-crypto-reviews/`](./codex-crypto-reviews/).

For the authoritative Turnkey SDK in TypeScript, see
[`github.com/tkhq/sdk`](https://github.com/tkhq/sdk).

## Status

Pre-release, internal-only. Not published to nuget.org. Not for public
production use. Trademark and distribution review pending.

## Features (planned for v0.1.0)

- `Turnkey.Encoding` — hex / base58 / base58check / UTF-8 utilities.
- `Turnkey.Crypto` — P-256 key pair generation, HPKE encrypt / decrypt,
  HKDF, Tonelli-Shanks modular square root, Turnkey credential-bundle
  decrypt, Turnkey export-bundle decrypt, Turnkey session JWT signature
  verification.
- `Turnkey.ApiKeyStamper` — ECDSA P-256 stamp for the Turnkey API
  `X-Stamp` header.
- `Turnkey.Http` — typed signed-request builders for the Turnkey API
  activity endpoints used by the peak wallet flow.

## Target frameworks

`netstandard2.1` and `net8.0`. Tested on .NET 8.

## Dependencies

- `BouncyCastle.Cryptography 2.5.0` — for ECDSA / ECDH / AES-GCM /
  SHA-256 / HMAC / BigInteger / EC point primitives only. **No
  BouncyCastle HPKE / HKDF / KDF wrappers are used.** HPKE, HKDF,
  Tonelli-Shanks, and bundle parsing are direct ports of the upstream
  Turnkey TypeScript logic.
- `System.Text.Json 8.0.5` — source-generated context only
  (`TurnkeyJsonContext`). No reflection-based serialization,
  no `System.Reflection.Emit`. Compatible with IL2CPP AOT trim.

## Project structure

```
src/        — production code
tests/      — xunit tests + golden fixtures from pinned Node packages
docs/security/threat-model.md — scope: key handling, signing, crypto
                                primitives, serialization, test-secret handling
codex-crypto-reviews/          — pinned upstream snapshots, source-pin docs,
                                 multi-round Codex review evidence
```

## Build and test

```bash
dotnet restore --use-lock-file
dotnet build turnkey-sdk-csharp.sln -c Release
dotnet test  turnkey-sdk-csharp.sln -c Release \
  --collect:"XPlat Code Coverage"
```

## Contributing

This repository is private. Contribution policy will be set after v0.1.0.

## License

MIT — see [LICENSE](./LICENSE). Upstream Turnkey TypeScript attribution
is recorded in [NOTICE](./NOTICE).
