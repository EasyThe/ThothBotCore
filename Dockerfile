# Multi-stage build for ThothBotCore (.NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/ThothBotCore.csproj"
RUN dotnet publish "src/ThothBotCore.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
ARG PUID=1000
ARG PGID=1000

# Install gosu reliably and required packages
USER root
RUN apt-get update \
  && apt-get install -y --no-install-recommends ca-certificates wget gnupg dirmngr \
  && rm -rf /var/lib/apt/lists/* \
  && set -eu; \
  ARCH="$(dpkg --print-architecture)"; \
  if [ "$ARCH" = "amd64" ]; then GOARCH=amd64; elif [ "$ARCH" = "arm64" ] || [ "$ARCH" = "arm64v8" ]; then GOARCH=arm64; else GOARCH="$ARCH"; fi; \
  GOSU_VERSION=1.16; \
  wget -O /usr/local/bin/gosu "https://github.com/tianon/gosu/releases/download/$GOSU_VERSION/gosu-$GOARCH" \
  && chmod +x /usr/local/bin/gosu \
  && gosu --version

# Create group/user with predictable IDs
RUN groupadd -g ${PGID} appgroup || true \
    && useradd -u ${PUID} -g ${PGID} -m -s /usr/sbin/nologin appuser || true

WORKDIR /app
COPY --from=build /app/publish .
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh \
    && mkdir -p /app/Config /app/Storage

ENV METRICS_PORT=9284
EXPOSE 9284

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
CMD ["dotnet", "ThothBotCore.dll"]