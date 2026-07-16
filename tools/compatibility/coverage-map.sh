#!/usr/bin/env bash
#
# coverage-map.sh — generate / verify the upstream test → C# test coverage matrix.
#
# Modes:
#   coverage-map.sh           — write coverage-map.tsv + coverage-map.md (no gate)
#   coverage-map.sh --check   — write outputs AND exit non-zero if any
#                                upstream test is MISSING from C# coverage and
#                                has no N/A reason recorded in coverage-map.na.tsv.
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
TSV_OUT="$COMPAT_DIR/coverage-map.tsv"
MD_OUT="$REPO_ROOT/docs/compatibility/coverage-map.md"

CHECK_MODE=0
if [[ "${1:-}" == "--check" ]]; then
  CHECK_MODE=1
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
trap 'rm -f "$TMP_UPSTREAM" "$TMP_MAPPED" "$TMP_NA" "$TMP_REPORT" 2>/dev/null || true' EXIT
TMP_MAPPED="$(mktemp)"
TMP_NA="$(mktemp)"
TMP_REPORT="$(mktemp)"

# Use find -print0 + xargs is safer for spaces but our path tree has no spaces.
# Keep simple: glob expansion with sorted ordering.
UPSTREAM_FILES=()
while IFS= read -r f; do
  UPSTREAM_FILES+=("$f")
done < <(find "$UPSTREAM_DIR" -type f \
            \( -name '*-test.ts' -o -name '*-tests.ts' \) \
            -path '*/__tests__/*' | sort)

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
    function reset_pending() { delete pending; pending_n=0 }
    BEGIN { pending_n=0 }
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
    /^[[:space:]]*public[[:space:]]+(async[[:space:]]+)?(void|Task)[[:space:]]+[A-Za-z_][A-Za-z0-9_]*[[:space:]]*\(/ {
      if (pending_n > 0) {
        s=$0
        sub(/^[[:space:]]*public[[:space:]]+(async[[:space:]]+)?(void|Task)[[:space:]]+/, "", s)
        sub(/[[:space:]]*\(.*$/, "", s)
        for (i=1; i<=pending_n; i++) {
          printf("%s\t%s:%d\t%s\n", pending[i], csfile, NR, s)
        }
        reset_pending()
      }
      next
    }
    # If we see a non-comment non-attribute line before the public decl,
    # drop any pending annotations (defensive: prevents drift if a /// upstream
    # comment sits orphaned at the end of a region without a matching method).
    /^[[:space:]]*\}/ && pending_n > 0 { reset_pending() }
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
  if [ "$exit_code" = "0" ]; then
    echo "coverage-map check passed: 0 MISSING, all N/A rows have reasons."
  fi
  exit "$exit_code"
fi

# Default mode prints a short summary to stderr so CI logs are readable.
{
  echo "Wrote $TSV_OUT"
  echo "Wrote $MD_OUT"
  awk -F '\t' 'NR>1 { c[$3]++ } END { for (k in c) printf("  %s: %d\n", k, c[k]) }' "$TSV_OUT" | sort
} 1>&2
