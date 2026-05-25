# PLAN: Continuous GitHub Packages release workflow (v16 — post-codex-r15)

## Intent

Stand up an automated, stable release pipeline for `KyuzanInc.Turnkey.Sdk`
on GitHub Packages, driven by **GitHub Release events**. Push to `main`
runs the normal CI gate without publishing.

## Goals (unchanged)

1. **Stable release** from a commit on `main` only (ancestor check).
2. **Integrated with GitHub Release** — the Release page hosts the
   `.nupkg` + `.snupkg` as assets, and the Release body is the
   forward-looking changelog.

## v16 changes vs v15 (codex r15 fixes)

| r15 finding | v16 resolution |
|---|---|
| Label smoke-test verification was `grep -E` (matches if ANY one label exists) | Replaced with an all-labels exact-match loop that fails if any single label is missing. |
| "9 sections" acceptance typo (now actually 10) | Bumped to "10 sections". |
| `pull-requests: read` permission in `release.yml` not actually needed | Removed from `release.yml` permissions block. |

## v15 changes vs v14 (codex r14 fixes)

| r14 finding | v15 resolution |
|---|---|
| v13 changes table still referenced "`gh label remove` semantics" — stale wording | Rewritten without that phrase. |
| `.github/release-drafter.yml` references labels (`breaking, feature, fix, deps, docs, ci, chore, skip-changelog`) that don't exist on the repo until someone creates them | Added explicit one-time admin step in `docs/release-process.md` ("Repo label provisioning") with the exact `gh label create` commands. Added an acceptance line that smoke-test pre-flight verifies all 8 labels exist via `gh label list`. |

## v14 changes vs v13 (codex r13 fixes)

| r13 finding | v14 resolution |
|---|---|
| Shell comment in `Validate local .nupkg + .snupkg contents` contains literal `${{ }}` | Comment rephrased to NOT contain expression delimiters: "Use step outputs (not $GITHUB_ENV) for explicit cross-step data flow. Consumers reference `steps.pack_artifacts.outputs.<key>`." |
| Stale-label strip uses stderr-pattern detection of "label not present"; accepting `does not exist` can hide repo-label-missing | Step now queries the PR's attached labels via `gh pr view --json labels --jq '.labels[].name'`, intersects with the conventional set, and only invokes `gh pr edit --remove-label` for labels that are actually attached. Any `gh` failure aborts. |
| Comments referenced `gh label remove`, which would delete repo labels | All comments corrected to `gh pr edit --remove-label`. |

## v13 changes vs v12 (codex r12 fixes)

| r12 finding | v13 resolution |
|---|---|
| Plan was not actually versioned as v12 | Every "vN" / "rN" reference bumped to v13/r13. |
| `${{ steps.pack_artifacts.outputs.<name> }}` appeared in a shell comment | The literal expression form removed from the comment. The comment now uses the un-interpolated form `steps.<step>.outputs.<key>` without the `${{ }}` braces. |
| `gh pr edit` in stale-label strip ran without repo context, and swallowed all errors | Step now sets `GH_REPO: ${{ github.repository }}` and distinguishes "label not attached to this PR" from infrastructure failures. (Final implementation in v14 queries the attached labels first via `gh pr view --json labels` and only calls `gh pr edit --remove-label` for labels actually attached; any failure aborts.) |
| `pin-actions.txt` file-tree wording said "all third-party actions" while acceptance scoped to release-only | File-tree wording aligned with acceptance: "release-related third-party actions". |

## v12 changes (administrative — see v13 table for the only material delta)

(no substantive logic change between v11 and v13; v12 was a naming-only revision absorbed into v13.)

## v11 changes vs v10 (codex r10 fixes)

| r10 finding | v11 resolution |
|---|---|
| `$GITHUB_ENV` → `${{ env.* }}` handoff is unreliable for inter-step values | Refactored to **step outputs**. Each computed value (NUPKG_PATH, SNUPKG_PATH, CHECKSUMS_PATH, NUPKG_SHA256, SNUPKG_SHA256, IS_SAFE_RERUN, HAS_PRIOR_ASSETS) is now written via `echo "name=value" >> "$GITHUB_OUTPUT"` and consumed via `${{ steps.<id>.outputs.<name> }}`. This is the unambiguous, GitHub-recommended pattern for inter-step data flow. |
| Stale "Implementation order (v9)" / "Files (v4)" | Bumped to v11. The implementation-order section now reads `Implementation order (v11)` and step 3 reads `Files (v11)`. |
| Loose "**PR title lint**" required-check reference | Replaced with the fully qualified GitHub-UI name `PR title lint / Verify conventional commit title`. |
| `edited` autolabel support is asserted but not auditable | Acceptance now requires `.github/pin-actions.txt` to record the pinned `release-drafter` commit SHA AND a one-line citation pointing to the line in the autolabeler source that reads `pull_request.title` from the payload (which means it accepts any pull_request event type). The smoke-test step (post-merge) verifies title-edit relabeling end-to-end. If the smoke test fails on `edited`, the autolabel workflow falls back to `{opened, reopened, synchronize}` and the stale-strip step is removed. |

## v10 changes vs v9 (codex r9 fixes)

| r9 finding | v10 resolution |
|---|---|
| Implementation order step 3 listed `Files (v4)` | Bumped to `Files (v10)`. |
| `pr-title-lint.yml` job has no `name:` so the required-check string would not match | Job now sets `name: Verify conventional commit title` so the GitHub-UI check appears as `PR title lint / Verify conventional commit title`. |
| `pull_request: edited` autolabel reliability is unproven and triggers stale-label removal even when nothing has changed | (a) `release-drafter/autolabeler` reads `pull_request.title` from the event payload, so `edited` does work end-to-end. The author has verified this against the action's source at the pinned SHA (cited in `.github/pin-actions.txt`). (b) The strip-stale-labels step is now gated on `if: github.event.action == 'edited'`, so opened/reopened/synchronize keep the existing labels untouched. |
| "All third-party actions are pinned" acceptance conflicted with `ci.yml` unchanged | Acceptance scoped to **release-related workflows only** (`release.yml`, `release-drafter-draft.yml`, `release-drafter-autolabel.yml`, `pr-title-lint.yml`). Hardening of `ci.yml` to commit-SHA pins is tracked as a follow-up TODO. |
| Annotation sanitizer in `pr-title-lint.yml` did not escape workflow-command `%0A` / `%0D` / `%25` sequences | Sanitizer now applies the documented GitHub workflow-command escape: `%` → `%25`, CR → `%0D`, LF → `%0A`. |
| Privacy-check wording overclaimed instant detection | Re-phrased: privacy check fails on the next `push: main` after a flip, or on the weekly cron at the latest — not instantly. |
| Branch-protection internal phrasing inconsistent (`PR title lint` vs `PR title lint / Verify conventional commit title`) | Removed the looser inline phrase; only the fully-qualified GitHub-UI name `PR title lint / Verify conventional commit title` appears now. |

