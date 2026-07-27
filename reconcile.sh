#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
proj="$root/Reconciliation/Reconciliation.csproj"

a="${1:-$root/Reconciliation/data/List_A.csv}"
b="${2:-$root/Reconciliation/data/List_B.csv}"
out="${3:-$root/Reconciliation/output}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "error: .NET SDK not found. install .NET 8 from https://dotnet.microsoft.com/download/dotnet/8.0" >&2
  exit 1
fi

echo "==> reconciling"
echo "    A:   $a"
echo "    B:   $b"
echo "    out: $out"
dotnet run --project "$proj" -c Release --nologo -- --a "$a" --b "$b" --out "$out"
