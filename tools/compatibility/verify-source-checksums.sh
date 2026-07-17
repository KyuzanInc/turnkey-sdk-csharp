#!/usr/bin/env bash

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MANIFEST="$REPO_ROOT/tests/UpstreamSources/source-file-checksums.txt"
PACKAGE_MANIFEST="$REPO_ROOT/tests/UpstreamSources/package-checksums.txt"
EXPECTED_PACKAGES=(
  "turnkey-api-key-stamper-0.5.0"
  "turnkey-crypto-2.8.8"
  "turnkey-encoding-0.6.0"
  "turnkey-http-3.16.0"
)

if [[ ! -f "$MANIFEST" ]]; then
  echo "ERROR: checksum manifest not found: $MANIFEST" >&2
  exit 1
fi
if [[ ! -f "$PACKAGE_MANIFEST" ]]; then
  echo "ERROR: package checksum manifest not found: $PACKAGE_MANIFEST" >&2
  exit 1
fi

cd "$REPO_ROOT"

expected_paths="$(mktemp)"
actual_paths="$(mktemp)"
package_entries="$(mktemp)"
expected_packages="$(mktemp)"
package_manifest_packages="$(mktemp)"
manifest_package_entries="$(mktemp)"
manifest_packages="$(mktemp)"
trap 'rm -f "$expected_paths" "$actual_paths" "$package_entries" "$expected_packages" "$package_manifest_packages" "$manifest_package_entries" "$manifest_packages"' EXIT

printf '%s\n' "${EXPECTED_PACKAGES[@]}" | sort > "$expected_packages"

if ! awk '
  /^[[:space:]]*#/ || /^[[:space:]]*$/ { next }
  NF != 2 ||
    length($1) != 64 ||
    $1 ~ /[^0-9a-f]/ ||
    $2 !~ /^[A-Za-z0-9][A-Za-z0-9._-]*\.tgz$/ { exit 1 }
  {
    package = $2
    sub(/\.tgz$/, "", package)
    print package
  }
' "$PACKAGE_MANIFEST" > "$package_entries"; then
  echo "ERROR: malformed package checksum manifest: $PACKAGE_MANIFEST" >&2
  exit 1
fi

sort "$package_entries" > "$package_manifest_packages"
if ! cmp -s "$expected_packages" "$package_manifest_packages"; then
  echo "ERROR: package checksum manifest must list the exact pinned package set." >&2
  diff -u "$expected_packages" "$package_manifest_packages" || true
  exit 1
fi

if ! awk '
  /^[[:space:]]*#/ || /^[[:space:]]*$/ { next }
  NF < 2 { exit 1 }
  {
    count = split($2, parts, "/")
    if (count < 5 ||
        parts[1] != "tests" ||
        parts[2] != "UpstreamSources" ||
        parts[3] == "" ||
        parts[4] != "ts-source") {
      exit 1
    }
    print parts[3]
  }
' "$MANIFEST" > "$manifest_package_entries"; then
  echo "ERROR: malformed retained-source path in checksum manifest: $MANIFEST" >&2
  exit 1
fi

sort -u "$manifest_package_entries" > "$manifest_packages"
if ! cmp -s "$expected_packages" "$manifest_packages"; then
  echo "ERROR: retained source package set differs from package-checksums.txt." >&2
  diff -u "$expected_packages" "$manifest_packages" || true
  exit 1
fi

while IFS= read -r package; do
  if [[ ! -f "tests/UpstreamSources/$package/package.json" ]]; then
    echo "ERROR: retained package metadata missing: tests/UpstreamSources/$package/package.json" >&2
    exit 1
  fi
done < "$expected_packages"

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
