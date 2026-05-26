# Plan: Auto-trigger `consumer-smoke.yml` on successful `Release`

**Status**: Draft v1 — awaiting Codex plan review.

## 1. Intent

Make `.github/workflows/consumer-smoke.yml` run automatically after every
**successful** completion of `.github/workflows/release.yml`, in addition
to the existing `workflow_dispatch` (manual) trigger.

Why: today, a publish that succeeds at registry write but breaks
consumer-side install (missing dep, TFM resolution failure, packageId
typo, BouncyCastle pin conflict, etc.) is only caught the moment a real
consumer tries `dotnet add package`. With this change, every Release
gets verified end-to-end shortly after publish, with zero manual effort.
(GitHub Actions does not provide a wall-clock SLA on when a
`workflow_run`-triggered downstream run starts; runner queue depth and
GitHub-internal scheduling apply.)

## 2. Scope

In:

- One change to `.github/workflows/consumer-smoke.yml`:
  - add `workflow_run` trigger
  - add a job-level conditional gate
  - add an "Resolve inputs" step that derives `package_version` + `tfm`
    from either `workflow_dispatch` inputs OR `workflow_run` event context
  - downstream steps reference the step's outputs

Out:

- TFM matrix expansion (`net8.0` + `netstandard2.1`) — deferred, separate
  follow-up if needed.
- OS matrix (ubuntu / windows / macOS) — not needed for a pure managed
  library, deferred.
- Weekly cron — deferred.
- Auto-revert on consumer-smoke failure — explicitly NOT in scope. NuGet
  packages are immutable; the failure is informational + early-warning.

## 3. Design

### 3.1 Trigger surface

`consumer-smoke.yml` `on:` block becomes:

```yaml
on:
  workflow_dispatch:
    inputs:
      package_version:
        description: "Version to install (e.g. 0.1.0-alpha.0)"
        required: true
        default: "0.1.0-alpha.0"
      tfm:
        description: "Target framework moniker"
        required: true
        default: "net8.0"
  workflow_run:
    workflows: ["Release"]
    types: [completed]
```

Notes:

- `workflows: ["Release"]` matches by the upstream workflow's `name:`
  field (release.yml line 1: `name: Release`). Filename match is NOT
  what `workflow_run` keys on.
- `types: [completed]` fires on every conclusion (success, failure,
  cancelled, …). The job-level conditional filters to success only.
- The two triggers co-exist; no breaking change to manual dispatch.

### 3.2 Job conditional

```yaml
jobs:
  consumer-install:
    if: |
      github.event_name == 'workflow_dispatch' ||
      (github.event_name == 'workflow_run' && github.event.workflow_run.conclusion == 'success')
```

Rationale: skip auto-runs on failed/cancelled Release (those produce no
package to verify) but always run on explicit user dispatch.

### 3.3 Input derivation step

`workflow_run` events do not populate the `inputs` context: `inputs.*`
is only set for `workflow_dispatch` and `workflow_call` events
(GitHub Actions contexts reference). When evaluated in a step `env:`
block on a `workflow_run`-triggered run, `${{ inputs.package_version }}`
stringifies to the empty string — it does NOT cause a YAML/expression
parse error. The Resolve step below branches on `EVENT_NAME` first, so
the empty `DISPATCH_VER` is never read on the workflow_run path.

Add the FIRST step in the job that resolves `package_version` + `tfm`
from either source and writes them to `$GITHUB_OUTPUT`:

```yaml
- name: Resolve install inputs
  id: resolve
  env:
    EVENT_NAME:    ${{ github.event_name }}
    DISPATCH_VER:  ${{ inputs.package_version }}
    DISPATCH_TFM:  ${{ inputs.tfm }}
    WR_BRANCH:     ${{ github.event.workflow_run.head_branch }}
    WR_EVENT:      ${{ github.event.workflow_run.event }}
  run: |
    set -euo pipefail
    if [[ "$EVENT_NAME" == "workflow_dispatch" ]]; then
      VER="$DISPATCH_VER"
      TFM="$DISPATCH_TFM"
    else
      # workflow_run from `release.yml`. The upstream Release runs on
      # `release: published`, so `workflow_run.event` MUST be 'release'
      # and `workflow_run.head_branch` is the tag ref short name
      # (e.g. 'v0.1.0-alpha.0'). Refuse anything else.
      if [[ "$WR_EVENT" != "release" ]]; then
        echo "::error::Unexpected upstream workflow_run.event '$WR_EVENT' (expected 'release'). Refusing to run."
        exit 1
      fi
      # Strict SemVer 2.0.0 with leading 'v' — same regex as release.yml.
      re='^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-((0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)(\.(0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*))*))?$'
      if [[ ! "$WR_BRANCH" =~ $re ]]; then
        echo "::error::workflow_run.head_branch '$WR_BRANCH' is not a strict SemVer 2.0.0 tag; refusing to install."
        exit 1
      fi
      VER="${WR_BRANCH#v}"
      TFM="net8.0"  # default for auto-runs; can be matrixed in a later plan
    fi
    echo "package_version=$VER" >> "$GITHUB_OUTPUT"
    echo "tfm=$TFM"             >> "$GITHUB_OUTPUT"
    echo "Resolved: package_version=$VER, tfm=$TFM (source=$EVENT_NAME)"
```

