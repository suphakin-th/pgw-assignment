using System.Diagnostics;

namespace PaymentApi.Logging;

public sealed class ReqLogMw
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ReqLogMw> _log;

    public ReqLogMw(RequestDelegate next, ILogger<ReqLogMw> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        var sw = Stopwatch.StartNew();
        var cid = Guid.NewGuid().ToString("N")[..12];
        ctx.Response.Headers["X-Correlation-Id"] = cid;

        _log.LogInformation("in  cid={Cid} {Method} {Path}", cid, ctx.Request.Method, ctx.Request.Path);

        try
        {
            await _next(ctx);
        }
        finally
        {
            sw.Stop();
            _log.LogInformation("out cid={Cid} {Status} {Ms}ms", cid, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
