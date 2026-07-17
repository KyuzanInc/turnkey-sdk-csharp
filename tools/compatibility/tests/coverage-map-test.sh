#!/usr/bin/env bash

set -euo pipefail

SOURCE_ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
TMP_ROOT="$(mktemp -d)"
trap 'rm -rf "$TMP_ROOT"' EXIT

mkdir -p \
  "$TMP_ROOT/tools/compatibility" \
  "$TMP_ROOT/docs/compatibility" \
  "$TMP_ROOT/tests/UpstreamSources/sample/ts-source/__tests__"

cp "$SOURCE_ROOT/tools/compatibility/coverage-map.sh" \
  "$TMP_ROOT/tools/compatibility/coverage-map.sh"
chmod +x "$TMP_ROOT/tools/compatibility/coverage-map.sh"

cat > "$TMP_ROOT/tests/UpstreamSources/sample/ts-source/__tests__/sample-test.ts" <<'EOF'
test("sample behavior", () => {});
EOF

: > "$TMP_ROOT/tools/compatibility/coverage-map.na.tsv"

write_test_method() {
  local attribute="$1"
  cat > "$TMP_ROOT/tests/SampleTests.cs" <<EOF
namespace Sample.Tests
{
    public class SampleTests
    {
        /// upstream: tests/UpstreamSources/sample/ts-source/__tests__/sample-test.ts:1 "sample behavior"
        $attribute
        public void SampleBehavior()
        {
        }
    }
}
EOF
}

assert_check_fails_with() {
  local expected="$1"
  local output="$TMP_ROOT/check-output.txt"
  if "$TMP_ROOT/tools/compatibility/coverage-map.sh" --check >"$output" 2>&1; then
    echo "ERROR: coverage-map check unexpectedly passed" >&2
    cat "$output" >&2
    exit 1
  fi
  if ! grep -Fq "$expected" "$output"; then
    echo "ERROR: expected coverage-map failure containing: $expected" >&2
    cat "$output" >&2
    exit 1
  fi
}

# An annotation on an ordinary method must not satisfy the gate.
write_test_method ""
"$TMP_ROOT/tools/compatibility/coverage-map.sh"
assert_check_fails_with "MISSING rows detected"

# A skipped xUnit test is not executable coverage.
write_test_method '[Fact(Skip = "not executed")]'
"$TMP_ROOT/tools/compatibility/coverage-map.sh"
assert_check_fails_with "MISSING rows detected"

# An executable xUnit test satisfies the mapping.
write_test_method "[Fact]"
"$TMP_ROOT/tools/compatibility/coverage-map.sh"
"$TMP_ROOT/tools/compatibility/coverage-map.sh" --check

# Check mode must reject stale committed outputs instead of rewriting them.
printf '\n<!-- stale -->\n' >> "$TMP_ROOT/docs/compatibility/coverage-map.md"
assert_check_fails_with "committed coverage map is out of date"

"$TMP_ROOT/tools/compatibility/coverage-map.sh"
"$TMP_ROOT/tools/compatibility/coverage-map.sh" --check

echo "coverage-map regression tests passed"
