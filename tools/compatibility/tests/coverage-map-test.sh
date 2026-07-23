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

# --- Regression cases: fail closed on upstream test forms the Step 1
# enumerator cannot parse, instead of silently under-counting coverage. ---

new_fixture() {
  # Prints the path to a fresh, isolated fixture root with coverage-map.sh
  # copied in and the standard directory layout created, but no upstream
  # test files yet.
  local root
  root="$(mktemp -d)"
  mkdir -p \
    "$root/tools/compatibility" \
    "$root/docs/compatibility" \
    "$root/tests/UpstreamSources"
  cp "$SOURCE_ROOT/tools/compatibility/coverage-map.sh" \
    "$root/tools/compatibility/coverage-map.sh"
  chmod +x "$root/tools/compatibility/coverage-map.sh"
  : > "$root/tools/compatibility/coverage-map.na.tsv"
  printf '%s' "$root"
}

# An unrecognized test form (e.g. test.concurrent(...)) must fail closed
# instead of being silently dropped from the coverage map.
UNRECOGNIZED_ROOT="$(new_fixture)"
mkdir -p "$UNRECOGNIZED_ROOT/tests/UpstreamSources/sample/ts-source/__tests__"
cat > "$UNRECOGNIZED_ROOT/tests/UpstreamSources/sample/ts-source/__tests__/sample-test.ts" <<'EOF'
test.concurrent("sample behavior", () => {});
EOF
if ( cd "$UNRECOGNIZED_ROOT" && ./tools/compatibility/coverage-map.sh ) \
    >"$UNRECOGNIZED_ROOT/output.txt" 2>&1; then
  echo "ERROR: coverage-map.sh accepted an unrecognized test form (test.concurrent)" >&2
  cat "$UNRECOGNIZED_ROOT/output.txt" >&2
  exit 1
fi
grep -Fq "UNRECOGNIZED" "$UNRECOGNIZED_ROOT/output.txt" || {
  echo "ERROR: expected an UNRECOGNIZED diagnostic for test.concurrent" >&2
  cat "$UNRECOGNIZED_ROOT/output.txt" >&2
  exit 1
}
rm -rf "$UNRECOGNIZED_ROOT"

# An empty upstream test match set must fail closed rather than silently
# succeed with an empty coverage map (see coverage-map.sh for the bash 3.2
# `set -u` / EXIT trap interaction this also guards against).
EMPTY_ROOT="$(new_fixture)"
if ( cd "$EMPTY_ROOT" && ./tools/compatibility/coverage-map.sh ) \
    >"$EMPTY_ROOT/output.txt" 2>&1; then
  echo "ERROR: coverage-map.sh accepted an empty upstream test match set" >&2
  cat "$EMPTY_ROOT/output.txt" >&2
  exit 1
fi
grep -Fq "no upstream test files found" "$EMPTY_ROOT/output.txt" || {
  echo "ERROR: expected a 'no upstream test files found' diagnostic" >&2
  cat "$EMPTY_ROOT/output.txt" >&2
  exit 1
}
rm -rf "$EMPTY_ROOT"

# A matched upstream file that contributes zero rows (no recognized test
# calls at all, e.g. a helper file swept up by the glob) must be caught by
# the --check row-count floor, since the MISSING-row check alone is
# vacuously true over zero rows.
ZERO_ROWS_ROOT="$(new_fixture)"
mkdir -p "$ZERO_ROWS_ROOT/tests/UpstreamSources/sample/ts-source/__tests__"
cat > "$ZERO_ROWS_ROOT/tests/UpstreamSources/sample/ts-source/__tests__/sample-test.ts" <<'EOF'
// no actual test() calls in this file
export const helper = () => 1;
EOF
( cd "$ZERO_ROWS_ROOT" && ./tools/compatibility/coverage-map.sh )
if ( cd "$ZERO_ROWS_ROOT" && ./tools/compatibility/coverage-map.sh --check ) \
    >"$ZERO_ROWS_ROOT/output.txt" 2>&1; then
  echo "ERROR: coverage-map.sh --check accepted a zero-row coverage map" >&2
  cat "$ZERO_ROWS_ROOT/output.txt" >&2
  exit 1
fi
grep -Fq "zero upstream test rows" "$ZERO_ROWS_ROOT/output.txt" || {
  echo "ERROR: expected a 'zero upstream test rows' diagnostic" >&2
  cat "$ZERO_ROWS_ROOT/output.txt" >&2
  exit 1
}
rm -rf "$ZERO_ROWS_ROOT"

echo "coverage-map regression tests passed"
