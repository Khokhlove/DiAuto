# Архитектура DiAvto

## Что это

Два независимых веб-приложения на Blazor (.NET 10, Interactive Server) и общее ядро на Clean Architecture:

| Приложение | Домен | Назначение |
|---|---|---|
| `DiAvto.Site` | `di-auto.ru` | Публичная визитка, каталог, отзывы, заявки, карточка клиента |
| `DiAvto.Admin` | `admin.di-auto.ru` | Управление витриной, контентом, заявками, каталогом che168 и пользователями |

Оба хоста используют одну базу SQLite и один набор Application/Infrastructure-сервисов, но не имеют общих страниц и маршрутов.

## Слои

```text
src/
  Domain/
    DiAvto.Domain/            сущности и правила, без EF Core
  Application/
    DiAvto.Application/       DTO, интерфейсы, бизнес-логика импорта и поиска
  Infrastructure/
    DiAvto.Infrastructure/    EF Core, SQLite, сидеры, реализации сервисов
  Presents/
    DiAvto.UI/                общие Blazor-компоненты и форматирование
    DiAvto.Site/              публичный хост
    DiAvto.Admin/             админский хост
  Tests/
    DiAvto.Tests/             unit-тесты
tools/
  DiAvto.Seed/                миграции и первичное заполнение БД
  DiAvto.MigrateLegacy/       перенос старых таблиц в новую схему
```

Направление зависимостей: `Domain ← Application ← Infrastructure ← Presents`.

- `Domain` не знает про EF Core, ASP.NET и SQLite.
- `Application` содержит сценарии, DTO, расчёты импорта, поиск и переводы характеристик.
- `Infrastructure` содержит `DiAvtoDbContext`, миграции, сидеры и реализации публичных сервисов.
- `DiAvto.UI` содержит переиспользуемые компоненты (`LeadForm`, `CarCard`, шапка/подвал сайта) и форматирование.
- `Site` и `Admin` — только composition root и страницы.

## Публичный контур

Страницы `DiAvto.Site`:

- `/` — главная визитка;
- `/catalog`, `/catalog/{id}` — каталог и карточка автомобиля;
- `/stages-of-work`, `/auto-credit`, `/reviews`, `/about`, `/contacts`;
- `/privacy-policy`;
- `/card/{PublicId}` — персональная карточка клиента с итоговой стоимостью (закрыта от индексации);
- `/health` — health-check.

Публичные данные читаются через интерфейсы Application:

- `IShowcaseCarService` — витрина и фильтры;
- `IReviewService`, `IOfficeService`, `IFaqService`;
- `ISiteSettingsService` — телефоны, соцсети, название;
- `IContentSectionService` — настраиваемые секции;
- `ILeadService` — заявки с форм;
- `IClientCardPublicService` — карточка клиента по `PublicId`.

## Админский контур

`DiAvto.Admin` сохраняет старые разделы и добавляет управление публичным контентом:

- сводка, поиски, автомобили che168, карточки клиента, тарифы, источники, каталог, курсы;
- витрина публичного сайта;
- отзывы, офисы, заявки;
- настройки сайта, FAQ, секции и карточки секций;
- пользователи, журнал, интеграция.

Доступ — ASP.NET Core Identity, роли `Admin` / `Manager` и политики `AdminPage.*`.

## База данных

- Провайдер: SQLite через EF Core.
- Локально: `src/data/diavto.db`.
- Production: `/opt/diavto/Data/diavto.db`.
- Миграции применяются при старте приложения. Публичный контент сидирует только `DiAvto.Site`, чтобы два хоста не дублировали данные.
- Демо-контент: `src/Infrastructure/DiAvto.Infrastructure/Seed/showcase-seed.json` (140 автомобилей, 2475 фото, отзывы, офисы, FAQ, секции).
- Фотографии витрины: `src/Presents/DiAvto.Site/wwwroot/images/cars`.

## Конфигурация

`appsettings.json` — локальная разработка:

- Site: `http://localhost:5263`;
- Admin: `http://localhost:5204`;
- БД: `../../data/diavto.db` относительно каталога проекта.

`appsettings.Production.json`:

- Site слушает `http://0.0.0.0:8080`;
- Admin слушает `http://0.0.0.0:8081`;
- БД: `/opt/diavto/Data/diavto.db`;
- HTTPS завершается на Nginx.

## Тесты

`src/Tests/DiAvto.Tests` — 50 unit-тестов:

- расчёт стоимости импорта и таможни;
- планирование бюджета поиска;
- переводы марок/моделей и характеристик;
- подпись запросов che168;
- политики доступа админки;
- миграции и сидирование публичного контента;
- маппинг публичной карточки клиента;
- фильтры и сортировка витрины.
