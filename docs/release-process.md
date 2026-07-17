# Release process

This document is the runbook for cutting a release of
`KyuzanInc.Turnkey.Sdk` to GitHub Packages.

It is also the policy reference for branch protection, repo labels, and
package consumption. Maintainer-only sections are marked **[admin]**.

---

## 1. Cut a release

Prerequisite: the change you want to release is already on `main` with a
clean CI run and a `release-drafter-draft.yml` run that has updated the
draft Release body.

1. Go to **Releases → Draft a new release** (or `gh release view --web`
   on the existing draft).
2. Verify the **tag**: must be strict SemVer 2.0.0 of the form
   `vMAJOR.MINOR.PATCH[-PRERELEASE]` (e.g. `v0.2.0`, `v1.0.0-alpha.3`).
   No build metadata.
3. Verify the **target**: a commit that exists on `main` (the workflow
   refuses anything whose tag commit is not contained in `main`).
4. **Pre-release toggle**:
   - For stable releases: leave the "Set as a pre-release" box
     **unchecked**.
   - For alpha / beta tags: **tick the "Set as a pre-release" box**.
     The workflow does NOT auto-detect this; the GitHub UI is the
     source of truth.
5. Review the auto-drafted body. Edit if needed.
6. Click **Publish release**. This fires the `release.yml` workflow.

### What happens after Publish

`release.yml` runs the following gates in order:

1. Strict SemVer tag check.
2. Ancestor check via GitHub compare API
   (`gh api compare/main...<tag_sha>`); accepts `behind` or
   `identical`, rejects anything else. (Not `git merge-base` —
   `actions/checkout` runs with `persist-credentials: false`, so
   `git fetch origin main` would require a persisted credential.)
3. Restore + build + full test suite. All tests must pass.
4. Pack (`.nupkg` + `.snupkg` with deterministic settings).
5. Strict path validation of the `.nupkg` and `.snupkg` contents.
6. Generate `release-checksums.txt`.
7. **Preflight** (unconditional): byte-compare an existing public
   `release-checksums.txt` asset against the locally generated file.
   Mismatch or any additional Release asset → FAIL.
8. **Registry check**: download the existing `.nupkg` from
   `nuget.pkg.github.com` if the version exists; sha256-compare with
   local. Mismatch → FAIL. 404 → new publish.
9. **Publish** to GitHub Packages via `dotnet nuget push --no-symbols`
   (only on new publish; safe-rerun skips this step).
10. **Upload** `release-checksums.txt` as the only Release asset.

The Release page hosts release notes and checksums. Package and symbol binaries
are not attached to the public Release; the `.nupkg` remains available only
from the private GitHub Packages registry at
`nuget.pkg.github.com/KyuzanInc/index.json`.

---

## 2. Prereleases

Pre-release versions use SemVer prerelease identifiers:

- `v0.1.0-alpha.0`, `v0.1.0-alpha.1`, ...
- `v0.1.0-beta.0`, `v0.1.0-beta.1`, ...
- `v0.1.0-rc.0`, ...

When you publish a Release with a prerelease tag, **you must tick the
"Set as a pre-release" checkbox in the GitHub UI**. The publish workflow
does not derive prerelease status from the tag — the GitHub Release
property is the source of truth.

Stable releases stay with the "Set as a pre-release" box unchecked.

---

## 3. Fix a broken release

Always ship `X.Y.Z+1`. **Never re-push `X.Y.Z`.** NuGet packages are
immutable once published; even if you delete the registry version, the
Release page still hosts the original `release-checksums.txt`, and the
workflow's invariant gates will refuse to publish a different `.nupkg`
under the same version.

The correct response to a broken release is:

1. Land the fix on `main` (normal PR flow).
2. Cut `vX.Y.(Z+1)` as a new Release.

If the broken release is the literal newest, you may also want to
unlist (not delete) the broken version on GitHub Packages so consumers
who follow "latest" pick up the fix. Unlist via the GitHub Packages
UI on the package page; do NOT delete unless you also intend to forfeit
the audit trail for the broken bits.

---

## 4. Same-tag reruns

If `release.yml` failed partway (transient registry error, runner
crash, etc.), you can re-trigger the same workflow run without bumping
the version. The workflow detects the partial-publish state and
decides what to do:

