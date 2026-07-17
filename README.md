# KyuzanInc.Turnkey.Sdk

> Unofficial, community-maintained Turnkey SDK for .NET by Kyuzan Inc.
> Not affiliated with, endorsed by, or maintained by Turnkey, Inc.

`KyuzanInc.Turnkey.Sdk` lets .NET applications construct signed Turnkey API
requests and handle Turnkey credential, import, and export bundles. It targets
`netstandard2.1` and `net8.0`.

The implementation is a logical compatibility port of the documented,
supported subset of explicitly pinned Turnkey TypeScript packages:

| Upstream package | Version |
|---|---:|
| `@turnkey/crypto` | 2.8.8 |
| `@turnkey/http` | 3.16.0 |
| `@turnkey/api-key-stamper` | 0.5.0 |
| `@turnkey/encoding` | 0.6.0 |

Exact npm tarball checksums and Git commits are documented in
[`docs/compatibility/upstream-pins.md`](./docs/compatibility/upstream-pins.md).
The authoritative Turnkey TypeScript SDK is
[`tkhq/sdk`](https://github.com/tkhq/sdk).

## Status and distribution

Version `1.0.0` is the first stable release of the documented supported API
surface. Packages are published to **GitHub Packages only**; this repository
does not publish to nuget.org. The GitHub Packages entry is intentionally
private, so consumers need both package access and authentication with
`read:packages`. GitHub Releases and CI runs do not attach downloadable
`.nupkg` or `.snupkg` binaries.

The supported API boundary and known verification gaps are explicit in:

- [`docs/compatibility/supported-surface.md`](./docs/compatibility/supported-surface.md)
- [`docs/compatibility/verification.md`](./docs/compatibility/verification.md)
- [`docs/security/threat-model.md`](./docs/security/threat-model.md)

## Features

- `Turnkey.Encoding`: hexadecimal, base64url, base58, base58check, UTF-8, and
  point-encoding helpers.
- `Turnkey.Crypto`: P-256 key generation, HPKE, HKDF, credential-bundle
  decrypt, import-bundle encrypt, export-bundle decrypt, and session-JWT
  signature verification.
- `Turnkey.ApiKeyStamper`: deterministic P-256 API-key stamps for the
  `X-Stamp` header.
- `Turnkey.Http`: typed signed-request builders for the supported Turnkey
  activity endpoints.

## Installation

Create a project-local `nuget.config` next to the solution. Keep credentials in
environment variables and do not commit them.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="kyuzan-github" value="https://nuget.pkg.github.com/KyuzanInc/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <kyuzan-github>
      <add key="Username" value="%KYUZAN_GH_USER%" />
      <add key="ClearTextPassword" value="%KYUZAN_GH_TOKEN%" />
    </kyuzan-github>
  </packageSourceCredentials>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="kyuzan-github">
      <package pattern="KyuzanInc.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

Set `KYUZAN_GH_USER` and a classic GitHub PAT with `read:packages`, then add
the desired published version:

```bash
dotnet add package KyuzanInc.Turnkey.Sdk --version 1.0.0
```

See [`docs/release-process.md`](./docs/release-process.md) for consumer access
and release details.

## Quick start

```csharp
using Turnkey;

// Generate an ephemeral P-256 recipient key pair.
var keyPair = Crypto.GenerateP256KeyPair();

// Construct a signed Turnkey request. The caller owns the HTTP transport.
var http = Http.FromTargetPrivateKey("<api-private-key-hex>");
SignedRequest signed = http.StampGetWhoami("<organization-id>");

// Stamp arbitrary JSON directly.
var stamper = new ApiKeyStamper(
    apiPublicKey: "<api-public-key-hex>",
    apiPrivateKey: "<api-private-key-hex>");
ApiKeyStamper.StampResult stamp = stamper.Stamp("{\"foo\":\"bar\"}");

// Decrypt a credential bundle using the ephemeral recipient key.
string apiPrivateKey = Crypto.DecryptCredentialBundle(
    encryptedCredentialBundle: "<bundle-from-turnkey>",
    embeddedKey: keyPair.PrivateKey);

// Decrypt an export bundle as hexadecimal key material.
string exported = Crypto.DecryptExportBundle(new Crypto.DecryptExportBundleParams
{
    ExportBundle = "<export-bundle-json>",
    EmbeddedKey = keyPair.PrivateKey,
    OrganizationId = "<organization-id>",
    ReturnMnemonic = false,
    KeyFormat = "HEXADECIMAL",
});
```

The SDK does not persist secrets or own network transport, retries, secure
storage, OAuth, OTP, or WebAuthn flows. Callers must send requests over HTTPS
and protect all private-key material.

## Build and verification

```bash
dotnet restore --locked-mode
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
./tools/compatibility/verify-source-checksums.sh
./tools/compatibility/coverage-map.sh --check
dotnet pack src/turnkey-sdk-csharp.csproj -c Release --no-build --output artifacts
```

Compatibility evidence is organized by owner:

```text
docs/adr/                    architectural decisions
docs/compatibility/          upstream pins, supported scope, and verification
tools/compatibility/         executable compatibility gates
tests/UpstreamSources/       checksum-pinned upstream source and test inputs
tests/Fixtures/              committed test fixtures and generators
```

## Design notes

- BouncyCastle supplies cryptographic primitives; Turnkey-specific HPKE,
  bundle parsing, labeling, and wire behavior remain explicit in this library.
- JSON used on the signed wire path is serialized through the source-generated
  `TurnkeyJsonContext` with deterministic property order.
- C# signatures are normalized to low-S. The pinned upstream PureJS fixture can
  produce the mathematically equivalent high-S form; tests compare the
  normalized relationship where byte identity is not the contract.

See the [`ADR index`](./docs/adr/README.md) for rationale and alternatives.

## Contributing

See [`CONTRIBUTING.md`](./CONTRIBUTING.md). Changes to upstream pins,
cryptography, or signed wire formats must update the corresponding fixtures,
coverage map, compatibility documentation, and ADRs when the decision changes.

Security vulnerabilities must be reported privately as described in
[`SECURITY.md`](./SECURITY.md), not through a public issue.

## License and attribution

Original repository code is licensed under MIT. Retained or adapted upstream
Turnkey material is covered by Apache-2.0. See [`LICENSE`](./LICENSE),
[`LICENSES/Apache-2.0.txt`](./LICENSES/Apache-2.0.txt), and
[`NOTICE`](./NOTICE).
