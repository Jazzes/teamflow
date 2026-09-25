using Microsoft.EntityFrameworkCore;

namespace Lab2.DataAccess.Data;

/// <summary>
/// Создаёт контекст для файла SQLite. Строка подключения приходит снаружи,
/// поэтому библиотека не привязана к конкретному пути или окружению.
/// </summary>
public static class LibraryDbContextFactory
{
    public static LibraryDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new LibraryDbContext(options);
    }
}
