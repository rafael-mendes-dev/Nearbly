FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Nearbly.sln ./
COPY src/Nearbly.Domain/Nearbly.Domain.csproj src/Nearbly.Domain/
COPY src/Nearbly.Application/Nearbly.Application.csproj src/Nearbly.Application/
COPY src/Nearbly.Infrastructure/Nearbly.Infrastructure.csproj src/Nearbly.Infrastructure/
COPY src/Nearbly.Api/Nearbly.Api.csproj src/Nearbly.Api/
COPY tests/Nearbly.UnitTests/Nearbly.UnitTests.csproj tests/Nearbly.UnitTests/
COPY tests/Nearbly.IntegrationTests/Nearbly.IntegrationTests.csproj tests/Nearbly.IntegrationTests/
RUN dotnet restore Nearbly.sln

COPY . .
RUN dotnet publish src/Nearbly.Api/Nearbly.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nearbly.Api.dll"]
