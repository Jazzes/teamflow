namespace Users.Core.Security;

/// <summary>
/// Источник случайной соли. Вынесен в интерфейс, чтобы в тестах подставлять фиксированную соль.
/// </summary>
public interface ISaltGenerator
{
    byte[] Generate(int size);
}
