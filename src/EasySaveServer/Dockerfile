FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

USER root
RUN mkdir -p /app/logs && chown -R $APP_UID:$APP_UID /app/logs

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EasySaveServer/EasySaveServer.csproj", "EasySaveServer/"]
COPY ["EasyLog/EasyLog.csproj", "EasyLog/"]

RUN dotnet restore "EasySaveServer/EasySaveServer.csproj"

COPY . .

WORKDIR "/src/EasySaveServer"
RUN dotnet build "EasySaveServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# On retire le UseAppHost=false pour éviter l'erreur NETSDK1067 si tu es en Self-Contained
RUN dotnet publish "EasySaveServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

VOLUME /app/logs

EXPOSE 11000
ENTRYPOINT ["dotnet", "EasySaveServer.dll"]
