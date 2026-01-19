namespace GameDataTool.Infrastructure.Persistence;

/// <summary>
/// 애플리케이션 데이터베이스 컨텍스트
/// Application database context
/// </summary>
public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(256).IsRequired();
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}
