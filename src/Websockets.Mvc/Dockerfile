FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

COPY *.csproj ./app/
WORKDIR ./app
RUN dotnet restore

COPY . .
RUN dotnet publish -c $BUILD_CONFIGURATION -o /release

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as final
WORKDIR /app/
COPY --from=build /release .
ENTRYPOINT ["dotnet", "Websockets.Mvc.dll"]


