using Lab2.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab2.DataAccess.Data;

/// <summary>
/// Контекст EF Core для базы библиотеки. Описывает таблицы и ограничения через Fluent API.
/// </summary>
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(author =>
        {
            author.ToTable("Authors");
            author.HasKey(a => a.Id);
            author.Property(a => a.FullName).IsRequired().HasMaxLength(200);
            author.HasIndex(a => a.FullName);
        });

        modelBuilder.Entity<Book>(book =>
        {
            book.ToTable("Books");
            book.HasKey(b => b.Id);
            book.Property(b => b.Title).IsRequired().HasMaxLength(300);
            book.Property(b => b.Price).HasPrecision(10, 2);
            book.HasIndex(b => b.Title);

            // Удалить автора, у которого есть книги, нельзя: сначала нужно разобраться с книгами
            book.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
