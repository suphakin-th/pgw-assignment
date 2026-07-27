namespace PaymentApi.Dtos;

public sealed class PayRes
{
    public string TransactionId { get; set; } = string.Empty;
    public string AcquirerReference { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
