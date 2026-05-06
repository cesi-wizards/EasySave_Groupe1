FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
ARG APP_UID=1654
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EasySaveServer/EasySaveServer.csproj", "EasySaveServer/"]
RUN dotnet restore "EasySaveServer/EasySaveServer.csproj"
COPY . .
WORKDIR "/src/EasySaveLogServer"
RUN dotnet build "./EasySaveServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EasySaveServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root
COPY --from=publish /app/publish .
RUN chown -R $APP_UID /app
USER $APP_UID

EXPOSE 11000
ENTRYPOINT ["dotnet", "EasySaveServer.dll"]
