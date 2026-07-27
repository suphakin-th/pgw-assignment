$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$sln = Join-Path $root "PgwAssignment.sln"
$api = Join-Path $root "PaymentApi\PaymentApi.csproj"

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "'.NET SDK not found. install .NET 8 from https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

$ver = & dotnet --version
Write-Host "dotnet $ver"
if (-not $ver.StartsWith("8.")) {
    Write-Warning ".NET 8 recommended, found $ver"
}

Write-Host "==> restore"
& dotnet restore $sln

Write-Host "==> build"
& dotnet build $sln -c Release --nologo

Write-Host "==> test"
& dotnet test $sln -c Release --nologo -v q

Write-Host "==> starting Payment API on http://localhost:5080 (Ctrl+C to stop)"
Write-Host "    swagger: http://localhost:5080/swagger"
Write-Host "    api key: pgw-demo-key-001"
$env:ASPNETCORE_URLS = "http://localhost:5080"
& dotnet run --project $api -c Release --nologo
