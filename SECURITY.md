# Security policy

## Supported versions

Security fixes are provided for the latest stable `1.x` release line.
Prerelease versions and versions older than `1.0.0` are not supported after
the stable release is available.

| Version | Supported |
|---|---|
| Latest stable `1.x` | Yes |
| Older `1.x` | No |
| `< 1.0.0` | No |

## Reporting a vulnerability

Please do not open a public issue for a suspected vulnerability. Use GitHub's
private vulnerability reporting flow from this repository's **Security** tab
and select **Report a vulnerability**.

Include, when possible:

- the affected package version and target framework;
- the security impact and expected trust boundary;
- a minimal reproduction or test vector;
- whether private-key material, signatures, or signed wire data are involved;
- any suggested mitigation.

Do not include real credentials, production private keys, customer data, or
other secrets. Use generated test keys and redacted identifiers.

Maintainers will acknowledge a complete report as soon as practicable,
coordinate remediation privately, and publish an advisory when users can take
effective action. Response and release timing depends on severity and the
quality of the reproduction.

## Scope

This policy covers the code and release pipeline in this repository. The
authoritative Turnkey services and TypeScript SDK are maintained separately by
Turnkey, Inc.; this community-maintained .NET SDK is not affiliated with or
endorsed by Turnkey, Inc.
