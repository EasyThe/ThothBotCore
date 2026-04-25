docker-entrypoint.sh
#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/app"
CONFIG_DIR="${APP_DIR}/Config"
CONFIG_FILE="${CONFIG_DIR}/Config.json"
STORAGE_DIR="${APP_DIR}/Storage"
APP_USER="appuser"
APP_GROUP="appgroup"

mkdir -p "${CONFIG_DIR}" "${STORAGE_DIR}"

# Ensure ownership of (possibly bind-mounted) directories
chown -R "${APP_USER}:${APP_GROUP}" "${CONFIG_DIR}" "${STORAGE_DIR}" || true
chmod 0775 "${CONFIG_DIR}" "${STORAGE_DIR}" || true

# Create config from env if missing/empty (matches BotConfig defaults)
if [ ! -s "${CONFIG_FILE}" ]; then
  cat > "${CONFIG_FILE}" <<JSON
{
  "Token": "${BOT_TOKEN:-TOKEN-HERE}",
  "devId": "${DEV_ID:-HiRezDevID}",
  "MongoDbURL": "${MONGODB_URL:-MongoURL}",
  "Sentry": "${SENTRY_DSN:-SentryURL}",
  "authKey": "${AUTH_KEY:-HiRezAuthKey}",
  "challongeKey": "${CHALLONGE_KEY:-ChallongeKey}",
  "trelloKey": "${TRELLO_KEY:-TrelloKey}",
  "trelloToken": "${TRELLO_TOKEN:-TrelloToken}",
  "prefix": "${PREFIX:-!!}",
  "setGame": "${SET_GAME:-!!help}",
  "botsAPI": "${BOTS_API:-DiscordBotsAPIkey}",
  "dblAPI": "${DBL_API:-DiscordBotListAPIkey}",
  "dbggAPI": "${DBGG_API:-DiscordBotsGGAPIkey}",
  "BotsOnDiscordAPI": "${BOTS_ON_DISCORD_API:-BotsOnDiscordAPIkey}",
  "DiscordServicesAPI": "${DISCORD_SERVICES_API:-DiscordServicesAPI}",
  "DiscordLabsAPI": "${DISCORD_LABS_API:-DiscordLabsAPI}",
  "StatCordAPI": "${STATCORD_API:-StatCordAPI}",
  "GoogleAPIKey": "${GOOGLE_API_KEY:-GoogleAPIKey}",
  "MetricsPort": "${METRICS_PORT:-9284}",
  "Debug": ${DEBUG:-false},
  "IsDev": ${IS_DEV:-0}
}
JSON
  chown "${APP_USER}:${APP_GROUP}" "${CONFIG_FILE}" || true
  chmod 0664 "${CONFIG_FILE}" || true
  echo "INFO: Created ${CONFIG_FILE}"
else
  echo "INFO: Using existing ${CONFIG_FILE}"
fi

# Drop privileges and run the CMD as the non-root user
exec gosu "${APP_USER}:${APP_GROUP}" "$@"