# ADR-0005: Separate public source from private package distribution

- Status: Accepted
- Date: 2026-07-17

## Context

The source repository is public OSS, while the NuGet package is distributed
only to explicitly authorized consumers through GitHub Packages. NuGet package
visibility is independent of repository visibility, but a linked package can
inherit repository permissions. Granting a public repository Actions access to
a private package can also extend package access to fork workflows.

Attaching `.nupkg` or `.snupkg` files to a public GitHub Release or Actions run
would bypass the private registry boundary even if the GitHub Packages
visibility remained `private`.

## Decision

- Keep `KyuzanInc.Turnkey.Sdk` private in GitHub Packages and disable access
  inheritance from the public source repository.
- Do not grant the public source repository GitHub Actions access to the
  private package.
- Publish only release notes and `release-checksums.txt` on public GitHub
  Releases. Do not upload package binaries as Release or Actions artifacts.
- Require a dedicated, least-privilege classic PAT in
  `NUGET_PUBLISH_TOKEN` with its `NUGET_PUBLISH_USERNAME` before a
  public-repository release can publish. Require a separate read-only
  `NUGET_READ_TOKEN` and `NUGET_READ_USERNAME` for consumer smoke
  verification. Store each pair in its own ref-restricted environment.
  Missing credentials fail closed based on live repository visibility.
- Permit the `v1.0.0` bootstrap release to use the repository
  `GITHUB_TOKEN` only while the repository is still private and package access
  inheritance is still enabled. Remove inheritance before public visibility.

## Alternatives considered

- Make the package public: rejected because distribution is intentionally
  limited to authorized consumers.
- Keep repository permission inheritance: rejected because a public repository
  would broaden package read access and create a fork access risk.
- Grant the public repository direct package Actions access: rejected for the
  same fork access risk.
- Publish from a separate private broker repository: strongest isolation, but
  deferred because it requires a separately governed repository and release
  workflow.

## Consequences

- Public users can inspect, build, and pack the OSS source, but cannot install
  the registry package without explicit package access.
- Maintainers must provision and rotate scoped package credentials and their
  corresponding usernames before the next release after `v1.0.0`.
- Release and consumer workflows fail before package access when the required
  credential is absent in a public repository.
- Checksums, and any future isolated provenance attestations, may be public
  without exposing package bytes.

## Related files

- `.github/workflows/release.yml`
- `.github/workflows/consumer-smoke.yml`
- `.github/workflows/ci.yml`
- `docs/release-process.md`
- `README.md`