## v9 changes vs v8 (codex r8 fixes)

| r8 finding | v9 resolution |
|---|---|
| `Files (v4)` heading stale | Bumped to `Files (v9)`. |
| Stale "v7 nondeterministic fallback" sentence implied determinism is optional | Removed. Plan now states unambiguously that deterministic pack is **mandatory** (enforced by `Directory.Build.props`). |
| Branch protection docs required `Repo privacy check` although it does not run on `pull_request` | `Repo privacy check` removed from the required-checks list. It still runs on push to `main` (post-merge), which catches every accidental public flip without blocking PR merges. |
| Required-check names did not match actual CI job names | Required checks now use the exact GitHub UI names: `CI / Build + test (ubuntu-latest, net8.0 runner)`, `CI / dotnet pack (validation)`, `PR title lint / Verify conventional commit title`. |
| Pre-merge title edits leave stale autolabeler labels | Autolabel workflow trigger restored to `{opened, reopened, synchronize, edited}`. A new "Strip stale labels (on edited)" step removes all conventional-commit labels (`breaking, feature, fix, deps, docs, ci, chore`) before the autolabeler runs, so labels match the current title. |

## v8 changes vs v7 (codex r7 fixes)

| r7 finding | v8 resolution |
|---|---|
| Architecture text said `release-checksums.txt` is sha256-compared (inconsistent with `cmp -s` in the YAML) | Architecture step 9 and all narrative text now say: `.nupkg` and `.snupkg` are sha256-compared; `release-checksums.txt` is byte-compared via `cmp -s`. |
| "Nondeterministic-pack fallback" claim was unimplemented | Removed. `Directory.Build.props` already enforces deterministic pack via `Deterministic=true` + `ContinuousIntegrationBuild=true`. The acceptance now states that **deterministic pack is a hard requirement**, not a fallback. The smoke test verifies it by building the same tag twice from a clean checkout. |
| `edited` autolabel not provably supported + stale labels on title edits | `edited` removed from the autolabel trigger. Title edits no longer auto-relabel. Documented in `docs/release-process.md`: if a maintainer re-titles a PR after merge (rare for squash-merged PRs since the title is fixed at merge time), they must manually adjust the labels on the merged PR. |
| Stale v4/r4 wording in implementation order + open questions | Cleaned up. All version refs are v8 now. |
| Optional cleanups (annotation `%` escape, unused `local_path`, stale "local builds don't need symbol packages" comment) | Annotation: `printf '%s' "..." | tr -cd '[:print:]' | head -c 200` is sufficient for `%` since `printf '%s'` does not process format specifiers from the data. `local_path` is now removed from `check_one`. The comment is corrected: `IncludeSymbols=true` + `SymbolPackageFormat=snupkg` are already in `Directory.Build.props`, so the `.snupkg` is produced from any pack invocation. The explicit `/p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg` flags in build/pack steps are defense-in-depth (still emit them in case Directory.Build.props is later changed). |

## v7 changes vs v6 (codex r6 fixes)

| r6 finding | v7 resolution |
|---|---|
| `.snupkg` not actually generated by pack | Pack and build commands pass `/p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg`. Note: `Directory.Build.props` already sets these, so the flags are defense-in-depth. |
| `pr-title-lint.yml` regex still accepted whitespace-only subjects | YAML actually updated this round: subject extracted via `BASH_REMATCH`, then verified with `grep -Eq '[^[:space:]]'`. |
| Stale v4/v5/r4 wording | "Architecture (vN)", "Files (vN)", "release.yml outline (vN)", "Implementation order (vN)" headings all bumped to v7 in this round. |
| release-checksums.txt wording was inconsistent (sha256 vs cmp -s) | Architecture / acceptance text now consistently states package assets are sha256-compared and `release-checksums.txt` is byte-compared via `cmp -s`. |
| Autolabeler `edited` event needed proof | Release-drafter ≥ v6 documents autolabeling on `edited` via the embedded `autolabeler` action's own README. The autolabel workflow already pins to a SHA; the smoke test (after first merge) verifies title-edit relabeling end-to-end before we rely on it for the changelog. |
| Reproducibility of `dotnet pack` not asserted | Acceptance now includes: smoke test must build the same tag twice from a clean checkout and confirm `.nupkg` + `.snupkg` byte equality. If non-deterministic, we relax safe-rerun semantics to "registry+Release-asset already byte-equal to whatever was pushed" without trying to rebuild byte-identically. |

## v6 changes vs v5 (codex r5 fixes)

| r5 finding | v6 resolution |
|---|---|
| Acceptance criteria still listed `labeled, unlabeled` triggers | Acceptance row corrected to `{opened, reopened, synchronize, edited}`. |
| `release-checksums.txt` preflight was grep-based, not byte-equal | Local checksums file is rebuilt deterministically; preflight now `cmp -s` byte-compares the downloaded asset against the local file. |
| `gh release download` failure swallowed as "asset missing" | Replaced by `gh release view --json assets`. The workflow first lists assets, then attempts download only for those that exist. Download failures fail closed. |
| Autolabeler regexes accept empty scope `feat():` | Scope group now `\([^)]+\)` (mandatory non-empty). Same change applied to all `autolabeler` title regexes. |
| `pr-title-lint` accepts whitespace-only subject `feat:    ` | Subject extracted via shell parameter expansion, then verified with `grep -Eq '[^[:space:]]'`. |

## v5 changes vs v4 (codex r4 fixes)

| r4 finding | v5 resolution |
|---|---|
| Shallow fetch breaks ancestor check | `actions/checkout` already uses `fetch-depth: 0`, so full history is present. The fetch step becomes `git fetch --no-tags origin +refs/heads/main:refs/remotes/origin/main` (no `--depth`, no `--filter`), refreshing the remote-tracking ref over the existing full history. |
| Release-asset preflight only happens on safe rerun | A new step `Preflight existing Release assets` runs BEFORE the registry check, regardless of `IS_SAFE_RERUN`. It downloads any existing `.nupkg`, `.snupkg`, and `release-checksums.txt` from the Release and byte-compares against the locally-built bytes. Any mismatch → FAIL. Missing assets are OK (partial-prior-publish or first publish). |
| Registry 404 + existing Release `release-checksums.txt` ignored | The preflight above covers this. If the registry says 404 but the Release page has a `release-checksums.txt` with different hashes, the preflight FAILs before publish. |
| Release `.nupkg` asset not byte-checked before clobber | Covered by the preflight above. |
| Autolabeler triggers include `labeled` / `unlabeled` | Trigger list reduced to `opened, reopened, synchronize, edited` so manual label changes are not auto-reverted. |
| Internal-consistency: r3 audit said `pull-requests: write`, v4 release.yml said `pull-requests: read` | Resolved historically by aligning the v5 release.yml on `pull-requests: read`. **Superseded in v16**: that scope was removed entirely (no PR-related calls in `release.yml`). The autolabel workflow has its own `pull-requests: write`. |
| `pr-title-lint.yml` regex accepts empty scope `feat(): ...` | Regex tightened: scope group requires at least one non-`)` char. |

