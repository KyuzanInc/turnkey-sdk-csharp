# Fixture generators

These Node scripts run the exact upstream npm versions pinned by
`package-lock.json` and write compatibility fixtures under `tests/Fixtures/`.
The committed JSON output is consumed by the .NET tests; Node is not required
for a normal `dotnet test` run.

## Reproduce

Node 20 or a compatible version in the `package.json` engine range is required.

```bash
cd tests/Fixtures/Generators
npm ci
npm run preflight
npm run gen:all
```

The scripts record the Node version, lockfile and generator checksums,
dependency-tree checksum, and a checksum of the generated data. Review fixture
changes as code: inspect the provenance diff, then run the full .NET test suite.

## Signature normalization

The deterministic PureJS stamper fixture records the pinned upstream DER
signature. For the retained vector, upstream emits high-S while the C# library
intentionally emits canonical low-S. The fixture records the low-S equivalent
and tests enforce identical `r`, `s_csharp = n - s_upstream`, and successful
verification of both forms. See
[`docs/adr/0003-cryptographic-adaptation-policy.md`](../../../docs/adr/0003-cryptographic-adaptation-policy.md).

## Scope

The maintained executable generator currently covers the deterministic
PureJS stamper path. Other JSON fixtures are retained directly from pinned
upstream tests and carry their own source provenance. Proof fixtures validate
provenance and structure only; proof verification is not ported.
