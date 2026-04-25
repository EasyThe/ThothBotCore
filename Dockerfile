# Multi-stage build for ThothBotCore (.NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore/publish
COPY . .
RUN dotnet restore "src/ThothBotCore.csproj"
RUN dotnet publish "src/ThothBotCore.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Create non-root user and ensure app dirs exist & are owned by appuser
RUN addgroup -S appgroup && adduser -S appuser -G appgroup \
    && mkdir -p /app/Config /app/Storage \
    && chown -R appuser:appgroup /app

USER appuser

# Default metrics port used by BotConfig (can be changed via mounted Config/Config.json)
ENV METRICS_PORT=9284

# Expose metrics port; change mapping in docker-compose if you change METRICS_PORT
EXPOSE 9284

# Run the published dll
ENTRYPOINT ["dotnet", "ThothBotCore.dll"]