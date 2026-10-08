namespace MultitransferVaslAdapter.Data;

public sealed class AdapterDbContext : DbContext
{
    public AdapterDbContext(DbContextOptions<AdapterDbContext> options) : base(options) 
    {
        Database.EnsureCreated();
    }

    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionEntity>(e =>
        {
            e.ToTable("transactions");
            e.HasKey(x => x.TransactionId);
            e.HasIndex(x => x.CreatedAtUtc);
            e.Property(x => x.RawRequestJson).HasColumnType("jsonb");
        });
    }
}