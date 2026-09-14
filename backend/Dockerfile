# ==============================================================================
# Multi-stage Dockerfile for Gestion_Ope Backend (.NET 8 Web API)
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for optimal layer caching
COPY GestionOpe.sln ./
COPY src/GestionOpe.Domain/*.csproj ./src/GestionOpe.Domain/
COPY src/GestionOpe.Application/*.csproj ./src/GestionOpe.Application/
COPY src/GestionOpe.Infrastructure/*.csproj ./src/GestionOpe.Infrastructure/
COPY src/GestionOpe.Api/*.csproj ./src/GestionOpe.Api/
COPY tests/GestionOpe.UnitTests/*.csproj ./tests/GestionOpe.UnitTests/
COPY tests/GestionOpe.IntegrationTests/*.csproj ./tests/GestionOpe.IntegrationTests/

# Restore dependencies
RUN dotnet restore GestionOpe.sln

# Copy all source code
COPY . .

# Run Unit tests in CI build phase
RUN dotnet test tests/GestionOpe.UnitTests/GestionOpe.UnitTests.csproj -c Release --no-restore

# Publish the API project
WORKDIR /src/src/GestionOpe.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: Non-root user
USER $APP_UID

# Copy build artifacts
COPY --from=build /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

ENTRYPOINT ["dotnet", "GestionOpe.Api.dll"]
