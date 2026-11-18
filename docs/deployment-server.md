# Развёртывание на сервере

## Сервер

| Параметр | Значение |
|---|---|
| IP | `179.66.186.141` |
| ОС | Ubuntu 26.04 LTS, x86_64 |
| Публичный адрес | `http://179.66.186.141` |
| Каталог приложения | `/opt/diavto` |
| База данных | `/opt/diavto/Data/diavto.db` |
| systemd-служба | `diavto.service` |
| Пользователь службы | `diavto` (не root) |
| Часовой пояс | `Asia/Krasnoyarsk` (UTC+7) |
| Порт | 80, Kestrel напрямую, без Nginx |

Приложение собрано как self-contained `linux-x64`, поэтому .NET на сервере не требуется.

## Управление

```bash
systemctl status diavto
systemctl restart diavto
systemctl stop diavto
journalctl -u diavto -f
```

Проверка:

```bash
curl http://127.0.0.1/healthz
```

## Обновление приложения

1. На Windows-машине собрать свежую версию:

```powershell
dotnet publish src\DiAvto.Web\DiAvto.Web.csproj -c Release -r linux-x64 --self-contained true -o C:\Users\Admin\Desktop\ChinaCars\publish-linux
```

2. Собрать архив и загрузить на сервер:

```powershell
tar.exe -czf C:\Users\Admin\Desktop\ChinaCars\diavto-linux.tar.gz -C C:\Users\Admin\Desktop\ChinaCars\publish-linux .
scp C:\Users\Admin\Desktop\ChinaCars\diavto-linux.tar.gz root@179.66.186.141:/tmp/
```

3. На сервере сохранить БД и обновить файлы:

```bash
systemctl stop diavto
cp /opt/diavto/Data/diavto.db /opt/diavto/backups/diavto-before-update-$(date +%F_%H-%M).db
tar -xzf /tmp/diavto-linux.tar.gz -C /opt/diavto
chown -R diavto:diavto /opt/diavto
chmod 755 /opt/diavto/DiAvto.Web
systemctl start diavto
```

`tar -xzf` не удаляет каталог `Data`, поэтому база сохраняется. Миграции EF Core применяются автоматически при старте.

При обновлении также перезаписывается `appsettings.Production.json`. Пароль для повторного сида администратора хранится не в нём, а в systemd-переменной `Seed__AdminPassword` в `/etc/systemd/system/diavto.service`. Не удаляйте эту строку при ручном редактировании службы.

## Резервное копирование

Ежедневно в 03:15 запускается `/usr/local/bin/diavto-backup.sh`, который делает консистентную копию SQLite через `sqlite3.backup()` и хранит 14 дней в `/opt/diavto/backups`.

Проверить вручную:

```bash
su -s /bin/bash diavto -c '/usr/local/bin/diavto-backup.sh'
ls -lh /opt/diavto/backups
```

## Настройки после развёртывания

- **Админка → Тарифы** — заполнить реальные стоимости доставки до Владивостока, доставки из Владивостока, осмотра и растаможки.
- **Админка → Карточки клиента** — проверить публичный адрес `http://179.66.186.141` и контакты менеджера.
- Пароль администратора изменён с `admin` на отдельный сложный пароль (передаётся отдельно). Сменить его можно в `/Account/Manage/ChangePassword`.

## Важно

- Сайт сейчас работает по **HTTP**. Для публичного доступа с паролем администратора и персональными карточками клиентов рекомендуется настроить домен и HTTPS (Nginx + Let's Encrypt). До этого не отправляйте ссылку на админку по открытым каналам.
- che168 может ограничивать запросы. Поиск и обновление карточек зависят от доступности их мобильного API.
- Фотографии и данные принадлежат che168.com / Autohome. На публичной карточке оставлена текстовая подпись источника без ссылки на оригинал.
