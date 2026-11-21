# Production-сервер

Актуальная инструкция по развёртыванию: [deployment.md](deployment.md).

## Параметры

| Параметр | Значение |
|---|---|
| IP | `179.66.186.141` |
| ОС | Ubuntu 26.04 LTS, x86_64 |
| Каталог приложения | `/opt/diavto` |
| База данных | `/opt/diavto/Data/diavto.db` |
| Часовой пояс | `Asia/Krasnoyarsk` (UTC+7) |

## Текущее состояние

На сервере пока работает старый `DiAvto.Web` (`diavto.service`, порт 80). Новый контур — две службы `diavto-site` (8080) и `diavto-admin` (8081) за Nginx — ещё не развёрнут.

Перед первым запуском нового контура:

1. Остановить и отключить `diavto.service`.
2. Сохранить старую БД в `/opt/diavto/backups`.
3. Убрать старый файл `/opt/diavto/Data/diavto.db` из рабочего пути.
4. Запустить `diavto-admin`, затем `diavto-site` — они создадут новую схему.

Старая база несовместима с новой историей миграций EF Core. Подробности и команды — в разделе «Первый деплой поверх старого приложения» файла [deployment.md](deployment.md).

## Управление

```bash
systemctl status diavto-site diavto-admin
systemctl restart diavto-admin diavto-site
journalctl -u diavto-site -f
journalctl -u diavto-admin -f
```

Проверка:

```bash
curl http://127.0.0.1:8080/health
curl http://127.0.0.1:8081/healthz
```

## Резервное копирование

```bash
su -s /bin/bash diavto -c '/opt/diavto/backup.sh'
ls -lh /opt/diavto/backups
```

Скрипт и инструкция: `deploy/backup/diavto-backup.sh`, [deployment.md](deployment.md).
