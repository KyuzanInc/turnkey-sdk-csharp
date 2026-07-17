# Contributing

Issues and pull requests are welcome.

Before changing behavior, read:

- `docs/compatibility/README.md`
- `docs/adr/README.md`
- `docs/security/threat-model.md`

For code changes, run:

```bash
dotnet restore --locked-mode
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
./tools/compatibility/verify-source-checksums.sh
./tools/compatibility/coverage-map.sh --check
dotnet pack src/turnkey-sdk-csharp.csproj -c Release --no-build --output artifacts
```

Changes to pinned upstream versions must follow the repinning procedure in
`docs/compatibility/upstream-pins.md`. Do not commit credentials, generated
build output, raw AI/session logs, or complete npm package extracts.

Pull-request titles use conventional-commit form, for example
`fix(crypto): handle leading-zero P-256 scalars`.
