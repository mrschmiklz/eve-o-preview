#!/usr/bin/env bash
#
# Cross-publish a runnable Windows build of EVE-O Preview from Linux.
#
# EVE-O Preview is a Windows-only WPF/WinForms app, but a Cursor Cloud Agent
# (Linux) can cross-compile a Windows executable with EnableWindowsTargeting.
# This produces the same downloadable "asset" the GitHub release workflow makes
# (see .github/workflows/release.yml), so you can download it and run it on
# Windows without a Windows build machine.
#
# Usage:
#   scripts/publish-windows.sh [-s|--self-contained] [-v <version>] [-o <outdir>]
#
#   (default)              Framework-dependent single-file exe (~2-3 MB).
#                          Requires the .NET 8 Desktop Runtime on the target PC:
#                          https://dotnet.microsoft.com/download/dotnet/8.0
#   -s, --self-contained   Bundle the runtime (~160 MB). Runs on any Windows
#                          x64 machine with no .NET install required.
#   -v <version>           Version stamped into the assembly/zip name.
#                          Defaults to the csproj ApplicationVersion.
#   -o <outdir>            Output directory for the zip. Default: dist/
#
# Output: dist/Release-<version>-Windows.zip  (unzip and run EVE-O-Preview.exe)
#
set -euo pipefail

SELF_CONTAINED="false"
VERSION=""
OUT_DIR=""

while [ $# -gt 0 ]; do
  case "$1" in
    -s|--self-contained) SELF_CONTAINED="true"; shift ;;
    -v|--version) VERSION="$2"; shift 2 ;;
    -o|--out) OUT_DIR="$2"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 2 ;;
  esac
done

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

# Make the SDK available even in a non-login shell.
if ! command -v dotnet >/dev/null 2>&1; then
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

CSPROJ="src/Eve-O-Preview/Eve-O-Preview.csproj"

# Default the version to the csproj ApplicationVersion.
if [ -z "$VERSION" ]; then
  VERSION="$(grep -oPm1 '(?<=<ApplicationVersion>)[^<]+' "$CSPROJ" || true)"
  [ -z "$VERSION" ] && VERSION="dev"
fi

[ -z "$OUT_DIR" ] && OUT_DIR="$REPO_ROOT/dist"
mkdir -p "$OUT_DIR"

STAGE="$OUT_DIR/Eve-O-Preview-$VERSION-Windows"
ZIP="$OUT_DIR/Release-$VERSION-Windows.zip"
rm -rf "$STAGE" "$ZIP"

echo "==> Publishing win-x64 (self-contained=$SELF_CONTAINED, version=$VERSION)"
dotnet publish "$CSPROJ" \
  -c Release \
  -r win-x64 \
  --self-contained "$SELF_CONTAINED" \
  -p:EnableWindowsTargeting=true \
  -p:AssemblyVersion="$VERSION" \
  -p:FileVersion="$VERSION" \
  -o "$STAGE" \
  --verbosity minimal

echo "==> Creating $ZIP"
( cd "$STAGE" && zip -qr "$ZIP" . )

echo
echo "Asset ready: $ZIP"
echo "  size: $(du -h "$ZIP" | cut -f1)"
echo "  contents:"
unzip -l "$ZIP" | sed 's/^/    /'
echo
echo "Download the zip, unzip on Windows, and run EVE-O-Preview.exe."
if [ "$SELF_CONTAINED" != "true" ]; then
  echo "Framework-dependent build: install the .NET 8 Desktop Runtime if prompted:"
  echo "  https://dotnet.microsoft.com/download/dotnet/8.0"
fi
