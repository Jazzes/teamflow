namespace Lab2.DataAccess.Entities;

/// <summary>
/// Книга в каталоге библиотеки.
/// </summary>
public class Book : IEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int PublishYear { get; set; }

    /// <summary>Цена в рублях. В SQLite хранится как TEXT, EF Core сам делает преобразование.</summary>
    public decimal Price { get; set; }

    public int AuthorId { get; set; }

    public Author? Author { get; set; }
}
