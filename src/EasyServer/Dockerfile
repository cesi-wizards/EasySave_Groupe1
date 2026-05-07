FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app
USER root
RUN mkdir -p /app/logs && chown -R $APP_UID:$APP_UID /app/logs

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

RUN dotnet restore "src/EasyServer/EasyServer.csproj"

WORKDIR "/src/src/EasyServer"

RUN dotnet publish "EasyServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false \
    --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

VOLUME /app/logs
EXPOSE 11000

ENTRYPOINT ["dotnet", "EasyServer.dll"]
