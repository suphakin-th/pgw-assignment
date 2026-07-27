using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Data;

public sealed class TxnRec
{
    [Key]
    public string TransactionId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string AcquirerReference { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CardLast4 { get; set; } = string.Empty;
    public string CardMasked { get; set; } = string.Empty;
    public string EmailMasked { get; set; } = string.Empty;
    public string? IdemKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
