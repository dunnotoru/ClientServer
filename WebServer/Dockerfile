FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY WebServer.csproj .
RUN dotnet restore ./WebServer.csproj
COPY . .
RUN dotnet build WebServer.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish WebServer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebServer.dll"]
