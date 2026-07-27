using PaymentApi.Domain;
using PaymentApi.Services;
using Xunit;

namespace PaymentApi.Tests;

public sealed class PayEngineTests
{
    private readonly PayEngine _engine = new();

    [Theory]
    [InlineData(10.00, "00")]
    [InlineData(0.00, "00")]
    [InlineData(999.00, "00")]
    public void Decide_whole_amount_is_approved(decimal amt, string code)
    {
        var r = _engine.Decide(amt);
        Assert.Equal(PayStatus.Approved, r.Status);
        Assert.Equal(code, r.ResponseCode);
        Assert.Equal("Payment Success", r.Message);
    }

    [Theory]
    [InlineData(10.05, "05")]
    [InlineData(10.99, "99")]
    [InlineData(7.01, "01")]
    public void Decide_fractional_amount_is_declined(decimal amt, string code)
    {
        var r = _engine.Decide(amt);
        Assert.Equal(PayStatus.Declined, r.Status);
        Assert.Equal(code, r.ResponseCode);
        Assert.Equal("Payment Reject", r.Message);
    }

    [Fact]
    public void Expiry_future_is_accepted()
    {
        var now = new DateOnly(2025, 4, 16);
        Assert.True(_engine.IsExpiryFuture("12/30", now));
        Assert.True(_engine.IsExpiryFuture("04/25", now));
    }

    [Fact]
    public void Expiry_past_or_bad_is_rejected()
    {
        var now = new DateOnly(2025, 4, 16);
        Assert.False(_engine.IsExpiryFuture("03/25", now));
        Assert.False(_engine.IsExpiryFuture("13/30", now));
        Assert.False(_engine.IsExpiryFuture("bad", now));
    }
}
