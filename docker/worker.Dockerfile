FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/worker ./src/worker
RUN dotnet restore "src/worker/AppHost/NetrinAF.Worker.AppHost.csproj"
RUN dotnet publish "src/worker/AppHost/NetrinAF.Worker.AppHost.csproj" \
    --no-restore \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetrinAF.Worker.AppHost.dll"]
