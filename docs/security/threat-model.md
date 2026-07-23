# Threat model — turnkey-sdk-csharp

**Scope (in)**: key handling in memory, ECDSA signing path, crypto primitives
surface, JSON serialization for wire payloads, test-secret handling.

**Scope (out)**: persistent storage, on-disk key storage, SecureStorage
implementations, OS-level keychain integration. Those belong to the consuming
application or a higher-level integration package.

This document records the current library boundary and known verification
limits. Compatibility gaps are tracked in `docs/compatibility/verification.md`.

## Assets

| Asset | Description | Confidentiality | Integrity | Availability |
|---|---|---|---|---|
| API key pair (private) | P-256 ECDSA private key held by the caller, passed to `ApiKeyStamper` | High | High | n/a |
| Session JWT | issued by Turnkey, decoded but not re-signed by this SDK | Medium | High | n/a |
| Credential bundle | HPKE-encrypted bundle exported by Turnkey, opened with the target private key | High | High | n/a |
| Target private key (HPKE session) | ephemeral P-256 private key the caller produces with `GenerateP256KeyPair`, used as HPKE recipient | High | High | n/a |
| Signed request bytes | canonical JSON body + ECDSA stamp over `SHA-256(body)` | n/a | High (wire-format must match Turnkey expectation) | n/a |

## Trust boundaries

```
Caller code ──► turnkey-sdk-csharp ──► HTTPS Turnkey API
            (passes secrets in)    (signed request bytes out)
```

- **Caller → SDK**: the caller holds and supplies all secrets. The SDK does
  not persist anything. The SDK never writes secrets to disk, never logs
  raw key material, never throws an exception that includes raw key bytes.
- **SDK → network**: the SDK produces request bytes; the caller is expected
  to send them over HTTPS. The SDK does not own the transport.
- **SDK → CI / test logs**: committed tests use public upstream or
  de-identified fixtures. CI and release jobs do not inject live Turnkey
  credentials.

## Threats and mitigations

| ID | Threat | Mitigation |
|---|---|---|
| T-1 | Signing path produces a payload that differs by a single byte from what Turnkey expects (canonical JSON drift) | Exact C# request-body assertions, ordered-property assertions, and signature verification cover the supported request builders. Node-generated HTTP body byte parity remains an explicit gap. |
| T-2 | ECDSA signature is not canonical low-S | Explicit low-S normalization; the deterministic Node fixture verifies identical `r`, the `s ↔ (n - s)` relationship, and both signatures. |
| T-3 | HPKE shared-secret derivation off by one constant or label, garbles bundle decrypt | C# round-trip tests plus pinned upstream credential/export bundle fixtures. Fixed-ephemeral RFC 9180 byte equality remains an explicit gap. |
| T-4 | HKDF Extract / Expand off-by-one or wrong info ordering | RFC 5869 SHA-256 test cases A.1-A.3. |
| T-5 | Caller passes a leading-zero P-256 scalar; BigInteger drops the leading zero, corrupts signature | leading-zero unit tests; explicit pad-to-32 helper used everywhere. |
| T-6 | AES-GCM tag/nonce/AAD layout drift vs noble | Exercised by HPKE round-trip tests and pinned Turnkey bundle fixtures. |
| T-7 | JSON serialization picks up unexpected property order, breaks Turnkey-side `SHA-256(body)` | Signed wire paths use explicit source-generated `JsonTypeInfo`; tests assert the exact `whoami` body and property order for supported DTOs. Cross-runtime Node HTTP byte parity remains an explicit gap. |
| T-8 | Reflection-based serialization path falls back at runtime under trimming, breaks signing | Signed wire paths use explicit source-generated `JsonTypeInfo`; a dedicated trimmed/AOT smoke test remains a documented gap. |
| T-9 | A future BouncyCastle bump silently changes ECDSA / AES-GCM semantics | exact pin via `[2.5.0]`; `packages.lock.json` committed. |
| T-10 | Test fixtures contain real org credentials | Only public upstream test vectors are retained and labeled; OSS-readiness scans review credential-shaped content. |
| T-11 | CI logs include signed payloads or live credentials | CI uses committed public/de-identified fixtures, does not inject live Turnkey credentials, and does not echo generated payloads. |
| T-12 | A future live-backend test leaks its test-org credentials | No live-backend harness is currently committed. Any future E2E path must use a separate manually triggered workflow, a protected environment, and sanitized logging. |
| T-13 | Sensitive material (private keys, HKDF PRKs, HPKE shared secrets, AES keys, decrypted mnemonics) stays resident in process memory longer than necessary, widening the window for memory-disclosure attacks (core dumps, swap, debugger attach) | Partial: derivation and signing paths explicitly clear `byte[]` buffers holding these values after use, shortening exposure windows and reducing the number of live copies. This reduces, but does not eliminate, residency — see "Things this SDK does NOT do." |

## Things this SDK does NOT do

- Does not store anything to disk.
- Does not implement `ISecureStorage`, Keychain, KeyStore, DPAPI, or any OS
  keystore wrapper.
- Does not provide retry / backoff / circuit breakers; that is caller scope.
- Does not handle WebAuthn / passkey stamping (outside the supported `1.0.0`
  API surface).
- Does not handle Google OAuth, OTP, or any higher-level identity flow.
  Those belong to the consuming application.
- Does not guarantee secrets are erased from memory. .NET `string` is
  immutable and cannot be zeroed in place, and the .NET GC is compacting: it
  may relocate (copy) a `byte[]` during collection before any explicit clear
  runs, leaving the pre-move bytes resident until that page is reclaimed or
  overwritten. Explicit `byte[]` clearing on the derivation and signing paths
  shortens exposure windows and reduces the number of live copies; it is a
  mitigation, not a guarantee of erasure (see T-13).
