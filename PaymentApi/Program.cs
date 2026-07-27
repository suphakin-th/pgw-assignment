using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Auth;
using PaymentApi.Data;
using PaymentApi.Dtos;
using PaymentApi.Logging;
using PaymentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        o.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    o.InvalidModelStateResponseFactory = ctx =>
    {
        var errs = ctx.ModelState
            .Where(kv => kv.Value is { Errors.Count: > 0 })
            .ToDictionary(
                kv => ToSnake(kv.Key),
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var body = new ErrRes
        {
            Status = "FAILED",
            Message = "request validation failed",
            Errors = errs,
        };
        return new UnprocessableEntityObjectResult(body);
    };
});

var conn = builder.Configuration.GetConnectionString("Pay") ?? "Data Source=pay.db";
builder.Services.AddDbContext<PayDbCtx>(o => o.UseSqlite(conn));

builder.Services.AddSingleton<PayEngine>();
builder.Services.AddScoped<PaySvc>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PayDbCtx>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ReqLogMw>();
app.UseMiddleware<ApiKeyMw>();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "UP" }));

app.Run();

static string ToSnake(string key)
{
    var last = key.Contains('.') ? key[(key.LastIndexOf('.') + 1)..] : key;
    return System.Text.Json.JsonNamingPolicy.SnakeCaseLower.ConvertName(last);
}

public partial class Program { }
