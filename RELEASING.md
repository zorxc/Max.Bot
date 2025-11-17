# Release Guide

This checklist keeps the release flow deterministic and mirrors the Telegram.Bot / VkNet cadence.

## 1. Pre-flight

- Ensure every user-facing change is documented in `CHANGELOG.md` under `[Unreleased]`.
- Confirm `src/Max.Bot/Max.Bot.csproj` has the correct `<Version>` / `<PackageId>` metadata.
- Verify secrets: `NUGET_API_KEY` (GitHub), `MAX_BOT_TOKEN` (local testing) stay out of source control.

## 2. Local verification

```powershell
dotnet format Max.Bot.sln --verify-no-changes
dotnet format analyzers Max.Bot.sln --verify-no-changes --no-restore
dotnet test Max.Bot.sln -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=\"cobertura%2copencover\" /p:CoverletOutput=TestResults/Coverage/ /p:Threshold=60 /p:ThresholdType=line /p:ThresholdStat=total
dotnet pack src/Max.Bot/Max.Bot.csproj -c Release /p:ContinuousIntegrationBuild=true
```

Inspect the generated `.nupkg`/`.snupkg` locally (e.g., `nuget package -Expanded` or `nuget locals`) to ensure README and XML docs are embedded.

## 3. Tagging

1. Bump `Version` in `Max.Bot.csproj` if needed (`0.3.0-alpha`, etc.).
2. Commit the changes and tag:

```powershell
git tag v0.3.0-alpha
git push origin v0.3.0-alpha
```

## 4. GitHub Actions

- Tag push triggers `.github/workflows/release.yml`.
- Workflow steps: restore → format → analyzers → build → tests (with coverage) → `dotnet pack` → upload artifacts → `dotnet nuget push` (skipping duplicates).
- Monitor the run at `https://github.com/MaxBotNet/MaxBotNet/actions`. The run must be green before announcing the release.

## 5. Post-publish validation

1. Confirm package visibility on NuGet.org.
2. Smoke test the package in a clean project:

```powershell
dotnet new console -n MaxBotNet.ReleaseSmoke
cd MaxBotNet.ReleaseSmoke
dotnet add package MaxMessenger.Bot --prerelease
```

3. Create a GitHub Release that references the changelog entry and attaches any notable artifacts if necessary.

Following these steps keeps parity with the Telegram.Bot / VkNet release discipline and prevents drift between libraries, docs, and shipped packages.


