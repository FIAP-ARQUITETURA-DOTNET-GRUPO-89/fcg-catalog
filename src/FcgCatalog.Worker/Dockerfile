# syntax=docker/dockerfile:1.7

########################################
# Build
########################################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Arquivos compartilhados da solução
COPY global.json .
COPY Directory.Build.props .
COPY Directory.Packages.props .

# Projetos
COPY src/FcgCatalog.Worker/FcgCatalog.Worker.csproj src/FcgCatalog.Worker/
COPY src/FcgCatalog.Application/FcgCatalog.Application.csproj src/FcgCatalog.Application/
COPY src/FcgCatalog.Domain/FcgCatalog.Domain.csproj src/FcgCatalog.Domain/
COPY src/FcgCatalog.Infrastructure/FcgCatalog.Infrastructure.csproj src/FcgCatalog.Infrastructure/
COPY src/FcgCatalog.IoC/FcgCatalog.IoC.csproj src/FcgCatalog.IoC/
COPY src/FcgCatalog.SharedKernel/FcgCatalog.SharedKernel.csproj src/FcgCatalog.SharedKernel/
COPY src/FcgCatalog.ServiceDefaults/FcgCatalog.ServiceDefaults.csproj src/FcgCatalog.ServiceDefaults/

# Restore
RUN dotnet restore src/FcgCatalog.Worker/FcgCatalog.Worker.csproj

# Copia todo o restante do código
COPY . .

# Publish
RUN dotnet publish src/FcgCatalog.Worker/FcgCatalog.Worker.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

########################################
# Runtime
########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Executa como usuário não-root
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FcgCatalog.Worker.dll"]
