#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="config/with-tailscale/docker-compose.yml"
BACKUP_DIR="backups"
HEALTH_WAIT=20

mkdir -p "$BACKUP_DIR"

echo "==> Backing up database..."
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/bloom_$TIMESTAMP.sql"
docker compose -f "$COMPOSE_FILE" exec -T bloom-db \
    sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' \
    > "$BACKUP_FILE"
echo "    Saved: $BACKUP_FILE"

echo "==> Pulling latest code..."
git pull origin main

echo "==> Building and restarting services..."
docker compose -f "$COMPOSE_FILE" up --build -d

echo "==> Waiting ${HEALTH_WAIT}s for health checks..."
sleep "$HEALTH_WAIT"

echo "==> Service status:"
docker compose -f "$COMPOSE_FILE" ps

echo "==> Deploy complete."
