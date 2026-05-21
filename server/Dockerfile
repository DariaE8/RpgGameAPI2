# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл решения и файлы проектов
COPY ["RpgGameAPI2.sln", "./"]
COPY ["RpgGame.API/RpgGame.API.csproj", "RpgGame.API/"]
COPY ["RpgGame.Core/RpgGame.Core.csproj", "RpgGame.Core/"]
COPY ["RpgGame.Infrastructure/RpgGame.Infrastructure.csproj", "RpgGame.Infrastructure/"]
COPY ["RpgGame.Services/RpgGame.Services.csproj", "RpgGame.Services/"]
COPY ["RpgGame.Tests/RpgGame.Tests.csproj", "RpgGame.Tests/"]

# Восстанавливаем зависимости
RUN dotnet restore "RpgGameAPI2.sln"

# Копируем весь остальной исходный код и собираем проект
COPY . .
WORKDIR "/src/RpgGame.API"
RUN dotnet publish "RpgGame.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный этап (только среда выполнения)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Задаем порт по умолчанию
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "RpgGame.API.dll"]