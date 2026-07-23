#!/usr/bin/env bash
#
# coverage-map.sh — generate / verify the upstream test → C# test coverage matrix.
#
# Modes:
#   coverage-map.sh           — write coverage-map.tsv + coverage-map.md (no gate)
#   coverage-map.sh --check   — generate into temporary files and exit non-zero
#                                if an upstream test is MISSING, an N/A reason is
#                                empty, or the committed outputs are stale.
#
# Inputs:
#   tests/UpstreamSources/*/ts-source/__tests__/*-test*.ts
#                                 — pinned upstream TypeScript test files
#   tests/*Tests.cs              — C# test files. Each test method's coverage is
#                                  declared by an XML doc-comment of the form:
#                                    /// upstream: <relpath>:<line> "<test name>"
#                                  placed immediately above the [Fact]/[Theory]
#                                  attribute. The annotation is hand-maintained.
#   tools/compatibility/coverage-map.na.tsv
#                                 — manually-curated rows for upstream tests that
#                                  the C# SDK deliberately does NOT port. Each
#                                  row is TAB-separated:
#                                    <upstream_relpath>:<line><TAB><reason>
#                                  The reason MUST be non-empty (the gate rejects
#                                  empty reasons).
#
# Outputs:
#   tools/compatibility/coverage-map.tsv
#                                 — joined matrix, one row per upstream test.
#   docs/compatibility/coverage-map.md
#                                 — same data rendered for human review.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
COMPAT_DIR="$REPO_ROOT/tools/compatibility"
UPSTREAM_DIR="$REPO_ROOT/tests/UpstreamSources"
TESTS_DIR="$REPO_ROOT/tests"
NA_FILE="$COMPAT_DIR/coverage-map.na.tsv"
CANONICAL_TSV_OUT="$COMPAT_DIR/coverage-map.tsv"
CANONICAL_MD_OUT="$REPO_ROOT/docs/compatibility/coverage-map.md"

CHECK_MODE=0
if [[ "${1:-}" == "--check" ]]; then
  CHECK_MODE=1
fi

CHECK_TSV_OUT=""
CHECK_MD_OUT=""
if [[ "$CHECK_MODE" = "1" ]]; then
  CHECK_TSV_OUT="$(mktemp)"
  CHECK_MD_OUT="$(mktemp)"
  TSV_OUT="$CHECK_TSV_OUT"
  MD_OUT="$CHECK_MD_OUT"
else
  TSV_OUT="$CANONICAL_TSV_OUT"
  MD_OUT="$CANONICAL_MD_OUT"
fi

cd "$REPO_ROOT"

# -------------------------------------------------------------------
# Step 1. Enumerate upstream tests.
#
# Format emitted to TMP_UPSTREAM (TAB-separated):
#   <relpath>:<line>\t<test-name>
#
# Handles two source forms:
#   (a) test("name", ...)            — single-string test()/it()
#   (b) test.each([...])("name", …)  — parametrized; name template can contain $var
# -------------------------------------------------------------------

TMP_UPSTREAM="$(mktemp)"
TMP_MAPPED="$(mktemp)"
TMP_NA="$(mktemp)"
trap 'rm -f "$TMP_UPSTREAM" "$TMP_MAPPED" "$TMP_NA" "$CHECK_TSV_OUT" "$CHECK_MD_OUT" 2>/dev/null || true' EXIT

# Use find -print0 + xargs is safer for spaces but our path tree has no spaces.
# Keep simple: glob expansion with sorted ordering.
UPSTREAM_FILES=()
while IFS= read -r f; do
  UPSTREAM_FILES+=("$f")
done < <(find "$UPSTREAM_DIR" -type f \
            \( -name '*-test.ts' -o -name '*-tests.ts' \) \
            -path '*/__tests__/*' | sort)