| State | Registry has `vX.Y.Z` | Release has checksum | Workflow does |
|---|---|---|---|
| First attempt | No | No | Publish + upload checksum |
| Partial: publish OK, checksum upload failed | Yes (byte-equal) | No | Skip publish, upload checksum |
| Safe rerun | Yes (byte-equal) | Yes (byte-equal) | Skip publish, --clobber checksum |
| Tamper: registry has DIFFERENT bytes | Yes (mismatch) | — | **FAIL**, bump version |
| Tamper: checksum file is DIFFERENT | — | Yes (mismatch) | **FAIL**, bump version |

To re-trigger the Release workflow without re-publishing the Release:

- **Preferred**: open the failed Actions run page and click
  **"Re-run failed jobs"** (or **"Re-run all jobs"**). The re-run uses
  the original `release: published` event payload, so `release.yml`
  runs again with the same tag. This is the only documented mechanism
  that re-fires `release: published` for an already-published Release.

- **NOT a re-trigger**: clicking "Edit release → Update release" in
  the GitHub UI fires `release: edited`, NOT `release: published`, so
  the publish workflow does not re-run.

- **Never** delete and re-create the Release with the same tag — the
  registry-byte invariant will refuse the publish anyway, and you
  lose the audit trail.

---

## 5. Consume the package

GitHub Packages NuGet requires authentication even for org members
(there is no anonymous access).

### One-time setup (per developer)

```bash
# 1. Create a PAT (classic) at github.com/settings/tokens with the
#    `read:packages` scope. Use classic PAT — GitHub's NuGet registry
#    docs prescribe classic PAT for direct package authentication.
#    Fine-grained PATs are not officially supported for the NuGet
#    registry by GitHub at the time of writing; if you have one, it
#    may or may not work depending on registry behaviour.

# 2. Add the source:
dotnet nuget add source \
  https://nuget.pkg.github.com/KyuzanInc/index.json \
  --name kyuzan-github \
  --username YOUR_GH_USERNAME \
  --password YOUR_PAT \
  --store-password-in-clear-text   # required on Linux/macOS

# 3. Install (replace the example with the release you need):
VERSION=1.0.0
dotnet add package KyuzanInc.Turnkey.Sdk --version "$VERSION"
```

### In an authorized private downstream GitHub Actions repository

A private downstream repository that has been granted **Read** under the
package's **Manage Actions access** list can authenticate with its
runner-issued `secrets.GITHUB_TOKEN`:

```yaml
- run: |
    dotnet nuget add source \
      "https://nuget.pkg.github.com/KyuzanInc/index.json" \
      --name kyuzan-github \
      --username "${{ github.actor }}" \
      --password "${{ secrets.GITHUB_TOKEN }}"
```

No additional PAT is needed for those explicitly authorized private
repositories. Do not grant the public source repository Actions access to the
private package; public-repository fork workflows would broaden the package
trust boundary.

---

## 6. Package access

GitHub Packages NuGet authentication is required even when the source
repository is public. Repository and package visibility are managed separately
by GitHub and must be checked before each public release announcement. The
package is intentionally kept private.

- Consumers need a GitHub account and a classic PAT with `read:packages`.
- Because the package is private, consumers also need explicit package read
  access, either directly or through an authorized private consuming
  repository.
- Making the source repository public does not change the release target:
  packages continue to publish only to GitHub Packages.
- Public GitHub Releases contain no `.nupkg` or `.snupkg` assets, so the
  Release page does not bypass private registry access.
- Pull-request and `main` CI validate packed binaries without uploading them as
  downloadable Actions artifacts.

Any visibility change is an explicit administrator operation on the GitHub
repository or package settings. This runbook does not authorize or automate it.
Publishing to nuget.org is out of scope.

### Maintainer credentials after source publication

The public source repository must not inherit private package permissions and
must not be added under the package's **Manage Actions access** list. This
prevents package access from extending to workflows from public forks.

After the source repository becomes public, maintainers configure:

- `NUGET_PUBLISH_USERNAME` and `NUGET_PUBLISH_TOKEN` as secrets in the
  protected `github-packages` environment. The username must be the PAT
  owner's GitHub login. Use a dedicated classic PAT with `write:packages`; its
  account must have **Write** or **Admin** access to this package. Restrict the
  environment to release tags (`v*`) and require maintainer approval where the
  repository plan supports it.