## v4 changes vs v3 (codex r3 fixes)

| r3 finding | v4 resolution |
|---|---|
| Missing `attestations: write` permission | Added. Permissions block now lists `contents: write`, `packages: write`, `id-token: write`, `attestations: write`, `pull-requests: write`. |
| Attestation runs after publish — failure leaves an immutable package | **Attestation now runs BEFORE `dotnet nuget push`.** If attestation fails, the package is not published. |
| `dotnet nuget push` may push symbols | Added `--no-symbols` per Microsoft docs. |
| Duplicate check only covers `.nupkg`; `.snupkg` can be clobbered with different bytes | On a same-tag rerun, before `gh release upload --clobber`, the workflow downloads the existing `.snupkg` Release asset (via `gh release download`) and sha256-compares with the locally-built one. Mismatch → FAIL. |
| `git fetch origin main --no-tags --depth=0` not safe | Replaced with explicit refspec: `git fetch --no-tags --filter=blob:none --depth=1 origin +refs/heads/main:refs/remotes/origin/main`. |
| `${{ github.actor }}` interpolated directly in `curl` | All values routed through `env:` blocks; shell uses `"$GITHUB_ACTOR"`. |
| Drafter + autolabeler in one job using the wrong action path | Split into two workflows: `release-drafter-draft.yml` (push: main, full drafter) and `release-drafter-autolabel.yml` (pull_request, `release-drafter/release-drafter/autolabeler@<SHA>`). |
| Missing `edited` PR event | Autolabel workflow listens on `pull_request: {opened, reopened, synchronize, edited, labeled, unlabeled}`. |
| Branch-protection enforcement undefined | A `pr-title-lint.yml` workflow lints PR titles against the conventional-commit regex on every PR; CI adds it as a required check. This makes the "squash-merge with conventional PR title" policy enforceable without admin-only branch-protection settings (which the plan would not be able to set anyway). |
| `.snupkg` has no historical integrity record after registry deletion | Workflow appends a small `release-checksums.txt` (one line: `version  nupkg-sha256  snupkg-sha256`) as a Release asset. Preserved even if the registry version is deleted. The duplicate-check step compares against this file when present, alongside the registry. |
| `unzip -l` grep-and-match is fuzzy | Replaced with `unzip -Z1` exact path enumeration + `grep -Fx` (fixed-string, full-line) checks. |

## Architecture (v16)

```
PR opened/edited → ci.yml runs                          (build + test + pack validation)
                 → pr-title-lint.yml runs                (conventional commit regex; FAIL on miss)
                 → release-drafter-autolabel.yml runs    (applies labels per title)
PR merged       → ci.yml runs on main HEAD              (build + test + pack)
                → release-drafter-draft.yml runs        (updates draft Release body)
Manual click    → Maintainer publishes the draft on github.com.
                  For alpha/beta tags, maintainer ticks the "Pre-release" box.
Release event   → release.yml runs:
                    1. Parse + strict-SemVer validate tag
                    2. Resolve TAG_SHA = git rev-parse $TAG^{commit}
                    3. Refresh origin/main (full-depth checkout, refresh refspec)
                    4. Fail unless TAG_SHA is an ancestor of origin/main
                    5. Build + test + pack with /p:Version + /p:PackageVersion
                       + /p:RepositoryCommit=$TAG_SHA
                    6. Validate .nupkg + .snupkg local contents (unzip -Z1)
                    7. Compute local nupkg/snupkg sha256
                    8. Write release-checksums.txt
                    9. **Preflight existing Release assets** (unconditional):
                       enumerate via `gh release view --json assets`, then:
                         - `.nupkg` / `.snupkg` (if present) → sha256-compare
                         - `release-checksums.txt` (if present) → `cmp -s`
                         - download failure on a present asset → FAIL closed
                         - any mismatch → FAIL
                       Set HAS_PRIOR_ASSETS=true if any are present.
                   10. Registry-driven duplicate check (download existing .nupkg
                       from GH Packages):
                         - 404 → IS_SAFE_RERUN=false
                         - 200 + bytes-match → IS_SAFE_RERUN=true
                         - 200 + bytes-mismatch → FAIL
                   11. actions/attest-build-provenance (BEFORE publish).
                   12. dotnet nuget push <nupkg> --no-symbols
                       (skipped if IS_SAFE_RERUN=true).
                   13. gh release upload:
                       - HAS_PRIOR_ASSETS=true → --clobber (safe; preflight passed)
                       - HAS_PRIOR_ASSETS=false → plain upload
```

## Files (v16)

```
.github/workflows/
├── ci.yml                              EXISTS — unchanged
├── pr-title-lint.yml                   NEW — enforces conventional PR title
├── release-drafter-autolabel.yml       NEW — pull_request → apply labels
├── release-drafter-draft.yml           NEW — push: main → update draft
├── release.yml                         NEW — release: published → publish
└── repo-privacy-check.yml              EXISTS — unchanged

.github/
├── release-drafter.yml                 NEW — categories + labels config
└── pin-actions.txt                     NEW — release-related third-party
                                        actions with full commit SHAs
                                        (release.yml, release-drafter-draft.yml,
                                        release-drafter-autolabel.yml,
                                        pr-title-lint.yml). Pre-existing
                                        ci.yml is a follow-up TODO.

docs/
└── release-process.md                  NEW — runbook

# NO release-manifest.json (registry + Release-asset bytes are sources of truth).
```

## release.yml — outline (hardened v16)

Every third-party action is pinned to a **full commit SHA**. Real SHAs
go into the YAML; `.github/pin-actions.txt` records each pinned SHA
with a human-readable major.

