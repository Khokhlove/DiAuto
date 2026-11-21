#!/usr/bin/env bash
# Проверяет, что база совместима с новой схемой DiAvto.
# Код возврата 0 — можно запускать; 1 — нужна ручная миграция/новый файл БД.
set -euo pipefail

DB_PATH="${1:-/opt/diavto/Data/diavto.db}"

if [ ! -f "${DB_PATH}" ]; then
    echo "DiAvto preflight: база ${DB_PATH} не найдена, будет создана новая."
    exit 0
fi

has_new_table="$(sqlite3 "${DB_PATH}" "select count(*) from sqlite_master where type='table' and name='ShowcaseCars';")"
has_new_migration="$(sqlite3 "${DB_PATH}" "select count(*) from __EFMigrationsHistory where MigrationId='20261120044141_InitialCreate';" 2>/dev/null || echo 0)"
has_legacy_migration="$(sqlite3 "${DB_PATH}" "select count(*) from __EFMigrationsHistory where MigrationId like '%InitialDomain%' or MigrationId like '%CreateIdentitySchema%';" 2>/dev/null || echo 0)"

if [ "${has_new_table}" = "1" ] || [ "${has_new_migration}" != "0" ]; then
    echo "DiAvto preflight: база совместима с новой схемой."
    exit 0
fi

if [ "${has_legacy_migration}" != "0" ]; then
    echo "DiAvto preflight: обнаружена база старого DiAvto.Web." >&2
    echo "Остановите старую службу, сохраните БД и запустите новую с чистым файлом." >&2
    echo "Подробности: docs/deployment.md, раздел «Первый деплой поверх старого приложения»." >&2
    exit 1
fi

echo "DiAvto preflight: неизвестная схема БД ${DB_PATH}. Требуется ручная проверка." >&2
exit 1
