# ADR-0002: Supported API surface and coverage exclusions

- Status: Accepted
- Date: 2026-07-16

## Context

The upstream packages expose full HTTP clients, multiple runtime backends,
polling, proof verification, auth-mode encryption, and other features beyond
this library's request-signing and bundle-handling responsibilities. Claiming a
complete port would be misleading, while silently ignoring upstream tests
would allow the implemented scope to drift.

## Decision

The SDK exposes a documented subset. Every upstream test discovered by the
coverage tool must either map to a C# test or appear in the N/A ledger with a
specific, non-empty reason tied to the documented library boundary.

N/A decisions are based on this package's public contract, not on a claim that
a particular private consumer does not use the API.

## Alternatives considered

- Port the complete upstream SDK: rejected for the current package because it
  would add unrelated orchestration and runtime surfaces.
- Ignore tests for unported APIs: rejected because omissions would become
  invisible and unauditable.
- Base scope only on one downstream code search: rejected because consumer
  usage changes and is not a stable public contract.

## Consequences

- Adding a feature requires implementation, tests, coverage-ledger updates,
  and documentation in one change.
- The coverage map can remain green with N/A rows only when their reasons are
  explicit.
- Consumers can distinguish missing features from defects.

## Related files

- `docs/compatibility/supported-surface.md`
- `tools/compatibility/coverage-map.na.tsv`
- `tools/compatibility/coverage-map.sh`
