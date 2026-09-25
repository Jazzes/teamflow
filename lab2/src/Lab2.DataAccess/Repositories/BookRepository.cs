using Lab2.DataAccess.Data;
using Lab2.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab2.DataAccess.Repositories;

/// <summary>
/// Репозиторий книг для SQLite. CRUD наследуется от GenericRepository,
/// здесь только специфичные запросы. Все значения попадают в SQL как параметры.
/// </summary>
public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(LibraryDbContext context)
        : base(context)
    {
    }

    public IEnumerable<Book> GetByAuthor(int authorId)
    {
        return Set.AsNoTracking()
            .Where(b => b.AuthorId == authorId)
            .OrderBy(b => b.PublishYear)
            .ToList();
    }

    public IEnumerable<Book> GetByPriceRange(decimal minPrice, decimal maxPrice)
    {
        if (minPrice > maxPrice)
        {
            throw new ArgumentException("Нижняя граница цены больше верхней.");
        }

        // Условие по цене выполняется в базе: для decimal провайдер SQLite генерирует функцию ef_compare.
        // А вот ORDER BY по decimal SQLite не поддерживает, поэтому сортировка идёт уже в памяти.
        return Set.AsNoTracking()
            .Include(b => b.Author)
            .Where(b => b.Price >= minPrice && b.Price <= maxPrice)
            .AsEnumerable()
            .OrderBy(b => b.Price)
            .ToList();
    }

    public IEnumerable<Book> SearchByTitle(string titleFragment)
    {
        if (string.IsNullOrWhiteSpace(titleFragment))
        {
            return Array.Empty<Book>();
        }

        return Set.AsNoTracking()
            .Where(b => b.Title.Contains(titleFragment))
            .OrderBy(b => b.Title)
            .ToList();
    }

    public async Task<IReadOnlyList<Book>> GetPublishedAfterAsync(int year, CancellationToken cancellationToken = default)
    {
        return await Set.AsNoTracking()
            .Include(b => b.Author)
            .Where(b => b.PublishYear > year)
            .OrderBy(b => b.PublishYear)
            .ToListAsync(cancellationToken);
    }
}
