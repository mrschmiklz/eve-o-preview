#Requires -Version 5.1
<#
.SYNOPSIS
  Push a tag and create a GitHub Release so Actions builds and uploads the zip assets.

.DESCRIPTION
  Workflow:
    1. Push your code changes to GitHub (this script checks the branch is pushed).
    2. Run this script with a version tag.
    3. Builds Release-<tag>-Windows.zip locally and uploads it to the GitHub release.

  GitHub Actions may also build assets on upstream; on this fork the local zip upload
  is the reliable path for Windows test builds.

  Download from: https://github.com/mrschmiklz/eve-o-preview/releases

.PARAMETER Tag
  Release tag, e.g. 8.0.2.0-test1

.PARAMETER Notes
  Release notes shown on GitHub.

.PARAMETER Prerelease
  Mark as pre-release (recommended for test builds on your fork).

.PARAMETER SkipLocalBuild
  Skip the local Windows build sanity check before publishing.

.EXAMPLE
  .\scripts\publish-release.ps1 -Tag 8.0.2.0-test1 -Notes "Testing thumbnail fix"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Tag,

    [string]$Notes = "Test build from local fork.",

    [switch]$Prerelease = $true,

    [switch]$SkipLocalBuild
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $RepoRoot

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

Write-Step "Checking prerequisites"
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is required. Install from https://cli.github.com/"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK is required for the local build check."
}

$Branch = (git rev-parse --abbrev-ref HEAD).Trim()
$Upstream = git rev-parse --abbrev-ref "@{u}" 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Current branch '$Branch' has no upstream. Push it first: git push -u origin $Branch"
}

$LocalHead = (git rev-parse HEAD).Trim()
$RemoteHead = (git rev-parse "@{u}").Trim()
if ($LocalHead -ne $RemoteHead) {
    throw "Branch '$Branch' is not pushed. Commit and run: git push origin $Branch"
}

if (git tag --list $Tag) {
    throw "Tag '$Tag' already exists locally. Choose a new tag name."
}

if (-not $SkipLocalBuild) {
    Write-Step "Local Windows build check"
    dotnet build "src\Eve-O-Preview\Eve-O-Preview.csproj" `
        -c Release `
        -p:AssemblyVersion="$Tag" `
        -p:FileVersion="$Tag" `
        --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Local build failed. Fix errors or pass -SkipLocalBuild to bypass."
    }
    Write-Host "Local build succeeded." -ForegroundColor Green
}

Write-Step "Creating and pushing tag '$Tag'"
git -c user.name="mrschmiklz" -c user.email="mrschmiklz@users.noreply.github.com" tag -a $Tag -m $Notes
git push origin $Tag

Write-Step "Creating GitHub release (triggers Actions to build zips)"
$releaseArgs = @(
    "release", "create", $Tag,
    "--repo", "mrschmiklz/eve-o-preview",
    "--title", "EVE-O Preview $Tag",
    "--notes", $Notes
)
if ($Prerelease) {
    $releaseArgs += "--prerelease"
}

gh @releaseArgs

if (-not $SkipLocalBuild) {
    Write-Step "Publishing Windows release zip"
    $outDir = Join-Path $RepoRoot "dist\Eve-O-Preview-$Tag-Windows"
    $zipPath = Join-Path $RepoRoot "dist\Release-$Tag-Windows.zip"

    if (Test-Path $outDir) {
        Remove-Item -Recurse -Force $outDir
    }
    if (Test-Path $zipPath) {
        Remove-Item -Force $zipPath
    }
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    dotnet publish "src\Eve-O-Preview\Eve-O-Preview.csproj" `
        -c Release `
        -o $outDir `
        -p:AssemblyVersion="$Tag" `
        -p:FileVersion="$Tag" `
        --self-contained false
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed."
    }

    Compress-Archive -Path (Join-Path $outDir "*") -DestinationPath $zipPath -Force

    gh release upload $Tag $zipPath --repo "mrschmiklz/eve-o-preview" --clobber
    Write-Host "Uploaded Release-$Tag-Windows.zip" -ForegroundColor Green
}

Write-Step "Done"
Write-Host @"

Release created: https://github.com/mrschmiklz/eve-o-preview/releases/tag/$Tag

Windows asset:
  - Release-$Tag-Windows.zip

On your other PC:
  1. Open the release URL above
  2. Download Release-$Tag-Windows.zip
  3. Unzip and run EVE-O-Preview.exe
  4. Install .NET 8 Desktop Runtime if prompted:
     https://dotnet.microsoft.com/download/dotnet/8.0

"@ -ForegroundColor Green
