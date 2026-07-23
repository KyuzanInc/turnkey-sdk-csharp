#!/usr/bin/env bash
set -euo pipefail

tools_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
verifier="$tools_dir/verify-action-pins.sh"
fixture=$(mktemp -d)
trap 'rm -rf "$fixture"' EXIT

mkdir -p "$fixture/.github/workflows" "$fixture/bin"
# A real commit in actions/checkout (verified via `gh api .../git/commits/<sha>`
# at the time this fixture was written), used as the one SHA the stub `gh`
# below reports as a real commit object.
sha=de0fac2e4500dabe0009e67214ff5f5447ce83dd

# Stub `gh` so this test never touches the network: it reports exactly
# GH_STUB_VALID_SHA as a real commit object (200) and everything else as
# not found (404) — simulating an annotated tag object or a bad ref.
cat > "$fixture/bin/gh" <<'STUB'
#!/usr/bin/env bash
if [[ "${1:-}" == "api" && "${2:-}" == repos/*/git/commits/* ]]; then
  found_sha="${2##*/git/commits/}"
  if [[ "$found_sha" == "${GH_STUB_VALID_SHA:-}" ]]; then
    echo "{\"sha\":\"$found_sha\"}"
    exit 0
  fi
  echo "gh: Not Found (HTTP 404)" >&2
  exit 1
fi
echo "gh stub: unsupported invocation: $*" >&2
exit 1
STUB
chmod +x "$fixture/bin/gh"

run_verifier() {
  PATH="$fixture/bin:$PATH" GH_STUB_VALID_SHA="$sha" \
    ACTION_PIN_REPO_ROOT="$fixture" "$verifier"
}

cat > "$fixture/.github/pin-actions.txt" <<MANIFEST
actions/checkout
  pinned_sha:     $sha
  pinned_tag:     v6.0.2
  used_in:        verify.yaml
MANIFEST

cat > "$fixture/.github/workflows/verify.yaml" <<WORKFLOW
name: Verify
jobs:
  verify:
    steps:
      - uses: actions/checkout@$sha
WORKFLOW

run_verifier >/dev/null

# An unpinned tag ref must be rejected.
cat > "$fixture/.github/workflows/verify.yaml" <<'WORKFLOW'
name: Verify
jobs:
  verify:
    steps:
      - uses: actions/checkout@v6
WORKFLOW

if run_verifier >/dev/null 2>&1; then
  echo "action pin verifier accepted an unpinned .yaml workflow" >&2
  exit 1
fi

# A `uses:` key with its value wrapped onto a following line (valid YAML)
# has no non-space, non-comment character after "uses:" on that line. The
# gate must reject this shape instead of silently skipping past it without
# ever checking a SHA.
cat > "$fixture/.github/workflows/verify.yaml" <<WORKFLOW
name: Verify
jobs:
  verify:
    steps:
      - uses:
          actions/checkout@$sha
WORKFLOW

if run_verifier >/dev/null 2>&1; then
  echo "action pin verifier accepted a line-wrapped 'uses:' value" >&2
  exit 1
fi

# A pinned SHA that does not resolve to a real commit object (e.g. an
# annotated tag object, like release-drafter/release-drafter's pin before
# it was corrected) must be rejected even though it is a well-formed
# 40-char hex string that matches the manifest.
cat > "$fixture/.github/workflows/verify.yaml" <<WORKFLOW
name: Verify
jobs:
  verify:
    steps:
      - uses: actions/checkout@$sha
WORKFLOW

if PATH="$fixture/bin:$PATH" GH_STUB_VALID_SHA="0000000000000000000000000000000000000000" \
    ACTION_PIN_REPO_ROOT="$fixture" "$verifier" >/dev/null 2>&1; then
  echo "action pin verifier accepted a SHA that is not a real commit object" >&2
  exit 1
fi

echo "action pin verifier regression tests passed"
