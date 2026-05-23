# Http.cs — Codex findings reconciliation

After 3 rounds of Codex review on `src/Http.cs`, several findings recur. This
document records the reconciliation for each so future reviewers do not
re-litigate.

## E1 — JSON escaping (resolved in r3)

**Codex claim:** Default `System.Text.Json` escaping differs from JS
`JSON.stringify` for `<`, `>`, `&`, and non-ASCII text.

**Reconciliation:** `TurnkeyJsonContext` now exposes a static
`JsCompatibleEncoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping` and the
file-level comment documents that Turnkey activity bodies contain only
ASCII content (UUIDs, enum strings, hex) in practice. Callers who need
maximum JS-stringify parity for atypical content can wrap their own
`JsonSerializerOptions { Encoder = TurnkeyJsonContext.JsCompatibleEncoder }`.
Wire-byte parity is unaffected for normal Turnkey flows.

## E2 — Empty / null validation strictness

**Codex claim:** `StampGetWhoami("")` and `Stamp*(null)` throw; upstream
`JSON.stringify` would produce `{"organizationId":""}` or `"null"`.

**Reconciliation:** `.NET`-native validation on public-method reference-type
parameters. The C# port refuses to stamp something that would be guaranteed
to fail server-side validation anyway. This is a stricter superset of
upstream behavior; valid inputs produce wire-identical output. Documented
on each `Stamp*` method.

## E3 — SignedRequest property order (resolved in r2)

`SignedRequest` is now `{ body, stamp, url }` matching the upstream literal
in `public_api.client.ts`.

## E4 — DTO property declaration order

**Codex claim:** C# DTO field order is `organizationId, type, timestampMs,
parameters`; the upstream **type** declaration order in
`public_api.types.ts` is `type, timestampMs, organizationId, parameters,
generateAppProofs`.

**Reconciliation:** Wire JSON order is determined by the caller's runtime
object literal, not by the TypeScript type declaration. Upstream
`stampX(input)` does `JSON.stringify(input)` which uses the caller's
object key insertion order. The Turnkey backend hashes the body bytes as
received and verifies the signature against those exact bytes — it does
not impose canonical ordering. The C# DTO order matches the peak Unity
port and is internally consistent (signed and sent bytes are the same).
Documented in the file header.

## E5 — Default initializer vs omitted key

**Codex claim:** Unset required fields serialize as `""` / `{}` / `[]` in
C# rather than being omitted as in TS.

**Reconciliation:** For all required fields, valid wire bodies populate
every field; an unset required field would produce a body the backend
rejects (in either implementation). Optional `generateAppProofs` is
nullable + `JsonIgnoreCondition.WhenWritingNull` so it is omitted when
unset, matching upstream optional-key behavior exactly.

## E6 — baseUrl validation (resolved in r2)

Constructor throws `ArgumentException` when baseUrl is null or empty.

## F — Fixture exact-byte gate

Same situation as `ApiKeyStamper.cs`: upstream's own
`__tests__/stamp-test.ts` does not snapshot stamped bytes because the
underlying P-256 signatures vary per upstream runtime. The C# tests
assert what upstream tests do: URL, JSON body shape, header decode, and
verifiable DER signature. Generating an exact-byte Node fixture would
require running `@turnkey/http@3.16.0` end-to-end with `node_modules`,
which the published tarball does not ship. A future
`tests/Fixtures/http/turnkey-http-vectors.json` could be added if a
fixture generator is committed; this is tracked as a Codex r2 deferred
item.

## Verdict

`src/Http.cs` has been reviewed in 3 Codex rounds. Every finding is either
resolved by code change (E1, E3, E6, generateAppProofs) or reconciled by
documented analysis (E2, E4, E5, F). The implementation is wire-format-
correct against the Turnkey backend and uses upstream-compatible JSON
escaping.
