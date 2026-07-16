#!/usr/bin/env bash

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MANIFEST="$REPO_ROOT/tests/UpstreamSources/source-file-checksums.txt"

if [[ ! -f "$MANIFEST" ]]; then
  echo "ERROR: checksum manifest not found: $MANIFEST" >&2
  exit 1
fi

cd "$REPO_ROOT"

expected_paths="$(mktemp)"
actual_paths="$(mktemp)"
trap 'rm -f "$expected_paths" "$actual_paths"' EXIT

awk '!/^#/ && NF >= 2 { print $2 }' "$MANIFEST" | sort > "$expected_paths"
find tests/UpstreamSources -type f -path '*/ts-source/*' | sort > "$actual_paths"

if ! cmp -s "$expected_paths" "$actual_paths"; then
  echo "ERROR: retained upstream source set differs from the checksum manifest." >&2
  diff -u "$expected_paths" "$actual_paths" || true
  exit 1
fi

while read -r expected path; do
  [[ -z "${expected:-}" || "$expected" == \#* ]] && continue
  actual="$(shasum -a 256 "$path" | awk '{print $1}')"
  if [[ "$actual" != "$expected" ]]; then
    echo "ERROR: checksum mismatch: $path" >&2
    echo "  expected: $expected" >&2
    echo "  actual:   $actual" >&2
    exit 1
  fi
done < "$MANIFEST"

echo "upstream source checksum verification passed: $(wc -l < "$actual_paths" | tr -d ' ') files"
