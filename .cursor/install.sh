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
# What works on this Linux environment:  dotnet restore / dotnet build.
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
