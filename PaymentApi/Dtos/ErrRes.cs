namespace PaymentApi.Dtos;

public sealed class ErrRes
{
    public string Status { get; set; } = "FAILED";
    public string Message { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
}
