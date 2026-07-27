using Microsoft.AspNetCore.Mvc;
using PaymentApi.Domain;
using PaymentApi.Dtos;
using PaymentApi.Services;

namespace PaymentApi.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class PayController : ControllerBase
{
    private const string IdemHdr = "Idempotency-Key";
    private readonly PaySvc _svc;
    private readonly PayEngine _engine;

    public PayController(PaySvc svc, PayEngine engine)
    {
        _svc = svc;
        _engine = engine;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] PayReq req, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!_engine.IsExpiryFuture(req.ExpiryDate, today))
        {
            return UnprocessableEntity(new ErrRes
            {
                Status = PayStatus.Failed,
                Message = "card expiry_date is not in the future",
            });
        }

        var idemKey = Request.Headers.TryGetValue(IdemHdr, out var k) ? k.ToString() : null;

        var result = await _svc.ProcessAsync(req, idemKey, ct);

        if (result.Replayed)
        {
            Response.Headers["Idempotent-Replayed"] = "true";
        }

        return Ok(result.Body);
    }
}
