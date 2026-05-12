# Phase 02 — CI Build & Test Workflow

## Goal

A GitHub Actions workflow that runs on every push and pull request, confirming the solution
builds and all tests pass. This gives the project a green/red badge and catches regressions
before they reach `main`.

## Trigger

```yaml
on:
  push:
  pull_request:
    branches: [main]
```

Push events run on all branches (useful during development of the pipeline itself). PR events
are restricted to PRs targeting `main`.

## Jobs

### build-and-test

Runs on `ubuntu-latest`.

Steps:
1. `actions/checkout@v4`
2. `actions/setup-dotnet@v4` with `dotnet-version: '8.0.x'`
   — .NET 8 SDK can build net472 via Mono/MSBuild on Linux; .NET 4.7.2 is not available on
   Linux but the `dotnet` CLI cross-targets correctly. Note: building `net472` on Linux requires
   the `mono-devel` package or targeting it via the .NET SDK which handles it through reference
   assemblies. Verify this works; if not, switch the runner to `windows-latest`.
3. `dotnet restore KSPMissionControl.sln`
   — restores all NuGet packages including stubs
4. Build stub projects in Release mode (required before Career can build without KSP):
   ```
   dotnet build lib/stubs/Assembly-CSharp/Assembly-CSharp.csproj -c Release
   dotnet build lib/stubs/UnityEngine.CoreModule/UnityEngine.CoreModule.csproj -c Release
   dotnet build lib/stubs/UnityEngine/UnityEngine.csproj -c Release
   dotnet build lib/stubs/KRPC.Core/KRPC.Core.csproj -c Release
   dotnet build lib/stubs/KRPC.SpaceCenter/KRPC.SpaceCenter.csproj -c Release
   ```
5. `dotnet build KSPMissionControl.sln -c Release --no-restore`
6. `dotnet test KSPMissionControl.sln -c Release --no-build --logger "github-actions"`

### Runner choice

Try `ubuntu-latest` first. If `net472` builds are problematic on Linux (MSBuild reference
assembly issues), switch to `windows-latest`. Most KSP mods use Windows runners for this
reason. Document the choice in the workflow YAML with a comment.

## Caching

Cache the NuGet package cache between runs using `actions/cache@v4` keyed on
`${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}`.

## Files expected to change

| File | Change |
|---|---|
| `.github/workflows/ci.yml` | new |

## Acceptance criteria

1. A push to any branch triggers the workflow
2. All `dotnet test` tests pass (the Shared and MCP test projects both run)
3. The Career project builds without `KspInstallDir` set
4. The workflow completes in under 5 minutes on a warm cache
5. A failed test causes the workflow to fail (non-zero exit)
