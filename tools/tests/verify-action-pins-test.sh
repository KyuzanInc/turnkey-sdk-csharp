#!/usr/bin/env bash
set -euo pipefail

tools_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
verifier="$tools_dir/verify-action-pins.sh"
fixture=$(mktemp -d)
trap 'rm -rf "$fixture"' EXIT

mkdir -p "$fixture/.github/workflows"
sha=de0fac2e4500dabe0009e67214ff5f5447ce83dd

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

ACTION_PIN_REPO_ROOT="$fixture" "$verifier" >/dev/null

cat > "$fixture/.github/workflows/verify.yaml" <<'WORKFLOW'
name: Verify
jobs:
  verify:
    steps:
      - uses: actions/checkout@v6
WORKFLOW

if ACTION_PIN_REPO_ROOT="$fixture" "$verifier" >/dev/null 2>&1; then
  echo "action pin verifier accepted an unpinned .yaml workflow" >&2
  exit 1
fi

echo "action pin verifier regression tests passed"
