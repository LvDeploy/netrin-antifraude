FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/antifraude ./src/antifraude
RUN dotnet restore "src/antifraude/Api/NetrinAF.Api/NetrinAF.Api.csproj"
RUN dotnet publish "src/antifraude/Api/NetrinAF.Api/NetrinAF.Api.csproj" \
    --no-restore \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetrinAF.Api.dll"]
