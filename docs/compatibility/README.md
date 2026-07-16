# Compatibility documentation

This directory contains the public, maintained compatibility contract for the
.NET port.

- [Upstream pins](./upstream-pins.md) — exact npm versions, hashes, source
  hierarchy, and repinning procedure.
- [Supported surface](./supported-surface.md) — ported and deliberately
  unported APIs.
- [Verification](./verification.md) — machine-verifiable checks and known
  gaps.
- [Coverage map](./coverage-map.md) — generated upstream-test-to-C#-test map.
- [Architecture decisions](../adr/README.md) — durable design rationale.

The raw output of particular review tools is not part of the compatibility
contract. Trust claims should be backed by pinned sources, fixtures, tests,
checksums, and reproducible commands.
