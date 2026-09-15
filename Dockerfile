# ============================================================
# BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["NuGet.config", "./"]

COPY ["src/FCG.Notifications.Functions/FCG_Notifications_Functions.csproj", "src/FCG.Notifications.Functions/"]
COPY ["src/FCG.Notifications.Application/FCG.Notifications.Application.csproj", "src/FCG.Notifications.Application/"]
COPY ["src/FCG.Notifications.Domain/FCG.Notifications.Domain.csproj", "src/FCG.Notifications.Domain/"]
COPY ["src/FCG.Notifications.Infrastructure/FCG.Notifications.Infrastructure.csproj", "src/FCG.Notifications.Infrastructure/"]

RUN dotnet restore \
    "src/FCG.Notifications.Functions/FCG_Notifications_Functions.csproj" \
    --configfile "/src/NuGet.config"

COPY . .

RUN dotnet publish \
    "src/FCG.Notifications.Functions/FCG_Notifications_Functions.csproj" \
    -c Release \
    -o /home/site/wwwroot \
    --no-restore


# ============================================================
# AZURE FUNCTIONS RUNTIME
# ============================================================
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

WORKDIR /home/site/wwwroot

COPY --from=build /home/site/wwwroot .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true