using Reconciliation.Model;
using Xunit;

namespace Reconciliation.Tests;

public sealed class SrcSchemaTests
{
    private static string[] RowA(string reference, string date, string amount)
        => new[] { "1", reference, date, amount, "-0.8", "-21.3", "3,877.90", "555555****1111", "Success" };

    private static string[] RowB(string reference, string date, string amount)
        => new[] { "1", reference, date, amount, "-0.8", "-21.3", "3,877.90", "Success" };

    [Fact]
    public void ListA_accepts_valid_row()
    {
        Assert.Null(SrcSchema.ListA.Validate(RowA("2696111", "16-04-2025", "3,900.00")));
    }

    [Fact]
    public void ListB_accepts_valid_row()
    {
        Assert.Null(SrcSchema.ListB.Validate(RowB("2696111", "2025-04-16", "3,900.00")));
    }

    [Fact]
    public void Rejects_non_numeric_reference()
    {
        var err = SrcSchema.ListA.Validate(RowA("26x11", "16-04-2025", "3,900.00"));
        Assert.Contains("not numeric", err);
    }

    [Fact]
    public void Rejects_wrong_date_format()
    {
        var err = SrcSchema.ListA.Validate(RowA("2696111", "2025-04-16", "3,900.00"));
        Assert.Contains("date", err);
    }

    [Fact]
    public void Rejects_non_numeric_amount()
    {
        var err = SrcSchema.ListB.Validate(RowB("2696111", "2025-04-16", "abc"));
        Assert.Contains("amount", err);
    }

    [Fact]
    public void Rejects_short_row()
    {
        var err = SrcSchema.ListB.Validate(new[] { "1", "2696111" });
        Assert.Contains("columns", err);
    }

    [Fact]
    public void Ref_trims_value()
    {
        Assert.Equal("2696111", SrcSchema.ListA.Ref(RowA(" 2696111 ", "16-04-2025", "1.00")));
    }
}