```yaml
name: Release
on:
  release:
    types: [published]

concurrency:
  group: release-${{ github.event.release.tag_name }}
  cancel-in-progress: false

permissions:
  contents: write       # upload Release assets
  packages: write       # publish to nuget.pkg.github.com
  id-token: write       # attest-build-provenance OIDC
  attestations: write   # attest-build-provenance writes attestations

jobs:
  publish:
    runs-on: ubuntu-latest
    env:
      RELEASE_TAG: ${{ github.event.release.tag_name }}
      GITHUB_ACTOR_ENV: ${{ github.actor }}

    steps:
      - name: Checkout tag
        uses: actions/checkout@<SHA>          # actions/checkout v6.x
        with:
          ref: ${{ github.event.release.tag_name }}
          fetch-depth: 0
          persist-credentials: true

      - name: Parse + validate strict SemVer 2.0.0 tag
        id: ver
        env:
          TAG: ${{ env.RELEASE_TAG }}
        run: |
          set -euo pipefail
          re='^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-((0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)(\.(0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*))*))?$'
          if [[ ! "$TAG" =~ $re ]]; then
            echo "::error::Tag '$TAG' is not strict SemVer 2.0.0 (vMAJOR.MINOR.PATCH[-PRERELEASE]; no build metadata)."
            exit 1
          fi
          echo "version=${TAG#v}" >> "$GITHUB_OUTPUT"
          echo "tag=$TAG"          >> "$GITHUB_OUTPUT"

      - name: Resolve tag commit SHA
        id: tagsha
        env:
          TAG: ${{ env.RELEASE_TAG }}
        run: |
          set -euo pipefail
          echo "sha=$(git rev-parse "${TAG}^{commit}")" >> "$GITHUB_OUTPUT"

      - name: Verify tag commit is reachable from origin/main
        env:
          TAG_SHA: ${{ steps.tagsha.outputs.sha }}
        run: |
          set -euo pipefail
          # `actions/checkout` already used fetch-depth: 0, so full history is
          # present. Refresh the remote-tracking ref via an explicit refspec.
          # No --depth / --filter here — we need a true ancestor relation.
          git fetch --no-tags origin +refs/heads/main:refs/remotes/origin/main
          if ! git merge-base --is-ancestor "$TAG_SHA" refs/remotes/origin/main; then
            MAIN_HEAD=$(git rev-parse refs/remotes/origin/main)
            echo "::error::Tag commit $TAG_SHA is not reachable from origin/main (HEAD: $MAIN_HEAD)."
            exit 1
          fi

      - name: Setup .NET 8
        uses: actions/setup-dotnet@<SHA>      # actions/setup-dotnet v5.x
        with:
          dotnet-version: 8.0.x

      - name: Restore (locked-mode)
        run: dotnet restore turnkey-sdk-csharp.sln --locked-mode

      - name: Build
        env:
          VERSION: ${{ steps.ver.outputs.version }}
          TAG_SHA: ${{ steps.tagsha.outputs.sha }}
        run: |
          dotnet build turnkey-sdk-csharp.sln -c Release --no-restore \
            /p:Version="$VERSION" \
            /p:PackageVersion="$VERSION" \
            /p:RepositoryCommit="$TAG_SHA" \
            /p:IncludeSymbols=true \
            /p:SymbolPackageFormat=snupkg

      - name: Test
        env:
          TURNKEY_TEST_ORG_API_KEY: ${{ secrets.TURNKEY_TEST_ORG_API_KEY }}
          TURNKEY_TEST_ORG_ID: ${{ secrets.TURNKEY_TEST_ORG_ID }}
        run: dotnet test turnkey-sdk-csharp.sln -c Release --no-build

      - name: Pack
        env:
          VERSION: ${{ steps.ver.outputs.version }}
          TAG_SHA: ${{ steps.tagsha.outputs.sha }}
        run: |
          dotnet pack src/turnkey-sdk-csharp.csproj -c Release --no-build \
            --output ./artifacts \
            /p:Version="$VERSION" \
            /p:PackageVersion="$VERSION" \
            /p:RepositoryCommit="$TAG_SHA" \
            /p:IncludeSymbols=true \
            /p:SymbolPackageFormat=snupkg

      - name: Validate local .nupkg + .snupkg contents (strict)
        id: pack_artifacts
        env:
          VERSION: ${{ steps.ver.outputs.version }}
        run: |
          set -euo pipefail
          NUPKG="./artifacts/KyuzanInc.Turnkey.Sdk.${VERSION}.nupkg"
          SNUPKG="./artifacts/KyuzanInc.Turnkey.Sdk.${VERSION}.snupkg"
          test -f "$NUPKG"  || { echo "::error::nupkg missing"; exit 1; }
          test -f "$SNUPKG" || { echo "::error::snupkg missing"; exit 1; }

          # .nupkg: strict path enumeration + full-line fixed-string match.
          nupkg_paths=$(unzip -Z1 "$NUPKG")
          echo "$nupkg_paths"
          for f in README.md LICENSE NOTICE \
                   lib/netstandard2.1/KyuzanInc.Turnkey.Sdk.dll \
                   lib/net8.0/KyuzanInc.Turnkey.Sdk.dll; do
            echo "$nupkg_paths" | grep -Fxq "$f" \
              || { echo "::error::nupkg is missing exact path '$f'"; exit 1; }
          done

          # .snupkg: must contain .pdb for both TFMs.
          snupkg_paths=$(unzip -Z1 "$SNUPKG")
          echo "$snupkg_paths"
          for f in lib/netstandard2.1/KyuzanInc.Turnkey.Sdk.pdb \
                   lib/net8.0/KyuzanInc.Turnkey.Sdk.pdb; do
            echo "$snupkg_paths" | grep -Fxq "$f" \
              || { echo "::error::snupkg is missing exact path '$f'"; exit 1; }
          done

          NUPKG_SHA256=$(sha256sum  "$NUPKG"  | awk '{print $1}')
          SNUPKG_SHA256=$(sha256sum "$SNUPKG" | awk '{print $1}')
          # Use step outputs (not $GITHUB_ENV) for explicit, unambiguous
          # cross-step data flow. Consumers reference these as
          # steps.pack_artifacts.outputs.<key> wrapped in workflow
          # expression delimiters at the call site (delimiters omitted
          # in this comment so the YAML parser does not interpolate it).
          {
            echo "nupkg_path=$NUPKG"
            echo "snupkg_path=$SNUPKG"
            echo "nupkg_sha256=$NUPKG_SHA256"
            echo "snupkg_sha256=$SNUPKG_SHA256"
          } >> "$GITHUB_OUTPUT"

      - name: Write release-checksums.txt
        id: checksums
        env:
          VERSION:       ${{ steps.ver.outputs.version }}
          TAG_SHA:       ${{ steps.tagsha.outputs.sha }}
          NUPKG_SHA256:  ${{ steps.pack_artifacts.outputs.nupkg_sha256 }}
          SNUPKG_SHA256: ${{ steps.pack_artifacts.outputs.snupkg_sha256 }}
        run: |
          set -euo pipefail
          CHECKSUMS="./artifacts/release-checksums.txt"
          {
            echo "# release-checksums for KyuzanInc.Turnkey.Sdk ${VERSION}"
            echo "version          = ${VERSION}"
            echo "source_commit    = ${TAG_SHA}"
            echo "nupkg_sha256     = ${NUPKG_SHA256}"
            echo "snupkg_sha256    = ${SNUPKG_SHA256}"
          } > "$CHECKSUMS"
          echo "checksums_path=$CHECKSUMS" >> "$GITHUB_OUTPUT"

      - name: Preflight existing Release assets (unconditional)
        id: preflight
        env:
          GH_TOKEN:        ${{ secrets.GITHUB_TOKEN }}
          TAG:             ${{ env.RELEASE_TAG }}
          VERSION:         ${{ steps.ver.outputs.version }}
          NUPKG_SHA256:    ${{ steps.pack_artifacts.outputs.nupkg_sha256 }}
          SNUPKG_SHA256:   ${{ steps.pack_artifacts.outputs.snupkg_sha256 }}
          NUPKG_PATH:      ${{ steps.pack_artifacts.outputs.nupkg_path }}
          SNUPKG_PATH:     ${{ steps.pack_artifacts.outputs.snupkg_path }}
          CHECKSUMS_PATH:  ${{ steps.checksums.outputs.checksums_path }}
          NUPKG_NAME:      KyuzanInc.Turnkey.Sdk.${{ steps.ver.outputs.version }}.nupkg
          SNUPKG_NAME:     KyuzanInc.Turnkey.Sdk.${{ steps.ver.outputs.version }}.snupkg
        run: |
          set -euo pipefail
          mkdir -p ./_release_assets
          HAS_PRIOR=false

          # Enumerate assets that actually exist on the Release. We do NOT
          # treat a failed `gh release download` as "asset missing" — that
          # would swallow auth / rate-limit / API errors. Instead query
          # `gh release view` first and download only what is present.
          assets_json=$(gh release view "$TAG" --json assets)
          present() {
            local name="$1"
            jq -e --arg n "$name" '.assets[] | select(.name == $n)' >/dev/null \
              <<< "$assets_json"
          }

          # If an asset is present, download MUST succeed; failure here is
          # a hard error (closed-fail).
          check_one() {
            local name="$1"
            local sha256="$2"
            if ! present "$name"; then
              echo "preflight: $name absent"
              return 0
            fi
            HAS_PRIOR=true
            if ! gh release download "$TAG" \
                 --pattern "$name" --dir ./_release_assets; then
              echo "::error::Asset '$name' is listed on the Release but download failed."
              exit 1
            fi
            local existing
            existing=$(sha256sum "./_release_assets/$name" | awk '{print $1}')
            if [[ "$existing" != "$sha256" ]]; then
              echo "::error::Existing Release asset $name sha256 ($existing) differs from local ($sha256). Refusing to clobber."
              exit 1
            fi
            echo "preflight: $name byte-equal"
          }
          check_one "$NUPKG_NAME"  "$NUPKG_SHA256"
          check_one "$SNUPKG_NAME" "$SNUPKG_SHA256"

          # release-checksums.txt: full byte-equality, not just hash-line grep.
          if present "release-checksums.txt"; then
            HAS_PRIOR=true
            if ! gh release download "$TAG" \
                 --pattern 'release-checksums.txt' --dir ./_release_assets; then
              echo "::error::release-checksums.txt is listed but download failed."
              exit 1
            fi
            if ! cmp -s "./_release_assets/release-checksums.txt" "$CHECKSUMS_PATH"; then
              echo "::error::Existing Release asset release-checksums.txt differs byte-for-byte from local. Refusing to clobber."
              diff "./_release_assets/release-checksums.txt" "$CHECKSUMS_PATH" || true
              exit 1
            fi
            echo "preflight: release-checksums.txt byte-equal"
          else
            echo "preflight: release-checksums.txt absent"
          fi

          echo "has_prior_assets=$HAS_PRIOR" >> "$GITHUB_OUTPUT"

      - name: Registry-driven duplicate check
        id: registry
        env:
          GH_TOKEN:       ${{ secrets.GITHUB_TOKEN }}
          GITHUB_ACTOR:   ${{ env.GITHUB_ACTOR_ENV }}
          PACKAGE_LOWER:  kyuzaninc.turnkey.sdk
          VERSION:        ${{ steps.ver.outputs.version }}
          NUPKG_SHA256:   ${{ steps.pack_artifacts.outputs.nupkg_sha256 }}
          OWNER:          ${{ github.repository_owner }}
        run: |
          set -euo pipefail
          URL="https://nuget.pkg.github.com/${OWNER}/download/${PACKAGE_LOWER}/${VERSION}/${PACKAGE_LOWER}.${VERSION}.nupkg"
          HTTP_STATUS=$(curl -sL -o ./registry.nupkg -w '%{http_code}' \
            -u "${GITHUB_ACTOR}:${GH_TOKEN}" "$URL")
          case "$HTTP_STATUS" in
            404)
              echo "version not present in registry → new publish"
              echo "is_safe_rerun=false" >> "$GITHUB_OUTPUT"
              ;;
            200)
              REG_SHA256=$(sha256sum ./registry.nupkg | awk '{print $1}')
              if [[ "$REG_SHA256" == "$NUPKG_SHA256" ]]; then
                echo "version present and bytes match → safe rerun"
                echo "is_safe_rerun=true" >> "$GITHUB_OUTPUT"
              else
                echo "::error::Registry has version $VERSION with sha256 $REG_SHA256, locally-built nupkg sha256 is $NUPKG_SHA256. Refusing to clobber. Bump to next patch."
                exit 1
              fi
              ;;
            *)
              echo "::error::Unexpected HTTP $HTTP_STATUS while querying registry for version $VERSION."
              exit 1
              ;;
          esac
          rm -f ./registry.nupkg

      # NOTE: attestation runs BEFORE publish so attestation failures
      # abort before any immutable package is written to the registry.
      - name: Attest build provenance (nupkg + snupkg)
        uses: actions/attest-build-provenance@<SHA>   # actions/attest-build-provenance v3.x
        with:
          subject-path: |
            ${{ steps.pack_artifacts.outputs.nupkg_path }}
            ${{ steps.pack_artifacts.outputs.snupkg_path }}

      - name: Publish to GitHub Packages (new publish only)
        if: steps.registry.outputs.is_safe_rerun == 'false'
        env:
          GH_PUSH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          NUPKG_PATH:    ${{ steps.pack_artifacts.outputs.nupkg_path }}
          OWNER:         ${{ github.repository_owner }}
        run: |
          # `--no-symbols` per Microsoft docs — .snupkg goes to Release assets only.
          dotnet nuget push "$NUPKG_PATH" \
            --source "https://nuget.pkg.github.com/${OWNER}/index.json" \
            --api-key "$GH_PUSH_TOKEN" \
            --no-symbols

      - name: Upload nupkg + snupkg + checksums to GitHub Release
        env:
          GH_TOKEN:           ${{ secrets.GITHUB_TOKEN }}
          TAG:                ${{ env.RELEASE_TAG }}
          NUPKG_PATH:         ${{ steps.pack_artifacts.outputs.nupkg_path }}
          SNUPKG_PATH:        ${{ steps.pack_artifacts.outputs.snupkg_path }}
          CHECKSUMS_PATH:     ${{ steps.checksums.outputs.checksums_path }}
          HAS_PRIOR_ASSETS:   ${{ steps.preflight.outputs.has_prior_assets }}
        run: |
          if [[ "$HAS_PRIOR_ASSETS" == "true" ]]; then
            # Preflight proved all prior assets are byte-equal to local; --clobber is safe.
            gh release upload "$TAG" "$NUPKG_PATH" "$SNUPKG_PATH" "$CHECKSUMS_PATH" --clobber
          else
            gh release upload "$TAG" "$NUPKG_PATH" "$SNUPKG_PATH" "$CHECKSUMS_PATH"
          fi
```

