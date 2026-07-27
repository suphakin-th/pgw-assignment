param(
    [string]$A,
    [string]$B,
    [string]$Out
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$proj = Join-Path $root "Reconciliation\Reconciliation.csproj"

if (-not $A)   { $A   = Join-Path $root "Reconciliation\data\List_A.csv" }
if (-not $B)   { $B   = Join-Path $root "Reconciliation\data\List_B.csv" }
if (-not $Out) { $Out = Join-Path $root "Reconciliation\output" }

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "'.NET SDK not found. install .NET 8 from https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Write-Host "==> reconciling"
Write-Host "    A:   $A"
Write-Host "    B:   $B"
Write-Host "    out: $Out"
& dotnet run --project $proj -c Release --nologo -- --a $A --b $B --out $Out