- `NUGET_READ_USERNAME` and `NUGET_READ_TOKEN` as secrets in the separate
  `github-packages-read` environment used by `consumer-smoke.yml`. The username
  must be the PAT owner's login. Use a separate classic PAT with
  `read:packages`; its account must have **Read** access to this package.
  Set its deployment branch policy to `main`; add a required reviewer if
  interactive approval is acceptable for the post-release smoke check.

The publish and consumer workflows query the repository's live visibility and
fail before package access if these values are absent in a public repository.
They do not trust the visibility snapshot stored in an old workflow event. The
`v1.0.0` bootstrap release may use the repository `GITHUB_TOKEN` only while the
source repository is still private and package permission inheritance is still
enabled. See
[`ADR-0005`](./adr/0005-public-source-private-package-distribution.md).

---

## 7. Symbols

The `.snupkg` is built and validated in the release workflow, but it is not
uploaded to the public GitHub Release and is not pushed to GitHub Packages.
This keeps all package binaries within the private
distribution boundary. Consumers who need symbols must build the matching tag
from source.

---

## 8. Provenance attestations

Artifact attestation is currently disabled. GitHub grants OIDC and attestation
permissions at job scope, so placing an optional attestation step in the
publish job would expose those permissions to restore, build, test, and package
publication steps even when the feature was unused.

Reintroduce attestation only through an isolated job or vetted reusable
workflow that can prove the attested bytes are exactly the bytes admitted by
the registry duplicate check before the immutable publish. Until then, the tag
ancestor check, full test suite, strict package-content validation, registry
byte invariant, checksum preflight, and `release-checksums.txt` remain the
mandatory provenance controls.

---

## 9. Repo label provisioning **[admin]**

The autolabeler (`.github/release-drafter.yml`) references the
following 8 labels. They must exist on the repo before the autolabeler
can attach them:

```bash
gh label create breaking       --color B60205 --description "Breaking change (major version bump)" --force
gh label create feature        --color 0E8A16 --description "New feature (minor version bump)"     --force
gh label create fix            --color D93F0B --description "Bug fix (patch version bump)"         --force
gh label create deps           --color 0366D6 --description "Dependency update"                    --force
gh label create docs           --color C5DEF5 --description "Documentation change"                 --force
gh label create ci             --color 5319E7 --description "CI / build change"                    --force
gh label create chore          --color CFD3D7 --description "Maintenance / chore"                  --force
gh label create skip-changelog --color FBCA04 --description "Exclude from release notes"           --force
```

Verify all 8 are present:

```bash
gh label list --limit 1000 --json name --jq '.[].name' > /tmp/repo-labels.txt
for label in breaking feature fix deps docs ci chore skip-changelog; do
  grep -Fxq "$label" /tmp/repo-labels.txt || {
    echo "::error::missing repo label: $label"; exit 1
  }
done
```

---

## 10. Branch protection **[admin]**

One-time admin task on the repo settings → Branches → `main`:

- **Require status checks to pass before merging**: enabled. Required
  checks (use the **exact** names that GitHub displays):
  - `CI / Build + test (ubuntu-latest, net8.0 runner)`
  - `CI / dotnet pack (validation)`
  - `PR title lint / Verify conventional commit title`
- **Require linear history**: enabled.
- **Require a pull request before merging**: enabled, "Squash and
  merge" allowed, "Merge commit" disabled, "Rebase merge" disabled.
- **Allow force pushes**: disabled.
- **Allow deletions**: disabled.

Repository and package visibility are not PR checks and are not continuously
changed by a workflow in this repository. Before a public release announcement,
an administrator must verify all of the following:

- repository visibility is `public`;
- package visibility is `private`;
- package access inheritance is disabled in the package settings UI;
- the public source repository is absent from **Manage Actions access**, while
  explicitly authorized private downstream repositories retain **Read**;
- no GitHub Release or active Actions artifact exposes `.nupkg` or `.snupkg`;
- secret scanning, push protection, Dependabot alerts/security updates, and
  private vulnerability reporting are enabled.

The repository visibility check is:

```bash
gh repo view KyuzanInc/turnkey-sdk-csharp \
  --json visibility \
  --jq '.visibility'
```

These checks are read-only except for the explicit administrator setup itself.
Package inheritance has no supported REST endpoint and must be verified in the
package settings UI.
