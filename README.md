# DiAvto

Публичная визитка и админка для импорта автомобилей из Японии, Кореи и Китая.

- **Сайт:** [di-auto.ru](https://di-auto.ru) — каталог, этапы работы, автокредит, отзывы, контакты, заявки.
- **Админка:** `admin.di-auto.ru` — витрина, контент, заявки, карточки клиента, каталог che168, пользователи.
- **Стек:** Blazor (.NET 10, Interactive Server), Clean Architecture, EF Core 10 + SQLite.
- **Без внешних платформ:** проект самобытный, инфраструктура Maltat не используется.

## Структура

```text
src/
  Domain/DiAvto.Domain/            сущности
  Application/DiAvto.Application/  DTO, интерфейсы, расчёты, поиск
  Infrastructure/DiAvto.Infrastructure/
                                   EF Core, миграции, сидеры, сервисы
  Presents/DiAvto.UI/              общие Blazor-компоненты
  Presents/DiAvto.Site/            публичный сайт
  Presents/DiAvto.Admin/           админка
  Tests/DiAvto.Tests/              unit-тесты
tools/DiAvto.Seed/                 миграции и заполнение БД
tools/DiAvto.MigrateLegacy/        перенос данных из старой БД
deploy/                            systemd, nginx, backup
docs/                              архитектура и развёртывание
```

Подробности: [docs/architecture.md](docs/architecture.md) и [docs/deployment.md](docs/deployment.md).

## Локальный запуск

Требуется .NET 10 SDK.

```powershell
dotnet restore DiAvto.slnx
dotnet build DiAvto.slnx
```

База создаётся автоматически при старте сайта: `src/data/diavto.db`. Демо-контент (140 автомобилей, фото, отзывы, офисы, FAQ, секции) загружается из `showcase-seed.json`.

Публичный сайт:

```powershell
dotnet run --project src\Presents\DiAvto.Site\DiAvto.Site.csproj
```

Открыть: `http://localhost:5263`.

Админка:

```powershell
dotnet run --project src\Presents\DiAvto.Admin\DiAvto.Admin.csproj
```

Открыть: `http://localhost:5204`. Логин по умолчанию — `admin` / `admin` (задаётся в `appsettings.json` → `Seed`). Смените пароль после первого входа.

Заполнить или обновить БД вручную:

```powershell
dotnet run --project tools\DiAvto.Seed\DiAvto.Seed.csproj
```

## Тесты

```powershell
dotnet test src\Tests\DiAvto.Tests\DiAvto.Tests.csproj
```

50 тестов: расчёты импорта и таможни, бюджет поиска, переводы характеристик, подпись che168, доступ к админке, витрина, сидирование и публичная карточка клиента.

## Публичная карточка клиента

Ссылка вида `https://di-auto.ru/card/{PublicId}` формируется в админке:

1. **Автомобили** → выбрать автомобиль → **Сформировать карточку клиента**.
2. **Карточки клиента** → указать **Публичный адрес сайта** (`https://di-auto.ru`).
3. Скопировать ссылку и отправить клиенту.

Карточка живёт на публичном хосте, закрыта от индексации и показывает итоговую стоимость под ключ.

## Публикация

Сборка и развёртывание двух хостов описаны в [docs/deployment.md](docs/deployment.md). Публикация выполняется вручную (workflow `Deploy (manual)`), автоматического деплоя с каждой ветки нет.

## Legacy

Старый проект `src/DiAvto.Web` и `tests/DiAvto.Tests` оставлены как источник миграции и не входят в `DiAvto.slnx`.
