namespace MultitransferVaslAdapter.src.MultitransferVaslAdapter.Data;

public partial class AdapterDbContext : DbContext
{
    public AdapterDbContext(DbContextOptions<AdapterDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
