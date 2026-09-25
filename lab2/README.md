# Лабораторная работа №2. Интерфейсы. Репозиторий. JSON

Дисциплина «Коллективная разработка приложений», Московский университет «Синергия».
Выполнил: Лисичкин Борис Олегович, группа ВБИо-402рсоб.

Код лежит в папке `lab2` репозитория teamflow. Все команды ниже выполняются из этой папки.

Стек: C# 12, .NET 8, Entity Framework Core 8 (SQLite), ASP.NET Core Minimal API.

## Структура решения

```
Lab2.sln
src/
  Lab2.Part1.Interfaces      Часть 1: интерфейсы, наследование интерфейсов, ISP, полиморфизм
  Lab2.DataAccess            Часть 2: библиотека доступа к данным (сущности, DbContext,
                             IRepository<T>, GenericRepository<T>, BookRepository,
                             AuthorRepository, Unit of Work)
  Lab2.Part2.RepositoryDemo  Часть 2: консольная демонстрация CRUD через репозитории
  Lab2.Part3.UsersApi        Часть 3: REST API для таблицы User (JSON, SQLite)
  Lab2.Part3.Client          Часть 3: клиент, который хеширует пароль и вызывает API
docs/                        исходники диаграмм (Graphviz)
```

## Запуск

Нужен .NET SDK 8 или новее.

```bash
dotnet build Lab2.sln

# Часть 1
dotnet run --project src/Lab2.Part1.Interfaces

# Часть 2 (создаёт library.db заново при каждом запуске)
dotnet run --project src/Lab2.Part2.RepositoryDemo

# Часть 3: в одном терминале сервер, в другом клиент
dotnet run --project src/Lab2.Part3.UsersApi
dotnet run --project src/Lab2.Part3.Client
```

## REST API (часть 3)

| Метод  | Маршрут        | Назначение                          | Коды ответа        |
|--------|----------------|-------------------------------------|--------------------|
| POST   | /user          | создание пользователя               | 201, 400, 409      |
| GET    | /user/{id}     | получение пользователя по Id        | 200, 404           |
| PUT    | /user/{id}     | обновление логина и хеша пароля     | 200, 400, 404, 409 |
| DELETE | /user/{id}     | удаление пользователя               | 200, 404           |
| GET    | /user          | список (дополнительно)              | 200                |
| POST   | /user/verify   | проверка пары логин/хеш (дополнительно) | 200, 400, 401  |

Пример тела запроса:

```json
{
  "Login": "example_user",
  "PassHash": "3875034e17855bac03a3cc9e107b1d28a9b44313d381c3335588525b4e70b55b"
}
```

`PassHash` это SHA-256 от пароля в hex (64 символа). Открытый пароль сервер отклоняет с кодом 400.
Перед записью в базу сервер дополнительно хеширует полученное значение алгоритмом PBKDF2
(SHA-256, 100 000 итераций, случайная соль 16 байт).

Готовые запросы лежат в `src/Lab2.Part3.UsersApi/requests.http`.

## Проверка в GitHub Actions

Workflow `.github/workflows/lab2-dotnet.yml` в корне репозитория запускается при изменениях в `lab2/`: собирает решение, выполняет демонстрации частей 1 и 2, поднимает API и прогоняет по нему клиента.
