# ==========================================
# Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/FcgCatalog.Api/FcgCatalog.Api.csproj src/FcgCatalog.Api/
COPY src/FcgCatalog.Application/FcgCatalog.Application.csproj src/FcgCatalog.Application/
COPY src/FcgCatalog.Domain/FcgCatalog.Domain.csproj src/FcgCatalog.Domain/
COPY src/FcgCatalog.Infrastructure/FcgCatalog.Infrastructure.csproj src/FcgCatalog.Infrastructure/
COPY src/FcgCatalog.IoC/FcgCatalog.IoC.csproj src/FcgCatalog.IoC/
COPY src/FcgCatalog.ServiceDefaults/FcgCatalog.ServiceDefaults.csproj src/FcgCatalog.ServiceDefaults/
COPY src/FcgCatalog.SharedKernel/FcgCatalog.SharedKernel.csproj src/FcgCatalog.SharedKernel/

RUN dotnet restore "src/FcgCatalog.Api/FcgCatalog.Api.csproj"

COPY src/ src/

WORKDIR /src/src/FcgCatalog.Api
RUN dotnet publish "FcgCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FcgCatalog.Api.dll"]
