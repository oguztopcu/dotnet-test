# ── Build stage ───────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/AcilEvrak.Domain/AcilEvrak.Domain.csproj src/AcilEvrak.Domain/
COPY src/AcilEvrak.Application/AcilEvrak.Application.csproj src/AcilEvrak.Application/
COPY src/AcilEvrak.Infrastructure/AcilEvrak.Infrastructure.csproj src/AcilEvrak.Infrastructure/
COPY src/AcilEvrak.WebAPI/AcilEvrak.WebAPI.csproj src/AcilEvrak.WebAPI/
RUN dotnet restore src/AcilEvrak.WebAPI/AcilEvrak.WebAPI.csproj

COPY src/ src/

RUN dotnet publish src/AcilEvrak.WebAPI/AcilEvrak.WebAPI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "AcilEvrak.WebAPI.dll"]
