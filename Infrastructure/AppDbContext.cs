using CLTTestApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CLTTestApi.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(500);

            entity.Property(user => user.IsActive)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.HasKey(address => address.Id);

            entity.Property(address => address.Street)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(address => address.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(address => address.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(address => address.ZipCode)
                .HasMaxLength(20);

            entity.HasOne(address => address.User)
                .WithMany(user => user.Addresses)
                .HasForeignKey(address => address.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("Currencies");
            entity.HasKey(currency => currency.Id);

            entity.Property(currency => currency.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(currency => currency.Code)
                .IsUnique();

            entity.Property(currency => currency.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(currency => currency.RateToBase)
                .HasPrecision(18, 6);
        });
    }
}
