FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /Src

COPY ["NotificationTelegramBot.API/NotificationTelegramBot.API.csproj", "NotificationTelegramBot.API/"]
RUN dotnet restore "NotificationTelegramBot.API/NotificationTelegramBot.API.csproj"

COPY . .
WORKDIR "/Src/NotificationTelegramBot.API"
RUN dotnet build "NotificationTelegramBot.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationTelegramBot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationTelegramBot.API.dll"]