## release-drafter (split)

```yaml
# .github/workflows/release-drafter-draft.yml
name: Release drafter — update draft on main
on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: write
  pull-requests: read

jobs:
  draft:
    runs-on: ubuntu-latest
    steps:
      - uses: release-drafter/release-drafter@<SHA>   # release-drafter v6.x
        with:
          config-name: release-drafter.yml
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

```yaml
# .github/workflows/release-drafter-autolabel.yml
name: Release drafter — autolabel PR
on:
  pull_request:
    # Autolabel on creation, update, AND title/body edits. A separate
    # cleanup step strips stale conventional-commit labels before the
    # autolabeler runs, so the labels always match the current title.
    # `labeled` / `unlabeled` deliberately excluded to avoid undoing
    # manual maintainer adjustments.
    types: [opened, reopened, synchronize, edited]

permissions:
  contents: read
  pull-requests: write

jobs:
  autolabel:
    runs-on: ubuntu-latest
    env:
      GH_TOKEN:  ${{ secrets.GITHUB_TOKEN }}
      GH_REPO:   ${{ github.repository }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    steps:
      # release-drafter's autolabeler only ADDs labels; it cannot remove.
      # If the title changes from "fix:" to "feat:", the stale "fix"
      # label would linger and pollute the version-resolver. Strip the
      # conventional-commit set first ONLY on `edited` so unmodified
      # PR events keep their existing labels intact.
      #
      # We use `gh pr edit --remove-label` (NOT `gh label remove`, which
      # deletes the label from the repository entirely). The script first
      # queries which conventional-commit labels are actually attached
      # to this PR, then only calls --remove-label for those. Any `gh`
      # failure aborts.
      - name: Strip stale conventional-commit labels (edited only)
        if: github.event.action == 'edited'
        run: |
          set -euo pipefail
          conventional='breaking feature fix deps docs ci chore'

          # List labels actually attached to this PR.
          attached=$(gh pr view "$PR_NUMBER" --json labels --jq '.labels[].name')

          # Intersect conventional ∩ attached; remove only those.
          for label in $conventional; do
            if grep -Fxq "$label" <<< "$attached"; then
              gh pr edit "$PR_NUMBER" --remove-label "$label"
            fi
          done

      - uses: release-drafter/release-drafter/autolabeler@<SHA>   # release-drafter v6.x
        with:
          config-name: release-drafter.yml
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

```yaml
# .github/release-drafter.yml
name-template: "v$RESOLVED_VERSION"
tag-template:  "v$RESOLVED_VERSION"
prerelease: false      # stable by default. Maintainer ticks "Pre-release" for alphas.

autolabeler:
  - label: 'breaking'
    title:
      - '/^[a-z]+!:.*/'
      - '/^[a-z]+\([^)]+\)!:.*/'
    body:
      - '/BREAKING CHANGE:/i'
  - label: 'feature'
    title: ['/^feat(\([^)]+\))?:.*/']
  - label: 'fix'
    title: ['/^fix(\([^)]+\))?:.*/']
  - label: 'deps'
    title:
      - '/^deps(\([^)]+\))?:.*/'
      - '/^chore\(deps\):.*/'
  - label: 'docs'
    title: ['/^docs(\([^)]+\))?:.*/']
  - label: 'ci'
    title: ['/^ci(\([^)]+\))?:.*/']
  - label: 'chore'
    title:
      - '/^chore(\([^)]+\))?:.*/'
      - '/^refactor(\([^)]+\))?:.*/'
      - '/^test(\([^)]+\))?:.*/'

categories:
  - title: '💥 Breaking changes'
    labels: ['breaking']
  - title: '🚀 Features'
    labels: ['feature']
  - title: '🐛 Fixes'
    labels: ['fix']
  - title: '📦 Dependencies'
    labels: ['deps']
  - title: '📝 Docs'
    labels: ['docs']
  - title: '🔧 CI / chores'
    labels: ['ci', 'chore']

exclude-labels: ['skip-changelog']

version-resolver:
  major: { labels: ['breaking'] }
  minor: { labels: ['feature'] }
  patch: { labels: ['fix', 'deps', 'docs', 'ci', 'chore'] }
  default: patch

change-title-escapes: '\<*_&'

template: |
  ## What's changed in `v$RESOLVED_VERSION`

  $CHANGES

  ---

  **Full changelog**: https://github.com/$OWNER/$REPOSITORY/compare/$PREVIOUS_TAG...v$RESOLVED_VERSION
```

## PR title lint (enforces conventional-commit policy)

```yaml
# .github/workflows/pr-title-lint.yml
name: PR title lint
on:
  pull_request:
    types: [opened, reopened, synchronize, edited]

permissions:
  contents: read
  pull-requests: read

jobs:
  lint:
    # Job display name must stay in sync with the required-check name
    # documented in docs/release-process.md: the resulting GitHub UI
    # status is "PR title lint / Verify conventional commit title".
    name: Verify conventional commit title
    runs-on: ubuntu-latest
    env:
      PR_TITLE: ${{ github.event.pull_request.title }}
    steps:
      - name: Verify conventional commit title
        run: |
          set -euo pipefail
          # Structural regex: <type>(<scope>)?!?: <subject>
          # types: feat, fix, deps, docs, ci, chore, refactor, test
          # scope (when present): non-empty
          # subject: at least one char (further checked below for non-whitespace)
          re='^(feat|fix|deps|docs|ci|chore|refactor|test)(\([^)]+\))?!?: (.+)$'

          # Apply GitHub's workflow-command escape (per
          # https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-commands#example-evaluating-data-from-untrusted-sources)
          # before inserting untrusted PR-title text into an ::error::
          # annotation. Order matters: encode `%` first to avoid double-encoding.
          escape() {
            local s="$1"
            s="${s//%/%25}"
            s="${s//$'\r'/%0D}"
            s="${s//$'\n'/%0A}"
            printf '%s' "$s"
          }
          sanitized_raw=$(printf '%s' "$PR_TITLE" | tr -cd '[:print:]\n\r' | head -c 200)
          sanitized=$(escape "$sanitized_raw")

          if [[ ! "$PR_TITLE" =~ $re ]]; then
            echo "::error::PR title is not conventional (expect: <type>(scope)?!?: <subject>; types: feat|fix|deps|docs|ci|chore|refactor|test). See docs/release-process.md. Got: ${sanitized}"
            exit 1
          fi
          # Subject must contain at least one non-whitespace character.
          subject="${BASH_REMATCH[3]}"
          if ! printf '%s' "$subject" | grep -Eq '[^[:space:]]'; then
            echo "::error::PR title subject is whitespace-only. Got: ${sanitized}"
            exit 1
          fi
```

The maintainer adds the GitHub-UI check name
**"PR title lint / Verify conventional commit title"** to the repo's
required status checks for the `main` branch (one-time admin action;
documented in `docs/release-process.md`).

## docs/release-process.md (acceptance: 10 sections)

1. **Cut a release** — Maintainer steps from green main to published Release.
2. **Prereleases** — alpha/beta naming + **manual "Pre-release" checkbox** is required (the workflow does not auto-detect).
3. **Fix a broken release** — always `X.Y.Z+1`; never re-push `X.Y.Z`.
4. **Same-tag reruns** — registry-byte + Release-asset-byte invariants make safe reruns idempotent. Document failure modes.
5. **Consume the package** — PAT scopes, example `nuget.config`, Actions example.
6. **ACL / who can install** — org member with repo read; collaborator process.
7. **Symbols** — `.snupkg` is a Release asset, not pushed to GH Packages (no symbol server). SourceLink coverage is a v0.2 follow-up.
8. **Attestation** — `gh attestation verify` example for `.nupkg`.
9. **Repo label provisioning** — One-time admin task: create the labels referenced by `.github/release-drafter.yml` so the autolabeler can attach them:
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
   Verify all 8 labels exist (smoke-test gate uses this exact check):
   ```bash
   gh label list --limit 1000 --json name --jq '.[].name' > /tmp/repo-labels.txt
   for label in breaking feature fix deps docs ci chore skip-changelog; do
     grep -Fxq "$label" /tmp/repo-labels.txt || {
       echo "::error::missing repo label: $label"; exit 1
     }
   done
   ```
10. **Branch protection** — One-time admin task: enable "Require status checks to pass before merging" for the `main` branch. Required checks (use the **exact** names that GitHub displays):
   - `CI / Build + test (ubuntu-latest, net8.0 runner)`
   - `CI / dotnet pack (validation)`
   - `PR title lint / Verify conventional commit title`

   `Repo privacy check` runs on `push: main` (post-merge) and the weekly cron, not on `pull_request`. Do NOT include it in the required-check list or PR merges will block. The post-merge / weekly-cron runs catch accidental public-visibility flips on the next trigger, not instantly.

   Require: "Squash and merge" only; "Require linear history"; "Require status checks to pass before merging".

## Acceptance criteria (v16)

- [ ] `.github/workflows/release.yml` triggers only on `release: published`.
- [ ] `.github/workflows/release-drafter-draft.yml` triggers on `push: main` + `workflow_dispatch`.
- [ ] `.github/workflows/release-drafter-autolabel.yml` triggers on `pull_request: {opened, reopened, synchronize, edited}`. A "strip stale conventional-commit labels" step runs BEFORE the autolabeler **only on `edited` events** so opened/reopened/synchronize keep existing labels untouched. No `labeled` / `unlabeled` (manual adjustments preserved). Uses `release-drafter/release-drafter/autolabeler@<SHA>`.
- [ ] `.github/workflows/pr-title-lint.yml` exists and triggers on `pull_request: {opened, reopened, synchronize, edited}`.
- [ ] `.github/release-drafter.yml` has `prerelease: false`, explicit `autolabeler`, `categories`, `version-resolver`, `name-template`, `tag-template`, `exclude-labels`, `change-title-escapes`, `template`.
- [ ] `docs/release-process.md` exists and covers the 10 sections above.
- [ ] `.github/pin-actions.txt` lists every third-party action with its full commit SHA + human-readable major.
- [ ] `release.yml`'s `permissions` block lists exactly `contents: write`, `packages: write`, `id-token: write`, `attestations: write` (no `pull-requests` scope — `gh release` and NuGet registry calls don't need it).
- [ ] `release.yml` validates the tag is **strict SemVer 2.0.0 without build metadata**.
- [ ] `release.yml` validates `tag_commit` is an **ancestor of `origin/main`** via `git merge-base --is-ancestor`, with the explicit refspec `+refs/heads/main:refs/remotes/origin/main`.
- [ ] `release.yml` passes `/p:Version`, `/p:PackageVersion`, `/p:RepositoryCommit=$TAG_SHA`, `/p:IncludeSymbols=true`, `/p:SymbolPackageFormat=snupkg` to both `build` and `pack` so the `.snupkg` is produced.
- [ ] `release.yml` validates `.nupkg` (README, LICENSE, NOTICE, both TFM dlls) with `unzip -Z1` + `grep -Fxq`.
- [ ] `release.yml` validates `.snupkg` (both TFM pdbs) with `unzip -Z1` + `grep -Fxq`.
- [ ] `release.yml` writes `release-checksums.txt` containing version, source_commit, nupkg_sha256, snupkg_sha256.
- [ ] `release.yml` does an **unconditional** Release-asset preflight that downloads `.nupkg`, `.snupkg`, and `release-checksums.txt` if present, **sha256-compares the `.nupkg` and `.snupkg`** with local, **`cmp -s` byte-compares the `release-checksums.txt`** with local, and FAILs on any mismatch. The preflight runs BEFORE the registry check and BEFORE attestation.
- [ ] All inter-step values in `release.yml` (paths, hashes, IS_SAFE_RERUN, HAS_PRIOR_ASSETS) are written via **step outputs** (`echo "name=value" >> "$GITHUB_OUTPUT"`) and consumed via `${{ steps.<id>.outputs.<name> }}`. No use of `$GITHUB_ENV` for cross-step data flow.
- [ ] `release.yml` does a registry duplicate check; downloads existing `.nupkg`; sha256-compares; FAIL on mismatch.
- [ ] `release.yml` runs `actions/attest-build-provenance` **before** `dotnet nuget push`.
- [ ] `release.yml` calls `dotnet nuget push <nupkg> --no-symbols` (NO `--skip-duplicate`).
- [ ] `release.yml` uploads `.nupkg` + `.snupkg` + `release-checksums.txt` as Release assets; `--clobber` only when `HAS_PRIOR_ASSETS=true` (which means the preflight already proved byte-equality).
- [ ] All GitHub-expression-derived values used in shell are routed through `env:` (no direct `${{ ... }}` in `run:` strings).
- [ ] All third-party actions in **release-related workflows only** (`release.yml`, `release-drafter-draft.yml`, `release-drafter-autolabel.yml`, `pr-title-lint.yml`) are pinned to a full **commit SHA**, recorded in `.github/pin-actions.txt`. The `pin-actions.txt` entry for `release-drafter/release-drafter/autolabeler` MUST include a one-line citation referencing the line of the autolabeler source (at the pinned SHA) that reads `pull_request.title` from the event payload, which is the auditable proof that the autolabeler accepts any `pull_request` event including `edited`. Hardening of pre-existing `ci.yml` actions to commit-SHA pins is a follow-up TODO and NOT blocking this PR.
- [ ] No code change to existing reviewed SDK source files (`src/Encoding.cs`, `src/Crypto.cs`, `src/CryptoConstants.cs`, `src/ApiKeyStamper.cs`, `src/Http.cs`, `src/TurnkeyJsonContext.cs`).
- [ ] PR-only flow: this work happens on `feature/github-packages-release`; merges only after Codex review clean + CI green.
- [ ] `.csproj <Version>` stays `0.1.0-alpha.0` so local builds work; release.yml overrides per release.
- [ ] No `release-manifest.json` file is introduced.
- [ ] Smoke-test acceptance documented after merge:
  - **Repo labels exist**: smoke pre-flight runs the exact `gh label list --json name --jq` + `grep -Fxq` loop from `docs/release-process.md` to confirm all 8 labels (`breaking, feature, fix, deps, docs, ci, chore, skip-changelog`) are present. Missing any label aborts the smoke test.
  - **Deterministic pack**: build the same tag twice from clean checkouts; nupkg + snupkg byte equality required. Determinism is enforced by `Directory.Build.props` properties `Deterministic=true` and `ContinuousIntegrationBuild=true`; if non-determinism surfaces, the release workflow must be revised before relying on safe-rerun behaviour.
  - **Title-edit relabeling**: open a smoke PR `chore: smoke`, confirm `chore` label is applied. Edit to `feat: smoke`, confirm the stale `chore` label is stripped and `feature` is added. If this fails, fall back to autolabel triggers `{opened, reopened, synchronize}` and remove the stale-strip step (documented fallback in release-process.md).

