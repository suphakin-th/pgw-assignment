using Reconciliation.Csv;
using Xunit;

namespace Reconciliation.Tests;

public sealed class CsvRdrTests
{
    private static List<string[]> Parse(string text)
    {
        using var rdr = new StringReader(text);
        return CsvRdr.Read(rdr).ToList();
    }

    [Fact]
    public void Reads_plain_rows()
    {
        var rows = Parse("a,b,c\n1,2,3\n");
        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "1", "2", "3" }, rows[1]);
    }

    [Fact]
    public void Keeps_commas_inside_quotes()
    {
        var rows = Parse("id,amount\n1,\"3,900.00\"\n");
        Assert.Equal("3,900.00", rows[1][1]);
    }

    [Fact]
    public void Handles_escaped_quote()
    {
        var rows = Parse("v\n\"a\"\"b\"\n");
        Assert.Equal("a\"b", rows[1][0]);
    }

    [Fact]
    public void Reads_last_row_without_trailing_newline()
    {
        var rows = Parse("a,b\n1,2");
        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "1", "2" }, rows[1]);
    }

    [Fact]
    public void Round_trips_field_needing_quotes()
    {
        var line = CsvRdr.Line(new[] { "x", "3,900.00", "he\"llo" });
        var back = Parse(line + "\n");
        Assert.Equal(new[] { "x", "3,900.00", "he\"llo" }, back[0]);
    }
}
