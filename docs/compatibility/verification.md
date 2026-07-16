# Compatibility verification

Compatibility is established through reproducible evidence rather than a
stored transcript from any particular review tool.

## Enforced checks

| Layer | Evidence | Enforcement |
|---|---|---|
| Upstream source integrity | Pinned TypeScript sources and SHA-256 manifest | `./tools/compatibility/verify-source-checksums.sh` and CI |
| Upstream test coverage | Each upstream test maps to a C# test or a reasoned N/A row | `./tools/compatibility/coverage-map.sh --check` and CI |
| Published-package provenance | Exact npm versions and tarball SHA-256 values | [`upstream-pins.md`](./upstream-pins.md) and [`package-checksums.txt`](../../tests/UpstreamSources/package-checksums.txt) |
| Wire/fixture behavior | Committed JSON fixtures and C# assertions | `dotnet test -c Release` |
| Primitive reference tests | RFC 5869 HKDF and existing crypto vectors | `tests/CryptoTests.cs` |
| Package integrity | Locked restore, build, test, pack, archive validation | CI and release workflows |

The generated [coverage map](./coverage-map.md) must contain zero `MISSING`
rows and every N/A row must have a non-empty explanation.

## Fixture regeneration

Node-side generators live under `tests/Fixtures/Generators/` and are not part
of the normal .NET build. Regeneration uses the committed npm lockfile:

```bash
cd tests/Fixtures/Generators
npm ci
npm run gen:all
```

Generated output must be reviewed as data: compare provenance hashes and run
the .NET tests before accepting changes.

## Current limits

- Only the deterministic API-key-stamper Node path has a committed
  Node-generated parity fixture.
- HTTP request-body byte equality against Node output is not yet committed.
- HPKE is covered by C# round-trip and upstream decrypt fixtures, but an
  externally fixed-ephemeral byte comparison to RFC 9180 remains unevaluated.
- Standard-vector coverage is incomplete beyond the committed HKDF and
  existing curve/vector tests.
- Live Turnkey backend tests are opt-in and are not a CI substitute.
- Proof fixtures validate provenance and structure only; no proof verifier is
  included.

These are explicit limitations, not implied guarantees. Changes that close a
gap should update this document and add an executable verification path.
