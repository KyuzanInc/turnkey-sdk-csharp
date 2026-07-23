#!/usr/bin/env bash
set -euo pipefail

if [[ -n "${ACTION_PIN_REPO_ROOT:-}" ]]; then
  repo_root=$ACTION_PIN_REPO_ROOT
else
  repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fi
manifest="$repo_root/.github/pin-actions.txt"
if [[ ! -f "$manifest" ]]; then
  echo "::error::Action pin manifest not found: $manifest" >&2
  exit 1
fi
observed=$(mktemp)
expected=$(mktemp)
gh_err=$(mktemp)
trap 'rm -f "$observed" "$expected" "$gh_err"' EXIT

shopt -s nullglob
workflows=(
  "$repo_root"/.github/workflows/*.yml
  "$repo_root"/.github/workflows/*.yaml
)
shopt -u nullglob

if (( ${#workflows[@]} == 0 )); then
  echo "::error::No workflow YAML files were found."
  exit 1
fi

for workflow in "${workflows[@]}"; do
  while IFS= read -r line; do
    # A `uses:` key with its value wrapped onto a following line (valid
    # YAML) has no non-space, non-comment character after "uses:" on this
    # line, so the check below silently `continue`s past it without ever
    # verifying a SHA. Reject that shape outright instead.
    if [[ "$line" =~ ^[[:space:]]*-?[[:space:]]*uses:[[:space:]]*(#.*)?$ ]]; then
      echo "::error::$(basename "$workflow") has a 'uses:' key with its value on a following line; this gate only verifies single-line 'uses: owner/repo@<sha>'." >&2
      exit 1
    fi
    [[ "$line" =~ uses:[[:space:]]*([^[:space:]#]+) ]] || continue
    spec=${BASH_REMATCH[1]}
    [[ "$spec" == ./* ]] && continue

    if [[ ! "$spec" =~ ^([^@]+)@([0-9a-f]{40})$ ]]; then
      echo "::error::$(basename "$workflow") contains an action that is not pinned to a full lowercase commit SHA."
      exit 1
    fi

    action_ref=${BASH_REMATCH[1]}
    sha=${BASH_REMATCH[2]}
    owner=${action_ref%%/*}
    remainder=${action_ref#*/}
    repository=${remainder%%/*}
    printf '%s/%s %s %s\n' \
      "$owner" "$repository" "$sha" "$(basename "$workflow")" \
      >> "$observed"
  done < "$workflow"
done

awk '
  /^[A-Za-z0-9_.-]+\/[A-Za-z0-9_.-]+$/ {
    action = $0
    sha = ""
    in_used = 0
    next
  }
  /^  pinned_sha:[[:space:]]+/ {
    sha = $2
    next
  }
  /^  used_in:[[:space:]]+/ {
    in_used = 1
    file = $2
    sub(/\(.*/, "", file)
    print action, sha, file
    next
  }
  in_used && /^  [A-Za-z_]+:/ {
    in_used = 0
    next
  }
  in_used && /^[[:space:]]+[^[:space:]]/ {
    file = $1
    sub(/\(.*/, "", file)
    print action, sha, file
  }
' "$manifest" > "$expected"

LC_ALL=C sort -u -o "$observed" "$observed"
LC_ALL=C sort -u -o "$expected" "$expected"

if ! diff -u "$expected" "$observed"; then
  echo "::error::Workflow action pins and .github/pin-actions.txt differ."
  exit 1
fi

# A full-length hex string pinned via `uses: owner/repo@<sha>` is not
# necessarily a commit: GitHub also accepts (and `git rev-parse` happily
# resolves) the SHA of an annotated TAG OBJECT, which is a separate,
# re-taggable pointer — pinning to one does not pin the underlying commit
# the way this manifest's format implies. `git/commits/<sha>` returns 200
# only for a real commit object and 422 for a tag object, so this is the
# one check that actually enforces "SHA means commit."
#
# This is a real availability tradeoff for a required gate: it adds a
# network dependency (the GitHub API) and a tool dependency (the `gh`
# CLI, authenticated) that the rest of this script does not otherwise
# need. It fails closed — missing `gh`, missing auth, rate-limiting, and
# any non-2xx response all fail the gate rather than silently passing —
# because a verification gate that fails open on its own tooling being
# unavailable is not a verification gate. Set
# ACTION_PIN_SKIP_COMMIT_VERIFY=1 for a local-only, network-free run with
# reduced guarantees; CI must never set it.
if [[ "${ACTION_PIN_SKIP_COMMIT_VERIFY:-0}" != "1" ]]; then
  if ! command -v gh >/dev/null 2>&1; then
    echo "::error::gh CLI not found. It is required to verify pinned SHAs are commit objects, not annotated tag objects. Install and authenticate gh, or set ACTION_PIN_SKIP_COMMIT_VERIFY=1 for a local-only run with reduced guarantees (never in CI)." >&2
    exit 1
  fi
  while read -r owner_repo pin_sha; do
    [[ -z "$owner_repo" ]] && continue
    if ! gh api "repos/${owner_repo}/git/commits/${pin_sha}" >/dev/null 2>"$gh_err"; then
      echo "::error::${owner_repo}@${pin_sha} did not resolve to a commit object via the GitHub API (it may be an annotated tag object, or the API call failed/rate-limited). Re-resolve the SHA with: gh api repos/${owner_repo}/git/ref/tags/<tag> --jq '.object' — if .object.type is \"tag\", dereference it once more via .object.sha to get the real commit." >&2
      cat "$gh_err" >&2
      exit 1
    fi
  done < <(awk '{print $1, $2}' "$observed" | LC_ALL=C sort -u)
fi

count=$(wc -l < "$observed" | tr -d ' ')
echo "action pin verification passed: ${count} action/workflow mappings"
