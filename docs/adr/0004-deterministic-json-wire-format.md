# ADR-0004: Deterministic JSON wire format

- Status: Accepted
- Date: 2026-07-16

## Context

Turnkey request signatures cover exact UTF-8 JSON bytes. Property order,
escaping, null handling, or reflection-based metadata changes can therefore
produce a valid-looking request whose signature does not match its body. The
library also needs AOT-friendly serialization.

## Decision

All SDK-owned wire DTOs use `TurnkeyJsonContext`, a source-generated
`System.Text.Json` context. DTO declaration order, explicit property names,
null handling, and JS-compatible escaping are treated as wire behavior. The
exact serialized string passed to the stamper is the body returned to the
caller for transmission.

## Alternatives considered

- Reflection-based `System.Text.Json`: rejected because metadata discovery and
  trimming behavior are less predictable under AOT.
- Newtonsoft.Json: rejected to avoid another serializer and differing defaults.
- Re-serialize after signing: rejected because it can change signed bytes.

## Consequences

- New wire DTOs must be registered in the source-generated context.
- Reordering DTO properties is a compatibility change and requires fixture
  verification.
- Callers remain responsible for sending the returned body unchanged.

## Related files

- `src/TurnkeyJsonContext.cs`
- `src/Http.cs`
- `src/ApiKeyStamper.cs`
