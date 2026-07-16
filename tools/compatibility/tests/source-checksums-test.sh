#!/usr/bin/env bash

set -euo pipefail

SOURCE_ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
TMP_ROOT="$(mktemp -d)"
trap 'rm -rf "$TMP_ROOT"' EXIT

mkdir -p "$TMP_ROOT/tools/compatibility"
cp "$SOURCE_ROOT/tools/compatibility/verify-source-checksums.sh" \
  "$TMP_ROOT/tools/compatibility/verify-source-checksums.sh"
chmod +x "$TMP_ROOT/tools/compatibility/verify-source-checksums.sh"

packages=(
  "turnkey-api-key-stamper-0.5.0"
  "turnkey-crypto-2.8.8"
  "turnkey-encoding-0.6.0"
  "turnkey-http-3.16.0"
)

write_fixture() {
  local package_count="$1"
  rm -rf "$TMP_ROOT/tests/UpstreamSources"
  mkdir -p "$TMP_ROOT/tests/UpstreamSources"
  : > "$TMP_ROOT/tests/UpstreamSources/package-checksums.txt"
  : > "$TMP_ROOT/tests/UpstreamSources/source-file-checksums.txt"

  local index package source_path source_sha
  for ((index = 0; index < package_count; index++)); do
    package="${packages[$index]}"
    source_path="tests/UpstreamSources/$package/ts-source/index.ts"
    mkdir -p "$TMP_ROOT/tests/UpstreamSources/$package/ts-source"
    printf '{}\n' > "$TMP_ROOT/tests/UpstreamSources/$package/package.json"
    printf 'export const packageName = "%s";\n' "$package" \
      > "$TMP_ROOT/$source_path"
    source_sha="$(shasum -a 256 "$TMP_ROOT/$source_path" | awk '{print $1}')"
    printf '%064d  %s.tgz\n' 0 "$package" \
      >> "$TMP_ROOT/tests/UpstreamSources/package-checksums.txt"
    printf '%s  %s\n' "$source_sha" "$source_path" \
      >> "$TMP_ROOT/tests/UpstreamSources/source-file-checksums.txt"
  done
}

assert_check_fails_with() {
  local expected="$1"
  local output="$TMP_ROOT/check-output.txt"
  if "$TMP_ROOT/tools/compatibility/verify-source-checksums.sh" \
    >"$output" 2>&1; then
    echo "ERROR: source checksum verification unexpectedly passed" >&2
    cat "$output" >&2
    exit 1
  fi
  if ! grep -Fq "$expected" "$output"; then
    echo "ERROR: expected source checksum failure containing: $expected" >&2
    cat "$output" >&2
    exit 1
  fi
}

# The complete four-package fixture passes.
write_fixture 4
"$TMP_ROOT/tools/compatibility/verify-source-checksums.sh"

# Removing one package from both the snapshot and source manifest must still
# fail because package-checksums.txt remains the package-set authority.
missing_package="${packages[3]}"
rm -rf "$TMP_ROOT/tests/UpstreamSources/$missing_package"
awk -v package="$missing_package" 'index($0, "/" package "/") == 0' \
  "$TMP_ROOT/tests/UpstreamSources/source-file-checksums.txt" \
  > "$TMP_ROOT/tests/UpstreamSources/source-file-checksums.txt.tmp"
mv "$TMP_ROOT/tests/UpstreamSources/source-file-checksums.txt.tmp" \
  "$TMP_ROOT/tests/UpstreamSources/source-file-checksums.txt"
assert_check_fails_with \
  "retained source package set differs from package-checksums.txt"

# Deleting the package from every machine-readable input must also fail.
write_fixture 3
assert_check_fails_with \
  "package checksum manifest must list the exact pinned package set"

# Replacing a pin with an unrelated package while preserving the count and all
# other machine-readable inputs must fail the exact-set check.
original_package="${packages[3]}"
packages[3]="unrelated-package-9.9.9"
write_fixture 4
assert_check_fails_with \
  "package checksum manifest must list the exact pinned package set"
packages[3]="$original_package"

# Tarball checksum fields must be lowercase 64-character SHA-256 hex strings.
write_fixture 4
awk '{$1 = "not-a-sha"; print}' \
  "$TMP_ROOT/tests/UpstreamSources/package-checksums.txt" \
  > "$TMP_ROOT/tests/UpstreamSources/package-checksums.txt.tmp"
mv "$TMP_ROOT/tests/UpstreamSources/package-checksums.txt.tmp" \
  "$TMP_ROOT/tests/UpstreamSources/package-checksums.txt"
assert_check_fails_with "malformed package checksum manifest"

echo "source checksum regression tests passed"
