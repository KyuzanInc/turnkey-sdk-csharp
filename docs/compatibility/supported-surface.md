# Supported upstream surface

The package is a deliberately scoped port, not a complete replacement for the
Turnkey TypeScript SDK. Its supported surface is the API needed to produce
signed requests and handle the credential/import/export flows exposed by this
library.

## Ported

- `@turnkey/encoding@0.6.0`: hexadecimal, base64url, base58, base58check,
  UTF-8, and point-encoding helpers used by the library.
- `@turnkey/crypto@2.8.8`: P-256 key generation, standard-mode HPKE,
  credential-bundle decrypt, import-bundle encrypt, export-bundle decrypt,
  public-key compression/decompression, and session-JWT signature checks.
- `@turnkey/api-key-stamper@0.5.0`: the PureJS-equivalent API-key stamping
  flow using a single BouncyCastle-backed runtime.
- `@turnkey/http@3.16.0`: request construction and signing for the five
  activity endpoints exposed by `Turnkey.Http`.

## Deliberately unported

- Auth-mode HPKE (`hpkeAuthEncrypt`) and QOS encryption
  (`quorumKeyEncrypt`).
- PKCS#8 extraction and DER signature parsing helpers.
- Request/stamp verification helpers not required to construct signed
  requests.
- Proof/attestation cryptographic verification. Proof JSON is retained only as
  a provenance-checked future fixture.
- `withAsyncPolling`, the generated full HTTP client, retry/error orchestration,
  WebAuthn, and browser/node-specific stamping runtimes.
- Higher-level OAuth, on-ramp, and enclave-secret helpers.

The coverage-map ledger records each upstream test that is outside this scope
with a non-empty reason. An N/A row means the API is outside the library's
documented contract; it is not a claim that no downstream project could ever
need the feature.

Adding an unported feature requires updating the public API, tests, coverage
ledger, threat model, and this document in the same change. See
[ADR-0002](../adr/0002-supported-api-surface-and-coverage.md).
