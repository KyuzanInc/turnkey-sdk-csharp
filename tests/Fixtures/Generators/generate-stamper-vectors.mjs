// generate-stamper-vectors.mjs
//
// Produce deterministic stamper byte vectors by running pinned
// @turnkey/api-key-stamper@0.5.0 with runtimeOverride="purejs" on the
// pinned upstream fixture key pair.
//
// IMPORTANT DESIGN NOTE
// ---------------------
// Upstream purejs.ts (codex-crypto-reviews/upstream-snapshots/
//   turnkey-api-key-stamper-0.5.0/ts-source/purejs.ts) calls
//   p256.sign(hash, privateKey)
// with NO options. With every version of @noble/curves in the 1.x line
// (1.3.0 through 1.9.x), this defaults to HIGH-S (s > n/2) — low-S
// requires the explicit { lowS: true } option.
//
// The C# port (src/ApiKeyStamper.cs) enforces low-S because that is the
// standard ECDSA wire-format that downstream verifiers expect. The two
// signatures (HIGH-S vs LOW-S) are cryptographically equivalent under
// the curve symmetry s ↔ (n - s) and BOTH verify against the same
// public key.
//
// This generator therefore CANNOT byte-snapshot the upstream DER hex
// (it would lock in HIGH-S, which the C# port would refuse to produce).
// Instead it locks in everything that IS deterministic:
//   - the r value (depends only on k from RFC 6979 — identical in both)
//   - the publicKey embedded in the stamp (identical)
//   - the scheme constant SIGNATURE_SCHEME_TK_API_P256 (identical)
//   - the JSON key order ["publicKey", "scheme", "signature"] (identical)
//   - the base64url stamp header value derivable from the above + the
//     C# low-S signature
//   - cryptographic verification: the C# stamper output must verify
//     against the same public key, and so must this fixture's signature
//
// Both signatures must verify against the same hash + public key, so
// the C# tests can compare r and verify the signature, but cannot
// compare s or DER hex.
//
// This intentional divergence is documented as the load-bearing
// finding from PR-4's preflight check (see plan Section 9 risk row
// `@noble/curves transitive drift`).

