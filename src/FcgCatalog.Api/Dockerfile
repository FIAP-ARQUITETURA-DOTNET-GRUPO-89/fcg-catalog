# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY global.json .
COPY Directory.Build.props .
COPY Directory.Packages.props .

COPY src/FcgCatalog.Api/FcgCatalog.Api.csproj src/FcgCatalog.Api/
COPY src/FcgCatalog.Application/FcgCatalog.Application.csproj src/FcgCatalog.Application/
COPY src/FcgCatalog.Domain/FcgCatalog.Domain.csproj src/FcgCatalog.Domain/
COPY src/FcgCatalog.Infrastructure/FcgCatalog.Infrastructure.csproj src/FcgCatalog.Infrastructure/
COPY src/FcgCatalog.IoC/FcgCatalog.IoC.csproj src/FcgCatalog.IoC/
COPY src/FcgCatalog.ServiceDefaults/FcgCatalog.ServiceDefaults.csproj src/FcgCatalog.ServiceDefaults/
COPY src/FcgCatalog.SharedKernel/FcgCatalog.SharedKernel.csproj src/FcgCatalog.SharedKernel/

RUN dotnet restore src/FcgCatalog.Api/FcgCatalog.Api.csproj

COPY . .

RUN dotnet publish src/FcgCatalog.Api/FcgCatalog.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

USER $APP_UID

EXPOSE 7030

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FcgCatalog.Api.dll"]