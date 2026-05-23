## VERDICT: GO

No new cross-file integration blocker found. I would ship `v0.1.0-alpha` for internal preview.

## A: Pin / fixture provenance audit

Pass. The source pins list the same tarball SHA256 values as the existing fixture provenance:

- `@turnkey/encoding@0.6.0`: [turnkey-source-pins.md](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/turnkey-source-pins.md:12), [fixture](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/tests/Fixtures/encoding/turnkey-encoding-vectors.json:3)
- `@turnkey/crypto@2.8.8`: [turnkey-source-pins.md](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/turnkey-source-pins.md:13), [fixture](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/tests/Fixtures/crypto/turnkey-crypto-vectors.json:3)
- `@turnkey/api-key-stamper@0.5.0`: [turnkey-source-pins.md](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/turnkey-source-pins.md:15), [fixture](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/tests/Fixtures/api-key-stamper/turnkey-api-key-stamper-vectors.json:3)

The Git tag SHAs are documented as secondary metadata because `gitHead` is not present in the npm tarballs: [turnkey-source-pins.md](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/turnkey-source-pins.md:33).

## B: Cross-file integration audit

Pass.

`Crypto.FormatHpkeBuf` serializes `HpkeBundlePayload` through `TurnkeyJsonContext.JsCompatibleOptions`: [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:639). The context includes that DTO and the same options resolver/encoder used by the other source-gen call sites: [TurnkeyJsonContext.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/TurnkeyJsonContext.cs:42), [TurnkeyJsonContext.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/TurnkeyJsonContext.cs:72).

`ApiKeyStamper` derives the public key via `Crypto.GetPublicKey` before signing, matching upstream purejs validation timing: [ApiKeyStamper.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/ApiKeyStamper.cs:153), [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:353). The upstream purejs path does the same derived-public-key check before signing.

`Http.GetHttpClient` chains bundle decrypt -> public-key derivation -> `ApiKeyStamper` construction correctly: [Http.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Http.cs:102). Stamped request bodies are signed over the exact JSON string returned to the caller: [Http.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Http.cs:203).

## C: D17 enforcement across codebase

Pass. I found no `HkdfBytesGenerator` usage and no `Org.BouncyCastle.Crypto.Hpke.*` usage.

The BouncyCastle use stays inside the allowed primitive set: P-256 ECDSA signing in [ApiKeyStamper.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/ApiKeyStamper.cs:179), P-256 ECDH in [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:927), AES-GCM with 128-bit tag in [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:1015), P-256 ECDSA verify in [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:1048), BigInteger/Base58 in [Encoding.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Encoding.cs:561), and Ed25519 public-key derivation only for SOLANA export formatting in [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:846).

## D: Upstream vector parity assertion

Pass. `DecryptCredentialBundle_UpstreamVector` is present and uses the Turnkey-pinned upstream credential bundle from `crypto-test.ts:179-184`: [CryptoTests.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/tests/CryptoTests.cs:524). It asserts exact decrypted private-key hex equality: [CryptoTests.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/tests/CryptoTests.cs:536). That is byte-equality through the canonical lowercase hex representation.

## E: Wire-format spot-check

Pass. I did not find an obvious wire-format issue that would prevent live backend interop.

The main backend-facing paths are internally consistent: Base58Check bundle decode is used for credential bundles: [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:668); HPKE decrypt uses uncompressed sender/receiver keys and exact AAD concatenation: [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:420); API stamps use `{ publicKey, scheme, signature }` and base64url JSON: [ApiKeyStamper.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/ApiKeyStamper.cs:129); HTTP paths and body/stamp pairing match upstream signed-request shape: [Http.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Http.cs:231).

I did not run `dotnet test` because this session is read-only and the test/build pipeline would write `bin/obj` artifacts.

## Notes for v0.2 follow-up (deferable)

- Add committed `tests/Fixtures/http/turnkey-http-vectors.json`; this is already documented as deferred in the Http reconciliation doc: [Http.cs-codex-findings-reconciliation.md](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/codex-crypto-reviews/Http.cs-codex-findings-reconciliation.md:67).
- Remove the broad unused `Org.BouncyCastle.Crypto.Generators` using in [Crypto.cs](/Users/takeshi/Kyuzan/src/turnkey-sdk-csharp/src/Crypto.cs:55) to make D17 cleaner, although it is not a runtime violation.
- Strengthen `HttpTests.Stamp_HeaderValueDecodesAndVerifies`; it currently checks DER shape, while `ApiKeyStamperTests` does the real signature verification.