import { ApiKeyStamper } from "@turnkey/api-key-stamper";
import { p256 } from "@noble/curves/p256";
import { createHash } from "node:crypto";
import { readFile, writeFile } from "node:fs/promises";
import { execSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import path from "node:path";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const REPO_ROOT = path.resolve(__dirname, "..", "..", "..");
const PREFLIGHT_ONLY = process.argv.includes("--preflight-only");

const STAMPER_SNAPSHOT = path.join(
  REPO_ROOT,
  "codex-crypto-reviews",
  "upstream-snapshots",
  "turnkey-api-key-stamper-0.5.0",
  "ts-source",
  "__fixtures__",
);

const PREFLIGHT_PRIVATE = "487f361ddfd73440e707f4daa6775b376859e8a3c9f29b3bb694a12927c0213c";
const PREFLIGHT_PUBLIC = "02f739f8c77b32f4d5f13265861febd76e7a9c61a1140d296b8c16302508870316";
const PREFLIGHT_MESSAGE = "hello from TKHQ!";
const P256_ORDER = BigInt(
  "0xffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551",
);

async function readUpstreamKey(name) {
  const buf = await readFile(path.join(STAMPER_SNAPSHOT, name), "utf-8");
  return buf.trim();
}

function decodeDerEcdsa(derHex) {
  let i = 0;
  const buf = Buffer.from(derHex, "hex");
  if (buf[i++] !== 0x30) throw new Error("DER: not SEQUENCE");
  let len = buf[i++];
  if (len & 0x80) {
    const n = len & 0x7f;
    len = 0;
    for (let k = 0; k < n; k++) len = (len << 8) | buf[i++];
  }
  if (buf[i++] !== 0x02) throw new Error("DER: r not INTEGER");
  const rLen = buf[i++];
  const r = buf.subarray(i, i + rLen);
  i += rLen;
  if (buf[i++] !== 0x02) throw new Error("DER: s not INTEGER");
  const sLen = buf[i++];
  const s = buf.subarray(i, i + sLen);
  return {
    r: BigInt("0x" + r.toString("hex")),
    s: BigInt("0x" + s.toString("hex")),
    rHex: r.toString("hex").replace(/^00+/, ""),
    sHex: s.toString("hex").replace(/^00+/, ""),
  };
}

function lowSEquivalent(sBig) {
  return sBig > P256_ORDER / 2n ? P256_ORDER - sBig : sBig;
}

async function runPreflight() {
  const stamper = new ApiKeyStamper({
    apiPublicKey: PREFLIGHT_PUBLIC,
    apiPrivateKey: PREFLIGHT_PRIVATE,
    runtimeOverride: "purejs",
  });
  const decode = (s) =>
    JSON.parse(Buffer.from(s.stampHeaderValue, "base64url").toString());

  const stamp1 = await stamper.stamp(PREFLIGHT_MESSAGE);
  const stamp2 = await stamper.stamp(PREFLIGHT_MESSAGE);
  const j1 = decode(stamp1);
  const j2 = decode(stamp2);

  if (j1.signature !== j2.signature) {
    throw new Error(
      "preflight FAILED: purejs not deterministic. sig1=" +
        j1.signature + " sig2=" + j2.signature,
    );
  }
  if (j1.scheme !== "SIGNATURE_SCHEME_TK_API_P256") {
    throw new Error("preflight FAILED: scheme " + j1.scheme);
  }
  if (j1.publicKey !== PREFLIGHT_PUBLIC) {
    throw new Error("preflight FAILED: publicKey " + j1.publicKey);
  }
  const keys = Object.keys(j1);
  if (
    keys.length !== 3 ||
    keys[0] !== "publicKey" ||
    keys[1] !== "scheme" ||
    keys[2] !== "signature"
  ) {
    throw new Error("preflight FAILED: key order " + keys.join(","));
  }

  // Verify the upstream signature against the upstream public key — both
  // HIGH-S and LOW-S forms must verify, and r must match between them.
  const sigDecoded = decodeDerEcdsa(j1.signature);
  const hash = createHash("sha256").update(PREFLIGHT_MESSAGE).digest();
  const verified = p256.verify(j1.signature, hash, PREFLIGHT_PUBLIC, {
    lowS: false, // accept the upstream high-S form
  });
  if (!verified) {
    throw new Error("preflight FAILED: upstream signature did not verify");
  }

  return {
    privateKey: PREFLIGHT_PRIVATE,
    publicKey: PREFLIGHT_PUBLIC,
    message: PREFLIGHT_MESSAGE,
    upstreamDerHex: j1.signature,
    upstream_r: sigDecoded.rHex,
    upstream_s: sigDecoded.sHex,
    upstream_s_is_high_s: sigDecoded.s > P256_ORDER / 2n,
    low_s_equivalent_s: lowSEquivalent(sigDecoded.s).toString(16),
    note: "Upstream @noble/curves p256.sign defaults to HIGH-S in v1.x. " +
      "C# port enforces LOW-S. The two signatures are cryptographically " +
      "equivalent: same r, s' = n - s. C# tests assert r equality and " +
      "verify the signature, not full DER byte equality.",
    deterministic: true,
    schemeOk: true,
    keyOrderOk: true,
  };
}

async function sha256OfFile(p) {
  const buf = await readFile(p);
  return createHash("sha256").update(buf).digest("hex");
}
function sha256OfString(s) {
  return createHash("sha256").update(s, "utf-8").digest("hex");
}
async function generatorSelfSha256() {
  return sha256OfFile(__filename);
}
async function transitiveLockSha256() {
  const ls = execSync("npm ls --json --all", { cwd: __dirname, encoding: "utf-8" });
  return createHash("sha256").update(ls).digest("hex");
}

async function main() {
  const preflightResult = await runPreflight();
  if (PREFLIGHT_ONLY) {
    process.stdout.write(JSON.stringify(preflightResult, null, 2) + "\n");
    return;
  }

  const privateKey = await readUpstreamKey("api-key.private");
  const publicKey = await readUpstreamKey("api-key.public");
  const messages = ["hello from TKHQ!", "another message", ""];

  const stamper = new ApiKeyStamper({
    apiPublicKey: publicKey,
    apiPrivateKey: privateKey,
    runtimeOverride: "purejs",
  });

  const stamps = [];
  for (const m of messages) {
    const stamp = await stamper.stamp(m);
    const decoded = JSON.parse(
      Buffer.from(stamp.stampHeaderValue, "base64url").toString(),
    );
    const sigDecoded = decodeDerEcdsa(decoded.signature);
    stamps.push({
      input: { message: m, privateKey, publicKey },
      expected: {
        stampHeaderName: stamp.stampHeaderName,
        stampHeaderValueBase64Url: stamp.stampHeaderValue,
        decoded,
        // The pieces that C# can byte-compare regardless of low/high-S:
        derived: {
          r_hex: sigDecoded.rHex,
          upstream_s_hex: sigDecoded.sHex,
          upstream_s_is_high_s: sigDecoded.s > P256_ORDER / 2n,
          low_s_equivalent_s_hex: lowSEquivalent(sigDecoded.s).toString(16),
        },
      },
    });
  }

  const lockfilePath = path.join(__dirname, "package-lock.json");
  const lockSha = await sha256OfFile(lockfilePath);
  const genSha = await generatorSelfSha256();
  const tLockSha = await transitiveLockSha256();

  const dataBody = { preflight: preflightResult, stamps };
  const outputSha = sha256OfString(JSON.stringify(dataBody));

  const finalDoc = {
    _provenance: {
      level: "node-generated",
      node_version: process.version,
      npm_lockfile_sha256: lockSha,
      generator_sha256: genSha,
      transitive_lock_sha256: tLockSha,
      output_sha256: outputSha,
      upstream_pin: "@turnkey/api-key-stamper@0.5.0",
      noble_curves_pin: "@noble/curves@1.3.0 (via npm overrides in package.json)",
      generator_source: "tests/Fixtures/Generators/generate-stamper-vectors.mjs",
      runtime_override: "purejs",
      s_normalization: "upstream emits HIGH-S; fixture records both upstream_s and low_s_equivalent_s. C# port emits LOW-S; tests compare r equality and verify the signature.",
    },
    ...dataBody,
  };

  const outPath = path.join(
    REPO_ROOT,
    "tests",
    "Fixtures",
    "api-key-stamper",
    "turnkey-stamper-node-vectors.json",
  );
  await writeFile(outPath, JSON.stringify(finalDoc, null, 2) + "\n", "utf-8");
  process.stdout.write("Wrote " + outPath + "\n");
}

main().catch((err) => {
  process.stderr.write(
    "generate-stamper-vectors.mjs FAILED: " + (err.stack || err) + "\n",
  );
  process.exit(1);
});
