# ==========================================
# Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Arquivos compartilhados da solução
COPY global.json .
COPY Directory.Build.props .
COPY Directory.Packages.props .

# Copia apenas os projetos para aproveitar o cache do Docker
COPY src/FcgCatalog.Api/FcgCatalog.Api.csproj src/FcgCatalog.Api/
COPY src/FcgCatalog.Application/FcgCatalog.Application.csproj src/FcgCatalog.Application/
COPY src/FcgCatalog.Domain/FcgCatalog.Domain.csproj src/FcgCatalog.Domain/
COPY src/FcgCatalog.Infrastructure/FcgCatalog.Infrastructure.csproj src/FcgCatalog.Infrastructure/
COPY src/FcgCatalog.IoC/FcgCatalog.IoC.csproj src/FcgCatalog.IoC/
COPY src/FcgCatalog.SharedKernel/FcgCatalog.SharedKernel.csproj src/FcgCatalog.SharedKernel/
COPY src/FcgCatalog.ServiceDefaults/FcgCatalog.ServiceDefaults.csproj src/FcgCatalog.ServiceDefaults/

# Restore
RUN dotnet restore src/FcgCatalog.Api/FcgCatalog.Api.csproj

# Copia todo o restante da solução
COPY . .

# Publicação
RUN dotnet publish src/FcgCatalog.Api/FcgCatalog.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ==========================================
# Runtime
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

EXPOSE 7030

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FcgCatalog.Api.dll"]