# Guard against an empty match set. Without this, "${UPSTREAM_FILES[@]}"
# below is an empty-array expansion: under bash >=4.4 the for loop just
# runs zero times (silently producing an empty TMP_UPSTREAM, which the
# --check row-count floor below now also catches), but under bash 3.2
# (macOS's default /bin/bash) it is an unbound-variable error under
# `set -u` — and because that error fires with $? already reset to 0,
# the `... || true` EXIT trap reports the whole script as exit 0 despite
# never generating a coverage map. Fail loudly and explicitly instead.
if (( ${#UPSTREAM_FILES[@]} == 0 )); then
  echo "ERROR: no upstream test files found under $UPSTREAM_DIR (path or glob pattern changed?)." >&2
  exit 1
fi

for f in "${UPSTREAM_FILES[@]}"; do
  rel="${f#$REPO_ROOT/}"
  awk -v rel="$rel" '
    BEGIN { in_each=0; each_line=0 }
    # match "test(" / "it(" with a literal-string first argument
    /^[[:space:]]*(test|it)[[:space:]]*\(["'"'"']/ {
      line=NR
      # extract quoted name. allow either " or '"'"'
      s=$0
      sub(/^[[:space:]]*(test|it)[[:space:]]*\(["'"'"']/, "", s)
      # cut at trailing matching quote — heuristic: last "," is the arg sep
      # so split at the last ", " preceding "(" / "=>"
      # simpler: cut at first unescaped trailing quote followed by comma
      n=index(s, "\","); if (n==0) n=index(s, "'"'"',")
      if (n>0) {
        name=substr(s, 1, n-1)
      } else {
        # fallback: strip leading quote, take until next quote
        name=s
        sub(/["'"'"'].*$/, "", name)
      }
      printf("%s:%d\t%s\n", rel, line, name)
      next
    }
    # match "test.each([" / "it.each([" — record the line; the template comes
    # on a later line and looks like:  ])("template", …)
    /^[[:space:]]*(test|it)\.each[[:space:]]*\([[:space:]]*\[/ {
      # A same-line closer ("])(" further in this same line) means the
      # whole test.each(...)(...) is written on one line. That form is
      # not parsed below (which expects the closer on its own line) and
      # would otherwise be silently dropped while also desyncing in_each
      # for the rest of the file. Fail closed instead.
      if ($0 ~ /\]\)[[:space:]]*\(/) {
        printf("UNRECOGNIZED upstream test form at %s:%d (single-line test.each): %s\n", rel, NR, $0) > "/dev/stderr"
        exit 1
      }
      in_each=1; each_line=NR; next
    }
    in_each && /^[[:space:]]*\]\)[[:space:]]*\(["'"'"']/ {
      s=$0
      sub(/^[[:space:]]*\]\)[[:space:]]*\(["'"'"']/, "", s)
      n=index(s, "\","); if (n==0) n=index(s, "'"'"',")
      if (n>0) name=substr(s,1,n-1); else { name=s; sub(/["'"'"'].*$/, "", name) }
      printf("%s:%d\t%s\n", rel, each_line, name)
      in_each=0
      next
    }
    # Anything else that opens with test/it followed by "(", "." (a
    # chained modifier such as .concurrent/.failing/.only/.skip/.todo, or
    # a same-line test.each collision not caught above) or whitespace is
    # a form this enumerator does not understand — e.g. a template-literal
    # test name, test.concurrent(...), it.todo(...). Silently skipping it
    # would under-count coverage without ever showing up as MISSING, since
    # the same enumerator both produces and (via committed-output staleness
    # checks) validates its own output. Fail closed instead.
    /^[[:space:]]*(test|it)[.( ]/ {
      printf("UNRECOGNIZED upstream test form at %s:%d: %s\n", rel, NR, $0) > "/dev/stderr"
      exit 1
    }
    END {
      if (in_each) {
        printf("UNRECOGNIZED upstream test form: unterminated test.each block starting at %s:%d\n", rel, each_line) > "/dev/stderr"
        exit 1
      }
    }
  ' "$f"
done > "$TMP_UPSTREAM"

# -------------------------------------------------------------------
# Step 2. Enumerate C# test annotations.
#
# Find every line of form:
#   /// upstream: <relpath>:<line> "<name>"
# and pair it with the closest following [Fact]/[Theory] + method.
#
# Output format (TAB-separated):
#   <relpath>:<line>\t<cs-file>:<cs-line>\t<cs-method>
# -------------------------------------------------------------------

for f in "$TESTS_DIR"/*Tests.cs; do
  [ -f "$f" ] || continue
  rel="${f#$REPO_ROOT/}"
  awk -v csfile="$rel" '
    function reset_pending() {
      delete pending
      pending_n=0
      has_test_attribute=0
      is_skipped=0
    }
    BEGIN { reset_pending() }
    /^[[:space:]]*\/\/\/[[:space:]]*upstream:[[:space:]]/ {
      # parse: /// upstream: <key> [..."<name>"]
      # capture the first space-delimited token after "upstream:"
      s=$0
      sub(/^[[:space:]]*\/\/\/[[:space:]]*upstream:[[:space:]]*/, "", s)
      # split by whitespace, take first field as the key
      n=split(s, parts, /[[:space:]]+/)
      key=parts[1]
      pending_n++
      pending[pending_n]=key
      next
    }
    pending_n > 0 && /^[[:space:]]*\[(Fact|Theory)(Attribute)?([[:space:]]|\(|\])/ {
      has_test_attribute=1
      if ($0 ~ /Skip[[:space:]]*=/) is_skipped=1
      next
    }
    pending_n > 0 && /^[[:space:]]*\[/ {
      if ($0 ~ /Skip[[:space:]]*=/) is_skipped=1
      next
    }
    pending_n > 0 && /^[[:space:]]*$/ { next }
    pending_n > 0 && /^[[:space:]]*\/\/\// { next }
    /^[[:space:]]*public[[:space:]]+(async[[:space:]]+)?(void|Task)[[:space:]]+[A-Za-z_][A-Za-z0-9_]*[[:space:]]*\(/ {
      if (pending_n > 0 && has_test_attribute && !is_skipped) {
        s=$0
        sub(/^[[:space:]]*public[[:space:]]+(async[[:space:]]+)?(void|Task)[[:space:]]+/, "", s)
        sub(/[[:space:]]*\(.*$/, "", s)
        for (i=1; i<=pending_n; i++) {
          printf("%s\t%s:%d\t%s\n", pending[i], csfile, NR, s)
        }
      }
      reset_pending()
      next
    }
    # Any other code before the public test declaration invalidates the
    # annotation. This prevents ordinary methods and skipped tests from
    # satisfying the compatibility gate.
    pending_n > 0 { reset_pending() }
  ' "$f"
done > "$TMP_MAPPED"

# -------------------------------------------------------------------
# Step 3. Load N/A entries.
# -------------------------------------------------------------------

if [ -f "$NA_FILE" ]; then
  awk -F '\t' '
    /^[[:space:]]*#/ { next }
    NF == 0 { next }
    {
      key=$1
      reason=$2
      printf("%s\t%s\n", key, reason)
    }
  ' "$NA_FILE" > "$TMP_NA"
else
  : > "$TMP_NA"
fi

# -------------------------------------------------------------------
# Step 4. Join — emit one row per upstream test.
#
# TSV columns (header on row 1):
#   upstream_key  upstream_name  status  csharp_file  csharp_line  csharp_method  reason
# -------------------------------------------------------------------

{
  printf "upstream_key\tupstream_name\tstatus\tcsharp_target\tcsharp_method\treason\n"
  awk -F '\t' \
      -v mapped="$TMP_MAPPED" \
      -v na="$TMP_NA" '
    function load_mapped() {
      while ((getline line < mapped) > 0) {
        n=split(line, p, "\t")
        if (n >= 3) {
          k=p[1]
          # store comma-separated list of cs targets
          if (k in mapk) {
            mapk[k] = mapk[k] ", " p[2] " " p[3]
          } else {
            mapk[k] = p[2] " " p[3]
          }
        }
      }
      close(mapped)
    }
    function load_na() {
      while ((getline line < na) > 0) {
        n=split(line, p, "\t")
        if (n >= 2) {
          nak[p[1]] = p[2]
        }
      }
      close(na)
    }
    BEGIN { load_mapped(); load_na() }
    {
      key=$1; name=$2
      status="MISSING"; tgt=""; meth=""; reason=""
      if (key in mapk) {
        status="COVERED"
        tgt=mapk[key]
      } else if (key in nak) {
        status="N/A"
        reason=nak[key]
      }
      # split cs_target back into file vs method for the tsv output
      if (status == "COVERED") {
        n=split(tgt, parts, " ")
        printf("%s\t%s\t%s\t%s\t%s\t-\n", key, name, status, parts[1], parts[2])
      } else {
        printf("%s\t%s\t%s\t\t\t%s\n", key, name, status, reason)
      }
    }
  ' "$TMP_UPSTREAM"
} > "$TSV_OUT"

# -------------------------------------------------------------------
# Step 5. Render markdown.
# -------------------------------------------------------------------

{
  echo "# Coverage map — upstream test ↔ C# test"
  echo ""
  echo "Generated by [\`tools/compatibility/coverage-map.sh\`](../../tools/compatibility/coverage-map.sh)."
  echo ""
  echo "Statuses:"
  echo ""
  echo "- **COVERED** — at least one C# \`[Fact]/[Theory]\` method has a"
  echo "  matching \`/// upstream: <relpath>:<line>\` annotation."
  echo "- **N/A** — the upstream test is intentionally not ported; the reason"
  echo "  is recorded in [\`coverage-map.na.tsv\`](../../tools/compatibility/coverage-map.na.tsv)."
  echo "- **MISSING** — neither of the above. The change must add C# coverage"
  echo "  or add an explicit N/A row with a non-empty reason."
  echo ""
  echo "## Summary"
  echo ""
  awk -F '\t' 'NR>1 { c[$3]++ } END { for (k in c) printf("- %s: %d\n", k, c[k]) }' "$TSV_OUT" | sort
  echo ""
  echo "## Rows"
  echo ""
  echo "| Upstream key | Upstream test name | Status | C# target | C# method | N/A reason |"
  echo "|---|---|---|---|---|---|"
  awk -F '\t' 'NR>1 {
    # escape pipes in names just in case
    for (i=1; i<=NF; i++) gsub(/\|/, "\\|", $i)
    printf("| `%s` | %s | %s | %s | %s | %s |\n", $1, $2, $3, $4, $5, $6)
  }' "$TSV_OUT"
} > "$MD_OUT"

# -------------------------------------------------------------------
# Step 6. --check mode: enforce gate.
# -------------------------------------------------------------------

if [ "$CHECK_MODE" = "1" ]; then
  exit_code=0
  # Row-count floor: a TSV with only the header row means enumeration
  # silently produced zero upstream test rows (e.g. an empty UPSTREAM_DIR
  # match set on a bash where the empty-array guard above didn't fire, or
  # every upstream file failed to match any recognized test form). The
  # MISSING check below is vacuously true over zero rows, so it would not
  # catch this on its own. Mirrors the matched_count==0 guard in
  # .github/workflows/upstream-drift.yml.
  row_count=$(($(wc -l < "$TSV_OUT" | tr -d ' ') - 1))
  if (( row_count <= 0 )); then
    echo "ERROR: coverage map has zero upstream test rows; refusing to treat that as passing. See $TSV_OUT."
    exit_code=1
  fi
  # any MISSING ?
  if awk -F '\t' 'NR>1 && $3=="MISSING" { print; found=1 } END { exit found?1:0 }' "$TSV_OUT" >/dev/null; then
    : # awk exit 0 means no missing
  else
    echo "ERROR: MISSING rows detected. See $TSV_OUT or $MD_OUT."
    awk -F '\t' 'NR>1 && $3=="MISSING" { print "  MISSING: " $1 "  " $2 }' "$TSV_OUT"
    exit_code=1
  fi
  # any N/A with empty reason ?
  if awk -F '\t' 'NR>1 && $3=="N/A" && ($6=="" || $6 ~ /^[[:space:]]*$/) { print; found=1 } END { exit found?1:0 }' "$TSV_OUT" >/dev/null; then
    :
  else
    echo "ERROR: N/A rows missing reason text:"
    awk -F '\t' 'NR>1 && $3=="N/A" && ($6=="" || $6 ~ /^[[:space:]]*$/) { print "  EMPTY-NA-REASON: " $1 "  " $2 }' "$TSV_OUT"
    exit_code=1
  fi
  if [[ ! -f "$CANONICAL_TSV_OUT" ]] || ! cmp -s "$CANONICAL_TSV_OUT" "$TSV_OUT"; then
    echo "ERROR: committed coverage map is out of date: $CANONICAL_TSV_OUT"
    if [[ -f "$CANONICAL_TSV_OUT" ]]; then
      diff -u "$CANONICAL_TSV_OUT" "$TSV_OUT" || true
    fi
    echo "Run ./tools/compatibility/coverage-map.sh and commit the result."
    exit_code=1
  fi
  if [[ ! -f "$CANONICAL_MD_OUT" ]] || ! cmp -s "$CANONICAL_MD_OUT" "$MD_OUT"; then
    echo "ERROR: committed coverage map is out of date: $CANONICAL_MD_OUT"
    if [[ -f "$CANONICAL_MD_OUT" ]]; then
      diff -u "$CANONICAL_MD_OUT" "$MD_OUT" || true
    fi
    echo "Run ./tools/compatibility/coverage-map.sh and commit the result."
    exit_code=1
  fi
  if [ "$exit_code" = "0" ]; then
    echo "coverage-map check passed: 0 MISSING, all N/A rows have reasons, committed outputs are current."
  fi
  exit "$exit_code"
fi

# Default mode prints a short summary to stderr so CI logs are readable.
{
  echo "Wrote $TSV_OUT"
  echo "Wrote $MD_OUT"
  awk -F '\t' 'NR>1 { c[$3]++ } END { for (k in c) printf("  %s: %d\n", k, c[k]) }' "$TSV_OUT" | sort
} 1>&2