Subsequent steps reference `${{ steps.resolve.outputs.package_version }}`
and `${{ steps.resolve.outputs.tfm }}` instead of `${{ inputs.* }}`.

### 3.4 Job name and workflow run-name (display surface)

The existing job declaration is:

```yaml
jobs:
  consumer-install:
    name: Install + use ${{ inputs.package_version }}
```

The `name:` field is rendered BEFORE the job starts (before the Resolve
step has produced its outputs), so it cannot reference
`steps.resolve.outputs.*`. On a `workflow_run`-triggered run,
`inputs.package_version` stringifies to empty, so the Actions UI would
show a card titled `Install + use ` with a dangling space. Two changes:

1. **Job name fallback** — chain `inputs.package_version` with
   `github.event.workflow_run.head_branch` using the GitHub Actions
   `||` operator. On `workflow_dispatch`, the first operand is the
   user-supplied version; on `workflow_run`, the first operand is
   empty (falsy) and the second operand resolves to the upstream tag
   short name (`v0.1.0-alpha.0` etc.):

   ```yaml
   jobs:
     consumer-install:
       name: "Install + use ${{ inputs.package_version || github.event.workflow_run.head_branch }}"
   ```

2. **Workflow `run-name`** — add a top-level
   [`run-name`](https://docs.github.com/en/actions/learn-github-actions/workflow-syntax-for-github-actions#run-name)
   so the run row in the Actions sidebar also reads usefully on auto
   runs:

   ```yaml
   run-name: "Consumer smoke (${{ inputs.package_version || github.event.workflow_run.head_branch || 'unknown' }})"
   ```

   The trailing `|| 'unknown'` guards the extremely unlikely case where
   neither source produces a string (e.g. a malformed event payload).

The job `name:` is the source of the green ✓ / red ✗ row label inside
the run page; `run-name:` is the title of the run itself. Both should
read usefully on both trigger paths.

### 3.5 Permissions, secrets, runner

Unchanged from the current workflow:

- `permissions: { contents: read, packages: read }` at workflow level
- `secrets.GITHUB_TOKEN` available (`packages: read` covers GH Packages
  download for same-org private packages)
- `runs-on: ubuntu-latest`

`workflow_run` jobs receive the same default token shape as
`workflow_dispatch` jobs; the declared `permissions:` block is honoured
identically.

### 3.6 Default-branch invariant (workflow_run security model)

`workflow_run`-triggered runs ALWAYS execute the workflow file that
exists on the **default branch** (`main`), not the head branch's
version. This is by GitHub design: it prevents a malicious PR from
crafting a workflow file that runs with elevated permissions on PR
events.

Implications for this change:

- On a feature branch (e.g. this branch `ci/consumer-smoke-auto-trigger`),
  the new `workflow_run` trigger is INERT until merged to main.
- After merge, the next Release publish auto-fires consumer-smoke.
- During development, verification of the new trigger path is by
  manual dispatch (already supported and unchanged).

## 4. Failure semantics

- consumer-smoke failure does NOT roll back the Release.
- consumer-smoke failure does NOT delete the published package
  (packages are immutable; the release.yml registry-byte invariant
  already enforces that).
- consumer-smoke failure surfaces in:
  - GitHub Actions UI (red ✗ on the run)
  - GitHub email / web notifications for collaborators following
    workflow runs
  - Future: GitHub branch protection on `main` can include consumer-smoke
    if/when a "release post-check" gate is desired (out of scope for v1).

A failed auto-run with `workflow_run.conclusion == 'success'`-gated
Release means the package was successfully written to the registry but
cannot be consumed. Action: investigate the Actions log, optionally cut
a follow-up patch release that fixes the consumer-side defect (per
`docs/release-process.md §3` — never re-push the same version).

## 5. Acceptance criteria

After this change is merged to `main`:

1. `.github/workflows/consumer-smoke.yml` has both `workflow_dispatch`
   AND `workflow_run` in its `on:` block, with no other triggers.
2. The job declares
   `if: github.event_name == 'workflow_dispatch' || (github.event_name == 'workflow_run' && github.event.workflow_run.conclusion == 'success')`.
3. A "Resolve install inputs" step is the first step in the job, with
   `id: resolve` and writing `package_version` + `tfm` to
   `$GITHUB_OUTPUT`.
4. All subsequent steps that previously used `${{ inputs.package_version }}`
   or `${{ inputs.tfm }}` now use
   `${{ steps.resolve.outputs.package_version }}` /
   `${{ steps.resolve.outputs.tfm }}`.
5. The SemVer regex used to validate `workflow_run.head_branch` matches
   the regex in `release.yml` step "Parse + validate strict SemVer 2.0.0
   tag" exactly. (Cross-file regex drift would be a bug.)
6. The job declaration sets
   `name: "Install + use ${{ inputs.package_version || github.event.workflow_run.head_branch }}"`
   so the job card title reads usefully on both `workflow_dispatch`
   and `workflow_run` paths.
7. The workflow file declares a top-level
   `run-name: "Consumer smoke (${{ inputs.package_version || github.event.workflow_run.head_branch || 'unknown' }})"`
   so the run title in the Actions sidebar also reads usefully on
   both trigger paths.
8. Manual dispatch still works against `main`: the existing inputs are
   preserved and the resolve step routes them through unchanged.
9. After merge, the next Release publish triggers consumer-smoke
   automatically, and the auto-run's "Resolve install inputs" step prints
   `Resolved: package_version=<X.Y.Z[-pre]>, tfm=net8.0 (source=workflow_run)`.

## 6. Codex implementation review focus

When reviewing the implementation PR, Codex should scrutinise:

- CRITICAL: any chance the `workflow_run` trigger combined with the
  `permissions:` block could leak `secrets.GITHUB_TOKEN` to an
  attacker-controlled path? (Answer expected: no — workflow_run uses
  the default-branch workflow only, and our existing token usage is
  unchanged.)
- WIRE: does `workflows: ["Release"]` correctly match release.yml's
  `name:`? (Cross-reference `.github/workflows/release.yml` line 1.)
- WIRE: does the SemVer regex match release.yml's regex byte-for-byte?
  (Cross-reference release.yml line ~49.)
- WIRE: does the `if:` condition use the GitHub Actions expression syntax
  correctly (especially the `||` / `&&` and the parenthesisation)?
  Reference: https://docs.github.com/en/actions/learn-github-actions/expressions
- WIRE: when `workflow_run` fires, the `inputs` context is not
  populated (per GitHub Actions docs, `inputs` is only available for
  `workflow_dispatch` and `workflow_call` events). A nonexistent
  property dereference of `inputs.package_version` evaluates to the
  empty string when interpolated into the step's `env:` block — it
  does NOT cause a YAML/expression parse error. Confirm both points
  against current docs.
- NIT: comment quality, naming, no `set -x` in the resolve step (token
  is not exposed in this step's env but defence in depth is cheap).

## 7. Out-of-scope follow-ups (post-merge candidates)

- TFM matrix: parametrise `tfm` as a matrix axis (`net8.0` +
  `netstandard2.1`-consumed-by-net8). Catches netstandard-only regressions.
- Cron schedule: weekly `schedule:` run that picks the latest published
  version via `gh release view --json tagName --jq .tagName` and re-runs
  install. Catches registry / auth-flow drift.
- Severity classification: differentiate between
  `dotnet add package` failure (registry / install) and `dotnet build`
  failure (API surface drift) in the conclusion message.

## 8. Risks

| ID | Risk | Severity | Mitigation |
|---|---|---|---|
| R-DEFAULT-BRANCH | `workflow_run` always uses main's workflow file; feature-branch changes don't exercise the new trigger | Low | Documented; verification via manual dispatch on feature branch + observation of next post-merge Release |
| R-SOURCE-FORK | If the upstream Release workflow were ever moved to a fork or different repo, the `workflow_run` reference by name silently breaks | Very low | Out of scope; not a realistic threat in this repo |
| R-REGEX-DRIFT | SemVer regex copy-paste from release.yml could drift independently | Low | Acceptance criterion #5 calls for byte-identical regex; a future shared-include refactor is a separate plan |
| R-INPUTS-NULL | `${{ inputs.package_version }}` on a `workflow_run`-triggered job renders to empty string, then bash compares to literal `""` | Low | Resolve step branches on `EVENT_NAME` first, so the empty `DISPATCH_VER` is never read in the workflow_run path |
| R-CONCURRENT | Two Releases published in quick succession could trigger overlapping consumer-smoke runs | Very low | No shared state mutated; runs are independent |

## 9. Execution plan

1. Codex plan review on this file → iterate until GO clean.
2. Apply the change to `consumer-smoke.yml` on branch
   `ci/consumer-smoke-auto-trigger`.
3. PR with `ci:` conventional-commit title. CI on PR (`Build + test`,
   `dotnet pack (validation)`, `PR title lint`, `autolabel`) must pass —
   no change to those workflows is expected.
4. Codex implementation review on the PR diff → iterate until GO clean.
5. Squash-merge to `main`.
6. Verify post-merge by manually dispatching consumer-smoke against
   `main` with `package_version=0.1.0-alpha.0` (we already know this
   version works — sanity check that the resolve step did not regress).
7. (Out of this PR's scope) the next real Release publish will
   auto-trigger consumer-smoke; observe the run + conclusion.
