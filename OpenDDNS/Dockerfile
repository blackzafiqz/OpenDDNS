FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OpenDDNS/OpenDDNS.csproj", "OpenDDNS/"]
COPY ["OpenDDNSLib/OpenDDNSLib.csproj", "OpenDDNSLib/"]
RUN dotnet restore "OpenDDNS/OpenDDNS.csproj"
COPY . .
WORKDIR "/src/OpenDDNS"
RUN dotnet build "OpenDDNS.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenDDNS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenDDNS.dll"]
