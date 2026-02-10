# Для разработки и production
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8081
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8081

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WebApplication2/WebApplication2.csproj", "WebApplication2/"]
RUN dotnet restore "WebApplication2/WebApplication2.csproj"
COPY . .
WORKDIR "/src/WebApplication2"
RUN dotnet build "WebApplication2.csproj" -c Release -o /app/build

# Этап публикации
FROM build AS publish
RUN dotnet publish "WebApplication2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный этап
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Healthcheck для мониторинга
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WebApplication2.dll"]