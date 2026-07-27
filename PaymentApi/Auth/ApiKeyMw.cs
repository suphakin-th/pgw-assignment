using System.Text.Json;
using PaymentApi.Dtos;

namespace PaymentApi.Auth;

public sealed class ApiKeyMw
{
    private const string HdrName = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _keys;

    public ApiKeyMw(RequestDelegate next, IConfiguration conf)
    {
        _next = next;
        _keys = conf.GetSection("Auth:ApiKeys").Get<string[]>()?.ToHashSet()
                ?? new HashSet<string>();
    }

    public async Task Invoke(HttpContext ctx)
    {
        if (!ctx.Request.Path.StartsWithSegments("/api"))
        {
            await _next(ctx);
            return;
        }

        if (!ctx.Request.Headers.TryGetValue(HdrName, out var got)
            || got.Count == 0
            || !_keys.Contains(got.ToString()))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json";
            var body = new ErrRes { Status = "FAILED", Message = "invalid or missing api key" };
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOpts.Snake));
            return;
        }

        await _next(ctx);
    }
}
