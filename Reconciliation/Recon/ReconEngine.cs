using Reconciliation.Csv;
using Reconciliation.Model;

namespace Reconciliation.Recon;

public sealed class ReconStats
{
    public long ARows;
    public long BRows;
    public long AInvalid;
    public long BInvalid;
    public long Matched;
    public long MissingInB;
    public long MissingInA;
}

public sealed class ReconEngine
{
    private readonly string _pathA;
    private readonly string _pathB;
    private readonly string _outDir;
    private readonly Action<string> _log;

    public ReconEngine(string pathA, string pathB, string outDir, Action<string> log)
    {
        _pathA = pathA;
        _pathB = pathB;
        _outDir = outDir;
        _log = log;
    }

    public ReconStats Run()
    {
        Directory.CreateDirectory(_outDir);
        var stats = new ReconStats();

        var bKeys = ScanBKeys(stats);
        _log($"list B unique references: {bKeys.Count}");

        WriteMatchedAndMissingInB(bKeys, stats);
        WriteMissingInA(bKeys, stats);

        return stats;
    }

    private Dictionary<string, bool> ScanBKeys(ReconStats stats)
    {
        var keys = new Dictionary<string, bool>(StringComparer.Ordinal);
        var schema = SrcSchema.ListB;

        using var badB = NewWriter("Invalid_B.csv");

        foreach (var row in Rows(_pathB, schema, badB, isHeaderDone =>
                 {
                     stats.BRows++;
                 }, () => stats.BInvalid++))
        {
            keys[schema.Ref(row)] = false;
        }

        return keys;
    }

    private void WriteMatchedAndMissingInB(Dictionary<string, bool> bKeys, ReconStats stats)
    {
        var schema = SrcSchema.ListA;

        using var matched = NewWriter("Matched_Records.csv");
        using var missB = NewWriter("Missing_In_B.csv");
        using var badA = NewWriter("Invalid_A.csv");

        var header = HeaderOf(_pathA);
        if (header is not null)
        {
            var withSrc = header.Concat(new[] { "Source" }).ToArray();
            matched.WriteLine(CsvRdr.Line(withSrc));
            missB.WriteLine(CsvRdr.Line(header));
        }

        foreach (var row in Rows(_pathA, schema, badA, _ => stats.ARows++, () => stats.AInvalid++))
        {
            var reference = schema.Ref(row);
            if (bKeys.ContainsKey(reference))
            {
                bKeys[reference] = true;
                stats.Matched++;
                matched.WriteLine(CsvRdr.Line(row.Concat(new[] { "A" })));
            }
            else
            {
                stats.MissingInB++;
                missB.WriteLine(CsvRdr.Line(row));
            }
        }
    }

    private void WriteMissingInA(Dictionary<string, bool> bKeys, ReconStats stats)
    {
        var schema = SrcSchema.ListB;

        using var missA = NewWriter("Missing_In_A.csv");
        var header = HeaderOf(_pathB);
        if (header is not null)
        {
            missA.WriteLine(CsvRdr.Line(header));
        }

        foreach (var row in Rows(_pathB, schema, null, null, null))
        {
            var reference = schema.Ref(row);
            if (bKeys.TryGetValue(reference, out var matched) && !matched)
            {
                stats.MissingInA++;
                missA.WriteLine(CsvRdr.Line(row));
            }
        }
    }

    private IEnumerable<string[]> Rows(
        string path,
        SrcSchema schema,
        StreamWriter? badSink,
        Action<bool>? onValid,
        Action? onInvalid)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16);
        using var rdr = new StreamReader(fs);

        long lineNo = 0;
        var headerWritten = false;

        foreach (var row in CsvRdr.Read(rdr))
        {
            lineNo++;
            if (lineNo == 1)
            {
                if (badSink is not null && !headerWritten)
                {
                    badSink.WriteLine(CsvRdr.Line(row.Concat(new[] { "Reason" })));
                    headerWritten = true;
                }
                continue;
            }

            if (IsBlank(row))
            {
                continue;
            }

            var err = schema.Validate(row);
            if (err is not null)
            {
                onInvalid?.Invoke();
                _log($"list {schema.Name} line {lineNo}: {err}");
                badSink?.WriteLine(CsvRdr.Line(row.Concat(new[] { err })));
                continue;
            }

            onValid?.Invoke(true);
            yield return row;
        }
    }

    private StreamWriter NewWriter(string name)
    {
        var path = Path.Combine(_outDir, name);
        var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16);
        return new StreamWriter(fs);
    }

    private static string[]? HeaderOf(string path)
    {
        using var rdr = new StreamReader(path);
        foreach (var row in CsvRdr.Read(rdr))
        {
            return row;
        }
        return null;
    }

    private static bool IsBlank(string[] row)
        => row.Length == 0 || row.All(f => f.Trim().Length == 0);
}
