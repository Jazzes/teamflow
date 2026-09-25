using Lab2.Part3.UsersApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Part3.UsersApi.Data;

/// <summary>
/// Контекст базы пользователей.
/// </summary>
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("Users");
            user.HasKey(u => u.Id);

            // NOCASE: логины Admin и admin считаются одинаковыми
            user.Property(u => u.Login)
                .IsRequired()
                .HasMaxLength(32)
                .UseCollation("NOCASE");

            // Уникальный индекс защищает от дублей даже при одновременных запросах
            user.HasIndex(u => u.Login).IsUnique();

            user.Property(u => u.PassHash).IsRequired().HasMaxLength(200);
        });
    }
}
