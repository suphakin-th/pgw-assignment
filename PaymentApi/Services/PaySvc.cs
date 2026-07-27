using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Domain;
using PaymentApi.Dtos;
using PaymentApi.Security;

namespace PaymentApi.Services;

public sealed class PaySvcResult
{
    public PayRes? Body { get; init; }
    public bool Replayed { get; init; }
}

public sealed class PaySvc
{
    private readonly PayEngine _engine;
    private readonly PayDbCtx _db;
    private readonly ILogger<PaySvc> _log;

    public PaySvc(PayEngine engine, PayDbCtx db, ILogger<PaySvc> log)
    {
        _engine = engine;
        _db = db;
        _log = log;
    }

    public async Task<PaySvcResult> ProcessAsync(PayReq req, string? idemKey, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(idemKey))
        {
            var prior = await _db.Txns.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdemKey == idemKey, ct);
            if (prior is not null)
            {
                _log.LogInformation("idempotent replay for key {Key} txn {Txn}", idemKey, prior.TransactionId);
                return new PaySvcResult { Body = ToRes(prior), Replayed = true };
            }
        }

        using var pan = new SecureStr(req.CardNumber);
        using var cvv = new SecureStr(req.Cvv);

        var last4 = pan.Use(p => p[^4..]);
        var masked = pan.Use(Mask.Pan);

        var outcome = _engine.Decide(req.Amount);
        var txnId = Guid.NewGuid().ToString();
        var acqRef = _engine.NewAcqRef();
        var now = DateTimeOffset.UtcNow;

        var rec = new TxnRec
        {
            TransactionId = txnId,
            OrderNumber = req.OrderNumber,
            AcquirerReference = acqRef,
            ResponseCode = outcome.ResponseCode,
            Status = outcome.Status,
            Amount = req.Amount,
            Currency = req.Currency,
            CardLast4 = last4,
            CardMasked = masked,
            EmailMasked = Mask.Email(req.Email),
            IdemKey = string.IsNullOrWhiteSpace(idemKey) ? null : idemKey,
            CreatedAt = now,
        };

        try
        {
            _db.Txns.Add(rec);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (!string.IsNullOrWhiteSpace(idemKey))
        {
            var winner = await _db.Txns.AsNoTracking()
                .FirstAsync(x => x.IdemKey == idemKey, ct);
            _log.LogInformation("idempotent race resolved for key {Key} txn {Txn}", idemKey, winner.TransactionId);
            return new PaySvcResult { Body = ToRes(winner), Replayed = true };
        }

        _log.LogInformation(
            "processed order {Order} txn {Txn} status {Status} code {Code} pan {Pan} amount {Amt} {Ccy}",
            req.OrderNumber, txnId, outcome.Status, outcome.ResponseCode, masked, req.Amount, req.Currency);

        return new PaySvcResult
        {
            Body = new PayRes
            {
                TransactionId = txnId,
                AcquirerReference = acqRef,
                ResponseCode = outcome.ResponseCode,
                Status = outcome.Status,
                Message = outcome.Message,
                Timestamp = now.ToString("o"),
                Amount = req.Amount,
            },
        };
    }

    private static PayRes ToRes(TxnRec r)
    {
        var msg = r.Status == PayStatus.Approved ? "Payment Success" : "Payment Reject";
        return new PayRes
        {
            TransactionId = r.TransactionId,
            AcquirerReference = r.AcquirerReference,
            ResponseCode = r.ResponseCode,
            Status = r.Status,
            Message = msg,
            Timestamp = r.CreatedAt.ToString("o"),
            Amount = r.Amount,
        };
    }
}
