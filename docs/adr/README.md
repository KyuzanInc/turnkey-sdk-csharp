# Architecture decision records

ADRs record durable decisions with meaningful alternatives and consequences.
They do not archive implementation plans, review transcripts, or temporary
findings.

Status values:

- **Accepted** — current policy.
- **Superseded** — replaced by a later ADR.
- **Deprecated** — retained for history but no longer recommended.

| ADR | Decision | Status |
|---|---|---|
| [0001](./0001-pinned-upstream-compatibility.md) | Pin compatibility to published npm artifacts and readable source snapshots | Accepted |
| [0002](./0002-supported-api-surface-and-coverage.md) | Maintain a deliberately scoped API with reasoned coverage exclusions | Accepted |
| [0003](./0003-cryptographic-adaptation-policy.md) | Limit BouncyCastle to primitives and normalize P-256 signatures to low-S | Accepted |
| [0004](./0004-deterministic-json-wire-format.md) | Use source-generated, deterministic JSON for signed wire bytes | Accepted |
