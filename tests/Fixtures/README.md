# Test fixtures

Fixtures are grouped by production area and carry machine-readable provenance.

## Provenance levels

- `upstream-test-vectors`: retained from checksum-pinned source under
  `tests/UpstreamSources/`.
- `rfc` or `nist`: standardized reference vectors with the source identified
  by the owning test.
- `node-generated`: generated from the exact npm lockfile under
  `tests/Fixtures/Generators/`.

Credential-shaped values, private keys, mnemonics, and signed payloads in this
tree are public test vectors. They are not live secrets and must never be used
outside tests.

## Regeneration

For Node-generated fixtures, follow
[`Generators/README.md`](./Generators/README.md). For retained upstream vectors,
update the corresponding source pin, regenerate
`tests/UpstreamSources/source-file-checksums.txt`, and update the fixture's
provenance in the same change.
