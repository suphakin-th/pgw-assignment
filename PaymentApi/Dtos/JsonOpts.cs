using System.Text.Json;

namespace PaymentApi.Dtos;

public static class JsonOpts
{
    public static readonly JsonSerializerOptions Snake = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}
