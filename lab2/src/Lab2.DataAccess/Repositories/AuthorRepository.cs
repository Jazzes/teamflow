using Lab2.DataAccess.Data;
using Lab2.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab2.DataAccess.Repositories;

/// <summary>
/// Репозиторий авторов.
/// </summary>
public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
{
    public AuthorRepository(LibraryDbContext context)
        : base(context)
    {
    }

    public Author? GetWithBooks(int id)
    {
        return Set.AsNoTracking()
            .Include(a => a.Books.OrderBy(b => b.PublishYear))
            .FirstOrDefault(a => a.Id == id);
    }

    public IEnumerable<Author> FindByName(string namePart)
    {
        return Set.AsNoTracking()
            .Where(a => a.FullName.Contains(namePart))
            .OrderBy(a => a.FullName)
            .ToList();
    }
}
