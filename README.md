# TeamFlow

Учебная информационная система учёта задач команды разработки (лабораторная работа №1, дисциплина «Коллективная разработка приложений»).

## Структура

```
packages/dal   Data Access Layer: схема PostgreSQL (Drizzle ORM), миграции, репозитории
packages/bll   Business Logic Layer: сервисы проектов, задач и спринтов, валидация, тесты
apps/api       Presentation Layer: REST API на Express
```

Зависимости слоёв идут строго сверху вниз: `api → bll → dal`. Они описаны в `package.json` каждого пакета и в ссылках TypeScript (`references` в `tsconfig.json`).

## Запуск

```bash
cp .env.example .env
docker compose up -d
npm install
npm run db:migrate
npm run db:seed
npm start
```

Проверка: `curl http://localhost:3000/api/health`.

## Тесты

```bash
npm test
```

## Публикация на GitHub

```bash
./publish.sh teamflow
```

Скрипт создаёт репозиторий через GitHub CLI, делает историю осмысленных коммитов по слоям и отправляет её в `main`. Дальше в Cursor/VS Code включён автопуш после коммита (`.vscode/settings.json`).
