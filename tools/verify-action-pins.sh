#!/usr/bin/env bash
set -euo pipefail

if [[ -n "${ACTION_PIN_REPO_ROOT:-}" ]]; then
  repo_root=$ACTION_PIN_REPO_ROOT
else
  repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fi
manifest="$repo_root/.github/pin-actions.txt"
observed=$(mktemp)
expected=$(mktemp)
trap 'rm -f "$observed" "$expected"' EXIT

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

count=$(wc -l < "$observed" | tr -d ' ')
echo "action pin verification passed: ${count} action/workflow mappings"
