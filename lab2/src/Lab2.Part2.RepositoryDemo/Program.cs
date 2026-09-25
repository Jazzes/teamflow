using System.Globalization;
using System.Text;
using Lab2.DataAccess.Data;
using Lab2.DataAccess.Entities;
using Lab2.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

// Путь к базе можно передать первым аргументом, по умолчанию файл library.db рядом с программой
var connectionString = args.Length > 0 ? args[0] : "Data Source=library.db";

await using IUnitOfWork uow = new UnitOfWork(LibraryDbContextFactory.Create(connectionString));
uow.EnsureDatabaseCreated(recreate: true);

Header("Create: добавляем авторов и книги (синхронные методы + SaveChanges)");
var tolstoy = new Author { FullName = "Лев Толстой", BirthYear = 1828 };
var dostoevsky = new Author { FullName = "Фёдор Достоевский", BirthYear = 1821 };
var bulgakov = new Author { FullName = "Михаил Булгаков", BirthYear = 1891 };
uow.Authors.Add(tolstoy);
uow.Authors.Add(dostoevsky);
uow.Authors.Add(bulgakov);
uow.Books.Add(new Book { Title = "Война и мир", PublishYear = 1869, Price = 1450.00m, Author = tolstoy });
uow.Books.Add(new Book { Title = "Анна Каренина", PublishYear = 1878, Price = 990.00m, Author = tolstoy });
uow.Books.Add(new Book { Title = "Преступление и наказание", PublishYear = 1866, Price = 720.50m, Author = dostoevsky });
uow.Books.Add(new Book { Title = "Идиот", PublishYear = 1869, Price = 810.00m, Author = dostoevsky });
uow.Books.Add(new Book { Title = "Мастер и Маргарита", PublishYear = 1967, Price = 1250.00m, Author = bulgakov });
uow.Books.Add(new Book { Title = "Белая гвардия", PublishYear = 1925, Price = 560.00m, Author = bulgakov });
var inserted = uow.SaveChanges();
Console.WriteLine($"  Сохранено строк: {inserted}");
PrintBooks(uow.Books.GetAll());

Header("Read: GetById, GetWithBooks, FindByName");
var book = uow.Books.GetById(3);
Console.WriteLine($"  GetById(3): {book?.Title} ({book?.PublishYear})");
Console.WriteLine($"  GetById(99): {(uow.Books.GetById(99) is null ? "не найдено, вернулся null" : "найдено")}");
var authorWithBooks = uow.Authors.GetWithBooks(bulgakov.Id);
Console.WriteLine($"  GetWithBooks({bulgakov.Id}): {authorWithBooks?.FullName}, книг: {authorWithBooks?.Books.Count}");
foreach (var b in authorWithBooks?.Books ?? new List<Book>())
{
    Console.WriteLine($"    - {b.Title}, {b.PublishYear}");
}
foreach (var a in uow.Authors.FindByName("Лев"))
{
    Console.WriteLine($"  FindByName(\"Лев\"): {a.FullName}, {a.BirthYear} г. р.");
}

Header("Специализированные запросы IBookRepository");
Console.WriteLine($"  GetByAuthor({dostoevsky.Id}):");
PrintBooks(uow.Books.GetByAuthor(dostoevsky.Id));
Console.WriteLine("  GetByPriceRange(500, 1000):");
PrintBooks(uow.Books.GetByPriceRange(500m, 1000m));
Console.WriteLine("  SearchByTitle(\"и\"):");
PrintBooks(uow.Books.SearchByTitle("и"));

Header("Update: меняем цену книги");
var toUpdate = uow.Books.GetById(1)!;
Console.WriteLine($"  Было: {toUpdate.Title}, {toUpdate.Price:N2} руб.");
toUpdate.Price = 1390.00m;
uow.Books.Update(toUpdate);
uow.SaveChanges();
// Сбрасываем кэш отслеживаемых сущностей, чтобы GetById реально сходил в базу
uow.Rollback();
Console.WriteLine($"  Стало (перечитано из БД): {uow.Books.GetById(1)!.Price:N2} руб.");

Header("Delete: удаляем книгу по Id");
Console.WriteLine($"  Delete(6) = {uow.Books.Delete(6)}");
Console.WriteLine($"  Delete(99) = {uow.Books.Delete(99)}");
uow.SaveChanges();
Console.WriteLine($"  Книг в базе: {uow.Books.GetAll().Count()}");

Header("Асинхронные методы: AddAsync, SaveChangesAsync, GetAllAsync, GetByIdAsync");
var chekhov = new Author { FullName = "Антон Чехов", BirthYear = 1860 };
await uow.Authors.AddAsync(chekhov);
await uow.Books.AddAsync(new Book { Title = "Вишнёвый сад", PublishYear = 1904, Price = 390.00m, Author = chekhov });
var asyncSaved = await uow.SaveChangesAsync();
Console.WriteLine($"  SaveChangesAsync сохранил строк: {asyncSaved}");
var allAuthors = await uow.Authors.GetAllAsync();
Console.WriteLine($"  GetAllAsync: авторов {allAuthors.Count}: {string.Join(", ", allAuthors.Select(a => a.FullName))}");
var found = await uow.Books.GetByIdAsync(7);
Console.WriteLine($"  GetByIdAsync(7): {found?.Title}");
Console.WriteLine("  GetPublishedAfterAsync(1900):");
foreach (var b in await uow.Books.GetPublishedAfterAsync(1900))
{
    Console.WriteLine($"    - {b.Title} ({b.PublishYear}), {b.Author?.FullName}");
}

Header("Unit of Work: одна транзакция на все изменения");
var authorsBefore = uow.Authors.GetAll().Count();
uow.Authors.Add(new Author { FullName = "Иван Тургенев", BirthYear = 1818 });
uow.Books.Add(new Book { Title = null!, PublishYear = 1862, Price = 450.00m, AuthorId = 1 });
try
{
    uow.SaveChanges();
}
catch (DbUpdateException ex)
{
    Console.WriteLine($"  Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}");
    uow.Rollback();
}
var authorsAfter = uow.Authors.GetAll().Count();
Console.WriteLine($"  Авторов до: {authorsBefore}, после: {authorsAfter}. Автор без книги тоже не сохранился.");

static void Header(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

static void PrintBooks(IEnumerable<Book> books)
{
    Console.WriteLine($"  {"Id",-3} {"Название",-26} {"Год",-5} {"Цена, руб.",10}");
    foreach (var b in books)
    {
        Console.WriteLine($"  {b.Id,-3} {b.Title,-26} {b.PublishYear,-5} {b.Price,10:N2}");
    }
}
