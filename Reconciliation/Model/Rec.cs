namespace Reconciliation.Model;

public sealed class Rec
{
    public required string Reference { get; init; }
    public required string[] Raw { get; init; }
    public long LineNo { get; init; }
}

public sealed class BadRow
{
    public required long LineNo { get; init; }
    public required string Reason { get; init; }
    public required string[] Raw { get; init; }
}
