# PGW Assignment

Two deliverables, both on .NET 8.

- **[PaymentApi](PaymentApi/README.md)** — Assignment 1. A mini payment gateway
  API (`POST /api/v1/pay`) with schema validation, api-key auth, PCI-safe
  handling of card data, amount-driven decisioning, idempotency, and SQLite
  storage.
- **[Reconciliation](Reconciliation/README.md)** — Assignment 2. A streaming
  batch tool that reconciles two financial CSV datasets and emits matched /
  missing reports, built to run on files of millions of rows with bounded memory.

## Prerequisites

Install the .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

Verify:

```bash
dotnet --version
```

## Quick start (one command)

The helper scripts check the SDK, restore, build, run all tests, then start the
Payment API on `http://localhost:5080`.

macOS / Linux / Git Bash:

```bash
./run.sh
```

Windows PowerShell:

```powershell
./run.ps1
```

To run the reconciliation on the bundled sample data:

```bash
./reconcile.sh                       # defaults to Reconciliation/data
./reconcile.sh <listA.csv> <listB.csv> <outDir>
```

```powershell
./reconcile.ps1
./reconcile.ps1 -A <listA.csv> -B <listB.csv> -Out <outDir>
```

## Build everything

```bash
dotnet build PgwAssignment.sln
```

## Run

Payment API:

```bash
cd PaymentApi
dotnet run
```

Reconciliation:

```bash
cd Reconciliation
dotnet run -- --a data/List_A.csv --b data/List_B.csv --out output
```

## Tests

```bash
dotnet test PgwAssignment.sln
```

22 xUnit tests across both suites: `PaymentApi.Tests` (amount decisioning,
expiry validation) and `Reconciliation.Tests` (CSV parsing, schema validation,
end-to-end reconcile).

See each project's README for endpoint details, curl samples, the Postman
collection, and design notes.
