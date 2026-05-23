#!/usr/bin/env bash
#
# codex-crypto-review.sh — run one Codex review round for one C# file.
#
# Usage:
#   ./codex-crypto-review.sh <CSharpFile> <Round>
# Example:
#   ./codex-crypto-review.sh Crypto.cs 1
#
# Output:
#   ./{CSharpFile}-r{Round}-{YYYYMMDD}.md  (codex stdout, verbatim)
#
# Pre-conditions:
#   - codex CLI is installed
#   - upstream-snapshots/ is populated (npm pack + extract)
#   - turnkey-source-pins.md maps each src/*.cs to its upstream package + path

set -euo pipefail

CS_FILE="${1:?Usage: $0 <CSharpFile> <Round>}"
ROUND="${2:?Usage: $0 <CSharpFile> <Round>}"

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
REVIEWS_DIR="$REPO_ROOT/codex-crypto-reviews"
DATE="$(date +%Y%m%d)"
OUT="$REVIEWS_DIR/${CS_FILE}-r${ROUND}-${DATE}.md"

# Resolve upstream package for the file via turnkey-source-pins.md.
# Convention: pins file contains a markdown table with one row per .cs file
# mapping it to a single upstream npm package and an upstream-snapshots path.
PINS="$REVIEWS_DIR/turnkey-source-pins.md"
if [[ ! -f "$PINS" ]]; then
  echo "ERROR: $PINS not found. Cannot resolve upstream source." >&2
  exit 1
fi

UPSTREAM=$(awk -F'|' -v f="$CS_FILE" '
  /^\| *src\/[A-Za-z]+\.cs/ {
    gsub(/^[ \t]+|[ \t]+$/, "", $2)
    if ($2 == "src/" f) {
      gsub(/^[ \t]+|[ \t]+$/, "", $3)
      print $3
      exit
    }
  }
' "$PINS")

if [[ -z "${UPSTREAM:-}" ]]; then
  echo "ERROR: no upstream pin found for src/$CS_FILE in $PINS" >&2
  exit 1
fi

CS_PATH="$REPO_ROOT/src/$CS_FILE"
UPSTREAM_DIR="$REVIEWS_DIR/upstream-snapshots/$UPSTREAM"
if [[ ! -d "$UPSTREAM_DIR" ]]; then
  echo "ERROR: upstream snapshot dir $UPSTREAM_DIR not found" >&2
  exit 1
fi

TARBALL_HASHES="$REVIEWS_DIR/upstream-snapshots/tarball-checksums.txt"
if [[ ! -f "$TARBALL_HASHES" ]]; then
  echo "ERROR: $TARBALL_HASHES not found" >&2
  exit 1
fi

CS_SHA="$(cd "$REPO_ROOT" && git log -n1 --format=%H -- "src/$CS_FILE" 2>/dev/null || echo 'uncommitted')"

PROMPT=$(cat <<PROMPT_EOF
You are reviewing a C# port of the Turnkey TypeScript SDK.
File under review (C#): src/$CS_FILE  (git SHA at last commit touching it: $CS_SHA)
Pinned upstream npm package directory: codex-crypto-reviews/upstream-snapshots/$UPSTREAM
Tarball checksums file: codex-crypto-reviews/upstream-snapshots/tarball-checksums.txt

This is REVIEW ROUND $ROUND of 3 for this file.

REQUIRED OUTPUTS (sections A through G must all appear):

A. Source pin acknowledgement:
   upstream package name, version, tarball sha256 (look up in tarball-checksums.txt),
   C# file git SHA ($CS_SHA).

B. Method coverage table: every public + internal helper method in src/$CS_FILE
   listed in a markdown table:
     | C# method (file:line) | Upstream TS function (path:line) | Status | Notes |
   Status: REVIEWED or NOT-REVIEWED. If NOT-REVIEWED, give a reason. Do not skip rows.

C. Intentional adaptations: list every C#/TS adaptation pattern with a one-line
   justification that it is structural (no wire-byte or observable-behavior change).

D. (Crypto.cs only) D17 enforcement check. BouncyCastle allow-list:
     - ECDSA P-256 sign/verify, ECDH P-256, AES-GCM (128-bit tag), SHA-256,
       HMAC-SHA256, BigInteger arithmetic, EC point operations.
     - Ed25519PrivateKeyParameters.GeneratePublicKey (seed -> public key)
       only in the SOLANA export branch of DecryptExportBundle.
   Banned APIs (must not appear):
     - Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator
     - Org.BouncyCastle.Crypto.Hpke.*
     - Any "high-level" KDF or HPKE wrapper
   If file is not Crypto.cs, write "N/A (not Crypto.cs)".

E. Logic divergence findings: every place C# behavior differs from upstream TS:
   algorithm step order, constants, error handling, byte ordering, leading-zero
   handling, padding, rounding/normalization, signature format (DER vs raw r||s,
   low-S), DTO shape (field names, order, presence, optionality), JSON
   serialization (property order, casing, null handling, escaping).
   For each item: C# file:line, TS upstream path:line, semantic diff,
   suggested fix.

F. Fixture comparison gate: for every fixture in tests/Fixtures/ that exercises
   this file, confirm it was generated from the pinned upstream package and the
   C# test asserts the same bytes Node would produce.

G. Unresolved assumptions you could not verify in this round.

PASS criterion for this round: B has zero NOT-REVIEWED rows (or each is
documented), D is "no banned API present" (Crypto.cs only) or N/A, E has zero
entries, F is "all fixtures match".

DO NOT use "looks good" or "no divergence found" without producing all sections.
PROMPT_EOF
)

cd "$REPO_ROOT"
{
  echo "# Codex review — src/$CS_FILE — round $ROUND — $DATE"
  echo ""
  echo "C# SHA: \`$CS_SHA\`"
  echo "Upstream snapshot: \`$UPSTREAM\`"
  echo ""
  echo "---"
  echo ""
  printf '%s\n' "$PROMPT" | codex exec \
    -s read-only \
    --skip-git-repo-check \
    -C "$REPO_ROOT" \
    -c model_reasoning_effort=high \
    - 2>&1
} > "$OUT"

echo "Wrote $OUT"
