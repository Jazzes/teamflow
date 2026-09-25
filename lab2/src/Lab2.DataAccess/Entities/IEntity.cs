namespace Lab2.DataAccess.Entities;

/// <summary>
/// Общий признак сущности, хранящейся в базе: у неё есть целочисленный ключ.
/// Нужен обобщённому репозиторию, чтобы сортировать и искать по Id.
/// </summary>
public interface IEntity
{
    int Id { get; }
}
