FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY TuranLogix.sln .
COPY src/TuranLogix.Domain/TuranLogix.Domain.csproj src/TuranLogix.Domain/
COPY src/TuranLogix.Application/TuranLogix.Application.csproj src/TuranLogix.Application/
COPY src/TuranLogix.Infrastructure/TuranLogix.Infrastructure.csproj src/TuranLogix.Infrastructure/
COPY src/TuranLogix.Api/TuranLogix.Api.csproj src/TuranLogix.Api/
COPY tests/TuranLogix.Domain.Tests/TuranLogix.Domain.Tests.csproj tests/TuranLogix.Domain.Tests/
COPY tests/TuranLogix.Application.Tests/TuranLogix.Application.Tests.csproj tests/TuranLogix.Application.Tests/

RUN dotnet restore src/TuranLogix.Api/TuranLogix.Api.csproj

COPY . .
RUN dotnet publish src/TuranLogix.Api/TuranLogix.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TuranLogix.Api.dll"]
