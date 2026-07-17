# Pinned upstream sources

This directory contains readable source snapshots from the Apache-2.0 licensed
[`tkhq/sdk`](https://github.com/tkhq/sdk) repository at the commits documented
in [`docs/compatibility/upstream-pins.md`](../../docs/compatibility/upstream-pins.md).

The files are compatibility-verification inputs only. They are not compiled
into the .NET library or packed into the NuGet package.

Each package directory contains:

- `ts-source/` — TypeScript source at the pinned Git commit;
- `package.json` — metadata from the corresponding published npm package.

`package-checksums.txt` records the controlling npm tarball hashes.
`source-file-checksums.txt` records every retained TypeScript source byte.

The fixture files named `api-key.private` and related keys are public upstream
test vectors. They are not live credentials and must never be used in
production. Secret scanners should allowlist only these exact paths or hashes,
not broad key patterns.

License terms are in [`LICENSES/Apache-2.0.txt`](../../LICENSES/Apache-2.0.txt)
and attribution is in [`NOTICE`](../../NOTICE).
