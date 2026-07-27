using System.Globalization;
using PaymentApi.Domain;
using PaymentApi.Dtos;

namespace PaymentApi.Services;

public sealed class PayOutcome
{
    public string Status { get; init; } = PayStatus.Unknown;
    public string ResponseCode { get; init; } = "96";
    public string Message { get; init; } = string.Empty;
}

public sealed class PayEngine
{
    public PayOutcome Decide(decimal amount)
    {
        var cents = (int)(Math.Round(amount, 2, MidpointRounding.AwayFromZero) * 100) % 100;

        if (cents == 0)
        {
            return new PayOutcome
            {
                Status = PayStatus.Approved,
                ResponseCode = "00",
                Message = "Payment Success",
            };
        }

        return new PayOutcome
        {
            Status = PayStatus.Declined,
            ResponseCode = cents.ToString("D2", CultureInfo.InvariantCulture),
            Message = "Payment Reject",
        };
    }

    public bool IsExpiryFuture(string mmYy, DateOnly now)
    {
        var parts = mmYy.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }
        if (!int.TryParse(parts[0], out var mm) || !int.TryParse(parts[1], out var yy))
        {
            return false;
        }
        if (mm < 1 || mm > 12)
        {
            return false;
        }
        var year = 2000 + yy;
        var lastDay = DateTime.DaysInMonth(year, mm);
        var exp = new DateOnly(year, mm, lastDay);
        return exp >= now;
    }

    public string NewAcqRef()
    {
        var stamp = DateTimeOffset.UtcNow.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
        var rnd = Random.Shared.Next(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        return $"ACQ{stamp}{rnd}";
    }
}
