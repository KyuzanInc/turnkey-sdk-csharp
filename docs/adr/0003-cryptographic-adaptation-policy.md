# ADR-0003: Cryptographic adaptation policy

- Status: Accepted
- Date: 2026-07-16

## Context

The TypeScript implementation uses WebCrypto, Node crypto, noble libraries,
and JavaScript byte types. A .NET port needs a provider for elliptic-curve and
AEAD primitives while retaining the upstream protocol construction. The pinned
PureJS stamper can produce a high-S ECDSA signature, while common verifiers and
this implementation use the canonical low-S form.

## Decision

BouncyCastle is limited to cryptographic primitives: P-256 ECDSA/ECDH,
AES-GCM, SHA-256, HMAC-SHA256, BigInteger/EC point operations, and the required
Ed25519 public-key derivation. HPKE labels/derivation, HKDF composition,
Tonelli-Shanks, bundle parsing, and request construction remain direct logical
ports rather than BouncyCastle high-level wrappers.

P-256 signatures emitted by the C# stamper are normalized to low-S. For an
upstream high-S signature, the C# value is the mathematically equivalent
`s_low = n - s_high`; `r` is unchanged and both signatures verify against the
same key and message.

## Alternatives considered

- Use high-level provider HPKE/KDF wrappers: rejected because their framing and
  defaults could diverge from the pinned SDK wire construction.
- Preserve upstream high-S output byte-for-byte: rejected in favor of canonical
  low-S signatures and broad verifier compatibility.
- Implement all primitives from scratch: rejected because audited provider
  primitives reduce implementation risk.

## Consequences

- Signature parity is semantic rather than DER-byte equality when upstream
  emits high-S.
- Fixture tests must verify `r`, the `n - s` relationship, and cryptographic
  validity instead of asserting identical DER bytes.
- New provider APIs require review against the primitive allowlist.

## Related files

- `src/ApiKeyStamper.cs`
- `src/Crypto.cs`
- `tests/ApiKeyStamperTests.cs`
- `tests/Fixtures/Generators/generate-stamper-vectors.mjs`
