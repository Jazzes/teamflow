using Lab2.DataAccess.Entities;

namespace Lab2.DataAccess.Repositories;

/// <summary>
/// Репозиторий авторов.
/// </summary>
public interface IAuthorRepository : IRepository<Author>
{
    /// <summary>Автор вместе со списком книг (жадная загрузка через Include).</summary>
    Author? GetWithBooks(int id);

    IEnumerable<Author> FindByName(string namePart);
}
