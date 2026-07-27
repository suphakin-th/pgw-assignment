# Mini Payment API

A small payment-gateway simulator built with .NET 8 Web API. It exposes a single
endpoint that validates a card payment request, decides the outcome from the
amount, stores a masked transaction record, and returns a structured response.

## Business rule

The outcome is driven by the decimal (cents) part of `amount`:

| amount   | cents | status     | response_code | message         |
|----------|-------|------------|---------------|-----------------|
| 10.00    | 00    | `APPROVED` | `00`          | Payment Success |
| 10.05    | 05    | `DECLINED` | `05`          | Payment Reject  |

`FAILED` is returned for requests that cannot be processed (validation / expired
card). `UNKNOWN` is reserved for undetermined outcomes.

## Endpoint

```
POST /api/v1/pay
X-Api-Key: pgw-demo-key-001
Idempotency-Key: <optional, any unique string>
Content-Type: application/json
```

Request:

```json
{
  "order_number": "ORD-1001",
  "card_number": "4111111111111111",
  "expiry_date": "12/30",
  "cvv": "123",
  "currency": "THB",
  "cardholder_name": "Jane Doe",
  "email": "jane@example.com",
  "amount": 10.00
}
```

Response:

```json
{
  "transaction_id": "b1e0...uuid",
  "acquirer_reference": "ACQ250416120000123456",
  "response_code": "00",
  "status": "APPROVED",
  "message": "Payment Success",
  "timestamp": "2025-04-16T12:00:00.0000000+00:00",
  "amount": 10.00
}
```

## How to run

Requires the .NET 8 SDK.

```bash
cd PaymentApi
dotnet restore
dotnet run
```

The API listens on `http://localhost:5080`. Swagger UI is at
`http://localhost:5080/swagger`. A SQLite file `pay.db` is created on first run.

## Test the 10.00 vs 10.05 logic

Approved:

```bash
curl -s http://localhost:5080/api/v1/pay \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: pgw-demo-key-001" \
  -d '{"order_number":"ORD-1001","card_number":"4111111111111111","expiry_date":"12/30","cvv":"123","currency":"THB","cardholder_name":"Jane Doe","email":"jane@example.com","amount":10.00}'
```

Declined:

```bash
curl -s http://localhost:5080/api/v1/pay \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: pgw-demo-key-001" \
  -d '{"order_number":"ORD-1002","card_number":"4111111111111111","expiry_date":"12/30","cvv":"123","currency":"THB","cardholder_name":"Jane Doe","email":"jane@example.com","amount":10.05}'
```

A Postman collection is provided: `PaymentApi.postman_collection.json`.

## Non-functional requirements

**Logging.** Every request/response is logged with a correlation id and latency
(`ReqLogMw`). The logging layer never reads the request body, so card and CVV
values cannot leak into logs.

**Validation.** `PayReq` uses data-annotation rules: 16-digit numeric card,
3-4 digit CVV, `MM/YY` expiry, ISO 4217 alpha-3 currency, valid email, positive
amount. Expiry must be in the future (`PayEngine.IsExpiryFuture`). Invalid input
returns `422` with a per-field `errors` map.

**Data security (PII / PCI).** The full PAN and CVV are never logged or stored.
Card data lives only inside a `SecureStr` (backed by `SecureString`, pinned and
zeroed after use) for the short time it is needed to derive the masked PAN and
last four digits. The persisted record (`TxnRec`) holds only the masked PAN,
last four, and a masked email — no CVV column exists.

**Authentication.** All `/api/*` routes require a valid `X-Api-Key`
(`ApiKeyMw`). Keys are configured in `appsettings.json` under `Auth:ApiKeys`.

**Idempotency.** Send an `Idempotency-Key` header. The first call is processed
and stored against that key; repeat calls with the same key return the original
result (with `Idempotent-Replayed: true`) instead of charging again. A unique
index plus a race-safe insert guard the double-submit case.

**Storage (bonus).** Transactions are persisted to SQLite via EF Core, storing
only non-sensitive / masked fields.

## Tests

```bash
dotnet test
```

Covers the amount decision rule and expiry validation.

## Layout

```
PaymentApi/
  Auth/ApiKeyMw.cs          api-key middleware
  Controllers/PayController.cs
  Data/PayDbCtx.cs, TxnRec.cs
  Domain/PayStatus.cs
  Dtos/PayReq.cs, PayRes.cs, ErrRes.cs, JsonOpts.cs
  Logging/ReqLogMw.cs
  Security/SecureStr.cs, Mask.cs
  Services/PayEngine.cs, PaySvc.cs
  Program.cs
```
