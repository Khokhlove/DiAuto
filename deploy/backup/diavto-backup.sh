#!/usr/bin/env bash
set -euo pipefail

DB_PATH="${DB_PATH:-/opt/diavto/Data/diavto.db}"
BACKUP_DIR="${BACKUP_DIR:-/opt/diavto/backups}"
KEEP_DAYS="${KEEP_DAYS:-14}"
STAMP="$(date +%F_%H-%M-%S)"
TARGET="${BACKUP_DIR}/diavto-${STAMP}.db"

mkdir -p "${BACKUP_DIR}"

sqlite3 "${DB_PATH}" ".backup '${TARGET}'"
gzip -f "${TARGET}"

find "${BACKUP_DIR}" -name 'diavto-*.db.gz' -type f -mtime "+${KEEP_DAYS}" -delete

echo "DiAvto backup: ${TARGET}.gz"
