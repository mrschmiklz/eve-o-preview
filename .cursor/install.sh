#!/usr/bin/env bash
#
# Cloud Agent bootstrap for EVE-O Preview.
#
# This repository is a Windows-only .NET 8 desktop application (WPF + WinForms,
# targeting net8.0-windows8.0). Cursor Cloud Agents run on Linux, so this script:
#
#   1. Installs the .NET 8 SDK (idempotent).
#   2. Puts the SDK on PATH for future agent shells.
#   3. Performs a cross-compile build of the whole solution using
#      EnableWindowsTargeting=true, which verifies the code compiles.
#
# What works on this Linux environment:  dotnet restore / dotnet build, and
#                                         cross-publishing a runnable Windows
#                                         .exe asset (scripts/publish-windows.sh).
# What still requires Windows:           running the app, the xUnit tests, and
#                                         the --smoke-test. Those need the
#                                         Microsoft.WindowsDesktop.App runtime,
#                                         which only exists on Windows. Use a
#                                         Windows machine or CI for those:
#                                           scripts/build-and-test.ps1
#                                           .github/workflows/ci.yml (windows-2022)
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_DIR="$HOME/.dotnet"
DOTNET_CHANNEL="8.0"

# 0. Ensure system prerequisites (curl for the SDK download; zip/unzip for
#    scripts/publish-windows.sh). Idempotent and best-effort.
ensure_pkgs() {
  local missing=()
  local c
  for c in curl zip unzip; do
    command -v "$c" >/dev/null 2>&1 || missing+=("$c")
  done
  [ ${#missing[@]} -eq 0 ] && return 0
  echo "==> Installing system packages: ${missing[*]}"
  if command -v sudo >/dev/null 2>&1; then
    sudo apt-get update -qq && sudo apt-get install -y -qq "${missing[@]}"
  elif [ "$(id -u)" = "0" ]; then
    apt-get update -qq && apt-get install -y -qq "${missing[@]}"
  else
    echo "WARNING: cannot install ${missing[*]} (no sudo/root)." >&2
  fi
}
ensure_pkgs

# 1. Install the .NET 8 SDK (idempotent).
if [ ! -x "$DOTNET_DIR/dotnet" ]; then
  echo "==> Installing .NET SDK ${DOTNET_CHANNEL} into ${DOTNET_DIR}"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_DIR"
else
  echo "==> .NET SDK already present at ${DOTNET_DIR}"
fi

export DOTNET_ROOT="$DOTNET_DIR"
export PATH="$DOTNET_DIR:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

# 2. Persist tooling on PATH for future agent shells (idempotent).
MARKER="# >>> eve-o-preview dotnet >>>"
if ! grep -qF "$MARKER" "$HOME/.bashrc" 2>/dev/null; then
  {
    echo ""
    echo "$MARKER"
    echo "export DOTNET_ROOT=\"$DOTNET_DIR\""
    echo "export PATH=\"$DOTNET_DIR:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
    echo "export DOTNET_NOLOGO=1"
    echo "# <<< eve-o-preview dotnet <<<"
  } >> "$HOME/.bashrc"
fi

# 3. Restore + build (cross-compile Windows targets on Linux).
echo "==> Building solution (EnableWindowsTargeting=true)"
cd "$REPO_ROOT"
dotnet build src/EVE-O-Preview.sln -c Release -p:EnableWindowsTargeting=true --verbosity minimal

echo "==> Environment ready. dotnet $(dotnet --version) installed."
