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
   refuses anything not reachable from `origin/main`).
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
2. `git merge-base --is-ancestor` against `origin/main`.
3. Restore + build + full test suite. All tests must pass.
4. Pack (`.nupkg` + `.snupkg` with deterministic settings).
5. Strict path validation of the `.nupkg` and `.snupkg` contents.
6. Generate `release-checksums.txt`.
7. **Preflight** (unconditional): byte-compare each existing Release
   asset (`.nupkg`, `.snupkg`, `release-checksums.txt`) against the
   locally-built bytes. Mismatch → FAIL.
8. **Registry check**: download the existing `.nupkg` from
   `nuget.pkg.github.com` if the version exists; sha256-compare with
   local. Mismatch → FAIL. 404 → new publish.
9. **Attestation**: `actions/attest-build-provenance` runs BEFORE
   publish so that attestation failures abort before the immutable
   registry write.
10. **Publish** to GitHub Packages via `dotnet nuget push --no-symbols`
    (only on new publish; safe-rerun skips this step).
11. **Upload** `.nupkg` + `.snupkg` + `release-checksums.txt` as
    Release assets.

The Release page now hosts the package + symbols + checksums and the
package is available at
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

| State | Registry has `vX.Y.Z` | Release has assets | Workflow does |
|---|---|---|---|
| First attempt | No | No | Publish + upload assets |
| Partial: attestation failed | No | No (attest blocked publish) | Publish + upload assets |
| Partial: publish OK, asset upload failed | Yes (byte-equal) | No | Skip publish, upload assets |
| Partial: publish OK, some assets uploaded | Yes (byte-equal) | Yes (byte-equal) | Skip publish, --clobber upload |
| Tamper: registry has DIFFERENT bytes | Yes (mismatch) | — | **FAIL**, bump version |
| Tamper: Release asset has DIFFERENT bytes | — | Yes (mismatch) | **FAIL**, bump version |

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

# 3. Install:
dotnet add package KyuzanInc.Turnkey.Sdk --version 0.1.0-alpha.0
```

### In GitHub Actions (same org)

The Actions-auto-issued `secrets.GITHUB_TOKEN` has `read:packages` for
the same org by default:

```yaml
- run: |
    dotnet nuget add source \
      "https://nuget.pkg.github.com/KyuzanInc/index.json" \
      --name kyuzan-github \
      --username "${{ github.actor }}" \
      --password "${{ secrets.GITHUB_TOKEN }}"
```

No additional PAT needed.

---

## 6. ACL / who can install

GitHub Packages inherits visibility from the source repo by default:

- `KyuzanInc/turnkey-sdk-csharp` is **private**, so the package is
  **private** (only users with read access can install).
- Org members with repo read access → can install.
- External contributors → need to be added as repo collaborators with
  at least `Read` access, then they can use a PAT scoped to
  `read:packages` + this repo.

To change package visibility (e.g. publish to nuget.org later or open
to a wider audience), an org admin must explicitly do so on the
package page → "Package settings → Change visibility".

---

## 7. Symbols

The `.snupkg` is uploaded as a **GitHub Release asset only**. It is
NOT pushed to GitHub Packages because GitHub Packages does not run a
symbol server.

For debug symbols today: download the `.snupkg` from the Release page
and load it via Visual Studio's "Tools → Options → Debugging → Symbols"
or `dotnet-symbol`.

SourceLink-driven step-through into the repo source is a v0.2 follow-up.

---

## 8. Attestation

Every published `.nupkg` and `.snupkg` carries a GitHub-issued build
provenance attestation. Verify with the GitHub CLI:

```bash
gh attestation verify \
  --owner KyuzanInc \
  KyuzanInc.Turnkey.Sdk.0.1.0-alpha.0.nupkg
```

The attestation proves the artifact was built by the
`KyuzanInc/turnkey-sdk-csharp` repo's `release.yml` workflow at the
tag's commit SHA, in the SLSA build-level-3-compatible GitHub Actions
runner.

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

`Repo privacy check` runs on `push: main` and the weekly cron, not on
`pull_request`. Do NOT include it in the required-check list (PRs
cannot satisfy it). The post-merge / weekly-cron runs catch accidental
public-visibility flips on the next trigger, not instantly.
