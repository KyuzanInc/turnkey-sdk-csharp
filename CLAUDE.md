# CLAUDE.md

Project-specific instructions for Claude Code agents operating in this repo.

## Project type

.NET NuGet library (`KyuzanInc.Turnkey.Sdk`). Targets `netstandard2.1` and
`net8.0`. **Not a web app.** No production URL. The "deploy artifact" is a
`.nupkg` published to GitHub Packages.

The crypto logic is a logical compatibility port of explicitly pinned Turnkey
TypeScript packages. See [`README.md`](./README.md) and
[`docs/compatibility/upstream-pins.md`](./docs/compatibility/upstream-pins.md)
for the exact pins and verification policy.

## Deploy Configuration (configured by /setup-deploy)

- **Platform**: GitHub Releases → GitHub Packages (NuGet)
- **Package**: `KyuzanInc.Turnkey.Sdk`
- **Production URL**: none (NuGet feed, not a website). Feed index is at
  `https://nuget.pkg.github.com/KyuzanInc/index.json` and requires
  `read:packages` scope to query.
- **Deploy workflow**: `.github/workflows/release.yml`
  - Trigger: `release: published` (manual `gh release create vX.Y.Z` is
    required; merging a PR to `main` alone does NOT trigger a deploy).
  - Steps: setup-dotnet 8.0.x → restore (locked-mode) → build (Release, both
    TFMs) → test → pack → validate `.nupkg` + `.snupkg` contents → preflight
    existing Release assets → registry-driven duplicate check → publish to
    `nuget.pkg.github.com` → write `release-checksums.txt`.
- **Post-deploy verification**: `.github/workflows/consumer-smoke.yml`
  - Trigger: `workflow_run` of `Release` (i.e. `release.yml`) completion.
  - Verifies the published package can be installed by a downstream consumer
    project via `dotnet add package`.
- **Staging**: none.
- **Auto-deploy on merge**: **NO.** Merging a PR to `main` only triggers
  `ci.yml` (build + test + coverage gate + coverage-map gate). A release
  publish is an explicit, separate step.
- **Merge method**: SQUASH (repo default; `viewerDefaultMergeMethod=SQUASH`).
- **Branch protection on main**: relies on `ci.yml` and PR-title-lint passes.

### Health check (used by /land-and-deploy after a PR merge)

A PR merge to `main` is "verified deployed" when, on the merge commit:

```bash
gh run list --branch main --limit 5 \
  --json workflowName,headSha,status,conclusion \
  --jq '.[] | select(.headSha == "<MERGE_SHA>") | .workflowName + " / " + .conclusion'
```

returns `CI / success` for the `Build + test (ubuntu-latest, net8.0 runner)`
workflow. There is no remote URL to canary.

For a `release.yml`-driven NuGet publish, the verification is:

```bash
# Requires a token with read:packages.
GH_TOKEN=<token-with-read-packages>
PACKAGE_LOWER="kyuzaninc.turnkey.sdk"
VERSION=<just-published>
curl -sf -H "Authorization: Bearer $GH_TOKEN" \
  "https://nuget.pkg.github.com/KyuzanInc/download/${PACKAGE_LOWER}/${VERSION}/${PACKAGE_LOWER}.${VERSION}.nupkg" \
  -o /dev/null -w "HTTP %{http_code}\n"
```

`HTTP 200` means the publish landed.

### Custom deploy hooks

- **Pre-merge**: none. CI gates handle pre-merge.
- **Deploy trigger**: manual `gh release create vX.Y.Z` after the relevant
  PR is merged to `main`. `release.yml` validates the tag points at a commit
  contained in `main`, so the release MUST come after the PR merge.
- **Deploy status**: `gh run watch <release.yml run id>`.
- **Health check**: see "Health check" block above.

## CI / verification standards

- All PRs must pass:
  - `Build + test (ubuntu-latest, net8.0 runner)` — `dotnet test` 0 failures,
    coverage ≥ 30% combined-TFM (alpha threshold).
  - `Coverage map gate (upstream test ↔ C# test)` — `tools/compatibility/coverage-map.sh --check`
    returns 0 MISSING and 0 empty N/A reasons.
  - `Verify conventional commit title` — PR title matches
    `^(feat|fix|deps|docs|ci|chore|refactor|test)(\([^)]+\))?!?: (.+)$`.
  - `autolabel` (informational).
  - `dotnet pack (validation)` — `.nupkg` contains README, LICENSE, NOTICE,
    and the Apache-2.0 license covering retained upstream-derived material.

- Changes to compatibility-sensitive code under `src/` must preserve the
  relevant pinned-source mapping, fixtures, and tests. Cryptography or wire
  format changes require independent maintainer review; review transcripts are
  not committed to the repository.

- All PRs that change pinned upstream versions must update
  [`docs/compatibility/upstream-pins.md`](./docs/compatibility/upstream-pins.md),
  regenerate `tests/UpstreamSources/source-file-checksums.txt`, and re-run the
  upstream-drift and compatibility verification described in
  [`docs/compatibility/verification.md`](./docs/compatibility/verification.md).

## Skill routing

When the user's request matches an available skill, invoke it via the Skill
tool. When in doubt, invoke the skill.

Key routing rules:

- Product ideas / brainstorming → invoke `/office-hours`
- Strategy / scope → invoke `/plan-ceo-review`
- Architecture → invoke `/plan-eng-review`
- Full review pipeline → invoke `/autoplan`
- Bugs / errors → invoke `/investigate`
- QA / testing site behavior → invoke `/qa` or `/qa-only`
- Code review / diff check → invoke `/review`
- Ship / PR → invoke `/ship`
- Land / merge / deploy → invoke `/land-and-deploy`
- Save progress → invoke `/context-save`
- Resume context → invoke `/context-restore`
- Compatibility review → follow
  [`docs/compatibility/verification.md`](./docs/compatibility/verification.md)
  and record durable architectural choices as ADRs under `docs/adr/`.
