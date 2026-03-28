FROM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY API/API.csproj API/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore API/API.csproj

COPY . .
RUN dotnet publish API/API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:ContinuousIntegrationBuild=true \
    /p:Deterministic=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache curl

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]
