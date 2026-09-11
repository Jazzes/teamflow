#!/usr/bin/env bash
set -euo pipefail

REPO_NAME="${1:-teamflow}"

if [ -d .git ]; then
  echo "Git-репозиторий уже инициализирован, публикация пропущена" >&2
  exit 1
fi
command -v gh >/dev/null || { echo "Нужен GitHub CLI: https://cli.github.com" >&2; exit 1; }
gh auth status >/dev/null || { echo "Выполните gh auth login" >&2; exit 1; }

git init -b main
git add package.json package-lock.json tsconfig.base.json tsconfig.json vitest.config.ts \
  .gitignore .editorconfig .env.example docker-compose.yml README.md .vscode
git commit -m "chore: инициализировать монорепозиторий и общие настройки"

git add packages/dal
git commit -m "feat(dal): схема БД из 12 таблиц, миграция и репозитории"

git add packages/bll
git commit -m "feat(bll): сервисы проектов, задач и спринтов с модульными тестами"

git add apps/api
git commit -m "feat(api): REST API на Express поверх бизнес-логики"

git add .github publish.sh
git commit -m "ci: сборка, тесты и проверка миграций в GitHub Actions"

git switch -c develop
git switch main

gh repo create "$REPO_NAME" --public --source=. --remote=origin --push
git push -u origin develop
echo "Готово: $(gh repo view --json url -q .url)"
