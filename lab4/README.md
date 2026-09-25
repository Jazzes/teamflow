# Лабораторная работа №4. Оптимизация проекта и разработка модульных тестов

Дисциплина «Коллективная разработка приложений», Московский университет «Синергия».
Выполнил: Лисичкин Борис Олегович, группа ВБИо-402рсоб.

Исходный проект: REST API пользователей из лабораторной работы №2 (`lab2/src/Lab2.Part3.UsersApi`).
Он перенесён сюда первым коммитом без изменений, всё остальное сделано рефакторингом, история видна в ветке `feature/lab4-tests`.

## Структура

```
src/
  Users.Core        бизнес-логика: UserService, валидатор, защита пароля (PBKDF2), интерфейсы
  Users.Data        хранилище на EF Core и SQLite: UsersDbContext, UserRepository
  Users.Api         HTTP-слой: маршруты, перевод результата сервиса в HTTP-ответ, сборка зависимостей
tests/
  Users.Tests       модульные тесты: NUnit 4, NSubstitute, coverlet.collector
  coverlet.runsettings
benchmarks/
  Users.Benchmarks  замеры производительности
ci/
  check-coverage.py проверка порога покрытия 100% в CI
```

## Запуск тестов с покрытием

```bash
dotnet test tests/Users.Tests --collect:"XPlat Code Coverage" --settings tests/coverlet.runsettings --results-directory TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/report -reporttypes:Html
python3 ci/check-coverage.py
```

Отчёт открывается в браузере: `TestResults/report/index.html`.

## Замеры производительности

```bash
dotnet run --project benchmarks/Users.Benchmarks -c Release
```

## CI

Workflow `.github/workflows/lab4-tests.yml` запускается на каждый push и pull request: сборка, тесты, отчёт о покрытии в сводке запуска, проверка порога 100%, замеры производительности. Проверка `lab4-tests` обязательна для ветки `main`, поэтому pull request с ошибкой слить нельзя.
