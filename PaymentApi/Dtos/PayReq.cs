using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Dtos;

public sealed class PayReq
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "card_number must be exactly 16 digits")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "expiry_date must be in MM/YY format")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "cvv must be 3 or 4 digits")]
    public string Cvv { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "currency must be an ISO 4217 alpha-3 code")]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string CardholderName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1_000_000_000, ErrorMessage = "amount must be a positive decimal")]
    public decimal Amount { get; set; }
}
