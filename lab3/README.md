# Лабораторная работа №3. Построение IDEF0 и UML/ER-диаграмм

Дисциплина «Коллективная разработка приложений», Московский университет «Синергия».
Выполнил: Лисичкин Борис Олегович, группа ВБИо-402рсоб.

Предметная область: система учёта задач команды разработки **TeamFlow** из лабораторной работы №1 (код лежит в корне этого репозитория, схема БД в `packages/dal/src/schema.ts`).

## Состав

```
diagrams/
  teamflow-idef0.drawio   функциональная модель IDEF0, 7 страниц: A-0, A0, A1–A5 (diagrams.net)
  teamflow-er.puml        ER-диаграмма базы данных, 12 сущностей (PlantUML, нотация IE)
  png/                    экспорт всех диаграмм в PNG
tools/
  idef0.py                генератор диаграмм IDEF0 в формате .drawio (геометрия и трассировка стрелок)
  model.py                сама модель: блоки, стрелки ICOM, декомпозиция
  export.sh               экспорт страниц .drawio в PNG через diagrams.net desktop
```

## Как открыть и пересобрать

IDEF0: открыть `diagrams/teamflow-idef0.drawio` в diagrams.net (https://app.diagrams.net или desktop-версия), страницы переключаются внизу окна. Чтобы пересобрать файл из модели:

```bash
python3 tools/model.py
./tools/export.sh diagrams/teamflow-idef0.drawio diagrams/png/idef0 7   # нужен drawio desktop
```

ER-диаграмма:

```bash
java -jar plantuml.jar -charset UTF-8 -tpng diagrams/teamflow-er.puml
```

## Функции верхнего уровня (A0)

| Узел | Функция |
|------|---------|
| A1 | Управлять командами и доступом |
| A2 | Вести проекты и бэклог |
| A3 | Планировать и проводить спринты |
| A4 | Выполнять задачи |
| A5 | Формировать отчётность |
