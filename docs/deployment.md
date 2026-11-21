# Развёртывание DiAvto

## Схема

| Компонент | Домен | Порт Kestrel | Каталог |
|---|---|---|---|
| Публичный сайт | `di-auto.ru` | `8080` | `/opt/diavto/site` |
| Админка | `admin.di-auto.ru` | `8081` | `/opt/diavto/admin` |
| SQLite | — | — | `/opt/diavto/Data/diavto.db` |

Nginx принимает HTTP/HTTPS на `80/443` и проксирует каждый домен на свой порт. Оба приложения работают под пользователем `diavto` и используют одну БД.

## Сборка

```powershell
dotnet publish src\Presents\DiAvto.Site\DiAvto.Site.csproj -c Release -r linux-x64 --self-contained true -o publish-linux\site
dotnet publish src\Presents\DiAvto.Admin\DiAvto.Admin.csproj -c Release -r linux-x64 --self-contained true -o publish-linux\admin
```

Архивы:

```powershell
tar.exe -czf diavto-site.tar.gz -C publish-linux\site .
tar.exe -czf diavto-admin.tar.gz -C publish-linux\admin .
```

## Первичная установка на сервере

> **Важно:** база старого `DiAvto.Web` имеет другую историю миграций. Новое приложение нельзя запускать поверх неё: EF Core попытается создать уже существующие таблицы и упадёт. См. раздел «Первый деплой поверх старого приложения».

```bash
useradd --system --home /opt/diavto --shell /usr/sbin/nologin diavto
mkdir -p /opt/diavto/site /opt/diavto/admin /opt/diavto/Data /opt/diavto/backups

tar -xzf /tmp/diavto-site.tar.gz -C /opt/diavto/site
tar -xzf /tmp/diavto-admin.tar.gz -C /opt/diavto/admin

cp deploy/scripts/preflight-db.sh /opt/diavto/preflight-db.sh
chmod 755 /opt/diavto/preflight-db.sh

chown -R diavto:diavto /opt/diavto
chmod 755 /opt/diavto/site/DiAvto.Site /opt/diavto/admin/DiAvto.Admin
```

Службы: `deploy/systemd/diavto-site.service` и `deploy/systemd/diavto-admin.service`.
Nginx: `deploy/nginx/di-auto.conf`.

```bash
cp deploy/systemd/diavto-site.service /etc/systemd/system/
cp deploy/systemd/diavto-admin.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable --now diavto-site diavto-admin
```

Проверка:

```bash
curl http://127.0.0.1:8080/health
curl http://127.0.0.1:8081/healthz
systemctl status diavto-site diavto-admin
journalctl -u diavto-site -f
```

## Первый деплой поверх старого приложения

1. Остановить старую службу: `systemctl stop diavto && systemctl disable diavto`.
2. Сохранить старую БД:
   ```bash
   cp /opt/diavto/Data/diavto.db /opt/diavto/backups/diavto-legacy-$(date +%F).db
   ```
3. Перенести данные в новую схему (необязательно, если старые данные не нужны):
   ```bash
   # на Windows
   dotnet publish tools\DiAvto.MigrateLegacy -c Release -r linux-x64 --self-contained true -o publish-linux\migrate
   # на сервере
   ./migrate/DiAvto.MigrateLegacy /opt/diavto/Data/diavto.db /opt/diavto/Data/diavto-new.db
   ```
   Инструмент создаёт новый файл, применяет новую схему и копирует общие таблицы (пользователи, поиски, автомобили, карточки, тарифы, настройки). Несовместимые таблицы пропускаются с сообщением.
4. Если перенос не нужен — просто убрать старый файл из рабочего пути:
   ```bash
   mv /opt/diavto/Data/diavto.db /opt/diavto/Data/diavto-legacy.db
   ```
5. Если перенос выполнен, заменить имя нового файла:
   ```bash
   mv /opt/diavto/Data/diavto-new.db /opt/diavto/Data/diavto.db
   ```
6. Запустить `diavto-admin`, затем `diavto-site` — сайт дозагрузит публичный контент, если он ещё не заполнен.

Проверка перед обновлением:

```bash
bash /opt/diavto/preflight-db.sh /opt/diavto/Data/diavto.db
```

Скрипт возвращает `0` для новой схемы и `1` для старой/неизвестной. Workflow деплоя вызывает его до остановки служб.

## Nginx и HTTPS

1. Положить конфигурацию в `/etc/nginx/sites-available/di-auto.conf` и включить симлинком.
2. Проверить `nginx -t` и перезагрузить `systemctl reload nginx`.
3. Выпустить сертификаты:

```bash
certbot --nginx -d di-auto.ru -d www.di-auto.ru
certbot --nginx -d admin.di-auto.ru
```

Для Blazor Server обязательно проксирование WebSocket (`Upgrade`/`Connection`) — оно уже включено в шаблоне.

## Обновление

```bash
systemctl stop diavto-site diavto-admin
cp /opt/diavto/Data/diavto.db /opt/diavto/backups/diavto-before-update-$(date +%F_%H-%M).db
tar -xzf /tmp/diavto-site.tar.gz -C /opt/diavto/site
tar -xzf /tmp/diavto-admin.tar.gz -C /opt/diavto/admin
chown -R diavto:diavto /opt/diavto
systemctl start diavto-admin diavto-site
```

`tar -xzf` не удаляет `Data` и `appsettings.Production.json`. Миграции применяются при старте. Первым запускается админка (создаёт роли/администратора), затем сайт (сидирует публичный контент, если база пустая).

## Переменные окружения

Пароль администратора и ключи не хранятся в репозитории. Они задаются в `/etc/systemd/system/diavto-admin.service`:

```ini
Environment=Seed__AdminPassword=...
Environment=Seed__AdminEmail=...
Environment=Seed__AdminUserName=admin
```

После изменения:

```bash
systemctl daemon-reload
systemctl restart diavto-admin
```

## Резервное копирование

Ежедневный запуск через cron или systemd timer. Пример скрипта — `deploy/backup/diavto-backup.sh`. Он делает консистентную копию SQLite через `sqlite3.backup()` и хранит 14 дней.

```bash
chmod 755 /opt/diavto/backup.sh
crontab -e
# 15 3 * * * /opt/diavto/backup.sh
```

## Проверка после деплоя

- `https://di-auto.ru` открывает визитку, каталог и карточку автомобиля.
- `https://admin.di-auto.ru` открывает вход в админку.
- В админке: **Карточки клиента → Публичный адрес сайта** = `https://di-auto.ru`.
- Ссылка `/card/{PublicId}` открывается на публичном домене, а не в админке.
- Форма заявки на сайте создаёт запись в **Админка → Заявки**.
- `https://di-auto.ru/health` возвращает `OK`.

## Ограничения

- che168 — неофициальный мобильный API, возможны ограничения и изменение формата.
- Фотографии витрины лежат в `wwwroot` сайта (~172 МБ). При необходимости их можно вынести на Nginx/CDN.
- SQLite подходит для текущей нагрузки; при росте числа одновременных записей стоит перейти на PostgreSQL.
