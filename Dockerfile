# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj first for better layer caching
COPY src/ThothBotCore.csproj src/

RUN dotnet restore src/ThothBotCore.csproj

# Copy everything else
COPY . .

RUN dotnet publish src/ThothBotCore.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Create non-root user (Debian-based image)
RUN groupadd appgroup && useradd -m -g appgroup appuser

# Create required directories BEFORE switching user
RUN mkdir -p /app/Config /app/Storage \
    && chown -R appuser:appgroup /app

# Copy published output
COPY --from=build /app/publish .

# Switch to non-root user
USER appuser

# Environment
ENV METRICS_PORT=9284

# Expose metrics port
EXPOSE 9284

# Run app
ENTRYPOINT ["dotnet", "ThothBotCore.dll"]