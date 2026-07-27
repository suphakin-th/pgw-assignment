using Microsoft.EntityFrameworkCore;

namespace PaymentApi.Data;

public sealed class PayDbCtx : DbContext
{
    public PayDbCtx(DbContextOptions<PayDbCtx> opts) : base(opts) { }

    public DbSet<TxnRec> Txns => Set<TxnRec>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        var e = mb.Entity<TxnRec>();
        e.HasIndex(x => x.IdemKey).IsUnique().HasFilter("IdemKey IS NOT NULL");
        e.HasIndex(x => x.OrderNumber);
        e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}
