using System.Globalization;

namespace Reconciliation.Model;

public sealed class SrcSchema
{
    public required string Name { get; init; }
    public required int RefIdx { get; init; }
    public required int DateIdx { get; init; }
    public required int AmountIdx { get; init; }
    public required int MinCols { get; init; }
    public required string[] DateFmts { get; init; }

    public static readonly SrcSchema ListA = new()
    {
        Name = "A",
        RefIdx = 1,
        DateIdx = 2,
        AmountIdx = 3,
        MinCols = 9,
        DateFmts = new[] { "dd-MM-yyyy" },
    };

    public static readonly SrcSchema ListB = new()
    {
        Name = "B",
        RefIdx = 1,
        DateIdx = 2,
        AmountIdx = 3,
        MinCols = 8,
        DateFmts = new[] { "yyyy-MM-dd" },
    };

    public string? Validate(string[] row)
    {
        if (row.Length < MinCols)
        {
            return $"expected at least {MinCols} columns, got {row.Length}";
        }

        var reference = row[RefIdx].Trim();
        if (reference.Length == 0)
        {
            return "reference is empty";
        }
        if (!reference.All(char.IsDigit))
        {
            return $"reference '{reference}' is not numeric";
        }

        var date = row[DateIdx].Trim();
        if (!DateTime.TryParseExact(date, DateFmts, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
        {
            return $"date '{date}' does not match {string.Join("|", DateFmts)}";
        }

        var amtRaw = row[AmountIdx].Trim().Replace(",", string.Empty);
        if (!decimal.TryParse(amtRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
        {
            return $"amount '{row[AmountIdx]}' is not numeric";
        }
        if (amt < 0)
        {
            return $"amount '{amt}' is negative";
        }

        return null;
    }

    public string Ref(string[] row) => row[RefIdx].Trim();
}
