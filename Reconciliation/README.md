# Reconciliation Utility

A streaming batch tool (.NET 8 console) that compares two financial datasets and
reports discrepancies on a shared unique reference.

## Unique reference

The reference is **List A's `Invoice Number`** matched against **List B's
`Order Number`** (both the numeric id in column index 1). List A and List B use
different date formats (`dd-MM-yyyy` vs `yyyy-MM-dd`) and List A carries an extra
masked card column; each source has its own schema in `SrcSchema`.

> Note on the spec: the brief's prose says the reference is "List A's Order
> Number and List B's Invoice Number", but the actual supplied files have the
> column names reversed — List A's id column is headed `Invoice Number` and List
> B's is headed `Order Number`. The tool matches on the physical id column of
> each file (index 1), which is the intended comparison regardless of the header
> label.

## Outputs

Three result files plus two validation reports are written to the output dir:

| file                  | contents                                          |
|-----------------------|---------------------------------------------------|
| `Matched_Records.csv` | references present in **both** A and B            |
| `Missing_In_B.csv`    | in A but **absent from B**                        |
| `Missing_In_A.csv`    | in B but **absent from A**                        |
| `Invalid_A.csv`       | rows from A that failed validation (with reason)  |
| `Invalid_B.csv`       | rows from B that failed validation (with reason)  |

For the provided sample data the expected result is **17 matched, 7 missing in
B, 7 missing in A**.

## How to run

Requires the .NET 8 SDK.

```bash
cd Reconciliation
dotnet run -- --a data/List_A.csv --b data/List_B.csv --out output
```

Arguments:

- `--a <path>` list A csv (required)
- `--b <path>` list B csv (required)
- `--out <dir>` output directory (default `output`)

Console output prints the row counts, invalid counts, match/mismatch totals, and
elapsed time.

## Design for high-volume data

The tool never loads a whole file into memory. It reads with a buffered
`StreamReader` and a hand-rolled streaming CSV parser (`CsvRdr.Read`) that yields
one row at a time and correctly handles quoted fields with embedded commas
(e.g. `"3,900.00"`).

Reconciliation runs in three streaming passes:

1. Stream List B, keeping only the **set of reference keys** in memory
   (`Dictionary<string,bool>`), not the full rows. Keys are small, so this scales
   to millions of rows.
2. Stream List A once: each valid row is either written to `Matched_Records.csv`
   (key exists in B) or `Missing_In_B.csv`, and its key is flagged as seen.
3. Stream List B again: any key never flagged in pass 2 is written to
   `Missing_In_A.csv`.

Memory use is bounded by the number of distinct references in B, independent of
row width or total file size.

## Validation and error handling

Every data row is validated before use (`SrcSchema.Validate`):

- reference present and numeric
- transaction date parses against the source's expected format
- amount is numeric (thousands separators tolerated) and non-negative
- minimum column count

Invalid rows are not fatal — they are counted, logged to stderr, and routed to
`Invalid_A.csv` / `Invalid_B.csv` with a reason, so a few bad rows never stop the
batch. Fatal conditions (missing input file, IO error) exit with a non-zero
status and a clear message.

## Tests

```bash
dotnet test ../Reconciliation.Tests/Reconciliation.Tests.csproj
```

Covers the CSV parser (quoted fields with embedded commas, escaped quotes,
missing trailing newline), per-source schema validation, and an end-to-end
reconcile that asserts the matched / missing-in-A / missing-in-B split and that
invalid rows are routed aside without failing the batch.

## Layout

```
Reconciliation/
  Csv/CsvRdr.cs           streaming csv read/write
  Model/Rec.cs            row / bad-row models
  Model/SrcSchema.cs      per-source schema + validation
  Recon/ReconEngine.cs    three-pass streaming reconciler
  Program.cs              cli entrypoint
  data/List_A.csv, List_B.csv
Reconciliation.Tests/     xunit: parser, validation, end-to-end reconcile
```
