#Requires -Version 5.1
<#
.SYNOPSIS
  Build Release, run unit tests, and smoke-test EVE-O-Preview.exe.

.DESCRIPTION
  Produces artifacts/verify-report.json for humans and Cursor agent loops.

  Typical local use:
    .\scripts\build-and-test.ps1

  With mock EVE clients (ExeFile.exe from Eve-O-Mock):
    .\scripts\build-and-test.ps1 -WithMockClients

.EXAMPLE
  .\scripts\build-and-test.ps1 -ReportPath artifacts/verify-report.json
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$SkipUnitTests,
    [switch]$SkipSmokeTest,
    [switch]$WithMockClients,
    [int]$SmokeTestSeconds = 8,
    [string]$ReportPath = "artifacts/verify-report.json"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $RepoRoot

$AppProject = Join-Path $RepoRoot "src\Eve-O-Preview\Eve-O-Preview.csproj"
$MockProject = Join-Path $RepoRoot "src\Eve-O-Mock\Eve-O-Mock.csproj"
$TestProject = Join-Path $RepoRoot "tests\Eve-O-Preview.Tests\Eve-O-Preview.Tests.csproj"
$ExampleConfig = Join-Path $RepoRoot "config\EVE-O-Preview.example.json"
$ExePath = Join-Path $RepoRoot "bin\net8.0-windows8.0\EVE-O-Preview.exe"
$MockExePath = Join-Path $RepoRoot "src\Eve-O-Mock\bin\Release\net8.0-windows\ExeFile.exe"

$startedAt = Get-Date
$report = [ordered]@{
    startedAt = $startedAt.ToString("o")
    repoRoot = $RepoRoot
    success = $false
    steps = @()
    failures = @()
}

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Add-Step([string]$Name, [bool]$Passed, [string]$Detail = "") {
    $report.steps += [ordered]@{
        name = $Name
        passed = $Passed
        detail = $Detail
    }
    if (-not $Passed) {
        $report.failures += "$Name`: $Detail"
    }
}

function Stop-AppProcesses {
    Get-Process -Name "EVE-O-Preview", "ExeFile" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}

try {
    Write-Step "Stopping running test processes"
    Stop-AppProcesses
    Add-Step "stop-processes" $true "Stopped EVE-O-Preview and ExeFile if running"

    if (-not $SkipBuild) {
        Write-Step "Building Release"
        dotnet build $AppProject -c Release --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            throw "Release build failed."
        }

        if ($WithMockClients) {
            dotnet build $MockProject -c Release --verbosity minimal
            if ($LASTEXITCODE -ne 0) {
                throw "Eve-O-Mock build failed."
            }
        }

        Add-Step "build" $true "Release build succeeded"
    }
    else {
        Add-Step "build" $true "Skipped (-SkipBuild)"
    }

    if (-not (Test-Path $ExePath)) {
        throw "Missing executable: $ExePath"
    }

    if (-not $SkipUnitTests) {
        Write-Step "Running unit tests"
        dotnet test $TestProject -c Release --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            throw "Unit tests failed."
        }
        Add-Step "unit-tests" $true "All unit tests passed"
    }
    else {
        Add-Step "unit-tests" $true "Skipped (-SkipUnitTests)"
    }

    if (-not $SkipSmokeTest) {
        Write-Step "Smoke testing EVE-O-Preview.exe"
        $smokeDir = Join-Path $env:TEMP ("eve-o-preview-smoke-" + [Guid]::NewGuid().ToString("N"))
        New-Item -ItemType Directory -Path $smokeDir | Out-Null

        try {
            Copy-Item $ExampleConfig (Join-Path $smokeDir "EVE-O-Preview.json") -Force
            $logPath = Join-Path $smokeDir "EVE-O-Preview.log"

            $mockProcesses = @()
            if ($WithMockClients) {
                if (-not (Test-Path $MockExePath)) {
                    throw "Missing mock client executable: $MockExePath"
                }

                for ($i = 0; $i -lt 2; $i++) {
                    $mockProcesses += Start-Process -FilePath $MockExePath -PassThru
                }
                Start-Sleep -Seconds 2
            }

            $process = Start-Process `
                -FilePath $ExePath `
                -ArgumentList "--smoke-test" `
                -WorkingDirectory $smokeDir `
                -PassThru

            $timeoutMs = ($SmokeTestSeconds + 10) * 1000
            if (-not $process.WaitForExit($timeoutMs)) {
                $process.Kill()
                throw "Smoke test timed out after $($timeoutMs / 1000)s."
            }

            if ($process.ExitCode -ne 0) {
                throw "Smoke test exited with code $($process.ExitCode)."
            }

            if (Test-Path $logPath) {
                $logTail = Get-Content $logPath -Tail 20 -ErrorAction SilentlyContinue
                throw "Crash log created during smoke test:`n$($logTail -join [Environment]::NewLine)"
            }

            Add-Step "smoke-test" $true "Exe ran --smoke-test and exited cleanly ($SmokeTestSeconds s window)"
        }
        finally {
            foreach ($mock in $mockProcesses) {
                if ($mock -and -not $mock.HasExited) {
                    Stop-Process -Id $mock.Id -Force -ErrorAction SilentlyContinue
                }
            }
            Remove-Item -Recurse -Force $smokeDir -ErrorAction SilentlyContinue
        }
    }
    else {
        Add-Step "smoke-test" $true "Skipped (-SkipSmokeTest)"
    }

    $report.success = ($report.failures.Count -eq 0)
}
catch {
    $report.success = $false
    if ($report.failures.Count -eq 0) {
        $report.failures += $_.Exception.Message
    }
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    Stop-AppProcesses
    $report.finishedAt = (Get-Date).ToString("o")
    $report.durationSeconds = [math]::Round(((Get-Date) - $startedAt).TotalSeconds, 1)

    $reportDirectory = Split-Path -Parent $ReportPath
    if ($reportDirectory -and -not (Test-Path $reportDirectory)) {
        New-Item -ItemType Directory -Path $reportDirectory | Out-Null
    }

    $report | ConvertTo-Json -Depth 6 | Set-Content -Path $ReportPath -Encoding UTF8
    Write-Host "`nReport: $ReportPath" -ForegroundColor Gray
}

if ($report.success) {
    Write-Host "`nVERIFY PASSED" -ForegroundColor Green
    exit 0
}

Write-Host "`nVERIFY FAILED" -ForegroundColor Red
foreach ($failure in $report.failures) {
    Write-Host " - $failure" -ForegroundColor Red
}
exit 1
