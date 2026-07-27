using Reconciliation.Recon;
using Xunit;

namespace Reconciliation.Tests;

public sealed class ReconEngineTests : IDisposable
{
    private readonly string _dir;

    public ReconEngineTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "recon-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { }
    }

    private string Write(string name, string content)
    {
        var path = Path.Combine(_dir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static int DataRows(string path)
        => File.ReadAllLines(path).Length - 1;

    [Fact]
    public void Splits_matched_and_missing_both_ways()
    {
        var a = Write("A.csv",
            "#,Invoice Number,Transaction Date,Amount,Fees1,Fees2,Net Total,Card Number,Status\n" +
            "1,100,16-04-2025,\"1,000.00\",-0.8,-1.0,998.2,555555****1111,Success\n" +
            "2,101,16-04-2025,200.00,-0.8,-1.0,198.2,555555****1111,Success\n" +
            "3,999,16-04-2025,300.00,-0.8,-1.0,298.2,555555****1111,Success\n");

        var b = Write("B.csv",
            "#,Order Number,Transaction Date,Amount,Fees1,Fees2,Net Total,Status\n" +
            "1,100,2025-04-16,\"1,000.00\",-0.8,-1.0,998.2,Success\n" +
            "2,101,2025-04-16,200.00,-0.8,-1.0,198.2,Success\n" +
            "3,888,2025-04-16,400.00,-0.8,-1.0,398.2,Success\n");

        var outDir = Path.Combine(_dir, "out");
        var stats = new ReconEngine(a, b, outDir, _ => { }).Run();

        Assert.Equal(2, stats.Matched);
        Assert.Equal(1, stats.MissingInB);
        Assert.Equal(1, stats.MissingInA);

        Assert.Equal(2, DataRows(Path.Combine(outDir, "Matched_Records.csv")));
        Assert.Equal(1, DataRows(Path.Combine(outDir, "Missing_In_B.csv")));
        Assert.Equal(1, DataRows(Path.Combine(outDir, "Missing_In_A.csv")));

        var missB = File.ReadAllText(Path.Combine(outDir, "Missing_In_B.csv"));
        Assert.Contains("999", missB);
        var missA = File.ReadAllText(Path.Combine(outDir, "Missing_In_A.csv"));
        Assert.Contains("888", missA);
    }

    [Fact]
    public void Routes_invalid_rows_without_failing()
    {
        var a = Write("A.csv",
            "#,Invoice Number,Transaction Date,Amount,Fees1,Fees2,Net Total,Card Number,Status\n" +
            "1,100,16-04-2025,100.00,-0.8,-1.0,98.2,555555****1111,Success\n" +
            "2,bad,16-04-2025,100.00,-0.8,-1.0,98.2,555555****1111,Success\n");

        var b = Write("B.csv",
            "#,Order Number,Transaction Date,Amount,Fees1,Fees2,Net Total,Status\n" +
            "1,100,2025-04-16,100.00,-0.8,-1.0,98.2,Success\n");

        var outDir = Path.Combine(_dir, "out");
        var stats = new ReconEngine(a, b, outDir, _ => { }).Run();

        Assert.Equal(1, stats.AInvalid);
        Assert.Equal(1, stats.Matched);
        Assert.Equal(1, DataRows(Path.Combine(outDir, "Invalid_A.csv")));
    }
}
