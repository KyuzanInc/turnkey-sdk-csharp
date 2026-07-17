# ADR-0001: Pinned upstream compatibility

- Status: Accepted
- Date: 2026-07-16

## Context

This SDK ports selected behavior from several independently versioned Turnkey
TypeScript packages. Following a floating branch or a different C# adaptation
can silently change constants, DTOs, signature behavior, or request bytes.
Committing full compiled npm package extracts, however, adds generated source
maps and large client surfaces that are not used by the build or tests.

## Decision

Compatibility is defined by exact npm package versions and tarball SHA-256
hashes. The corresponding TypeScript source at a pinned Git commit is retained
for readable comparison, coverage enumeration, and fixture provenance.
Complete compiled npm `dist/` extracts are reproducibly fetched when needed
rather than committed.

Published npm bytes outrank readable Git source if they disagree. Other C# or
Unity ports are adaptation references only.

## Alternatives considered

- Follow the latest upstream branch: rejected because compatibility would move
  without a reviewed repin.
- Treat a prior C# port as authoritative: rejected because it may target
  different upstream versions.
- Commit complete npm extracts: rejected because no current build/test path
  consumes the generated output and source maps.

## Consequences

- Every repin must update version, tarball hash, Git commit, source snapshot,
  checksums, tests, and documentation together.
- Inspecting compiled published output may require `npm pack` and network
  access, while normal build and test remain offline after restore.
- The repository stays smaller without weakening existing CI/test consumers.

## Related files

- `docs/compatibility/upstream-pins.md`
- `tests/UpstreamSources/`
- `.github/workflows/upstream-drift.yml`
