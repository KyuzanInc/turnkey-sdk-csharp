# Upstream compatibility pins

This SDK targets fixed releases of the Turnkey TypeScript packages. The npm
package version and tarball SHA-256 are the wire-format compatibility pin. A
readable TypeScript snapshot from the corresponding Git commit is committed
under [`tests/UpstreamSources/`](../../tests/UpstreamSources/) for source
mapping, coverage enumeration, and fixture provenance.

| C# implementation | npm package | Version | npm tarball SHA-256 | Git commit |
|---|---|---:|---|---|
| `src/Encoding.cs` | `@turnkey/encoding` | 0.6.0 | `2cf9e6ee1f47ac7e3cc3e644cdb0e3e112c906a6ea1af737777f4658b73fb7bc` | `60a997f4c52ac5f98bdd285af934f02699b88bff` |
| `src/Crypto.cs`, `src/CryptoConstants.cs` | `@turnkey/crypto` | 2.8.8 | `75115706fccc29c664f9f918d9a0c2c1798eb03f261cb5b0c186c75663cf79d3` | `b35dc642bd7c1728f97acd43d4cba66976b65084` |
| `src/ApiKeyStamper.cs` | `@turnkey/api-key-stamper` | 0.5.0 | `962a2d22c7c40240f05be98533769b37ab7dad7dbb5abec762c41007233d02bd` | `b711befbb88ec522452dbdac68f0e98762be10dd` |
| `src/Http.cs` | `@turnkey/http` | 3.16.0 | `d849f2156633f63062c52067785df1ce33eac0659044df932862c5d6de9dbdaf` | `8def9ba521233137437ac7294693a4ae0a0d14da` |

`src/TurnkeyJsonContext.cs` is C#-specific infrastructure covering DTOs from
multiple packages and therefore has no single upstream source file.

## Source hierarchy

1. Published npm tarball bytes, identified by version and SHA-256.
2. TypeScript source at the pinned Git commit, used for readable comparison.
3. C# adaptation references, which may help implementation but are never the
   compatibility authority.

If published JavaScript and TypeScript source disagree, the published npm
tarball controls observable compatibility.

## Repository retention policy

The repository intentionally does not commit complete npm tarball extracts or
compiled `dist/` source maps. They are reproducible from the version and hash
above and added substantial generated content with no build or test consumer.
The repository retains:

- readable TypeScript source and upstream tests;
- published package metadata (`package.json`);
- package and source checksum manifests;
- the Apache-2.0 license and NOTICE attribution.

See [ADR-0001](../adr/0001-pinned-upstream-compatibility.md).

## Re-pinning procedure

When changing a pin:

1. Run `npm pack <package>@<version>` in a temporary directory.
2. Verify and record the tarball SHA-256 in this file and
   `tests/UpstreamSources/package-checksums.txt`.
3. Update the exact package-version lists in
   `tools/compatibility/verify-source-checksums.sh` and
   `tools/compatibility/tests/source-checksums-test.sh`.
4. Update the matching `.github/workflows/upstream-drift.yml` matrix entry:
   `package`, `git_tag_sha`, and `tarball_dir` are mandatory; update `owner`
   and `repo` if the upstream location changed.
5. Resolve and record the matching upstream Git commit.
6. Refresh the matching `tests/UpstreamSources/<package-version>/ts-source/`
   tree and `package.json`.
7. Regenerate `tests/UpstreamSources/source-file-checksums.txt` and run
   `./tools/compatibility/tests/source-checksums-test.sh` followed by
   `./tools/compatibility/verify-source-checksums.sh`.
8. Re-run `./tools/compatibility/coverage-map.sh --check`, fixture generators,
   the full .NET test suite, and an independent code review of affected files.
9. Update `CHANGELOG.md` and compatibility documentation.
10. After the repin is merged, run the `Upstream drift detection` workflow
    manually on `main` and confirm every package-matrix job succeeds. This
    verifies the periodic monitor uses the new package path and Git commit.

Do not update only the readable source snapshot: a repin is incomplete until
the npm tarball hash, Git commit, tests, and documentation agree.
