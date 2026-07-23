#!/usr/bin/env bash
# Regression test for the "Compute drift report" step in
# .github/workflows/upstream-drift.yml.
#
# That step is only ever executed on a monthly schedule (or manual
# workflow_dispatch), so a quoting bug in it can sit undetected for a long
# time. It previously had a stray unescaped backtick at the end of each of
# the four DRIFT report lines, which opened an unterminated command
# substitution: `set -euo pipefail` did not catch it (the failure happens
# inside a command substitution used as an echo argument, which is not a
# command failure), so the step still exited 0, but silently dropped the
# "expected sha256" and "live ref" lines from the report (2 lines emitted
# instead of 4). See the fixed lines for the correct escaping.
#
# This test extracts the actual `run: |` block for that step out of the
# real workflow file (so it exercises the shipped script, not a copy that
# can drift out of sync), executes it against a synthetic mismatch with a
# stubbed `curl`, and asserts the drift report has all 4 lines.
set -euo pipefail

tools_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
repo_root=$(cd "$tools_dir/.." && pwd)
workflow_file="$repo_root/.github/workflows/upstream-drift.yml"

fixture=$(mktemp -d)
trap 'rm -rf "$fixture"' EXIT

# Extract the "Compute drift report" step's `run: |` body verbatim. This
# relies on the step's known indentation (script body at 10 spaces, under
# an 8-space `run: |`) per YAML's literal block scalar rule, rather than a
# full YAML parse.
awk '
  /^        id: drift$/ { seen = 1 }
  seen && /^        run: \|$/ { inblock = 1; next }
  inblock {
    if ($0 ~ /^          /) { print substr($0, 11); next }
    if ($0 ~ /^[[:space:]]*$/) { print ""; next }
    exit
  }
' "$workflow_file" > "$fixture/drift-report.sh"

if [[ ! -s "$fixture/drift-report.sh" ]]; then
  echo "failed to extract the 'Compute drift report' run block from $workflow_file (step renamed or re-indented?)" >&2
  exit 1
fi
bash -n "$fixture/drift-report.sh"

# Stub `curl` so the extracted script never leaves the sandbox: any call
# answers 200 with a fixed body, decoupled from the real GitHub API. This
# only needs to support the one invocation shape the step uses:
#   curl ... --output FILE --write-out '%{http_code}' ... URL
mkdir -p "$fixture/bin"
cat > "$fixture/bin/curl" <<'STUB'
#!/usr/bin/env bash
out=""
args=("$@")
for ((i = 0; i < ${#args[@]}; i++)); do
  if [[ "${args[$i]}" == "--output" ]]; then
    out="${args[$((i + 1))]}"
  fi
done
printf '{"encoding":"base64","content":"%s"}' "$(printf 'mismatched-live-content' | base64)" > "$out"
printf '200'
STUB
chmod +x "$fixture/bin/curl"

mkdir -p "$fixture/tests/UpstreamSources"
# An all-zero sha256 never matches real content, so this deterministically
# hits the DRIFT branch without needing to predict a real hash.
cat > "$fixture/tests/UpstreamSources/source-file-checksums.txt" <<'MANIFEST'
0000000000000000000000000000000000000000000000000000000000000000  tests/UpstreamSources/demo-pkg/ts-source/__tests__/demo-test.ts
MANIFEST

github_output="$fixture/github_output"
: > "$github_output"

(
  cd "$fixture"
  PATH="$fixture/bin:$PATH" \
  GH_TOKEN=dummy-token \
  PACKAGE=demo-pkg \
  OWNER=demo-owner \
  REPO=demo-repo \
  GIT_SHA=deadbeefdeadbeefdeadbeefdeadbeefdeadbeef \
  TARBALL_DIR=packages/demo \
  GITHUB_OUTPUT="$github_output" \
  bash "$fixture/drift-report.sh"
)

report_path=$(awk -F= '$1 == "REPORT_PATH" { print $2 }' "$github_output")
drift_count=$(awk -F= '$1 == "DRIFT_COUNT" { print $2 }' "$github_output")

if [[ -z "$report_path" || ! -f "$report_path" ]]; then
  echo "drift report script did not emit a usable REPORT_PATH" >&2
  exit 1
fi
if [[ "$drift_count" != "1" ]]; then
  echo "expected DRIFT_COUNT=1, got '$drift_count'" >&2
  exit 1
fi

line_count=$(wc -l < "$report_path" | tr -d ' ')
if [[ "$line_count" != "4" ]]; then
  echo "expected a 4-line drift report block, got $line_count line(s):" >&2
  cat "$report_path" >&2
  exit 1
fi

for fragment in '**DRIFT**' 'expected sha256' 'live sha256' 'live ref'; do
  grep -qF "$fragment" "$report_path" || {
    echo "drift report is missing expected fragment: $fragment" >&2
    cat "$report_path" >&2
    exit 1
  }
done

echo "upstream-drift report regression test passed"
