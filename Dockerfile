# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/ThothBotCore.csproj src/
RUN dotnet restore src/ThothBotCore.csproj

COPY . .
RUN dotnet publish src/ThothBotCore.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# Runtime stage (IMPORTANT FIX HERE)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user
RUN groupadd appgroup && useradd -m -g appgroup appuser

# Create folders before switching user
RUN mkdir -p /app/Config /app/Storage \
    && chown -R appuser:appgroup /app

COPY --from=build /app/publish .

USER appuser

ENV METRICS_PORT=9284
EXPOSE 9284

ENTRYPOINT ["dotnet", "ThothBotCore.dll"]