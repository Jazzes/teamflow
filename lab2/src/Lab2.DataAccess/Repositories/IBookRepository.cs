using Lab2.DataAccess.Entities;

namespace Lab2.DataAccess.Repositories;

/// <summary>
/// Репозиторий книг: к базовым CRUD-операциям добавлены запросы предметной области.
/// </summary>
public interface IBookRepository : IRepository<Book>
{
    IEnumerable<Book> GetByAuthor(int authorId);

    IEnumerable<Book> GetByPriceRange(decimal minPrice, decimal maxPrice);

    IEnumerable<Book> SearchByTitle(string titleFragment);

    Task<IReadOnlyList<Book>> GetPublishedAfterAsync(int year, CancellationToken cancellationToken = default);
}
