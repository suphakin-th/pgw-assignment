#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
sln="$root/PgwAssignment.sln"
api="$root/PaymentApi/PaymentApi.csproj"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "error: .NET SDK not found. install .NET 8 from https://dotnet.microsoft.com/download/dotnet/8.0" >&2
  exit 1
fi

ver="$(dotnet --version)"
echo "dotnet $ver"
case "$ver" in
  8.*) ;;
  *) echo "warning: .NET 8 recommended, found $ver" >&2 ;;
esac

echo "==> restore"
dotnet restore "$sln"

echo "==> build"
dotnet build "$sln" -c Release --nologo

echo "==> test"
dotnet test "$sln" -c Release --nologo -v q

echo "==> starting Payment API on http://localhost:5080 (Ctrl+C to stop)"
echo "    swagger: http://localhost:5080/swagger"
echo "    api key: pgw-demo-key-001"
ASPNETCORE_URLS="http://localhost:5080" dotnet run --project "$api" -c Release --nologo
