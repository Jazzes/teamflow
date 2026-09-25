namespace Lab2.DataAccess.Entities;

/// <summary>
/// Автор книг.
/// </summary>
public class Author : IEntity
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int BirthYear { get; set; }

    /// <summary>Книги автора (навигационное свойство, связь один ко многим).</summary>
    public List<Book> Books { get; set; } = new();
}
