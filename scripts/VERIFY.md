# Build → Test → Refine loop

This repo includes a local verify pipeline and CI that build Release, run unit tests, and smoke-test `EVE-O-Preview.exe`.

## Quick start

```powershell
.\scripts\build-and-test.ps1
```

With mock EVE clients (`ExeFile.exe` from `Eve-O-Mock`):

```powershell
.\scripts\build-and-test.ps1 -WithMockClients
```

On success you get `VERIFY PASSED` and a JSON report at `artifacts/verify-report.json`.

## What it checks

| Step | What it does |
|------|----------------|
| **Build** | `dotnet build` Release for the main app (and mock clients if requested) |
| **Unit tests** | xUnit tests in `tests/Eve-O-Preview.Tests` |
| **Smoke test** | Runs `EVE-O-Preview.exe --smoke-test` in an isolated temp folder with example config; fails if the process crashes, times out, or writes `EVE-O-Preview.log` |

## Cursor agent refine loop

After making changes, ask the agent:

> Run `.\scripts\build-and-test.ps1`, read `artifacts/verify-report.json`, fix any failures, and repeat until VERIFY PASSED.

Or use a fixed interval loop in chat:

```
/loop 10m Run scripts/build-and-test.ps1, read artifacts/verify-report.json, fix failures, rebuild until VERIFY PASSED.
```

## CI

GitHub Actions workflow `.github/workflows/ci.yml` runs the same script on push/PR to `develop` / `main`. Failed runs upload `verify-report.json` as an artifact.

## Flags

| Flag | Purpose |
|------|---------|
| `-SkipBuild` | Reuse existing build output |
| `-SkipUnitTests` | Smoke test only |
| `-SkipSmokeTest` | Build + unit tests only |
| `-WithMockClients` | Start two `ExeFile.exe` mock windows during smoke test |
| `-SmokeTestSeconds` | How long the app runs before auto-exit (default 8) |
| `-ReportPath` | Custom JSON report path |

## Release publishing

After verify passes locally:

```powershell
.\scripts\publish-release.ps1 -Tag 8.0.x.x -Notes "Your notes"
```

`publish-release.ps1` already runs a Release build check before tagging.
