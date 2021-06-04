FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0 AS build
WORKDIR /src
COPY ["ArcticFox/ArcticFox.Codec/ArcticFox.Codec.csproj", "ArcticFox/ArcticFox.Codec/"]
COPY ["ArcticFox/ArcticFox.Net/ArcticFox.Net.csproj", "ArcticFox/ArcticFox.Net/"]
COPY ["ArcticFox/ArcticFox.SmartFoxServer/ArcticFox.SmartFoxServer.csproj", "ArcticFox/ArcticFox.SmartFoxServer/"]
COPY ["WeevilWorld.Protocol/WeevilWorld.Protocol.csproj", "WeevilWorld.Protocol/"]
COPY ["WeevilWorld.Server/WeevilWorld.Server.csproj", "WeevilWorld.Server/"]
RUN dotnet restore "WeevilWorld.Server/WeevilWorld.Server.csproj"
COPY . .
WORKDIR "/src/WeevilWorld.Server"
RUN dotnet build "WeevilWorld.Server.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "WeevilWorld.Server.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeevilWorld.Server.dll"]
