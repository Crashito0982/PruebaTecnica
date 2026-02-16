using Microsoft.EntityFrameworkCore;
using Gastos.Domain.Entities;

namespace Gastos.Infrastructure.Persistence;

public class GastosDbContext : DbContext
{
    public GastosDbContext(DbContextOptions<GastosDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<Category>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            b.HasIndex(x => new { x.UsuarioId, x.Nombre }).IsUnique();

            b.HasMany(x => x.Gastos)
             .WithOne()
             .HasForeignKey(e => e.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Expense>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
            b.Property(x => x.Fecha).HasColumnType("date");
            b.ToTable(t => t.HasCheckConstraint("CK_Expenses_Monto_Positive", "[Monto] > 0"));
        });
    }
}
