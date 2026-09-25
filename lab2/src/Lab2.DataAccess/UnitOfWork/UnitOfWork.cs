using Lab2.DataAccess.Data;
using Lab2.DataAccess.Repositories;

namespace Lab2.DataAccess.UnitOfWork;

/// <summary>
/// Реализация Unit of Work поверх одного LibraryDbContext.
/// Репозитории создаются лениво при первом обращении.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _context;
    private IBookRepository? _books;
    private IAuthorRepository? _authors;

    public UnitOfWork(LibraryDbContext context)
    {
        _context = context;
    }

    public IBookRepository Books => _books ??= new BookRepository(_context);

    public IAuthorRepository Authors => _authors ??= new AuthorRepository(_context);

    public int SaveChanges() => _context.SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public void Rollback() => _context.ChangeTracker.Clear();

    public void EnsureDatabaseCreated(bool recreate = false)
    {
        if (recreate)
        {
            _context.Database.EnsureDeleted();
        }

        _context.Database.EnsureCreated();
    }

    public void Dispose() => _context.Dispose();

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
