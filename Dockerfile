FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["NuGet.config", "./"]

COPY ["src/FCG.Notifications.Worker/FCG.Notifications.Worker.csproj", "src/FCG.Notifications.Worker/"]
COPY ["src/FCG.Notifications.Application/FCG.Notifications.Application.csproj", "src/FCG.Notifications.Application/"]
COPY ["src/FCG.Notifications.Domain/FCG.Notifications.Domain.csproj", "src/FCG.Notifications.Domain/"]
COPY ["src/FCG.Notifications.Infrastructure/FCG.Notifications.Infrastructure.csproj", "src/FCG.Notifications.Infrastructure/"]

RUN dotnet restore "src/FCG.Notifications.Worker/FCG.Notifications.Worker.csproj" \
    --configfile "/src/NuGet.config"

COPY . .

RUN dotnet publish "src/FCG.Notifications.Worker/FCG.Notifications.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.Notifications.Worker.dll"]