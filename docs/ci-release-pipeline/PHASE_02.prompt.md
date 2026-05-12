Read docs/ci-release-pipeline/PHASE_02.md for full context before starting.

You are implementing Phase 2 of the CI/Release Pipeline epic for the KSP Mission Control project.
Work in branch `feature/ci-release-pipeline-phase-2` (create it from `feature/ci-release-pipeline`
after Phase 01's PR has been merged into that branch).

Phase 01 must be complete before starting this phase: the stub projects in `lib/stubs/` must
exist and `Career.csproj` must have conditional references.

## Objective

Create `.github/workflows/ci.yml` — a GitHub Actions workflow that builds the solution and
runs all tests on every push and pull request.

## File to create: `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: windows-latest   # net472 builds are more reliable on Windows

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore
        run: dotnet restore KSPMissionControl.sln

      - name: Build stubs
        run: |
          dotnet build lib/stubs/Assembly-CSharp/Assembly-CSharp.csproj -c Release --no-restore
          dotnet build lib/stubs/UnityEngine.CoreModule/UnityEngine.CoreModule.csproj -c Release --no-restore
          dotnet build lib/stubs/UnityEngine/UnityEngine.csproj -c Release --no-restore
          dotnet build lib/stubs/KRPC.Core/KRPC.Core.csproj -c Release --no-restore
          dotnet build lib/stubs/KRPC.SpaceCenter/KRPC.SpaceCenter.csproj -c Release --no-restore

      - name: Build solution
        run: dotnet build KSPMissionControl.sln -c Release --no-restore

      - name: Test
        run: dotnet test KSPMissionControl.sln -c Release --no-build --verbosity normal
```

## Implementation notes

- Use `windows-latest` for the runner. Building `net472` on Linux via the .NET SDK requires
  Mono reference assemblies that are not always present on GitHub-hosted Ubuntu runners;
  Windows has the full .NET Framework targeting pack available.

- `--no-restore` on build steps is important — restore runs once and subsequent steps reuse
  the cache. Omitting it would trigger restore again and miss the cache.

- `dotnet test` without `--no-build` would rebuild, which is slower and redundant. Use
  `--no-build` since we just ran the build step.

- Stubs must be built before the full solution build because `Career.csproj` HintPaths point
  to stub `bin/Release/net472/` output. If stubs haven't been built, the reference resolution
  silently falls back and the Career build may fail.

- Do NOT set `DOTNET_NOLOGO` or `DOTNET_CLI_TELEMETRY_OPTOUT` — these are cosmetic and not
  necessary.

- Do NOT pin the GitHub Actions runner OS version (e.g. `windows-2022`) — use the `latest`
  alias so the workflow benefits from runner updates automatically.

## Verification (before committing)

Confirm by reading the workflow file back and checking that:
1. The trigger fires on push to all branches
2. The stub build step lists all five stub projects
3. `dotnet test` uses `--no-build`
4. The cache key includes a hash of all `.csproj` files

You cannot run the workflow locally, but you can validate YAML syntax by looking for consistent
indentation (2 spaces) and correct `run:` vs `uses:` keys. Do not add a Python smoke-test step —
that requires a running KSP instance and is out of scope for CI.

## Completion

When the phase is done:
1. Open `docs/ci-release-pipeline/PROGRESS.md`
2. Set Phase 02 Status to `complete`, fill in the completed date
3. Commit `.github/workflows/ci.yml` and the updated PROGRESS.md to `feature/ci-release-pipeline-phase-2`
4. Open a PR targeting `feature/ci-release-pipeline`
5. After the PR is merged, confirm in the GitHub Actions tab that the workflow ran and passed