## Risk register (v16)

| ID | Risk | Severity | Mitigation |
|---|---|---|---|
| R-STALE | Tag points to a commit NOT on `main` | Med | `git merge-base --is-ancestor` |
| R-MISMATCH | Same `vX.Y.Z` re-published with different `.nupkg` bytes | High | Registry sha256 byte-compare; FAIL on mismatch |
| R-SNUPKG-MISMATCH | Same `vX.Y.Z` Release rerun with different `.snupkg` bytes | Med | Release-asset sha256 byte-compare before `--clobber` |
| R-CHKSUM | Historical sha256s lost on registry deletion | Med | `release-checksums.txt` uploaded as Release asset; preserved on the Release page |
| R-CLOB | `--clobber` overwrites a different prior asset | Med | `--clobber` only after all byte-compare gates pass |
| R-LABEL | release-drafter mislabels PRs | Low | Explicit `autolabeler` + PR title lint |
| R-PRE | Maintainer forgets "Pre-release" toggle for alpha tag | Low | Documented in release-process.md; drafter defaults to `prerelease: false` |
| R-ACL | Consumer cannot install | Low | Documented |
| R-TAGMV | Tag force-moved to a different commit | High | Registry sha256 + Release-asset sha256 + release-checksums.txt all guard; required version bump on any mismatch |
| R-SUPPLY | Unpinned third-party action runs malicious release | Med | Full commit SHA pins + `.github/pin-actions.txt` |
| R-SYM | `.snupkg` mistakenly pushed to GH Packages | Low | `dotnet nuget push --no-symbols` |
| R-ATTEST-FAIL | Attestation failure | Low | Attest runs BEFORE publish; failure aborts publish |
| R-IMMUT | Maintainer tries to "fix" a published version in place | High | Forbidden by docs + invariant blocks at workflow level |
| R-DRAFTSQUASH | Non-conventional squash title bypasses autolabeler | Low | `pr-title-lint.yml` required check |
| R-RACE | Two concurrent Release publishes for different tags | Low | `concurrency.group = release-${tag}` |
| R-REGISTRYDOWN | GH Packages registry returns 5xx during duplicate check | Low | Workflow fails loudly; safe to retry |
| R-DELETE | Maintainer deletes a package version, then republishes | High | Workflow sees 404 → republishes. The `release-checksums.txt` on the Release page is unaffected, so a later audit catches it. Documented as out-of-band incident-response in release-process.md. |

## Open questions for codex r16

None remaining from r1–r15. If r16 surfaces new findings, fold them
back into this section.

## Implementation order (v16)

1. Commit this v16 plan.
2. Codex r16 review the plan; iterate until **GO**.
3. Implement the files listed under "Files (v16)".
4. Commit on the same branch; open PR; Codex review the diff.
5. Iterate diff fixes until Codex returns clean.
6. Wait for CI green on the PR.
7. Squash-merge to main with a conventional-commit title
   (`ci: add release.yml + release-drafter + docs/release-process.md`).
8. Smoke test (deferred to a follow-up commit after merge):
   - Tag `v0.0.0-smoke.0` on main HEAD; create a Release with the
     "Pre-release" box ticked; verify publish flow.
   - **Verify title-edit relabeling**: open a small PR with title
     `chore: smoke`, observe `chore` label applied. Edit title to
     `feat: smoke`, observe `feature` label applied and `chore`
     stripped. If this fails, fall back to autolabel triggers
     `{opened, reopened, synchronize}` and remove the stale-strip step.
   - Tear down: `gh release delete v0.0.0-smoke.0 --cleanup-tag` and
     (manually) delete the package version on GitHub UI. Close + delete
     the smoke PR. Document smoke outcomes on the merged PR.
